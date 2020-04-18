namespace KtaneManualDownloader
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            this.downloadBtn = new System.Windows.Forms.Button();
            this.mergeCheck = new System.Windows.Forms.CheckBox();
            this.modMergeRadio = new System.Windows.Forms.RadioButton();
            this.moduleMergeRadio = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.diffMergeRadio = new System.Windows.Forms.RadioButton();
            this.moduleGroupCheck = new System.Windows.Forms.CheckBox();
            this.reverseOrderCheck = new System.Windows.Forms.CheckBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.modsFolderBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.selectModsDirBtn = new System.Windows.Forms.Button();
            this.modListPanel = new System.Windows.Forms.Panel();
            this.vanillaMergeCheck = new System.Windows.Forms.CheckBox();
            this.mergedPDFPathBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.mergedPDFPathBtn = new System.Windows.Forms.Button();
            this.manualDownloadsBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.manualDownloadsBtn = new System.Windows.Forms.Button();
            this.vanillaMergeTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // downloadBtn
            // 
            this.downloadBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.downloadBtn.Location = new System.Drawing.Point(333, 387);
            this.downloadBtn.Name = "downloadBtn";
            this.downloadBtn.Size = new System.Drawing.Size(75, 22);
            this.downloadBtn.TabIndex = 0;
            this.downloadBtn.Text = "Download";
            this.downloadBtn.UseVisualStyleBackColor = true;
            this.downloadBtn.Click += new System.EventHandler(this.downloadBtn_Click);
            // 
            // mergeCheck
            // 
            this.mergeCheck.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mergeCheck.AutoSize = true;
            this.mergeCheck.Checked = true;
            this.mergeCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mergeCheck.Location = new System.Drawing.Point(414, 391);
            this.mergeCheck.Name = "mergeCheck";
            this.mergeCheck.Size = new System.Drawing.Size(85, 17);
            this.mergeCheck.TabIndex = 1;
            this.mergeCheck.Text = "Merge PDFs";
            this.mergeCheck.UseVisualStyleBackColor = true;
            this.mergeCheck.CheckedChanged += new System.EventHandler(this.mergeCheck_CheckedChanged);
            // 
            // modMergeRadio
            // 
            this.modMergeRadio.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.modMergeRadio.AutoSize = true;
            this.modMergeRadio.Location = new System.Drawing.Point(541, 353);
            this.modMergeRadio.Name = "modMergeRadio";
            this.modMergeRadio.Size = new System.Drawing.Size(110, 17);
            this.modMergeRadio.TabIndex = 2;
            this.modMergeRadio.Text = "Sort by mod name";
            this.modMergeRadio.UseVisualStyleBackColor = true;
            // 
            // moduleMergeRadio
            // 
            this.moduleMergeRadio.AutoSize = true;
            this.moduleMergeRadio.Checked = true;
            this.moduleMergeRadio.Location = new System.Drawing.Point(668, 353);
            this.moduleMergeRadio.Name = "moduleMergeRadio";
            this.moduleMergeRadio.Size = new System.Drawing.Size(124, 17);
            this.moduleMergeRadio.TabIndex = 3;
            this.moduleMergeRadio.TabStop = true;
            this.moduleMergeRadio.Text = "Sort by module name";
            this.moduleMergeRadio.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(624, 334);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Alphabetical:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(637, 373);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Other:";
            // 
            // diffMergeRadio
            // 
            this.diffMergeRadio.AutoSize = true;
            this.diffMergeRadio.Location = new System.Drawing.Point(541, 389);
            this.diffMergeRadio.Name = "diffMergeRadio";
            this.diffMergeRadio.Size = new System.Drawing.Size(99, 17);
            this.diffMergeRadio.TabIndex = 6;
            this.diffMergeRadio.Text = "Sort by difficulty";
            this.diffMergeRadio.UseVisualStyleBackColor = true;
            // 
            // moduleGroupCheck
            // 
            this.moduleGroupCheck.AutoSize = true;
            this.moduleGroupCheck.Location = new System.Drawing.Point(668, 390);
            this.moduleGroupCheck.Name = "moduleGroupCheck";
            this.moduleGroupCheck.Size = new System.Drawing.Size(129, 17);
            this.moduleGroupCheck.TabIndex = 7;
            this.moduleGroupCheck.Text = "Group by module type";
            this.moduleGroupCheck.UseVisualStyleBackColor = true;
            // 
            // reverseOrderCheck
            // 
            this.reverseOrderCheck.AutoSize = true;
            this.reverseOrderCheck.Location = new System.Drawing.Point(414, 374);
            this.reverseOrderCheck.Name = "reverseOrderCheck";
            this.reverseOrderCheck.Size = new System.Drawing.Size(93, 17);
            this.reverseOrderCheck.TabIndex = 8;
            this.reverseOrderCheck.Text = "Reverse order";
            this.reverseOrderCheck.UseVisualStyleBackColor = true;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 415);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(776, 23);
            this.progressBar.TabIndex = 9;
            // 
            // modsFolderBox
            // 
            this.modsFolderBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.modsFolderBox.Location = new System.Drawing.Point(118, 388);
            this.modsFolderBox.Name = "modsFolderBox";
            this.modsFolderBox.Size = new System.Drawing.Size(148, 20);
            this.modsFolderBox.TabIndex = 11;
            this.modsFolderBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.modsFolderBox_KeyPress);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 391);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(103, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "KTANE mods folder:";
            // 
            // selectModsDirBtn
            // 
            this.selectModsDirBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.selectModsDirBtn.Location = new System.Drawing.Point(266, 387);
            this.selectModsDirBtn.Name = "selectModsDirBtn";
            this.selectModsDirBtn.Size = new System.Drawing.Size(25, 22);
            this.selectModsDirBtn.TabIndex = 13;
            this.selectModsDirBtn.Text = "...";
            this.selectModsDirBtn.UseVisualStyleBackColor = true;
            this.selectModsDirBtn.Click += new System.EventHandler(this.selectModsDirBtn_Click);
            // 
            // modListPanel
            // 
            this.modListPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.modListPanel.AutoScroll = true;
            this.modListPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.modListPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.modListPanel.Location = new System.Drawing.Point(12, 12);
            this.modListPanel.Name = "modListPanel";
            this.modListPanel.Size = new System.Drawing.Size(782, 292);
            this.modListPanel.TabIndex = 14;
            // 
            // vanillaMergeCheck
            // 
            this.vanillaMergeCheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.vanillaMergeCheck.AutoSize = true;
            this.vanillaMergeCheck.Location = new System.Drawing.Point(414, 357);
            this.vanillaMergeCheck.Name = "vanillaMergeCheck";
            this.vanillaMergeCheck.Size = new System.Drawing.Size(90, 17);
            this.vanillaMergeCheck.TabIndex = 15;
            this.vanillaMergeCheck.Text = "Merge Vanilla";
            this.vanillaMergeTooltip.SetToolTip(this.vanillaMergeCheck, "If this is checked, the manual cover,\r\nintro pages, and vanilla appendixes\r\nwill " +
        "be merged into the final manual.");
            this.vanillaMergeCheck.UseVisualStyleBackColor = true;
            // 
            // mergedPDFPathBox
            // 
            this.mergedPDFPathBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.mergedPDFPathBox.Location = new System.Drawing.Point(118, 344);
            this.mergedPDFPathBox.Name = "mergedPDFPathBox";
            this.mergedPDFPathBox.Size = new System.Drawing.Size(148, 20);
            this.mergedPDFPathBox.TabIndex = 16;
            this.mergedPDFPathBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.mergedPDFPathBox_KeyPress);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 351);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(94, 13);
            this.label4.TabIndex = 17;
            this.label4.Text = "Merged PDF path:";
            // 
            // mergedPDFPathBtn
            // 
            this.mergedPDFPathBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.mergedPDFPathBtn.Location = new System.Drawing.Point(266, 343);
            this.mergedPDFPathBtn.Name = "mergedPDFPathBtn";
            this.mergedPDFPathBtn.Size = new System.Drawing.Size(25, 22);
            this.mergedPDFPathBtn.TabIndex = 18;
            this.mergedPDFPathBtn.Text = "...";
            this.mergedPDFPathBtn.UseVisualStyleBackColor = true;
            this.mergedPDFPathBtn.Click += new System.EventHandler(this.mergedPDFPathBtn_Click);
            // 
            // manualDownloadsBox
            // 
            this.manualDownloadsBox.Location = new System.Drawing.Point(118, 366);
            this.manualDownloadsBox.Name = "manualDownloadsBox";
            this.manualDownloadsBox.Size = new System.Drawing.Size(148, 20);
            this.manualDownloadsBox.TabIndex = 19;
            this.manualDownloadsBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.manualDownloadsBox_KeyPress);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 371);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(96, 13);
            this.label5.TabIndex = 20;
            this.label5.Text = "Manual downloads";
            // 
            // manualDownloadsBtn
            // 
            this.manualDownloadsBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.manualDownloadsBtn.Location = new System.Drawing.Point(266, 365);
            this.manualDownloadsBtn.Name = "manualDownloadsBtn";
            this.manualDownloadsBtn.Size = new System.Drawing.Size(25, 22);
            this.manualDownloadsBtn.TabIndex = 21;
            this.manualDownloadsBtn.Text = "...";
            this.manualDownloadsBtn.UseVisualStyleBackColor = true;
            this.manualDownloadsBtn.Click += new System.EventHandler(this.manualDownloadsBtn_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.manualDownloadsBtn);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.manualDownloadsBox);
            this.Controls.Add(this.mergedPDFPathBtn);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.mergedPDFPathBox);
            this.Controls.Add(this.vanillaMergeCheck);
            this.Controls.Add(this.modListPanel);
            this.Controls.Add(this.selectModsDirBtn);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.modsFolderBox);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.reverseOrderCheck);
            this.Controls.Add(this.moduleGroupCheck);
            this.Controls.Add(this.diffMergeRadio);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.moduleMergeRadio);
            this.Controls.Add(this.modMergeRadio);
            this.Controls.Add(this.mergeCheck);
            this.Controls.Add(this.downloadBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "KTANE Manual Downloader";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button downloadBtn;
        private System.Windows.Forms.CheckBox mergeCheck;
        private System.Windows.Forms.RadioButton modMergeRadio;
        private System.Windows.Forms.RadioButton moduleMergeRadio;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton diffMergeRadio;
        private System.Windows.Forms.CheckBox moduleGroupCheck;
        private System.Windows.Forms.CheckBox reverseOrderCheck;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TextBox modsFolderBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button selectModsDirBtn;
        private System.Windows.Forms.Panel modListPanel;
        private System.Windows.Forms.CheckBox vanillaMergeCheck;
        private System.Windows.Forms.TextBox mergedPDFPathBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button mergedPDFPathBtn;
        private System.Windows.Forms.TextBox manualDownloadsBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button manualDownloadsBtn;
        private System.Windows.Forms.ToolTip vanillaMergeTooltip;
    }
}

