using mzxrules.Helper;
using System.IO;

namespace uCode
{

    public class Microcode
    {
        public uint EncodingHigh { get; private set; }
        public uint EncodingLow { get; private set; }
        public G_ Name
        {
            get
            {
                try { return (G_)(EncodingHigh >> 24); }
                catch { return G_.G_NOOP; }
            }
        }
        public Microcode(BinaryReader br)
        {
            EncodingHigh = br.ReadBigUInt32();
            EncodingLow = br.ReadBigUInt32();
        }
        public Microcode(uint hi, uint low)
        {
            EncodingHigh = hi;
            EncodingLow = low;
        }
        public Microcode(uint hi, N64Ptr low) : this(hi, (uint)low) { }

        public override string ToString()
        {
            return $"{EncodingHigh:X8} {EncodingLow:X8}";
        }
    }
}