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
    public partial class NumControl : BaseControl
    {
        public TextBox notesTextBox;
    
        public NumControl()
        {
            InitializeComponent();
        }

        public override void SetUi(XVariable node)
        {
            base.SetUi(node);

            descriptionLabel.Text = node.Description;
            if (!node.nullable)
                nullCheckBox.Visible = false;
            UpdateValue(Default);
        }

        protected override void UpdateValue(ushort value)
        {
            base.UpdateValue(value);

            var valueNote = (from e in VariableDef.Value
                             where Convert.ToInt16(e.Data, 16) == value
                             select e).SingleOrDefault();

            if (valueNote != null)
                notesTextBox.Text = valueNote.Description;
            else
                notesTextBox.Text = "";
        }

        private void inputTextBox_Validating(object sender, CancelEventArgs e)
        {
            inputTextBox_Base_Validating(sender, e);
        }

        private void inputTextBox_Validated(object sender, EventArgs e)
        {
            inputTextBox_Base_Validated(sender, e);
        }

        private void nullCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            base.nullCheckBox_Base_CheckedChanged(sender, e);
        }

        private void InitializeComponent()
        {
            this.notesTextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // inputTextBox
            // 
            this.inputTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.inputTextBox_Validating);
            this.inputTextBox.Validated += new System.EventHandler(this.inputTextBox_Validated);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.notesTextBox);
            this.panel1.Controls.SetChildIndex(this.nullCheckBox, 0);
            this.panel1.Controls.SetChildIndex(this.inputTextBox, 0);
            this.panel1.Controls.SetChildIndex(this.notesTextBox, 0);
            // 
            // notesTextBox
            // 
            this.notesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.notesTextBox.Location = new System.Drawing.Point(91, 0);
            this.notesTextBox.Name = "notesTextBox";
            this.notesTextBox.ReadOnly = true;
            this.notesTextBox.Size = new System.Drawing.Size(296, 20);
            this.notesTextBox.TabIndex = 2;
            this.notesTextBox.TabStop = false;
            // 
            // NumControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Name = "NumControl";
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
