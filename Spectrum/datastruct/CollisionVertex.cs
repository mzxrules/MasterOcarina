using mzxrules.Helper;

namespace Spectrum
{
    class CollisionVertex
    {
        public short Id;
        public Vector3<short> Value;
        public CollisionVertex(short id, short x, short y, short z)
        {
            Id = id;
            Value = new Vector3<short>(x, y, z);
        }
        public static CollisionVertex GetVertex(CollisionMesh mesh, short id)
        {
            var ptr = mesh.VertexArray.RelOff(id * 6);
            return new CollisionVertex(id,
                ptr.ReadInt16(0),
                ptr.ReadInt16(2),
                ptr.ReadInt16(4));
        }
        public override string ToString()
        {
            return $"ID: {Id:X4} ({Value.x},{Value.y},{Value.z})";
        }
    }
}
