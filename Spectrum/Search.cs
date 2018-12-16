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

    class Pattern<T>
    {
        Dictionary<T, List<int>> Lookup = new Dictionary<T, List<int>>();
        public T[] Data;
        public int Length => Data.Length;

        public Pattern(T[] pattern)
        {
            Data = pattern;
            Dictionary<T, int> nextLowest = new Dictionary<T, int>();

            for (int i = 0; i < pattern.Length; i++)
            {
                foreach (var item in nextLowest)
                {
                    Lookup[item.Key][i] = item.Value;
                }

                T key = pattern[i];
                if (!Lookup.ContainsKey(key))
                {
                    Lookup[key] = Enumerable.Repeat(-1, pattern.Length).ToList();
                    nextLowest[key] = i;
                    continue;
                }
                else
                {
                    Lookup[key][i] = nextLowest[key];
                    nextLowest[key] = i;
                }
            }
        }
        public int GetShift(T key, int index)
        {
            if (Lookup.TryGetValue(key, out List<int> list))
            {
                return list[index];
            }
            return -1;
        }
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
