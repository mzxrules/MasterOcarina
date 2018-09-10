using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum
{
    [DataContract]
    class DisplayListRecord
    {
        [DataMember]
        int EndOffset { get; set; }

        public DisplayListRecord() { }

        public DisplayListRecord(int endOffset)
        {
            EndOffset = endOffset;
        }

        public override string ToString()
        {
            return EndOffset.ToString("X6");
        }
    }
}
