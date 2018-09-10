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
            Console.WriteLine("compress   \"inputRom\" \"outputRom\" \"GameId\" \"Version\"");
            Console.WriteLine("decompress \"inputRom\" \"outputRom\" \"GameId\" \"Version\"");
            Console.WriteLine("swap       \"inputRom\" \"outputRom\" \"SwapType\"");
            Console.WriteLine();

            Console.Write("Press Enter to Continue...");
            Console.ReadLine();
            Console.Clear();

            Console.WriteLine("Ocarina of Time: use GameId \"oot\"");
            Console.WriteLine("Version:");
            foreach (var item in ORom.GetSupportedBuilds())
            {
                var info = ORom.BuildInformation.Get(item);
                Console.WriteLine(" {0,-5} {1}", info.Version + ":", info.Name);
            }
            Console.WriteLine();

            Console.WriteLine("Majora's Mask: use GameId \"mm\"");
            Console.WriteLine("Version:");
            foreach (var item in MRom.GetSupportedBuilds())
            {
                var info = MRom.BuildInformation.Get(item);
                Console.WriteLine(" {0,-5} {1}", info.Version + ":", info.Name);
            }
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
            string inRom;

            if (args.Length != 1)
                return;

            inRom = args[0];

            if (!FileExists(inRom))
                return;

            using (FileStream fs = new FileStream(inRom, FileMode.Open))
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
            string inRom;
            string outRom;
            string inRefRom;
            string enumStr;
            ORom.Build build;

            if (args.Length != 4)
                return;

            inRom = args[0];
            outRom = args[1];
            inRefRom = args[2];
            enumStr = args[3];

            if (!FileExists(inRom))
                return;

            if (!FileExists(inRefRom))
                return;

            if (!TryGetBuild(enumStr, out build))
                return;

            using (FileStream fw = new FileStream(outRom, FileMode.Create))
            {
                RomBuilder.CompressRomOptimized(new ORom(inRom, build), new ORom(inRefRom, build), fw);
            }
        }

        private static void CompressRom(string[] args)
        {
            string inRom;
            string outRom;
            string gameId;
            string build;

            if (args.Length != 5)
                return;
            
            inRom = args[0];
            outRom = args[1];
            gameId = args[2];
            build = args[3];
            string inRefRom = args[4];

            if (!FileExists(inRom))
            {
                Console.WriteLine($"Cannot find file {inRom}");
                return;
            }

            RomVersion version = new RomVersion(gameId, build);
            if (version.Game == Game.Undefined)
            {
                Console.WriteLine($"Unrecognized game and version: {gameId} {build}");
                return;
            }

            using (FileStream fw = new FileStream(outRom, FileMode.Create))
            {
                RomBuilder.CompressRom(Rom.New(inRom, version), new ORom(inRefRom, version), fw);
                //using (FileStream fs = new FileStream(inRom, FileMode.Open, FileAccess.Read))
                //{
                //    RomBuilder.CompressRom(fs, version, new ORom(inRefRom, version), fw);
                //}
            }
        }

        private static void DecompressRom(string[] args)
        {
            string inRom;
            string outRom;
            string gameStr;
            string buildStr;
            RomVersion version;
            if (args.Length != 4)
                return;

            inRom = args[0];
            outRom = args[1];
            gameStr = args[2];
            buildStr = args[3];

            if (!FileExists(inRom))
                return;

            version = new RomVersion(gameStr, buildStr);

            if (version.Game == Game.Undefined)
                return;

            using (FileStream fw = new FileStream(outRom, FileMode.Create))
            {
                Rom rom = Rom.New(inRom, version);
                //if (version.Game == Game.OcarinaOfTime)
                //    rom = new ORom(inRom, version);
                //else
                //    rom = new MRom(inRom, version);
                RomBuilder.DecompressRom(rom, fw);
            }
        }

        private static bool TryGetBuild(string enumStr, out ORom.Build build)
        {
            build = ORom.Build.UNKNOWN;
            if (!Enum.TryParse(enumStr, true, out build))
                return false;

            if (!ORom.IsBuildNintendo(build))
                return false;

            return true;
        }

        private static void SwapRom(string[] args)
        {
            string inRom;
            string outRom;
            string swap;
            FileEncoding fileEncoding = FileEncoding.Error;
            
            if (args.Length != 3)
                return;

            inRom = args[0];
            outRom = args[1];
            swap = args[2];

            if (!FileExists(inRom))
                return;

            switch (swap.ToLower())
            {
                case "little16": fileEncoding = FileEncoding.LittleEndian16; break;
                case "little32": fileEncoding = FileEncoding.LittleEndian32; break;
                //case "":
                default: return;
            }
            using (FileStream fs = new FileStream(inRom, FileMode.Open, FileAccess.Read))
            {
                using (FileStream fw = new FileStream(outRom, FileMode.Create))
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
