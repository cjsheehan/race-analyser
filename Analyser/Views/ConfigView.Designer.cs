namespace Betabelter
{
    partial class ConfigView
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
            this.buttonWorkDir = new System.Windows.Forms.Button();
            this.textBoxWorkDir = new System.Windows.Forms.TextBox();
            this.labelWorkDir = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonWorkDir
            // 
            this.buttonWorkDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonWorkDir.Location = new System.Drawing.Point(252, 33);
            this.buttonWorkDir.Name = "buttonWorkDir";
            this.buttonWorkDir.Size = new System.Drawing.Size(66, 23);
            this.buttonWorkDir.TabIndex = 0;
            this.buttonWorkDir.Text = "Browse...";
            this.buttonWorkDir.UseVisualStyleBackColor = true;
            this.buttonWorkDir.Click += new System.EventHandler(this.buttonWorkDir_Click);
            // 
            // textBoxWorkDir
            // 
            this.textBoxWorkDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxWorkDir.Location = new System.Drawing.Point(12, 36);
            this.textBoxWorkDir.Name = "textBoxWorkDir";
            this.textBoxWorkDir.Size = new System.Drawing.Size(223, 20);
            this.textBoxWorkDir.TabIndex = 1;
            // 
            // labelWorkDir
            // 
            this.labelWorkDir.AutoSize = true;
            this.labelWorkDir.Location = new System.Drawing.Point(12, 20);
            this.labelWorkDir.Name = "labelWorkDir";
            this.labelWorkDir.Size = new System.Drawing.Size(101, 13);
            this.labelWorkDir.TabIndex = 2;
            this.labelWorkDir.Text = "Working Directory : ";
            // 
            // ConfigView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(330, 89);
            this.Controls.Add(this.labelWorkDir);
            this.Controls.Add(this.textBoxWorkDir);
            this.Controls.Add(this.buttonWorkDir);
            this.Name = "ConfigView";
            this.Text = "Config";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonWorkDir;
        private System.Windows.Forms.TextBox textBoxWorkDir;
        private System.Windows.Forms.Label labelWorkDir;
    }
}