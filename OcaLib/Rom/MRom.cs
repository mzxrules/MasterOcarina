using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mzxrules.OcaLib
{
    public partial class MRom: Rom
    {
        public new MFileTable Files { get { return (MFileTable) base.Files; } }
        public GameText Text { get; set; }

        public MRom(string fileLocation, Build version)
            : base(fileLocation, version)
        {
            
            //Scenes = 0x6E;
        }
    }
}
