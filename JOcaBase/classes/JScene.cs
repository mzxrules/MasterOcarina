using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JOcaBase
{
    [DataContract]
    public class JScene
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Filename { get; set; }
        [DataMember]
        public string TitleCard { get; set; }
        [DataMember]
        public List<string> Rooms { get; set; }

        public JSceneVersioned VersionSpecific { get; set; }

    }


    [DataContract]
    public class JSceneVersioned
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public List<JCutscene> Cutscenes { get; set; }

        public JSceneVersioned() { }
        public JSceneVersioned(int id)
        {
            Id = id;
            Cutscenes = new List<JCutscene>();
        }
    }

}
