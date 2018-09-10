using System.Runtime.Serialization;

namespace JOcaBase
{
    [DataContract]
    public class JDmaData
    {

        [DataMember]
        public string Filename { get; set; }

        [DataMember]
        public int VRomStart { get; set; }

        [DataMember]
        public int VRomEnd { get; set; }
    }
}
