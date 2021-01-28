using mzxrules.Helper;
using System.Collections.Generic;

namespace mzxrules.OcaLib.SymbolMapParser
{
    public class Section
    {
        public string Name { get; set; }
        public N64Ptr Address { get; set; }
        public uint Size { get; set; }
        public string File { get; set; }

        public List<Symbol> Symbols = new List<Symbol>();

        public override string ToString()
        {
            return $"{Name} {Address} 0x{Size:X} {File}";
        }
    }
}
