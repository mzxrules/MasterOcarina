//mipsdis.c
//redistribute and modify at will, my name is spinout, keep it here.
//ported to C# by mzxrules

//using mzxrules.Helper;


namespace Atom
{
    enum Opcodes : uint
    {
        #region Opcode
        //R_Type = SPECIAL
        /* SPECIAL  = 0x00000000    REGIMM      = 0x04000000*/  J           = 0x08000000,   JAL         = 0x0C000000,
        BEQ         = 0x10000000,   BNE         = 0x14000000,   BLEZ        = 0x18000000,   BGTZ        = 0x1C000000,
        ADDI        = 0x20000000,   ADDIU       = 0x24000000,   SLTI        = 0x28000000,   SLTIU       = 0x2C000000,
        ANDI        = 0x30000000,   ORI         = 0x34000000,   XORI        = 0x38000000,   LUI         = 0x3C000000,
        /*COP0      = 0x40000000*/  COP1        = 0x44000000,   COP2        = 0x48000000,   COP3        = 0x4C000000,
        BEQL        = 0x50000000,   BNEL        = 0x54000000,   BLEZL       = 0x58000000,   BGTZL       = 0x5C000000,
        DADDI       = 0x60000000,   DADDIU      = 0x64000000,   LDL         = 0x68000000,   LDR         = 0x6C000000,
        NONE_28     = 0x70000000,   NONE_29     = 0x74000000,   NONE_30     = 0x78000000,   NONE_31     = 0x7C000000,
        LB          = 0x80000000,   LH          = 0x84000000,   LWL         = 0x88000000,   LW          = 0x8C000000,
        LBU         = 0x90000000,   LHU         = 0x94000000,   LWR         = 0x98000000,   LWU         = 0x9C000000,
        SB          = 0xA0000000,   SH          = 0xA4000000,   SWL         = 0xA8000000,   SW          = 0xAC000000,
        SDL         = 0xB0000000,   SDR         = 0xB4000000,   SWR         = 0xB8000000,   CACHE       = 0xBC000000,
        LL          = 0xC0000000,   LWC1        = 0xC4000000,   LWC2        = 0xC8000000,   NONE_51     = 0xCC000000,
        LLD         = 0xD0000000,   LDC1        = 0xD4000000,   LDC2        = 0xD8000000,   LD          = 0xDC000000,
        SC          = 0xE0000000,   SWC1        = 0xE4000000,   SWC2        = 0xE8000000,   NONE_59     = 0xEC000000,
        SCD         = 0xF0000000,   SDC1        = 0xF4000000,   SDC2        = 0xF8000000,   SD          = 0xFC000000,
        #endregion

        #region SPECIAL = 0x00000000

        SLL = 0,    NONE_R01,   SRL,        SRA,        SLLV,       NONE_R05,   SRLV,       SRAV,
        JR,         JALR,       NONE_R10,   NONE_R11,   SYSCALL,    BREAK,      NONE_R14,   SYNC,
        MFHI,       MTHI,       MFLO,       MTLO,       DSLLV,      NONE_R21,   DSRLV,      DSRAV,
        MULT,       MULTU,      DIV,        DIVU,       DMULT,      DMULTU,     DDIV,       DDIVU,
        ADD,        ADDU,       SUB,        SUBU,       AND,        OR,         XOR,        NOR,
        NONE_R40,   NONE_R41,   SLT,        SLTU,       DADD,       DADDU,      DSUB,       DSUBU,
        TGE,        TGEU,       TLT,        TLTU,       TEQ,        NONE_R53,   TNE,        NONE_R55,
        DSLL,       NONE_R57,   DSRL,       DSRA,       DSLL32,     NONE_R61,   DSRL32,     DSRA32,
        //NOP = 0
        #endregion

        #region REGIMM = 0x04000000

        BLTZ        = 0x04000000,   BGEZ        = 0x04010000,   BLTZL       = 0x04020000,   BGEZL       = 0x04030000,
        NONE_I04    = 0x04040000,   NONE_I05    = 0x04050000,   NONE_I06    = 0x04060000,   NONE_I07    = 0x04070000,
        TGEI        = 0x04080000,   TGEIU       = 0x04090000,   TLTI        = 0x040A0000,   TLTIU       = 0x040B0000,
        TEQI        = 0x040C0000,   NONE_I13    = 0x040D0000,   TNEI        = 0x040E0000,   NONE_I15    = 0x040F0000,
        BLTZAL      = 0x04100000,   BGEZAL      = 0x04110000,   BLTZALL     = 0x04120000,   BGEZALL     = 0x04130000,
        NONE_I20    = 0x04140000,   NONE_I21    = 0x04150000,   NONE_I22    = 0x04160000,   NONE_I23    = 0x04170000,
        NONE_I24    = 0x04180000,   NONE_I25    = 0x04190000,   NONE_I26    = 0x041A0000,   NONE_I27    = 0x041B0000,
        NONE_I28    = 0x041C0000,   NONE_I29    = 0x041D0000,   NONE_I30    = 0x041E0000,   NONE_I31    = 0x041F0000,

        #endregion
            
        #region COP0 = 0x40000000
        MFC0        = 0x40000000,   MTC0        = 0x40800000,
        #region COP0 -> TLB = 0x42000000

        TLB_NONE    = 0x42000000,   TLBR        = 0x42000001,   TLBWI       = 0x42000002, //TLB_NONE,
        /* TLB_NONE,                TLB_NONE, */                TLBWR       = 0x42000006, //TLB_NONE,
        TLBP        = 0x42000008, //TLB_NONE,                   TLB_NONE,                   TLB_NONE,
        //TLB_NONE,                 TLB_NONE,                   TLB_NONE,                   TLB_NONE,
        //TLB_NONE,                 TLB_NONE,                   TLB_NONE,                   TLB_NONE,
        //TLB_NONE,                 TLB_NONE,                   TLB_NONE,                   TLB_NONE,
        #endregion
        ERET = 0x42000010,
        #endregion

        #region COP1 = 0x44000000

        #endregion
    }
    enum COP1_fmt : uint
    {
        MFC1, DMFC1, CF1, FMT_03, 
        MTC1, DMTC1, CTC1, FMT_07,
        BC,
        S = 16, D, FMT_18, FMT_19,
        W, L

    }
    enum COP1_Func
    {
        ADD,    SUB,    MUL,    DIV,    SQRT,   ABS,    MOV,    NEG,       
        ROUND_L,TRUNC_L,CEIL_L, FLOOR_L,ROUND_W,TRUNC_W,CEIL_W, FLOOR_W,
        INV_16, INV_17, INV_18, INV_19, INV_20, INV_21, INV_22, INV_23,
        INV_24, INV_25, INV_26, INV_27, INV_28, INV_29, INV_30, INV_31,
        CVT_S,  CVT_D,  INV_34, INV_35, CVT_W,  CVT_L,  INV_38, INV_39,
        INV_40, INV_41, INV_42, INV_43, INV_44, INV_45, INV_46, INV_47,
        C_F,    C_UN,   C_EQ,   C_UEQ,  C_OLT,  C_ULT,  C_OLE,  C_ULE,
        C_SF,   C_NGLE, C_SEQ,  C_NGL,  C_LT,   C_NGE,  C_LE,   C_NGT

    }
    enum COP1_nd_tf
    {
        BC1F, BC1T,
        BC1FL, BC1TL
    }

    
}