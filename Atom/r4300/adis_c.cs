//mipsdis.c
//redistribute and modify at will, my name is spinout, keep it here.
//ported to C# by mzxrules

using System;
using System.Collections.Generic;
using mzxrules.Helper;

using static Atom.MipsFields;

namespace Atom
{
    public partial class Disassemble
    {
        /// <summary>
        /// List of defined labels
        /// </summary>
        public static Dictionary<N64Ptr, Label> Labels = new Dictionary<N64Ptr, Label>();
        static Dictionary<N64Ptr, N64Ptr> RelocationLabels = new Dictionary<N64Ptr, N64Ptr>();

        static Label GetRelocLabel(N64Ptr pc) => Labels[RelocationLabels[pc]];
        
        static bool First_Parse = false;
        static bool Rel_Parse = false;
        static N64Ptr Rel_Label_Addr = 0;
        internal static bool PrintRelocations = false;
        internal static bool GccOutput = false;

        static N64Ptr pc = 0x80000000;
        static int EndOfFunction = -1;
        static int jaltaken = 0;
        static int[] gpr_regs = new int[32];
        
        public static string[] gpr_rn = new string[32]
        {
            "$zero","$at",   "v0",   "v1",   "a0",   "a1",   "a2",   "a3",
            "t0",   "t1",   "t2",   "t3",   "t4",   "t5",   "t6",   "t7",
            "s0",   "s1",   "s2",   "s3",   "s4",   "s5",   "s6",   "s7",
            "t8",   "t9",   "k0",   "k1",   "gp",   "$sp",  "s8",   "$ra"
        };

        static string[] cop_rn = new string[32]
        {
            "Index",    "Random",   "EntryLo0", "EntryLo1", "Context",  "PageMask", "Wired",    "Reserved",
            "BadVAddr", "Count",    "EntryHi",  "Compare",  "Status",   "Cause",    "Epc",      "PRevID",
            "Config",   "LLAddr",   "WatchLo",  "WatchHi",  "XContext", "Reserved", "Reserved", "Reserved",
            "Reserved", "Reserved", "PErr",     "CacheErr", "TagLo",    "TagHi",    "ErrorEpc", "Reserved"
        };

        static string[] fpr_rn = new string[32]
        {
            "$f0",  "$f1",  "$f2",  "$f3",  "$f4",  "$f5",  "$f6",  "$f7",
            "$f8",  "$f9",  "$f10", "$f11", "$f12", "$f13", "$f14", "$f15",
            "$f16", "$f17", "$f18", "$f19", "$f20", "$f21", "$f22", "$f23",
            "$f24", "$f25", "$f26", "$f27", "$f28", "$f29", "$f30", "$f31"
        };

        static void Reset_Gpr_Regs()
        {
            for (int i = 0; i < 32; i++)
            {
                gpr_regs[i] = 0;
            }
        }

        //for JALs
        static void Reset_Gpr_Regs_Soft()
        {
            gpr_regs[1] = 0;
            gpr_regs[2] = 0;
            gpr_regs[3] = 0;
            gpr_regs[4] = 0;
            gpr_regs[5] = 0;
            gpr_regs[6] = 0;
            gpr_regs[7] = 0;
            gpr_regs[8] = 0;
            gpr_regs[9] = 0;
            gpr_regs[10] = 0;
            gpr_regs[11] = 0;
            gpr_regs[12] = 0;
            gpr_regs[13] = 0;
            gpr_regs[14] = 0;
            gpr_regs[15] = 0;
            gpr_regs[24] = 0;
            gpr_regs[25] = 0;
        }
        

        static string GetLabel(uint iw, N64Ptr pc)
        {
            N64Ptr addr = OFFSET(iw) + (pc | 0x80000000) + 4;
            Label label = AddLabel(addr);
            return $"{label}";
        }

        //mips
        static string PrintJump(uint iw, string opcode)
        {
            N64Ptr addr = TARGET(iw) | 0x80000000;

            if (Labels.TryGetValue(addr, out Label label)
                && label.Kind == Label.Type.FUNC)
            {
                var func = label;
                if (!string.IsNullOrWhiteSpace(func.Name))
                    return $"{opcode}\t{func}\t## {func.InlineDesc}";
                else
                    return $"{opcode}\t{func}";
            }
            else
            {
                label = new Label(Label.Type.FUNC, addr, false);
                return $"{opcode}\t{label}\t## NO_FUNCTION_DOCUMENTED {TARGET(iw):X8}";
            }
        }

        public static string GetOP(int iw)
        {
            if (iw == 0)
                return "nop";
            var result = MAIN_T[(iw >> 26) & 0x3F]((uint)iw);
            gpr_regs[0] = 0; //Just in case anything tries to change $zero
            return result;
        }

    }
}