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
        }

        public static List<ActorInstance> GetActorsInCategory(int cat)
        {
            return OvlActor.GetActorInstances(cat);
        }
        
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
