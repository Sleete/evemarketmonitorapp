namespace EveMarketMonitorApp.GUIElements
{
    partial class SettingsMenu
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsMenu));
            this.btnReportStyleSettings = new System.Windows.Forms.Button();
            this.btnItemsTradedSettings = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnStandings = new System.Windows.Forms.Button();
            this.btnCourierSettings = new System.Windows.Forms.Button();
            this.btnLocations = new System.Windows.Forms.Button();
            this.btnOrderNotifySettings = new System.Windows.Forms.Button();
            this.btnAutopilot = new System.Windows.Forms.Button();
            this.btnAutoUpdateSettings = new System.Windows.Forms.Button();
            this.btnAPIUpdateSettings = new System.Windows.Forms.Button();
            this.btnReprocessSettings = new System.Windows.Forms.Button();
            this.btnTradedItems = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnReportStyleSettings
            // 
            this.btnReportStyleSettings.Location = new System.Drawing.Point(12, 12);
            this.btnReportStyleSettings.Name = "btnReportStyleSettings";
            this.btnReportStyleSettings.Size = new System.Drawing.Size(140, 23);
            this.btnReportStyleSettings.TabIndex = 0;
            this.btnReportStyleSettings.Text = "Report Style Settings";
            this.toolTip1.SetToolTip(this.btnReportStyleSettings, "Configure fonts, colours and even the logo used when producing reports");
            this.btnReportStyleSettings.UseVisualStyleBackColor = true;
            this.btnReportStyleSettings.Click += new System.EventHandler(this.btnReportStyleSettings_Click);
            // 
            // btnItemsTradedSettings
            // 
            this.btnItemsTradedSettings.Location = new System.Drawing.Point(12, 41);
            this.btnItemsTradedSettings.Name = "btnItemsTradedSettings";
            this.btnItemsTradedSettings.Size = new System.Drawing.Size(140, 23);
            this.btnItemsTradedSettings.TabIndex = 1;
            this.btnItemsTradedSettings.Text = "Item Values";
            this.toolTip1.SetToolTip(this.btnItemsTradedSettings, "Specify which items you trade and default prices");
            this.btnItemsTradedSettings.UseMnemonic = false;
            this.btnItemsTradedSettings.UseVisualStyleBackColor = true;
            this.btnItemsTradedSettings.Click += new System.EventHandler(this.btnItemsTradedSettings_Click);
            // 
            // btnStandings
            // 
            this.btnStandings.Location = new System.Drawing.Point(11, 302);
            this.btnStandings.Name = "btnStandings";
            this.btnStandings.Size = new System.Drawing.Size(140, 23);
            this.btnStandings.TabIndex = 2;
            this.btnStandings.Text = "Refresh Standings";
            this.toolTip1.SetToolTip(this.btnStandings, "Update standings for the current characters and corps");
            this.btnStandings.UseVisualStyleBackColor = true;
            this.btnStandings.Click += new System.EventHandler(this.btnStandings_Click);
            // 
            // btnCourierSettings
            // 
            this.btnCourierSettings.Location = new System.Drawing.Point(11, 99);
            this.btnCourierSettings.Name = "btnCourierSettings";
            this.btnCourierSettings.Size = new System.Drawing.Size(140, 23);
            this.btnCourierSettings.TabIndex = 3;
            this.btnCourierSettings.Text = "Courier Settings";
            this.toolTip1.SetToolTip(this.btnCourierSettings, "Courier contract settings");
            this.btnCourierSettings.UseVisualStyleBackColor = true;
            this.btnCourierSettings.Click += new System.EventHandler(this.btnCourierSettings_Click);
            // 
            // btnLocations
            // 
            this.btnLocations.Location = new System.Drawing.Point(11, 128);
            this.btnLocations.Name = "btnLocations";
            this.btnLocations.Size = new System.Drawing.Size(140, 23);
            this.btnLocations.TabIndex = 4;
            this.btnLocations.Text = "Group Locations";
            this.toolTip1.SetToolTip(this.btnLocations, "Create and edit report group locations");
            this.btnLocations.UseVisualStyleBackColor = true;
            this.btnLocations.Click += new System.EventHandler(this.btnLocations_Click);
            // 
            // btnOrderNotifySettings
            // 
            this.btnOrderNotifySettings.Location = new System.Drawing.Point(11, 157);
            this.btnOrderNotifySettings.Name = "btnOrderNotifySettings";
            this.btnOrderNotifySettings.Size = new System.Drawing.Size(140, 23);
            this.btnOrderNotifySettings.TabIndex = 5;
            this.btnOrderNotifySettings.Text = "Order Notification Settings";
            this.toolTip1.SetToolTip(this.btnOrderNotifySettings, "Settings for market order notifications");
            this.btnOrderNotifySettings.UseVisualStyleBackColor = true;
            this.btnOrderNotifySettings.Click += new System.EventHandler(this.btnOrderNotifySettings_Click);
            // 
            // btnAutopilot
            // 
            this.btnAutopilot.Location = new System.Drawing.Point(11, 215);
            this.btnAutopilot.Name = "btnAutopilot";
            this.btnAutopilot.Size = new System.Drawing.Size(140, 23);
            this.btnAutopilot.TabIndex = 6;
            this.btnAutopilot.Text = "Autopilot Settings";
            this.toolTip1.SetToolTip(this.btnAutopilot, "Settings for the autopilot used by the route planner tool");
            this.btnAutopilot.UseVisualStyleBackColor = true;
            this.btnAutopilot.Click += new System.EventHandler(this.btnAutopilot_Click);
            // 
            // btnAutoUpdateSettings
            // 
            this.btnAutoUpdateSettings.Location = new System.Drawing.Point(11, 273);
            this.btnAutoUpdateSettings.Name = "btnAutoUpdateSettings";
            this.btnAutoUpdateSettings.Size = new System.Drawing.Size(140, 23);
            this.btnAutoUpdateSettings.TabIndex = 7;
            this.btnAutoUpdateSettings.Text = "Other Settings";
            this.toolTip1.SetToolTip(this.btnAutoUpdateSettings, "Automatic update settings");
            this.btnAutoUpdateSettings.UseVisualStyleBackColor = true;
            this.btnAutoUpdateSettings.Click += new System.EventHandler(this.btnAutoUpdateSettings_Click);
            // 
            // btnAPIUpdateSettings
            // 
            this.btnAPIUpdateSettings.Location = new System.Drawing.Point(11, 186);
            this.btnAPIUpdateSettings.Name = "btnAPIUpdateSettings";
            this.btnAPIUpdateSettings.Size = new System.Drawing.Size(140, 23);
            this.btnAPIUpdateSettings.TabIndex = 8;
            this.btnAPIUpdateSettings.Text = "API Update Settings";
            this.toolTip1.SetToolTip(this.btnAPIUpdateSettings, "Update standings for the current characters and corps");
            this.btnAPIUpdateSettings.UseVisualStyleBackColor = true;
            this.btnAPIUpdateSettings.Click += new System.EventHandler(this.btnAPIUpdateSettings_Click);
            // 
            // btnReprocessSettings
            // 
            this.btnReprocessSettings.Location = new System.Drawing.Point(11, 244);
            this.btnReprocessSettings.Name = "btnReprocessSettings";
            this.btnReprocessSettings.Size = new System.Drawing.Size(140, 23);
            this.btnReprocessSettings.TabIndex = 9;
            this.btnReprocessSettings.Text = "Reprocess Settings";
            this.toolTip1.SetToolTip(this.btnReprocessSettings, "Automatic update settings");
            this.btnReprocessSettings.UseVisualStyleBackColor = true;
            this.btnReprocessSettings.Click += new System.EventHandler(this.btnReprocessSettings_Click);
            // 
            // btnTradedItems
            // 
            this.btnTradedItems.Location = new System.Drawing.Point(12, 70);
            this.btnTradedItems.Name = "btnTradedItems";
            this.btnTradedItems.Size = new System.Drawing.Size(140, 23);
            this.btnTradedItems.TabIndex = 10;
            this.btnTradedItems.Text = "Traded Items";
            this.toolTip1.SetToolTip(this.btnTradedItems, "Configure the list of traded items. This is used by the item detail screen, auto " +
                    "contractor, etc");
            this.btnTradedItems.UseVisualStyleBackColor = true;
            this.btnTradedItems.Click += new System.EventHandler(this.btnTradedItems_Click);
            // 
            // SettingsMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(163, 337);
            this.Controls.Add(this.btnTradedItems);
            this.Controls.Add(this.btnReprocessSettings);
            this.Controls.Add(this.btnAPIUpdateSettings);
            this.Controls.Add(this.btnAutoUpdateSettings);
            this.Controls.Add(this.btnAutopilot);
            this.Controls.Add(this.btnOrderNotifySettings);
            this.Controls.Add(this.btnLocations);
            this.Controls.Add(this.btnCourierSettings);
            this.Controls.Add(this.btnStandings);
            this.Controls.Add(this.btnItemsTradedSettings);
            this.Controls.Add(this.btnReportStyleSettings);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsMenu";
            this.Text = "Settings";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnReportStyleSettings;
        private System.Windows.Forms.Button btnItemsTradedSettings;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnStandings;
        private System.Windows.Forms.Button btnCourierSettings;
        private System.Windows.Forms.Button btnLocations;
        private System.Windows.Forms.Button btnOrderNotifySettings;
        private System.Windows.Forms.Button btnAutopilot;
        private System.Windows.Forms.Button btnAutoUpdateSettings;
        private System.Windows.Forms.Button btnAPIUpdateSettings;
        private System.Windows.Forms.Button btnReprocessSettings;
        private System.Windows.Forms.Button btnTradedItems;
    }
}