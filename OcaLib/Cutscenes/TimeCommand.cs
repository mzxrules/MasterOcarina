using System.Collections.Generic;
using System.IO;
using System.Text;
using mzxrules.Helper;

namespace mzxrules.OcaLib.Cutscenes
{
    internal class TimeCommand : CutsceneCommand
    {
        const int LENGTH = 8;
        private int commandId;
        public List<TimeEntry> Entries = new();

        public TimeCommand(int commandId, BinaryReader br)
        {
            this.commandId = commandId;

            int entries = br.ReadBigInt32();
            for (int i = 0; i< entries; i++)
            {
                Entries.Add(new TimeEntry(this, br));
            }
        }

        protected override int GetLength()
        {
            return Entries.Count * TimeEntry.LENGTH + LENGTH;
        }

        public override string ReadCommand()
        {
            StringBuilder sb = new();

            sb.AppendLine(ToString());
            foreach (TimeEntry entry in Entries)
            {
                sb.AppendLine($"   {entry}");
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            return $"{Command:X4}: Set Time, Entries: {Entries.Count}";
        }
    }
}