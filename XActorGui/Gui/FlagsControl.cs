using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mzxrules.XActor.Gui
{
    public partial class FlagsControl : NumControl
    {
        public FlagsControl()
        {
            InitializeComponent();
        }

        protected override void UpdateValue(ushort value)
        {
            string flagType;
            string flagDomain;
            int flagId;
            base.UpdateValue(value);

            switch (VariableDef.UI.Item)
            {
                case UIChestFlag cf: flagType = "Chest"; break;
                case UISwitchFlag sf: flagType = "Switch"; break;
                case UICollectFlag cf: flagType = "Collectible"; break;
                default: flagType = "Error"; break;
            }

            if (value < 0x20)
            {
                flagDomain = "Permanent";
                flagId = value;
            }
            else if (value < 0x38)
            {
                flagDomain = "Temporary";
                flagId = value - 0x20;
            }
            else if (value < 0x3F)
            {
                flagDomain = "Room Local";
                flagId = value - 0x38;
            }
            else
            {
                flagDomain = "No";
                flagId = value;
            }

            var Value = (from v in VariableDef.Value
                        where Convert.ToInt16(v.Data, 16) == value
                        select v).SingleOrDefault();


            notesTextBox.Text = string.Format("{0} {1} Flag: {2:X2}{3}",
                flagDomain, flagType, flagId,
                (Value == null) ? "" : " //" + Value.Description);
        }

        private void inputTextBox_Validating(object sender, CancelEventArgs e)
        {
            base.inputTextBox_Base_Validating(sender, e);
        }

        private void inputTextBox_Validated(object sender, EventArgs e)
        {
            base.inputTextBox_Base_Validated(sender, e);
        }
    }
}
