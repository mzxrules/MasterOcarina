using mzxrules.Helper;
using mzxrules.OcaLib.Actor;

namespace mzxrules.XActor.OActors
{
    public static class TransitionActorFactory
    {
        public static TransitionActor New(byte[] record)
        {
            ushort actor;
            Endian.Convert(out actor, record, 4);
            switch (actor)
            {
                case 0x0009: return new StandardDoorActor(record);
                case 0x0023: return new TransitionPlaneActor(record);
                case 0x002E: return new LiftingDoorActor(record);
                default: return new TransitionActor(record);
            }
        }
        
    }
}
