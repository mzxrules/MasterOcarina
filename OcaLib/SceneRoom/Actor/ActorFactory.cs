namespace mzxrules.OcaLib.Actor
{
    public static class ActorSpawnFactory
    {   
        internal static ActorSpawn OcarinaActors(short[] data)
        {
            return XActor.XActorFactory.NewOcaActor(data);
        }

        internal static TransitionActorSpawn OcarinaTransitionActors(byte[] data)
        {
            return XActor.XActorFactory.NewOcaTransitionActor(data);
        }

        internal static ActorSpawn MaskActors(short[] data)
        {
            return XActor.XActorFactory.NewMaskActor(data);
        }

        internal static TransitionActorSpawn MaskTransitionActors(byte[] data)
        {
            return XActor.XActorFactory.NewMaskTransitionActor(data);
        }
    }
}
