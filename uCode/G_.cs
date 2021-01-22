namespace uCode
{
    public enum G_
    {
        //# ifdef   F3DEX_GBI_2
        G_NOOP =			0x00,
        G_RDPHALF_2 =		0xf1,
        G_SETOTHERMODE_H =	0xe3,
        G_SETOTHERMODE_L =	0xe2,
        G_RDPHALF_1 =		0xe1,
        G_SPNOOP =		    0xe0,
        G_ENDDL =			0xdf,
        G_DL =			    0xde,
        G_LOAD_UCODE =		0xdd,
        G_MOVEMEM =		    0xdc,
        G_MOVEWORD =		0xdb,
        G_MTX =			    0xda,
        G_GEOMETRYMODE =	0xd9,
        G_POPMTX = 		    0xd8,
        G_TEXTURE =		    0xd7,
        G_SUBMODULE =		0xd6, //G_DMA_IO

        G_VTX =			    0x01,
        G_MODIFYVTX =		0x02,
        G_CULLDL =		    0x03,
        G_BRANCH_Z =		0x04,
        G_TRI1	=		    0x05,
        G_TRI2 =			0x06,
        G_LINE3D =		    0x07,

        ///#else	/* F3DEX_GBI_2 */

            /* DMA commands: */
        //G_SPNOOP =		    0,	/* handle 0 gracefully */
        //G_MTX =			1,
        //G_RESERVED0 =		2,	/* not implemeted */
        //G_MOVEMEM =		3,	/* move a block of memory (up to 4 words) to dmem */
        //G_VTX =			4,
        //G_RESERVED1 =		5,	/* not implemeted */
        //G_DL =			    6,
        //G_RESERVED2 =		7,	/* not implemeted */
        //G_RESERVED3 =		8,	/* not implemeted */
        //G_SPRITE2D_BASE =	9,	/* sprite command */

        /* IMMEDIATE commands: */
        //G_IMMFIRST =		-65,
        //G_TRI1 =			(-65 - 0),
        //G_CULLDL=		    (-65-1),
        //G_POPMTX =		(-65-2),
        //G_MOVEWORD =		(-65-3),
        //G_TEXTURE =		(-65-4),
        //G_SETOTHERMODE_H =	(-65-5),
        //G_SETOTHERMODE_L =	(-65-6),
        //G_ENDDL =			(-65-7),
        //G_SETGEOMETRYMODE =	(-65-8),
        //G_CLEARGEOMETRYMODE =	(-65-9),
        //G_LINE3D =		(-65-10),
        //G_RDPHALF_1 =		(-65-11),
        //G_RDPHALF_2 =		(-65-12),

        //#if (defined(F3DEX_GBI)||defined(F3DLP_GBI))
        // G_MODIFYVTX		(G_IMMFIRST-13)
        // G_TRI2		(G_IMMFIRST-14)
        // G_BRANCH_Z		(G_IMMFIRST-15)
        // G_LOAD_UCODE		(G_IMMFIRST-16)
        //#else
        // G_RDPHALF_CONT	(G_IMMFIRST-13)
        //#endif

        /* We are overloading 2 of the immediate commands
           to keep the byte alignment of dmem the same */

        //G_SPRITE2D_SCALEFLIP =  (-65-1),
        //G_SPRITE2D_DRAW =       (-65-2),

        /* RDP commands: */
        //G_NOOP			0xc0	/*   0 */

            //#endif	/* F3DEX_GBI_2 */

            /* RDP commands: */
        G_SETCIMG=		    0xff,	/*  -1 */
        G_SETZIMG=		    0xfe,	/*  -2 */
        G_SETTIMG=		    0xfd,	/*  -3 */
        G_SETCOMBINE=		0xfc,	/*  -4 */
        G_SETENVCOLOR=		0xfb,	/*  -5 */
        G_SETPRIMCOLOR=		0xfa,	/*  -6 */
        G_SETBLENDCOLOR=	0xf9,	/*  -7 */
        G_SETFOGCOLOR=		0xf8,	/*  -8 */
        G_SETFILLCOLOR=		0xf7,	/*  -9 */
        G_FILLRECT=		    0xf6,	/* -10 */
        G_SETTILE=		    0xf5,	/* -11 */
        G_LOADTILE=		    0xf4,	/* -12 */
        G_LOADBLOCK=		0xf3,	/* -13 */
        G_SETTILESIZE=		0xf2,	/* -14 */
        //G_RDPHALF_2 =     0xf1,
        G_LOADTLUT=		    0xf0,	/* -16 */
        G_RDPSETOTHERMODE=	0xef,	/* -17 */
        G_SETPRIMDEPTH=		0xee,	/* -18 */
        G_SETSCISSOR=		0xed,	/* -19 */
        G_SETCONVERT=		0xec,	/* -20 */
        G_SETKEYR=		    0xeb,	/* -21 */
        G_SETKEYGB=		    0xea,	/* -22 */
        G_RDPFULLSYNC=		0xe9,	/* -23 */
        G_RDPTILESYNC=		0xe8,	/* -24 */
        G_RDPPIPESYNC=		0xe7,	/* -25 */
        G_RDPLOADSYNC=		0xe6,	/* -26 */
        G_TEXRECTFLIP=		0xe5,	/* -27 */
        G_TEXRECT =		    0xe4,	/* -28 */
    }
}
