using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Experimental.Data
{
    static partial class Get
    {
        static class MM3d
        {
            public static class RAM
            {
                const int fileAddr = 0x03AEB5E4;
                const int ramAddr = 0x0069B348;

                public static int GetFileRelAddr(int value)
                {
                    return value - ramAddr + fileAddr;
                }
            }
            public class EntranceRecord
            {
                public short Id;
                public long Addr;
                public byte Scene;
                public byte Spawn;
                public short Params;

                public EntranceRecord(short id)
                {
                    this.Id = id;
                }
                public EntranceRecord(short id, BinaryReader br)
                {
                    this.Id = id;
                    Addr = br.BaseStream.Position;
                    Scene = br.ReadByte();
                    Spawn = br.ReadByte();
                    Params = br.ReadInt16();
                }

                public override string ToString()
                {
                    return string.Format("{3:X4},{0:X2},{1:X2},{2:X4},{4:X8}",
                        Scene, Spawn, Params, Id, Addr);
                }
            }

            public class EntranceIndexRecord
            {
                public int scene;
                public int Entrances;
                public int TablePointer;
                public int UnknownPointer;

                public EntranceIndexRecord(int index, BinaryReader br)
                {
                    scene = index;
                    Entrances = br.ReadInt32();
                    TablePointer = br.ReadInt32();
                    UnknownPointer = br.ReadInt32();
                }

                public override string ToString()
                {
                    //"Scene: {0}, Entrances {1}, Pointer {2:X8}, Unknown
                    return string.Format("{0},{1},{2:X8},{3:X8}",
                        scene, Entrances, TablePointer, UnknownPointer);
                }

            }

            public static short EntranceIndex(int scene, int ent, int setup)
            {
                scene &= 0x7F;
                ent &= 0x1F;
                setup &= 0xF;
                return (short)((scene << 9) + (ent << 4) + setup);
            }

            public class GetItemRecord
            {
                public short Id;
                public byte Item;
                public byte Flag;
                public short ModelId;
                public short TextId;
                public short ObjectId;

                public GetItemRecord(short index, BinaryReader br)
                {
                    Id = index;
                    Item = br.ReadByte();
                    Flag = br.ReadByte();
                    ModelId = br.ReadInt16();
                    TextId = br.ReadInt16();
                    ObjectId = br.ReadInt16();
                }

                public override string ToString()
                {
                    return string.Format("{0},{1:X4},{2:X2},{3:X2},{4:X4},{5:X4},{6:X4}",
                        Id, Id,
                        Item,
                        Flag,
                        ModelId,
                        TextId,
                        ObjectId);
                }
            }

        }
        public static void MM3DEntranceTable(IExperimentFace face, List<string> file)
        {
            const int Entrance_Index_Table = 0x69FB44;
            const int SceneCount = 0x70;

            List<MM3d.EntranceIndexRecord> eiRecord = new List<MM3d.EntranceIndexRecord>();

            using (FileStream fs = new FileStream(file[0], FileMode.Open, FileAccess.Read))
            {
                BinaryReader br = new BinaryReader(fs);

                br.BaseStream.Position = MM3d.RAM.GetFileRelAddr(Entrance_Index_Table);

                for (int scene = 0; scene < SceneCount; scene++)
                {
                    eiRecord.Add(new MM3d.EntranceIndexRecord(scene, br));
                }

                StringBuilder sb = new StringBuilder();
                List<MM3d.EntranceRecord> entRecord = new List<MM3d.EntranceRecord>();

                //For every external scene
                foreach (MM3d.EntranceIndexRecord record in eiRecord)
                {
                    //if there are no entrances, don't bother going deeper
                    if (record.Entrances == 0)
                    {
                        entRecord.Add(new MM3d.EntranceRecord(MM3d.EntranceIndex(record.scene, 0, 0)));
                        continue;
                    }

                    int listPtr = MM3d.RAM.GetFileRelAddr(record.TablePointer);

                    for (int ent = 0; ent < record.Entrances; ent++)
                    {
                        br.BaseStream.Position = listPtr + (ent * 4);

                        int innerPtr = MM3d.RAM.GetFileRelAddr(br.ReadInt32());

                        for (int setup = 0; setup < 0x10; setup++)
                        {
                            var EntranceIndex = MM3d.EntranceIndex(record.scene, ent, setup);
                            try
                            {
                                br.BaseStream.Position = innerPtr + (setup * 4);
                                var temp = new MM3d.EntranceRecord(EntranceIndex, br);
                                entRecord.Add(temp);
                            }
                            catch
                            {
                                entRecord.Add(new MM3d.EntranceRecord(EntranceIndex));
                            }
                        }
                    }
                }

                foreach (MM3d.EntranceRecord record in entRecord)
                {
                    sb.AppendLine(record.ToString());
                }
                face.OutputText(sb.ToString());

            }
        }
    }
}
