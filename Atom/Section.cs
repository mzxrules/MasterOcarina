using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atom
{
    public class Section
    {
        public string Name;
        public N64Ptr VRam { get; protected set; }
        public long VRom { get; protected set; }
        public int Size { get; protected set; }
        public bool IsCode { get; protected set; }

        public Section(string name, N64Ptr vram, long vrom, int size, bool isCode = false)
        {
            Name = name;
            VRam = vram;
            VRom = vrom;
            Size = size;
            IsCode = isCode;
        }
        public bool IsWithin(N64Ptr ptr)
        {
            return (VRam < ptr && ptr < (VRam + Size));
        }
    }
}
