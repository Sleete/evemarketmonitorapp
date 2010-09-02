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
            this.lblName = new System.Windows.Forms.Label();
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.picPortrait)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // picPortrait
            // 
            this.picPortrait.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picPortrait.Image = ((System.Drawing.Image)(resources.GetObject("picPortrait.Image")));
            this.picPortrait.InitialImage = null;
            this.picPortrait.Location = new System.Drawing.Point(0, 0);
            this.picPortrait.Name = "picPortrait";
            this.picPortrait.Size = new System.Drawing.Size(60, 60);
            this.picPortrait.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picPortrait.TabIndex = 0;
            this.picPortrait.TabStop = false;
            // 
            // lblCorpTag
            // 
            this.lblCorpTag.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCorpTag.Location = new System.Drawing.Point(1, 19);
            this.lblCorpTag.Name = "lblCorpTag";
            this.lblCorpTag.Size = new System.Drawing.Size(58, 23);
            this.lblCorpTag.TabIndex = 13;
            this.lblCorpTag.Text = "[EMMAT]";
            this.lblCorpTag.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblCorpTag.Visible = false;
            // 
            // lblName
            // 
            this.lblName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblName.Location = new System.Drawing.Point(3, 3);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(108, 14);
            this.lblName.TabIndex = 14;
            this.lblName.Text = "Update from API";
            // 
            // chkUpdate
            // 
            this.chkUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkUpdate.AutoSize = true;
            this.chkUpdate.Location = new System.Drawing.Point(117, 3);
            this.chkUpdate.Name = "chkUpdate";
            this.chkUpdate.Size = new System.Drawing.Size(15, 14);
            this.chkUpdate.TabIndex = 15;
            this.chkUpdate.UseVisualStyleBackColor = true;
            this.chkUpdate.CheckedChanged += new System.EventHandler(this.chkUpdate_CheckedChanged);
            // 
            // lblTransStatus
            // 
            this.lblTransStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.lblTransStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTransStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTransStatus.Location = new System.Drawing.Point(69, 26);
            this.lblTransStatus.Name = "lblTransStatus";
            this.lblTransStatus.Size = new System.Drawing.Size(22, 34);
            this.lblTransStatus.TabIndex = 16;
            this.lblTransStatus.Text = "T";
            this.lblTransStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // chkAutoIndustryJobs
            // 
            this.chkAutoIndustryJobs.AutoSize = true;
            this.chkAutoIndustryJobs.Location = new System.Drawing.Point(14, 0);
            this.chkAutoIndustryJobs.Name = "chkAutoIndustryJobs";
            this.chkAutoIndustryJobs.Size = new System.Drawing.Size(15, 14);
            this.chkAutoIndustryJobs.TabIndex = 17;
            this.chkAutoIndustryJobs.UseVisualStyleBackColor = true;
            this.chkAutoIndustryJobs.Visible = false;
            // 
            // chkAutoAssets
            // 
            this.chkAutoAssets.AutoSize = true;
            this.chkAutoAssets.Location = new System.Drawing.Point(25, -1);
            this.chkAutoAssets.Name = "chkAutoAssets";
            this.chkAutoAssets.Size = new System.Drawing.Size(15, 14);
            this.chkAutoAssets.TabIndex = 18;
            this.chkAutoAssets.UseVisualStyleBackColor = true;
            this.chkAutoAssets.Visible = false;
            // 
            // chkAutoOrders
            // 
            this.chkAutoOrders.AutoSize = true;
            this.chkAutoOrders.Location = new System.Drawing.Point(35, -1);
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
            this.chkAutoTrans.Location = new System.Drawing.Point(67, -1);
            this.chkAutoTrans.Name = "chkAutoTrans";
            this.chkAutoTrans.Size = new System.Drawing.Size(15, 14);
            this.chkAutoTrans.TabIndex = 20;
            this.chkAutoTrans.UseVisualStyleBackColor = true;
            this.chkAutoTrans.Visible = false;
            // 
            // chkAutoJournal
            // 
            this.chkAutoJournal.AutoSize = true;
            this.chkAutoJournal.Location = new System.Drawing.Point(46, 0);
            this.chkAutoJournal.Name = "chkAutoJournal";
            this.chkAutoJournal.Size = new System.Drawing.Size(15, 14);
            this.chkAutoJournal.TabIndex = 21;
            this.chkAutoJournal.UseVisualStyleBackColor = true;
            this.chkAutoJournal.Visible = false;
            // 
            // lblJournalStatus
            // 
            this.lblJournalStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.lblJournalStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblJournalStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblJournalStatus.Location = new System.Drawing.Point(97, 26);
            this.lblJournalStatus.Name = "lblJournalStatus";
            this.lblJournalStatus.Size = new System.Drawing.Size(22, 34);
            this.lblJournalStatus.TabIndex = 22;
            this.lblJournalStatus.Text = "J";
            this.lblJournalStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAssetsStatus
            // 
            this.lblAssetsStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.lblAssetsStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAssetsStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAssetsStatus.Location = new System.Drawing.Point(125, 26);
            this.lblAssetsStatus.Name = "lblAssetsStatus";
            this.lblAssetsStatus.Size = new System.Drawing.Size(22, 34);
            this.lblAssetsStatus.TabIndex = 23;
            this.lblAssetsStatus.Text = "A";
            this.lblAssetsStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblOrdersStatus
            // 
            this.lblOrdersStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.lblOrdersStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblOrdersStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOrdersStatus.Location = new System.Drawing.Point(153, 26);
            this.lblOrdersStatus.Name = "lblOrdersStatus";
            this.lblOrdersStatus.Size = new System.Drawing.Size(22, 34);
            this.lblOrdersStatus.TabIndex = 24;
            this.lblOrdersStatus.Text = "O";
            this.lblOrdersStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblIndustryJobsStatus
            // 
            this.lblIndustryJobsStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.lblIndustryJobsStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblIndustryJobsStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblIndustryJobsStatus.Location = new System.Drawing.Point(181, 26);
            this.lblIndustryJobsStatus.Name = "lblIndustryJobsStatus";
            this.lblIndustryJobsStatus.Size = new System.Drawing.Size(23, 34);
            this.lblIndustryJobsStatus.TabIndex = 25;
            this.lblIndustryJobsStatus.Text = "I";
            this.lblIndustryJobsStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 6;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 66F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.Controls.Add(this.lblOrdersStatus, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblJournalStatus, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblIndustryJobsStatus, 5, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel3, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblAssetsStatus, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblTransStatus, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 6F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(207, 66);
            this.tableLayoutPanel1.TabIndex = 26;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.lblCorpTag);
            this.panel2.Controls.Add(this.picPortrait);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.tableLayoutPanel1.SetRowSpan(this.panel2, 3);
            this.panel2.Size = new System.Drawing.Size(60, 60);
            this.panel2.TabIndex = 27;
            // 
            // panel3
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.panel3, 5);
            this.panel3.Controls.Add(this.lblName);
            this.panel3.Controls.Add(this.chkUpdate);
            this.panel3.Controls.Add(this.chkAutoIndustryJobs);
            this.panel3.Controls.Add(this.chkAutoAssets);
            this.panel3.Controls.Add(this.chkAutoOrders);
            this.panel3.Controls.Add(this.chkAutoJournal);
            this.panel3.Controls.Add(this.chkAutoTrans);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(69, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(135, 20);
            this.panel3.TabIndex = 28;
            // 
            // UpdatePanelCompact
            // 
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "UpdatePanelCompact";
            this.Size = new System.Drawing.Size(207, 66);
            ((System.ComponentModel.ISupportInitialize)(this.picPortrait)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox picPortrait;
        private System.Windows.Forms.Label lblCorpTag;
        private System.Windows.Forms.ToolTip errorToolTip;
        private System.Windows.Forms.Label lblName;
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
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
    }
}
