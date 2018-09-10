using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum3D.memory
{
    [DataContract]
    public class Emulator
    {
        [DataMember]
        public string ProcessName { get; set; }
        [DataMember]
        public string ProcessDescription { get; set; }
        [DataMember]
        public string RamStart { get; set; }

        public Emulator(string processName, string processDescription, string ramStart)
        {
            //Key = key;
            ProcessName = processName;
            ProcessDescription = processDescription;
            RamStart = ramStart;
        }

        public override string ToString()
        {
            return string.Format("ProcessName: {0}, {1}, Ram Start: {2}", ProcessName, ProcessDescription, RamStart);
        }
    }
}