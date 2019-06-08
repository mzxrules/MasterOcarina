using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum
{
    class CollisionShape
    {
        public CollisionCollider collider;

        public static CollisionShape Initialize(Ptr ptr)
        {
            var collider = new CollisionCollider(ptr);

            switch (collider.Shape)
            {
                case 0: return new ColliderCylinderCollection(ptr, collider);
                case 2: return new ColliderTriCollection(ptr, collider);
                default:
                    return new CollisionShape()
                    {
                        collider = collider
                    };
            }
        }
        public override string ToString()
        {
            return collider.ToString();
        }
    }

    class CollisionCollider
    {
        public N64Ptr Address;
        public short ActorId;

        /* 0x00 */
        public N64Ptr Instance;
        /* 0x04 */
        public N64Ptr CollidingInstance1;
        /* 0x08 */
        public N64Ptr CollidingInstance2;
        /* 0x0C */
        public N64Ptr CollidingInstance3;
        int flags1;
        byte unk_0x14;
        public byte Shape;
        short flags2;

        public CollisionCollider(Ptr pointer)
        {
            Address = (int)pointer;
            Instance = pointer.ReadInt32(0);
            ActorId = pointer.Deref().ReadInt16(0);
            CollidingInstance1 = pointer.ReadInt32(0x04);
            CollidingInstance2 = pointer.ReadInt32(0x08);
            CollidingInstance3 = pointer.ReadInt32(0x0C);
            flags1 = pointer.ReadInt32(0x10);
            unk_0x14 = pointer.ReadByte(0x14);
            Shape = pointer.ReadByte(0x15);
            flags2 = pointer.ReadInt16(0x16);
        }
        public override string ToString()
        {
            return $"{Address.Offset:X6}: AI {ActorId:X4} OFF: {Address - Instance & 0xFFFFFF:X4}  "
                + $" {(int)Instance:X8} {(int)CollidingInstance1:X8} {(int)CollidingInstance2:X8} {(int)CollidingInstance3:X8}  "
                + $" {flags1:X8} {unk_0x14:X2} {Shape:X2} {flags2:X4}";
        }
    }

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

    class ColliderBump
    {
        public int flags;
        public byte effect;
        public byte unk_0x05;
        public int unk_0x08;

        public ColliderBump(Ptr pointer)
        {
            flags = pointer.ReadInt32(0);
            effect = pointer.ReadByte(4);
            unk_0x05 = pointer.ReadByte(5);
            unk_0x08 = pointer.ReadInt32(8);

        }

        public override string ToString()
        {
            return $"Bump:  [Flags: {flags:X8} Effect: {effect:X2} unk_0x05: {unk_0x05:X2} unk_0x08: {unk_0x08:X8}]";
        }
    }
    class ColliderTriCollection : CollisionShape
    {
        int count;
        N64Ptr listPtr;
        List<ColliderTri> items = new List<ColliderTri>();
        public ColliderTriCollection(Ptr ptr, CollisionCollider col)
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
                    items.Add(new ColliderTri(ptr.RelOff(i * 0x5C)));
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

    class ColliderBody
    {
        ColliderTouch toucher;
        ColliderBump bumper;
        byte flags;
        byte toucher_flags;
        byte bumper_flags;
        byte flags_2;
        int unk_0x18;
        N64Ptr colliderPtr;
        int unk_0x20;
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
                $" {flags:X2} {toucher_flags:X2} {bumper_flags:X2} {flags_2:X2} {unk_0x18:X8}{Environment.NewLine}" +
                $" {colliderPtr} {unk_0x20:X8} {collidingPtr}";
        }
    }

    class ColliderTri
    {
        public ColliderBody body;
        public Vector3<float> pointA;
        public Vector3<float> pointB;
        public Vector3<float> pointC;
        public Vector3<float> unit_normal;
        public float normal_dist;

        public ColliderTri(Ptr p)
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

    class ColliderCylinderCollection : CollisionShape
    {
        public int count;
        public N64Ptr listPtr;
        public List<ColliderCylinder> items = new List<ColliderCylinder>();
        public ColliderCylinderCollection(Ptr ptr, CollisionCollider col)
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
                    items.Add(new ColliderCylinder(ptr.RelOff(i * 0x5C)));
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

    class ColliderCylinder
    {
        public ColliderBody body;
        public Vector3<short> unk_0x28;
        public short unk_0x2E;
        public Vector3<short> position;
        public short unk_0x36;
        public int unk_0x38;
        public byte unk_0x3C;
        public ColliderCylinder(Ptr p)
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