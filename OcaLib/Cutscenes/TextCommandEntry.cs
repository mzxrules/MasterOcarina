using mzxrules.Helper;
using System;
using System.IO;

namespace mzxrules.OcaLib.Cutscenes
{
    public class TextCommandEntry : IFrameData
    {
        public const int LENGTH = 12;
        public ushort TextId;
        public CutsceneCommand RootCommand { get; set; }
        public short StartFrame { get; set; }
        public short EndFrame { get; set; }
        public ushort Option, TextIdChoiceA, TextIdChoiceB;
        internal void Load(CutsceneCommand cmd, BinaryReader br)
        {
            /* 0x00 */ TextId = br.ReadBigUInt16();
            /* 0x02 */ StartFrame = br.ReadBigInt16();
            /* 0x04 */ EndFrame = br.ReadBigInt16();
            /* 0x06 */ Option = br.ReadBigUInt16();
            /* 0x08 */ TextIdChoiceA = br.ReadBigUInt16();
            /* 0x0A */ TextIdChoiceB = br.ReadBigUInt16();
            RootCommand = cmd;
        }
        public void Save(BinaryWriter bw)
        {
            bw.WriteBig(TextId);
            bw.WriteBig(StartFrame);
            bw.WriteBig(EndFrame);
            bw.WriteBig(Option);
            bw.WriteBig(TextIdChoiceA);
            bw.WriteBig(TextIdChoiceB);
        }
        public override string ToString()
        {
            string txt = (TextId != 0xFFFF) ? $"{TextId:X4}" : "None";
            return 
                $"Text {txt}, Start: {StartFrame:X4}, End: {EndFrame:X4}," +
                $" Option: {Option:X4}  {TextIdChoiceA:X4} {TextIdChoiceB:X4}";
        }
    }
}
