using mzxrules.Helper;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace mzxrules.OcaLib.SceneRoom
{
    public struct CollisionPolygon
    {
        public static int SIZE = 0x10;
        
        /* 0x00 */ public short Type;
        /* 0x02 */ public short VertexA, VertexB, VertexC;
        public byte VertexFlagsA;
        public byte VertexFlagsB;
        public byte VertexFlagsC;

        /* 0x08 */ public Vector3<short> Normal;
        /* 0x0E */ public short D;

        public CollisionPolygon(byte[] data)
        {
            Normal = new Vector3<short>();
            Endian.Convert(out Type, data, 0x00);
            Endian.Convert(out ushort vA, data, 0x02);
            Endian.Convert(out ushort vB, data, 0x04);
            Endian.Convert(out ushort vC, data, 0x06);
            Endian.Convert(out Normal, data, 0x08);
            Endian.Convert(out D, data, 0x0E);

            VertexA = (short)(vA & 0x1FFF);
            VertexB = (short)(vB & 0x1FFF);
            VertexC = (short)(vC & 0x1FFF);
            VertexFlagsA = Shift.AsByte(vA, 0xE000);
            VertexFlagsB = Shift.AsByte(vB, 0xE000);
            VertexFlagsC = Shift.AsByte(vC, 0xE000);
        }

        public string Print(List<Vector3<short>> vertexData)
        {
            var va = PrintVertex(VertexA);
            var vb = PrintVertex(VertexB);
            var vc = PrintVertex(VertexC);


            return $"{Type:X4} {VertexFlagsA:X1} {VertexFlagsB:X1} {VertexFlagsC:X1} {va} {vb} {vc} {Normal} {D}";

            string PrintVertex(short vId)
            {
                var v = vertexData[vId];
                return $"{vId::X4} {v}";
            }
        }

        public override string ToString()
        {
            return $"{Type:X4} {VertexFlagsA:X1} {VertexFlagsB:X1} {VertexFlagsC:X1} {VertexA:X4} {VertexB:X4} {VertexC:X4} {Normal} {D}";
        }

        //public static bool operator==(CollisionPolygon a, CollisionPolygon b)
        //{
        //    return
        //        a.Type == b.Type
        //        && a.VertexA == b.VertexA
        //        && a.VertexB == b.VertexB
        //        && a.VertexC == b.VertexC
        //        && a.VertexFlagsA == b.VertexFlagsA
        //        && a.VertexFlagsB == b.VertexFlagsB
        //        && a.VertexFlagsC == b.VertexFlagsC;
        //}

        //public static bool operator!=(CollisionPolygon a, CollisionPolygon b)
        //{
        //    return !(a == b);
        //}
    }


    public struct CollisionPolyType
    {
        public static int SIZE = 0x8;
        public int HighWord { get; }
        public int LowWord { get; }

        static readonly uint[] hi = new uint[]
        {
            0x8000_0000,
            0x4000_0000,
            0x3C00_0000,
            0x03E0_0000,
            0x001C_0000,
            0x0003_E000,
            0x0000_1F00,
            0x0000_00FF,
        };

        static readonly uint[] lo = new uint[]
        {
            0x0800_0000,
            0x07E0_0000,
            0x001C_0000,
            0x0002_0000,
            0x0001_F800,
            0x0000_07C0,
            0x0000_0030,
            0x0000_000F,
        };

        public CollisionPolyType(int high, int low)
        {
            HighWord = high;
            LowWord = low;
        }

        public string PrintPackedVars()
        {
            string result = "";
            List<string> hiV = new List<string>();
            List<string> loV = new List<string>();

            foreach (var item in hi)
            {
                hiV.Add($"{item:X8} = {Shift.AsByte((uint)HighWord, item):X2}");
            }

            foreach (var item in lo)
            {
                loV.Add($"{item:X8} = {Shift.AsByte((uint)LowWord, item):X2}");
            }
            for (int i = 0; i < hiV.Count; i++)
            {
                result += $"{hiV[i]}    {loV[i]}{Environment.NewLine}";
            }
            return result;
        }

        public override string ToString()
        {
            return $"{HighWord:X8}:{LowWord:X8}";
        }
    }

    public struct CollisionCameraData
    {
        public static int SIZE = 0x8;
        /* 0x00 */ public short CameraS;
        /* 0x02 */ public short NumCameras;
        /* 0x04 */ public SegmentAddress PositionAddress;

        public List<CollisionCameraPosition> PositionList;

        public CollisionCameraData(BinaryReader br)
        {
            CameraS = br.ReadBigInt16();
            NumCameras = br.ReadBigInt16();
            PositionAddress = br.ReadBigInt32();

            if (PositionAddress != 0)
            {
                var seekback = br.Seek(PositionAddress.Offset);
                byte[] data;

                PositionList = new List<CollisionCameraPosition>();
                for (int i = 0; i < NumCameras; i += 3)
                {
                    data = br.ReadBytes(CollisionCameraPosition.SIZE);
                    var pos = new CollisionCameraPosition(data);
                    PositionList.Add(pos);
                }

                br.Seek(seekback);
            }
            else
            {
                PositionList = null;
            }
        }

        public bool IsPositionListIdentical(CollisionCameraData other)
        {
            var a = PositionList;
            var b = other.PositionList;

            if (a.Count != b.Count)
                return false;

            for (int i = 0; i < a.Count; i++)
            {
                if (!a[i].Data.SequenceEqual(b[i].Data))
                {
                    return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            return $"Camera S: {CameraS:X4} Num: {NumCameras} Positions: {PositionAddress} -> {PositionList?[0].ToString()}";
        }
    }

    public struct CollisionWaterBox
    {
        public static int SIZE = 0x10;
        public short[] Data;
        
        public CollisionWaterBox(byte[] data)
        {
            Data = new short[SIZE / 2];
            for (int i = 0; i < SIZE / 2; i++)
            {
                Endian.Convert(out short d, data, i * 2);
                Data[i] = d;
            }
        }
        public override string ToString()
        {
            return string.Join(", ", Data);
        }
    }

    public struct CollisionCameraPosition
    {
        public static int SIZE = 0x12;
        public short[] Data;

        public CollisionCameraPosition(byte[] data)
        {
            Data = new short[SIZE / 2];
            for (int i = 0; i < SIZE/2; i++)
            {
                Endian.Convert(out short d, data, i * 2);
                Data[i] = d;
            }
        }
        public override string ToString()
        {
            return string.Join(", ", Data);
        }
    }
}
