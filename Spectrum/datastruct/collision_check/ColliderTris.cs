using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Spectrum
{
    class ColliderTris : CollisionShape
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
            return $"{collider}{Environment.NewLine}Tri Collection: count {count}, {listPtr}{Environment.NewLine}{tris}";
        }
    }

    class ColliderTriElement
    {
        public ColliderBody body;
        public Vector3<float> pointA;
        public Vector3<float> pointB;
        public Vector3<float> pointC;
        public Vector3<float> unit_normal;
        public float normal_dist;

        public ColliderTriElement(Ptr p)
        {
            body = new ColliderBody(p);
            pointA = new Vector3<float>
                (p.ReadFloat(0x28), p.ReadFloat(0x2C), p.ReadFloat(0x30));
            pointB = new Vector3<float>
                (p.ReadFloat(0x34), p.ReadFloat(0x38), p.ReadFloat(0x3C));
            pointC = new Vector3<float>
                (p.ReadFloat(0x40), p.ReadFloat(0x44), p.ReadFloat(0x48));
            unit_normal = new Vector3<float>
                (p.ReadFloat(0x4C), p.ReadFloat(0x50), p.ReadFloat(0x54));
            normal_dist = p.ReadFloat(0x58);
        }

        public override string ToString()
        {
            return $"{body}{Environment.NewLine}" +
                $"A: {pointA}{Environment.NewLine}" +
                $"B: {pointB}{Environment.NewLine}" +
                $"C: {pointC}{Environment.NewLine}" +
                $"Normal: {unit_normal} {normal_dist}";
        }
    }

}