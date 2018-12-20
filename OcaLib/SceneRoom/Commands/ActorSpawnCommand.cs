using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using mzxrules.Helper;
using mzxrules.OcaLib.Actor;

namespace mzxrules.OcaLib.SceneRoom.Commands
{
    class ActorSpawnCommand : SceneCommand, IActorList, IDataCommand
    {
        Game Game { get; set; }
        public SegmentAddress SegmentAddress { get; set; }

        public int Actors { get; set; }
        public List<IActor> ActorList = new List<IActor>();
        private delegate ActorSpawn GetActorRecord(short[] data);
        GetActorRecord NewActor;

        public ActorSpawnCommand(Game game)
        {
            Game = game;

            if (Game == Game.OcarinaOfTime)
                NewActor = ActorFactory.OcarinaActors;
            else if (Game == Game.MajorasMask)
                NewActor = ActorFactory.MaskActors;
        }

        public override void SetCommand(SceneWord command)
        {
            base.SetCommand(command);
            Actors = command.Data1;
            SegmentAddress = command.Data2;

            if (SegmentAddress.Segment != (byte)ORom.Bank.map
                && SegmentAddress.Segment != (byte)ORom.Bank.scene)
                throw new Exception();
        }

        public override string Read()
        {
            string result;

            result = ToString();
            foreach (ActorSpawn a in ActorList)
            {
                result += Environment.NewLine + a.Print();
            }
            return result;
        }
        public override string ToString()
        {
            if (Code == (byte)HeaderCommands.PositionList)
                return $"There are {Actors} position(s). List starts at {SegmentAddress:X8}.";
            else
                return $"There are {Actors} actor(s). List starts at {SegmentAddress.Offset:X8}";
        }
        
        public void Initialize(BinaryReader br)
        {
            br.BaseStream.Position = SegmentAddress.Offset;
            for (int i = 0; i < Actors; i++)
            {
                short[] actorArray = new short[ActorSpawn.SIZE / 2];
                for (int j = 0; j < ActorSpawn.SIZE/2; j ++)
                {
                    actorArray[j] = br.ReadBigInt16();
                }
                ActorList.Add(NewActor(actorArray));
            }
        }
        public List<ActorSpawn> GetActors()
        {
            return ActorList.Cast<ActorSpawn>().ToList();
        }
    }
}
