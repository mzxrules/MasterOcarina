using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum
{
    [DataContract]
    public class Emulator
    {
        [DataMember]
        public string ProcessName { get; set; }
        [DataMember]
        public string ProcessDescription { get; set; }
        [DataMember]
        public int ProcessType { get; set; }
        [DataMember]
        public string RamStart { get; set; }
        [DataMember]
        public byte BigEndian { get; set; }

        public Emulator(string pName, string pDesc, int pType, string ramStart, byte bigEndian)
        {
            ProcessName = pName;
            ProcessDescription = pDesc;
            ProcessType = pType;
            RamStart = ramStart;
            BigEndian = bigEndian;
        }

        public override string ToString()
        {
            return $"{ProcessName}, {ProcessType}-Bit Process, {ProcessDescription}, Ram Start: {RamStart}, Big Endian: {BigEndian}";
        }
    }
}