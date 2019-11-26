namespace mzxrules.OcaLib
{
    public enum AddressToken
    {
        invalid,
        /// <summary>
        /// Block Start
        /// </summary>
        __Start,
        ActorTable_Start,
        EntranceIndexTable_Start,
        GameContextTable_Start,
        HyruleSkyboxTable_Start,
        MapMarkDataTable_Start,
        ObjectTable_Start,
        ParticleTable_Start,
        PlayerPauseOverlayTable_Start,
        Scenes_Start,
        SceneTable_Start,
        TextbankTable,
        TransitionTable_Start,

        ACTOR_CAT_LL_Start,
        GFX_START,
        OBJ_ALLOC_TABLE,
        ROOM_ALLOC_ADDR,
        RAM_ARENA_DEBUG,
        RAM_ARENA_MAIN,
        RAM_ARENA_SCENES,
        RAM_GLOBAL_CONTEXT,
        RAM_SEGMENT_TABLE,
        SRAM_START,
        STACK_LIST,
        QUEUE_THREAD,
    }
}
