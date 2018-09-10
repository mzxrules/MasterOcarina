using mzxrules.OcaLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VerboseOcarina
{
    public partial class VersionSelector : Form
    {
        public Game Game { get; set; }
        public RomVersion Version { get; set; } = ORom.Build.UNKNOWN;

        public VersionSelector()
        {
            InitializeComponent();
        }

        private void VersionSelector_Load(object sender, EventArgs e)
        {
            if (Game == Game.OcarinaOfTime)
                versionComboBox.DataSource = ORom.GetSupportedBuilds().ToList();
            else if (Game == Game.MajorasMask)
                versionComboBox.DataSource = MRom.GetSupportedBuilds().ToList();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (versionComboBox.SelectedIndex >= 0)
            {
                Version = (RomVersion) versionComboBox.SelectedItem;
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
