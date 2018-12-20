using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mzxrules.XActor
{
    class XActorParser
    {
        private Game game;
        List<XVariableParser> Variables = new List<XVariableParser>();
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
        public string GetVariables(short[] record, CaptureExpression.GetValueDelegate GetValue)
        {
            List<string> results = new List<string>();
            foreach (var item in Variables)
            {
                if (item.TestCondition(record, GetValue))
                {
                    results.Add(item.PrintVariable(record, GetValue));
                }
                //Console.WriteLine($"{item.item.Description} {item.item.Condition} {record[4]:X4} {record[5]:X4} {record[6]:X4} {record[7]:X4}");
            }
            return string.Join(", ", results);
        }
    }
}
