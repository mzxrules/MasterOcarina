namespace mzxrules.Helper
{
    public struct SegmentAddress
    {
        private int value;

        public byte Segment
        {
            get { return (byte)(value >> 24); }
        }

        public int Offset
        {
            get { return value & 0xFFFFFF; }
        }

        public SegmentAddress(int addr)
        {
            value = addr;
        }
        public SegmentAddress(uint addr)
        {
            value = (int)addr;
        }
        public SegmentAddress(byte bank, int offset)
        {
            value = value = (bank << 24) | (offset & 0xFFFFFF);
        }

        public SegmentAddress(SegmentAddress seg, int offset) : this(seg.Segment, offset) { }

        public static implicit operator SegmentAddress(int ptr)
        {
            return new SegmentAddress(ptr);
        }

        public static implicit operator SegmentAddress(uint ptr)
        {
            return new SegmentAddress(ptr);
        }

        public static bool operator == (SegmentAddress seg, int value)
        {
            return seg.value == value;
        }

        public static bool operator != (SegmentAddress seg, int value)
        {
            return seg.value != value;
        }

        public static SegmentAddress operator +(SegmentAddress seg, int value)
        {
            return new SegmentAddress(seg.value + value);
        }

        public static SegmentAddress operator -(SegmentAddress seg, int value)
        {
            return new SegmentAddress(seg.value - value);
        }

        public override string ToString()
        {
            return $"{value:X8}";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SegmentAddress))
            {
                return false;
            }

            var address = (SegmentAddress)obj;
            return value == address.value;
        }

        public override int GetHashCode()
        {
            var hashCode = 1113510858;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + value.GetHashCode();
            return hashCode;
        }
    }
}