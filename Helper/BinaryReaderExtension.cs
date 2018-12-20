using System;
using System.IO;

namespace mzxrules.Helper
{
    public static class BinaryReaderExtension
    {
        public static Int16 ReadBigInt16(this BinaryReader b)
        {
            return Endian.ConvertShort(b.ReadInt16());
        }
        public static Int32 ReadBigInt32(this BinaryReader b)
        {
            return Endian.ConvertInt32(b.ReadInt32());
        }

        public static UInt16 ReadBigUInt16(this BinaryReader b)
        {
            return Endian.ConvertUShort(b.ReadUInt16());
        }
        public static UInt32 ReadBigUInt32(this BinaryReader b)
        {
            return Endian.ConvertUInt32(b.ReadUInt32());
        }

        public static float ReadBigFloat(this BinaryReader b)
        {
            byte[] fl = b.ReadBytes(4);
            Endian.ReverseBytes(ref fl, 4);
            return BitConverter.ToSingle(fl, 0);
        }

        public static bool CanReadNext(this BinaryReader br, long size)
        {
            return size <= (br.BaseStream.Length - br.BaseStream.Position);
        }

        public static long Seek(this BinaryReader b, long address)
        {
            long seekback = b.BaseStream.Position;
            b.BaseStream.Position = address;
            return seekback;
        }
    }
}
