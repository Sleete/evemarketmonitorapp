namespace EveMarketMonitorApp.GUIElements
{
    partial class ViewIndustryJobs
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewIndustryJobs));
            this.industryJobsDataGridView = new EveMarketMonitorApp.Common.MultisortDataGridView();
            this.GridContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyCellDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyRowDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyCellTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyRowTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dtpEndDate = new System.Windows.Forms.DateTimePicker();
            this.dtpStartDate = new System.Windows.Forms.DateTimePicker();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.chkIgnoreWallet = new System.Windows.Forms.CheckBox();
            this.chkIngoreOwner = new System.Windows.Forms.CheckBox();
            this.lblWallet = new System.Windows.Forms.Label();
            this.cmbWallet = new System.Windows.Forms.ComboBox();
            this.cmbOwner = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCSV = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.IDColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DateColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.InstalledItemColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.InstalledLocationColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.InstalledQuantityColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.InstalledMEColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.InstalledPEColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.InstalledRunsRemainingColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OutputLocationColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.InstallerColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.JobRunsColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OutputRunsColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ArgNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AmountColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BalanceColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ReasonColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.industryJobsDataGridView)).BeginInit();
            this.GridContextMenu.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // industryJobsDataGridView
            // 
            this.industryJobsDataGridView.AllowUserToAddRows = false;
            this.industryJobsDataGridView.AllowUserToDeleteRows = false;
            this.industryJobsDataGridView.AllowUserToResizeRows = false;
            this.industryJobsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.IDColumn,
            this.DateColumn,
            this.InstalledItemColumn,
            this.InstalledLocationColumn,
            this.InstalledQuantityColumn,
            this.InstalledMEColumn,
            this.InstalledPEColumn,
            this.InstalledRunsRemainingColumn,
            this.OutputLocationColumn,
            this.InstallerColumn,
            this.JobRunsColumn,
            this.OutputRunsColumn,
            this.ArgNameColumn,
            this.AmountColumn,
            this.BalanceColumn,
            this.ReasonColumn});
            this.industryJobsDataGridView.ContextMenuStrip = this.GridContextMenu;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.industryJobsDataGridView.DefaultCellStyle = dataGridViewCellStyle3;
            this.industryJobsDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.industryJobsDataGridView.Location = new System.Drawing.Point(3, 89);
            this.industryJobsDataGridView.Name = "industryJobsDataGridView";
            this.industryJobsDataGridView.ReadOnly = true;
            this.industryJobsDataGridView.Size = new System.Drawing.Size(1064, 304);
            this.industryJobsDataGridView.TabIndex = 1;
            this.industryJobsDataGridView.VirtualMode = true;
            this.industryJobsDataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.journalDataGridView_MouseDown);
            this.industryJobsDataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.journalDataGridView_CellFormatting);
            // 
            // GridContextMenu
            // 
            this.GridContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyCellDataToolStripMenuItem,
            this.copyRowDataToolStripMenuItem,
            this.copyCellTextToolStripMenuItem,
            this.copyRowTextToolStripMenuItem});
            this.GridContextMenu.Name = "GridContextMenu";
            this.GridContextMenu.Size = new System.Drawing.Size(161, 92);
            // 
            // copyCellDataToolStripMenuItem
            // 
            this.copyCellDataToolStripMenuItem.Name = "copyCellDataToolStripMenuItem";
            this.copyCellDataToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.copyCellDataToolStripMenuItem.Text = "Copy Cell Data";
            this.copyCellDataToolStripMenuItem.Click += new System.EventHandler(this.copyCellDataToolStripMenuItem_Click);
            // 
            // copyRowDataToolStripMenuItem
            // 
            this.copyRowDataToolStripMenuItem.Name = "copyRowDataToolStripMenuItem";
            this.copyRowDataToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.copyRowDataToolStripMenuItem.Text = "Copy Row Data";
            this.copyRowDataToolStripMenuItem.Click += new System.EventHandler(this.copyRowDataToolStripMenuItem_Click);
            // 
            // copyCellTextToolStripMenuItem
            // 
            this.copyCellTextToolStripMenuItem.Name = "copyCellTextToolStripMenuItem";
            this.copyCellTextToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.copyCellTextToolStripMenuItem.Text = "Copy Cell Text";
            this.copyCellTextToolStripMenuItem.Click += new System.EventHandler(this.copyCellTextToolStripMenuItem_Click);
            // 
            // copyRowTextToolStripMenuItem
            // 
            this.copyRowTextToolStripMenuItem.Name = "copyRowTextToolStripMenuItem";
            this.copyRowTextToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.copyRowTextToolStripMenuItem.Text = "Copy Row Text";
            this.copyRowTextToolStripMenuItem.Click += new System.EventHandler(this.copyRowTextToolStripMenuItem_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.industryJobsDataGridView, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 86F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1070, 430);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dtpEndDate);
            this.groupBox1.Controls.Add(this.dtpStartDate);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.chkIgnoreWallet);
            this.groupBox1.Controls.Add(this.chkIngoreOwner);
            this.groupBox1.Controls.Add(this.lblWallet);
            this.groupBox1.Controls.Add(this.cmbWallet);
            this.groupBox1.Controls.Add(this.cmbOwner);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1064, 80);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Filters";
            // 
            // dtpEndDate
            // 
            this.dtpEndDate.CustomFormat = "dd-MMM-yyyy HH:mm:ss";
            this.dtpEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpEndDate.Location = new System.Drawing.Point(316, 19);
            this.dtpEndDate.Name = "dtpEndDate";
            this.dtpEndDate.Size = new System.Drawing.Size(171, 20);
            this.dtpEndDate.TabIndex = 21;
            this.dtpEndDate.DropDown += new System.EventHandler(this.dtpEndDate_DropDown);
            this.dtpEndDate.CloseUp += new System.EventHandler(this.dtpEndDate_CloseUp);
            // 
            // dtpStartDate
            // 
            this.dtpStartDate.CustomFormat = "dd-MMM-yyyy HH:mm:ss";
            this.dtpStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpStartDate.Location = new System.Drawing.Point(113, 19);
            this.dtpStartDate.Name = "dtpStartDate";
            this.dtpStartDate.Size = new System.Drawing.Size(171, 20);
            this.dtpStartDate.TabIndex = 20;
            this.dtpStartDate.DropDown += new System.EventHandler(this.dtpStartDate_DropDown);
            this.dtpStartDate.CloseUp += new System.EventHandler(this.dtpStartDate_CloseUp);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(290, 23);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(16, 13);
            this.label5.TabIndex = 19;
            this.label5.Text = "to";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 23);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(60, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "Date range";
            // 
            // chkIgnoreWallet
            // 
            this.chkIgnoreWallet.AutoSize = true;
            this.chkIgnoreWallet.Location = new System.Drawing.Point(954, 49);
            this.chkIgnoreWallet.Name = "chkIgnoreWallet";
            this.chkIgnoreWallet.Size = new System.Drawing.Size(77, 17);
            this.chkIgnoreWallet.TabIndex = 17;
            this.chkIgnoreWallet.Text = "Any Wallet";
            this.chkIgnoreWallet.UseVisualStyleBackColor = true;
            // 
            // chkIngoreOwner
            // 
            this.chkIngoreOwner.AutoSize = true;
            this.chkIngoreOwner.Location = new System.Drawing.Point(954, 22);
            this.chkIngoreOwner.Name = "chkIngoreOwner";
            this.chkIngoreOwner.Size = new System.Drawing.Size(78, 17);
            this.chkIngoreOwner.TabIndex = 16;
            this.chkIngoreOwner.Text = "Any Owner";
            this.chkIngoreOwner.UseVisualStyleBackColor = true;
            // 
            // lblWallet
            // 
            this.lblWallet.AutoSize = true;
            this.lblWallet.Location = new System.Drawing.Point(515, 49);
            this.lblWallet.Name = "lblWallet";
            this.lblWallet.Size = new System.Drawing.Size(37, 13);
            this.lblWallet.TabIndex = 15;
            this.lblWallet.Text = "Wallet";
            // 
            // cmbWallet
            // 
            this.cmbWallet.FormattingEnabled = true;
            this.cmbWallet.Location = new System.Drawing.Point(592, 46);
            this.cmbWallet.Name = "cmbWallet";
            this.cmbWallet.Size = new System.Drawing.Size(344, 21);
            this.cmbWallet.TabIndex = 14;
            // 
            // cmbOwner
            // 
            this.cmbOwner.FormattingEnabled = true;
            this.cmbOwner.Location = new System.Drawing.Point(592, 20);
            this.cmbOwner.Name = "cmbOwner";
            this.cmbOwner.Size = new System.Drawing.Size(344, 21);
            this.cmbOwner.TabIndex = 13;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(514, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Owner";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCSV);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 399);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1064, 28);
            this.panel1.TabIndex = 2;
            // 
            // btnCSV
            // 
            this.btnCSV.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCSV.Location = new System.Drawing.Point(0, 0);
            this.btnCSV.Name = "btnCSV";
            this.btnCSV.Size = new System.Drawing.Size(93, 29);
            this.btnCSV.TabIndex = 2;
            this.btnCSV.Text = "Export to CSV";
            this.btnCSV.UseVisualStyleBackColor = true;
            this.btnCSV.Click += new System.EventHandler(this.btnCSV_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(971, 0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(93, 29);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // IDColumn
            // 
            this.IDColumn.HeaderText = "ID";
            this.IDColumn.Name = "IDColumn";
            this.IDColumn.ReadOnly = true;
            this.IDColumn.Visible = false;
            // 
            // DateColumn
            // 
            this.DateColumn.HeaderText = "Date & Time";
            this.DateColumn.Name = "DateColumn";
            this.DateColumn.ReadOnly = true;
            // 
            // InstalledItemColumn
            // 
            this.InstalledItemColumn.HeaderText = "Installed Item";
            this.InstalledItemColumn.Name = "InstalledItemColumn";
            this.InstalledItemColumn.ReadOnly = true;
            this.InstalledItemColumn.Width = 170;
            // 
            // InstalledLocationColumn
            // 
            this.InstalledLocationColumn.HeaderText = "Installed Location";
            this.InstalledLocationColumn.Name = "InstalledLocationColumn";
            this.InstalledLocationColumn.ReadOnly = true;
            this.InstalledLocationColumn.Width = 150;
            // 
            // InstalledQuantityColumn
            // 
            this.InstalledQuantityColumn.HeaderText = "Installed Quantity";
            this.InstalledQuantityColumn.Name = "InstalledQuantityColumn";
            this.InstalledQuantityColumn.ReadOnly = true;
            // 
            // InstalledMEColumn
            // 
            this.InstalledMEColumn.HeaderText = "Installed ME";
            this.InstalledMEColumn.Name = "InstalledMEColumn";
            this.InstalledMEColumn.ReadOnly = true;
            this.InstalledMEColumn.Width = 80;
            // 
            // InstalledPEColumn
            // 
            this.InstalledPEColumn.HeaderText = "Installed PE";
            this.InstalledPEColumn.Name = "InstalledPEColumn";
            this.InstalledPEColumn.ReadOnly = true;
            this.InstalledPEColumn.Width = 80;
            // 
            // InstalledRunsRemainingColumn
            // 
            this.InstalledRunsRemainingColumn.HeaderText = "Installed Runs Remaining";
            this.InstalledRunsRemainingColumn.Name = "InstalledRunsRemainingColumn";
            this.InstalledRunsRemainingColumn.ReadOnly = true;
            this.InstalledRunsRemainingColumn.Width = 80;
            // 
            // OutputLocationColumn
            // 
            this.OutputLocationColumn.HeaderText = "Output Location";
            this.OutputLocationColumn.Name = "OutputLocationColumn";
            this.OutputLocationColumn.ReadOnly = true;
            this.OutputLocationColumn.Visible = false;
            this.OutputLocationColumn.Width = 150;
            // 
            // InstallerColumn
            // 
            this.InstallerColumn.HeaderText = "Installer";
            this.InstallerColumn.Name = "InstallerColumn";
            this.InstallerColumn.ReadOnly = true;
            this.InstallerColumn.Width = 120;
            // 
            // JobRunsColumn
            // 
            this.JobRunsColumn.HeaderText = "Job Runs";
            this.JobRunsColumn.Name = "JobRunsColumn";
            this.JobRunsColumn.ReadOnly = true;
            this.JobRunsColumn.Width = 80;
            // 
            // OutputRunsColumn
            // 
            this.OutputRunsColumn.HeaderText = "Output Runs";
            this.OutputRunsColumn.Name = "OutputRunsColumn";
            this.OutputRunsColumn.ReadOnly = true;
            this.OutputRunsColumn.Width = 80;
            // 
            // ArgNameColumn
            // 
            this.ArgNameColumn.HeaderText = "Arg Name";
            this.ArgNameColumn.Name = "ArgNameColumn";
            this.ArgNameColumn.ReadOnly = true;
            this.ArgNameColumn.Width = 120;
            // 
            // AmountColumn
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.AmountColumn.DefaultCellStyle = dataGridViewCellStyle1;
            this.AmountColumn.HeaderText = "Amount";
            this.AmountColumn.Name = "AmountColumn";
            this.AmountColumn.ReadOnly = true;
            this.AmountColumn.Width = 130;
            // 
            // BalanceColumn
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.BalanceColumn.DefaultCellStyle = dataGridViewCellStyle2;
            this.BalanceColumn.HeaderText = "Balance";
            this.BalanceColumn.Name = "BalanceColumn";
            this.BalanceColumn.ReadOnly = true;
            this.BalanceColumn.Width = 140;
            // 
            // ReasonColumn
            // 
            this.ReasonColumn.HeaderText = "Reason";
            this.ReasonColumn.Name = "ReasonColumn";
            this.ReasonColumn.ReadOnly = true;
            this.ReasonColumn.Width = 300;
            // 
            // ViewIndustryJobs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1070, 430);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ViewIndustryJobs";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Industry Jobs";
            this.Load += new System.EventHandler(this.ViewJournal_Load);
            ((System.ComponentModel.ISupportInitialize)(this.industryJobsDataGridView)).EndInit();
            this.GridContextMenu.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private EveMarketMonitorApp.Common.MultisortDataGridView industryJobsDataGridView;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkIgnoreWallet;
        private System.Windows.Forms.CheckBox chkIngoreOwner;
        private System.Windows.Forms.Label lblWallet;
        private System.Windows.Forms.ComboBox cmbWallet;
        private System.Windows.Forms.ComboBox cmbOwner;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DateTimePicker dtpEndDate;
        private System.Windows.Forms.DateTimePicker dtpStartDate;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ContextMenuStrip GridContextMenu;
        private System.Windows.Forms.ToolStripMenuItem copyCellDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyRowDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyCellTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyRowTextToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCSV;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.DataGridViewTextBoxColumn IDColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DateColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn InstalledItemColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn InstalledLocationColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn InstalledQuantityColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn InstalledMEColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn InstalledPEColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn InstalledRunsRemainingColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn OutputLocationColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn InstallerColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn JobRunsColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn OutputRunsColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ArgNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn AmountColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn BalanceColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ReasonColumn;

    }
}