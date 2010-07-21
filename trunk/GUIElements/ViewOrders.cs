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
using EveMarketMonitorApp.AbstractionClasses;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class ViewOrders : Form
    {
        private OrdersList _orders;
        private BindingSource _ordersBindingSource;
        private List<int> _personalOwners;
        private List<int> _corporateOwners;
        private List<AssetAccessParams> _accessParams;
        private List<string> _recentStations;
        private string _lastStation = "";

        private DataGridViewRow _clickedRow;
        private DataGridViewCell _clickedCell;


        public ViewOrders()
        {
            InitializeComponent();
            ordersGrid.AutoGenerateColumns = false;
            UserAccount.Settings.GetFormSizeLoc(this);
            ordersGrid.Tag = "Orders Data";
            if (Globals.calculator != null)
            {
                Globals.calculator.BindGrid(ordersGrid);
            }
        }

        private void ViewOrders_Load(object sender, EventArgs e)
        {
            try
            {
                _orders = new OrdersList();
                _ordersBindingSource = new BindingSource();
                _ordersBindingSource.DataSource = _orders;

                DataGridViewCellStyle iskStyle = new DataGridViewCellStyle(PriceColumn.DefaultCellStyle);
                iskStyle.Format = IskAmount.FormatString();
                PriceColumn.DefaultCellStyle = iskStyle;
                EscrowColumn.DefaultCellStyle = iskStyle;
                TotalValueColumn.DefaultCellStyle = iskStyle;
                RemainingValueColumn.DefaultCellStyle = iskStyle;
                DataGridViewCellStyle dayStyle = new DataGridViewCellStyle(DurationColumn.DefaultCellStyle);
                dayStyle.Format = "# Days;-# Days;# Days";
                DurationColumn.DefaultCellStyle = dayStyle;

                ordersGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                ordersGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

                ordersGrid.DataSource = _ordersBindingSource;
                DateIssuedColumn.DataPropertyName = "Date";
                OwnerColumn.DataPropertyName = "Owner";
                ItemColumn.DataPropertyName = "Item";
                PriceColumn.DataPropertyName = "Price";
                StationColumn.DataPropertyName = "Station";
                SystemColumn.DataPropertyName = "System";
                RegionColumn.DataPropertyName = "Region";
                TypeColumn.DataPropertyName = "Type";
                TotalUnitsColumn.DataPropertyName = "TotalVol";
                QuantityColumn.DataPropertyName = "RemainingVol";
                EscrowColumn.DataPropertyName = "Escrow";
                StateColumn.DataPropertyName = "State";
                RangeColumn.DataPropertyName = "RangeText";
                DurationColumn.DataPropertyName = "Duration";
                TotalValueColumn.DataPropertyName = "TotalValue";
                RemainingValueColumn.DataPropertyName = "RemainingValue";

                UserAccount.Settings.GetColumnWidths(this.Name, ordersGrid);

                List<CharCorpOption> charcorps = UserAccount.CurrentGroup.GetCharCorpOptions(APIDataType.Orders);
                _corporateOwners = new List<int>();
                _personalOwners = new List<int>();
                foreach (CharCorpOption chop in charcorps)
                {
                    if (chop.Corp)
                    {
                        _corporateOwners.Add(chop.CharacterObj.CorpID);
                    }
                    else
                    {
                        _personalOwners.Add(chop.CharacterObj.CharID);
                    }
                }
                cmbOwner.DisplayMember = "Name";
                cmbOwner.ValueMember = "Data";
                charcorps.Sort();
                cmbOwner.DataSource = charcorps;
                cmbOwner.SelectedValue = 0;
                cmbOwner.Enabled = false;
                cmbOwner.SelectedIndexChanged += new EventHandler(cmbOwner_SelectedIndexChanged);
                chkIngoreOwner.Checked = true;
                chkIngoreOwner.CheckedChanged += new EventHandler(chkIngoreOwner_CheckedChanged);

                EMMADataSet.OrderStatesDataTable allStates = OrderStates.GetAllStates();
                EMMADataSet.OrderStatesRow newState = allStates.NewOrderStatesRow();
                newState.StateID = 0;
                newState.Description = "All States";
                allStates.AddOrderStatesRow(newState);
                BindingSource stateSource = new BindingSource();
                stateSource.DataSource = allStates;
                stateSource.Sort = "Description";
                cmbStateFilter.DisplayMember = "Description";
                cmbStateFilter.ValueMember = "StateID";
                cmbStateFilter.DataSource = stateSource;
                cmbStateFilter.SelectedIndexChanged += new EventHandler(cmbStateFilter_SelectedIndexChanged);

                _recentStations = UserAccount.CurrentGroup.Settings.RecentStations;
                _recentStations.Sort();
                AutoCompleteStringCollection stations = new AutoCompleteStringCollection();
                stations.AddRange(_recentStations.ToArray());
                txtStation.AutoCompleteCustomSource = stations;
                txtStation.AutoCompleteSource = AutoCompleteSource.CustomSource;
                txtStation.AutoCompleteMode = AutoCompleteMode.Suggest;
                txtStation.Leave += new EventHandler(txtStation_Leave);
                txtStation.KeyDown += new KeyEventHandler(txtStation_KeyDown);
                txtStation.Tag = 0;
                txtStation.Text = "";

                cmbType.SelectedIndexChanged += new EventHandler(cmbType_SelectedIndexChanged);
                
                this.FormClosing += new FormClosingEventHandler(ViewOrders_FormClosing);

                DisplayOrders();
            }
            catch (Exception ex)
            {
                // Creating new EMMAexception will cause error to be logged.
                EMMAException emmaex = ex as EMMAException;
                if (emmaex == null)
                {
                    emmaex = new EMMAException(ExceptionSeverity.Critical, "Error setting up orders form", ex);
                }
                MessageBox.Show("Problem setting up orders view.\r\nCheck " + Globals.AppDataDir + "Logging\\ExceptionLog.txt" +
                    " for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayOrders();
        }

        void ViewOrders_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (UserAccount.Settings != null)
            {
                // Store the current column widths.
                UserAccount.Settings.StoreColumnWidths(this.Name, ordersGrid);
                UserAccount.Settings.StoreFormSizeLoc(this);
            }
            if (Globals.calculator != null)
            {
                Globals.calculator.RemoveGrid(ordersGrid);
            }
        }

        void cmbStateFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayOrders();
        }

        void chkIngoreOwner_CheckedChanged(object sender, EventArgs e)
        {
            cmbOwner.Enabled = !chkIngoreOwner.Checked;
            if (chkIngoreOwner.Checked)
            {
                DisplayOrders();
            }
        }

        void cmbOwner_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayOrders();
        }

        void txtStation_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Enter)
            {
                SetSelectedStation();
                DisplayOrders();
            }
        }

        void txtStation_Leave(object sender, EventArgs e)
        {
            SetSelectedStation();
            DisplayOrders();
        }

        private void SetSelectedStation()
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (!txtStation.Text.Equals(_lastStation))
                {
                    txtStation.Tag = 0;
                    if (!txtStation.Text.Equals(""))
                    {
                        try
                        {
                            EveDataSet.staStationsRow station = Stations.GetStation(txtStation.Text);
                            if (station != null)
                            {
                                txtStation.Tag = station.stationID;
                                string name = station.stationName;
                                txtStation.Text = name;
                                if (!_recentStations.Contains(name))
                                {
                                    _recentStations.Add(name);
                                    txtStation.AutoCompleteCustomSource.Add(name);
                                }
                            }
                        }
                        catch (EMMADataException) { }

                        _lastStation = txtStation.Text;
                    }

                    if ((int)txtStation.Tag == 0)
                    {
                        txtStation.Text = "";
                    }
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void DisplayOrders()
        {
            int ownerID = 0;
            List<int> items = new List<int>();
            items.Add(0);
            List<int> stations = new List<int>();
            try
            {
                stations.Add((int)txtStation.Tag);
            }
            catch (InvalidCastException)
            {
                stations.Add(0);
            }
            int stateID = 0;
            string type = cmbType.Text;

            if (cmbStateFilter.SelectedValue != null)
            {
                stateID = (short)cmbStateFilter.SelectedValue;
            }

            bool corpAssets = false;
            if (cmbOwner.SelectedValue != null && !chkIngoreOwner.Checked)
            {
                CharCorp data = (CharCorp)cmbOwner.SelectedValue;
                ownerID = data.characterObj.CharID;
                corpAssets = data.corp;
            }

            _accessParams = new List<AssetAccessParams>();
            if (ownerID == 0)
            {
                //List<int> ignore = new List<int>();
                foreach (int id in _personalOwners)
                {
                    _accessParams.Add(new AssetAccessParams(id));
                    //_accessParams.Add(new AssetAccessParams(id, true, _corporateOwners.Contains(id)));
                    //ignore.Add(id);
                }
                foreach (int id in _corporateOwners)
                {
                    _accessParams.Add(new AssetAccessParams(id));
                    //if (!ignore.Contains(id))
                    //{
                    //    _accessParams.Add(new AssetAccessParams(id, false, true));
                    //}
                }
            }
            else
            {
                _accessParams.Add(new AssetAccessParams(ownerID));
                //_accessParams.Add(new AssetAccessParams(ownerID, !corpAssets, corpAssets));
            }

            //ListSortDirection sortDirection = ListSortDirection.Descending;
            //DataGridViewColumn sortColumn = ordersGrid.SortedColumn;
            //if (ordersGrid.SortOrder == SortOrder.Ascending) sortDirection = ListSortDirection.Ascending;
            List<SortInfo> sortinfo = ordersGrid.GridSortInfo;

            _orders = Orders.LoadOrders(_accessParams, items, stations, stateID, type);
            _ordersBindingSource.DataSource = _orders;

            //ordersGrid.AutoResizeColumns();
            //ordersGrid.AutoResizeRows();

            //if (sortColumn != null)
            //{
            //    ordersGrid.Sort(sortColumn, sortDirection);
            //}
            //else
            //{
            //    ordersGrid.Sort(DateIssuedColumn, ListSortDirection.Descending);
            //}
            if (sortinfo.Count == 0)
            {
                DataGridViewColumn column = ordersGrid.Columns["DateIssuedColumn"];
                sortinfo.Add(new SortInfo(column.Index, column.DataPropertyName));
            }
            ordersGrid.GridSortInfo = sortinfo;


            Text = "Viewing " + _ordersBindingSource.Count + " orders";
        }

        private void ordersGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (ordersGrid.Rows[e.RowIndex] != null)
            {
                if (ordersGrid.Columns[e.ColumnIndex].Name.Equals("PriceColumn") ||
                    ordersGrid.Columns[e.ColumnIndex].Name.Equals("TotalValueColumn") ||
                    ordersGrid.Columns[e.ColumnIndex].Name.Equals("RemainingValueColumn"))
                {
                    bool buyOrder = ordersGrid["TypeColumn", e.RowIndex].Value.ToString().Equals("Buy");
                    DataGridViewCellStyle style = e.CellStyle;
                    if (buyOrder)
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

        private void ordersGrid_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                DataGridView.HitTestInfo Hti;
                Hti = ordersGrid.HitTest(e.X, e.Y);

                if (Hti.Type == DataGridViewHitTestType.Cell)
                {
                    // store a reference to the cell and row that the user has clicked on.
                    _clickedRow = ordersGrid.Rows[Hti.RowIndex];
                    _clickedCell = ordersGrid[Hti.ColumnIndex, Hti.RowIndex];
                    _clickedRow.Selected = true;
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnUnacknowledgedOrders_Click(object sender, EventArgs e)
        {
            ((Main)this.MdiParent).TryShowUnackOrders();
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

        private void btnCSV_Click(object sender, EventArgs e)
        {
            CSVExport.Export(ordersGrid, "orders");
        }



    }
}