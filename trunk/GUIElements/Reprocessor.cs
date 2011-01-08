using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.Reporting;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class Reprocessor : Form
    {
        private List<string> _recentStations;
        private string _lastStation = "";
        private AssetList _assets;
        private ReprocessJob _reprocessJob;
        private bool _toggleOn = true;
        private static bool _leaving = false;

        Dictionary<int, decimal> _resultPrices = new Dictionary<int, decimal>(); 

        public Reprocessor()
        {
            InitializeComponent();

            UserAccount.Settings.GetFormSizeLoc(this);
            UserAccount.Settings.GetColumnWidths(this.Name, ItemsToReprocessView);
            UserAccount.Settings.GetColumnWidths(this.Name, ReprocessResultsView);
        }

        private void Reprocessor_Load(object sender, EventArgs e)
        {
            try
            {
                List<CharCorpOption> charcorps = UserAccount.CurrentGroup.GetCharCorpOptions();
                cmbDefaultReprocessor.DisplayMember = "Name";
                cmbDefaultReprocessor.ValueMember = "Data";
                charcorps.Sort();
                cmbDefaultReprocessor.DataSource = charcorps;
                cmbDefaultReprocessor.SelectedIndexChanged += new EventHandler(cmbDefaultReprocessor_SelectedIndexChanged);
                cmbDefaultReprocessor.SelectedIndex = 0;

                _recentStations = UserAccount.CurrentGroup.Settings.RecentStations;
                _recentStations.Sort();
                AutoCompleteStringCollection stations = new AutoCompleteStringCollection();
                stations.AddRange(_recentStations.ToArray());
                txtStation.AutoCompleteCustomSource = stations;
                txtStation.AutoCompleteSource = AutoCompleteSource.CustomSource;
                txtStation.AutoCompleteMode = AutoCompleteMode.Suggest;
                txtStation.Leave += new EventHandler(txtStation_Leave);
                txtStation.KeyDown += new KeyEventHandler(txtStation_KeyDown);
                txtStation.Tag = (long)0;
                txtStation.Text = "";

                DataGridViewCellStyle iskStyle = new DataGridViewCellStyle(UnitValueColumn.DefaultCellStyle);
                iskStyle.Format = IskAmount.FormatString();
                UnitValueColumn.DefaultCellStyle = iskStyle;
                TotalValueColumn.DefaultCellStyle = iskStyle;
                ResultValueColumn.DefaultCellStyle = iskStyle;
                ResultTotalValueColumn.DefaultCellStyle = iskStyle;
                ReprocessValueColumn.DefaultCellStyle = iskStyle;

                _assets = new AssetList();
                ItemsToReprocessView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                ItemsToReprocessView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                ItemsToReprocessView.AutoGenerateColumns = false;
                ItemColumn.DataPropertyName = "Item";
                QuantityColumn.DataPropertyName = "Quantity";
                UnitValueColumn.DataPropertyName = "UnitValue";
                TotalValueColumn.DataPropertyName = "TotalValue";
                ReprocessColumn.DataPropertyName = "Selected";
                ReprocessValueColumn.DataPropertyName = "ReprocessValue";
                ItemsToReprocessView.DataSource = _assets;

                ReprocessResultsView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                ReprocessResultsView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                ReprocessResultsView.AutoGenerateColumns = false;
                ResultItemColumn.DataPropertyName = "Item";
                ResultMaxQuantityColumn.DataPropertyName = "MaxQuantity";
                ResultActualQuantityColumn.DataPropertyName = "ActualQuantity";
                StationTakesQuantityColumn.DataPropertyName = "StationTakes";
                ResultFinalQuantityColumn.DataPropertyName = "Quantity";
                ResultValueColumn.DataPropertyName = "UnitSellPrice";
                ResultTotalValueColumn.DataPropertyName = "EstSellPrice";

                lblValueAfter.Text = "0.00 ISK";
                lblValueBefore.Tag = 0.0m;
                lblValueBefore.Text = "0.00 ISK";

                this.FormClosing += new FormClosingEventHandler(Reprocessor_FormClosing);

                RefreshContainerList();
                RefreshItemList();

                cmbContainers.SelectedIndexChanged += new EventHandler(cmbContainers_SelectedIndexChanged);
            }
            catch (Exception ex)
            {
                EMMAException emmaEx = ex as EMMAException;
                if (emmaEx == null)
                {
                    new EMMAException(ExceptionSeverity.Error, "Problem loading reprocessor", ex);
                }
                MessageBox.Show("Problem loading reprocessor:\r\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void Reprocessor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (UserAccount.Settings != null)
            {
                if (UserAccount.CurrentGroup != null)
                {
                    UserAccount.CurrentGroup.Settings.RecentStations = _recentStations;
                }
                UserAccount.Settings.StoreFormSizeLoc(this);
                UserAccount.Settings.StoreColumnWidths(this.Name, ItemsToReprocessView);
                UserAccount.Settings.StoreColumnWidths(this.Name, ReprocessResultsView);
            }
        }

        void cmbDefaultReprocessor_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshContainerList();
            RefreshItemList();
        }

        void cmbContainers_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshItemList();
        }

        void txtStation_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Enter)
            {
                SetSelectedStation();
                RefreshContainerList();
                RefreshItemList();
            }
        }

        void txtStation_Leave(object sender, EventArgs e)
        {
            // For some reason, 'RefreshContainerList' will somtimes fire the txtStation.Leave event so
            // this bit of code prevents it from actually doing everything twice.
            if (!_leaving)
            {
                _leaving = true;
                SetSelectedStation();
                RefreshContainerList();
                RefreshItemList();
                _leaving = false;
            }
        }

        private void SetSelectedStation()
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (!txtStation.Text.Equals(_lastStation))
                {
                    txtStation.Tag = (long)0;
                    if (!txtStation.Text.Equals(""))
                    {
                        try
                        {
                            EveDataSet.staStationsRow station = Stations.GetStation(txtStation.Text);
                            if (station != null)
                            {
                                txtStation.Tag = (long)station.stationID;
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

                    if ((long)txtStation.Tag == 0)
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

        private void RefreshContainerList()
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                cmbContainers.Items.Clear();
                ContainerItem defaultItem = new ContainerItem(null);
                cmbContainers.Items.Add(defaultItem);
                cmbContainers.Enabled = false;

                if ((long)txtStation.Tag != 0 && cmbDefaultReprocessor.SelectedValue != null)
                {
                    CharCorp charcorp = (CharCorp)cmbDefaultReprocessor.SelectedValue;
                    long station = (long)txtStation.Tag;

                    AssetList containers = Assets.LoadReprocessableAssets(charcorp.ID, station, 1, true, false);

                    if (containers.Count > 0 && cmbContainers.Items.Count == 1)
                    {
                        cmbContainers.Enabled = true;
                        foreach (Asset container in containers)
                        {
                            cmbContainers.Items.Add(new ContainerItem(container));
                        }
                    }
                }

                cmbContainers.SelectedItem = defaultItem;
            }
            catch (Exception ex)
            {
                new EMMAException(ExceptionSeverity.Error, "Problem refreshing available containers", ex);
                MessageBox.Show("Problem refreshing available containers.\r\nSee Logging/exceptionlog.txt" +
                    " for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void RefreshItemList()
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                DataGridViewColumn sortColumn = ItemsToReprocessView.SortedColumn;
                ListSortDirection sortDirection = ListSortDirection.Ascending;
                if (ItemsToReprocessView.SortOrder == SortOrder.Descending) { sortDirection = ListSortDirection.Descending; }

                _assets = new AssetList();
                _reprocessJob = null;
                Asset container = null;
                if (cmbContainers.SelectedItem != null)
                {
                    container = ((ContainerItem)cmbContainers.SelectedItem).Data;
                }

                if ((long)txtStation.Tag != 0 && cmbDefaultReprocessor.SelectedValue != null)
                {
                    CharCorp charcorp = (CharCorp)cmbDefaultReprocessor.SelectedValue;
                    long station = (long)txtStation.Tag;

                    _reprocessJob = new ReprocessJob(station, UserAccount.CurrentGroup.ID, charcorp.ID);
                    _reprocessJob.SetDefaultResultPrices(_resultPrices);

                    if (container == null)
                    {
                        _assets = Assets.LoadReprocessableAssets(charcorp.ID, station, 1, false, true);
                    }
                    else
                    {
                        _assets = container.Contents;
                    }

                    foreach (Asset asset in _assets)
                    {
                        asset.ForceNoReproValAsUnitVal = true;
                    }
                }

                ItemsToReprocessView.DataSource = _assets;
                decimal total = 0;
                foreach (Asset asset in _assets)
                {
                    if (asset.Selected)
                    {
                        total += asset.TotalValue;
                    }
                }
                lblValueBefore.Tag = total;
                lblValueBefore.Text = new IskAmount(total).ToString();

                if (sortColumn != null)
                {
                    ItemsToReprocessView.Sort(sortColumn, sortDirection);
                }
            }
            catch (Exception ex)
            {
                new EMMAException(ExceptionSeverity.Error, "Problem refreshing available assets", ex);
                MessageBox.Show("Problem refreshing available assets.\r\nSee Logging/exceptionlog.txt" +
                    " for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void RefreshJobDetails()
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                if (_reprocessJob != null)
                {
                    DataGridViewColumn sortColumn = ReprocessResultsView.SortedColumn;
                    ListSortDirection sortDirection = ListSortDirection.Ascending;
                    if (ReprocessResultsView.SortOrder == SortOrder.Descending) { sortDirection = ListSortDirection.Descending; }

                    _reprocessJob.ClearSourceItems();
                    foreach (Asset item in _assets)
                    {
                        if (item.Selected)
                        {
                            _reprocessJob.AddItem(item.ItemID, item.Quantity, item.TotalBuyPrice);
                        }
                    }
                    _reprocessJob.UpdateResults();
                    UpdateValueAfter();

                    ReprocessResultsView.DataSource = _reprocessJob.Results;

                    if (sortColumn != null)
                    {
                        ReprocessResultsView.Sort(sortColumn, sortDirection);
                    }
                }
            }
            catch (Exception ex)
            {
                new EMMAException(ExceptionSeverity.Error, "Problem refreshing reprocess job details", ex);
                MessageBox.Show("Problem refreshing reprocess job details.\r\nSee Logging/exceptionlog.txt" +
                    " for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); 
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void UpdateValueAfter()
        {
            if (_reprocessJob != null)
            {
                lblValueAfter.Text = new IskAmount(_reprocessJob.TotalResultsValue).ToString();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            ReprocessSettings settings = new ReprocessSettings();
            settings.ShowDialog();
            RefreshJobDetails();
            foreach (Asset item in _assets)
            {
                item.ClearReprocessValue();
            }
            ItemsToReprocessView.Refresh();
        }

        private void btnToggleReprocess_Click(object sender, EventArgs e)
        {
            decimal total = 0;
            foreach (Asset asset in _assets)
            {
                asset.Selected = _toggleOn;
                if (_toggleOn) { total += asset.TotalValue; }
            }
            _toggleOn = !_toggleOn;

            lblValueBefore.Tag = total;
            lblValueBefore.Text = new IskAmount(total).ToString();

            ItemsToReprocessView.Refresh();
            RefreshJobDetails();
        }

        private void btnReprocess_Click(object sender, EventArgs e)
        {
            try
            {
                _reprocessJob.CompleteJob();
                ReprocessJobs.StoreJob(_reprocessJob);

                RefreshItemList();
                /*List<int> removeIndicies = new List<int>(); 
                for(int i = 0; i < _assets.Count ; i++) 
                {
                    Asset asset = _assets[i];
                    if (asset.Selected)
                    {
                        removeIndicies.Add(i);
                    }
                }
                foreach (int index in removeIndicies)
                {
                    _assets.RemoveAt(index);
                }*/
                RefreshJobDetails();
            }
            catch (Exception ex)
            {
                EMMAException emmaEx = ex as EMMAException;
                if (emmaEx == null)
                {
                    new EMMAException(ExceptionSeverity.Error, "Problem reprocessing items", ex);
                }
                MessageBox.Show("Problem reprocessing items\r\n" +
                    "see 'Logging/ExceptionLog.txt' for details", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCompleteReprocess_Click(object sender, EventArgs e)
        {

        }

        private void ItemsToReprocessView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0)
            {
                if (ItemsToReprocessView.Columns[e.ColumnIndex] == ReprocessColumn)
                {
                    Asset assetData = (Asset)ItemsToReprocessView.Rows[e.RowIndex].DataBoundItem;

                    decimal total = (decimal)lblValueBefore.Tag;

                    assetData.Selected = !assetData.Selected;

                    total = total + ((assetData.Selected ? 1 : -1) * assetData.TotalValue);
                    lblValueBefore.Tag = total;
                    lblValueBefore.Text = new IskAmount(total).ToString();

                    RefreshJobDetails();
                }
            }
        }

        private void ItemsToReprocessView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                if (ItemsToReprocessView.Columns[e.ColumnIndex] == QuantityColumn)
                {
                    Asset assetData = (Asset)ItemsToReprocessView.Rows[e.RowIndex].DataBoundItem;
                    if (assetData.Selected)
                    {
                        decimal total = (decimal)lblValueBefore.Tag;
                        total = total - assetData.TotalValue;
                        lblValueBefore.Tag = total;
                    }
                }
            }
        }

        private void ItemsToReprocessView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                if (ItemsToReprocessView.Columns[e.ColumnIndex] == QuantityColumn)
                {
                    Asset assetData = (Asset)ItemsToReprocessView.Rows[e.RowIndex].DataBoundItem;
                    if (assetData.Selected)
                    {
                        decimal total = (decimal)lblValueBefore.Tag;
                        total = total + assetData.TotalValue;
                        lblValueBefore.Tag = total;
                        lblValueBefore.Text = new IskAmount(total).ToString();

                        RefreshJobDetails();
                    }
                }
            }
        }

        private void ReprocessResultsView_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            if (e.ColumnIndex > -1)
            {
                if (ReprocessResultsView.Columns[e.ColumnIndex] == ResultValueColumn)
                {
                    decimal value = 0.0m;
                    string text = e.Value.ToString();
                    int removePoint = text.IndexOf("ISK");
                    if (removePoint > 0)
                    {
                        text = text.Remove(removePoint);
                    }
                    try
                    {
                        value = decimal.Parse(text);
                    }
                    catch { }

                    e.Value = value;
                    e.ParsingApplied = true;
                }
            }
        }

        private void ReprocessResultsView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                ReprocessResult resultData = (ReprocessResult)ReprocessResultsView.Rows[e.RowIndex].DataBoundItem;
                if (_resultPrices.ContainsKey(resultData.ItemID))
                {
                    _resultPrices[resultData.ItemID] = resultData.UnitSellPrice;
                }
                else
                {
                    _resultPrices.Add(resultData.ItemID, resultData.UnitSellPrice);
                }

                ReprocessResultsView.Refresh();
                _reprocessJob.ClearTotalResultsValue();
                UpdateValueAfter();

                foreach (Asset item in _assets)
                {
                    item.ClearReprocessValue();
                    item.SetReprocessPrices(_resultPrices);
                }
                ItemsToReprocessView.Refresh();
            }
        }


        private void ItemsToReprocessView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                if (ItemsToReprocessView.Columns[e.ColumnIndex] == TotalValueColumn)
                {
                    Asset assetData = (Asset)ItemsToReprocessView.Rows[e.RowIndex].DataBoundItem;
                    decimal totalValue = assetData.TotalValue;
                    decimal reprocessValue = assetData.ReprocessValue;

                    Color col = UserAccount.CurrentGroup.Settings.Rpt_WarningDataTextColour;

                    if (totalValue > (reprocessValue * 1.05m))
                    {
                        col = UserAccount.CurrentGroup.Settings.Rpt_GoodDataTextColour;
                    }
                    else if (reprocessValue  > (totalValue * 1.05m))
                    {
                        col = UserAccount.CurrentGroup.Settings.Rpt_DangerDataTextColour;
                    }

                    DataGridViewCellStyle style = new DataGridViewCellStyle(e.CellStyle);
                    style.ForeColor = col;
                    e.CellStyle.ApplyStyle(style);
                }
                else if (ItemsToReprocessView.Columns[e.ColumnIndex] == ReprocessValueColumn)
                {
                    Asset assetData = (Asset)ItemsToReprocessView.Rows[e.RowIndex].DataBoundItem;
                    decimal totalValue = assetData.TotalValue;
                    decimal reprocessValue = assetData.ReprocessValue;

                    Color col = UserAccount.CurrentGroup.Settings.Rpt_WarningDataTextColour;

                    if (totalValue > (reprocessValue * 1.05m))
                    {
                        col = UserAccount.CurrentGroup.Settings.Rpt_DangerDataTextColour;
                    }
                    else if (reprocessValue > (totalValue * 1.05m))
                    {
                        col = UserAccount.CurrentGroup.Settings.Rpt_GoodDataTextColour;
                    }

                    DataGridViewCellStyle style = new DataGridViewCellStyle(e.CellStyle);
                    style.ForeColor = col;
                    e.CellStyle.ApplyStyle(style);
                }
            }
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            ReprocessHistory history = new ReprocessHistory();
            history.MdiParent = this.MdiParent;
            history.Show();
        }


        private class ContainerItem
        {
            private string _text;
            private Asset _data;

            public ContainerItem(Asset data)
            {
                if (data != null)
                {
                    _text = data.Item;
                }
                else
                {
                    _text = "All items not in a container";
                }
                _data = data;
            }

            public Asset Data
            {
                get { return _data; }
            }

            public override string ToString()
            {
                return _text;
            } 
        }
    }
}