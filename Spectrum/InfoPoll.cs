using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mzxrules.Helper;

namespace Spectrum
{
    static class InfoPoll
    {

        public static List<ActorInstance> GetAllActorInstances()
        {
            return OvlActor.GetActorInstances();
            #region Comment
            //int actors ;
            //uint actorPtr;
            //List<ActorInstance> result = new List<ActorInstance>();

            //for (int i = 0; i < ACTOR_LL_LENGTH; i++)
            //{
            //    actors = Zpr.ReadRamInt32(ACTOR_LL_TABLE_ADDR + (i * 8));
            //    actorPtr = (uint)Zpr.ReadRamInt32(ACTOR_LL_TABLE_ADDR + (i * 8) + 4);

            //    //Console.WriteLine(string.Format("{0:D2}: Actors {1:D2}, {2:X8}",
            //    //    i,
            //    //    actors,
            //    //    actorPtr));

            //    if (actors > 0)
            //    {
            //        for (int j = 0; j < actors; j++)
            //        {
            //            ActorInstance ai = new ActorInstance(actorPtr, Zpr.ReadRam((int)(actorPtr & 0xFFFFFF), 0x13C));
            //            result.Add(ai);
            //            //Console.WriteLine(ai.ToString());
            //            actorPtr = ai.NextActor;
            //        }
            //    }
            //}
            //return result;
            #endregion

        }

        public static List<ActorInstance> GetActorsInCategory(int cat)
        {
            return OvlActor.GetActorInstances(cat);
        }

        //public class ActorCategoryTableRecord
        //{
        //    private int category;
        //    private int actors;
        //    private N64Ptr ptr;

        //    public ActorCategoryTableRecord(int category, int actors, N64Ptr ptr)
        //    {
        //        this.category = category;
        //        this.actors = actors;
        //        this.ptr = ptr;
        //    }
        //}
        //public static void GetActorCategoryTableInfo()
        //{
        //    int category = 0;
        //    int actors = 0;
        //    mzxrules.Helper.N64Ptr ptr = 0;

        //    var v = new ActorCategoryTableRecord(category, actors, ptr);
        //}

        public static string GetMemory(List<BlockNode> list)
        {
            uint total = 0;
            uint available = 0;
            uint largestBlock = 0;
            foreach (BlockNode item in list)
            {
                total += item.Size + (uint)BlockNode.LENGTH;
                if (item.IsFree)
                {
                    available += item.Size;
                    if (largestBlock < item.Size)
                        largestBlock = item.Size;
                }
            }

            return $"Actor Mem: {available:X6}/{total:X6} Largest Block {largestBlock:X6}";
        }
    }
}
