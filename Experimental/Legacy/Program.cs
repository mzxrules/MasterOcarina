using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Scripts
{
    internal class Script_Global
    {
        public static string DecompressedRomLoc = @"C:\Users\mzxrules\Roms\N64\Games\Ocarina\Legend of Zelda, Ocarina of Time (U) (V1.0) [!]\OcarinaV10_d.z64";
        public static string CompressedRomLoc = @"C:\Users\mzxrules\Roms\N64\Games\Ocarina\Legend of Zelda, Ocarina of Time (U) (V1.0) [!]\Ocarina (V1.0).z64";
        public static string DebugRomLoc = @"C:\Users\mzxrules\Roms\N64\Games\Ocarina\Rom Hacks\ootdebug2\ZELOOTMA.z64";
    }
    class Script_Program
    {
        class RomFileAddress { }
        const long EntranceIndexTable = 0x00B9F360;

        static void TestForChecksum()
        {
            List<int> PossibleChecksums = new List<int>();
            ushort checksum = 0;
            ushort read;

            using (BinaryReader fs = new BinaryReader(new FileStream("save01.bin", FileMode.Open, FileAccess.Read)))
            {
                using (StreamWriter sw = new StreamWriter("out.txt"))
                {
                    for (int i = 0; i < 20; i++)
                    {
                        checksum += fs.ReadUInt16();
                    }
                    while (fs.BaseStream.Position < fs.BaseStream.Length)
                    {
                        read = fs.ReadUInt16();
                        if (checksum == read)
                        {
                            PossibleChecksums.Add((int)fs.BaseStream.Position);
                        }
                        checksum += read;
                    }
                    checksum += 1;
                    checksum -= 1;
                    foreach (int item in PossibleChecksums)
                    {
                        sw.WriteLine(string.Format("{0:X4}", item));
                    }
                }
            }
        }

        static void WriteEntranceIndexTableToFile()
        {
            using (FileStream fs = new FileStream(Script_Global.DebugRomLoc, FileMode.Open))
            {
                using (StreamWriter sw = new StreamWriter("entranceindex.txt"))
                {
                    sw.Write(GetEntranceList(fs));
                }
            }
        }
        public static string GetEntranceList(FileStream sr)
        {
            StringBuilder result = new StringBuilder();
            byte[] word = new byte[4];

            sr.Seek(EntranceIndexTable, SeekOrigin.Begin);

            for (int i = 0; i < 0x614; i++)
            {
                sr.Read(word, 0, 4);
                result.AppendFormat("{0},{1},{2:X2},{3:X2}",
                    word[0],
                    word[1],
                    word[2],
                    word[3]);
                result.AppendLine();

            }
            return result.ToString();
        }
        public static string GetReducedEntranceList(FileStream sr)
        {
            StringBuilder result = new StringBuilder();
            byte[] word = new byte[4];
            byte[] last = new byte[2];
            ushort lastIndex;

            last[0] = 0;
            last[1] = 0;
            lastIndex = 0;
            //result.AppendLine("0,0");
            sr.Seek(EntranceIndexTable, SeekOrigin.Begin);

            for (int i = 0; i < 0x614; i++)
            {
                sr.Read(word, 0, 4);
                if (word[0] != last[0] || word[1] != last[1])
                {
                    result.AppendFormat("{0},{1},{2}",
                        lastIndex,
                        last[0],
                        last[1]);
                    result.AppendLine();

                    lastIndex = (ushort)i;
                    last[0] = word[0];
                    last[1] = word[1];

                }
            }
            result.AppendFormat("{0},{1},{2}",
                lastIndex,
                last[0],
                last[1]);
            result.AppendLine();
            return result.ToString();
        }

        private static void ConvertTabDelimitedRecordsToBinary()
        {
            StreamReader sr;
            FileStream fs;
            List<RomFileAddress> addressList = new List<RomFileAddress>();
            char[] separator = { '\t' };
            string[] fileAddress = new string[2];

            sr = new StreamReader("Scenefile.csv");
            fs = new FileStream("out2.dat", FileMode.Create);
            while (sr.Peek() > -1)
            {
                fileAddress = sr.ReadLine().Split(separator);
                //addressList.Add(new RomFileAddress(
                //    long.Parse(fileAddress[0], System.Globalization.NumberStyles.HexNumber),
                //    long.Parse(fileAddress[1], System.Globalization.NumberStyles.HexNumber)));
                for (int i = 0; i < 2; i++)
                {
                    //WriteReverseBytes(fs, int.Parse(fileAddress[i], System.Globalization.NumberStyles.HexNumber));
                }
            }
            sr.Close();
            fs.Close();
        }

        const int SceneCount = 101;
        private void ReadSceneTableFromRam()
        {
            using (FileStream fs = new FileStream("Ocarina10FileTable.dat", FileMode.Open))
            {
                byte[] array = new byte[fs.Length];
                fs.Read(array, 0, (int)fs.Length);

                int[] intArray = new int[SceneCount * 5];
                Buffer.BlockCopy(array, 0, intArray, 0, intArray.Length * sizeof(int));

                for (int i = 0; i < intArray.Length; i += 5)
                {
                    Console.WriteLine(string.Format("{0}, {1:X8}, {2:X8}, {3:X8}, {4:X8}, {5:X8}",
                        intArray[i],
                        intArray[i],
                        intArray[i + 1],
                        intArray[i + 2],
                        intArray[i + 3],
                        intArray[i + 4]));
                }
            }
        }
    }
}
