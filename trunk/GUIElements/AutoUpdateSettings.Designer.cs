namespace EveMarketMonitorApp.GUIElements
{
    partial class AutoUpdateSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoUpdateSettings));
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.chkAutoUpdate = new System.Windows.Forms.CheckBox();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkDocUpdates = new System.Windows.Forms.CheckBox();
            this.btnDocUpdates = new System.Windows.Forms.Button();
            this.btnServerDefaults = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.lstServers = new System.Windows.Forms.ListBox();
            this.chkBeta = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.rdbEveTime = new System.Windows.Forms.RadioButton();
            this.rdbLocalTime = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.cmbAssetsViewWarning = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.chkGridCalcEnabled = new System.Windows.Forms.CheckBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(197, 242);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(82, 29);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(109, 242);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(82, 29);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // chkAutoUpdate
            // 
            this.chkAutoUpdate.AutoSize = true;
            this.chkAutoUpdate.Location = new System.Drawing.Point(6, 19);
            this.chkAutoUpdate.Name = "chkAutoUpdate";
            this.chkAutoUpdate.Size = new System.Drawing.Size(205, 17);
            this.chkAutoUpdate.TabIndex = 2;
            this.chkAutoUpdate.Text = "Check for updates when EMMA starts";
            this.chkAutoUpdate.UseVisualStyleBackColor = true;
            // 
            // btnUpdate
            // 
            this.btnUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnUpdate.Location = new System.Drawing.Point(6, 225);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(83, 37);
            this.btnUpdate.TabIndex = 3;
            this.btnUpdate.Text = "Update Now";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.chkDocUpdates);
            this.groupBox1.Controls.Add(this.btnDocUpdates);
            this.groupBox1.Controls.Add(this.btnServerDefaults);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.lstServers);
            this.groupBox1.Controls.Add(this.chkBeta);
            this.groupBox1.Controls.Add(this.chkAutoUpdate);
            this.groupBox1.Controls.Add(this.btnUpdate);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(274, 268);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Update Settings";
            // 
            // chkDocUpdates
            // 
            this.chkDocUpdates.AutoSize = true;
            this.chkDocUpdates.Location = new System.Drawing.Point(6, 42);
            this.chkDocUpdates.Name = "chkDocUpdates";
            this.chkDocUpdates.Size = new System.Drawing.Size(236, 17);
            this.chkDocUpdates.TabIndex = 7;
            this.chkDocUpdates.Text = "Check for documentation updates on startup";
            this.chkDocUpdates.UseVisualStyleBackColor = true;
            // 
            // btnDocUpdates
            // 
            this.btnDocUpdates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDocUpdates.Location = new System.Drawing.Point(95, 225);
            this.btnDocUpdates.Name = "btnDocUpdates";
            this.btnDocUpdates.Size = new System.Drawing.Size(83, 37);
            this.btnDocUpdates.TabIndex = 7;
            this.btnDocUpdates.Text = "Check for Doc Updates";
            this.btnDocUpdates.UseVisualStyleBackColor = true;
            this.btnDocUpdates.Click += new System.EventHandler(this.btnDocUpdates_Click);
            // 
            // btnServerDefaults
            // 
            this.btnServerDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnServerDefaults.Location = new System.Drawing.Point(184, 225);
            this.btnServerDefaults.Name = "btnServerDefaults";
            this.btnServerDefaults.Size = new System.Drawing.Size(83, 37);
            this.btnServerDefaults.TabIndex = 7;
            this.btnServerDefaults.Text = "Defaults";
            this.btnServerDefaults.UseVisualStyleBackColor = true;
            this.btnServerDefaults.Click += new System.EventHandler(this.btnServerDefaults_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 88);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Update Servers";
            // 
            // lstServers
            // 
            this.lstServers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstServers.FormattingEnabled = true;
            this.lstServers.Location = new System.Drawing.Point(6, 104);
            this.lstServers.Name = "lstServers";
            this.lstServers.Size = new System.Drawing.Size(262, 108);
            this.lstServers.TabIndex = 5;
            this.lstServers.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lstServers_KeyDown);
            // 
            // chkBeta
            // 
            this.chkBeta.AutoSize = true;
            this.chkBeta.Location = new System.Drawing.Point(6, 65);
            this.chkBeta.Name = "chkBeta";
            this.chkBeta.Size = new System.Drawing.Size(228, 17);
            this.chkBeta.TabIndex = 4;
            this.chkBeta.Text = "Check for beta updates (i.e not fully tested)";
            this.chkBeta.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.rdbEveTime);
            this.groupBox2.Controls.Add(this.rdbLocalTime);
            this.groupBox2.Location = new System.Drawing.Point(3, 99);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(276, 103);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Timezone settings";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(261, 35);
            this.label1.TabIndex = 2;
            this.label1.Text = "This determines whether the times displayed in views are eve-time (UTC) or your s" +
                "ystem\'s local timezone.";
            // 
            // rdbEveTime
            // 
            this.rdbEveTime.AutoSize = true;
            this.rdbEveTime.Location = new System.Drawing.Point(6, 54);
            this.rdbEveTime.Name = "rdbEveTime";
            this.rdbEveTime.Size = new System.Drawing.Size(66, 17);
            this.rdbEveTime.TabIndex = 1;
            this.rdbEveTime.TabStop = true;
            this.rdbEveTime.Text = "Eve-time";
            this.rdbEveTime.UseVisualStyleBackColor = true;
            // 
            // rdbLocalTime
            // 
            this.rdbLocalTime.AutoSize = true;
            this.rdbLocalTime.Location = new System.Drawing.Point(6, 77);
            this.rdbLocalTime.Name = "rdbLocalTime";
            this.rdbLocalTime.Size = new System.Drawing.Size(108, 17);
            this.rdbLocalTime.TabIndex = 0;
            this.rdbLocalTime.TabStop = true;
            this.rdbLocalTime.Text = "Local system time";
            this.rdbLocalTime.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.cmbAssetsViewWarning);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.chkGridCalcEnabled);
            this.groupBox3.Location = new System.Drawing.Point(3, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(276, 90);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Interface Settings";
            // 
            // cmbAssetsViewWarning
            // 
            this.cmbAssetsViewWarning.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cmbAssetsViewWarning.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbAssetsViewWarning.FormattingEnabled = true;
            this.cmbAssetsViewWarning.Items.AddRange(new object[] {
            "WARN",
            "FORCE YES",
            "FORCE NO"});
            this.cmbAssetsViewWarning.Location = new System.Drawing.Point(141, 62);
            this.cmbAssetsViewWarning.Name = "cmbAssetsViewWarning";
            this.cmbAssetsViewWarning.Size = new System.Drawing.Size(126, 21);
            this.cmbAssetsViewWarning.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(6, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(264, 29);
            this.label3.TabIndex = 1;
            this.label3.Text = "When user selects to view all assets in a region or eve-wide:";
            // 
            // chkGridCalcEnabled
            // 
            this.chkGridCalcEnabled.AutoSize = true;
            this.chkGridCalcEnabled.Location = new System.Drawing.Point(6, 19);
            this.chkGridCalcEnabled.Name = "chkGridCalcEnabled";
            this.chkGridCalcEnabled.Size = new System.Drawing.Size(155, 17);
            this.chkGridCalcEnabled.TabIndex = 0;
            this.chkGridCalcEnabled.Text = "Grid calculator tool enabled";
            this.chkGridCalcEnabled.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer1.Panel2.Controls.Add(this.btnCancel);
            this.splitContainer1.Panel2.Controls.Add(this.btnOk);
            this.splitContainer1.Panel2.Controls.Add(this.groupBox3);
            this.splitContainer1.Size = new System.Drawing.Size(566, 283);
            this.splitContainer1.SplitterDistance = 280;
            this.splitContainer1.TabIndex = 7;
            // 
            // AutoUpdateSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(566, 283);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AutoUpdateSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "General Settings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.CheckBox chkAutoUpdate;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton rdbEveTime;
        private System.Windows.Forms.RadioButton rdbLocalTime;
        private System.Windows.Forms.CheckBox chkBeta;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox chkGridCalcEnabled;
        private System.Windows.Forms.Button btnServerDefaults;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox lstServers;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbAssetsViewWarning;
        private System.Windows.Forms.Button btnDocUpdates;
        private System.Windows.Forms.CheckBox chkDocUpdates;
    }
}