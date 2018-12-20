using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mzxrules.Helper;

namespace mzxrules.OcaLib.Actor
{
    interface IActor
    {
        ushort Actor { get; set; }
        ushort Variable { get; set; }
        Vector3<short> Coords { get; set; }
        Vector3<bool> NoRotation { get; set; }
        Vector3<ushort> Rotation { get; set; }
    }
}
