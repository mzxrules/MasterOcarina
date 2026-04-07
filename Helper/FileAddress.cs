using System;

namespace mzxrules.Helper
{
    [Serializable]
    public record struct FileAddress(int Start, int End)
    {
        public readonly int Size => End - Start;

        public FileAddress(long start, long end) : this((int)start, (int)end)
        {
        }

        public override readonly string ToString() => $"{Start:X8}:{End:X8}";
    }
}
