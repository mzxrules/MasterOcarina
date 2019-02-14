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
        /// <param name="src">Yaz raw data, past the header</param>
        /// <param name="dst">Decompressed file</param>
        /// <param name="decompressedSize">Decompressed size, in bytes</param>
        static void Decode(byte[] src, out byte[] dst, int decompressedSize)
        {
            int srcPos = 0; //read position
            int dstPos = 0; //write position

            int validBitCount = 0; //number of valid bits left in "code" byte
            byte curCodeByte = 0; //set on first pass

            dst = new byte[decompressedSize];

            while (dstPos < decompressedSize)
            {
                //read new "code" byte if the current one is used up
                if (validBitCount == 0)
                {
                    curCodeByte = src[srcPos];
                    ++srcPos;
                    validBitCount = 8;
                }

                if ((curCodeByte & 0x80) != 0)
                {
                    //straight copy
                    dst[dstPos] = src[srcPos];
                    dstPos++;
                    srcPos++;
                }
                else
                {
                    //RLE part
                    byte byte1 = src[srcPos];
                    byte byte2 = src[srcPos + 1];
                    srcPos += 2;

                    int dist = ((byte1 << 8) | byte2) & 0xFFF;
                    int copySource = dstPos - (dist + 1);

                    int numBytes = byte1 >> 4;
                    if (numBytes == 0)
                    {
                        numBytes = src[srcPos] + 0x12;
                        srcPos++;
                    }
                    else
                        numBytes += 2;

                    //copy run
                    for (int i = 0; i < numBytes; ++i)
                    {
                        dst[dstPos] = dst[copySource];
                        copySource++;
                        dstPos++;
                    }
                }

                //use next bit from "code" byte
                curCodeByte <<= 1;
                validBitCount -= 1;
            }
        }

        /// <summary>
        /// Simple and straight encoding scheme for Yaz
        /// </summary>
        /// <param name="src">Source array</param>
        /// <param name="size"></param>
        /// <param name="pos"></param>
        /// <param name="pMatchPos"></param>
        /// <returns></returns>
        static int SimpleEnc(byte[] src, int size, int pos, ref int matchPos)
        {
            //int matchPos = 0;
            int startPos = Math.Max(pos - 0x1000, 0);
            int numBytes = 1;
            int maxEnc = Math.Min(size - pos, 0xff + 0x12); //limits forward seeking to the maximum number of bytes that can be encoded


            for (int i = startPos; i < pos; i++)
            {
                int j;
                for (j = 0; j < maxEnc; j++)
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
            if (numBytes == 2)
                numBytes = 1;
            return numBytes;
        }

        class SimpleEncodeResult
        {
            public int Length;
            public int matchPos;
            public bool UseResult;
        }

        /// <summary>
        /// a lookahead encoding scheme for Yaz
        /// </summary>
        /// <param name="src"></param>
        /// <param name="size"></param>
        /// <param name="pos"></param>
        /// <param name="matchPos"></param>
        /// <returns></returns>
        static int NintendoEnc(byte[] src, int size, int pos, ref int matchPos, SimpleEncodeResult prev)
        {
            int numBytes = 1;

            // if UseResult is set, it means that the previous position was determined by look-ahead try,
            // so just use it. this is not the best optimization, but nintendo's choice for speed.
            if (prev.UseResult == true)
            {
                matchPos = prev.matchPos;
                prev.matchPos = 0;
                prev.UseResult = false;
                return prev.Length;
            }
            prev.UseResult = false;
            numBytes = SimpleEnc(src, size, pos, ref prev.matchPos); 
            matchPos = prev.matchPos;

            // if this position is RLE encoded, then compare to copying 1 byte and next position(pos+1) encoding
            if (numBytes >= 3)
            {
                prev.Length = SimpleEnc(src, size, pos + 1, ref prev.matchPos);
                // if the next position encoding is +2 longer than current position, choose it.
                // this does not guarantee the best optimization, but fairly good optimization with speed.
                if (prev.Length >= numBytes + 2)
                {
                    numBytes = 1;
                    prev.UseResult = true;
                }
            }
            return numBytes;
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
            int srcPos = 0;
            int dstPos = 0;
            byte[] dst = new byte[24]; // 8 codes * 3 bytes maximum
            int dstSize = 0;

            int validBitCount = 0; //number of valid bits left in "code" byte
            byte curCodeByte = 0;

            int matchPos = 0;

            SimpleEncodeResult var = new SimpleEncodeResult();

            //Write Header
            byte[] srcSizeArr = BitConverter.GetBytes(srcSize);
            byte[] header = new byte[] { 0x59, 0x61, 0x7A, 0x30 }; //Yaz0
            if (BitConverter.IsLittleEndian)
                Array.Reverse(srcSizeArr);

            dstFile.Write(header, 0, 4);
            dstFile.Write(srcSizeArr, 0, 4);
            for (int i = 0; i < 8; i++)
                dstFile.WriteByte(0);

            while (srcPos < srcSize)
            {
                int numBytes = NintendoEnc(src, srcSize, srcPos, ref matchPos, var); //matchPos passed ref &matchpos

                if (numBytes < 3)
                {
                    //straight copy
                    dst[dstPos] = src[srcPos];
                    dstPos++;
                    srcPos++;
                    //set flag for straight copy
                    curCodeByte |= (byte)(0x80 >> validBitCount);
                }
                else
                {
                    //RLE part
                    int dist = srcPos - matchPos - 1;

                    if (numBytes < 0x12) // 2 byte encoding
                    {
                        dst[dstPos++] = (byte)(((numBytes - 2) << 4) | (dist >> 8));
                        dst[dstPos++] = (byte)(dist & 0xff);
                    }
                    else // 3 byte encoding
                    {
                        //write offset
                        dst[dstPos++] = (byte)(dist >> 8);
                        dst[dstPos++] = (byte)(dist & 0xff);
                        //write num bytes, displaced by 0x12. maximum encoding = 0xff + 0x12 bytes
                        dst[dstPos++] = (byte)(numBytes - 0x12);
                    }
                    srcPos += numBytes;
                }
                validBitCount++;
                //write eight codes
                if (validBitCount == 8)
                {
                    dstFile.WriteByte(curCodeByte);
                    dstFile.Write(dst, 0, dstPos);

                    dstSize += dstPos + 1;

                    curCodeByte = 0;
                    validBitCount = 0;
                    dstPos = 0;
                }
            }
            if (validBitCount > 0)
            {
                dstFile.WriteByte(curCodeByte);
                dstFile.Write(dst, 0, dstPos);
                dstSize += dstPos + 1;

                curCodeByte = 0;
                validBitCount = 0;
                dstPos = 0;
            }
            return dstSize;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="srcSize"></param>
        /// <param name="dstFile"></param>
        /// <returns>Size of the file aligned to the nearest 0x10, or -1 if the file can't be compressed</returns>
        public static int Encode(byte[] src, int srcSize, out byte[] dstFile)
        {
            if (srcSize < 0x10)
            {
                dstFile = new byte[0];
                return -1;
            }

            int srcPos = 0;
            int dstPos = 0;
            
            byte[] dst = new byte[24]; // 8 codes * 3 bytes maximum
            int dstSize = 0;
            dstFile = new byte[srcSize];

            int validBitCount = 0; //number of valid bits left in "code" byte
            byte currCodeByte = 0;
            
            int matchPos = 0;

            SimpleEncodeResult prev = new SimpleEncodeResult();

            //Write Header
            byte[] srcSizeArr = BitConverter.GetBytes(srcSize);
            byte[] header = new byte[] { 0x59, 0x61, 0x7A, 0x30 }; //Yaz0
            if (BitConverter.IsLittleEndian)
                Array.Reverse(srcSizeArr);
            
            Array.Copy(header, dstFile, 4);
            Array.Copy(srcSizeArr, 0, dstFile, 4, 4);
            dstSize += 0x10;

            while (srcPos < srcSize)
            {
                int numBytes = NintendoEnc(src, srcSize, srcPos, ref matchPos, prev); //matchPos passed ref &matchpos

                if (numBytes < 3) //one byte copy
                {
                    dst[dstPos] = src[srcPos];
                    dstPos++;
                    srcPos++;
                    //set flag for straight copy
                    currCodeByte |= (byte)(0x80 >> validBitCount);
                }
                else
                {
                    //RLE part
                    int dist = srcPos - matchPos - 1;

                    if (numBytes < 0x12)  // 2 byte encoding
                    {
                        dst[dstPos++] = (byte)(((numBytes - 2) << 4) | (dist >> 8));
                        dst[dstPos++] = (byte)(dist & 0xff);
                    }
                    else  // 3 byte encoding
                    {
                        //write offset
                        dst[dstPos++] = (byte)(dist >> 8);
                        dst[dstPos++] = (byte)(dist & 0xff);
                        //write num bytes, displaced by 0x12. maximum encoding = 0xff + 0x12 bytes
                        dst[dstPos++] = (byte)(numBytes - 0x12);
                    }
                    srcPos += numBytes;
                }
                validBitCount++;
                //write eight codes
                if (validBitCount == 8)
                {
                    int dstSizeNext = dstSize + dstPos + 1;
                    if (dstSizeNext > dstFile.Length)
                        return -1;

                    dstFile[dstSize] = currCodeByte;
                    Array.Copy(dst, 0, dstFile, dstSize + 1, dstPos);
                    dstSize = dstSizeNext;

                    currCodeByte = 0;
                    validBitCount = 0;
                    dstPos = 0;
                }
            }
            if (validBitCount > 0)
            {
                int dstSizeNext = dstSize + dstPos + 1;
                if (dstSizeNext > dstFile.Length)
                    return -1;

                dstFile[dstSize] = currCodeByte;
                Array.Copy(dst, 0, dstFile, dstSize + 1, dstPos);
                dstSize = dstSizeNext;

                currCodeByte = 0;
                validBitCount = 0;
                dstPos = 0;
            }
            return Align.To16(dstSize);
        }

        /// <summary>
        /// Dumps only the code bytes within a compressed file
        /// </summary>
        /// <param name="src">Compressed file</param>
        /// <param name="srcSize">File size</param>
        /// <param name="dstSize">Output encoding data file size</param>
        /// <returns></returns>
        public static byte[] GetEncodingData(byte[] src, int srcSize, out int dstSize)
        {
            int srcPos = 0x10;
            dstSize = 0x00;

            byte[] dst = new byte[srcSize -= 0x10];

            byte curCodeByte = 0;
            int validBitCount = 0;

            while (srcPos < srcSize)
            {
                if (validBitCount == 0)
                {
                    curCodeByte = src[srcPos];
                    validBitCount = 8;
                    dst[dstSize] = curCodeByte;
                    srcPos++;
                    dstSize++;
                }
                if ((curCodeByte & 0x80) != 1)
                {
                    srcPos++;
                }
                else
                {
                    byte temp = src[srcPos];
                    dst[dstSize++] = src[srcPos++];
                    dst[dstSize++] = src[srcPos++];
                    if ((temp & 0xF0) == 0)
                        dst[dstSize++] = src[srcPos++];
                }
                curCodeByte <<= 1;
                validBitCount--;
            }

            return dst;
        }
    }
}