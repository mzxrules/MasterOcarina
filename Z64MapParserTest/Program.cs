using System;
using System.IO;
using System.Text;
using mzxrules.OcaLib.SymbolMapParser;

namespace Z64MapParserTest
{
    class Program
    {
        const string TestPath = @"C:\Users\mzxrules\Source\Repos\oot\build\z64.map";
        static void Main(string[] args)
        {
            var info = Parser.Parse(TestPath);
            StringBuilder sb = new StringBuilder();
            foreach (Segment segment in info)
            {
                sb.Append($"{segment}\n");
                foreach (Section section in segment.Sections)
                {
                    sb.Append($" {section}\n");
                    foreach(Symbol sym in section.Symbols)
                    {
                        sb.Append($"    {sym}\n");
                    }
                }    
            }
            File.WriteAllText("test.txt", sb.ToString());
        }
    }
}
