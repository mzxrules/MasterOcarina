namespace Atom
{
    public static class MipsFields
    {
        public static uint RS(uint iw) { return ((iw) >> 21 & 0x1F); }
        public static uint RT(uint iw) { return ((iw) >> 16 & 0x1F); }
        public static uint RD(uint iw) { return ((iw) >> 11 & 0x1F); }

        public static uint VS(uint iw) { return ((iw) >> 11 & 0x1F); }
        public static uint VT(uint iw) { return ((iw) >> 16 & 0x1F); }
        public static uint VD(uint iw) { return ((iw) >> 6 & 0x1F); }
        public static uint DE(uint iw) { return ((iw) >> 11 & 0x1F); }
        public static uint EL(uint iw) { return ((iw) >> 7 & 0xF); }
        public static uint E(uint iw) { return ((iw) >> 21 & 0xF); }
        

        public static uint SA(uint iw) { return ((iw >> 6) & 0x1F); }
        public static uint FT(uint iw) { return iw >> 16 & 0x1F; }
        public static uint FS(uint iw) { return iw >> 11 & 0x1F; }
        public static uint FD(uint iw) { return iw >> 6 & 0x1F; }
        public static uint BASE(uint iw) { return iw >> 21 & 0x1F; }
        public static short IMM(uint iw) { return (short)(iw & 0xFFFF); }
        public static int OFFSET(uint iw) { return ((short)(iw & 0xFFFF)) * 4; }
        public static uint TARGET(uint iw) { return (iw & 0x3FFFFFF) << 2; }

        public static uint OP(uint iw) { return RT(iw); }
        public static uint CODE(uint iw) { return (iw & 0x3FFFFC0) >> 6; } //BREAK op
    }
}
