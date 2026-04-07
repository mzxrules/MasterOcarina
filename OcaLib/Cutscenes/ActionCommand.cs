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
        public List<ActionEntry> Entries = [];

        public IEnumerable<IFrameData> IFrameDataEnum => Entries;

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
            int entryCount;

            /* 0x04 */ entryCount = br.ReadBigInt32();

            for (int i = 0; i < entryCount; i++)
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
            return $"{Command:X4}: {GetName()}, Entries: {Entries.Count}";
        }

        private string GetName()
        {
            return Command switch
            {
                0x03 => "Special Execution",
                0x04 => "Set Lighting",
                0x0A => "Link",
                0x27 => "Rauru (08)",
                0x29 => "Darunia (Chamber of Sages)",
                0x2A => "Ruto (Adult) (11)",
                0x2B => "Saria (12)",
                0x2C => "Sage? (13)",
                0x2F => "Sheik (12)",
                0x32 => "Darunia (Death Mountain Trail)",
                0x3E => "Navi",
                0x55 => "Zelda (Adult)",
                0x56 => "Play Background Music",
                0x57 => "Stop Background Music",
                0x7C => "Fade Background Music",
                _ => "Actor",
            };
        }

        public override string ReadCommand()
        {
            StringBuilder sb = new();

            sb.AppendLine(ToString());
            foreach (ActionEntry entry in Entries)
            {
                int i = 0;
                foreach (var line in entry.ToString().Split(Environment.NewLine))
                {
                    i += 2;
                    sb.AppendLine("".PadLeft(i) + line);
                }
            }

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

        public void AddEntry(IFrameData entry)
        {
            ArgumentNullException.ThrowIfNull(entry);
            Entries.Add((ActionEntry)entry);
        }
    }
}