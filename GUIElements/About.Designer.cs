namespace EveMarketMonitorApp.GUIElements
{
    partial class About
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(About));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.componentVersionsGrid = new System.Windows.Forms.DataGridView();
            this.NameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.VersionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label5 = new System.Windows.Forms.Label();
            this.lnkEMMAThread = new System.Windows.Forms.LinkLabel();
            this.lnkSourceForge = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.componentVersionsGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.componentVersionsGrid);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.lnkEMMAThread);
            this.groupBox1.Controls.Add(this.lnkSourceForge);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(457, 285);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Information";
            // 
            // componentVersionsGrid
            // 
            this.componentVersionsGrid.AllowUserToAddRows = false;
            this.componentVersionsGrid.AllowUserToDeleteRows = false;
            this.componentVersionsGrid.AllowUserToResizeRows = false;
            this.componentVersionsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.componentVersionsGrid.BackgroundColor = System.Drawing.SystemColors.ButtonHighlight;
            this.componentVersionsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.componentVersionsGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.NameColumn,
            this.VersionColumn});
            this.componentVersionsGrid.GridColor = System.Drawing.SystemColors.ButtonHighlight;
            this.componentVersionsGrid.Location = new System.Drawing.Point(9, 164);
            this.componentVersionsGrid.MultiSelect = false;
            this.componentVersionsGrid.Name = "componentVersionsGrid";
            this.componentVersionsGrid.ReadOnly = true;
            this.componentVersionsGrid.RowHeadersVisible = false;
            this.componentVersionsGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.componentVersionsGrid.Size = new System.Drawing.Size(442, 115);
            this.componentVersionsGrid.TabIndex = 10;
            // 
            // NameColumn
            // 
            this.NameColumn.HeaderText = "Component Name";
            this.NameColumn.Name = "NameColumn";
            this.NameColumn.ReadOnly = true;
            this.NameColumn.Width = 320;
            // 
            // VersionColumn
            // 
            this.VersionColumn.HeaderText = "Version";
            this.VersionColumn.Name = "VersionColumn";
            this.VersionColumn.ReadOnly = true;
            this.VersionColumn.Width = 90;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 148);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(142, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Current component versions:";
            // 
            // lnkEMMAThread
            // 
            this.lnkEMMAThread.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkEMMAThread.LinkArea = new System.Windows.Forms.LinkArea(41, 11);
            this.lnkEMMAThread.Location = new System.Drawing.Point(9, 107);
            this.lnkEMMAThread.Name = "lnkEMMAThread";
            this.lnkEMMAThread.Size = new System.Drawing.Size(442, 32);
            this.lnkEMMAThread.TabIndex = 5;
            this.lnkEMMAThread.TabStop = true;
            this.lnkEMMAThread.Text = "Alternativley you can contact us via the EMMA thread on the Eve forums, or throug" +
                "h in-game eve-mail/chat.";
            this.lnkEMMAThread.UseCompatibleTextRendering = true;
            this.lnkEMMAThread.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkEMMAThread_LinkClicked);
            // 
            // lnkSourceForge
            // 
            this.lnkSourceForge.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkSourceForge.LinkArea = new System.Windows.Forms.LinkArea(94, 25);
            this.lnkSourceForge.Location = new System.Drawing.Point(9, 74);
            this.lnkSourceForge.Name = "lnkSourceForge";
            this.lnkSourceForge.Size = new System.Drawing.Size(442, 33);
            this.lnkSourceForge.TabIndex = 4;
            this.lnkSourceForge.TabStop = true;
            this.lnkSourceForge.Text = "If you have any problems or sugestions for improvements then you can log them dir" +
                "ectly at the googlecode project pages.";
            this.lnkSourceForge.UseCompatibleTextRendering = true;
            this.lnkSourceForge.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkSourceForge_LinkClicked);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(445, 58);
            this.label1.TabIndex = 0;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(197, 303);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(86, 28);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // About
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(481, 343);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "About";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About EMMA";
            this.Load += new System.EventHandler(this.About_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.componentVersionsGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.LinkLabel lnkSourceForge;
        private System.Windows.Forms.DataGridView componentVersionsGrid;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.LinkLabel lnkEMMAThread;
        private System.Windows.Forms.DataGridViewTextBoxColumn NameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn VersionColumn;
    }
}