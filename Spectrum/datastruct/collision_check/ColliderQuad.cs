using mzxrules.Helper;
using System;
using System.Collections.Generic;

namespace Spectrum
{
    class ColliderQuad : ColliderShape
    {
        ColliderBody body;
        List<Vector3<float>> quad;
        Vector3<short> dcMidpoint;
        Vector3<short> baMidpoint;
        float unk7C;
        public ColliderQuad(Ptr ptr, Collider c)
        {
            collider = c;
            body = new ColliderBody(ptr.RelOff(0x18));
            quad = new List<Vector3<float>>()
            {
                ptr.ReadVec3f(0x40),
                ptr.ReadVec3f(0x4C),
                ptr.ReadVec3f(0x58),
                ptr.ReadVec3f(0x64)
            };
            dcMidpoint = ptr.ReadVec3s(0x70);
            baMidpoint = ptr.ReadVec3s(0x76);
            unk7C = ptr.ReadFloat(0x7C);
        }
        public override string ToString()
        {
            return $"ColliderQuad{Environment.NewLine}" +
                $"{collider}{Environment.NewLine}" +
                $"{body}{Environment.NewLine}" +
                $" A: {quad[0]}{Environment.NewLine}" +
                $" B: {quad[1]}{Environment.NewLine}" +
                $" C: {quad[2]}{Environment.NewLine}" +
                $" D: {quad[3]}{Environment.NewLine}" +
                $" DC midpoint: {dcMidpoint}{Environment.NewLine}" +
                $" BA midpoint: {baMidpoint}{Environment.NewLine}" +
                $" unk7C: {unk7C}";
        }
    }
}
