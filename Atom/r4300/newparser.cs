//using mzxrules.Helper;
//using mzxrules.OcaLib;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using static Atom.MipsFields;
//using static Atom.Opcodes;

//namespace Atom
//{
//    class newparser
//    {
//        private static Opcodes GetOpcode(uint op)
//        {
//            return (Opcodes)(op & 0xFC00_0000);
//        }

//        private static SpecialClass GetSpecialClass(uint op)
//        {
//            return (SpecialClass)(op & 0x3F);
//        }

//        private static bool IsCode(uint op, SpecialClass code)
//        {
//            return (GetOpcode(op) == Opcodes.SPECIAL) && GetSpecialClass(op) == code;
//        }

//        private static void InitRelocationLabels(BinaryReader br, DisassemblyTask task, Dictionary<N64Ptr, Label> symbols, Dictionary<N64Ptr, N64Ptr> RelocationLabels)
//        {
//            N64Ptr hi_addr = 0;

//            N64Ptr start = task.VRam.Start;
//            List<N64PtrRange> textSections = GetTextSectionRanges(task);

//            foreach (var reloc in task.Relocations)
//            {
//                N64Ptr relAddr = task.VRam.Start + reloc.Offset;
//                br.BaseStream.Position = reloc.Offset;

//                if (reloc.RelocType == Reloc.R_MIPS_HI16
//                    || reloc.RelocType == Reloc.R_MIPS_LO16)
//                {
//                    GetOP(br.ReadBigUInt32());
//                    if (reloc.RelocType == Reloc.R_MIPS_HI16)
//                    {
//                        hi_addr = relAddr;
//                    }
//                    else if (reloc.RelocType == Reloc.R_MIPS_LO16)
//                    {
//                        //Fixes bug where gcc generates a R_MIPS_HI16 
//                        //after LO16 that is part of the same chain of LO16s
//                        if (!RelocationLabels.ContainsKey(hi_addr))
//                        {
//                            RelocationLabels[hi_addr] = Rel_Label_Addr;
//                        }
//                        RelocationLabels[relAddr] = Rel_Label_Addr;
//                    }
//                }

//                else if (reloc.RelocType == Reloc.R_MIPS32)
//                {
//                    N64Ptr ptr = br.ReadBigInt32();

//                    if (textSections.Exists(x => x.IsInRange(ptr)))
//                    {
//                        AddLabel(ptr, false);
//                    }
//                    else
//                        Symbols[ptr] = new Label(Label.Type.VAR, ptr, false, mips_to_c: Disassemble.MipsToC);
//                }
//            }

//            br.BaseStream.Position = 0;
//        }

//        private static List<N64PtrRange> GetTextSectionRanges(DisassemblyTask task)
//        {
//            List<N64PtrRange> textSections = new List<N64PtrRange>();
//            foreach (var section in task.Sections.Values.Where(x => x.IsCode))
//            {
//                textSections.Add(new N64PtrRange(section.VRam, section.VRam + section.Size));
//            }

//            return textSections;
//        }

//        internal static void FirstParse(BinaryReader br, Section section, Dictionary<N64Ptr, Label> symbols)
//        {
//            br.BaseStream.Position = section.Offset;
//            N64Ptr pc = section.VRam;
//            N64Ptr endOfFunction = -1;

//            uint word;
//            Opcodes opId;

//            int text_size = section.Size;

//            //Start of text will always be some function
//            AddFunction(symbols, pc, true);

//            // First scan - just to map out branches/jumps 
//            for (int i = 0; i < text_size; i += 4)
//            {
//                word = br.ReadBigUInt32();
//                opId = GetOpcode(word);
//                if (opId == Opcodes.J || opId == Opcodes.JAL)
//                {
//                    var addr = (pc & 0xFC000000) | TARGET(word);
//                    AddFunction(symbols, addr, true);
//                }

//                if (IsCode(word, SpecialClass.JR) && RS(word) == 0x1F)
//                {
//                    endOfFunction = pc + 4;
//                }    

//                if (pc == (endOfFunction + 4))
//                {
//                    if (word != 0)
//                    {
//                        AddFunction(symbols, pc, false);
//                    }
//                    else
//                        endOfFunction += 4;
//                }
//                pc += 4;
//            }
//        }

//        static Label AddLabel(Dictionary<N64Ptr, Label> symbols, N64Ptr addr, bool confirmed /* = true */)
//        {
//            if (!symbols.TryGetValue(addr, out Label label)
//                || (label.Confirmed == false && confirmed == true))
//            {
//                label = new Label(Label.Type.LBL, addr, confirmed, mips_to_c: Disassemble.MipsToC);
//                symbols[addr] = label;
//            }

//            return label;
//        }

//        static void AddFunction(Dictionary<N64Ptr, Label> symbols, N64Ptr addr, bool confirmed)
//        {
//            // Label already mapped?
//            if (symbols.TryGetValue(addr, out Label lbl))
//            {
//                if (lbl.Kind != Label.Type.FUNC)
//                {
//                    if (confirmed || !lbl.Confirmed)
//                    {
//                        lbl.Kind = Label.Type.FUNC;
//                    }
//                }
//                if (lbl.Kind == Label.Type.FUNC)
//                {
//                    lbl.hits++;
//                    if (confirmed)
//                        lbl.Confirmed = confirmed;
//                }
//            }
//            else
//            {
//                symbols.Add(addr, new Label(Label.Type.FUNC, addr, confirmed, mips_to_c: Disassemble.MipsToC));
//            }
//        }
//    }
//}
