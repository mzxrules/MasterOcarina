using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace mzxrules.OcaLib
{
    public static class ImageHelper
    {
        const int TITLE_MASK_SIZE = 192 * 192 / 2;      //192x192, 4 bit depth
        const int TITLE_MASK_WIDTH = 192 / 2;           //192 pix, 4 bit depth
        const int TITLE_MASK_TILE_SIZE = 64 * 64 / 2;   //64x64, 4 bit depth
        const int TITLE_MASK_TILE_WIDTH = 64 / 2;       //64 pix, 4 bit depth

        /// <summary>
        /// Converts 192x192 I4 mask to 9 64x64 contiguous I4 files
        /// </summary>
        /// <param name="br"></param>
        /// <returns></returns>
        public static byte[] ConvertTitleMask(BinaryReader br)
        {
            int cur;            //offset into result arr
            byte[] original;
            byte[] result;
            if (br.BaseStream.Length != TITLE_MASK_SIZE)
                throw new ArgumentException("File too big");

            original = br.ReadBytes(TITLE_MASK_SIZE);
            result = new byte[TITLE_MASK_SIZE];

            cur = 0;
            for (int i = 0; i < 9; i++)
            {
                ConvertTitleMaskTile(original, result, i, cur);
                cur += TITLE_MASK_TILE_SIZE;
            }


            return result;
        }

        private static void ConvertTitleMaskTile(byte[] original, byte[] result, int num, int cur)
        {
            int scanlineOff;
            int x, y;
            int xOff, yOff;

            x = num % 3;
            y = num / 3;

            xOff = x * TITLE_MASK_TILE_WIDTH;
            yOff = TITLE_MASK_TILE_SIZE * 3 * y;

            for (int i = 0; i < 64; i++)
            {
                scanlineOff = xOff + yOff + (i * TITLE_MASK_WIDTH);
                Array.Copy(original, scanlineOff,
                    result, i * TITLE_MASK_TILE_WIDTH + cur, //iteration * scanline size + offset
                    TITLE_MASK_TILE_WIDTH);
            }
        }
        public static byte[] ConvertRGBAToIA4(BinaryReader br)
        {
            byte[] arr;
            byte[] arrOut = new byte[0x1000/2];

            arr = br.ReadBytes(0x1000);

            for (int i = 0; i < arrOut.Length; i++)
            {
                arrOut[i] = (byte)((arr[i * 2] & 0xF0) | (arr[i * 2 + 1] & 0x0F));
            }

            return arrOut;
        }


        /// <summary>
        /// I don't think this ever worked.
        /// </summary>
        /// <param name="br"></param>
        /// <returns></returns>
        public static List<Byte[]> GetRawRGBA(BinaryReader br)
        {
            int width = 0;
            int height = 0;
            int cur = 0;
            int chunkLength;
            string chunkName;
            bool skipFirstByte = true;
            bool end = false;
            List<byte[]> result = new List<byte[]>();

            //skip header
            br.ReadBytes(8);

            while (!end)
            {
                chunkLength = BitConverter.ToInt32(br.ReadBytes(4).Reverse().ToArray(), 0);
                chunkName = new string(br.ReadChars(4));
                switch (chunkName)
                {
                    case "IHDR":
                        width = BitConverter.ToInt32(br.ReadBytes(4).Reverse().ToArray(), 0);
                        height = BitConverter.ToInt32(br.ReadBytes(4).Reverse().ToArray(), 0);
                        //result = new byte[width * height * 4];
                        br.BaseStream.Position += chunkLength - 4;
                        break;
                    case "IDAT":
                        result.AddRange(ReadIdat(br.ReadBytes(chunkLength), ref cur, width, height, ref skipFirstByte));//result.Add(br.ReadBytes(chunkLength));
                        br.ReadInt32();
                        break;
                    case "IEND": end = true; break;
                    default:
                        br.BaseStream.Position += chunkLength + 4;
                        break;
                }
            }
            return result;
        }

        private static List<byte[]> ReadIdat(byte[] p, ref int cur, int width, int height, ref bool skipFirstByte)
        {
            List<byte[]> result = new List<byte[]>();
            byte[] arr;
            int index = 0;
            int truncLineLength;

            width *= 4;

            if (skipFirstByte)
            {
                index++;
            }
            skipFirstByte = false;

            if (cur % width != 0)
            {
                truncLineLength = width - (cur % width);
                arr = new byte[truncLineLength];
                Array.Copy(p, index, arr, 0, truncLineLength);
                result.Add(arr);
                index += truncLineLength + 1;//skip next line's scanline byte
                cur += truncLineLength;

                //may have been last line to store
                if (cur == width * height)
                    return result;
            }

            //start of new scanline of data

            while (true)
            {
                if (index + width <= p.Length)
                {
                    arr = new byte[width];
                    Array.Copy(p, index, arr, 0, width);
                    result.Add(arr);
                    index += width;
                    cur += width;
                }
                else
                {
                    truncLineLength = p.Length - index;

                    arr = new byte[truncLineLength];
                    Array.Copy(p, index, arr, 0, truncLineLength);
                    result.Add(arr);
                    index += truncLineLength;
                    cur += truncLineLength;
                    break;
                }
                if (index == p.Length)
                {
                    skipFirstByte = true;
                    break;
                }
                else
                    index++;
            }

            return result;
        }
    }
}
