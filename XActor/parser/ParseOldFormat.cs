using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mzxrules.XActor
{
    class ParseOldFormat
    {
        Regex Comment = new Regex(@"//(.*?)$");
        Regex IdLine = new Regex(@"^([0-9a-fA-F]{4})\s([0-9a-fA-F]{4})\b(.*?)$");
        Regex MaskLine = new Regex(@"^\s*(&|\|)\s*([0-9a-fA-F]{4})\s*(?:-|=)\s*(.*?)$");
        Regex ValueLine = new Regex(@"^\s*(?:-|=)\s*([0-9a-fA-F]{1,4})(\+)?\s*(?:\(([0-9a-fA-F?]{4})\))?\s*(?:-|=)\s*(.*?)$");

        public XActors ParseLines(String[] lines)
        {
            XActors root = new XActors();
            String currentComment = string.Empty;
            XActor currentActor = null;
            XVariable currentVariable = null;
            Object commentParent = null;

            foreach (string line in lines)
            {
                string line_nocomment;
                String lineComment;//comment within the current line

                //remove comment
                line_nocomment = StripComment(line, out lineComment);

                //if line only contains a comment
                if (line_nocomment.Trim().Length == 0)
                {
                    //continue the current comment
                    currentComment += (String.IsNullOrEmpty(currentComment)) ? lineComment : Environment.NewLine + lineComment;
                    continue;
                }

                //Else match one of three patterns:
                //Actor/Object id line,
                //Mask Line
                //Value line

                //If Actor/Object id line
                if (IdLine.IsMatch(line_nocomment))
                {
                    //attach currentComment to proper node
                    AddCommentToNode(ref commentParent, ref currentComment, lineComment);

                    //Start a new XActor, attach it
                    currentActor = new XActor();
                    root.Actor.Add(currentActor);

                    //set new comment node
                    commentParent = currentActor;
                    
                    //null current variable
                    currentVariable = null;

                    NewActorDefinition(IdLine.Matches(line_nocomment)[0].Groups, currentActor);
                }
                //mask value line
                else if (MaskLine.IsMatch(line_nocomment))
                {
                    //Push currentComment to proper node
                    AddCommentToNode(ref commentParent, ref currentComment, lineComment);

                    currentVariable = new XVariable();
                    currentActor.Variables.Add(currentVariable);

                    commentParent = currentVariable;
                    throw new NotImplementedException();
                    //NewVariableDefinition(MaskLine.Matches(line_nocomment)[0].Groups, currentVariable);
                }
                else if (ValueLine.IsMatch(line_nocomment))
                {
                    //Push currentComment to proper node
                    AddCommentToNode(ref commentParent, ref currentComment, lineComment);

                    if (currentVariable == null)
                    {
                        currentVariable = NoMaskSet(currentActor);
                    }

                    XVariableValue val = new XVariableValue();
                    currentVariable.Value.Add(val);

                    commentParent = val;

                    NewVariableValue(ValueLine.Matches(line_nocomment)[0].Groups, val);
                }
                else //unknown
                {
                    currentComment += String.Format("{1}#NOPARSE: {0}", line_nocomment,
                        (String.IsNullOrEmpty(currentComment)) ? string.Empty : Environment.NewLine);
                    continue;
                }
            }
            AddCommentToNode(ref commentParent, ref currentComment, string.Empty);
            return root;
        }

        private void NewVariableValue(GroupCollection groupCollection, XVariableValue val)
        {
            throw new NotImplementedException();
            //Regex ValueLine = new Regex(@"^\s*(?:-|=)\s*([0-9a-fA-F]{1,4})(\+)?\s*(?:\(([0-9a-fA-F?]{4})\))?\s*(?:-|=)\s*(.*?)$");
            //Groups:
            //Hex Value, repeater?, Hex Object?, description

            //val.Data = groupCollection[1].Value;
            //if (!String.IsNullOrEmpty(groupCollection[2].Value))
            //    val.repeat = groupCollection[2].Value;
            //if (!String.IsNullOrEmpty(groupCollection[3].Value))
            //    val.Data.objectid = groupCollection[3].Value;
            //val.Description = groupCollection[4].Value.Trim();

        }

        private XVariable NoMaskSet(XActor currentActor)
        {
            XVariable r = new XVariable();
            currentActor.Variables.Add(r);

            //r.maskType =  MaskType.And;
            //r.mask = "FFFF";
            r.Description = "#N/A";
            return r;
        }

        //private void NewVariableDefinition(GroupCollection groupCollection, XVariable currentVariable)
        //{
        //    //Groups:
        //    //Mask type, Mask, Description
        //    currentVariable.maskType = groupCollection[1].Value;
        //    currentVariable.mask = groupCollection[2].Value;
        //    currentVariable.Description = groupCollection[3].Value.Trim();
        //}

        private void NewActorDefinition(GroupCollection groupCollection, XActor currentActor)
        {
            //Groups:
            //actor id hex, object id hex, description (includes multiple objects)
            currentActor.id = groupCollection[1].Value;
            currentActor.Objects.Object.Add(groupCollection[2].Value);
            currentActor.Description = groupCollection[3].Value.Trim();
        }


        private void AddCommentToNode(ref object commentParent, ref string currentComment, string lineComment)
        {
            //Testing that the comment is empty will prevent null exception on first pass
            if (!String.IsNullOrEmpty(currentComment))
            {
                //determine commentParent type
                if (commentParent is XActor)
                    ((XActor)commentParent).Comment = currentComment;
                else if (commentParent is XVariable)
                    ((XVariable)commentParent).Comment = currentComment;
                else if (commentParent is XVariableValue)
                    ((XVariableValue)commentParent).Comment = currentComment;
                else throw new ArgumentNullException();
                
            }
            currentComment = lineComment;
            commentParent = null;
        }

        private string StripComment(string line, out string lineComment)
        {
            if (Comment.IsMatch(line))
            {
                lineComment = Comment.Match(line).Groups[1].Value;
                return Comment.Replace(line, "");
            }
            else
            {
                lineComment = String.Empty;
                return line;
            }
        }
    }
}
