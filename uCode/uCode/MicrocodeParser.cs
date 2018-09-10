using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace uCode
{
    public class MicrocodeParserTask
    {
        public N64Ptr[] SegmentTable = new N64Ptr[16];
        public Stack<N64Ptr> DisplayListStack = new Stack<N64Ptr>();
        public N64Ptr StartAddress { get; }
        public N64Ptr CurrentAddress { get; private set; }

        public UInt32 G_RDPHALF_1;
        public UInt32 G_RDPHALF_2;
        
        public MicrocodeParserTask(N64Ptr startAddress)
        {
            StartAddress = startAddress;
            CurrentAddress = startAddress;
        }
    }
    public static class MicrocodeParser
    {
        /// <summary>
        /// Traces 
        /// </summary>
        /// <param name="memory">Full RDRAM dump</param>
        /// <param name="address">The entry point address for the display list trace</param>
        /// <returns></returns>
        public static IEnumerable<(N64Ptr ptr, Microcode gbi, MicrocodeParserTask task)> DeepTrace(BinaryReader memory, N64Ptr address)
        {
            bool keepParsing = true;
            MicrocodeParserTask task = new MicrocodeParserTask(address);

            memory.BaseStream.Position = address & 0xFFFFFF;

            while (keepParsing)
            {
                var microcode = new Microcode(memory);
                var addr = memory.BaseStream.Position - 8;
                yield return (addr, microcode, task);
                
                keepParsing = TraceNext(memory, task, microcode);
            }
        }

        public static IEnumerable<string> DeepParse(BinaryReader memory, N64Ptr address)
        {
            bool keepParsing = true;
            MicrocodeParserTask task = new MicrocodeParserTask(address);

            memory.BaseStream.Position = address & 0xFFFFFF;

            while (keepParsing)
            {
                var microcode = new Microcode(memory);
                yield return $"{memory.BaseStream.Position - 8:X6}: {PrintMicrocode(memory, microcode, false)}";

                keepParsing = TraceNext(memory, task, microcode);
            }
        }

        private static bool TraceNext(BinaryReader br, MicrocodeParserTask task, Microcode microcode)
        {
            switch (microcode.Name)
            {
                case G_.G_MOVEWORD:
                    if ((G_MW)((microcode.EncodingHigh >> 16) & 0xFF) == G_MW.G_MW_SEGMENT)
                    {
                        task.SegmentTable[(microcode.EncodingHigh & 0xFFFF) >> 2] = microcode.EncodingLow;
                    }
                    break;
                case G_.G_BRANCH_Z:
                    {
                        N64Ptr addr;
                        var bank = (task.G_RDPHALF_1 >> 24) & 0xFF;
                        if (bank < 16)
                        {
                            addr = task.SegmentTable[bank] + (task.G_RDPHALF_1 & 0xFFFFFF);
                        }
                        else
                            addr = task.G_RDPHALF_1;

                        //already advanced the cursor so this should be good
                        task.DisplayListStack.Push(0x80000000 | br.BaseStream.Position);

                        //jump
                        br.BaseStream.Position = addr & 0xFFFFFF;
                    }
                    
                    break;
                case G_.G_DL:
                    {
                        // set address we're branching to
                        N64Ptr addr;
                        var bank = (microcode.EncodingLow >> 24) & 0xFF;
                        if (bank < 16)
                        {
                            addr = task.SegmentTable[bank] + (microcode.EncodingLow & 0xFFFFFF);
                        }
                        else
                            addr = microcode.EncodingLow;

                        //if branching
                        if (((microcode.EncodingHigh >> 16) & 0xFF) == 0)
                        {
                            //already advanced the cursor so this should be good
                            task.DisplayListStack.Push(0x80000000 | br.BaseStream.Position);
                        }
                        //jump
                        br.BaseStream.Position = addr & 0xFFFFFF;
                    }
                    break;
                case G_.G_ENDDL:
                    if (task.DisplayListStack.Count == 0)
                    {
                        return false;
                    }
                    else
                    {
                        N64Ptr jumpback = task.DisplayListStack.Pop();
                        br.BaseStream.Position = jumpback & 0xFFFFFF;
                    }
                    break;
                case G_.G_RDPHALF_1: task.G_RDPHALF_1 = microcode.EncodingLow;
                    break;
                case G_.G_RDPHALF_2: task.G_RDPHALF_2 = microcode.EncodingLow;
                    break;

            }
            return true;
        }

        public static IEnumerable<string> SimpleParse(BinaryReader br, int iterations)
        {
            int i = 0;
            while (i < iterations)
            {
                var microcode = new Microcode(br);
                yield return PrintMicrocode(br, microcode);
                i++;
            }
        }

        public static string PrintMicrocode(BinaryReader memory, Microcode microcode, bool simpleParse = true)
        {
            string result;
            switch (microcode.Name)
            {
                case G_.G_NOOP:
                    result = $"{microcode} // {microcode.Name} {ParseG_NOOP(memory, microcode, simpleParse)}";
                    break;
                case G_.G_MOVEWORD:
                    result = 
                        string.Format("{0} // {1} ({2} #{3:D2}, {4:X8})",
                        microcode, microcode.Name,
                        (G_MW)((microcode.EncodingHigh >> 16) & 0xFF),
                        (microcode.EncodingHigh & 0xFFFF) >> 2, microcode.EncodingLow); break;
                case G_.G_DL:
                    result = (((microcode.EncodingHigh >> 16) & 0xFF) == 0) ?
                        $"{microcode} // {microcode.Name} Jump and Link"
                        : $"{microcode} // {microcode.Name} Branch"; break;
                case G_.G_MTX:
                    result = ParseG_MTX(memory, microcode, simpleParse); break;
                case G_.G_MOVEMEM:
                    result = ParseG_MOVEMEM(memory, microcode, simpleParse);
                    break;
                case G_.G_SETTILE: result = $"{microcode} // {microcode.Name} {ParseG_SETTILE(microcode)}"; break;
                case G_.G_SETTIMG: result = $"{microcode} // {microcode.Name} {ParseG_SET_IMG(microcode, false)}"; break;
                case G_.G_SETCIMG: result = $"{microcode} // {microcode.Name} {ParseG_SET_IMG(microcode, true)}"; break;
                case G_.G_LOADBLOCK: result = $"{microcode} // {microcode.Name} {ParseG_LOADBLOCK(microcode)}"; break;
                case G_.G_SETTILESIZE: //same parameters
                case G_.G_LOADTILE: result = $"{microcode} // {microcode.Name} {ParseG_LOADTILE(microcode)}"; break;
                case G_.G_LOADTLUT: result = $"{microcode} // {microcode.Name} {ParseG_LOADTLUT(microcode)}"; break;
                case G_.G_ENDDL: goto default;
                default:
                    result = $"{microcode} // {microcode.Name}";
                    break;
            }
            return result;
        }

        private static string ParseG_LOADBLOCK(Microcode microcode)
        {
            float uls = ((float)Shift.AsUInt16(microcode.EncodingHigh, 0xFFF000)) / 4;
            float ult = ((float)Shift.AsUInt16(microcode.EncodingHigh, 0x000FFF)) / 4;
            uint tile = Shift.AsUInt16(microcode.EncodingLow, 0x0F000000);
            int texels = Shift.AsUInt16(microcode.EncodingLow, 0x00FFF000) + 1;
            float dxt = microcode.EncodingLow & 0xFFF;
            dxt /= 2048;
            float rdxt = 1 / dxt;
            return $"TILE {tile} ST ({uls:F2},{ult:F2}) TEXELS {texels:X4} DXT {dxt} 1/DXT {rdxt}";
        }

        private static string ParseG_LOADTILE(Microcode microcode)
        {
            float uls = ((float)Shift.AsUInt16(microcode.EncodingHigh, 0xFFF000)) / 4;
            float ult = ((float)Shift.AsUInt16(microcode.EncodingHigh, 0x000FFF)) / 4;
            uint tile = Shift.AsUInt16(microcode.EncodingLow, 0x07000000);

            float lrs = ((float)Shift.AsUInt16(microcode.EncodingLow, 0xFFF000)) / 4;
            float lrt = ((float)Shift.AsUInt16(microcode.EncodingLow, 0x000FFF)) / 4;
            return $"TILE {tile} ST ({uls:F2},{ult:F2}), ({lrs:F2},{lrt:F2})";
        }

        private static string ParseG_LOADTLUT(Microcode microcode)
        {
            uint tile = Shift.AsUInt16(microcode.EncodingLow, 0x07000000);
            int colors = Shift.AsUInt16(microcode.EncodingLow, 0xFFC000) + 1;

            return $"TILE {tile} COLORS {colors}";
        }

        private static string ParseG_SETTILE(Microcode microcode)
        {
            uint fmt = Shift.AsUInt16(microcode.EncodingHigh,  0xE00000);
            uint siz = Shift.AsUInt16(microcode.EncodingHigh,  0x180000);
            uint line = Shift.AsUInt16(microcode.EncodingHigh, 0x03FE00);
            uint tmem = Shift.AsUInt16(microcode.EncodingHigh, 0x0001FF);
            uint tile = Shift.AsUInt16(microcode.EncodingLow, 0x07000000);
            uint palette = Shift.AsUInt16(microcode.EncodingLow, 0x00F00000);
            uint T_dat = Shift.AsUInt16(microcode.EncodingLow, 0x000FFC00);
            uint S_dat = Shift.AsUInt16(microcode.EncodingLow, 0x000003FF);
            (G_TX cmA, G_TX cmB, int mask, int shift) T, S;
            T = SetAxis(T_dat);
            S = SetAxis(S_dat);

            return $"TILE {tile} {(G_IM_FMT_)fmt} {(G_IM_SIZ_)siz} line {line} PAL {palette}";// +
                //$" T ({PrintAxis(T)}) S ({PrintAxis(S)})";

            (G_TX cmA, G_TX cmB, int mask, int shift) SetAxis(uint t_dat)
            {
                G_TX cmA = (Shift.AsBool(t_dat, 0x100)) ? G_TX.G_TX_MIRROR : G_TX.G_TX_NOMIRROR;
                G_TX cmB = (Shift.AsBool(t_dat, 0x200)) ? G_TX.G_TX_CLAMP : G_TX.G_TX_WRAP;
                int mask = Shift.AsUInt16(t_dat, 0xF0);
                int shift = Shift.AsUInt16(t_dat, 0x0F);
                return (cmA, cmB, mask, shift);
            }
            string PrintAxis((G_TX cmA, G_TX cmB, int mask, int shift) t)
            {
                return $"{t.cmA} {t.cmB} MASK {t.mask} SHIFT {t.shift}";
            }
        }
        
        private static string ParseG_SET_IMG(Microcode microcode, bool showWidth)
        {
            uint fmt = Shift.AsUInt16(microcode.EncodingHigh, 0xE00000);
            uint siz = Shift.AsUInt16(microcode.EncodingHigh, 0x180000);
            uint width = (microcode.EncodingHigh & 0xFFF) + 1;

            if (showWidth)
                return $"{(G_IM_FMT_)fmt} {(G_IM_SIZ_)siz} {width:X4}";

            return $"{(G_IM_FMT_)fmt} {(G_IM_SIZ_)siz}";
        }

        private static string ParseG_MOVEMEM(BinaryReader br, Microcode microcode, bool simpleParse)
        {
            string result;
            byte offset = (byte)((microcode.EncodingHigh >> 8) * 8);
            byte size = (byte)(((microcode.EncodingHigh >> 16) & 0xF8) + 8);

            result = $"{microcode} // {microcode.Name} {(G_MV)(microcode.EncodingHigh & 0xff)}: +0x{offset:X2} size 0x{size:X2}";

            if (!simpleParse)
            {
                int[] data = new int[size / 4];
                var jumpback = br.BaseStream.Position;
                br.BaseStream.Position = microcode.EncodingLow & 0xFFFFFF;

                for (int i = 0; i < size / 4; i++)
                    data[i] = br.ReadBigInt32();
                
                br.BaseStream.Position = jumpback;
                result += Environment.NewLine + data.JoinFormat(", ", "{0:X8}");
            }

            return result;
        }

        internal static string JoinFormat<T>(this IEnumerable<T> list, string separator,
                                   string formatString)
        {
            formatString = string.IsNullOrWhiteSpace(formatString) ? "{0}" : formatString;
            return string.Join(separator,
                                 list.Select(item => string.Format(formatString, item)));
        }

        private static string ParseG_NOOP(BinaryReader br, Microcode microcode, bool simpleParse)
        {
            string result = string.Empty;

            if (microcode.EncodingLow == 0 || simpleParse)
                return result;

            if (microcode.EncodingLow >> 24 != 0x80 
                    && microcode.EncodingLow >> 24 != 0x00)
                return result;

            var jumpBack = br.BaseStream.Position;
            br.BaseStream.Position = microcode.EncodingLow & 0xFFFFFF;

            Encoding enc = Encoding.GetEncoding("EUC-JP");
            StreamReader sw = new StreamReader(br.BaseStream, enc);

            char c = (char)sw.Read();
            int i = 0;
            while(c != '\0' && i < 0x100)
            {
                result += c;
                c = (char)sw.Read();
                i++;
            }
            br.BaseStream.Position = jumpBack;
            return result;
        }
        
        private static string ParseG_MTX(BinaryReader br, Microcode microcode, bool simpleParse)
        {
            Matrix m;
            string result;
            List<G_MTX_[]> Type = new List<G_MTX_[]>()
            {
                new G_MTX_[] { G_MTX_.G_MTX_NOPUSH, G_MTX_.G_MTX_PUSH},
                new G_MTX_[] { G_MTX_.G_MTX_MUL, G_MTX_.G_MTX_LOAD},
                new G_MTX_[] { G_MTX_.G_MTX_MODEL_VIEW, G_MTX_.G_MTX_PROJECTION}
            };

            var lowerHigh = microcode.EncodingHigh & 0xFFFF;
            result = string.Format("{0} // {1} ({2}, {3}, {4})", microcode, microcode.Name,
                Type[2][(lowerHigh >> 2) & 1],
                Type[1][(lowerHigh >> 1) & 1],
                Type[0][(lowerHigh >> 0) & 1]);

            if (!simpleParse)
            {
                var jumpback = br.BaseStream.Position;
                br.BaseStream.Position = microcode.EncodingLow & 0xFFFFFF;

                m = new Matrix(br);
                br.BaseStream.Position = jumpback;
                result += Environment.NewLine + m.ToString();
            }

            return result;
        }
    }
}
