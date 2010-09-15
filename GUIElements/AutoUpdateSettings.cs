using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections.Specialized;

using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.DatabaseClasses;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class AutoUpdateSettings : Form
    {
        Main _main = null;

        public AutoUpdateSettings(Main mainForm)
        {
            InitializeComponent();
            this.DialogResult = DialogResult.Cancel;
            _main = mainForm;
            chkAutoUpdate.Checked = Properties.Settings.Default.AutoUpdate;
            chkDocUpdates.Checked = Properties.Settings.Default.DoDocCheck;
            chkBeta.Checked = Properties.Settings.Default.BetaUpdates;
            foreach (string server in Properties.Settings.Default.UpdateServers)
            {
                lstServers.Items.Add(server);
            }
            //btnUpdate.Enabled = !Globals.EMMAUpdateServer.Equals("");
            if (UserAccount.Settings.UseLocalTimezone)
            {
                rdbLocalTime.Checked = true;
            }
            else
            {
                rdbEveTime.Checked = true;
            }
            chkGridCalcEnabled.Checked = UserAccount.Settings.GridCalcEnabled;
            cmbAssetsViewWarning.Text = UserAccount.Settings.AssetsViewWarning;
            chkShowInTaskbar.Checked = UserAccount.Settings.ShowInTaskbarWhenMinimised;
            chkExtendedDiags.Checked = UserAccount.Settings.ExtendedDiagnostics;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.AutoUpdate = chkAutoUpdate.Checked;
            Properties.Settings.Default.DoDocCheck = chkDocUpdates.Checked;
            Properties.Settings.Default.BetaUpdates = chkBeta.Checked;
            StringCollection updateServers = new StringCollection();
            foreach (object item in lstServers.Items)
            {
                updateServers.Add(item.ToString());
            }
            Properties.Settings.Default.UpdateServers = updateServers;
            Properties.Settings.Default.Save();
            UserAccount.Settings.UseLocalTimezone = rdbLocalTime.Checked;
            UserAccount.Settings.GridCalcEnabled = chkGridCalcEnabled.Checked;
            UserAccount.Settings.AssetsViewWarning = cmbAssetsViewWarning.Text;
            UserAccount.Settings.ShowInTaskbarWhenMinimised = chkShowInTaskbar.Checked;
            UserAccount.Settings.ExtendedDiagnostics = chkExtendedDiags.Checked;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            AutoUpdateCheck checker = new AutoUpdateCheck(AppDomain.CurrentDomain.BaseDirectory,
                Globals.EMMAUpdateServer, Properties.Settings.Default.BetaUpdates);
            if (checker.UpdateNeeded)
            {
                checker.ShowDialog();
            }
            else
            {
                MessageBox.Show("No updates are currently available.", "Notification",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            /*if (!_main.CheckForUpdatesInProg())
            {
                if (MessageBox.Show("Running the updater will cause EMMA to close if updates are available." +
                    "Are you sure you wish to continue?", "Warning", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    string exeFile = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "AutoUpdater.exe";
                    if (File.Exists(exeFile))
                    {
                        string tmpDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                            Path.DirectorySeparatorChar + "EMMA";
                        string exeTmp = tmpDir + Path.DirectorySeparatorChar + "AutoUpdater.exe";
                        if (Directory.Exists(tmpDir))
                        {
                            Directory.Delete(tmpDir, true);
                        }
                        Directory.CreateDirectory(tmpDir);
                        File.Copy(exeFile, exeTmp);

                        string parameters = "/p " + System.Diagnostics.Process.GetCurrentProcess().Id +
                            " /s " + Globals.EMMAUpdateServer +
                            " /b " + Properties.Settings.Default.BetaUpdates.ToString() +
                            " /h \"" + AppDomain.CurrentDomain.BaseDirectory + "\"";
                        System.Diagnostics.Process updateProcess = System.Diagnostics.Process.Start(exeTmp, parameters);
                        while (!updateProcess.HasExited) { }

                        MessageBox.Show("No updates are currently available.", "Notification",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Updater program cannot be found.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }*/
        }

        private void btnServerDefaults_Click(object sender, EventArgs e)
        {
            lstServers.Items.Clear();
            lstServers.Items.Add("www.starfreeze.com");
            lstServers.Items.Add("www.eve-files.com");
        }

        private void lstServers_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && lstServers.SelectedItem != null)
            {
                lstServers.Items.Remove(lstServers.SelectedItem);
            }
        }

        private void btnDocUpdates_Click(object sender, EventArgs e)
        {
            if (NewDocumentationForm.DocUpdateAvailable())
            {
                NewDocumentationForm form = new NewDocumentationForm();
                form.ShowDialog();
            }
            else
            {
                MessageBox.Show("No documentation updates currently available");
            }
        }


    }
}