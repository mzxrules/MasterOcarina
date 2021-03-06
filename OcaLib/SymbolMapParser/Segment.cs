﻿using mzxrules.Helper;
using System.Collections.Generic;

namespace mzxrules.OcaLib.SymbolMapParser
{
    public class Segment
    {
        public string Name { get; set; }

        public N64Ptr Address { get; set; }

        public uint Size { get; set; }

        public uint LoadAddress { get; set; }

        public bool HasLoadAddress => !Name.EndsWith(".bss");

        public List<Symbol> Symbols = new List<Symbol>();

        public List<Section> Sections = new List<Section>();

        public override string ToString()
        {
            return $"..{Name} {Address} 0x{Size:X} {HasLoadAddress} {LoadAddress:X}";
        }
    }
}
