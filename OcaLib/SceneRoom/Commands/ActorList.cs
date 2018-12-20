using mzxrules.Helper;
using mzxrules.OcaLib.Actor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mzxrules.OcaLib.SceneRoom.Commands
{
    class ActorList : IActorList
    {
        /* Public members:
         * 
         * Segment Address
         * Actors
         * Actor Spawns
         * 
         * Factory elements:
         * 
         * GetActorRecord (determined by game)
        */

        public SegmentAddress Address;
        List<IActor> Actors = new List<IActor>();
        private int actors;


        private delegate ActorSpawn GetActorRecord(short[] data);
        GetActorRecord NewActor;

        public ActorList(Game game, SegmentAddress addr, int actors)
        {
            Address = addr;
            this.actors = actors;

            if (game == Game.OcarinaOfTime)
                NewActor = ActorFactory.OcarinaActors;
            else if (game == Game.MajorasMask)
                NewActor = ActorFactory.MaskActors;
        }

        public void LoadSpawns(BinaryReader br)
        {
            List<IActor> list = new List<IActor>();
            br.BaseStream.Position = Address.Offset;

            for (int i = 0; i < actors; i++)
            {
                short[] actorArray = new short[ActorSpawn.SIZE / 2];
                for (int j = 0; j < ActorSpawn.SIZE / 2; j++)
                {
                    actorArray[j] = br.ReadBigInt16();
                }
                list.Add(NewActor(actorArray));
            }
            Actors = list;
        }

        public void LoadTransitions(BinaryReader br)
        {
            List<IActor> list = new List<IActor>();
            br.BaseStream.Position = Address.Offset;

            var readRemaining = br.BaseStream.Length - br.BaseStream.Position;
            var maxLoops = readRemaining / ActorSpawn.SIZE;

            var loop = (maxLoops > actors) ? actors : maxLoops;

            for (int i = 0; i < loop; i++)
            {
                byte[] actorArray = new byte[ActorSpawn.SIZE];
                br.Read(actorArray, 0, 16);
                list.Add(ActorFactory.OcarinaTransitionActors(actorArray));
            }

            Actors = list;
        }

        internal List<IActor> GetActorById(int id)
        {
            if (id == 0xFFFF)
                return Actors;
            else
                return Actors.Where(x => x.Actor == id).ToList(); 
            
        }

        public List<ActorSpawn> GetActors()
        {
            return Actors.Cast<ActorSpawn>().ToList();
        }
    }
}
