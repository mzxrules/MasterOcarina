namespace mzxrules.Helper
{
    public record SegmentAddress
    {
        private readonly int value;

        public byte Segment => (byte)(value >> 24);

        public int Offset => value & 0xFFFFFF;

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
            value = (bank << 24) | (offset & 0xFFFFFF);
        }

        public SegmentAddress(SegmentAddress seg, int offset) : this(seg.Segment, offset) { }

        public static implicit operator SegmentAddress(int ptr) => new(ptr);

        public static implicit operator SegmentAddress(uint ptr) => new(ptr);

        public static bool operator == (SegmentAddress seg, int value)
        {
            return seg.value == value;
        }

        public static bool operator != (SegmentAddress seg, int value)
        {
            return seg.value != value;
        }

        public static SegmentAddress operator +(SegmentAddress seg, int value) => new(seg.value + value);

        public static SegmentAddress operator -(SegmentAddress seg, int value) => new(seg.value - value);

        public override string ToString()
        {
            return $"{value:X8}";
        }
    }
}