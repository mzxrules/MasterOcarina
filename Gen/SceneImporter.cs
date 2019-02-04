using mzxrules.Helper;
using mzxrules.OcaLib;
using mzxrules.OcaLib.SceneRoom;
using mzxrules.OcaLib.SceneRoom.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gen
{
    public static class SceneImporter
    {
        static int FileTable_Off;
        static int SceneTable_Start;

        static Rom dumpRom;

        //get scene file from mqd folder
        //get file size
        //add file to location ref by FileTable_Open
        //update file table record after FileTable_Open
        //add rooms
        //update root scene's room addresses

        /// <summary>
        /// Imports a select set of scenes into a designated rom
        /// </summary>
        /// <param name="romfile">The rom being modified</param>
        /// <param name="scenesLocation">The location of the scene/room files</param>
        /// <param name="version">The target version of the rom</param>
        /// <param name="importScenes">the scenes to import</param>
        public static void ImportToUncompressedRom(string romfile, RomVersion version, string sceneFilesLocation, List<int> importScenes)
        {
            FileTable_Off = Addresser.GetRom(ORom.FileList.dmadata, version, "Scenes_Start");
            SceneTable_Start = Addresser.GetRom(ORom.FileList.code, version, "SceneTable_Start");

            int NextWriteAddress;

            using (FileStream fs_r = new FileStream(romfile, FileMode.Open, FileAccess.ReadWrite))
            {
                BinaryReader addrReader = new BinaryReader(fs_r);
                addrReader.BaseStream.Position = FileTable_Off;
                NextWriteAddress = addrReader.ReadBigInt32();
                BinaryWriter bw = new BinaryWriter(fs_r);

                //wipe the scene table
                for (int i = 0; i < 101; i++)
                    UpdateSceneTable(bw, i, new FileAddress(0,0));

                foreach (int sceneIndex in importScenes)
                {
                    AddSceneAndRooms(sceneIndex, ref NextWriteAddress, sceneFilesLocation, bw);
                }
            }
        }

        private static void UpdateSceneTable(BinaryWriter bw, int scene, FileAddress addr)
        {
                //update the scene table
                bw.BaseStream.Position = SceneTable_Start + (scene * 5 * sizeof(int));
                bw.WriteBig(addr.Start);
                bw.WriteBig(addr.End);
        }

        private static void AddSceneAndRooms(int sceneId, ref int NextWriteAddress, string fileLocation, BinaryWriter bw)
        {
            Scene scene;
            int roomCount;
            List<FileAddress> roomAddresses = new List<FileAddress>();
            FileAddress fileAddr;

            //get scene file from mqd folder
            using (FileStream fs_s = new FileStream(string.Format("{0}/{1:D2}_h", fileLocation, sceneId),
                FileMode.Open, FileAccess.Read))
            {
                scene = SceneRoomReader.InitializeScene(Game.OcarinaOfTime, sceneId, new BinaryReader(fs_s));
                //add the file to the rom
                fileAddr = AddFile(bw, fs_s, ref NextWriteAddress);

                //pass the new file address to the scene for later
                scene.VirtualAddress = fileAddr;

                UpdateSceneTable(bw, sceneId, fileAddr);
            }
            roomCount = scene.Header.GetRoomAddresses().Count;

            for (int i = 0; i < roomCount; i++)
            {
                using (FileStream fs_m = new FileStream(string.Format("{0}/{1:D2}_{2:D2}",fileLocation, sceneId, i),
                    FileMode.Open, FileAccess.Read))
                {
                    roomAddresses.Add(AddFile(bw, fs_m, ref NextWriteAddress));
                }
            }
            UpdateRoomList(bw, scene, roomAddresses);
        }

        private static void UpdateFileTable(BinaryWriter bw, FileAddress fileAddr)
        {
            bw.BaseStream.Position = FileTable_Off;
            bw.WriteBig(fileAddr.Start);
            bw.WriteBig(fileAddr.End);
            bw.WriteBig(fileAddr.Start);
            FileTable_Off += 0x10;
        }

        /// <summary>
        /// Adds the given file to the rom, and updates the file table
        /// </summary>
        /// <param name="bw">binary writer that references the rom</param>
        /// <param name="fs">file stream of the file</param>
        /// <param name="WriteAddress">Address to write the file to</param>
        /// <returns></returns>
        private static FileAddress AddFile(BinaryWriter bw, FileStream fs, ref int WriteAddress)
        {
            FileAddress result;

            bw.BaseStream.Position = WriteAddress;
            fs.Position = 0;
            fs.CopyTo(bw.BaseStream);

            result = new FileAddress(WriteAddress,
                fs.Length + WriteAddress);
            //scrub out unused values

            //Set WriteAddress to the next open address
            WriteAddress = (result.End + 0xFFF) & (-0x1000);

            //update file table
            UpdateFileTable(bw, result);
            return result;
        }


        private static void UpdateRoomList(BinaryWriter bw, Scene scene, List<FileAddress> addr)
        {
            RoomListCommand ml;

            ml = (RoomListCommand)scene.Header[HeaderCommands.RoomList];

            //update room list
            bw.BaseStream.Position = scene.VirtualAddress.Start + ml.RoomListAddress;
            foreach (FileAddress room in addr)
            {
                bw.WriteBig(room.Start);
                bw.WriteBig(room.End);
            }

            //check if scene has alt headers
            if (scene.Header.HasAlternateHeaders())
            {
                //update all alternate headers
                foreach (SceneHeader header in scene.Header.Alternate.Headers) 
                {
                    if (header != null)
                    {
                        ml = (RoomListCommand)header[HeaderCommands.RoomList];
                        //update room list
                        bw.BaseStream.Position = scene.VirtualAddress.Start + ml.RoomListAddress;
                        foreach (FileAddress room in addr)
                        {
                            bw.WriteBig(room.Start);
                            bw.WriteBig(room.End);
                        }
                    }
                }
            }
        }

        //int[] keepscenes = new int[] { 46, 59, 61, 67, 71, 79, 80, 81, 95 };
        //System.IO.Directory.CreateDirectory("mqd");
        public static void DumpScenesAndRooms(Rom rom, string folderLocation,  List<int> scenes)
        {
            dumpRom = rom;
            //SceneRoomReader.LocalFileTable = new OFileTable(rom.Files, rom.Version);
            foreach (int i in scenes)
            {
                DumpSceneAndRooms(dumpRom, i,folderLocation);
            }
        }

        private static void DumpSceneAndRooms(Rom rom, int i, string folderLocation)
        {
            Scene scene;
            StreamWriter sw;
            Stream br;
            RomFile file;

            file = rom.Files.GetSceneFile(i);

            scene = SceneRoomReader.InitializeScene(file, i);
            br = file;
            sw = new StreamWriter(Path.Combine(folderLocation, string.Format("{0:D2}_h", i)));

            br.CopyTo(sw.BaseStream);
            sw.Close();

            int rNum = 0;
            foreach (FileAddress addr in scene.Header.GetRoomAddresses())
            {
                br =   rom.Files.GetFile(addr.Start);
                sw = new StreamWriter(Path.Combine(folderLocation, string.Format("{0:D2}_{1:D2}", i, rNum)));
                br.CopyTo(sw.BaseStream);
                sw.Close();
                rNum++;
            }
        }
    }
}
