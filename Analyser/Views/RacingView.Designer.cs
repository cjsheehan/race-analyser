namespace Betabelter
{
    partial class RacingForm
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
            this.buttonGetMeetings = new System.Windows.Forms.Button();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.listBoxMeetings = new System.Windows.Forms.ListBox();
            this.dataGridViewRaceCards = new System.Windows.Forms.DataGridView();
            this.buttonGetRaceData = new System.Windows.Forms.Button();
            this.comboBoxDefaultRaceType = new System.Windows.Forms.ComboBox();
            this.groupBoxMeetings = new System.Windows.Forms.GroupBox();
            this.groupBoxCards = new System.Windows.Forms.GroupBox();
            this.checkBoxSelectAll = new System.Windows.Forms.CheckBox();
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.buttonAnalyse = new System.Windows.Forms.Button();
            this.groupBoxNtf = new System.Windows.Forms.GroupBox();
            this.buttonOpenWorkDir = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.labelProgress = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewRaceCards)).BeginInit();
            this.groupBoxMeetings.SuspendLayout();
            this.groupBoxCards.SuspendLayout();
            this.groupBoxNtf.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonGetMeetings
            // 
            this.buttonGetMeetings.Location = new System.Drawing.Point(142, 19);
            this.buttonGetMeetings.Name = "buttonGetMeetings";
            this.buttonGetMeetings.Size = new System.Drawing.Size(68, 35);
            this.buttonGetMeetings.TabIndex = 1;
            this.buttonGetMeetings.Text = "Get Meetings";
            this.buttonGetMeetings.UseVisualStyleBackColor = true;
            this.buttonGetMeetings.Click += new System.EventHandler(this.buttonGetMeetings_Click);
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Location = new System.Drawing.Point(6, 24);
            this.dateTimePicker1.MinDate = new System.DateTime(2014, 9, 16, 0, 0, 0, 0);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(115, 20);
            this.dateTimePicker1.TabIndex = 2;
            this.dateTimePicker1.ValueChanged += new System.EventHandler(this.dateTimePicker1_ValueChanged);
            // 
            // listBoxMeetings
            // 
            this.listBoxMeetings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxMeetings.FormattingEnabled = true;
            this.listBoxMeetings.Location = new System.Drawing.Point(6, 78);
            this.listBoxMeetings.Name = "listBoxMeetings";
            this.listBoxMeetings.Size = new System.Drawing.Size(204, 290);
            this.listBoxMeetings.TabIndex = 4;
            this.listBoxMeetings.SelectedIndexChanged += new System.EventHandler(this.listBoxMeetings_SelectedIndexChanged);
            // 
            // dataGridViewRaceCards
            // 
            this.dataGridViewRaceCards.AllowUserToAddRows = false;
            this.dataGridViewRaceCards.AllowUserToDeleteRows = false;
            this.dataGridViewRaceCards.AllowUserToResizeColumns = false;
            this.dataGridViewRaceCards.AllowUserToResizeRows = false;
            this.dataGridViewRaceCards.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.dataGridViewRaceCards.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridViewRaceCards.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridViewRaceCards.Location = new System.Drawing.Point(1, 69);
            this.dataGridViewRaceCards.Name = "dataGridViewRaceCards";
            this.dataGridViewRaceCards.Size = new System.Drawing.Size(847, 301);
            this.dataGridViewRaceCards.TabIndex = 5;
            // 
            // buttonGetRaceData
            // 
            this.buttonGetRaceData.Location = new System.Drawing.Point(780, 16);
            this.buttonGetRaceData.Name = "buttonGetRaceData";
            this.buttonGetRaceData.Size = new System.Drawing.Size(68, 35);
            this.buttonGetRaceData.TabIndex = 7;
            this.buttonGetRaceData.Text = "Get Race Data";
            this.buttonGetRaceData.UseVisualStyleBackColor = true;
            this.buttonGetRaceData.Click += new System.EventHandler(this.buttonGetRaceData_Click);
            // 
            // comboBoxDefaultRaceType
            // 
            this.comboBoxDefaultRaceType.FormattingEnabled = true;
            this.comboBoxDefaultRaceType.Location = new System.Drawing.Point(158, 24);
            this.comboBoxDefaultRaceType.Name = "comboBoxDefaultRaceType";
            this.comboBoxDefaultRaceType.Size = new System.Drawing.Size(97, 21);
            this.comboBoxDefaultRaceType.TabIndex = 8;
            this.comboBoxDefaultRaceType.SelectedIndexChanged += new System.EventHandler(this.comboBoxDefaultRaceType_SelectedIndexChanged);
            // 
            // groupBoxMeetings
            // 
            this.groupBoxMeetings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxMeetings.Controls.Add(this.listBoxMeetings);
            this.groupBoxMeetings.Controls.Add(this.buttonGetMeetings);
            this.groupBoxMeetings.Controls.Add(this.dateTimePicker1);
            this.groupBoxMeetings.Location = new System.Drawing.Point(31, 44);
            this.groupBoxMeetings.Name = "groupBoxMeetings";
            this.groupBoxMeetings.Size = new System.Drawing.Size(216, 377);
            this.groupBoxMeetings.TabIndex = 11;
            this.groupBoxMeetings.TabStop = false;
            this.groupBoxMeetings.Text = "Meetings";
            // 
            // groupBoxCards
            // 
            this.groupBoxCards.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxCards.Controls.Add(this.checkBoxSelectAll);
            this.groupBoxCards.Controls.Add(this.comboBoxDefaultRaceType);
            this.groupBoxCards.Controls.Add(this.dataGridViewRaceCards);
            this.groupBoxCards.Controls.Add(this.buttonGetRaceData);
            this.groupBoxCards.Location = new System.Drawing.Point(253, 44);
            this.groupBoxCards.Name = "groupBoxCards";
            this.groupBoxCards.Size = new System.Drawing.Size(854, 377);
            this.groupBoxCards.TabIndex = 12;
            this.groupBoxCards.TabStop = false;
            this.groupBoxCards.Text = "Races";
            // 
            // checkBoxSelectAll
            // 
            this.checkBoxSelectAll.AutoSize = true;
            this.checkBoxSelectAll.Location = new System.Drawing.Point(69, 27);
            this.checkBoxSelectAll.Name = "checkBoxSelectAll";
            this.checkBoxSelectAll.Size = new System.Drawing.Size(70, 17);
            this.checkBoxSelectAll.TabIndex = 10;
            this.checkBoxSelectAll.Text = "Select All";
            this.checkBoxSelectAll.UseVisualStyleBackColor = true;
            this.checkBoxSelectAll.CheckedChanged += new System.EventHandler(this.checkBoxSelectAll_CheckedChanged);
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOutput.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxOutput.Location = new System.Drawing.Point(6, 69);
            this.textBoxOutput.Multiline = true;
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxOutput.Size = new System.Drawing.Size(341, 301);
            this.textBoxOutput.TabIndex = 6;
            this.textBoxOutput.WordWrap = false;
            // 
            // buttonAnalyse
            // 
            this.buttonAnalyse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAnalyse.Location = new System.Drawing.Point(279, 17);
            this.buttonAnalyse.Name = "buttonAnalyse";
            this.buttonAnalyse.Size = new System.Drawing.Size(68, 35);
            this.buttonAnalyse.TabIndex = 11;
            this.buttonAnalyse.Text = "Analyse Races";
            this.buttonAnalyse.UseVisualStyleBackColor = true;
            this.buttonAnalyse.Click += new System.EventHandler(this.buttonAnalyse_Click);
            // 
            // groupBoxNtf
            // 
            this.groupBoxNtf.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxNtf.Controls.Add(this.buttonOpenWorkDir);
            this.groupBoxNtf.Controls.Add(this.buttonAnalyse);
            this.groupBoxNtf.Controls.Add(this.textBoxOutput);
            this.groupBoxNtf.Location = new System.Drawing.Point(1113, 44);
            this.groupBoxNtf.Name = "groupBoxNtf";
            this.groupBoxNtf.Size = new System.Drawing.Size(353, 377);
            this.groupBoxNtf.TabIndex = 13;
            this.groupBoxNtf.TabStop = false;
            this.groupBoxNtf.Text = "Output";
            // 
            // buttonOpenWorkDir
            // 
            this.buttonOpenWorkDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOpenWorkDir.Location = new System.Drawing.Point(6, 17);
            this.buttonOpenWorkDir.Name = "buttonOpenWorkDir";
            this.buttonOpenWorkDir.Size = new System.Drawing.Size(68, 35);
            this.buttonOpenWorkDir.TabIndex = 12;
            this.buttonOpenWorkDir.Text = "Open";
            this.buttonOpenWorkDir.UseVisualStyleBackColor = true;
            this.buttonOpenWorkDir.Click += new System.EventHandler(this.buttonOpenWorkDir_Click);
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(31, 441);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(1435, 23);
            this.progressBar.TabIndex = 14;
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.Highlight;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1478, 24);
            this.menuStrip1.TabIndex = 15;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.optionsToolStripMenuItem.Text = "Options...";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
            // 
            // labelProgress
            // 
            this.labelProgress.AutoSize = true;
            this.labelProgress.Location = new System.Drawing.Point(34, 424);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(48, 13);
            this.labelProgress.TabIndex = 16;
            this.labelProgress.Text = "Progress";
            // 
            // RacingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1478, 476);
            this.Controls.Add(this.labelProgress);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.groupBoxNtf);
            this.Controls.Add(this.groupBoxCards);
            this.Controls.Add(this.groupBoxMeetings);
            this.Controls.Add(this.menuStrip1);
            this.MinimumSize = new System.Drawing.Size(1494, 485);
            this.Name = "RacingForm";
            this.Text = "Betabelter";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewRaceCards)).EndInit();
            this.groupBoxMeetings.ResumeLayout(false);
            this.groupBoxCards.ResumeLayout(false);
            this.groupBoxCards.PerformLayout();
            this.groupBoxNtf.ResumeLayout(false);
            this.groupBoxNtf.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonGetMeetings;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.ListBox listBoxMeetings;
        private System.Windows.Forms.DataGridView dataGridViewRaceCards;
        private System.Windows.Forms.Button buttonGetRaceData;
        private System.Windows.Forms.ComboBox comboBoxDefaultRaceType;
        private System.Windows.Forms.GroupBox groupBoxMeetings;
        private System.Windows.Forms.GroupBox groupBoxCards;
        private System.Windows.Forms.CheckBox checkBoxSelectAll;
        private System.Windows.Forms.TextBox textBoxOutput;
        private System.Windows.Forms.Button buttonAnalyse;
        private System.Windows.Forms.GroupBox groupBoxNtf;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.Button buttonOpenWorkDir;
        private System.Windows.Forms.Label labelProgress;
    }
}

