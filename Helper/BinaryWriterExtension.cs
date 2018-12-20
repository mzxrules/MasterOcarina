using System;
using System.IO;

namespace mzxrules.Helper
{
    public static class BinaryWriterExtension
    {
        public static void WriteBig(this BinaryWriter bw, int v)
        {
            bw.Write(Endian.ConvertInt32(v));
        }

        public static void WriteBig(this BinaryWriter bw, uint v)
        {
            bw.Write(Endian.ConvertUInt32(v));
        }

        public static void WriteBig(this BinaryWriter bw, short v)
        {
            bw.Write(Endian.ConvertInt16(v));
        }

        public static void WriteBig(this BinaryWriter bw, ushort v)
        {
            bw.Write(Endian.ConvertUInt16(v));
        }

        public static void WriteBig(this BinaryWriter bw, float v)
        {
            byte[] d = BitConverter.GetBytes(v);
            Endian.ReverseBytes(ref d);
            bw.Write(d);
        }
        public static void WriteBig(this BinaryWriter bw, Vector3<short> v)
        {
            WriteBig(bw, v.x);
            WriteBig(bw, v.y);
            WriteBig(bw, v.z);
        }
        public static void WriteBig(this BinaryWriter bw, Vector3<ushort> v)
        {
            WriteBig(bw, v.x);
            WriteBig(bw, v.y);
            WriteBig(bw, v.z);
        }
        public static void WriteBig(this BinaryWriter bw, Vector3<int> v)
        {
            WriteBig(bw, v.x);
            WriteBig(bw, v.y);
            WriteBig(bw, v.z);
        }
        public static void WriteBig(this BinaryWriter bw, Vector3<uint> v)
        {
            WriteBig(bw, v.x);
            WriteBig(bw, v.y);
            WriteBig(bw, v.z);
        }
        public static void WriteBig(this BinaryWriter bw, Vector3<float> v)
        {
            WriteBig(bw, v.x);
            WriteBig(bw, v.y);
            WriteBig(bw, v.z);
        }
    }
}
