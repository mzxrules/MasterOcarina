namespace Z64Compresser
{
    partial class RomCompresserForm
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
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.modButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.mRomLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.comButton = new System.Windows.Forms.Button();
            this.cLabel = new System.Windows.Forms.Label();
            this.modifiedOpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.CompressedOpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.compressButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(12, 12);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 21);
            this.comboBox1.TabIndex = 0;
            // 
            // modButton
            // 
            this.modButton.Location = new System.Drawing.Point(12, 64);
            this.modButton.Name = "modButton";
            this.modButton.Size = new System.Drawing.Size(75, 23);
            this.modButton.TabIndex = 1;
            this.modButton.Text = "Get File";
            this.modButton.UseVisualStyleBackColor = true;
            this.modButton.Click += new System.EventHandler(this.modButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Modified Rom:";
            // 
            // mRomLabel
            // 
            this.mRomLabel.AutoSize = true;
            this.mRomLabel.Location = new System.Drawing.Point(93, 48);
            this.mRomLabel.Name = "mRomLabel";
            this.mRomLabel.Size = new System.Drawing.Size(25, 13);
            this.mRomLabel.TabIndex = 3;
            this.mRomLabel.Text = "___";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 100);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(93, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Compressed Rom:";
            // 
            // comButton
            // 
            this.comButton.Location = new System.Drawing.Point(12, 116);
            this.comButton.Name = "comButton";
            this.comButton.Size = new System.Drawing.Size(75, 23);
            this.comButton.TabIndex = 5;
            this.comButton.Text = "Get File";
            this.comButton.UseVisualStyleBackColor = true;
            this.comButton.Click += new System.EventHandler(this.comButton_Click);
            // 
            // cLabel
            // 
            this.cLabel.AutoSize = true;
            this.cLabel.Location = new System.Drawing.Point(111, 100);
            this.cLabel.Name = "cLabel";
            this.cLabel.Size = new System.Drawing.Size(25, 13);
            this.cLabel.TabIndex = 6;
            this.cLabel.Text = "___";
            // 
            // modifiedOpenFileDialog
            // 
            this.modifiedOpenFileDialog.FileName = "openFileDialog1";
            this.modifiedOpenFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.modifiedOpenFileDialog_FileOk);
            // 
            // CompressedOpenFileDialog
            // 
            this.CompressedOpenFileDialog.FileName = "openFileDialog2";
            this.CompressedOpenFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.CompressedOpenFileDialog_FileOk);
            // 
            // compressButton
            // 
            this.compressButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.compressButton.Location = new System.Drawing.Point(534, 119);
            this.compressButton.Name = "compressButton";
            this.compressButton.Size = new System.Drawing.Size(75, 23);
            this.compressButton.TabIndex = 7;
            this.compressButton.Text = "Compress";
            this.compressButton.UseVisualStyleBackColor = true;
            this.compressButton.Click += new System.EventHandler(this.compressButton_Click);
            // 
            // RomCompresserForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(621, 154);
            this.Controls.Add(this.compressButton);
            this.Controls.Add(this.cLabel);
            this.Controls.Add(this.comButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.mRomLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.modButton);
            this.Controls.Add(this.comboBox1);
            this.Name = "RomCompresserForm";
            this.Text = "Rom Recompresser";
            this.Load += new System.EventHandler(this.RomCompresserForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button modButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label mRomLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button comButton;
        private System.Windows.Forms.Label cLabel;
        private System.Windows.Forms.OpenFileDialog modifiedOpenFileDialog;
        private System.Windows.Forms.OpenFileDialog CompressedOpenFileDialog;
        private System.Windows.Forms.Button compressButton;
    }
}

