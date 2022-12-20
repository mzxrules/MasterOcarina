using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using mzxrules.Helper;

namespace mzxrules.OcaLib.Cutscenes
{
    public class DestinationCommand : CutsceneCommand, IFrameData
    {
        const int LENGTH = 16;
        public CutsceneCommand RootCommand { get; set; }

        int Entries = 1;

        public ushort Action;
        public short StartFrame { get; set; }
        public short EndFrame { get; set; }
        short endFrame2;

        public DestinationCommand(ushort asm, short start, short end )
        {
            RootCommand = this;
            Command = 0x3E8;

            Action = asm;
            StartFrame = start;
            EndFrame = end;
            endFrame2 = end;
        }

        public DestinationCommand(DestinationCommand copy)
        {
            RootCommand = this; //root command should be self, not the copied instance
            Command = copy.Command;
            Entries = copy.Entries;
            Action = copy.Action;
            StartFrame = copy.StartFrame;
            EndFrame = copy.EndFrame;
            endFrame2 = copy.endFrame2;
        }

        public DestinationCommand(int command, BinaryReader br)
            : base(command, br)
        {
            Load(br);
        }

        private void Load(BinaryReader br)
        {
            /* 0x00 */ RootCommand = this;
            /* 0x04 */ Entries = br.ReadBigInt32();

            /* 0x08 */ Action = br.ReadBigUInt16();
            /* 0x0A */ StartFrame = br.ReadBigInt16();
            /* 0x0C */ EndFrame = br.ReadBigInt16();
            /* 0x0E */ endFrame2 = br.ReadBigInt16();

            if (Entries != 1)
                throw new ArgumentOutOfRangeException("Too many 3E8 command entries");
        }

        public override void Save(BinaryWriter bw)
        {
            bw.WriteBig(Command);
            bw.WriteBig(Entries);

            bw.WriteBig(Action);
            bw.WriteBig(StartFrame);
            bw.WriteBig(EndFrame);
            bw.WriteBig(EndFrame);//EndFrame2
        }

        public override string ReadCommand()
        {
            StringBuilder r;
            r = new StringBuilder();

            r.AppendLine(ToString());
            return r.ToString();
        }

        public override string ToString()
        {
            StringBuilder sb = new();

            sb.AppendLine($"{Command:X4}: Destination, Entries: {Entries}");
            sb.Append($"  Action: {Action:X4}, Start: {StartFrame:X4}, End: {EndFrame:X4}, {endFrame2:X4}");
            return sb.ToString();
        }

        protected override int GetLength()
        {
            return LENGTH;
        }

        public IEnumerable<IFrameData> GetIFrameDataEnumerator()
        {
            yield return this;
        }
    }
}