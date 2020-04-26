using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Spectrum
{
    class ColliderJntSph : ColliderShape
    {
        public int count;
        public N64Ptr listPtr;
        public List<ColliderJntSphElement> items = new List<ColliderJntSphElement>();
        public ColliderJntSph(Ptr ptr, Collider col)
        {
            collider = col;
            count = ptr.ReadInt32(0x18);
            listPtr = ptr.ReadInt32(0x1C);

            if (listPtr.IsInRDRAM())
            {
                ptr = ptr.Deref(0x1C);
                int loop = Math.Min(count, 0x40); //prevent malformed data hanging program
                for (int i = 0; i < loop; i++)
                {
                    items.Add(new ColliderJntSphElement(ptr.RelOff(i * 0x40)));
                }
            }
        }
        public override string ToString()
        {
            string spheres = string.Join(
                Environment.NewLine + Environment.NewLine, items.Select(i => i.ToString()));
            return $"ColliderJntSph{Environment.NewLine}" +
                $"{collider}{Environment.NewLine}" +
                $"Count: {count} List: {listPtr}{Environment.NewLine}{spheres}";
        }
    }


    class ColliderJntSphElement
    {
        public class Sphere
        {
            public Vector3<short> position;
            public short radius;

            public Sphere(Ptr p)
            {
                position = new Vector3<short>(
                    p.ReadInt16(0), p.ReadInt16(2), p.ReadInt16(4));
                radius = p.ReadInt16(6);
            }
            public override string ToString()
            {
                return $"{position} Radius: {radius}";
            }
        }

        N64Ptr address;
        public ColliderBody body;
        public Sphere modelSphere;
        public Sphere worldSphere;
        public float scale;
        public byte unk_0x3C;
        public ColliderJntSphElement(Ptr p)
        {
            address = p;
            body = new ColliderBody(p);
            modelSphere = new Sphere(p.RelOff(0x28));
            worldSphere = new Sphere(p.RelOff(0x30));
            scale = p.ReadFloat(0x38);
            unk_0x3C = p.ReadByte(0x3C);
        }

        public override string ToString()
        {
            return $"{address} " +
                $"{body}{Environment.NewLine}" +
                $"Params:{Environment.NewLine}" +
                $" Model Space: {modelSphere}{Environment.NewLine}" +
                $" World Space: {worldSphere}{Environment.NewLine}" +
                $" Scale: {scale}{Environment.NewLine}" +
                $" Joint?: {unk_0x3C:X2}";
        }
    }
}