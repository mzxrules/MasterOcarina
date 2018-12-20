// Yaz (de)compression algorithm was found on http://www.amnoid.de and ported to C#

using System;
using System.IO;
using System.Threading.Tasks;

namespace mzxrules.Helper
{
    public class Yaz
    {
        /// <summary>
        /// Returns the size of the Yaz block
        /// </summary>
        /// <param name="sr">Stream pointing to Yaz block</param>
        /// <param name="size">Returns the decompressed size of the block</param>
        /// <returns>True if value read, else false</returns>
        public static bool DecodeSize(Stream sr, out long sizeInt)
        {
            int[] size = new int[1];
            byte[] buf = new byte[sizeof(int)];

            sr.Position += 4;
            sr.Read(buf, 0, sizeof(int));
            Endian.ReverseBytes(ref buf, sizeof(int));
            Buffer.BlockCopy(buf, 0, size, 0, sizeof(int));
            sizeInt = size[0];
            sr.Position -= 8;
            return true;
        }

        /// <summary>
        /// Decompresses a block compressed with the Yaz algorithm with respect to a little endian machine
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="yazBlockSize">The size of the block without header?</param>
        /// <returns></returns>
        public static byte[] Decode(Stream sr, int yazBlockSize)
        {
            byte[] buf;
            int[] size;

            buf = new byte[sizeof(int)];
            size = new int[1];

            sr.Position += 4;
            sr.Read(buf, 0, sizeof(int));
            Endian.ReverseBytes(ref buf, sizeof(int));
            Buffer.BlockCopy(buf, 0, size, 0, sizeof(int));
            sr.Position += 8;

            buf = new byte[yazBlockSize];
            sr.Read(buf, 0, yazBlockSize);

            Decode(buf, out byte[] result, size[0]);
            return result;
        }

        /// <summary>
        /// Decodes a Yaz block
        /// </summary>
        /// <param name="src">points to the Yaz source data (to the "real" source data, not at the header!)</param>
        /// <param name="dst">points to a buffer uncompressedSize bytes large (you get uncompressedSize from</param>
        /// <param name="uncompressedSize">the second 4 bytes in the Yaz header)</param>
        static void Decode(byte[] src, out byte[] dst, int uncompressedSize)
        {
            int srcPlace = 0, dstPlace = 0; //current read/write positions

            int validBitCount = 0; //number of valid bits left in "code" byte
            byte currCodeByte = 0;//set on first pass

            dst = new byte[uncompressedSize];

            while (dstPlace < uncompressedSize)
            {
                //read new "code" byte if the current one is used up
                if (validBitCount == 0)
                {
                    currCodeByte = src[srcPlace];
                    ++srcPlace;
                    validBitCount = 8;
                }

                if ((currCodeByte & 0x80) != 0)
                {
                    //straight copy
                    dst[dstPlace] = src[srcPlace];
                    dstPlace++;
                    srcPlace++;
                }
                else
                {
                    //RLE part
                    byte byte1 = src[srcPlace];
                    byte byte2 = src[srcPlace + 1];
                    srcPlace += 2;

                    uint dist = (uint)((byte1 & 0xF) << 8) | byte2;
                    uint copySource = (uint)dstPlace - (dist + 1);

                    uint numBytes = (uint)byte1 >> 4;
                    if (numBytes == 0)
                    {
                        numBytes = (uint)src[srcPlace] + 0x12;
                        srcPlace++;
                    }
                    else
                        numBytes += 2;

                    //copy run
                    for (int i = 0; i < numBytes; ++i)
                    {
                        dst[dstPlace] = dst[copySource];
                        copySource++;
                        dstPlace++;
                    }
                }

                //use next bit from "code" byte
                currCodeByte <<= 1;
                validBitCount -= 1;
            }
        }

        struct Ret
        {
            public int srcPos, dstPos;
            public Ret(int s, int d)
            {
                srcPos = s;
                dstPos = d;
            }
        }

        /// <summary>
        /// Simple and straight encoding scheme for Yaz
        /// </summary>
        /// <param name="src"></param>
        /// <param name="size"></param>
        /// <param name="pos"></param>
        /// <param name="pMatchPos"></param>
        /// <returns></returns>
        static UInt32 SimpleEnc(byte[] src, int size, int pos, ref uint /* u32* */ pMatchPos)
        {
            int startPos = pos - 0x1000;
            int numBytes = 1;
            int matchPos = 0;
            int sizeMinusPos = size - pos; //Replaces J loop terminating condition

            if (startPos < 0)
                startPos = 0;

            //limits forward seeking to the maximum number of bytes that can be encoded
            if (sizeMinusPos > 0xff + 0x12)
                sizeMinusPos = 0xff + 0x12;

            for (int i = startPos; i < pos; i++)
            {
                int j;
                for (j = 0; j < sizeMinusPos/*size - pos*/; j++)
                {
                    if (src[i + j] != src[j + pos])
                        break;
                }
                if (j > numBytes)
                {
                    numBytes = j;
                    matchPos = i;
                }
            }
            pMatchPos = (uint)matchPos; //*pMatchPos = matchPos;
            if (numBytes == 2)
                numBytes = 1;
            return (uint)numBytes;
        }

        class StaticEncodeVars
        {
            public uint numBytes1;
            public uint matchPos;
            public int prevFlag;
        }

        /// <summary>
        /// a lookahead encoding scheme for ngc Yaz
        /// </summary>
        /// <param name="src"></param>
        /// <param name="size"></param>
        /// <param name="pos"></param>
        /// <param name="pMatchPos"></param>
        /// <returns></returns>
        static uint NintendoEnc(byte[] src, int size, int pos, ref uint pMatchPos, StaticEncodeVars var)
        //u32 nintendoEnc(u8* src, int size, int pos, u32 *pMatchPos)
        {
            //int startPos = pos - 0x1000;
            uint numBytes = 1;

            // if prevFlag is set, it means that the previous position was determined by look-ahead try.
            // so just use it. this is not the best optimization, but nintendo's choice for speed.
            if (var.prevFlag == 1)
            {
                pMatchPos = var.matchPos; //*pMatchPos = matchPos;
                var.prevFlag = 0;
                return var.numBytes1;
            }
            var.prevFlag = 0;
            numBytes = SimpleEnc(src, size, pos, ref var.matchPos); //numBytes = simpleEnc(src, size, pos, &matchPos);
            pMatchPos = var.matchPos; //*pMatchPos = matchPos;

            // if this position is RLE encoded, then compare to copying 1 byte and next position(pos+1) encoding
            if (numBytes >= 3)
            {
                var.numBytes1 = SimpleEnc(src, size, pos + 1, ref var.matchPos); //numBytes1 = simpleEnc(src, size, pos+1, &matchPos);
                // if the next position encoding is +2 longer than current position, choose it.
                // this does not guarantee the best optimization, but fairly good optimization with speed.
                if (var.numBytes1 >= numBytes + 2)
                {
                    numBytes = 1;
                    var.prevFlag = 1;
                }
            }
            return numBytes;
        }

        public static Task<int> EncodeAsync(byte[] src, int srcSize, Stream dstFile)
        {
            Task<int> encodeTask = new Task<int>(() => Encode(src, srcSize, dstFile));
            encodeTask.Start();
            return encodeTask;
        }

        /// <summary>
        /// Writes compressed file to given stream, starting at stream's position.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="srcSize"></param>
        /// <param name="dstFile"></param>
        /// <returns></returns>
        public static int Encode(byte[] src, int srcSize, Stream dstFile)
        {
            Ret r = new Ret(0, 0);
            byte[] dst = new byte[24]; // 8 codes * 3 bytes maximum
            int dstSize = 0;

            uint validBitCount = 0; //number of valid bits left in "code" byte
            byte currCodeByte = 0;

            uint numBytes;
            uint matchPos = 0;

            StaticEncodeVars var = new StaticEncodeVars();

            //Write Header
            byte[] srcSizeArr = BitConverter.GetBytes(srcSize);
            byte[] header = new byte[] { 0x59, 0x61, 0x7A, 0x30 }; //Yaz0
            if (BitConverter.IsLittleEndian)
                Array.Reverse(srcSizeArr);

            dstFile.Write(header, 0, 4);
            dstFile.Write(srcSizeArr, 0, 4);
            for (int i = 0; i < 8; i++)
                dstFile.WriteByte(0);

            while (r.srcPos < srcSize)
            {
                numBytes = NintendoEnc(src, srcSize, r.srcPos, ref matchPos, var); //matchPos passed ref &matchpos

                if (numBytes < 3)
                {
                    //straight copy
                    dst[r.dstPos] = src[r.srcPos];
                    r.dstPos++;
                    r.srcPos++;
                    //set flag for straight copy
                    currCodeByte |= (byte)(0x80 >> (int)validBitCount);
                }
                else
                {
                    //RLE part
                    uint dist = (uint)r.srcPos - matchPos - 1;
                    byte byte1, byte2, byte3;

                    if (numBytes >= 0x12)  // 3 byte encoding
                    {
                        byte1 = (byte)(0 | (dist >> 8));
                        byte2 = (byte)(dist & 0xff);
                        dst[r.dstPos++] = byte1;
                        dst[r.dstPos++] = byte2;
                        // maximum runlength for 3 byte encoding
                        if (numBytes > 0xff + 0x12)
                            numBytes = 0xff + 0x12;
                        byte3 = (byte)(numBytes - 0x12);
                        dst[r.dstPos++] = byte3;
                    }
                    else  // 2 byte encoding
                    {
                        byte1 = (byte)(((numBytes - 2) << 4) | (dist >> 8));
                        byte2 = (byte)(dist & 0xff);
                        dst[r.dstPos++] = byte1;
                        dst[r.dstPos++] = byte2;
                    }
                    r.srcPos += (int)numBytes;
                }
                validBitCount++;
                //write eight codes
                if (validBitCount == 8)
                {
                    dstFile.WriteByte(currCodeByte);
                    dstFile.Write(dst, 0, r.dstPos);

                    dstSize += r.dstPos + 1;

                    currCodeByte = 0;
                    validBitCount = 0;
                    r.dstPos = 0;
                }
            }
            if (validBitCount > 0)
            {
                dstFile.WriteByte(currCodeByte);
                dstFile.Write(dst, 0, r.dstPos);
                dstSize += r.dstPos + 1;

                currCodeByte = 0;
                validBitCount = 0;
                r.dstPos = 0;
            }
            return dstSize;
        }
    }
}