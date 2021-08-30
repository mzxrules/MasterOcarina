using Gen;
using mzxrules.OcaLib;
using mzxrules.Helper;
using System;
using System.IO;
using System.Linq;

namespace ZCodec
{
    class Program
    {
        public static bool PromptUser = false;
        static void Main(string[] args)
        {

            foreach (string s in args)
                Console.WriteLine(s);

            if (args.Length > 0)
                ProcessCommand(args);
            else
                DisplayHelp();


            Console.ReadKey();
        }

        private static void ProcessCommand(string[] arguments)
        {
            string[] args = arguments.Skip(1).ToArray();
            string command = arguments[0].ToLower();

            switch (command)
            {
                case "compress": CompressRom(args); break;      //in, out, game, build
                //case "ocompress": OptimizedCompressRom(args); break;
                case "decompress": DecompressRom(args); break;  //in, out, game, build
                case "spit": SpitFiles(args); break;
                case "swap": SwapRom(args); break;              //in, out, target
                case "crc": SetCRC(args); break;                //in, out, crc
            }
        }

        private static void DisplayHelp()
        {
            Console.WriteLine("compress   'inputRom' 'outputRom' 'GameId' 'Version'");
            Console.WriteLine("decompress 'inputRom' 'outputRom' 'GameId' 'Version'");
            Console.WriteLine("swap       'inputRom' 'outputRom' 'SwapType'");
            Console.WriteLine();

            Console.Write("Press Enter to Continue...");
            Console.ReadLine();
            Console.Clear();

            ORom.ConsolePrintSupportedVersions();
            Console.WriteLine();

            MRom.ConsolePrintSupportedVersions();
            Console.WriteLine();

            Console.Write("Press Enter to Continue...");
            Console.ReadLine();
            Console.Clear();

            Console.WriteLine("Swap Types:");
            //                                    00 01 02 03
            Console.WriteLine("little16: converts 01 00 03 02 -> 00 01 02 03");
            Console.WriteLine("little32: converts 03 02 01 00 -> 00 01 02 03");
            //                                    02 03 00 01


            Console.WriteLine();
            Console.WriteLine("Press Enter to Quit");
            Console.ReadLine();
        }
        
        private static void SetCRC(string[] args)
        {
            if (args.Length != 1)
                return;

            string inRom = args[0];

            if (!FileExists(inRom))
                return;

            using (FileStream fs = File.Open(inRom, FileMode.Open))
            {
                CRC.Write(fs);
            }
        }

        private static void SpitFiles(string[] args)
        {
            throw new NotImplementedException();
        }

        private static void OptimizedCompressRom(string[] args)
        {
            if (args.Length != 4)
                return;

            string inRom = args[0];
            string outRom = args[1];
            string inRefRom = args[2];
            string enumStr = args[3];

            if (!FileExists(inRom))
                return;

            if (!FileExists(inRefRom))
                return;

            if (!TryGetBuild(enumStr, out ORom.Build build))
                return;

            using (var fw = File.Create(outRom))
            {
                RomBuilder.CompressRomOptimized(new ORom(inRom, build), new ORom(inRefRom, build), fw);
            }
        }

        private static void CompressRom(string[] args)
        {
            if (args.Length != 5)
                return;

            string inRom = args[0];
            string outRom = args[1];
            string gameId = args[2];
            string build = args[3];
            string inRefRom = args[4];

            if (!FileExists(inRom))
            {
                Console.WriteLine($"Cannot find file {inRom}");
                return;
            }

            RomVersion version = new(gameId, build);
            if (version.Game == Game.Undefined)
            {
                Console.WriteLine($"Unrecognized game and version: {gameId} {build}");
                return;
            }

            using (var fw = File.Create(outRom))
            {
                RomBuilder.CompressRom(Rom.New(inRom, version), new ORom(inRefRom, version), fw);
            }
        }

        private static void DecompressRom(string[] args)
        {
            if (args.Length != 4)
                return;

            string inRom = args[0];
            string outRom = args[1];
            string gameStr = args[2];
            string buildStr = args[3];

            if (!FileExists(inRom))
                return;

            RomVersion version = new(gameStr, buildStr);

            if (version.Game == Game.Undefined)
                return;

            using (FileStream fw = File.Create(outRom))
            {
                Rom rom = Rom.New(inRom, version);
                RomBuilder.DecompressRom(rom, fw);
            }
        }

        private static bool TryGetBuild(string enumStr, out ORom.Build build)
        {
            if (!Enum.TryParse(enumStr, true, out build))
                return false;

            if (!ORom.IsBuildNintendo(build))
                return false;

            return true;
        }

        private static void SwapRom(string[] args)
        {
            if (args.Length != 3)
                return;

            string inRom = args[0];
            string outRom = args[1];
            string swap = args[2];

            if (!FileExists(inRom))
                return;

            FileEncoding fileEncoding;
            switch (swap.ToLower())
            {
                case "little16": fileEncoding = FileEncoding.LittleEndian16; break;
                case "little32": fileEncoding = FileEncoding.LittleEndian32; break;
                default: return;
            }
            using (FileStream fs = File.OpenRead(inRom))
            {
                using (FileStream fw = File.Create(outRom))
                {
                    FileOrder.ToBigEndian32(fs, fw, fileEncoding);
                }
            }
        }

        private static bool FileExists(string file)
        {
            if (File.Exists(file))
            {
                return true;
            }
            Console.WriteLine("File does not exist.");
            return false;
        }
    }
}
