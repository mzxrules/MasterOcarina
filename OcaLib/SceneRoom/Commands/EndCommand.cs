namespace mzxrules.OcaLib.SceneRoom.Commands
{
    class EndCommand : SceneCommand
    {
        public override string Read()
        {
            return ToString();
        }
        public override string ToString()
        {
            return "End";
        }
    }
}