using mzxrules.Helper;
using System;
using System.IO;

namespace uCode
{
    public struct Matrix
    {
        public short[,] intpart; //4x4
        public ushort[,] fracpart; //4x4

        private float[,] matrix;

        public Matrix(BinaryReader br)
        {
            intpart = new short[4, 4];
            fracpart = new ushort[4, 4];
            matrix = new float[4, 4];

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    intpart[i, j] = br.ReadBigInt16();

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    int fInt;
                    float fConv;
                    ushort r = br.ReadBigUInt16();
                    fracpart[i, j] = r;

                    fInt = intpart[i, j] << 16;
                    fInt += r;

                    fConv = Convert.ToSingle(fInt);

                    matrix[i, j] = fConv / 0x10000;  
                }
        }

        public override string ToString()
        {
            return string.Format("({0,12:0.00000}, {1,12:0.00000}, {2,12:0.00000}, {3,12:0.00000}),\n"
                + "({4,12:0.00000}, {5,12:0.00000}, {6,12:0.00000}, {7,12:0.00000}),\n"
                + "({8,12:0.00000}, {9,12:0.00000}, {10,12:0.00000}, {11,12:0.00000}),\n"
                + "({12,12:0.00000}, {13,12:0.00000}, {14,12:0.00000}, {15,12:0.00000})",
                matrix[0, 0], matrix[0, 1], matrix[0, 2], matrix[0, 3],
                matrix[1, 0], matrix[1, 1], matrix[1, 2], matrix[1, 3],
                matrix[2, 0], matrix[2, 1], matrix[2, 2], matrix[2, 3],
                matrix[3, 0], matrix[3, 1], matrix[3, 2], matrix[3, 3]);
        }
    }
}
