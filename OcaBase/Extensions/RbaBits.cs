using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OcaBase
{
    partial class RbaBit
    {
        public string GetValue(int v)
        {
            switch (v)
            {
                case 0: return _1;
                case 1: return _2;
                case 2: return _4;
                case 3: return _8;
                case 4: return _16;
                case 5: return _32;
                case 6: return _64;
                case 7: return _128;
                default: throw new ArgumentOutOfRangeException("int v", "Value must be within 0 and 7 inclusive");
            }
        }
    }
    partial class RbaBits2
    {
        public string GetValue(int v)
        {
            switch (v)
            {
                case 0: return _0;
                case 1: return _1;
                case 2: return _2;
                case 3: return _3;
                case 4: return _4;
                case 5: return _5;
                case 6: return _6;
                case 7: return _7;
                default: throw new ArgumentOutOfRangeException("int v", "Value must be within 0 and 7 inclusive");
            }
        }
    }
}
