using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mzxrules.XActor
{
    internal class ParseGBFormat
    {
        static Regex MaskLine = new Regex(@"^\s*(&|\|)\s*([0-9a-fA-F]{4})(?:\:)(.*?)$");
        static Regex ValueLine = new Regex(@"^([0-9a-fA-F]{1,4})(\+)?\s*(?:\[([0-9a-fA-F?]{4})\])?\s*(.*)$");

        public static XActors ParseLines(string[] lines)
        {
            XActors root = new XActors();
            XActor curActor = null;
            XVariable curVariable = null;

            //assume lines are trimmed
            foreach (var line_untrimmed in lines)
            {
                string line = line_untrimmed.Trim();
                if (line.StartsWith("==A") ||
                    line.StartsWith("Variables:"))
                //do nothing
                {
                    continue;
                }
                if (line.StartsWith("===0"))
                //new actor
                {
                    curActor = new XActor()
                    {
                        id = line.Substring(3, 4)
                    };
                    root.Actor.Add(curActor);
                }
                else if (line.StartsWith("Identity:"))
                //name
                {
                    curActor.Description = line.Substring(9).Trim();
                }
                else if (line.StartsWith("Objects:"))
                {
                    curActor.Objects = new XObjects();
                    curActor.Objects.Object.Add(line.Substring(8).Trim());
                }
                else if (line.StartsWith("Notes:"))
                {
                    var comment = line.Substring(6).Trim();
                    if (comment.Length > 0)
                        curActor.Comment = comment;
                }
                else if (line.ToLower().StartsWith("x rot:")
                    || line.ToLower().StartsWith("y rot:")
                    || line.ToLower().StartsWith("z rot:"))
                //capture
                {
                    string vType = line[0].ToString().ToLower();
                    string desc = line.Substring(6).Trim();

                    curVariable = new XVariable()
                    {
                        Capture = $"r{vType} &>> 0x1FF",
                        Description = desc,
                    };
                    
                    curActor.Variables.Add(curVariable);
                }
                else if (line.StartsWith("&"))
                //new variable
                {
                    var extract = MaskLine.Match(line);
                    
                    curVariable = new XVariable()
                    {
                        Capture = $"v {extract.Groups[1].Value}>> 0x{extract.Groups[2].Value}",
                        Description = extract.Groups[3].ToString()
                    };
                    curActor.Variables.Add(curVariable);
                }
                else
                //variable value
                {
                    var extract = ValueLine.Match(line).Groups;
                    var value = new XVariableValue()
                    {
                        Description = extract[4].ToString(),
                        Data = extract[1].ToString()
                    };
                    if (extract[2].ToString() == "+")
                        value.repeat = "+";

                    curVariable.Value.Add(value);
                }
            }

            return root;
        }
    }
}
