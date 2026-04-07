using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atom.RSP
{
    public class rsp_escape_table : List<rsp_opcode_escape>
    {
        public void Add(ushort offset, byte shift, byte mask)
        {
            Add(new rsp_opcode_escape(offset, shift, mask));
        }
    }

    public class rsp_opcode_table : List<rsp_opcode>
    {
        public void Add(Opcodes op)
        {
            Add(new rsp_opcode(op));
        }
    }


    public class decoder_c
    {
        public static rsp_opcode_table rsp_opcode_table = new rsp_opcode_table
        {
          {Opcodes.SLL},     {Opcodes.INVALID}, {Opcodes.SRL},     {Opcodes.SRA},
          {Opcodes.SLLV},    {Opcodes.INVALID}, {Opcodes.SRLV},    {Opcodes.SRAV},
          {Opcodes.JR},      {Opcodes.JALR},    {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.BREAK},   {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.ADDU},    {Opcodes.ADDU},    {Opcodes.SUBU},    {Opcodes.SUBU},
          {Opcodes.AND},     {Opcodes.OR},      {Opcodes.XOR},     {Opcodes.NOR},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.SLT},     {Opcodes.SLTU},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},

        // ============================================================================
        //  Escaped opcode table: RegImm.
        //
        //      31---------26----------20-------16------------------------------0
        //      | REGIMM/6  |          |  FMT/5  |                              |
        //      ------6---------------------5------------------------------------
        //      |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--|
        //   00 | BLTZ  | BGEZ  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
        //   01 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
        //   10 |BLTZAL |BGEZAL |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
        //   11 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
        //      |-------|-------|-------|-------|-------|-------|-------|-------|
        //
        // ============================================================================
          {Opcodes.BLTZ},    {Opcodes.BGEZ},    {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.BLTZAL},  {Opcodes.BGEZAL},  {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},

        // ============================================================================
        //  Escaped opcode table: COP0.
        //
        //      31--------26-25------21 ----------------------------------------0
        //      |  COP0/6   |  FMT/5  |                                         |
        //      ------6----------5-----------------------------------------------
        //      |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--|
        //   00 | MFC0  |  ---  |  ---  |  ---  | MTC0  |  ---  |  ---  |  ---  |
        //   01 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
        //   10 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
        //   11 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
        //      |-------|-------|-------|-------|-------|-------|-------|-------|
        // ============================================================================
          {Opcodes.MFC0},    {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.MTC0},    {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},

        // ============================================================================
        //  Escaped opcode table: COP2/1.
        //
        //      31--------26-25------21 ----------------------------------------0
        //      |  COP2/6   |  FMT/5  |                                         |
        //      ------6----------5-----------------------------------------------
        //      |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--|
        //   00 | MFC2  |  ---  | CFC2  |  ---  | MTC2  |  ---  | CTC2  |  ---  |
        //   01 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
        //   10 | *VECT | *VECT | *VECT | *VECT | *VECT | *VECT | *VECT | *VECT |
        //   11 | *VECT | *VECT | *VECT | *VECT | *VECT | *VECT | *VECT | *VECT |
        //      |-------|-------|-------|-------|-------|-------|-------|-------|
        //
        // ============================================================================
          {Opcodes.MFC2},    {Opcodes.INVALID}, {Opcodes.CFC2},    {Opcodes.INVALID},
          {Opcodes.MTC2},    {Opcodes.INVALID}, {Opcodes.CTC2},    {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},

        // ============================================================================
        //  Escaped opcode table: COP2/2.
        //
        //      31---------26---25------------------------------------5---------0
        //      |  = COP2   | 1 |                                     |  FMT/6  |
        //      ------6-------1--------------------------------------------6-----
        //      |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--|
        //  000 | VMULF | VMULU | VRNDP | VMULQ | VMUDL | VMUDM | VMUDN | VMUDH |
        //  001 | VMACF | VMACU | VRNDN | VMACQ | VMADL | VMADM | VMADN | VMADH |
        //  010 | VADD  | VSUB  |  ---  | VABS  | VADDC | VSUBC |  ---  |  ---  |
        //  011 |  ---  |  ---  |  ---  |  ---  |  ---  | VSAR  |  ---  |  ---  |
        //  100 |  VLT  |  VEQ  |  VNE  |  VGE  |  VCL  |  VCH  |  VCR  | VMRG  |
        //  101 | VAND  | VNAND |  VOR  | VNOR  | VXOR  | VNXOR |  ---  |  ---  |
        //  110 | VRCP  | VRCPL | VRCPH | VMOV  | VRSQ  | VRSQL | VRSQH | VNOP  |
        //  111 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  | VNULL |
        //      |-------|-------|-------|-------|-------|-------|-------|-------|
        //
        // ============================================================================
          {Opcodes.VMULF},    {Opcodes.VMULU},    {Opcodes.VRNDP},    {Opcodes.VMULQ},
          {Opcodes.VMUDL},    {Opcodes.VMUDM},    {Opcodes.VMUDN},    {Opcodes.VMUDH},
          {Opcodes.VMACF},    {Opcodes.VMACU},    {Opcodes.VRNDN},    {Opcodes.VMACQ},
          {Opcodes.VMADL},    {Opcodes.VMADM},    {Opcodes.VMADN},    {Opcodes.VMADH},
          {Opcodes.VADD},     {Opcodes.VSUB},     {Opcodes.VINVALID}, {Opcodes.VABS},
          {Opcodes.VADDC},    {Opcodes.VSUBC},    {Opcodes.VINVALID}, {Opcodes.VINVALID},
          {Opcodes.VINVALID}, {Opcodes.VINVALID}, {Opcodes.VINVALID}, {Opcodes.VINVALID},
          {Opcodes.VINVALID}, {Opcodes.VSAR},     {Opcodes.VINVALID}, {Opcodes.VINVALID},
          {Opcodes.VLT},      {Opcodes.VEQ},      {Opcodes.VNE},      {Opcodes.VGE},
          {Opcodes.VCL},      {Opcodes.VCH},      {Opcodes.VCR},      {Opcodes.VMRG},
          {Opcodes.VAND},     {Opcodes.VNAND},    {Opcodes.VOR},      {Opcodes.VNOR},
          {Opcodes.VXOR},     {Opcodes.VNXOR},    {Opcodes.VINVALID}, {Opcodes.VINVALID},
          {Opcodes.VRCP},     {Opcodes.VRCPL},    {Opcodes.VRCPH},    {Opcodes.VMOV},
          {Opcodes.VRSQ},     {Opcodes.VRSQL},    {Opcodes.VRSQH},    {Opcodes.VNOP},
          {Opcodes.VINVALID}, {Opcodes.VINVALID}, {Opcodes.VINVALID}, {Opcodes.VINVALID},
          {Opcodes.VINVALID}, {Opcodes.VINVALID}, {Opcodes.VINVALID}, {Opcodes.VNULL},

        // ============================================================================
        //  Escaped opcode table: LWC2.
        //
        //      31---------26-------------------15-------11---------------------0
        //      |   LWC2/6  |                   | FUNC/5 |                      |
        //      ------6-----------------------------5----------------------------
        //      |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--|
        //   00 |  LBV  |  LSV  |  LLV  |  LDV  |  LQV  |  LRV  |  LPV  |  LUV  |
        //   01 |  LHV  |  LFV  |  ---  |  LTV  |  ---  |  ---  |  ---  |  ---  |
        //   10 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
        //   11 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
        //      |-------|-------|-------|-------|-------|-------|-------|-------|
        //
        // ============================================================================
          {Opcodes.LBV},     {Opcodes.LSV},     {Opcodes.LLV},     {Opcodes.LDV},
          {Opcodes.LQV},     {Opcodes.LRV},     {Opcodes.LPV},     {Opcodes.LUV},
          {Opcodes.LHV},     {Opcodes.LFV},     {Opcodes.INVALID}, {Opcodes.LTV},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},

        // ============================================================================
        //  Escaped opcode table: SWC2.
        //
        //      31---------26-------------------15-------11---------------------0
        //      |   SWC2/6  |                   | FMT/5  |                      |
        //      ------6-----------------------------5----------------------------
        //      |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--|
        //   00 |  SBV  |  SSV  |  SLV  |  SDV  |  SQV  |  SRV  |  SPV  |  SUV  |
        //   01 |  SHV  |  SFV  |  SWV  |  STV  |  ---  |  ---  |  ---  |  ---  |
        //   10 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
        //   11 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
        //      |-------|-------|-------|-------|-------|-------|-------|-------|
        //
        // ============================================================================
          {Opcodes.SBV},     {Opcodes.SSV},     {Opcodes.SLV},     {Opcodes.SDV},
          {Opcodes.SQV},     {Opcodes.SRV},     {Opcodes.SPV},     {Opcodes.SUV},
          {Opcodes.SHV},     {Opcodes.SFV},     {Opcodes.SWV},     {Opcodes.STV},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},

        // ============================================================================
        //  First-order opcode table.
        //
        //  0b000000   => Lookup in 0.
        //  0b000001   => Lookup in 64.
        //  0b010000   => Lookup in 96.
        //  0b010001   => Lookup in rsp_cop2_opcode_table.
        //  0b110010   => Lookup in 208.
        //  0b111010   => Lookup in 240.
        //
        //      31---------26---------------------------------------------------0
        //      |  OPCODE/6 |                                                   |
        //      ------6----------------------------------------------------------
        //      |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--|
        //  000 | *SPEC | *RGIM |   J   |  JAL  |  BEQ  |  BNE  | BLEZ  | BGTZ  |
        //  001 | ADDI  | ADDIU | SLTI  | SLTIU | ANDI  |  ORI  | XORI  |  LUI  |
        //  010 | *COP0 |  ---  | *COP2 |  ---  |  ---  |  ---  |  ---  |  ---  |
        //  011 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
        //  100 |  LB   |  LH   |  ---  |  LW   |  LBU  |  LHU  |  ---  |  ---  |
        //  101 |  SB   |  SH   |  ---  |  SW   |  ---  |  ---  |  ---  |  ---  |
        //  110 |  ---  |  ---  | *LWC2 |  ---  |  ---  |  ---  |  ---  |  ---  |
        //  111 |  ---  |  ---  | *SWC2 |  ---  |  ---  |  ---  |  ---  |  ---  |
        //      |-------|-------|-------|-------|-------|-------|-------|-------|
        // ============================================================================
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.J},       {Opcodes.JAL},
          {Opcodes.BEQ},     {Opcodes.BNE},     {Opcodes.BLEZ},    {Opcodes.BGTZ},
          {Opcodes.ADDIU},   {Opcodes.ADDIU},   {Opcodes.SLTI},    {Opcodes.SLTIU},
          {Opcodes.ANDI},    {Opcodes.ORI},     {Opcodes.XORI},    {Opcodes.LUI},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.LB},      {Opcodes.LH},      {Opcodes.INVALID}, {Opcodes.LW},
          {Opcodes.LBU},     {Opcodes.LHU},     {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.SB},      {Opcodes.SH},      {Opcodes.INVALID}, {Opcodes.SW},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID},
          {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}, {Opcodes.INVALID}
        };


        /// <summary>
        /// 128 items
        /// </summary>
        public static rsp_escape_table rsp_escape_table = new rsp_escape_table
        {
             {0,    0, 0x3F}, {0,    0, 0x3F},
             {64,  16, 0x1F}, {64,  16, 0x1F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},

             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},

             {96,  21, 0x1F}, {96,  21, 0x1F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {128, 21, 0x1F}, {144,  0, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},

             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},

             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},

             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},

             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {208, 11, 0x1F}, {208, 11, 0x1F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},

             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {240, 11, 0x1F}, {240, 11, 0x1F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
             {272, 26, 0x3F}, {272, 26, 0x3F},
            };

        public static rsp_opcode rsp_decode_instruction(uint iw)
        {
            var i = (int)(iw >> 25);
            rsp_opcode_escape escape = rsp_escape_table[i];
            uint index = (iw >> escape.shift) & escape.mask;

            rsp_opcode group = rsp_opcode_table[escape.offset + (ushort)index];

            return group;
        }
    }

    public struct rsp_opcode_escape
    {
        public ushort offset;
        public byte shift, mask;
        public rsp_opcode_escape(ushort o, byte s, byte m)
        {
            offset = o;
            shift = s;
            mask = m;
        }
    }

}
