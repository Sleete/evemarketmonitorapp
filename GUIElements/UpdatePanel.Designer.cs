namespace EveMarketMonitorApp.GUIElements
{
    partial class UpdatePanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdatePanel));
            this.picPortrait = new System.Windows.Forms.PictureBox();
            this.lblTransactions = new System.Windows.Forms.Label();
            this.lblJournal = new System.Windows.Forms.Label();
            this.lblOrders = new System.Windows.Forms.Label();
            this.lblAssets = new System.Windows.Forms.Label();
            this.chkAutoTrans = new System.Windows.Forms.CheckBox();
            this.chkAutoJournal = new System.Windows.Forms.CheckBox();
            this.chkAutoOrders = new System.Windows.Forms.CheckBox();
            this.chkAutoAssets = new System.Windows.Forms.CheckBox();
            this.lblTransStatus = new System.Windows.Forms.Label();
            this.lblJournalStatus = new System.Windows.Forms.Label();
            this.lblOrdersStatus = new System.Windows.Forms.Label();
            this.lblAssetsStatus = new System.Windows.Forms.Label();
            this.lblCorpTag = new System.Windows.Forms.Label();
            this.errorToolTip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.picPortrait)).BeginInit();
            this.SuspendLayout();
            // 
            // picPortrait
            // 
            this.picPortrait.Image = ((System.Drawing.Image)(resources.GetObject("picPortrait.Image")));
            this.picPortrait.InitialImage = null;
            this.picPortrait.Location = new System.Drawing.Point(3, 3);
            this.picPortrait.Name = "picPortrait";
            this.picPortrait.Size = new System.Drawing.Size(76, 76);
            this.picPortrait.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picPortrait.TabIndex = 0;
            this.picPortrait.TabStop = false;
            this.picPortrait.Click += new System.EventHandler(this.picPortrait_Click);
            // 
            // lblTransactions
            // 
            this.lblTransactions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTransactions.BackColor = System.Drawing.Color.LightSteelBlue;
            this.lblTransactions.Location = new System.Drawing.Point(85, 3);
            this.lblTransactions.Name = "lblTransactions";
            this.lblTransactions.Size = new System.Drawing.Size(165, 14);
            this.lblTransactions.TabIndex = 1;
            this.lblTransactions.Text = "Transactions";
            this.lblTransactions.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblJournal
            // 
            this.lblJournal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblJournal.BackColor = System.Drawing.Color.LightSteelBlue;
            this.lblJournal.Location = new System.Drawing.Point(85, 23);
            this.lblJournal.Name = "lblJournal";
            this.lblJournal.Size = new System.Drawing.Size(165, 14);
            this.lblJournal.TabIndex = 2;
            this.lblJournal.Text = "Journal";
            this.lblJournal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblOrders
            // 
            this.lblOrders.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblOrders.BackColor = System.Drawing.Color.LightSteelBlue;
            this.lblOrders.Location = new System.Drawing.Point(85, 43);
            this.lblOrders.Name = "lblOrders";
            this.lblOrders.Size = new System.Drawing.Size(165, 14);
            this.lblOrders.TabIndex = 3;
            this.lblOrders.Text = "Orders";
            this.lblOrders.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAssets
            // 
            this.lblAssets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAssets.BackColor = System.Drawing.Color.LightSteelBlue;
            this.lblAssets.Location = new System.Drawing.Point(85, 63);
            this.lblAssets.Name = "lblAssets";
            this.lblAssets.Size = new System.Drawing.Size(165, 14);
            this.lblAssets.TabIndex = 4;
            this.lblAssets.Text = "Assets";
            this.lblAssets.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // chkAutoTrans
            // 
            this.chkAutoTrans.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAutoTrans.AutoSize = true;
            this.chkAutoTrans.Location = new System.Drawing.Point(258, 3);
            this.chkAutoTrans.Name = "chkAutoTrans";
            this.chkAutoTrans.Size = new System.Drawing.Size(15, 14);
            this.chkAutoTrans.TabIndex = 5;
            this.chkAutoTrans.UseVisualStyleBackColor = true;
            // 
            // chkAutoJournal
            // 
            this.chkAutoJournal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAutoJournal.AutoSize = true;
            this.chkAutoJournal.Location = new System.Drawing.Point(258, 23);
            this.chkAutoJournal.Name = "chkAutoJournal";
            this.chkAutoJournal.Size = new System.Drawing.Size(15, 14);
            this.chkAutoJournal.TabIndex = 6;
            this.chkAutoJournal.UseVisualStyleBackColor = true;
            // 
            // chkAutoOrders
            // 
            this.chkAutoOrders.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAutoOrders.AutoSize = true;
            this.chkAutoOrders.Location = new System.Drawing.Point(258, 43);
            this.chkAutoOrders.Name = "chkAutoOrders";
            this.chkAutoOrders.Size = new System.Drawing.Size(15, 14);
            this.chkAutoOrders.TabIndex = 7;
            this.chkAutoOrders.UseVisualStyleBackColor = true;
            // 
            // chkAutoAssets
            // 
            this.chkAutoAssets.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAutoAssets.AutoSize = true;
            this.chkAutoAssets.Location = new System.Drawing.Point(258, 63);
            this.chkAutoAssets.Name = "chkAutoAssets";
            this.chkAutoAssets.Size = new System.Drawing.Size(15, 14);
            this.chkAutoAssets.TabIndex = 8;
            this.chkAutoAssets.UseVisualStyleBackColor = true;
            // 
            // lblTransStatus
            // 
            this.lblTransStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTransStatus.AutoEllipsis = true;
            this.lblTransStatus.BackColor = System.Drawing.Color.Lavender;
            this.lblTransStatus.Location = new System.Drawing.Point(155, 3);
            this.lblTransStatus.Name = "lblTransStatus";
            this.lblTransStatus.Size = new System.Drawing.Size(95, 14);
            this.lblTransStatus.TabIndex = 9;
            this.lblTransStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblJournalStatus
            // 
            this.lblJournalStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblJournalStatus.AutoEllipsis = true;
            this.lblJournalStatus.BackColor = System.Drawing.Color.Lavender;
            this.lblJournalStatus.Location = new System.Drawing.Point(155, 23);
            this.lblJournalStatus.Name = "lblJournalStatus";
            this.lblJournalStatus.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblJournalStatus.Size = new System.Drawing.Size(95, 14);
            this.lblJournalStatus.TabIndex = 10;
            this.lblJournalStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblOrdersStatus
            // 
            this.lblOrdersStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblOrdersStatus.AutoEllipsis = true;
            this.lblOrdersStatus.BackColor = System.Drawing.Color.Lavender;
            this.lblOrdersStatus.Location = new System.Drawing.Point(155, 43);
            this.lblOrdersStatus.Name = "lblOrdersStatus";
            this.lblOrdersStatus.Size = new System.Drawing.Size(95, 14);
            this.lblOrdersStatus.TabIndex = 11;
            this.lblOrdersStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblAssetsStatus
            // 
            this.lblAssetsStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAssetsStatus.AutoEllipsis = true;
            this.lblAssetsStatus.BackColor = System.Drawing.Color.Lavender;
            this.lblAssetsStatus.Location = new System.Drawing.Point(155, 63);
            this.lblAssetsStatus.Name = "lblAssetsStatus";
            this.lblAssetsStatus.Size = new System.Drawing.Size(95, 14);
            this.lblAssetsStatus.TabIndex = 12;
            this.lblAssetsStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblCorpTag
            // 
            this.lblCorpTag.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCorpTag.Location = new System.Drawing.Point(4, 30);
            this.lblCorpTag.Name = "lblCorpTag";
            this.lblCorpTag.Size = new System.Drawing.Size(74, 23);
            this.lblCorpTag.TabIndex = 13;
            this.lblCorpTag.Text = "[CORP]";
            this.lblCorpTag.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblCorpTag.Visible = false;
            this.lblCorpTag.Click += new System.EventHandler(this.lblCorpTag_Click);
            // 
            // UpdatePanel
            // 
            this.Controls.Add(this.lblCorpTag);
            this.Controls.Add(this.lblAssetsStatus);
            this.Controls.Add(this.lblOrdersStatus);
            this.Controls.Add(this.lblJournalStatus);
            this.Controls.Add(this.lblTransStatus);
            this.Controls.Add(this.chkAutoAssets);
            this.Controls.Add(this.chkAutoOrders);
            this.Controls.Add(this.chkAutoJournal);
            this.Controls.Add(this.chkAutoTrans);
            this.Controls.Add(this.lblAssets);
            this.Controls.Add(this.lblOrders);
            this.Controls.Add(this.lblJournal);
            this.Controls.Add(this.lblTransactions);
            this.Controls.Add(this.picPortrait);
            this.Name = "UpdatePanel";
            this.Size = new System.Drawing.Size(276, 82);
            ((System.ComponentModel.ISupportInitialize)(this.picPortrait)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picPortrait;
        private System.Windows.Forms.Label lblTransactions;
        private System.Windows.Forms.Label lblJournal;
        private System.Windows.Forms.Label lblOrders;
        private System.Windows.Forms.Label lblAssets;
        private System.Windows.Forms.CheckBox chkAutoTrans;
        private System.Windows.Forms.CheckBox chkAutoJournal;
        private System.Windows.Forms.CheckBox chkAutoOrders;
        private System.Windows.Forms.CheckBox chkAutoAssets;
        private System.Windows.Forms.Label lblTransStatus;
        private System.Windows.Forms.Label lblJournalStatus;
        private System.Windows.Forms.Label lblOrdersStatus;
        private System.Windows.Forms.Label lblAssetsStatus;
        private System.Windows.Forms.Label lblCorpTag;
        private System.Windows.Forms.ToolTip errorToolTip;
    }
}
