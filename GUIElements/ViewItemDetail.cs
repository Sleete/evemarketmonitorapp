using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.Reporting;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class ViewItemDetail : Form
    {
        private CharCorpOption _lastSelectedOwner;
        private Dictionary<int, List<int>> _useDataFrom;
        private bool _selectAllOwners = false;
        private bool _selectAllItems = false;

        private List<FinanceAccessParams> _finParams = new List<FinanceAccessParams>();
        private List<AssetAccessParams> _assetParams = new List<AssetAccessParams>();
        private List<int> _itemIDs = new List<int>();
        private bool _generatePlaceholderOrders = false;

        private bool _ordersDataVisible = false;
        private bool _inventoryDataVisible = false;
        private bool _transactionsDataVisible = false;

        private bool _ordersGettingData = false;
        private bool _inventoryGettingData = false;
        private bool _transactionsGettingData = false;

        private Thread _ordersUpdateThread;
        private Thread _transactionsUpdateThread;
        private Thread _inventoryUpdateThread;

        private OrdersList _sellOrders;
        private OrdersList _buyOrders;
        private TransactionList _transactions;
        private AssetList _inventory;

        delegate void UpdateCallback();
        delegate void AddTransCallback(TransactionList transactions);
        delegate void AddInventoryCallback(AssetList inventory);
        
        public ViewItemDetail()
        {
            InitializeComponent();

            UserAccount.Settings.GetFormSizeLoc(this);
            UserAccount.Settings.GetColumnWidths(this.Name, buyOrdersView);
            UserAccount.Settings.GetColumnWidths(this.Name, sellOrdersView);
            UserAccount.Settings.GetColumnWidths(this.Name, inventoryView);
            UserAccount.Settings.GetColumnWidths(this.Name, transactionsView);
            this.FormClosing += new FormClosingEventHandler(ViewItemDetail_FormClosing);

            _useDataFrom = new Dictionary<int, List<int>>();

            dtpStartDate.Value = UserAccount.Settings.UseLocalTimezone ? 
                DateTime.Now.AddMonths(-1) : DateTime.UtcNow.AddMonths(-1);
            dtpEndDate.Value = UserAccount.Settings.UseLocalTimezone ? DateTime.Now : DateTime.UtcNow;
            rdbNone.Checked = true;
            chkGenPlaceholders.Checked = _generatePlaceholderOrders;
        }

        private void ViewItemDetail_Load(object sender, EventArgs e)
        {
            try
            {
                chkOwners.ItemCheck += new ItemCheckEventHandler(chkOwners_ItemCheck);
                chkOwners.SelectedIndexChanged += new EventHandler(chkOwners_SelectedIndexChanged);
                RefreshCharList();

                EveDataSet.invTypesDataTable allitems = UserAccount.CurrentGroup.ItemValues.GetAllItems();
                ItemInfo[] info = new ItemInfo[allitems.Count];
                for(int i = 0 ; i < allitems.Count; i++)
                {
                    EveDataSet.invTypesRow item = allitems[i];
                    info[i] = new ItemInfo(item.typeID, item.typeName);
                }
                chkItems.Sorted = true;
                chkItems.Items.AddRange(info);

                DataGridViewCellStyle style = new DataGridViewCellStyle(BuyOrderPriceColumn.DefaultCellStyle);
                style.Format = IskAmount.FormatString();
                BuyOrderPriceColumn.DefaultCellStyle = style;
                SellOrderPriceColumn.DefaultCellStyle = style;
                TransactionsPriceColumn.DefaultCellStyle = style;
                TransactionsTotalColumn.DefaultCellStyle = style;
                TransactionsGrossProfitColumn.DefaultCellStyle = style;
                TransactionsGrossUnitProfit.DefaultCellStyle = style;

                buyOrdersView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                buyOrdersView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                buyOrdersView.AutoGenerateColumns = false;
                BuyOrderDateTimeColumn.DataPropertyName = "Date";
                BuyOrderItemColumn.DataPropertyName = "Item";
                BuyOrderMovement12HoursColumn.DataPropertyName = "TwelveHourMovement";
                BuyOrderMovement2DaysColumn.DataPropertyName = "TwoDayMovement";
                BuyOrderMovement7DaysColumn.DataPropertyName = "SevenDayMovement";
                BuyOrderOwnerColumn.DataPropertyName = "Owner";
                BuyOrderPriceColumn.DataPropertyName = "Price";
                BuyOrderRangeColumn.DataPropertyName = "RangeText";
                BuyOrderRegionColumn.DataPropertyName = "Region";
                BuyOrderRemainingUnits.DataPropertyName = "RemainingVol";
                BuyOrderStationColumn.DataPropertyName = "Station";
                BuyOrderStatus.DataPropertyName = "State";
                BuyOrderSystemColumn.DataPropertyName = "System";
                BuyOrderTotaUnits.DataPropertyName = "TotalVol";

                sellOrdersView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                sellOrdersView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                sellOrdersView.AutoGenerateColumns = false;
                SellOrderDateColumn.DataPropertyName = "Date";
                SellOrderItemColumn.DataPropertyName = "Item";
                SellOrderMovement12HoursColumn.DataPropertyName = "TwelveHourMovement";
                SellOrderMovement2DaysColumn.DataPropertyName = "TwoDayMovement";
                SellOrderMovement7DaysColumn.DataPropertyName = "SevenDayMovement";
                SellOrderOwnerColumn.DataPropertyName = "Owner";
                SellOrderPriceColumn.DataPropertyName = "Price";
                SellOrderRangeColumn.DataPropertyName = "RangeText";
                SellOrderRegionColumn.DataPropertyName = "Region";
                SellOrderRemainingUnitsColumn.DataPropertyName = "RemainingVol";
                SellOrderStationColumn.DataPropertyName = "Station";
                SellOrderStatusColumn.DataPropertyName = "State";
                SellOrderSystemColumn.DataPropertyName = "System";
                SellOrderTotalUnitsColumn.DataPropertyName = "TotalVol";

                transactionsView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                transactionsView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                transactionsView.AutoGenerateColumns = false;
                TransactionsBuyerCharacterColumn.DataPropertyName = "BuyerChar";
                TransactionsBuyerColumn.DataPropertyName = "Buyer";
                TransactionsBuyerWalletColumn.DataPropertyName = "BuyerWallet";
                TransactionsDateTimeColumn.DataPropertyName = "Datetime";
                TransactionsItemColumn.DataPropertyName = "Item";
                TransactionsPriceColumn.DataPropertyName = "Price";
                TransactionsQuantityColumn.DataPropertyName = "Quantity";
                TransactionsSellerCharacterColumn.DataPropertyName = "SellerChar";
                TransactionsSellerColumn.DataPropertyName = "Seller";
                TransactionsSellerWalletColumn.DataPropertyName = "SellerWallet";
                TransactionsStationColumn.DataPropertyName = "Station";
                TransactionsTotalColumn.DataPropertyName = "Total";
                TransactionsGrossProfitColumn.DataPropertyName = "GrossProfit";
                TransactionsGrossUnitProfit.DataPropertyName = "GrossUnitProfit";

                inventoryView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                inventoryView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                inventoryView.AutoGenerateColumns = false;
                InventoryItemColumn.DataPropertyName = "Item";
                InventoryQuantityColumn.DataPropertyName = "Quantity";
                InventoryRegionColumn.DataPropertyName = "Region";
                InventoryLocationColumn.DataPropertyName = "Location";
                InventorySystemColumn.DataPropertyName = "System";
                InventoryOwnerColumn.DataPropertyName = "Owner";

                _transactions = new TransactionList();
                _inventory = new AssetList();
                transactionsView.DataSource = _transactions;
                inventoryView.DataSource = _inventory;
            }
            catch (Exception ex)
            {
                EMMAException emmaEx = ex as EMMAException;
                if (emmaEx == null)
                {
                    new EMMAException(ExceptionSeverity.Error, "Problem loading item detail view", ex);
                }
                MessageBox.Show("Problem loading item detail view:\r\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshCharList()
        {
            chkOwners.Items.Clear();
            chkOwners.Enabled = false;

            List<CharCorpOption> owners = UserAccount.CurrentGroup.GetCharCorpOptions();
            foreach (CharCorpOption owner in owners)
            {
                // Note that 'useDataFrom' is automatically initalised as the items are added 
                // to the list by the 'chkOwners_ItemCheck' method.
                chkOwners.Items.Add(owner, true);
            }
            chkOwners.Enabled = true;
            chkOwners.Sorted = true;

            RefreshWalletList();
        }

        private void RefreshWalletList()
        {
            CharCorpOption selectedOwner = (CharCorpOption)chkOwners.SelectedItem;
            chkWallets.Items.Clear();
            chkWallets.Enabled = false;

            if (selectedOwner != null)
            {
                int id = selectedOwner.Corp ? selectedOwner.CharacterObj.CorpID : selectedOwner.CharacterObj.CharID;
                if (_useDataFrom.ContainsKey(id))
                {
                    if (selectedOwner.Corp)
                    {
                        foreach (EMMADataSet.WalletDivisionsRow wallet in selectedOwner.CharacterObj.WalletDivisions)
                        {
                            chkWallets.Items.Add(wallet.ID + (wallet.ID == 1000 ? " (Master)" : "") +
                                " - " + wallet.Name, _useDataFrom[id].Contains(wallet.ID));
                        }
                        chkWallets.Enabled = true;
                    }
                    else
                    {
                        chkWallets.Items.Add("Main Wallet", true);
                    }
                }
            }
        }

        private void chkOwners_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_lastSelectedOwner != null)
            {
                if (_lastSelectedOwner.Corp && !_lastSelectedOwner.Equals((CharCorpOption)chkOwners.SelectedItem))
                {
                    int id = _lastSelectedOwner.Corp ?
                        _lastSelectedOwner.CharacterObj.CorpID : _lastSelectedOwner.CharacterObj.CharID;

                    if (_useDataFrom.ContainsKey(id))
                    {
                        _useDataFrom.Remove(id);
                    }

                    if (chkWallets.CheckedItems.Count > 0)
                    {
                        _useDataFrom.Add(id, new List<int>());
                        foreach (object wallet in chkWallets.CheckedItems)
                        {
                            string walletText = wallet.ToString();
                            int walletID = int.Parse(walletText.Remove(walletText.IndexOf(" ")));
                            _useDataFrom[id].Add(walletID);
                        }
                    }
                }
            }

            _lastSelectedOwner = (CharCorpOption)chkOwners.SelectedItem;
            RefreshWalletList();
        }

        private void chkOwners_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            _ordersDataVisible = false;
            _inventoryDataVisible = false;
            _transactionsDataVisible = false;
            _ordersGettingData = false;
            _inventoryGettingData = false;
            _transactionsGettingData = false;
            RefreshGUI();

            CharCorpOption item = (CharCorpOption)chkOwners.Items[e.Index];
            if (item != null)
            {
                int id = item.Corp ? item.CharacterObj.CorpID : item.CharacterObj.CharID;
                if (e.NewValue == CheckState.Unchecked && _useDataFrom.ContainsKey(id))
                {
                    _useDataFrom.Remove(id);
                }
                if (e.NewValue == CheckState.Checked && !_useDataFrom.ContainsKey(id))
                {
                    _useDataFrom.Add(id, new List<int>());
                    if (item.Corp)
                    {
                        int[] walletIDs = { 1000, 1001, 1002, 1003, 1004, 1005, 1006 };
                        _useDataFrom[id].AddRange(walletIDs);
                    }
                    else
                    {
                        _useDataFrom[id].Add(1000);
                    }
                }
                RefreshWalletList();
            }
        }

        private void chkItems_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            _ordersDataVisible = false;
            _inventoryDataVisible = false;
            _transactionsDataVisible = false;
            _ordersGettingData = false;
            _inventoryGettingData = false;
            _transactionsGettingData = false;
            RefreshGUI();
        }

        private void chkWallets_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            _ordersDataVisible = false;
            _inventoryDataVisible = false;
            _transactionsDataVisible = false;
            _ordersGettingData = false;
            _inventoryGettingData = false;
            _transactionsGettingData = false;
            RefreshGUI();
        }

        private void btnToggleAllOwners_Click(object sender, EventArgs e)
        {
            _ordersDataVisible = false;
            _inventoryDataVisible = false;
            _transactionsDataVisible = false;
            _ordersGettingData = false;
            _inventoryGettingData = false;
            _transactionsGettingData = false;
            RefreshGUI();

            for (int i = 0; i < chkOwners.Items.Count; i++)
            {
                chkOwners.SetItemChecked(i, _selectAllOwners);
            }

            _selectAllOwners = !_selectAllOwners;
            RefreshWalletList();
        }

        private void btnToggleAllItems_Click(object sender, EventArgs e)
        {
            _ordersDataVisible = false;
            _inventoryDataVisible = false;
            _transactionsDataVisible = false;
            _ordersGettingData = false;
            _inventoryGettingData = false;
            _transactionsGettingData = false;
            RefreshGUI();
            
            for (int i = 0; i < chkItems.Items.Count; i++)
            {
                chkItems.SetItemChecked(i, _selectAllItems);
            }

            _selectAllItems = !_selectAllItems;
        }


        private void btnRetrieve_Click(object sender, EventArgs e)
        {
            bool gettingData = _inventoryGettingData || _transactionsGettingData || _ordersGettingData;
            if (!gettingData)
            {
                StartUpdate();
            }
            else
            {
                StopUpdate(true);
            }
        }


        private void StartUpdate()
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                _ordersDataVisible = false;
                _inventoryDataVisible = false;
                _transactionsDataVisible = false;
                _ordersGettingData = true;
                _inventoryGettingData = true;
                _transactionsGettingData = true;
                RefreshGUI();

                _inventory = new AssetList();
                _transactions = new TransactionList();
                inventoryView.DataSource = _inventory;
                transactionsView.DataSource = _transactions;

                // Generate finance access parameters
                _finParams = new List<FinanceAccessParams>();
                Dictionary<int, List<int>>.Enumerator enumerator = _useDataFrom.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    // If we are accessing all wallets for a corp then no need to bether with this 
                    // as the default is to access everything when a blank list is supplied.
                    List<short> walletIDs = new List<short>();
                    if (enumerator.Current.Value.Count < 7)
                    {
                        foreach (short walletId in enumerator.Current.Value)
                        {
                            walletIDs.Add(walletId);
                        }
                    }

                    _finParams.Add(new FinanceAccessParams(enumerator.Current.Key, walletIDs));
                }

                // Generate asset access parameters
                _assetParams = new List<AssetAccessParams>();
                foreach (object item in chkOwners.CheckedItems)
                {
                    bool done = false;
                    CharCorpOption owner = (CharCorpOption)item;

                    foreach (AssetAccessParams character in _assetParams)
                    {
                        if (character.CharID == owner.CharacterObj.CharID)
                        {
                            if (owner.Corp) { character.IncludeCorporate = true; }
                            else { character.IncludePersonal = true; }
                            done = true;
                        }
                    }

                    if (!done)
                    {
                        _assetParams.Add(new AssetAccessParams(owner.CharacterObj.CharID, !owner.Corp, owner.Corp));
                    }
                }

                _itemIDs = new List<int>();
                foreach (object item in chkItems.CheckedItems)
                {
                    _itemIDs.Add(((ItemInfo)item).ID);
                }

                _generatePlaceholderOrders = chkGenPlaceholders.Checked;

                // Kick off the threads that will do the updating of the data tables.
                _ordersUpdateThread = new Thread(new ThreadStart(UpdateOrders));
                _ordersUpdateThread.Start();
                _transactionsUpdateThread = new Thread(new ThreadStart(UpdateTransactions));
                _transactionsUpdateThread.Start();
                _inventoryUpdateThread = new Thread(new ThreadStart(UpdateInventory));
                _inventoryUpdateThread.Start();
            }
            catch (Exception ex)
            {
                new EMMAException(ExceptionSeverity.Error, "Error when starting data update in item detail", ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void StopUpdate(bool refreshGUI)
        {
            if (_ordersUpdateThread != null && _ordersUpdateThread.ThreadState == ThreadState.Running)
            {
                _ordersUpdateThread.Abort();
            }
            if (_transactionsUpdateThread != null && _transactionsUpdateThread.ThreadState == ThreadState.Running)
            {
                _transactionsUpdateThread.Abort();
            }
            if (_inventoryUpdateThread != null && _inventoryUpdateThread.ThreadState == ThreadState.Running)
            {
                _inventoryUpdateThread.Abort();
            }

            if (refreshGUI)
            {
                _ordersGettingData = false;
                _transactionsGettingData = false;
                _inventoryGettingData = false;
                RefreshGUI();
            }
        }

        private void UpdateOrders()
        {
            try
            {
                // Retrieve data
                _buyOrders = Orders.LoadOrders(_assetParams, _itemIDs, new List<int>(), (int)OrderState.Active, "buy");
                _sellOrders = Orders.LoadOrders(_assetParams, _itemIDs, new List<int>(), (int)OrderState.Active, "sell");

                // Now add placeholder rows for item/owner combos that do not exist in the retrieved data.
                if (_generatePlaceholderOrders)
                {
                    foreach (object ownerObj in chkOwners.CheckedItems)
                    {
                        foreach (object itemObj in chkItems.CheckedItems)
                        {
                            CharCorpOption owner = (CharCorpOption)ownerObj;
                            int ownerID = owner.Data.ID;
                            int itemID = ((ItemInfo)itemObj).ID;

                            _buyOrders.ItemFilter = "OwnerID = " + ownerID + " AND ItemID = " + itemID;
                            if (_buyOrders.Count == 0)
                            {
                                _buyOrders.Add(new Order(ownerID, itemID, true));
                            }
                            _sellOrders.ItemFilter = "OwnerID = " + ownerID + " AND ItemID = " + itemID;
                            if (_sellOrders.Count == 0)
                            {
                                _sellOrders.Add(new Order(ownerID, itemID, false));
                            }
                        }
                    }
                    _buyOrders.ItemFilter = "";
                    _sellOrders.ItemFilter = "";
                }
                
                _ordersDataVisible = true;
                _ordersGettingData = false;
                RefreshOrders();
                RefreshGUI();
            }
            catch (ThreadAbortException)
            {
                // User has aborted, just drop out.
            }
            catch (Exception ex)
            {
                new EMMAException(ExceptionSeverity.Error, "Error when updating orders data in item detail", ex);
            }
        }

        private void UpdateTransactions()
        {
            try
            {
                Transactions.BuildResults(_finParams, _itemIDs, new List<int>(), new List<int>(),
                    dtpStartDate.Value, dtpEndDate.Value, "any");
                _transactionsDataVisible = true;
                RefreshGUI();

                int pageSize = 20;
                int startRow = 1;
                TransactionList transactions = new TransactionList();

                while (Transactions.GetResultsPage(startRow, pageSize, ref transactions))
                {
                   AddTransactions(transactions);
                    transactions = new TransactionList();
                    startRow += pageSize;
                }
                AddTransactions(transactions);
                _transactionsGettingData = false;
                RefreshGUI();
            }
            catch (ThreadAbortException)
            {
                // User has aborted, just drop out.
            }
            catch (Exception ex)
            {
                new EMMAException(ExceptionSeverity.Error, "Error when updating transaction data in item detail", ex);
            }
        }

        private void UpdateInventory()
        {
            try
            {
                string groupBy = "None";
                if (rdbOwner.Checked) { groupBy = "Owner"; }
                if (rdbRegion.Checked) { groupBy = "Region"; }
                if (rdbSystem.Checked) { groupBy = "System"; }

                Assets.BuildResults(_assetParams, _itemIDs, groupBy);
                _inventoryDataVisible = true;
                RefreshGUI();
                int pageSize = 100;
                int startRow = 1;
                AssetList inventory = new AssetList();

                while (Assets.GetResultsPage(startRow, pageSize, ref inventory))
                {
                    AddInventory(inventory);
                    startRow += pageSize;
                    inventory = new AssetList();
                }
                AddInventory(inventory);
                _inventoryGettingData = false;
                RefreshGUI();
            }
            catch (ThreadAbortException)
            {
                // User has aborted, just drop out.
            }
            catch (Exception ex)
            {
                new EMMAException(ExceptionSeverity.Error, "Error when updating inventory data in item detail", ex);
            }
        }

        private void RefreshGUI()
        {
            if (this.InvokeRequired)
            {
                UpdateCallback callback = new UpdateCallback(RefreshGUI);
                this.Invoke(callback, null);
            }
            else
            {
                lblSellOrders.Visible = _ordersDataVisible;
                sellOrdersView.Visible = _ordersDataVisible;
                lblBuyOrders.Visible = _ordersDataVisible;
                buyOrdersView.Visible = _ordersDataVisible;
                lblBuildingOrdersData.Visible = !_ordersDataVisible;
                lblBuildingOrdersData.Text = _ordersGettingData ? "Building data view..." :
                    "Set the filters and click retrieve to display data";

                //grpTransactionsSummary.Visible = _transactionsDataVisible;
                if (!_transactionsDataVisible)
                {
                    lblTransactionsRetrieved.Text = "0 transactions retrieved";
                }
                transactionsView.Visible = _transactionsDataVisible;
                lblBuildingTransactionsData.Visible = !_transactionsDataVisible;
                lblBuildingTransactionsData.Text = _transactionsGettingData ? "Building data view..." : 
                    "Set the filters and click retrieve to display data";

                //grpInventorySummary.Visible = _inventoryDataVisible;
                inventoryView.Visible = _inventoryDataVisible;
                lblBuildingInventoryData.Visible = !_inventoryDataVisible;
                lblBuildingInventoryData.Text = _inventoryGettingData ? "Building data view..." :
                    "Set the filters and click retrieve to display data";

                //tabControl1.Enabled = _ordersDataVisible || _transactionsDataVisible || _inventoryDataVisible;

                bool gettingData = _inventoryGettingData || _transactionsGettingData || _ordersGettingData;
                chkItems.Enabled = !gettingData;
                chkOwners.Enabled = !gettingData;
                chkWallets.Enabled = !gettingData;
                btnToggleAllItems.Enabled = !gettingData;
                btnToggleAllOwners.Enabled = !gettingData;
                btnRetrieve.Text = gettingData ? "Stop" : "Retrieve";
            }
        }

        private void RefreshOrders()
        {
            if (this.InvokeRequired)
            {
                UpdateCallback callback = new UpdateCallback(RefreshOrders);
                this.Invoke(callback, null);
            }
            else
            {
                buyOrdersView.DataSource = _buyOrders;
                sellOrdersView.DataSource = _sellOrders;

                List<SortInfo> sortData = new List<SortInfo>();
                sortData.Add(new SortInfo(BuyOrderDateTimeColumn.Index, BuyOrderDateTimeColumn.DataPropertyName));
                buyOrdersView.GridSortInfo = sortData;

                List<SortInfo> sortData2 = new List<SortInfo>();
                sortData2.Add(new SortInfo(SellOrderDateColumn.Index, SellOrderDateColumn.DataPropertyName));
                sellOrdersView.GridSortInfo = sortData2;
            }
        }

        private void AddTransactions(TransactionList transactions)
        {
            if (this.InvokeRequired)
            {
                AddTransCallback callback = new AddTransCallback(AddTransactions);
                object[] parameters = new object[1];
                parameters[0] = transactions;
                this.Invoke(callback, parameters);
            }
            else
            {
                foreach (Transaction trans in transactions)
                {
                    _transactions.Add(trans);
                }
                //transactionsView.Refresh();
                lblTransactionsRetrieved.Text = _transactions.Count + " Transactions retrieved";
            }
        }

        private void AddInventory(AssetList inventory)
        {
            if (this.InvokeRequired)
            {
                AddInventoryCallback callback = new AddInventoryCallback(AddInventory);
                object[] parameters = new object[1];
                parameters[0] = inventory;
                this.Invoke(callback, parameters);
            }
            else
            {
                foreach (Asset asset in inventory)
                {
                    _inventory.Add(asset);
                }
                //inventoryView.Refresh();
            }
        }

        private void rdbNone_CheckedChanged(object sender, EventArgs e)
        {
            _inventoryDataVisible = false;
            RefreshGUI();
        }

        private void rdbOwner_CheckedChanged(object sender, EventArgs e)
        {
            _inventoryDataVisible = false;
            RefreshGUI();
        }

        private void rdbRegion_CheckedChanged(object sender, EventArgs e)
        {
            _inventoryDataVisible = false;
            RefreshGUI();
        }

        private void rdbSystem_CheckedChanged(object sender, EventArgs e)
        {
            _inventoryDataVisible = false;
            RefreshGUI();
        }

        private void dtpStartDate_ValueChanged(object sender, EventArgs e)
        {
            _transactionsDataVisible = false;
            RefreshGUI();
        }

        private void dtpEndDate_ValueChanged(object sender, EventArgs e)
        {
            _transactionsDataVisible = false;
            RefreshGUI();
        }

        private void chkGenPlaceholders_CheckedChanged(object sender, EventArgs e)
        {
            _ordersDataVisible = false;
            RefreshGUI();
        }


        void ViewItemDetail_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopUpdate(false);
            if (UserAccount.Settings != null)
            {
                UserAccount.Settings.StoreFormSizeLoc(this);
                UserAccount.Settings.StoreColumnWidths(this.Name, buyOrdersView);
                UserAccount.Settings.StoreColumnWidths(this.Name, sellOrdersView);
                UserAccount.Settings.StoreColumnWidths(this.Name, transactionsView);
                UserAccount.Settings.StoreColumnWidths(this.Name, inventoryView);
            }
        }


        private class ItemInfo
        {
            private int _itemID;
            private string _itemName = "";

            public ItemInfo(int itemID, string itemName)
            {
                _itemID = itemID;
                _itemName = itemName;
            }

            public int ID
            {
                get { return _itemID; }
            }

            public string ItemName
            {
                get { return _itemName; }
            }

            public override string ToString()
            {
                return ItemName;
            }
        }

        private void transactionsView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (transactionsView.Rows[e.RowIndex] != null)
            {
                if (transactionsView.Columns[e.ColumnIndex].Name.Equals("TransactionsPriceColumn")
                    || transactionsView.Columns[e.ColumnIndex].Name.Equals("TransactionsTotalColumn"))
                {
                    TransactionType type = ((Transaction)transactionsView.Rows[e.RowIndex].DataBoundItem).Type;

                    DataGridViewCellStyle style = e.CellStyle;
                    if (type == TransactionType.Both)
                    {
                        style.ForeColor = Color.Blue;
                    }
                    else if (type == TransactionType.Buy)
                    {
                        style.ForeColor = Color.Red;
                    }
                    else if (type == TransactionType.Sell)
                    {
                        style.ForeColor = Color.Green;
                    }

                    e.CellStyle = style;
                }
                if (transactionsView.Columns[e.ColumnIndex].Name.Equals("TransactionsGrossUnitProfit")
                    || transactionsView.Columns[e.ColumnIndex].Name.Equals("TransactionsGrossProfitColumn"))
                {
                    decimal profit = ((Transaction)transactionsView.Rows[e.RowIndex].DataBoundItem).GrossProfit;

                    DataGridViewCellStyle style = e.CellStyle;
                    if (profit > 0)
                    {
                        style.ForeColor = Color.Green;
                    }
                    else if (profit < 0)
                    {
                        style.ForeColor = Color.Red;
                    }

                    e.CellStyle = style;
                }
            }
        }

        private void sellOrdersView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (sellOrdersView.Rows[e.RowIndex] != null)
            {
                bool noOrder = ((Order)sellOrdersView.Rows[e.RowIndex].DataBoundItem).State.Equals("NO ORDER");
                bool movement12hours = ((Order)sellOrdersView.Rows[e.RowIndex].DataBoundItem).TwelveHourMovement > 0;
                bool movement2days = ((Order)sellOrdersView.Rows[e.RowIndex].DataBoundItem).TwoDayMovement > 0;
                bool movement7days = ((Order)sellOrdersView.Rows[e.RowIndex].DataBoundItem).SevenDayMovement > 0;


                DataGridViewCellStyle style = e.CellStyle;
                if (noOrder)
                {
                    style.BackColor = Color.Red;
                }
                else
                {
                    if (!movement7days)
                    {
                        style.BackColor = Color.OrangeRed;
                    }
                    else if (!movement2days)
                    {
                        style.BackColor = Color.Orange;
                    }
                    else if (!movement12hours)
                    {
                        style.BackColor = Color.Yellow;
                    }
                    else
                    {
                        style.BackColor = Color.White;
                    }
                }

                e.CellStyle = style;
            }
        }

        private void buyOrdersView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (buyOrdersView.Rows[e.RowIndex] != null)
            {
                bool noOrder = ((Order)buyOrdersView.Rows[e.RowIndex].DataBoundItem).State.Equals("NO ORDER");
                bool movement12hours = ((Order)buyOrdersView.Rows[e.RowIndex].DataBoundItem).TwelveHourMovement > 0;
                bool movement2days = ((Order)buyOrdersView.Rows[e.RowIndex].DataBoundItem).TwoDayMovement > 0;
                bool movement7days = ((Order)buyOrdersView.Rows[e.RowIndex].DataBoundItem).SevenDayMovement > 0;


                DataGridViewCellStyle style = e.CellStyle;
                if (noOrder)
                {
                    style.BackColor = Color.Red;
                }
                else
                {
                    if (!movement7days)
                    {
                        style.BackColor = Color.OrangeRed;
                    }
                    else if (!movement2days)
                    {
                        style.BackColor = Color.Orange;
                    }
                    else if (!movement12hours)
                    {
                        style.BackColor = Color.Yellow;
                    }
                    else
                    {
                        style.BackColor = Color.White;
                    }
                }

                e.CellStyle = style;
            }
        }

 

    }
}