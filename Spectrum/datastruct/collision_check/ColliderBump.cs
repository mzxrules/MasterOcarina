using mzxrules.Helper;

namespace Spectrum
{
    class ColliderBump
    {
        public int flags;
        public byte effect;
        public byte unk_0x05;
        public int unk_0x08;

        public ColliderBump(Ptr pointer)
        {
            flags = pointer.ReadInt32(0);
            effect = pointer.ReadByte(4);
            unk_0x05 = pointer.ReadByte(5);
            unk_0x08 = pointer.ReadInt32(8);

        }

        public override string ToString()
        {
            return $"Bump:  [Flags: {flags:X8} Effect: {effect:X2} unk_0x05: {unk_0x05:X2} unk_0x08: {unk_0x08:X8}]";
        }
    }

}