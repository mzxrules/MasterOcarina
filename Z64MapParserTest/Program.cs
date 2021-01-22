using System;
using mzxrules.SymbolParser;

namespace Z64MapParserTest
{
    class Program
    {
        const string TestPath = @"C:\Users\mzxrules\Source\Repos\oot\build\z64.map";
        static void Main(string[] args)
        {
            Parser p = new Parser();
            p.Parse(TestPath);

        }
    }
}
