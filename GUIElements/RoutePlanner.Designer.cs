namespace EveMarketMonitorApp.GUIElements
{
    partial class RoutePlanner
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RoutePlanner));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btnClear = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.lstRoute = new System.Windows.Forms.ListBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnAutopilotSettings = new System.Windows.Forms.Button();
            this.btnGenRoute = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtMinQuantity = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbItem = new System.Windows.Forms.ComboBox();
            this.chkExcludeContainers = new System.Windows.Forms.CheckBox();
            this.chkHighSecAssetsOnly = new System.Windows.Forms.CheckBox();
            this.cmbLocation = new System.Windows.Forms.ComboBox();
            this.btnAddAssets = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbOwner = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.lstWaypoints = new System.Windows.Forms.ListBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.txtSystem = new System.Windows.Forms.TextBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtStartSystem = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtEndSystem = new System.Windows.Forms.TextBox();
            this.panel5 = new System.Windows.Forms.Panel();
            this.lblRouteInfo = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel5.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox1, 2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(826, 64);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Info";
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(820, 45);
            this.label1.TabIndex = 0;
            this.label1.Text = resources.GetString("label1.Text");
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.panel4, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lstRoute, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 10);
            this.tableLayoutPanel1.Controls.Add(this.panel3, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.lstWaypoints, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtStartSystem, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label8, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.txtEndSystem, 0, 9);
            this.tableLayoutPanel1.Controls.Add(this.panel5, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 11;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(832, 659);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.btnClear);
            this.panel4.Controls.Add(this.label3);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(3, 157);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(410, 20);
            this.panel4.TabIndex = 3;
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.Location = new System.Drawing.Point(348, 0);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(62, 21);
            this.btnClear.TabIndex = 3;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(0, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Waypoints";
            // 
            // lstRoute
            // 
            this.lstRoute.ContextMenuStrip = this.contextMenuStrip1;
            this.lstRoute.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstRoute.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstRoute.FormattingEnabled = true;
            this.lstRoute.Location = new System.Drawing.Point(419, 89);
            this.lstRoute.Name = "lstRoute";
            this.tableLayoutPanel1.SetRowSpan(this.lstRoute, 8);
            this.lstRoute.Size = new System.Drawing.Size(410, 524);
            this.lstRoute.TabIndex = 1;
            this.lstRoute.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lstRoute_DrawItem);
            this.lstRoute.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.lstRoute_MeasureItem);
            // 
            // panel1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.panel1, 2);
            this.panel1.Controls.Add(this.btnAutopilotSettings);
            this.panel1.Controls.Add(this.btnGenRoute);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 620);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(826, 36);
            this.panel1.TabIndex = 5;
            // 
            // btnAutopilotSettings
            // 
            this.btnAutopilotSettings.Location = new System.Drawing.Point(113, 4);
            this.btnAutopilotSettings.Name = "btnAutopilotSettings";
            this.btnAutopilotSettings.Size = new System.Drawing.Size(104, 29);
            this.btnAutopilotSettings.TabIndex = 2;
            this.btnAutopilotSettings.Text = "Autopilot Settings";
            this.btnAutopilotSettings.UseVisualStyleBackColor = true;
            this.btnAutopilotSettings.Click += new System.EventHandler(this.btnAutopilotSettings_Click);
            // 
            // btnGenRoute
            // 
            this.btnGenRoute.Location = new System.Drawing.Point(3, 4);
            this.btnGenRoute.Name = "btnGenRoute";
            this.btnGenRoute.Size = new System.Drawing.Size(104, 29);
            this.btnGenRoute.TabIndex = 1;
            this.btnGenRoute.Text = "Generate Route";
            this.btnGenRoute.UseVisualStyleBackColor = true;
            this.btnGenRoute.Click += new System.EventHandler(this.btnGenRoute_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(719, 4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(104, 29);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.groupBox2);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 375);
            this.panel3.Margin = new System.Windows.Forms.Padding(0);
            this.panel3.Name = "panel3";
            this.panel3.Padding = new System.Windows.Forms.Padding(3);
            this.panel3.Size = new System.Drawing.Size(416, 200);
            this.panel3.TabIndex = 9;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.txtMinQuantity);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.cmbItem);
            this.groupBox2.Controls.Add(this.chkExcludeContainers);
            this.groupBox2.Controls.Add(this.chkHighSecAssetsOnly);
            this.groupBox2.Controls.Add(this.cmbLocation);
            this.groupBox2.Controls.Add(this.btnAddAssets);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.cmbOwner);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(3, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(410, 194);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Add waypoints from assets ";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 149);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(64, 13);
            this.label9.TabIndex = 19;
            this.label9.Text = "Min quantity";
            // 
            // txtMinQuantity
            // 
            this.txtMinQuantity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMinQuantity.Location = new System.Drawing.Point(127, 146);
            this.txtMinQuantity.Name = "txtMinQuantity";
            this.txtMinQuantity.Size = new System.Drawing.Size(277, 20);
            this.txtMinQuantity.TabIndex = 18;
            this.txtMinQuantity.Leave += new System.EventHandler(this.txtMinQuantity_Leave);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 122);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "Item Name";
            // 
            // cmbItem
            // 
            this.cmbItem.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbItem.FormattingEnabled = true;
            this.cmbItem.Location = new System.Drawing.Point(127, 119);
            this.cmbItem.Name = "cmbItem";
            this.cmbItem.Size = new System.Drawing.Size(277, 21);
            this.cmbItem.TabIndex = 16;
            // 
            // chkExcludeContainers
            // 
            this.chkExcludeContainers.AutoSize = true;
            this.chkExcludeContainers.Location = new System.Drawing.Point(9, 42);
            this.chkExcludeContainers.Name = "chkExcludeContainers";
            this.chkExcludeContainers.Size = new System.Drawing.Size(333, 17);
            this.chkExcludeContainers.TabIndex = 15;
            this.chkExcludeContainers.Text = "Exclude unpackaged ships, fittings, containers and thier contents";
            this.chkExcludeContainers.UseVisualStyleBackColor = true;
            // 
            // chkHighSecAssetsOnly
            // 
            this.chkHighSecAssetsOnly.AutoSize = true;
            this.chkHighSecAssetsOnly.Location = new System.Drawing.Point(9, 19);
            this.chkHighSecAssetsOnly.Name = "chkHighSecAssetsOnly";
            this.chkHighSecAssetsOnly.Size = new System.Drawing.Size(209, 17);
            this.chkHighSecAssetsOnly.TabIndex = 14;
            this.chkHighSecAssetsOnly.Text = "Only add waypoints for high-sec assets";
            this.chkHighSecAssetsOnly.UseVisualStyleBackColor = true;
            // 
            // cmbLocation
            // 
            this.cmbLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbLocation.FormattingEnabled = true;
            this.cmbLocation.Location = new System.Drawing.Point(127, 65);
            this.cmbLocation.Name = "cmbLocation";
            this.cmbLocation.Size = new System.Drawing.Size(277, 21);
            this.cmbLocation.TabIndex = 0;
            // 
            // btnAddAssets
            // 
            this.btnAddAssets.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddAssets.Location = new System.Drawing.Point(342, 170);
            this.btnAddAssets.Name = "btnAddAssets";
            this.btnAddAssets.Size = new System.Drawing.Size(62, 21);
            this.btnAddAssets.TabIndex = 1;
            this.btnAddAssets.Text = "Add WPs";
            this.toolTip1.SetToolTip(this.btnAddAssets, "Add all systems that contain assets and are in the given location to the list of " +
                    "waypoints.");
            this.btnAddAssets.UseVisualStyleBackColor = true;
            this.btnAddAssets.Click += new System.EventHandler(this.btnAddAssets_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 68);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(48, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Location";
            // 
            // cmbOwner
            // 
            this.cmbOwner.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbOwner.FormattingEnabled = true;
            this.cmbOwner.Location = new System.Drawing.Point(127, 92);
            this.cmbOwner.Name = "cmbOwner";
            this.cmbOwner.Size = new System.Drawing.Size(277, 21);
            this.cmbOwner.TabIndex = 13;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 95);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(115, 13);
            this.label7.TabIndex = 12;
            this.label7.Text = "Owner of assets to use";
            // 
            // lstWaypoints
            // 
            this.lstWaypoints.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstWaypoints.FormattingEnabled = true;
            this.lstWaypoints.Location = new System.Drawing.Point(3, 183);
            this.lstWaypoints.Name = "lstWaypoints";
            this.lstWaypoints.Size = new System.Drawing.Size(410, 186);
            this.lstWaypoints.Sorted = true;
            this.lstWaypoints.TabIndex = 2;
            this.lstWaypoints.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lstWaypoints_KeyDown);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.txtSystem);
            this.panel2.Controls.Add(this.btnAdd);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 128);
            this.panel2.Margin = new System.Windows.Forms.Padding(0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(416, 26);
            this.panel2.TabIndex = 7;
            // 
            // txtSystem
            // 
            this.txtSystem.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSystem.Location = new System.Drawing.Point(3, 4);
            this.txtSystem.Name = "txtSystem";
            this.txtSystem.Size = new System.Drawing.Size(342, 20);
            this.txtSystem.TabIndex = 2;
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdd.Location = new System.Drawing.Point(351, 3);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(62, 21);
            this.btnAdd.TabIndex = 1;
            this.btnAdd.Text = "Add WP";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 112);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "System";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 70);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(66, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Start System";
            // 
            // txtStartSystem
            // 
            this.txtStartSystem.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtStartSystem.Location = new System.Drawing.Point(3, 89);
            this.txtStartSystem.Name = "txtStartSystem";
            this.txtStartSystem.Size = new System.Drawing.Size(410, 20);
            this.txtStartSystem.TabIndex = 11;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 575);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(63, 13);
            this.label8.TabIndex = 12;
            this.label8.Text = "End System";
            // 
            // txtEndSystem
            // 
            this.txtEndSystem.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtEndSystem.Location = new System.Drawing.Point(3, 594);
            this.txtEndSystem.Name = "txtEndSystem";
            this.txtEndSystem.Size = new System.Drawing.Size(410, 20);
            this.txtEndSystem.TabIndex = 13;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.lblRouteInfo);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel5.Location = new System.Drawing.Point(416, 70);
            this.panel5.Margin = new System.Windows.Forms.Padding(0);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(416, 16);
            this.panel5.TabIndex = 14;
            // 
            // lblRouteInfo
            // 
            this.lblRouteInfo.AutoSize = true;
            this.lblRouteInfo.Location = new System.Drawing.Point(3, 0);
            this.lblRouteInfo.Name = "lblRouteInfo";
            this.lblRouteInfo.Size = new System.Drawing.Size(36, 13);
            this.lblRouteInfo.TabIndex = 0;
            this.lblRouteInfo.Text = "Route";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(328, 26);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(327, 22);
            this.removeToolStripMenuItem.Text = "Remove waypoints before this system in the route";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // RoutePlanner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(832, 659);
            this.Controls.Add(this.tableLayoutPanel1);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RoutePlanner";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Route Planner";
            this.Load += new System.EventHandler(this.RoutePlanner_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RoutePlanner_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox lstWaypoints;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnAddAssets;
        private System.Windows.Forms.ComboBox cmbLocation;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnGenRoute;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cmbOwner;
        private System.Windows.Forms.ListBox lstRoute;
        private System.Windows.Forms.Button btnAutopilotSettings;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkHighSecAssetsOnly;
        private System.Windows.Forms.TextBox txtStartSystem;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtEndSystem;
        private System.Windows.Forms.TextBox txtSystem;
        private System.Windows.Forms.CheckBox chkExcludeContainers;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbItem;
        private System.Windows.Forms.TextBox txtMinQuantity;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Label lblRouteInfo;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
    }
}