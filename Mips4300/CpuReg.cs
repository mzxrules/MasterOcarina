using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mips4300
{
    public enum CpuReg
    {
        r0 = 0x00,
        at = 0x01,
        v0 = 0x02,
        v1 = 0x03,

        a0 = 0x04,
        a1 = 0x05,
        a2 = 0x06,
        a3 = 0x07,

        t0 = 0x08,
        t1 = 0x09,
        t2 = 0x0A,
        t3 = 0x0B,
        t4 = 0x0C,
        t5 = 0x0D,
        t6 = 0x0E,
        t7 = 0x0F,

        s0 = 0x10,
        s1 = 0x11,
        s2 = 0x12,
        s3 = 0x13,
        s4 = 0x14,
        s5 = 0x15,
        s6 = 0x16,
        s7 = 0x17,

        t8 = 0x18,
        t9 = 0x19,
        k0 = 0x1A,
        k1 = 0x1B,
        gp = 0x1C,
        sp = 0x1D,
        s8 = 0x1E,
        ra = 0x1F,
    }


    public enum COP0Reg
    {
        Index       = 0x00,
        Random      = 0x01,
        EntryLo0    = 0x02,
        EntryLo1    = 0x03,
        Context     = 0x04,
        PageMask    = 0x05,
        Wired       = 0x06,
        RESERVE07   = 0x07,

        BadVAddr    = 0x08,
        Count       = 0x09,
        EntryHi     = 0x0A,
        Compare     = 0x0B,
        Status      = 0x0C,
        Cause       = 0x0D,
        EPC         = 0x0E,
        PRevID      = 0x0F,

        Config      = 0x10,
        LLAddr      = 0x11,
        WatchLo     = 0x12,
        WatchHi     = 0x13,
        XContext    = 0x14,
        RESERVE15   = 0x15,
        RESERVE16   = 0x16,
        RESERVE17   = 0x17,

        RESERVE18   = 0x18,
        RESERVE19   = 0x19,
        PErr        = 0x1A,
        CaacheErr   = 0x1B,
        TagLo       = 0x1C,
        TagHi       = 0x1D,
        ErrorEPC    = 0x1E,
        RESERVE1F   = 0x1F,
    }
}
