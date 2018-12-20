// Yaz (de)compression algorithm was found on http://www.amnoid.de and ported to C#
 
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
//using System.Runtime.CompilerServices;


namespace mzxrules.Helper
{
    public class Yaz0
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

                    UInt32 dist = (UInt32)((byte1 & 0xF) << 8) | byte2;
                    UInt32 copySource = (UInt32)dstPlace - (dist + 1);

                    UInt32 numBytes = (UInt32)byte1 >> 4;
                    if (numBytes == 0)
                    {
                        numBytes = (UInt32)src[srcPlace] + 0x12;
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
        const int SEEKBACK_SEARCH = 0x1000;

        /// <summary>
        /// Simple and straight encoding scheme for Yaz
        /// </summary>
        /// <param name="src"></param>
        /// <param name="size"></param>
        /// <param name="pos"></param>
        /// <param name="pMatchPos"></param>
        /// <returns></returns>
        
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int SimpleEnc_W(byte[] src, int size, int pos, out int matchPos)
        {
            int Abytes = SimpleEnc_N(src, size, pos, out int Am);
            uint Bbytes = SimpleEnc(src, size, pos, out uint Bm);

            if (Abytes != Bbytes)
            {
                Console.WriteLine($"{Abytes} pos {Am:X8}; {Bbytes} {Bm:X8}");
            }
            matchPos = (int)Bm;
            return (int)Bbytes;
        }

        static UInt32 SimpleEnc(byte[] src, int size, int pos, out uint /* u32* */ pMatchPos)
        //u32 simpleEnc(u8* src, int size, int pos, u32 *pMatchPos)
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

        static int SimpleEnc_N(byte[] src, int size, int pos, out int matchPos)
        {
            int startPos = pos - SEEKBACK_SEARCH;
            int numBytes = 1;
            matchPos = 0;
            int maxEnc = size - pos; //Replaces J loop terminating condition

            if (maxEnc < 3)
                return 1;

            if (startPos < 0)
            {
                startPos = 0;
            }

            //limits forward seeking to the maximum number of bytes that can be encoded
            if (maxEnc > 0xff + 0x12)
            {
                maxEnc = 0xff + 0x12;
            }

            //we're only interested in encoding 3 bytes or more
            //int end = pos - 2; 
            for (int i = startPos; i < pos; i++)
            {
                if ((src[i] != src[pos])
                    || (src[i + 1] != src[1 + pos]))
                {
                    continue;
                }

                int j;

                for (j = 2; j < maxEnc; j++)
                {
                    if (src[i + j] != src[j + pos])
                        break;
                }
                if (j > numBytes)
                {
                    numBytes = j;
                    matchPos = i;
                    //end = pos - numBytes;
                }
            }
            //Profile(numBytes);
            //if (numBytes == 2)
            //    numBytes = 1;
            return numBytes;
        }
        

        static List<double> avg10000 = new List<double>();
        static List<int> avg = new List<int>(100);

        static void Profile(int i)
        {
            avg.Add(i);
            if (avg.Count < 100)
                return;
            avg10000.Add(avg.Average());
            avg.Clear();
            if (avg10000.Count < 100)
                return;
            Console.WriteLine(avg10000.Average());
            avg10000.Clear();
        }

        struct SimpleEncodeResult
        {
            public bool flag;
            public int numBytes;
            public int matchPos;
        }

        /// <summary>
        /// a lookahead encoding scheme for ngc Yaz
        /// </summary>
        /// <param name="src"></param>
        /// <param name="size"></param>
        /// <param name="pos"></param>
        /// <param name="matchPos"></param>
        /// <returns></returns>

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int NintendoEnc(byte[] src, int size, int pos, out int matchPos, SimpleEncodeResult prev)
        {
            // if prevFlag is set, it means that the previous position was determined by look-ahead try.
            // so just use it. this is not the best optimization, but nintendo's choice for speed.
            if (prev.flag == true)
            {
                matchPos = prev.matchPos; 
                prev.flag = false;
                return prev.numBytes;
            }
            int numBytes = SimpleEnc_N(src, size, pos, out matchPos); 

            // if this position is RLE encoded, then compare to copying 1 byte and next position(pos+1) encoding
            if (numBytes >= 3)
            {
                prev.numBytes = SimpleEnc_N(src, size, pos + 1, out prev.matchPos); 
                // if the next position encoding is +2 longer than current position, choose it.
                // this does not guarantee the best optimization, but fairly good optimization with speed.
                if (prev.numBytes >= numBytes + 2)
                {
                    numBytes = 1;
                    prev.flag = true;
                }
                return numBytes;
            }
            //SimpleEnc sets numBytes to 1 if < 3
            return 1;  
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
        /// <returns>Size in bytes of the compressed file, including header</returns>
        public static int Encode(byte[] src, int srcSize, Stream dstFile)
        {
            Ret r = new Ret(0, 0);
            byte[] dst = new byte[24]; // 8 codes * 3 bytes maximum
            int dstSize = 0;

            uint validBitCount = 0; //number of valid bits left in "code" byte
            byte currCodeByte = 0;
            
            SimpleEncodeResult prev = new SimpleEncodeResult();

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
                int numBytes = NintendoEnc(src, srcSize, r.srcPos, out int matchPos, prev); 
                
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
                    int dist = r.srcPos - matchPos - 1;
                    byte byte1, byte2, byte3;

                    if (numBytes >= 0x12)  // 3 byte encoding
                    {
                        byte1 = (byte)(0 | (dist >> 8));
                        byte2 = (byte)(dist & 0xff);
                        dst[r.dstPos++] = byte1;
                        dst[r.dstPos++] = byte2;
                        // maximum runlength for 3 byte encoding
                        //if (numBytes > 0xff + 0x12)
                        //    numBytes = 0xff + 0x12;
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
                    r.srcPos += numBytes;
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
            return dstSize + 0x10;
        }
    }
}
