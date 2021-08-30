using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OcaBase
{
    partial class File
    {
        class Addr
        {
            public int Start;
            public int End;
            public int Size { get { return (End > Start) ? End - Start : 0; } }

            public Addr(int start, int end)
            {
                this.Start = start;
                this.End = end;
            }
        }

        private void GetAddr(Address addr, out Addr v)
        {
            v = new Addr(
                 (addr != null) ? addr.StartAddr : 0,
                 (addr != null) ? addr.EndAddr : 0);
        }

        public string PrintFileIdentity()
        {
            return string.Format("{0}: {1}{2}",
                FileId,
                Filename,
                string.IsNullOrWhiteSpace(Description) ? "" : "  " + Description);
        }
        public string PrintAddresses()
        {
            Addr dbg, n0, mqp;

            GetAddr(Address_DBG, out dbg);
            GetAddr(Address_N0, out n0);
            GetAddr(Address_MQP, out mqp);

            return string.Format("{0}: DBG {1:X8}:{2:X8}, Size 0x{3:X4}  NTSC 1.0 {4:X8}:{5:X8}, Size 0x{6:X4}  PAL MQ {7:X8}:{8:X8}, Size 0x{9:X4}",
                FileId,
                dbg.Start, dbg.End,
                dbg.Size,
                n0.Start, n0.End,
                n0.Size,
                mqp.Start, mqp.End,
                mqp.Size);
        }
    }
}
