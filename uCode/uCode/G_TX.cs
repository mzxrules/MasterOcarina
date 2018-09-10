using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uCode
{
    //Tile Descriptors
    public enum G_TX
    {
        G_TX_NOMIRROR = 0,
        G_TX_MIRROR = 1,
        G_TX_WRAP,// = 0,
        G_TX_CLAMP,// = 2
        //G_TX_RENDERTILE = 0,
        //G_TX_LOADTILE = 7
    }
}
