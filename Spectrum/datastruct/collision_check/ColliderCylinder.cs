using mzxrules.Helper;
using System;

namespace Spectrum
{
    class ColliderCylinder : ColliderShape
    {
        ColliderBody body;
        short radius;
        short height;
        short yShift;
        Vector3<short> position;

        public ColliderCylinder(Ptr ptr, Collider c)
        {
            collider = c;
            body = new ColliderBody(ptr.RelOff(0x18));
            radius = ptr.ReadInt16(0x40);
            height = ptr.ReadInt16(0x42);
            yShift = ptr.ReadInt16(0x44);
            position = ptr.ReadVec3s(0x46);
        }
        public override string ToString()
        {
            return $"ColliderCylinder{Environment.NewLine}" +
                $"{collider}{Environment.NewLine}" +
                $"{body}{Environment.NewLine}" +
                $"Radius: {radius} Height: {height} YShift: {yShift}{Environment.NewLine}" +
                $"Position: {position}";
        }
    }

}