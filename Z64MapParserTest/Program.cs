using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using mzxrules.OcaLib.SymbolMapParser;
using mzxrules.Helper;
using System.Linq;

namespace Z64MapParserTest
{
    class Program
    {
        const string TestPath = @"C:\Users\mzxrules\Source\Repos\oot\build\z64.map";
        static void Main(string[] args)
        {
            var info = SymbolMapParser.Parse(TestPath);
            PrintData(info.Map);
            while (true)
            {
                Console.Write("Input:");
                var input = Console.ReadLine();
                var romAddr = int.Parse(input, System.Globalization.NumberStyles.HexNumber);

                foreach (Segment segment in info.Map)
                {
                    if (!segment.HasLoadAddress)
                    {
                        continue;
                    }

                    FileAddress fileAddr = new(segment.LoadAddress, segment.LoadAddress + segment.Size);
                    if (fileAddr.Start <= romAddr && romAddr < fileAddr.End)
                    {
                        int offset = romAddr - fileAddr.Start;
                        N64Ptr addr = segment.Address + offset;

                        Section section = segment.Sections.SingleOrDefault(x => x.Address <= addr && addr < (x.Address + x.Size));
                        Symbol sym = section.Symbols.SingleOrDefault(x => x.Address == addr);
                        if (sym != null)
                        {
                            Console.WriteLine($"{sym.Address}");
                            Console.WriteLine(
                                $"<MapBinding>\n" +
                                $"  <Segment>{segment.Name}</Segment>\n" +
                                $"  <File>{section.File}</File>\n" +
                                $"  <Symbol>{sym.Name}</Symbol>\n" +
                                $"</MapBinding>");
                        }
                        else
                        {
                            Console.WriteLine($"NOT FOUND: {addr} {segment.Name} {section.File}");
                        }    
                        break;
                    }

                }
            }    
        }

        private static void PrintData(List<Segment> info)
        {
            StringBuilder sb = new();
            foreach (Segment segment in info)
            {
                sb.Append($"{segment}\n");
                foreach (Section section in segment.Sections)
                {
                    sb.Append($" {section}\n");
                    foreach (Symbol sym in section.Symbols)
                    {
                        sb.Append($"    {sym}\n");
                    }
                }
            }
            File.WriteAllText("test.txt", sb.ToString());
        }
    }
}
