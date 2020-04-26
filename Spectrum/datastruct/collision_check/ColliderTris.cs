using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Spectrum
{
    class ColliderTris : ColliderShape
    {
        int count;
        N64Ptr listPtr;
        List<ColliderTriElement> items = new List<ColliderTriElement>();
        public ColliderTris(Ptr ptr, Collider col)
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
                    items.Add(new ColliderTriElement(ptr.RelOff(i * 0x5C)));
                }
            }
        }

        public override string ToString()
        {
            string tris = string.Join(
                Environment.NewLine, items.Select(i => i.ToString()));
            return $"ColliderTris{Environment.NewLine}" +
                $"{collider}{Environment.NewLine}" +
                $"Count: {count} List: {listPtr}{Environment.NewLine}{tris}";
        }
    }

    class ColliderTriElement
    {
        N64Ptr address;
        public ColliderBody body;
        public Vector3<float> pointA;
        public Vector3<float> pointB;
        public Vector3<float> pointC;
        public Vector3<float> unit_normal;
        public float normal_dist;

        public ColliderTriElement(Ptr p)
        {
            address = p;
            body = new ColliderBody(p);
            pointA = p.ReadVec3f(0x28);
            pointB = p.ReadVec3f(0x34);
            pointC = p.ReadVec3f(0x40);
            unit_normal = p.ReadVec3f(0x4C);
            normal_dist = p.ReadFloat(0x58);
        }

        public override string ToString()
        {
            return $"=== {address}" +
                $"{body}{Environment.NewLine}" +
                $" A: {pointA}{Environment.NewLine}" +
                $" B: {pointB}{Environment.NewLine}" +
                $" C: {pointC}{Environment.NewLine}" +
                $" Normal: {unit_normal} {normal_dist}";
        }
    }

}