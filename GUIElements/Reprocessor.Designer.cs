namespace EveMarketMonitorApp.GUIElements
{
    partial class Reprocessor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Reprocessor));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.ReprocessResultsView = new System.Windows.Forms.DataGridView();
            this.ResultItemColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ResultMaxQuantityColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ResultActualQuantityColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StationTakesQuantityColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ResultFinalQuantityColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ResultValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ResultTotalValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ItemsToReprocessView = new System.Windows.Forms.DataGridView();
            this.ItemColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.QuantityColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UnitValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TotalValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ReprocessValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ReprocessColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cmbContainers = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbDefaultReprocessor = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtStation = new System.Windows.Forms.TextBox();
            this.btnReprocess = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.lblValueBefore = new System.Windows.Forms.Label();
            this.lblValueAfter = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.btnToggleReprocess = new System.Windows.Forms.Button();
            this.btnCompleteReprocess = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnHistory = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ReprocessResultsView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ItemsToReprocessView)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1145, 69);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Information";
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(1139, 50);
            this.label2.TabIndex = 0;
            this.label2.Text = resources.GetString("label2.Text");
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(93, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Items to reprocess";
            // 
            // ReprocessResultsView
            // 
            this.ReprocessResultsView.AllowUserToAddRows = false;
            this.ReprocessResultsView.AllowUserToDeleteRows = false;
            this.ReprocessResultsView.AllowUserToResizeRows = false;
            this.ReprocessResultsView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ReprocessResultsView.BackgroundColor = System.Drawing.Color.White;
            this.ReprocessResultsView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ReprocessResultsView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.ReprocessResultsView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ReprocessResultsView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ResultItemColumn,
            this.ResultMaxQuantityColumn,
            this.ResultActualQuantityColumn,
            this.StationTakesQuantityColumn,
            this.ResultFinalQuantityColumn,
            this.ResultValueColumn,
            this.ResultTotalValueColumn});
            this.ReprocessResultsView.Location = new System.Drawing.Point(3, 16);
            this.ReprocessResultsView.Name = "ReprocessResultsView";
            this.ReprocessResultsView.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.ReprocessResultsView.RowHeadersVisible = false;
            this.ReprocessResultsView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.ReprocessResultsView.RowTemplate.Height = 20;
            this.ReprocessResultsView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.ReprocessResultsView.Size = new System.Drawing.Size(560, 246);
            this.ReprocessResultsView.TabIndex = 11;
            this.ReprocessResultsView.CellParsing += new System.Windows.Forms.DataGridViewCellParsingEventHandler(this.ReprocessResultsView_CellParsing);
            this.ReprocessResultsView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.ReprocessResultsView_CellEndEdit);
            // 
            // ResultItemColumn
            // 
            this.ResultItemColumn.HeaderText = "Item";
            this.ResultItemColumn.Name = "ResultItemColumn";
            this.ResultItemColumn.ReadOnly = true;
            this.ResultItemColumn.Width = 180;
            // 
            // ResultMaxQuantityColumn
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.ResultMaxQuantityColumn.DefaultCellStyle = dataGridViewCellStyle1;
            this.ResultMaxQuantityColumn.HeaderText = "Max Quantity";
            this.ResultMaxQuantityColumn.Name = "ResultMaxQuantityColumn";
            this.ResultMaxQuantityColumn.ReadOnly = true;
            // 
            // ResultActualQuantityColumn
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.ResultActualQuantityColumn.DefaultCellStyle = dataGridViewCellStyle2;
            this.ResultActualQuantityColumn.HeaderText = "Actual Quantity";
            this.ResultActualQuantityColumn.Name = "ResultActualQuantityColumn";
            this.ResultActualQuantityColumn.ReadOnly = true;
            this.ResultActualQuantityColumn.Width = 105;
            // 
            // StationTakesQuantityColumn
            // 
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.StationTakesQuantityColumn.DefaultCellStyle = dataGridViewCellStyle3;
            this.StationTakesQuantityColumn.HeaderText = "Station Takes";
            this.StationTakesQuantityColumn.Name = "StationTakesQuantityColumn";
            this.StationTakesQuantityColumn.ReadOnly = true;
            // 
            // ResultFinalQuantityColumn
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.ResultFinalQuantityColumn.DefaultCellStyle = dataGridViewCellStyle4;
            this.ResultFinalQuantityColumn.HeaderText = "Final Quantity";
            this.ResultFinalQuantityColumn.Name = "ResultFinalQuantityColumn";
            this.ResultFinalQuantityColumn.ReadOnly = true;
            // 
            // ResultValueColumn
            // 
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.ResultValueColumn.DefaultCellStyle = dataGridViewCellStyle5;
            this.ResultValueColumn.HeaderText = "Unit Value";
            this.ResultValueColumn.Name = "ResultValueColumn";
            // 
            // ResultTotalValueColumn
            // 
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.ResultTotalValueColumn.DefaultCellStyle = dataGridViewCellStyle6;
            this.ResultTotalValueColumn.HeaderText = "Total Value";
            this.ResultTotalValueColumn.Name = "ResultTotalValueColumn";
            this.ResultTotalValueColumn.ReadOnly = true;
            this.ResultTotalValueColumn.Width = 140;
            // 
            // ItemsToReprocessView
            // 
            this.ItemsToReprocessView.AllowUserToAddRows = false;
            this.ItemsToReprocessView.AllowUserToDeleteRows = false;
            this.ItemsToReprocessView.AllowUserToResizeRows = false;
            this.ItemsToReprocessView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ItemsToReprocessView.BackgroundColor = System.Drawing.Color.White;
            this.ItemsToReprocessView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ItemsToReprocessView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.ItemsToReprocessView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ItemsToReprocessView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ItemColumn,
            this.QuantityColumn,
            this.UnitValueColumn,
            this.TotalValueColumn,
            this.ReprocessValueColumn,
            this.ReprocessColumn});
            this.ItemsToReprocessView.Location = new System.Drawing.Point(3, 16);
            this.ItemsToReprocessView.Name = "ItemsToReprocessView";
            this.ItemsToReprocessView.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.ItemsToReprocessView.RowHeadersVisible = false;
            this.ItemsToReprocessView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.ItemsToReprocessView.RowTemplate.Height = 20;
            this.ItemsToReprocessView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.ItemsToReprocessView.Size = new System.Drawing.Size(566, 246);
            this.ItemsToReprocessView.TabIndex = 12;
            this.ItemsToReprocessView.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.ItemsToReprocessView_CellBeginEdit);
            this.ItemsToReprocessView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.ItemsToReprocessView_CellFormatting);
            this.ItemsToReprocessView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.ItemsToReprocessView_CellEndEdit);
            this.ItemsToReprocessView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.ItemsToReprocessView_CellContentClick);
            // 
            // ItemColumn
            // 
            this.ItemColumn.HeaderText = "Item";
            this.ItemColumn.Name = "ItemColumn";
            this.ItemColumn.ReadOnly = true;
            this.ItemColumn.Width = 180;
            // 
            // QuantityColumn
            // 
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.QuantityColumn.DefaultCellStyle = dataGridViewCellStyle7;
            this.QuantityColumn.HeaderText = "Quantity";
            this.QuantityColumn.Name = "QuantityColumn";
            this.QuantityColumn.Width = 80;
            // 
            // UnitValueColumn
            // 
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.UnitValueColumn.DefaultCellStyle = dataGridViewCellStyle8;
            this.UnitValueColumn.HeaderText = "Unit Value";
            this.UnitValueColumn.Name = "UnitValueColumn";
            this.UnitValueColumn.ReadOnly = true;
            this.UnitValueColumn.Width = 140;
            // 
            // TotalValueColumn
            // 
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.TotalValueColumn.DefaultCellStyle = dataGridViewCellStyle9;
            this.TotalValueColumn.HeaderText = "Total Value";
            this.TotalValueColumn.Name = "TotalValueColumn";
            this.TotalValueColumn.ReadOnly = true;
            this.TotalValueColumn.Width = 140;
            // 
            // ReprocessValueColumn
            // 
            dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.ReprocessValueColumn.DefaultCellStyle = dataGridViewCellStyle10;
            this.ReprocessValueColumn.HeaderText = "Reproc. Value";
            this.ReprocessValueColumn.Name = "ReprocessValueColumn";
            this.ReprocessValueColumn.ReadOnly = true;
            this.ReprocessValueColumn.Width = 140;
            // 
            // ReprocessColumn
            // 
            this.ReprocessColumn.HeaderText = "Reprocess";
            this.ReprocessColumn.Name = "ReprocessColumn";
            this.ReprocessColumn.Width = 80;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.splitContainer1);
            this.groupBox3.Location = new System.Drawing.Point(12, 196);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(1145, 284);
            this.groupBox3.TabIndex = 13;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Details";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 16);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.ItemsToReprocessView);
            this.splitContainer1.Panel1.Controls.Add(this.label6);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.ReprocessResultsView);
            this.splitContainer1.Panel2.Controls.Add(this.label7);
            this.splitContainer1.Size = new System.Drawing.Size(1139, 265);
            this.splitContainer1.SplitterDistance = 572;
            this.splitContainer1.TabIndex = 14;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(91, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "Reprocess results";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.cmbContainers);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.cmbDefaultReprocessor);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.txtStation);
            this.groupBox2.Location = new System.Drawing.Point(12, 87);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1145, 103);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Filters";
            // 
            // cmbContainers
            // 
            this.cmbContainers.FormattingEnabled = true;
            this.cmbContainers.Location = new System.Drawing.Point(90, 72);
            this.cmbContainers.Name = "cmbContainers";
            this.cmbContainers.Size = new System.Drawing.Size(260, 21);
            this.cmbContainers.Sorted = true;
            this.cmbContainers.TabIndex = 25;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 75);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(52, 13);
            this.label9.TabIndex = 24;
            this.label9.Text = "Container";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.Location = new System.Drawing.Point(356, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(783, 18);
            this.label4.TabIndex = 23;
            this.label4.Text = "Note. These filters only define what items appear in the list below. To set skill" +
                "s, how much the station takes, etc, use the \'Reprocessor Settings\' button.";
            // 
            // cmbDefaultReprocessor
            // 
            this.cmbDefaultReprocessor.FormattingEnabled = true;
            this.cmbDefaultReprocessor.Location = new System.Drawing.Point(90, 45);
            this.cmbDefaultReprocessor.Name = "cmbDefaultReprocessor";
            this.cmbDefaultReprocessor.Size = new System.Drawing.Size(260, 21);
            this.cmbDefaultReprocessor.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 22;
            this.label1.Text = "Character";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Station";
            // 
            // txtStation
            // 
            this.txtStation.Location = new System.Drawing.Point(90, 19);
            this.txtStation.Name = "txtStation";
            this.txtStation.Size = new System.Drawing.Size(662, 20);
            this.txtStation.TabIndex = 0;
            // 
            // btnReprocess
            // 
            this.btnReprocess.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReprocess.Location = new System.Drawing.Point(971, 544);
            this.btnReprocess.Name = "btnReprocess";
            this.btnReprocess.Size = new System.Drawing.Size(90, 35);
            this.btnReprocess.TabIndex = 15;
            this.btnReprocess.Text = "Reprocess";
            this.toolTip1.SetToolTip(this.btnReprocess, "Reprocess the selected items");
            this.btnReprocess.UseVisualStyleBackColor = true;
            this.btnReprocess.Click += new System.EventHandler(this.btnReprocess_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(1067, 544);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 35);
            this.btnCancel.TabIndex = 16;
            this.btnCancel.Text = "Close";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSettings
            // 
            this.btnSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSettings.Location = new System.Drawing.Point(12, 544);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(90, 35);
            this.btnSettings.TabIndex = 17;
            this.btnSettings.Text = "Reprocessor Settings";
            this.toolTip1.SetToolTip(this.btnSettings, "Open the reprocessor settings screen");
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.lblValueBefore);
            this.groupBox4.Controls.Add(this.lblValueAfter);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Location = new System.Drawing.Point(12, 486);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(1145, 52);
            this.groupBox4.TabIndex = 18;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Summary";
            // 
            // lblValueBefore
            // 
            this.lblValueBefore.Location = new System.Drawing.Point(304, 16);
            this.lblValueBefore.Name = "lblValueBefore";
            this.lblValueBefore.Size = new System.Drawing.Size(138, 13);
            this.lblValueBefore.TabIndex = 3;
            this.lblValueBefore.Text = "0.00 ISK";
            this.lblValueBefore.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblValueAfter
            // 
            this.lblValueAfter.Location = new System.Drawing.Point(301, 29);
            this.lblValueAfter.Name = "lblValueAfter";
            this.lblValueAfter.Size = new System.Drawing.Size(141, 13);
            this.lblValueAfter.TabIndex = 2;
            this.lblValueAfter.Text = "100,000,000,000.00 ISK";
            this.lblValueAfter.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 29);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(237, 13);
            this.label8.TabIndex = 1;
            this.label8.Text = "Total estimated value of items after reprocessing:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(289, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Total estimated value of selected items before reprocessing:";
            // 
            // btnToggleReprocess
            // 
            this.btnToggleReprocess.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnToggleReprocess.Location = new System.Drawing.Point(204, 544);
            this.btnToggleReprocess.Name = "btnToggleReprocess";
            this.btnToggleReprocess.Size = new System.Drawing.Size(90, 35);
            this.btnToggleReprocess.TabIndex = 19;
            this.btnToggleReprocess.Text = "Toggle All Items";
            this.toolTip1.SetToolTip(this.btnToggleReprocess, "Toggle all items to reprocess on/off");
            this.btnToggleReprocess.UseVisualStyleBackColor = true;
            this.btnToggleReprocess.Click += new System.EventHandler(this.btnToggleReprocess_Click);
            // 
            // btnCompleteReprocess
            // 
            this.btnCompleteReprocess.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCompleteReprocess.Location = new System.Drawing.Point(875, 544);
            this.btnCompleteReprocess.Name = "btnCompleteReprocess";
            this.btnCompleteReprocess.Size = new System.Drawing.Size(90, 35);
            this.btnCompleteReprocess.TabIndex = 20;
            this.btnCompleteReprocess.Text = "Complete Reprocess";
            this.toolTip1.SetToolTip(this.btnCompleteReprocess, "Reprocess all selected items + fully reprocess the resulting items as well");
            this.btnCompleteReprocess.UseVisualStyleBackColor = true;
            this.btnCompleteReprocess.Visible = false;
            this.btnCompleteReprocess.Click += new System.EventHandler(this.btnCompleteReprocess_Click);
            // 
            // btnHistory
            // 
            this.btnHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnHistory.Location = new System.Drawing.Point(108, 544);
            this.btnHistory.Name = "btnHistory";
            this.btnHistory.Size = new System.Drawing.Size(90, 35);
            this.btnHistory.TabIndex = 21;
            this.btnHistory.Text = "Reprocess History";
            this.toolTip1.SetToolTip(this.btnHistory, "Toggle all items to reprocess on/off");
            this.btnHistory.UseVisualStyleBackColor = true;
            this.btnHistory.Click += new System.EventHandler(this.btnHistory_Click);
            // 
            // Reprocessor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1169, 591);
            this.Controls.Add(this.btnHistory);
            this.Controls.Add(this.btnCompleteReprocess);
            this.Controls.Add(this.btnToggleReprocess);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.btnReprocess);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Reprocessor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Reprocessor";
            this.Load += new System.EventHandler(this.Reprocessor_Load);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ReprocessResultsView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ItemsToReprocessView)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView ItemsToReprocessView;
        private System.Windows.Forms.DataGridView ReprocessResultsView;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ComboBox cmbDefaultReprocessor;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtStation;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnReprocess;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblValueAfter;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lblValueBefore;
        private System.Windows.Forms.Button btnToggleReprocess;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnCompleteReprocess;
        private System.Windows.Forms.DataGridViewTextBoxColumn ResultItemColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ResultMaxQuantityColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ResultActualQuantityColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn StationTakesQuantityColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ResultFinalQuantityColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ResultValueColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ResultTotalValueColumn;
        private System.Windows.Forms.Button btnHistory;
        private System.Windows.Forms.DataGridViewTextBoxColumn ItemColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn QuantityColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn UnitValueColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn TotalValueColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ReprocessValueColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ReprocessColumn;
        private System.Windows.Forms.ComboBox cmbContainers;
        private System.Windows.Forms.Label label9;
    }
}