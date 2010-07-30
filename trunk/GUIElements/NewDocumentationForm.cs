using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class NewDocumentationForm : Form
    {
        // We just use a hardcoded 'version' number.
        // On the one hand, this means that a code change is required for EMMA to recognise that
        // documentation has been updated.
        // ON the other, this avoids slow calls to the web and doc changes are only likely after
        // a code update anyway.
        private static int CURRENTDOCVERSION = 2;

        public NewDocumentationForm()
        {
            InitializeComponent();
        }

        public static bool DocUpdateAvailable()
        {
            bool retVal = false;
            if (EveMarketMonitorApp.Properties.Settings.Default.DocumentationVersion <
                NewDocumentationForm.CURRENTDOCVERSION)
            {
                retVal = true;
            }

            return retVal;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.Links[linkLabel1.Links.IndexOf(e.Link)].Visited = true;

            string target = @"http://code.google.com/p/evemarketmonitorapp/downloads/list";
            System.Diagnostics.Process.Start(target);
        }

        private void NewDocumentationForm_Load(object sender, EventArgs e)
        {
            chkDontShow.Checked = !EveMarketMonitorApp.Properties.Settings.Default.DoDocCheck;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void NewDocumentationForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            EveMarketMonitorApp.Properties.Settings.Default.DocumentationVersion = 
                NewDocumentationForm.CURRENTDOCVERSION;
            EveMarketMonitorApp.Properties.Settings.Default.DoDocCheck = !chkDontShow.Checked;
            EveMarketMonitorApp.Properties.Settings.Default.Save();
        }
    }
}
