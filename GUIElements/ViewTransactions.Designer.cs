namespace EveMarketMonitorApp.GUIElements
{
    partial class ViewTransactions
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewTransactions));
            this.transactionGrid = new EveMarketMonitorApp.Common.MultisortDataGridView();
            this.GridContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyCellDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyRowDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyCellTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyRowTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCSV = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cmbType = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
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
            this.cmbStation = new System.Windows.Forms.ComboBox();
            this.cmbItem = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.IDColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DateTimeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ItemColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PriceColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.QuantityColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TotalValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UnitProfitColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BuyerColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BuyerIDColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SellerColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SellerIDColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BuyerCharacterColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BuyerCharIDColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SellerCharacterColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SellerCharIDColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BuyerWalletColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SellerWalletColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StationColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.transactionGrid)).BeginInit();
            this.GridContextMenu.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // transactionGrid
            // 
            this.transactionGrid.AllowUserToAddRows = false;
            this.transactionGrid.AllowUserToDeleteRows = false;
            this.transactionGrid.AllowUserToResizeRows = false;
            this.transactionGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.transactionGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.IDColumn,
            this.DateTimeColumn,
            this.ItemColumn,
            this.PriceColumn,
            this.QuantityColumn,
            this.TotalValueColumn,
            this.UnitProfitColumn,
            this.BuyerColumn,
            this.BuyerIDColumn,
            this.SellerColumn,
            this.SellerIDColumn,
            this.BuyerCharacterColumn,
            this.BuyerCharIDColumn,
            this.SellerCharacterColumn,
            this.SellerCharIDColumn,
            this.BuyerWalletColumn,
            this.SellerWalletColumn,
            this.StationColumn});
            this.transactionGrid.ContextMenuStrip = this.GridContextMenu;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.transactionGrid.DefaultCellStyle = dataGridViewCellStyle5;
            this.transactionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.transactionGrid.Location = new System.Drawing.Point(3, 113);
            this.transactionGrid.Name = "transactionGrid";
            this.transactionGrid.ReadOnly = true;
            this.transactionGrid.Size = new System.Drawing.Size(991, 330);
            this.transactionGrid.TabIndex = 0;
            this.transactionGrid.VirtualMode = true;
            this.transactionGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.transactionGrid_MouseDown);
            this.transactionGrid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.transactionGrid_CellFormatting);
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
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.transactionGrid, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(997, 480);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCSV);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 449);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(991, 28);
            this.panel1.TabIndex = 3;
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
            this.btnClose.Location = new System.Drawing.Point(898, 0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(93, 29);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cmbType);
            this.groupBox1.Controls.Add(this.label6);
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
            this.groupBox1.Controls.Add(this.cmbStation);
            this.groupBox1.Controls.Add(this.cmbItem);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(991, 104);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Filters";
            // 
            // cmbType
            // 
            this.cmbType.FormattingEnabled = true;
            this.cmbType.Items.AddRange(new object[] {
            "Any",
            "Buy",
            "Sell"});
            this.cmbType.Location = new System.Drawing.Point(544, 19);
            this.cmbType.Name = "cmbType";
            this.cmbType.Size = new System.Drawing.Size(157, 21);
            this.cmbType.TabIndex = 17;
            this.cmbType.Text = "Any";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(466, 23);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(31, 13);
            this.label6.TabIndex = 16;
            this.label6.Text = "Type";
            // 
            // dtpEndDate
            // 
            this.dtpEndDate.CustomFormat = "dd-MMM-yyyy HH:mm:ss";
            this.dtpEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpEndDate.Location = new System.Drawing.Point(269, 19);
            this.dtpEndDate.Name = "dtpEndDate";
            this.dtpEndDate.Size = new System.Drawing.Size(157, 20);
            this.dtpEndDate.TabIndex = 15;
            // 
            // dtpStartDate
            // 
            this.dtpStartDate.CustomFormat = "dd-MMM-yyyy HH:mm:ss";
            this.dtpStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpStartDate.Location = new System.Drawing.Point(82, 19);
            this.dtpStartDate.Name = "dtpStartDate";
            this.dtpStartDate.Size = new System.Drawing.Size(157, 20);
            this.dtpStartDate.TabIndex = 14;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(245, 23);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(16, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "to";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 23);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(60, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Date range";
            // 
            // chkIgnoreWallet
            // 
            this.chkIgnoreWallet.AutoSize = true;
            this.chkIgnoreWallet.Location = new System.Drawing.Point(906, 75);
            this.chkIgnoreWallet.Name = "chkIgnoreWallet";
            this.chkIgnoreWallet.Size = new System.Drawing.Size(77, 17);
            this.chkIgnoreWallet.TabIndex = 11;
            this.chkIgnoreWallet.Text = "Any Wallet";
            this.chkIgnoreWallet.UseVisualStyleBackColor = true;
            // 
            // chkIngoreOwner
            // 
            this.chkIngoreOwner.AutoSize = true;
            this.chkIngoreOwner.Location = new System.Drawing.Point(906, 48);
            this.chkIngoreOwner.Name = "chkIngoreOwner";
            this.chkIngoreOwner.Size = new System.Drawing.Size(78, 17);
            this.chkIngoreOwner.TabIndex = 10;
            this.chkIngoreOwner.Text = "Any Owner";
            this.chkIngoreOwner.UseVisualStyleBackColor = true;
            // 
            // lblWallet
            // 
            this.lblWallet.AutoSize = true;
            this.lblWallet.Location = new System.Drawing.Point(466, 75);
            this.lblWallet.Name = "lblWallet";
            this.lblWallet.Size = new System.Drawing.Size(37, 13);
            this.lblWallet.TabIndex = 9;
            this.lblWallet.Text = "Wallet";
            // 
            // cmbWallet
            // 
            this.cmbWallet.FormattingEnabled = true;
            this.cmbWallet.Location = new System.Drawing.Point(544, 72);
            this.cmbWallet.Name = "cmbWallet";
            this.cmbWallet.Size = new System.Drawing.Size(344, 21);
            this.cmbWallet.TabIndex = 8;
            // 
            // cmbOwner
            // 
            this.cmbOwner.FormattingEnabled = true;
            this.cmbOwner.Location = new System.Drawing.Point(544, 45);
            this.cmbOwner.Name = "cmbOwner";
            this.cmbOwner.Size = new System.Drawing.Size(344, 21);
            this.cmbOwner.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(466, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Owner";
            // 
            // cmbStation
            // 
            this.cmbStation.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbStation.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbStation.FormattingEnabled = true;
            this.cmbStation.Location = new System.Drawing.Point(82, 72);
            this.cmbStation.Name = "cmbStation";
            this.cmbStation.Size = new System.Drawing.Size(344, 21);
            this.cmbStation.TabIndex = 5;
            // 
            // cmbItem
            // 
            this.cmbItem.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbItem.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbItem.FormattingEnabled = true;
            this.cmbItem.Location = new System.Drawing.Point(82, 45);
            this.cmbItem.Name = "cmbItem";
            this.cmbItem.Size = new System.Drawing.Size(344, 21);
            this.cmbItem.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Station";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(27, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Item";
            // 
            // IDColumn
            // 
            this.IDColumn.HeaderText = "ID";
            this.IDColumn.Name = "IDColumn";
            this.IDColumn.ReadOnly = true;
            this.IDColumn.Visible = false;
            // 
            // DateTimeColumn
            // 
            this.DateTimeColumn.HeaderText = "Date & Time";
            this.DateTimeColumn.Name = "DateTimeColumn";
            this.DateTimeColumn.ReadOnly = true;
            // 
            // ItemColumn
            // 
            this.ItemColumn.HeaderText = "Item";
            this.ItemColumn.Name = "ItemColumn";
            this.ItemColumn.ReadOnly = true;
            this.ItemColumn.Width = 240;
            // 
            // PriceColumn
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.PriceColumn.DefaultCellStyle = dataGridViewCellStyle1;
            this.PriceColumn.HeaderText = "Price";
            this.PriceColumn.Name = "PriceColumn";
            this.PriceColumn.ReadOnly = true;
            this.PriceColumn.Width = 150;
            // 
            // QuantityColumn
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.QuantityColumn.DefaultCellStyle = dataGridViewCellStyle2;
            this.QuantityColumn.HeaderText = "Quantity";
            this.QuantityColumn.Name = "QuantityColumn";
            this.QuantityColumn.ReadOnly = true;
            this.QuantityColumn.Width = 80;
            // 
            // TotalValueColumn
            // 
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.TotalValueColumn.DefaultCellStyle = dataGridViewCellStyle3;
            this.TotalValueColumn.HeaderText = "Total";
            this.TotalValueColumn.Name = "TotalValueColumn";
            this.TotalValueColumn.ReadOnly = true;
            this.TotalValueColumn.Width = 150;
            // 
            // UnitProfitColumn
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.UnitProfitColumn.DefaultCellStyle = dataGridViewCellStyle4;
            this.UnitProfitColumn.HeaderText = "Unit Profit";
            this.UnitProfitColumn.Name = "UnitProfitColumn";
            this.UnitProfitColumn.ReadOnly = true;
            this.UnitProfitColumn.Width = 150;
            // 
            // BuyerColumn
            // 
            this.BuyerColumn.HeaderText = "Buyer";
            this.BuyerColumn.Name = "BuyerColumn";
            this.BuyerColumn.ReadOnly = true;
            this.BuyerColumn.Width = 120;
            // 
            // BuyerIDColumn
            // 
            this.BuyerIDColumn.HeaderText = "BuyerID";
            this.BuyerIDColumn.Name = "BuyerIDColumn";
            this.BuyerIDColumn.ReadOnly = true;
            this.BuyerIDColumn.Visible = false;
            // 
            // SellerColumn
            // 
            this.SellerColumn.HeaderText = "Seller";
            this.SellerColumn.Name = "SellerColumn";
            this.SellerColumn.ReadOnly = true;
            this.SellerColumn.Width = 120;
            // 
            // SellerIDColumn
            // 
            this.SellerIDColumn.HeaderText = "SellerID";
            this.SellerIDColumn.Name = "SellerIDColumn";
            this.SellerIDColumn.ReadOnly = true;
            this.SellerIDColumn.Visible = false;
            // 
            // BuyerCharacterColumn
            // 
            this.BuyerCharacterColumn.HeaderText = "Buyer Character";
            this.BuyerCharacterColumn.Name = "BuyerCharacterColumn";
            this.BuyerCharacterColumn.ReadOnly = true;
            this.BuyerCharacterColumn.Width = 120;
            // 
            // BuyerCharIDColumn
            // 
            this.BuyerCharIDColumn.HeaderText = "BuyerCharID";
            this.BuyerCharIDColumn.Name = "BuyerCharIDColumn";
            this.BuyerCharIDColumn.ReadOnly = true;
            this.BuyerCharIDColumn.Visible = false;
            // 
            // SellerCharacterColumn
            // 
            this.SellerCharacterColumn.HeaderText = "Seller Character";
            this.SellerCharacterColumn.Name = "SellerCharacterColumn";
            this.SellerCharacterColumn.ReadOnly = true;
            this.SellerCharacterColumn.Width = 120;
            // 
            // SellerCharIDColumn
            // 
            this.SellerCharIDColumn.HeaderText = "SellerCharID";
            this.SellerCharIDColumn.Name = "SellerCharIDColumn";
            this.SellerCharIDColumn.ReadOnly = true;
            this.SellerCharIDColumn.Visible = false;
            // 
            // BuyerWalletColumn
            // 
            this.BuyerWalletColumn.HeaderText = "Buyer Wallet";
            this.BuyerWalletColumn.Name = "BuyerWalletColumn";
            this.BuyerWalletColumn.ReadOnly = true;
            this.BuyerWalletColumn.Width = 180;
            // 
            // SellerWalletColumn
            // 
            this.SellerWalletColumn.HeaderText = "Seller Wallet";
            this.SellerWalletColumn.Name = "SellerWalletColumn";
            this.SellerWalletColumn.ReadOnly = true;
            this.SellerWalletColumn.Width = 180;
            // 
            // StationColumn
            // 
            this.StationColumn.HeaderText = "Station";
            this.StationColumn.Name = "StationColumn";
            this.StationColumn.ReadOnly = true;
            this.StationColumn.Width = 300;
            // 
            // ViewTransactions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(997, 480);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ViewTransactions";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Transactions";
            this.Load += new System.EventHandler(this.ViewTrans_Load);
            ((System.ComponentModel.ISupportInitialize)(this.transactionGrid)).EndInit();
            this.GridContextMenu.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private EveMarketMonitorApp.Common.MultisortDataGridView transactionGrid;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cmbStation;
        private System.Windows.Forms.ComboBox cmbItem;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblWallet;
        private System.Windows.Forms.ComboBox cmbWallet;
        private System.Windows.Forms.ComboBox cmbOwner;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkIngoreOwner;
        private System.Windows.Forms.CheckBox chkIgnoreWallet;
        private System.Windows.Forms.DateTimePicker dtpEndDate;
        private System.Windows.Forms.DateTimePicker dtpStartDate;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbType;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ContextMenuStrip GridContextMenu;
        private System.Windows.Forms.ToolStripMenuItem copyCellDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyRowDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyCellTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyRowTextToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCSV;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.DataGridViewTextBoxColumn IDColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DateTimeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ItemColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn PriceColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn QuantityColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn TotalValueColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn UnitProfitColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn BuyerColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn BuyerIDColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn SellerColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn SellerIDColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn BuyerCharacterColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn BuyerCharIDColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn SellerCharacterColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn SellerCharIDColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn BuyerWalletColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn SellerWalletColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn StationColumn;
    }
}