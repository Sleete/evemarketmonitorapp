namespace EveMarketMonitorApp.GUIElements
{
    partial class RptParamsItems
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RptParamsItems));
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.chkColumns = new System.Windows.Forms.CheckedListBox();
            this.dtpStartDate = new System.Windows.Forms.DateTimePicker();
            this.dtpEndDate = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.chkItemsByGroup = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbLocation = new System.Windows.Forms.ComboBox();
            this.btnNewLocation = new System.Windows.Forms.Button();
            this.btnModifyLocation = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.chkUseRecentBuyPrices = new System.Windows.Forms.CheckBox();
            this.chkTradedItems = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(95, 522);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(88, 29);
            this.btnOk.TabIndex = 5;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(189, 522);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 29);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(255, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "End Date: The latest date included in the report data";
            // 
            // chkColumns
            // 
            this.chkColumns.CheckOnClick = true;
            this.chkColumns.FormattingEnabled = true;
            this.chkColumns.Location = new System.Drawing.Point(12, 334);
            this.chkColumns.Name = "chkColumns";
            this.chkColumns.Size = new System.Drawing.Size(265, 109);
            this.chkColumns.TabIndex = 3;
            // 
            // dtpStartDate
            // 
            this.dtpStartDate.CustomFormat = "dd-MMM-yyyy HH:mm:ss";
            this.dtpStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpStartDate.Location = new System.Drawing.Point(12, 25);
            this.dtpStartDate.Name = "dtpStartDate";
            this.dtpStartDate.Size = new System.Drawing.Size(200, 20);
            this.dtpStartDate.TabIndex = 0;
            // 
            // dtpEndDate
            // 
            this.dtpEndDate.CustomFormat = "dd-MMM-yyyy HH:mm:ss";
            this.dtpEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpEndDate.Location = new System.Drawing.Point(12, 64);
            this.dtpEndDate.Name = "dtpEndDate";
            this.dtpEndDate.Size = new System.Drawing.Size(200, 20);
            this.dtpEndDate.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(264, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Start date: The earliest date included in the report data";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 318);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(260, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Displayed Columns: The columns shown on the report";
            // 
            // chkItemsByGroup
            // 
            this.chkItemsByGroup.AutoSize = true;
            this.chkItemsByGroup.Location = new System.Drawing.Point(12, 90);
            this.chkItemsByGroup.Name = "chkItemsByGroup";
            this.chkItemsByGroup.Size = new System.Drawing.Size(104, 17);
            this.chkItemsByGroup.TabIndex = 2;
            this.chkItemsByGroup.Text = "Categorise Items";
            this.chkItemsByGroup.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(12, 110);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(260, 42);
            this.label5.TabIndex = 15;
            this.label5.Text = "(Produces a report where items are organised by categories with sub totals on rel" +
                "evant fields for each category)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 456);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(51, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Location:";
            // 
            // cmbLocation
            // 
            this.cmbLocation.FormattingEnabled = true;
            this.cmbLocation.Location = new System.Drawing.Point(12, 491);
            this.cmbLocation.Name = "cmbLocation";
            this.cmbLocation.Size = new System.Drawing.Size(265, 21);
            this.cmbLocation.TabIndex = 4;
            // 
            // btnNewLocation
            // 
            this.btnNewLocation.Location = new System.Drawing.Point(95, 456);
            this.btnNewLocation.Name = "btnNewLocation";
            this.btnNewLocation.Size = new System.Drawing.Size(88, 29);
            this.btnNewLocation.TabIndex = 7;
            this.btnNewLocation.Text = "Create New";
            this.btnNewLocation.UseVisualStyleBackColor = true;
            this.btnNewLocation.Click += new System.EventHandler(this.btnNewLocation_Click);
            // 
            // btnModifyLocation
            // 
            this.btnModifyLocation.Location = new System.Drawing.Point(189, 456);
            this.btnModifyLocation.Name = "btnModifyLocation";
            this.btnModifyLocation.Size = new System.Drawing.Size(88, 29);
            this.btnModifyLocation.TabIndex = 8;
            this.btnModifyLocation.Text = "Modify";
            this.btnModifyLocation.UseVisualStyleBackColor = true;
            this.btnModifyLocation.Click += new System.EventHandler(this.btnModifyLocation_Click);
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(12, 175);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(260, 66);
            this.label6.TabIndex = 18;
            this.label6.Text = resources.GetString("label6.Text");
            // 
            // chkUseRecentBuyPrices
            // 
            this.chkUseRecentBuyPrices.AutoSize = true;
            this.chkUseRecentBuyPrices.Location = new System.Drawing.Point(12, 155);
            this.chkUseRecentBuyPrices.Name = "chkUseRecentBuyPrices";
            this.chkUseRecentBuyPrices.Size = new System.Drawing.Size(254, 17);
            this.chkUseRecentBuyPrices.TabIndex = 17;
            this.chkUseRecentBuyPrices.Text = "Use most recent buy prices as \'cost of units sold\'";
            this.chkUseRecentBuyPrices.UseVisualStyleBackColor = true;
            // 
            // chkTradedItems
            // 
            this.chkTradedItems.AutoSize = true;
            this.chkTradedItems.Location = new System.Drawing.Point(12, 244);
            this.chkTradedItems.Name = "chkTradedItems";
            this.chkTradedItems.Size = new System.Drawing.Size(144, 17);
            this.chkTradedItems.TabIndex = 19;
            this.chkTradedItems.Text = "Only include traded items";
            this.chkTradedItems.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(12, 264);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(260, 40);
            this.label7.TabIndex = 20;
            this.label7.Text = "(If enabled then the report will only contain items that are on the list of trade" +
                "d items, configured in settings -> traded items)";
            // 
            // RptParamsItems
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(289, 563);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.chkTradedItems);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.chkUseRecentBuyPrices);
            this.Controls.Add(this.btnModifyLocation);
            this.Controls.Add(this.btnNewLocation);
            this.Controls.Add(this.cmbLocation);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.chkItemsByGroup);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dtpEndDate);
            this.Controls.Add(this.dtpStartDate);
            this.Controls.Add(this.chkColumns);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RptParamsItems";
            this.Text = "Item Report Parameters";
            this.Load += new System.EventHandler(this.ItemReportParams_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckedListBox chkColumns;
        private System.Windows.Forms.DateTimePicker dtpStartDate;
        private System.Windows.Forms.DateTimePicker dtpEndDate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkItemsByGroup;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbLocation;
        private System.Windows.Forms.Button btnNewLocation;
        private System.Windows.Forms.Button btnModifyLocation;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkUseRecentBuyPrices;
        private System.Windows.Forms.CheckBox chkTradedItems;
        private System.Windows.Forms.Label label7;
    }
}