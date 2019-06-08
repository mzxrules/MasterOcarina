using mzxrules.Helper;

namespace Spectrum
{
    class ColliderTouch
    {
        public int flags;
        public byte unk_0x04;
        public byte damage;

        public ColliderTouch(Ptr pointer)
        {
            flags = pointer.ReadInt32(0);
            unk_0x04 = pointer.ReadByte(4);
            damage = pointer.ReadByte(5);

        }

        public override string ToString()
        {
            return $"Touch: [Flags: {flags:X8} unk_0x04: {unk_0x04:X2} Damage: {damage:X2}]";
        }
    }

}