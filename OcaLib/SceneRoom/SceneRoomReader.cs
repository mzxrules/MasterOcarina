using mzxrules.Helper;
using mzxrules.OcaLib.Actor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace mzxrules.OcaLib.SceneRoom
{
    /// <summary>
    /// A gross class that was written to initially manage VerboseOcarina's core functionality
    /// </summary>
    public static class SceneRoomReader
    {
        struct SceneRoomSetup
        {
            public int Scene;
            public int Room;
            public int Setup;

            public SceneRoomSetup(int scene, int room, int setup)
            {
                Scene = scene;
                Room = room;
                Setup = setup;
            }
        }

        public static Scene InitializeScene(Rom rom, int id)
        {
            var file = rom.Files.GetSceneFile(id);
            return InitializeScene(file, id);
        }

        public static Scene InitializeScene(RomFile file, int id)
        {
            Scene scene = null;

            if (file == null)
                return scene;

            scene = new Scene(file.Version.Game, id, file.Record.VirtualAddress);
            BinaryReader br = new BinaryReader(file);

            //if (LocalFileTable.Version == ORom.Build.N0
            //    && number == 6)
            //    SpiritHack.LoadSpiritSceneHeader(br, scene);
            //else
            //try
            //{
                LoadISceneRoomHeader(br, scene);
            //}
            //catch
            //{
            //    scene = null;
            //}
            return scene;
        }

        public static Scene InitializeScene(Game game, int id, BinaryReader br)
        {
            Scene scene;
            scene = new Scene(game, id, new FileAddress());
            LoadISceneRoomHeader(br, scene);
            return scene;
        }

        public static Room InitializeRoom(RomFile file)
        {
            BinaryReader br;
            Room newRoom = new Room(file.Version.Game, file.Record.VirtualAddress);

            br = new BinaryReader(file);
            LoadISceneRoomHeader(br, newRoom);
            return newRoom;
        }

        //public static Room LoadSpiritRoom(FileAddress addr, int roomNo)
        //{
        //    BinaryReader br;
        //    Room item = new Room(LocalFileTable.Version.Game, addr);

        //    br = new BinaryReader(LocalFileTable.GetFile(item.VirtualAddress.Start));
        //    SpiritHack.LoadSpiritRoomHeader(br, item, roomNo);
        //    return item;
        //}

        #region InitializeMembers

        private static void LoadISceneRoomHeader(BinaryReader br, ISceneRoomHeader item)
        {
            SceneHeader header;
            header = item.Header;

            //Load the root header
            header.Load(br, 0);
            header.InitializeAssets(br);

            if (header.HasAlternateHeaders())
            {
                for (int i = 0; i < header.Alternate.HeaderList.Count; i++)
                {
                    if (header.Alternate.HeaderList[i] != null)
                    {
                        header.Alternate.HeaderList[i].Load(br, header.Alternate.HeaderOffsetsList[i]);
                        header.Alternate.HeaderList[i].InitializeAssets(br);
                    }
                }
            }
        }

        #endregion

        public static bool TryGetCutscene(Rom rom, long address, out Cutscenes.Cutscene cutscene)
        {
            FileRecord addr;
            cutscene = null;

            addr = rom.Files.GetFileStart(address);
            if (addr == null)
                return false;
            var s = (Stream)rom.Files.GetFile(addr.VirtualAddress);
            s.Position = addr.GetRelativeAddress(address);
            cutscene = new Cutscenes.Cutscene(s);
            return true;
        }

        public static string ReadScene(Scene scene)
        {
            StringBuilder result = new StringBuilder();

            if (scene == null)
                return "NULL";

            result.AppendLine($"Scene at {scene.VirtualAddress.Start:X8}");
            result.Append(ReadHeader(scene.Header));

            return result.ToString();
        }

        public static string ReadRoom(Room room)
        {
            StringBuilder result = new StringBuilder();

            result.AppendLine($"Room at {room.VirtualAddress.Start:X8}");

            result.Append(ReadHeader(room.Header));
            return result.ToString();
        }

        private static string ReadHeader(SceneHeader Header)
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("Setup 0: 00000000");
            result.AppendLine(Header.Read());
            if (Header.HasAlternateHeaders())
            {
                var Alternate = Header.Alternate;
                for (int i = 0; i < Alternate.HeaderList.Count; i++)
                {
                    if (Alternate.HeaderList[i] == null)
                        continue;

                    if (i < 3)
                    {
                        result.Append($"Setup {(i + 1)}: ");
                    }
                    else
                    {
                        result.Append($"Cutscene {(i - 3)}: ");
                    }
                    result.AppendLine(Alternate.HeaderOffsetsList[i].ToString("X8"));
                    result.AppendLine(Alternate.HeaderList[i].Read());
                }
            }
            return result.ToString();
        }

        public static string GetEntranceCount(FileStream sr)
        {
            List<byte[]> list = new List<byte[]>();
            byte[] word = new byte[4];
            int foundIndex;
            StringBuilder result = new StringBuilder();

            sr.Seek(Addresser.GetRom(ORom.FileList.code, ORom.Build.DBGMQ, "EntranceIndexTable_Start"),
                SeekOrigin.Begin);
            for (int i = 0; i < 0x614; i++)
            {
                sr.Read(word, 0, 4);
                foundIndex = list.FindIndex(item => item[0] == word[0]);
                if (foundIndex == -1)
                {
                    list.Add(new byte[] { word[0], word[1] });
                }
                else if (list[foundIndex][1] < word[1])
                {
                    list[foundIndex][1] = word[1];
                }
            }
            foreach (byte[] item in list)
            {
                result.AppendLine($"{item[0]}, {(item[1] + 1)}");
            }
            return result.ToString();
        }

        public static string GetObjectsById(Rom r, int id)
        {
            string cols = "Scene,Setup,Room" + Environment.NewLine;
            return cols + PrintCollectionById(r, id, ObjectListPrintout);
        }

        public static string GetActorsById(Rom r, int id)
        {
            string cols = "Scene,Setup,Room";
            if (r.Version.Game == Game.MajorasMask)
            {
                cols += ",Id,Variable,Type,Description,x,y,z,rx,ry,rz,vrx,vry,vrz,Day Flags,Scene_0x1B";
            }
            return cols + Environment.NewLine + PrintCollectionById(r, id, ActorListPrintout);
        }

        public static string GetCommandsById(Rom r, int id)
        {
            string cols = "Scene,Setup,Room" + Environment.NewLine;
            return cols + PrintCollectionById(r, id, CommandListPrintout);
        }


        delegate int PrintList(int id, SceneHeader header, int sceneId, int roomId, StringBuilder result);

        private static string PrintCollectionById(Rom rom, int id, PrintList PrintListFunction)
        {
            Scene scene;
            List<FileAddress> roomAddresses;
            Room room;
            int sceneId;
            int roomId;
            StringBuilder result;
            int count = 0;

            result = new StringBuilder();
            for (sceneId = 0; sceneId < rom.Scenes; sceneId++)
            {
                //set room negative to denote scene level actors
                roomId = -1;

                //load scene
                var sceneFile = rom.Files.GetSceneFile(sceneId);

                scene = InitializeScene(sceneFile, sceneId);
                if (scene == null)
                    continue;

                //scene level actors
                count += PrintListFunction(id, scene.Header, sceneId, roomId, result);

                //load room list
                roomAddresses = scene.Header.GetRoomAddresses();

                for (roomId = 0; roomId < roomAddresses.Count; roomId++)
                {
                    try
                    {
                        room = InitializeRoom(rom.Files.GetFile(roomAddresses[roomId]));
                        count += PrintListFunction(id, room.Header, sceneId, roomId, result);
                    }
                    catch { }
                }
            }
            result.AppendLine($"Total: {count}");
            return result.ToString();
        }
        
        private static List<ItemLocation<T>> CreateCollectionById<T>(Rom rom, int id, CreateListThings<T> CreateThings)
        {
            Scene scene;
            List<FileAddress> roomAddresses;
            Room room;
            List<ItemLocation<T>> result = new List<ItemLocation<T>>();
            
            for (int sceneId = 0; sceneId < rom.Scenes; sceneId++)
            {
                //load scene
                var sceneFile = rom.Files.GetSceneFile(sceneId);

                scene = InitializeScene(sceneFile, sceneId);
                if (scene == null)
                    continue;

                //scene level actors
                //set room negative to denote scene level actors
                result.AddRange(CreateList<T>(id, scene.Header, sceneId, -1, CreateThings));

                //load room list
                roomAddresses = scene.Header.GetRoomAddresses();

                for (int roomId = 0; roomId < roomAddresses.Count; roomId++)
                {
                    try
                    {
                        room = InitializeRoom(rom.Files.GetFile(roomAddresses[roomId]));
                        result.AddRange(CreateList<T>(id, scene.Header, sceneId, roomId, CreateThings));
                    }
                    catch { }
                }
            }
            return result;
        }
        
        private static List<ItemLocation<T>> CreateList<T>(int id, SceneHeader header, int scene, int room, CreateListThings<T> del)
        {
            List<ItemLocation<T>> result = new List<ItemLocation<T>>();
            List<List<T>> test = del(id);

            for (int i = 0; i < test.Count; i++)
            {
                SceneRoomSetup srs = new SceneRoomSetup(scene, room, i);
                foreach (var item in test[i])
                    result.Add(new ItemLocation<T>(item, srs));
            }
            return result;
        }

        class ItemLocation<T>
        {
            public T Item { get; set; }
            public SceneRoomSetup Location { get; set; }

            public ItemLocation(T i, SceneRoomSetup loc)
            {
                Item = i;
                Location = loc;
            }
        }

        delegate List<List<T>> CreateListThings<T>(int id);


        private static int ObjectListPrintout(int id, SceneHeader header, int sceneId, int roomId, StringBuilder result)
        {
            List<List<ushort>> objectList = header.GetObjectsWithId(id);
            AppendObjectList(objectList, sceneId, roomId, result);
            return GetSublistCount(objectList);
        }

        private static int ActorListPrintout(int id, SceneHeader header, int sceneId, int roomId, StringBuilder result)
        {
            List<List<ActorSpawn>> actorList = header.GetActorsWithId(id);
            AppendActorList(actorList, sceneId, roomId, result);
            return GetSublistCount(actorList);
        }

        private static int CommandListPrintout(int id, SceneHeader header, int sceneId, int roomId, StringBuilder result)
        {
            List<List<SceneCommand>> commandList = header.GetAllCommandsWithId(id);
            AppendCommandList(commandList, sceneId, roomId, result);
            return GetSublistCount(commandList);
        }

        public static int GetSublistCount<T>(List<List<T>> list)
        {
            int count = 0;
            foreach (List<T> sublist in list)
            {
                count += sublist.Count;
            }
            return count;
        }

        private static void AppendActorList(List<List<ActorSpawn>> actorList, int scene, int room, StringBuilder result)
        {
            List<ActorSpawn> setupList;
            string locationStr;
            if (actorList.Count > 0)
            {
                for (int setup = 0; setup < actorList.Count; setup++)
                {
                    locationStr = $"{scene},{setup},{room},";
                    setupList = actorList[setup];
                    foreach (ActorSpawn actor in setupList)
                    {
                        result.AppendLine(locationStr + actor.PrintCommaDelimited());
                    }
                }
            }
        }

        private static void AppendObjectList(List<List<ushort>> objectList, int scene, int room, StringBuilder result)
        {
            List<ushort> setupList;
            string locationStr;
            if (objectList.Count > 0)
            {
                for (int setup = 0; setup < objectList.Count; setup++)
                {
                    setupList = objectList[setup];
                    var list = setupList.ConvertAll(new Converter<ushort, string>((x) => { return x.ToString("X4"); }));

                    if (setupList.Count == 0)
                        continue;
                    locationStr = $"{scene:D3},{setup:D2},{room:D2},";
                    result.AppendLine(locationStr + string.Join(" ", list));

                    //foreach (ushort obj in setupList)
                    //{
                    //    result.AppendLine(locationStr + obj.ToString("X4"));
                    //}
                }
            }
        }
        
        private static void AppendCommandList(List<List<SceneCommand>> commandList, int scene, int room, StringBuilder result)
        {
            List<SceneCommand> setupList;
            string locationStr;
            if (commandList.Count > 0)
            {
                for (int setup = 0; setup < commandList.Count; setup++)
                {
                    locationStr = $"{scene},{setup},{room},";
                    setupList = commandList[setup];
                    foreach (SceneCommand cmd in setupList)
                    {
                        result.AppendLine(locationStr + cmd.ToString());
                    }
                }
            }
        }
    }
}