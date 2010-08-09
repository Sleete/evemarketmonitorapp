namespace EveMarketMonitorApp.GUIElements
{
    partial class UpdatePanelCompact
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdatePanelCompact));
            this.picPortrait = new System.Windows.Forms.PictureBox();
            this.lblCorpTag = new System.Windows.Forms.Label();
            this.errorToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.chkUpdate = new System.Windows.Forms.CheckBox();
            this.lblTransStatus = new System.Windows.Forms.Label();
            this.chkAutoIndustryJobs = new System.Windows.Forms.CheckBox();
            this.chkAutoAssets = new System.Windows.Forms.CheckBox();
            this.chkAutoOrders = new System.Windows.Forms.CheckBox();
            this.chkAutoTrans = new System.Windows.Forms.CheckBox();
            this.chkAutoJournal = new System.Windows.Forms.CheckBox();
            this.lblJournalStatus = new System.Windows.Forms.Label();
            this.lblAssetsStatus = new System.Windows.Forms.Label();
            this.lblOrdersStatus = new System.Windows.Forms.Label();
            this.lblIndustryJobsStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picPortrait)).BeginInit();
            this.SuspendLayout();
            // 
            // picPortrait
            // 
            this.picPortrait.Image = ((System.Drawing.Image)(resources.GetObject("picPortrait.Image")));
            this.picPortrait.InitialImage = null;
            this.picPortrait.Location = new System.Drawing.Point(3, 3);
            this.picPortrait.Name = "picPortrait";
            this.picPortrait.Size = new System.Drawing.Size(60, 60);
            this.picPortrait.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picPortrait.TabIndex = 0;
            this.picPortrait.TabStop = false;
            // 
            // lblCorpTag
            // 
            this.lblCorpTag.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCorpTag.Location = new System.Drawing.Point(4, 19);
            this.lblCorpTag.Name = "lblCorpTag";
            this.lblCorpTag.Size = new System.Drawing.Size(58, 23);
            this.lblCorpTag.TabIndex = 13;
            this.lblCorpTag.Text = "[EMMAT]";
            this.lblCorpTag.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblCorpTag.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(69, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "Update from API";
            // 
            // chkUpdate
            // 
            this.chkUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkUpdate.AutoSize = true;
            this.chkUpdate.Location = new System.Drawing.Point(209, 2);
            this.chkUpdate.Name = "chkUpdate";
            this.chkUpdate.Size = new System.Drawing.Size(15, 14);
            this.chkUpdate.TabIndex = 15;
            this.chkUpdate.UseVisualStyleBackColor = true;
            this.chkUpdate.CheckedChanged += new System.EventHandler(this.chkUpdate_CheckedChanged);
            // 
            // lblTransStatus
            // 
            this.lblTransStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.lblTransStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTransStatus.Location = new System.Drawing.Point(69, 30);
            this.lblTransStatus.Name = "lblTransStatus";
            this.lblTransStatus.Size = new System.Drawing.Size(26, 33);
            this.lblTransStatus.TabIndex = 16;
            this.lblTransStatus.Text = "T";
            this.lblTransStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // chkAutoIndustryJobs
            // 
            this.chkAutoIndustryJobs.AutoSize = true;
            this.chkAutoIndustryJobs.Location = new System.Drawing.Point(150, 21);
            this.chkAutoIndustryJobs.Name = "chkAutoIndustryJobs";
            this.chkAutoIndustryJobs.Size = new System.Drawing.Size(15, 14);
            this.chkAutoIndustryJobs.TabIndex = 17;
            this.chkAutoIndustryJobs.UseVisualStyleBackColor = true;
            this.chkAutoIndustryJobs.Visible = false;
            // 
            // chkAutoAssets
            // 
            this.chkAutoAssets.AutoSize = true;
            this.chkAutoAssets.Location = new System.Drawing.Point(129, 21);
            this.chkAutoAssets.Name = "chkAutoAssets";
            this.chkAutoAssets.Size = new System.Drawing.Size(15, 14);
            this.chkAutoAssets.TabIndex = 18;
            this.chkAutoAssets.UseVisualStyleBackColor = true;
            this.chkAutoAssets.Visible = false;
            // 
            // chkAutoOrders
            // 
            this.chkAutoOrders.AutoSize = true;
            this.chkAutoOrders.Location = new System.Drawing.Point(108, 21);
            this.chkAutoOrders.Name = "chkAutoOrders";
            this.chkAutoOrders.Size = new System.Drawing.Size(15, 14);
            this.chkAutoOrders.TabIndex = 19;
            this.chkAutoOrders.UseCompatibleTextRendering = true;
            this.chkAutoOrders.UseVisualStyleBackColor = true;
            this.chkAutoOrders.Visible = false;
            // 
            // chkAutoTrans
            // 
            this.chkAutoTrans.AutoSize = true;
            this.chkAutoTrans.Location = new System.Drawing.Point(87, 21);
            this.chkAutoTrans.Name = "chkAutoTrans";
            this.chkAutoTrans.Size = new System.Drawing.Size(15, 14);
            this.chkAutoTrans.TabIndex = 20;
            this.chkAutoTrans.UseVisualStyleBackColor = true;
            this.chkAutoTrans.Visible = false;
            // 
            // chkAutoJournal
            // 
            this.chkAutoJournal.AutoSize = true;
            this.chkAutoJournal.Location = new System.Drawing.Point(66, 21);
            this.chkAutoJournal.Name = "chkAutoJournal";
            this.chkAutoJournal.Size = new System.Drawing.Size(15, 14);
            this.chkAutoJournal.TabIndex = 21;
            this.chkAutoJournal.UseVisualStyleBackColor = true;
            this.chkAutoJournal.Visible = false;
            // 
            // lblJournalStatus
            // 
            this.lblJournalStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.lblJournalStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblJournalStatus.Location = new System.Drawing.Point(101, 30);
            this.lblJournalStatus.Name = "lblJournalStatus";
            this.lblJournalStatus.Size = new System.Drawing.Size(26, 33);
            this.lblJournalStatus.TabIndex = 22;
            this.lblJournalStatus.Text = "J";
            this.lblJournalStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAssetsStatus
            // 
            this.lblAssetsStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.lblAssetsStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAssetsStatus.Location = new System.Drawing.Point(133, 30);
            this.lblAssetsStatus.Name = "lblAssetsStatus";
            this.lblAssetsStatus.Size = new System.Drawing.Size(26, 33);
            this.lblAssetsStatus.TabIndex = 23;
            this.lblAssetsStatus.Text = "A";
            this.lblAssetsStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblOrdersStatus
            // 
            this.lblOrdersStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.lblOrdersStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOrdersStatus.Location = new System.Drawing.Point(165, 30);
            this.lblOrdersStatus.Name = "lblOrdersStatus";
            this.lblOrdersStatus.Size = new System.Drawing.Size(26, 33);
            this.lblOrdersStatus.TabIndex = 24;
            this.lblOrdersStatus.Text = "O";
            this.lblOrdersStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblIndustryJobsStatus
            // 
            this.lblIndustryJobsStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.lblIndustryJobsStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblIndustryJobsStatus.Location = new System.Drawing.Point(197, 30);
            this.lblIndustryJobsStatus.Name = "lblIndustryJobsStatus";
            this.lblIndustryJobsStatus.Size = new System.Drawing.Size(26, 33);
            this.lblIndustryJobsStatus.TabIndex = 25;
            this.lblIndustryJobsStatus.Text = "I";
            this.lblIndustryJobsStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // UpdatePanelCompact
            // 
            this.Controls.Add(this.lblIndustryJobsStatus);
            this.Controls.Add(this.lblOrdersStatus);
            this.Controls.Add(this.lblAssetsStatus);
            this.Controls.Add(this.lblJournalStatus);
            this.Controls.Add(this.chkAutoJournal);
            this.Controls.Add(this.chkAutoTrans);
            this.Controls.Add(this.chkAutoOrders);
            this.Controls.Add(this.chkAutoAssets);
            this.Controls.Add(this.chkAutoIndustryJobs);
            this.Controls.Add(this.lblTransStatus);
            this.Controls.Add(this.chkUpdate);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblCorpTag);
            this.Controls.Add(this.picPortrait);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "UpdatePanelCompact";
            this.Size = new System.Drawing.Size(227, 66);
            ((System.ComponentModel.ISupportInitialize)(this.picPortrait)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picPortrait;
        private System.Windows.Forms.Label lblCorpTag;
        private System.Windows.Forms.ToolTip errorToolTip;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkUpdate;
        private System.Windows.Forms.Label lblTransStatus;
        private System.Windows.Forms.CheckBox chkAutoIndustryJobs;
        private System.Windows.Forms.CheckBox chkAutoAssets;
        private System.Windows.Forms.CheckBox chkAutoOrders;
        private System.Windows.Forms.CheckBox chkAutoTrans;
        private System.Windows.Forms.CheckBox chkAutoJournal;
        private System.Windows.Forms.Label lblJournalStatus;
        private System.Windows.Forms.Label lblAssetsStatus;
        private System.Windows.Forms.Label lblOrdersStatus;
        private System.Windows.Forms.Label lblIndustryJobsStatus;
    }
}
