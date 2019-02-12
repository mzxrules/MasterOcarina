using System;
using System.IO;
using mzxrules.Helper;

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
                    sw.Write(Util.GetExclusions(br.ReadBytes((int)br.BaseStream.Length), dmadata));
                }
            }
        }

        private static void CompressRom(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Invalid args");
            }
            CompressTask g0;

            using (StreamReader reader = new StreamReader(args[1]))
            {
                g0 = new CompressTask(reader);
            }

            using (BinaryReader reader = new BinaryReader(File.OpenRead(args[2])))
            {
                byte[] file = reader.ReadBytes((int)reader.BaseStream.Length);
                using (BinaryWriter writer = new BinaryWriter(File.OpenWrite("comp.z64")))
                {
                    byte[] compressed = new byte[0x400_0000];
                    int size = Util.Compress(file, compressed, g0);
                    Span<byte> final = (size <= 0x200_0000)? new Span<byte>(compressed,0, 0x200_0000): compressed;
                    writer.Write(final);
                }
            }
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
                    const int STATIC_SEGMENT = 0x400_0000 - 0x10000;

                    g0.Dmadata = STATIC_SEGMENT + 0x40;
                    g1.Dmadata = g0.Dmadata + 0x6200;

                    byte[] compressed = new byte[0x400_0000];
                    ReadOnlySpan<byte> file = reader.ReadBytes((int)reader.BaseStream.Length);

                    Console.WriteLine("Compressing G0");
                    int cur = Util.Compress(file, compressed, g0);

                    Console.WriteLine($"Compressing G1, cur = {cur:X8}");
                    cur = Util.Compress(file, compressed, g1, cur);

                    Console.WriteLine($"Compression Complete, cur = {cur:X8}");

                    reader.Seek(STATIC_SEGMENT);
                    Span<byte> header = reader.ReadBytes(0x40);
                    Span<byte> comp = compressed;
                    header.CopyTo(comp.Slice(STATIC_SEGMENT, 0x40));
                    writer.Write(compressed);
                }
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine("excl <dmadata addr> <excl file> <output path>");
            Console.WriteLine("comp <excl path> <output path>");
            Console.WriteLine("compdual <g0 excl path> <g1 excl path> <output path>");
        }
    }
}
