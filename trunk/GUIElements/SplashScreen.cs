using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class SplashScreen : Form
    {
        private string _justStatus = "";
        private int _numDits = 0;
        private int _maxDits = 4;

        private static StatusChangeArgs _status;
        private IProvideStatus _progressObject;

        delegate void RefreshProgress();
        delegate void ShowMessageCallback(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon);

        public SplashScreen(IProvideStatus progressObject)
        {
            _progressObject = progressObject;
            _progressObject.StatusChange += new StatusChangeHandler(progressObject_OnStatusChange);

            InitializeComponent();
            lblStatus.Text = "";
            _status = new StatusChangeArgs(0, 0, "", "", false);
        }

        private void SplashScreen_Load(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            timer1.Start();
            UpdateDialog();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this._progressObject.StatusChange -= new StatusChangeHandler(progressObject_OnStatusChange);
            timer1.Stop();
            base.OnClosing(e);
        }

        void progressObject_OnStatusChange(object myObject, StatusChangeArgs args)
        {
            _status = args;
            if (this.Visible == true)
            {
                UpdateDialog();
            }
        }

        private void UpdateDialog()
        {
            if (this.InvokeRequired)
            {
                RefreshProgress callback = new RefreshProgress(UpdateDialog);
                this.Invoke(callback);
            }
            else
            {
                _numDits = 0;
                _justStatus = _status.Section +
                    (_status.SectionStatus.Equals("") ? "" : " - " + _status.SectionStatus);
                lblStatus.Text = _justStatus;

                if (_status.Done)
                {
                    this.Close();
                }
            }
        }

        public void ShowMessage(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            if (this.InvokeRequired)
            {
                ShowMessageCallback callback = new ShowMessageCallback(ShowMessage);
                this.Invoke(callback, new object[] {message, caption, buttons, icon});
            }
            else
            {
                MessageBox.Show(message, caption, buttons, icon);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            StringBuilder str = new StringBuilder(_justStatus);
            _numDits += 1;
            if (_numDits > _maxDits) _numDits = 0;
            for (int i = 0; i < _numDits; i++)
            {
                str.Append(".");
            }
            lblStatus.Text = str.ToString();
        }
       

    }
}