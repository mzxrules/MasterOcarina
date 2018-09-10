using mzxrules.Helper;
using mzxrules.OcaLib.SceneRoom;
using System;

using PolyType = mzxrules.OcaLib.SceneRoom.CollisionPolyType;

namespace Spectrum
{

    class CollisionPolyType
    {
        public short Id;
        public PolyType Type;
        public N64Ptr Ptr;
        public CollisionPolyType(short id, N64Ptr ptr, int high, int low)
        {
            Id = id;
            Ptr = ptr;

            Type = new PolyType(high, low);
        }
        public static CollisionPolyType GetPolyType(CollisionMesh mesh, short id)
        {
            var ptr = mesh.PolyTypeArray.RelOff(id * 8);
            return new CollisionPolyType(id, ptr, ptr.ReadInt32(0), ptr.ReadInt32(4));
        }
        public override string ToString()
        {
            return $"ID: {Id:X4} {Ptr} -> {Type.HighWord:X8}:{Type.LowWord:X8}{Environment.NewLine}" +
                Type.PrintPackedVars();
        }
    }
}
