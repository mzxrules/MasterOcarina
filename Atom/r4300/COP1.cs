//mipsdis.c
//redistribute and modify at will, my name is spinout, keep it here.
//ported to C# by mzxrules

//cop1
//MIPS R3400i Co-processor 1 (FPU processor)

using System;
using static Atom.MipsFields;

namespace Atom
{
    public partial class Disassemble
    {
        static Func<uint, string>[] COP1_T = new Func<uint, string>[32]
        {
            MFC1,       DMFC1,      CFC1,       COP1_NONE,
            MTC1,       DMTC1,      CTC1,       COP1_NONE,
            BC,         COP1_NONE,  COP1_NONE,  COP1_NONE,  COP1_NONE,  COP1_NONE,  COP1_NONE,  COP1_NONE,
            S,          D,          COP1_NONE,  COP1_NONE,
            W,          L,          COP1_NONE,  COP1_NONE,
            COP1_NONE,  COP1_NONE,  COP1_NONE,  COP1_NONE,  COP1_NONE,  COP1_NONE,  COP1_NONE,  COP1_NONE
        };

        #region /* S */
        //function (& 0x63)
        static Func<uint,char,string>[] S_T = new Func<uint,char,string>[]
        {
            ADD,  SUB,  MUL,  DIV,  SQRT, ABS,  MOV,  NEG,
            ROUND_L,  TRUNC_L,  CEIL_L,   FLOOR_L,  ROUND_W,  TRUNC_W,  CEIL_W,   FLOOR_W,
            COP1_NONE,  COP1_NONE,  COP1_NONE,  COP1_NONE,  COP1_NONE,  COP1_NONE,  COP1_NONE,  COP1_NONE,
            COP1_NONE,  COP1_NONE,  COP1_NONE,  COP1_NONE,  COP1_NONE,  COP1_NONE,  COP1_NONE,  COP1_NONE,
            CVT_S,  CVT_D,    COP1_NONE,  COP1_NONE,  CVT_W,    CVT_L,    COP1_NONE,  COP1_NONE,
            COP1_NONE,  COP1_NONE,  COP1_NONE,  COP1_NONE,  COP1_NONE,  COP1_NONE,  COP1_NONE,  COP1_NONE,
            C_F,  C_UN, C_EQ, C_UEQ,    C_OLT,    C_ULT,    C_OLE,    C_ULE,
            C_SF, C_NGLE,   C_SEQ,    C_NGL,    C_LT, C_NGE,    C_LE, C_NGT
        };




        //COP1.L
        static string CVT_S_L(uint iw)
        {
            return $"cvt.s.l\t{fd_fs(iw)}";
        }

        static string CVT_D_L(uint iw)
        {
            return $"cvt.d.l\t{fd_fs(iw)}";
        }

        //COP1.W
        static string CVT_S_W(uint iw)
        {
            return $"cvt.s.w\t{fd_fs(iw)}";
        }

        static string CVT_D_W(uint iw)
        {
            return $"cvt.d.w\t{fd_fs(iw)}";
        }

        //COP1.S
        static string ADD(uint iw, char t)
        {   //00 (00)
            return $"add.{t}\t{fd_fs_ft(iw)}";
        }

        static string SUB(uint iw, char t)
        {   //01 (01)
            return $"sub.{t}\t{fd_fs_ft(iw)}";
        }

        static string MUL(uint iw, char t)
        {   //02 (02)
            return $"mul.{t}\t{fd_fs_ft(iw)}";
        }

        static string  DIV(uint iw, char t)
        {   //03 (03)
            return $"div.{t}\t{fd_fs_ft(iw)}";
        }

        static string SQRT(uint iw, char t)
        {   //04 (04)
            return $"sqrt.{t}\t{fd_fs(iw)}";
        }

        static string ABS(uint iw, char t)
        {   //05 (05)
            return $"abs.{t}\t{fd_fs(iw)}";
        }

        static string MOV(uint iw, char t)
        {   //06 (06)
            return $"mov.{t}\t{fd_fs(iw)}";
        }

        static string NEG(uint iw, char t)
        {   //07 (07)
            return $"neg.{t}\t{fd_fs(iw)}";
        }

        static string ROUND_L(uint iw, char t)
        {   //08 (08)
            return $"round.l.{t}\t{fd_fs(iw)}";
        }

        static string TRUNC_L(uint iw, char t)
        {   //09 (09)
            return $"trunc.l.{t}\t{fd_fs(iw)}";
        }

        static string CEIL_L(uint iw, char t)
        {   //10 (0A)
            return $"ceil.l.{t}\t{fd_fs(iw)}";
        }

        static string FLOOR_L(uint iw, char t)
        {   //11 (0B)
            return $"floor.l.{t}\t{fd_fs(iw)}";
        }

        static string ROUND_W(uint iw, char t)
        {   //12 (0C)
            return $"round.w.{t}\t{fd_fs(iw)}";
        }

        static string TRUNC_W(uint iw, char t)
        {   //13 (0D)
            return $"trunc.w.{t}\t{fd_fs(iw)}";
        }

        static string CEIL_W(uint iw, char t)
        {   //14 (0E)
            return $"ceil.w.{t}\t{fd_fs(iw)}";
        }

        static string FLOOR_W(uint iw, char t)
        {   //15 (0F)
            return $"floor.w.{t}\t{fd_fs(iw)}";
        }


        static string CVT_S(uint iw, char t)
        {   //32 (20)
            if (t == 's')
                return COP1_NONE(iw, t);
            return $"cvt.s.{t}\t{fd_fs(iw)}";
        }

        static string CVT_D(uint iw, char t)
        {   //33 (21)
            if (t == 'd')
                return COP1_NONE(iw, t);
            return $"cvt.d.{t}\t{fd_fs(iw)}";
        }

        static string CVT_W(uint iw, char t)
        {   //36 (24)
            return $"cvt.w.{t}\t{fd_fs(iw)}";
        }

        static string CVT_L(uint iw, char t)
        {   //37 (25)
            return $"cvt.l.{t}\t{fd_fs(iw)}";
        }

        static string C_F(uint iw, char t)
        {   //48 (30)
            return $"c.f.{t}\t{fs_ft(iw)}";
        }

        static string C_UN(uint iw, char t)
        {   //49 (31)
            return $"c.un.{t}\t{fs_ft(iw)}";
        }

        static string C_EQ(uint iw, char t)
        {   //50 (32)
            return $"c.eq.{t}\t{fs_ft(iw)}";
        }

        static string C_UEQ(uint iw, char t)
        {   //51 (33)
            return $"c.ueq.{t}\t{fs_ft(iw)}";
        }

        static string C_OLT(uint iw, char t)
        {   //52 (34)
            return $"c.olt.{t}\t{fs_ft(iw)}";
        }

        static string C_ULT(uint iw, char t)
        {   //53 (35)
            return $"c.ult.{t}\t{fs_ft(iw)}";
        }

        static string C_OLE(uint iw, char t)
        {   //54 (36)
            return $"c.ole.{t}\t{fs_ft(iw)}";
        }

        static string C_ULE(uint iw, char t)
        {   //55 (37)
            return $"c.ule.{t}\t{fs_ft(iw)}";
        }

        static string C_SF(uint iw, char t)
        {   //56 (38)
            return $"c.sf.{t}\t{fs_ft(iw)}";
        }

        static string C_NGLE(uint iw, char t)
        {   //57 (39)
            return $"c.ngle.{t}\t{fs_ft(iw)}";
        }

        static string C_SEQ(uint iw, char t)
        {   //58 (3A)
            return $"c.seq.{t}\t{fs_ft(iw)}";
        }

        static string C_NGL(uint iw, char t)
        {   //59 (3B)
            return $"c.ngl.{t}\t{fs_ft(iw)}";
        }

        static string C_LT(uint iw, char t)
        {   //60 (3C)
            return $"c.lt.{t}\t{fs_ft(iw)}";
        }

        static string C_NGE(uint iw, char t)
        {   //61 (3D)
            return $"c.nge.{t}\t{fs_ft(iw)}";
        }

        static string C_LE(uint iw, char t)
        {   //62 (3E)
            return $"c.le.{t}\t{fs_ft(iw)}";
        }

        static string C_NGT(uint iw, char t)
        {   //63 (3F)
            return $"c.ngt.{t}\t{fs_ft(iw)}";
        }

        #endregion

        #region /* BC */
        //COP1.BC opcodes
        
        static Func<uint, string>[] BC_T = new Func<uint, string>[4]
                { BC1F, BC1T, BC1FL, BC1TL };
        
        static string BC1F(uint iw)
        {
            return $"bc1f\t{GetBranchLabel(iw, pc)}";
        }

        static string BC1T(uint iw)
        {
            return $"bc1t\t{GetBranchLabel(iw, pc)}";
        }

        static string BC1FL(uint iw)
        {
            return $"bc1fl\t{GetBranchLabel(iw, pc)}";
        }

        static string BC1TL(uint iw)
        {
            return $"bc1tl\t{GetBranchLabel(iw, pc)}";
        }
        #endregion

        //COP1 op types

        static string MFC1(uint iw)
        {       //00 (00)
            return $"mfc1\t{rt_fs(iw)}";
        }

        static string DMFC1(uint iw)
        {   //01 (01)
            return $"dmfc1\t{rt_fs(iw)}";
        }

        static string CFC1(uint iw)
        {       //02 (02)
            return $"cfc1\t{rt_fs(iw)}";
        }

        static string MTC1(uint iw)
        {       //04 (04)
                //float* value = &gpr_regs[getRT(iw)];
            int intermediate = gpr_regs[RT(iw)];
            float value = BitConverter.ToSingle(BitConverter.GetBytes(intermediate), 0);

            return $"mtc1\t{rt_fs(iw)}\t## {fpr_rn[FS(iw)]} = {value:F}";
        }

        static string DMTC1(uint iw)
        {   //05 (05)
            return $"dmtc1\t{rt_fs(iw)}";
        }

        static string CTC1(uint iw)
        {       //06 (06)
            return $"ctc1\t{rt_fs(iw)}";
        }

        static string BC(uint iw)
        {       //08 (08)
            return BC_T[FT(iw) & 3](iw);
        }

        static string S(uint iw)
        {       //16 (10)
            return S_T[iw & 63](iw, 's');
        }

        static string D(uint iw)
        {       //17 (11)
            //D_T[iw & 63](iw);
            return S_T[iw & 63](iw, 'd');
        }

        static string W(uint iw)
        {       //20 (14)
            if (!((iw & 0x1E) != 0))
            {
                if ((iw & 1) != 0)
                    return CVT_D_W(iw);
                else
                    return CVT_S_W(iw);
            }
            else
                return COP1_NONE(iw);
        }

        static string L(uint iw)
        {       //21 (15)
            if (!((iw & 0x1E) != 0))
            {
                if ((iw & 1) != 0)
                    return CVT_D_L(iw);
                else
                    return CVT_S_L(iw);
            }
            else
                return COP1_NONE(iw);
        }

        static string COP1_NONE(uint iw)
        {
            return $"(Invalid COP1: {iw:X8})";
        }
        static string COP1_NONE(uint iw, char t)
        {
            return COP1_NONE(iw);
        }

    }
}
