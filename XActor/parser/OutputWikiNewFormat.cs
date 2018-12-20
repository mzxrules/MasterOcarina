using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mzxrules.XActor
{
    partial class OutputWikiNewFormat
    {
        static Game SetGame;
        public static StringBuilder Output(XActors root, Game game)
        {
            StringBuilder sb = new StringBuilder();
            SetGame = game;
            sb.AppendLine("== Actors ==");
            sb.AppendLine("<div style=\"font-family: monospace, Consolas, DejaVu Sans Mono, Droid Sans Mono, Lucida Console, Courier New;\">");
            foreach (XActor actor in root.Actor)
            {
                PrintActor(sb, actor, game);
                sb.AppendLine();
            }
            sb.AppendLine("</div>");
            return sb;
        }

        private static void PrintActor(StringBuilder sb, XActor actor, Game game)
        {
            sb.AppendLine($"==={actor.name}===");
            sb.AppendLine($"{actor.Description}<br />");
            sb.AppendLine($"Id: {actor.id}<br />");
            sb.AppendFormat("Object{0}: {1}<br />",
                (actor.Objects.Object.Count > 1) ? "s" : "",
                string.Join(", ", actor.Objects.Object));
            sb.AppendLine();
            PrintComments(sb, actor.Comment);
            if (actor.Variables.Count > 0)
                sb.AppendLine("<br />");

            foreach (XVariable var in actor.Variables)
            {
                PrintVariable(sb, var, game);
            }
            if (!string.IsNullOrEmpty(actor.CommentOther))
            {
                sb.AppendLine();
                PrintComments(sb, actor.CommentOther);
            }
        }


        /// <summary>
        /// Prints the stats on a packed initialization variable
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="var"></param>
        private static void PrintVariable(StringBuilder sb, XVariable var, Game game)
        {
            //sb.AppendFormat(" {0} {1} - {2} ",
            //    (var.maskType == MaskType.And) ? "&" : "|",
            //    var.mask,
            //    var.Description);
            sb.Append($";{var.Capture} = {var.Description}");
            //sb.Append($" {GetCaptureCatch(var.Capture)} - {var.Description} ");

            PrintComments(sb, var.Comment, true);
            sb.AppendLine();

            CaptureExpression capture = new CaptureExpression(var.Capture);
            foreach (XVariableValue value in var.Value)
            {
                PrintVariableValue(sb, value, capture);
            }
        }

        private static void PrintVariableValue(StringBuilder sb, XVariableValue value, CaptureExpression capture)
        {
            int shiftback = ShiftBack(int.Parse(value.Data, System.Globalization.NumberStyles.HexNumber), capture.Mask);
            string obj = null;

            if (SetGame != Game.Oca && capture.VarType != CaptureVar.v)
            {
                shiftback = ShiftBack(shiftback, 0xFF80);
            }

            if (value.Meta != null)
                obj = string.Join(", ", value.Meta.Object);
            obj = (!string.IsNullOrEmpty(obj)) ? $" ({obj})" : "";


            sb.AppendFormat(":{0} [{2:X4}]{1} {3} {4}",
                value.Data,
                (!string.IsNullOrEmpty(value.repeat)) ? "+" : "",
                shiftback,
                obj,
                value.Description);
            PrintComments(sb, value.Comment, true);
            sb.AppendLine();
        }

        private static int ShiftBack(int p, int mask)
        {
            return p << GetShift(mask);
        }
        private static int GetShift(int mask)
        {
            int shift;

            //a mask of 0 isn't valid, but could be set in the xml file by mistake.
            if (mask == 0)
                return 0;

            //Check the right bit
            //If the right bit is 0 shift the mask over one and increment the shift count
            //If the right bit is 1, the right side of the mask is found, we know how much to shift by
            for (shift = 0; (mask & 1) == 0; mask >>= 1)
            {
                shift++;
            }

            return shift;
        }

        private static void PrintComments(StringBuilder sb, string p, bool inline = false)
        {
            string[] commentLines;
            bool emptyLine = false;
            bool firstLine = true;

            if (p == null)
                return;

            commentLines = p.Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.None);

            if (commentLines.Length == 0)
                return;

            if (inline == true)
            {
                if (commentLines.Length == 1)
                {
                    var comment = commentLines[0].Trim();
                    sb.Append($" //{comment}");
                    return;
                }
                else
                {
                    sb.AppendLine("<br />");
                }
            }

            for (int i = 0; i < commentLines.Length; i++)
            {
                string s = commentLines[i].Trim();

                if (s.Length == 0)
                {
                    emptyLine = true;
                    continue;
                }
                if (emptyLine && !firstLine)
                {
                    sb.AppendLine();
                }
                emptyLine = false;
                firstLine = false;
                sb.AppendLine($"//{s}<br />");
            }
        }
    }
}
