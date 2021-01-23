using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using mzxrules.Helper;
using System.IO;
using mzxrules.OcaLib.SceneRoom;
using mzxrules.OcaLib.SceneRoom.Commands;
using mzxrules.OcaLib;
using mzxrules.OcaLib.Maps;
using System.Runtime.Serialization.Json;

namespace Experimental.Data
{
    static partial class Get
    {
        public static void MQRandoCompareHeaders(IExperimentFace face, List<string> filePath)
            => MQJson.MQRandoCompareHeaders(face, filePath);

        public static void CompareCollision(IExperimentFace face, List<string> filePath)
            => MQJson.CompareCollision(face, filePath); 

        public static void OutputMQJson(IExperimentFace face, List<string> filePath)
            => MQJson.OutputMQJson(face, filePath);


        public static void MQJsonImportAndPatch(IExperimentFace face, List<string> filePath)
            => MQJson.MQJsonImportAndPatch(face, filePath);

        public static void ImportMapData(IExperimentFace face, List<string> filePath)
            => MQJson.ImportMapData(face, filePath);

    }
    class MQJson
    {
        [DataContract(Name = "File")]
        class File_MQJson
        {
            [DataMember(Order = 1)]
            public string Name { get; set; }

            [DataMember(Order = 2)]
            public string Start { get; set; }

            [DataMember(Order = 3)]
            public string End { get; set; }

            [DataMember(Order = 4)]
            public string RemapStart { get; set; }
        }


        [DataContract(Name = "Scene")]
        class Scene_MQJson
        {
            [DataMember(Order = 1)]
            public File_MQJson File { get; set; }

            [DataMember(Order = 2)]
            public int Id { get; set; }

            [DataMember(Order = 3)]
            List<string> TActors { get; set; } = new List<string>();

            [DataMember(Order = 4)]
            List<Path_MQJson> Paths { get; set; } = new List<Path_MQJson>();

            public int RoomsCount = 0;

            public SegmentAddress RoomsAddress = 0;

            [DataMember(Order = 5)]
            public List<Room_MQJson> Rooms { get; set; } = new List<Room_MQJson>();

            [DataMember(Order = 6)]
            public Col_MQJson ColDelta { get; set; }

            [DataMember(Order = 7)]
            public List<DungeonFloor> FloorMaps { get; set; }

            [DataMember(Order = 8)]
            public List<DungeonMinimap> Minimaps { get; set; }


            public Scene_MQJson(BinaryReader br, int id, int start, int end)
            {
                File = new File_MQJson()
                {
                    Name = $"Scene {id}",
                    Start = start.ToString("X8"),
                    End = end.ToString("X8"),
                };

                Id = id;

                SceneWord cmd = new SceneWord();
                do
                {
                    br.Read(cmd, 0, 8);

                    var seekback = br.BaseStream.Position;
                    HeaderCommands code = (HeaderCommands)cmd.Code;


                    if (code == HeaderCommands.PathList)
                    {
                        SegmentAddress offset = cmd.Data2;
                        br.BaseStream.Position = offset.Offset;

                        InitPaths(br);
                    }
                    else if (code == HeaderCommands.TransitionActorList)
                    {
                        SegmentAddress actorOff = cmd.Data2;
                        br.BaseStream.Position = actorOff.Offset;

                        for (int i = 0; i < cmd.Data1; i++)
                        {
                            TActors.Add(Actor_MQJson.Read(br));
                        }
                    }
                    if (code == HeaderCommands.RoomList)
                    {
                        RoomsCount = (int)cmd.Data1;
                        RoomsAddress = cmd.Data2;
                    }

                    br.BaseStream.Position = seekback;
                }
                while ((HeaderCommands)cmd.Code != HeaderCommands.End);
            }

            private void InitPaths(BinaryReader br)
            {
                int loop = 0;
                do
                {
                    sbyte nodes = br.ReadSByte();
                    br.BaseStream.Position += 3;
                    SegmentAddress address = br.ReadBigInt32();

                    if (nodes > 0
                        && loop < 20
                        && (address.Offset < 0x2F_FFFF) //assuming an address range of 0200 0000 to 022F FFFF, quite larger than expected
                        && address.Segment == 0x02)
                    {
                        var seekback = br.BaseStream.Position;
                        Path_MQJson path = new Path_MQJson();
                        br.BaseStream.Position = address.Offset;
                        for (int i = 0; i < nodes; i++)
                        {
                            var point = new short[] { br.ReadBigInt16(), br.ReadBigInt16(), br.ReadBigInt16() };
                            path.Points.Add(point);
                        }
                        Paths.Add(path);
                        br.BaseStream.Position = seekback;

                        loop++;
                    }
                    else
                    {
                        break;
                    }
                }
                while (true);
            }
        }

        [DataContract(Name = "Path")]
        class Path_MQJson
        {
            [DataMember]
            public List<short[]> Points { get; set; } = new List<short[]>();
        }

        [DataContract(Name = "Room")]
        class Room_MQJson
        {
            [DataMember(Order = 1)]
            public File_MQJson File { get; set; }

            [DataMember(Order = 2)]
            public int Id { get; set; }

            [DataMember(Order = 3)]
            List<string> Objects { get; set; } = new List<string>();

            [DataMember(Order = 4)]
            List<string> Actors { get; set; } = new List<string>();

            public Room_MQJson() { }

            public Room_MQJson(BinaryReader br, int sceneId, int roomId, int start, int end)
            {
                File = new File_MQJson()
                {
                    Name = $"Scene {sceneId}, Room {roomId}",
                    Start = start.ToString("X8"),
                    End = end.ToString("X8"),
                };

                Id = roomId;

                SceneWord cmd = new SceneWord();
                do
                {
                    br.Read(cmd, 0, 8);

                    var seekback = br.BaseStream.Position;

                    if ((HeaderCommands)cmd.Code == HeaderCommands.ObjectList)
                    {
                        SegmentAddress offset = cmd.Data2;
                        br.BaseStream.Position = offset.Offset;

                        for (int i = 0; i < cmd.Data1; i++)
                        {
                            Objects.Add(br.ReadBigInt16().ToString("X4"));
                        }
                    }
                    else if ((HeaderCommands)cmd.Code == HeaderCommands.ActorList)
                    {
                        SegmentAddress actorOff = cmd.Data2;
                        br.BaseStream.Position = actorOff.Offset;

                        for (int i = 0; i < cmd.Data1; i++)
                        {
                            Actors.Add(Actor_MQJson.Read(br));
                        }
                    }

                    br.BaseStream.Position = seekback;
                }
                while ((HeaderCommands)cmd.Code != HeaderCommands.End);
            }

        }

        [DataContract(Name = "CollisionDelta")]
        class Col_MQJson
        {
            [DataMember(Order = 1)]
            public bool IsLarger { get; set; }

            [DataMember(Order = 2)]
            public List<ColPoly_MQJson> Polys = new List<ColPoly_MQJson>();

            [DataMember(Order = 3)]
            public List<ColMat_MQJson> PolyTypes = new List<ColMat_MQJson>();

            [DataMember(Order = 4)]
            public List<ColCam_MQJson> Cams = new List<ColCam_MQJson>();

            public Col_MQJson(CollisionMesh n0, CollisionMesh mq)
            {
                IsLarger = (n0.GetFileSize() < mq.GetFileSize());

                for (int i = 0; i < mq.PolyList.Count; i++)
                {
                    var n0poly = n0.PolyList[i];
                    var mqpoly = mq.PolyList[i];

                    if (!n0poly.Equals(mqpoly))
                    {
                        Polys.Add(new ColPoly_MQJson(i, mqpoly.Type, mqpoly.VertexFlagsA));
                    }
                }

                for (int i = 0; i < mq.PolyTypeList.Count; i++)
                {
                    var mqpolytype = mq.PolyTypeList[i];

                    if (!(i < n0.PolyTypeList.Count)
                        || !n0.PolyTypeList[i].Equals(mqpolytype))
                    {
                        PolyTypes.Add(new ColMat_MQJson(i, mqpolytype.HighWord, mqpolytype.LowWord));
                    }
                }

                for (int i = 0; i < mq.CameraDataList.Count; i++)
                {
                    var mqcam = mq.CameraDataList[i];
                    ColCam_MQJson cam = null;
                    if (mqcam.PositionAddress == 0)
                    {
                        cam = new ColCam_MQJson(mqcam.CameraS, mqcam.NumCameras, -1);
                    }
                    else
                    {
                        for (int j = 0; j < n0.CameraDataList.Count; j++)
                        {
                            var n0cam = n0.CameraDataList[j];
                            if (n0cam.IsPositionListIdentical(mqcam))
                            {
                                cam = new ColCam_MQJson(mqcam.CameraS, mqcam.NumCameras, j);
                                break;
                            }
                        }
                    }
                    if (cam == null)
                    {
                        throw new Exception("BLERG");
                    }
                    Cams.Add(cam);
                }
            }

        }

        [DataContract(Name = "Poly")]
        class ColPoly_MQJson
        {
            [DataMember(Order = 1)]
            public int Id { get; set; }

            [DataMember(Order = 2)]
            public int Type { get; set; }

            [DataMember(Order = 3)]
            public int Flags { get; set; }

            public ColPoly_MQJson(int id, int type, int ex)
            {
                Id = id;
                Type = type;
                Flags = ex;
            }
        }

        [DataContract(Name = "PolyType")]
        class ColMat_MQJson
        {
            [DataMember(Order = 1)]
            public int Id { get; set; }

            [DataMember(Order = 2)]
            public int High { get; set; }

            [DataMember(Order = 3)]
            public int Low { get; set; }

            public ColMat_MQJson(int id, int hi, int lo)
            {
                Id = id;
                High = hi;
                Low = lo;
            }
        }

        [DataContract(Name = "Cam")]
        class ColCam_MQJson
        {
            [DataMember(Order = 1)]
            public int Data { get; set; }

            [DataMember(Order = 2)]
            public int PositionIndex { get; set; }

            public ColCam_MQJson(short cam, short num, int positionIndex)
            {
                Data = (cam << 16) + num;
                PositionIndex = positionIndex;
            }
        }


        static class Actor_MQJson
        {
            public static string Read(BinaryReader read)
            {
                List<string> result = new List<string>();
                for (int i = 0; i < 8; i++)
                {
                    string v = read.ReadBigInt16().ToString("X4");
                    result.Add(v);
                }

                return string.Join(" ", result);
            }
        }


        static readonly int[] MQRandoScenes = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 13 };

        public static void MQRandoCompareHeaders(IExperimentFace face, List<string> filePath)
        {
            Rom n0 = new ORom(filePath[0], ORom.Build.N0);
            Rom mq = new ORom(filePath[1], ORom.Build.MQU);

            StringWriter sw = new StringWriter();

            foreach (Rom rom in new Rom[] { n0, mq })
            {
                for (int i = 0; i < 16; i++)
                {
                    var scn = rom.Files.GetSceneFile(i);
                    BinaryReader br = new BinaryReader(scn);

                    SceneWord cmd;
                    SceneWord roomCmd = new SceneWord();
                    bool hasRoom = false;
                    do
                    {
                        cmd = new SceneWord();
                        br.Read(cmd, 0, 8);
                        //sw.WriteLine($"{i} -1 {cmd}");

                        if ((HeaderCommands)cmd.Code == HeaderCommands.RoomList)
                        {
                            hasRoom = true;
                            roomCmd = cmd;
                        }
                    }
                    while ((HeaderCommands)cmd.Code != HeaderCommands.End);

                    if (hasRoom)
                    {
                        var seekback = br.BaseStream.Position;

                        br.BaseStream.Position = ((SegmentAddress)roomCmd.Data2).Offset;
                        for (int j = 0; j < roomCmd.Data1; j++)
                        {
                            var roomAddr = br.ReadBigInt32();
                            br.ReadBigInt32();

                            var roomFile = rom.Files.GetFile(roomAddr);
                            BinaryReader brRoom = new BinaryReader(roomFile);
                            do
                            {
                                cmd = new SceneWord();
                                brRoom.Read(cmd, 0, 8);
                                //sw.WriteLine($"{i} {j} {cmd}");

                                if ((HeaderCommands)cmd.Code == HeaderCommands.ActorList)
                                {
                                    var va = roomFile.Record.VRom;
                                    sw.WriteLine($"{i} {j} {va.Start:X8} {va.End:X8} {(cmd.Data1 * 0x10):X8}");
                                }
                            }
                            while ((HeaderCommands)cmd.Code != HeaderCommands.End);
                        }
                        br.BaseStream.Position = seekback;
                    }
                }
            }
            face.OutputText(sw.ToString());
        }

        public static void OutputMQJson(IExperimentFace face, List<string> filePath)
        {
            Rom n0 = new ORom(filePath[0], ORom.Build.N0);
            Rom mq = new ORom(filePath[1], ORom.Build.MQU);


            var rom_scenes = new List<List<Scene_MQJson>>();
            List<Scene_MQJson> scenes;

            foreach (Rom rom in new Rom[] { n0, mq })
            {
                scenes = new List<Scene_MQJson>();
                foreach (int sceneId in MQRandoScenes)
                {
                    if (sceneId == 12)
                        continue;

                    var sceneFile = rom.Files.GetSceneFile(sceneId);
                    using (BinaryReader br = new BinaryReader(sceneFile))
                    {
                        var va = sceneFile.Record.VRom;
                        var scene = new Scene_MQJson(br, sceneId, va.Start, va.End);

                        br.BaseStream.Position = scene.RoomsAddress.Offset;

                        for (int roomId = 0; roomId < scene.RoomsCount; roomId++)
                        {
                            int start = br.ReadBigInt32();
                            int end = br.ReadBigInt32();

                            var roomFile = rom.Files.GetFile(start);

                            var room = new Room_MQJson(new BinaryReader(roomFile), sceneId, roomId, start, end);
                            scene.Rooms.Add(room);
                        }
                        scenes.Add(scene);
                    }
                }
                rom_scenes.Add(scenes);
            }

            var n0_scenes = rom_scenes[0];
            scenes = rom_scenes[1];
            SetN0Addresses(scenes, n0_scenes);

            MemoryStream ms = SerializeScenes(scenes);
            StreamReader sr = new StreamReader(ms);
            string json = sr.ReadToEnd();

            face.OutputText(json);

        }

        private static MemoryStream SerializeScenes(List<Scene_MQJson> scenes)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<Scene_MQJson>));
            MemoryStream ms = new MemoryStream();

            ser.WriteObject(ms, scenes);
            ms.Position = 0;
            return ms;
        }

        public static void MQJsonImportAndPatch(IExperimentFace face, List<string> filePath)
        {
            Rom n0 = new ORom(filePath[0], ORom.Build.N0);
            Rom mq = new ORom(filePath[1], ORom.Build.MQU);

            List<Scene_MQJson> scenes = Load_Scene_MQJson();

            foreach (var scene in scenes)
            {
                if (!MQRandoScenes.Contains(scene.Id))
                    continue;

                var n0_scene = SceneRoomReader.InitializeScene(n0, scene.Id);
                var mq_scene = SceneRoomReader.InitializeScene(mq, scene.Id);

                CollisionMesh n0_mesh = ((CollisionCommand)n0_scene.Header[HeaderCommands.Collision]).Mesh;
                CollisionMesh mq_mesh = ((CollisionCommand)mq_scene.Header[HeaderCommands.Collision]).Mesh;

                var delta = new Col_MQJson(n0_mesh, mq_mesh);
                scene.ColDelta = delta;
            }
            MemoryStream s = SerializeScenes(scenes);
            StreamReader sr = new StreamReader(s);
            face.OutputText(sr.ReadToEnd());

        }

        private static List<Scene_MQJson> Load_Scene_MQJson()
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<Scene_MQJson>));

            List<Scene_MQJson> scenes;
            using (FileStream fs = new FileStream("mqu.json", FileMode.Open))
            {
                scenes = (List<Scene_MQJson>)ser.ReadObject(fs);
            }

            return scenes;
        }

        static void PrintList<T>(StringBuilder sb, List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                sb.AppendLine($"{i:X4} {item.ToString()}");
            }
        }

        public static void CompareCollision(IExperimentFace face, List<string> filePath)
        {
            Rom n0 = new ORom(filePath[0], ORom.Build.N0);
            Rom mq = new ORom(filePath[1], ORom.Build.MQU);

            StringBuilder sb_n0 = new StringBuilder();
            StringBuilder sb_mq = new StringBuilder();

            foreach (int id in MQRandoScenes)
            {
                var n0_scene = SceneRoomReader.InitializeScene(n0, id);
                var mq_scene = SceneRoomReader.InitializeScene(mq, id);

                CollisionMesh n0_mesh = ((CollisionCommand)n0_scene.Header[HeaderCommands.Collision]).Mesh;
                CollisionMesh mq_mesh = ((CollisionCommand)mq_scene.Header[HeaderCommands.Collision]).Mesh;

                sb_n0.AppendLine($"Scene {id}");
                sb_mq.AppendLine($"Scene {id}");

                sb_n0.AppendLine(n0_mesh.Print());
                sb_n0.AppendLine();

                sb_n0.AppendLine(mq_mesh.Print());
                sb_n0.AppendLine();

                //PrintList(sb_n0, n0_mesh.CameraDataList);
                //PrintList(sb_n0, mq_mesh.CameraDataList);

                //PrintList(sb_n0, n0_mesh.WaterBoxList);
                //PrintList(sb_n0, mq_mesh.WaterBoxList);


                //for (int i = 0; i < n0_mesh.VertexList.Count; i++)
                //{
                //    var vertN0 = n0_mesh.VertexList[i];
                //    var vertMQ = mq_mesh.VertexList[i];

                //    if (vertN0 != vertMQ)
                //    {
                //        sb_n0.AppendLine($"{i:X4}: {vertN0}");
                //        sb_mq.AppendLine($"{i:X4}: {vertMQ}");
                //    }
                //}

                //for (int i = 0; i < n0_mesh.Polys; i++)
                //{
                //    var n0poly = n0_mesh.PolyList[i];
                //    var mqpoly = mq_mesh.PolyList[i];
                //    if (!n0poly.Equals(mqpoly))
                //    {
                //        if (n0poly.VertexFlagsC != mqpoly.VertexFlagsC
                //            || n0poly.VertexFlagsB != mqpoly.VertexFlagsB)
                //        {
                //            sb_n0.AppendLine($"{i:X4}: {n0poly}");
                //            sb_mq.AppendLine($"{i:X4}: {mqpoly}");
                //        }
                //    }
                //}

                //sb_mq.AppendLine();
            }
            string result = sb_n0.ToString()
                + $"{Environment.NewLine}~SPLIT{Environment.NewLine}"
                + sb_mq.ToString();
            face.OutputText(result);

        }

        private static void SetN0Addresses(List<Scene_MQJson> scenes, List<Scene_MQJson> n0_scenes)
        {
            for (int i = 0; i < scenes.Count; i++)
            {
                var sA = scenes[i];
                var sB = n0_scenes[i];

                sA.File.Start = sB.File.Start;
                sA.File.End = sB.File.End;

                for (int j = 0; j < sA.Rooms.Count; j++)
                {
                    var rA = sA.Rooms[j];
                    var rB = sB.Rooms[j];

                    rA.File.Start = rB.File.Start;
                    rA.File.End = rB.File.End;
                }
            }
        }



        public static void ImportMapData(IExperimentFace face, List<string> files)
        {
            Rom n0 = new ORom(files[0], ORom.Build.N0);
            Rom mq = new ORom(files[1], ORom.Build.MQU);

            var scenes = Load_Scene_MQJson();

            const int dung_mark_vrom_mq = 0xBE78D8;
            N64Ptr dung_mark_vram_mq = 0x8085CFF8;

            var Minimaps = GetDungeonMinimaps(mq, dung_mark_vrom_mq, dung_mark_vram_mq);

            //const int floor_index_n0 = 0xB6C934;
            //const int floor_data_n0 = 0xBC7E00;

            const int floor_index_mq = 0xB6AFC4;
            const int floor_data_mq = 0xBB49E0;

            var FloorMap = GetDungeonFloorData(mq, floor_index_mq, floor_data_mq);

            for (int i = 0; i < 10; i++)
            {
                scenes[i].FloorMaps = FloorMap[i];
                scenes[i].Minimaps = Minimaps[i];
            }

            var ms = SerializeScenes(scenes);

            StreamReader sr = new StreamReader(ms);
            
            face.OutputText(sr.ReadToEnd());
        }

        private static List<List<DungeonMinimap>> GetDungeonMinimaps(Rom rom, int dung_mark_vrom, N64Ptr dung_mark_vram)
        {
            //uint map_mark_vram_n0 = 0x808567F0;
            //uint map_mark_rom_n0 = 0xBF40D0;
            //uint map_mark_table_n0 = 0xBFABBC;
            //uint minimaps_per_dungeon_n0 = 0xB6C794;
            List<List<DungeonMinimap>> result = new List<List<DungeonMinimap>>();

            var map_mark_record = rom.Files.GetFileStart(dung_mark_vrom).VRom;
            var map_mark_file = rom.Files.GetFile(map_mark_record);
            var dung_mark_off = map_mark_file.Record.GetRelativeAddress(dung_mark_vrom);
            N64Ptr map_mark_vram_start = (dung_mark_vram - dung_mark_off);

            List<N64Ptr> dung_ptr_data = new List<N64Ptr>();

            using (BinaryReader br = new BinaryReader(map_mark_file))
            {
                br.BaseStream.Position = dung_mark_off;
                for (int i = 0; i < 10; i++)
                {
                    dung_ptr_data.Add(br.ReadBigInt32());
                }

                dung_ptr_data.Add(dung_mark_vram);

                for (int i = 0; i < 10; i++)
                {
                    List<DungeonMinimap> m = new List<DungeonMinimap>();
                    var minimaps = (dung_ptr_data[i + 1] - dung_ptr_data[i]) / 0x72;

                    br.BaseStream.Position = dung_ptr_data[i] - map_mark_vram_start;

                    for (int j = 0; j < minimaps; j++)
                    {
                        m.Add(new DungeonMinimap(br));
                    }
                    result.Add(m);
                }
            }
            return result;
        }

        private static List<List<DungeonFloor>> GetDungeonFloorData(Rom rom, int floor_index_vrom, int floor_data_vrom)
        {
            const int max_floors = (0x42 / 2) + 1;
            List<short> floorIndices = new List<short>();

            List<List<DungeonFloor>> result = new List<List<DungeonFloor>>();

            var code_file = rom.Files.GetFile(ORom.FileList.code);
            using (BinaryReader br = new BinaryReader(code_file))
            {
                br.BaseStream.Position = code_file.Record.GetRelativeAddress(floor_index_vrom);
                for (int i = 0; i < 10; i++)
                {
                    short index = (short)(br.ReadBigInt16() / 2);
                    floorIndices.Add(index);
                }
            }

            floorIndices.Add(max_floors);

            var kaliedo_addr = rom.Files.GetPlayPauseAddress(0);
            var kaliedo_file = rom.Files.GetFile(kaliedo_addr);

            List<DungeonFloor> floors = new List<DungeonFloor>();

            //read dungeons
            using (BinaryReader br = new BinaryReader(kaliedo_file))
            {
                br.BaseStream.Position = kaliedo_file.Record.GetRelativeAddress(floor_data_vrom);
                for (int i = 0; i < max_floors; i++)
                {
                    floors.Add(new DungeonFloor(br));
                }
            }

            //build dungeon list
            for (int sceneId = 0; sceneId < floorIndices.Count-1; sceneId++)
            {
                int index = floorIndices[sceneId];
                int end = floorIndices[sceneId + 1];

                List<DungeonFloor> dung = new List<DungeonFloor>();

                for (int i = index; i < end; i++)
                {
                    dung.Add(floors[i]);
                }
                result.Add(dung);
            }

            return result;
        }



        public static void GetActors(IExperimentFace face, List<string> files)
        {

        }
    }
}
