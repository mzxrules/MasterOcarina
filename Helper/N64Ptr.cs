﻿using System;
namespace mzxrules.Helper
{
    public struct N64Ptr : IEquatable<N64Ptr>, IComparable<N64Ptr>
    {
        private readonly long value;

        public byte Segment => (byte)(value >> 24);

        public int Offset => (int)(value & 0xFFFFFF);

        public N64Ptr(long ptr)
        {
            value = (ptr << 32) >> 32;
        }
        
        public static implicit operator N64Ptr(int ptr)
        {
            return new N64Ptr(ptr);
        }

        public static implicit operator N64Ptr(uint ptr)
        {
            return new N64Ptr(ptr);
        }

        public static implicit operator N64Ptr(long ptr)
        {
            return new N64Ptr(ptr);
        }

        public static explicit operator uint(N64Ptr ptr)
        {
            return (uint)ptr.value;
        }

        public static implicit operator int(N64Ptr ptr)
        {
            return (int)ptr.value;
        }

        public static bool operator ==(N64Ptr a, N64Ptr b)
        {
            return a.value == b.value;
        }

        public static bool operator != (N64Ptr a, N64Ptr b)
        {
            return !(a == b);
        }

        public static bool operator > (N64Ptr a, N64Ptr b)
        {
            return a.CompareTo(b) == 1;
        }

        public static bool operator < (N64Ptr a, N64Ptr b)
        {
            return a.CompareTo(b) == -1;
        }

        public static bool operator >= (N64Ptr a, N64Ptr b)
        {
            return a.CompareTo(b) >= 0;
        }

        public static bool operator <= (N64Ptr a, N64Ptr b)
        {
            return a.CompareTo(b) <= 0;
        }

        public override bool Equals(object obj)
        {
            return obj is N64Ptr && this == (N64Ptr)obj;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return $"{(int)value:X8}";
        }

        public bool Equals(N64Ptr other)
        {
            return value == other.value;
        }

        public int CompareTo(N64Ptr other)
        {
            return ((uint)value).CompareTo((uint)other.value);
        }

        public bool IsNull()
        {
            return value == 0;
        }

        public bool IsInRDRAM()
        {
            uint value = (uint)(this.value & 0x7FFF_FFFF);
            return !IsNull() && value < 0x80_0000;
        }
    }
}
