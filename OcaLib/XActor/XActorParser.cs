using System.Collections.Generic;
using mzxrules.OcaLib;

namespace mzxrules.XActor
{
    class XActorParser
    {
        private readonly Game game;
        private readonly List<XVariableParser> Variables = new();
        public string Description { get; private set; }

        public XActorParser(XActor actor, Game game)
        {
            this.game = game;
            Description = actor.Description;
            foreach(var item in actor.Variables)
            {
                Variables.Add(new XVariableParser(item, game));
            }
        }

        public string GetDescription(short[] record, CaptureExpression.GetValueDelegate GetValue)
        {
            foreach (var item in Variables)
            {
                if (item.isDescriptionOverride)
                {
                    return item.PrintVariable(record, GetValue);
                }
            }
            return Description;
        }

        public string GetVariables(short[] record, CaptureExpression.GetValueDelegate GetValue)
        {
            List<string> results = new();
            foreach (var item in Variables)
            {
                if (!item.isDescriptionOverride && !item.hidden && item.TestCondition(record, GetValue))
                {
                    results.Add(item.PrintVariable(record, GetValue));
                }
            }
            return string.Join(", ", results);
        }
    }
}
