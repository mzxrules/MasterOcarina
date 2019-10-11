namespace Atom
{
    public static class MipsFields
    {
        public static uint RS(uint iw) => (iw >> 21) & 0x1F;
        public static uint RT(uint iw) => (iw >> 16) & 0x1F;
        public static uint RD(uint iw) => (iw >> 11) & 0x1F;

        public static uint VS(uint iw) => (iw >> 11) & 0x1F;
        public static uint VT(uint iw) => (iw >> 16) & 0x1F;
        public static uint VD(uint iw) => (iw >> 6) & 0x1F;
        public static uint DE(uint iw) => (iw >> 11) & 0x1F;
        public static uint EL(uint iw) => (iw >> 7) & 0xF;
        public static uint E(uint iw) => (iw >> 21) & 0xF;


        public static uint SA(uint iw) => (iw >> 6) & 0x1F;
        public static uint FT(uint iw) => (iw >> 16) & 0x1F;
        public static uint FS(uint iw) => (iw >> 11) & 0x1F;
        public static uint FD(uint iw) => (iw >> 6) & 0x1F;
        public static uint BASE(uint iw) => (iw >> 21) & 0x1F;
        public static short IMM(uint iw) => (short)(iw & 0xFFFF);
        public static int OFFSET(uint iw) => ((short)(iw & 0xFFFF)) * 4;
        public static uint TARGET(uint iw) => (iw & 0x3FFFFFF) << 2;

        public static uint OP(uint iw) => RT(iw);
        public static uint CODE(uint iw) => (iw & 0x3FFFFC0) >> 6;  //BREAK op
    }
}
