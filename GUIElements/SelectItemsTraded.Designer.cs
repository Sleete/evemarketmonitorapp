namespace EveMarketMonitorApp.GUIElements
{
    partial class SelectItemsTraded
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectItemsTraded));
            this.lstItems = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtItemName = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnAutoAdd = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnAutoAddConfig = new System.Windows.Forms.Button();
            this.chkEveMarketPrices = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.rdbMaxBuy = new System.Windows.Forms.RadioButton();
            this.rdbMedianBuy = new System.Windows.Forms.RadioButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.lnkEveMetrics = new System.Windows.Forms.LinkLabel();
            this.lnkEveCentral = new System.Windows.Forms.LinkLabel();
            this.rdbEveMetrics = new System.Windows.Forms.RadioButton();
            this.rdbEveCentral = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.txtItemSellPrice = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbStation = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblCalculatedSellPrice = new System.Windows.Forms.Label();
            this.lblLastUpdated = new System.Windows.Forms.Label();
            this.btnValueHist = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.txtDefaultBuyPrice = new System.Windows.Forms.TextBox();
            this.chkUseReprocessVal = new System.Windows.Forms.CheckBox();
            this.btnValueCalculationDetail = new System.Windows.Forms.Button();
            this.chkForceDefaultBuyPrice = new System.Windows.Forms.CheckBox();
            this.chkForceDefaultSellPrice = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstItems
            // 
            this.lstItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstItems.FormattingEnabled = true;
            this.lstItems.Location = new System.Drawing.Point(12, 132);
            this.lstItems.Name = "lstItems";
            this.lstItems.Size = new System.Drawing.Size(504, 199);
            this.lstItems.Sorted = true;
            this.lstItems.TabIndex = 1;
            this.lstItems.SelectedIndexChanged += new System.EventHandler(this.lstItems_SelectedIndexChanged);
            this.lstItems.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lstItems_KeyDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 109);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Item name:";
            // 
            // txtItemName
            // 
            this.txtItemName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtItemName.Location = new System.Drawing.Point(76, 106);
            this.txtItemName.Name = "txtItemName";
            this.txtItemName.Size = new System.Drawing.Size(440, 20);
            this.txtItemName.TabIndex = 0;
            this.txtItemName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtItemName_KeyDown);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.textBox2);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(504, 91);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Information";
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.SystemColors.Control;
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox2.Cursor = System.Windows.Forms.Cursors.Default;
            this.textBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox2.Location = new System.Drawing.Point(3, 16);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(498, 72);
            this.textBox2.TabIndex = 0;
            this.textBox2.TabStop = false;
            this.textBox2.Text = resources.GetString("textBox2.Text");
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(318, 688);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(96, 29);
            this.btnOk.TabIndex = 2;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(420, 688);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(96, 29);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnAutoAdd
            // 
            this.btnAutoAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAutoAdd.Location = new System.Drawing.Point(12, 688);
            this.btnAutoAdd.Name = "btnAutoAdd";
            this.btnAutoAdd.Size = new System.Drawing.Size(96, 29);
            this.btnAutoAdd.TabIndex = 7;
            this.btnAutoAdd.Text = "Auto Add";
            this.btnAutoAdd.UseVisualStyleBackColor = true;
            this.btnAutoAdd.Click += new System.EventHandler(this.btnAutoAdd_Click);
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClear.Location = new System.Drawing.Point(216, 688);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(96, 29);
            this.btnClear.TabIndex = 8;
            this.btnClear.Text = "Clear All";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnAutoAddConfig
            // 
            this.btnAutoAddConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAutoAddConfig.Location = new System.Drawing.Point(114, 688);
            this.btnAutoAddConfig.Name = "btnAutoAddConfig";
            this.btnAutoAddConfig.Size = new System.Drawing.Size(96, 29);
            this.btnAutoAddConfig.TabIndex = 12;
            this.btnAutoAddConfig.Text = "Auto Add Config";
            this.btnAutoAddConfig.UseVisualStyleBackColor = true;
            this.btnAutoAddConfig.Click += new System.EventHandler(this.btnAutoAddConfig_Click);
            // 
            // chkEveMarketPrices
            // 
            this.chkEveMarketPrices.AutoSize = true;
            this.chkEveMarketPrices.Location = new System.Drawing.Point(6, 19);
            this.chkEveMarketPrices.Name = "chkEveMarketPrices";
            this.chkEveMarketPrices.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.chkEveMarketPrices.Size = new System.Drawing.Size(331, 17);
            this.chkEveMarketPrices.TabIndex = 13;
            this.chkEveMarketPrices.Text = "Use market values when no other pricing information is available";
            this.chkEveMarketPrices.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.rdbMaxBuy);
            this.groupBox3.Controls.Add(this.rdbMedianBuy);
            this.groupBox3.Controls.Add(this.chkEveMarketPrices);
            this.groupBox3.Location = new System.Drawing.Point(12, 520);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(504, 85);
            this.groupBox3.TabIndex = 14;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "General settings";
            // 
            // rdbMaxBuy
            // 
            this.rdbMaxBuy.AutoSize = true;
            this.rdbMaxBuy.Location = new System.Drawing.Point(29, 61);
            this.rdbMaxBuy.Name = "rdbMaxBuy";
            this.rdbMaxBuy.Size = new System.Drawing.Size(177, 17);
            this.rdbMaxBuy.TabIndex = 15;
            this.rdbMaxBuy.TabStop = true;
            this.rdbMaxBuy.Text = "Use max buy price as item value";
            this.rdbMaxBuy.UseVisualStyleBackColor = true;
            // 
            // rdbMedianBuy
            // 
            this.rdbMedianBuy.AutoSize = true;
            this.rdbMedianBuy.Location = new System.Drawing.Point(29, 38);
            this.rdbMedianBuy.Name = "rdbMedianBuy";
            this.rdbMedianBuy.Size = new System.Drawing.Size(192, 17);
            this.rdbMedianBuy.TabIndex = 14;
            this.rdbMedianBuy.TabStop = true;
            this.rdbMedianBuy.Text = "Use median buy price as item value";
            this.rdbMedianBuy.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.lnkEveMetrics);
            this.groupBox4.Controls.Add(this.lnkEveCentral);
            this.groupBox4.Controls.Add(this.rdbEveMetrics);
            this.groupBox4.Controls.Add(this.rdbEveCentral);
            this.groupBox4.Location = new System.Drawing.Point(12, 611);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(504, 71);
            this.groupBox4.TabIndex = 15;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Market Price Source";
            // 
            // lnkEveMetrics
            // 
            this.lnkEveMetrics.AutoSize = true;
            this.lnkEveMetrics.Location = new System.Drawing.Point(227, 44);
            this.lnkEveMetrics.Name = "lnkEveMetrics";
            this.lnkEveMetrics.Size = new System.Drawing.Size(111, 13);
            this.lnkEveMetrics.TabIndex = 18;
            this.lnkEveMetrics.TabStop = true;
            this.lnkEveMetrics.Text = "www.eve-metrics.com";
            this.lnkEveMetrics.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkEveMetrics_LinkClicked);
            // 
            // lnkEveCentral
            // 
            this.lnkEveCentral.AutoSize = true;
            this.lnkEveCentral.Location = new System.Drawing.Point(227, 21);
            this.lnkEveCentral.Name = "lnkEveCentral";
            this.lnkEveCentral.Size = new System.Drawing.Size(110, 13);
            this.lnkEveCentral.TabIndex = 17;
            this.lnkEveCentral.TabStop = true;
            this.lnkEveCentral.Text = "www.eve-central.com";
            this.lnkEveCentral.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkEveCentral_LinkClicked);
            // 
            // rdbEveMetrics
            // 
            this.rdbEveMetrics.AutoSize = true;
            this.rdbEveMetrics.Location = new System.Drawing.Point(8, 42);
            this.rdbEveMetrics.Name = "rdbEveMetrics";
            this.rdbEveMetrics.Size = new System.Drawing.Size(182, 17);
            this.rdbEveMetrics.TabIndex = 16;
            this.rdbEveMetrics.TabStop = true;
            this.rdbEveMetrics.Text = "Use eve-metrics for market prices";
            this.rdbEveMetrics.UseVisualStyleBackColor = true;
            // 
            // rdbEveCentral
            // 
            this.rdbEveCentral.AutoSize = true;
            this.rdbEveCentral.Location = new System.Drawing.Point(8, 19);
            this.rdbEveCentral.Name = "rdbEveCentral";
            this.rdbEveCentral.Size = new System.Drawing.Size(181, 17);
            this.rdbEveCentral.TabIndex = 15;
            this.rdbEveCentral.TabStop = true;
            this.rdbEveCentral.Text = "Use eve-central for market prices";
            this.rdbEveCentral.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 77);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(92, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Default sale price:";
            // 
            // txtItemSellPrice
            // 
            this.txtItemSellPrice.Location = new System.Drawing.Point(123, 74);
            this.txtItemSellPrice.Name = "txtItemSellPrice";
            this.txtItemSellPrice.Size = new System.Drawing.Size(159, 20);
            this.txtItemSellPrice.TabIndex = 9;
            this.txtItemSellPrice.Leave += new System.EventHandler(this.txtItemSellPrice_Leave);
            this.txtItemSellPrice.Enter += new System.EventHandler(this.txtItemSellPrice_Enter);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 49);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Region";
            // 
            // cmbStation
            // 
            this.cmbStation.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cmbStation.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbStation.FormattingEnabled = true;
            this.cmbStation.Location = new System.Drawing.Point(123, 46);
            this.cmbStation.Name = "cmbStation";
            this.cmbStation.Size = new System.Drawing.Size(374, 21);
            this.cmbStation.TabIndex = 12;
            this.cmbStation.SelectedIndexChanged += new System.EventHandler(this.cmbStation_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(5, 133);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(108, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "Calculated sale price:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(5, 159);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(72, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "Last updated:";
            // 
            // lblCalculatedSellPrice
            // 
            this.lblCalculatedSellPrice.AutoSize = true;
            this.lblCalculatedSellPrice.Location = new System.Drawing.Point(120, 133);
            this.lblCalculatedSellPrice.Name = "lblCalculatedSellPrice";
            this.lblCalculatedSellPrice.Size = new System.Drawing.Size(76, 13);
            this.lblCalculatedSellPrice.TabIndex = 16;
            this.lblCalculatedSellPrice.Text = "10,000,000.00";
            // 
            // lblLastUpdated
            // 
            this.lblLastUpdated.AutoSize = true;
            this.lblLastUpdated.Location = new System.Drawing.Point(120, 159);
            this.lblLastUpdated.Name = "lblLastUpdated";
            this.lblLastUpdated.Size = new System.Drawing.Size(65, 13);
            this.lblLastUpdated.TabIndex = 17;
            this.lblLastUpdated.Text = "01/01/2008";
            // 
            // btnValueHist
            // 
            this.btnValueHist.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnValueHist.Location = new System.Drawing.Point(402, 142);
            this.btnValueHist.Name = "btnValueHist";
            this.btnValueHist.Size = new System.Drawing.Size(96, 29);
            this.btnValueHist.TabIndex = 15;
            this.btnValueHist.Text = "Value History";
            this.btnValueHist.UseVisualStyleBackColor = true;
            this.btnValueHist.Click += new System.EventHandler(this.btnValueHist_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(5, 103);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(90, 13);
            this.label6.TabIndex = 19;
            this.label6.Text = "Default buy price:";
            // 
            // txtDefaultBuyPrice
            // 
            this.txtDefaultBuyPrice.Location = new System.Drawing.Point(123, 100);
            this.txtDefaultBuyPrice.Name = "txtDefaultBuyPrice";
            this.txtDefaultBuyPrice.Size = new System.Drawing.Size(159, 20);
            this.txtDefaultBuyPrice.TabIndex = 18;
            this.txtDefaultBuyPrice.Leave += new System.EventHandler(this.txtItemBuyPrice_Leave);
            this.txtDefaultBuyPrice.Enter += new System.EventHandler(this.txtItemBuyPrice_Enter);
            // 
            // chkUseReprocessVal
            // 
            this.chkUseReprocessVal.AutoSize = true;
            this.chkUseReprocessVal.Location = new System.Drawing.Point(6, 23);
            this.chkUseReprocessVal.Name = "chkUseReprocessVal";
            this.chkUseReprocessVal.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.chkUseReprocessVal.Size = new System.Drawing.Size(398, 17);
            this.chkUseReprocessVal.TabIndex = 16;
            this.chkUseReprocessVal.Text = "Use reprocess value instead of expected sell price as the item value in reports.";
            this.chkUseReprocessVal.UseVisualStyleBackColor = true;
            // 
            // btnValueCalculationDetail
            // 
            this.btnValueCalculationDetail.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnValueCalculationDetail.Location = new System.Drawing.Point(300, 142);
            this.btnValueCalculationDetail.Name = "btnValueCalculationDetail";
            this.btnValueCalculationDetail.Size = new System.Drawing.Size(96, 29);
            this.btnValueCalculationDetail.TabIndex = 20;
            this.btnValueCalculationDetail.Text = "Value Calc Detail";
            this.btnValueCalculationDetail.UseVisualStyleBackColor = true;
            this.btnValueCalculationDetail.Click += new System.EventHandler(this.btnValueCalculationDetail_Click);
            // 
            // chkForceDefaultBuyPrice
            // 
            this.chkForceDefaultBuyPrice.AutoSize = true;
            this.chkForceDefaultBuyPrice.Location = new System.Drawing.Point(300, 102);
            this.chkForceDefaultBuyPrice.Name = "chkForceDefaultBuyPrice";
            this.chkForceDefaultBuyPrice.Size = new System.Drawing.Size(160, 17);
            this.chkForceDefaultBuyPrice.TabIndex = 21;
            this.chkForceDefaultBuyPrice.Text = "Always use default buy price";
            this.chkForceDefaultBuyPrice.UseVisualStyleBackColor = true;
            // 
            // chkForceDefaultSellPrice
            // 
            this.chkForceDefaultSellPrice.AutoSize = true;
            this.chkForceDefaultSellPrice.Location = new System.Drawing.Point(300, 76);
            this.chkForceDefaultSellPrice.Name = "chkForceDefaultSellPrice";
            this.chkForceDefaultSellPrice.Size = new System.Drawing.Size(158, 17);
            this.chkForceDefaultSellPrice.TabIndex = 22;
            this.chkForceDefaultSellPrice.Text = "Always use default sell price";
            this.chkForceDefaultSellPrice.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.chkForceDefaultSellPrice);
            this.groupBox1.Controls.Add(this.chkForceDefaultBuyPrice);
            this.groupBox1.Controls.Add(this.btnValueCalculationDetail);
            this.groupBox1.Controls.Add(this.chkUseReprocessVal);
            this.groupBox1.Controls.Add(this.txtDefaultBuyPrice);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.btnValueHist);
            this.groupBox1.Controls.Add(this.lblLastUpdated);
            this.groupBox1.Controls.Add(this.lblCalculatedSellPrice);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.cmbStation);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtItemSellPrice);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(12, 337);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(504, 177);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Pricing details";
            // 
            // SelectItemsTraded
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(528, 729);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.btnAutoAddConfig);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnAutoAdd);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.lstItems);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.txtItemName);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SelectItemsTraded";
            this.Text = "Item Value";
            this.Load += new System.EventHandler(this.SelectItemsTraded_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstItems;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtItemName;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnAutoAdd;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnAutoAddConfig;
        private System.Windows.Forms.CheckBox chkEveMarketPrices;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton rdbMaxBuy;
        private System.Windows.Forms.RadioButton rdbMedianBuy;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.RadioButton rdbEveCentral;
        private System.Windows.Forms.RadioButton rdbEveMetrics;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtItemSellPrice;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbStation;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblCalculatedSellPrice;
        private System.Windows.Forms.Label lblLastUpdated;
        private System.Windows.Forms.Button btnValueHist;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtDefaultBuyPrice;
        private System.Windows.Forms.CheckBox chkUseReprocessVal;
        private System.Windows.Forms.Button btnValueCalculationDetail;
        private System.Windows.Forms.CheckBox chkForceDefaultBuyPrice;
        private System.Windows.Forms.CheckBox chkForceDefaultSellPrice;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.LinkLabel lnkEveMetrics;
        private System.Windows.Forms.LinkLabel lnkEveCentral;
    }
}