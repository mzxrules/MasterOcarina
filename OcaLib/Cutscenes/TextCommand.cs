using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using mzxrules.Helper;

namespace mzxrules.OcaLib.Cutscenes
{
    public class TextCommand : CutsceneCommand, IFrameCollection
    {
        const int LENGTH = 8;
        public List<TextCommandEntry> Entries = [];

        public IEnumerable<IFrameData> IFrameDataEnum => GetIFrameDataEnumerator();

        public TextCommand(int command, BinaryReader br)
            : base(command, br)
        {
            Load(br);
        }

        protected override int GetLength()
        {
            return TextCommandEntry.LENGTH * Entries.Count + LENGTH;
        }

        private void Load(BinaryReader br)
        {
            int entryCount = br.ReadBigInt32();

            for (int i = 0; i < entryCount; i++)
            {
                Entries.Add(new());
                Entries[i].Load(this, br);
            }
        }

        public void RemoveEntry(IFrameData item)
        {
            Entries.Remove((TextCommandEntry)item);
        }

        public override void Save(BinaryWriter bw)
        {
            bw.WriteBig(Command);
            bw.WriteBig(Entries.Count);
            foreach (TextCommandEntry item in Entries)
                item.Save(bw);
        }

        public override string ToString()
        {
            return $"{Command:X4}: Text Command, Entries: {Entries.Count}";
        }

        public override string ReadCommand()
        {
            StringBuilder sb = new();

            sb.AppendLine(ToString());
            foreach (TextCommandEntry entry in Entries)
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

        public IEnumerable<IFrameData> GetIFrameDataEnumerator()
        {
            foreach (IFrameData fd in Entries)
                yield return fd;
        }

        public void AddEntry(IFrameData d)
        {
            throw new NotImplementedException();
        }
    }
}
