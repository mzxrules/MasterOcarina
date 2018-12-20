using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using mzxrules.Helper;

namespace mzxrules.OcaLib.Cutscenes
{
    public class ExitCommand : CutsceneCommand, IFrameData
    {
        const int LENGTH = 16;
        public CutsceneCommand RootCommand { get; set; }

        int Unknown = 1;

        public ushort Asm;
        public short StartFrame { get; set; }
        public short EndFrame { get; set; }
        short endFrame2;

        public ExitCommand(ushort asm, short start, short end )
        {
            RootCommand = this;
            Command = 0x3E8;

            Asm = asm;
            StartFrame = start;
            EndFrame = end;
            endFrame2 = end;
        }

        public ExitCommand(ExitCommand copy)
        {
            RootCommand = this; //root command should be self, not the copied instance
            Command = copy.Command;
            Unknown = copy.Unknown;
            Asm = copy.Asm;
            StartFrame = copy.StartFrame;
            EndFrame = copy.EndFrame;
            endFrame2 = copy.endFrame2;
        }

        public ExitCommand(int command, BinaryReader br)
            : base(command, br)
        {
            Load(br);
        }

        private void Load(BinaryReader br)
        {
            RootCommand = this;
            Unknown = br.ReadBigInt32();

            Asm = br.ReadBigUInt16();
            StartFrame = br.ReadBigInt16();
            EndFrame = br.ReadBigInt16();
            endFrame2 = br.ReadBigInt16();

            if (Unknown != 1)
                throw new ArgumentOutOfRangeException("Too many 3E8 command entries");
        }

        public override void Save(BinaryWriter bw)
        {
            bw.WriteBig(Command);
            bw.WriteBig(Unknown);

            bw.WriteBig(Asm);
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
            return $"{Command:X4}: Exit Command, {Unknown} entries{Environment.NewLine}"
                + $"   asm: {Asm:X4}, start: {StartFrame:X4}, end: {EndFrame:X4}, {endFrame2:X4}";
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