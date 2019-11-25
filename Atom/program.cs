using mzxrules.OcaLib;
using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using mzxrules.OcaLib.Elf;
using PathUtil = mzxrules.OcaLib.PathUtil.PathUtil;
using CommandLine;

namespace Atom
{
    // prototype: 
    // adis o stalfo.s 2 ZELOOTMA.z64 
    //80A44CD0 80A46710
    //int NAMETABLE_ADD = 0xA771A0;

    //generation special
    //var_8084C51B to var_80854514 - 0x7FF9
    //808391F4 //hi
    //808391FC //lo
    public partial class Atom
    {
        static void Main(string[] args)
        {
            PathUtil.Initialize();

            var result = Parser.Default.ParseArguments(args, VerbTypes)
                .WithParsed<PathOptions>(
                    a => UpdateRomPath(a))
                .WithParsed<AllOptions>(
                    a => DisassembleAll(a))
                .WithParsed<DisassembleFileOptions>(
                    a => DisassembleFile(a))
                .WithParsed<ScriptOptions>(
                    a => HandleInjectionScript(a))
                .WithParsed<OvlOptions>(
                    a => ElfToOverlay(a))
                .WithParsed<VersionOptions>(
                    a => ListSupportedVersions());
        }

        private static void HandleInjectionScript(ScriptOptions a)
        {
            if (a.GenerateNewScript)
            {
                if (File.Exists(a.ScriptPath))
                {
                    Console.WriteLine($"A file already exists at the given location, no script created!");
                    return;
                }
                ElfUtil.CreateDummyScript(a.ScriptPath);
            }
            else
            {
                ElfUtil.ProcessInjectScript(a.ScriptPath);
            }
        }

        private static void ListSupportedVersions()
        {
            ORom.ConsolePrintSupportedVersions();
            MRom.ConsolePrintSupportedVersions();
        }

        private static int UpdateRomPath(PathOptions a)
        {
            if (!RomVersion.TryGet(a.GameId, a.VersionId, out RomVersion ver))
            {
                Console.WriteLine($"Unrecognized version {a.GameId} {a.VersionId}");
                return -1;
            }
            string path = a.RomPath;
            if (!File.Exists(path))
            {
                Console.WriteLine($"Cannot find file {path}");
                return -1;
            }
            PathUtil.SetRomLocation(ver, path);
            Console.WriteLine($"Path updated! Re-run program to continue");
            return 0;
        }

        private static int DisassembleAll(AllOptions opts)
        {
            return TryDisassemble(opts, (v, p) =>
            {
                return x => true;
            });
        }

        static void DisassembleFile(DisassembleFileOptions opts)
        {
            TryDisassemble(opts, (v, p) =>
            {
                return GetDisassembleOverlayFilter(opts, v, p);
            });
        }

        private static int TryDisassemble(DisassemblyOptions opts, Func<RomVersion, string, Func<DisassemblyTask, bool>> getFilter)
        {
            string path;
            if (!RomVersion.TryGet(opts.GameId, opts.VersionId, out RomVersion version))
            {
                Console.WriteLine($"Unrecognized version {opts.GameId} {opts.VersionId}");
                return -1;
            }
            if (opts.RomPath != null)
            {
                path = opts.RomPath;
            }
            else if (!PathUtil.TryGetRomLocation(version, out path))
            {
                Console.WriteLine($"Cannot find path for {opts.GameId} {opts.VersionId}");
                return -1;
            }

            Disassemble.GccOutput = !opts.ReadableOutput;
            Disassemble.PrintRelocations = true;
            Func<DisassemblyTask, bool> filter = getFilter(version, path);
            if (filter == null)
            {
                return -1;
            }
            DisassembleRom(version, path, filter);
            return 0;
        }

        static void DisassembleRom(RomVersion version, string path, Func<DisassemblyTask, bool> filter)
        {
            Console.WriteLine($"{version} {path}");
            Console.Write("Initializing task list:  ");
            Rom rom = Rom.New(path, version);
            List<DisassemblyTask> taskList = DisassemblyTask.CreateTaskList(rom);
            taskList = taskList.Where(filter).Where(x => x.VRom.End > 0).ToList();

            if (taskList.Count == 0)
            {
                Console.WriteLine("Error: No tasks to process!");
                return;
            }

            Console.WriteLine("DONE!");
            Console.Write($"Loading symbol table from file: ");
            LoadFunctionDatabase(version);

            Console.WriteLine("DONE!");
            Console.WriteLine("Disassembling files: ");

            Stream getFile(FileAddress x) => rom.Files.GetFile(x);
            DisassembleTasks(rom.Version, taskList, getFile);
            DumpFoundFunctions(rom.Version, Disassemble.GetFunctions());
        }


        private static Func<DisassemblyTask, bool> GetDisassembleOverlayFilter(DisassembleFileOptions opts, RomVersion version, string path)
        {
            Func<DisassemblyTask, bool> filter = x => false;

            if (opts.FileStart != null)
            {
                if (int.TryParse(opts.FileStart, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int fileStart))
                {
                    filter = x => x.VRom.Start == fileStart;
                }
                else
                {
                    Console.WriteLine($"invalid start address {opts.FileStart}");
                    return null;
                }
            }
            else if (opts.File != null)
            {
                filter = x => x.Name == opts.File || x.Name == $"ovl_{opts.File}";
            }
            else
            {
                Rom rom = Rom.New(path, version);
                OverlayRecord dlf_record;

                if (!int.TryParse(opts.ActorIndex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int actorIndex))
                {
                    Console.WriteLine($"actor index {actorIndex} is invalid");
                    return null;
                }

                if (actorIndex == 0)
                {
                    dlf_record = rom.Files.GetPlayPauseOverlayRecord(1);
                }
                else
                {
                    dlf_record = rom.Files.GetActorOverlayRecord(actorIndex);
                    if (dlf_record.VRom.Size == 0)
                    {
                        Console.WriteLine($"actor 0x{opts.ActorIndex:X4} does not have an overlay file");
                        return null;
                    }
                }
                filter = x => x.VRom.Start == dlf_record.VRom.Start;
            }

            return filter;
        }


        private static void ElfToOverlay(OvlOptions a)
        {
            string file = a.Path;
            if (!int.TryParse(a.VRam, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int addr))
            {
                Console.WriteLine("Cannot parse address given");
                return;
            }
            if (!File.Exists(file))
            {
                Console.WriteLine($"Cannot find file {Path.GetFullPath(file)}");
                return;
            }

            if (ElfUtil.TryConvertToOverlay(file, $"{file}.z64", addr))
            {
                Console.WriteLine("Conversion success");
            }
            else
            {
                Console.WriteLine("A dumb error occurred");
                Console.ReadKey();
            }
        }


        static void MMDebugTest()
        {
            RomVersion ver = MRom.Build.DBG;
            PathUtil.TryGetRomLocation(ver, out string path);
            Rom rom = new MRom(path, ver);
            DisassemblyTask task = null;
            Disassemble.PrintRelocations = true;

            using StreamWriter sw = File.CreateText("__code.txt");
            using BinaryReader br = new BinaryReader(rom.Files.GetFile(task.VRom));
            Disassemble.FirstParse(br, task);
            Disassemble.Task(sw, br, task);
        }


        private static void TestSymbolGeneration(RomVersion ver)
        {
            Dictionary<string, bool> __last_symbol = File
                .ReadAllLines($"__{ver.GetGameAbbr()}_{ver.GetVerAbbr()}_last_symbol.txt")
                .ToDictionary(v => v, v => true);
            List<string> __next_symbol = new List<string>();

            using (var sw = File.CreateText("__last_symbol.txt"))
            {
                foreach (var symbol in Disassemble.Symbols.Values.OrderBy(x => x.Addr))
                {
                    string symbolInfo = $"{symbol.Kind.ToString().ToLowerInvariant(),4}_{symbol.Addr} => {symbol.Confirmed,-5} {symbol.Name}";
                    if (__last_symbol.ContainsKey(symbolInfo))
                    {
                        __last_symbol.Remove(symbolInfo);
                    }
                    else
                    {
                        __next_symbol.Add(symbolInfo);
                    }
                    sw.WriteLine(symbolInfo);
                }
            }
            SortedList<string, string> errors = new SortedList<string, string>();
            foreach(var sym in __last_symbol.Keys)
            {
                errors.Add(sym, "o");
            }
            foreach(var sym in __next_symbol)
            {
                errors.Add(sym, "n");
            }

            using (var sw = File.CreateText("__last_symbol_compare.txt"))
            {
                foreach (var item in errors)
                {
                    sw.WriteLine($"{item.Value} {item.Key}");
                }
            }
        }

        private static void DisassembleTasks(RomVersion ver, List<DisassemblyTask> taskList, Func<FileAddress, Stream> getFile)
        {
            var folder = GetFolder(ver);
            Console.CursorVisible = false;
            foreach (var task in taskList)
            {
                string filename = $"{folder}/{task.Name}.txt";
                Console.Write($"\r{filename.PadRight(Console.BufferWidth)}");


                var file = getFile(task.VRom);

                foreach (var action in task.PreparseActions)
                {
                    action(file);
                }

                foreach (var item in task.Functions)
                    Disassemble.AddFunction(item);

                using var br = new BinaryReader(file);
                Disassemble.FirstParse(br, task);

                using var sw = File.CreateText(filename);
                Disassemble.Task(sw, br, task);
            }
            Console.CursorVisible = true;
        }

        private static void LoadFunctionDatabase(RomVersion version)
        {
            var funcInfo = JQuery.GetFunctionInfo(version);
            Disassemble.AddFunctions(funcInfo);
        }
        
        private static void DumpFoundFunctions(RomVersion Version, IEnumerable<Label> funcList)
        {
            using var outputf = File.CreateText($"{GetFolder(Version)}/~~func.txt");
            foreach (var func in funcList.OrderBy(x => x.Addr))
            {
                outputf.WriteLine($"{func.Addr:X8},{func.Name}");
            }
        }

        private static string GetFolder(RomVersion Version)
        {
            string game = Version.Game.ToString()[0].ToString();
            var dir = $"{game}/{Version.ToString().ToUpper()}";
            if (!Disassemble.GccOutput)
            {
                dir += "_r";
            }
            Directory.CreateDirectory(dir);
            return dir;
        }
    }
}
