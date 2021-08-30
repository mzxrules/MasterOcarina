using mzxrules.Helper;

namespace Spectrum
{
    class ColliderShape
    {
        public Collider collider;

        public static ColliderShape Initialize(Ptr ptr)
        {
            var collider = new Collider(ptr);

            return collider.Shape switch
            {
                0 => new ColliderJntSph(ptr, collider),
                1 => new ColliderCylinder(ptr, collider),
                2 => new ColliderTris(ptr, collider),
                3 => new ColliderQuad(ptr, collider),
                _ => new ColliderShape()
                {
                    collider = collider
                },
            };
        }
        public override string ToString()
        {
            return collider.ToString();
        }
    }

}