using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.AbstractionClasses;
using EveMarketMonitorApp.Common;


namespace EveMarketMonitorApp.GUIElements
{
    public delegate void AssetChangesAcknowledgedHandler(object myObject, EventArgs args);



    public partial class ViewUnacknowledgedAssets : Form, IProvideStatus
    {
        private AssetList _lostAssets = null;
        private AssetList _gainedAssets = null;
        private List<AssetChangeType> _assetGainChangeTypes = null;
        private List<AssetChangeType> _assetLossChangeTypes = null;

        private Dictionary<TempAssetKey, List<TempAssetKey>> _assetsUsedForManufacture =
            new Dictionary<TempAssetKey, List<TempAssetKey>>();

        public event AssetChangesAcknowledgedHandler AssetChangesAcknowledged; 
        private delegate void Callback();

        private static StatusChangeArgs _status;

        private static int _errorCount = 0;

        #region IProvideStatus Members
        public event StatusChangeHandler StatusChange;
        #endregion


        public ViewUnacknowledgedAssets()
        {
            InitializeComponent();

            UserAccount.Settings.GetFormSizeLoc(this);
            UserAccount.Settings.GetColumnWidths(this.Name, gainedItemsGrid);
            UserAccount.Settings.GetColumnWidths(this.Name, lostItemsGrid);
            this.FormClosing += new FormClosingEventHandler(ViewUnacknowledgedAssets_FormClosing);

            lblBusy.Hide();

            label1.Text = "This screen shows items that have been added or removed since the last asset " +
                "API update. Usually, these will have been destroyed or used up in the case of lst items and " +
                "found or built in the case of added items.\r\n";
            if (UserAccount.Settings.ManufacturingMode)
            {
                label1.Text = label1.Text +
                    "Select what has happened to each item and EMMA will be able to show you more accurate " +
                    "reports in future. However, if you just wish to ignore this screen then you can click the " +
                    "ok button right away. EMMA will try and match gained items against missing items elsewhere.";
            }
            else
            {
                label1.Text = label1.Text + 
                    "Select what has happened to each item and EMMA will be able to show you more accurate " +
                    "reports in future. However, if you just wish to ignore this screen then you can click the " +
                    "ok button right away. By default, EMMA assumes that any added items have been found " +
                    "(e.g. mission loot, mining, etc) and any lost items have been destroyed or used up.";
            }
        }

        private void ViewUnacknowledgedAssets_Load(object sender, EventArgs e)
        {
            try
            {
                _assetGainChangeTypes = AssetChangeTypes.GetAllChangeTypes(AssetChangeTypes.ChangeMetaType.Gain);
                _assetLossChangeTypes = AssetChangeTypes.GetAllChangeTypes(AssetChangeTypes.ChangeMetaType.Loss);

                gainedItemsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                gainedItemsGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                gainedItemsGrid.AutoGenerateColumns = false;
                GainedOwnerColumn.DataPropertyName = "Owner";
                GainedItemColumn.DataPropertyName = "Item";
                GainedLocationColumn.DataPropertyName = "Location";
                GainedQuantityColumn.DataPropertyName = "Quantity";
                GainedReasonColumn.DataPropertyName = "ChangeTypeIntID";
                GainedReasonColumn.DataSource = _assetGainChangeTypes;
                GainedReasonColumn.ValueMember = "ID";
                GainedReasonColumn.DisplayMember = "Description";
                gainedItemsGrid.CellEndEdit += new DataGridViewCellEventHandler(gainedItemsGrid_CellEndEdit);
                gainedItemsGrid.CellBeginEdit += new DataGridViewCellCancelEventHandler(itemsGrid_CellBeginEdit);
                gainedItemsGrid.RowEnter += new DataGridViewCellEventHandler(gainedItemsGrid_RowEnter);
                gainedItemsGrid.RowLeave += new DataGridViewCellEventHandler(gainedItemsGrid_RowLeave);

                lostItemsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                lostItemsGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                lostItemsGrid.AutoGenerateColumns = false;
                LostOwnerColumn.DataPropertyName = "Owner";
                LostItemColumn.DataPropertyName = "Item";
                LostLocationColumn.DataPropertyName = "Location";
                LostQuantityColumn.DataPropertyName = "Quantity";
                LostReasonColumn.DataPropertyName = "ChangeTypeIntID";
                LostReasonColumn.DataSource = _assetLossChangeTypes;
                LostReasonColumn.ValueMember = "ID";
                LostReasonColumn.DisplayMember = "Description";
                lostItemsGrid.CellBeginEdit += new DataGridViewCellCancelEventHandler(itemsGrid_CellBeginEdit);

                PopulateLists();
            }
            catch (Exception ex)
            {
                EMMAException emmaEx = ex as EMMAException;
                if (emmaEx == null)
                {
                    new EMMAException(ExceptionSeverity.Error, "Problem loading unacknowledged assets view", ex);
                }
                MessageBox.Show("Problem loading unacknowledged assets view:\r\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowData()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Callback(ShowData));
            }
            else
            {
                btnOk.Enabled = true;

                lblBusy.Hide();
                busyProgress.Hide();

                lostItemsGrid.DataSource = _lostAssets;
                gainedItemsGrid.DataSource = _gainedAssets;
                lostItemsGrid.Visible = true;
                gainedItemsGrid.Visible = true;
            }
        }
        private void FilterData()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Callback(FilterData));
            }
            else
            {
                if (grpMaterials.Enabled)
                {
                    this.Cursor = Cursors.WaitCursor;
                    try
                    {
                        AssetList tmpAssets = new AssetList();
                        foreach (TempAssetKey material in lstMaterials.Items)
                        {
                            _lostAssets.ItemFilter = "ItemID = " + material.ItemID;
                            foreach (Asset a in _lostAssets.FiltredItems)
                            {
                                tmpAssets.Add(a);
                            }
                        }
                        lostItemsGrid.DataSource = tmpAssets;
                        _lostAssets.ItemFilter = "";
                    }
                    finally
                    {
                        this.Cursor = Cursors.Default;
                    }
                }
                else
                {
                    lostItemsGrid.DataSource = _lostAssets;
                }
            }
        }
        private void HideData()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Callback(HideData));
            }
            else
            {
                btnOk.Enabled = false;

                lblBusy.BringToFront();
                lblBusy.Font = new Font(lblBusy.Font.FontFamily, 20, FontStyle.Bold);
                lblBusy.AutoSize = false;
                lblBusy.Dock = DockStyle.Fill;
                lblBusy.BackColor = Color.Transparent;
                lblBusy.TextAlign = ContentAlignment.MiddleCenter;
                lblBusy.Text = "Busy";
                lblBusy.Show();

                busyProgress.BringToFront();
                busyProgress.Value = 0;
                busyProgress.Maximum = 1;
                busyProgress.Show();

                lostItemsGrid.Visible = false;
                gainedItemsGrid.Visible = false;
                lostItemsGrid.DataSource = null;
                gainedItemsGrid.DataSource = null;
                // For some reason, the lost items data source gets reset when the gained 
                // items data source is set to null... Wierd but just null it again and
                // it'll be fine.
                lostItemsGrid.DataSource = null;
            }
        }

        void ViewUnacknowledgedAssets_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!btnOk.Enabled)
            {
                e.Cancel = true;
            }
            else
            {
                // We've finished processing so let EMMA know that the user has ackowledged 
                // all outstanding assets.
                if (AssetChangesAcknowledged != null)
                {
                    AssetChangesAcknowledged(this, null);
                }

                if (UserAccount.Settings != null)
                {
                    UserAccount.Settings.StoreFormSizeLoc(this);
                    UserAccount.Settings.StoreColumnWidths(this.Name, gainedItemsGrid);
                    UserAccount.Settings.StoreColumnWidths(this.Name, lostItemsGrid);
                }
            }
        }

        public void PopulateLists()
        {
            if (!this.InvokeRequired)
            {
                this.StatusChange += new StatusChangeHandler(ViewUnacknowledgedAssets_StatusChange);

                Thread t0 = new Thread(new ThreadStart(PopulateLists));
                t0.Start();
            }
            else
            {
                HideData();

                if (_lostAssets == null) { _lostAssets = new AssetList(); }
                if (_gainedAssets == null) { _gainedAssets = new AssetList(); }

                // Build complete lists of unacknowledged gained/lost assets for all 
                // characters and corps within the report group.
                foreach (EVEAccount account in UserAccount.CurrentGroup.Accounts)
                {
                    foreach (APICharacter character in account.Chars)
                    {
                        if (character.UnacknowledgedGains != null && character.UnacknowledgedGains.Count > 0)
                        {
                            _gainedAssets.Add(character.UnacknowledgedGains);
                            character.UnacknowledgedGains = null;
                        }
                        if (character.CorpUnacknowledgedGains != null && character.CorpUnacknowledgedGains.Count > 0)
                        {
                            _gainedAssets.Add(character.CorpUnacknowledgedGains);
                            character.CorpUnacknowledgedGains = null;
                        }
                        if (character.UnacknowledgedLosses != null && character.UnacknowledgedLosses.Count > 0)
                        {
                            _lostAssets.Add(character.UnacknowledgedLosses);
                            character.UnacknowledgedLosses = null;
                        }
                        if (character.CorpUnacknowledgedLosses != null && character.CorpUnacknowledgedLosses.Count > 0)
                        {
                            _lostAssets.Add(character.CorpUnacknowledgedLosses);
                            character.CorpUnacknowledgedLosses = null;
                        }
                    }
                }

                // Only cross check if we are not in manufacturing mode
                if (!UserAccount.Settings.ManufacturingMode)
                {
                    CrossCheckAssetChanges(false);
                }

                ShowData();

                this.StatusChange -= ViewUnacknowledgedAssets_StatusChange;
            }
        }

        private void CrossCheckAssetChanges()
        {
            CrossCheckAssetChanges(true);
        }
        private void CrossCheckAssetChanges(bool showAndHideData)
        {
            // Only allow one of these checks to be running at once. If a later check 
            // attempts to start then it must wait for the current one to finish and then run.
            lock (_lostAssets)
            {
                if (showAndHideData) { HideData(); }

                try
                {
                    EMMADataSet.AssetsDataTable assetChanges = new EMMADataSet.AssetsDataTable();

                    // Try and match one character/corp gains to anothers losses.
                    List<Asset> gAssetsToRemove = new List<Asset>();
                    for (int i = 0; i < _gainedAssets.Count; i++)
                    {
                        UpdateStatus(i, _gainedAssets.Count, "", "", false);

                        Asset gainedAsset = _gainedAssets[i];
                        decimal totalCost = 0;
                        long qRemaining = gainedAsset.Quantity;
                        _lostAssets.ItemFilter = "ItemID = " + gainedAsset.ItemID;
                        List<Asset> assetsToRemove = new List<Asset>();
                        for (int j = 0; j < _lostAssets.FiltredItems.Count; j++)
                        {
                            Asset lostAsset = (Asset)_lostAssets.FiltredItems[j];

                            // If a match is found then use the cost of the lost items to calculate
                            // the cost of the gained items.
                            if (lostAsset.Quantity < 0 && gainedAsset.Quantity > 0)
                            {
                                long deltaQ = Math.Min(qRemaining, Math.Abs(lostAsset.Quantity));
                                totalCost += lostAsset.UnitBuyPrice * deltaQ;
                                qRemaining -= deltaQ;

                                lostAsset.Quantity += deltaQ;
                            }
                            if (lostAsset.Quantity == 0) { assetsToRemove.Add(lostAsset); }
                        }
                        // Remove any lost items that have been accounted for.
                        foreach (Asset a in assetsToRemove)
                        {
                            _lostAssets.FiltredItems.Remove(a);
                        }

                        // If we found some lost items to match against this gained item then
                        // retrieve the gained asset row from the database and recalculate
                        // the cost of the stack.
                        if (qRemaining < gainedAsset.Quantity)
                        {
                            long assetID = 0;
                            Assets.AssetExists(assetChanges, gainedAsset.OwnerID, gainedAsset.CorpAsset,
                                gainedAsset.LocationID, gainedAsset.ItemID, gainedAsset.StatusID,
                                gainedAsset.ContainerID != 0, gainedAsset.ContainerID, gainedAsset.IsContainer,
                                false, true, gainedAsset.AutoConExclude, false, gainedAsset.EveItemInstanceID,
                                ref assetID);
                            EMMADataSet.AssetsRow assetRow = assetChanges.FindByID(assetID);

                            // Note if the asset row is null then it is not in the database.
                            // i.e. we previously had a negative quantity of items here.
                            // In this case, we can try and match the assets with a transaction 
                            // market with the 'calcProfitFromAssets' flag.
                            if (assetRow != null)
                            {
                                long totalQ = gainedAsset.Quantity - qRemaining;
                                if (assetRow.Quantity > gainedAsset.Quantity && assetRow.CostCalc)
                                {
                                    // If the asset stack contains more items than our 'gained' record
                                    // and it alreay has a calculated cost then take the stack's current
                                    // cost into account as well.
                                    totalCost += assetRow.Cost * (assetRow.Quantity - gainedAsset.Quantity);
                                    totalQ += assetRow.Quantity - gainedAsset.Quantity;
                                }
                                assetRow.Cost = totalCost / totalQ;
                                assetRow.CostCalc = true;
                            }
                            else
                            {
                            }
                        }

                        gainedAsset.Quantity = qRemaining;
                        if (gainedAsset.Quantity == 0)
                        {
                            gAssetsToRemove.Add(gainedAsset);
                        }
                    }

                    // Remove any gained items that have been accounted for.
                    foreach (Asset a in gAssetsToRemove)
                    {
                        _gainedAssets.Remove(a);
                    }

                    Assets.UpdateDatabase(assetChanges);

                    _lostAssets.ItemFilter = "";
                    _gainedAssets.ItemFilter = "";
                }
                catch (Exception ex)
                {
                    EMMAException emmaEx = ex as EMMAException;
                    if (emmaEx == null)
                    {
                        new EMMAException(ExceptionSeverity.Error, "Problem cross checking asset changes in " +
                            "unacknowledged assets view", ex);
                    }
                    MessageBox.Show("Problem cross checking asset changes in unacknowledged assets view:\r\n" +
                        ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (showAndHideData) { ShowData(); }
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.StatusChange += new StatusChangeHandler(ViewUnacknowledgedAssets_StatusChange);

            // Clear the data grids.
            // If we don't do this then trying to modify the gained/lost assets collections
            // will cause errors (because the changes are being made on a different thread) 
            gainedItemsGrid.DataSource = new AssetList();
            lostItemsGrid.DataSource = new AssetList();

            this.Cursor = Cursors.WaitCursor;

            Thread t0 = new Thread(new ThreadStart(ProcessAssetsAsMarked));
            t0.Start();
        }

        private void ProcessAssetsAsMarked()
        {
            try
            {
                _errorCount = 0;

                EMMADataSet.AssetsDataTable assetChanges = new EMMADataSet.AssetsDataTable();
                List<Asset> assetsToRemove = new List<Asset>();
                foreach (Asset gainedAsset in _gainedAssets)
                {
                    long assetID = 0;
                    Assets.AssetExists(assetChanges, gainedAsset.OwnerID, gainedAsset.CorpAsset,
                        gainedAsset.LocationID, gainedAsset.ItemID, gainedAsset.StatusID,
                        gainedAsset.ContainerID != 0, gainedAsset.ContainerID, gainedAsset.IsContainer,
                        false, true, gainedAsset.AutoConExclude, false, gainedAsset.EveItemInstanceID, ref assetID);
                    EMMADataSet.AssetsRow assetRow = assetChanges.FindByID(assetID);

                    if (assetRow != null)
                    {
                        switch (gainedAsset.ChangeTypeID)
                        {
                            case AssetChangeTypes.ChangeType.Found:
                                assetRow.Cost = 0;
                                assetRow.CostCalc = true;
                                AssetsProduced.Add(gainedAsset);
                                assetsToRemove.Add(gainedAsset);
                                break;
                            //case AssetChangeTypes.ChangeType.Made:
                            //    assetRow.Cost = gainedAsset.UnitBuyPricePrecalculated ? gainedAsset.UnitBuyPrice : 0;
                            //    assetRow.CostCalc = gainedAsset.UnitBuyPricePrecalculated;
                            //    AssetsProduced.Add(gainedAsset);
                            //    assetsToRemove.Add(gainedAsset);
                            //    break;
                            case AssetChangeTypes.ChangeType.WasNeverMissing:
                                assetRow.Cost = 0;
                                assetRow.CostCalc = false;
                                assetsToRemove.Add(gainedAsset);
                                break;
                            case AssetChangeTypes.ChangeType.BoughtViaContract:
                                assetRow.Cost = 0;
                                assetRow.CostCalc = false;
                                assetRow.BoughtViaContract = true;
                                assetsToRemove.Add(gainedAsset);
                                break;
                            case AssetChangeTypes.ChangeType.CancelledContract:
                                List<AssetAccessParams> access = new List<AssetAccessParams>();
                                bool corporate = false;
                                access.Add(new AssetAccessParams(assetRow.OwnerID, !assetRow.CorpAsset, assetRow.CorpAsset));
                                AssetList assets = Assets.LoadAssets(access, new List<int>(), assetRow.ItemID, 0, 0, 
                                    false, (int)AssetStatus.States.ForSaleViaContract, true);
                                if (assets.Count > 0)
                                {
                                    List<long> matchedAssetsForSale = new List<long>(); 
                                    long qToFind = assetRow.Quantity;
                                    decimal totalCost = 0;
                                    long costq = 0;
                                    bool costPreCalc = false;
                                    foreach (Asset a in assets)
                                    {
                                        if (qToFind > 0)
                                        {
                                            long deltaQ = Math.Min(a.Quantity, qToFind);
                                            qToFind -= deltaQ;
                                            totalCost += a.UnitBuyPricePrecalculated ? a.TotalBuyPrice : 0;
                                            costq += a.UnitBuyPricePrecalculated ? deltaQ : 0;
                                            if (a.UnitBuyPricePrecalculated) { costPreCalc = true; }
                                            matchedAssetsForSale.Add(a.ID);
                                        }
                                    }

                                    assetRow.Cost = (costq > 0 ? totalCost / costq : 0);
                                    assetRow.CostCalc = costPreCalc;
                                    // We've set the cost of the asset, now to remove the old one(s).
                                    foreach (long matchedAssetID in matchedAssetsForSale)
                                    {
                                        EMMADataSet.AssetsRow assetData = Assets.GetAssetDetail(matchedAssetID);
                                        assetData.Delete();
                                        Assets.UpdateDatabase(assetData);
                                    }
                                }
                                else
                                {
                                    // Can't find old asset so don't know cost.
                                    assetRow.Cost = 0;
                                    assetRow.CostCalc = false;
                                }
                                break;
                            case AssetChangeTypes.ChangeType.Unknown:
                                // Can only be 'unknown' if we're in manufacturing mode.
                                // let the cross check sort it out...
                                break;
                            default:
                                throw new EMMAException(ExceptionSeverity.Error, "Unexpected gained asset change type: '" +
                                    gainedAsset.ChangeType + "' ", true);
                                break;
                        }
                    }
                    else
                    {
                        // This indicates that the asset row was removed due to being marked as unprocessed.
                        // i.e. the gain in items was from -x to 0.
                        // We can happily ignore this.
                        //new EMMAException(ExceptionSeverity.Warning, "Could not find gained asset to update\r\n" +
                        //    "\tOwner: " + gainedAsset.OwnerID + "\r\n\tCorpAsset: " + gainedAsset.CorpAsset +
                        //    "\r\n\tLocation: " + gainedAsset.LocationID + "\r\n\tItem : " + gainedAsset.ItemID +
                        //    "\r\n\tStatus: " + gainedAsset.StatusID + "\r\n\tContainerID: " +
                        //    gainedAsset.ContainerID + "\r\n\tIsContainer: " + gainedAsset.IsContainer.ToString() +
                        //    "\r\n\tAutoConExclude: " + gainedAsset.AutoConExclude.ToString() +
                        //    "\r\n\tEveItemInstanceID: " + gainedAsset.EveItemInstanceID, true);
                        //_errorCount++;
                    }
                }
                foreach (Asset a in assetsToRemove)
                {
                    _gainedAssets.Remove(a);
                }

                assetsToRemove = new List<Asset>();
                foreach (Asset lostAsset in _lostAssets)
                {
                    switch (lostAsset.ChangeTypeID)
                    {
                        case AssetChangeTypes.ChangeType.ForSaleViaContract:
                            EMMADataSet.AssetsRow assetRow = assetChanges.NewAssetsRow();
                            assetRow.AutoConExclude = true;
                            assetRow.ContainerID = lostAsset.ContainerID;
                            assetRow.CorpAsset = lostAsset.CorpAsset;
                            assetRow.Cost = lostAsset.UnitBuyPricePrecalculated ? lostAsset.UnitBuyPrice : 0;
                            assetRow.CostCalc = lostAsset.UnitBuyPricePrecalculated;
                            assetRow.IsContainer = lostAsset.IsContainer;
                            assetRow.ItemID = lostAsset.ItemID;
                            assetRow.EveItemID = 0;
                            assetRow.LocationID = lostAsset.LocationID;
                            assetRow.OwnerID = lostAsset.OwnerID;
                            assetRow.Processed = false;
                            assetRow.Quantity = lostAsset.Quantity * -1;
                            assetRow.RegionID = lostAsset.RegionID;
                            assetRow.ReprocExclude = true;
                            assetRow.Status = (int)AssetStatus.States.ForSaleViaContract;
                            assetRow.SystemID = lostAsset.SystemID;
                            assetRow.BoughtViaContract = false;
                            assetChanges.AddAssetsRow(assetRow);
                            assetsToRemove.Add(lostAsset);
                            break;
                        case AssetChangeTypes.ChangeType.DestroyedOrUsed:
                            AssetsLost.Add(lostAsset);
                            assetsToRemove.Add(lostAsset);
                            break;
                        case AssetChangeTypes.ChangeType.Unknown:
                            // Can only be 'unknown' if we're in manufacturing mode.
                            // let the cross check sort it out...
                            break;
                        default:
                            throw new EMMAException(ExceptionSeverity.Error, "Unexpected lost asset change type: '" +
                                lostAsset.ChangeType + "' ", true);
                            break;
                    }
                }

                foreach (Asset a in assetsToRemove)
                {
                    _lostAssets.Remove(a);
                }

                if (UserAccount.Settings.ManufacturingMode)
                {
                    CrossCheckAssetChanges();
                }
            }
            catch (Exception ex)
            {
                EMMAException emmaEx = ex as EMMAException;
                if (emmaEx == null)
                {
                    new EMMAException(ExceptionSeverity.Error, "Problem processing asset changes " +
                        "unacknowledged assets view", ex);
                }
                MessageBox.Show("Problem processing asset changes in unacknowledged assets view:\r\n" +
                    ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            UpdateStatus(0, 0, "FinalTasks", "", true);   
        }

        void itemsGrid_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.ColumnIndex < gainedItemsGrid.ColumnCount &&
                e.RowIndex >= 0 && e.RowIndex < gainedItemsGrid.RowCount)
            {
                if (gainedItemsGrid.Columns[e.ColumnIndex].Equals(GainedReasonColumn))
                {
                    Asset asset = (Asset)gainedItemsGrid.Rows[e.RowIndex].DataBoundItem;
                    e.Cancel = asset.ChangeTypeLocked;
                }
                else { e.Cancel = true; }
            }
        }

        void gainedItemsGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // If the user has set a gained item to 'Made' then try and find the
            // items that were used in it's construction and calculate it's cost
            // accordingly.
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0)
            {
                //if (gainedItemsGrid.Columns[e.ColumnIndex].Equals(GainedReasonColumn))
                //{
                //    this.Cursor = Cursors.WaitCursor;

                //    try
                //    {
                //        //Asset producedAsset = (Asset)gainedItemsGrid.Rows[e.RowIndex].DataBoundItem;
                //        //TempAssetKey prodAssetKey = new TempAssetKey(producedAsset.ID, producedAsset.OwnerID, 
                //        //    producedAsset.CorpAsset);
                //        //if ((int)gainedItemsGrid[e.ColumnIndex, e.RowIndex].Value ==
                //        //    (int)AssetChangeTypes.ChangeType.Made)
                //        //{
                //            //
                //            // What we want to do here is match manufactured items with
                //            // materials that have been used in thier construction.
                //            //

                //            // What we need:
                //            // perfect blueprint material requirements (http://wiki.eve-id.net/Bill_of_Materials)
                //            // relevant skill levels
                //            // ME - this is tricky. Perhaps this can be inferred?
                            
                //            // Also need to cover T2 POS production chain, Invented T2 BPCs, etc
                //            // ...On hold for now...
                //        //}
                //        //else
                //        //{
                //        //    // If this item was previously set to having been made then clear the
                //        //    // locked flag of any items that were thought to have been used in it's 
                //        //    // construction.
                //        //    if (_assetsUsedForManufacture.ContainsKey(prodAssetKey))
                //        //    {
                //        //        List<TempAssetKey> materials = _assetsUsedForManufacture[prodAssetKey];
                //        //        foreach (TempAssetKey mat in materials)
                //        //        {
                //        //            _lostAssets.ItemFilter = "ID = " + mat.AssetID + " AND OwnerID = " +
                //        //                mat.OwnerID + " AND CorpAsset = " + mat.ForCorp;
                //        //            if (_lostAssets.ItemFilter.Length > 0)
                //        //            {
                //        //                Asset materialAsset = (Asset)_lostAssets.FiltredItems[0];
                //        //                materialAsset.ChangeTypeLocked = false;
                //        //            }
                //        //        }
                //        //        _assetsUsedForManufacture.Remove(prodAssetKey);
                //        //    }
                //        //}
                //    }
                //    finally
                //    {
                //        this.Cursor = Cursors.Default;
                //    }
                //}
            }
        }

        void gainedItemsGrid_RowLeave(object sender, DataGridViewCellEventArgs e)
        {
            grpMaterials.Enabled = false;
            lstMaterials.Items.Clear();
            FilterData();
        }

        void gainedItemsGrid_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > 0)
            {
                //Asset asset = (Asset)gainedItemsGrid.Rows[e.RowIndex].DataBoundItem;
                //if (asset.ChangeTypeID == AssetChangeTypes.ChangeType.Made)
                //{
                //    grpMaterials.Enabled = true;
                //}
            }
        }


        void ViewUnacknowledgedAssets_StatusChange(object myObject, StatusChangeArgs args)
        {
            _status = args;

            if (_status.Done && _status.Section.Equals("FinalTasks"))
            {
                Finish();
            }
            else
            {
                UpdateProgress();
            }
        }

        private void Finish()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Callback(Finish));
            }
            else
            {
                if (_errorCount != 0)
                {
                    MessageBox.Show(_errorCount + " errors occured during processing please see " + 
                        EMMAException.logFile + " for details.", "Warning", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                this.Close();
            }
        }

        private void UpdateProgress()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Callback(UpdateProgress));
            }
            else
            {
                busyProgress.Maximum = _status.MaxProgress;
                busyProgress.Value = _status.CurrentProgress;
            }
        }

        private void UpdateStatus(int progress, int maxprogress, string sectionName, string status, bool complete)
        {
            if (StatusChange != null)
            {
                StatusChange(this, new StatusChangeArgs(progress, maxprogress, sectionName, status, complete));
            }
        }


        private class TempAssetKey
        {
            private long _assetID;
            private int _ownerID;
            private bool _forCorp;

            private string _itemName;
            private int _itemID;
            private long _quantity;

            public TempAssetKey(long assetID, int ownerID, bool forCorp)
            {
                _assetID = assetID;
                _ownerID = ownerID;
                _forCorp = forCorp;
            }

            public long AssetID
            {
                get { return _assetID; }
                set { _assetID = value; }
            }
            public int OwnerID
            {
                get { return _ownerID; }
                set { _ownerID = value; }
            }
            public bool ForCorp
            {
                get { return _forCorp; }
                set { _forCorp = value; }
            }

            public int ItemID
            {
                get { return _itemID; }
                set { _itemID = value; }
            }
            public string ItemName
            {
                get { return _itemName; }
                set { _itemName = value; }
            }
            public long Quantity
            {
                get { return _quantity; }
                set { _quantity = value; }
            }

            public override bool Equals(object obj)
            {
                return this.GetHashCode() == obj.GetHashCode();
            }

            public override int GetHashCode()
            {
                return this.ToString().GetHashCode();
            }

            public override string ToString()
            {
                return _assetID.ToString() + _ownerID.ToString() + _forCorp.ToString();
            }

            public void Draw(Graphics g, Rectangle bounds, Font font)
            {
                string text = Quantity + "\t" + ItemName;
                Color col = Color.Black;
                g.DrawString(text, font, new SolidBrush(col), bounds);
            }
        }

    }



}
