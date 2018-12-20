namespace mzxrules.XActor.Gui
{
    partial class ActorControl
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
            this.actorLabel = new System.Windows.Forms.Label();
            this.objectsLabel = new System.Windows.Forms.Label();
            this.flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // actorLabel
            // 
            this.actorLabel.AutoSize = true;
            this.actorLabel.Location = new System.Drawing.Point(3, 3);
            this.actorLabel.Margin = new System.Windows.Forms.Padding(3);
            this.actorLabel.Name = "actorLabel";
            this.actorLabel.Size = new System.Drawing.Size(32, 13);
            this.actorLabel.TabIndex = 0;
            this.actorLabel.Text = "Actor";
            // 
            // objectsLabel
            // 
            this.objectsLabel.AutoSize = true;
            this.objectsLabel.Location = new System.Drawing.Point(3, 22);
            this.objectsLabel.Margin = new System.Windows.Forms.Padding(3);
            this.objectsLabel.Name = "objectsLabel";
            this.objectsLabel.Size = new System.Drawing.Size(38, 13);
            this.objectsLabel.TabIndex = 1;
            this.objectsLabel.Text = "Object";
            // 
            // flowLayoutPanel
            // 
            this.flowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel.AutoScroll = true;
            this.flowLayoutPanel.BackColor = System.Drawing.SystemColors.Control;
            this.flowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel.Location = new System.Drawing.Point(0, 38);
            this.flowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel.MinimumSize = new System.Drawing.Size(350, 0);
            this.flowLayoutPanel.Name = "flowLayoutPanel";
            this.flowLayoutPanel.Size = new System.Drawing.Size(390, 229);
            this.flowLayoutPanel.TabIndex = 3;
            this.flowLayoutPanel.WrapContents = false;
            // 
            // ActorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.objectsLabel);
            this.Controls.Add(this.actorLabel);
            this.Controls.Add(this.flowLayoutPanel);
            this.MinimumSize = new System.Drawing.Size(350, 0);
            this.Name = "ActorControl";
            this.Size = new System.Drawing.Size(390, 267);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label actorLabel;
        private System.Windows.Forms.Label objectsLabel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
    }
}
