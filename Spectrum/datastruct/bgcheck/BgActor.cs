using System;
using mzxrules.Helper;

namespace Spectrum
{
    class BgActor
    {
        public class DynaLookup
        {
            public ushort polyStartIndex;
            public ushort floor;
            public ushort wall;
            public ushort ceiling;

            public DynaLookup(Ptr ptr)
            {
                polyStartIndex = ptr.ReadUInt16(0);
                floor = ptr.ReadUInt16(2);
                wall = ptr.ReadUInt16(4);
                ceiling = ptr.ReadUInt16(6);
            }
        }
        public class Moment
        {
            Vector3<float> Scale;
            Vector3<short> Rotation;
            Vector3<float> Coordinates;

            public Moment(Ptr start)
            {
                Scale = new Vector3<float>(start.ReadFloat(0x00), start.ReadFloat(0x04), start.ReadFloat(0x08));
                Rotation = new Vector3<short>(start.ReadInt16(0x0C), start.ReadInt16(0x0A), start.ReadInt16(0x10));
                Coordinates = new Vector3<float>(start.ReadFloat(0x14), start.ReadFloat(0x18), start.ReadFloat(0x1C));
            }

            public override string ToString()
            {
                return string.Format("({0:F2}, {1:F2}, {2:F2}) {3:X4} {4:X4} {5:X4} ({6:F1}, {7:F1}, {8:F1})",
                    Scale.x, Scale.y, Scale.z,
                    Rotation.x, Rotation.y, Rotation.z,
                    Coordinates.x, Coordinates.y, Coordinates.z);

            }
        }

        public N64Ptr Address;
        /* 0x00 */ public Ptr ActorInstance; 
        public short ActorId;
        /* 0x04 */ public Ptr MeshPtr;
        /* 0x08 */ public DynaLookup dynaLookup;
        /* 0x10 */ public ushort vtxStartIndex;
        /* 0x12 */ public ushort waterBoxStartIndex; //MM only
        /* 0x14 */ public Moment Prev;
        /* 0x34 */ public Moment Current;
        /* 0x54 */ public Sphere16 boundingSphere;
        /* 0x5C */ public float minY;
        /* 0x60 */ public float maxY;

        public BgActor(Ptr pointer)
        {
            Address = (int)pointer;
            ActorInstance = pointer.Deref(0);
            ActorId = pointer.Deref().ReadInt16(0);
            MeshPtr = pointer.Deref(4);
            dynaLookup = new DynaLookup(pointer.RelOff(0x08));
            vtxStartIndex = pointer.ReadUInt16(0x10);
            waterBoxStartIndex = pointer.ReadUInt16(0x12);
            Prev = new Moment(pointer.RelOff(0x14));
            Current = new Moment(pointer.RelOff(0x34));
            boundingSphere = new Sphere16(pointer.RelOff(0x54));
            minY = pointer.ReadFloat(0x5C);
            maxY = pointer.ReadFloat(0x60);
        }

        public override string ToString()
        {
            return $"{Address.Offset:X6}: AI {ActorId:X4} - {ActorInstance}   MESH {MeshPtr} {Environment.NewLine}"
                + $"         Dyna:      Poly: {dynaLookup.polyStartIndex:X4}  Vert: {vtxStartIndex:X4}  Floor: {dynaLookup.floor:X4}  Wall: {dynaLookup.wall:X4}  Ceil: {dynaLookup.ceiling:X4}{Environment.NewLine}"
                + $"         Prev:      {Prev}{Environment.NewLine}"
                + $"         Cur:       {Current}{Environment.NewLine}" 
                + $"         Sphere:    {boundingSphere}{Environment.NewLine}"
                + $"         Min/Max Y: {minY} {maxY}";
        }
    }
}
