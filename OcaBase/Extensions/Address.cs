using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OcaBase
{
    partial class Address
    {
        public string GetDescription()
        {
            return string.Format("{0}: {1} index {2}, {3:X8}:{4:X8}",
                    ID,
                    VersionCode,
                    Index,
                    StartAddr, EndAddr);
        }
    }
}
