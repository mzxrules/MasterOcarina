using System.Collections.Generic;
using System.Globalization;
using mzxrules.OcaLib.Actor;
using mzxrules.OcaLib;
using mzxrules.Helper;
using System.Linq;

namespace mzxrules.XActor
{
    public class XActorFactory
    {
        static XActors OcarinaDoc;
        static Dictionary<short, XActorParser> OcarinaActorParsers;
        static XActors MaskDoc;
        static Dictionary<short, XActorParser> MaskActorParsers;
        static XActorFactory()
        {
            OcarinaDoc = XActors.LoadFromFile(XActors.OcaXmlPath);
            OcarinaActorParsers = GetXActorParsers(OcarinaDoc, Game.OcarinaOfTime);

            MaskDoc = XActors.LoadFromFile(XActors.MaskXmlPath);
            MaskActorParsers = GetXActorParsers(MaskDoc, Game.MajorasMask);
        }

        static Dictionary<short, XActorParser> GetXActorParsers(XActors root, Game game)
        {
            Dictionary<short, XActorParser> result = new Dictionary<short, XActorParser>();
            foreach (var item in root.Actor)
            {
                short id = short.Parse(item.id, NumberStyles.HexNumber);
                result.Add(id, new XActorParser(item, game));
            }
            return result;
        }

        public static List<XActor> GetXActorList(RomVersion version)
        {
            List<XActor> result = new();

            if(version.Game == Game.OcarinaOfTime)
            {
                return OcarinaDoc.Actor;
            }    
            else if (version.Game == Game.MajorasMask)
            {
                return MaskDoc.Actor;
            }    
            else
            {
                return result;
            }    
        }


        public static ActorSpawn NewOcaActor(short[] record)
        {
            var actor = record[0];
            if (!OcarinaActorParsers.TryGetValue(actor, out XActorParser xactorParser))
            {
                return new ActorSpawn(record);
            }
            return new XActorSpawn(record, xactorParser.Description, xactorParser.GetVariables(record, CaptureExpression.GetOcaActorValue));
        }

        public static ActorSpawn NewMaskActor(short[] record)
        {
            var actor = (short)(record[0] & 0xFFF);
            if (!MaskActorParsers.TryGetValue(actor, out XActorParser xActorParser))
            {
                return new MActorSpawn(record);
            }
            return new XMActorSpawn(record, xActorParser.Description, xActorParser.GetVariables(record, CaptureExpression.GetMMActorValue));
        }

        public static TransitionActorSpawn NewOcaTransitionActor(byte[] record)
        {
            short[] rec = Endian.BytesToBigShorts(record);
            var actor = rec[2];
            if (!OcarinaActorParsers.TryGetValue(actor, out XActorParser xactorParser))
            {
                return new TransitionActorSpawn(record);
            }
            return new XTransitionActorSpawn(rec, xactorParser.Description, xactorParser.GetVariables(rec, CaptureExpression.GetOcaActorValue));
        }

        public static TransitionActorSpawn NewMaskTransitionActor(byte[] record)
        {
            short[] rec = Endian.BytesToBigShorts(record);
            var actor = rec[2];
            if (!MaskActorParsers.TryGetValue(actor, out XActorParser xactorParser))
            {
                return new TransitionActorSpawn(record);
            }
            return new XTransitionActorSpawn(rec, xactorParser.Description, xactorParser.GetVariables(rec, CaptureExpression.GetOcaActorValue));
        }

        public class XActorSpawn : ActorSpawn
        {
            string name;
            string vars;
            public XActorSpawn(short[] record, string name, string variables) : base(record)
            {
                this.name = name;
                this.vars = variables;
            }
            protected override string GetActorName()
            {
                return name;
            }
            protected override string GetVariable()
            {
                return vars;
            }
        }

        public class XTransitionActorSpawn : TransitionActorSpawn
        {
            string name;
            string vars;
            public XTransitionActorSpawn(short[] record, string name, string variables) :
                base(Endian.BigShortsToBytes(record))
            {
                this.name = name;
                this.vars = variables;
            }
            protected override string GetActorName()
            {
                return name;
            }
            protected override string GetVariable()
            {
                return vars;
            }
        }

        public class XMActorSpawn : MActorSpawn
        {
            string name;
            string vars;
            public XMActorSpawn(short[] record, string name, string variables) : base(record)
            {
                this.name = name;
                this.vars = variables;
            }
            protected override string GetActorName()
            {
                return name;
            }
            protected override string GetVariable()
            {
                return vars;
            }
        }
        
    }
}
