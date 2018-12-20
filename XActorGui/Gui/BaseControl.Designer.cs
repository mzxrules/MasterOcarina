namespace mzxrules.XActor.Gui
{
    partial class BaseControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.descriptionLabel = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.inputTextBox = new System.Windows.Forms.TextBox();
            this.nullCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // errorProvider
            // 
            this.errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errorProvider.ContainerControl = this;
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.AutoSize = true;
            this.descriptionLabel.Location = new System.Drawing.Point(3, 3);
            this.descriptionLabel.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(60, 13);
            this.descriptionLabel.TabIndex = 0;
            this.descriptionLabel.Text = "Description";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.inputTextBox);
            this.panel1.Controls.Add(this.nullCheckBox);
            this.panel1.Location = new System.Drawing.Point(0, 16);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(390, 21);
            this.panel1.TabIndex = 1;
            // 
            // inputTextBox
            // 
            this.inputTextBox.Location = new System.Drawing.Point(21, 0);
            this.inputTextBox.MaxLength = 8;
            this.inputTextBox.Name = "inputTextBox";
            this.inputTextBox.Size = new System.Drawing.Size(52, 20);
            this.inputTextBox.TabIndex = 1;
            this.inputTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.inputTextBox_Base_Validating);
            this.inputTextBox.Validated += new System.EventHandler(this.inputTextBox_Base_Validated);
            // 
            // nullCheckBox
            // 
            this.nullCheckBox.AutoSize = true;
            this.nullCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.nullCheckBox.Location = new System.Drawing.Point(3, 4);
            this.nullCheckBox.Name = "nullCheckBox";
            this.nullCheckBox.Size = new System.Drawing.Size(12, 11);
            this.nullCheckBox.TabIndex = 0;
            this.nullCheckBox.UseVisualStyleBackColor = true;
            this.nullCheckBox.CheckedChanged += new System.EventHandler(this.nullCheckBox_Base_CheckedChanged);
            // 
            // BaseControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
            this.Controls.Add(this.descriptionLabel);
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumSize = new System.Drawing.Size(350, 37);
            this.Name = "BaseControl";
            this.Size = new System.Drawing.Size(390, 37);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.ErrorProvider errorProvider;
        public System.Windows.Forms.TextBox inputTextBox;
        public System.Windows.Forms.Label descriptionLabel;
        public System.Windows.Forms.Panel panel1;
        public System.Windows.Forms.CheckBox nullCheckBox;




    }
}
