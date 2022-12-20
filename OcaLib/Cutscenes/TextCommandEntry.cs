using mzxrules.Helper;
using System;
using System.IO;
using System.Text;

namespace mzxrules.OcaLib.Cutscenes
{
    public class TextCommandEntry : IFrameData
    {
        public const int LENGTH = 12;
        public ushort Action;
        public CutsceneCommand RootCommand { get; set; }
        public short StartFrame { get; set; }
        public short EndFrame { get; set; }
        public ushort Option, TextIdChoiceA, TextIdChoiceB;
        internal void Load(CutsceneCommand cmd, BinaryReader br)
        {
            /* 0x00 */ Action = br.ReadBigUInt16();
            /* 0x02 */ StartFrame = br.ReadBigInt16();
            /* 0x04 */ EndFrame = br.ReadBigInt16();
            /* 0x06 */ Option = br.ReadBigUInt16();
            /* 0x08 */ TextIdChoiceA = br.ReadBigUInt16();
            /* 0x0A */ TextIdChoiceB = br.ReadBigUInt16();
            RootCommand = cmd;
        }
        public void Save(BinaryWriter bw)
        {
            bw.WriteBig(Action);
            bw.WriteBig(StartFrame);
            bw.WriteBig(EndFrame);
            bw.WriteBig(Option);
            bw.WriteBig(TextIdChoiceA);
            bw.WriteBig(TextIdChoiceB);
        }
        public override string ToString()
        {
            StringBuilder sb = new();
            string option = $"{Option:X4}";
            switch (Option)
            {
                case 0:
                    option = "Mesg";
                    break;
                case 1:
                    option = "Choice";
                    break;
                case 2:
                    option = "Ocarina";
                    break;
                case 3:
                    option = "Sapphire Branch";
                    break;
                case 4:
                    option = "Ruby Branch";
                    break;
            }


            sb.AppendLine($"Action: {Action:X4}, Start: {StartFrame:X4}, End: {EndFrame:X4}");
            sb.Append($"Option: {option}  {TextIdChoiceA:X4} {TextIdChoiceB:X4}");
            return sb.ToString();
        }
    }
}
