using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uCode
{
    public enum GeometryMode
    {
        G_ZBUFFER =		        0x00000001,
        G_TEXTURE_ENABLE =	    0x00000002,	/* Microcode use only */
        G_SHADE =			    0x00000004,	/* enable Gouraud interp */
        /* rest of low byte reserved for setup ucode */
        G_SHADING_SMOOTH =	    0x00000200,	/* flat or smooth shaded */
        G_CULL_FRONT =		    0x00001000, 
        G_CULL_BACK =		    0x00002000,
        G_CULL_BOTH =		    0x00003000, /* To make code cleaner */
        G_FOG =			        0x00010000,
        G_LIGHTING	=           0x00020000,
        G_TEXTURE_GEN =		    0x00040000,
        G_TEXTURE_GEN_LINEAR =	0x00080000,
        G_LOD =			        0x00100000, /* NOT IMPLEMENTED */
        G_CLIPPING =		    0x00800000,
    }
}
