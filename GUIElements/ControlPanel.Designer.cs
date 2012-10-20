namespace EveMarketMonitorApp.GUIElements
{
    partial class ControlPanel
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
            this.btnManageGroup = new System.Windows.Forms.Button();
            this.btnChangeGroup = new System.Windows.Forms.Button();
            this.btnLogout = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnQuit = new System.Windows.Forms.Button();
            this.settingsMenuTimer = new System.Windows.Forms.Timer(this.components);
            this.btnTutorial = new System.Windows.Forms.Button();
            this.btnLicense = new System.Windows.Forms.Button();
            this.btnVerifyDB = new System.Windows.Forms.Button();
            this.btnAbout = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnManageGroup
            // 
            this.btnManageGroup.Location = new System.Drawing.Point(12, 12);
            this.btnManageGroup.Name = "btnManageGroup";
            this.btnManageGroup.Size = new System.Drawing.Size(140, 29);
            this.btnManageGroup.TabIndex = 0;
            this.btnManageGroup.Text = "&Manage Group";
            this.btnManageGroup.UseVisualStyleBackColor = true;
            this.btnManageGroup.Click += new System.EventHandler(this.Button_Click);
            // 
            // btnChangeGroup
            // 
            this.btnChangeGroup.Location = new System.Drawing.Point(12, 47);
            this.btnChangeGroup.Name = "btnChangeGroup";
            this.btnChangeGroup.Size = new System.Drawing.Size(140, 29);
            this.btnChangeGroup.TabIndex = 1;
            this.btnChangeGroup.Text = "Select &Group";
            this.btnChangeGroup.UseVisualStyleBackColor = true;
            this.btnChangeGroup.Click += new System.EventHandler(this.Button_Click);
            // 
            // btnLogout
            // 
            this.btnLogout.Location = new System.Drawing.Point(12, 292);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(140, 29);
            this.btnLogout.TabIndex = 5;
            this.btnLogout.Text = "&Logout User";
            this.btnLogout.UseVisualStyleBackColor = true;
            this.btnLogout.Click += new System.EventHandler(this.Button_Click);
            // 
            // btnSettings
            // 
            this.btnSettings.Location = new System.Drawing.Point(12, 82);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(140, 29);
            this.btnSettings.TabIndex = 2;
            this.btnSettings.Text = "&Settings";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(12, 152);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(140, 29);
            this.btnImport.TabIndex = 3;
            this.btnImport.Text = "I&mport Data";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.Button_Click);
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(12, 187);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(140, 29);
            this.btnExport.TabIndex = 4;
            this.btnExport.Text = "E&xport Data";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.Button_Click);
            // 
            // btnQuit
            // 
            this.btnQuit.Location = new System.Drawing.Point(12, 362);
            this.btnQuit.Name = "btnQuit";
            this.btnQuit.Size = new System.Drawing.Size(140, 29);
            this.btnQuit.TabIndex = 6;
            this.btnQuit.Text = "&Quit";
            this.btnQuit.UseVisualStyleBackColor = true;
            this.btnQuit.Click += new System.EventHandler(this.Button_Click);
            // 
            // settingsMenuTimer
            // 
            this.settingsMenuTimer.Interval = 2;
            this.settingsMenuTimer.Tick += new System.EventHandler(this.settingsMenuTimer_Tick);
            // 
            // btnTutorial
            // 
            this.btnTutorial.Location = new System.Drawing.Point(12, 117);
            this.btnTutorial.Name = "btnTutorial";
            this.btnTutorial.Size = new System.Drawing.Size(140, 29);
            this.btnTutorial.TabIndex = 7;
            this.btnTutorial.Text = "T&utorial";
            this.btnTutorial.UseVisualStyleBackColor = true;
            this.btnTutorial.Click += new System.EventHandler(this.Button_Click);
            // 
            // btnLicense
            // 
            this.btnLicense.Location = new System.Drawing.Point(12, 222);
            this.btnLicense.Name = "btnLicense";
            this.btnLicense.Size = new System.Drawing.Size(140, 29);
            this.btnLicense.TabIndex = 8;
            this.btnLicense.Text = "Lice&nse";
            this.btnLicense.UseVisualStyleBackColor = true;
            this.btnLicense.Click += new System.EventHandler(this.Button_Click);
            // 
            // btnVerifyDB
            // 
            this.btnVerifyDB.Location = new System.Drawing.Point(12, 257);
            this.btnVerifyDB.Name = "btnVerifyDB";
            this.btnVerifyDB.Size = new System.Drawing.Size(140, 29);
            this.btnVerifyDB.TabIndex = 9;
            this.btnVerifyDB.Text = "Check/Repair Data";
            this.btnVerifyDB.UseVisualStyleBackColor = true;
            this.btnVerifyDB.Click += new System.EventHandler(this.Button_Click);
            // 
            // btnAbout
            // 
            this.btnAbout.Location = new System.Drawing.Point(12, 327);
            this.btnAbout.Name = "btnAbout";
            this.btnAbout.Size = new System.Drawing.Size(140, 29);
            this.btnAbout.TabIndex = 10;
            this.btnAbout.Text = "A&bout";
            this.btnAbout.UseVisualStyleBackColor = true;
            this.btnAbout.Click += new System.EventHandler(this.Button_Click);
            // 
            // ControlPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(164, 403);
            this.Controls.Add(this.btnAbout);
            this.Controls.Add(this.btnVerifyDB);
            this.Controls.Add(this.btnLicense);
            this.Controls.Add(this.btnTutorial);
            this.Controls.Add(this.btnQuit);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.btnLogout);
            this.Controls.Add(this.btnChangeGroup);
            this.Controls.Add(this.btnManageGroup);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ControlPanel";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Control Panel";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnManageGroup;
        private System.Windows.Forms.Button btnChangeGroup;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnQuit;
        private System.Windows.Forms.Timer settingsMenuTimer;
        private System.Windows.Forms.Button btnTutorial;
        private System.Windows.Forms.Button btnLicense;
        private System.Windows.Forms.Button btnVerifyDB;
        private System.Windows.Forms.Button btnAbout;
    }
}