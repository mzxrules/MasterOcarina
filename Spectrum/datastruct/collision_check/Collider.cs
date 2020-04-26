using mzxrules.Helper;

namespace Spectrum
{
    class Collider
    {
        public N64Ptr Address;
        public short ActorId;

        /* 0x00 */
        public N64Ptr Instance;
        /* 0x04 */
        public N64Ptr At;
        /* 0x08 */
        public N64Ptr Ac;
        /* 0x0C */
        public N64Ptr Oc;
        int flags1;
        byte unk_0x14;
        public byte Shape;
        short flags2;

        public Collider(Ptr pointer)
        {
            Address = (int)pointer;
            Instance = pointer.ReadInt32(0);
            ActorId = pointer.Deref().ReadInt16(0);
            At = pointer.ReadInt32(0x04);
            Ac = pointer.ReadInt32(0x08);
            Oc = pointer.ReadInt32(0x0C);
            flags1 = pointer.ReadInt32(0x10);
            unk_0x14 = pointer.ReadByte(0x14);
            Shape = pointer.ReadByte(0x15);
            flags2 = pointer.ReadInt16(0x16);
        }
        public override string ToString()
        {
            return $"{Address.Offset:X6}: AI {ActorId:X4} OFF:{Address - Instance & 0xFFFFFF:X4}  "
                + $" {Instance} AT:{At} AC:{Ac} OC:{Oc}  "
                + $" {flags1:X8} {unk_0x14:X2} {Shape:X2} {flags2:X4}";
        }
    }

}