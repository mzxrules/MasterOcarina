using System.IO;
using mzxrules.Helper;

namespace mzxrules.OcaLib.Elf
{
    class FileHeader
    {
        byte[] _elf = { 0x7F, 0x45, 0x4c, 0x46 };
        byte[] Header = new byte[0x10];

        public int e_type;
        public int e_machine;
        public int e_version;
        /// <summary>
        /// Execution Point
        /// </summary>
        public int e_entry;
        public int e_phoff;
        /// <summary>
        /// Start of section header table
        /// </summary>
        public int e_shoff;
        public int e_flags;
        /// <summary>
        /// File Header size
        /// </summary>
        public short e_ehsize;
        /// <summary>
        /// Size of a program header table entry.
        /// </summary>
        public short e_phentsize;
        /// <summary>
        /// Number of entries in the program header table.
        /// </summary>
        public short e_phnum;
        /// <summary>
        /// Size of a section header table entry.
        /// </summary>
        public short e_shentsize;
        /// <summary>
        /// Number of entries in the section header table
        /// </summary>
        public short e_shnum;
        /// <summary>
        /// Index of the section header table entry that contains the section names.
        /// </summary>
        public short e_shstrndx;


        public FileHeader(BinaryReader br)
        {
            Header = br.ReadBytes(0x10);
            if (!VerifyMagic())
            {
                return;
            }
            e_type = br.ReadBigInt16();
            e_machine = br.ReadBigInt16();
            e_version = br.ReadBigInt32();
            e_entry = br.ReadBigInt32();
            e_phoff = br.ReadBigInt32();
            e_shoff = br.ReadBigInt32();
            e_flags = br.ReadBigInt32();
            e_ehsize = br.ReadBigInt16();
            e_phentsize = br.ReadBigInt16();
            e_phnum = br.ReadBigInt16();
            e_shentsize = br.ReadBigInt16();
            e_shnum = br.ReadBigInt16();
            e_shstrndx = br.ReadBigInt16();
        }

        public bool VerifyMagic()
        {
            for (int i = 0; i < 4; i++)
            {
                if (_elf[i] != Header[i])
                    return false;
            }
            if (Header[0x04] != 1 //32 bit format
                || Header[0x05] != 2 //big endian
                || Header[0x06] != 1
                )
            {
                return false;
            }
            return true;
        }
    }
}
