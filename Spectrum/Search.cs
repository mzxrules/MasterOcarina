using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum
{
    class Search
    {
    }
    class BayerMoore
    {
        class Pattern
        {
            int[] shift = new int[256];
            public byte[] Data { get; set; }

            public int this[int key]
            {
                get { return Data[key]; }
            }

            public Pattern(byte[] pattern)
            {
                Data = pattern;
                for (int i = pattern.Length-1; i >= 0; i--)
                {
                    int j = pattern[i];
                    if (shift[j] == 0)
                    {
                        shift[j] = i + 1;
                    }
                }
            }
        }
        public BayerMoore(byte[] pattern, byte[] text)
        {
            Pattern v = new Pattern(pattern);
        }
    }

}
