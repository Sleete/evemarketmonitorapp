using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using EveMarketMonitorApp.AbstractionClasses;
using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.GUIElements
{
    public delegate void HideSettings(object myObject, EventArgs args);
    
    public partial class SettingsMenu : Form
    {
        private bool _allowClose = false;
        private ControlPanel _cp;

        public event HideSettings Retract;

        public bool AllowClose
        {
            get { return _allowClose; }
            set { _allowClose = value; }
        }
        
        public SettingsMenu()
        {
            InitializeComponent();
            btnStandings.Enabled = !Globals.EveAPIDown;
        }

        public void SetControlPanel(ControlPanel cp)
        {
            _cp = cp;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_cp != null) { _cp.Focus(); }
            // Never allow user to close this. 
            // If the close command has come from the parent window then we need to allow it though.
            e.Cancel = !_allowClose;
            if (!_allowClose && Retract != null)
            {
                // Fire a retract event rather than closing the form
                Retract(this, new EventArgs());
            }
            base.OnClosing(e);
        }

        private void btnReportStyleSettings_Click(object sender, EventArgs e)
        {
            if (_cp != null) { _cp.Focus(); }
            ReportStyleSettings rss = new ReportStyleSettings();
            rss.ShowDialog();
        }

        private void btnItemsTradedSettings_Click(object sender, EventArgs e)
        {
            if (_cp != null) { _cp.Focus(); }
            ItemValuesManager sit = new ItemValuesManager();
            sit.ShowDialog();
        }

        private void btnStandings_Click(object sender, EventArgs e)
        {
            if (_cp != null) { _cp.Focus(); }
            Cursor = Cursors.WaitCursor;
            try
            {
                foreach (EVEAccount account in UserAccount.CurrentGroup.Accounts)
                {
                    foreach (APICharacter character in account.Chars)
                    {
                        if (character.CharIncWithRptGroup) { character.UpdateStandings(CharOrCorp.Char); }
                        if (character.CorpIncWithRptGroup) { character.UpdateStandings(CharOrCorp.Corp); }
                    }
                }
                MessageBox.Show("Standings refresh complete.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnCourierSettings_Click(object sender, EventArgs e)
        {
            if (_cp != null) { _cp.Focus(); }
            CourierSettings courierSettings = new CourierSettings();
            courierSettings.ShowDialog();
        }

        private void btnLocations_Click(object sender, EventArgs e)
        {
            if (_cp != null) { _cp.Focus(); }
            GroupLocationList grpLoc = new GroupLocationList();
            grpLoc.ShowDialog();
        }

        private void btnOrderNotifySettings_Click(object sender, EventArgs e)
        {
            if (_cp != null) { _cp.Focus(); }
            OrderNotificationSettings settings = new OrderNotificationSettings();
            settings.ShowDialog();
        }

        private void btnAutopilot_Click(object sender, EventArgs e)
        {
            if (_cp != null) { _cp.Focus(); }
            RouteCalcSettings settings = new RouteCalcSettings();
            settings.ShowDialog();
        }

        private void btnAutoUpdateSettings_Click(object sender, EventArgs e)
        {
            if (_cp != null) { _cp.Focus(); }
            AutoUpdateSettings settings = new AutoUpdateSettings((Main)MdiParent);
            settings.ShowDialog();
        }

        private void btnAPIUpdateSettings_Click(object sender, EventArgs e)
        {
            if (_cp != null) { _cp.Focus(); }
            APIUpdateSettings settings = new APIUpdateSettings();
            settings.ShowDialog();
            Main mainForm = this.MdiParent as Main;
            if (mainForm != null)
            {
                mainForm.RebuildUpdatePanel();
            }
        }

        private void btnReprocessSettings_Click(object sender, EventArgs e)
        {
            if (_cp != null) { _cp.Focus(); }
            ReprocessSettings settings = new ReprocessSettings();
            settings.ShowDialog();
        }

        private void btnTradedItems_Click(object sender, EventArgs e)
        {
            if (_cp != null) { _cp.Focus(); }
            TradedItemsManager settings = new TradedItemsManager();
            settings.ShowDialog();
        }

    }
}