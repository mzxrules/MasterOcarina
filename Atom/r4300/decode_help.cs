using System;
using mzxrules.Helper;
using static Atom.MipsFields;

namespace Atom
{
    public partial class Disassemble
    {
        static string base_imm(uint iw, string reg)
        {
            var addr = gpr_regs[BASE(iw)] + IMM(iw);

            if (Rel_Parse)
            {
                Rel_Label_Addr = addr;
            }
            if (!First_Parse && RelocationLabels.ContainsKey(pc))
            {
                if (PrintRelocations)
                {
                    var label = GetRelocLabel(pc);
                    return $"{reg}, %lo({label})({gpr_rn[BASE(iw)]})";
                }
            }
            string result = $"{reg}, {IMM_P(iw)}({gpr_rn[BASE(iw)]})";
            if (BASE(iw) != 29)
                result += $"\t## {addr:X8}";
            return result;
        }

        private static object IMM_P(uint iw)
        {
            //not called by LUI
            short imm = IMM(iw);
            if (!GccOutput)
                return $"0x{imm:X4}";
            if (imm >= 0)
                return $"0x{imm:X4}";
            else
                return $"-0x{0x10000 - (ushort)imm:X4}";
        }

        static string ft_base_imm(uint iw)
        {
            return base_imm(iw, fpr_rn[FT(iw)]);
        }
        static string rt_base_imm(uint iw)
        {
            return base_imm(iw, gpr_rn[RT(iw)]);
        }

        static string rt_imm(uint iw)
        {
            if (!First_Parse && PrintRelocations && RelocationLabels.ContainsKey(pc))
            {
                var label = GetRelocLabel(pc);
                return $"{gpr_rn[RT(iw)]}, %hi({label})";
            }

            return $"{gpr_rn[RT(iw)]}, 0x{IMM(iw):X4}";
        }

        static string rt_rs_imm(uint iw)
        {
            if (Rel_Parse)
            {
                Rel_Label_Addr = gpr_regs[RT(iw)];
            }
            if (!First_Parse && RelocationLabels.ContainsKey(pc))
            {
                if (PrintRelocations)
                {
                    var label = GetRelocLabel(pc);
                    return $"{gpr_rn[RT(iw)]}, {gpr_rn[RS(iw)]}, %lo({label})";
                }
            }
            return $"{gpr_rn[RT(iw)]}, {gpr_rn[RS(iw)]}, 0x{IMM(iw):X4}";
        }


        static string rs_imm(uint iw)
        {
            return $"{gpr_rn[RS(iw)]}, 0x{IMM(iw):X4}";
        }
        static string rs_rt(uint iw, bool division = false)
        {
            if (!GccOutput || !division)
                return $"{gpr_rn[RS(iw)]}, {gpr_rn[RT(iw)]}";
            return $"{gpr_rn[0]}, {gpr_rn[RS(iw)]}, {gpr_rn[RT(iw)]}";
        }
        static string rd(uint iw)
        {
            return $"{gpr_rn[RD(iw)]}";
        }
        static string rd_rs_rt(uint iw) //careful
        {
            return $"{gpr_rn[RD(iw)]}, {gpr_rn[RS(iw)]}, {gpr_rn[RT(iw)]}";
        }
        static string rd_rt_rs(uint iw) //careful
        {
            return $"{gpr_rn[RD(iw)]}, {gpr_rn[RT(iw)]}, {gpr_rn[RS(iw)]}";
        }
        static string rd_rt_sa(uint iw)
        {
            return $"{gpr_rn[RD(iw)]}, {gpr_rn[RT(iw)]}, {SA(iw),2}";
        }
        static string rs(uint iw)
        {
            return $"{gpr_rn[RS(iw)]}";
        }
        static string rs_branch(uint iw, N64Ptr pc)
        {
            return $"{gpr_rn[RS(iw)]}, {GetLabel(iw, pc)}";
        }
        static string rs_rt_branch(uint iw, N64Ptr pc)
        {
            return $"{gpr_rn[RS(iw)]}, {gpr_rn[RT(iw)]}, {GetLabel(iw, pc)}";
        }
        static string rd_rs(uint iw)
        {
            return $"{gpr_rn[RD(iw)]}, {gpr_rn[RS(iw)]}";
        }
        static string NONE(uint iw)
        {
            return $"(Invalid opcode: {iw:X8})";
        }

        //COP1
        static string fs_ft(uint iw)
        {
            return $"{fpr_rn[FS(iw)]}, {fpr_rn[FT(iw)]}";
        }
        static string fd_fs(uint iw)
        {
            return $"{fpr_rn[FD(iw)]}, {fpr_rn[FS(iw)]}";
        }
        static string fd_fs_ft(uint iw)
        {
            return $"{fpr_rn[FD(iw)]}, {fpr_rn[FS(iw)]}, {fpr_rn[FT(iw)]}";
        }
        static string rt_fs(uint iw)
        {
            return $"{gpr_rn[RT(iw)]}, {fpr_rn[FS(iw)]}";
        }
    }
}
