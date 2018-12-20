using System;
using System.IO;
using System.Linq;

namespace mzxrules.Helper
{
    public class CRC
    {
        public static void Write(string file)
        {
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.ReadWrite))
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
                crc[0] = (crc[0] >> 24) | ((crc[0] >> 8) & 0xFF00) | ((crc[0] << 8) & 0xFF0000) | ((crc[0] << 24) & 0xFF000000);
                crc[1] = (crc[1] >> 24) | ((crc[1] >> 8) & 0xFF00) | ((crc[1] << 8) & 0xFF0000) | ((crc[1] << 24) & 0xFF000000);
            }
            
            //Seek to 0x10 from rom start
            sw.Position = 0x10;
            BinaryWriter br = new BinaryWriter(sw);
            br.Write(crc[0]);
            br.Write(crc[1]);
        }

        public static FileEncoding VerifyRom(Stream rom, UInt64 TargetCrc)
        {
            UInt64 crc_File;
            using (BinaryReader br = new BinaryReader(rom))
            {
                br.BaseStream.Position = 0x10;
                crc_File = br.ReadUInt64();
                Endian.Convert(ref crc_File);
            }
            //crc_File now contains the crc from file with proper endianness

            //if big endian
            if (TargetCrc == crc_File)
                return FileEncoding.BigEndian32;

            //if rom is Little Endian (32 bit)
            if (TargetCrc == ConvertToLittleEndian32(crc_File))
            {
                return FileEncoding.LittleEndian32;
            }

            //if rom is Little Endian 16 bit
            else if (TargetCrc == ConvertToLittleEndian16(crc_File))
            {
                return FileEncoding.LittleEndian16;
            }
            return FileEncoding.Error;
        }

        private static UInt64 ConvertToLittleEndian32(UInt64 crc)
        {
            byte[] crcLeft;
            byte[] crcRight;
            crcLeft = BitConverter.GetBytes((UInt32)(crc >> 32));
            crcRight = BitConverter.GetBytes((UInt32)crc);

            crcLeft = crcLeft.Reverse().ToArray();
            crcRight = crcRight.Reverse().ToArray();

            return (((UInt64)BitConverter.ToUInt32(crcLeft, 0)) << 32 | BitConverter.ToUInt32(crcRight, 0));
        }

        private static UInt64 ConvertToLittleEndian16(UInt64 crc)
        {
            UInt64 resultCrc = 0;
            UInt16 intermediate;
            for (int i = 3; i >= 0; i--)
            {
                intermediate = Endian.ConvertUShort((ushort)(crc >> (16 * i)));
                resultCrc |= ((UInt64)(intermediate)) << (16 * i);

            }
            return resultCrc;
        }
    }
}
