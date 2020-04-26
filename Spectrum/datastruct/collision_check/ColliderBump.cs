using mzxrules.Helper;

namespace Spectrum
{
    class ColliderBump
    {
        public int flags;
        public byte effect;
        public byte defense;
        public Vector3<short> unk_0x06;

        public ColliderBump(Ptr p)
        {
            flags = p.ReadInt32(0);
            effect = p.ReadByte(4);
            defense = p.ReadByte(5);
            unk_0x06 = new Vector3<short>(p.ReadInt16(0x06), p.ReadInt16(0x08), p.ReadInt16(0x0A));

        }

        public override string ToString()
        {
            return $"Bump:  [Flags: {flags:X8} Effect: {effect:X2} Defense: {defense:X2} unk_0x06: {unk_0x06}]";
        }
    }

}