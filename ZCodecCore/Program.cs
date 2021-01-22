using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using CommandLine.Text;
using mzxrules.Helper;

namespace ZCodecCore
{
    [Verb("excl", HelpText = "ass")]
    class ExclusionOptions
    {
        //Console.WriteLine("excl <dmadata addr> <excl file> <output path>");
        [Value(1, HelpText = "rom address of dmadata file")]
        string FileAddress { get; set; }

        [Value(2,
            HelpText = "path to generate exclusion file output",
            MetaName = "exclusion file")]
        string ExclusionPath { get; set; }

        [Value(3,
            HelpText = "path to generate exclusion file output",
            MetaName = "exclusion file")]
        string RomPath { get; set; }

    }

    [Verb("crc", HelpText = "updates the rom's crc")]
    class CrcOptions
    {

    }

    [Verb("compress", HelpText = "smaller ass")]
    class CompressOptions
    {

    }

    [Verb("compdual", HelpText = "compress the z64 dual rom")]
    class CompressDualOptions
    {

    }

    [Verb("decompress", HelpText = "decompresses a rom")]
    class DecompressOptions
    {

    }


    class Program
    {
        Dictionary<uint, FileEncoding> encoding = new Dictionary<uint, FileEncoding>()
        {
            [0x80371240] = FileEncoding.BigEndian32,
            [0x12408037] = FileEncoding.HalfwordSwap,
            [0x40123780] = FileEncoding.LittleEndian32,
            [0x37804012] = FileEncoding.LittleEndian16,
        };

        static readonly Type[] VerbTypes = new Type[] {
            typeof(ExclusionOptions),
            typeof(CrcOptions),
            typeof(CompressOptions),
            typeof(CompressDualOptions),
            typeof(DecompressOptions),
        };

        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                switch (args[0])
                {
                    case "exclusion": GenExclusionTable(args); return;
                    case "compress": CompressRom(args); return;
                    case "compdual": CompressDualRom(args); return;
                    default: PrintHelp(); return;
                }
            }
            PrintHelp();
        }

        private static void PrintHelp()
        {
            Console.WriteLine("comp <excl path> <output path>");
            Console.WriteLine("compdual <g0 excl path> <g1 excl path> <output path>");
        }

        private static void GenExclusionTable(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Invalid args");
            }

            int dmadata = int.Parse(args[1], System.Globalization.NumberStyles.HexNumber);

            var br = File.ReadAllBytes(args[3]);
            var exclusions = Util.GetExclusions(br, dmadata);
            File.WriteAllText(args[2], exclusions);
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
                byte[] compressed = new byte[0x400_0000];
                int size = Util.Compress(file, compressed, g0);
                Span<byte> final = (size <= 0x200_0000) ? new Span<byte>(compressed, 0, 0x200_0000) : compressed;

                using var writer = File.OpenWrite("comp.z64");
                writer.Write(final);
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

                using var writer = File.OpenWrite("dual.z64");
                writer.Write(compressed);
            }
        }
    }
}
