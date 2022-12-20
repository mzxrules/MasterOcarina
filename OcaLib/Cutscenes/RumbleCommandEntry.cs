using System.IO;
using System.Text;
using mzxrules.Helper;

namespace mzxrules.OcaLib.Cutscenes
{
    class RumbleCommandEntry : IFrameData
    {
        public CutsceneCommand RootCommand { get; set; }
        public const int LENGTH = 12;

        ushort Action;
        public short StartFrame { get; set; }
        public short EndFrame { get; set; }
        byte SourceStrength;
        byte Duration;
        byte DecreaseRate;
        byte unk1;
        ushort unk2;

        public RumbleCommandEntry(CutsceneCommand cmd, BinaryReader br)
        {
            RootCommand = cmd;
            Action = br.ReadBigUInt16();
            StartFrame = br.ReadBigInt16();
            EndFrame = br.ReadBigInt16();

            SourceStrength = br.ReadByte();
            Duration = br.ReadByte();
            DecreaseRate = br.ReadByte();
            unk1 = br.ReadByte();
            unk2 = br.ReadBigUInt16();
        }

        internal void Save(BinaryWriter bw)
        {
            bw.WriteBig(Action);
            bw.WriteBig(StartFrame);
            bw.WriteBig(EndFrame);
            bw.WriteBig(SourceStrength);
            bw.WriteBig(Duration);
            bw.WriteBig(DecreaseRate);
            bw.WriteBig(unk2);
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine($"Action: {Action:X4} Start Frame: {StartFrame:X4} End Frame: {EndFrame:X4}");
            sb.Append($"Strength: {SourceStrength:X2}, Duration: {Duration:X2}, Decrease: {DecreaseRate:X2},  Unknown: {unk1:X2} {unk2:X4}");

            return sb.ToString();
        }
    }
}
