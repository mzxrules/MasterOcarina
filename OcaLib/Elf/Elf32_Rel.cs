using System.IO;
using mzxrules.Helper;

namespace mzxrules.OcaLib.Elf
{
    /// <summary>
    /// Relocation Entries
    /// </summary>
    struct Elf32_Rel
    {
        public int r_offset; //offset relative to section to apply relocation
        public int r_info; //

        public Reloc R_Type => (Reloc)r_info;
        public int R_Sym => r_info >> 8;

        public Elf32_Rel(BinaryReader br)
        {
            r_offset = br.ReadBigInt32();
            r_info = br.ReadBigInt32();
        }
    }
}
