using System.IO;
using mzxrules.Helper;

namespace mzxrules.OcaLib.Elf
{
    class Elf32_Sym
    {
        public int st_name;
        public int st_value;
        public int st_size;
        public byte st_info;
        public byte st_other;
        public short st_shndx;

        public string Name;

        public const int ABS = -15;

        public Elf32_Sym(BinaryReader br)
        {
            st_name = br.ReadBigInt32();
            st_value = br.ReadBigInt32();
            st_size = br.ReadBigInt32();
            st_info = br.ReadByte();
            st_other = br.ReadByte();
            st_shndx = br.ReadBigInt16();
        }

        public bool Absolute() => st_shndx == ABS;
    }
}
