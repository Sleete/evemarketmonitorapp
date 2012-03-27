using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.AbstractionClasses;
using EveMarketMonitorApp.Common;

using System.Linq;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class ReportGroupSetup : Form
    {
        List<EVEAccount> _accounts;
        // Note: need to keep a list of removed accounts for resetting the corp/char 
        // 'included' vars if the user cancels.
        List<EVEAccount> _removedAccounts;
        List<APICharacter> _characters;
        ReportGroup _group;

        public ReportGroupSetup()
        {
            InitializeComponent();
            _group = UserAccount.CurrentGroup;
            _removedAccounts = new List<EVEAccount>();
            UpdateCharLists();
        }


        private void ReportGroupSetup_Load(object sender, EventArgs e)
        {
            ShowAccounts();
        }

        private void ShowAccounts()
        {
            if (_accounts == null)
            {
                _accounts = _group.Accounts;
                if (_accounts != null)
                {
                    foreach (EVEAccount account in _accounts)
                    {
                        //try
                        //{
                        //    account.UpdateCharList(false);
                        //}
                        //catch (EMMAEveAPIException apiEx) 
                        //{ 
                        //    // If we get an API error updating the list of characters then let the user know.
                        //    // We still want to allow them to continue because this is most likley due to an
                        //    // API key change and we want to allow them to update it.
                        //    MessageBox.Show("Warning: The list of characters on account '" + account.UserID + 
                        //        "' could not be updated.\r\n" +
                        //        "Eve API error: " + apiEx.EveCode + ", " + apiEx.EveDescription + "\r\n" +
                        //        "This may mean that the list of characters displayed here is not the " +
                        //        "same as the characters that are actually on this account.", "Warning",
                        //        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        //}
                        foreach (APICharacter apiChar in account.Chars)
                        {
                            bool x = apiChar.CharIncWithRptGroup;
                            x = apiChar.CorpIncWithRptGroup;
                        }
                    }
                }
            }

            eveAccountsGrid.AutoGenerateColumns = false;
            eveAccountsGrid.DataSource = null;
            eveAccountsGrid.DataSource = _accounts;
            UserIDColumn.DataPropertyName = "UserID";
            ApiKeyColumn.DataPropertyName = "ApiKey";

            if (_accounts == null || _accounts.Count == 0)
            {
                lblNoAccounts.Visible = true;
            }
            else
            {
                lblNoAccounts.Visible = false;
                eveAccountsGrid.SelectAll();
            }
        }

        private void UpdateCharLists()
        {
            if (_accounts == null)
            {
                _accounts = _group.Accounts;
                if (_accounts != null)
                {
                    foreach (EVEAccount account in _accounts)
                    {
                        try
                        {
                            account.UpdateCharList(true);
                        }
                        catch (EMMAEveAPIException apiEx)
                        {
                            // If we get an API error updating the list of characters then let the user know.
                            // We still want to allow them to continue because this is most likley due to an
                            // API key change and we want to allow them to update it.
                            MessageBox.Show("Warning: The list of characters on account '" + account.UserID +
                                "' could not be updated.\r\n" +
                                "Eve API error: " + apiEx.EveCode + ", " + apiEx.EveDescription + "\r\n" +
                                "This may mean that the list of characters displayed here is not the " +
                                "same as the characters that are actually on this account.", "Warning",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
        }

        private void ShowCharacters()
        {
            if (_characters != null)
            {
                // Check for multiple selections of the same corporation through different
                // characters.
                foreach (APICharacter char1 in _characters)
                {
                    foreach (APICharacter char2 in _characters)
                    {
                        if (char2.CorpID == char1.CorpID && char2.CharID != char1.CharID &&
                            char2.CorpIncWithRptGroup)
                        {
                            char1.CorpIncWithRptGroup = false;
                        }
                    }
                }
            }

            charsAndCorpsGrid.AutoGenerateColumns = false;
            charsAndCorpsGrid.DataSource = _characters.Where(c => c.AccessType == CharOrCorp.Char).ToList<APICharacter>();

            var charsOnly = from cc in _characters
                            where cc.AccessType == CharOrCorp.Char
                            select cc;

            charNameColumn.DataPropertyName = "CharName";
            charIDColumn.DataPropertyName = "CharID";
            charIncludedColumn.DataPropertyName = "CharIncWithRptGroup";
            corpNameColumn.DataPropertyName = "CorpName";
            corpIDColumn.DataPropertyName = "CorpID";
            
            CorprGrid.AutoGenerateColumns = false;
            CorprGrid.DataSource = _characters.Where(c => c.AccessType == CharOrCorp.Corp).ToList<APICharacter>();
            charNameColumn_corp.DataPropertyName = "CharName";
            corpNameColumn_Corp.DataPropertyName = "CorpName";
            corpIDColumn_Corp.DataPropertyName = "CorpID";
            corpIncludedColumn_Corp.DataPropertyName = "CorpIncWithRptGroup";

            if (_characters == null || _characters.Count == 0)
            {
                lblNoChars.Visible = true;
                lblNoCorp.Visible = true;
            }
            else
            {
                lblNoChars.Visible = false;
                lblNoCorp.Visible = false;
            }
        }

        private void btnNewAccount_Click(object sender, EventArgs e)
        {
            EnterAccountDetails details = new EnterAccountDetails();
            if (details.ShowDialog() != DialogResult.Cancel)
            {
                details.Account.UpdateCharList(true);
                _accounts.Add(details.Account);
                if (_removedAccounts.Contains(details.Account))
                {
                    _removedAccounts.Remove(details.Account);
                }
                foreach (APICharacter apiChar in details.Account.Chars)
                {
                    bool x = apiChar.CharIncWithRptGroup;
                    x = apiChar.CorpIncWithRptGroup;
                }
            }
            ShowAccounts();
        }

        private void btnDeleteAccount_Click(object sender, EventArgs e)
        {
            RemoveAccount();
        }

        private void eveAccountsGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                RemoveAccount();
            }
        }

        private void RemoveAccount()
        {
            if (eveAccountsGrid.SelectedRows != null)
            {
                DataGridViewSelectedRowCollection selected = eveAccountsGrid.SelectedRows;
                foreach (DataGridViewRow row in selected)
                {
                    EVEAccount account = (EVEAccount)row.DataBoundItem;
                    _accounts.Remove(account);
                    _removedAccounts.Add(account);
                }
                ShowAccounts();
            }
        }

        private void eveAccountsGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (eveAccountsGrid.SelectedRows != null)
            {
                _characters = new List<APICharacter>();

                DataGridViewSelectedRowCollection selected = eveAccountsGrid.SelectedRows;
                foreach (DataGridViewRow row in selected)
                {
                    EVEAccount account = (EVEAccount)row.DataBoundItem;
                    _characters.AddRange(account.Chars);
                }

                ShowCharacters();
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                // Store the 'included' flags for any chars/corps as they have been setup by the user.
                _characters = new List<APICharacter>();
                foreach (EVEAccount account in _accounts)
                {
                    _characters.AddRange(account.Chars);
                }
                foreach (APICharacter apiChar in _characters)
                {
                    // API key may have been changed so make sure we update it.
                    apiChar.APIKey = UserAccount.CurrentGroup.GetAccount(apiChar.UserID).ApiKey;
                    // update standings for all those that are part of the group
                    //if (apiChar.CharIncWithRptGroup) { apiChar.UpdateStandings(CharOrCorp.Char); }
                    //if (apiChar.CorpIncWithRptGroup) { apiChar.UpdateStandings(CharOrCorp.Corp); }
                    if (apiChar.AccessType == CharOrCorp.Char){apiChar.UpdateStandings(CharOrCorp.Char);}
                    if (apiChar.AccessType == CharOrCorp.Corp){apiChar.UpdateStandings(CharOrCorp.Corp);}

                    if (apiChar.AccessType == CharOrCorp.Char)
                    {
                        apiChar.StoreGroupLevelSettings(SettingsStoreType.Char);
                    }

                    if (apiChar.AccessType == CharOrCorp.Corp)
                    {
                        apiChar.StoreGroupLevelSettings(SettingsStoreType.Corp);
                    }
                    //apiChar.StoreGroupLevelSettings(SettingsStoreType.Both);
                }

                // Make sure to clear 'included' flags for any chars/corps on accounts that have been removed.
                _characters = new List<APICharacter>();
                foreach (EVEAccount account in _removedAccounts)
                {
                    _characters.AddRange(account.Chars);
                }
                foreach (APICharacter apiChar in _characters)
                {
                    apiChar.CharIncWithRptGroup = false;
                    apiChar.CorpIncWithRptGroup = false;
                    apiChar.StoreGroupLevelSettings(SettingsStoreType.Both);
                }

                // Finally, update the list of eve accounts stored against this report group.
                _group.StoreEveAccounts();

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                EMMAException emmaex = ex as EMMAException;
                if (emmaex == null)
                {
                    emmaex = new EMMAException(ExceptionSeverity.Error, "Problem saving report group setup", ex);
                }
                MessageBox.Show("Problem storing data: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // First we want to clear whatever the 'included' variables on chars and corps have been set to...
            _characters = new List<APICharacter>();
            foreach (EVEAccount account in _accounts)
            {
                _characters.AddRange(account.Chars);
            } 
            foreach (EVEAccount account in _removedAccounts)
            {
                _characters.AddRange(account.Chars);
            }
            foreach(APICharacter apiChar in _characters) 
            {
                apiChar.ResetIncludedVars();
            }

            // Next, reload the Eve account list for the current report group.
            UserAccount.CurrentGroup.LoadEveAccounts();

            // We havn't saved anything to the database yet so no changes required there, just exit the form.
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void charsAndCorpsGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // No longer needed with new API Keys                    
            //if(e.RowIndex >= 0 && e.ColumnIndex >=0) 
            //{
            //    object value = charsAndCorpsGrid["corpIDColumn", e.RowIndex].Value;
            //    if (value != null)
            //    {
            //        // Don't allow player to select an NPC corp.
            //        if (NPCCorps.GetCorp((long)value) != null)
            //        {
            //            charsAndCorpsGrid["corpIncludedColumn_Corp", e.RowIndex].ReadOnly = true;
            //        }
            //    }
            //}
        }

        private void charsAndCorpsGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            CheckForDuplicateCorp(CharOrCorp.Char, e);
        }
        private void charsAndCorpsGrid_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            CheckForDuplicateCorp(CharOrCorp.Char, e);
        }

        private void CorprGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            CheckForDuplicateCorp(CharOrCorp.Corp, e);
        }
        private void CorprGrid_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            CheckForDuplicateCorp(CharOrCorp.Corp, e);
        }


        private void CheckForDuplicateCorp(CharOrCorp gridType, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                if (gridType == CharOrCorp.Corp && CorprGrid.Columns[e.ColumnIndex] == corpIncludedColumn_Corp)
                {
                    DataGridViewDataErrorContexts context = DataGridViewDataErrorContexts.Commit;
                    CorprGrid.CommitEdit(context);
                    if ((bool)CorprGrid[e.ColumnIndex, e.RowIndex].Value == true)
                    {
                        // Don't allow player to select the same corp on different characters.
                        //object charIDValue = CorprGrid["charIDColumn", e.RowIndex].Value;
                        object corpIDvalue = CorprGrid["corpIDColumn_Corp", e.RowIndex].Value;
                        if (corpIDvalue != null) // && charIDValue != null)
                        {
                            for (int i = 0; i < CorprGrid.Rows.Count; i++)
                            {
                                object rowCorpID = CorprGrid["corpIDColumn_Corp", i].Value;
                                //object rowCharID = CorprGrid["charIDColumn", i].Value;
                                object rowCorpSelected = CorprGrid["corpIncludedColumn_Corp", i].Value;

                                if (/*rowCharID != null &&*/ rowCorpID != null && rowCorpSelected != null)
                                {
                                    if ((long)rowCorpID == (long)corpIDvalue && 
                                        e.RowIndex != i &&
                                        /*(int)rowCharID != (int)charIDValue &&*/
                                        (bool)rowCorpSelected)
                                    {
                                        // If a row that is NOT the row we are editing is the same corp and 
                                        // that corp is already included then de-select the
                                        // corp on the row being editied.
                                        CorprGrid[e.ColumnIndex, e.RowIndex].Value = false;
                                        CorprGrid.RefreshEdit();

                                        MessageBox.Show("You cannot select the same corp more than once.",
                                            "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }
                                }
                            }
                        }
                    }
                }
                // If user has 'lite' license then do not allow more than one 
                // character/corp to be selected.
                if (Globals.License == Enforcer.LicenseType.Lite)
                {
                    if ((gridType == CharOrCorp.Corp && CorprGrid.Columns[e.ColumnIndex] == corpIncludedColumn_Corp) ||
                        (gridType == CharOrCorp.Char && charsAndCorpsGrid.Columns[e.ColumnIndex] == charIncludedColumn))
                    {
                        DataGridViewDataErrorContexts context = DataGridViewDataErrorContexts.Commit;
                        charsAndCorpsGrid.CommitEdit(context);
                        bool showWarning = false;
                        if ((gridType == CharOrCorp.Char && (bool)charsAndCorpsGrid[e.ColumnIndex, e.RowIndex].Value) ||
                            (gridType == CharOrCorp.Corp && (bool)CorprGrid[e.ColumnIndex, e.RowIndex].Value))
                        {
                            bool setFalse = false;

                            for (int i = 0; i < charsAndCorpsGrid.Rows.Count; i++)
                            {
                                if (gridType == CharOrCorp.Corp || e.RowIndex != i)
                                {
                                    setFalse = setFalse || (bool)charsAndCorpsGrid[e.ColumnIndex, e.RowIndex].Value;
                                }
                            }
                            for (int i = 0; i < CorprGrid.Rows.Count; i++)
                            {
                                if (gridType == CharOrCorp.Char || e.RowIndex != i)
                                {
                                    setFalse = setFalse || (bool)CorprGrid[e.ColumnIndex, e.RowIndex].Value;
                                }
                            }


                            if (setFalse)
                            {
                                if (gridType == CharOrCorp.Char)
                                {
                                    showWarning = true;
                                    charsAndCorpsGrid[e.ColumnIndex, e.RowIndex].Value = false;
                                    charsAndCorpsGrid.RefreshEdit();
                                }
                                if (gridType == CharOrCorp.Corp)
                                {
                                    showWarning = true;
                                    CorprGrid[e.ColumnIndex, e.RowIndex].Value = false;
                                    CorprGrid.RefreshEdit();
                                }
                            }

                        }

                        if (showWarning)
                        {
                            MessageBox.Show("You have an EMMA 'lite' license so cannot select more " +
                                "than one character or corporation.",
                                "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }

        }

        private void eveAccountsGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                EVEAccount account = eveAccountsGrid.Rows[e.RowIndex].DataBoundItem as EVEAccount;

                if (account != null)
                {
                    EnterAccountDetails details = new EnterAccountDetails(account);
                    if (details.ShowDialog() != DialogResult.Cancel)
                    {
                        account.ApiKey = details.Account.ApiKey;
                    }
                }
            }
        }

        private void eveAccountsGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }







    }
}