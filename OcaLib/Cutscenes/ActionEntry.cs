using mzxrules.Helper;
using System;
using System.IO;
using System.Text;

namespace mzxrules.OcaLib.Cutscenes
{
    public class ActionEntry : IFrameData
    {
        public CutsceneCommand RootCommand { get; set; }
        public short StartFrame { get; set; }
        public short EndFrame { get; set; }
        public const int LENGTH = 0x30;
        public ushort Action;
        public Vector3<ushort> Rotation = new Vector3<ushort>();
        public Vector3<int> StartPosition = new Vector3<int>();
        public Vector3<int> EndPosition = new Vector3<int>();
        public Vector3<float> Normal = new Vector3<float>();

        public ActionEntry(CutsceneCommand cmd, BinaryReader br)
        {
            RootCommand = cmd;
            Load(br);
        }

        private void Load(BinaryReader br)
        {
            byte[] arr = br.ReadBytes(LENGTH);

            /* 0x00 */ Endian.Convert(out Action, arr, 0x00);
            /* 0x02 */ Endian.Convert(out short startFrame, arr, 0x02);
            /* 0x04 */ Endian.Convert(out short endFrame, arr, 0x04);
            /* 0x06 */ Endian.Convert(out Rotation, arr, 0x06);
            /* 0x0C */ Endian.Convert(out StartPosition, arr, 0x0C);
            /* 0x18 */ Endian.Convert(out EndPosition, arr, 0x18);
            /* 0x24 */ Endian.Convert(out Normal, arr, 0x24);

            StartFrame = startFrame;
            EndFrame = endFrame;
        }

        public void Save(BinaryWriter bw)
        {
            bw.WriteBig(Action);
            bw.WriteBig(StartFrame);
            bw.WriteBig(EndFrame);
            bw.WriteBig(Rotation);
            bw.WriteBig(StartPosition);
            bw.WriteBig(EndPosition);
            bw.WriteBig(Normal);
        }

        public override string ToString()
        {
            return 
                $"Action: {Action:X4}, Start: {StartFrame:X4}, End: {EndFrame:X4}, " +
                $"Rotation: {Rotation.x:X4} {Rotation.y:X4} {Rotation.z:X4} " +
                $"Start: ({StartPosition.x}, {StartPosition.y}, {StartPosition.z}) " +
                $"End: ({EndPosition.x}, {EndPosition.y}, {EndPosition.z}) " +
                $"Normal: ({Normal.x:F4}, {Normal.y:F4}, {Normal.z:F4})";
        }
    }
}
