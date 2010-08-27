using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;

using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class Login : Form
    {
        private bool _creatingNewAccount = false;
        private bool _ignoreChange = false;

        public Login()
        {
            InitializeComponent();
            SetMode(false);

            chkAutomatic.CheckedChanged += new EventHandler(chkAutomatic_CheckedChanged);

            /*if (firstRun)
            {
                MessageBox.Show("Welcome to EMMA.\r\nTo learn how to use the program, you must first create " +
                    "a new account.",
                    "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }*/
        }

        private void SetMode(bool creatingNewAccount) 
        {
            _creatingNewAccount = creatingNewAccount;
            if (creatingNewAccount)
            {
                lblPasswordAgain.Visible = true;
                txtPasswordAgain.Visible = true;
                cmdNew.Visible = false;
                txtUsername.Text = "";
                txtPassword.Text = "";
                txtPasswordAgain.Text = "";
                chkAutomatic.Checked = false;
                chkAutomatic.Visible = false;
                txtUsername.Focus();
            }
            else
            {
                lblPasswordAgain.Visible = false;
                txtPasswordAgain.Visible = false;
                cmdNew.Visible = true;
                txtUsername.Text = "";
                txtPassword.Text = "";
                txtPasswordAgain.Text = "";
                chkAutomatic.Visible = true;
                if (File.Exists(Globals.AutoLoginFile))
                {
                    _ignoreChange = true;
                    chkAutomatic.Checked = true;
                    _ignoreChange = false;
                }
                txtUsername.Focus();
            }
        }

        private void chkAutomatic_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutomatic.Checked && !_ignoreChange)
            {
                DialogResult result = MessageBox.Show("Warning. Selecting this option will create a file " +
                    "on your computer with your EMMA password stored in plain text format.\r\n" +
                    "Are you sure you wish to do this?",
                    "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                {
                    chkAutomatic.Checked = false;
                }
            }
        }
        
        private void cmdNew_Click(object sender, EventArgs e)
        {
            SetMode(true);
        }

        private void cmdClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cmdOk_Click(object sender, EventArgs e)
        {
            if (_creatingNewAccount)
            {
                bool createAccount = true;

                if (!txtPassword.Text.Equals(txtPasswordAgain.Text))
                {
                    MessageBox.Show("Error: Specified passwords do not match.", "Error", MessageBoxButtons.OK, 
                        MessageBoxIcon.Error);
                    createAccount = false;
                }

                if (createAccount)
                {
                    try
                    {
                        UserAccount.CreateNewAccount(txtUsername.Text, txtPassword.Text);
                        //ReportGroups.LinkTutorialGroup(txtUsername.Text);

                        MessageBox.Show("New account created successfully. Please log in to start using EMMA.",
                            "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        SetMode(false);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error creating new account: " + ex.Message, "Error", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }

                
            }
            else
            {
                try
                {
                    try
                    {
                        UserAccount.OpenAccount(txtUsername.Text, txtPassword.Text);
                    }
                    catch (EMMALicensingException) 
                    {
                        // This happens if EMMA is not licensed to open the default report group
                        // just continue as normal.
                        // The user will have to choose to use a different group or create a new one.
                    }

                    UpdateAutoLoginFile();

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (EMMAInvalidPasswordException)
                {
                    MessageBox.Show("Error: invalid password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error opening account: " + ex.Message, "Error", MessageBoxButtons.OK, 
                        MessageBoxIcon.Error);
                }
            }
        }

        private void UpdateAutoLoginFile()
        {
            try
            {
                if (File.Exists(Globals.AutoLoginFile))
                {
                    File.Delete(Globals.AutoLoginFile);
                }
                if (chkAutomatic.Checked)
                {
                    // If specified then save the login credentials to the auto login xml file.
                    XmlDocument autoLoginXML = new XmlDocument();
                    XmlDeclaration declNode = autoLoginXML.CreateXmlDeclaration("1.0", "UTF-8", String.Empty);
                    XmlComment commentNode = autoLoginXML.CreateComment("EMMA Auto Login File");
                    XmlElement settingsNode = autoLoginXML.CreateElement("Settings");

                    autoLoginXML.AppendChild(declNode);
                    autoLoginXML.AppendChild(commentNode);
                    autoLoginXML.AppendChild(settingsNode);

                    XmlNode usernameNode = autoLoginXML.CreateElement("Username");
                    settingsNode.AppendChild(usernameNode);
                    XmlText usernameValue = autoLoginXML.CreateTextNode(txtUsername.Text);
                    usernameNode.AppendChild(usernameValue);
                    XmlNode passwordNode = autoLoginXML.CreateElement("Password");
                    settingsNode.AppendChild(passwordNode);
                    XmlText passwordValue = autoLoginXML.CreateTextNode(txtPassword.Text);
                    passwordNode.AppendChild(passwordValue);

                    autoLoginXML.Save(Globals.AutoLoginFile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update auto login settings file: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




    }
}