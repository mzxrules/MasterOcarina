using mzxrules.Helper;
using System.IO;

namespace mzxrules.OcaLib.Elf
{
    class SectionHeader
    {
        public enum SHT_
        {
            NULL = 0x0,
            PROGBITS = 0x1,
            SYMTAB = 0x2,
            STRTAB = 0x3,
            RELA = 0x4,
            HASH = 0x5,
            DYNAMIC = 0x6,
            NOTE = 0x7,
            NOBITS = 0x8,
            REL = 0x9,
            SHLIB = 0xA,
            DYNSYM = 0xB,
            INIT_ARRAY = 0xE,
            FINI_ARRAY = 0xF,
            PREINIT_ARRAY = 0x10,
            GROUP = 0x11,
            SYMTAB_SHNDX = 0x12,
            NUM = 0x13
        }

        /// <summary>
        /// Offset to a string in the .shstrtab section that represents the name of this section
        /// </summary>
        public int sh_name;
        /// <summary>
        /// Type
        /// </summary>
        public SHT_ sh_type;
        /// <summary>
        /// Attributes
        /// </summary>
        public int sh_flags;
        /// <summary>
        /// Virtual address of the section in memory, for sections that are loaded.
        /// </summary>
        public int sh_addr;
        /// <summary>
        /// Offset of the section in the file image.
        /// </summary>
        public int sh_offset;
        /// <summary>
        /// Size of the section
        /// </summary>
        public int sh_size;
        public int sh_link;
        public int sh_info;
        public int sh_addralign;
        /// <summary>
        /// Size of an entry for this section
        /// </summary>
        public int sh_entsize;

        #region Custom
        public string Name;
        internal SectionHelper NS;
        #endregion

        public SectionHeader(BinaryReader br)
        {
            sh_name = br.ReadBigInt32();
            sh_type = (SHT_)br.ReadBigInt32();
            sh_flags = br.ReadBigInt32();
            sh_addr = br.ReadBigInt32();
            sh_offset = br.ReadBigInt32();
            sh_size = br.ReadBigInt32();
            sh_link = br.ReadBigInt32();
            sh_info = br.ReadBigInt32();
            sh_addralign = br.ReadBigInt32();
            sh_entsize = br.ReadBigInt32();
        }
    }

    internal class SectionHelper
    {
        public SectionHeader Section;
        public N64Ptr Addr;
        public int FileOff;
        public int NewSecId;
        public int SecOff;

        public SectionHelper(SectionHeader sec, N64Ptr rel, int fileOff, int secId, int secOff)
        {
            Section = sec;
            Addr = rel;
            FileOff = fileOff;
            NewSecId = secId;
            SecOff = secOff;
        }
        public void Deconstruct(out SectionHeader sec, out N64Ptr ptr, out int fOff, out int secId, out int sOff)
        {
            sec = Section;
            ptr = Addr;
            fOff = FileOff;
            secId = NewSecId;
            sOff = SecOff;
        }
    }
}
