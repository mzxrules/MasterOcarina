using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum
{
    public class SimpleRamItem : IRamItem
    {
        public N64PtrRange Ram { get; set; }

        public string Description { get; set; }

        public override string ToString()
        {
            return Description;
        }
    }

    public class SimpleFile : SimpleRamItem, IFile
    {
        public FileAddress VRom { get; set; }

        public override string ToString()
        {
            return Description;
        }
    }
}
