using mzxrules.Helper;
using mzxrules.OcaLib;
using mzxrules.OcaLib.Actor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Atom
{
    public class DisassemblyTask
    {
        public class SectionList
        {
            public List<Section> Values = new List<Section>();
            public Section this[string name]
            {
                get
                {
                    return Values.Single(x => x.Name == name);
                }
                set
                {
                    Values.Add(value);
                }
            }
            public Section this[int i]
            {
                get
                {
                    return Values[i];
                }
            }

            public bool IsWithinCode(N64Ptr ptr)
            {
                foreach (var item in Values)
                {
                    if (item.IsWithin(ptr))
                        return item.IsCode;
                }
                return false;
            }
        }
        public string Name;
        public FileAddress VRam { get; protected set; }
        public FileAddress VRom { get; protected set; }

        public SectionList Sections = new SectionList();

        public List<Overlay.RelocationWord> Relocations = new List<Overlay.RelocationWord>();
        public List<Label> Functions = new List<Label>();
        
        public enum OvlType
        {
            Actor,
            Particle,
            PlayPause,
            Game,
        }

        static Dictionary<(string, RomVersion), (FileAddress vrom, FileAddress vram, int text_size)> FileInfo =
            new Dictionary<(string, RomVersion), (FileAddress, FileAddress, int)>
            {
                { ("boot", ORom.Build.N0),      ((0x1060, 0x007430),    (0x80000460, 0x80006830), (int)(0x800060B0 - 0x80000460)) },
                { ("boot", ORom.Build.DBGMQ),   ((0x1060, 0x012F70),    (0x80000460, 0x80012370), (int)(0x12F70 - 0x1060)) },
                { ("boot", MRom.Build.J0),      ((0x1060, 0x01C110),    (0x80080060, 0x8009B110), (int)(0x800979BC-0x80080060))},//800979BC
                { ("boot", MRom.Build.DBG),     ((0x1060, 0x024F60),    (0x80080060, 0x800A3F60), (int)(0x8009F4D0-0x80080060))},
                { ("code", ORom.Build.N0),      ((0xA87000, 0xB8AD30),  (0x800110A0, 0x80114DD0), (int)(0x800E30E0 + 0x2C - 0x800110A0)) },
                { ("code", ORom.Build.DBGMQ),   ((0xA94000, 0xBCEF30),  (0x8001CE60, 0x80157D90), (int)(0xBCEF30 - 0xA94000)) },
                { ("code", MRom.Build.J0),      ((0xB5F000, 0xC9B6F0),  (0x800A76A0, 0x801E3D90), (int)(0x801A5990-0x800A76A0))},
                { ("code", MRom.Build.DBG),     ((0xC95000, 0xE12600),  (0x800B6AC0, 0x802340C0), (int)(0x801F3070-0x800B6AC0))},
                { ("n64dd", ORom.Build.N0), ((0xB8AD30,0xB8AD30+0x12D10), (0x801C6E80,0x801D9B90),0x00012D10) } 
            };

        public override string ToString()
        {
            return Name;
        }

        public static DisassemblyTask New(string name, RomVersion version)
        {
            var task = new DisassemblyTask()
            {
                Name = name,
                //Map = new Overlay()
            };

            if (name == "boot" || name == "code" || name == "n64dd")
            {
                if (!FileInfo.ContainsKey((name, version)))
                    return null;

                var (vrom, vram, text_size) = FileInfo[(name, version)];
                task.VRom = vrom;
                task.VRam = vram;
                Section text = new Section("text", task.VRam.Start, task.VRom.Start, text_size, true);
                task.Sections[text.Name] = text;
            }

            return task;
        }

        private static DisassemblyTask New(List<JOcaBase.JDmaData> DmaData, Rom rom, int index,
            OverlayRecord ovlInfo, OvlType nameClass)
        {
            RomFile file; //what we're going to disassemble 
            BinaryReader FileReader;
            ActorInit actorInfo = new ActorInit();
            
            var overlay = new Overlay();
            var dmaRecord = DmaData.SingleOrDefault(x => x.VRomStart == ovlInfo.VRom.Start && ovlInfo.VRom.Start != 0);
            string name = (dmaRecord != null)? dmaRecord.Filename : $"{nameClass}_{index:X4}";

            if (ovlInfo.VRom.Size != 0)
            {
                file = rom.Files.GetFile(ovlInfo.VRom);
                FileReader = new BinaryReader(file);
                overlay = new Overlay(FileReader);
            }

            DisassemblyTask task = new DisassemblyTask()
            {
                Name = name,
                VRam = ovlInfo.VRam,
                VRom = ovlInfo.VRom,
                Relocations = overlay.Relocations
            };

            Section text = new Section("text", task.VRam.Start, task.VRom.Start, overlay.TextSize, true);
            Section data = new Section("data", text.VRam + text.Size, text.VRom + text.Size, overlay.DataSize);
            Section rodata = new Section("rodata", data.VRam + data.Size, data.VRom + data.Size, overlay.RodataSize);
            long off = ovlInfo.VRam.Start + ovlInfo.VRom.Size;
            Section bss = new Section("bss", off, 0, overlay.BssSize);

            task.Sections.Values.Add(text);
            task.Sections.Values.Add(data);
            task.Sections.Values.Add(rodata);
            task.Sections.Values.Add(bss);
            return task;
        }

        private static void GetActorSymbolNames(DisassemblyTask task, Rom rom, ActorOverlayRecord ovlRec)
        {
            ActorInit actorInfo = new ActorInit();

            if (ovlRec.VRamActorInfo == 0)
                return;

            N64Ptr startAddr;
            RomFile file;
            if (ovlRec.VRom.Size == 0)
            {
                file = rom.Files.GetFile(ORom.FileList.code);
                Addresser.TryGetRam(ORom.FileList.code, rom.Version, out int code_start);
                startAddr = (code_start | 0x80000000);
            }
            else
            {
                file = rom.Files.GetFile(ovlRec.VRom);
                startAddr = ovlRec.VRam.Start;
            }
            
            file.Stream.Position = ovlRec.VRamActorInfo - startAddr;
            actorInfo = new ActorInit(new BinaryReader(file));

            if (ovlRec.VRamActorInfo != 0)
            {
                var lbl = new Label(Label.Type.VAR, ovlRec.VRamActorInfo, true)
                {
                    Name = $"{task.Name}_InitVars"
                };
                task.Functions.Add(lbl);
            }
            
            if (actorInfo.init_func != 0)
            {
                var func = new Label(Label.Type.FUNC, actorInfo.init_func, true)
                {
                    Name = $"{task.Name}_Init"
                };
                task.Functions.Add(func);
            }
            if (actorInfo.draw_func != 0)
            {
                var func = new Label(Label.Type.FUNC, actorInfo.draw_func, true)
                {
                    Name = $"{task.Name}_Draw"
                };
                task.Functions.Add(func);
            }

            if (actorInfo.update_func != 0)
            {
                var func = new Label(Label.Type.FUNC, actorInfo.update_func, true)
                {
                    Name = $"{task.Name}_Update"
                };
                task.Functions.Add(func);
            }
            if (actorInfo.dest_func != 0)
            {
                var func = new Label(Label.Type.FUNC, actorInfo.dest_func, true)
                {
                    Name = $"{task.Name}_Destructor"
                };
                task.Functions.Add(func);
            }
        }

        public static List<DisassemblyTask> CreateTaskList(Rom rom)
        {
            List<DisassemblyTask> taskList = new List<DisassemblyTask>();
            List<JOcaBase.JDmaData> DmaData;
            
            if (rom.Version.Game == Game.OcarinaOfTime)
            {
                DmaData = JOcaBase.JQuery.GetOcaDmaData(rom.Version.ToString());
            }
            else if (rom.Version.Game == Game.MajorasMask)
            {
                DmaData = JOcaBase.JQuery.GetMaskDmaData(rom.Version.ToString());
            }
            else
            {
                return taskList;
            }

            var tables = rom.Files.Tables;

            for (int i = 0; i < tables.Actors.Records; i++)
            {
                var ovlRec = rom.Files.GetActorOverlayRecord(i);
                var task = New(DmaData, rom, i, ovlRec, OvlType.Actor);

                //set functions
                GetActorSymbolNames(task, rom, ovlRec);

                taskList.Add(task);
            }

            for (int i = 0; i < tables.Particles.Records; i++)
            {
                var ovlRec = rom.Files.GetParticleOverlayRecord(i);
                taskList.Add(New(DmaData, rom, i, ovlRec, OvlType.Particle));
            }

            for (int i = 0; i < tables.GameOvls.Records; i++)
            {
                var ovlRec = rom.Files.GetGameContextRecord(i);
                taskList.Add(New(DmaData, rom, i, ovlRec, OvlType.Game));
            }

            for (int i = 0; i < tables.PlayerPause.Records; i++)
            {
                var ovlRec = rom.Files.GetPlayPauseOverlayRecord(i);
                taskList.Add(New(DmaData, rom, i, ovlRec, OvlType.PlayPause));
            }

            string[] special;

            if (rom.Version == Game.OcarinaOfTime)
                special = new string[] { "code", "boot", "n64dd" };
            else
                special = new string[] { "code", "boot" };

            foreach (var name in special)
            {
                var task = New(name, rom.Version);
                if (task != null)
                    taskList.Add(task);
            }

            return taskList;
        }
    }
}
