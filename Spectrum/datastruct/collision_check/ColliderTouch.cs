using mzxrules.Helper;

namespace Spectrum
{
    class ColliderTouch
    {
        public int flags;
        public byte effect;
        public byte damage;

        public ColliderTouch(Ptr pointer)
        {
            flags = pointer.ReadInt32(0);
            effect = pointer.ReadByte(4);
            damage = pointer.ReadByte(5);
        }

        public override string ToString()
        {
            return $"Touch: [Flags: {flags:X8} Effect: {effect:X2} Damage: {damage:X2}]";
        }
    }

}