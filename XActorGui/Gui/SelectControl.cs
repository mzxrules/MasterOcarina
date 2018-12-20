using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;

namespace mzxrules.XActor.Gui
{
    public partial class SelectControl : BaseControl
    {
        public SelectControl()
        {
            InitializeComponent();
        }

        public override void SetUi(XVariable node)
        {
            base.SetUi(node);
            List<XVariableValueDisplay> display = new List<XVariableValueDisplay>();

            descriptionLabel.Text = VariableDef.Description;
            foreach (var item in node.Value)
            {
                if (!item.uihide)
                display.Add(new XVariableValueDisplay(item));
            }
            comboBox.DataSource = display;

            if (!node.nullable)
                nullCheckBox.Visible = false;

            UpdateValue(Default);
        }

        protected override void UpdateValue(ushort value)
        {
            base.UpdateValue(value);
            var comboItem = (from e in (List<XVariableValueDisplay>)comboBox.DataSource
                             where Convert.ToUInt16(e.Source.Data, 16) == value
                             select e).SingleOrDefault();

            if (comboItem != null)
            {
                comboBox.SelectedItem = comboItem;
            }
            else
            {
                comboBox.SelectedIndex = -1;
                comboBox.Text = "Unknown";
            }
        }

        private void comboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var item = (XVariableValueDisplay)comboBox.SelectedValue;

            UpdateValue(item.Value);
        }

        private void inputTextBox_Validating(object sender, CancelEventArgs e)
        {
            base.inputTextBox_Base_Validating(sender, e);
        }

        private void inputTextBox_Validated(object sender, EventArgs e)
        {
            base.inputTextBox_Base_Validated(sender, e);
        }

        private void nullCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            base.nullCheckBox_Base_CheckedChanged(sender, e);
        }
    }
}
