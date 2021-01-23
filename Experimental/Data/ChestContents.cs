using mzxrules.Helper;
using mzxrules.OcaLib;
using System;
using System.IO;

namespace Experimental.Data
{
    static partial class Get
    {
        public static void ChestContents(ORom rom)
        {
            //int NTSC10 = 0xBEEE94;
            int NTSC12 = 0xBEEF74;
            //int PAL11 = 0xBEF204;
            int val = NTSC12;
            Get.Table(rom, val - (129 * 6), "chestcontents", DumpChestContents);

            //int tableStart_N0 = 0xBEEE94;
            //FileRecord start;

            //if (rom.Version != Rom.Build.N0)
            //    return;

            //start = rom.Files.GetFileStart(tableStart_N0);

            //if (start == null)
            //    return;

            //using (StreamWriter fs = new StreamWriter(String.Format("{0}-chestcontents.txt", rom.Version)))
            //using (BinaryReader file = new BinaryReader(rom.Files.GetFile(start.VirtualAddress)))
            //{
            //    file.BaseStream.Position = start.GetRelativeAddress(tableStart_N0);
                
            //}
        }
        static void DumpChestContents(ORom rom, BinaryReader file, StreamWriter write)
        {
            for (int i = 0; i < 0x7D +128 +2; i++)
            {
                write.WriteLine(String.Format("{0:X2} {1:X2} {2:X2} {3:X2} {4:X4}",
                    file.ReadByte(),
                    file.ReadByte(),
                    file.ReadByte(),
                    file.ReadByte(),
                    file.ReadBigUInt16()));//Endian.ConvertUShort(file.ReadUInt16())));
            }
        }
    }
}