using mzxrules.OcaLib;
using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using mzxrules.OcaLib.Elf;
using PathUtil = mzxrules.OcaLib.PathUtil.PathUtil;

namespace Atom
{
    public partial class Atom
    {
        /* prototype:
         adis o stalfo.s 2 ZELOOTMA.z64
        */

        //80A44CD0 80A46710

        //int NAMETABLE_ADD = 0xA771A0;
        

        static void Main(string[] a)
        {
            PathUtil.Initialize();
            
            if (a.Length == 0)
            {
                Console.WriteLine("Help:");
                Console.WriteLine("path [gameid] [versionid] [path] //sets a path to a rom");
                Console.WriteLine("all [gameid] [versionid] //creates a gcc compatible disassembly of the entire rom");
                Console.WriteLine("all_r [gameid] [versionid] //creates a more readable disassembly of the entire rom");
                Console.WriteLine("df [gameid] [versionid] [name] //creates a disassembly of a specific overlay file");
                //Console.WriteLine("daf [gameid] [versionid] [id] //creates a disassembly of a specific actor file, using the actor");
                Console.WriteLine("script [path] //converts elf (.o) files to overlay form, and injects into the rom");
                Console.WriteLine("newscript //creates a file called dummy.xml for easy templating");
                Console.WriteLine("ovl [vram] [path] //creates an overlay file named [path].ovl from an elf (.o) file");
                Console.WriteLine("dcust [pc] [path]");
                Console.WriteLine();
                Console.WriteLine("Enter a command:");
                var read = Console.ReadLine();
                a = ParseArguments(read);
                if (a.Length == 0)
                {
                    Console.WriteLine("Terminated!");
                    return;
                }
            }
            if (a[0] == "path" && (a.Length == 3 || a.Length == 4))
            {
                string path = "";
                if (!RomVersion.TryGet(a[1], a[2], out RomVersion ver))
                {
                    Console.WriteLine($"Unrecognized version {a[1]} {a[2]}");
                    return;
                }
                if (a.Length == 4)
                {
                    path = a[3];
                }
                else
                {
                    Console.Write("Enter a path: ");
                    path = Console.ReadLine();
                }
                if (!File.Exists(path))
                {
                    Console.WriteLine($"Cannot find file {path}");
                    return;
                }
                PathUtil.SetRomLocation(ver, path);
                Console.WriteLine($"Path updated! Re-run program to continue");
            }
            else if (a.Length == 3 && a[0] == "all")
            {
                if (!RomVersion.TryGet(a[1], a[2], out RomVersion version))
                {
                    Console.WriteLine($"Unrecognized version {a[1]} {a[2]}");
                    return;
                }
                if (PathUtil.TryGetRomLocation(version, out string path))
                {
                    Disassemble.GccOutput = true;
                    Disassemble.PrintRelocations = true;
                    DisassembleRom(version, path);
                }
            }
            else if (a.Length == 3 && a[0] == "all_r")
            {
                if (!RomVersion.TryGet(a[1], a[2], out RomVersion version))
                {
                    Console.WriteLine($"Unrecognized version {a[1]} {a[2]}");
                    return;
                }
                if (PathUtil.TryGetRomLocation(version, out string path))
                {
                    Disassemble.GccOutput = false;
                    Disassemble.PrintRelocations = true;
                    DisassembleRom(version, path);
                }
            }
            else if (a.Length == 3 && a[0] == "ovl")
            {
                if (!int.TryParse(a[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int addr))
                {
                    Console.WriteLine("Cannot parse address given");
                    return;
                }
                ElfToOverlay(a[2], addr);
            }
            else if (a.Length == 2 && a[0] == "script")
            {
                ElfUtil.ProcessInjectScript(a[1]);
            }
            else if (a[0] == "newscript")
            {
                ElfUtil.CreateDummyScript("dummy.xml");
            }
            else if (a[0] == "df" && a.Length == 4)
            {
                if (!RomVersion.TryGet(a[1], a[2], out RomVersion version))
                {
                    Console.WriteLine($"Unrecognized version {a[1]} {a[2]}");
                    return;
                }
                OverlayTest(version, a[3]); //ovl_Boss_Fd // "ovl_Fishing"; 
            }
            else if (a[0] == "elftest")
            {
                ElfToOverlayTest();
            }
        }

        static string[] ParseArguments(string commandLine)
        {
            char[] parmChars = commandLine.ToCharArray();
            bool inQuote = false;
            for (int index = 0; index < parmChars.Length; index++)
            {
                if (parmChars[index] == '"')
                {
                    inQuote = !inQuote;
                    parmChars[index] = '\n';
                }
                if (!inQuote && parmChars[index] == ' ')
                    parmChars[index] = '\n';
            }
            return (new string(parmChars)).Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static void ElfToOverlay(string file, N64Ptr addr)
        {
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

        private static void ElfToOverlayTest()
        {
            string file = "basm.o";
            N64Ptr addr = 0x80948F60;

            ElfToOverlay(file, addr);
        }

        static void MMDebugTest()
        {
            RomVersion ver = MRom.Build.DBG;
            PathUtil.TryGetRomLocation(ver, out string path);
            Rom rom = new MRom(path, ver);
            DisassemblyTask task = null;
            Disassemble.PrintRelocations = true;

            var reader = new BinaryReader(rom.Files.GetFile(task.VRom));

            using (StreamWriter sw = new StreamWriter("__code.txt"))
            {
                BinaryReader br = new BinaryReader(rom.Files.GetFile(task.VRom));
                Disassemble.FirstParse(br, task);
                Disassemble.Task(sw, br, task);
            }
        }

        static void OverlayTest(RomVersion ver, string testOvl)
        {
            PathUtil.TryGetRomLocation(ver, out string path);
            Rom rom = Rom.New(path, ver);
            var tasks = DisassemblyTask.CreateTaskList(rom);
            Disassemble.PrintRelocations = true;
            Disassemble.GccOutput = true;

            var task = tasks.SingleOrDefault(x => x.Name == testOvl || x.Name == $"ovl_{testOvl}");
            if (task == null)
            {
                Console.WriteLine("Cannot find overlay");
                return;
            }
            //var taskbss = tasks.Where(x => x.Sections["bss"]?.Size > 0).ToList();
            
            var reader = new BinaryReader(rom.Files.GetFile(task.VRom));
            //using (StreamWriter sw = new StreamWriter("__test.txt"))
            //{
            //    foreach (var rel in task.Map.Relocations.Where(x => x.SectionId == Overlay.RelocationWord.Section.text))
            //    {
            //        reader.BaseStream.Position = rel.Offset;
            //        sw.WriteLine($"{rel.Offset:X6}: {rel.RelocType}  {GetOP(reader.ReadBigInt32())}");
            //    }
            //}

            using (StreamWriter sw = new StreamWriter($"__{testOvl}.txt"))
            {
                BinaryReader br = new BinaryReader(rom.Files.GetFile(task.VRom));
                Disassemble.FirstParse(br, task);
                if (Disassemble.GccOutput)
                {
                    sw.WriteLine("#include <mips.h>");
                    sw.WriteLine(".set noreorder");
                    sw.WriteLine(".set noat");
                    sw.WriteLine();
                }
                Disassemble.Task(sw, br, task);
            }
            
            using (StreamWriter sw = new StreamWriter($"__{testOvl}_f.txt"))
            {
                foreach (var item in Disassemble.Symbols.OrderBy(x => x.Key))
                {
                    sw.WriteLine($"{item.Value.ToString()} = 0x{item.Key}");
                }
            }
        }

        static void DisassembleRom(RomVersion ver, string path)
        {
            Rom rom;

            Console.Write("Initializing task list:  ");
            if (ver.Game == Game.OcarinaOfTime)
            {
                rom = new ORom(path, ver);
            }
            else
            {
                rom = new MRom(path, ver);
            }
            
            List<DisassemblyTask> taskList = DisassemblyTask.CreateTaskList(rom);
            taskList = taskList.Where(x => x.VRom.End > 0).ToList();
            Console.WriteLine("DONE!");
            Console.Write($"Building symbol table: ");
            Stream getFile(FileAddress x) => rom.Files.GetFile(x);

            LoadFunctionDatabase(ver);
            GetSymbols(taskList, rom.Version, getFile);

            Console.WriteLine("DONE!");
            Console.WriteLine("Disassembling files: ");

            DisassembleTasks(rom.Version, taskList, getFile);
            DumpFoundFunctions(rom.Version, Disassemble.GetFunctions());
        }

        private static void GetSymbols(List<DisassemblyTask> tasks, RomVersion ver, Func<FileAddress, Stream> getFile)
        {
            foreach (var task in tasks)
            {
                BinaryReader FileReader = new BinaryReader(getFile(task.VRom));

                foreach (var item in task.Functions)
                    Disassemble.AddFunction(item);

                //Get a list of function names
                Disassemble.FirstParse(FileReader, task);
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

                using (StreamWriter sw = new StreamWriter(new FileStream(filename, FileMode.Create, FileAccess.Write)))
                {
                    var reader = new BinaryReader(getFile(task.VRom));
                    if (Disassemble.GccOutput)
                    {
                        sw.WriteLine("#include <mips.h>");
                        sw.WriteLine(".set noreorder");
                        sw.WriteLine(".set noat");
                        sw.WriteLine();
                    }
                    Disassemble.Task(sw, reader, task);
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
