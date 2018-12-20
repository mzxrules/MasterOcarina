namespace mzxrules.XActor
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
            this.dataInRichTextBox = new System.Windows.Forms.RichTextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.outRichTextBox = new System.Windows.Forms.RichTextBox();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.wikiButton = new System.Windows.Forms.Button();
            this.uiTestButton = new System.Windows.Forms.Button();
            this.actorTextBox = new System.Windows.Forms.TextBox();
            this.actorControl = new mzxrules.XActor.Gui.ActorControl();
            this.loadOcaButton = new System.Windows.Forms.Button();
            this.loadMMButton = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataInRichTextBox
            // 
            this.dataInRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataInRichTextBox.Location = new System.Drawing.Point(4, 4);
            this.dataInRichTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.dataInRichTextBox.Name = "dataInRichTextBox";
            this.dataInRichTextBox.Size = new System.Drawing.Size(545, 409);
            this.dataInRichTextBox.TabIndex = 1;
            this.dataInRichTextBox.Text = "";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(1066, 478);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 28);
            this.button1.TabIndex = 2;
            this.button1.Text = "Serialize";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.serializeButton_click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Location = new System.Drawing.Point(605, 15);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(561, 446);
            this.tabControl1.TabIndex = 3;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.outRichTextBox);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage2.Size = new System.Drawing.Size(553, 417);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Edit";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // outRichTextBox
            // 
            this.outRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outRichTextBox.Location = new System.Drawing.Point(4, 4);
            this.outRichTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.outRichTextBox.Name = "outRichTextBox";
            this.outRichTextBox.Size = new System.Drawing.Size(545, 409);
            this.outRichTextBox.TabIndex = 0;
            this.outRichTextBox.Text = "";
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dataInRichTextBox);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage1.Size = new System.Drawing.Size(553, 417);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Data";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // wikiButton
            // 
            this.wikiButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.wikiButton.Location = new System.Drawing.Point(958, 478);
            this.wikiButton.Margin = new System.Windows.Forms.Padding(4);
            this.wikiButton.Name = "wikiButton";
            this.wikiButton.Size = new System.Drawing.Size(100, 28);
            this.wikiButton.TabIndex = 4;
            this.wikiButton.Text = "Wiki";
            this.wikiButton.UseVisualStyleBackColor = true;
            this.wikiButton.Click += new System.EventHandler(this.wikiButton_Click);
            // 
            // uiTestButton
            // 
            this.uiTestButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.uiTestButton.Location = new System.Drawing.Point(156, 477);
            this.uiTestButton.Margin = new System.Windows.Forms.Padding(4);
            this.uiTestButton.Name = "uiTestButton";
            this.uiTestButton.Size = new System.Drawing.Size(100, 28);
            this.uiTestButton.TabIndex = 7;
            this.uiTestButton.Text = "Test UI";
            this.uiTestButton.UseVisualStyleBackColor = true;
            this.uiTestButton.Click += new System.EventHandler(this.uiTestButton_Click);
            // 
            // actorTextBox
            // 
            this.actorTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.actorTextBox.Location = new System.Drawing.Point(16, 480);
            this.actorTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.actorTextBox.Name = "actorTextBox";
            this.actorTextBox.Size = new System.Drawing.Size(132, 22);
            this.actorTextBox.TabIndex = 9;
            // 
            // actorControl
            // 
            this.actorControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.actorControl.Document = null;
            this.actorControl.Location = new System.Drawing.Point(16, 15);
            this.actorControl.Margin = new System.Windows.Forms.Padding(5);
            this.actorControl.MinimumSize = new System.Drawing.Size(467, 0);
            this.actorControl.Name = "actorControl";
            this.actorControl.Size = new System.Drawing.Size(581, 446);
            this.actorControl.TabIndex = 8;
            // 
            // loadOcaButton
            // 
            this.loadOcaButton.Location = new System.Drawing.Point(745, 478);
            this.loadOcaButton.Name = "loadOcaButton";
            this.loadOcaButton.Size = new System.Drawing.Size(100, 28);
            this.loadOcaButton.TabIndex = 10;
            this.loadOcaButton.Text = "OoT";
            this.loadOcaButton.UseVisualStyleBackColor = true;
            this.loadOcaButton.Click += new System.EventHandler(this.loadOcaButton_Click);
            // 
            // loadMMButton
            // 
            this.loadMMButton.Location = new System.Drawing.Point(851, 478);
            this.loadMMButton.Name = "loadMMButton";
            this.loadMMButton.Size = new System.Drawing.Size(100, 28);
            this.loadMMButton.TabIndex = 11;
            this.loadMMButton.Text = "MM";
            this.loadMMButton.UseVisualStyleBackColor = true;
            this.loadMMButton.Click += new System.EventHandler(this.loadMMButton_Click);
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1179, 518);
            this.Controls.Add(this.loadMMButton);
            this.Controls.Add(this.loadOcaButton);
            this.Controls.Add(this.actorTextBox);
            this.Controls.Add(this.actorControl);
            this.Controls.Add(this.uiTestButton);
            this.Controls.Add(this.wikiButton);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.button1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "TestForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.TestForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox dataInRichTextBox;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.RichTextBox outRichTextBox;
        private System.Windows.Forms.Button wikiButton;
        private System.Windows.Forms.Button uiTestButton;
        private Gui.ActorControl actorControl;
        private System.Windows.Forms.TextBox actorTextBox;
        private System.Windows.Forms.Button loadOcaButton;
        private System.Windows.Forms.Button loadMMButton;
    }
}

