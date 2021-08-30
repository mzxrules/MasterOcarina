using System;
using System.Globalization;

namespace mzxrules.XActor
{
    internal class ConditionExpression
    {
        enum ConditionType
        {
            Invalid,
            Equality,
            Inequality,
        }

        private string condition;

        public Func<short[], CaptureExpression.GetValueDelegate, bool> Test;

        public ConditionExpression(string condition)
        {
            this.condition = condition;

            if (string.IsNullOrWhiteSpace(condition))
            {
                Test = (x,y) => true;
            }
            else
            {
                string splitStr = "";
                ConditionType ct = ConditionType.Invalid;

                if (condition.Contains("=="))
                {
                    splitStr = "==";
                    ct = ConditionType.Equality;
                }
                else if (condition.Contains("!="))
                {
                    splitStr = "!=";
                    ct = ConditionType.Inequality;
                }

                var splitIndex = condition.IndexOf(splitStr);
                var captureStr = condition.Substring(0, splitIndex).Trim();
                var valueStr = condition.Substring(splitIndex + splitStr.Length).Trim();

                CaptureExpression capture = new(captureStr);

                ushort value;
                if (valueStr.StartsWith("0x"))
                {
                    value = ushort.Parse(valueStr.Substring(2), NumberStyles.HexNumber);
                }
                else
                {
                    value = ushort.Parse(valueStr);
                }

                if (ct == ConditionType.Equality)
                {
                    Test = (x, getValueDelegate) =>
                    {
                        var GetValue = getValueDelegate(capture);
                        var v = GetValue(x);
                        return v == value;
                    };
                }
                else if (ct == ConditionType.Inequality)
                {
                    Test = (x, getValueDelegate) =>
                    {
                        var GetValue = getValueDelegate(capture);
                        var v = GetValue(x);
                        return v != value;
                    };
                }
            }
        }
    }
}