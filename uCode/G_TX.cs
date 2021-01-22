using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uCode
{
    //Tile Descriptors
    public static class G_TX_
    {
        public const int G_TX_NOMIRROR = 0;
        public const int G_TX_MIRROR = 1;
        public const int G_TX_WRAP = 0;
        public const int G_TX_CLAMP = 2;

        public const int G_TX_RENDERTILE = 0;
        public const int G_TX_LOADTILE = 7;

        public static string Tile(int tile)
        {
            switch (tile)
            {
                case G_TX_LOADTILE: return nameof(G_TX_LOADTILE);
                case G_TX_RENDERTILE: return nameof(G_TX_RENDERTILE);
                default: return $"TILE {tile}";
            }
        }
    }
}
