using mzxrules.Helper;
using mzxrules.OcaLib;
using mzxrules.OcaLib.Actor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Atom
{
    public partial class DisassemblyTask
    {
        public class SectionList
        {
            public List<Section> Values = new List<Section>();
            
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
        public N64PtrRange VRam { get; protected set; }
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
            Transition
        }

        
        public override string ToString()
        {
            return Name;
        }
        

        public static DisassemblyTask New(JFileInfo file)
        {
            FileAddress ptr = file.Ram.Convert();
            var task = new DisassemblyTask()
            {
                Name = file.File,
                VRam = new N64PtrRange(ptr.Start, ptr.End),
                VRom = file.Rom.Convert()
            };

            foreach(var item in file.Sections)
            {
                var ram = item.Ram.Convert();
                Section section = new Section(item.Name, task.VRam.Start, ram.Start, ram.Size, item.Subsection, item.IsCode);
                task.Sections.Values.Add(section);
            }
            return task;
        }

        private static DisassemblyTask New(List<JOcaBase.JDmaData> dmadata, Rom rom, int index,
            OverlayRecord ovlInfo, OvlType nameClass)
        {
            RomFile file; //what we're going to disassemble 

            var overlay = new Overlay();
            var dmaRecord = dmadata.SingleOrDefault(x => x.VRomStart == ovlInfo.VRom.Start && ovlInfo.VRom.Start != 0);
            string name = (dmaRecord != null)? dmaRecord.Filename : $"{nameClass}_{index:X4}";

            if (ovlInfo.VRom.Size != 0)
            {
                file = rom.Files.GetFile(ovlInfo.VRom);
                BinaryReader br = new BinaryReader(file);
                overlay = new Overlay(br);
            }

            DisassemblyTask task = new DisassemblyTask()
            {
                Name = name,
                VRam = ovlInfo.VRam,
                VRom = ovlInfo.VRom,
                Relocations = overlay.Relocations
            };

            N64Ptr fstart = task.VRam.Start;

            Section text = new Section("text", fstart, fstart, overlay.TextSize, 0, true);
            Section data = new Section("data", fstart, text.VRam + text.Size, overlay.DataSize, 0);
            Section rodata = new Section("rodata", fstart, data.VRam + data.Size, overlay.RodataSize, 0);
            long off = ovlInfo.VRam.Start + ovlInfo.VRom.Size;
            Section bss = new Section("bss", fstart, off, overlay.BssSize, 0);

            task.Sections.Values.Add(text);
            task.Sections.Values.Add(data);
            task.Sections.Values.Add(rodata);
            task.Sections.Values.Add(bss);
            return task;
        }

        private static void GetActorSymbolNames(DisassemblyTask task, Rom rom, ActorOverlayRecord ovlRec)
        {
            if (ovlRec.VRamActorInfo == 0)
                return;

            N64Ptr startAddr;
            RomFile file;
            if (ovlRec.VRom.Size == 0)
            {
                file = rom.Files.GetFile(ORom.FileList.code);
                Addresser.TryGetRam(ORom.FileList.code, rom.Version, out int code_start);
                startAddr = code_start | 0x80000000;
            }
            else
            {
                file = rom.Files.GetFile(ovlRec.VRom);
                startAddr = ovlRec.VRam.Start;
            }
            
            file.Stream.Position = ovlRec.VRamActorInfo - startAddr;
            ActorInit actorInfo = new ActorInit(new BinaryReader(file));
            
            BindSymbol(ovlRec.VRamActorInfo, Label.Type.VAR, "InitVars");
            BindSymbol(actorInfo.init_func, Label.Type.FUNC, "Init");
            BindSymbol(actorInfo.draw_func, Label.Type.FUNC, "Draw");
            BindSymbol(actorInfo.update_func, Label.Type.FUNC, "Update");
            BindSymbol(actorInfo.dest_func, Label.Type.FUNC, "Destructor");
            
            void BindSymbol(N64Ptr ptr, Label.Type type, string name)
            {
                if (ptr != 0)
                {
                    var func = new Label(type, ptr, true)
                    {
                        Name = $"{task.Name}_{name}"
                    };
                    task.Functions.Add(func);
                }
            }
        }

        public static List<DisassemblyTask> CreateTaskList(Rom rom)
        {
            List<DisassemblyTask> taskList = new List<DisassemblyTask>();
            List<JOcaBase.JDmaData> dmadata;
            
            if (rom.Version.Game == Game.OcarinaOfTime)
            {
                dmadata = JOcaBase.JQuery.GetOcaDmaData(rom.Version.ToString());
            }
            else if (rom.Version.Game == Game.MajorasMask)
            {
                dmadata = JOcaBase.JQuery.GetMaskDmaData(rom.Version.ToString());
            }
            else
            {
                return taskList;
            }

            var tables = rom.Files.Tables;

            for (int i = 0; i < tables.Actors.Records; i++)
            {
                var ovlRec = rom.Files.GetActorOverlayRecord(i);
                var task = New(dmadata, rom, i, ovlRec, OvlType.Actor);

                //set functions
                GetActorSymbolNames(task, rom, ovlRec);

                taskList.Add(task);
            }

            for (int i = 0; i < tables.Particles.Records; i++)
            {
                var ovlRec = rom.Files.GetParticleOverlayRecord(i);
                taskList.Add(New(dmadata, rom, i, ovlRec, OvlType.Particle));
            }

            for (int i = 0; i < tables.GameOvls.Records; i++)
            {
                var ovlRec = rom.Files.GetGameContextRecord(i);
                taskList.Add(New(dmadata, rom, i, ovlRec, OvlType.Game));
            }

            for (int i = 0; i < tables.PlayerPause.Records; i++)
            {
                var ovlRec = rom.Files.GetPlayPauseOverlayRecord(i);
                taskList.Add(New(dmadata, rom, i, ovlRec, OvlType.PlayPause));
            }

            for (int i = 0; i < tables.Transitions.Records; i++)
            {
                var ovlRec = rom.Files.GetOverlayRecord(i, TableInfo.Type.Transitions);
                taskList.Add(New(dmadata, rom, i, ovlRec, OvlType.Transition));
            }

            List<JFileInfo> fileInfo = JQuery.Deserialize<List<JFileInfo>>("data/fileinfo.json");
            
            foreach(var file in fileInfo.Where
                (x => x.Game == rom.Version.Game.ToString() 
                && x.Version == rom.Version.ToString()))
            {
                taskList.Add(New(file));
            }
            return taskList;
        }
    }
}
