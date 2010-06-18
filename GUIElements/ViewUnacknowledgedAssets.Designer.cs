namespace EveMarketMonitorApp.GUIElements
{
    partial class ViewUnacknowledgedAssets
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewUnacknowledgedAssets));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lblBusy = new System.Windows.Forms.Label();
            this.gainedItemsGrid = new EveMarketMonitorApp.Common.MultisortDataGridView();
            this.GainedOwnerColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GainedItemColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GainedLocationColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GainedQuantityColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GainedReasonColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.busyProgress = new System.Windows.Forms.ProgressBar();
            this.lostItemsGrid = new EveMarketMonitorApp.Common.MultisortDataGridView();
            this.LostOwnerColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LostItemColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LostLocationColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LostQuantityColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LostReasonColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.grpMaterials = new System.Windows.Forms.GroupBox();
            this.lstMaterials = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkMaterialsOnly = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gainedItemsGrid)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lostItemsGrid)).BeginInit();
            this.grpMaterials.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox1, 3);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1129, 92);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Information";
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(1123, 73);
            this.label1.TabIndex = 1;
            this.label1.Text = resources.GetString("label1.Text");
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(1052, 524);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(80, 29);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.grpMaterials, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnOk, 2, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1135, 556);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // groupBox2
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox2, 2);
            this.groupBox2.Controls.Add(this.lblBusy);
            this.groupBox2.Controls.Add(this.gainedItemsGrid);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(3, 103);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(928, 202);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Items Gained";
            // 
            // lblBusy
            // 
            this.lblBusy.AutoSize = true;
            this.lblBusy.Location = new System.Drawing.Point(377, 86);
            this.lblBusy.Name = "lblBusy";
            this.lblBusy.Size = new System.Drawing.Size(35, 13);
            this.lblBusy.TabIndex = 1;
            this.lblBusy.Text = "label2";
            // 
            // gainedItemsGrid
            // 
            this.gainedItemsGrid.AllowUserToAddRows = false;
            this.gainedItemsGrid.AllowUserToDeleteRows = false;
            this.gainedItemsGrid.AllowUserToResizeRows = false;
            this.gainedItemsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gainedItemsGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.GainedOwnerColumn,
            this.GainedItemColumn,
            this.GainedLocationColumn,
            this.GainedQuantityColumn,
            this.GainedReasonColumn});
            this.gainedItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gainedItemsGrid.Location = new System.Drawing.Point(3, 16);
            this.gainedItemsGrid.Name = "gainedItemsGrid";
            this.gainedItemsGrid.RowHeadersVisible = false;
            this.gainedItemsGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gainedItemsGrid.Size = new System.Drawing.Size(922, 183);
            this.gainedItemsGrid.TabIndex = 0;
            // 
            // GainedOwnerColumn
            // 
            this.GainedOwnerColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.GainedOwnerColumn.HeaderText = "Owner";
            this.GainedOwnerColumn.Name = "GainedOwnerColumn";
            this.GainedOwnerColumn.ReadOnly = true;
            this.GainedOwnerColumn.Width = 160;
            // 
            // GainedItemColumn
            // 
            this.GainedItemColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.GainedItemColumn.HeaderText = "Item";
            this.GainedItemColumn.Name = "GainedItemColumn";
            this.GainedItemColumn.ReadOnly = true;
            this.GainedItemColumn.Width = 240;
            // 
            // GainedLocationColumn
            // 
            this.GainedLocationColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.GainedLocationColumn.HeaderText = "Location";
            this.GainedLocationColumn.Name = "GainedLocationColumn";
            this.GainedLocationColumn.ReadOnly = true;
            this.GainedLocationColumn.Width = 240;
            // 
            // GainedQuantityColumn
            // 
            this.GainedQuantityColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.GainedQuantityColumn.HeaderText = "Quantity";
            this.GainedQuantityColumn.Name = "GainedQuantityColumn";
            this.GainedQuantityColumn.ReadOnly = true;
            // 
            // GainedReasonColumn
            // 
            this.GainedReasonColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.GainedReasonColumn.HeaderText = "Reason";
            this.GainedReasonColumn.Name = "GainedReasonColumn";
            this.GainedReasonColumn.Width = 160;
            // 
            // groupBox3
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox3, 2);
            this.groupBox3.Controls.Add(this.busyProgress);
            this.groupBox3.Controls.Add(this.lostItemsGrid);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(3, 311);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(928, 202);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Items Missing";
            // 
            // busyProgress
            // 
            this.busyProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.busyProgress.Location = new System.Drawing.Point(187, 82);
            this.busyProgress.Name = "busyProgress";
            this.busyProgress.Size = new System.Drawing.Size(555, 33);
            this.busyProgress.TabIndex = 1;
            // 
            // lostItemsGrid
            // 
            this.lostItemsGrid.AllowUserToAddRows = false;
            this.lostItemsGrid.AllowUserToDeleteRows = false;
            this.lostItemsGrid.AllowUserToResizeRows = false;
            this.lostItemsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.lostItemsGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.LostOwnerColumn,
            this.LostItemColumn,
            this.LostLocationColumn,
            this.LostQuantityColumn,
            this.LostReasonColumn});
            this.lostItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lostItemsGrid.Location = new System.Drawing.Point(3, 16);
            this.lostItemsGrid.Name = "lostItemsGrid";
            this.lostItemsGrid.RowHeadersVisible = false;
            this.lostItemsGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.lostItemsGrid.Size = new System.Drawing.Size(922, 183);
            this.lostItemsGrid.TabIndex = 0;
            // 
            // LostOwnerColumn
            // 
            this.LostOwnerColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.LostOwnerColumn.HeaderText = "Owner";
            this.LostOwnerColumn.Name = "LostOwnerColumn";
            this.LostOwnerColumn.ReadOnly = true;
            this.LostOwnerColumn.Width = 160;
            // 
            // LostItemColumn
            // 
            this.LostItemColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.LostItemColumn.HeaderText = "Item";
            this.LostItemColumn.Name = "LostItemColumn";
            this.LostItemColumn.ReadOnly = true;
            this.LostItemColumn.Width = 240;
            // 
            // LostLocationColumn
            // 
            this.LostLocationColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.LostLocationColumn.HeaderText = "Location";
            this.LostLocationColumn.Name = "LostLocationColumn";
            this.LostLocationColumn.ReadOnly = true;
            this.LostLocationColumn.Width = 240;
            // 
            // LostQuantityColumn
            // 
            this.LostQuantityColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.LostQuantityColumn.HeaderText = "Quantity";
            this.LostQuantityColumn.Name = "LostQuantityColumn";
            this.LostQuantityColumn.ReadOnly = true;
            // 
            // LostReasonColumn
            // 
            this.LostReasonColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.LostReasonColumn.HeaderText = "Reason";
            this.LostReasonColumn.Name = "LostReasonColumn";
            this.LostReasonColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.LostReasonColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.LostReasonColumn.Width = 160;
            // 
            // grpMaterials
            // 
            this.grpMaterials.Controls.Add(this.lstMaterials);
            this.grpMaterials.Controls.Add(this.label2);
            this.grpMaterials.Controls.Add(this.chkMaterialsOnly);
            this.grpMaterials.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpMaterials.Location = new System.Drawing.Point(937, 103);
            this.grpMaterials.Name = "grpMaterials";
            this.tableLayoutPanel1.SetRowSpan(this.grpMaterials, 2);
            this.grpMaterials.Size = new System.Drawing.Size(195, 410);
            this.grpMaterials.TabIndex = 4;
            this.grpMaterials.TabStop = false;
            this.grpMaterials.Text = "Bill of Materials";
            // 
            // lstMaterials
            // 
            this.lstMaterials.FormattingEnabled = true;
            this.lstMaterials.Location = new System.Drawing.Point(6, 147);
            this.lstMaterials.Name = "lstMaterials";
            this.lstMaterials.Size = new System.Drawing.Size(182, 225);
            this.lstMaterials.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(6, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(182, 128);
            this.label2.TabIndex = 1;
            this.label2.Text = resources.GetString("label2.Text");
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // chkMaterialsOnly
            // 
            this.chkMaterialsOnly.AutoSize = true;
            this.chkMaterialsOnly.Location = new System.Drawing.Point(9, 387);
            this.chkMaterialsOnly.Name = "chkMaterialsOnly";
            this.chkMaterialsOnly.Size = new System.Drawing.Size(156, 17);
            this.chkMaterialsOnly.TabIndex = 0;
            this.chkMaterialsOnly.Text = "Show only missing materials";
            this.chkMaterialsOnly.UseVisualStyleBackColor = true;
            // 
            // ViewUnacknowledgedAssets
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1135, 556);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ViewUnacknowledgedAssets";
            this.Text = "Unacknowledged Asset Changes";
            this.Load += new System.EventHandler(this.ViewUnacknowledgedAssets_Load);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gainedItemsGrid)).EndInit();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.lostItemsGrid)).EndInit();
            this.grpMaterials.ResumeLayout(false);
            this.grpMaterials.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private EveMarketMonitorApp.Common.MultisortDataGridView gainedItemsGrid;
        private EveMarketMonitorApp.Common.MultisortDataGridView lostItemsGrid;
        private System.Windows.Forms.Label lblBusy;
        private System.Windows.Forms.ProgressBar busyProgress;
        private System.Windows.Forms.GroupBox grpMaterials;
        private System.Windows.Forms.CheckBox chkMaterialsOnly;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox lstMaterials;
        private System.Windows.Forms.DataGridViewTextBoxColumn GainedOwnerColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn GainedItemColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn GainedLocationColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn GainedQuantityColumn;
        private System.Windows.Forms.DataGridViewComboBoxColumn GainedReasonColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn LostOwnerColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn LostItemColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn LostLocationColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn LostQuantityColumn;
        private System.Windows.Forms.DataGridViewComboBoxColumn LostReasonColumn;
    }
}