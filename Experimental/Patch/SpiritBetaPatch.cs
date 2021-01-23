using mzxrules.Helper;
using mzxrules.OcaLib;
using mzxrules.OcaLib.SceneRoom;
using mzxrules.OcaLib.SceneRoom.Commands;
using System.Collections.Generic;
using System.IO;

namespace Experimental
{
    class SpiritBetaPatch
    {
        public static void Export(string file, ORom.Build build)
        {
            OFileTable rom = new OFileTable(file, build);
            Scene spirit;
            ExportModifiedScene(rom, out spirit, "06_h");
            RoomListCommand roomCommand = (RoomListCommand)spirit.Header[HeaderCommands.RoomList];
            for (int i = 0; i < 29; i++)
            {
                BinaryReader br;

                Room sRoom = new Room(Game.OcarinaOfTime, roomCommand.RoomAddresses[i]);
                Room beta = new Room(Game.OcarinaOfTime, new FileAddress());
                br = new BinaryReader(rom.GetFile(sRoom.VirtualAddress));

                sRoom.Header.Load(br, 0);
                beta.Header.Load(br, SpiritHack.GetBetaRoomSetupOffset(0, i));

                List<SceneCommand> cmd = beta.Header.Commands();
                SceneCommand objectCmd = sRoom.Header[HeaderCommands.ObjectList];
                int index = cmd.FindIndex(x => x.Code == (int)HeaderCommands.ActorList);

                if (index > -1)
                    cmd.Insert(index, objectCmd);

                using (BinaryWriter bw = new BinaryWriter(new FileStream($"r/06_{i:D2}", FileMode.CreateNew)))
                {
                    beta.Header.WriteHeader(bw);
                    br.BaseStream.Position = bw.BaseStream.Position;

                    while (br.BaseStream.Position < br.BaseStream.Length)
                    {
                        bw.Write(br.ReadUInt32());
                    }
                }
            }

        }

        private static void ExportModifiedScene(OFileTable rom, out Scene spirit, string filename)
        {
            BinaryReader br;

            spirit = new Scene(Game.OcarinaOfTime, 6, new FileAddress(0, 0));
            Scene beta = new Scene(Game.OcarinaOfTime, 6, new FileAddress(0, 0));

            br = new BinaryReader(rom.GetFile(rom.GetSceneVirtualAddress(6)));

            spirit.Header.Load(br, 0);
            beta.Header.Load(br, SpiritHack.GetBetaSceneSetupOffset(0));

            using (BinaryWriter bw = new BinaryWriter(new FileStream($"r/{filename}", FileMode.Create)))
            {
                beta.Header.WriteHeader(bw);
                br.BaseStream.Position = bw.BaseStream.Position;

                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    bw.Write(br.ReadUInt32());
                }
            }
            spirit.Header.InitializeAssets(br);
        }

        public static void Import(string file, ORom.Build build)
        {
            Gen.SceneImporter.ImportToUncompressedRom(file, build, "r", new List<int>(new int[] { 81, 6 }));
            CRC.Write(file);
        }

    }
}
