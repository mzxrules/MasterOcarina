using System;
using System.IO;

namespace ZCodecCore
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                switch (args[0])
                {
                    case "excl": GenExclusionTable(args); return;
                    case "comp": CompressRom(args); return;
                    case "compdual": CompressDualRom(args); return;
                    default: PrintHelp(); return;
                }
            }
            PrintHelp();
        }

        private static void GenExclusionTable(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Invalid args");
            }

            int dmadata = int.Parse(args[1], System.Globalization.NumberStyles.HexNumber);

            using (BinaryReader br = new BinaryReader(File.OpenRead(args[3])))
            {
                using (StreamWriter sw = new StreamWriter(args[2]))
                {
                    sw.Write(Util.GetExclusions(br, dmadata));
                }
            }
        }

        private static void CompressRom(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Invalid args");
            }
            throw new NotImplementedException();
        }

        private static void CompressDualRom(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Invalid args");
            }

            CompressTask g0;
            CompressTask g1;

            using (StreamReader reader = new StreamReader(args[1]))
            {
                g0 = new CompressTask(reader);
            }
            using (StreamReader reader = new StreamReader(args[2]))
            {
                g1 = new CompressTask(reader);
            }

            using (BinaryReader reader = new BinaryReader(File.OpenRead(args[3])))
            {
                using (BinaryWriter writer = new BinaryWriter(File.OpenWrite("dual.z64")))
                {
                    Util.CompressDual(reader, writer, g0, g1);
                }
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine("excl <dmadata addr> <excl file> <path>");
            Console.WriteLine("comp <excl path> <path>");
            Console.WriteLine("compdual <g0 excl path> <g1 excl path> <path>");
        }
    }
}
