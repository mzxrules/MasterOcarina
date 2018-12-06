using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum
{
    enum SearchValues
    {
        INT,
        UINT,
        SHORT,
        USHORT,
        BYTE,
        SBYTE,

    }

    enum SearchFuncs
    {
        R,
        NEW,
        GT,
        LT,
        E,
        N,
        X,
        FORCE
    }

    public class ValueSearch
    {
        class Record
        {
            public int off, initial, prev, cur;

        }
        readonly SearchOptions Options;
        List<Record> Data = new List<Record>();

        public int FoundElements => Data.Count;

        public ValueSearch()
        {
            Options = new SearchOptions();
        }


        public ValueSearch(SearchOptions options)
        {
            Options = options;
        }

        public void Initialize()
        {
            Data = new List<Record>();
            Ptr ptr = SPtr.New(Options.StartPtr);
            for (int i = 0; i < Options.Size; i += 4)
            {
                var addr = Options.StartPtr + i;
                int v = ptr.ReadInt32(i);
                var rec = new Record()
                {
                    off = i,
                    initial = v,
                    prev = v,
                    cur = v
                };
                Data.Add(rec);
            }
        }

        public void GreaterThan()
        {
            Update();
            Data.RemoveAll(x => !(x.cur > x.prev));
        }
        public void LessThan()
        {
            Update();
            Data.RemoveAll(x => !(x.cur < x.prev));
        }
        public void Different()
        {
            Update();
            Data.RemoveAll(x => !(x.cur != x.prev));
        }
        public void Equal()
        {
            Update();
            Data.RemoveAll(x => !(x.cur == x.prev));
        }

        public void Exact(int value)
        {
            Update();
            Data.RemoveAll(x => !(x.cur == value));
        }

        private void Update()
        {
            Ptr ptr = SPtr.New(Options.StartPtr);
            foreach (var item in Data)
            {
                item.prev = item.cur;
                item.cur = ptr.ReadInt32(item.off);
            }
        }

        public List<string> GetList(int max = 40)
        {
            List<string> output = new List<string>();

            if (Data.Count > max)
                return output;

            foreach(var item in Data)
            {
                N64Ptr ptr = Options.StartPtr + item.off;
                output.Add($"{ptr}: {item.cur:X8} {item.prev:X8} {item.initial:X8}");
            }
            return output;
        }
    }

    public class SearchOptions
    {
        public N64Ptr StartPtr;
        public int Size;
        public Type ValueType;

        public SearchOptions() { }

        public SearchOptions(N64Ptr start, int size)
        {
            StartPtr = start;
            Size = size;
        }
    }
}
