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
        public void Add(opcodes op)
        {
            Add(new rsp_opcode(op));
        }
    }


    public class decoder_c
    {
        public static rsp_opcode_table rsp_opcode_table = new rsp_opcode_table
        {
          {opcodes.SLL},     {opcodes.INVALID}, {opcodes.SRL},     {opcodes.SRA},
          {opcodes.SLLV},    {opcodes.INVALID}, {opcodes.SRLV},    {opcodes.SRAV},
          {opcodes.JR},      {opcodes.JALR},    {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.BREAK},   {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.ADDU},    {opcodes.ADDU},    {opcodes.SUBU},    {opcodes.SUBU},
          {opcodes.AND},     {opcodes.OR},      {opcodes.XOR},     {opcodes.NOR},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.SLT},     {opcodes.SLTU},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},

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
          {opcodes.BLTZ},    {opcodes.BGEZ},    {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.BLTZAL},  {opcodes.BGEZAL},  {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},

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
          {opcodes.MFC0},    {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.MTC0},    {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},

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
          {opcodes.MFC2},    {opcodes.INVALID}, {opcodes.CFC2},    {opcodes.INVALID},
          {opcodes.MTC2},    {opcodes.INVALID}, {opcodes.CTC2},    {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},

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
          {opcodes.VMULF},    {opcodes.VMULU},    {opcodes.VRNDP},    {opcodes.VMULQ},
          {opcodes.VMUDL},    {opcodes.VMUDM},    {opcodes.VMUDN},    {opcodes.VMUDH},
          {opcodes.VMACF},    {opcodes.VMACU},    {opcodes.VRNDN},    {opcodes.VMACQ},
          {opcodes.VMADL},    {opcodes.VMADM},    {opcodes.VMADN},    {opcodes.VMADH},
          {opcodes.VADD},     {opcodes.VSUB},     {opcodes.VINVALID}, {opcodes.VABS},
          {opcodes.VADDC},    {opcodes.VSUBC},    {opcodes.VINVALID}, {opcodes.VINVALID},
          {opcodes.VINVALID}, {opcodes.VINVALID}, {opcodes.VINVALID}, {opcodes.VINVALID},
          {opcodes.VINVALID}, {opcodes.VSAR},     {opcodes.VINVALID}, {opcodes.VINVALID},
          {opcodes.VLT},      {opcodes.VEQ},      {opcodes.VNE},      {opcodes.VGE},
          {opcodes.VCL},      {opcodes.VCH},      {opcodes.VCR},      {opcodes.VMRG},
          {opcodes.VAND},     {opcodes.VNAND},    {opcodes.VOR},      {opcodes.VNOR},
          {opcodes.VXOR},     {opcodes.VNXOR},    {opcodes.VINVALID}, {opcodes.VINVALID},
          {opcodes.VRCP},     {opcodes.VRCPL},    {opcodes.VRCPH},    {opcodes.VMOV},
          {opcodes.VRSQ},     {opcodes.VRSQL},    {opcodes.VRSQH},    {opcodes.VNOP},
          {opcodes.VINVALID}, {opcodes.VINVALID}, {opcodes.VINVALID}, {opcodes.VINVALID},
          {opcodes.VINVALID}, {opcodes.VINVALID}, {opcodes.VINVALID}, {opcodes.VNULL},

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
          {opcodes.LBV},     {opcodes.LSV},     {opcodes.LLV},     {opcodes.LDV},
          {opcodes.LQV},     {opcodes.LRV},     {opcodes.LPV},     {opcodes.LUV},
          {opcodes.LHV},     {opcodes.LFV},     {opcodes.INVALID}, {opcodes.LTV},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},

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
          {opcodes.SBV},     {opcodes.SSV},     {opcodes.SLV},     {opcodes.SDV},
          {opcodes.SQV},     {opcodes.SRV},     {opcodes.SPV},     {opcodes.SUV},
          {opcodes.SHV},     {opcodes.SFV},     {opcodes.SWV},     {opcodes.STV},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},

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
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.J},       {opcodes.JAL},
          {opcodes.BEQ},     {opcodes.BNE},     {opcodes.BLEZ},    {opcodes.BGTZ},
          {opcodes.ADDIU},   {opcodes.ADDIU},   {opcodes.SLTI},    {opcodes.SLTIU},
          {opcodes.ANDI},    {opcodes.ORI},     {opcodes.XORI},    {opcodes.LUI},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.LB},      {opcodes.LH},      {opcodes.INVALID}, {opcodes.LW},
          {opcodes.LBU},     {opcodes.LHU},     {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.SB},      {opcodes.SH},      {opcodes.INVALID}, {opcodes.SW},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID},
          {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}, {opcodes.INVALID}
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
