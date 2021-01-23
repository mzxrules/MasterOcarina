using mzxrules.Helper;
using mzxrules.OcaLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Experimental.Data
{
    static class UltimaWrongWarp
    {
        public static StringBuilder Calculate(IExperimentFace face, List<string> files)
        {
            StringBuilder result = new StringBuilder();
            int N0_SceneEndAddr = 0x384980;
            int N0_CodeAddr = 0x110A0;
            SegmentAddress[] SegmentTable = new SegmentAddress[16];

            Dictionary<short, EntranceTableRecord> EntranceTableSimplified = new Dictionary<short, EntranceTableRecord>();
            Rom N0 = new ORom(files[0], ORom.Build.N0);

            Addresser.TryGetRom(ORom.FileList.code, N0.Version, AddressToken.EntranceIndexTable_Start, out int entranceTableAddress);
            Addresser.TryGetRom(ORom.FileList.code, N0.Version, AddressToken.ActorTable_Start, out int actorTableRomAddress);

            var code_File = N0.Files.GetFile(ORom.FileList.code);
            var codeStr = new BinaryReader(code_File);
            codeStr.BaseStream.Position = code_File.Record.GetRelativeAddress(entranceTableAddress);
            
            //remove redundant entrance table records
            for (int i = 0; i < 0x614; i++)
            {
                var record = new EntranceTableRecord(codeStr);
                short key = (short)((record.Scene << 8) + record.Spawn);

                if (!EntranceTableSimplified.ContainsKey(key))
                    EntranceTableSimplified.Add(key, record);
            }

            int lastScene = -1;
            RomFile scene_File = null;
            BinaryReader sceneStr = null;


            foreach (EntranceTableRecord record in EntranceTableSimplified.Values.OrderBy(x => x.Scene))
            {
                if (sceneStr != null)
                    sceneStr.BaseStream.Position = 0;
                if (record.Scene != lastScene)
                {
                    if (record.Scene >= 101)
                    {
                        WriteResult(result, record, -1, ResultType.Crash, "No Scene");
                        continue;
                    }
                    scene_File = N0.Files.GetSceneFile(record.Scene);
                    sceneStr = new BinaryReader(scene_File.Stream);
                    SegmentTable[2] = N0_SceneEndAddr - scene_File.Record.VRom.Size;
                }

                //First, 0x18 command
                byte cmdId = sceneStr.ReadByte();
                sceneStr.BaseStream.Position--;

                List<AlternateSetup> setups = new List<AlternateSetup>();

                if (cmdId == 0x18)
                {
                    sceneStr.BaseStream.Position += 4;
                    int headerListOffset = sceneStr.ReadBigInt32() & 0xFFFFFF;
                    sceneStr.BaseStream.Position = headerListOffset + 0xC;
                    for (int i = 0; i < 0xD; i++)
                    {
                        int data = sceneStr.ReadBigInt32();
                        setups.Add(new AlternateSetup(i, data));
                    }
                }
                else
                    setups.Add(new AlternateSetup(-1, 0x02000000));

                //parse headers
                foreach(var setup in setups)
                {
                    SceneHeader sceneHeader = new SceneHeader();
                    FaroresTest ft = FaroresTest.NA;

                    //resolve header start
                    if (setup.SegmentAddress.Segment != 2 
                        || !(setup.SegmentAddress.Offset < scene_File.Record.VRom.Size))
                    {
                        WriteResult(result, record, setup.SceneSetup, ResultType.Crash_Likely,
                            UnresolvedAddress("Scene Setup", setup.SegmentAddress));
                        continue;
                    }
                    //set header start
                    sceneStr.BaseStream.Position = setup.SegmentAddress.Offset;

                    int loop = 32;

                    while (loop > 0)
                    {
                        loop--;
                        cmdId = sceneStr.ReadByte();
                        sceneStr.BaseStream.Position--;

                        switch (cmdId)
                        {
                            case 0x14:
                                if (sceneHeader.Cutscene == 0)
                                    WriteResult(result, record, setup.SceneSetup, ResultType.Cutscene_Pointer, "No Known Issues", ft);
                                else
                                    WriteResult(result, record, setup.SceneSetup, ResultType.Cutscene, "No Known Issues", ft);
                                loop = -1; break;
                            case 0x04: //room definitions
                                {
                                    sceneStr.BaseStream.Position += 1;
                                    sceneHeader.Rooms = sceneStr.ReadByte();
                                    sceneStr.BaseStream.Position += 2;
                                    sceneHeader.RoomsAddress = sceneStr.ReadBigInt32();
                                    break;
                                }
                            case 0x06: //entrance index definitions
                                {
                                    sceneStr.BaseStream.Position += 4;
                                    sceneHeader.EntranceIndexDefinitionsAddress = sceneStr.ReadBigInt32();
                                    break;
                                }
                            case 0x00: //Link spawns definitions
                                {
                                    long seekBack;
                                    sceneStr.BaseStream.Position += 1;
                                    sceneHeader.LinkSpawns = sceneStr.ReadByte();
                                    sceneStr.BaseStream.Position += 2;
                                    sceneHeader.LinkSpawnsAddress = sceneStr.ReadBigInt32();

                                    //start resolving things here I guess
                                    SegmentAddress selectEntDefAddr = sceneHeader.EntranceIndexDefinitionsAddress
                                        + (record.Spawn << 1);

                                   
                                    if (selectEntDefAddr.Segment != 2 &&
                                        selectEntDefAddr.Offset > scene_File.Record.VRom.Size)
                                    {
                                        WriteResult(result, record, setup.SceneSetup, ResultType.Crash_Likely,
                                            UnresolvedAddress("Entrance Definitions", selectEntDefAddr));
                                        loop = -1; break;
                                    }

                                    seekBack = sceneStr.BaseStream.Position;

                                    sceneStr.BaseStream.Position = selectEntDefAddr.Offset;
                                    int spawnId = sceneStr.ReadByte();
                                    int mapId = sceneStr.ReadByte();

                                    //test if Link Spawn is invalid (phase 1)
                                    SegmentAddress selectSpawnAddr = sceneHeader.LinkSpawnsAddress + (spawnId << 4);

                                    if (selectSpawnAddr.Segment != 2 &&
                                        selectSpawnAddr.Offset > scene_File.Record.VRom.Size)
                                    {
                                        WriteResult(result, record, setup.SceneSetup, ResultType.Crash_Likely,
                                            UnresolvedAddress("Link Spawn", selectSpawnAddr), ft);
                                        loop = -1; break;
                                    }
                                    
                                    //test if Map Id is invalid, making FW mandatory
                                    if (!(mapId < sceneHeader.Rooms))
                                    {
                                        WriteResult(result, record, setup.SceneSetup, ResultType.Crash,
                                            string.Format("Invalid Room Id ({0})", mapId), FaroresTest.Without);
                                        ft = FaroresTest.With;
                                        //Don't break because we can continue parsing for FW purposes
                                    }

                                    //Check if Link spawn is valid (phase 2)
                                    sceneStr.BaseStream.Position = selectSpawnAddr.Offset;
                                    var actorId = sceneStr.ReadBigInt16();
                                    int linkActorVarsRomAddr = actorId * 0x20 + 0x14;
                                    linkActorVarsRomAddr += actorTableRomAddress;
                                    var linkActorVarsRelOff = code_File.Record.GetRelativeAddress(linkActorVarsRomAddr);

                                    //pointer to Link's Actor vars can't be resolved
                                    if (linkActorVarsRelOff < 0 ||
                                        !(linkActorVarsRelOff < code_File.Record.VRom.Size))
                                    {
                                        WriteResult(result, record, setup.SceneSetup, ResultType.Crash_Likely,
                                            UnresolvedAddress("Link Actor Var Pointer", (int)(linkActorVarsRelOff + N0_CodeAddr)), ft);
                                        loop = -1; break;
                                    }

                                    codeStr.BaseStream.Position = linkActorVarsRelOff;
                                    int linkActorVarsWriteAddr = codeStr.ReadBigInt32();
                                    int linkActorVarsWriteOff = (linkActorVarsWriteAddr - N0_CodeAddr + 8) & 0xFFFFFF;

                                    //pointer to where to update Link's object number can't be resolved
                                    if (linkActorVarsWriteOff < 0 ||
                                        !(linkActorVarsWriteOff < code_File.Record.VRom.Size))
                                    {
                                        WriteResult(result, record, setup.SceneSetup, ResultType.Crash_Likely,
                                            UnresolvedAddress("Link Object Dependency Write", linkActorVarsWriteAddr + 8), ft);
                                        loop = -1; break;
                                    }

                                    //check if pointer is going to do an unaligned write
                                    if (linkActorVarsWriteAddr %2 == 1)
                                    {
                                        WriteResult(result, record, setup.SceneSetup, ResultType.Crash,
                                            string.Format("N64: Unaligned SH write to address {0:X8}", linkActorVarsWriteAddr + 8), ft);
                                    }
                                    sceneStr.BaseStream.Position = seekBack;
                                    break;
                                }
                            case 0x17:
                                {
                                    sceneStr.BaseStream.Position += 4;
                                    sceneHeader.Cutscene = sceneStr.ReadBigInt32();
                                    break;
                                }
                            default: sceneStr.BaseStream.Position += 8; break;
                        }
                        if (loop == 0)
                            WriteResult(result, record, setup.SceneSetup, ResultType.Error, "Header Parse Error");
                    }
                }
            }
            face.OutputText(result.ToString());
            return result;
        }

        private static string UnresolvedAddress(string v, SegmentAddress address)
        {
            return $"Unresolved {v} Address: {address}";
        }

        private static void WriteResult(StringBuilder result, EntranceTableRecord record, int setup,
            ResultType type, string description,
            FaroresTest fwTest = FaroresTest.NA)
        {
            result.AppendFormat("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                record.Scene, record.Spawn, setup,
                (int)fwTest,//fwTest.ToString(),
                (int)type,//type.ToString().Replace('_', ' '),
                description);
            result.AppendLine();
        }

        class SceneHeader
        {
            public int Rooms;
            public SegmentAddress RoomsAddress = 0;
            public SegmentAddress EntranceIndexDefinitionsAddress = 0;
            public int LinkSpawns;
            public SegmentAddress LinkSpawnsAddress = 0;
            public SegmentAddress Cutscene = 0;
        }


        enum ResultType
        {
            Error,
            Crash,
            Crash_Likely,
            Cutscene_Pointer,
            Cutscene,
            Success,
        }

        enum FaroresTest
        {
            NA = 0,
            Without = 1,
            With = 2
        }

        class AlternateSetup
        {
            public int SceneSetup;
            public SegmentAddress SegmentAddress;
            public AlternateSetup(int setup, int segment)
            {
                SceneSetup = setup;
                SegmentAddress = segment;
            }
        }
    }
}
