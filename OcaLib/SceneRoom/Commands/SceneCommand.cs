namespace mzxrules.OcaLib.SceneRoom
{
    public class SceneCommand
    {
        public SceneWord Command { get; protected set; }
        public long OffsetFromFile { get; set; }
        public int Code { get { return Command.Code; } }
        
        public virtual string Read()
        {
            return ToString();
        }

        public override string ToString()
        {
            return Command.ToString();
        }

        public virtual void SetCommand(SceneWord command)
        {
            Command = command;
        }
    }
}
