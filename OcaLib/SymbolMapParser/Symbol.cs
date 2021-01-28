using mzxrules.Helper;

namespace mzxrules.OcaLib.SymbolMapParser
{
    public class Symbol
    {
        public N64Ptr Address { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{Address} {Name}";
        }
    }
}
