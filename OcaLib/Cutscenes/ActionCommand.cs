using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using mzxrules.Helper;

namespace mzxrules.OcaLib.Cutscenes
{
    public class ActionCommand : CutsceneCommand, IFrameCollection
    {
        const int LENGTH = 8;
        public List<ActionEntry> Entries = new List<ActionEntry>();

        public IEnumerable<IFrameData> IFrameDataEnum => throw new NotImplementedException();

        public ActionCommand(int command, BinaryReader br)
            : base(command, br)
        {
            Load(br);
        }
        
        public ActionCommand(int command, ActionEntry entry)
        {
            Command = command;
            entry.RootCommand = this;
            Entries.Add(entry);
        }
        
        private void Load(BinaryReader br)
        {
            int EntryCount;
            EntryCount = br.ReadBigInt32();

            for (int i = 0; i < EntryCount; i++)
            {
                Entries.Add(new ActionEntry(this, br));
            }
        }

        public override void Save(BinaryWriter bw)
        {
            bw.WriteBig(Command);
            bw.WriteBig(Entries.Count);
            foreach (ActionEntry item in Entries)
                item.Save(bw);
        }

        public void RemoveEntry(IFrameData i)
        {
            Entries.Remove((ActionEntry)i);
        }

        public override string ToString()
        {
            return $"{Command:X4}: {GetName()}, Entries: {Entries.Count:X8}";
        }

        private string GetName()
        {
            switch (Command)
            {
                case 0x03: return "Special Execution";
                case 0x04: return "Set Lighting";
                case 0x0A: return "Link";
                case 0x27: return "Rauru (08)";
                case 0x29: return "Darunia (Chamber of Sages)";
                case 0x2A: return "Ruto (Adult) (11)";
                case 0x2B: return "Saria (12)";
                case 0x2C: return "Sage? (13)";
                case 0x2F: return "Sheik (12)";
                case 0x32: return "Darunia (Death Mountain Trail)";
                case 0x3E: return "Navi";
                case 0x55: return "Zelda (Adult)";
                case 0x56: return "Play Background Music";
                case 0x57: return "Stop Background Music";
                case 0x7C: return "Fade Background Music";
                default: return "Actor";
            }
        }

        public override string ReadCommand()
        {
            StringBuilder sb;
            sb = new StringBuilder();

            sb.AppendLine(ToString());
            foreach (ActionEntry e in Entries)
                sb.AppendLine($"   {e}");
            return sb.ToString();
        }

        protected override int GetLength()
        {
            return Entries.Count * ActionEntry.LENGTH + LENGTH;
        }

        public IEnumerable<IFrameData> GetIFrameDataEnumerator()
        {
            foreach (IFrameData fd in Entries)
                yield return fd;
        }

        public void AddEntry(IFrameData item)
        {
            Entries.Add((ActionEntry)item);
        }
    }
}