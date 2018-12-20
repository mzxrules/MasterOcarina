using mzxrules.Helper;

using System.Collections.Generic;
using System.IO;

namespace mzxrules.OcaLib
{
    public enum Reloc : byte
    {
        R_MIPS32 = 2,
        R_MIPS26 = 4,
        R_MIPS_HI16 = 5,
        R_MIPS_LO16 = 6,
    }

    public class Overlay
    {
        public int TextSize;
        public int DataSize;
        public int RodataSize;
        public int BssSize;
        public int RelCount;
        
        public List<RelocationWord> Relocations = new List<RelocationWord>();

        public Overlay() { }

        public Overlay(BinaryReader br)
        {
            //Get Header
            int physicalSize = (int)br.BaseStream.Length;

            int size = physicalSize;
            br.BaseStream.Position = size - 4;

            int header_inset = br.ReadBigInt32();
            br.BaseStream.Position = size - header_inset;

            TextSize = br.ReadBigInt32();
            DataSize = br.ReadBigInt32();
            RodataSize = br.ReadBigInt32();
            BssSize = br.ReadBigInt32();
            RelCount = br.ReadBigInt32();

            int virtualSize = physicalSize + BssSize;

            for (int i = 0; i < RelCount; i++)
            {
                var rel = new RelocationWord(this, br.ReadBigInt32());
                Relocations.Add(rel);
            }
        }

        public int GetRelocationOffset(RelocationWord item)
        {
            int off = item.SectionOffset;
            switch (item.SectionId)
            {
                case Section.data:   off += TextSize; break;
                case Section.rodata: off += TextSize + DataSize; break;
                case Section.bss:    off += TextSize + DataSize + RodataSize; break;
            }

            return off;
        }
        
        public struct RelocationWord
        {
            public int Word;
            public Section SectionId { get { return (Section)Shift.AsByte((uint)Word, 0xC0000000); } }
            public Reloc RelocType { get { return (Reloc)Shift.AsByte((uint)Word, 0x3F000000); } }
            public int SectionOffset { get { return Word & 0xFFFFFF; } }
            public int Offset { get { return Parent.GetRelocationOffset(this); } }
            private Overlay Parent;

            public RelocationWord(Overlay parent, int word)
            {
                Parent = parent;
                Word = word;
            }
            public RelocationWord(Section secId, Reloc r, int offset)
            {
                var secL = Shift.GetRight(0xC0000000);
                var relL = Shift.GetRight(0x3F000000);

                Word = ((int)secId << secL) | ((int)r << relL) | offset;
                Parent = null;
            }

            public override string ToString()
            {
                return $"{SectionOffset:X6}: {SectionId} {RelocType}";
            }
        }

        public enum Section : byte
        {
            text = 1,
            data = 2,
            rodata = 3,
            bss = 4,
        }
    }
}
