using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace mzxrules.OcaLib.Cutscenes
{
    public class TransitionCommand : CutsceneCommand, IFrameData
    {
        const int LENGTH = 0x10;
        uint Entries;
        public ushort Transition;
        public short StartFrame { get; set; }
        public short EndFrame { get; set; }
        public CutsceneCommand RootCommand
        {
            get { return this; }
            set { throw new InvalidOperationException(); }
        }
        short EndFrameD;

        public TransitionCommand(int command, BinaryReader br)
            : base(command, br)
        {
            Load(br);
        }

        public TransitionCommand(short start, short end, ushort transition)
        {
            Command = 0x2D;
            Transition = transition;
            StartFrame = start;
            EndFrame = end;
            EndFrameD = end;
        }
       
        private void Load(BinaryReader br)
        {
            byte[] arr;

            arr = br.ReadBytes(12);

            Endian.Convert(out Entries, arr, 0);
            Endian.Convert(out Transition, arr, 4);
            Endian.Convert(out short startFrame, arr, 6);
            Endian.Convert(out short endFrame, arr, 8);
            Endian.Convert(out EndFrameD, arr, 10);

            StartFrame = startFrame;
            EndFrame = endFrame;
        }

        public override void Save(BinaryWriter bw)
        {
            bw.WriteBig(Command);
            bw.WriteBig(Entries);
            bw.WriteBig(Transition);
            bw.WriteBig(StartFrame);
            bw.WriteBig(EndFrame);
            bw.WriteBig(EndFrame); //EndframeD
        }
        public override string ReadCommand()
        {
            return ToString();
        }
        public override string ToString()
        {
            StringBuilder sb = new();

            sb.AppendLine($"{Command:X4}: Transition, Entries {Entries}");
            sb.AppendLine($"  Action: {Transition:X4}, Start: {StartFrame:X4} End: {EndFrame:X4} End: {EndFrameD:X4}");
            return sb.ToString();
        }
        public IEnumerable<IFrameData> GetIFrameDataEnumerator()
        {
            yield return this;
        }

        protected override int GetLength()
        {
            return LENGTH;
        }
    }
}
