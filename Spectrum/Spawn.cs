using System.Collections.Generic;
using System.IO;

namespace Spectrum
{
    class SpawnData
    {
        public static List<EntranceIndex> Indexes = new List<EntranceIndex>();
        public static List<Spawn> Spawns = new List<Spawn>();
        public static void Load()
        {
            using (StreamReader sw = new StreamReader("data/Spawn.txt"))
            {
                while (sw.Peek() >= 0)
                {
                    string s = sw.ReadLine();
                    string[] line = s.Split(new char[] { '\t' });
                    if (line.Length != 9)
                        continue;
                    Spawn spawn = new Spawn(
                        int.Parse(line[0]),
                        int.Parse(line[1]),
                        int.Parse(line[2]),
                        byte.Parse(line[3]),
                        (short)int.Parse(line[5]),
                        (short)int.Parse(line[6]),
                        (short)int.Parse(line[7]),
                        (short)int.Parse(line[8]));

                    Spawns.Add(spawn);
                }
            }
            using (StreamReader sw = new StreamReader("data/Index.txt"))
            {
                while (sw.Peek() >= 0)
                {
                    string s = sw.ReadLine();
                    var line = s.Split(new char[] { '\t' });
                    if (line.Length != 5)
                        continue;

                    EntranceIndex eIndex = new EntranceIndex(
                        int.Parse(line[0]),
                        int.Parse(line[1]),
                        short.Parse(line[2]));

                    Indexes.Add(eIndex);
                }
            }
        }
    }

    public struct EntranceIndex
    {
        public int scene;
        public int ent;
        public short id;

        public EntranceIndex(int s, int e, short i)
        {
            scene = s;
            ent = e;
            id = i;
        }
    }

    public struct Spawn
    {
        public int scene;
        public int setup;
        public int ent;
        public byte room;
        public float x, y, z;
        public short rot;

        public Spawn(int sc, int set, int e, byte room,
            short x, short y, short z,
            short rot)
        {
            scene = sc;
            setup = set;
            ent = e;
            this.room = room;
            this.x = x;
            this.y = y;
            this.z = z;
            this.rot = rot;
        }
    }
}
