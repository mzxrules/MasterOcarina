//mipsdis.c
//redistribute and modify at will, my name is spinout, keep it here.
//ported to C# by mzxrules

//Main Processor
using System;
using static Atom.MipsFields;

namespace Atom
{
    public partial class Disassemble
    {
        #region Main op types

        static Func<uint, string>[] MAIN_T = new Func<uint, string>[64]
        {
            SPECIAL,REGIMM, J,      JAL,    BEQ,    BNE,    BLEZ,   BGTZ,
            ADDI,   ADDIU,  SLTI,   SLTIU,  ANDI,   ORI,    XORI,   LUI,
            COP0,   COP1,   NONE,   NONE,   BEQL,   BNEL,   BLEZL,  BGTZL,
            DADDI,  DADDIU, LDL,    LDR,    NONE,   NONE,   NONE,   NONE,
            LB,     LH,     LWL,    LW,     LBU,    LHU,    LWR,    LWU,
            SB,     SH,     SWL,    SW,     SDL,    SDR,    SWR,    CACHE,
            LL,     LWC1,   NONE,   NONE,   LLD,    LDC1,   LDC2,   LD,
            SC,     SWC1,   NONE,   NONE,   SCD,    SDC1,   SDC2,   SD
        };

        static string SPECIAL(uint iw) => SPECIAL_T[iw & 63](iw);

        static string REGIMM(uint iw) => REGIMM_T[RT(iw)](iw);

        static string J(uint iw) => PrintJump(iw, "j");

        static string JAL(uint iw)
        {       //03(03)
            jaltaken = 2;
            return PrintJump(iw, "jal");
        }

        static string BEQ(uint iw) => $"beq\t{rs_rt_branch(iw, pc)}";

        static string BNE(uint iw) => $"bne\t{rs_rt_branch(iw, pc)}";

        static string BLEZ(uint iw) => $"blez\t{rs_branch(iw, pc)}";

        static string BGTZ(uint iw) => $"bgtz\t{rs_branch(iw, pc)}";

        static string ADDI(uint iw)
        {       //08(08)
            gpr_regs[RT(iw)] = (gpr_regs[RS(iw)] + IMM(iw));
            return $"addi\t{rt_rs_imm(iw)}\t## {gpr_rn[RT(iw)]} = {gpr_regs[RT(iw)]:X8}";
        }

        static string ADDIU(uint iw)
        {   //09(09)
            gpr_regs[RT(iw)] = (gpr_regs[RS(iw)] + IMM(iw));
            return $"addiu\t{rt_rs_imm(iw)}\t## {gpr_rn[RT(iw)]} = {gpr_regs[RT(iw)]:X8}";
        }

        static string SLTI(uint iw) => $"slti\t{rt_rs_imm(iw)}";

        static string SLTIU(uint iw) => $"sltiu\t{rt_rs_imm(iw)}";

        static string ANDI(uint iw)
        {       //12(0C)
            gpr_regs[RT(iw)] = gpr_regs[RS(iw)] & (IMM(iw) & 0xFFFF);
            return $"andi\t{rt_rs_imm(iw)}\t## {gpr_rn[RT(iw)]} = {gpr_regs[RT(iw)]:X8}";
        }

        static string ORI(uint iw)
        {       //13(0D)
            gpr_regs[RT(iw)] = gpr_regs[RS(iw)] | (IMM(iw) & 0xFFFF);
            return $"ori\t{rt_rs_imm(iw)}\t## {gpr_rn[RT(iw)]} = {gpr_regs[RT(iw)]:X8}";
        }

        static string XORI(uint iw)
        {       //14(0E)
            gpr_regs[RT(iw)] = gpr_regs[RS(iw)] ^ IMM(iw);
            return $"xori\t{rt_rs_imm(iw)}\t## {gpr_rn[RT(iw)]} = {gpr_regs[RT(iw)]:X8}";
        }

        static string LUI(uint iw)
        {       //15(0F)
            gpr_regs[RT(iw)] = IMM(iw) << 16;
            return $"lui\t{rt_imm(iw)}\t## {gpr_rn[RT(iw)]} = {gpr_regs[RT(iw)]:X8}";
        }

        static string COP0(uint iw) => COP0_T[RS(iw)](iw);

        static string COP1(uint iw) => COP1_T[RS(iw)](iw);

        static string BEQL(uint iw) => $"beql\t{rs_rt_branch(iw, pc)}";

        static string BNEL(uint iw) => $"bnel\t{rs_rt_branch(iw, pc)}";

        static string BLEZL(uint iw) => $"blezl\t{rs_branch(iw, pc)}";

        static string BGTZL(uint iw) => $"bgtzl\t{rs_branch(iw, pc)}";

        static string DADDI(uint iw) => $"daddi\t{rt_rs_imm(iw)}";

        static string DADDIU(uint iw) => $"daddiu\t{rt_rs_imm(iw)}";

        static string LDL(uint iw) => $"ldl\t{rt_base_imm(iw)}";

        static string LDR(uint iw) => $"ldr\t{rt_base_imm(iw)}";

        static string LB(uint iw) => $"lb\t{rt_base_imm(iw)}";

        static string LH(uint iw) => $"lh\t{rt_base_imm(iw)}";

        static string LWL(uint iw) => $"lwl\t{rt_base_imm(iw)}";

        static string LW(uint iw) => $"lw\t{rt_base_imm(iw)}";

        static string LBU(uint iw) => $"lbu\t{rt_base_imm(iw)}";

        static string LHU(uint iw) => $"lhu\t{rt_base_imm(iw)}";

        static string LWR(uint iw) => $"lwr\t{rt_base_imm(iw)}";

        static string LWU(uint iw) => $"lwu\t{rt_base_imm(iw)}";

        static string SB(uint iw) => $"sb\t{rt_base_imm(iw)}";

        static string SH(uint iw) => $"sh\t{rt_base_imm(iw)}";

        static string SWL(uint iw) => $"swl\t{rt_base_imm(iw)}";

        static string SW(uint iw) => $"sw\t{rt_base_imm(iw)}";

        static string SDL(uint iw) => $"sdl\t{rt_base_imm(iw)}";

        static string SDR(uint iw) => $"sdr\t{rt_base_imm(iw)}";

        static string SWR(uint iw) => $"swr\t{rt_base_imm(iw)}";

        static string CACHE(uint iw) => $"cache\t0x{OP(iw):X2}, {IMM_P(iw)}({gpr_rn[BASE(iw)]})";

        static string LL(uint iw) => $"ll\t{rt_base_imm(iw)}";

        static string LWC1(uint iw) => $"lwc1\t{ft_base_imm(iw)}";

        static string LLD(uint iw) => $"lld\t{rt_base_imm(iw)}";

        static string LDC1(uint iw) => $"ldc1\t{ft_base_imm(iw)}";

        static string LDC2(uint iw) => NONE(iw);//return $"ldc2\t{rt_base_imm(iw)}";

        static string LD(uint iw) => $"ld\t{rt_base_imm(iw)}";

        static string SC(uint iw) => $"sc\t{rt_base_imm(iw)}";

        static string SWC1(uint iw) => $"swc1\t{ft_base_imm(iw)}";

        static string SCD(uint iw) => $"sdc\t{rt_base_imm(iw)}";

        static string SDC1(uint iw) => $"sdc1\t{ft_base_imm(iw)}";

        static string SDC2(uint iw) => NONE(iw);//return $"sdc2\t{rt_base_imm(iw)}";

        static string SD(uint iw) => $"sd\t{rt_base_imm(iw)}";

        #endregion

        #region SPECIAL op types

        static Func<uint, string>[] SPECIAL_T = new Func<uint, string>[64]
        {
            SLL,    NONE,   SRL,    SRA,    SLLV,   NONE,   SRLV,   SRAV,
            JR,     JALR,   NONE,   NONE,   SYSCALL,BREAK,  NONE,   SYNC,
            MFHI,   MTHI,   MFLO,   MTLO,   DSLLV,  NONE,   DSRLV,  DSRAV,
            MULT,   MULTU,  DIV,    DIVU,   DMULT,  DMULTU, DDIV,   DDIVU,
            ADD,    ADDU,   SUB,    SUBU,   AND,    OR,     XOR,    NOR,
            NONE,   NONE,   SLT,    SLTU,   DADD,   DADDU,  DSUB,   DSUBU,
            TGE,    TGEU,   TLT,    TLTU,   TEQ,    NONE,   TNE,    NONE,
            DSLL,   NONE,   DSRL,   DSRA,   DSLL32, NONE,   DSRL32, DSRA32
        };


        static string SLL(uint iw) => $"sll\t{rd_rt_sa(iw)}";

        static string SRL(uint iw) => $"srl\t{rd_rt_sa(iw)}";

        static string SRA(uint iw) => $"sra\t{rd_rt_sa(iw)}";

        static string SLLV(uint iw) => $"sllv\t{rd_rt_rs(iw)}";

        static string SRLV(uint iw) => $"srlv\t{rd_rt_rs(iw)}";

        static string SRAV(uint iw) => $"srav\t{rd_rt_rs(iw)}";

        static string JR(uint iw)
        {       //08 (08)
            if (RS(iw) == 0x1F)
                EndOfFunction = pc + 4;
            return $"jr\t{rs(iw)}";
        }

        static string JALR(uint iw)
        {       //09 (09)
            jaltaken = 2;
            return $"jalr\t{rd_rs(iw)}";

        }

        static string SYSCALL(uint iw) => $"syscall\t{IMM_P(iw)}";

        static string BREAK(uint iw) => $"break\t## 0x{CODE(iw):X5}";

        static string SYNC(uint iw) => $"sync";

        static string MFHI(uint iw) => $"mfhi\t{rd(iw)}";

        static string MTHI(uint iw) => $"mthi\t{rd(iw)}";

        static string MFLO(uint iw) => $"mflo\t{rd(iw)}";

        static string MTLO(uint iw) => $"mtlo\t{rd(iw)}";

        static string DSLLV(uint iw) => $"dsllv\t{rd_rt_rs(iw)}";

        static string DSRLV(uint iw) => $"dsrlv\t{rd_rt_rs(iw)}";

        static string DSRAV(uint iw) => $"dsrav\t{rd_rt_rs(iw)}";

        static string MULT(uint iw) => $"mult\t{rs_rt(iw)}";

        static string MULTU(uint iw) => $"multu\t{rs_rt(iw)}";

        static string DIV(uint iw) => $"div\t{rs_rt(iw, true)}";

        static string DIVU(uint iw) => $"divu\t{rs_rt(iw, true)}";

        static string DMULT(uint iw) => $"dmult\t{rs_rt(iw)}";

        static string DMULTU(uint iw) => $"dmultu\t{rs_rt(iw)}";

        static string DDIV(uint iw) => $"ddiv\t{rs_rt(iw, true)}";

        static string DDIVU(uint iw) => $"ddivu\t{rs_rt(iw, true)}";

        static string ADD(uint iw) => $"add\t{rd_rs_rt(iw)}";

        static string ADDU(uint iw) => $"addu\t{rd_rs_rt(iw)}";

        static string SUB(uint iw) => $"sub\t{rd_rs_rt(iw)}";

        static string SUBU(uint iw) => $"subu\t{rd_rs_rt(iw)}";

        static string AND(uint iw) => $"and\t{rd_rs_rt(iw)}";

        static string OR(uint iw)
        {       //37 (25)
            gpr_regs[RD(iw)] = gpr_regs[RS(iw)] | gpr_regs[RT(iw)];
            return $"or\t{rd_rs_rt(iw)}\t## {gpr_rn[RD(iw)]} = {gpr_regs[RD(iw)]:X8}";
        }

        static string XOR(uint iw) => $"xor\t{rd_rs_rt(iw)}";

        static string NOR(uint iw) => $"nor\t{rd_rs_rt(iw)}";

        static string SLT(uint iw) => $"slt\t{rd_rs_rt(iw)}";

        static string SLTU(uint iw) => $"sltu\t{rd_rs_rt(iw)}";

        static string DADD(uint iw) => $"dadd\t{rd_rs_rt(iw)}";

        static string DADDU(uint iw) => $"daddu\t{rd_rs_rt(iw)}";

        static string DSUB(uint iw) => $"dsub\t{rd_rs_rt(iw)}";

        static string DSUBU(uint iw) => $"dsubu\t{rd_rs_rt(iw)}";

        static string TGE(uint iw) => $"tge\t{rs_rt(iw)}";

        static string TGEU(uint iw) => $"tgeu\t{rs_rt(iw)}";

        static string TLT(uint iw) => $"tlt\t{rs_rt(iw)}";

        static string TLTU(uint iw) => $"tltu\t{rs_rt(iw)}";

        static string TEQ(uint iw) => $"teq\t{rs_rt(iw)}";

        static string TNE(uint iw) => $"tne\t{rs_rt(iw)}";

        static string DSLL(uint iw) => $"dsll\t{rd_rt_sa(iw)}";

        static string DSRL(uint iw) => $"dsrl\t{rd_rt_sa(iw)}";

        static string DSRA(uint iw) => $"dsra\t{rd_rt_sa(iw)}";

        static string DSLL32(uint iw) => $"dsll32\t{rd_rt_sa(iw)}";

        static string DSRL32(uint iw) => $"dsrl32\t{rd_rt_sa(iw)}";

        static string DSRA32(uint iw) => $"dsra32\t{rd_rt_sa(iw)}";

        #endregion
        
        #region REGIMM op types
        static Func<uint, string>[] REGIMM_T = new Func<uint, string>[32]
        {
            BLTZ,   BGEZ,   BLTZL,  BGEZL,  NONE,   NONE,   NONE,   NONE,
            TGEI,   TGEIU,  TLTI,   TLTIU,  TEQI,   NONE,   TNEI,   NONE,
            BLTZAL,BGEZAL,  BLTZALL,BGEZALL,NONE,   NONE,   NONE,   NONE,
            NONE,   NONE,   NONE,   NONE,   NONE,   NONE,   NONE,   NONE
        };

        static string BLTZ(uint iw) => $"bltz\t{rs_branch(iw, pc)}";

        static string BGEZ(uint iw) => $"bgez\t{rs_branch(iw, pc)}";

        static string BLTZL(uint iw) => $"bltzl\t{rs_branch(iw, pc)}";

        static string BGEZL(uint iw) => $"bgezl\t{rs_branch(iw, pc)}";

        static string TGEI(uint iw) => $"tgei\t{rs_imm(iw)}";

        static string TGEIU(uint iw) => $"tgeiu\t{rs_imm(iw)}";

        static string TLTI(uint iw) => $"tlti\t{rs_imm(iw)}";

        static string TLTIU(uint iw) => $"tltiu\t{rs_imm(iw)}";

        static string TEQI(uint iw) => $"tqei\t{rs_imm(iw)}";

        static string TNEI(uint iw) => $"tnei\t{rs_imm(iw)}";

        static string BLTZAL(uint iw) => $"bltzal\t{rs_branch(iw, pc)}";

        static string BGEZAL(uint iw) => $"bgezal\t{rs_branch(iw, pc)}";

        static string BLTZALL(uint iw) => $"bltzall\t{rs_branch(iw, pc)}";

        static string BGEZALL(uint iw) => $"bgezall\t{rs_branch(iw, pc)}";

        #endregion
    }
}
