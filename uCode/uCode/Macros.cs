using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        /*
         *  Allow tmem address to be specified
         */
        //#define	_gsDPLoadTextureBlock(timg, tmem, fmt, siz, width, height,	\
        //		pal, cms, cmt, masks, maskt, shifts, shiftt)		\
        //									\
        //	gsDPSetTextureImage(fmt, siz##_LOAD_BLOCK, 1, timg),		\
        //	gsDPSetTile(fmt, siz##_LOAD_BLOCK, 0, tmem, G_TX_LOADTILE, 	\
        //		0 , cmt, maskt,	shiftt, cms, masks, shifts),		\
        //	gsDPLoadSync(),							\
        //	gsDPLoadBlock(G_TX_LOADTILE, 0, 0, 				\
        //		(((width)*(height) + siz##_INCR) >> siz##_SHIFT)-1,	\
        //		CALC_DXT(width, siz##_BYTES)), 				\
        //	gsDPPipeSync(),							\
        //	gsDPSetTile(fmt, siz,						\
        //		((((width) * siz##_LINE_BYTES)+7)>>3), tmem,		\
        //		G_TX_RENDERTILE, pal, cmt, maskt, shiftt, cms, masks,	\
        //		shifts),						\
        //	gsDPSetTileSize(G_TX_RENDERTILE, 0, 0,				\
        //		((width)-1) << G_TEXTURE_IMAGE_FRAC,			\
        //		((height)-1) << G_TEXTURE_IMAGE_FRAC)


        /*
         *  Allow tmem address and render_tile to be specified
         */
        //#define	_gsDPLoadTextureBlockTile(timg, tmem, rtile, fmt, siz, width,	\
        //		height, pal, cms, cmt, masks, maskt, shifts, shiftt)	\
        //									\
        //	gsDPSetTextureImage(fmt, siz##_LOAD_BLOCK, 1, timg),		\
        //	gsDPSetTile(fmt, siz##_LOAD_BLOCK, 0, tmem, G_TX_LOADTILE, 	\
        //		0 , cmt, maskt,	shiftt, cms, masks, shifts),		\
        //	gsDPLoadSync(),							\
        //	gsDPLoadBlock(G_TX_LOADTILE, 0, 0, 				\
        //		(((width)*(height) + siz##_INCR) >> siz##_SHIFT)-1,	\
        //		CALC_DXT(width, siz##_BYTES)), 				\
        //	gsDPPipeSync(),							\
        //	gsDPSetTile(fmt, siz,						\
        //		((((width) * siz##_LINE_BYTES)+7)>>3), tmem,		\
        //		rtile, pal, cmt, maskt, shiftt, cms, masks,		\
        //		shifts),						\
        //	gsDPSetTileSize(rtile, 0, 0,					\
        //		((width)-1) << G_TEXTURE_IMAGE_FRAC,			\
        //		((height)-1) << G_TEXTURE_IMAGE_FRAC)

    }
}