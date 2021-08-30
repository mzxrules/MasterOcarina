using mzxrules.OcaLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mzxrules.Helper;

namespace Experimental.Data
{
    static partial class Get
    {
        public static void MMEntranceTable(IExperimentFace face, List<string> file)
        {
            MRom rom = new(file[0], MRom.Build.U0);

            //scenes (7 bits, 0x6E max)
            //entrance sets (5 bits, 32 max)

            //entrance setups(32 max)
            StringBuilder sb = new();
            RomFile code = rom.Files.GetFile(MRom.FileList.code);
            BinaryReader br = new(code);
            int sceneBase;

            //Sets of entrance records per scene
            int entranceSetsPointerAddr;
            uint entranceSetsPointer;
            int entranceSetsAddr;

            //Single set of entrance records (single entrance)
            int eNumPointerAddr;
            uint eNumPointer;
            int eNumAddr;

            //get rom address of sceneTableBase
            sceneBase = Addresser.GetRom(MRom.FileList.code, rom.Version, AddressToken.EntranceIndexTable_Start);

            //for every scene
            for (int scene = 0; scene < 0x6E; scene++)
            {
                //get offset of pointer to the entrance sets
                entranceSetsPointerAddr = sceneBase + (sizeof(int) * 3) * scene + 4;

                //move the stream to the entrance sets pointer
                br.BaseStream.Position = code.Record.GetRelativeAddress(entranceSetsPointerAddr);

                //read the entranceSetsPointer (scene)
                entranceSetsPointer = br.ReadBigUInt32();

                //if invalid
                if (! IsPointer(entranceSetsPointer)
                    || !Addresser.TryGetRom
                    (MRom.FileList.code, rom.Version, entranceSetsPointer, out entranceSetsAddr)
                    || code.Record.GetRelativeAddress(entranceSetsAddr) >= code.Record.VRom.End)
                {
                    //entrance index base, offset, sptr, eptr, entb1,2,3,4
                    sb.AppendFormat("{0:X4},{1},{2:X8},{3:X8},{4:X2},{5:X2},{6:X2},{7:X2}",
                        GetEntranceIndex(scene, 0),
                        0,
                        entranceSetsPointer,
                        0,
                        255, 0, 0, 0);
                    sb.AppendLine();
                    continue;
                }

                //entranceSetsAddr now contains the rom address to the first entrance set pointer

                //for every theoretical entrance set
                for (int entranceSet = 0; entranceSet < 32; entranceSet++)
                {
                    eNumPointerAddr = entranceSetsAddr + (sizeof(UInt32) * entranceSet);

                    //move the stream to the entrance set pointer
                    br.BaseStream.Position = code.Record.GetRelativeAddress(eNumPointerAddr);

                    //read the entranceSetPointer (entrance set)
                    eNumPointer = br.ReadBigUInt32();

                    //if invalid
                    if (!IsPointer(eNumPointer)
                        || !Addresser.TryGetRom
                        (MRom.FileList.code, rom.Version, eNumPointer, out eNumAddr)
                        || code.Record.GetRelativeAddress(eNumAddr) >= code.Record.VRom.End)
                    {
                        //entrance index base, offset, sptr, eptr, entb1,2,3,4
                        sb.AppendFormat("{0:X4},{1},{2:X8},{3:X8},{4:X2},{5:X2},{6:X2},{7:X2}",
                            GetEntranceIndex(scene, entranceSet),
                            0,
                            entranceSetsPointer,
                            eNumPointer,
                            255, 0, 0, 0);
                        sb.AppendLine();
                        continue;
                    }
                    //eNumAddr is valid

                    br.BaseStream.Position = code.Record.GetRelativeAddress(eNumAddr);
                    for (int entrance = 0; entrance < 32; entrance++)
                    {
                        //entrance index base, offset, sptr, eptr, entb1,2,3,4
                        sb.AppendFormat("{0:X4},{1},{2:X8},{3:X8},{4:X2},{5:X2},{6:X2},{7:X2}",
                            GetEntranceIndex(scene, entranceSet),
                            entrance,
                            entranceSetsPointer,
                            eNumPointer,
                            br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte());
                        sb.AppendLine();
                    }
                }
            }
            face.OutputText(sb.ToString());
        }

        private static bool IsPointer(uint pointer)
        {
            return (pointer >> 24) == 0x80;
        }


        private static ushort GetEntranceIndex(int scene, int entranceSet)
        {
            return (ushort)(((byte)scene << 9) + ((byte)entranceSet << 4));
        }
        //private void Funct(MRom.Build version, BinaryReader code, FileAddress codeAddr, int entranceTableAddr)
        //{
        //    sbyte sceneIndex;
        //    uint EntranceRecord;
        //    int entranceAddr;


            

        //    //Capture pointer
        //    if (!Addresser.TryConvertToRom(MRom.FileList.code, version, (uint)ReadInt32(entranceTableAddr), out entranceAddr)
        //        || !Addresser.TryConvertToRom(MRom.FileList.code, version, (uint)ReadInt32(entranceAddr), out entranceAddr))
        //    {
        //        return null;
        //    }
        //    EntranceRecord = (uint)ReadInt32(entranceAddr);

        //    sceneIndex = (sbyte)(EntranceRecord >> 24);
        //    sceneIndex = Math.Abs(sceneIndex);

        //    return (byte?)sceneIndex;
        //}

    }
}
