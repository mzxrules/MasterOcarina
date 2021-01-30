using mzxrules.Helper;
using mzxrules.OcaLib;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uCode;
using System.Threading;
using mzxrules.OcaLib.PathUtil;
using mzxrules.OcaLib.SceneRoom;
using mzxrules.XActor;
using mzxrules.OcaLib.SymbolMapParser;

namespace Spectrum
{
    partial class Program
    {
        #region Help
        [SpectrumCommand(
            Name = "help",
            Cat = SpectrumCommand.Category.Help,
            Description = "Shows Help")]
        [SpectrumCommandSignature(Sig = new Tokens[] { })]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.LITERAL },
            Help = "{0} = Command Name")]
        private static void Help(Arguments args)
        {
            Console.Clear();
            if (args.Length == 0)
            {
                Help();
                return;
            }

            string key = (string)args[0];
            if (!CommandDictionary.TryGetValue(key, out var data))
            {
                Console.WriteLine($"Cannot find help for command '{key}'");
                return;
            }
            Console.WriteLine($"{data.attr.Name}  [{data.attr.PrintSupportedVersions()}] - {data.attr.Description}");
            foreach (var signature in data.args)
            {
                Console.WriteLine();
                var sig = HelpFunc.ConvertTokensToString(signature.Sig).ToArray();
                Console.WriteLine($"{data.attr.Name} {HelpFunc.GetArguments(sig)}");
                if (signature.Help != null)
                {
                    string helpStr = string.Format(signature.Help, sig);
                    string[] helpLines = helpStr.Split(';');
                    foreach (var line in helpLines)
                    {
                        Console.WriteLine($"  {line.Trim()}");
                    }
                }
            }
        }

        private static void Help()
        {
            var commandNotes = new List<(SpectrumCommand.Category cat, string Id, string Support, string Description, string args, bool hide)>();

            foreach (var (attr, args, method) in CommandDictionary.Values)
            {
                int l = 0;

                foreach (var signature in args)
                {
                    var test = HelpFunc.ConvertTokensToString(signature.Sig);
                    string var = HelpFunc.GetArguments(test);
                    commandNotes.Add((attr.Cat, attr.Name, attr.PrintSupportedVersions(), attr.Description, var, (l != 0)));
                    l++;
                }
            }

            commandNotes = commandNotes.OrderBy(x => x.cat).ThenBy(x => x.Id).ToList();
            int lines = commandNotes.Count + 1;
            int i = 0;

            while (i < lines)
            {
                int linesPerScreen = Console.WindowHeight - 1;
                int linesToWrite = lines - i;
                linesToWrite = (linesToWrite < linesPerScreen) ? linesToWrite : linesPerScreen;
                PrintDescription(linesToWrite);
                Console.ReadKey();
                Console.Clear();
                PrintArgs(linesToWrite);
                i += linesToWrite;
                if (i < lines)
                {
                    Console.ReadKey();
                    Console.Clear();
                }
            }


            void PrintDescription(int count)
            {
                for (int j = 0; j < count; j++)
                {
                    if (i + j == 0)
                        Console.WriteLine("Commands:");
                    else
                    {
                        var (cat, Id, Support, Description, args, hide) = commandNotes[i + j - 1];
                        if (hide)
                            Console.WriteLine($"{Id,-11} {Support} -");
                        else
                            Console.WriteLine($"{Id,-11} {Support} - {Description}");
                    }
                }
            }
            void PrintArgs(int count)
            {
                for (int j = 0; j < count; j++)
                {
                    if (i + j == 0)
                        Console.WriteLine("Commands:");
                    else
                    {
                        var (cat, Id, Support, Description, args, hide) = commandNotes[i + j - 1];
                        Console.WriteLine($"{Id,-11} - {args}");
                    }
                }
            }

        }

        #endregion

        #region Spectrum

        [SpectrumCommand(
            Name = "",
            Cat = SpectrumCommand.Category.Spectrum,
            Description = "Creates a memory map")]
        private static void CreateMemoryMap(Arguments args)
        {
            Default();
        }

        private static void Default()
        {
            PrintAddresses(MemoryMapper.GetRamMap(Options).OrderBy(x => x.Ram.Start.Offset));
        }

        [SpectrumCommand(
            Name = "=",
            Cat = SpectrumCommand.Category.Spectrum,
            Description = "Evaluates an expression")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.EXPRESSION_S })]
        private static void Evaluate(Arguments args)
        {
            if (!TryEvaluate((string)args[0], out long result))
                return;

            Console.WriteLine($"{result:X}");
        }

        [SpectrumCommand(
            Name = "game",
            Cat = SpectrumCommand.Category.Spectrum,
            Description = "Sets game to profile, and optionally lets you specify a version")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.LITERAL },
            Help = "{0} = Game. oot for Ocarina of Time, mm for Majora's Mask")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.LITERAL, Tokens.LITERAL },
            Help = "{0} = Game. oot for Ocarina of Time, mm for Majora's Mask;" +
            "{1} = Version. Run the 'ver' command for more information")]
        private static void SetGame(Arguments args)
        {
            if (args.Length == 1)
            {
                switch (((string)args[0]).ToLower())
                {
                    case "oot":
                        Options.Version = ORom.Build.N0;
                        ChangeVersion((Options, true)); break;
                    case "mm":
                        Options.Version = MRom.Build.U0;
                        ChangeVersion((Options, true)); break;
                }
            }
            else if (args.Length == 2)
            {
                RomVersion v = new RomVersion(((string)args[0]).ToLowerInvariant(), ((string)args[1]).ToLowerInvariant());
                if (v.Game == Game.Undefined)
                    Console.WriteLine("Unknown version code");
                else
                {
                    Options.Version = v;
                    ChangeVersion((Options, true));
                }
            }
        }

        [SpectrumCommand(
            Name = "trainer",
            Cat = SpectrumCommand.Category.Spectrum,
            Description = "Runs a trainer script for configuring an emulator")]
        private static void MupenTrainer(Arguments args)
        {
            Console.WriteLine("Launch a mupen64plus or pj64 based emulator, and open Ocarina of Time V1.0.");
            Console.WriteLine("Pause the game on the title screen. Type n to cancel the scan, or enter to continue");
            var key = Console.ReadKey();
            if (key.KeyChar == 'n')
                return;
            var signature = SearchSignature.Deserialize("data/trainer/n0_signature.bin");
            var emulator = Zpr.Trainer(signature);
            if (emulator != null)
            {
                AddEmulator(emulator.ProcessName, emulator);
                MountEmulator();
            }
        }

        [SpectrumCommand(
            Name = "emu",
            Cat = SpectrumCommand.Category.Spectrum,
            Description = "Sets/updates emulator settings")]
        [SpectrumCommandSignature(Help =
            "Testing")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.LITERAL, Tokens.STRING, Tokens.STRING, Tokens.S16, Tokens.STRING, Tokens.U8 },
            Help =
            "{0} = Key, used to specify a specific emulator with the mount command;" +
            "{1} = Process Name, used to locate the emulator process;" +
            "{2} = Process Description, used to describe the emulator in more detail;" +
            "{3} = Process Type, 32 for 32 bit, 64 for 64 bit;" +
            "{4} = Ram Start Expression;" +
            "{5} = Is Big Endian (for now);" +
            "When writing expressions, you can wrap a module name with back ticks to reference;" +
            "that module's start address. You can also wrap [] around an expression to" +
            "dereference a 32 bit pointer at that address. For example, PJ64 1.6's ram start" +
            "expression is this:;" +
            "[`project64.exe`+d6a1c]")]
        private static void SetEmulator(Arguments args)
        {
            Emulator emu;
            string key = "";
            if (args.Length != 0)
                emu = SetEmulator_Legacy(args, ref key);
            else
                emu = SetEmulator_GetArgs(args, ref key);

            AddEmulator(key, emu);
        }

        private static void AddEmulator(string key, Emulator emu)
        {
            if (!Options.Emulators.TryGetValue(key, out Emulator emuOld))
            {
                Options.Emulators.Add(key, emu);
                SaveSettings();
            }
            else
            {
                Console.WriteLine("Update emulator reference? Type yes to confirm:");
                Console.WriteLine($"Old: {emuOld}");
                Console.WriteLine($"New: {emu}");
                string input = Console.ReadLine();

                if (input.ToLowerInvariant() == "yes")
                {
                    Options.Emulators.Remove(key);
                    Options.Emulators.Add(key, emu);
                    Console.WriteLine("Reference updated!");
                    SaveSettings();
                }
                else
                    Console.WriteLine("Changes cancelled.");
            }
        }

        private static Emulator SetEmulator_GetArgs(Arguments args, ref string key)
        {
            bool validateProcess(object a)
            { return (int)a == 32 || (int)a == 64; }

            bool validateByteOrder(object a)
            {
                string byteOrder = (string)a;
                return byteOrder == "Big32" || byteOrder == "Lit32";
            }

            (Tokens t, Func<object, bool> validateFunc, string s)[] inputChain =
            {
                (Tokens.STRING, (a)=>true, "Enter the exact name of the emulator process, omitting the .exe"),
                (Tokens.S32, validateProcess, "Enter 32 if emulator is a 32 bit process, or 64 if emulator is a 64 bit process"),
                (Tokens.STRING, (a)=>true, "Enter an expression that can be used to locate the start of ram"),
                (Tokens.LITERAL, validateByteOrder, "Enter the emulator's byte order. Big32 = Big Endian 32, Lit32 = Little Endian 32"),
                (Tokens.LITERAL, (a)=>true, "Enter a short string (no spaces) to reference this emulator via the mount command."),
                (Tokens.STRING, (a)=>true, "Enter a description for the emulator"),
            };

            List<object> arguments = new List<object>();

            foreach (var (token, validate, request) in inputChain)
            {
                bool loop = true;
                do
                {
                    Console.WriteLine(request);
                    var readline = Console.ReadLine();
                    Arguments arg = new Arguments(readline, token);

                    if (arg.Valid && validate(arg[0]))
                    {
                        arguments.Add(arg[0]);
                        loop = false;
                    }

                } while (loop);
            }


            key = (string)arguments[4];

            return new Emulator(
                (string)arguments[0],
                (string)arguments[5],
                (int)arguments[1],
                (string)arguments[2],
                ((string)arguments[3] == "Lit32") ? (byte)0 : (byte)1
                );
        }

        private static Emulator SetEmulator_Legacy(Arguments args, ref string key)
        {
            key = (string)args[0];

            return new Emulator(
                (string)args[1],
                (string)args[2],
                (int)args[3],
                (string)args[4],
                (byte)args[5]);
        }

        [SpectrumCommand(
            Name = "emulist",
            Cat = SpectrumCommand.Category.Spectrum,
            Description = "Lists all emulators defined in Spectrum's settings file")]
        private static void ListEmulators(Arguments args)
        {
            foreach (var emulator in Options.Emulators)
            {
                Console.WriteLine($"{emulator.Key} {emulator.Value}");
            }
        }

        [SpectrumCommand(
            Name = "emudel",
            Cat = SpectrumCommand.Category.Spectrum,
            Description = "Removes emulator settings for an emulator")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.LITERAL })]
        private static void DeleteEmulator(Arguments args)
        {
            var key = (string)args[0];

            if (!Options.Emulators.TryGetValue(key, out Emulator emu))
            {
                Console.WriteLine($"{key} not found.");
                return;
            }

            Console.WriteLine("Are you sure you want to delete this emulator (type yes to confirm):");
            Console.WriteLine(emu.ProcessName);
            Console.WriteLine(emu.ProcessDescription);
            Console.WriteLine("Ram Start expression: " + emu.RamStart);
            string input = Console.ReadLine();

            if (input.ToLowerInvariant() != "yes")
            {
                Console.WriteLine("Changes cancelled.");
                return;
            }

            Options.Emulators.Remove(key);
            SaveSettings();
        }

        [SpectrumCommand(
            Name = "mount",
            Cat = SpectrumCommand.Category.Spectrum,
            Description = "Locates a running emulator to interface with")]
        [SpectrumCommandSignature(Help = "Attempts to locate any emulators in memory")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.LITERAL },
            Help = "{0} = key identifier used to specify an emulator when more than one is running")]
        private static void MountEmulator(Arguments args)
        {
            string emu = (args.Length == 1) ? (string)args[0] : "";
            MountEmulator(emu);
        }

        private static void MountEmulator() => MountEmulator("");

        private static void MountEmulator(string emu)
        {
            List<Emulator> emulatorsToMount = new List<Emulator>();

            if (emu == "")
            {
                emulatorsToMount = Options.Emulators.Values.ToList();
            }
            else
            {
                if (!Options.Emulators.TryGetValue(emu, out Emulator emulator))
                {
                    Console.WriteLine("No emulator setting found.");
                }
                else
                {
                    emulatorsToMount = new List<Emulator>() { emulator };
                }
            }
            if (Zpr.TryMountEmulator(emulatorsToMount))
                ChangeVersion((Options, true));
            else
                Console.WriteLine("Emulator selection failed.");

            if (!Zpr.IsEmulatorSet)
            {
                Console.WriteLine("Warning: No emulator is set up! Type mount while a recognized emulator is running");
                Console.WriteLine("or use the emu command to create a new emulator definition.");
            }
        }

        [SpectrumCommand(
            Name = "t",
            Cat = SpectrumCommand.Category.Spectrum,
            Description = "Toggles Setting")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.LITERAL },
            Help = "{0} = Settings key. The following values are used:;;" +
            "ll = Show/Hide link list nodes;" +
            "size = Show Size or End Address;" +
            "actor = Show/Hide actor files/instances;" +
            "obj = Show/Hide object files;" +
            "part = Show/Hide particle files;" +
            "thread = Show/Hide thread structs")]
        private static void ToggleSettings(Arguments args)
        {
            string key = (string)args[0];
            switch (key.ToLowerInvariant())
            {
                case "ll": ToggleSettings(ref Options.ShowLinkedList); Default(); break;
                case "obj": ToggleSettings(ref Options.ShowObjects); Default(); break;
                case "size": ToggleSettings(ref Options.ShowSize); Default(); break;
                case "actor": ToggleSettings(ref Options.ShowActors); Default(); break;
                case "part": ToggleSettings(ref Options.ShowParticles); Default(); break;
                case "thread": ToggleSettings(ref Options.ShowThreadingStructs); Default(); break;
            }
        }

        [SpectrumCommand(
            Name = "ver",
            Cat = SpectrumCommand.Category.Spectrum,
            Description = "Sets game version/displays supported game versions")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { },
            Help = "Lists all valid version codes recognized by the 'game' and 'ver' commands")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.LITERAL },
            Help = "{0} = version code")]
        private static void TrySetVersion(Arguments args)
        {
            if (args.Length == 0)
            {
                Console.Clear();
                ORom.ConsolePrintSupportedVersions();
                Console.WriteLine();
                MRom.ConsolePrintSupportedVersions();
                Console.WriteLine();
                return;
            }

            string build = (string)args[0];

            var version = new RomVersion(Options.Version.Game, build);

            if (version.Game != Game.Undefined)
            {
                Options.Version = version;
                ChangeVersion((Options, true));
            }
            else
                Console.WriteLine($"Invalid code for {Options.Version.Game}. Run ver (no arguments) for correct version codes");
        }

        [SpectrumCommand(
            Name = "sympath",
            Cat = SpectrumCommand.Category.Spectrum,
            Description = "Sets the path location of a decompilation symbol map file")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.PATH })]
        private static void SetSymbolMapPath(Arguments args)
        {
            string path = (string)args[0];
            Options.MapfileOptions.Path = path; 
            if (!File.Exists(path))
            {
                Console.WriteLine($"Cannot find symbol file: {path}");
                return;
            }
            else
            {
                Console.WriteLine($"Path: {path}");
                Console.WriteLine($"Path saved. Run symload with the correct version selected to load the symbol map");
            }
            SaveSettings();
        }

        [SpectrumCommand(
            Name = "symload",
            Cat = SpectrumCommand.Category.Spectrum,
            Description = "Use symbol data from file to set up Spectrum's addresses. Overrides current version settings")]
        private static void LoadSymbolMap(Arguments args)
        {
            string path = Options.MapfileOptions.Path;
            if (!File.Exists(path))
            {
                Console.WriteLine($"Cannot find symbol file: {Options.MapfileOptions.Path}");
                return;
            }

            Options.MapfileOptions.SymbolMap = SymbolMapParser.Parse(Options.MapfileOptions.Path);
            Options.MapfileOptions.Version = Options.Version;
            Options.MapfileOptions.UseMap = true;
            ChangeVersion((Options, true));
        }

        [SpectrumCommand(
            Name = "symunload",
            Cat = SpectrumCommand.Category.Spectrum,
            Description = "Disable using symbol data for Spectrum's addresses")]
        private static void UnloadSymbolMap(Arguments args)
        {
            Options.MapfileOptions.UseMap = false;
            ChangeVersion((Options, true));
        }

        [SpectrumCommand(
            Name = "save",
            Cat = SpectrumCommand.Category.Spectrum,
            Description = "Save Settings (program usually does this automatically)")]
        private static void SaveSettingsCommand(Arguments args)
        {
            SaveSettings();
        }

        #endregion

        #region Actor
        [SpectrumCommand(
            Name = "hidea",
            Cat = SpectrumCommand.Category.Actor,
            Description = "Hide actor file/instance of a given id")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.HEX_S16 },
            Help = "{0} = Id of the Actor Instances/Files to hide")]
        private static void HideActor(Arguments args)
        {
            short actor = (short)args[0];
            Options.HiddenActors.Add(actor);
        }

        [SpectrumCommand(
            Name = "showalla",
            Cat = SpectrumCommand.Category.Actor,
            Description = "Unhide all hidden actors")]
        private static void ShowAllActors(Arguments args)
        {
            Options.HiddenActors.Clear();
            Default();
        }

        [SpectrumCommand(
            Name = "anear",
            Cat = SpectrumCommand.Category.Actor,
            Description = "Locates actors nearest to a 3D point (or Link if none specified)")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { },
            Help = "Finds actors closest to Link")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.COORDS_FLOAT },
            Help = "{0} = coordinates to look for the nearest actor")]
        private static void FindNearestActor(Arguments args)
        {
            Vector3<float> position;

            var actors = ActorMemoryMapper.FetchInstances().Instances;

            if (args.Length == 0)
            {
                var link = actors.Where(x => x.Actor == 0).FirstOrDefault();

                if (link == null)
                {
                    Console.WriteLine("No Link Actor");
                    return;
                }
                actors.Remove(link);
                position = link.Position;
            }
            else
                position = (Vector3<float>)args[0];

            Console.Clear();
            PrintAddresses(actors.OrderBy(x => CalculateDistance3D(position, x.Position)).Take(20));

        }

        [SpectrumCommand(
            Name = "dumpa",
            Cat = SpectrumCommand.Category.Actor,
            Description = "Dumps all data for an actor instance")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.EXPRESSION_S })]
        private static void DumpActor(Arguments args)
        {
            if (!TryEvaluate((string)args[0], out long addr))
                return;
            N64Ptr address = (addr & 0xFFFFFF) | 0x8000_0000;
            var actors = from n in ActorMemoryMapper.FetchInstances().Instances
                         where n.Ram.Start.Offset == address.Offset
                         select n;

            foreach (ActorInstance a in actors)
            {
                Ptr ptr = SPtr.New(a.Ram.Start);
                string path = $"dump/AI{a.Actor:X3}_{ptr}.z64";
                using (BinaryWriter bw = new BinaryWriter(new FileStream(path, FileMode.Create)))
                {
                    for (int i = 0; i < a.Ram.Size; i += 4)
                    {
                        bw.WriteBig(ptr.ReadInt32(i));
                    }
                }
                Console.WriteLine($"{path} created.");
            }
        }


        [SpectrumCommand(
            Name = "axyz",
            Cat = SpectrumCommand.Category.Actor,
            Description = "Sets actor's x,y,z coordinates")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.EXPRESSION_S, Tokens.COORDS_FLOAT })]
        private static void SetActorCoordinates(Arguments args)
        {
            if (!TryEvaluate((string)args[0], out long addr))
                return;

            addr |= 0x80000000;
            N64Ptr address = addr;

            var instance = ActorMemoryMapper.FetchInstances().Instances.Where(x => x.Address.Offset == address.Offset).SingleOrDefault();

            if (instance == null)
                return;

            Vector3<float> a = (Vector3<float>)args[1];

            Ptr ptr = SPtr.New(instance.Address);

            SetActorCoordinates(ptr, a.x, a.y, a.z);
        }

        #endregion

        #region Ram
        [SpectrumCommand(
            Name = "addr",
            Cat = SpectrumCommand.Category.Ram,
            Description = "Converts N64 address into an emulator's process address")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.EXPRESSION_S })]
        private static void PrintEmuAddr(Arguments args)
        {
            if (!TryEvaluate((string)args[0], out long addr))
                return;
            Console.WriteLine($"{Zpr.GetEmulatedAddress(addr):X8}");
        }

        [SpectrumCommand(
            Name = "cram32",
            Cat = SpectrumCommand.Category.Proto,
            Description = "Copies RDRAM starting at the given address in 32 bit chunks, and stores in clipboard")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.EXPRESSION_S, Tokens.HEX_S32 },
            Help = "{0} = RDRAM start;{1} = hexadecimal size in bytes")]
        private static void CopyRam32(Arguments args)
        {
            throw new NotImplementedException("FIXME");
            if (!TryEvaluate((string)args[0], out long address))
                return;

            int size = (int)args[1];

            if (address % 4 != 0)
            {
                Console.WriteLine($"Alignment Error: {address}");
                return;
            }
            if (size <= 0)
            {
                Console.WriteLine("Invalid size");
                return;
            }
            if (size > 0x2000)
            {
                Console.WriteLine($"Size capped at 0x2000 ");
                return;
            }

            var end = (size + 3) & -4;

            StringBuilder stringBuilder = new StringBuilder();
            for (int off = 0; off < end; off += 4)
            {
                stringBuilder.AppendLine($"{Zpr.ReadRamInt32(address + off):X8}");
            }

            string clipboardText = stringBuilder.ToString();

            //var t = new Thread(
            //    (text) => { Clipboard.SetText((string)text); });

            //t.SetApartmentState(ApartmentState.STA);
            //t.Start(clipboardText);
            //t.Join();

            Console.Clear();
            Console.WriteLine("Copied: ");
            PrintRam(address, PrintRam_X8);
        }


        [SpectrumCommand(
            Name = "ram",
            Cat = SpectrumCommand.Category.Ram,
            Description = "Prints RDRAM starting at the given address")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.EXPRESSION_S })]
        private static void PrintRam(Arguments args)
        {
            if (!TryEvaluate((string)args[0], out long address))
                return;

            Console.Clear();
            PrintRam(address, PrintRam_X8);
        }

        [SpectrumCommand(
            Name = "ramf",
            Cat = SpectrumCommand.Category.Ram,
            Description = "Prints RDRAM starting at the given address")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.EXPRESSION_S })]
        private static void PrintRamF(Arguments args)
        {
            if (!TryEvaluate((string)args[0], out long address))
                return;

            Console.Clear();
            PrintRam(address, PrintRam_F);
        }


        [SpectrumCommand(
            Name = "rams16",
            Cat = SpectrumCommand.Category.Ram,
            Description = "Prints RDRAM starting at the given address")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.EXPRESSION_S })]
        private static void PrintRamS16(Arguments args)
        {
            if (!TryEvaluate((string)args[0], out long address))
                return;

            Console.Clear();
            PrintRam(address, PrintRam_S16);
        }


        [SpectrumCommand(
            Name = "ramu16",
            Cat = SpectrumCommand.Category.Ram,
            Description = "Prints RDRAM starting at the given address")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.EXPRESSION_S })]
        private static void PrintRamU16(Arguments args)
        {
            if (!TryEvaluate((string)args[0], out long address))
                return;

            Console.Clear();
            PrintRam(address, PrintRam_U16);
        }


        private static void PrintRam(N64Ptr address, Action<N64Ptr> PrintLine, int lines = 0x10)
        {
            address &= -4;
            for (int i = 0; i < lines; i++)
            {
                PrintLine(address + i * 0x10);
            }
        }

        private static void PrintRam_X8(N64Ptr a)
        {
            Ptr p = SPtr.New(a);
            Console.WriteLine(string.Format("{0:X8}: {1:X8} {2:X8} {3:X8} {4:X8}",
                p,
                p.ReadInt32(0x00),
                p.ReadInt32(0x04),
                p.ReadInt32(0x08),
                p.ReadInt32(0x0C)));
        }

        private static void PrintRam_U16(N64Ptr a)
        {
            Ptr p = SPtr.New(a);
            string[] v = new string[8];

            for (int i = 0; i < 8; i++)
            {
                ushort value = p.ReadUInt16(i * 2);
                v[i] = $"{value,6}";
            }
            Console.WriteLine($"{a:X8}: {string.Join(" ", v)}");
        }

        private static void PrintRam_S16(N64Ptr a)
        {
            Ptr p = SPtr.New(a);
            string[] v = new string[8];

            for (int i = 0; i < 8; i++)
            {
                short value = p.ReadInt16(i * 2);
                v[i] = $"{value,6}";
            }
            Console.WriteLine($"{a:X8}: {string.Join(" ", v)}");
        }

        private static void PrintRam_F(N64Ptr a)
        {
            Ptr p = SPtr.New(a);
            string[] v = new string[4];

            for (int i = 0; i < 4; i++)
            {
                float val = p.ReadFloat(i * 4);
                if (val == 0f)
                {
                    v[i] = "0.0  ";
                }
                else
                {
                    var exponent = Math.Floor(Math.Log10(Math.Abs(val)));
                    if (exponent >= 0 && exponent < 8)
                        v[i] = val.ToString("F3");
                    else
                        v[i] = val.ToString("E5");
                }
            }

            Console.WriteLine($"{a:X8}: {v[0],13} {v[1],13} {v[2],13} {v[3],13}");
        }

        [SpectrumCommand(
            Name = "a",
            Cat = SpectrumCommand.Category.Ram,
            Description = "Returns whatever major structure is located at the given address")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.EXPRESSION_S })]
        private static void GetAddresses(Arguments args)
        {
            if (!TryEvaluate((string)args[0], out long a))
                return;

            IEnumerable<IRamItem> items;
            IEnumerable<IVRamItem> vItems;

            N64Ptr inAddr = a | 0x80000000;
            N64Ptr addr = inAddr;

            //if virtual address
            if (inAddr.Offset >= 0x800000)
            {
                vItems = from x in MemoryMapper.GetRamMap(Options, true).OfType<IVRamItem>()
                         where (x.VRam.Start & 0xFFFFFF) <= inAddr.Offset
                            && (x.VRam.End & 0xFFFFFF) > inAddr.Offset
                         select x;
                items = vItems.Cast<IRamItem>();
                var temp = vItems.ToList();
                if (temp.Count == 0)
                {
                    Console.WriteLine($"{inAddr:X8}: No Conversion");
                }
                else if (temp.Count == 1)
                {
                    N64Ptr vram = temp[0].VRam.Start;
                    N64Ptr ram = ((IRamItem)temp[0]).Ram.Start;
                    addr = inAddr - vram + ram;

                    Console.WriteLine($"{inAddr:X8} -> {addr:X8}:");
                }
                else
                {
                    Console.WriteLine("Conversion error");
                }
            }
            else
            {
                Console.WriteLine($"{addr:X8}:");
                items = from x in MemoryMapper.GetRamMap(Options, true)
                        where (x.Ram.Start & 0xFFFFFF) <= addr.Offset && (x.Ram.End & 0xFFFFFF) > addr.Offset select x;
            }

            foreach (var item in items)
            {
                Console.WriteLine(item.ToString());
                int offset = addr.Offset - item.Ram.Start.Offset;
                string line = $"Start {(int)item.Ram.Start:X8} Off: {offset:X6} ";

                if (item is IFile iFile)
                {
                    if (iFile.VRom.Size > offset)
                        line += $"VRom: {iFile.VRom.Start + offset:X8} ";
                    else
                        line += $".bss: {offset - iFile.VRom.Size:X8} ";
                }

                if (item is IVRamItem iRamItem)
                    line += $"VRam: {(int)(iRamItem.VRam.Start + offset):X8}";

                Console.WriteLine(line);
            }
        }

        [SpectrumCommand(
            Name = "r",
            Cat = SpectrumCommand.Category.Ram,
            Description = "Converts rom address to ram, if possible")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.EXPRESSION_S })]
        private static void ConvertRomToRam(Arguments args)
        {
            if (!TryEvaluate((string)args[0], out long address))
                return;

            var items = from x in MemoryMapper.GetRamMap(Options, true).OfType<IFile>()
                        where x.VRom.Start <= address
                        && x.VRom.End > address
                        select x;

            foreach (var item in items)
            {
                if (item is IRamItem iRamItem)
                {
                    var (start, end) = item.VRom;
                    Console.WriteLine(item);
                    long offset = address - item.VRom.Start;
                    N64Ptr addr = iRamItem.Ram.Start + offset;
                    addr = 0x8000_0000 + addr.Offset;

                    Console.WriteLine($"Addr: {addr}");
                }
            }
        }

        [SpectrumCommand(
            Name = "var",
            Cat = SpectrumCommand.Category.Ram,
            Description = "Shows the values of a list of important game variables")]
        private static void GetVariablesCommand(Arguments args)
        {
            SpectrumVariables.GetVariables();
        }

        [SpectrumCommand(
            Name = "heap",
            Cat = SpectrumCommand.Category.Ram,
            Description = "Dump heap stats")]
        private static void GetHeapInfo(Arguments args)
        {
            var arena_main = BlockNode.GetBlockList(SpectrumVariables.Main_Heap_Ptr);
            var arena_scene = BlockNode.GetBlockList(SpectrumVariables.Scene_Heap_Ptr);


            PrintHeapInfo("ARENA_MAIN", arena_main);
            PrintHeapInfo("ARENA_SCENE", arena_scene);

            if (SpectrumVariables.Debug_Heap_Ptr != 0)
            {
                var arena_debug = BlockNode.GetBlockList(SpectrumVariables.Debug_Heap_Ptr);
                PrintHeapInfo("ARENA_DEBUG", arena_debug);
            }

        }

        private static void PrintHeapInfo(string heap, List<BlockNode> list)
        {
            uint size = 0;
            uint heapFree = 0;
            uint heapAlloc = 0;
            uint largestFree = 0;

            foreach (var node in list)
            {
                size += (uint)BlockNode.LENGTH;
                heapAlloc += (uint)BlockNode.LENGTH;
                size += node.Size;
                if (node.IsFree)
                {
                    heapFree += node.Size;
                    if (largestFree < node.Size)
                    {
                        largestFree = node.Size;
                    }
                }
                else //not free
                {
                    heapAlloc += node.Size;
                }
            }
            Console.WriteLine($"{heap:,-11‬} Size: {size:X6} Alloc: {heapAlloc:X6} Free: {heapFree:X6} ({largestFree:X6})");
        }

        static ValueSearch search = new ValueSearch();

        [SpectrumCommand(
            Name = "s",
            Cat = SpectrumCommand.Category.Ram,
            Description = "Dump heap stats")]
        [SpectrumCommandSignature(Sig = new Tokens[] { })]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.LITERAL })]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.LITERAL, Tokens.HEX_S32 })]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.LITERAL, Tokens.HEX_S32, Tokens.HEX_S32 })]
        private static void SearchRam(Arguments args)
        {
            if (args.Length > 0)
            {
                if (!Enum.TryParse((string)args[0], true, out SearchFuncs str))
                {
                    Console.WriteLine("Invalid Operation");
                    return;
                }
                switch (str)
                {
                    case SearchFuncs.R: search.Initialize(); break;
                    case SearchFuncs.NEW:
                        {
                            if (args.Length != 3)
                            {
                                Console.WriteLine("Invalid range: specify ptr and search size");
                                return;
                            }

                            N64Ptr addr = (int)args[1];
                            int size = (int)args[2] & -4;
                            int ramSize = GetRamSize();

                            if (addr.Offset + size > ramSize)
                            {
                                Console.WriteLine("Invalid Range");
                                return;
                            }
                            var options = new SearchOptions(addr, size);
                            search = new ValueSearch(options);
                            search.Initialize();

                        } break;
                    case SearchFuncs.X:
                        {
                            if (args.Length != 2)
                            {
                                Console.WriteLine("Invalid exact value");
                                return;
                            }
                            search.Exact((int)args[1]);

                        } break;
                    case SearchFuncs.GT: search.GreaterThan(); break;
                    case SearchFuncs.LT: search.LessThan(); break;
                    case SearchFuncs.E: search.Equal(); break;
                    case SearchFuncs.N: search.Different(); break;
                    case SearchFuncs.FORCE: PrintSearchResult(0x3E8); return;
                }
            }
            PrintSearchResult(40);
        }

        private static void PrintSearchResult(int count)
        {
            Console.Clear();
            foreach (var item in search.GetList(count))
            {
                Console.WriteLine(item);
            }
            Console.WriteLine($"Found {search.FoundElements} items.");
        }


        #endregion

        #region Spawn
        [SpectrumCommand(
            Name = "ent",
            Cat = SpectrumCommand.Category.Spawn,
            Description = "Spawns Link at a given entrance index")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.HEX_S16 },
            Help = "{0} = Entrance Index")]
        private static void SpawnAtEntranceIndex(Arguments args)
        {
            SetEntranceIndexSpawn((short)args[0]);
        }

        [SpectrumCommand(
            Name = "sp",
            Cat = SpectrumCommand.Category.Spawn,
            Description = "Spawns Link in a given scene number")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.U8 },
            Help = "{0} = Scene")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.U8, Tokens.U8 },
            Help = "{0} = Scene; {1} = Spawn Id")]
        private static void SpawnToEntrance(Arguments args)
        {
            int scene = (byte)args[0];
            int ent = (args.Length == 2) ? (byte)args[1] : 0;

            if (Options.Version == Game.MajorasMask)
            {
                scene = GetExternalSceneId_Mask((byte)scene);
                short mmIndex = (short)((scene << 5) + ent << 4);
                SetEntranceIndexSpawn(mmIndex);
                return;
            }

            if (TryGetOoTEntranceIndex(scene, ent, out EntranceIndex index))
            {
                SetEntranceIndexSpawn(index.id);
            }
        }

        [SpectrumCommand(
            Name = "spc",
            Cat = SpectrumCommand.Category.Spawn,
            Description = "Spawns Link in a given scene number, with a specific cutscene",
            Sup = SpectrumCommand.Supported.OoT)]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.U8, Tokens.HEX_U16 },
            Help = "{0} = Scene; {1} = Cutscene")]
        private static void SpawnToCutscene(Arguments args)
        {
            int scene = (byte)args[0];
            ushort cutscene = (ushort)args[1];

            if (TryGetOoTEntranceIndex(scene, 0, out EntranceIndex index))
            {
                SaveContext.Write(0x1412, cutscene);
                SetEntranceIndexSpawn(index.id);
            }
        }

        private static bool TryGetOoTEntranceIndex(int scene, int ent, out EntranceIndex index)
        {
            if (!TryGetEntranceIndex(scene, ent, out index))
                return false;

            var i = index;

            var spawnQuery = from a in SpawnData.Spawns
                             where a.scene == i.scene && (a.ent == i.ent || a.ent == 0)
                             orderby a.ent descending
                             select a;

            var spawnList = spawnQuery.ToList();

            if (spawnList.Count == 0)
                return false;

            var spawn = spawnList[0];
            return true;
        }

        [SpectrumCommand(
            Name = "sa",
            Cat = SpectrumCommand.Category.Spawn,
            Description = "Spawn anywhere with a given scene, room, x, y, z")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.U8, Tokens.U8, Tokens.COORDS_FLOAT },
            Help = "{0} = Scene; {1} = Room; {2} = x, y, z")]
        private static void SpawnAnywhere(Arguments args)
        {
            int s = (byte)args[0];
            byte r = (byte)args[1];
            Vector3<float> c = (Vector3<float>)args[2];

            if (Options.Version.Game != Game.OcarinaOfTime)
            {
                return;
            }
            if (!TryGetEntranceIndex(s, 0, out EntranceIndex index))
                return;

            Spawn spawn = new Spawn(-1, -1, -1, r, (short)c.x, (short)c.y, (short)c.z, 0);

            if (Options.Version.Game == Game.OcarinaOfTime)
                SetZoneoutSpawn_Ocarina(index.id, ref spawn);
            else if (Options.Version.Game == Game.MajorasMask)
                SetZoneoutSpawn_Mask(index.id, ref spawn);
        }

        [SpectrumCommand(
            Name = "sr",
            Cat = SpectrumCommand.Category.Spawn,
            Description = "Spawns in a specified room and x, y, z within the current scene")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.U8, Tokens.COORDS_FLOAT },
            Help = "{0} = Room; {1} = x, y, z")]
        private static void SpawnInRoom(Arguments args)
        {
            byte r = (byte)args[0];
            Vector3<float> a = (Vector3<float>)args[1];

            Spawn spawn = new Spawn(-1, -1, -1, r, (short)a.x, (short)a.y, (short)a.z, 0);

            if (Options.Version.Game == Game.OcarinaOfTime)
                SetZoneoutSpawn_Ocarina(null, ref spawn);
            else if (Options.Version.Game == Game.MajorasMask)
                SetZoneoutSpawn_Mask(null, ref spawn);
        }

        [SpectrumCommand(
            Name = "y",
            Cat = SpectrumCommand.Category.Spawn,
            Description = "Sets y coordinate")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.FLOAT })]
        private static void SetCoordinatesY(Arguments args)
        {
            var linkInstance = GetLinkInstance();
            if (linkInstance == null)
                return;

            SetActorCoordinates(linkInstance, null, (float)args[0], null);
        }

        [SpectrumCommand(
            Name = "xyz",
            Cat = SpectrumCommand.Category.Spawn,
            Description = "Sets Link's x,y,z coordinates")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.COORDS_FLOAT })]
        private static void SetCoordinates(Arguments args)
        {
            var linkInstance = GetLinkInstance();
            if (linkInstance == null)
                return;

            Vector3<float> a = (Vector3<float>)args[0];

            SetActorCoordinates(linkInstance, a.x, a.y, a.z);
        }

        #endregion

        #region Graphics
        [SpectrumCommand(
            Name = "gfx",
            Cat = SpectrumCommand.Category.Gfx,
            Description = "Displays variables related to the 'Graphics Context'")]
        private static void DisplayGraphicsContext(Arguments args)
        { 
            if (Gfx != 0)
            {
                GfxDList[] dlists = GetFrameDlists(Gfx);
                Console.Clear();
                PrintGraphicsContext(dlists);
            }
        }

        private static void PrintGraphicsContext(GfxDList[] DLists)
        {
            Console.WriteLine($"{(int)Gfx:X6}: Graphics Context");
            Console.WriteLine($"DLIST COMPLETE: {Gfx.ReadUInt32(0xB8):X8}");
            foreach (var dlist in DLists)
            {
                var freeSize = dlist.AppendEndPtr - dlist.AppendStartPtr;
                Console.WriteLine("{6:X4} {0,-13}: {1:X5} {2:X6} {3:X6} {4:X6} Free: {5:X5}", dlist.Name, dlist.Size,
                    dlist.StartPtr.Offset,
                    dlist.AppendStartPtr.Offset,
                    dlist.AppendEndPtr.Offset,
                    (freeSize >= 0) ? freeSize : 0xFFFFF,
                    (dlist.RecordPtr - (Gfx & 0xFFFFFF)) & 0xFFFFFF);
            }
        }

        [SpectrumCommand(
            Name = "gfxset",
            Cat = SpectrumCommand.Category.Gfx,
            Description = "Allows the 'Graphics Context' pointer to be set manually")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.EXPRESSION_S })]
        private static void SetGraphicsContext(Arguments args)
        {
            if (TryEvaluate((string)args[0], out long address))
            {
                SpectrumVariables.SetGfxContext((int)address);
                Console.WriteLine($"GFX set to {SpectrumVariables.Gfx:X8}");
            }
        }

        [SpectrumCommand(
            Name = "gbiclean",
            Cat = SpectrumCommand.Category.Gbi,
            Description = "Zero clears 'garbage' data in frame display list buffers")]
        private static void CleanFrameDlistBuffers(Arguments args)
        {
            if (Gfx == 0)
                return;

            var dlists = GetFrameDlists(Gfx);

            foreach (var item in dlists)
            {
                var freeSize = item.AppendEndPtr - item.StartPtr;
                if (item.Size < 0
                    || item.Size > 0x100000
                    || freeSize < 0
                    || freeSize > item.Size
                    || item.AppendStartPtr % 8 != 0
                    || item.AppendEndPtr % 8 != 0)
                {
                    Console.WriteLine($"Error with {item.Name}: {item.RecordPtr}");
                    return;
                }
                int i = 0;
                while (item.AppendStartPtr + i != item.AppendEndPtr)
                {
                    Zpr.WriteRam32(item.AppendStartPtr + i, 0);
                    Zpr.WriteRam32(item.AppendStartPtr + i + 4, 0);
                    i += 8;
                }
            }
        }


        [SpectrumCommand(
            Name = "gbi",
            Cat = SpectrumCommand.Category.Gbi,
            Description = "Prints out a display list, starting at the specified address and without following jumps")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.EXPRESSION_S })]
        private static void PrintGbi(Arguments args)
        {
            if (!TryEvaluate((string)args[0], out long address))
                return;

            Console.Clear();
            SimplePrintGbi(address, 0x100);
        }

        private static void SimplePrintGbi(N64Ptr address, int bytes)
        {
            var data = Zpr.ReadRam((int)address, bytes);

            using BinaryReader br = new BinaryReader(new MemoryStream(data));
            int i = 0;
            foreach (var line in uCode.MicrocodeParser.SimpleParse(br, bytes / 8))
            {
                Console.WriteLine($"{address + i:X6}: {line}");
                i += 8;
            }
        }

        [SpectrumCommand(
            Name = "gbidump",
            Cat = SpectrumCommand.Category.Gbi,
            Description = "Creates a text dump of a display list, starting at the specified address")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.EXPRESSION_S })]
        private static void DumpGbi(Arguments args)
        {
            if (!TryEvaluate((string)args[0], out long address))
                return;

            var data = Zpr.ReadRam(0, GetRamSize());

            string gameStr = string.Format("{0}_{1}_frame.txt",
                (Options.Version == Game.OcarinaOfTime) ? "oot" : (Options.Version == Game.MajorasMask) ? "mm" : "unk",
                Options.Version.ToString());


            using StreamWriter sw = new StreamWriter("dump/" + gameStr);
            using BinaryReader memory = new BinaryReader(new MemoryStream(data));
            foreach (var line in MicrocodeParser.DeepParse(memory, address))
            {
                sw.WriteLine(line);
            }
        }

        [SpectrumCommand(
            Name = "gbidumpimg",
            Cat = SpectrumCommand.Category.Gbi,
            Description = "Creates a text dump of all texture related instructions in a display list task")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.EXPRESSION_S })]
        private static void DumpGbiTextures(Arguments args)
        {
            if (!TryEvaluate((string)args[0], out long address))
                return;

            var fileMap = MemoryMapper.GetRamMap(Options, true).OfType<IFile>().ToList();

            var data = Zpr.ReadRam(0, GetRamSize());

            string gameStr = string.Format("{0}_{1}_frame_img.txt",
                (Options.Version == Game.OcarinaOfTime) ? "oot" : (Options.Version == Game.MajorasMask) ? "mm" : "unk",
                Options.Version.ToString());


            using (StreamWriter sw = new StreamWriter("dump/" + gameStr))
            using (BinaryReader memory = new BinaryReader(new MemoryStream(data)))
            {
                Queue<string> writeBuffer = new Queue<string>();
                Queue<G_> gbiQueue = new Queue<G_>();
                foreach (var line in MicrocodeParser.DeepTrace(memory, address))
                {
                    var gbi = line.gbi;
                    gbiQueue.Enqueue(gbi.Name);
                    switch (gbi.Name)
                    {
                        case G_.G_SETTIMG:
                            {
                                var imgAddr = GetAddress(line.task, gbi.EncodingLow);
                                var file = GetIFile(fileMap, imgAddr);
                                var print = MicrocodeParser.PrintMicrocode(memory, gbi, false);
                                writeBuffer.Enqueue(
                                    $"{(int)line.ptr:X6}:{$"{imgAddr & 0xFFFFFF:X6}:{(int)GetRom(file, imgAddr):X8}",-18}{print}"
                                    );
                            }
                            break;
                        case G_.G_LOADBLOCK:
                        case G_.G_LOADTILE:
                        case G_.G_LOADTLUT:
                        case G_.G_SETTILE:
                        case G_.G_RDPLOADSYNC:
                        case G_.G_RDPPIPESYNC:
                        case G_.G_SETTILESIZE:
                            writeBuffer.Enqueue($"{(int)line.ptr:X6}:{"",-18}{MicrocodeParser.PrintMicrocode(memory, gbi, false)}");
                            break;
                        default:
                            writeBuffer.Enqueue(""); break;
                    }

                    TestTexture();
                    TestTLUT();

                    if (gbiQueue.Count == 7)
                    {
                        gbiQueue.Dequeue();
                        var v = writeBuffer.Dequeue();
                        if (v.Length != 0)
                            sw.WriteLine(v);
                    }
                }
                Dequeue();

                void Dequeue()
                {
                    while (gbiQueue.Count > 0)
                    {
                        gbiQueue.Dequeue();
                        var v = writeBuffer.Dequeue();
                        if (v.Length != 0)
                            sw.WriteLine(v);
                    }
                }
                void TestTexture()
                {
                    if (gbiQueue.Count < 7)
                        return;
                    int i = 0;
                    bool isBlock = true;
                    bool isTile = true;
                    foreach (var item in gbiQueue)
                    {
                        if (Macros.gsDPLoadTextureBlock[i] != item)
                            isBlock = false;
                        if (Macros.gsDPLoadTextureTile[i] != item)
                            isTile = false;

                        i++;
                        if (i == 7)
                            break;
                    }
                    if (!isTile && !isBlock)
                        return;

                    sw.WriteLine($"---- gsDPLoadTexture{((isBlock) ? "Block" : "Tile")} ----");
                    Dequeue();
                    sw.WriteLine();
                }
                void TestTLUT()
                {
                    //Test TLUT
                    if (gbiQueue.Count < 6)
                        return;
                    int i = 0;
                    foreach (var item in gbiQueue)
                    {
                        if (Macros.gsDPLoadTLUT[i] != item)
                            return;
                        i++;
                        if (i == 6)
                            break;
                    }
                    sw.WriteLine($"---- gsDPLoadTLUT ----");
                    Dequeue();
                    sw.WriteLine();
                }
            }


            N64Ptr GetAddress(MicrocodeParserTask task, uint addr)
            {
                var id = (addr & 0x0F000000) >> 24;
                return task.SegmentTable[id] + (addr & 0xFFFFFF);
            }
            N64Ptr GetRom(IFile file, N64Ptr ptr)
            {
                if (file == null)
                    return 0;
                return file.VRom.Start + (ptr - file.Ram.Start);
            }
        }

        [SpectrumCommand(
            Name = "gbitrace",
            Sup = SpectrumCommand.Supported.OoT, //can't let you do that STAR FOX
            Cat = SpectrumCommand.Category.Gbi,
            Description = "Auto-documents display lists for the current frame")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.EXPRESSION_S })]
        private static void TraceGbi(Arguments args)
        {
            if (GlobalContext == 0)
                return;

            var topDlist = GlobalContext.Deref().RelOff(0xB8).Deref();

            var ramMap = MemoryMapper.GetRamMap(Options, true).OfType<IFile>().ToList();
            var data = Zpr.ReadRam(0, GetRamSize());

            using BinaryReader br = new BinaryReader(new MemoryStream(data));
            var traceEnumerable = MicrocodeParser.DeepTrace(br, (int)topDlist);
            var traceEnumerator = traceEnumerable.GetEnumerator();
            traceEnumerator.MoveNext();
            TraceGbiRecursive(traceEnumerator, ramMap);
        }


        static GfxDList[] LoggedDisplayListState = null;
        [SpectrumCommand(
            Name = "gbistart",
            Cat = SpectrumCommand.Category.Gbi,
            Description = "Sets start point for logging changes in the frame display lists")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { })]
        private static void GbiStartLogging(Arguments args)
        {
            GbiLogging(true);
        }

        [SpectrumCommand(
            Name = "gbiend",
            Cat = SpectrumCommand.Category.Gbi,
            Description = "Prints changed display lists since last gbistart")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { })]
        private static void GbiEndLogging(Arguments args)
        {
            GbiLogging(false);
        }

        private static void GbiLogging(bool isStart)
        {
            if (Gfx == 0)
            {
                Console.WriteLine("Graphics Context pointer can't be found");
                if (isStart)
                    LoggedDisplayListState = null;
                return;
            }
            var frameDlist = GetFrameDlists(Gfx);
            if (isStart)
            {
                LoggedDisplayListState = frameDlist;
                Console.Clear();
                Console.WriteLine("Gbi Log Start: ");
                PrintGraphicsContext(LoggedDisplayListState);
                return;
            }
            //Process End Logging
            if (frameDlist == null)
            {
                Console.WriteLine("Cannot Log");
                return;
            }
            //test if log start was from the same frame
            if (LoggedDisplayListState.Length != frameDlist.Length)
            {
                Console.WriteLine("Incompatible logs");
                return;
            }

            Console.WriteLine("Gbi Log End: ");
            PrintGraphicsContext(frameDlist);
            Console.WriteLine();

            (GfxDList ds, GfxDList de, int startDelta, int endDelta)[] result = new (GfxDList, GfxDList, int, int)[frameDlist.Length];
            for (int i = 0; i < frameDlist.Length; i++)
            {
                result[i] =
                    (LoggedDisplayListState[i], frameDlist[i],
                    frameDlist[i].AppendStartPtr - LoggedDisplayListState[i].AppendStartPtr,
                    LoggedDisplayListState[i].AppendEndPtr - frameDlist[i].AppendEndPtr);

                if (result[i].startDelta < 0 || result[i].endDelta < 0)
                {
                    Console.WriteLine("Log start is from different frame");
                    return;
                }
            }
            foreach (var (ds, de, startDelta, endDelta) in result)
            {
                Console.WriteLine(
                    $"{ds.Name,-13}  {ds.AppendStartPtr:X8}: {startDelta:X4} {de.AppendEndPtr:X8}: {endDelta:X4}");
                SimplePrintGbi(ds.AppendStartPtr, startDelta);
            }
        }


        private static void TraceGbiRecursive(IEnumerator<(N64Ptr ptr, Microcode gbi, MicrocodeParserTask task)> trace, List<IFile> ramMap)
        {
            var start = trace.Current;
            var cur = start;

            while (trace.MoveNext()
                && cur.gbi.Name != G_.G_ENDDL)
            {
                //trace.Current = next gbi instruction
                if (cur.gbi.Name == G_.G_DL)
                {
                    TraceGbiRecursive(trace, ramMap);
                }
                cur = trace.Current;
            }
            var endOffset = cur.ptr - start.ptr;

            var file = GetIFile(ramMap, start.ptr);

            if (file == null)
                return;

            DisplayListLogger.AddReference(Options.Version, file, start.ptr, new DisplayListRecord(endOffset));
        }

        [SpectrumCommand(
            Name = "gbibindump",
            Sup = SpectrumCommand.Supported.OoT, //oops
            Cat = SpectrumCommand.Category.Gbi_bin,
            Description = "Dumps gbi buffer for frame")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.S8 },
            Help = "{0} = -1, 0, 1")]
        private static void DumpFrame(Arguments args)
        {
            int frameOffset = (sbyte)args[0];

            if (frameOffset < -1 && frameOffset > 1)
                return;

            frameOffset *= 0x12410;

            var addr = Gfx.ReadInt32(0);
            addr += frameOffset;

            using BinaryWriter bw = new BinaryWriter(new FileStream("dump/frame.bin", FileMode.Create));
            byte[] frameData = Zpr.ReadRam(addr, 0x12400);
            bw.Write(frameData);
        }

        [SpectrumCommand(
            Name = "gbibinload",
            Cat = SpectrumCommand.Category.Gbi_bin,
            Description = "Restores gbi buffer for frame from file 'dump/frame.bin'",
            Sup = SpectrumCommand.Supported.OoT)]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.S8 },
            Help = "{0} = -1, 0, 1")]
        private static void LoadFrame(Arguments args)
        {
            N64Ptr frameBuffer;
            uint delta;
            N64Ptr frameContextPtr1 = 0x8016A648;
            N64Ptr frameContextPtr2 = 0x8017CA58;

            var ptrs = Constants.GetFramebufferPointers(Options.Version);
            int buffer0 = ptrs[0];
            int buffer1 = ptrs[1];

            // Constants.  
            N64Ptr frameBufferPtr1 = buffer0; //0x803B5000;
            N64Ptr frameBufferPtr2 = buffer1; //0x803DA800;

            byte[] convertFrameContext = new byte[] { 0x01, 0xDA, 0xDB, 0xDC, 0xDE };

            if (!File.Exists("dump/frame.bin"))
                return;

            int frameOffset = (sbyte)args[0];

            if (frameOffset < -1 && frameOffset > 1)
                return;

            frameOffset *= 0x12410;

            N64Ptr frame = Gfx.ReadInt32(0) + frameOffset;

            if (frame == frameContextPtr1)
            {
                frameBuffer = frameBufferPtr1;
                delta = (uint)(frameContextPtr2 - frame);
            }
            else if (frame == frameContextPtr2)
            {
                frameBuffer = frameBufferPtr2;
                delta = (uint)(frameContextPtr1 - frame);
            }
            else
            {
                Console.WriteLine("out of bounds");
                return;
            }

            using BinaryReader br = new BinaryReader(new FileStream("dump/frame.bin", FileMode.Open));
            for (int i = 0; i < 0x12400; i += 8)
            {
                var input = new Microcode(br);

                if (input.EncodingHigh == 0xDB06003C
                    || input.EncodingHigh == 0xFF10013F
                    && (input.EncodingLow == frameBufferPtr1 || input.EncodingLow == frameBufferPtr2))
                {
                    input = new Microcode(input.EncodingHigh, frameBuffer);
                }
                else if (convertFrameContext.Contains((byte)input.Name))
                {
                    if (input.EncodingLow >= frame + delta && input.EncodingLow < frame + delta + 0x12400)
                        input = new Microcode(input.EncodingHigh, input.EncodingLow - delta);
                }

                Zpr.WriteRam32(frame + i, (int)input.EncodingHigh);
                Zpr.WriteRam32(frame + i + 4, (int)input.EncodingLow);
            }
        }

        #endregion

        #region Collision

        [SpectrumCommand(
            Name = "colctx",
            Cat = SpectrumCommand.Category.Collision,
            Description = "Prints Collision Context (Global Context + 0x7C0 oot, + 0x830 mm)")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { })]
        private static void PrintCollisionContext(Arguments args)
        {
            Console.Clear();
            CollisionCtx cctx = new CollisionCtx(GetColCtxPtr(), Options.Version);
            Console.WriteLine(cctx);
        }

        [SpectrumCommand(
            Name = "colm",
            Cat = SpectrumCommand.Category.Collision,
            Description = "Gets 'complex mesh' collision data")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { })]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.HEX_S16 },
            Help = "{0} = Mesh Id, 0x32 = Scene Mesh")]
        private static void GetActorCollision(Arguments args)
        {
            Console.Clear();

            if (args.Length == 1)
            {
                var mesh = GetCollisionMesh((short)args[0]);
                if (mesh != null)
                    Console.WriteLine(mesh);
            }
            else
            {
                PrintBgActorCollision();
            }
        }

        private static void PrintBgActorCollision()
        {
            var ptr = GetColCtxPtr();
            List<(int, BgActor)> bgActors = new List<(int, BgActor)>();
            for (int i = 0; i < 50; i++)
            {
                BgActor actor = new BgActor(ptr.RelOff(0x54 + (0x64 * i)));
                if (!actor.ActorInstance.IsNull())
                {
                    bgActors.Add((i, actor));
                }
            }

            foreach (var (i, actor) in bgActors)
            {
                //IFile obj = GetIFile(actor.MeshPtr);

                //CollisionActorDoc.AddNewRecord(actor, obj, Options.Version);

                Console.WriteLine($"{i:X2} {actor}");
            }
        }

        [SpectrumCommand(
            Name = "cola",
            Cat = SpectrumCommand.Category.Collision,
            Description = "Gets 'simple body' collision data")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { },
            Help = "Print hitbox list")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.EXPRESSION_S },
            Help = "{0} = Collision Body")]
        private static void CommandCola(Arguments args)
        {
            if (args.Length == 0)
            {
                GetActorBodyCollisionLists();
            }
            else
            {
                if (!TryEvaluate((string)args[0], out long addr))
                    return;
                Console.Clear();
                Console.WriteLine(ColliderShape.Initialize(SPtr.New(addr)));
            }
        }
        private static void GetActorBodyCollisionLists()
        {
            var pools = GetActorCollisionPools();
            Console.Clear();
            foreach (var (desc, shapes) in pools)
            {
                Console.WriteLine(desc);
                foreach (var shape in shapes)
                {
                    Console.WriteLine($" {shape.collider}");
                }
                Console.WriteLine();
            }
        }

        private static List<(string description, List<ColliderShape>)> GetActorCollisionPools()
        {
            var result = new List<(string, List<ColliderShape>)>();

            int colaOffset;

            if (Options.Version.Game == Game.OcarinaOfTime)
                colaOffset = 0x11E60;
            else if (Options.Version == MRom.Build.J0
                || Options.Version == MRom.Build.J1)
                colaOffset = 0x18864;
            else
                colaOffset = 0x18884;

            Ptr colPtr = GlobalContext.RelOff(colaOffset);

            //get AT collection
            short colAtCount = colPtr.ReadInt16(0);
            ushort colAtUnk = colPtr.ReadUInt16(2);

            string colAt = $"{colPtr}: colAT, {colAtCount:D2} elements, {colAtUnk:X4}";
            List<ColliderShape> shapes = new List<ColliderShape>();

            result.Add((colAt, shapes));

            if (colAtCount <= 70)
            {
                var curPtr = colPtr.RelOff(4);
                for (int i = 0; i < (colAtCount * 4); i += 4)
                {
                    var shape = ColliderShape.Initialize(curPtr.RelOff(i).Deref());
                    shapes.Add(shape);
                }
            }

            result.Add(ColB_GetGroup("AC", colPtr.RelOff(0xCC)));
            result.Add(ColB_GetGroup("OT", colPtr.RelOff(0x1C0)));
            return result;
        }

        private static (string, List<ColliderShape>) ColB_GetGroup(string name, Ptr colPtr)
        {
            int count = colPtr.ReadInt32(0);

            string description = $"{colPtr}: col{name}, {count:D2} elements";
            List<ColliderShape> shapes = new List<ColliderShape>();

            if (count <= 70)
            {
                colPtr = colPtr.RelOff(4);
                for (int i = 0; i < (count * 4); i += 4)
                {
                    var shape = ColliderShape.Initialize(colPtr.RelOff(i).Deref());
                    shapes.Add(shape);
                }
            }
            return (description, shapes);
        }

        [SpectrumCommand(
            Name = "colxyz",
            Cat = SpectrumCommand.Category.Collision,
            Description = "converts xyz into scene collision hashtable coords/finds record")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.COORDS_FLOAT },
            Help = "{0} = World x, y, z")]
        private static void GetSceneCollisionCoords(Arguments args)
        {
            Vector3<float> coords = (Vector3<float>)args[0];
            GetColInfo(coords, false);
        }

        [SpectrumCommand(
            Name = "colsec",
            Cat = SpectrumCommand.Category.Collision,
            Description = "converts collision sector coords into bounding box for that unit")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.COORDS_FLOAT },
            Help = "{0} = Sector x, y, z")]
        private static void GetSceneCollsionSection(Arguments args)
        {
            Vector3<float> coords = (Vector3<float>)args[0];
            GetColInfo(coords, true);
        }

        private static void GetColInfo(Vector3<float> xyz, bool coordsAreColSec)
        {
            CollisionCtx colctx = new CollisionCtx(GetColCtxPtr(), Options.Version);

            int[] colsec;

            if (coordsAreColSec)
            {
                colsec = new int[3]
                {
                    (int)xyz.x,
                    (int)xyz.y,
                    (int)xyz.z
                };
            }
            else //compute colsec
            {
                colsec = colctx.ComputeColSec(xyz);
            }

            N64Ptr sectorDataAddr = colctx.GetColSecDataPtr(colsec);

            float[] blockMin = new float[3];
            float[] blockMax = new float[3];

            for (int i = 0, off = 0; i < 3; i++, off += 4)
            {
                blockMin[i] = colsec[i] * colctx.unitSize.Index(i) + colctx.boxmin.Index(i);
                blockMax[i] = (colsec[i] + 1) * colctx.unitSize.Index(i) + colctx.boxmin.Index(i);
            }

            Console.WriteLine($"Collision Unit Max: ({colctx.max.x}, {colctx.max.y}, {colctx.max.z})");
            Console.WriteLine($"Unit Size: ({colctx.unitSize.x}, {colctx.unitSize.y}, {colctx.unitSize.z})");
            Console.WriteLine($"Sector: {sectorDataAddr:X8} ({colsec[0]}, {colsec[1]}, {colsec[2]})");
            Console.WriteLine($"Sector Min: ({blockMin[0]}, {blockMin[1]}, {blockMin[2]})");
            Console.WriteLine($"Sector Max: ({blockMax[0]}, {blockMax[1]}, {blockMax[2]})");
            Console.WriteLine();
        }


        [SpectrumCommand(
            Name = "dumpcolsec",
            Cat = SpectrumCommand.Category.Collision,
            Description = "Dumps the collision sector state to file")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.COORDS_FLOAT },
            Help = "{0} = Sector x, y, z")]
        private static void GetColSecPoly(Arguments args)
        {
            Vector3<float> xyz = (Vector3<float>)args[0];
            int[] colsec = new int[3]
            {
                (int)xyz.x,
                (int)xyz.y,
                (int)xyz.z
            };

            CollisionCtx ctx = new CollisionCtx(GetColCtxPtr(), Options.Version);
            BgMesh mesh = GetCollisionMesh(0x32);


            N64Ptr colsecAddr = ctx.GetColSecDataPtr(colsec);
            Ptr colsecPtr = SPtr.New(colsecAddr);

            TextWriter console = Console.Out;

            string gameStr = string.Format("{0}_{1}_colsec.txt",
                (Options.Version == Game.OcarinaOfTime) ? "oot" : (Options.Version == Game.MajorasMask) ? "mm" : "unk",
                Options.Version.ToString());


            using (StreamWriter sw = new StreamWriter("dump/" + gameStr))
            {
                Console.SetOut(sw);
                Console.WriteLine($"{colsec[0]},{colsec[1]},{colsec[2]}");
                for (int off = 0; off < 0x06; off += 2)
                {
                    int depthLimit = 1000;
                    short topLinkId = colsecPtr.ReadInt16(off);
                    short linkId = topLinkId;

                    while (depthLimit > 0 && linkId != -1)
                    {
                        depthLimit--;

                        //Get Next Link Record
                        short polyId = ctx.Links.ReadInt16(linkId * 4);
                        linkId = ctx.Links.ReadInt16(linkId * 4 + 2);

                        var polyInfo = mesh.GetPolyById(polyId);
                        Console.WriteLine($"{off:X2}\t{topLinkId:X4}\t{polyId:X4}\t{linkId:X4}\t{polyInfo.TSV()}");
                    }
                }
                Console.SetOut(console);
            }
            console.WriteLine($"{gameStr} created.");
        }

        [SpectrumCommand(
            Name = "colsecinfo",
            Cat = SpectrumCommand.Category.Collision,
            Description = "Outputs the colsec info")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.COORDS_FLOAT },
            Help = "{0} = Sector x, y, z")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.HEX_S16, Tokens.COORDS_FLOAT },
            Help = "{0} = PolyId;" +
                   "{1} = Sector x, y, z")]
        private static void GetColSecInfo(Arguments args)
        {
            Vector3<float> xyz;
            short searchPolyId;

            if (args.Length == 1)
            {
                searchPolyId = -1;
                xyz = (Vector3<float>)args[0];
            }
            else
            {
                searchPolyId = (short)args[0];
                xyz = (Vector3<float>)args[1];
            }

            int[] colsec = new int[3]
            {
                (int)xyz.x,
                (int)xyz.y,
                (int)xyz.z
            };


            CollisionCtx ctx = new CollisionCtx(GetColCtxPtr(), Options.Version);
            if (!ctx.ColSecInBounds(colsec))
                return;

            string[] type = new string[3]
            {
                "Floor",
                "Wall",
                "Ceiling"
            };

            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine(type[i]);
                foreach (var item in ctx.YieldPolyChain(colsec, i))
                {
                    var (polyId, linkId) = item;
                    if (searchPolyId == -1 || polyId == searchPolyId)
                    {
                        Console.WriteLine($"{polyId:X4}:{linkId:X4}");
                    }
                }
            }
        }


        [SpectrumCommand(
            Name = "colsecf",
            Cat = SpectrumCommand.Category.Collision,
            Description = "Locates all instances of polyId")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.HEX_S16 },
            Help = "{0} = PolyId")]
        private static void FindColSecPoly(Arguments args)
        {
            short searchPolyId = (short)args[0];
            int[] colsec = new int[3];

            CollisionCtx ctx = new CollisionCtx(GetColCtxPtr(), Options.Version);

            string[] type = new string[3]
            {
                "Floor",
                "Wall",
                "Ceiling"
            };

            for (colsec[0] = 0; colsec[0] < ctx.max.x; colsec[0]++)
                for (colsec[1] = 0; colsec[1] < ctx.max.y; colsec[1]++)
                    for (colsec[2] = 0; colsec[2] < ctx.max.z; colsec[2]++)
                        for (int i = 0; i < 3; i++)
                        {
                            foreach (var item in ctx.YieldPolyChain(colsec, i))
                            {
                                var (polyId, linkId) = item;
                                if (polyId == searchPolyId)
                                {
                                    Console.WriteLine($"{colsec[0]},{colsec[1]},{colsec[2]}");
                                    Console.WriteLine($"{type[i]}: {polyId:X4}:{linkId:X4}");
                                }
                            }
                        }
        }


        [SpectrumCommand(
            Name = "colpoly",
            Cat = SpectrumCommand.Category.Collision,
            Description = "Returns stats on a collision poly surface")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.HEX_S16, Tokens.EXPRESSION_S },
            Help =
            "{0} = Collision Id. 0x32 = Scene Collision;" +
            "{1} = Address Expression")]
        private static void GetCollisionPoly(Arguments args)
        {
            int id = (short)args[0];
            if (!TryEvaluate((string)args[1], out long addr))
                return;

            BgMesh mesh = GetCollisionMesh(id);
            if (mesh == null)
                return;
            Console.WriteLine(mesh.GetPolyFormattedInfo(addr));
        }

        public static Ptr GetColCtxPtr()
        {
            int colctxOff = (Options.Version.Game == Game.OcarinaOfTime) ?
                0x7C0 : 0x830;
            return GlobalContext.RelOff(colctxOff);
        }

        private static BgMesh GetCollisionMesh(int id)
        {
            if (id < 0 || id > 0x32)
                return null;

            Ptr MeshPtr;

            Ptr ColContext = GetColCtxPtr();
            if (id == 0x32)
                MeshPtr = ColContext.Deref();
            else
                MeshPtr = ColContext.RelOff(0x58).Deref(0x64 * id);

            return new BgMesh(MeshPtr);
        }

        [SpectrumCommand(
            Name = "apoly",
            Cat = SpectrumCommand.Category.Collision,
            Description = "Prints Actor's Poly Collision info")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.EXPRESSION_S })]
        private static void GetActorPolyInfo(Arguments args)
        {
            if (!TryEvaluate((string)args[0], out long addr))
                return;

            if (Options.Version.Game != Game.OcarinaOfTime)
                return;
            Ptr actor = SPtr.New(new N64Ptr(addr));
            N64Ptr wallPoly = actor.Deref(0x74);
            N64Ptr floorPoly = actor.Deref(0x78);
            byte wallPolySource = actor.ReadByte(0x7C);
            byte floorPolySource = actor.ReadByte(0x7D);

            var list = new (string, N64Ptr, byte)[]
            {
                ("Wall", wallPoly, wallPolySource),
                ("Floor", floorPoly, floorPolySource)
            };

            Console.Clear();
            foreach (var item in list)
            {
                var (name, polyPtr, source) = item;
                Console.WriteLine($"{name}: {polyPtr} {source:X2}");
                if (polyPtr != 0)
                {
                    BgMesh mesh = GetCollisionMesh(source);
                    var poly = mesh.GetPolyFormattedInfo(polyPtr);
                    Console.WriteLine(poly);
                }
            }
        }

        #endregion

        #region Conversion

        [SpectrumCommand(
            Name = "f",
            Cat = SpectrumCommand.Category.Conversion,
            Description = "Converts floating point to hex representation")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.FLOAT })]
        private static void FloatToHex(Arguments args) { ToHex(args); }

        [SpectrumCommand(
            Name = "i",
            Cat = SpectrumCommand.Category.Conversion,
            Description = "Converts integer to hex representation")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.S32 })]
        private static void IntToHex(Arguments args) { ToHex(args); }

        private static void ToHex(Arguments args)
        {
            if (args[0] is float f)
            {
                var bytes = BitConverter.GetBytes(f);
                Console.WriteLine("{0:X8}", BitConverter.ToInt32(bytes, 0));
            }
            else if (args[0] is int s32)
            {
                Console.WriteLine($"{s32:X8}");
            }
        }

        [SpectrumCommand(
            Name = "ff",
            Cat = SpectrumCommand.Category.Conversion,
            Description = "Converts hex to floating point")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.HEX_FLOAT })]
        private static void FloatFromHex(Arguments args) { FromHex(args); }

        [SpectrumCommand(
            Name = "ii",
            Cat = SpectrumCommand.Category.Conversion,
            Description = "Converts hex to integer")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.HEX_S32 })]
        private static void IntFromHex(Arguments args) { FromHex(args); }

        private static void FromHex(Arguments args)
        {
            if (args[0] is int integer)
            {
                if (integer < 0)
                {
                    Console.WriteLine($"{integer} or {(uint)integer}");
                }
            }
            Console.WriteLine(args[0]);
        }

        #endregion

        #region Write
        [SpectrumCommand(
            Name = "w8",
            Cat = SpectrumCommand.Category.Write,
            Description = "Write 8 bit data")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.EXPRESSION_S, Tokens.HEX_U8 },
            Help = "{0} = Address; {1} = Value to write")]
        private static void Write8(Arguments args)
        {
            WriteRam((string)args[0], (byte)args[1]);
        }

        [SpectrumCommand(
            Name = "w16",
            Cat = SpectrumCommand.Category.Write,
            Description = "Write 16 bit data")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.EXPRESSION_S, Tokens.HEX_S16 },
            Help = "{0} = Address; {1} = Value to write")]
        private static void Write16(Arguments args)
        {
            WriteRam((string)args[0], (short)args[1]);
        }
        [SpectrumCommand(
            Name = "w32",
            Cat = SpectrumCommand.Category.Write,
            Description = "Write 32 bit data")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.EXPRESSION_S, Tokens.HEX_S32 },
            Help = "{0} = Address; {1} = Value to write")]
        private static void Write32(Arguments args)
        {
            WriteRam((string)args[0], (int)args[1]);
        }
        [SpectrumCommand(
            Name = "wf",
            Cat = SpectrumCommand.Category.Write,
            Description = "Write float")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.EXPRESSION_S, Tokens.FLOAT },
            Help = "{0} = Address; {1} = Value to write")]
        private static void WriteFloat(Arguments args)
        {
            WriteRam((string)args[0], (float)args[1]);
        }

        [SpectrumCommand(
            Name = "w32t",
            Cat = SpectrumCommand.Category.Proto,
            Description = "Write a series of 32 bit data")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.EXPRESSION_S },
            Help = "{0} = Address; Values are \n separated, address must be 4 byte aligned")]
        private static void WriteS32s(Arguments args)
        {
            if (!TryEvaluate((string)args[0], out long address))
                return;

            N64Ptr addr = address;
            Console.WriteLine($"Overwriting {addr}:");
            if (addr % 4 != 0)
            {
                Console.WriteLine("Alignment error");
                return;
            }

            var data = Console.ReadLine();
            var lines = data.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => int.Parse(x.Trim(), NumberStyles.HexNumber));

            var cur = addr;
            foreach (var item in lines)
            {
                Zpr.WriteRam32(cur, item);
                cur += 4;
            }

            PrintRam(addr, PrintRam_X8, 1);
        }
        #endregion

        #region VerboseOcarina
        [SpectrumCommand(
            Name = "rompath",
            Sup = SpectrumCommand.Supported.OoT | SpectrumCommand.Supported.MM,
            Description = "Sets the rom location for the currently assigned version",
            Cat = SpectrumCommand.Category.VerboseOcarina)]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.PATH },
            Help = "{0} = Path to rom")]
        private static void SetRomPath(Arguments args)
        {
            string path = (string)args[0];
            if (!File.Exists(path))
            {
                Console.WriteLine($"Cannot find rom file: {path}");
                return;
            }
            else
            {
                PathUtil.SetRomLocation(Options.Version, path);
                Console.WriteLine($"Saved path to {Options.Version}: {path}");
            }
        }

        [SpectrumCommand(
            Name = "vo",
            Sup = SpectrumCommand.Supported.OoT | SpectrumCommand.Supported.MM,
            Description = "Runs VerboseOcarina command",
            Cat = SpectrumCommand.Category.VerboseOcarina)]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.LITERAL, Tokens.STRING },
            Help = "{0} = Command. Currently supported commands:;" +
            " scene;" +
            " actor;" +
            "{1} = Command args")]
        private static void GetVerboseOcarinaOutput(Arguments args)
        {
            string literal = ((string)args[0]).ToLowerInvariant();
            string p = (string)args[1];

            switch(literal)
            {
                case "scene":
                    int sceneId = -1;
                    int roomId = -1;

                    var split = p.Split('.', StringSplitOptions.TrimEntries);
                    if (int.TryParse(split[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int parse))
                    {
                        sceneId = parse;
                    }
                    if (split.Length > 1 && int.TryParse(split[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out parse))
                    {
                        roomId = parse;
                    }

                    if (sceneId < 0)
                    {
                        Console.WriteLine($"Invalid scene id: {sceneId}");
                        return;
                    }

                    if (!TryGetCurRom())
                    {
                        Console.WriteLine($"Path invalid for {Options.Version}");
                        return;
                    }

                    StringBuilder sb = new StringBuilder();
                    VerboseOcarinaGetScene(sceneId, roomId, sb);
                    Console.Clear();
                    Console.WriteLine(sb.ToString());
                    break;
                case "actor":
                    if (!int.TryParse(p, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int actorId))
                    {
                        Console.WriteLine($"Invalid actor id {actorId}");
                        return;
                    }

                    if (!TryGetCurRom())
                    {
                        Console.WriteLine($"Path invalid for {Options.Version}");
                        return;
                    }
                    Console.Clear();
                    Console.Write(SceneRoomReader.GetActorsById(curRom, actorId));
                    break;
            }
        }

        private static void VerboseOcarinaGetScene(int sceneId, int roomId, StringBuilder sb)
        {
            Scene scene = SceneRoomReader.InitializeScene(curRom.Files.GetSceneFile(sceneId), sceneId);
            List<Room> rooms = new List<Room>();
            if (scene == null)
            {
                sb.AppendFormat("Exception: Scene not found");
                sb.AppendLine();
                return;
            }

            List<FileAddress> roomAddr = scene.Header.GetRoomAddresses();

            if (roomId >= 0 && roomAddr.Count > roomId)
            {
                var temp = roomAddr;
                roomAddr = new List<FileAddress>
                {
                    temp[roomId]
                };
            }
            else
            {
                roomAddr = scene.Header.GetRoomAddresses();
            }

            foreach (FileAddress addr in roomAddr)
            {
                try
                {
                    RomFile file = curRom.Files.GetFile(addr);
                    rooms.Add(SceneRoomReader.InitializeRoom(file));
                }
                catch
                {
                    sb.AppendLine($"Exception: room {addr.Start:X8} not found");
                }
            }

            sb.Append(SceneRoomReader.ReadScene(scene));

            if (roomId < 0)
            {
                for (int i = 0; i < rooms.Count; i++)
                {
                    sb.AppendLine($"Room {i}");
                    sb.Append(SceneRoomReader.ReadRoom(rooms[i]));
                }
            }
            else
            {
                sb.AppendLine($"Room {roomId}");
                sb.Append(SceneRoomReader.ReadRoom(rooms[0]));
            }    
        }

        private static bool TryGetCurRom()
        {
            if (curRom == null || curRom.Version != Options.Version)
            {
                if (!PathUtil.TryGetRomLocation(Options.Version, out string romPath))
                {
                    return false;
                }
                curRom = Rom.New(romPath, Options.Version);
            }
            return true;
        }
        #endregion


        [SpectrumCommand(
            Name = "name",
            Cat = SpectrumCommand.Category.Actor,
            Description = "Displays a list of actor names and their associated ids",
            Sup = SpectrumCommand.Supported.OoT | SpectrumCommand.Supported.MM)]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.LITERAL })]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.LITERAL, Tokens.HEX_S32 })]
        private static void PrintActorNames(Arguments args)
        {
            string nameType = (string)args[0];
            int index = (args.Length == 2) ? (int)args[1] : -1;

            Console.Clear();
            switch (nameType.ToLowerInvariant())
            {
                case "actor":
                    var info = XActorFactory.GetXActorList(Options.Version);
                    if (index != -1)
                    {
                        var actor = info.SingleOrDefault(x => short.Parse(x.id, NumberStyles.HexNumber) == index);
                        if (actor == null)
                        {
                            Console.WriteLine($"Invalid actor id {index}");
                        }
                        else
                        {
                            Console.WriteLine($"{short.Parse(actor.id, NumberStyles.HexNumber):X4}: {actor.name}, {actor.Description}");
                        }
                    }
                    else
                    {
                        foreach (var item in info)
                        {
                            Console.WriteLine($"{short.Parse(item.id, NumberStyles.HexNumber):X4}: {item.name}, {item.Description}");
                        }
                    }
                    break;
            }

        }

        [SpectrumCommand(
            Name = "age",
            Sup = SpectrumCommand.Supported.OoT,
            Description = "Sets Link's Age")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.U8 },
            Help = "{0} = 0 for Adult Link, 1 for Child Link, 2+ to kill the game :)")]
        private static void SetAge(Arguments args)
        {
            GlobalContext.Write(0x11DE8, (byte)args[0]);
        }

        [SpectrumCommand(
            Name = "loc",
            Sup = SpectrumCommand.Supported.OoT,
            Description = "Dumps Location Information")]
        private static void PrintLocation(Arguments args)
        {
            int scene;
            int room;

            scene = GlobalContext.ReadInt16(0x00A4);
            room = GlobalContext.ReadByte(0x11CBF);//1DA15F

            Console.WriteLine($"Scene {scene} Room {room}");
        }

        [SpectrumCommand(
            Name = "flag",
            Sup = SpectrumCommand.Supported.OoT,
            Description = "Reads or modifies the state of various flags"
            )]
        [SpectrumCommandSignature(Help = "Displays options")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.LITERAL, Tokens.LITERAL }
        )]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.LITERAL, Tokens.LITERAL, Tokens.HEX_S32 }
        )]
        private static void FlagCommand(Arguments args)
        {
            if (args.Length == 2)
            {
                OFlagsOperation.Process((string)args[0], (string)args[1], null);
            }
            else if (args.Length == 3)
            {
                OFlagsOperation.Process((string)args[0], (string)args[1], (int)args[2]);
            }
        }

        [SpectrumCommand(
            Name = "ef",
            Cat = SpectrumCommand.Category.Deprecated,
            Sup = SpectrumCommand.Supported.OoT,
            Description = "Sets event flag state")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.U8, Tokens.HEX_S16, Tokens.U8 },
            Help =
            "{0} = Value to write (0 or 1);" +
            "{1} = flag address, relative to the start of the Save Context;" +
            "{2} = number of the bit to flip (0-7)")]
        private static void EventFlag(Arguments args)
        {
            bool setFlag = (byte)args[0] > 0;
            short offset = (short)args[1];
            byte bit = (byte)args[2];

            if (!(offset >= 0xED4 && offset < 0xF34))
                return;

            int val = SaveContext.ReadByte(offset);

            if (setFlag)
            {
                int shift = 1 << bit;
                val |= shift;
            }
            else
            {
                int shift = 1 << bit;
                shift ^= 0xFF;
                val &= shift;
            }
            SaveContext.Write((int)offset, (byte)val);
        }

        [SpectrumCommand(
            Name = "time",
            Description = "Gets world time")]
        [SpectrumCommandSignature(Sig = new Tokens[] { })]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.HEX_U16 },
            Help = "{0} = Custom Time")]
        private static void GetTime(Arguments args)
        {
            ushort time;
            if (args.Length == 0)
                time = SaveContext.ReadUInt16(0xC);
            else
                time = (ushort)args[0];

            PrintTime(time);
        }

        private static void PrintTime(ushort time)
        {
            float f_time = ((float)time * 24) / 0x10000;
            int hour = (int)(f_time);
            int min = (int)(f_time * 60) % 60;
            int sec = (int)(f_time * 3600) % 60;
            Console.WriteLine($"Time: {hour:D2}:{min:D2}:{sec:D2}");
        }

        [SpectrumCommand(
            Name = "settime",
            Description = "Gets world time")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.LITERAL },
            Help = "{0} = Time;" +
            "You can specify time using HH:MM, or with a hex literal")]
        private static void SetTime(Arguments args)
        {
            string timeCode = (string)args[0];
            ushort time;
            if (timeCode.Contains(":"))
            {
                var code = timeCode.Split(new char[] { ':' });
                if (!int.TryParse(code[0], out int hr)
                    || !int.TryParse(code[1], out int min))
                {
                    Console.WriteLine("Invalid time code format. Expecting HH:MM");
                    return;
                }
                const int mins_in_day = 24 * 60;
                int totalMins = (hr * 60 + min) % mins_in_day;
                time = (ushort)((float)totalMins * 0x10000 / mins_in_day);

            }
            else if (!ushort.TryParse(timeCode, NumberStyles.HexNumber, new CultureInfo("en-US"), out time))
            {
                Console.WriteLine("Invalid hex code");
            }

            SaveContext.Write(0x0C, time);
            if (Options.Version.Game == Game.OcarinaOfTime)
            {
                SaveContext.Write(0x141A, time);
            }
            PrintTime(time);
        }

        [SpectrumCommand(
            Name = "thread",
            Description = "View the state of a thread(s) on last context swap")]
        [SpectrumCommandSignature(Sig = new Tokens[] { },
            Help = "Dump a listing of all initialized threads")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.EXPRESSION_S },
            Help = "{0} = Address of OSThread")]
        private static void ViewThread(Arguments args)
        {
            if (args.Length == 0)
            {
                ViewThreads(args);
                return;
            }
            if (!TryEvaluate((string)args[0], out long address))
                return;
            OSThread thread = new OSThread(SPtr.New((int)address));

            Console.Clear();
            thread.PrintState();
        }

        private static void ViewThreads(Arguments args)
        {
            Console.Clear();
            var threadStart = SpectrumVariables.Queue_Thread_Ptr;
            if (threadStart == 0)
                return;

            N64Ptr threadCur = threadStart.Deref(0x0C);

            int kill = 40;


            List<OSThread> threads = new List<OSThread>();
            while (kill-- > 0
                && threadCur != 0
                && threadCur != threadStart)
            {
                OSThread thread = new OSThread(SPtr.New(threadCur));
                threads.Add(thread);
                threadCur = thread.OSThread_tlnext;
            }
            foreach (var thread in threads)
            {
                thread.PrintBasicInfo();
                Console.WriteLine();
            }

        }


        #region Framebuffer

        [SpectrumCommand(
            Name = "view",
            Cat = SpectrumCommand.Category.Framebuffer,
            Description = "View framebuffer in console window live")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { },
            Help = "When command is running, press up and down to flip between buffers")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.EXPRESSION_S },
            Help = "{0} = address to view")]
        private static void ViewFrameBuffer(Arguments args)
        {
            if (args.Length == 0)
            {
                var ptrs = Constants.GetFramebufferPointers(Options.Version);
                int index = 0;
                while (index < ptrs.Count)
                {
                    var key = ColorBufferUtil.ViewFrameBuffer(ptrs[index]);

                    switch (key)
                    {
                        case ConsoleKey.UpArrow:
                            index = Math.Max(index - 1, 0); break;
                        case ConsoleKey.DownArrow:
                            index++; break;
                        default:
                            index = ptrs.Count; break;
                    }
                }
                Console.Clear();
                Console.WriteLine("Done");
            }
            else
            {
                if (!TryEvaluate((string)args[0], out long address))
                    return;

                ColorBufferUtil.ViewFrameBuffer((int)address);
                Console.Clear();
                Console.WriteLine("Done");
            }
        }


        [SpectrumCommand(
            Name = "viewf",
            Cat = SpectrumCommand.Category.Framebuffer,
            Description = "View data as ia8 texture")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.LITERAL, Tokens.EXPRESSION_S, Tokens.S16, Tokens.S16 },
            Help = "{0} = address to view")]
        private static void ViewIA8Buffer(Arguments args)
        {
            string inFormat = (string)args[0];
            if (!TryEvaluate((string)args[1], out long address))
                return;

            if (!Enum.TryParse(inFormat, true, out TextureFormat format))
                return;

            if (!ColorBufferUtil.IsSupported(format))
            {
                Console.WriteLine($"{inFormat} not supported");
                return;
            }

            ColorBufferRequest request = new ColorBufferRequest()
            {
                Format = format,
                PixelPtr = address,
                Width = (short)args[2],
                Height = (short)args[3],
                Scale = 3
            };
            ColorBufferUtil.ViewColorBuffer(request);
            Console.Clear();
            Console.WriteLine("Done");
        }


        [SpectrumCommand(
            Name = "viewram",
            Cat = SpectrumCommand.Category.Framebuffer,
            Description = "View rdram as a RBG5A1 320x240 texture in console window live")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { },
            Help = "When command is running, press down to advance to the next framebuffer")]
        private static void ViewRdram(Arguments args)
        {
            for (int address = 0; address < GetRamSize(); address += 0x25800)
            {
                ColorBufferUtil.ViewFrameBuffer(address);
                Console.Clear();
            }
            Console.WriteLine("Done");
        }




        [SpectrumCommand(
            Name = "framepng",
            Cat = SpectrumCommand.Category.Framebuffer,
            Description = "Dumps framebuffer to .png")]
        private static void GetFrameBufferPng(Arguments args)
        {
            throw new NotImplementedException();

            foreach (var ptr in Constants.GetFramebufferPointers(Options.Version))
            {
                ColorBufferUtil.ViewFrameBuffer(ptr);
            }
        }
        #endregion

        #region Items
        [SpectrumCommand(
            Name = "items",
            Cat = SpectrumCommand.Category.Item,
            Description = "Lists all items")]
        [SpectrumCommandSignature(Sig = new Tokens[] { },
            Help = "")]
        private static void ListItems(Arguments args)
        {
            if (Options.Version.Game == Game.OcarinaOfTime)
            {
                var names = Enum.GetNames(typeof(OItems.Item));
                var values = Enum.GetValues(typeof(OItems.Item));

                Console.Clear();

                for (int i = 0; i < names.Length; i++)
                {
                    int value = (int)values.GetValue(i);
                    if (value >= 255)
                    {
                        return;
                    }

                    Console.WriteLine($"{names[i],-13} = {value:X2}");
                }
            }
            else
            {
                foreach (var name in Enum.GetNames(typeof(MItems.Item)))
                {
                    Console.WriteLine($"{name}");
                }
            }
        }

        [SpectrumCommand(
            Name = "have",
            Cat = SpectrumCommand.Category.Item,
            Description = "Gives Item")]
        [SpectrumCommandSignature(Sig = new Tokens[] { Tokens.STRING },
            Help = "{0} = Item to give")]
        private static void GiveItem(Arguments args)
        {
            string arg = (string)args[0];
            if (Options.Version.Game == Game.OcarinaOfTime)
                GiveOItem(arg);
            else
                GiveMItem(arg);

        }

        private static void GiveMItem(string arg)
        {
            if (!Enum.TryParse<MItems.Item>(arg, true, out var item))
            {
                return;
            }

            if (item >= MItems.Item.None)
            {
                return;
            }
            MItems.SetInventoryItem(Options.Version, item, SaveContext);
        }

        private static void GiveOItem(string arg)
        {
            if (!Enum.TryParse<OItems.Item>(arg, true, out var item))
            {
                return;
            }

            if (item >= OItems.Item.None)
            {
                return;
            }

            if (item <= OItems.Item.ClaimCheck)
            {
                OItems.SetInventoryItem(item, SaveContext);
            }
            else if (item <= OItems.Item.HoverBoots)
            {
                short equip = SaveContext.ReadInt16(0x9C);
                OItems.SetEquipment(item, true, ref equip);
                SaveContext.Write(0x9C, equip);
            }
            else if (item <= OItems.Item.StoneOfAgony)
            {
                int quest = SaveContext.ReadInt32(0xA4);
                OItems.SetQuestItem(item, true, ref quest);
                SaveContext.Write(0xA4, quest);
            }
        }
        #endregion


        [SpectrumCommand(
            Name = "txt",
            Description = "Displays data at address as a null terminated string")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.EXPRESSION_S })]
        private static void DisplayText(Arguments args)
        {
            if (!TryEvaluate((string)args[0], out long address))
                return;

            var data = Zpr.ReadRam(address, 0x100);
            string result = CStr.Get(data, "EUC-JP");
            Console.WriteLine(result);
        }


        [SpectrumCommand(
            Name = "reg",
            Description = "Get Static Context REG information")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.HEX_S32 },
            Help = "{0} = Offset relative to the start of the Static Context")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.LITERAL },
            Help = "{0} = case sensitive REG() macro, as defined in the Memory Editor;" +
            "e.g. reg REG(68)")]
        private static void GetStaticContextReg(Arguments args)
        {
            if (args[0] is int off)
            {
                Console.WriteLine(StaticCtx.GetRegFromOffset(off));
            }
            else
            {
                string regStr = (string)args[0];
                if (!StaticCtx.TryGetOffsetFromReg(regStr, out off))
                {
                    Console.WriteLine("Invalid Reg");
                    return;
                }
                if (MemoryMapper.TryGetStaticContext(out IRamItem staticCtx))
                {
                    N64Ptr ctxStart = staticCtx.Ram.Start;
                    Console.WriteLine($"{ctxStart + off:X8}:");
                    Console.WriteLine($"Start {ctxStart} Off: {off:X6}");
                }
                else
                {
                    Console.WriteLine($"Start UNK Off: {off:X4}");
                }
            }
        }

        [SpectrumCommand(
            Name = "setreg",
            Description = "Sets a Static Context REG's value by its macro")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.LITERAL, Tokens.HEX_S16 },
            Help =
            "{0} = case sensitive REG() macro, as defined in the Memory Editor;" +
            "{1} = hexadecimal value to assign")]
        private static void SetStaticContextReg(Arguments args)
        {
            string regStr = (string)args[0];
            short val = (short)args[1];
            SetStaticContextReg(regStr, val);
        }

        [SpectrumCommand(
            Name = "setregi",
            Description = "Sets a Static Context REG's value by its macro")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.LITERAL, Tokens.S16 },
            Help =
            "{0} = case sensitive REG() macro, as defined in the Memory Editor;" +
            "{1} = signed 16 bit integer value to assign")]
        private static void SetStaticContextRegi(Arguments args)
        {
            string regStr = (string)args[0];
            short val = (short)args[1];
            SetStaticContextReg(regStr, val);
        }

        private static void SetStaticContextReg(string regStr, short val)
        {
            if (!StaticCtx.TryGetOffsetFromReg(regStr, out int off))
            {
                Console.WriteLine("Invalid Reg");
                return;
            }
            if (MemoryMapper.TryGetStaticContext(out IRamItem staticCtx))
            {
                N64Ptr writeAddr = staticCtx.Ram.Start + off;
                WriteRam(writeAddr, val);
            }
        }

        [SpectrumCommand(
            Name = "rsp",
            Cat = SpectrumCommand.Category.Proto,
            Description = "RSP disassembler")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.EXPRESSION_S })]
        private static void PrintRspAsm(Arguments args)
        {
            if (!TryEvaluate((string)args[0], out long address))
                return;

            address &= -4;
            Console.Clear();

            for (int i = 0; i < 25 * 4; i += 4)
            {
                var op = Zpr.ReadRamInt32((int)(address + i));
                //var decodeOp = Atom.RSP.decoder_c.rsp_decode_instruction((uint)op);
                var decodedOp = "PrintRspAsm not supported"; //decodeOp.parse((uint)op);
                Console.WriteLine($"{address + i:X8}: {op:X8} {decodedOp}");
            }
        }



        [SpectrumCommand(
            Name = "jtxt",
            Cat = SpectrumCommand.Category.Proto,
            Description = "Dumps logged data to json",
            Sup = SpectrumCommand.Supported.OoT)]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.LITERAL })]
        private static void DumpJsonToText(Arguments args)
        {
            switch ((string)args[0])
            {
                case "col": DumpCollisionData(); break;
                case "dlist": DumpReferenceLogger(DisplayListLogger, "dump/dlist.txt"); break;
            }
        }

        [SpectrumCommand(
            Name = "module",
            Cat = SpectrumCommand.Category.Proto,
            Description = "test function")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.STRING })]
        private static void GetModule(Arguments args)
        {
            if (!int.TryParse((string)args[0], NumberStyles.HexNumber, new CultureInfo("en-US"), out int addr))
                return;

            Zpr.GetContainingModule(new IntPtr(addr));
        }

        [SpectrumCommand(
            Name = "status",
            Cat = SpectrumCommand.Category.Proto,
            Description = "Incomplete, calculate cop0 status register info")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.U32 },
            Help = "{0} = Status Register Value")]
        private static void StatusRegister(Arguments args)
        {
            uint status = (uint)args[0];

            //Determines COP usability
            //COP0 is always usable when running in Kernel Mode or Debug Mode
            bool[] CU = new bool[4];

            CU[0] = Shift.AsBool(status, 0x10000000);
            CU[1] = Shift.AsBool(status, 0x20000000);
            CU[2] = Shift.AsBool(status, 0x40000000);
            CU[3] = Shift.AsBool(status, 0x80000000);

            //Enables reduced power mode on some implementations (maybe not N64?)
            bool RP = Shift.AsBool(status, 0x08000000);
            //Controls floating point register mode on MIPS64 processors. Readonly
            bool FR = Shift.AsBool(status, 0x04000000);
            //Enables Reverse-endian memory references while in user mode
            bool RE = Shift.AsBool(status, 0x02000000);

            //Interrupt Mask. Controls enabling of each of the external, internal, software intterupts
            bool[] IM = new bool[8];

            IM[0] = Shift.AsBool(status, 0x00000100);
            IM[1] = Shift.AsBool(status, 0x00000200);
            IM[2] = Shift.AsBool(status, 0x00000400);
            IM[3] = Shift.AsBool(status, 0x00000800);
            IM[4] = Shift.AsBool(status, 0x00001000);
            IM[5] = Shift.AsBool(status, 0x00002000);
            IM[6] = Shift.AsBool(status, 0x00004000);
            IM[7] = Shift.AsBool(status, 0x00008000);

            //Interrupt Enable. Acts as master enable for software and hardware intterupts
            bool IE = Shift.AsBool(status, 0x00000001);

        }

        [SpectrumCommand(
            Name = "fstatus",
            Cat = SpectrumCommand.Category.Proto,
            Description = "Incomplete, calculate cop1 status register info")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.S32 },
            Help = "{0} = Status Register Value")]
        private static void FloatingPointStatusRegister(Arguments args)
        {
            //int status = (int)args[0];

            //int roundingMode = status & 3;
            //string roundingStr;

            //switch (roundingMode)
            //{
            //    case 0: roundingStr = "Round to Nearest"; break;
            //    case 1: roundingStr = "Round to Zero"; break;
            //    case 2: roundingStr = "Round to Infinity"; break;
            //    default: roundingStr = "Round to Minus Infinity"; break;
            //}
        }

        static List<int> Stored = new List<int>();
        static long StoredAddress = 0;
        
        [SpectrumCommand(
            Name = "dh",
            Cat = SpectrumCommand.Category.Proto,
            Description = "Unstable, halts game execution to allow for piece-wise display list viewing")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { })]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.EXPRESSION_S },
            Help = "{0} = I forget")]
        private static void DlistHalt(Arguments args)
        {
            if (args.Length == 0)
            {
                if (StoredAddress == 0)
                    return;
                //restore the microcode sequence
                for (int i = 0; i < Stored.Count; i++)
                {
                    Zpr.WriteRam32((int)StoredAddress + (i * 4), Stored[i]);
                }
                StoredAddress = 0;
                return;
            }
            
            if (!TryEvaluate((string)args[0], out long address))
                return;

            //save sequence
            Stored = new List<int>();
            for (int i = 0; i < 6; i++)
            {
                Stored.Add(Zpr.ReadRamInt32((int)address + (i * 4)));
            }
            Ptr writeAddress = SPtr.New((int)address);
            writeAddress.Write(
                0x00, 0xE7000000, 0x04, 0x00000000,
                0x08, 0xE9000000, 0x0C, 0x00000000,
                0x10, 0xDF000000, 0x14, 0x00000000);

            StoredAddress = address;
        }

        [SpectrumCommand(
            Name = "mips",
            Cat = SpectrumCommand.Category.Proto,
            Description = "Unstable, prints mips assembly")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.EXPRESSION_S },
            Help = "{0} = Address")]
        private static void PrintMipsAsm(Arguments args)
        {
            if (!TryEvaluate((string)args[0], out long address))
                return;

            address &= -4;
            Console.Clear();

            for (int i = 0; i < 25 * 4; i += 4)
            {
                var op = Zpr.ReadRamInt32((int)(address + i));
                var decodedOp = "PrintMipsAsm not supported";//Atom.Atom.GetOP(op);
                Console.WriteLine($"{address + i:X8}: {op:X8} {decodedOp}");
            }
        }

        [SpectrumCommand(
            Name = "tsv",
            Cat = SpectrumCommand.Category.Proto,
            Description = "Generates a tab separated values table on the clipboard.")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.EXPRESSION_S, Tokens.EXPRESSION_S, Tokens.EXPRESSION_S },
            Help = 
            "{0} = address;" +
            "{1} = record size" +
            "{2} = count")]
        private static void GenerateTSVFromRam(Arguments args)
        {
            if (!TryEvaluate((string)args[0], out long address))
                return;

            if (!TryEvaluate((string)args[1], out long recsize))
                return;

            if (!TryEvaluate((string)args[1], out long count))
                return;

            if (count * recsize > 0x6000)
            {
                Console.WriteLine($"Table is 0x{count * recsize:X8} bytes, continue?");
                Console.Write("Type 'yes' to confirm: ");
                string line = Console.ReadLine().Trim().ToLowerInvariant();
                if (line != "yes")
                    return;
            }
        }


        [SpectrumCommand(
            Name = "gctx",
            Cat = SpectrumCommand.Category.Proto,
            Description = "Sets Global Context Pointer")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.EXPRESSION_S })]
        private static void SetGlobalContext(Arguments args)
        {
            if (!TryEvaluate((string)args[0], out long address))
                return;

            SpectrumVariables.GlobalContext = SPtr.New(address);
            ChangeVersion((Options, false));
        }

        [SpectrumCommand(
            Name = "dbgr",
            Cat = SpectrumCommand.Category.Proto,
            Description = "Skips crash debugger input code (update dynarec)")]
        [SpectrumCommandSignature()]
        private static void SetNoInputDebugger(Arguments args)
        {
            if (Options.Version == ORom.Build.N0)
            {
                SPtr.New(0xAEA5C).Write(0x00, (ushort)0x5400);
            }
            else
            {
                Console.WriteLine("Version not supported");
            }
        }


        [SpectrumCommand(
            Name = "plane",
            Cat = SpectrumCommand.Category.Proto,
            Description = "Computes if a point falls within a plane")]
        [SpectrumCommandSignature(
            Sig = new Tokens[] { Tokens.HEX_S16, Tokens.HEX_S16, Tokens.HEX_S16, Tokens.S16, Tokens.COORDS_FLOAT })]
        private static void ComputePolyDist(Arguments args)
        {
            short normX = (short)args[0];
            short normY = (short)args[1];
            short normZ = (short)args[2];

            short dist = (short)args[3];
            const float factor = 1f / 32767;

            Console.Clear();

            Vector3<float> point = (Vector3<float>)args[4];
            float rA = (point.x * normX) + (point.y * normY) + (point.z * normZ);
            var bytes = BitConverter.GetBytes(rA);
            int rA_i = BitConverter.ToInt32(bytes,0);
            Console.WriteLine($"(point.x * normX) + (point.y * normY) + (point.z * normZ) = {rA} -> {rA_i:X8}");
            float rB = rA * factor;
            bytes = BitConverter.GetBytes(rB);
            int rB_i = BitConverter.ToInt32(bytes, 0);

            Console.WriteLine($"{rA} * 1f/32767 = {rB} -> {rB_i:X8}");

            float rC = rB + dist;
            bytes = BitConverter.GetBytes(rB);
            int rC_i = BitConverter.ToInt32(bytes, 0);
            Console.WriteLine($"result {rC} -> {rC_i:X8}");

            DetectRoundingError();
        }

        private static void DetectRoundingError()
        {
            short normX = 0;
            short normY = 0x7FFF;
            short normZ = 0;
            const float factor = 1f / 32767;

            for (short dist = 0; dist < 8000; dist++)
            {
                Vector3<float> point = new Vector3<float>(0, dist, 0);
                float rA = (point.x * normX) + (point.y * normY) + (point.z * normZ);
                var bytes = BitConverter.GetBytes(rA);
                int rA_i = BitConverter.ToInt32(bytes, 0);
                //Console.WriteLine($"(point.x * normX) + (point.y * normY) + (point.z * normZ) = {rA} -> {rA_i:X8}");
                float rB = rA * factor;
                //bytes = BitConverter.GetBytes(rB);
                int rB_i = BitConverter.ToInt32(bytes, 0);

                //Console.WriteLine($"{rA} * 1f/32767 = {rB} -> {rB_i:X8}");

                float rC = rB + -dist;
                bytes = BitConverter.GetBytes(rB);
                int rC_i = BitConverter.ToInt32(bytes, 0);
                //Console.WriteLine($"result {rC} -> {rC_i:X8}");

                if (rC != 0)
                {
                    Console.WriteLine($"{dist}\tA: {rA} -> {rA_i:X8} B: {rB} -> {rB_i:X8} F: {rC} -> {rC_i:X8}");
                }
            }
        }

        [SpectrumCommand(
            Name = "pause",
            Cat = SpectrumCommand.Category.Proto,
            Description = "Pauses game by setting game state's 'update' func pointer to return",
            Sup = SpectrumCommand.Supported.OoT)]
        [SpectrumCommandSignature()]
        private static void PauseGame(Arguments args)
        {
            if (Options.Version != ORom.Build.N0)
                return;

            if (GlobalContext == 0)
                return;

            int update = GlobalContext.ReadInt32(4);
            int deconstruct = GlobalContext.ReadInt32(8);
            if (update == EXECUTE_PTR)
            {
                //unpause
                if (deconstruct != FrameHaltVars.deconstructor)
                    return;
                GlobalContext.Write(4, FrameHaltVars.update);
                return;
            }
            else
            {
                //pause
                FrameHaltVars = new FrameHalt(update, deconstruct);
                GlobalContext.Write(4, EXECUTE_PTR);
                return;
            }

        }

        public static ModelViewerOoT ModelViewer;
        [SpectrumCommand(
            Name = "mpause",
            Cat = SpectrumCommand.Category.Proto,
            Description = "secret :)",
            Sup = SpectrumCommand.Supported.OoT)]
        [SpectrumCommandSignature()]
        private static void PauseGameModel(Arguments args)
        {
            if (Options.Version != ORom.Build.N0)
                return;

            Ptr staticContext = SPtr.New(0x11BA00).Deref();

            if (staticContext == 0)
                return;

            Ptr pauseFlag = staticContext.RelOff(0x15D4);

            if (pauseFlag.ReadInt32(0) == 0)
            {
                pauseFlag.Write(0, 1);
                System.Threading.CancellationToken token = new System.Threading.CancellationToken();
                //Action syncToEmu = 
                Task.Run(async () => 
                {
                    while (GlobalContext.ReadInt32(4) != EXECUTE_PTR)
                    {
                        await Task.Delay(200);
                    }
                    Console.WriteLine("Sync Complete");
                }, token);
                ModelViewer = new ModelViewerOoT(GetFrameDlists(Gfx));
                ModelControl();

            }

            pauseFlag.Write(0, 0);
            ModelViewer?.RestoreBranches();
            ModelViewer = null;
        }
    }
}
