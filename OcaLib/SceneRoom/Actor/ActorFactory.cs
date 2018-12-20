namespace mzxrules.OcaLib.Actor
{
    public static class ActorFactory
    {
        #region Delegates
        public delegate ActorSpawn OcarinaActorFactoryDelegate(short[] data);
        public delegate TransitionActor OcarinaTransitionActorFactoryDelegate(byte[] data);
        public delegate ActorSpawn MaskActorFactoryDelegate(short[] data);
        public delegate TransitionActor MaskTransitionActorFactoryDelegate(byte[] data);

        private static  OcarinaActorFactoryDelegate OcarinaActorFactory_In;
        private static  OcarinaTransitionActorFactoryDelegate OcarinaTransitionActorFactory_In;
        private static  MaskActorFactoryDelegate MaskActorFactory_In;
        private static  MaskTransitionActorFactoryDelegate MaskTransitionActorFactory_In;
        #endregion

        public static void BindOcarinaActorFactories
            (OcarinaActorFactoryDelegate actor,
            OcarinaTransitionActorFactoryDelegate transitionActor)
        {
            OcarinaActorFactory_In = actor;
            OcarinaTransitionActorFactory_In = transitionActor;
        }

        public static void BindMaskActorFactories
            (MaskActorFactoryDelegate actor,
            MaskTransitionActorFactoryDelegate transitionActor)
        {
            MaskActorFactory_In = actor;
            MaskTransitionActorFactory_In = transitionActor;
        }


        internal static ActorSpawn OcarinaActors(short[] data)
        {
            if (OcarinaActorFactory_In != null)
                return OcarinaActorFactory_In(data);
            else
                return new ActorSpawn(data);
        }

        internal static TransitionActor OcarinaTransitionActors(byte[] data)
        {
            if (OcarinaTransitionActorFactory_In != null)
                return OcarinaTransitionActorFactory_In(data);
            else
                return new TransitionActor(data);
        }

        internal static ActorSpawn MaskActors(short[] data)
        {
            if (MaskActorFactory_In != null)
                return MaskActorFactory_In(data);
            else
                return new ActorSpawn(data);
        }

        internal static TransitionActor MaskTransitionActors(byte[] data)
        {
            if (MaskTransitionActorFactory_In != null)
                return MaskTransitionActorFactory_In(data);
            else
                return new TransitionActor(data);
        }
    }
}
