using mzxrules.OcaLib.Actor;
namespace mzxrules.XActor.OActors
{
    /// <summary>
    /// Wrapper class to avoid refactoring the ZActor lib
    /// </summary>
    public class ActorRecord_Wrapper : ActorSpawn
    {
        public ActorRecord_Wrapper(short[] record, params int[] p) : base(record)
        {
        }
        protected ActorRecord_Wrapper()
        {
        }
    }
}
