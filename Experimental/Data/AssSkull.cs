using mzxrules.OcaLib;
using mzxrules.OcaLib.SceneRoom;
using mzxrules.OcaLib.SceneRoom.Commands;
using mzxrules.Helper;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Experimental.Data
{
    static partial class Get
    {
        public static void AssSkulls(IExperimentFace face, List<string> filePath)
        {
            ORom r = new ORom(filePath[0], ORom.Build.N0);
            List<(FileAddress addr, int scene, int room, Room info)> lookup = new List<(FileAddress addr, int scene, int room, Room info)>();
            for (int sceneId = 0; sceneId < r.Scenes; sceneId++)
            {
                var sceneFile = r.Files.GetSceneFile(sceneId);
                Scene scene = SceneRoomReader.InitializeScene(sceneFile, sceneId);
                scene.Header.InitializeAssets(new BinaryReader(sceneFile.Stream));

                int roomId = -1;
                foreach (var roomAddr in scene.Header.GetRoomAddresses())
                {
                    roomId++;
                    var roomFile = r.Files.GetFile(roomAddr);
                    Room room = SceneRoomReader.InitializeRoom(roomFile);
                    lookup.Add((roomAddr, sceneId, roomId, room));
                }
            }
            //now for some ass

            StringBuilder sb = new StringBuilder();

            for (int i = ass.GetLowerBound(0); i <= ass.GetUpperBound(0); i++)
            {
                for (int j = ass.GetLowerBound(1); j <= ass.GetUpperBound(1); j++)
                {
                    foreach(var item in ass[i,j])
                    {
                        if (item is ACTOR actor)
                        {
                            var (addr, scene, room, info) = lookup.Where(x => x.addr.Start <= actor.addr && x.addr.End > actor.addr).Single();

                            int setup = 0;
                            if (info.Header.HasAlternateHeaders())
                            {
                                for(int hId = 0; hId < info.Header.Alternate.Headers.Count; hId++)
                                {
                                    var alt = info.Header.Alternate.Headers[hId];
                                    if (alt == null)
                                        continue;
                                    var actorList = alt[HeaderCommands.ActorList];
                                    if (actorList == null)
                                        continue;
                                    int actors = actorList.Command.Data1;
                                    SegmentAddress loc = actorList.Command.Data2;
                                    int actorsStart = addr.Start + loc.Offset;
                                    int actorsEnd = addr.End + loc.Offset + (0x10 * actors);

                                    if (actor.addr >= actorsStart && actor.addr < actorsEnd)
                                    {
                                        setup = hId + 1;
                                        break;
                                    }
                                }

                            }
                            
                            sb.AppendLine($"{i}\t{j}\t{scene}\t{room}\t{setup}\t{item.ToString()}");
                        }
                    }
                }
            }

            face.OutputText(sb.ToString());
        }
    

        class ACTOR
        {
            public int addr;
            public int id;
            public short x;
            private short y;
            private short z;
            private uint rx;
            private uint ry;
            private uint rz;
            private uint var;

            public ACTOR(int addr, int id, short x, short y, short z, uint rx, uint ry, uint rz, uint var)
            {
                this.addr = addr;
                this.id = id;
                this.x = x;
                this.y = y;
                this.z = z;
                this.rx = rx;
                this.ry = ry;
                this.rz = rz;
                this.var = var;
            }
            public override string ToString()
            {
                return $"{addr:X8}\t{id:X4}\t{x}\t{y}\t{z}\t{rx:X4}\t{ry:X4}\t{rz:X4}\t{var:X4}";
            }
        }

        private static ACTOR SetActor(int addr, int id, short x, short y, short z, uint rx, uint ry, uint rz, uint var)
        {
            return new ACTOR(addr, id, x, y, z, rx, ry, rz, var);
        }
        public static int OBJECT(int addr, int id)
        {
            return 0;
        }
        public static int PATCH(int addr)
        {
            return 0;
        }
        public static int PATCH2(int addr, int addr2)
        {
            return 0;
        }
        public static void AssFunc()
        {

            
        }

    }
}