using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mzxrules.XActor.Gui
{
    class XVariableValueDisplay
    {
        public XVariableValue Source { get; private set; }
        public ushort Value { get { return UInt16.Parse(Source.Data, System.Globalization.NumberStyles.HexNumber); } }
        public XVariableValueDisplay(XVariableValue data)
        {
            Source = data;
        }
        public override string ToString()
        {
            return Source.Description;
            //return String.Format("{0}: {1}", Source.Data.Value, Source.Description);
        }
    }
}
