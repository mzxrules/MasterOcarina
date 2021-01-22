using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uCode
{
    public struct Vertex_Color
    {
        public short[] position; // (X, Y, and Z) coordinate
        public ushort reserved;   // unused
        public short[] texcoord; // S and T texture coordinate
        public byte[] color;    // R, G, B, A color components

        public Vertex_Color(short x, short y, short z,
            short S, short T,
            byte r, byte g, byte b, byte a)
        {
            position = new short[] { x, y, z };
            reserved = 0;
            texcoord = new short[] { S, T };
            color = new byte[] { r, g, b, a };
        }
    }

    public struct Vertex_Normal
    {
        public short[] position;   // (X, Y, and Z) coordinate
        public ushort reserved;    // unused
        public short[] texcoord;   // S and T texture coordinate
        public sbyte[] normal;     // X, Y, and Z of vertex normal
        public byte Alpha;         // Alpha component of vertex

        public Vertex_Normal(short x, short y, short z,
            short S, short T,
            sbyte nx, sbyte ny, sbyte nz,
            byte alpha)
        {
            position = new short[] { x, y, z };
            reserved = 0;
            texcoord = new short[] { S, T };
            normal = new sbyte[] { nx, ny, nz };
            Alpha = alpha;
        }
    }
}
