using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace EveMarketMonitorApp.Common
{
    public partial class ProgressDialog : Form, IProgressDialog
    {
        static StatusChangeArgs status;
        List<string> sections;
        string lastSectStatus = "";

        string justSectionStatusText = "";
        int numDits = 0;
        int maxDits = 4;

        IProvideStatus _progressObject;
        delegate void RefreshProgress();

        public ProgressDialog(string caption, IProvideStatus progressObject)
        {
            InitializeComponent();

            this.Text = caption;
            lblSection.Text = "";
            lblSectionStatus.Text = "";
            lstStatusHistory.Items.Clear();
            status = new StatusChangeArgs(0, 0, "", "", false);

            sections = new List<string>();
            // Default to cancelled status so if the user closes the window at any time the code can cancel out
            // of whatever it is doing correctly
            this.DialogResult = DialogResult.Cancel;

            _progressObject = progressObject;
        }

        private void ProgressDialog_Load(object sender, EventArgs e)
        {
            timer1.Start();
            _progressObject.StatusChange += new StatusChangeHandler(progressObject_OnStatusChange);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this._progressObject.StatusChange -= new StatusChangeHandler(progressObject_OnStatusChange);
            timer1.Stop();
            base.OnClosing(e);
        }

        void progressObject_OnStatusChange(object myObject, StatusChangeArgs args)
        {
            status = args;
            UpdateDialog();
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
                if (!status.Section.Equals(""))
                {
                    lblSection.Text = status.Section;
                }
                if (!status.SectionStatus.Equals(""))
                {
                    lblSectionStatus.Text = status.SectionStatus;
                    justSectionStatusText = status.SectionStatus;
                }
                else if(status.MaxProgress > 0)
                {
                    lblSectionStatus.Text = justSectionStatusText + " (" + status.CurrentProgress + "/" +
                        status.MaxProgress + ")";
                }
                numDits = 0;
                if (status.MaxProgress > 0)
                {
                    if (prgProgress.Value > status.MaxProgress) { prgProgress.Value = status.MaxProgress; }
                    prgProgress.Maximum = status.MaxProgress;
                }
                if (status.CurrentProgress >= 0)
                {
                    try
                    {
                        prgProgress.Value = status.CurrentProgress;
                    }
                    catch (ArgumentException)
                    {
                        // If this exception occurs then just leave progress where it is.
                    }
                }
                if (!sections.Contains(status.Section) && !status.Section.Equals(""))
                {
                    lstStatusHistory.Items.Add(status.Section);
                    SetHorizScroll();
                    sections.Add(status.Section);

                    lastSectStatus = "";
                    //lstStatusHistory.SelectedIndex = lstStatusHistory.Items.Count - 1;
                }
                if (!status.SectionStatus.Equals(lastSectStatus) && !status.SectionStatus.Equals(""))
                {
                    lstStatusHistory.Items.Add("\t" + status.SectionStatus);
                    SetHorizScroll();
                    lastSectStatus = status.SectionStatus;
                    //lstStatusHistory.SelectedIndex = lstStatusHistory.Items.Count - 1;
                }

                if (status.Done)
                {
                    timer1.Stop();
                    lblSectionStatus.Text = justSectionStatusText;
                    btnOk.Visible = true;
                }
            }
        }

        private void SetHorizScroll()
        {
            lstStatusHistory.HorizontalScrollbar = true;

            Graphics g = lstStatusHistory.CreateGraphics();

            // Determine the size for HorizontalExtent using the MeasureString method using the last item in the list.
            int hzSize = (int)g.MeasureString(lstStatusHistory.Items[lstStatusHistory.Items.Count - 1].ToString()
                , lstStatusHistory.Font).Width;
            // Set the HorizontalExtent property.
            if (hzSize > lstStatusHistory.HorizontalExtent)
            {
                lstStatusHistory.HorizontalExtent = hzSize;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            StringBuilder str = new StringBuilder(justSectionStatusText);
            numDits += 1;
            if (numDits > maxDits) numDits = 0;
            for (int i = 0; i < numDits; i++)
            {
                str.Append(".");
            }
            lblSectionStatus.Text = str.ToString();
        }

    }
}