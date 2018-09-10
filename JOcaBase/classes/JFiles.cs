using System.Runtime.Serialization;

namespace JOcaBase
{
    [DataContract]
    public class JFiles
    {
        [DataMember]
        public string Filename { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public bool IsStatic { get; set; }
    }
}
