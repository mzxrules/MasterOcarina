using System;

namespace mzxrules.Helper
{
    public record Vector2<T1>
    {
        public T1 x;
        public T1 y;
        public Vector2()
        {
        }
        public Vector2(T1 inV)
        {
            x = inV;
            y = inV;
        }
        public Vector2(T1 inX, T1 inY)
        {
            x = inX;
            y = inY;
        }
    }
    public class Vector2<T1, T2>
    {
        public T1 x;
        public T2 y;
    }
    public record Vector3<T1> : IFormattable, IEquatable<Vector3<T1>> where T1 : IEquatable<T1>
    {
        public T1 x { get; }
        public T1 y { get; }
        public T1 z { get; }
        public Vector3()
        {
        }
        public Vector3(T1 inV)
        {
            x = inV;
            y = inV;
            z = inV;
        }
        public Vector3(T1 inX, T1 inY, T1 inZ)
        {
            x = inX;
            y = inY;
            z = inZ;
        }
        public T1 Index(int i) => i switch
        {
            0 => x,
            1 => y,
            2 => z,
            _ => throw new ArgumentOutOfRangeException(nameof(i), "Expected range between 0 and 2"),
        };

        public override int GetHashCode()
        {
            return x.GetHashCode() + y.GetHashCode() + z.GetHashCode();
        }

        public string ToString(string format, IFormatProvider provider)
        {
            if (x is IFormattable a && y is IFormattable b && z is IFormattable c)
            {
                return $"({a.ToString(format, provider)}, {b.ToString(format, provider)}, {c.ToString(format, provider)})";
            }
            return ToString();
        }

        public override string ToString()
        {
            return $"({x}, {y}, {z})";
        }

        public T1[] ToArray()
        {
            return [x, y, z];
        }
    }
    public class Vector3<T1, T2, T3>
    {
        public T1 x;
        public T2 y;
        public T3 z;
    }
}
