using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mzxrules.OcaLib.Actor
{
    public class LinkActor : ActorSpawn
    {
        public LinkActor(short[] data) : base(data) { }
        public LinkActor() { }
        
        protected override string GetActorName()
        {
            return "Link";
        }

    }
}
