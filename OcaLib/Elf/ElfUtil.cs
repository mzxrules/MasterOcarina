using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using mzxrules.Helper;
using System.Xml.Serialization;
using System.Xml;

namespace mzxrules.OcaLib.Elf
{
    public class ElfUtil
    {
        static string[] ovlSectionsStr = { ".text", ".data", ".rodata", ".bss" };
        
        internal class Ovl
        {
            public N64Ptr start;
            public N64Ptr header;
            public N64Ptr pEnd;
            public N64Ptr vEnd;

            public Dictionary<string, string> BindingSymbolName = new Dictionary<string, string>();
            public Dictionary<string, N64Ptr> BindingValue = new Dictionary<string, N64Ptr>();

            public void UpdateActorOverlay(BinaryWriter rom, int actorTable, int actor, uint vromStart, uint dmadata)
            {
                uint vromEnd = vromStart + (uint)(pEnd - start);
                string dmawriteback = (dmadata == 0) ? "No Writeback" : $"{dmadata:X8}";
                Console.WriteLine($"{vromStart:X8}:{vromEnd:X8} - Actor {actor:X4}; dmadata - {dmawriteback}");

                if (dmadata > 0)
                {
                    //update dmadata
                    rom.BaseStream.Position = dmadata;
                    rom.WriteBig(vromStart);
                    rom.WriteBig(vromEnd);
                    rom.WriteBig(vromStart);
                    rom.WriteBig(0);
                }

                rom.BaseStream.Position = actorTable + (actor * 0x20);

                rom.WriteBig(vromStart);
                rom.WriteBig(vromEnd);
                rom.WriteBig(start);
                rom.WriteBig(vEnd);
                rom.Seek(4, SeekOrigin.Current);
                rom.WriteBig(BindingValue["init"]);
            }
        }

        public static void CreateDummyScript(string path)
        {
            Script script = new Script
            {
                Rom = new XRom()
                {
                    Path = "Rom Path",
                    ActorTable = "Actor table location, no hex specifier.",
                },
                Actor = new List<XActor>()
                {
                     new XActor()
                     {
                         Path = "Path to gcc object file (*.o)",
                         InitBinding = "Symbol name of custom actor's initialization variables within your source .o file",
                         Id = "Actor Id, Hexadecimal",
                         DMA = "Set dmadata table record offset. optional... either set to 0 or delete DMA element",
                         VRam = "Virtual ram address to inject overlay",
                         VRom = "Virtual rom address to inject overlay"
                     },
                     new XActor()
                     {
                         Path = "Example Actor:",
                         InitBinding = "initVars",
                         Id = "00D8",
                         DMA = "D280",
                         VRom = "0347E000",
                         VRam = "80B87280"
                     }
                },
            };
            script.SaveToFile(path);
        }

        public static void ProcessInjectScript(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Script));
            Script script;
            try
            {
                using (XmlReader reader = XmlReader.Create(new FileStream(path, FileMode.Open)))
                {
                    script = (Script)serializer.Deserialize(reader);
                }
                ProcessInjectScript(script);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ProcessInjectScript(Script script)
        {
            if (!File.Exists(script.Rom.Path))
            {
                Console.WriteLine($"Cannot find inject rom {script.Rom.Path}");
                return;
            }
            if (!int.TryParse(script.Rom.ActorTable, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int actorTable))
            {
                Console.WriteLine($"Cannot parse Actor Table address {script.Rom.ActorTable}");
                return;
            }

            Console.WriteLine($"Rom: {script.Rom.Path}");
            Console.WriteLine($"Actor Table: {script.Rom.ActorTable:X8}");

            using (FileStream rom = new FileStream(script.Rom.Path, FileMode.Open))
            {
                foreach (var actor in script.Actor)
                {
                    if (!File.Exists(actor.Path))
                    {
                        Console.WriteLine($"Cannot find file {actor.Path}");
                        continue;
                    }
                    if (!int.TryParse(actor.Id, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int id)
                        || !uint.TryParse(actor.VRam, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint vram)
                        || !uint.TryParse(actor.VRom, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint vrom))
                    {
                        Console.WriteLine($"Invalid parameter for actor {actor.Path}");
                        continue;
                    }

                    if (!uint.TryParse(actor.DMA, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint dma))
                    {
                        dma = 0;
                    }

                    if (!File.Exists(actor.Path))
                    {
                        Console.WriteLine($"Cannot locate {actor.Path}");
                        continue;
                    }
                    Ovl odata = new Ovl
                    {
                        start = vram,
                        BindingSymbolName = { { "init", actor.InitBinding } }
                    };
                    byte[] data = new byte[0];
                    using (FileStream elf = new FileStream(actor.Path, FileMode.Open))
                    {
                        BinaryReader br = new BinaryReader(elf);
                        if (!TryConvertToOverlay(br, odata, out data))
                        {
                            Console.WriteLine($"Conversion of {actor.Path} failed!");
                            continue;
                        }
                    }


                    odata.UpdateActorOverlay(new BinaryWriter(rom), actorTable, id, vrom, dma);
                    rom.Position = vrom;
                    rom.Write(data, 0, data.Length);
                }
                CRC.Write(rom);
            }
        }

        public static bool TryConvertToOverlay(string elfpath, string ovlpath, N64Ptr rpoint)
        {
            Ovl odata = new Ovl
            {
                start = rpoint,
                BindingSymbolName = { {"init", "initVars" } }
            };

            bool result = false;

            MemoryStream ms = new MemoryStream();

            using (FileStream elfFile = new FileStream(elfpath, FileMode.Open))
            {
                elfFile.CopyTo(ms);
                ms.Position = 0;
            }
            result = TryConvertToOverlay(new BinaryReader(ms), odata, out byte[] ovlByte);

            using (FileStream fs = new FileStream(ovlpath, FileMode.Create))
            {
                fs.Write(ovlByte, 0, ovlByte.Length);
            }

            return result;
        }

        private static bool TryConvertToOverlay(BinaryReader elf, Ovl odata, out byte[] ovlByte)
        {
            ovlByte = new byte[0];
            FileHeader header = new FileHeader(elf);
            if (!header.VerifyMagic())
            {
                Console.WriteLine("Not an elf, or not a supported elf format");
                return false;
            }

            //Extract section headers
            List<SectionHeader> sections = GetSections(elf, header);

            //build memory map
            int relocations = GetRelocations(sections);

            Dictionary<string, SectionHeader> map = new Dictionary<string, SectionHeader>();
            Dictionary<string, (N64Ptr addr, int size)> newMap = new Dictionary<string, (N64Ptr addr, int size)>();
            
            N64Ptr cur = odata.start;

            for (int i = 0; i < ovlSectionsStr.Length; i++)
            {
                var secName = ovlSectionsStr[i];
                if (secName == ".bss")
                {
                    odata.header = cur - odata.start;
                    cur += 0x14 + (relocations * 4) + 4; //header size, reloction size, header offset
                    cur = Align.To16(cur );
                    odata.pEnd = cur;
                }
                N64Ptr localStart = cur;
                foreach (var item in sections.Where(x => x.Name.StartsWith(secName)))
                {
                    map[item.Name] = item;
                    item.NS = new SectionHelper(item, cur, cur - odata.start, i + 1, cur - localStart);
                    cur = Align.To16(cur + item.sh_size);
                }
                if (secName == ".bss")
                {
                    odata.vEnd = cur;
                }
                int size = cur - localStart;
                newMap[secName] = (localStart, size);
            }

            //get relocated symtables
            Dictionary<int, List<Elf32_Sym>> symtabs = GetSymbols(elf, sections);

            //create our file
            ovlByte = new byte[odata.pEnd - odata.start];
            List<Overlay.RelocationWord> ovlRelocations = new List<Overlay.RelocationWord>();
            using (BinaryWriter ovl = new BinaryWriter(new MemoryStream(ovlByte)))
            {
                //copy data into the overlay
                foreach (var kv in map.OrderBy(x => x.Value.NS.Addr))
                {
                    var (sec, ptr, fOff, newSecId, sOff) = kv.Value.NS;
                    if (sec == null)
                        continue;

                    if (sec.sh_type == SectionHeader.SHT_.PROGBITS)
                    {
                        ovl.BaseStream.Position = fOff;
                        elf.BaseStream.Position = sec.sh_offset;
                        byte[] data = elf.ReadBytes(sec.sh_size);
                        ovl.Write(data);
                    }
                }

                //handle relocations
                foreach (var section in sections.Where(x => x.sh_type == SectionHeader.SHT_.REL))
                {
                    List<Elf32_Rel> rel = GetSectionRelocations(elf, section);
                    var secInfo = sections[section.sh_info];
                    if (secInfo.Name == ".pdr")
                        continue;
                    var (sec, ptr, fOff, newSecId, sOff) = secInfo.NS; //Sets section that REL applies to
                    var symtab = symtabs[section.sh_link];

                    for (int i = 0; i < rel.Count; i++)
                    {
                        var relCur = rel[i];
                        var symbol = symtab[relCur.R_Sym];
                        if (symbol.st_shndx == 0)
                        {
                            Console.WriteLine($"{symbol.Name}: Undefined Section");
                            continue;
                        }

                        N64Ptr relSecAddr = sections[symbol.st_shndx].NS.Addr;
                        switch (relCur.R_Type)
                        {
                            case Reloc.R_MIPS32:
                                {
                                    ovl.BaseStream.Position = fOff + relCur.r_offset;
                                    elf.BaseStream.Position = sec.sh_offset + relCur.r_offset;
                                    var data = elf.ReadBigInt32() + relSecAddr + symbol.st_value;
                                    ovl.WriteBig(data);
                                    ovlRelocations.Add(new Overlay.RelocationWord((Overlay.Section)newSecId, Reloc.R_MIPS32, sOff + relCur.r_offset));
                                    break;
                                }
                            case Reloc.R_MIPS26:
                                {
                                    ovl.BaseStream.Position = fOff + relCur.r_offset;
                                    elf.BaseStream.Position = sec.sh_offset + relCur.r_offset;
                                    var data = elf.ReadBigUInt32();
                                    var newAddrDat = (uint)((relSecAddr + symbol.st_value >> 2) & 0x3FFFFFF);
                                    data += newAddrDat;
                                    ovl.WriteBig(data);
                                    ovlRelocations.Add(new Overlay.RelocationWord((Overlay.Section)newSecId, Reloc.R_MIPS26, sOff + relCur.r_offset));
                                    break;
                                }
                            case Reloc.R_MIPS_HI16:
                                {
                                    if (!(i + 1 < rel.Count))
                                    {
                                        Console.WriteLine("R_MIPS_HI16 without a proper pair");
                                        return false;
                                    }

                                    i++;
                                    var relLo = rel[i];
                                    //have hi and lo relocations

                                    elf.BaseStream.Position = sec.sh_offset + relCur.r_offset + 2;
                                    var hiData = elf.ReadBigUInt16();
                                    elf.BaseStream.Position = sec.sh_offset + relLo.r_offset;
                                    var loData = elf.ReadBigUInt32();

                                    N64Ptr hiAddr = hiData << 16;
                                    N64Ptr addr = hiAddr + (loData & 0xFFFF) + relSecAddr + symtab[relLo.R_Sym].st_value;

                                    var hi16 = (ushort)((addr >> 16));
                                    var lo16 = (ushort)(addr & 0xFFFF);
                                    bool isOri = ((loData >> 26) & 0x3F) == 0x0D;

                                    if (lo16 > 0x8000 && !isOri)
                                    {
                                        hi16++;
                                    }

                                    ovl.BaseStream.Position = fOff + relCur.r_offset + 2;
                                    ovl.WriteBig(hi16);

                                    ovl.BaseStream.Position = fOff + relLo.r_offset + 2;
                                    ovl.WriteBig(lo16);

                                    ovlRelocations.Add(new Overlay.RelocationWord((Overlay.Section)newSecId, Reloc.R_MIPS_HI16, sOff + relCur.r_offset));
                                    ovlRelocations.Add(new Overlay.RelocationWord((Overlay.Section)newSecId, Reloc.R_MIPS_LO16, sOff + relLo.r_offset));
                                    
                                    //Write repeating R_MIPS_LO16s
                                    while (i+1 < rel.Count)
                                    {
                                        i++;
                                        relLo = rel[i];
                                        if (relLo.R_Type != Reloc.R_MIPS_LO16)
                                        {
                                            i--;
                                            break;
                                        }

                                        elf.BaseStream.Position = sec.sh_offset + relLo.r_offset + 2;
                                        loData = elf.ReadBigUInt16();
                                        addr = hiAddr + (loData & 0xFFFF) + relSecAddr + symtab[relLo.R_Sym].st_value;
                                        lo16 = (ushort)(addr & 0xFFFF);
                                        ovl.BaseStream.Position = fOff + relLo.r_offset + 2;
                                        ovl.WriteBig(lo16);
                                        ovlRelocations.Add(new Overlay.RelocationWord((Overlay.Section)newSecId, Reloc.R_MIPS_LO16, sOff + relLo.r_offset));
                                    }

                                    break;
                                }
                            case Reloc.R_MIPS_LO16:
                                {
                                    Console.WriteLine($"R_MIPS_LO16 {fOff + relCur.r_offset:X6} without a proper pair");
                                    return false;
                                }
                            default:
                                {
                                    Console.WriteLine("Incompatible R_MIPS type, cannot convert");
                                    return false;
                                }
                        }
                    }
                }

                ovl.BaseStream.Position = odata.header;

                foreach (var name in ovlSectionsStr)
                    ovl.WriteBig(newMap[name].size);
                ovl.WriteBig(relocations);

                foreach (var item in ovlRelocations)
                {
                    ovl.WriteBig(item.Word);
                }
                ovl.BaseStream.Position = ovl.BaseStream.Length - 4;
                var seekback = (int)(ovl.BaseStream.Length - odata.header);
                ovl.WriteBig(seekback);
            }

            BindRelocatedSymbols(odata, sections, symtabs);

            return true;
        }

        private static void BindRelocatedSymbols(Ovl odata, List<SectionHeader> sections, Dictionary<int, List<Elf32_Sym>> symtabs)
        {
            var list = symtabs.Values.SelectMany(x => x);

            foreach(var kv in odata.BindingSymbolName)
            {
                var symbol = list.Where(x => x.Name == kv.Value).SingleOrDefault();
                if (symbol != null
                    && (symbol.st_shndx > 0 && symbol.st_shndx < sections.Count) )
                {
                    var section = sections[symbol.st_shndx];
                    if (section.NS != null)
                    {
                        odata.BindingValue[kv.Key] = symbol.st_value + section.NS.Addr;
                        continue;
                    }
                }
                Console.WriteLine($"Binding {kv.Key}<-{kv.Value} failed");
            }
        }

        private static Dictionary<int, List<Elf32_Sym>> GetSymbols(BinaryReader elf, List<SectionHeader> sections)
        {
            Dictionary<int, List<Elf32_Sym>> symtabs = new Dictionary<int, List<Elf32_Sym>>();

            foreach (var section in sections.Where(x => x.sh_type == SectionHeader.SHT_.REL))
            {
                var secInfo = sections[section.sh_info];
                if (secInfo.Name == ".pdr")
                    continue;

                var sh_link = section.sh_link;
                if (!symtabs.ContainsKey(sh_link))
                {
                    symtabs.Add(sh_link, InitializeSymtab(elf, sections, sh_link));
                }
            }

            return symtabs;
        }

        private static List<SectionHeader> GetSections(BinaryReader elf, FileHeader header)
        {
            List<SectionHeader> sections = new List<SectionHeader>();
            elf.BaseStream.Position = header.e_shoff;
            for (int i = 0; i < header.e_shnum; i++)
            {
                var section = new SectionHeader(elf);
                sections.Add(section);
            }
            
            //set section names
            var shstr = sections[header.e_shstrndx];

            elf.BaseStream.Position = shstr.sh_offset;
            MemoryStream ms = new MemoryStream(elf.ReadBytes(shstr.sh_size));
            foreach (var section in sections)
            {
                ms.Position = section.sh_name;
                section.Name = CStr.Get(ms);
            }

            return sections;
        }

        private static int GetRelocations(List<SectionHeader> sections)
        {
            int relocations = 0;
            foreach (var section in sections.Where(x => x.sh_type == SectionHeader.SHT_.REL))
            {
                var secInfo = sections[section.sh_info];
                if (secInfo.Name == ".pdr")
                    continue;
                
                var entries = section.sh_size / section.sh_entsize;
                relocations += entries;
            }

            return relocations;
        }

        private static List<Elf32_Sym> InitializeSymtab(BinaryReader elf, List<SectionHeader> sections, int symtabId)
        {
            SectionHeader symtab = sections[symtabId];
            SectionHeader strtab = sections[symtab.sh_link];

            List<Elf32_Sym> symbols = new List<Elf32_Sym>();
            var elements = symtab.sh_size / symtab.sh_entsize;
            elf.BaseStream.Position = symtab.sh_offset;
            for (int i = 0; i < elements; i++)
            {
                var symbol = new Elf32_Sym(elf);
                symbols.Add(symbol);
            }

            elf.BaseStream.Position = strtab.sh_offset;
            MemoryStream ms = new MemoryStream(elf.ReadBytes(strtab.sh_size));
            foreach (var symbol in symbols)
            {
                ms.Position = symbol.st_name;
                symbol.Name = CStr.Get(ms);
                
                //if (symbol.st_shndx > 0 && symbol.st_shndx < sections.Count)
                //{
                //    var section = sections[symbol.st_shndx];
                //    if (section.NS == null)
                //        continue;

                //    symbol.st_value += section.NS.Addr;
                //    symbol.st_shndx = Elf32_Sym.ABS;
                //}

            }
            return symbols;
        }
        
        private static List<Elf32_Rel> GetSectionRelocations(BinaryReader elf, SectionHeader section)
        {
            List<Elf32_Rel> rel = new List<Elf32_Rel>();

            elf.BaseStream.Position = section.sh_offset;
            for (int i = 0; i < section.sh_size / section.sh_entsize; i++)
            {
                rel.Add(new Elf32_Rel(elf));
            }
            return rel;
        }
    }
}
