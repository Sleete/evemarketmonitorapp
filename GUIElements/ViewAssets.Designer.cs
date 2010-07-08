namespace EveMarketMonitorApp.GUIElements
{
    partial class ViewAssets
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewAssets));
            this.AssetsGrid = new EveMarketMonitorApp.Common.MultisortDataGridView();
            this.GridContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyCellDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyRowDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyCellTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyRowTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.assetsTree = new System.Windows.Forms.TreeView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkIngoreOwner = new System.Windows.Forms.CheckBox();
            this.cmbOwner = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbItem = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCSV = new System.Windows.Forms.Button();
            this.btnStore = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.OwnerColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ItemColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LocationColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.QuantityColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StatusColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AutoConExcludeColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ReprocessorExcludeColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.CostColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.AssetsGrid)).BeginInit();
            this.GridContextMenu.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // AssetsGrid
            // 
            this.AssetsGrid.AllowUserToAddRows = false;
            this.AssetsGrid.AllowUserToDeleteRows = false;
            this.AssetsGrid.AllowUserToResizeRows = false;
            this.AssetsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.AssetsGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.OwnerColumn,
            this.ItemColumn,
            this.LocationColumn,
            this.QuantityColumn,
            this.StatusColumn,
            this.AutoConExcludeColumn,
            this.ReprocessorExcludeColumn,
            this.CostColumn});
            this.AssetsGrid.ContextMenuStrip = this.GridContextMenu;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.AssetsGrid.DefaultCellStyle = dataGridViewCellStyle3;
            this.AssetsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AssetsGrid.Location = new System.Drawing.Point(0, 0);
            this.AssetsGrid.Name = "AssetsGrid";
            this.AssetsGrid.Size = new System.Drawing.Size(846, 317);
            this.AssetsGrid.TabIndex = 0;
            this.AssetsGrid.VirtualMode = true;
            this.AssetsGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.AssetsGrid_MouseDown);
            this.AssetsGrid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.AssetsGrid_CellFormatting);
            this.AssetsGrid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.AssetsGrid_CellContentClick);
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
            this.tableLayoutPanel1.Controls.Add(this.splitContainer1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1123, 437);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(3, 83);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.assetsTree);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.AssetsGrid);
            this.splitContainer1.Size = new System.Drawing.Size(1117, 317);
            this.splitContainer1.SplitterDistance = 267;
            this.splitContainer1.TabIndex = 18;
            // 
            // assetsTree
            // 
            this.assetsTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.assetsTree.HideSelection = false;
            this.assetsTree.Location = new System.Drawing.Point(0, 0);
            this.assetsTree.Name = "assetsTree";
            this.assetsTree.Size = new System.Drawing.Size(267, 317);
            this.assetsTree.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkIngoreOwner);
            this.groupBox1.Controls.Add(this.cmbOwner);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.cmbItem);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1117, 74);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Filters";
            // 
            // chkIngoreOwner
            // 
            this.chkIngoreOwner.AutoSize = true;
            this.chkIngoreOwner.Location = new System.Drawing.Point(1005, 22);
            this.chkIngoreOwner.Name = "chkIngoreOwner";
            this.chkIngoreOwner.Size = new System.Drawing.Size(78, 17);
            this.chkIngoreOwner.TabIndex = 16;
            this.chkIngoreOwner.Text = "Any Owner";
            this.chkIngoreOwner.UseVisualStyleBackColor = true;
            // 
            // cmbOwner
            // 
            this.cmbOwner.FormattingEnabled = true;
            this.cmbOwner.Location = new System.Drawing.Point(642, 20);
            this.cmbOwner.Name = "cmbOwner";
            this.cmbOwner.Size = new System.Drawing.Size(344, 21);
            this.cmbOwner.TabIndex = 13;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(566, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Owner";
            // 
            // cmbItem
            // 
            this.cmbItem.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbItem.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbItem.FormattingEnabled = true;
            this.cmbItem.Location = new System.Drawing.Point(102, 18);
            this.cmbItem.Name = "cmbItem";
            this.cmbItem.Size = new System.Drawing.Size(400, 21);
            this.cmbItem.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(27, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Item";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCSV);
            this.panel1.Controls.Add(this.btnStore);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 406);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1117, 28);
            this.panel1.TabIndex = 19;
            // 
            // btnCSV
            // 
            this.btnCSV.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCSV.Location = new System.Drawing.Point(0, 0);
            this.btnCSV.Name = "btnCSV";
            this.btnCSV.Size = new System.Drawing.Size(93, 29);
            this.btnCSV.TabIndex = 2;
            this.btnCSV.Text = "Export to CSV";
            this.btnCSV.UseVisualStyleBackColor = true;
            this.btnCSV.Click += new System.EventHandler(this.btnCSV_Click);
            // 
            // btnStore
            // 
            this.btnStore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStore.Location = new System.Drawing.Point(925, -1);
            this.btnStore.Name = "btnStore";
            this.btnStore.Size = new System.Drawing.Size(93, 29);
            this.btnStore.TabIndex = 1;
            this.btnStore.Text = "Save Changes";
            this.btnStore.UseVisualStyleBackColor = true;
            this.btnStore.Click += new System.EventHandler(this.btnStore_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(1024, -1);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(93, 29);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // OwnerColumn
            // 
            this.OwnerColumn.HeaderText = "Owner";
            this.OwnerColumn.Name = "OwnerColumn";
            this.OwnerColumn.ReadOnly = true;
            this.OwnerColumn.Width = 120;
            // 
            // ItemColumn
            // 
            this.ItemColumn.HeaderText = "Item";
            this.ItemColumn.Name = "ItemColumn";
            this.ItemColumn.ReadOnly = true;
            this.ItemColumn.Width = 210;
            // 
            // LocationColumn
            // 
            this.LocationColumn.HeaderText = "Location";
            this.LocationColumn.Name = "LocationColumn";
            this.LocationColumn.ReadOnly = true;
            this.LocationColumn.Width = 240;
            // 
            // QuantityColumn
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.QuantityColumn.DefaultCellStyle = dataGridViewCellStyle1;
            this.QuantityColumn.HeaderText = "Quantity";
            this.QuantityColumn.Name = "QuantityColumn";
            this.QuantityColumn.ReadOnly = true;
            this.QuantityColumn.Width = 80;
            // 
            // StatusColumn
            // 
            this.StatusColumn.HeaderText = "Status";
            this.StatusColumn.Name = "StatusColumn";
            this.StatusColumn.ReadOnly = true;
            this.StatusColumn.Width = 70;
            // 
            // AutoConExcludeColumn
            // 
            this.AutoConExcludeColumn.HeaderText = "Auto-contractor Exclude";
            this.AutoConExcludeColumn.Name = "AutoConExcludeColumn";
            // 
            // ReprocessorExcludeColumn
            // 
            this.ReprocessorExcludeColumn.HeaderText = "Reprocessor Exclude";
            this.ReprocessorExcludeColumn.Name = "ReprocessorExcludeColumn";
            this.ReprocessorExcludeColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.ReprocessorExcludeColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // CostColumn
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.CostColumn.DefaultCellStyle = dataGridViewCellStyle2;
            this.CostColumn.HeaderText = "Unit Cost";
            this.CostColumn.Name = "CostColumn";
            this.CostColumn.Width = 150;
            // 
            // ViewAssets
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1123, 437);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ViewAssets";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Assets";
            this.Load += new System.EventHandler(this.ViewAssets_Load);
            ((System.ComponentModel.ISupportInitialize)(this.AssetsGrid)).EndInit();
            this.GridContextMenu.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private EveMarketMonitorApp.Common.MultisortDataGridView AssetsGrid;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cmbItem;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkIngoreOwner;
        private System.Windows.Forms.ComboBox cmbOwner;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView assetsTree;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnStore;
        private System.Windows.Forms.ContextMenuStrip GridContextMenu;
        private System.Windows.Forms.ToolStripMenuItem copyCellDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyRowDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyCellTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyRowTextToolStripMenuItem;
        private System.Windows.Forms.Button btnCSV;
        private System.Windows.Forms.DataGridViewTextBoxColumn OwnerColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ItemColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn LocationColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn QuantityColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn StatusColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn AutoConExcludeColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ReprocessorExcludeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn CostColumn;
    }
}