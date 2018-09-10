using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Atom.MipsFields;
namespace Atom.RSP
{
    class decoder_h
    {
    }

    [Flags] public enum OPCODE_INFO
    {
        NONE = 0,
        VECTOR = 1<<1,
        BRANCH = 1<<31,
        NEEDRS = 1<<3,
        NEEDRT = 1<<4,
        NEEDVS = 1<<3,
        NEEDVT = 1<<4,
        LOAD = 1<<5,
        STORE = 1<<6,

        //custom
         
    }

    public class rsp_opcode
    {
        //public opcodes opcode;
        public uint id;
        public uint flags;
        public opcodes op;

        public rsp_opcode(opcodes op)
        {
            this.op = op;
            id = (uint)op;
            flags = opcodes_priv_h.OpcodeFlags[op];
        }

        public string parse(uint op)
        {
            string result = $"{this.op.ToString().ToLower()}";

            OPCODE_INFO test = (OPCODE_INFO)flags;

            if (!test.HasFlag(OPCODE_INFO.VECTOR))
            {
                if (test.HasFlag(OPCODE_INFO.NEEDRS))
                {
                    result += $", {Disassemble.gpr_rn[RS(op)]}";
                }

                if (test.HasFlag(OPCODE_INFO.NEEDRT))
                {
                    var t = Disassemble.gpr_rn[RT(op)];

                    if (test.HasFlag(OPCODE_INFO.LOAD) | test.HasFlag(OPCODE_INFO.STORE))
                    {
                        t = $"({t})";
                    }

                    result += $", {t}";
                }
            }
            else //vector opcodes
            {

                if (test.HasFlag(OPCODE_INFO.NEEDVS))
                {
                    result += $", $V{RT(op):D2}";
                }
                if (test.HasFlag(OPCODE_INFO.NEEDVT))
                {
                    var t = $"$V{RT(op):D2}";

                    if (test.HasFlag(OPCODE_INFO.LOAD) | test.HasFlag(OPCODE_INFO.STORE))
                    {
                        t = $"({t})";
                    }

                    result += $", {t}";
                }
            }

            return result;
        }
    }
}
