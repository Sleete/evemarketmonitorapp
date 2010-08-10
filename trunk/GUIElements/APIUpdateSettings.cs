using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using EveMarketMonitorApp.DatabaseClasses;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class APIUpdateSettings : Form
    {
        public APIUpdateSettings()
        {
            InitializeComponent();
            this.DialogResult = DialogResult.Cancel;
        }

        private void APIUpdateSettings_Load(object sender, EventArgs e)
        {
            txtAssetsHours.Text = UserAccount.Settings.APIAssetUpdatePeriod.Hours.ToString();
            txtAssetsMins.Text = UserAccount.Settings.APIAssetUpdatePeriod.Minutes.ToString();
            if (UserAccount.Settings.APIAssetUpdatePeriod.Hours < 23 ||
                (UserAccount.Settings.APIAssetUpdatePeriod.Hours == 23 &&
                UserAccount.Settings.APIAssetUpdatePeriod.Minutes <= 0))
            {
                txtAssetsHours.Text = "23";
                txtAssetsMins.Text = "1";
            }
            txtJournHours.Text = UserAccount.Settings.APIJournUpdatePeriod.Hours.ToString();
            txtJournMins.Text = UserAccount.Settings.APIJournUpdatePeriod.Minutes.ToString();
            if (UserAccount.Settings.APIJournUpdatePeriod.Hours < 1 ||
                (UserAccount.Settings.APIJournUpdatePeriod.Hours == 1 &&
                UserAccount.Settings.APIJournUpdatePeriod.Minutes <= 0))
            {
                txtJournHours.Text = "1";
                txtJournMins.Text = "1";
            }
            txtOrdersHours.Text = UserAccount.Settings.APIOrderUpdatePeriod.Hours.ToString();
            txtOrdersMins.Text = UserAccount.Settings.APIOrderUpdatePeriod.Minutes.ToString();
            if (UserAccount.Settings.APIOrderUpdatePeriod.Hours < 1 ||
                (UserAccount.Settings.APIOrderUpdatePeriod.Hours == 1 &&
                UserAccount.Settings.APIOrderUpdatePeriod.Minutes <= 0))
            {
                txtOrdersHours.Text = "1";
                txtOrdersMins.Text = "1";
            }
            txtTransHours.Text = UserAccount.Settings.APITransUpdatePeriod.Hours.ToString();
            txtTransMins.Text = UserAccount.Settings.APITransUpdatePeriod.Minutes.ToString();
            if (UserAccount.Settings.APITransUpdatePeriod.Hours < 1 ||
                (UserAccount.Settings.APITransUpdatePeriod.Hours == 1 &&
                UserAccount.Settings.APITransUpdatePeriod.Minutes <= 0))
            {
                txtTransHours.Text = "1";
                txtTransMins.Text = "1";
            }
            txtAssetsUpdateMaxMinutes.Text = UserAccount.Settings.AssetsUpdateMaxMinutes.ToString();
            chkManufacturing.Checked = UserAccount.Settings.ManufacturingMode;
            chkAllowIndividualUpdate.Checked = UserAccount.Settings.APIIndividualUpdate;
            chkCompactView.Checked = UserAccount.Settings.UseCompactUpdatePanel;
            RefreshDisplay();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (ValidateValues())
            {
                UserAccount.Settings.APIAssetUpdatePeriod = new TimeSpan(int.Parse(txtAssetsHours.Text),
                    int.Parse(txtAssetsMins.Text), 0);
                UserAccount.Settings.APIJournUpdatePeriod = new TimeSpan(int.Parse(txtJournHours.Text),
                    int.Parse(txtJournMins.Text), 0);
                UserAccount.Settings.APIOrderUpdatePeriod = new TimeSpan(int.Parse(txtOrdersHours.Text),
                    int.Parse(txtOrdersMins.Text), 0);
                UserAccount.Settings.APITransUpdatePeriod = new TimeSpan(int.Parse(txtTransHours.Text),
                    int.Parse(txtTransMins.Text), 0);
                UserAccount.Settings.AssetsUpdateMaxMinutes = int.Parse(txtAssetsUpdateMaxMinutes.Text);
                UserAccount.Settings.ManufacturingMode = chkManufacturing.Checked;
                UserAccount.Settings.APIIndividualUpdate = chkAllowIndividualUpdate.Checked;
                UserAccount.Settings.UseCompactUpdatePanel = chkCompactView.Checked;

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private bool ValidateValues()
        {
            bool retVal = true;

            // ----- Asset update timer -----
            try
            {
                int hours = int.Parse(txtAssetsHours.Text);
                int minutes = int.Parse(txtAssetsMins.Text);
                TimeSpan time = new TimeSpan(hours, minutes, 0);

                if (time.CompareTo(new TimeSpan(23, 1, 0)) < 0)
                {
                    retVal = false;
                    txtAssetsHours.Text = "23";
                    txtAssetsMins.Text = "1";
                    MessageBox.Show("Assets cannot be updated more than once every 23 hours.", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch 
            {
                retVal = false;
                txtAssetsHours.Text = "23";
                txtAssetsMins.Text = "1";
                MessageBox.Show("Problem parsing asset update time values.\r\n" + 
                    "update time has been reset to the default 23 hours.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // ----- Journal update timer -----
            try
            {
                int hours = int.Parse(txtJournHours.Text);
                int minutes = int.Parse(txtJournMins.Text);
                TimeSpan time = new TimeSpan(hours, minutes, 0);

                if (time.CompareTo(new TimeSpan(1, 1, 0)) < 0)
                {
                    retVal = false;
                    txtJournHours.Text = "1";
                    txtJournMins.Text = "1";
                    MessageBox.Show("Journal cannot be updated more than once every hour.", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch
            {
                retVal = false;
                txtJournHours.Text = "1";
                txtJournMins.Text = "1";
                MessageBox.Show("Problem parsing journal update time values.\r\n" +
                    "update time has been reset to the default 1 hour.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // ----- Orders update timer -----
            try
            {
                int hours = int.Parse(txtOrdersHours.Text);
                int minutes = int.Parse(txtOrdersMins.Text);
                TimeSpan time = new TimeSpan(hours, minutes, 0);

                if (time.CompareTo(new TimeSpan(1, 1, 0)) < 0)
                {
                    retVal = false;
                    txtOrdersHours.Text = "1";
                    txtOrdersMins.Text = "1";
                    MessageBox.Show("Orders cannot be updated more than once every hour.", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch
            {
                retVal = false;
                txtOrdersHours.Text = "1";
                txtOrdersMins.Text = "1";
                MessageBox.Show("Problem parsing orders update time values.\r\n" +
                    "update time has been reset to the default 1 hour.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // ----- Transaction update timer -----
            try
            {
                int hours = int.Parse(txtTransHours.Text);
                int minutes = int.Parse(txtTransMins.Text);
                TimeSpan time = new TimeSpan(hours, minutes, 0);

                if (time.CompareTo(new TimeSpan(1, 1, 0)) < 0)
                {
                    retVal = false;
                    txtTransHours.Text = "1";
                    txtTransMins.Text = "1";
                    MessageBox.Show("Transactions cannot be updated more than once every hour.", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch
            {
                retVal = false;
                txtTransHours.Text = "1";
                txtTransMins.Text = "1";
                MessageBox.Show("Problem parsing transactions update time values.\r\n" +
                    "update time has been reset to the default 1 hour.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
             // ----- Assets update max miuntes setting -----
            try
            {
                int mins = int.Parse(txtAssetsUpdateMaxMinutes.Text);
            }
            catch
            {
                retVal = false;
                txtAssetsUpdateMaxMinutes.Text = "10";
                MessageBox.Show("Problem parsing assets update maximum minutes value.\r\n" +
                    "value has been reset to the default 10 minutes.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return retVal;
        }

        private void chkCompactView_CheckedChanged(object sender, EventArgs e)
        {
            RefreshDisplay();
        }


        private void RefreshDisplay()
        {
            if (chkCompactView.Checked)
            {
                chkAllowIndividualUpdate.Checked = false;
                chkAllowIndividualUpdate.Enabled = false;
            }
            else
            {
                chkAllowIndividualUpdate.Enabled = true;
            }
        }


    }
}