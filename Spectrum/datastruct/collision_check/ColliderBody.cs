using mzxrules.Helper;
using System;

namespace Spectrum
{
    class ColliderBody
    {
        ColliderTouch toucher;
        ColliderBump bumper;
        byte flags;
        byte toucher_flags;
        byte bumper_flags;
        byte flags_2;
        N64Ptr unk_0x18;
        N64Ptr colliderPtr;
        N64Ptr unk_0x20;
        N64Ptr collidingPtr;

        public ColliderBody(Ptr p)
        {
            toucher = new ColliderTouch(p);
            bumper = new ColliderBump(p.RelOff(0x08));
            flags = p.ReadByte(0x14);
            toucher_flags = p.ReadByte(0x15);
            bumper_flags = p.ReadByte(0x16);
            flags_2 = p.ReadByte(0x17);
            unk_0x18 = p.ReadInt32(0x18);
            colliderPtr = p.ReadInt32(0x1C);
            unk_0x20 = p.ReadInt32(0x20);
            collidingPtr = p.ReadInt32(0x24);
        }

        public override string ToString()
        {
            return $"Body: {Environment.NewLine}" +
                $" {toucher}{Environment.NewLine}" +
                $" {bumper}{Environment.NewLine}" +
                $" {flags:X2} {toucher_flags:X2} {bumper_flags:X2} {flags_2:X2}{Environment.NewLine}" +
                $" AT? {unk_0x18:X8} AC? {colliderPtr} ATe? {unk_0x20:X8} ACe? {collidingPtr}";
        }
    }

}