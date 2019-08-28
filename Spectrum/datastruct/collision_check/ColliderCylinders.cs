using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Spectrum
{
    class ColliderCylinders : CollisionShape
    {
        public int count;
        public N64Ptr listPtr;
        public List<ColliderCylinderElement> items = new List<ColliderCylinderElement>();
        public ColliderCylinders(Ptr ptr, Collider col)
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
                    items.Add(new ColliderCylinderElement(ptr.RelOff(i * 0x40)));
                }
            }
        }
        public override string ToString()
        {
            string cylinders = string.Join(
                Environment.NewLine, items.Select(i => i.ToString()));
            return $"{collider}{Environment.NewLine}Cylinder Collection: count {count}, {listPtr}{Environment.NewLine}{cylinders}";
        }
    }


    class ColliderCylinderElement
    {
        public ColliderBody body;
        public Vector3<short> unk_0x28;
        public short unk_0x2E;
        public Vector3<short> position;
        public short unk_0x36;
        public int unk_0x38;
        public byte unk_0x3C;
        public ColliderCylinderElement(Ptr p)
        {
            body = new ColliderBody(p);
            unk_0x28 = new Vector3<short>(
                p.ReadInt16(0x28), p.ReadInt16(0x2A), p.ReadInt16(0x2C));
            unk_0x2E = p.ReadInt16(0x2E);
            position = new Vector3<short>(
                p.ReadInt16(0x30), p.ReadInt16(0x32), p.ReadInt16(0x34));
            unk_0x36 = p.ReadInt16(0x36);
            unk_0x38 = p.ReadInt32(0x38);
            unk_0x3C = p.ReadByte(0x3C);
        }

        public override string ToString()
        {
            return $"{body}{Environment.NewLine}" +
                $"unk_0x28: {unk_0x28}{Environment.NewLine}" +
                $"unk_0x2E: {unk_0x2E:X4}{Environment.NewLine}" +
                $"Position: {position}{Environment.NewLine}" +
                $"0x36: {unk_0x36:X4} 0x38: {unk_0x38:X8} 0x3C: {unk_0x3C:X2}";
        }
    }
}