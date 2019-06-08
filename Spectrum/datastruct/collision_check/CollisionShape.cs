using mzxrules.Helper;

namespace Spectrum
{
    class CollisionShape
    {
        public Collider collider;

        public static CollisionShape Initialize(Ptr ptr)
        {
            var collider = new Collider(ptr);

            switch (collider.Shape)
            {
                case 0: return new ColliderCylinders(ptr, collider);
                case 2: return new ColliderTris(ptr, collider);
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

}