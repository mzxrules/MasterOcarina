using System;
using System.IO;
using System.Linq;

namespace mzxrules.Helper
{
    public class CRC
    {
        public static void Write(string file)
        {
            using (var fs = File.Open(file, FileMode.Open, FileAccess.ReadWrite))
            {
                Write(fs);
            }
        }

        public static void Write(Stream sw)
        {
            uint[] crc = new uint[2];
            byte[] data = new byte[0x00101000];
			
            uint t1, t2, t3, t4, t5, t6 = 0xDF26F436;

            t1 = t2 = t3 = t4 = t5 = t6;

            sw.Position = 0; 
            sw.Read(data, 0, 0x00101000);

            for (int i = 0x00001000; i < 0x00101000; i += 4)
            {
                uint d = (uint)((data[i] << 24) | (data[i + 1] << 16) | (data[i + 2] << 8) | data[i + 3]);
                if ((t6 + d) < t6)
                    t4++;
                t6 += d;
                t3 ^= d;
                uint r = (d << (int)(d & 0x1F)) | (d >> (32 - (int)(d & 0x1F)));
                t5 += r;
                if (t2 > d)
                    t2 ^= r;
                else
                    t2 ^= t6 ^ d;
                t1 += (uint)((data[0x0750 + (i & 0xFF)] << 24) | (data[0x0751 + (i & 0xFF)] << 16) |
                      (data[0x0752 + (i & 0xFF)] << 8) | data[0x0753 + (i & 0xFF)]) ^ d;
            }
            crc[0] = t6 ^ t4 ^ t3;
            crc[1] = t5 ^ t2 ^ t1;

            if (BitConverter.IsLittleEndian)
            {
                crc[0] = Endian.ConvertUInt32(crc[0]);
                crc[1] = Endian.ConvertUInt32(crc[1]);
            }
            
            //Seek to 0x10 from rom start
            sw.Position = 0x10;
            sw.Write(BitConverter.GetBytes(crc[0]), 0, sizeof(uint));
            sw.Write(BitConverter.GetBytes(crc[1]), 0, sizeof(uint));
        }

        public static FileEncoding VerifyRom(Stream rom, ulong targetCrc)
        {
            ulong crc_File;
            using (BinaryReader br = new BinaryReader(rom))
            {
                br.BaseStream.Position = 0x10;
                crc_File = br.ReadUInt64();
                Endian.Convert(ref crc_File);
            }
            //crc_File now contains the crc from file with proper endianness

            //if big endian
            if (targetCrc == crc_File)
                return FileEncoding.BigEndian32;

            //if rom is Little Endian (32 bit)
            if (targetCrc == ConvertToLittleEndian32(crc_File))
            {
                return FileEncoding.LittleEndian32;
            }

            //if rom is Little Endian 16 bit
            else if (targetCrc == ConvertToLittleEndian16(crc_File))
            {
                return FileEncoding.LittleEndian16;
            }
            return FileEncoding.Error;
        }

        private static ulong ConvertToLittleEndian32(ulong crc)
        {
            byte[] crcLeft;
            byte[] crcRight;
            crcLeft = BitConverter.GetBytes((uint)(crc >> 32));
            crcRight = BitConverter.GetBytes((uint)crc);

            crcLeft = crcLeft.Reverse().ToArray();
            crcRight = crcRight.Reverse().ToArray();

            return ((ulong)BitConverter.ToUInt32(crcLeft, 0)) << 32 | BitConverter.ToUInt32(crcRight, 0);
        }

        private static ulong ConvertToLittleEndian16(ulong crc)
        {
            ulong resultCrc = 0;
            for (int i = 3; i >= 0; i--)
            {
                ushort intermediate = Endian.ConvertUShort((ushort)(crc >> (16 * i)));
                resultCrc |= ((ulong)(intermediate)) << (16 * i);

            }
            return resultCrc;
        }
    }
}
