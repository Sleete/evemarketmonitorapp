namespace EveMarketMonitorApp.GUIElements
{
    partial class RptParamsAssets
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RptParamsAssets));
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.chkColumns = new System.Windows.Forms.CheckedListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chkItemsByGroup = new System.Windows.Forms.CheckBox();
            this.cmbValueRegion = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnModifyLocation = new System.Windows.Forms.Button();
            this.btnNewLocation = new System.Windows.Forms.Button();
            this.cmbLocation = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.chkInTransit = new System.Windows.Forms.CheckBox();
            this.chkIncludeContainers = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(158, 302);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(88, 29);
            this.btnOk.TabIndex = 2;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(252, 302);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 29);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // chkColumns
            // 
            this.chkColumns.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.chkColumns.FormattingEnabled = true;
            this.chkColumns.Location = new System.Drawing.Point(12, 48);
            this.chkColumns.Name = "chkColumns";
            this.chkColumns.Size = new System.Drawing.Size(328, 79);
            this.chkColumns.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 32);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(260, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Displayed Columns: The columns shown on the report";
            // 
            // chkItemsByGroup
            // 
            this.chkItemsByGroup.AutoSize = true;
            this.chkItemsByGroup.Location = new System.Drawing.Point(12, 12);
            this.chkItemsByGroup.Name = "chkItemsByGroup";
            this.chkItemsByGroup.Size = new System.Drawing.Size(145, 17);
            this.chkItemsByGroup.TabIndex = 12;
            this.chkItemsByGroup.Text = "Arrange items by category";
            this.chkItemsByGroup.UseVisualStyleBackColor = true;
            // 
            // cmbValueRegion
            // 
            this.cmbValueRegion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbValueRegion.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cmbValueRegion.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbValueRegion.FormattingEnabled = true;
            this.cmbValueRegion.Location = new System.Drawing.Point(12, 146);
            this.cmbValueRegion.Name = "cmbValueRegion";
            this.cmbValueRegion.Size = new System.Drawing.Size(328, 21);
            this.cmbValueRegion.TabIndex = 13;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 130);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(181, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "Region to use when estimating value";
            // 
            // btnModifyLocation
            // 
            this.btnModifyLocation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnModifyLocation.Location = new System.Drawing.Point(252, 216);
            this.btnModifyLocation.Name = "btnModifyLocation";
            this.btnModifyLocation.Size = new System.Drawing.Size(88, 29);
            this.btnModifyLocation.TabIndex = 19;
            this.btnModifyLocation.Text = "Modify";
            this.btnModifyLocation.UseVisualStyleBackColor = true;
            // 
            // btnNewLocation
            // 
            this.btnNewLocation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNewLocation.Location = new System.Drawing.Point(158, 216);
            this.btnNewLocation.Name = "btnNewLocation";
            this.btnNewLocation.Size = new System.Drawing.Size(88, 29);
            this.btnNewLocation.TabIndex = 18;
            this.btnNewLocation.Text = "Create New";
            this.btnNewLocation.UseVisualStyleBackColor = true;
            // 
            // cmbLocation
            // 
            this.cmbLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbLocation.FormattingEnabled = true;
            this.cmbLocation.Location = new System.Drawing.Point(12, 189);
            this.cmbLocation.Name = "cmbLocation";
            this.cmbLocation.Size = new System.Drawing.Size(328, 21);
            this.cmbLocation.TabIndex = 17;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 173);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(106, 13);
            this.label3.TabIndex = 20;
            this.label3.Text = "Locations to Include:";
            // 
            // chkInTransit
            // 
            this.chkInTransit.AutoSize = true;
            this.chkInTransit.Location = new System.Drawing.Point(12, 251);
            this.chkInTransit.Name = "chkInTransit";
            this.chkInTransit.Size = new System.Drawing.Size(130, 17);
            this.chkInTransit.TabIndex = 21;
            this.chkInTransit.Text = "Include items in transit";
            this.chkInTransit.UseVisualStyleBackColor = true;
            // 
            // chkIncludeContainers
            // 
            this.chkIncludeContainers.AutoSize = true;
            this.chkIncludeContainers.Location = new System.Drawing.Point(12, 274);
            this.chkIncludeContainers.Name = "chkIncludeContainers";
            this.chkIncludeContainers.Size = new System.Drawing.Size(330, 17);
            this.chkIncludeContainers.TabIndex = 22;
            this.chkIncludeContainers.Text = "Include unpackaged ships, fittings, containers and their contents";
            this.chkIncludeContainers.UseVisualStyleBackColor = true;
            // 
            // RptParamsAssets
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(352, 343);
            this.Controls.Add(this.chkIncludeContainers);
            this.Controls.Add(this.chkInTransit);
            this.Controls.Add(this.btnModifyLocation);
            this.Controls.Add(this.btnNewLocation);
            this.Controls.Add(this.cmbLocation);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbValueRegion);
            this.Controls.Add(this.chkItemsByGroup);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.chkColumns);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RptParamsAssets";
            this.Text = "Item Report Parameters";
            this.Load += new System.EventHandler(this.AssetsReportParams_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckedListBox chkColumns;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkItemsByGroup;
        private System.Windows.Forms.ComboBox cmbValueRegion;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnModifyLocation;
        private System.Windows.Forms.Button btnNewLocation;
        private System.Windows.Forms.ComboBox cmbLocation;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkInTransit;
        private System.Windows.Forms.CheckBox chkIncludeContainers;
    }
}