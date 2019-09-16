using mzxrules.Helper;
using System;
using System.IO;

namespace Z64Tables
{
    enum OutputTokensEnum
    {
        //special. Doesn't capture input, but generates output
        index,
        rel,

        //Stream control tokens. Don't capture data
        align,
        back,
        go,

        //Standard captures
        s8, u8,
        s16, u16,
        s32, u32,
        i64, u64,
        f32,

        //Custom bitwidth, does not move cursor
        c8,
        c16,
        c32,
        c64,
        sc8,
        sc16,
        sc32,
        sc64,
    }

    class OutputToken
    {
        public OutputTokensEnum Type { get; set; }
        public string Format { get; set; }
        public bool IsValueReturning
        {
            get
            {
                return Type != OutputTokensEnum.align
                    && Type != OutputTokensEnum.back
                    && Type != OutputTokensEnum.go;
            }
        }
        long Parameter;

        public OutputToken(OutputTokensEnum t, string f, long p)
        {
            Type = t;
            Format = f;
            Parameter = p;
        }

        public string Process(BinaryReader br, ParserTask v)
        {
            ulong result = 0;
            switch (Type)
            {
                case OutputTokensEnum.align:
                    if (Parameter != 4 && Parameter != 8)
                        throw new InvalidOperationException();
                    long off = ((br.BaseStream.Position + Parameter - 1) / Parameter) * Parameter;
                    br.BaseStream.Position = off;
                    break;
                case OutputTokensEnum.go:
                    br.BaseStream.Position += Parameter;
                    break;
                //Standard Captures
                case OutputTokensEnum.s8:
                    return string.Format("{0:" + Format + "}", br.ReadSByte());
                case OutputTokensEnum.u8:
                    return string.Format("{0:" + Format + "}", br.ReadByte());
                case OutputTokensEnum.s16:
                    return string.Format("{0:" + Format + "}", br.ReadBigInt16());
                case OutputTokensEnum.u16:
                    return string.Format("{0:" + Format + "}", br.ReadBigUInt16());
                case OutputTokensEnum.s32:
                    return string.Format("{0:" + Format + "}", br.ReadBigInt32());
                case OutputTokensEnum.u32:
                    return string.Format("{0:" + Format + "}", br.ReadBigUInt32());
                case OutputTokensEnum.f32:
                    return string.Format("{0:" + Format + "}", br.ReadBigFloat());
                case OutputTokensEnum.c8:
                    result = CaptureShift(br.ReadByte());
                    br.BaseStream.Position -= 1;
                    return string.Format("{0:" + Format + "}", (ushort)result);
                case OutputTokensEnum.sc8:
                    result = CaptureShift(br.ReadByte());
                    br.BaseStream.Position -= 1;
                    return string.Format("{0:" + Format + "}", (ushort)result);
                case OutputTokensEnum.c16:
                    result = CaptureShift(br.ReadBigUInt16());
                    br.BaseStream.Position -= 2;
                    return string.Format("{0:" + Format + "}", (ushort)result);
                case OutputTokensEnum.sc16:
                    result = CaptureShift(br.ReadBigUInt16());
                    br.BaseStream.Position -= 2;
                    return string.Format("{0:" + Format + "}", (ushort)result);
                case OutputTokensEnum.c32:
                    result = CaptureShift(br.ReadBigUInt32());
                    br.BaseStream.Position -= 4;
                    return string.Format("{0:" + Format + "}", (uint)result);
                case OutputTokensEnum.sc32:
                    result = CaptureShift(br.ReadBigUInt32());
                    br.BaseStream.Position -= 4;
                    return string.Format("{0:" + Format + "}", (uint)result);
                case OutputTokensEnum.index:
                    return string.Format("{0:" + Format + "}", v.Index);
            }
            return string.Empty;
        }

        private ulong CaptureShift(ulong value)
        {
            ulong mask = (ulong)Parameter;
            ulong result = value & mask;

            int right = Shift.GetRight(mask);
            //If signed, extend the sign
            if (Type == OutputTokensEnum.sc8
                || Type == OutputTokensEnum.sc16
                || Type == OutputTokensEnum.sc32
                || Type == OutputTokensEnum.sc64)
            {
                int left = Shift.GetLeft(mask);
                result <<= left;
                result = (ulong)(((long)result) >> (left + right));
                return result;
            }
            else
            {
                return result >> right;
            }
        }
    }
}