using System.IO;
using System.Text;
using mzxrules.Helper;

namespace mzxrules.OcaLib.Cutscenes
{
    public class TimeEntry : IFrameData
    {
        public static int LENGTH = 0xC;
        public CutsceneCommand RootCommand { get; set; }

        /* 0x00 */ short Action;
        /* 0x02 */ public short StartFrame { get; set; } //ushort
        /* 0x04 */ public short EndFrame { get; set; } //ushort
        /* 0x06 */ byte Hour;
        /* 0x07 */ byte Minute;
        /* 0x08 */ int unknown;

        public TimeEntry(CutsceneCommand root, BinaryReader br)
        {
            RootCommand = root;

            Action = br.ReadBigInt16();
            StartFrame = br.ReadBigInt16();
            EndFrame = br.ReadBigInt16();
            Hour = br.ReadByte();
            Minute = br.ReadByte();
            unknown = br.ReadBigInt32();
        }

        public override string ToString()
        {
            StringBuilder sb = new();

            sb.AppendLine($"Action: {Action:X4}, Start Frame: {StartFrame}, End: {EndFrame}");
            sb.Append($"Time: {Hour:D2}:{Minute:D2}, {unknown:X8}");
            return sb.ToString();
        }
    }
}