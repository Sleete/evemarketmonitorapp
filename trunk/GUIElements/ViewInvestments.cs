using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.Reporting;
using System.Threading;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class ViewInvestments : Form
    {
        PublicCorpsList _investments;
        PublicCorp selectedCorp= null;
        bool _bankMode = false;

        public ViewInvestments()
        {
            InitializeComponent();
            chkInvestedOnly.Checked = true;
            UserAccount.Settings.GetFormSizeLoc(this);
        }

        private void ViewInvestments_Load(object sender, EventArgs e)
        {
            this.FormClosing += new FormClosingEventHandler(ViewInvestments_FormClosing);
            DisplayData();
        }

        void ViewInvestments_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (UserAccount.Settings != null)
            {
                UserAccount.Settings.StoreFormSizeLoc(this);
            }
        }

        private void DisplayData()
        {
            try
            {
                //PublicCorp oldSelection = null;
                //if (sharesGrid.SelectedRows != null && sharesGrid.SelectedRows.Count > 0)
                //{
                //    oldSelection = (PublicCorp)sharesGrid.SelectedRows[0].DataBoundItem;
                //}

                _investments = PublicCorps.GetReportGroupInvestments(UserAccount.CurrentGroup.ID,
                    _bankMode);
                if (chkInvestedOnly.Checked && !_bankMode)
                {
                    _investments.ItemFilter = "SharesOwned > 0";
                }
                else
                {
                    _investments.ItemFilter = "";
                }

                DataGridViewCellStyle style = new DataGridViewCellStyle(ValueColumn.DefaultCellStyle);
                style.Format = IskAmount.FormatString();
                ValueColumn.DefaultCellStyle = style;

                sharesGrid.AutoGenerateColumns = false;
                CorpNameColumn.DataPropertyName = "Name";
                QuantityColumn.DataPropertyName = "SharesOwned";
                QuantityColumn.Visible = !_bankMode;
                if (!_bankMode)
                {
                    ValueColumn.DataPropertyName = "SharesOwnedValue";
                    ValueColumn.HeaderText = "Value";
                    AccountOwnerColumn.DataPropertyName = "";
                    AccountOwnerColumn.Visible = false;
                }
                else
                {
                    ValueColumn.DataPropertyName = "AmountInAccount";
                    ValueColumn.HeaderText = "Account Balance";
                    AccountOwnerColumn.DataPropertyName = "Owner";
                    AccountOwnerColumn.Visible = true;
                }

                DataGridViewColumn sortColumn = CorpNameColumn;
                ListSortDirection sortDirection = ListSortDirection.Ascending;
                if (sharesGrid.SortedColumn != null) { sortColumn = sharesGrid.SortedColumn; }
                if (sharesGrid.SortOrder == SortOrder.Descending)
                {
                    sortDirection = ListSortDirection.Descending;
                }

                if (_investments.ItemFilter.Length > 0)
                {
                    sharesGrid.DataSource = _investments.FiltredItems;
                }
                else
                {
                    sharesGrid.DataSource = _investments;
                }

                //if (oldSelection != null)
                //{
                //    for (int i = 0; i < sharesGrid.Rows.Count; i++)
                //    {
                //        if (((PublicCorp)sharesGrid.Rows[i].DataBoundItem).Equals(oldSelection))
                //        {
                //            sharesGrid.Rows[i].Selected = true;
                //        }
                //    }
                //}

                sharesGrid.Sort(sortColumn, sortDirection);

                btnCorpDetail.Enabled = false;
                btnDeleteCorp.Enabled = false;
                btnBuySell.Enabled = !_bankMode;

                if (sharesGrid.Rows.Count > 0) 
                {
                    sharesGrid.Rows[0].Selected = true;
                    RowSelected(0);
                }
            }
            catch (Exception ex)
            {
                EMMAException emmaEx = ex as EMMAException;
                if (emmaEx == null)
                {
                    emmaEx = new EMMAException(ExceptionSeverity.Error, "Problem displaying current investments", ex);
                }
                MessageBox.Show(emmaEx.Message + ": " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnBuySell_Click(object sender, EventArgs e)
        {
            if (!_bankMode)
            {
                MaintShareTrans newShareTrans = new MaintShareTrans();
                newShareTrans.ShowDialog();
                DisplayData();
            }
            else
            {
                if (selectedCorp != null)
                {
                    MaintBankAccount bankAccount = new MaintBankAccount(selectedCorp);
                    bankAccount.ShowDialog();
                    DisplayData();
                }
            }
        }

        private void sharesGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            ShowCorpDetail();
        }

        private void btnCorpDetail_Click(object sender, EventArgs e)
        {
            ShowCorpDetail();
        }

        private void ShowCorpDetail()
        {
            if (selectedCorp != null)
            {
                MaintPublicCorp corpDetail = new MaintPublicCorp(selectedCorp);
                corpDetail.ShowDialog();
                DisplayData();
            }
        }

        private void btnNewCorp_Click(object sender, EventArgs e)
        {
            MaintPublicCorp corpDetails = new MaintPublicCorp();
            corpDetails.ShowDialog();
            DisplayData();
        }

        private void sharesGrid_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                RowSelected(e.RowIndex);
            }
        }
        private void RowSelected(int rowIndex)
        {
            bool done = false;
            while (!done)
            {
                try
                {
                    selectedCorp = (PublicCorp)sharesGrid.Rows[rowIndex].DataBoundItem;
                    btnCorpDetail.Enabled = true;
                    btnBuySell.Enabled = (selectedCorp.Bank ? selectedCorp.BankAccountID > 0 : true);
                    btnDeleteCorp.Enabled = (selectedCorp.Bank ?
                        selectedCorp.BankAccountID > 0 : PublicCorps.AllowDelete(selectedCorp.ID));
                    done = true;
                }
                catch (InvalidOperationException) 
                { 
                    // This can happen when deleting corps. Just wait a little and try again 
                    Thread.Sleep(100);
                }
            }
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            if (!_bankMode)
            {
                ListShareTransactions shareTrans = new ListShareTransactions(selectedCorp);
                shareTrans.ShowDialog();
                DisplayData();
            }
            else
            {
                MaintBankAccount bankAccount = new MaintBankAccount(null);
                bankAccount.ShowDialog();
                DisplayData();
            }
        }

        private void btnAutoDiv_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                DivUpdateInfo info = Dividends.UpdateFromJournal(false);
                string corpNames = "";
                foreach (string corp in info.missingCorps)
                {
                    corpNames = corpNames + (corpNames.Length == 0 ? "" : ", ") + corp;
                }
                MessageBox.Show("Dividends processing completed\r\n" + info.dividendsAdded + " dividends added" +
                    (info.dividendsNotAdded == 0 ? "" :
                    "\r\n" + info.dividendsNotAdded + " dividends could not be added because the corp " +
                    "is not defined\r\nList of undefined corp names: " + corpNames + "\r\n" +
                    "Create these corps and then run dividend auto add again."));
            }
            catch (Exception ex)
            {
                EMMAException emmaex = ex as EMMAException;
                if (emmaex == null)
                {
                    emmaex = new EMMAException(ExceptionSeverity.Error, "Problem creating dividend entries from" +
                        "journal data.", ex);
                }
                MessageBox.Show("Error trying to create new dividends: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void chkInvestedOnly_CheckedChanged(object sender, EventArgs e)
        {
            DisplayData();
        }

        private void btnToggleBanks_Click(object sender, EventArgs e)
        {
            if (_bankMode)
            {
                // Change to shares mode..
                btnToggleBanks.Text = "Show Banks";
                btnBuySell.Text = "Buy/Sell Shares";
                btnHistory.Text = "Transaction History";
                btnDeleteCorp.Text = "Delete Corp";
                chkInvestedOnly.Enabled = true;
                _bankMode = false;
            }
            else
            {
                // Change to bank mode..
                btnToggleBanks.Text = "Show Investments";
                btnBuySell.Text = "Account Detail";
                btnHistory.Text = "New Account";
                btnDeleteCorp.Text = "Delete Account";
                chkInvestedOnly.Enabled = false;
                _bankMode = true;
            }
            DisplayData();
            sharesGrid.Focus();
        }

        private void btnDeleteCorp_Click(object sender, EventArgs e)
        {
            if (!_bankMode)
            {
                try
                {
                    PublicCorps.Delete(selectedCorp.ID);
                    DisplayData();
                }
                catch (Exception ex)
                {
                    new EMMADataException(ExceptionSeverity.Error, "Problem removing public corp " +
                        selectedCorp.ID, ex);
                    MessageBox.Show("Problem removing public corp: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                try
                {
                    BankAccounts.DeleteAccount(selectedCorp.ID, selectedCorp.OwnerID);
                    DisplayData();
                }
                catch (Exception ex)
                {
                    new EMMADataException(ExceptionSeverity.Error, "Problem removing bank account " +
                        selectedCorp.Name + " for " + selectedCorp.Owner, ex);
                    MessageBox.Show("Problem removing bank account: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }



    }
}