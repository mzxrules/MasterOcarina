using mzxrules.Helper;
using mzxrules.OcaLib;
using mzxrules.OcaLib.Cutscenes;
using mzxrules.OcaLib.PathUtil;
using mzxrules.OcaLib.SceneRoom;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VerboseOcarina
{
    public partial class VerboseOcarina : Form
    {
        Rom rom;
        RomVersion DefaultVersion = ORom.Build.N0;
        Dictionary<RadioButton, RomVersion> RadioButtonBindings = new Dictionary<RadioButton, RomVersion>() {};

        public VerboseOcarina()
        {
            InitializeComponent();
            SetRadioButtonBindings();
            PathUtil.Initialize();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private void SetRadioButtonBindings()
        {
            RadioButtonBindings = new Dictionary<RadioButton, RomVersion>
            {
                //Ocarina of Time Versions
                //NTSC
                { v10RadioButton, ORom.Build.N0 },
                { v11RadioButton, ORom.Build.N1 },
                { v12RadioButton, ORom.Build.N2 },
                //PAL
                { v10PRadioButton, ORom.Build.P0 },
                { v11PRadioButton, ORom.Build.P1 },
                //GCN Vanilla
                { gcnjRadioButton, ORom.Build.GCJ },
                { gcnuRadioButton, ORom.Build.GCU },
                { gcnpRadioButton, ORom.Build.GCP },
                //MQ
                { mqjRadioButton, ORom.Build.MQJ },
                { mquRadioButton, ORom.Build.MQU },
                { mqpRadioButton, ORom.Build.MQP },
                //Special
                { dbgRadioButton, ORom.Build.DBGMQ },
                { chnRadioButton, ORom.Build.IQUEC },
                { tChnRadioButton, ORom.Build.IQUET },
                { customOoTRadioButton, ORom.Build.CUSTOM },
                //Majora's Mask Versions
                { mJ10RadioButton, MRom.Build.J0 },
                { mJ11RadioButton, MRom.Build.J1 },
                { mU10RadioButton, MRom.Build.U0 },
                { mP10RadioButton, MRom.Build.P0 },
                { mP11RadioButton, MRom.Build.P1 },
                { mGCJRadioButton, MRom.Build.GCJ },
                { mGCURadioButton, MRom.Build.GCU },
                { mGCPRadioButton, MRom.Build.GCP },
                { mDBGRadioButton, MRom.Build.DBG },
                { customMMRadioButton, MRom.Build.CUSTOM },

            };
        }

        private void SetRomTypeSettings(RomVersion version)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            DialogResult result = DialogResult.OK;
            RomVersion inputVersion = version;

            //Set openFile title in case we need to look for a rom
            openFile.Title = $"Open {version} rom";

            string romLocation;
            if (!version.IsCustomBuild())
            {
                if (!PathUtil.TryGetRomLocation(version, out romLocation))
                {
                    result = openFile.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        PathUtil.SetRomLocation(version, openFile.FileName);
                        PathUtil.TryGetRomLocation(version, out romLocation);
                    }
                }
            }
            else
            {
                result = openFile.ShowDialog();
                romLocation = openFile.FileName;

                if (result == DialogResult.OK)
                {
                    using (VersionSelector vs = new VersionSelector())
                    {
                        vs.Game = version.Game;
                        result = vs.ShowDialog();
                        version = vs.Version;
                    }
                }
            }
            openFile.Dispose();

            rom = result == DialogResult.OK ? Rom.New(romLocation, version) : null;

            //update
            string romStats = rom == null ? 
                $"Error: No Rom!" 
                : $"{inputVersion.Game}, File Stats Mode: {version}{Environment.NewLine}{romLocation}";

            outputRichTextBox.Clear();
            outputRichTextBox.AppendText(romStats);
        }

        private void fetchButton_Click(object sender, EventArgs e)
        {
            if (rom == null)
                return;

            if (sceneRadioButton.Checked)
            {
                FetchScene();
            }
            else
            {
                if (!int.TryParse(numberTextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int id))
                    return;

                if (actorRadioButton.Checked)
                    FetchActors(id);
                else if (objectRadioButton.Checked)
                    FetchObjects(id);
                else if (commandRadioButton.Checked)
                    FetchCommand(id);
                else if (cutsceneRadioButton.Checked)
                    FetchCutscene(id);
                else if (textRadioButton.Checked)
                    FetchMessageOoT(id);
            }
        }

        private void FetchMessageOoT(int id)
        {
            ORom trom = (ORom)rom;
            List<Rom.Language> languages = trom.GetSupportedLanguages().ToList();

            outputRichTextBox.Clear();

            foreach (Rom.Language lang in languages)
            {
                outputRichTextBox.AppendText($" {lang}:{Environment.NewLine}");
                if (lang == Rom.Language.Japanese)
                {
                    var font = outputRichTextBox.Font;
                    outputRichTextBox.SelectionFont = new System.Drawing.Font("MS PGothic", 8.25f);
                    outputRichTextBox.AppendText(trom.Text.GetMessage((ushort)id, lang));
                    outputRichTextBox.SelectionFont = font;
                }
                else
                    outputRichTextBox.AppendText(trom.Text.GetMessage((ushort)id, lang));
                outputRichTextBox.AppendText(Environment.NewLine + Environment.NewLine );
            }
        }

        private void FetchMessage(string s)
        {
            string[] txt;
            if (numberTextBox.Text.Contains("."))
            {
                txt = numberTextBox.Text.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                if (txt.Length == 2
                    && int.TryParse(txt[0], out int langId)
                    && int.TryParse(txt[1], NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture, out int mesgId))
                {
                    FetchMessageOoT(mesgId, langId);
                }
            }
        }

        private void FetchAllMessages()
        {
            StringBuilder sb = new StringBuilder();
            List<Rom.Language> languages = new List<Rom.Language>();

            TryFetchAllMessages();
            outputRichTextBox.Text = sb.ToString();

            void TryFetchAllMessages()
            {
                if (!int.TryParse(numberTextBox.Text, out int langId))
                {
                    langId = -1;
                }

                if (rom is ORom oRom)
                {
                    languages = oRom.GetSupportedLanguages().ToList();
                    if (langId < 0 || langId >= languages.Count)
                    {
                        PrintLanguages();
                        return;
                    }
                    var lang = languages[langId];
                    foreach (ushort mesgId in oRom.Text.GetMessageEnumerator(lang))
                    {
                        sb.AppendLine($"[ID {mesgId:X4}]");
                        sb.AppendLine(oRom.Text.GetMessage(mesgId, lang));
                        sb.AppendLine();
                    }
                }
                else if (rom is MRom mRom)
                {
                    PrintLanguages();
                }
                return;

                void PrintLanguages()
                {
                    sb.AppendLine("Invalid language id. See the following supported ids:");
                    for (int i = 0; i < languages.Count; i++)
                    {
                        sb.AppendLine($"{i}: {languages[i]}");
                    }
                }
            }
        }

        private void FetchMessageOoT(int mesgId, int langId)
        {
            List<Rom.Language> languages = ((ORom)rom).GetSupportedLanguages().ToList();

            if (!(langId < languages.Count)) 
                return;
            
            outputRichTextBox.Text = ((ORom)rom).Text.GetMessage((ushort)mesgId, languages[langId]);
        }

        private void FetchCutscene(int address)
        {
            outputRichTextBox.Clear();
            if (!SceneRoomReader.TryGetCutscene(rom, address, out Cutscene cutscene))
            {
                outputRichTextBox.Text = "Input address does not point within a file.";
            }
            else
            {
                outputRichTextBox.Text = cutscene.PrintByOccurrence();
                    //+ "==== TIMELINE ====" + Environment.NewLine + cutscene.PrintByTimeline();
            }
        }

        private void fetchAllButton_Click(object sender, EventArgs e)
        {
            if (rom == null)
                return;
            if (sceneRadioButton.Checked)
            {
                FetchAllScenes();
            }
            else if (actorRadioButton.Checked)
            {
                FetchActors(-1);
            }
            else if (objectRadioButton.Checked)
            {
                FetchObjects(-1);
            }
            else if (textRadioButton.Checked)
            {
                FetchAllMessages();
            }
        }

        private void FetchScene()
        {
            StringBuilder sb = new StringBuilder();

            if (!int.TryParse(numberTextBox.Text, out int sceneNumber)
                || sceneNumber < 0
                || !(sceneNumber < rom.Scenes))
            {
                MessageBox.Show($"Value must be between 0 and {rom.Scenes - 1}");
                return;
            }

            outputRichTextBox.Clear();
            PrintScene(sceneNumber, sb);
            outputRichTextBox.Text = sb.ToString();
        }

        private void PrintScene(int sceneId, StringBuilder sb)
        {
            Scene scene = SceneRoomReader.InitializeScene(rom.Files.GetSceneFile(sceneId), sceneId);
            List<Room> rooms = new List<Room>();
            if (scene == null)
            {
                sb.AppendFormat("Exception: Scene not found");
                sb.AppendLine();
                return;
            }

            var roomAddrs = scene.Header.GetRoomAddresses();
            for (int i = 0; i < roomAddrs.Count; i++)
            {
                FileAddress addr = roomAddrs[i];

                //if (scene.ID == 6 && rom.Version == ORom.Build.N0)
                //{
                //    rooms.Add(SceneRoomReader.LoadSpiritRoom(addr, i));
                //}
                //else

                try
                {
                    RomFile file = rom.Files.GetFile(addr);
                    rooms.Add(SceneRoomReader.InitializeRoom(file));
                }
                catch
                {
                    sb.AppendLine($"Exception: room {addr.Start:X8} not found");
                }

            }
            
            sb.Append(SceneRoomReader.ReadScene(scene));
            for (int i = 0; i < rooms.Count; i++)
            {
                sb.AppendLine($"Room {i}");
                sb.Append(SceneRoomReader.ReadRoom(rooms[i]));
            }
        }

        private void FetchAllScenes()
        {
            StringBuilder sb = new StringBuilder();

            outputRichTextBox.Clear();

            for (int i = 0; i < rom.Scenes; i++)
            {
                sb.AppendLine("-- SCENE " + i.ToString() + " --");
                PrintScene(i, sb);
            }
            outputRichTextBox.Text = sb.ToString();
        }

        private void FetchActors(int id)
        {
            outputRichTextBox.Clear();
            outputRichTextBox.Text = SceneRoomReader.GetActorsById(rom, id);
        }

        private void FetchObjects(int id)
        {
            outputRichTextBox.Clear();
            var oldFont = outputRichTextBox.Font;
            var prot = outputRichTextBox.SelectionProtected;
            outputRichTextBox.SelectionFont = new System.Drawing.Font("Lucida Console", 8.25f);
            outputRichTextBox.AppendText(SceneRoomReader.GetObjectsById(rom, id));
            outputRichTextBox.SelectionFont = oldFont;
        }

        private void FetchCommand(int id)
        {
            outputRichTextBox.Clear();
            outputRichTextBox.Text = SceneRoomReader.GetCommandsById(rom, id);
        }

        private void versionRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton version = (RadioButton)sender;
            if (version.Checked == false)
                return;
            if (RadioButtonBindings.TryGetValue(version, out RomVersion versionId))
                SetRomTypeSettings(versionId);
            else
                throw new NotImplementedException();
        }

        private void evalButton_Click(object sender, EventArgs e)
        {
            if (rom == null)
                return;
            StringBuilder sb = new StringBuilder();
            for (int sceneNumber = 0; sceneNumber < rom.Scenes; sceneNumber++)
            {
                Scene scene = null;
                var sceneFile = rom.Files.GetSceneFile(sceneNumber);
                if (sceneFile == null)
                {
                    sb.AppendLine($"Warning: Scene {sceneNumber} not found");
                    continue;
                }
                try
                {
                    scene = SceneRoomReader.InitializeScene(sceneFile, sceneNumber);
                }
                catch (Exception ex)
                {
                    var va = sceneFile.Record.VRom;
                    sb.AppendLine($"ParseError: Scene {sceneNumber} {va.Start:X8}-{va.End:X8}");
                    sb.AppendLine(ex.TargetSite.ToString());
                }

                var roomAddrs = scene.Header.GetRoomAddresses();
                for (int i = 0; i < roomAddrs.Count; i++)
                {
                    FileAddress addr = roomAddrs[i];
                    RomFile roomFile = null;
                    
                    try
                    {
                        roomFile = rom.Files.GetFile(addr);
                        Room room = SceneRoomReader.InitializeRoom(roomFile);
                    }
                    catch //(Exception ex)
                    {
                        if (roomFile == null)
                            sb.AppendLine($"Exception: Scene {sceneNumber}, room {addr.Start:X8}:{addr.End:X8} not found");
                        else
                        {
                            sb.AppendLine($"ParseError: Scene {sceneNumber}, room {addr.Start:X8}:{addr.End:X8}");
                            //sb.AppendLine(ex.StackTrace.ToString());
                        }
                    }

                }
            }
            sb.AppendLine("Evaluation Complete");
            outputRichTextBox.Text = sb.ToString();
        }

        private void VerboseOcarina_Shown(object sender, EventArgs e)
        {
            numberTextBox.Text = "0";
            SetRomTypeSettings(DefaultVersion);
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {

        }
    }
}
