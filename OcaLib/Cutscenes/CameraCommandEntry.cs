using mzxrules.Helper;
using System;
using System.IO;

namespace mzxrules.OcaLib.Cutscenes
{
    public class CameraCommandEntry : IFrameData
    {
        public const int LENGTH = 0x10;

        //0x00
        public byte Terminator;
        sbyte Rotation;
        public ushort Frames;
        //0x04
        public float AngleOfView;
        //0x08
        public Vector3<short> Coordinates = new Vector3<short>();
        //0x0E
        public ushort d;

        public bool IsLastEntry { get { return (Terminator == 0xFF); } }

        public CutsceneCommand RootCommand { get; set; }
        public short StartFrame { get; set; }
        public short EndFrame { get; set; }

        public CameraCommandEntry(CutsceneCommand root)
        {
            RootCommand = root;
        }

        public CameraCommandEntry(CutsceneCommand root, short startFrame,
            sbyte rotation, ushort frames, float angleofview, Vector3<short> coordinates)
        {
            //IFrame data
            RootCommand = root;
            StartFrame = startFrame;
            EndFrame = (short)(startFrame + frames);

            Terminator = 0;
            Rotation = rotation;
            Frames = frames;
            AngleOfView = angleofview;
            Coordinates = coordinates;
            d = 0;
        }

        public void Load(short startFrame, BinaryReader br)
        {
            byte[] arr = br.ReadBytes(0x10);

            Terminator = arr[0];
            Rotation = (sbyte)arr[1];

            Endian.Convert(out Frames, arr, 2);
            Endian.Convert(out AngleOfView, arr, 4);
            Endian.Convert(out Coordinates, arr, 8);
            Endian.Convert(out d, arr, 14);

            //IFrameData
            StartFrame = startFrame;
            EndFrame = (short)(startFrame + Frames);
        }

        public void Save(BinaryWriter bw)
        {
            CameraCommandEntry last = null;

            if (RootCommand != null)
            {
                CameraCommand c = (CameraCommand)RootCommand;
                if (c.Entries.Count > 0)
                    last = c.Entries[c.Entries.Count - 1];
            }
            byte terminator = (this == last) ? (byte)0xFF : (byte)0x00;

            //0x00
            bw.Write(terminator);
            bw.Write(Rotation);
            bw.WriteBig(Frames);
            //0x04
            bw.WriteBig(AngleOfView);
            //0x08
            bw.WriteBig(Coordinates.x);
            bw.WriteBig(Coordinates.y);
            //0x0C
            bw.WriteBig(Coordinates.z);
            bw.WriteBig(d);
        }

        public override string ToString()
        {
            float roll = (float)Rotation * 180 / 128;
            return 
                $"{Terminator:X2} Frames: {Frames:X4} Roll: {roll:F2}, View Angle: {AngleOfView:F4} " +
                $"({Coordinates.x}, {Coordinates.y}, {Coordinates.z}) {d:X4}";
        }
    }
}
