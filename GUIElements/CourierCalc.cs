using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;

using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.Reporting;
using EveMarketMonitorApp.AbstractionClasses;
using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class CourierCalc : Form
    {
        private string _collateralBasedOn;
        private decimal _collateralPerc;
        private string _rewardBasedOn;
        private decimal _maxReward;
        private decimal _minReward;
        private decimal _maxRewardPerc;
        private decimal _minRewardPerc;
        private decimal _lowsecPickupBonus;
        private decimal _rewardPercPerJump;
        private decimal _rewardPercPerVol;

        private BindingSource _itemsBindingSource;
        private Contract _contract;
        public Contract FinalContract { get { return _contract; } }
        private bool _maintMode = false;
        private List<string> _recentStations;
        private List<string> _recentItems;
        private bool _blockAutoPriceRefresh = false;

        private string _lastItem = "", _lastDest = "", _lastPickup = "";

        public CourierCalc(int ownerID, ContractType type)
        {
            InitializeComponent();

            lblProfit.Text = "";
            lblProfitPerc.Text = "";
            _contract = new Contract(ownerID, 5);
            if (type != ContractType.Any) { _contract.Type = type; }
            InitGUI();
            InitVariables();
            UserAccount.Settings.GetFormSizeLoc(this);
        }

        /// <summary>
        /// Opens the courier calculator in maintenance mode to allow the user to modify the specified contract.
        /// </summary>
        /// <param name="contractID"></param>
        public CourierCalc(Contract contract, bool fromDatabase)
        {
            InitializeComponent();

            _contract = contract;
            btnCreateContract.Text = fromDatabase ? "Accept Changes" : "Create";
            btnClose.Text = "Cancel";
            _maintMode = fromDatabase;
            InitGUI();

            DisplayRouteInfo();

            txtCollateral.Text = new IskAmount(contract.Collateral).ToString();
            txtReward.Text = new IskAmount(contract.Reward).ToString();

            InitVariables();
            CalcValues(true, true);
            UserAccount.Settings.GetFormSizeLoc(this);
        }

        private void InitGUI()
        {
            this.Text = _contract.Type == ContractType.Courier ? "Courier Calculator" : "Create contract";
            lblDropoff.Text = (_contract.Type == ContractType.Courier || 
                _contract.Type == ContractType.Cargo ? "Drop off:" : "Date/Time:");
            cmbDestination.Visible = _contract.Type == ContractType.Courier || _contract.Type == ContractType.Cargo;
            dtpDate.Visible = _contract.Type == ContractType.ItemExchange;
            lblPickup.Text = (_contract.Type == ContractType.Courier || 
                _contract.Type == ContractType.Cargo ? "Pickup:" : "Station:");
            lblBuyPrice.Visible = _contract.Type == ContractType.Courier;
            lblSellPrice.Text = (_contract.Type == ContractType.Courier ? "Sell Price:" : "Price");
            lblSellPrice.Visible = _contract.Type != ContractType.Cargo;
            txtBuyPrice.Visible = _contract.Type == ContractType.Courier;
            txtSellPrice.Visible = _contract.Type != ContractType.Cargo;
            VolPercentageColumn.Visible = _contract.Type == ContractType.Courier;
            ProfitPercentageColumn.Visible = _contract.Type == ContractType.Courier;
            btnExclude.Visible = _contract.Type == ContractType.Courier;
            btnAuto.Visible = _contract.Type == ContractType.Courier || _contract.Type == ContractType.Cargo;
            lblVolume.Text = (_contract.Type == ContractType.Courier || 
                _contract.Type == ContractType.Cargo ? "Volume:" : "Buy/Sell:");
            cmbBuySell.Visible = _contract.Type == ContractType.ItemExchange;
            cmbBuySell.Location = txtVolume.Location;
            txtVolume.Visible = _contract.Type == ContractType.Courier || _contract.Type == ContractType.Cargo;
            txtJumps.Visible = _contract.Type == ContractType.Courier;
            chkLowSec.Visible = _contract.Type == ContractType.Courier;
            lblJumps.Visible = _contract.Type == ContractType.Courier;
            lblLowSec.Visible = _contract.Type == ContractType.Courier;
            lblProfit.Visible = _contract.Type == ContractType.Courier;
            lblGrossProfit.Visible = _contract.Type == ContractType.Courier;
            lblProfitPerc.Visible = _contract.Type == ContractType.Courier;
            lblReward.Visible = _contract.Type == ContractType.Courier;
            txtReward.Visible = _contract.Type == ContractType.Courier;
            lblCollateral.Text = (_contract.Type == ContractType.Courier ? "Collateral:" : "Total Price:");
            lblCollateral.Visible = _contract.Type != ContractType.Cargo;
            SellPriceColumn.HeaderText = (_contract.Type == ContractType.Courier ? "Sell Price" : "Price");
            SellPriceColumn.Visible = _contract.Type != ContractType.Cargo;
            BuyPriceColumn.HeaderText = (_contract.Type == ContractType.Courier ? "Buy Price" : "Est. Value");
            BuyPriceColumn.Visible = _contract.Type != ContractType.Cargo;
            chkAutoCalcItemPrice.Visible = _contract.Type == ContractType.ItemExchange;
            ProfitPercentageColumn.Visible = _contract.Type != ContractType.Cargo;
        }

        public void InitVariables()
        {
            ReportGroupSettings settings = UserAccount.CurrentGroup.Settings;
            _collateralBasedOn = settings.CollateralBasedOn;
            _collateralPerc = settings.CollateralPercentage;
            _rewardBasedOn = settings.RewardBasedOn;
            _minReward = settings.MinReward;
            _minRewardPerc = settings.MinRewardPercentage;
            _maxReward = settings.MaxReward;
            _maxRewardPerc = settings.MaxRewardPercentage;
            _lowsecPickupBonus = settings.LowSecPickupBonusPerc;
            _rewardPercPerJump = settings.RewardPercPerJump;
            _rewardPercPerVol = settings.VolumeBasedRewardPerc;
            
            _recentStations = settings.RecentStations;
            _recentItems = settings.RecentItems;
        }

        private void CourierCalc_Load(object sender, EventArgs e)
        {
            bool corp = false;

            Diagnostics.ResetAllTimers();
            Diagnostics.StartTimer("CourierCalc.Load");
            Diagnostics.StartTimer("CourierCalc.Load.Part1");
            APICharacter character = UserAccount.CurrentGroup.GetCharacter(_contract.OwnerID, ref corp);

            _itemsBindingSource = new BindingSource();
            _itemsBindingSource.DataSource = _contract.Items;
            contractItemsGrid.AutoGenerateColumns = false;

            DataGridViewCellStyle iskStyle = new DataGridViewCellStyle(BuyPriceColumn.DefaultCellStyle);
            iskStyle.Format = IskAmount.FormatString();
            BuyPriceColumn.DefaultCellStyle = iskStyle;
            SellPriceColumn.DefaultCellStyle = iskStyle;
            DataGridViewCellStyle percentStyle = new DataGridViewCellStyle(VolPercentageColumn.DefaultCellStyle);
            percentStyle.Format = "#0.00 %;-#0.00 %;#0.00 %";
            VolPercentageColumn.DefaultCellStyle = percentStyle;
            ProfitPercentageColumn.DefaultCellStyle = percentStyle;

            contractItemsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            contractItemsGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            UserAccount.Settings.GetColumnWidths(this.Name, contractItemsGrid);

            contractItemsGrid.DataSource = _itemsBindingSource;
            ItemColumn.DataPropertyName = "Item";
            QuantityColumn.DataPropertyName = "Quantity";
            SellPriceColumn.DataPropertyName = "SellPrice";
            BuyPriceColumn.DataPropertyName = "BuyPrice";
            VolPercentageColumn.DataPropertyName = "PercOfTotalVolume";
            ProfitPercentageColumn.DataPropertyName = "PercOfTotalProfit";

            contractItemsGrid.SelectionChanged += new EventHandler(contractItemsGrid_SelectionChanged);
            contractItemsGrid.KeyDown += new KeyEventHandler(contractItemsGrid_KeyDown);
            Diagnostics.StopTimer("CourierCalc.Load.Part1");

            Diagnostics.StartTimer("CourierCalc.Load.Part3");
            cmbPickup.KeyDown += new KeyEventHandler(cmbPickup_KeyDown);
            cmbPickup.SelectedIndexChanged += new EventHandler(cmbPickup_SelectedIndexChanged);
            cmbDestination.KeyDown += new KeyEventHandler(cmbDestination_KeyDown);
            cmbDestination.SelectedIndexChanged += new EventHandler(cmbDestination_SelectedIndexChanged);

            if (_contract.DestinationStationID != 0)
            {
                EveDataSet.staStationsRow station = Stations.GetStation(_contract.DestinationStationID);
                cmbDestination.Text = station.stationName;
            }
            else
            {
                _contract.DestinationStationID = character.Settings.LastCourierDestination;
                EveDataSet.staStationsRow station = Stations.GetStation(character.Settings.LastCourierDestination);
                cmbDestination.Text = station.stationName;
            }
            if (_contract.PickupStationID != 0)
            {
                EveDataSet.staStationsRow station = Stations.GetStation(_contract.PickupStationID);
                cmbPickup.Text = station.stationName;
            }
            else
            {
                cmbPickup.Text = "";
            }
            cmbDestination.Items.AddRange(_recentStations.ToArray());
            cmbDestination.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbDestination.AutoCompleteMode = AutoCompleteMode.Suggest;
            cmbPickup.Items.AddRange(_recentStations.ToArray());
            cmbPickup.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbPickup.AutoCompleteMode = AutoCompleteMode.Suggest;

            if (_contract.Type == ContractType.ItemExchange)
            {
                cmbBuySell.SelectedItem = _maintMode ? (_contract.Collateral > 0 ? "Selling" : "Buying") : "Buying";
                dtpDate.Value = _contract.IssueDate;
            }

            _recentItems.Sort();
            AutoCompleteStringCollection items = new AutoCompleteStringCollection();
            items.AddRange(_recentItems.ToArray());
            txtItem.AutoCompleteCustomSource = items;
            txtItem.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtItem.AutoCompleteMode = AutoCompleteMode.Suggest;
            txtItem.Leave += new EventHandler(txtItem_Leave);
            txtItem.KeyDown += new KeyEventHandler(txtItem_KeyDown);
            txtItem.Enter+=new EventHandler(txtItem_Enter);
            txtItem.Tag = 0;
            txtItem.Text = "";

            EnableGUIElements();

            txtBuyPrice.Enter += new EventHandler(txtBuyPrice_Enter);
            txtBuyPrice.Leave += new EventHandler(txtBuyPrice_Leave);
            txtSellPrice.Enter += new EventHandler(txtSellPrice_Enter);
            txtSellPrice.Leave += new EventHandler(txtSellPrice_Leave);
            txtVolume.Enter += new EventHandler(txtVolume_Enter);
            txtVolume.Leave += new EventHandler(txtVolume_Leave);
            this.FormClosing += new FormClosingEventHandler(CourierCalc_FormClosing);

            txtBuyPrice.Tag = 0;
            txtSellPrice.Tag = 0;
            Diagnostics.StopTimer("CourierCalc.Load.Part3");

            Diagnostics.StartTimer("CourierCalc.Load.Part6");
            RefreshGridView();
            Diagnostics.StopTimer("CourierCalc.Load.Part6");
            Diagnostics.StopTimer("CourierCalc.Load");

            Diagnostics.DisplayDiag("Courier calculator total load time: " +
                Diagnostics.GetRunningTime("CourierCalc.Load").ToString() +
                "\r\n  Data grid setup: " + Diagnostics.GetRunningTime("CourierCalc.Load.Part1").ToString() +
                "\r\n  Set defaults + setup handlers: " + Diagnostics.GetRunningTime("CourierCalc.Load.Part3").ToString() +
                "\r\n  Refresh grid view: " + Diagnostics.GetRunningTime("CourierCalc.Load.Part6").ToString());
        }


        private void EnableGUIElements()
        {
            if (_contract != null)
            {
                if (_contract.Type == ContractType.Courier)
                {
                    if (_contract.DestinationStationID != 0 && _contract.PickupStationID != 0)
                    {
                        btnAddItem.Enabled = true;
                        btnAuto.Enabled = !_maintMode;
                        txtJumps.Enabled = true;
                        txtCollateral.Enabled = true;
                        txtReward.Enabled = true;
                        chkLowSec.Enabled = true;
                        txtItem.Enabled = true;
                        txtQuantity.Enabled = true;
                        btnCreateContract.Enabled = true;
                        txtVolume.Enabled = true;
                    }
                    else
                    {
                        btnAddItem.Enabled = false;
                        btnAuto.Enabled = false;
                        txtJumps.Enabled = false;
                        txtCollateral.Enabled = false;
                        txtReward.Enabled = false;
                        chkLowSec.Enabled = false;
                        txtItem.Enabled = false;
                        txtQuantity.Enabled = false;
                        btnCreateContract.Enabled = false;
                        txtVolume.Enabled = false;
                    }

                    if (contractItemsGrid.SelectedRows.Count == 1)
                    {
                        ContractItem item = contractItemsGrid.SelectedRows[0].DataBoundItem as ContractItem;
                        if (item != null)
                        {
                            txtBuyPrice.Tag = item.BuyPrice;
                            txtBuyPrice.Text = new IskAmount(item.BuyPrice).ToString();
                            txtSellPrice.Tag = item.SellPrice;
                            txtSellPrice.Text = new IskAmount(item.SellPrice).ToString();
                            txtQuantity.Text = item.Quantity.ToString();
                        }

                        txtSellPrice.Enabled = true;
                        txtBuyPrice.Enabled = true;
                        btnApplyPrice.Enabled = true;
                        btnExclude.Enabled = !_maintMode;
                        lblSellPrice.Enabled = true;
                        lblBuyPrice.Enabled = true;
                    }
                    else if (contractItemsGrid.SelectedRows.Count != 0)
                    {
                        // We have multiple items selected..
                        txtSellPrice.Text = "";
                        txtBuyPrice.Text = "";
                        txtQuantity.Text = "";
                        txtSellPrice.Enabled = false;
                        txtBuyPrice.Enabled = false;
                        btnApplyPrice.Enabled = false;
                        btnExclude.Enabled = !_maintMode;
                        lblSellPrice.Enabled = false;
                        lblBuyPrice.Enabled = false;
                    }
                    else
                    {
                        // We have no items selected.
                        txtSellPrice.Text = "";
                        txtBuyPrice.Text = "";
                        txtQuantity.Text = "";
                        txtSellPrice.Enabled = false;
                        txtBuyPrice.Enabled = false;
                        btnApplyPrice.Enabled = false;
                        btnExclude.Enabled = false;
                        lblSellPrice.Enabled = false;
                        lblBuyPrice.Enabled = false;
                    }
                }
                else if (_contract.Type == ContractType.ItemExchange)
                {
                    if (contractItemsGrid.SelectedRows.Count == 1)
                    {
                        _blockAutoPriceRefresh = true;
                        ContractItem item = contractItemsGrid.SelectedRows[0].DataBoundItem as ContractItem;
                        if (item != null)
                        {
                            chkAutoCalcItemPrice.Checked = !item.ForcePrice;
                            txtSellPrice.Tag = item.SellPrice;
                            txtSellPrice.Text = new IskAmount(item.SellPrice).ToString();
                            txtQuantity.Text = item.Quantity.ToString();
                        }

                        chkAutoCalcItemPrice.Enabled = true;
                        btnApplyPrice.Enabled = true;
                        txtSellPrice.Enabled = !chkAutoCalcItemPrice.Checked;
                        lblSellPrice.Enabled = !chkAutoCalcItemPrice.Checked;
                        _blockAutoPriceRefresh = false;
                    }
                    else if (contractItemsGrid.SelectedRows.Count != 0)
                    {
                        // We have multiple items selected..
                        txtSellPrice.Text = "";
                        txtQuantity.Text = "";
                        chkAutoCalcItemPrice.Enabled = false;
                        txtSellPrice.Enabled = false;
                        btnApplyPrice.Enabled = false;
                        lblSellPrice.Enabled = false;
                    }
                    else
                    {
                        // We have no items selected.
                        txtSellPrice.Text = "";
                        txtQuantity.Text = "";
                        chkAutoCalcItemPrice.Enabled = false;
                        txtSellPrice.Enabled = false;
                        btnApplyPrice.Enabled = false;
                        btnExclude.Enabled = false;
                        lblSellPrice.Enabled = false;
                    }
                }
            }
            else if (_contract.Type == ContractType.Cargo)
            {
                if (_contract.DestinationStationID != 0 && _contract.PickupStationID != 0)
                {
                    btnAddItem.Enabled = true;
                    btnAuto.Enabled = !_maintMode;
                    txtItem.Enabled = true;
                    txtQuantity.Enabled = true;
                    btnCreateContract.Enabled = true;
                    txtVolume.Enabled = true;
                }
                else
                {
                    btnAddItem.Enabled = false;
                    btnAuto.Enabled = false;
                    txtItem.Enabled = false;
                    txtQuantity.Enabled = false;
                    btnCreateContract.Enabled = false;
                    txtVolume.Enabled = false;
                }

                if (contractItemsGrid.SelectedRows.Count == 1)
                {
                    ContractItem item = contractItemsGrid.SelectedRows[0].DataBoundItem as ContractItem;
                    if (item != null)
                    {
                        txtQuantity.Text = item.Quantity.ToString();
                    }
                }
                else if (contractItemsGrid.SelectedRows.Count != 0)
                {
                    // We have multiple items selected..
                    txtQuantity.Text = "";
                }
                else
                {
                    // We have no items selected.
                    txtQuantity.Text = "";
                }
            }
        }


        private void btnAddItem_Click(object sender, EventArgs e)
        {
            int itemID = int.Parse(txtItem.Tag.ToString());
            int quantity = 0;
            try
            {
                quantity = int.Parse(txtQuantity.Text);
            }
            catch { }

            if (quantity > 0 && itemID != 0)
            {
                ContractItem existing = null;

                foreach (ContractItem oldItem in _contract.Items)
                {
                    if (oldItem.ItemID == itemID) { existing = oldItem; }
                }
                if (existing != null)
                {
                    existing.Quantity = existing.Quantity + quantity;
                }
                else
                {
                    ContractItem item = new ContractItem(itemID, quantity,
                        AutoContractor.GetSellPrice(itemID, _contract.DestinationStationID),
                        AutoContractor.GetBuyPrice(_contract.OwnerID, itemID, _contract.PickupStationID, quantity, 0),
                        _contract);
                    _contract.Items.Add(item);
                }
                _contract.ExpectedProfit = 0;
                _contract.TotalVolume = 0;
                CalcValues();
            }
        }


        void contractItemsGrid_SelectionChanged(object sender, EventArgs e)
        {
            EnableGUIElements();
        }

        private void btnApplyPrice_Click(object sender, EventArgs e)
        {
            if (contractItemsGrid.SelectedRows.Count > 0)
            {
                ContractItem item = contractItemsGrid.SelectedRows[0].DataBoundItem as ContractItem;
                decimal tmpCollateral = 0;
                if (_contract.Type == ContractType.ItemExchange) { tmpCollateral = _contract.Collateral; }
                if (item != null)
                {
                    if (_contract.Type == ContractType.Courier) { item.BuyPrice = (decimal)txtBuyPrice.Tag; }
                    if (_contract.Type == ContractType.ItemExchange)
                    {
                        item.ForcePrice = !chkAutoCalcItemPrice.Checked; 
                    }
                    item.SellPrice = (decimal)txtSellPrice.Tag;
                    if (_contract.Type == ContractType.ItemExchange)
                    {
                        if ((cmbBuySell.SelectedItem.Equals("Buying") && item.SellPrice > 0) ||
                            (cmbBuySell.SelectedItem.Equals("Selling") && item.SellPrice < 0))
                        {
                            item.SellPrice *= -1;
                        }
                    }
                    item.Quantity = int.Parse(txtQuantity.Text);
                }
                if (_contract.Type == ContractType.ItemExchange) { _contract.Collateral = tmpCollateral; }

                _contract.ExpectedProfit = 0;
                _contract.TotalVolume = 0;
                CalcValues();

                contractItemsGrid.Focus();
                for (int i = 0; i < contractItemsGrid.Rows.Count; i++)
                {
                    if (((ContractItem)contractItemsGrid.Rows[i].DataBoundItem).ItemID == item.ItemID)
                    {
                        contractItemsGrid.Rows[i].Selected = true;
                    }
                }
            }
        }

        private void DisplayRouteInfo()
        {
            if (_contract.PickupStationID != 0 && _contract.DestinationStationID != 0)
            {
                try
                {
                    int jumps = SolarSystemDistances.GetDistanceBetweenStations(
                        _contract.PickupStationID, _contract.DestinationStationID);
                    txtJumps.Text = jumps.ToString();
                    chkLowSec.Checked = Stations.IsLowSec(_contract.PickupStationID) ||
                        Stations.IsLowSec(_contract.DestinationStationID);
                }
                catch (Exception ex)
                {
                    EMMAException emmaEx = ex as EMMAException;
                    if (emmaEx == null)
                    {
                        new EMMAException(ExceptionSeverity.Error, "Problem calculating route from " +
                            _contract.PickupStationID + " to " + _contract.DestinationStationID, ex);
                    }
                    MessageBox.Show("Problem calculating route: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                txtJumps.Text = "0";
                chkLowSec.Checked = false;
            }
        }
        

        private void CalcValues()
        {
            CalcValues(false, false);
        }

        private void CalcValues(bool leaveReward, bool leaveCollateral)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                decimal totBuy = 0;
                decimal totSell = 0;
                decimal totProfit = 0;
                decimal totVolume = 0;
                decimal collateral = 0;
                decimal reward = 0;

                if (_contract.Type == ContractType.Courier)
                {
                    if (!txtJumps.Text.Equals(""))
                    {
                        int jumps = int.Parse(txtJumps.Text);

                        if (_contract.PickupStationID != 0 && _contract.DestinationStationID != 0)
                        {
                            foreach (ContractItem item in _contract.Items)
                            {
                                totBuy += item.BuyPrice * item.Quantity;
                                totSell += item.SellPrice * item.Quantity;
                                totVolume += item.Quantity * (decimal)Items.GetItemVolume(item.ItemID);
                            }

                            totProfit = totSell - totBuy;
                            collateral = AutoContractor.CalcCollateral(_collateralBasedOn, _collateralPerc, totBuy, totSell);
                            reward = AutoContractor.CalcReward(_rewardBasedOn, collateral, totProfit, _minReward, _maxReward,
                                _minRewardPerc, _maxRewardPerc, _rewardPercPerJump, _rewardPercPerVol, _lowsecPickupBonus,
                                jumps, totVolume, Stations.IsLowSec(_contract.PickupStationID));

                            if (!leaveCollateral) { _contract.Collateral = collateral; }
                            if (!leaveReward) { _contract.Reward = reward; }
                            _contract.TotalVolume = totVolume;

                            lblProfit.Text = new IskAmount(totProfit).ToString();
                            txtCollateral.Text = new IskAmount(_contract.Collateral).ToString();
                            txtReward.Text = new IskAmount(_contract.Reward).ToString();

                            if (totProfit > 0)
                            {
                                decimal percprofit = (_contract.Reward / totProfit);
                                lblProfitPerc.Text = "(" + percprofit.ToString("#,##0.0%;-#,##0.0%;0%") + " of profit)";
                            }
                            else
                            {
                                lblProfitPerc.Text = "";
                            }
                            txtVolume.Text = totVolume.ToString("#,##0.00 m3;-#,##0.00 m3;0 m3");
                        }
                    }
                }
                else if (_contract.Type == ContractType.ItemExchange)
                {
                    decimal tmpCollateral = _contract.Collateral;
                    decimal cashToAllocate = tmpCollateral;
                    decimal totalCash = tmpCollateral;
                    decimal totalValue = 0.0m;
                    List<int> unvaluedItems = new List<int>();
                    ContractItemList items = _contract.Items;
                    foreach (ContractItem item in items)
                    {
                        item.BuyPrice = UserAccount.CurrentGroup.ItemValues.GetItemValue(item.ItemID);
                        totalValue += (item.ForcePrice ? 0 : item.Quantity * item.BuyPrice);
                        totalCash -= (item.ForcePrice ? item.Quantity * item.SellPrice : 0);
                    }
                    cashToAllocate = totalCash;

                    for (int i = 0; i < items.Count; i++)
                    {
                        ContractItem item = items[i];
                        if (!item.ForcePrice)
                        {
                            decimal itemTotalValue = item.Quantity * item.BuyPrice;
                            if (itemTotalValue != 0)
                            {
                                item.SellPrice = ((itemTotalValue / totalValue) * totalCash) / item.Quantity;
                                cashToAllocate -= item.SellPrice * item.Quantity;
                            }
                            else
                            {
                                unvaluedItems.Add(i);
                            }
                        }
                    }

                    foreach (int index in unvaluedItems)
                    {
                        ContractItem item = items[index];
                        item.SellPrice = (cashToAllocate / unvaluedItems.Count) / item.Quantity;
                        cashToAllocate -= item.SellPrice * item.Quantity;
                    }

                    if (Math.Abs(cashToAllocate) > 0.1m)
                    {
                        MessageBox.Show("The total price of items does not match the total contract price.\r\n" +
                            "The contract price will be adjusted to match.", 
                            "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        tmpCollateral -= cashToAllocate;
                    }

                    // Setting the sell price clears collateral so just make sure we set it again after
                    // we're done.
                    _contract.Collateral = tmpCollateral;
                }
                RefreshGridView();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void RefreshGridView()
        {
            if (_itemsBindingSource == null) { _itemsBindingSource = new BindingSource(); }
            _itemsBindingSource.DataSource = _contract.Items;

            if (contractItemsGrid.DataSource == null)
            {
                contractItemsGrid.AutoGenerateColumns = false;
                contractItemsGrid.DataSource = _itemsBindingSource;
                ItemColumn.DataPropertyName = "Item";
                QuantityColumn.DataPropertyName = "Quantity";
                SellPriceColumn.DataPropertyName = "SellPrice";
                BuyPriceColumn.DataPropertyName = "BuyPrice";
                VolPercentageColumn.DataPropertyName = "PercOfTotalVolume";
                ProfitPercentageColumn.DataPropertyName = "PercOfTotalProfit";
            }

            ListSortDirection sortDirection;
            if (contractItemsGrid.SortOrder == SortOrder.Descending) { sortDirection = ListSortDirection.Descending; }
            else { sortDirection = ListSortDirection.Ascending; }

            if (contractItemsGrid.SortedColumn == null)
            {
                contractItemsGrid.Sort(ItemColumn, sortDirection);
            }
            else
            {
                contractItemsGrid.Sort(contractItemsGrid.SortedColumn, sortDirection);
            }

            contractItemsGrid.Refresh();
            contractItemsGrid.ClearSelection();
        }


        private void btnClose_Click(object sender, EventArgs e)
        {
            if (_maintMode)
            {
                this.DialogResult = DialogResult.Cancel;
            }
            this.Close();
        }

        void contractItemsGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                List<ContractItem> itemsToRemove = new List<ContractItem>();
                foreach (DataGridViewRow row in contractItemsGrid.SelectedRows)
                {
                    itemsToRemove.Add((ContractItem)row.DataBoundItem);
                }
                RemoveItems(itemsToRemove, false);
            }
        }

        private void cmdExclude_Click(object sender, EventArgs e)
        {
            List<ContractItem> itemsToRemove = new List<ContractItem>();
            foreach (DataGridViewRow row in contractItemsGrid.SelectedRows)
            {
                itemsToRemove.Add((ContractItem)row.DataBoundItem);
            }
            RemoveItems(itemsToRemove, true);
        }

        private void RemoveItems(List<ContractItem> items, bool permenantly)
        {
            if (items.Count > 0)
            {
                bool corp = false;
                UserAccount.CurrentGroup.GetCharacter(_contract.OwnerID, ref corp);
                foreach (ContractItem item in items)
                {
                    if (permenantly)
                    {
                        Assets.SetAutoConExcludeFlag(_contract.OwnerID, corp, _contract.PickupStationID, 
                            item.ItemID, true);
                    }
                    _contract.Items.Remove(item);
                }
                CalcValues();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            _contract.Items = new ContractItemList();
            RefreshGridView();
        }

        private void btnCreateContract_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                if (_contract.Type == ContractType.Courier)
                {
                    int pickupID = _contract.PickupStationID;
                    int destID = _contract.DestinationStationID;
                    CreateContract();
                    int ownerID = _contract.OwnerID;
                    _contract = new Contract(ownerID, 5);
                    _contract.PickupStationID = pickupID;
                    _contract.DestinationStationID = destID;
                    RefreshGridView();
                }
                else if (_contract.Type == ContractType.ItemExchange)
                {
                    CreateContract();
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            btnClose.Text = "Close";

            if (_maintMode || _contract.Type == ContractType.ItemExchange ||
                _contract.Type == ContractType.Cargo)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void CreateContract()
        {
            if (dtpDate.Value != null && _contract.Type == ContractType.ItemExchange)
            {
                _contract.IssueDate = dtpDate.Value;
            }

            if (_maintMode)
            {
                // If in maintentance mode then first delete the old contract.
                Contracts.Delete(_contract);
            }

            Contracts.Create(_contract);
        }

        private void btnAuto_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            try
            {
                Diagnostics.ResetAllTimers();
                Diagnostics.StartTimer("CourierCalc.Auto");
                _contract.Items = new ContractItemList();

                AutoContractor autoCon = new AutoContractor();
                ContractList list = autoCon.GenerateContracts(_contract.OwnerID, _contract.PickupStationID, 
                    _contract.DestinationStationID, false);
                if (list.Count > 0)
                {
                    _contract = list[0];
                }

                Diagnostics.StartTimer("CourierCalc.CalcValues");
                CalcValues();
                Diagnostics.StopTimer("CourierCalc.CalcValues");
                Diagnostics.StopTimer("CourierCalc.Auto");

                Diagnostics.DisplayDiag("Total time: " +
                    Diagnostics.GetRunningTime("CourierCalc.Auto").ToString() +
                    "\r\n\tLoad assets: " + Diagnostics.GetRunningTime("GenerateContract.Part1").ToString() +
                    "\r\n\tGet sell prices: " + Diagnostics.GetRunningTime("GenerateContract.Part2").ToString() +
                    "\r\n\tGet buy prices: " + Diagnostics.GetRunningTime("GenerateContract.Part6").ToString() +
                    "\r\n\tCalc collateral: " + Diagnostics.GetRunningTime("GenerateContract.Part3").ToString() +
                    "\r\n\tRoute length: " + Diagnostics.GetRunningTime("GenerateContract.Part4").ToString() +
                    "\r\n\tClearing up: " + Diagnostics.GetRunningTime("GenerateContract.Part5").ToString() +
                    "\r\n\tRecalc values: " + Diagnostics.GetRunningTime("CourierCalc.CalcValues").ToString());
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        void txtItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Enter)
            {
                SetSelectedItem();
            }
        }

        void txtItem_Leave(object sender, EventArgs e)
        {
            SetSelectedItem();
        }

        private void SetSelectedItem()
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if(!txtItem.Text.Equals(_lastItem))
                {
                    txtItem.Tag = (short)0;
                    if (!txtItem.Text.Equals(""))
                    {
                        try
                        {
                            EveDataSet.invTypesRow item = Items.GetItem(txtItem.Text);
                            if (item != null)
                            {
                                txtItem.Tag = item.typeID;
                                string name = item.typeName;
                                txtItem.Text = name;
                                if (!_recentItems.Contains(name))
                                {
                                    _recentItems.Add(name);
                                    txtItem.AutoCompleteCustomSource.Add(name);
                                }
                            }
                        }
                        catch (EMMADataException) { }
                        _lastItem = txtItem.Text;
                        txtQuantity.Text = "1";
                    }
                    if ((short)txtItem.Tag == 0) { txtItem.Text = ""; }
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        void cmbDestination_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                SetDestination();
            }
        }

        void cmbDestination_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetDestination();
        }

        private void SetDestination()
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (!cmbDestination.Text.Equals("") && !cmbDestination.Text.Equals(_lastDest))
                {
                    try
                    {
                        EveDataSet.staStationsRow station = Stations.GetStation(cmbDestination.Text);
                        if (station != null)
                        {
                            _contract.DestinationStationID = station.stationID;
                            string name = station.stationName;
                            cmbDestination.Text = name;
                            if (!_recentStations.Contains(name))
                            {
                                cmbDestination.Items.Add(name);
                                _recentStations.Add(name);
                            }
                        }
                    }
                    catch (EMMADataException)
                    {
                        _contract.DestinationStationID = 0;
                        cmbDestination.Text = "";
                    }

                    _lastDest = cmbDestination.Text;
                    DisplayRouteInfo();
                    foreach (ContractItem item in _contract.Items)
                    {
                        if (_contract.PickupStationID != 0)
                        {
                            item.BuyPrice = AutoContractor.GetBuyPrice(
                                _contract.OwnerID, item.ItemID, _contract.PickupStationID, item.Quantity, 0);
                        }
                        if (_contract.DestinationStationID != 0)
                        {
                            item.SellPrice = AutoContractor.GetSellPrice(
                                item.ItemID, _contract.DestinationStationID);
                        }
                    }
                    CalcValues();
                }
                if (!_maintMode)
                {
                    _contract.Items = new ContractItemList();
                }

                EnableGUIElements();
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        void cmbPickup_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                SetSelectedPickup();
            }
        }

        void cmbPickup_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetSelectedPickup();
        }

        private void SetSelectedPickup()
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (!cmbPickup.Text.Equals("") && !cmbPickup.Text.Equals(_lastPickup))
                {
                    try
                    {
                        EveDataSet.staStationsRow station = Stations.GetStation(cmbPickup.Text);
                        if (station != null)
                        {
                            _contract.PickupStationID = station.stationID;
                            string name = station.stationName;
                            cmbPickup.Text = name;
                            if (!_recentStations.Contains(name))
                            {
                                cmbPickup.Items.Add(name);
                                _recentStations.Add(name);
                            }
                        }
                    }
                    catch (EMMADataException)
                    {
                        _contract.PickupStationID = 0;
                        cmbPickup.Text = "";
                    }

                    _lastPickup = cmbPickup.Text;
                    if (_contract.Type == ContractType.Courier)
                    {
                        DisplayRouteInfo();
                        foreach (ContractItem item in _contract.Items)
                        {
                            if (_contract.PickupStationID != 0)
                            {
                                item.BuyPrice = AutoContractor.GetBuyPrice(
                                    _contract.OwnerID, item.ItemID, _contract.PickupStationID, item.Quantity, 0);
                            }
                            if (_contract.DestinationStationID != 0)
                            {
                                item.SellPrice = UserAccount.CurrentGroup.ItemValues.GetItemValue(
                                    item.ItemID, _contract.DestinationStationID);
                            }
                        }
                        CalcValues();
                    }
                }
                EnableGUIElements();
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }


        private void txtReward_Enter(object sender, EventArgs e)
        {
            decimal value = _contract.Reward;
            value = Decimal.Round(value, 2);
            txtReward.Text = value.ToString();
        }

        private void txtReward_Leave(object sender, EventArgs e)
        {
            try
            {
                _contract.Reward = decimal.Parse(txtReward.Text);
            }
            catch
            {
                _contract.Reward = 0;
            }
            txtReward.Text = new IskAmount(_contract.Reward).ToString();
            CalcValues(true, true);
        }

        private void txtCollateral_Enter(object sender, EventArgs e)
        {
            decimal value = _contract.Collateral;
            value = Decimal.Round(value, 2);
            txtCollateral.Text = value.ToString();
        }

        private void txtCollateral_Leave(object sender, EventArgs e)
        {
            try
            {
                bool done = false;
                if (_contract.Type == ContractType.ItemExchange)
                {
                    if (cmbBuySell.SelectedItem == null || cmbBuySell.SelectedItem.ToString().Equals("Buying"))
                    {
                        _contract.Collateral = -1 * decimal.Parse(txtCollateral.Text);
                        if (_contract.Collateral > 0) { _contract.Collateral *= -1; }
                        done = true;
                    }
                }
               
                if(!done)
                {
                    _contract.Collateral = decimal.Parse(txtCollateral.Text);
                    if (_contract.Collateral < 0) { _contract.Collateral *= -1; }
                }
            }
            catch
            {
                _contract.Collateral = 0;
            }
            txtCollateral.Text = new IskAmount(_contract.Type == ContractType.ItemExchange ? 
                Math.Abs(_contract.Collateral) : _contract.Collateral).ToString();
            CalcValues(true, true);
        }

        private void txtBuyPrice_Enter(object sender, EventArgs e)
        {
            decimal value = (decimal)txtBuyPrice.Tag;
            value = Decimal.Round(value, 2);
            txtBuyPrice.Text = value.ToString();
        }

        private void txtBuyPrice_Leave(object sender, EventArgs e)
        {
            try
            {
                txtBuyPrice.Tag = decimal.Parse(txtBuyPrice.Text);
            }
            catch
            {
                txtBuyPrice.Tag = 0;
            }
            txtBuyPrice.Text = new IskAmount((decimal)txtBuyPrice.Tag).ToString();
        }

        private void txtSellPrice_Enter(object sender, EventArgs e)
        {
            decimal value = (decimal)txtSellPrice.Tag;
            value = Decimal.Round(value, 2);
            txtSellPrice.Text = value.ToString();
        }

        private void txtSellPrice_Leave(object sender, EventArgs e)
        {
            try
            {
                txtSellPrice.Tag = decimal.Parse(txtSellPrice.Text);
            }
            catch
            {
                txtSellPrice.Tag = 0;
            }
            txtSellPrice.Text = new IskAmount((decimal)txtSellPrice.Tag).ToString();
        }

        void txtVolume_Enter(object sender, EventArgs e)
        {
            txtVolume.Text = _contract.TotalVolume.ToString();
        }

        void txtVolume_Leave(object sender, EventArgs e)
        {
            try
            {
                _contract.TotalVolume = decimal.Parse(txtVolume.Text);
            }
            catch
            {
                _contract.TotalVolume = 0;
            }
            txtVolume.Text = ((decimal)_contract.TotalVolume).ToString("#,##0.00 m3;-#,##0.00 m3;0 m3");
            CalcValues();
        }

        void CourierCalc_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (UserAccount.Settings != null)
            {
                if (UserAccount.CurrentGroup != null)
                {
                    ReportGroupSettings settings = UserAccount.CurrentGroup.Settings;
                    settings.RecentStations = _recentStations;
                    settings.RecentItems = _recentItems;
                    if (_contract.DestinationStationID != 0)
                    {
                        UserAccount.CurrentGroup.GetCharacter(_contract.OwnerID).Settings.LastCourierDestination =
                            _contract.DestinationStationID;
                    }
                }
                UserAccount.Settings.StoreColumnWidths(this.Name, contractItemsGrid);
                UserAccount.Settings.StoreFormSizeLoc(this);
            }
        }

        private class ItemNameComparer : IComparer
        {
            private static int sortOrderModifier = 1;

            public ItemNameComparer(SortOrder sortOrder)
            {
                if (sortOrder == SortOrder.Descending)
                {
                    sortOrderModifier = -1;
                }
                else if (sortOrder == SortOrder.Ascending)
                {
                    sortOrderModifier = 1;
                }
            }

            public int Compare(object x, object y)
            {
                DataGridViewRow dataGridViewRow1 = (DataGridViewRow)x;
                DataGridViewRow dataGridViewRow2 = (DataGridViewRow)y;

                string sValue1 = dataGridViewRow1.Cells["ItemColumn"].Value.ToString();
                string sValue2 = dataGridViewRow2.Cells["ItemColumn"].Value.ToString();

                int compareResult = String.Compare(sValue1, sValue2, StringComparison.InvariantCultureIgnoreCase);
                return compareResult * sortOrderModifier;
            }

            /*private class myString : IComparable
            {
                private string _value;

                public override bool Equals(object obj)
                {
                    return base.Equals(obj);
                }

                public int CompareTo(object obj)
                {
                    
                }
            }*/
        }

        private void txtItem_Enter(object sender, EventArgs e)
        {
            contractItemsGrid.ClearSelection();
        }

        private void cmbBuySell_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbBuySell.SelectedItem.ToString().Equals("Buying"))
            {
                if (_contract.Collateral > 0)
                {
                    _contract.Collateral *= -1;
                }
            }
            else
            {
                if (_contract.Collateral < 0)
                {
                    _contract.Collateral *= -1;
                }
            }
            txtCollateral.Text = (new IskAmount(_contract.Collateral)).ToString();
            CalcValues();
        }

        private void chkAutoCalcItemPrice_CheckedChanged(object sender, EventArgs e)
        {
            if (contractItemsGrid.SelectedRows.Count > 0)
            {
                ContractItem item = contractItemsGrid.SelectedRows[0].DataBoundItem as ContractItem;
                if (item != null)
                {
                    item.ForcePrice = !chkAutoCalcItemPrice.Checked;
                }
                if (!_blockAutoPriceRefresh)
                {
                    EnableGUIElements();
                    if (chkAutoCalcItemPrice.Checked)
                    {
                        CalcValues();
                    }
                }
            }
        }

    }


}