using mzxrules.OcaLib;
using mzxrules.OcaLib.SceneRoom;
using mzxrules.OcaLib.SceneRoom.Commands;
using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Experimental
{
    class BuildDR2
    {
        static int FileTable_Off;
        static int SceneTable_Start;

        //get scene file from mqd folder
        //get file size
        //add file to location ref by FileTable_Open
        //update file table record after FileTable_Open
        //add maps
        //update root scene map addresses

        /// <summary>
        /// Imports a select set of scenes into a designated rom
        /// </summary>
        /// <param name="romfile">The rom being modified</param>
        /// <param name="version">The target version of the rom</param>
        /// <param name="importScenes">the scenes to import</param>
        public static void ImportScenesIntoUncompressedRom(string romfile, RomVersion version, List<int> importScenes)
        {
            FileTable_Off = Addresser.GetRom(ORom.FileList.dmadata, version, AddressToken.Scenes_Start);
            SceneTable_Start = Addresser.GetRom(ORom.FileList.code, version, AddressToken.SceneTable_Start);

            int NextWriteAddress;

            using (FileStream fs_r = new FileStream(romfile, FileMode.Open, FileAccess.ReadWrite))
            {
                BinaryReader addrReader = new BinaryReader(fs_r);
                addrReader.BaseStream.Position = FileTable_Off;
                NextWriteAddress = Endian.ConvertInt32(addrReader.ReadInt32());
                BinaryWriter bw = new BinaryWriter(fs_r);

                //wipe the scene table
                for (int i = 0; i < 101; i++)
                    UpdateSceneTable(bw, i, new FileAddress(0,0));

                foreach (int sceneIndex in importScenes)
                {
                    AddSceneAndRooms(sceneIndex, NextWriteAddress, bw);
                }
            }
        }

        private static void UpdateSceneTable(BinaryWriter bw, int scene, FileAddress addr)
        {
                //update the scene table
                bw.BaseStream.Position = SceneTable_Start + (scene * 5 * sizeof(int));
                bw.Write(Endian.ConvertInt32(addr.Start));
                bw.Write(Endian.ConvertInt32(addr.End));
        }

        private static void AddSceneAndRooms(int sceneId, int NextWriteAddress, BinaryWriter bw)
        {
            Scene scene;
            int mapCount;
            List<FileAddress> mapAddresses = new();
            FileAddress fileAddr;

            //get scene file from mqd folder
            using (FileStream fs_s = new FileStream(string.Format("dr2/{0:D2}_h", sceneId), FileMode.Open, FileAccess.Read))
            {
                scene = SceneRoomReader.InitializeScene(Game.OcarinaOfTime, sceneId, new BinaryReader(fs_s));
                //add the file to the rom
                fileAddr = AddFile(bw, fs_s, ref NextWriteAddress);
         
                //pass the new file address to the scene for later
                scene.VirtualAddress = fileAddr;

                UpdateSceneTable(bw, sceneId, fileAddr);
            }
            mapCount = scene.Header.GetRoomAddresses().Count;

            for (int i = 0; i < mapCount; i++)
            {
                using (FileStream fs_m = new FileStream(string.Format("dr2/{0:D2}_{1:D2}", sceneId, i),
                    FileMode.Open, FileAccess.Read))
                {
                    mapAddresses.Add(AddFile(bw, fs_m, ref NextWriteAddress));
                }
            }
            UpdateMapList(bw, scene, mapAddresses);
        }

        private static void UpdateFileTable(BinaryWriter bw, FileAddress fileAddr)
        {
            bw.BaseStream.Position = FileTable_Off;
            bw.Write(Endian.ConvertInt32(fileAddr.Start));
            bw.Write(Endian.ConvertInt32(fileAddr.End));
            bw.Write(Endian.ConvertInt32(fileAddr.Start));
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

            result = new FileAddress(WriteAddress, fs.Length + WriteAddress);
            //scrub out unused values

            //set CurFileAddress to the next open address
            WriteAddress = (((int)result.End / 0x1000) + 1) * 0x1000;

            //update file table
            UpdateFileTable(bw, result);
            return result;
        }


        private static void UpdateMapList(BinaryWriter bw, Scene scene, List<FileAddress> addr)
        {
            RoomListCommand ml;

            ml = (RoomListCommand)scene.Header[HeaderCommands.RoomList];

            //update map list
            bw.BaseStream.Position = scene.VirtualAddress.Start + ml.RoomListAddress;
            foreach (FileAddress map in addr)
            {
                bw.Write(Endian.ConvertInt32(map.Start));
                bw.Write(Endian.ConvertInt32(map.End));
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
                        //update map list
                        bw.BaseStream.Position = scene.VirtualAddress.Start + ml.RoomListAddress;
                        foreach (FileAddress map in addr)
                        {
                            bw.Write(Endian.ConvertInt32((int)map.Start));
                            bw.Write(Endian.ConvertInt32((int)map.End));
                        }
                    }
                }
            }
        }
    }
}
