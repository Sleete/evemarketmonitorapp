using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using EveMarketMonitorApp.Reporting;
using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.AbstractionClasses;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class ViewTransactions : Form
    {
        private List<long> _possibleOwners = new List<long>();
        private TransactionList _transactions;
        private BindingSource _transBindingSource;
        private List<string> _recentStations;
        private List<string> _recentItems;

        private string _lastStation = "", _lastItem = "";
        private static bool _allowRefresh = true;
        private static bool _acceptStartDate = true;
        private static bool _acceptEndDate = true;
        private DateTime _lastEndDate = new DateTime(), _lastStartDate = new DateTime();

        private DataGridViewRow _clickedRow;
        private DataGridViewCell _clickedCell;

        public ViewTransactions()
        {
            InitializeComponent();
            transactionGrid.AutoGenerateColumns = false;
            UserAccount.Settings.GetFormSizeLoc(this);
            transactionGrid.Tag = "Transactions Data";
            if (Globals.calculator != null)
            {
                Globals.calculator.BindGrid(transactionGrid);
            }
        }

        private void ViewTrans_Load(object sender, EventArgs e)
        {
            try
            {
                _transactions = new TransactionList();
                _transBindingSource = new BindingSource();
                _transBindingSource.DataSource = _transactions;

                DataGridViewCellStyle style = new DataGridViewCellStyle(PriceColumn.DefaultCellStyle);
                style.Format = IskAmount.FormatString();
                PriceColumn.DefaultCellStyle = style;
                TotalValueColumn.DefaultCellStyle = style;
                UnitProfitColumn.DefaultCellStyle = style;

                transactionGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                transactionGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

                transactionGrid.DataSource = _transBindingSource;
                IDColumn.DataPropertyName = "Id";
                DateTimeColumn.DataPropertyName = "Datetime";
                ItemColumn.DataPropertyName = "Item";
                PriceColumn.DataPropertyName = "Price";
                BuyerColumn.DataPropertyName = "Buyer";
                QuantityColumn.DataPropertyName = "Quantity";
                TotalValueColumn.DataPropertyName = "Total";
                SellerColumn.DataPropertyName = "Seller";
                BuyerCharacterColumn.DataPropertyName = "BuyerChar";
                SellerCharacterColumn.DataPropertyName = "SellerChar";
                StationColumn.DataPropertyName = "Station";
                RegionColumn.DataPropertyName = "Region";
                BuyerIDColumn.DataPropertyName = "BuyerID";
                SellerIDColumn.DataPropertyName = "SellerID";
                BuyerCharIDColumn.DataPropertyName = "BuyerCharID";
                SellerCharIDColumn.DataPropertyName = "SellerCharID";
                BuyerWalletColumn.DataPropertyName = "BuyerWallet";
                SellerWalletColumn.DataPropertyName = "SellerWallet";
                UnitProfitColumn.DataPropertyName = UserAccount.Settings.CalcProfitInTransView ? 
                    "GrossUnitProfit" : "PureGrossUnitProfit";

                UserAccount.Settings.GetColumnWidths(this.Name, transactionGrid);

                dtpEndDate.Value = DateTime.Now;
                dtpStartDate.Value = DateTime.Now.AddDays(-2);

                dtpEndDate.DropDown+=new EventHandler(dtpEndDate_DropDown);
                dtpEndDate.CloseUp+=new EventHandler(dtpEndDate_CloseUp);
                dtpEndDate.KeyDown += new KeyEventHandler(dtpEndDate_KeyDown);
                dtpEndDate.Leave += new EventHandler(dtpEndDate_Leave);

                dtpStartDate.DropDown += new EventHandler(dtpStartDate_DropDown);
                dtpStartDate.CloseUp += new EventHandler(dtpStartDate_CloseUp);
                dtpStartDate.KeyDown += new KeyEventHandler(dtpStartDate_KeyDown);
                dtpStartDate.Leave += new EventHandler(dtpStartDate_Leave);

                List<CharCorpOption> charcorps = UserAccount.CurrentGroup.GetCharCorpOptions(
                    APIDataType.Transactions);
                _possibleOwners = new List<long>();
                foreach (CharCorpOption chop in charcorps)
                {
                    _possibleOwners.Add(chop.Corp ? chop.CharacterObj.CorpID : chop.CharacterObj.CharID);
                }
                cmbOwner.DisplayMember = "Name";
                cmbOwner.ValueMember = "Data";
                charcorps.Sort();
                cmbOwner.DataSource = charcorps;
                cmbOwner.SelectedValue = 0;
                cmbOwner.SelectedIndexChanged += new EventHandler(cmbOwner_SelectedIndexChanged);
                cmbOwner.Enabled = false;
                chkIngoreOwner.Checked = true;
                chkIngoreOwner.CheckedChanged += new EventHandler(chkIngoreOwner_CheckedChanged);

                cmbWallet.SelectedIndexChanged += new EventHandler(cmbWallet_SelectedIndexChanged);
                cmbType.SelectedIndexChanged += new EventHandler(cmbType_SelectedIndexChanged);

                _recentItems = UserAccount.CurrentGroup.Settings.RecentItems;
                _recentItems.Sort();
                cmbItem.Items.AddRange(_recentItems.ToArray());
                cmbItem.AutoCompleteSource = AutoCompleteSource.ListItems;
                cmbItem.AutoCompleteMode = AutoCompleteMode.Suggest;
                cmbItem.KeyDown += new KeyEventHandler(cmbItem_KeyDown);
                cmbItem.SelectedIndexChanged += new EventHandler(cmbItem_SelectedIndexChanged);
                cmbItem.Tag = 0;

                _recentStations = UserAccount.CurrentGroup.Settings.RecentStations;
                _recentStations.Sort();
                cmbStation.Items.AddRange(_recentStations.ToArray());
                cmbStation.AutoCompleteSource = AutoCompleteSource.ListItems;
                cmbStation.AutoCompleteMode = AutoCompleteMode.Suggest;
                cmbStation.KeyDown += new KeyEventHandler(cmbStation_KeyDown);
                cmbStation.SelectedIndexChanged += new EventHandler(cmbStation_SelectedIndexChanged);
                cmbStation.Tag = 0;

                chkCalcProfit.Checked = UserAccount.Settings.CalcProfitInTransView;
                chkCalcProfit.CheckedChanged += new EventHandler(chkCalcProfit_CheckedChanged);

                this.FormClosing += new FormClosingEventHandler(ViewTransactions_FormClosing);
                DisplayWallets();
                chkIgnoreWallet.CheckedChanged += new EventHandler(chkIgnoreWallet_CheckedChanged);
                DisplayTrans();
            }
            catch (Exception ex)
            {
                // Creating new EMMAexception will cause error to be logged.
                EMMAException emmaex = ex as EMMAException;
                if (emmaex == null)
                {
                    emmaex = new EMMAException(ExceptionSeverity.Critical, "Error setting up transactions form", ex);
                }
                MessageBox.Show("Problem setting up transactions view.\r\nCheck " + Globals.AppDataDir + "Logging\\ExceptionLog.txt" +
                    " for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void chkCalcProfit_CheckedChanged(object sender, EventArgs e)
        {
            UserAccount.Settings.CalcProfitInTransView = chkCalcProfit.Checked;
            UnitProfitColumn.DataPropertyName = UserAccount.Settings.CalcProfitInTransView ?
                "GrossUnitProfit" : "PureGrossUnitProfit";
        }

        void ViewTransactions_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (UserAccount.Settings != null)
            {
                UserAccount.Settings.StoreColumnWidths(this.Name, transactionGrid);
                UserAccount.Settings.StoreFormSizeLoc(this);
            }
            if (Globals.calculator != null)
            {
                Globals.calculator.RemoveGrid(transactionGrid);
            }
        }

        void cmbStation_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                SetSelectedStation();
            }
        }

        void cmbStation_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetSelectedStation();
        }

        private void SetSelectedStation()
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (!cmbStation.Text.Equals(_lastStation))
                {
                    cmbStation.Tag = 0;
                    if (!cmbStation.Text.Equals(""))
                    {
                        try
                        {
                            EveDataSet.staStationsRow station = Stations.GetStation(cmbStation.Text);
                            if (station != null)
                            {
                                cmbStation.Tag = station.stationID;
                                string name = station.stationName;
                                cmbStation.Text = name;
                                if (!cmbStation.Items.Contains(name))
                                {
                                    cmbStation.Items.Add(name);
                                    _recentStations.Add(name);
                                }
                            }
                        }
                        catch (EMMADataException) { }

                        _lastStation = cmbStation.Text;
                        DisplayTrans();
                    }
                    if ((int)cmbStation.Tag == 0) { cmbStation.Text = ""; }
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayTrans();
        }

        void cmbItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                SetSelectedItem();
            }
        }

        void cmbItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetSelectedItem();
        }

        private void SetSelectedItem()
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (!cmbItem.Text.Equals(_lastItem))
                {
                    cmbItem.Tag = (short)0;
                    if (!cmbItem.Text.Equals(""))
                    {
                        try
                        {
                            EveDataSet.invTypesRow item = Items.GetItem(cmbItem.Text);
                            if (item != null)
                            {
                                cmbItem.Tag = item.typeID;
                                string name = item.typeName;
                                cmbItem.Text = name;
                                if (!cmbItem.Items.Contains(name))
                                {
                                    cmbItem.Items.Add(name);
                                    _recentItems.Add(name);
                                }
                            }
                        }
                        catch (EMMADataException) { }
                    }

                    if ((short)cmbItem.Tag == 0) { cmbItem.Text = ""; }
                    _lastItem = cmbItem.Text;
                    DisplayTrans();
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        void chkIgnoreWallet_CheckedChanged(object sender, EventArgs e)
        {
            cmbWallet.Enabled = !chkIgnoreWallet.Checked;
            DisplayWallets();
            if (chkIgnoreWallet.Checked)
            {
                DisplayTrans();
            }
        }

        void chkIngoreOwner_CheckedChanged(object sender, EventArgs e)
        {
            cmbOwner.Enabled = !chkIngoreOwner.Checked;
            DisplayWallets();
            if (chkIngoreOwner.Checked)
            {
                DisplayTrans();
            }
        }

        void cmbWallet_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayTrans();
        }

        void cmbOwner_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayWallets();
            DisplayTrans();
        }

        private void dtpStartDate_Leave(object sender, EventArgs e)
        {
            StartDateChanged();
        }
        private void dtpStartDate_KeyDown(object sender, KeyEventArgs e)
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
            if (_acceptStartDate && _lastStartDate != dtpStartDate.Value)
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
                DisplayTrans();
            }
        }

        private void dtpEndDate_Leave(object sender, EventArgs e)
        {
            EndDateChanged();
        }
        private void dtpEndDate_KeyDown(object sender, KeyEventArgs e)
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
            if (_acceptEndDate && _lastEndDate != dtpEndDate.Value)
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
                DisplayTrans();
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

        private void DisplayTrans()
        {
            if (_allowRefresh)
            {
                List<int> itemIDs = new List<int>();
                itemIDs.Add(short.Parse(cmbItem.Tag.ToString()));
                List<long> stationIDs = new List<long>();
                stationIDs.Add(long.Parse(cmbStation.Tag.ToString()));
                DateTime utcStart = new DateTime();
                DateTime utcEnd = new DateTime();
                string type = cmbType.Text;

                Cursor = Cursors.WaitCursor;

                try
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

                    List<FinanceAccessParams> accessParams = new List<FinanceAccessParams>();
                    if (ownerID == 0)
                    {
                        foreach (long id in _possibleOwners)
                        {
                            accessParams.Add(new FinanceAccessParams(id));
                        }
                    }
                    else
                    {
                        List<short> wallets = new List<short>();
                        if (walletID != 0)
                        {
                            wallets.Add((short)walletID);
                        }
                        accessParams.Add(new FinanceAccessParams(ownerID, wallets));
                    }

                    utcStart = dtpStartDate.Value.ToUniversalTime();
                    utcEnd = dtpEndDate.Value.ToUniversalTime();

                    //ListSortDirection sortDirection = ListSortDirection.Descending;
                    //DataGridViewColumn sortColumn = transactionGrid.SortedColumn;
                    //if (transactionGrid.SortOrder == SortOrder.Ascending) sortDirection = ListSortDirection.Ascending;
                    List<SortInfo> sortinfo = transactionGrid.GridSortInfo;

                    _transactions = Transactions.LoadTransactions(accessParams, itemIDs, stationIDs,
                        utcStart, utcEnd, type);
                    _transBindingSource.DataSource = _transactions;

                    //transactionGrid.AutoResizeColumns();
                    //transactionGrid.AutoResizeRows();

                    //if (sortColumn != null)
                    //{
                    //    transactionGrid.Sort(sortColumn, sortDirection);
                    //}
                    //else
                    //{
                    //    transactionGrid.Sort(DateTimeColumn, ListSortDirection.Descending);
                    //}
                    if (sortinfo.Count == 0)
                    {
                        DataGridViewColumn column = transactionGrid.Columns["DateTimeColumn"];
                        sortinfo.Add(new SortInfo(column.Index, column.DataPropertyName));
                    }
                    transactionGrid.GridSortInfo = sortinfo;

                    Text = "Viewing " + _transBindingSource.Count + " transactions";
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        }
        

        private void transactionGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (transactionGrid.Rows[e.RowIndex] != null)
            {
                if (transactionGrid.Columns[e.ColumnIndex].Name.Equals("PriceColumn")
                    || transactionGrid.Columns[e.ColumnIndex].Name.Equals("TotalValueColumn"))
                {
                    int buyerID = (int)transactionGrid["BuyerIDColumn", e.RowIndex].Value;
                    int sellerID = (int)transactionGrid["SellerIDColumn", e.RowIndex].Value;
                    bool matchBuyer = _possibleOwners.Contains(buyerID);
                    bool matchSeller = _possibleOwners.Contains(sellerID);
                    DataGridViewCellStyle style = e.CellStyle;
                    //DataGridViewCellStyle style2 = transactionGrid[e.ColumnIndex, e.RowIndex].Style;
                    if (matchBuyer && matchSeller)
                    {
                        style.ForeColor = Color.Blue;
                        //style2.ForeColor = Color.Blue;
                    }
                    else if (matchBuyer)
                    {
                        style.ForeColor = Color.Red;
                        //style2.ForeColor = Color.Red;
                    }
                    else if (matchSeller)
                    {
                        style.ForeColor = Color.Green;
                        //style2.ForeColor = Color.Green;
                    }
                    
                    //e.Value = new IskAmount((decimal)e.Value).ToString();
                    //e.CellStyle = style;
                    //e.FormattingApplied = true;
                }
                if (transactionGrid.Columns[e.ColumnIndex].Name.Equals("UnitProfitColumn"))
                {
                    DataGridViewCellStyle style = e.CellStyle;
                    if ((decimal)e.Value < 0)
                    {
                        style.ForeColor = Color.Red;
                    }
                    else
                    {
                        style.ForeColor = Color.Green;
                    }

                    //e.Value = new IskAmount((decimal)e.Value).ToString();
                    //e.CellStyle = style;
                    //e.FormattingApplied = true;
                }
            }
        }


        private void transactionGrid_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                DataGridView.HitTestInfo Hti;
                Hti = transactionGrid.HitTest(e.X, e.Y);

                if (Hti.Type == DataGridViewHitTestType.Cell)
                {
                    // store a reference to the cell and row that the user has clicked on.
                    _clickedRow = transactionGrid.Rows[Hti.RowIndex];
                    _clickedCell = transactionGrid[Hti.ColumnIndex, Hti.RowIndex];
                    _clickedRow.Selected = true;
                }
                else
                {
                    _clickedRow = null;
                    _clickedCell = null;
                }
            }

        }

        private void GridContextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (_clickedRow != null)
            {
                DataGridViewCell cell = _clickedRow.Cells["ItemColumn"];
                if (cell != null && cell.Value != null)
                    showOnlyThisItemToolStripMenuItem.Text = cell.Value.ToString();
            }
            else
            {
                showOnlyThisItemToolStripMenuItem.Text = "No Item selected!";
            }
        }

        private void showOnlyThisItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_clickedRow != null)
            {
                DataGridViewCell cell = _clickedRow.Cells["ItemColumn"];
                if (cell != null && cell.Value != null)
                {
                    cmbItem.Text = cell.Value.ToString();
                    SetSelectedItem();
                }
            }
        }

        private void showAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cmbItem.Text = "";
            SetSelectedItem();
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
            CSVExport.Export(transactionGrid, "transactions");
        }
    }
}