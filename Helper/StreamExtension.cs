using System;
using System.Collections.Generic;
using System.IO;

namespace mzxrules.Helper
{
    public static class StreamExtension
    {
        public static bool PadToLength(this Stream sw, long length)
        {
            //if the pad to length isn't greater than the stream position, 
            //don't pad the stream
            if (!sw.CanWrite)
                return false;

            if (length <= sw.Position)
                return true;

            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, "PadToLength must fit within int");
            }

            byte[] padding = new byte[0x10];
            int offset = (int)(sw.Position % padding.Length);
            if (offset != 0)
            {
                sw.Write(padding, offset, padding.Length - offset);
            }
            int count = (int)((length - sw.Position) / padding.Length);
            for (int i = 0; i < count; i++)
            {
                sw.Write(padding, 0, padding.Length);
            }
            return true;
        }
        public static bool PadToNextLine(this Stream sw)
        {
            int offset;
            if (!sw.CanWrite)
                return false;

            offset = (0x10 - ((int)sw.Length % 0x10)) & 0x0F;
            return sw.Pad(offset);
        }
        /// <summary>
        /// Zero pads the stream by n bytes
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="n">Number of bytes to pad the stream by</param>
        /// <returns></returns>
        public static bool Pad(this Stream sw, int n)
        {
            if (!sw.CanWrite)
                return false;
            while (n > 0)
            {
                sw.WriteByte(0);
                n--;
            }
            return true;
        }
        
        public static IEnumerable<int[]> GetDelta(this Stream a, Stream b, int max = -1)
        {
            long aPos, bPos;
            BinaryReader brA = new BinaryReader(a, System.Text.Encoding.UTF8, true);
            BinaryReader brB = new BinaryReader(b, System.Text.Encoding.UTF8, true);
            int vA, vB;

            if (!a.CanSeek || !b.CanSeek)
                throw new NotImplementedException("Can't compare non-seekable stream");
            
            aPos = a.Position;
            bPos = b.Position;

            a.Position = 0;
            b.Position = 0;


            while (a.Position < a.Length)
            {
                vA = brA.ReadBigInt32();
                vB = brB.ReadBigInt32();

                if (vA != vB)
                {
                    yield return new int[] { (int)a.Position, vA, vB };
                    max--;
                    if (max == 0)
                        break;
                }

            }
            a.Position = aPos;
            b.Position = bPos;
            brA.Close();
            brB.Close();
        }

        public static bool IsDifferentTo(this Stream a, Stream b)
        {
            long aPos;
            long bPos;

            if (!a.CanSeek || !b.CanSeek)
                throw new NotImplementedException("Can't compare non-seekable stream");
            
            if (a.Length != b.Length)
                return true;
            
            aPos = a.Position;
            bPos = b.Position;

            a.Position = 0;
            b.Position = 0;

            while (a.Position < a.Length)
            {
                if (a.ReadByte() != b.ReadByte())
                {
                    a.Position = aPos;
                    b.Position = bPos;
                    return true;
                }
            }
            a.Position = aPos;
            b.Position = bPos;
            return false;
        }
    }
}
