namespace mzxrules.Helper
{
    public static class Align
    {
        public static int To4(int value)
        {
            return (value + 0x3) & -0x4;
        }

        public static int To8(int value)
        {
            return (value + 0x7) & -0x8;
        }

        public static long To8(long value)
        {
            return (value + 0x7) & -0x8;
        }

        public static int To16(int value)
        {
            return (value + 0xF) & -0x10;
        }

        public static long To16(long value)
        {
            return (value + 0xF) & -0x10;
        }
    }
}
