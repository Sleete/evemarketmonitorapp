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
    public partial class EnterAccountDetails : Form
    {
        private EVEAccount _account = null;
        //private bool _newAccount = false;

        public EVEAccount Account
        {
            get { return _account; }
        }

        public EnterAccountDetails()
        {
            InitializeComponent();
            eveApiKeyLink.Links[0].LinkData = eveApiKeyLink.Text.Substring(eveApiKeyLink.Links[0].Start,
                eveApiKeyLink.Links[0].Length);
        }

        public EnterAccountDetails(EVEAccount account)
        {
            InitializeComponent();
            eveApiKeyLink.Links[0].LinkData = eveApiKeyLink.Text.Substring(eveApiKeyLink.Links[0].Start,
                eveApiKeyLink.Links[0].Length);
            _account = account;
        }

        private void EnterAccountDetails_Load(object sender, EventArgs e)
        {
            if (_account != null)
            {
                txtUserID.Text = _account.UserID.ToString();
                txtUserID.Enabled = false;
                txtApiKey.Text = _account.ApiKey;
            }
        }
        
        private void eveApiKeyLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.eveApiKeyLink.Links[eveApiKeyLink.Links.IndexOf(e.Link)].Visited = true;

            string target = e.Link.LinkData as string;
            System.Diagnostics.Process.Start(target);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                if (_account == null)
                {
                    SetEveAccount();
                }
                _account.ApiKey = txtApiKey.Text;

                try
                {
                    if (_account.VerifyAccount())
                    {
                        this.DialogResult = DialogResult.OK;
                        /*if (_newAccount) {*/ EveAccounts.Store(_account); /*}*/
                        Close();
                    }
                    else
                    {
                        //_newAccount = false;
                        _account = null;
                        MessageBox.Show("Authentication failure: Invalid UserID and/or ApiKey",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (EMMAEveAPIException emmaEx)
                {
                    // Indicates not that the userID/apikey are invalid but that there was an
                    // error trying to validate.
                    string message;
                    message = "Problem validating account details:\r\n" + emmaEx.Message;
                    if (emmaEx.InnerException != null) message += "\r\n" + emmaEx.InnerException.Message;

                    MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                string message;
                message = "Problem setting account details:\r\n" + ex.Message;
                if (ex.InnerException != null) message += "\r\n" + ex.InnerException.Message;

                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        /// <summary>
        /// Set the _account variable to be the account specified by the entered userID.
        /// If it already exists in the EMMA database then retrieve it if the entered apikey 
        /// matches the one in the DB.
        /// If it does not already exist then create a new EVEAccount object using the
        /// userID and apikey enterd. (It will be verified and saved later).
        /// </summary>
        private void SetEveAccount()
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                // First, check if the specified userID is already in the EMMA database..
                int userID = 0;
                try
                {
                    userID = int.Parse(txtUserID.Text);
                }
                catch
                {
                    throw new EMMAException(ExceptionSeverity.Error, "Supplied user ID '" + txtUserID.Text +
                        "' contains invalid characters.");
                }

                _account = EveAccounts.GetAccount(userID);

                // If the userID is not already in the database then set the userID and apikey using
                // the values entered by the user.
                // Otherwise, check if the apikey entered by the user matches the apikey in the
                // database.
                if (_account == null)
                {
                    _account = new EVEAccount(userID, txtApiKey.Text);
                    //_newAccount = true;
                }
                else
                {
                    if (EveAccounts.AccountInUse(userID))
                    {
                        if (!_account.ApiKey.Equals(txtApiKey.Text))
                        {
                            _account = null;
                            throw new EMMAException(ExceptionSeverity.Error, "Supplied user ID '" +
                                txtUserID.Text + "' is already in the database.\r\n" +
                                "However, the supplied APIKey does not match the one in the database.");
                        }
                    }
                    else
                    {
                        _account.ApiKey = txtApiKey.Text;
                    }
                    //_newAccount = false;
                }
            }
            finally 
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }


    }
}