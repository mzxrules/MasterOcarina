using mzxrules.Helper;

namespace Spectrum
{
    public class Sphere16
    {
        public Vector3<short> position;
        public short radius;

        public Sphere16(Ptr p)
        {
            position = new Vector3<short>(
                p.ReadInt16(0), p.ReadInt16(2), p.ReadInt16(4));
            radius = p.ReadInt16(6);
        }
        public override string ToString()
        {
            return $"{position} Radius: {radius}";
        }
    }
}
