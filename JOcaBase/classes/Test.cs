using System.Runtime.Serialization;

namespace JOcaBase
{
    [DataContract]
    public class Files_Old
    {
        [DataMember]
        public int Index { get; set; }

        [DataMember(Name ="VRom Start")]
        public string VRomStart { get; set; }
        
        [DataMember(Name = "VRom End")]
        public string VRomEnd { get; set; }

        [DataMember]
        public string Filename { get; set; }

        [DataMember]
        public string Description { get; set; }
    }
    
    [DataContract]
    public class SceneData_In
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Filename { get; set; }
        [DataMember]
        public string TitleCard { get; set; }
    }


    [DataContract]
    public class DmaData_Wiki
    {
        [DataMember]
        public string Filename { get; set; }

        [DataMember]
        public string VRomStart { get; set; }

        [DataMember]
        public string VRomEnd { get; set; }
    }

    [DataContract]
    public class ActorOverlayTable
    {
        [DataMember]
        public int Index { get; set; }

        [DataMember]
        public int VRomStart { get; set; }

        [DataMember]
        public int VRomEnd { get; set; }
        
        [DataMember]
        public int VRamStart { get; set; }

        [DataMember]
        public int VRamEnd { get; set; }

        [DataMember]
        public int VRamEntry { get; set; }

        [DataMember]
        public string Filename { get; set; }
    }
}
