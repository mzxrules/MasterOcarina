using System;
namespace mzxrules.Helper
{
    public readonly record struct N64Ptr(long Value) : IEquatable<N64Ptr>, IComparable<N64Ptr>
    {
        private readonly long Value = (Value << 32) >> 32;

        public readonly byte Segment => (byte)(Value >> 24);

        public readonly int Offset => (int)(Value & 0xFFFFFF);

        public static implicit operator N64Ptr(int ptr) => new(ptr);

        public static implicit operator N64Ptr(uint ptr) => new(ptr);

        public static implicit operator N64Ptr(long ptr) => new(ptr);

        public static explicit operator uint(N64Ptr ptr) => (uint)ptr.Value;

        public static implicit operator int(N64Ptr ptr) => (int)ptr.Value;

        public override readonly string ToString() => $"{(int)Value:X8}";

        public static bool operator <(N64Ptr left, N64Ptr right) => left.CompareTo(right) < 0;

        public static bool operator <=(N64Ptr left, N64Ptr right) => left.CompareTo(right) <= 0;

        public static bool operator >(N64Ptr left, N64Ptr right) => left.CompareTo(right) > 0;

        public static bool operator >=(N64Ptr left, N64Ptr right) => left.CompareTo(right) >= 0;

        public readonly int CompareTo(N64Ptr other)
        {
            return ((uint)Value).CompareTo((uint)other.Value);
        }

        public readonly bool IsNull() => Value == 0;

        public readonly bool IsInRDRAM()
        {
            return !IsNull() && (uint)(Value & 0x7FFF_FFFF) < 0x80_0000;
        }

        public readonly bool IsEqualRam(N64Ptr right)
        {
            return (uint)(Value & 0x7FFF_FFFF) == (uint)(right & 0x7FFF_FFFF);
        }
    }
}
