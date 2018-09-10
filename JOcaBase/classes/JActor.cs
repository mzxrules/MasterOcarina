using System.Runtime.Serialization;

namespace JOcaBase
{
    [DataContract]
    public class JActor
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        public int Int_Id { get { return int.Parse(Id, System.Globalization.NumberStyles.HexNumber); } }
    }
}
