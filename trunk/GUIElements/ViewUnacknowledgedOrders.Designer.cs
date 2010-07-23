namespace EveMarketMonitorApp.GUIElements
{
    partial class ViewUnacknowledgedOrders
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewUnacknowledgedOrders));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.ordersGrid = new EveMarketMonitorApp.Common.MultisortDataGridView();
            this.DateTimeColmun = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OwnerColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ItemColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PriceColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StationColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TypeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.QuantityColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RangeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DurationColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AcknowledgeColumn = new System.Windows.Forms.DataGridViewImageColumn();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnAcknowledgeAll = new System.Windows.Forms.Button();
            this.icons = new System.Windows.Forms.ImageList(this.components);
            this.btnAckSelected = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ordersGrid)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.ordersGrid, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1144, 333);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // ordersGrid
            // 
            this.ordersGrid.AllowUserToAddRows = false;
            this.ordersGrid.AllowUserToDeleteRows = false;
            this.ordersGrid.AllowUserToResizeRows = false;
            this.ordersGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ordersGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.DateTimeColmun,
            this.OwnerColumn,
            this.ItemColumn,
            this.PriceColumn,
            this.StationColumn,
            this.TypeColumn,
            this.QuantityColumn,
            this.RangeColumn,
            this.DurationColumn,
            this.AcknowledgeColumn});
            this.ordersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ordersGrid.Location = new System.Drawing.Point(3, 83);
            this.ordersGrid.Name = "ordersGrid";
            this.ordersGrid.ReadOnly = true;
            this.ordersGrid.RowHeadersVisible = false;
            this.ordersGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.ordersGrid.Size = new System.Drawing.Size(1138, 205);
            this.ordersGrid.TabIndex = 1;
            this.ordersGrid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.ordersGrid_CellFormatting);
            // 
            // DateTimeColmun
            // 
            this.DateTimeColmun.HeaderText = "Creation Date";
            this.DateTimeColmun.Name = "DateTimeColmun";
            this.DateTimeColmun.ReadOnly = true;
            // 
            // OwnerColumn
            // 
            this.OwnerColumn.HeaderText = "Owner";
            this.OwnerColumn.Name = "OwnerColumn";
            this.OwnerColumn.ReadOnly = true;
            this.OwnerColumn.Width = 140;
            // 
            // ItemColumn
            // 
            this.ItemColumn.HeaderText = "Item";
            this.ItemColumn.Name = "ItemColumn";
            this.ItemColumn.ReadOnly = true;
            this.ItemColumn.Width = 200;
            // 
            // PriceColumn
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.PriceColumn.DefaultCellStyle = dataGridViewCellStyle1;
            this.PriceColumn.HeaderText = "Price";
            this.PriceColumn.Name = "PriceColumn";
            this.PriceColumn.ReadOnly = true;
            this.PriceColumn.Width = 140;
            // 
            // StationColumn
            // 
            this.StationColumn.HeaderText = "Station";
            this.StationColumn.Name = "StationColumn";
            this.StationColumn.ReadOnly = true;
            this.StationColumn.Width = 200;
            // 
            // TypeColumn
            // 
            this.TypeColumn.HeaderText = "Type";
            this.TypeColumn.Name = "TypeColumn";
            this.TypeColumn.ReadOnly = true;
            this.TypeColumn.Width = 80;
            // 
            // QuantityColumn
            // 
            this.QuantityColumn.HeaderText = "Total Units";
            this.QuantityColumn.Name = "QuantityColumn";
            this.QuantityColumn.ReadOnly = true;
            this.QuantityColumn.Width = 90;
            // 
            // RangeColumn
            // 
            this.RangeColumn.HeaderText = "Range";
            this.RangeColumn.Name = "RangeColumn";
            this.RangeColumn.ReadOnly = true;
            this.RangeColumn.Width = 80;
            // 
            // DurationColumn
            // 
            this.DurationColumn.HeaderText = "Duration";
            this.DurationColumn.Name = "DurationColumn";
            this.DurationColumn.ReadOnly = true;
            // 
            // AcknowledgeColumn
            // 
            this.AcknowledgeColumn.HeaderText = "Acknowledge";
            this.AcknowledgeColumn.Name = "AcknowledgeColumn";
            this.AcknowledgeColumn.ReadOnly = true;
            this.AcknowledgeColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.AcknowledgeColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.AcknowledgeColumn.ToolTipText = "Set order status to acknowledged";
            this.AcknowledgeColumn.Width = 80;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1138, 74);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Info";
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(1132, 55);
            this.label1.TabIndex = 3;
            this.label1.Text = resources.GetString("label1.Text");
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnAckSelected);
            this.panel1.Controls.Add(this.btnAcknowledgeAll);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 294);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1138, 36);
            this.panel1.TabIndex = 3;
            // 
            // btnAcknowledgeAll
            // 
            this.btnAcknowledgeAll.Location = new System.Drawing.Point(3, 0);
            this.btnAcknowledgeAll.Name = "btnAcknowledgeAll";
            this.btnAcknowledgeAll.Size = new System.Drawing.Size(98, 35);
            this.btnAcknowledgeAll.TabIndex = 0;
            this.btnAcknowledgeAll.Text = "Acknowledge All";
            this.btnAcknowledgeAll.UseVisualStyleBackColor = true;
            this.btnAcknowledgeAll.Click += new System.EventHandler(this.btnAcknowledgeAll_Click);
            // 
            // icons
            // 
            this.icons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("icons.ImageStream")));
            this.icons.TransparentColor = System.Drawing.Color.Transparent;
            this.icons.Images.SetKeyName(0, "tick.gif");
            // 
            // btnAckSelected
            // 
            this.btnAckSelected.Location = new System.Drawing.Point(107, 0);
            this.btnAckSelected.Name = "btnAckSelected";
            this.btnAckSelected.Size = new System.Drawing.Size(98, 35);
            this.btnAckSelected.TabIndex = 1;
            this.btnAckSelected.Text = "Acknowledge Selected";
            this.btnAckSelected.UseVisualStyleBackColor = true;
            this.btnAckSelected.Click += new System.EventHandler(this.btnAckSelected_Click);
            // 
            // ViewUnacknowledgedOrders
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1144, 333);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ViewUnacknowledgedOrders";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Expired and Completed Orders";
            this.Load += new System.EventHandler(this.ViewUnacknowledgedOrders_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ordersGrid)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private EveMarketMonitorApp.Common.MultisortDataGridView ordersGrid;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ImageList icons;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnAcknowledgeAll;
        private System.Windows.Forms.DataGridViewTextBoxColumn DateTimeColmun;
        private System.Windows.Forms.DataGridViewTextBoxColumn OwnerColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ItemColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn PriceColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn StationColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn TypeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn QuantityColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn RangeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DurationColumn;
        private System.Windows.Forms.DataGridViewImageColumn AcknowledgeColumn;
        private System.Windows.Forms.Button btnAckSelected;
    }
}