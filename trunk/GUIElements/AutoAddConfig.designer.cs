namespace EveMarketMonitorApp.GUIElements
{
    partial class AutoAddConfig
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoAddConfig));
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.lblpart1 = new System.Windows.Forms.Label();
            this.txtMinRequired = new System.Windows.Forms.TextBox();
            this.txtMinSales = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtMinPurchases = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.dtpTransactionStartDate = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.lstBuyStations = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnClearBuyStations = new System.Windows.Forms.Button();
            this.btnAddBuyStation = new System.Windows.Forms.Button();
            this.txtBuyStation = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnAddSellStation = new System.Windows.Forms.Button();
            this.btnClearSellStations = new System.Windows.Forms.Button();
            this.lstSellStations = new System.Windows.Forms.ListBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtSellStation = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(483, 2);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(88, 29);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(577, 2);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 29);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // groupBox1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox1, 2);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(668, 94);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Information";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.Location = new System.Drawing.Point(6, 16);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(656, 75);
            this.label6.TabIndex = 12;
            this.label6.Text = resources.GetString("label6.Text");
            this.label6.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblpart1
            // 
            this.lblpart1.AutoSize = true;
            this.lblpart1.Location = new System.Drawing.Point(3, 32);
            this.lblpart1.Name = "lblpart1";
            this.lblpart1.Size = new System.Drawing.Size(188, 13);
            this.lblpart1.TabIndex = 4;
            this.lblpart1.Text = "Minimum total item transaction quantity";
            // 
            // txtMinRequired
            // 
            this.txtMinRequired.Location = new System.Drawing.Point(213, 29);
            this.txtMinRequired.Name = "txtMinRequired";
            this.txtMinRequired.Size = new System.Drawing.Size(58, 20);
            this.txtMinRequired.TabIndex = 1;
            this.txtMinRequired.TextChanged += new System.EventHandler(this.txtMinRequired_TextChanged);
            // 
            // txtMinSales
            // 
            this.txtMinSales.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMinSales.Location = new System.Drawing.Point(267, 19);
            this.txtMinSales.Name = "txtMinSales";
            this.txtMinSales.Size = new System.Drawing.Size(58, 20);
            this.txtMinSales.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(151, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Minimum total item sell quantity";
            // 
            // txtMinPurchases
            // 
            this.txtMinPurchases.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMinPurchases.Location = new System.Drawing.Point(267, 15);
            this.txtMinPurchases.Name = "txtMinPurchases";
            this.txtMinPurchases.Size = new System.Drawing.Size(58, 20);
            this.txtMinPurchases.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(153, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Minimum total item buy quantity";
            // 
            // dtpTransactionStartDate
            // 
            this.dtpTransactionStartDate.CustomFormat = "dd-MMM-yyyy HH:mm:ss";
            this.dtpTransactionStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpTransactionStartDate.Location = new System.Drawing.Point(213, 7);
            this.dtpTransactionStartDate.Name = "dtpTransactionStartDate";
            this.dtpTransactionStartDate.Size = new System.Drawing.Size(149, 20);
            this.dtpTransactionStartDate.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 11);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(192, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Only include transactions after this date";
            // 
            // lstBuyStations
            // 
            this.lstBuyStations.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstBuyStations.FormattingEnabled = true;
            this.lstBuyStations.Location = new System.Drawing.Point(9, 82);
            this.lstBuyStations.Name = "lstBuyStations";
            this.lstBuyStations.Size = new System.Drawing.Size(316, 186);
            this.lstBuyStations.TabIndex = 3;
            this.lstBuyStations.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lstBuyStations_KeyDown);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 40);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(236, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "Only include buy transactions from these stations";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.groupBox3, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(674, 504);
            this.tableLayoutPanel1.TabIndex = 15;
            // 
            // panel1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.panel1, 2);
            this.panel1.Controls.Add(this.dtpTransactionStartDate);
            this.panel1.Controls.Add(this.lblpart1);
            this.panel1.Controls.Add(this.txtMinRequired);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 103);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(668, 54);
            this.panel1.TabIndex = 3;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnClearBuyStations);
            this.groupBox2.Controls.Add(this.btnAddBuyStation);
            this.groupBox2.Controls.Add(this.txtBuyStation);
            this.groupBox2.Controls.Add(this.txtMinPurchases);
            this.groupBox2.Controls.Add(this.lstBuyStations);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(3, 163);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(331, 298);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Buy Constraints";
            // 
            // btnClearBuyStations
            // 
            this.btnClearBuyStations.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearBuyStations.Location = new System.Drawing.Point(279, 272);
            this.btnClearBuyStations.Name = "btnClearBuyStations";
            this.btnClearBuyStations.Size = new System.Drawing.Size(46, 20);
            this.btnClearBuyStations.TabIndex = 17;
            this.btnClearBuyStations.Text = "Clear";
            this.btnClearBuyStations.UseVisualStyleBackColor = true;
            this.btnClearBuyStations.Click += new System.EventHandler(this.btnClearBuyStations_Click);
            // 
            // btnAddBuyStation
            // 
            this.btnAddBuyStation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddBuyStation.Location = new System.Drawing.Point(279, 56);
            this.btnAddBuyStation.Name = "btnAddBuyStation";
            this.btnAddBuyStation.Size = new System.Drawing.Size(46, 20);
            this.btnAddBuyStation.TabIndex = 2;
            this.btnAddBuyStation.Text = "Add";
            this.btnAddBuyStation.UseVisualStyleBackColor = true;
            this.btnAddBuyStation.Click += new System.EventHandler(this.btnAddBuyStation_Click);
            // 
            // txtBuyStation
            // 
            this.txtBuyStation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBuyStation.Location = new System.Drawing.Point(9, 56);
            this.txtBuyStation.Name = "txtBuyStation";
            this.txtBuyStation.Size = new System.Drawing.Size(264, 20);
            this.txtBuyStation.TabIndex = 1;
            this.txtBuyStation.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtBuyStation_KeyDown);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnAddSellStation);
            this.groupBox3.Controls.Add(this.btnClearSellStations);
            this.groupBox3.Controls.Add(this.lstSellStations);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.txtSellStation);
            this.groupBox3.Controls.Add(this.txtMinSales);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(340, 163);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(331, 298);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Sell Constraints";
            // 
            // btnAddSellStation
            // 
            this.btnAddSellStation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddSellStation.Location = new System.Drawing.Point(279, 57);
            this.btnAddSellStation.Name = "btnAddSellStation";
            this.btnAddSellStation.Size = new System.Drawing.Size(46, 20);
            this.btnAddSellStation.TabIndex = 2;
            this.btnAddSellStation.Text = "Add";
            this.btnAddSellStation.UseVisualStyleBackColor = true;
            this.btnAddSellStation.Click += new System.EventHandler(this.btnAddSellStation_Click);
            // 
            // btnClearSellStations
            // 
            this.btnClearSellStations.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearSellStations.Location = new System.Drawing.Point(279, 272);
            this.btnClearSellStations.Name = "btnClearSellStations";
            this.btnClearSellStations.Size = new System.Drawing.Size(46, 20);
            this.btnClearSellStations.TabIndex = 11;
            this.btnClearSellStations.Text = "Clear";
            this.btnClearSellStations.UseVisualStyleBackColor = true;
            this.btnClearSellStations.Click += new System.EventHandler(this.btnClearSellStations_Click);
            // 
            // lstSellStations
            // 
            this.lstSellStations.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstSellStations.FormattingEnabled = true;
            this.lstSellStations.Location = new System.Drawing.Point(9, 82);
            this.lstSellStations.Name = "lstSellStations";
            this.lstSellStations.Size = new System.Drawing.Size(316, 186);
            this.lstSellStations.TabIndex = 3;
            this.lstSellStations.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lstSellStations_KeyDown);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 40);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(234, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Only include sell transactions from these stations";
            // 
            // txtSellStation
            // 
            this.txtSellStation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSellStation.Location = new System.Drawing.Point(9, 57);
            this.txtSellStation.Name = "txtSellStation";
            this.txtSellStation.Size = new System.Drawing.Size(264, 20);
            this.txtSellStation.TabIndex = 1;
            this.txtSellStation.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSellStation_KeyDown);
            // 
            // panel2
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.panel2, 2);
            this.panel2.Controls.Add(this.btnCancel);
            this.panel2.Controls.Add(this.btnOk);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 467);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(668, 34);
            this.panel2.TabIndex = 6;
            // 
            // AutoAddConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(674, 504);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AutoAddConfig";
            this.Text = "Auto Add Config";
            this.Load += new System.EventHandler(this.AutoAddConfig_Load);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblpart1;
        private System.Windows.Forms.TextBox txtMinRequired;
        private System.Windows.Forms.TextBox txtMinSales;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtMinPurchases;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dtpTransactionStartDate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListBox lstBuyStations;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox txtBuyStation;
        private System.Windows.Forms.ListBox lstSellStations;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtSellStation;
        private System.Windows.Forms.Button btnClearBuyStations;
        private System.Windows.Forms.Button btnAddBuyStation;
        private System.Windows.Forms.Button btnAddSellStation;
        private System.Windows.Forms.Button btnClearSellStations;
        private System.Windows.Forms.Label label6;
    }
}