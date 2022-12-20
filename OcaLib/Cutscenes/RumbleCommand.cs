using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using mzxrules.Helper;

namespace mzxrules.OcaLib.Cutscenes
{
    public partial class RumbleCommand : CutsceneCommand, IFrameCollection
    {
        const int LENGTH = 8;
        List<RumbleCommandEntry> Entries = new();

        public IEnumerable<IFrameData> IFrameDataEnum => GetIFrameDataEnumerator();
        
        public RumbleCommand(int command, BinaryReader br)
            : base(command, br)
        {
            Load(br);
        }

        private void Load(BinaryReader br)
        {
            int entryCount = br.ReadBigInt32();
            
            for (int i = 0; i < entryCount; i++)
            {
                Entries.Add(new RumbleCommandEntry(this, br));
            }

        }
        public override void Save(BinaryWriter bw)
        {
            bw.WriteBig(Command);
            bw.WriteBig(Entries.Count);
            foreach (var item in Entries)
            {
                item.Save(bw);
            }
        }

        public override string ToString()
        {
            return $"{Command:X4}: Rumble, Entries: {Entries.Count}";
        }

        public override string ReadCommand()
        {
            StringBuilder sb = new();

            sb.AppendLine(ToString());

            foreach (RumbleCommandEntry ent in Entries)
            {
                int i = 0;
                foreach (var line in ent.ToString().Split(Environment.NewLine))
                {
                    i += 2;
                    sb.AppendLine("".PadLeft(i) + line);
                }
            }
            return sb.ToString();
        }

        protected override int GetLength()
        {
            return Entries.Count * RumbleCommandEntry.LENGTH + LENGTH;
        }

        public  void AddEntry(IFrameData item)
        {
            Entries.Add((RumbleCommandEntry)item);
        }

        public  void RemoveEntry(IFrameData item)
        {
            Entries.Remove((RumbleCommandEntry)item);
        }

        public IEnumerable<IFrameData> GetIFrameDataEnumerator()
        {
            foreach (IFrameData fd in Entries)
                yield return fd;
        }
    }
}
