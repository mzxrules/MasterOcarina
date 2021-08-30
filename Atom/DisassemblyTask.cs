using mzxrules.Helper;
using mzxrules.OcaLib;
using mzxrules.OcaLib.Actor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JOcaBase;
using System;

namespace Atom
{
    public partial class DisassemblyTask
    {
        public class SectionList
        {
            public List<Section> Values = new();
            
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

        public SectionList Sections = new();

        public List<Overlay.RelocationWord> Relocations = new();
        
        //used for oot decomp
        public N64PtrRange HeaderAndReloc { get; protected set; }

        public bool SplitFunctions { get; protected set; }

        public List<Label> Functions = new();
        public List<Action<Stream>> PreparseActions = new();

        
        public enum OvlType
        {
            Actor,
            Particle,
            PlayPause,
            Game,
            Transition,
            MapMarkData
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
                Section section = new(item.Name, task.VRam.Start, ram.Start, ram.Size, item.Subsection, item.IsCode);
                task.Sections.Values.Add(section);
            }
            return task;
        }

        private static DisassemblyTask New(List<JDmaData> dmadata, int index, OverlayRecord ovlInfo,
            OvlType nameClass)
        {
            string name = GetTaskName(dmadata, index, ovlInfo, nameClass);

            DisassemblyTask task = new()
            {
                Name = name,
                VRam = ovlInfo.VRam,
                VRom = ovlInfo.VRom,
            };
            task.PreparseActions.Add(input => {
                OverlayPreprocess(task, input, ovlInfo); 
            });
            return task;
        }

        private static void OverlayPreprocess(DisassemblyTask task, Stream file, OverlayRecord ovlInfo)
        {
            var overlay = new Overlay();
            if (ovlInfo.VRom.Size != 0)
            {
                BinaryReader br = new(file);
                overlay = new Overlay(br);
            }
            task.Relocations = overlay.Relocations;

            N64Ptr fstart = task.VRam.Start;

            Section text = new("text", fstart, fstart, overlay.TextSize, 0, true);
            Section data = new("data", fstart, text.VRam + text.Size, overlay.DataSize, 0);
            Section rodata = new("rodata", fstart, data.VRam + data.Size, overlay.RodataSize, 0);
            long off = task.VRam.Start + task.VRom.Size;

            task.HeaderAndReloc = new N64PtrRange(task.VRam.Start + overlay.header_offset, off);
            Section bss = new("bss", fstart, off, overlay.BssSize, 0);

            task.Sections.Values.Add(text);
            task.Sections.Values.Add(data);
            task.Sections.Values.Add(rodata);
            task.Sections.Values.Add(bss);
        }

        private static string GetTaskName(List<JDmaData> dmadata, int index, OverlayRecord ovlInfo, OvlType nameClass)
        {
            var dmaRecord = dmadata.SingleOrDefault(x => x.VRomStart == ovlInfo.VRom.Start && ovlInfo.VRom.Start != 0);
            string name = (dmaRecord != null) ? dmaRecord.Filename : $"{nameClass}_{index:X4}";
            return name;
        }

        private static void GetActorSymbolNames(DisassemblyTask task, Rom rom, ActorOverlayRecord ovlRec)
        {
            if (ovlRec.VRamActorInit == 0)
                return;

            if (ovlRec.VRom.Size == 0)
            {
                RomFile file = rom.Files.GetFile(ORom.FileList.code);
                Addresser.TryGetRam(ORom.FileList.code, rom.Version, out N64Ptr code_start);
                N64Ptr startAddr = code_start;

                GetActorInfoSymbols(task, startAddr, ovlRec.VRamActorInit, file);
            }
            else
            {
                task.PreparseActions.Add(file => {
                    GetActorInfoSymbols(task, ovlRec.VRam.Start, ovlRec.VRamActorInit, file);
                });
            }

        }

        private static void GetActorInfoSymbols(DisassemblyTask task, N64Ptr startAddr, N64Ptr vramActorInfo, Stream file)
        {
            file.Position = vramActorInfo - startAddr;
            ActorInit actorInfo = new(new BinaryReader(file));

            BindSymbol(vramActorInfo, Label.Type.VAR, "InitVars");
            BindSymbol(actorInfo.init_func, Label.Type.FUNC, "Init");
            BindSymbol(actorInfo.draw_func, Label.Type.FUNC, "Draw");
            BindSymbol(actorInfo.update_func, Label.Type.FUNC, "Update");
            BindSymbol(actorInfo.dest_func, Label.Type.FUNC, "Destructor");

            void BindSymbol(N64Ptr ptr, Label.Type type, string name)
            {
                if (ptr != 0)
                {
                    var func = new Label(type, ptr, true, Disassemble.MipsToC)
                    {
                        Name = $"{task.Name}_{name}"
                    };
                    task.Functions.Add(func);
                }
            }
        }

        public static List<DisassemblyTask> CreateTaskList(Rom rom)
        {
            List<DisassemblyTask> taskList = new();
            List<JDmaData> dmadata;
            
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
                var task = New(dmadata, i, ovlRec, OvlType.Actor);

                //set functions
                GetActorSymbolNames(task, rom, ovlRec);

                taskList.Add(task);
            }

            for (int i = 0; i < tables.Particles.Records; i++)
            {
                var ovlRec = rom.Files.GetParticleOverlayRecord(i);
                taskList.Add(New(dmadata, i, ovlRec, OvlType.Particle));
            }

            for (int i = 0; i < tables.GameOvls.Records; i++)
            {
                var ovlRec = rom.Files.GetGameContextRecord(i);
                taskList.Add(New(dmadata, i, ovlRec, OvlType.Game));
            }

            for (int i = 0; i < tables.PlayerPause.Records; i++)
            {
                var ovlRec = rom.Files.GetPlayPauseOverlayRecord(i);
                taskList.Add(New(dmadata, i, ovlRec, OvlType.PlayPause));
            }

            for (int i = 0; i < tables.Transitions.Records; i++)
            {
                var ovlRec = rom.Files.GetOverlayRecord(i, TableInfo.Type.Transitions);
                taskList.Add(New(dmadata, i, ovlRec, OvlType.Transition));
            }

            for (int i = 0; i < tables.MapMarkData.Records; i++)
            {
                var ovlRec = rom.Files.GetOverlayRecord(i, TableInfo.Type.MapMarkData);
                if (ovlRec != null)
                {
                    taskList.Add(New(dmadata, i, ovlRec, OvlType.MapMarkData));
                }
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
