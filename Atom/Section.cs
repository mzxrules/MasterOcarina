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
        public int Subsection;
        public N64Ptr FileRam { get; protected set; }
        public N64Ptr VRam { get; protected set; }
        public int Offset => VRam - FileRam;
        public int Size { get; protected set; }
        public bool IsCode { get; protected set; }

        public Section(string name, N64Ptr fileRam, N64Ptr vram, int size, int subsection, bool isCode = false)
        {
            Name = name;
            Subsection = subsection;
            FileRam = fileRam;
            VRam = vram;
            Size = size;
            IsCode = isCode;
        }
        public bool IsWithin(N64Ptr ptr)
        {
            return (VRam < ptr && ptr < (VRam + Size));
        }
    }
}
