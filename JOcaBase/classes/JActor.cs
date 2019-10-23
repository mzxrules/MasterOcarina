using System.Runtime.Serialization;
using System.Globalization;

namespace JOcaBase
{
    [DataContract]
    public class JActor
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        public int Int_Id => int.Parse(Id, NumberStyles.HexNumber);
    }
}
