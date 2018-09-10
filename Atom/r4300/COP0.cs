//mipsdis.c
//redistribute and modify at will, my name is spinout, keep it here.
//ported to C# by mzxrules

//cop0
//MIPS R3400i Co-processor 0 (Status processor)

using System;
using static Atom.MipsFields;

namespace Atom
{
    public partial class Disassemble
    {
        static Func<uint, string>[] COP0_T = new Func<uint, string>[32]
        {
            MFC0,   COP0_NONE,  COP0_NONE,  COP0_NONE,  MTC0,       COP0_NONE,  COP0_NONE,  COP0_NONE,
            NONE,   COP0_NONE,  COP0_NONE,  COP0_NONE,  COP0_NONE,  COP0_NONE,  COP0_NONE,  COP0_NONE,
            TLB,    COP0_NONE,  COP0_NONE,  COP0_NONE,  COP0_NONE,  COP0_NONE,  COP0_NONE,  COP0_NONE,
            NONE,   COP0_NONE,  COP0_NONE,  COP0_NONE,  COP0_NONE,  COP0_NONE,  COP0_NONE,  COP0_NONE
        };

        /* TLB op types */
        static Func<uint, string>[] TLB_T = new Func<uint, string>[64]
        {
            TLB_NONE,   TLBR,       TLBWI,      TLB_NONE,   TLB_NONE,   TLB_NONE,   TLBWR,      TLB_NONE,
            TLBP,       TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,
            TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,
            ERET,       TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,
            TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,
            TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,
            TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,
            TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE,   TLB_NONE
        };

        /* COP0 op types */
        static string MFC0(uint iw)
        {       /* 00 */
            return $"mfc0\t{gpr_rn[RT(iw)]}, {cop_rn[FS(iw)]}";
        }

        static string MTC0(uint iw)
        {       /* 04 */
            return $"mtc0\t{gpr_rn[RT(iw)]}, {cop_rn[FS(iw)]}";
        }

        static string TLB(uint iw)
        {
            return TLB_T[iw & 63](iw);
        }

        static string TLBR(uint iw)
        {
            return "tlbr";
        }

        static string TLBWI(uint iw)
        {
            return "tlbwi";
        }

        static string TLBWR(uint iw)
        {
            return "tlbwr";
        }

        static string TLBP(uint iw)
        {
            return "tlbp";
        }

        static string ERET(uint iw)
        {
            return "eret";
        }

        static string TLB_NONE(uint iw)
        {
            return $"(Invalid TLB: {iw:X8})";
        }

        static string COP0_NONE(uint iw)
        {
            return $"(Invalid COP0: {iw:X8})";
        }
    }
}
