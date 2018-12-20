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
    public partial class BaseControl : UserControl
    {
        public XVariable VariableDef { get; private set; }
        public UInt16 Value { get; protected set; }
        protected UInt16 Default;
        protected UInt16 Mask;
        protected UInt16 ShiftedMask;

        public BaseControl()
        {
            InitializeComponent();
        }
        public virtual void SetUi(XVariable node)
        {
            VariableDef = node;
            GetMaskFromCapture(node.Capture, out Mask, out ShiftedMask);
            //Helper.TryParseHex(node.mask, out Mask);
            //ShiftedMask = Helper.ShiftMask(Mask);
            Helper.TryParseHex(node.@default, out Default);
        }

        private void GetMaskFromCapture(string capture, out ushort mask, out ushort shiftedMask)
        {
            mask = Convert.ToUInt16(capture.Substring(capture.IndexOf("0x")), 16);
            shiftedMask = Helper.ShiftMask(mask);
        }

        protected virtual void UpdateValue(ushort value)
        {
            Value = value;
            inputTextBox.Text = value.ToString("X");
        }

        protected void inputTextBox_Base_Validating(object sender, CancelEventArgs e)
        {
            TextBox inputTextBox = (TextBox)sender;
            UInt16 value;

            if (!UInt16.TryParse(inputTextBox.Text, System.Globalization.NumberStyles.HexNumber,
                CultureInfo.InvariantCulture, out value))
            {
                e.Cancel = true;
                this.errorProvider.SetError(inputTextBox, "Insert a hexadecimal number");
            }
            else if (value > (ushort)ShiftedMask)
            {
                e.Cancel = true;
                this.errorProvider.SetError(inputTextBox, "Value outside the defined range");
            }
        }

        protected void inputTextBox_Base_Validated(object sender, EventArgs e)
        {
            UInt16 v;
            TextBox tInputTextBox = (TextBox)sender;

            this.errorProvider.SetError(tInputTextBox, "");
            Helper.TryParseHex(tInputTextBox.Text, out v);

            this.UpdateValue(v);
        }

        protected void nullCheckBox_Base_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkbox = (CheckBox)sender;

            if (checkbox.Checked)
                UpdateValue(Default);
        }

    }
}
