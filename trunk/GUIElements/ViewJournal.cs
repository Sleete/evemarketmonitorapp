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
using EveMarketMonitorApp.AbstractionClasses;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class ViewJournal : Form
    {
        private JournalList _entries;
        private BindingSource _journalBindingSource;
        private List<long> _possibleOwners;
        private List<FinanceAccessParams> _accessParams = new List<FinanceAccessParams>();
        private static bool _allowRefresh = true;
        private static bool _acceptStartDate = true;
        private static bool _acceptEndDate = true;
        private DateTime _lastStartDate = new DateTime(), _lastEndDate = new DateTime();

        private DataGridViewRow _clickedRow;
        private DataGridViewCell _clickedCell;

        public ViewJournal()
        {
            InitializeComponent();
            journalDataGridView.AutoGenerateColumns = false;
            UserAccount.Settings.GetFormSizeLoc(this);
            journalDataGridView.Tag = "Journal Data";
            if (Globals.calculator != null)
            {
                Globals.calculator.BindGrid(journalDataGridView);
            }
        }

        private void ViewJournal_Load(object sender, EventArgs e)
        {
            try
            {
                Diagnostics.ResetAllTimers();
                Diagnostics.StartTimer("ViewJournal.Load");
                Diagnostics.StartTimer("ViewJournal.Load.Part1");
                _entries = new JournalList();
                _journalBindingSource = new BindingSource();
                _journalBindingSource.DataSource = _entries;

                DataGridViewCellStyle style = new DataGridViewCellStyle(AmountColumn.DefaultCellStyle);
                style.Format = IskAmount.FormatString();
                AmountColumn.DefaultCellStyle = style;
                DataGridViewCellStyle style2 = new DataGridViewCellStyle(BalanceColumn.DefaultCellStyle);
                style2.Format = IskAmount.FormatString();
                BalanceColumn.DefaultCellStyle = style2;

                journalDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                journalDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

                journalDataGridView.DataSource = _journalBindingSource;
                IDColumn.DataPropertyName = "Id";
                DateColumn.DataPropertyName = "Date";
                TypeColumn.DataPropertyName = "Type";
                OwnerIsSenderColumn.DataPropertyName = "OwnerIsSender";
                Owner1Column.DataPropertyName = "Sender";
                OwnerID1Column.DataPropertyName = "SenderID";
                Owner2Column.DataPropertyName = "Reciever";
                OwnerID2Column.DataPropertyName = "RecieverID";
                ArgIDColumn.DataPropertyName = "ArgID";
                ArgNameColumn.DataPropertyName = "ArgName";
                AmountColumn.DataPropertyName = "Amount";
                BalanceColumn.DataPropertyName = "Balance";
                ReasonColumn.DataPropertyName = "Reason";
                Owner1CorpColumn.DataPropertyName = "SenderCorp";
                Owner2CorpColumn.DataPropertyName = "RecieverCorp";
                Owner1WalletColumn.DataPropertyName = "SenderWallet";
                Owner2WalletColumn.DataPropertyName = "RecieverWallet";

                UserAccount.Settings.GetColumnWidths(this.Name, journalDataGridView);
                
                dtpEndDate.Value = DateTime.Now;
                dtpStartDate.Value = DateTime.Now.AddDays(-2);
                dtpEndDate.KeyDown += new KeyEventHandler(dtpEndDate_KeyDown);
                dtpEndDate.Leave += new EventHandler(dtpEndDate_Leave);
                dtpStartDate.KeyDown += new KeyEventHandler(dtpStartDate_KeyDown);
                dtpStartDate.Leave += new EventHandler(dtpStartDate_Leave);

                Diagnostics.StopTimer("ViewJournal.Load.Part1");

                Diagnostics.StartTimer("ViewJournal.Load.Part2");
                List<CharCorpOption> charcorps = UserAccount.CurrentGroup.GetCharCorpOptions(APIDataType.Journal);
                _possibleOwners = new List<long>();
                foreach (CharCorpOption chop in charcorps)
                {
                    _possibleOwners.Add(chop.Corp ? chop.CharacterObj.CorpID : chop.CharacterObj.CharID);
                }
                _accessParams = new List<FinanceAccessParams>();
                foreach (long id in _possibleOwners)
                {
                    _accessParams.Add(new FinanceAccessParams(id));
                }
                cmbOwner.DisplayMember = "Name";
                cmbOwner.ValueMember = "Data";
                charcorps.Sort();
                cmbOwner.DataSource = charcorps;
                cmbOwner.SelectedValue = 0;
                cmbOwner.Enabled = false;
                Diagnostics.StopTimer("ViewJournal.Load.Part2");

                Diagnostics.StartTimer("ViewJournal.Load.Part3");
                EMMADataSet.JournalRefTypesDataTable types = JournalRefTypes.GetTypesByJournal(_accessParams);
                EMMADataSet.JournalRefTypesRow newType = types.NewJournalRefTypesRow();
                newType.ID = 0;
                newType.RefName = "All Types";
                types.AddJournalRefTypesRow(newType);
                BindingSource typesSource = new BindingSource();
                typesSource.DataSource = types;
                typesSource.Sort = "RefName";
                cmbType.DisplayMember = "RefName";
                cmbType.ValueMember = "ID";
                cmbType.DataSource = typesSource;
                cmbType.SelectedValue = 0;
                Diagnostics.StopTimer("ViewJournal.Load.Part3");

                cmbType.SelectedIndexChanged += new EventHandler(cmbType_SelectedIndexChanged);

                cmbOwner.SelectedIndexChanged += new EventHandler(cmbOwner_SelectedIndexChanged);
                chkIngoreOwner.Checked = true;
                chkIngoreOwner.CheckedChanged += new EventHandler(chkIngoreOwner_CheckedChanged);

                cmbWallet.SelectedIndexChanged += new EventHandler(cmbWallet_SelectedIndexChanged);

                DisplayWallets();
                chkIgnoreWallet.CheckedChanged += new EventHandler(chkIgnoreWallet_CheckedChanged);
                txtName.KeyDown += new KeyEventHandler(txtName_KeyDown);
                txtName.Leave += new EventHandler(txtName_Leave);

                Diagnostics.StartTimer("ViewJournal.Load.Part4");
                this.FormClosing += new FormClosingEventHandler(ViewJournal_FormClosing);
                DisplayEntries();
                Diagnostics.StopTimer("ViewJournal.Load.Part4");
                Diagnostics.StopTimer("ViewJournal.Load");

                Diagnostics.DisplayDiag(
                    "Total form load time: " + Diagnostics.GetRunningTime("ViewJournal.Load").ToString() +
                    "\r\nSplit time 1: " + Diagnostics.GetRunningTime("ViewJournal.Load.Part1").ToString() +
                    "\r\nSplit time 2: " + Diagnostics.GetRunningTime("ViewJournal.Load.Part2").ToString() +
                    "\r\nSplit time 3: " + Diagnostics.GetRunningTime("ViewJournal.Load.Part3").ToString() +
                    "\r\nSplit time 4: " + Diagnostics.GetRunningTime("ViewJournal.Load.Part4").ToString());
            }
            catch (Exception ex)
            {
                // Creating new EMMAexception will cause error to be logged.
                EMMAException emmaex = ex as EMMAException;
                if (emmaex == null)
                {
                    emmaex = new EMMAException(ExceptionSeverity.Critical, "Error setting up journal form", ex);
                }
                MessageBox.Show("Problem setting up journal view.\r\nCheck " + Globals.AppDataDir + "Logging\\ExceptionLog.txt" +
                    " for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void ViewJournal_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (UserAccount.Settings != null)
            {
                UserAccount.Settings.StoreColumnWidths(this.Name, journalDataGridView);
                UserAccount.Settings.StoreFormSizeLoc(this);
            }
            if (Globals.calculator != null)
            {
                Globals.calculator.RemoveGrid(journalDataGridView);
            }
        }


        private void DisplayWallets()
        {
            bool disable = true;

            if (!chkIngoreOwner.Checked && cmbOwner.SelectedValue != null)
            {
                CharCorp data = (CharCorp)cmbOwner.SelectedValue;
                if (data.corp)
                {
                    disable = false;
                    chkIgnoreWallet.Enabled = true;

                    if (!chkIgnoreWallet.Checked)
                    {
                        cmbWallet.Enabled = true;

                        EMMADataSet.WalletDivisionsDataTable wallets = data.characterObj.WalletDivisions;
                        cmbWallet.DisplayMember = "Name";
                        cmbWallet.ValueMember = "ID";
                        cmbWallet.DataSource = wallets;
                        cmbWallet.SelectedValue = 0;
                    }
                }
            }

            if (disable)
            {
                cmbWallet.Enabled = false;
                chkIgnoreWallet.Checked = true;
                chkIgnoreWallet.Enabled = false;
            }
        }

        void chkIgnoreWallet_CheckedChanged(object sender, EventArgs e)
        {
            cmbWallet.Enabled = !chkIgnoreWallet.Checked;
            DisplayWallets();
            if (chkIgnoreWallet.Checked)
            {
                DisplayEntries();
            }
        }

        void chkIngoreOwner_CheckedChanged(object sender, EventArgs e)
        {
            cmbOwner.Enabled = !chkIngoreOwner.Checked;
            DisplayWallets();
            if (chkIngoreOwner.Checked)
            {
                DisplayEntries();
            }
        }

        void cmbWallet_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayEntries();
        }

        void cmbOwner_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayWallets();
            DisplayEntries();
        }


        void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayEntries();
        }

        void dtpStartDate_Leave(object sender, EventArgs e)
        {
            StartDateChanged();
        }
        void dtpStartDate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                StartDateChanged();
            }
        }
        private void dtpStartDate_DropDown(object sender, EventArgs e)
        {
            _acceptStartDate = false;
        }
        private void dtpStartDate_CloseUp(object sender, EventArgs e)
        {
            _acceptStartDate = true;
            StartDateChanged();
        }
        private void StartDateChanged()
        {
            if (_acceptStartDate && !_lastStartDate.Equals(dtpStartDate.Value))
            {
                if (dtpEndDate.Value.CompareTo(dtpStartDate.Value) < 0)
                {
                    MessageBox.Show("Start date/time must be before end date/time", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _allowRefresh = false;
                    dtpStartDate.Value = dtpEndDate.Value.AddHours(-1);
                    _allowRefresh = true;
                }
                _lastStartDate = dtpStartDate.Value;
                DisplayEntries();
            }
        }


        void dtpEndDate_Leave(object sender, EventArgs e)
        {
            EndDateChanged();
        }
        void dtpEndDate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                EndDateChanged();
            }
        }
        private void dtpEndDate_DropDown(object sender, EventArgs e)
        {
            _acceptEndDate = false;
        }
        private void dtpEndDate_CloseUp(object sender, EventArgs e)
        {
            _acceptEndDate = true;
            EndDateChanged();
        }
        private void EndDateChanged()
        {
            if (_acceptEndDate && !_lastEndDate.Equals(dtpEndDate.Value))
            {
                if (dtpEndDate.Value.CompareTo(dtpStartDate.Value) < 0)
                {
                    MessageBox.Show("Start date/time must be before end date/time", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _allowRefresh = false;
                    dtpEndDate.Value = dtpStartDate.Value.AddHours(1);
                    _allowRefresh = true;
                }
                _lastEndDate = dtpEndDate.Value;
                DisplayEntries();
            }
        }

        void txtName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Tab || e.KeyCode == Keys.Enter)
            {
                if (txtName.Text.Length > 98)
                {
                    txtName.Text = txtName.Text.Remove(99);
                }
                DisplayEntries();
            }
        }
        void txtName_Leave(object sender, EventArgs e)
        {
            if (txtName.Text.Length > 98)
            {
                txtName.Text = txtName.Text.Remove(99);
            }
            DisplayEntries();
        }

        private void DisplayEntries()
        {
            Diagnostics.StartTimer("ViewJournal.Refresh");
            List<short> typeIDs = new List<short>();
            DateTime startDate, endDate;
            typeIDs.Add((short)cmbType.SelectedValue);
            string nameProfile = "";
            if (!txtName.Text.Equals(""))
            {
                nameProfile = "%" + txtName.Text + "%";
            }

            Cursor = Cursors.WaitCursor;

            try
            {
                if (_allowRefresh)
                {
                    long ownerID = 0;
                    if (cmbOwner.SelectedValue != null && !chkIngoreOwner.Checked)
                    {
                        CharCorp data = (CharCorp)cmbOwner.SelectedValue;
                        ownerID = data.corp ? data.characterObj.CorpID : data.characterObj.CharID;
                    }
                    int walletID = 0;
                    if (cmbWallet.SelectedValue != null && !chkIgnoreWallet.Checked)
                    {
                        walletID = (int)cmbWallet.SelectedValue;
                    }

                    _accessParams = new List<FinanceAccessParams>();
                    if (ownerID == 0)
                    {
                        foreach (long id in _possibleOwners)
                        {
                            _accessParams.Add(new FinanceAccessParams(id));
                        }
                    }
                    else
                    {
                        List<short> wallets = new List<short>();
                        if (walletID != 0)
                        {
                            wallets.Add((short)walletID);
                        }
                        _accessParams.Add(new FinanceAccessParams(ownerID, wallets));
                    }

                    startDate = dtpStartDate.Value.ToUniversalTime();
                    endDate = dtpEndDate.Value.ToUniversalTime();

                    //ListSortDirection sortDirection = ListSortDirection.Descending;
                    //DataGridViewColumn sortColumn = journalDataGridView.SortedColumn;
                    //if (journalDataGridView.SortOrder == SortOrder.Ascending) sortDirection = ListSortDirection.Ascending;
                    List<SortInfo> sortinfo = journalDataGridView.GridSortInfo;

                    Diagnostics.StartTimer("ViewJournal.Refresh.Load");
                    _entries = Journal.LoadEntries(_accessParams, typeIDs, startDate, endDate, nameProfile);
                    Diagnostics.StopTimer("ViewJournal.Refresh.Load");
                    _journalBindingSource.DataSource = _entries;

                    //journalDataGridView.AutoResizeColumns();
                    //journalDataGridView.AutoResizeRows();

                    //if (sortColumn != null)
                    //{
                    //   journalDataGridView.Sort(sortColumn, sortDirection);
                    //}
                    //else
                    //{
                    //    journalDataGridView.Sort(DateColumn, ListSortDirection.Descending);
                    //}
                    if (sortinfo.Count == 0)
                    {
                        DataGridViewColumn column = journalDataGridView.Columns["DateColumn"];
                        sortinfo.Add(new SortInfo(column.Index, column.DataPropertyName));
                    }
                    journalDataGridView.GridSortInfo = sortinfo;

                    Text = "Viewing " + _journalBindingSource.Count + " entries";
                    Diagnostics.StopTimer("ViewJournal.Refresh");
                    Diagnostics.StopTimer("ViewJournal.Load");
                    Diagnostics.StopTimer("ViewJournal.Load.Part4");
                    Diagnostics.DisplayDiag(
                        "Time to refresh: " + Diagnostics.GetRunningTime("ViewJournal.Refresh").ToString() +
                        "\r\nData load time: " + Diagnostics.GetRunningTime("ViewJournal.Refresh.Load").ToString() +
                        "\r\n\tDatabase access time: " + Diagnostics.GetRunningTime("Journal.LoadEntries.Database").ToString() +
                        "\r\n\tJournal list build time: " + Diagnostics.GetRunningTime("Journal.LoadEntries.BuildList").ToString());
                    Diagnostics.StartTimer("ViewJournal.Load");
                    Diagnostics.StartTimer("ViewJournal.Load.Part4");
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void journalDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (journalDataGridView.Rows[e.RowIndex] != null)
            {
                if (journalDataGridView.Columns[e.ColumnIndex].Name.Equals("AmountColumn"))
                {
                    bool ownerIsSender = (bool)journalDataGridView["OwnerIsSenderColumn", e.RowIndex].Value;

                    DataGridViewCellStyle style = e.CellStyle;
                    if (ownerIsSender)
                    {
                        style.ForeColor = Color.Red;
                    }
                    else
                    {
                        style.ForeColor = Color.Green;
                    }

                    e.CellStyle = style;
                }
            }
        }


        private void journalDataGridView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                DataGridView.HitTestInfo Hti;
                Hti = journalDataGridView.HitTest(e.X, e.Y);

                if (Hti.Type == DataGridViewHitTestType.Cell)
                {
                    // store a reference to the cell and row that the user has clicked on.
                    _clickedRow = journalDataGridView.Rows[Hti.RowIndex];
                    _clickedCell = journalDataGridView[Hti.ColumnIndex, Hti.RowIndex];
                    _clickedRow.Selected = true;
                }
            }
        }
        
        private void copyCellDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_clickedCell != null && _clickedCell.Value != null)
            {
                Clipboard.SetText(_clickedCell.Value.ToString());
            }
        }

        private void copyRowDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_clickedRow != null)
            {
                StringBuilder stringData = new StringBuilder();
                foreach (DataGridViewCell cell in _clickedRow.Cells)
                {
                    if (cell.Value != null)
                    {
                        if (stringData.Length > 0) { stringData.Append(","); }
                        stringData.Append(cell.Value.ToString());
                    }
                }
                Clipboard.SetText(stringData.ToString(), TextDataFormat.Text);
            }
        }

        private void copyCellTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_clickedCell != null && _clickedCell.FormattedValue != null)
            {
                Clipboard.SetText(_clickedCell.FormattedValue.ToString());
            }
        }

        private void copyRowTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_clickedRow != null)
            {
                StringBuilder stringData = new StringBuilder();
                foreach (DataGridViewCell cell in _clickedRow.Cells)
                {
                    if (cell.FormattedValue != null)
                    {
                        if (stringData.Length > 0) { stringData.Append(","); }
                        stringData.Append(cell.FormattedValue.ToString());
                    }
                }
                Clipboard.SetText(stringData.ToString(), TextDataFormat.Text);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnCSV_Click(object sender, EventArgs e)
        {
            CSVExport.Export(journalDataGridView, "journal");
        }


    }
}