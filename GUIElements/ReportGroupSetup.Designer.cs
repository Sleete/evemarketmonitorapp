namespace EveMarketMonitorApp.GUIElements
{
    partial class ReportGroupSetup
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReportGroupSetup));
            this.eveAccountsGrid = new System.Windows.Forms.DataGridView();
            this.UserIDColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ApiKeyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.charsAndCorpsGrid = new System.Windows.Forms.DataGridView();
            this.charIDColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.charNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.charIncludedColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.corpIDColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.corpNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnNewAccount = new System.Windows.Forms.Button();
            this.btnDeleteAccount = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblNoAccounts = new System.Windows.Forms.Label();
            this.lblNoChars = new System.Windows.Forms.Label();
            this.CorprGrid = new System.Windows.Forms.DataGridView();
            this.corpIDColumn_Corp = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.corpNameColumn_Corp = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.corpIncludedColumn_Corp = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.charNameColumn_corp = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblNoCorp = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.eveAccountsGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.charsAndCorpsGrid)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CorprGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // eveAccountsGrid
            // 
            this.eveAccountsGrid.AllowUserToAddRows = false;
            this.eveAccountsGrid.AllowUserToDeleteRows = false;
            this.eveAccountsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.eveAccountsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.eveAccountsGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.UserIDColumn,
            this.ApiKeyColumn});
            this.eveAccountsGrid.Location = new System.Drawing.Point(12, 85);
            this.eveAccountsGrid.Name = "eveAccountsGrid";
            this.eveAccountsGrid.ReadOnly = true;
            this.eveAccountsGrid.RowHeadersVisible = false;
            this.eveAccountsGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.eveAccountsGrid.Size = new System.Drawing.Size(865, 118);
            this.eveAccountsGrid.TabIndex = 0;
            this.eveAccountsGrid.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.eveAccountsGrid_CellDoubleClick);
            this.eveAccountsGrid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.eveAccountsGrid_KeyDown);
            this.eveAccountsGrid.SelectionChanged += new System.EventHandler(this.eveAccountsGrid_SelectionChanged);
            this.eveAccountsGrid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.eveAccountsGrid_CellContentClick);
            // 
            // UserIDColumn
            // 
            this.UserIDColumn.HeaderText = "Key ID";
            this.UserIDColumn.Name = "UserIDColumn";
            this.UserIDColumn.ReadOnly = true;
            // 
            // ApiKeyColumn
            // 
            this.ApiKeyColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ApiKeyColumn.HeaderText = "Verification Code";
            this.ApiKeyColumn.Name = "ApiKeyColumn";
            this.ApiKeyColumn.ReadOnly = true;
            // 
            // charsAndCorpsGrid
            // 
            this.charsAndCorpsGrid.AllowUserToAddRows = false;
            this.charsAndCorpsGrid.AllowUserToDeleteRows = false;
            this.charsAndCorpsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.charsAndCorpsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.charsAndCorpsGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.charIDColumn,
            this.charNameColumn,
            this.charIncludedColumn,
            this.corpIDColumn,
            this.corpNameColumn});
            this.charsAndCorpsGrid.Location = new System.Drawing.Point(12, 245);
            this.charsAndCorpsGrid.Name = "charsAndCorpsGrid";
            this.charsAndCorpsGrid.RowHeadersVisible = false;
            this.charsAndCorpsGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.charsAndCorpsGrid.Size = new System.Drawing.Size(865, 171);
            this.charsAndCorpsGrid.TabIndex = 1;
            this.charsAndCorpsGrid.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.charsAndCorpsGrid_CellContentDoubleClick);
            this.charsAndCorpsGrid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.charsAndCorpsGrid_CellFormatting);
            this.charsAndCorpsGrid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.charsAndCorpsGrid_CellContentClick);
            // 
            // charIDColumn
            // 
            this.charIDColumn.HeaderText = "Character ID";
            this.charIDColumn.Name = "charIDColumn";
            this.charIDColumn.ReadOnly = true;
            // 
            // charNameColumn
            // 
            this.charNameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.charNameColumn.HeaderText = "Character Name";
            this.charNameColumn.Name = "charNameColumn";
            this.charNameColumn.ReadOnly = true;
            // 
            // charIncludedColumn
            // 
            this.charIncludedColumn.HeaderText = "Character Included";
            this.charIncludedColumn.Name = "charIncludedColumn";
            // 
            // corpIDColumn
            // 
            this.corpIDColumn.HeaderText = "Corp ID";
            this.corpIDColumn.Name = "corpIDColumn";
            this.corpIDColumn.ReadOnly = true;
            // 
            // corpNameColumn
            // 
            this.corpNameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.corpNameColumn.HeaderText = "Corp Name";
            this.corpNameColumn.Name = "corpNameColumn";
            this.corpNameColumn.ReadOnly = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(865, 67);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Information";
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(859, 48);
            this.label1.TabIndex = 0;
            this.label1.Text = resources.GetString("label1.Text");
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // btnNewAccount
            // 
            this.btnNewAccount.Location = new System.Drawing.Point(12, 209);
            this.btnNewAccount.Name = "btnNewAccount";
            this.btnNewAccount.Size = new System.Drawing.Size(105, 30);
            this.btnNewAccount.TabIndex = 5;
            this.btnNewAccount.Text = "New Account";
            this.btnNewAccount.UseVisualStyleBackColor = true;
            this.btnNewAccount.Click += new System.EventHandler(this.btnNewAccount_Click);
            // 
            // btnDeleteAccount
            // 
            this.btnDeleteAccount.Location = new System.Drawing.Point(123, 209);
            this.btnDeleteAccount.Name = "btnDeleteAccount";
            this.btnDeleteAccount.Size = new System.Drawing.Size(105, 30);
            this.btnDeleteAccount.TabIndex = 6;
            this.btnDeleteAccount.Text = "Remove Account";
            this.btnDeleteAccount.UseVisualStyleBackColor = true;
            this.btnDeleteAccount.Click += new System.EventHandler(this.btnDeleteAccount_Click);
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(661, 574);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(105, 30);
            this.btnOk.TabIndex = 7;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(772, 574);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(105, 30);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblNoAccounts
            // 
            this.lblNoAccounts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNoAccounts.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.lblNoAccounts.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNoAccounts.Location = new System.Drawing.Point(18, 112);
            this.lblNoAccounts.Name = "lblNoAccounts";
            this.lblNoAccounts.Size = new System.Drawing.Size(856, 57);
            this.lblNoAccounts.TabIndex = 9;
            this.lblNoAccounts.Text = "You currently have no Eve accounts as part of this report group, click \'New Accou" +
                "nt\' to add one.";
            this.lblNoAccounts.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblNoChars
            // 
            this.lblNoChars.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNoChars.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.lblNoChars.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNoChars.Location = new System.Drawing.Point(18, 288);
            this.lblNoChars.Name = "lblNoChars";
            this.lblNoChars.Size = new System.Drawing.Size(856, 57);
            this.lblNoChars.TabIndex = 10;
            this.lblNoChars.Text = "Select one or more accounts above to see the characters that are involved.";
            this.lblNoChars.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CorprGrid
            // 
            this.CorprGrid.AllowUserToAddRows = false;
            this.CorprGrid.AllowUserToDeleteRows = false;
            this.CorprGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.CorprGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.CorprGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.corpIDColumn_Corp,
            this.corpNameColumn_Corp,
            this.corpIncludedColumn_Corp,
            this.charNameColumn_corp});
            this.CorprGrid.Location = new System.Drawing.Point(12, 422);
            this.CorprGrid.Name = "CorprGrid";
            this.CorprGrid.RowHeadersVisible = false;
            this.CorprGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.CorprGrid.Size = new System.Drawing.Size(865, 130);
            this.CorprGrid.TabIndex = 11;
            // 
            // corpIDColumn_Corp
            // 
            this.corpIDColumn_Corp.HeaderText = "Corp ID";
            this.corpIDColumn_Corp.Name = "corpIDColumn_Corp";
            this.corpIDColumn_Corp.ReadOnly = true;
            // 
            // corpNameColumn_Corp
            // 
            this.corpNameColumn_Corp.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.corpNameColumn_Corp.HeaderText = "Corp Name";
            this.corpNameColumn_Corp.Name = "corpNameColumn_Corp";
            this.corpNameColumn_Corp.ReadOnly = true;
            // 
            // corpIncludedColumn_Corp
            // 
            this.corpIncludedColumn_Corp.HeaderText = "Corp Included";
            this.corpIncludedColumn_Corp.Name = "corpIncludedColumn_Corp";
            // 
            // charNameColumn_corp
            // 
            this.charNameColumn_corp.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.charNameColumn_corp.HeaderText = "Character Name";
            this.charNameColumn_corp.Name = "charNameColumn_corp";
            this.charNameColumn_corp.ReadOnly = true;
            // 
            // lblNoCorp
            // 
            this.lblNoCorp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNoCorp.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.lblNoCorp.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNoCorp.Location = new System.Drawing.Point(12, 445);
            this.lblNoCorp.Name = "lblNoCorp";
            this.lblNoCorp.Size = new System.Drawing.Size(856, 52);
            this.lblNoCorp.TabIndex = 12;
            this.lblNoCorp.Text = "Select one or more accounts above to see the Corporations that are involved.";
            this.lblNoCorp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ReportGroupSetup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(889, 616);
            this.Controls.Add(this.lblNoCorp);
            this.Controls.Add(this.CorprGrid);
            this.Controls.Add(this.lblNoChars);
            this.Controls.Add(this.lblNoAccounts);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnDeleteAccount);
            this.Controls.Add(this.btnNewAccount);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.charsAndCorpsGrid);
            this.Controls.Add(this.eveAccountsGrid);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ReportGroupSetup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Report Group Setup";
            this.Load += new System.EventHandler(this.ReportGroupSetup_Load);
            ((System.ComponentModel.ISupportInitialize)(this.eveAccountsGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.charsAndCorpsGrid)).EndInit();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.CorprGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView eveAccountsGrid;
        private System.Windows.Forms.DataGridView charsAndCorpsGrid;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnNewAccount;
        private System.Windows.Forms.Button btnDeleteAccount;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblNoAccounts;
        private System.Windows.Forms.Label lblNoChars;
        private System.Windows.Forms.DataGridViewTextBoxColumn UserIDColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ApiKeyColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn charIDColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn charNameColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn charIncludedColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn corpIDColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn corpNameColumn;
        private System.Windows.Forms.DataGridView CorprGrid;
        private System.Windows.Forms.Label lblNoCorp;
        private System.Windows.Forms.DataGridViewTextBoxColumn corpIDColumn_Corp;
        private System.Windows.Forms.DataGridViewTextBoxColumn corpNameColumn_Corp;
        private System.Windows.Forms.DataGridViewCheckBoxColumn corpIncludedColumn_Corp;
        private System.Windows.Forms.DataGridViewTextBoxColumn charNameColumn_corp;
    }
}