using System;

namespace mzxrules.Helper
{
    public record N64PtrRange(N64Ptr Start, N64Ptr End)
    {
        public N64Ptr Start { get; private set; } = Start;
        public N64Ptr End { get; private set; } = End;

        public N64Ptr Size => End - Start;

        public N64PtrRange() : this(0, 0)
        {
        }

        public N64PtrRange(byte[] data) : this(default, default)
        {
            int[] list = new int[2];

            Endian.ReverseBytes(ref data, sizeof(int));
            Buffer.BlockCopy(data, 0, list, 0, 8);
            Start = list[0];
            End = list[1];
        }

        public override string ToString()
        {
            return $"{Start:X8}:{End:X8}";
        }

        public bool IsInRange(N64Ptr value)
        {
            N64Ptr start;
            N64Ptr end;

            if (Start < End)
            {
                start = Start;
                end = End;
            }
            else
            {
                end = Start;
                start = End;
            }
            return start <= value && value < end;
        }
    }
}
