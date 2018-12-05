using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mzxrules.Helper;

using u64 = System.UInt64;
using u32 = System.UInt32;
using static System.Console;

namespace Spectrum
{
    static class ThreadStructs
    {
        public enum OS_STATE
        {
            STOPPED = 1,
            RUNNABLE = 2,
            RUNNING = 4,
            WAITING = 8
        }
        public class OSThread
        {
            N64Ptr address;
            /*0x00*/
            public N64Ptr OSThread_next { get; set; } //run/mesg queue link
            public int priority;
            public N64Ptr OSThread_queue { get; set; } //queue thread is on. Double pointer
            public N64Ptr OSThread_tlnext { get; set; } //all threads queue link
            /*0x10*/
            public ushort state { get; set; } //OS_STATE_
            public ushort flags; //flags for rmon
            public int OSId { get; set; } //id for debugging
            public int fp; //thread has used floating point unit (COP0)
            /*0x1C*/ //OSThreadprofile, workarea for thread profiler
            /*0x20*/
            OSThreadContext c; //register/interrupt mask

            public OSThread(Ptr ptr)
            {
                address = ptr;

                OSThread_next = ptr.ReadUInt32(0);
                priority = ptr.ReadInt32(4);
                OSThread_queue = ptr.ReadUInt32(8);
                OSThread_tlnext = ptr.ReadUInt32(0x0C);

                state = ptr.ReadUInt16(0x10);
                flags = ptr.ReadUInt16(0x12);
                OSId = ptr.ReadInt32(0x14);
                fp = ptr.ReadInt32(0x18);
                c = new OSThreadContext(ptr.RelOff(0x20));
            }
            public void PrintState()
            {
                PrintBasicInfo();
                WriteLine();
                WriteLine($"AT: {w(c.at)}   V0: {w(c.v0)}   V1: {w(c.v1)}");
                WriteLine($"A0: {w(c.a0)}   A1: {w(c.a1)}   A2: {w(c.a2)}");
                WriteLine($"A3: {w(c.a3)}   T0: {w(c.t0)}   T1: {w(c.t1)}");
                WriteLine($"T2: {w(c.t2)}   T3: {w(c.t3)}   T4: {w(c.t4)}");
                WriteLine($"T5: {w(c.t5)}   T6: {w(c.t6)}   T7: {w(c.t7)}");
                WriteLine($"S0: {w(c.s0)}   S1: {w(c.s1)}   S2: {w(c.s2)}");
                WriteLine($"S3: {w(c.s3)}   S4: {w(c.s4)}   S5: {w(c.s5)}");
                WriteLine($"S6: {w(c.s6)}   S7: {w(c.s7)}   T8: {w(c.t8)}");
                WriteLine($"T9: {w(c.t9)}   GP: {w(c.gp)}   SP: {w(c.sp)}");
                WriteLine($"S8: {w(c.s8)}   RA: {w(c.ra)}   LO: {w(c.lo)}");
                WriteLine();
                WriteLine($"FPCSR: {c.fpcsr:X8}");
                WriteLine();
                if (fp > 0)
                {
                    WriteLine($"F00:{(float)c.fp0,14:0.0000000e+00}     F02:{(float)c.fp2,14:0.0000000e+00}");
                    WriteLine($"F04:{(float)c.fp4,14:0.0000000e+00}     F06:{(float)c.fp6,14:0.0000000e+00}");
                    WriteLine($"F08:{(float)c.fp8,14:0.0000000e+00}     F10:{(float)c.fp10,14:0.0000000e+00}");
                    WriteLine($"F12:{(float)c.fp12,14:0.0000000e+00}     F14:{(float)c.fp14,14:0.0000000e+00}");
                    WriteLine($"F16:{(float)c.fp16,14:0.0000000e+00}     F18:{(float)c.fp18,14:0.0000000e+00}");
                    WriteLine($"F20:{(float)c.fp20,14:0.0000000e+00}     F22:{(float)c.fp22,14:0.0000000e+00}");
                    WriteLine($"F24:{(float)c.fp24,14:0.0000000e+00}     F26:{(float)c.fp26,14:0.0000000e+00}");
                    WriteLine($"F28:{(float)c.fp28,14:0.0000000e+00}     F30:{(float)c.fp30,14:0.0000000e+00}");
                }
                else
                {
                    WriteLine("FP UNUSED");
                }

                string w(ulong val)
                {
                    return $"{(uint)val:X8}";
                }
            }

            public void PrintBasicInfo()
            {
                WriteLine($"{address} THREAD: {OSId}  {(OS_STATE)state}  PRIORITY: {priority:X2}");
                WriteLine($"next: {OSThread_next} queue: {OSThread_queue} tlnext: {OSThread_tlnext}");
                WriteLine($"PC: {w(c.pc)}   SR: {w(c.sr)} VA: {w(c.badvaddr)}");

                string w(ulong val)
                {
                    return $"{(uint)val:X8}";
                }
            }
        }
        class OSThreadContext
        {
            public u64 at, v0, v1, a0, a1, a2, a3;
            public u64 t0, t1, t2, t3, t4, t5, t6, t7;
            public u64 s0, s1, s2, s3, s4, s5, s6, s7;
            public u64 t8, t9, gp, sp, s8, ra;
            public u64 lo, hi;
            public u32 sr, pc, cause, badvaddr, rcp;
            public u32 fpcsr;
            public __OSfp fp0, fp2, fp4, fp6, fp8, fp10, fp12, fp14;
            public __OSfp fp16, fp18, fp20, fp22, fp24, fp26, fp28, fp30;

            public OSThreadContext(Ptr ptr)
            {
                at = ptr.ReadUInt64(0x00);
                v0 = ptr.ReadUInt64(0x08);
                v1 = ptr.ReadUInt64(0x10);
                a0 = ptr.ReadUInt64(0x18);
                a1 = ptr.ReadUInt64(0x20);
                a2 = ptr.ReadUInt64(0x28);
                a3 = ptr.ReadUInt64(0x30);

                t0 = ptr.ReadUInt64(0x38);
                t1 = ptr.ReadUInt64(0x40);
                t2 = ptr.ReadUInt64(0x48);
                t3 = ptr.ReadUInt64(0x50);
                t4 = ptr.ReadUInt64(0x58);
                t5 = ptr.ReadUInt64(0x60);
                t6 = ptr.ReadUInt64(0x68);
                t7 = ptr.ReadUInt64(0x70);

                s0 = ptr.ReadUInt64(0x78);
                s1 = ptr.ReadUInt64(0x80);
                s2 = ptr.ReadUInt64(0x88);
                s3 = ptr.ReadUInt64(0x90);
                s4 = ptr.ReadUInt64(0x98);
                s5 = ptr.ReadUInt64(0xA0);
                s6 = ptr.ReadUInt64(0xA8);
                s7 = ptr.ReadUInt64(0xB0);

                t8 = ptr.ReadUInt64(0xB8);
                t9 = ptr.ReadUInt64(0xC0);
                gp = ptr.ReadUInt64(0xC8);
                sp = ptr.ReadUInt64(0xD0);
                s8 = ptr.ReadUInt64(0xD8);
                ra = ptr.ReadUInt64(0xE0);
                lo = ptr.ReadUInt64(0xE8);
                hi = ptr.ReadUInt64(0xF0);

                sr = ptr.ReadUInt32(0xF8);
                pc = ptr.ReadUInt32(0xFC);
                cause = ptr.ReadUInt32(0x100);
                badvaddr = ptr.ReadUInt32(0x104);
                rcp = ptr.ReadUInt32(0x108);
                fpcsr = ptr.ReadUInt32(0x10C);

                fp0 = new __OSfp(ptr.RelOff(0x110));
                fp2 = new __OSfp(ptr.RelOff(0x118));
                fp4 = new __OSfp(ptr.RelOff(0x120));
                fp6 = new __OSfp(ptr.RelOff(0x128));
                fp8 = new __OSfp(ptr.RelOff(0x130));
                fp10 = new __OSfp(ptr.RelOff(0x138));
                fp12 = new __OSfp(ptr.RelOff(0x140));
                fp14 = new __OSfp(ptr.RelOff(0x148));
                fp16 = new __OSfp(ptr.RelOff(0x150));
                fp18 = new __OSfp(ptr.RelOff(0x158));
                fp20 = new __OSfp(ptr.RelOff(0x160));
                fp22 = new __OSfp(ptr.RelOff(0x168));
                fp22 = new __OSfp(ptr.RelOff(0x170));
                fp24 = new __OSfp(ptr.RelOff(0x178));
                fp26 = new __OSfp(ptr.RelOff(0x180));
                fp28 = new __OSfp(ptr.RelOff(0x188));
                fp30 = new __OSfp(ptr.RelOff(0x190));
            }

            public class __OSfp
            {
                //u64 data;
                float fValue;
                double dValue;
                public __OSfp(Ptr ptr)
                {
                    fValue = ptr.ReadFloat(0);
                }

                public static implicit operator float(__OSfp v)
                {
                    return v.fValue;
                }

                public static implicit operator __OSfp(float v)
                {
                    throw new NotImplementedException();
                }
            }
        }
        static ThreadStackCtx[] ThreadContextA = new ThreadStackCtx[]{
                new ThreadStackCtx(0x80006830, "boot"),
                new ThreadStackCtx(0x80006E00, "idle"),
                new ThreadStackCtx(0x80007BD0, "main"),
                new ThreadStackCtx(0x80007D20, "dmamgr"),
                new ThreadStackCtx(0x80121C68, "fault"),
                new ThreadStackCtx(0x80120C18, "irqmgr"),
                new ThreadStackCtx(0x80120BF8, "padmgr"),
                new ThreadStackCtx(0x80120BD8, "audio"),
                new ThreadStackCtx(0x80120BB8, "sched"),
                new ThreadStackCtx(0x80120B98, "graph")
            };

        class ThreadStackData: IRamItem
        {
            public FileAddress Ram { get; set; }
            private ThreadStackCtx Parent;

            public ThreadStackData (ThreadStackCtx parent, FileAddress addr)
            {
                Parent = parent;
                Ram = addr;
            }
            public override string ToString()
            {
                return string.Format("{0,-6} STACK {1:X8}:{2:X8}",
                    Parent.Name,
                    (int)Ram.Start, (int)Ram.End);

            }
        }

        class ThreadStackCtx: IRamItem
        {
            public FileAddress Ram { get; }
            public string Name { get; }


            public N64Ptr NextPtr { get; set; }
            public N64Ptr PrevPtr { get; set; }
            public ThreadStackData StackAddr { get; set; }
            //0x10 = init value
            public int Unknown { get; set; }
            //0x18 = str ptr
            public int Unknown2 { get; set; }

            public ThreadStackCtx(N64Ptr addr, string name)
            {
                Ram = new FileAddress(addr, addr + 0x20);
                Name = name;

            }
            public void Initialize()
            {
                Ptr ptr = SPtr.New(Ram.Start);
                NextPtr = ptr.ReadInt32(0x00);
                PrevPtr = ptr.ReadInt32(0x04);
                int StartPtr = ptr.ReadInt32(0x08);
                int EndPtr = ptr.ReadInt32(0x0C);
                StackAddr = new ThreadStackData(this, new FileAddress(StartPtr, EndPtr));
                //0x10
                Unknown = ptr.ReadInt32(0x14);
                //0x18
                Unknown2 = ptr.ReadInt32(0x1C);
            }

            public override string ToString()
            {
                return string.Format("{0,-6} Ctx1 NEXT {1:X6} PREV {2:X6} - {3:X6}:{4:X6}",
                    Name,
                    NextPtr, PrevPtr,
                    StackAddr.Ram.Start & 0xFFFFFF,
                    StackAddr.Ram.End & 0xFFFFFF);
            }
        }
        public static List<IRamItem> GetIRamItems()
        {
            var result = new List<IRamItem>();

            foreach (var item in ThreadContextA)
            {
                item.Initialize();
                result.Add(item);
                result.Add(item.StackAddr);
            }

            return result;
        }
    }
}
