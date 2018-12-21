using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using mzxrules.OcaLib;

namespace mzxrules.XActor
{
    public partial class TestForm : Form
    {
        XActors Document { get; set; }
        Game Game = Game.OcarinaOfTime;


        public TestForm()
        {
            InitializeComponent();
        }

        private void TestForm_Load(object sender, EventArgs e)
        {
            Document = null;
        }

        private void serializeButton_click(object sender, EventArgs e)
        {
            var test = XActorFactory.NewOcaActor(new short[] { 0, 0, 0, 0, 0, 0, 0, 0 });
            if (Document != null)
                outRichTextBox.Text = Document.Serialize();
            //ParseMMGbFormat();
            //SCRIPT_Ui_Test();
            //SCRIPT_Select_Width_Test();   
        }

        private void SCRIPT_Select_Width_Test()
        {
            List<string> Descriptions = new List<string>();

            foreach (var actor in Document.Actor)
            {
                foreach (var variable in actor.Variables)
                {
                    foreach (var value in variable.Value)
                        Descriptions.Add(string.Format("{0}:{1}", actor.id, value.Description));
                }
            }
            Descriptions = Descriptions.OrderBy(x => x.Length).ToList();

            StringBuilder sb = new StringBuilder();
            foreach (string s in Descriptions)
                sb.AppendLine(s);

            outRichTextBox.Text = sb.ToString();
        }

        private void SCRIPT_Ui_Test()
        {
            List<string> Descriptions = new List<string>();

            foreach (var actor in Document.Actor)
            {
                foreach (var variable in actor.Variables)
                {
                    if (variable.UI.Item is UIDefault)
                        Descriptions.Add($"{actor.id}:Default");

                }
            }
            Descriptions = Descriptions.OrderBy(x => x.Length).ToList();

            StringBuilder sb = new StringBuilder();
            foreach (string s in Descriptions)
                sb.AppendLine(s);

            outRichTextBox.Text = sb.ToString();
        }

        private void SetBitflagParam()
        {
            XActors actors;
            Int16[] bitDictionary = new Int16[16];
            actors = XActors.LoadFromFile(XActors.OcaXmlPath);

            for (int i = 0; i < 16; i++)
            {
                bitDictionary[i] = (Int16)(1 << i);
            }

            foreach (XActor actor in actors.Actor)
            {
                var value = from v in actor.Variables
                            where bitDictionary.Contains(GetCaptureMask(v.Capture))
                            select v;

                foreach (var v in value)
                {
                    if (v.UI.Item is UIDefault)
                        v.UI.Item = new UIBitFlag();
                }
            }
            outRichTextBox.Text = actors.Serialize();
        }

        private short GetCaptureMask(string capture)
        {
            throw new NotImplementedException();
        }

        private void ParseOld()
        {
            string[] Lines;
            XActors resultXActors;
            ParseOldFormat oldFormat = new ParseOldFormat();

            Lines = dataInRichTextBox.Text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            resultXActors = oldFormat.ParseLines(Lines);

            XmlSerializer serializer = new XmlSerializer(typeof(XActors));
            XmlWriterSettings xmlOutSettings = new XmlWriterSettings()
            {
                Indent = true
            };
            StringBuilder sb = new StringBuilder();

            using (XmlWriter writer = XmlWriter.Create(sb, xmlOutSettings))
            {
                serializer.Serialize(writer, resultXActors);
            }
            dataInRichTextBox.Text = sb.ToString();

            outRichTextBox.Text = OutputWikiOldFormat.Output(resultXActors).ToString();
        }

        private void ParseMMGbFormat()
        {
            string[] Lines;
            XActors resultXActors;

            Lines = dataInRichTextBox.Text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            resultXActors = ParseGBFormat.ParseLines(Lines);

            XmlSerializer serializer = new XmlSerializer(typeof(XActors));
            XmlWriterSettings xmlOutSettings = new XmlWriterSettings()
            {
                Indent = true
            };
            StringBuilder sb = new StringBuilder();

            using (XmlWriter writer = XmlWriter.Create(sb, xmlOutSettings))
            {
                serializer.Serialize(writer, resultXActors);
            }
            dataInRichTextBox.Text = sb.ToString();

            outRichTextBox.Text = OutputWikiOldFormat.Output(resultXActors).ToString();

        }

        private void TestGarbage()
        {
            string[] Lines;

            Lines = dataInRichTextBox.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);


            Regex TestRegex = new Regex(""); //new Regex(IdLine + Comment);


            MatchCollection matches = TestRegex.Matches(Lines[0].Trim());

            dataInRichTextBox.Clear();

            foreach (Match m in matches)
            {
                //firstConvertRichTextBox.AppendText(m.ToString());
                //firstConvertRichTextBox.AppendText(Environment.NewLine);
                foreach (Group g in m.Groups)
                {
                    dataInRichTextBox.AppendText(g.Value);
                    dataInRichTextBox.AppendText(Environment.NewLine);
                }

            }

            //firstConvertRichTextBox.Text = IdLine.IsMatch(Lines[0].Trim()).ToString();
        }
        

        private void ActorObjectRelationshipsFromXml()
        {
            XActors actorList;
            StringBuilder sb = new StringBuilder();
            List<Tuple<string, string>> ActorToObjects = new List<Tuple<string,string>>();

            actorList = XActors.LoadFromFile(XActors.OcaXmlPath);

            sb.AppendLine("{|class=\"wikitable sortable\"");
            sb.AppendLine("! data-sort-type=\"text\" | Actor");
            sb.AppendLine("! data-sort-type=\"text\" | Object");
            foreach (XActor actor in actorList.Actor)
            {
                foreach (string obj in actor.Objects.Object)
                {
                    if (obj != "????")
                    {
                        ActorToObjects.Add(new Tuple<string,string>(actor.id, obj));
                    }
                    else 
                    {
                        //var objList = actor.Variables.SelectMany(v => v.Value)
                        //     .Select(x => x.Data.objectid)
                        //     .Where(y => !String.IsNullOrEmpty(y))
                        //     .Distinct();

                        //foreach (string obj_sub in objList)
                        //{
                        //    ActorToObjects.Add(new Tuple<string, string>(actor.id, obj_sub));
                        //}
                    }
                }
            }
            foreach (Tuple<string, string> a2o in ActorToObjects)
            {
                sb.AppendLine("|-");
                sb.AppendFormat("|{0}||{1}", a2o.Item1, a2o.Item2);
                sb.AppendLine();
            }
            sb.AppendLine("|}");
            dataInRichTextBox.Text = sb.ToString();
        }


        private void PrintListFromXml()
        {
            outRichTextBox.Text += OutputWikiNewFormat.Output(Document, Game).ToString().TrimEnd();
        }

        private void uiTestButton_Click(object sender, EventArgs e)
        {
            if (short.TryParse(actorTextBox.Text, NumberStyles.HexNumber,
                CultureInfo.InvariantCulture, out short actor))
            {
                actorControl.SetActor(actor);
            }
        }

        private void loadOcaButton_Click(object sender, EventArgs e)
        {
            Document = XActors.LoadFromFile(XActors.OcaXmlPath);
            actorControl.Document = Document;
            Game = Game.OcarinaOfTime;

            //foreach (var item in Document.Actor)
            //{
            //    item.name = ActorNameConstants.OcaActorNames[Convert.ToUInt16(item.id, 16)];
            //}
        }
        private void loadMMButton_Click(object sender, EventArgs e)
        {
            Document = XActors.LoadFromFile(XActors.MaskXmlPath);
            actorControl.Document = Document;
            Game = Game.MajorasMask;

            //foreach(var item in Document.Actor)
            //{
            //    item.name = ActorNameConstants.MaskActorNames[Convert.ToUInt16(item.id, 16)];
            //}
            //ParseMMGbFormat();
        }

        private void wikiButton_Click(object sender, EventArgs e)
        {
            //ActorObjectRelationshipsFromXml();
            
            if (Document != null)
                PrintListFromXml();
        }

    }
}
