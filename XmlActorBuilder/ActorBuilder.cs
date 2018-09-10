using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace XmlActorBuilder
{
    public partial class ActorBuilder : Form
    {
        ActorDatabase db;
        public ActorBuilder()
        {
            InitializeComponent();
        }

        private void experimentButton_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
        }

        private void openFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ActorDatabase));
            using (XmlReader reader = XmlReader.Create(openFileDialog.FileName))
            {
                db = (ActorDatabase)xmlSerializer.Deserialize(reader);
            };
            comboBox1.DataSource = db.Definition;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = comboBox1.Items[comboBox1.SelectedIndex];
        }

        private void saveFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ActorDatabase));
            using (XmlWriter writer = XmlWriter.Create(saveFileDialog.FileName, settings))
            {

                xmlSerializer.Serialize(writer, db);
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (db != null)
            {
                saveFileDialog.ShowDialog();
            }
        }

        private void toggleCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (db == null)
                return;

            if (toggleCheckBox.Checked)
            {
                propertyGrid1.SelectedObject = db;
            }
            else if (comboBox1.SelectedIndex >= 0)
            {
                propertyGrid1.SelectedObject = comboBox1.Items[comboBox1.SelectedIndex];
            }

        }

        private void newActorButton_Click(object sender, EventArgs e)
        {
            ActorDatabase template;

            if (db == null)
                return;

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ActorDatabase));
            using (XmlReader reader = XmlReader.Create("Default.xml"))
            {
                 template = (ActorDatabase)xmlSerializer.Deserialize(reader);
            }

            var defList = db.Definition.ToList();
            defList.Add(template.Definition[0]);
            db.Definition = defList.ToArray();
            comboBox1.DataSource = db.Definition;
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            comboBox1.DataSource = new List<string>();
            comboBox1.DataSource = db.Definition;
        }
    }
}
