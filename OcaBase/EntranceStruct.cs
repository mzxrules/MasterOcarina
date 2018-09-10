using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using OcaBase;

namespace OcaBase
{
    public class EntranceStruct
    {
        public EntranceIndex Index;
        public EntranceDef Definition;

        public string GetDescription()
        {
            return string.Format("x{0:X4}{1}: {2}, var x{3:X2}",
                Index.ID,
                (Index.BaseIndex == Index.ID) ? "" : ", Base x" + Index.BaseIndex.ToString("X4"),
                Definition.GetDescription(),
                Index.Variable);
        }
        public string GetShortDescription(bool hideDestination, bool useBaseIndex)
        {
            return string.Format("x{0:X4}: {1}",
                (useBaseIndex) ? Index.BaseIndex : Index.ID,
                Definition.GetShortDescription(hideDestination));
        }
    }
}
