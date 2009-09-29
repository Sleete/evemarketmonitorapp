namespace EveMarketMonitorApp.GUIElements
{
    partial class DeliveryPlanner
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeliveryPlanner));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btnAddCargo = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.lstRoute = new System.Windows.Forms.ListBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnAutopilotSettings = new System.Windows.Forms.Button();
            this.btnGenRoute = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chkExcludeContainers = new System.Windows.Forms.CheckBox();
            this.chkHighSecAssetsOnly = new System.Windows.Forms.CheckBox();
            this.cmbLocation = new System.Windows.Forms.ComboBox();
            this.btnAddAssets = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbOwner = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lstCargoHold = new System.Windows.Forms.ListBox();
            this.btnAddContainer = new System.Windows.Forms.Button();
            this.txtVolume = new System.Windows.Forms.TextBox();
            this.cargoDataView = new System.Windows.Forms.DataGridView();
            this.cargoPickupColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cargoDestinationColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cargoVolumeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label8 = new System.Windows.Forms.Label();
            this.txtStartSystem = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnClearCotnainers = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cargoDataView)).BeginInit();
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
            this.groupBox1.Size = new System.Drawing.Size(838, 64);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Info";
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(832, 45);
            this.label1.TabIndex = 0;
            this.label1.Text = resources.GetString("label1.Text");
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.panel4, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lstRoute, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.panel3, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.groupBox3, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.cargoDataView, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.label8, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtStartSystem, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label2, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 8;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(844, 556);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.btnAddCargo);
            this.panel4.Controls.Add(this.btnClear);
            this.panel4.Controls.Add(this.label3);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(3, 115);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(416, 20);
            this.panel4.TabIndex = 3;
            // 
            // btnAddCargo
            // 
            this.btnAddCargo.Location = new System.Drawing.Point(290, 0);
            this.btnAddCargo.Name = "btnAddCargo";
            this.btnAddCargo.Size = new System.Drawing.Size(58, 21);
            this.btnAddCargo.TabIndex = 1;
            this.btnAddCargo.Text = "Add";
            this.btnAddCargo.UseVisualStyleBackColor = true;
            this.btnAddCargo.Click += new System.EventHandler(this.btnAddCargo_Click);
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.Location = new System.Drawing.Point(354, 0);
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
            this.label3.Size = new System.Drawing.Size(120, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Cargo pickup waypoints";
            // 
            // lstRoute
            // 
            this.lstRoute.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstRoute.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstRoute.FormattingEnabled = true;
            this.lstRoute.Location = new System.Drawing.Point(425, 89);
            this.lstRoute.Name = "lstRoute";
            this.tableLayoutPanel1.SetRowSpan(this.lstRoute, 5);
            this.lstRoute.Size = new System.Drawing.Size(416, 420);
            this.lstRoute.TabIndex = 1;
            // 
            // panel1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.panel1, 2);
            this.panel1.Controls.Add(this.btnAutopilotSettings);
            this.panel1.Controls.Add(this.btnGenRoute);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 517);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(838, 36);
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
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(731, 4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(104, 29);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.groupBox2);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 246);
            this.panel3.Margin = new System.Windows.Forms.Padding(0);
            this.panel3.Name = "panel3";
            this.panel3.Padding = new System.Windows.Forms.Padding(3);
            this.panel3.Size = new System.Drawing.Size(422, 160);
            this.panel3.TabIndex = 9;
            // 
            // groupBox2
            // 
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
            this.groupBox2.Size = new System.Drawing.Size(416, 154);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Add waypoints from assets ";
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
            this.cmbLocation.Location = new System.Drawing.Point(6, 87);
            this.cmbLocation.Name = "cmbLocation";
            this.cmbLocation.Size = new System.Drawing.Size(333, 21);
            this.cmbLocation.TabIndex = 0;
            // 
            // btnAddAssets
            // 
            this.btnAddAssets.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddAssets.Location = new System.Drawing.Point(348, 86);
            this.btnAddAssets.Name = "btnAddAssets";
            this.btnAddAssets.Size = new System.Drawing.Size(62, 21);
            this.btnAddAssets.TabIndex = 1;
            this.btnAddAssets.Text = "Add WPs";
            this.toolTip1.SetToolTip(this.btnAddAssets, "Add all systems that contain assets and are in the given location to the list of " +
                    "waypoints.");
            this.btnAddAssets.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 71);
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
            this.cmbOwner.Location = new System.Drawing.Point(6, 127);
            this.cmbOwner.Name = "cmbOwner";
            this.cmbOwner.Size = new System.Drawing.Size(404, 21);
            this.cmbOwner.TabIndex = 13;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 111);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(115, 13);
            this.label7.TabIndex = 12;
            this.label7.Text = "Owner of assets to use";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnClearCotnainers);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.lstCargoHold);
            this.groupBox3.Controls.Add(this.btnAddContainer);
            this.groupBox3.Controls.Add(this.txtVolume);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(3, 409);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(416, 102);
            this.groupBox3.TabIndex = 15;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Cargo hold details";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(242, 23);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(21, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "m3";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(42, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Volume";
            // 
            // lstCargoHold
            // 
            this.lstCargoHold.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstCargoHold.FormattingEnabled = true;
            this.lstCargoHold.Location = new System.Drawing.Point(6, 45);
            this.lstCargoHold.Name = "lstCargoHold";
            this.lstCargoHold.Size = new System.Drawing.Size(404, 43);
            this.lstCargoHold.TabIndex = 2;
            // 
            // btnAddContainer
            // 
            this.btnAddContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddContainer.Location = new System.Drawing.Point(280, 18);
            this.btnAddContainer.Name = "btnAddContainer";
            this.btnAddContainer.Size = new System.Drawing.Size(62, 21);
            this.btnAddContainer.TabIndex = 1;
            this.btnAddContainer.Text = "Add";
            this.btnAddContainer.UseVisualStyleBackColor = true;
            this.btnAddContainer.Click += new System.EventHandler(this.btnAddContainer_Click);
            // 
            // txtVolume
            // 
            this.txtVolume.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtVolume.Location = new System.Drawing.Point(75, 19);
            this.txtVolume.Name = "txtVolume";
            this.txtVolume.Size = new System.Drawing.Size(161, 20);
            this.txtVolume.TabIndex = 0;
            // 
            // cargoDataView
            // 
            this.cargoDataView.AllowUserToAddRows = false;
            this.cargoDataView.AllowUserToDeleteRows = false;
            this.cargoDataView.AllowUserToResizeRows = false;
            this.cargoDataView.BackgroundColor = System.Drawing.Color.White;
            this.cargoDataView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.cargoDataView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.cargoDataView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.cargoDataView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.cargoDataView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.cargoPickupColumn,
            this.cargoDestinationColumn,
            this.cargoVolumeColumn});
            this.cargoDataView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cargoDataView.EnableHeadersVisualStyles = false;
            this.cargoDataView.GridColor = System.Drawing.Color.DarkGray;
            this.cargoDataView.Location = new System.Drawing.Point(3, 141);
            this.cargoDataView.Name = "cargoDataView";
            this.cargoDataView.ReadOnly = true;
            this.cargoDataView.RowHeadersVisible = false;
            this.cargoDataView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.cargoDataView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.cargoDataView.Size = new System.Drawing.Size(416, 102);
            this.cargoDataView.TabIndex = 16;
            this.cargoDataView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cargoDataView_KeyDown);
            // 
            // cargoPickupColumn
            // 
            this.cargoPickupColumn.HeaderText = "Pickup";
            this.cargoPickupColumn.Name = "cargoPickupColumn";
            this.cargoPickupColumn.ReadOnly = true;
            this.cargoPickupColumn.Width = 140;
            // 
            // cargoDestinationColumn
            // 
            this.cargoDestinationColumn.HeaderText = "Destination";
            this.cargoDestinationColumn.Name = "cargoDestinationColumn";
            this.cargoDestinationColumn.ReadOnly = true;
            this.cargoDestinationColumn.Width = 140;
            // 
            // cargoVolumeColumn
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.cargoVolumeColumn.DefaultCellStyle = dataGridViewCellStyle2;
            this.cargoVolumeColumn.HeaderText = "Total Volume";
            this.cargoVolumeColumn.Name = "cargoVolumeColumn";
            this.cargoVolumeColumn.ReadOnly = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 70);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(64, 13);
            this.label8.TabIndex = 17;
            this.label8.Text = "Start system";
            // 
            // txtStartSystem
            // 
            this.txtStartSystem.Location = new System.Drawing.Point(3, 89);
            this.txtStartSystem.Name = "txtStartSystem";
            this.txtStartSystem.Size = new System.Drawing.Size(416, 20);
            this.txtStartSystem.TabIndex = 18;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(425, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 19;
            this.label2.Text = "Route";
            // 
            // btnClearCotnainers
            // 
            this.btnClearCotnainers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearCotnainers.Location = new System.Drawing.Point(348, 18);
            this.btnClearCotnainers.Name = "btnClearCotnainers";
            this.btnClearCotnainers.Size = new System.Drawing.Size(62, 21);
            this.btnClearCotnainers.TabIndex = 5;
            this.btnClearCotnainers.Text = "Clear";
            this.btnClearCotnainers.UseVisualStyleBackColor = true;
            this.btnClearCotnainers.Click += new System.EventHandler(this.btnClearCotnainers_Click);
            // 
            // DeliveryPlanner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(844, 556);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DeliveryPlanner";
            this.Text = "Delivery Planner";
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cargoDataView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnAddAssets;
        private System.Windows.Forms.ComboBox cmbLocation;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnGenRoute;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cmbOwner;
        private System.Windows.Forms.Button btnAutopilotSettings;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox chkHighSecAssetsOnly;
        private System.Windows.Forms.CheckBox chkExcludeContainers;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.ListBox lstRoute;
        private System.Windows.Forms.Button btnAddCargo;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox lstCargoHold;
        private System.Windows.Forms.Button btnAddContainer;
        private System.Windows.Forms.TextBox txtVolume;
        private System.Windows.Forms.DataGridView cargoDataView;
        private System.Windows.Forms.DataGridViewTextBoxColumn cargoPickupColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn cargoDestinationColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn cargoVolumeColumn;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtStartSystem;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnClearCotnainers;
    }
}