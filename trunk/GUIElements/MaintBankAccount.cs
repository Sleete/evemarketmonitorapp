using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.Reporting;
using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class MaintBankAccount : Form
    {
        private PublicCorp _corp = null;
        private BindingSource _bindingSource = null;
        private BankTransactionsList _trans = new BankTransactionsList();
        private PublicCorpsList _corpData;
        private bool _accountExists = true;

        public PublicCorp _oldCorpData = null;
        private long _oldOwner = 0;
        private int _oldCorp = 0;

        private long _lastOwner = 0;
        private int _lastCorp = 0;

        public MaintBankAccount(PublicCorp corp)
        {
            InitializeComponent();
            _corp = corp;
            _oldCorpData = _corp;

            if (_corp != null)
            {
                _oldCorp = _corp.ID;
                _oldOwner = _corp.OwnerID;
                _lastCorp = _corp.ID;
                _lastOwner = _corp.OwnerID;
            }

            _bindingSource = new BindingSource();
        }

        private void MaintBankAccount_Load(object sender, EventArgs e)
        {
            try
            {
                if (_oldCorpData != null)
                {
                    cmbAccountCorp.Enabled = false;
                    if (_oldCorpData.OwnerID != 0)
                    {
                        cmbOwner.Enabled = false;
                    }
                }

                _corpData = PublicCorps.GetAll(true, false);
                _corpData.Sort("Name ASC");
                cmbAccountCorp.DisplayMember = "Name";
                cmbAccountCorp.ValueMember = "ID";
                cmbAccountCorp.DataSource = _corpData;
                if (_oldCorpData != null)
                {
                    cmbAccountCorp.SelectedValue = _oldCorp;
                }

                cmbAccountCorp.SelectedIndexChanged += new EventHandler(cmbAccountCorp_SelectedIndexChanged);
                cmbOwner.SelectedIndexChanged += new EventHandler(cmbOwner_SelectedIndexChanged);

                if (_corp == null)
                {
                    _corp = new PublicCorp();
                }
                CorpChanged();

                //DisplayOwners();
                //DisplayData();
            }
            catch (Exception ex)
            {
                EMMAException emmaEx = ex as EMMAException;
                if (emmaEx == null)
                {
                    emmaEx = new EMMAException(ExceptionSeverity.Error, "Problem loading " +
                        "bank account window", ex);
                }
                MessageBox.Show("Problem loading bank account window: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void cmbOwner_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayData();
        }

        private void cmbAccountCorp_SelectedIndexChanged(object sender, EventArgs e)
        {
            CorpChanged();
        }

        private void CorpChanged()
        {
            if (cmbAccountCorp.SelectedValue != null && _oldCorpData == null)
            {
                int newCorp = (int)cmbAccountCorp.SelectedValue;
                if (_lastCorp > 0 && _lastCorp != newCorp && _lastOwner > 0)
                {
                    BankAccounts.DeleteAccount(_lastCorp, _lastOwner);
                }
                _corp = PublicCorps.GetCorp(newCorp);
                // If the corp comes from the cache it may still have details for a different
                // owner's bank account, need to make sure we clear any existing details.
                _corp.OwnerID = -1;
                _corp.ReloadBankAccountDetails();
            }
            _lastCorp = _corp.ID;
            DisplayOwners();
            DisplayData();
        }

        private void DisplayOwners()
        {
            if (_corp != null)
            {
                cmbOwner.SelectedIndexChanged -= cmbOwner_SelectedIndexChanged;

                List<long> existingOwners = new List<long>();
                EMMADataSet.BankAccountDataTable accounts = BankAccounts.GetBankAccountData(
                    UserAccount.CurrentGroup.ID, 0, _corp.ID);
                foreach (EMMADataSet.BankAccountRow account in accounts)
                {
                    existingOwners.Add(account.OwnerID);
                }

                List<CharCorpOption> charcorps = UserAccount.CurrentGroup.GetCharCorpOptions();
                List<CharCorpOption> optionsToRemove = new List<CharCorpOption>();

                if (cmbOwner.Enabled)
                {
                    foreach (CharCorpOption chop in charcorps)
                    {
                        long id = chop.Corp ? chop.CharacterObj.CorpID : chop.CharacterObj.CharID;
                        if (existingOwners.Contains(id))
                        {
                            optionsToRemove.Add(chop);
                        }
                    }
                    foreach (CharCorpOption chop in optionsToRemove)
                    {
                        charcorps.Remove(chop);
                    }
                }

                cmbOwner.DisplayMember = "Name";
                cmbOwner.ValueMember = "Data";
                charcorps.Sort();
                cmbOwner.DataSource = charcorps;
                if (_oldCorpData != null)
                {
                    if (_oldOwner != 0)
                    {
                        CharCorp value = null;
                        foreach (CharCorpOption opt in charcorps)
                        {
                            if (opt.Data.ID == _oldOwner) { value = opt.Data; }
                        }
                        cmbOwner.SelectedValue = value;
                    }
                }

                cmbOwner.SelectedIndexChanged += new EventHandler(cmbOwner_SelectedIndexChanged);
            }
        }

        private void DisplayData()
        {
            try
            {
                _accountExists = _oldCorpData != null;

                if (cmbOwner.SelectedValue != null && (_oldOwner == 0 || _oldCorpData == null))
                {
                    long newOwner = ((CharCorp)cmbOwner.SelectedValue).ID;
                    //if (_lastOwner > 0 && _lastOwner != newOwner && _lastCorp > 0)
                    //{
                    //    BankAccounts.DeleteAccount(_lastCorp, _lastOwner);
                    //}
                    _corp.OwnerID = newOwner;
                    //_corp.ReloadBankAccountDetails();
                }

                _lastOwner = _corp.OwnerID;

                if ((_oldCorpData == null || _oldOwner <= 0) && _corp.ID > 0 && _corp.OwnerID > 0)
                {
                    // Account does not exist, we must create it.
                    BankAccounts.StoreAccount(_corp, UserAccount.CurrentGroup.ID, _corp.OwnerID);
                    _accountExists = true;
                }

                if (_accountExists)
                {
                    btnDeposit.Enabled = true;
                    btnWithdraw.Enabled = true;
                    btnRecalcInterest.Enabled = true;
                    btnAdjust.Enabled = true;
                    btnDelete.Enabled = true;

                    lblBalance.Text = new IskAmount(_corp.AmountInAccount).ToString();
                    lblInterest.Text = new IskAmount(_corp.TotalInterest).ToString();

                    _trans = BankAccounts.GetAccountTransactions(_corp.BankAccountID);
                    _bindingSource.DataSource = _trans;
                    SetFilter();

                    DataGridViewCellStyle iskStyle = new DataGridViewCellStyle(AmountColumn.DefaultCellStyle);
                    iskStyle.Format = IskAmount.FormatString();
                    AmountColumn.DefaultCellStyle = iskStyle;

                    TransHistoryGrid.AutoGenerateColumns = false;
                    DateColumn.DataPropertyName = "Date";
                    AmountColumn.DataPropertyName = "Change";
                    TypeColumn.DataPropertyName = "TypeDescription";
                    TransHistoryGrid.DataSource = _bindingSource;

                    DataGridViewColumn sortColumn = DateColumn;
                    ListSortDirection sortDirection = ListSortDirection.Descending;
                    if (TransHistoryGrid.SortedColumn != null) { sortColumn = TransHistoryGrid.SortedColumn; }
                    if (TransHistoryGrid.SortOrder == SortOrder.Ascending)
                    {
                        sortDirection = ListSortDirection.Ascending;
                    }
                    TransHistoryGrid.Sort(sortColumn, sortDirection);
                }
                else
                {
                    btnDeposit.Enabled = false;
                    btnWithdraw.Enabled = false;
                    btnRecalcInterest.Enabled = false;
                    btnAdjust.Enabled = false;
                    btnDelete.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                EMMAException emmaEx = ex as EMMAException;
                if (emmaEx == null)
                {
                    emmaEx = new EMMAException(ExceptionSeverity.Error, "Problem refreshing " +
                        "bank account window", ex);
                }
                MessageBox.Show("Problem refreshing bank account window: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void SetFilter()
        {
            _bindingSource.Filter = (chkHideInterest.Checked ? "TypeID != " +
                (short)BankTransactionType.InterestPayment : "");
        }

        private void chkHideInterest_CheckedChanged(object sender, EventArgs e)
        {
            SetFilter();
        }

        private void btnDeposit_Click(object sender, EventArgs e)
        {
            NewTransaction(BankTransactionType.Deposit);
        }

        private void btnWithdraw_Click(object sender, EventArgs e)
        {
            NewTransaction(BankTransactionType.Withdrawl);
        }

        private void btnAdjust_Click(object sender, EventArgs e)
        {
            NewTransaction(BankTransactionType.ManualAdjustment);
        }

        private void NewTransaction(BankTransactionType defaultType)
        {
            MaintBankTrans maintTrans = new MaintBankTrans(_corp, defaultType);
            if (maintTrans.ShowDialog() == DialogResult.OK)
            {
                // clear and recalulate interest payments after the transaction that
                // has just been added.
                BankAccounts.RecalculateInterestAfter(_corp.BankAccountID, maintTrans.TransactionDate);
                DisplayData();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (_corp != null)
            {
                if (_corp.AmountInAccount == 0)
                {
                    BankAccounts.DeleteAccount(_corp.ID, _corp.OwnerID);
                }
            }

            this.Close();
        }

        private void btnRecalcInterest_Click(object sender, EventArgs e)
        {
            RecalcAllInterest();
            DisplayData();
        }

        private void RecalcAllInterest()
        {
            BankAccounts.RecalculateInterestAfter(_corp.BankAccountID, new DateTime(2000, 1, 1));
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            BankTransactionsList trans = new BankTransactionsList();
            foreach (DataGridViewRow row in TransHistoryGrid.SelectedRows)
            {
                trans.Add((BankTransaction)row.DataBoundItem);
            }
            BankAccounts.DeleteTransactions(trans);
            RecalcAllInterest();
            DisplayData();
        }



     }
}