using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OcaBase
{
    partial class Item
    {
        public string GetName(bool OOT3D)
        {
            if (OOT3D)
            {
                if (ID == 44)
                    return "No Mask";
                if (ID == 49)
                    return "Odd Poultice";
            }
            return Name;
        }
    }
}
