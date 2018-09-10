using mzxrules.Helper;
using mzxrules.OcaLib;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

using static Atom.MipsFields;
namespace Atom
{
    public partial class Disassemble
    {
        /// <summary>
        /// Performs a dissassembly using the initialized function list
        /// </summary>
        /// <param name="sw">the output disassembly</param>
        /// <param name="br">source file to disassemble</param>
        /// <param name="task">meta-data container for the disassembly task</param>
        internal static void TextDisassembly(StreamWriter sw, BinaryReader br, DisassemblyTask task)
        {
            sw.WriteLine(".text");
            SimpleDisassebly(sw, br, task.VRam.Start, task.Sections["text"].Size);
        }

        internal static void SimpleDisassebly(StreamWriter sw, BinaryReader br, N64Ptr pcount, int textsize)
        {
            //disable locating branches
            First_Parse = false;
            jaltaken = 0;

            pc = pcount;
            //pc = task.OvlRecord.VRam.Start;
            br.BaseStream.Position = 0;
            Reset_Gpr_Regs();

            for (int i = 0; i < textsize; i += 4)
            {
                if (Labels.ContainsKey(pc))
                {
                    var label = Labels[pc];

                    //if function start
                    if (label.Kind == Label.Type.FUNC)
                    {
                        PrintFunctionStart(sw);
                        Reset_Gpr_Regs();
                    }
                    else
                    {
                        sw.WriteLine($"{label}:");
                    }
                }

                //read and write opcode
                int word = br.ReadBigInt32();
                sw.WriteLine($"\t{FormatOp(GetOP(word))}");

                if (jaltaken != 0)
                {
                    jaltaken--;
                    if (!(jaltaken != 0))
                        Reset_Gpr_Regs_Soft();
                }

                pc += 4;

                if (Labels.ContainsKey(pc)
                    && Labels[pc].Kind == Label.Type.FUNC)
                {

                    sw.WriteLine();
                    sw.WriteLine();
                }
            }

        }

        internal static void DataDisassembly(StreamWriter w, BinaryReader br, DisassemblyTask task)
        {
            br.BaseStream.Position = task.Sections["text"].Size;
            var rel = task.Relocations.Where(x => x.RelocType == Reloc.R_MIPS32);
            var ovlStart = new N64Ptr(task.VRam.Start);
            var bssEnd = new N64Ptr(task.VRam.End);


            var dataStart = ovlStart + task.Sections["text"].Size;
            var rodataStart = dataStart + task.Sections["data"].Size;
            var rodataEnd = rodataStart + task.Sections["rodata"].Size;
            var bssStart = Align.To16(new N64Ptr(rodataEnd + 0x14 + (task.Relocations.Count * 4)));

            (string name, N64Ptr start, N64Ptr end)[] blocks =
            {
                (".data", dataStart, rodataStart),
                (".section .rodata", rodataStart, rodataEnd)
            };

            //.data and .rodata sections
            for (int blockId = 0; blockId < 2; blockId++)
            {
                var (name, start, end) = blocks[blockId];
                List<byte> byteChain = new List<byte>();
                w.WriteLine();
                w.WriteLine(name);
                w.WriteLine();
                pc = start;
                while (pc < end)
                {
                    if (Labels.ContainsKey(pc))
                    {
                        DumpByteChain(w, ref byteChain);
                        w.Write($"{Labels[pc]}: ");
                    }

                    if (rel.Any(x => x.Offset == br.BaseStream.Position))
                    {
                        DumpByteChain(w, ref byteChain);
                        N64Ptr lbl = br.ReadBigInt32();
                        pc += 4;
                        w.WriteLine($".word {Labels[lbl]}");
                    }
                    else
                    {
                        byteChain.Add(br.ReadByte());
                        pc += 1;
                    }
                }
                DumpByteChain(w, ref byteChain);
            }

            Bss(w, bssStart, bssEnd);
        }

        private static void Bss(StreamWriter w, N64Ptr bssStart, N64Ptr bssEnd)
        {
            //.bss
            var bssLabels = Labels.Where(x => x.Key >= bssStart && x.Key < bssEnd).Select(x => x.Value).OrderBy(x => x.Addr).ToList();
            if (bssLabels.Count == 0)
                return;

            bssLabels.Add(new Label(Label.Type.VAR, bssEnd, false));
            w.WriteLine();
            w.WriteLine(".bss");
            w.WriteLine();
            var firstBss = bssLabels[0];
            if (firstBss.Addr != bssStart)
            {
                w.WriteLine($".space 0x{firstBss.Addr - bssStart:X2}");
            }
            for (int i = 0; i < bssLabels.Count - 1; i++)
            {
                var cur = bssLabels[i];
                var next = bssLabels[i + 1];
                w.Write($"{cur}: ");
                w.WriteLine($".space 0x{next.Addr - cur.Addr:X2}");
            }
        }

        private static void DumpByteChain(StreamWriter outputf, ref List<byte> bytes)
        {
            const int width = 16;

            if (bytes.Count == 0)
                return;

            bool printWords = bytes.Count % 4 == 0;
            string type = (printWords) ? "word" : "byte";

            if (bytes.Count > width)
            {
                outputf.WriteLine($".{type} \\");
            }
            else
            {
                outputf.Write($".{type} ");
            }

            for (int i = 0; i < bytes.Count; i += width)
            {
                var loop = (bytes.Count - i < width) ? bytes.Count - i : width;
                var line = (printWords) ? GetLineWord(bytes, i, loop) : GetLineByte(bytes, i, loop);
                outputf.Write(line);
                if (i + width < bytes.Count)
                    outputf.WriteLine(", \\");
            }
            outputf.WriteLine();
            bytes.Clear();
            
            string GetLineByte(List<byte> chain, int index, int count)
            {
                List<string> values = new List<string>();
                foreach (var item in chain.GetRange(index, count))
                {
                    values.Add($"0x{item:X2}");
                }
                return string.Join(", ", values);
            }
            string GetLineWord(List<byte> chain, int index, int count)
            {
                List<string> values = new List<string>();
                List<byte> items = chain.GetRange(index, count);
                for (int i = 0; i < count; i += 4)
                {
                    values.Add($"0x{items[i + 0]:X2}{items[i + 1]:X2}{items[i + 2]:X2}{items[i + 3]:X2}");
                }
                return string.Join(", ", values);
            }
        }
        
        public static string FormatOp(string op)
        {
            string comment = "";
            string opStr = op;
            var index = op.IndexOf("##");
            if (index > 0)
            {
                comment = op.Substring(index);
                opStr = op.Substring(0, index).Trim();
            }
            string[] ops = opStr.Split(new char[] { '\t' });
            if (ops.Length != 2)
            {
                return op;
            }

            return $"{ops[0],-7} {ops[1],-26} {comment}";
        }

        private static void PrintFunctionStart(StreamWriter sw)
        {
            Label curFunc = Labels[pc];

            sw.WriteLine($"{curFunc}:");
            PrintCommentLines(curFunc.Name);
            PrintCommentLines(curFunc.Desc);
            PrintCommentLines(curFunc.Desc2);
            PrintCommentLines(curFunc.Args);
            //
            void PrintCommentLines(string v)
            {
                if (!string.IsNullOrWhiteSpace(v))
                    foreach (var line in v.Split(new char[] { ';' }))
                        sw.WriteLine($"## {line}");
            }
        }

        #region SymbolDetection

        /// <summary>
        /// Performs a first pass over the assembly, computing various constants 
        /// </summary>
        /// <param name="br"></param>
        /// <param name="task"></param>
        internal static void FirstParse(BinaryReader br, DisassemblyTask task)
        {
            pc = task.VRam.Start;

            int word;
            int opId;

            int text_size = task.Sections["text"].Size;

            First_Parse = true;
            InitRelocationLabels(br, task);

            Reset_Gpr_Regs();

            //Start of text will always be some function
            AddFunction(pc, true);

            // First scan - just to map out branches/jumps 
            for (int i = 0; i < text_size; i += 4)
            {
                word = br.ReadBigInt32();
                GetOP(word);
                opId = (word >> 26) & 0x3F;
                if (opId == 2 || opId == 3)
                {
                    var addr = TARGET((uint)word) | 0x80000000;
                    AddFunction(addr, true);
                }

                if (pc == (EndOfFunction + 4))
                {
                    if (word != 0)
                    {
                        AddFunction(pc, false);
                        Reset_Gpr_Regs();
                    }
                    else
                        EndOfFunction += 4;
                }
                pc += 4;
            }
            First_Parse = false;
            Reset_Gpr_Regs();

            //complete the label list
            RemoveFalseFunctions();

            foreach (var addr in RelocationLabels.Values)
            {
                if (!Labels.ContainsKey(addr))
                {
                    var v = addr - new N64Ptr(task.VRam.Start);
                    if (v < text_size)
                    {
                        Labels[addr] = new Label(Label.Type.LBL, addr);
                    }
                    else
                    {
                        Labels[addr] = new Label(Label.Type.VAR, addr);
                    }
                }
            }
        }

        private static void RemoveFalseFunctions()
        {
            foreach (var func in Labels.Values.Where(x => x.Kind == Label.Type.FUNC && x.Confirmed == false))
            {
                if (Labels.TryGetValue(func.Addr + 4, out Label lbl))
                {
                    func.Kind = Label.Type.LBL;
                }
            }
        }

        private static void InitRelocationLabels(BinaryReader br, DisassemblyTask task)
        {
            N64Ptr hi_addr = 0;
            Rel_Parse = true;

            N64Ptr start = task.VRam.Start;
            N64Ptr text_end = start + task.Sections["text"].Size;

            foreach (var reloc in task.Relocations)
            {
                N64Ptr relAddr = task.VRam.Start + reloc.Offset;
                br.BaseStream.Position = reloc.Offset;

                if (reloc.RelocType == Reloc.R_MIPS_HI16
                    || reloc.RelocType == Reloc.R_MIPS_LO16)
                {
                    GetOP(br.ReadBigInt32());
                    if (reloc.RelocType == Reloc.R_MIPS_HI16)
                    {
                        hi_addr = relAddr;
                    }
                    else if (reloc.RelocType == Reloc.R_MIPS_LO16)
                    {
                        //Fixes bug where gcc generates a R_MIPS_HI16 
                        //after LO16 that is part of the same chain of LO16s
                        if (!RelocationLabels.ContainsKey(hi_addr))
                        {
                            RelocationLabels[hi_addr] = Rel_Label_Addr;
                        }
                        RelocationLabels[relAddr] = Rel_Label_Addr;
                    }
                }

                else if (reloc.RelocType == Reloc.R_MIPS32)
                {
                    N64Ptr ptr = br.ReadBigInt32();
                    if (ptr >= start && ptr < text_end)
                    {
                        AddLabel(ptr, false);
                    }
                    else
                        Labels[ptr] = new Label(Label.Type.VAR, ptr);
                }
            }

            Rel_Parse = false;
            br.BaseStream.Position = 0;
        }

        internal static List<Label> GetFunctions()
        {
            return Labels.Values.Where(x => x.Kind == Label.Type.FUNC).ToList();
        }

        
        internal static void AddFunctions(IEnumerable<FunctionInfo> funcs)
        {
            foreach(var f in funcs)
            {
                AddFunction(new Label(f));
            }
        }

        internal static void AddFunction(Label f)
        {
            if (!Labels.ContainsKey(f.Addr))
                Labels.Add(f.Addr, f);
        }
        
        /// <summary>
        /// Should return bool
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="confirmed"></param>
        /// <returns></returns>
        static void AddFunction(N64Ptr addr, bool confirmed)
        {
            addr = addr | 0x80000000;

            // Label already mapped?
            if (Labels.TryGetValue(addr, out Label lbl))
            {
                if (lbl.Kind != Label.Type.FUNC)
                {
                    if (confirmed || !lbl.Confirmed)
                    {
                        lbl.Kind = Label.Type.FUNC;
                    }
                }
                if (lbl.Kind == Label.Type.FUNC)
                {
                    lbl.hits++;
                    if (confirmed)
                        lbl.Confirmed = confirmed;
                }
            }
            else
            {
                Labels.Add(addr, new Label(Label.Type.FUNC, addr, confirmed));
            }
        }

        static Label AddLabel(N64Ptr addr, bool confirmed = true)
        {
            if (!Labels.TryGetValue(addr, out Label label)
                || (label.Confirmed == false && confirmed == true))
            {
                label = new Label(Label.Type.LBL, addr, confirmed);
                Labels[addr] = label;
            }

            return label;
        }

        #endregion

    }
}
//useless and spammy
//outputf.Write("\t.set\tnoreorder\n\t.set\tnoat\n\t.global\t{0}\n\t.ent\t{1}\n\n", currFuncName, currFuncName);
//fprintf(outputf, "\n\t.end\t%s\n\t.set\tat\n\t.set\tnoreorder\n\n    /* #~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~# */\n\n", currFuncName);