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
    public partial class Atom
    {
        static void Main(string[] args)
        {
            PathUtil.Initialize();
            {
                var result = Parser.Default.ParseArguments(args, VerbTypes)
                    .WithParsed<PathOptions>(
                        a => UpdateRomPath(a))
                    .WithParsed<AllOptions>(
                        a => DisassembleAll(a))
                    .WithParsed<DisassembleFileOptions>(
                        a => DisassembleFile(a))
                    .WithParsed<DisassembleOverlayOptions>(
                        a => DisassembleOverlayFile(a))
                    .WithParsed<ScriptOptions>(
                        a => HandleInjectionScript(a))
                    .WithParsed<OvlOptions>(
                        a => ElfToOverlay(a))
                    .WithParsed<VersionOptions>(
                        a => ListSupportedVersions());
            }
        }

        private static void DisassembleFile(DisassembleFileOptions a)
        {
            throw new NotImplementedException();
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
            string path;
            if (!RomVersion.TryGet(opts.GameId, opts.VersionId, out RomVersion version))
            {
                Console.WriteLine($"Unrecognized version {opts.GameId} {opts.VersionId}");
                return -1;
            }
            if (opts.RomPath != null )
            {
                path = opts.RomPath;
            }
            else if (!PathUtil.TryGetRomLocation(version, out path))
            {
                Console.WriteLine($"Cannot find path for {opts.GameId} {opts.VersionId}");
                return -1;
            }
            Disassemble.GccOutput = opts.ReadableOutput;
            Disassemble.PrintRelocations = true;
            Console.WriteLine($"{version} {path}");
            DisassembleRom(version, path);
            return 0;
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

            using (StreamWriter sw = new StreamWriter("__code.txt"))
            {
                using (BinaryReader br = new BinaryReader(rom.Files.GetFile(task.VRom)))
                {
                    Disassemble.FirstParse(br, task);
                    Disassemble.Task(sw, br, task);
                }
            }
        }

        static void DisassembleOverlayFile(DisassembleOverlayOptions a)
        {
            if (!RomVersion.TryGet(a.GameId, a.VersionId, out RomVersion ver))
            {
                Console.WriteLine($"Unrecognized version {a.GameId} {a.VersionId}");
                return;
            }
            string path = a.RomPath;
            if (path == null)
            {
                PathUtil.TryGetRomLocation(ver, out path);
            }
            
            Rom rom = Rom.New(path, ver);
            var tasks = DisassemblyTask.CreateTaskList(rom);
            Disassemble.PrintRelocations = true;
            Disassemble.GccOutput = true;

            var task = tasks.SingleOrDefault(x => x.Name == a.File || x.Name == $"ovl_{a.File}");
            if (task == null)
            {
                Console.WriteLine("Cannot find overlay");
                return;
            }
            //var taskbss = tasks.Where(x => x.Sections.Values.SingleOrDefault(y => y.Name == "bss")?.Size > 0).ToList();

            //using (var reader = new BinaryReader(rom.Files.GetFile(task.VRom)))
            //{
            //    using (StreamWriter sw = new StreamWriter("__test.txt"))
            //    {
            //        foreach (var rel in task.Relocations.Where(x => x.SectionId == Overlay.Section.text))
            //        {
            //            reader.BaseStream.Position = rel.Offset;
            //            sw.WriteLine($"{rel.Offset:X6}: {rel.RelocType}  {Disassemble.GetOP(reader.ReadBigInt32())}");
            //        }
            //    }
            //}

            using (StreamWriter sw = new StreamWriter($"__{a.File}.txt"))
            {
                using (BinaryReader br = new BinaryReader(rom.Files.GetFile(task.VRom)))
                {
                    Disassemble.FirstParse(br, task);
                    Disassemble.Task(sw, br, task);
                }
            }
            
            using (StreamWriter sw = new StreamWriter($"__{a.File}_f.txt"))
            {
                foreach (var item in Disassemble.Symbols.OrderBy(x => x.Key))
                {
                    sw.WriteLine($"{item.Value.ToString()} = 0x{item.Key}");
                }
            }
        }

        static void DisassembleRom(RomVersion ver, string path)
        {
            Console.Write("Initializing task list:  ");
            Rom rom = Rom.New(path, ver);

            List<DisassemblyTask> taskList = DisassemblyTask.CreateTaskList(rom);
            taskList = taskList.Where(x => x.VRom.End > 0).ToList();
            Console.WriteLine("DONE!");
            Console.Write($"Building symbol table: ");
            Stream getFile(FileAddress x) => rom.Files.GetFile(x);

            LoadFunctionDatabase(ver);
            GetSymbols(taskList, getFile);

            Console.WriteLine("DONE!");
            Console.WriteLine("Disassembling files: ");

            DisassembleTasks(rom.Version, taskList, getFile);
            DumpFoundFunctions(rom.Version, Disassemble.GetFunctions());
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

        private static void GetSymbols(List<DisassemblyTask> tasks, Func<FileAddress, Stream> getFile)
        {
            foreach (var task in tasks)
            {
                foreach (var item in task.Functions)
                    Disassemble.AddFunction(item);

                using (BinaryReader br = new BinaryReader(getFile(task.VRom)))
                {
                    //Get a list of function names
                    Disassemble.FirstParse(br, task);
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

                using (var sw = File.CreateText(filename))
                {
                    using (var br = new BinaryReader(getFile(task.VRom)))
                    {
                        Disassemble.Task(sw, br, task);
                    }
                }
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
            using (StreamWriter outputf = new StreamWriter(new FileStream($"{GetFolder(Version)}/~~func.txt", FileMode.Create, FileAccess.Write)))
            {
                foreach (var func in funcList.OrderBy(x => x.Addr))
                {
                    outputf.WriteLine($"{func.Addr:X8},{func.Name}");
                }
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
