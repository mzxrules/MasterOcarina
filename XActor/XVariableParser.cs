using System;
using System.Collections.Generic;
using System.Globalization;

namespace mzxrules.XActor
{
    internal class XVariableParser
    {
        private XVariable item;
        private Game game;

        public CaptureExpression capture;
        public ConditionExpression condition;
        
        internal delegate string PrintVariableDelegate(short[] actorRecord, CaptureExpression.GetValueDelegate getValDelegate);
        internal PrintVariableDelegate PrintVariable;

        Dictionary<short, XVariableValue> valueDefinitions = new Dictionary<short, XVariableValue>();


        public XVariableParser(XVariable var, Game game)
        {
            this.item = var;
            this.game = game;

            foreach (var v in var.Value)
            {
                short key = short.Parse(v.Data, NumberStyles.HexNumber);
                if (valueDefinitions.ContainsKey(key))
                {
                    Console.WriteLine($"Duplicate key: {key}, {v.Description} {v.Comment}");
                }
                else
                {
                    valueDefinitions.Add(key, v);
                }
            }

            capture = new CaptureExpression(var.Capture);
            condition = new ConditionExpression(var.Condition);
            switch(var.UI.Item)
            {
                case UITextId textId:
                    PrintVariable = (x,get) => 
                    {
                        ushort baseId = ushort.Parse(textId.Base, NumberStyles.HexNumber);
                        return $"{item.Description}: { get(capture)(x) + baseId:X4}";
                    }; break;
                case UISwitchFlag sf:
                    PrintVariable = (x,get) => { return $"{item.Description}: {get(capture)(x):X2}"; }; break;
                case UIBitFlag bf:
                    PrintVariable = (x,get) => { return $"{item.Description}: {get(capture)(x) > 0}"; }; break;
                default:
                    PrintVariable = (x,get) => 
                    {
                        short value = (short)get(capture)(x);
                        if (valueDefinitions.TryGetValue(value, out XVariableValue valueDef))
                        {
                            if (item.Description == "Type")
                            {
                                return valueDef.Description;
                            }
                            else
                                return $"{item.Description}: {valueDef.Description}";
                        }
                        else
                            return $"{item.Description}: {value:X4}";
                    }; break;
            }
        }

        internal bool TestCondition(short[] record,CaptureExpression.GetValueDelegate GetValue)
        {
            return condition.Test(record, GetValue);
        }
    }
}
