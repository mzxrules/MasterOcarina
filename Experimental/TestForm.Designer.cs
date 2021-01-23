namespace Experimental
{
    partial class TestForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.experimentButton = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.outRichTextBox = new System.Windows.Forms.RichTextBox();
            this.inputTextBox = new System.Windows.Forms.TextBox();
            this.inputButton = new System.Windows.Forms.Button();
            this.testComboBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // experimentButton
            // 
            this.experimentButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.experimentButton.Location = new System.Drawing.Point(12, 339);
            this.experimentButton.Name = "experimentButton";
            this.experimentButton.Size = new System.Drawing.Size(75, 23);
            this.experimentButton.TabIndex = 0;
            this.experimentButton.Text = "Experiment";
            this.experimentButton.UseVisualStyleBackColor = true;
            this.experimentButton.Click += new System.EventHandler(this.experimentButton_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "All files|*.*";
            // 
            // outRichTextBox
            // 
            this.outRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outRichTextBox.Location = new System.Drawing.Point(262, 12);
            this.outRichTextBox.Name = "outRichTextBox";
            this.outRichTextBox.Size = new System.Drawing.Size(569, 321);
            this.outRichTextBox.TabIndex = 1;
            this.outRichTextBox.Text = "";
            // 
            // inputTextBox
            // 
            this.inputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.inputTextBox.Location = new System.Drawing.Point(650, 342);
            this.inputTextBox.Name = "inputTextBox";
            this.inputTextBox.Size = new System.Drawing.Size(100, 20);
            this.inputTextBox.TabIndex = 2;
            // 
            // inputButton
            // 
            this.inputButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.inputButton.Location = new System.Drawing.Point(756, 339);
            this.inputButton.Name = "inputButton";
            this.inputButton.Size = new System.Drawing.Size(75, 23);
            this.inputButton.TabIndex = 3;
            this.inputButton.Text = "button1";
            this.inputButton.UseVisualStyleBackColor = true;
            this.inputButton.Click += new System.EventHandler(this.inputButton_Click);
            // 
            // testComboBox
            // 
            this.testComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.Simple;
            this.testComboBox.FormattingEnabled = true;
            this.testComboBox.Location = new System.Drawing.Point(12, 12);
            this.testComboBox.Name = "testComboBox";
            this.testComboBox.Size = new System.Drawing.Size(225, 319);
            this.testComboBox.TabIndex = 4;
            // 
            // ExperimentalForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(843, 374);
            this.Controls.Add(this.testComboBox);
            this.Controls.Add(this.inputButton);
            this.Controls.Add(this.inputTextBox);
            this.Controls.Add(this.outRichTextBox);
            this.Controls.Add(this.experimentButton);
            this.Name = "ExperimentalForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.ExperimentalForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button experimentButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.RichTextBox outRichTextBox;
        private System.Windows.Forms.TextBox inputTextBox;
        private System.Windows.Forms.Button inputButton;
        private System.Windows.Forms.ComboBox testComboBox;
    }
}

