using System.Runtime.Serialization;

namespace JOcaBase
{
    [DataContract]
    public class JCutscene
    {
        [DataMember]
        public int FileOffset { get; set; }

        [DataMember]
        public int Setup { get; set; }

        [DataMember]
        public string Description { get; set; }

        public JCutscene() { }
        public JCutscene(int off, int setup, string desc)
        {
            FileOffset = off;
            Setup = setup;
            Description = desc;
        }
    }
}
