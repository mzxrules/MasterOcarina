//Extension Methods for Endian class that will be integrated when Helper moves to .net Core 3.0
using System;
using System.Collections.Generic;
using System.Text;
using System.Buffers.Binary;

namespace ZCodecCore
{
    public static class EndianX
    {
        public static int ConvertInt32(ReadOnlySpan<byte> span, int offset = 0)
        {
            return BinaryPrimitives.ReadInt32BigEndian(span.Slice(offset));
        }
    }
}
