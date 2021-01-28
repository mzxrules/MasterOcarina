using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mzxrules.Helper;
using mzxrules.OcaLib.Addr2;

namespace mzxrules.OcaLib.SymbolMapParser
{
    public class SymbolMap
    {
        public string Path { get; internal set; }
        public List<Segment> Map = new List<Segment>();


        public bool TryGetSymbolAddress(MapBinding binding, out N64Ptr ptr)
        {
            return TryGetSymbolAddress(binding.Segment, binding.File, binding.Symbol, out ptr);
        }

        public bool TryGetSymbolAddress(string segment, string file, string symbolname, out N64Ptr ptr)
        {
            foreach(Segment seg in Map.Where(x => x.Name == segment || x.Name == segment + ".bss"))
            {
                foreach(Section sec in seg.Sections.Where(x=>x.File == file))
                {
                    foreach(Symbol sym in sec.Symbols.Where(x => x.Name == symbolname))
                    {
                        ptr = sym.Address;
                        return true;
                    }
                }
            }
            ptr = 0;
            return false;
        }

        public Segment GetSegment(string segment)
        {
            return Map.SingleOrDefault(x => x.Name == segment);
        }
    }
}
