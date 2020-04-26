using mzxrules.Helper;

namespace Spectrum
{
    class ColliderShape
    {
        public Collider collider;

        public static ColliderShape Initialize(Ptr ptr)
        {
            var collider = new Collider(ptr);

            switch (collider.Shape)
            {
                case 0: return new ColliderJntSph(ptr, collider);
                case 1: return new ColliderCylinder(ptr, collider); 
                case 2: return new ColliderTris(ptr, collider);
                case 3: return new ColliderQuad(ptr, collider);
                default:
                    return new ColliderShape()
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

}