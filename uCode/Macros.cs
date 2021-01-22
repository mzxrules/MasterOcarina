namespace uCode
{
    public static class Macros
    {
        static G_[] gsSPTextureRectangle = new G_[]
        {
            G_.G_TEXRECT,
            G_.G_RDPHALF_1,
            G_.G_RDPHALF_2
        };
        static G_[] gsSPTextureRectangleFlip = new G_[]
        {
            G_.G_TEXRECTFLIP,
            G_.G_RDPHALF_1,
            G_.G_RDPHALF_2
        };
        static G_[] gsDPWord = new G_[]
        {
            G_.G_RDPHALF_1,
            G_.G_RDPHALF_2
        };
        public static G_[] gsDPLoadTextureBlock = new G_[]
        {
            G_.G_SETTIMG,
            G_.G_SETTILE,
            G_.G_RDPLOADSYNC,
            G_.G_LOADBLOCK,
            G_.G_RDPPIPESYNC,
            G_.G_SETTILE,
            G_.G_SETTILESIZE
        };

        public static G_[] gsDPLoadTextureTile = new G_[]
        {
            G_.G_SETTIMG,
            G_.G_SETTILE,
            G_.G_RDPLOADSYNC,
            G_.G_LOADTILE,
            G_.G_RDPPIPESYNC,
            G_.G_SETTILE,
            G_.G_SETTILESIZE
        };

        public static G_[] gsDPLoadTLUT = new G_[]
        {
            G_.G_SETTIMG,
            G_.G_RDPTILESYNC,
            G_.G_SETTILE,
            G_.G_RDPLOADSYNC,
            G_.G_LOADTLUT,
            G_.G_RDPPIPESYNC
        };
    }
}