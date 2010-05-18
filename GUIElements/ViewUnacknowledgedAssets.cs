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

    public partial class ViewUnacknowledgedAssets : Form
    {
        private AssetList _lostAssets = null;
        private AssetList _gainedAssets = null;
        private List<AssetChangeType> _assetChangeTypes = null;

        public event AssetChangesAcknowledgedHandler AssetChangesAcknowledged; 
        private delegate void Callback();

        public ViewUnacknowledgedAssets()
        {
            InitializeComponent();

            UserAccount.Settings.GetFormSizeLoc(this);
            UserAccount.Settings.GetColumnWidths(this.Name, gainedItemsGrid);
            UserAccount.Settings.GetColumnWidths(this.Name, lostItemsGrid);
            this.FormClosing += new FormClosingEventHandler(ViewUnacknowledgedAssets_FormClosing);

            lblBusy.Hide();
        }

        private void ViewUnacknowledgedAssets_Load(object sender, EventArgs e)
        {
            try
            {
                Thread t0 = new Thread(new ThreadStart(CrossCheckAssetChanges));
                t0.Start();

                _assetChangeTypes = AssetChangeTypes.GetAllChangeTypes();

                gainedItemsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                gainedItemsGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                gainedItemsGrid.AutoGenerateColumns = false;
                GainedOwnerColumn.DataPropertyName = "Owner";
                GainedItemColumn.DataPropertyName = "Item";
                GainedLocationColumn.DataPropertyName = "Location";
                GainedQuantityColumn.DataPropertyName = "Quantity";
                GainedReasonColumn.DataPropertyName = "ChangeTypeID";
                GainedReasonColumn.DataSource = _assetChangeTypes;
                GainedReasonColumn.ValueMember = "ID";
                GainedReasonColumn.DisplayMember = "Description";

                lostItemsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                lostItemsGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                lostItemsGrid.AutoGenerateColumns = false;
                LostOwnerColumn.DataPropertyName = "Owner";
                LostItemColumn.DataPropertyName = "Item";
                LostLocationColumn.DataPropertyName = "Location";
                LostQuantityColumn.DataPropertyName = "Quantity";
                LostReasonColumn.DataPropertyName = "ChangeTypeID";
                LostReasonColumn.DataSource = _assetChangeTypes;
                LostReasonColumn.ValueMember = "ID";
                LostReasonColumn.DisplayMember = "Description";
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
            lblBusy.Hide();

            lostItemsGrid.DataSource = _lostAssets;
            gainedItemsGrid.DataSource = _gainedAssets;
            lostItemsGrid.Enabled = true;
            gainedItemsGrid.Enabled = true;
        }
        private void HideData()
        {
            lblBusy.Font = new Font(lblBusy.Font.FontFamily, 20, FontStyle.Bold);
            lblBusy.Dock = DockStyle.Fill;
            lblBusy.BackColor = Color.Transparent;
            lblBusy.Text = "Busy";
            lblBusy.Show();

            lostItemsGrid.Enabled = false;
            gainedItemsGrid.Enabled = false;
            lostItemsGrid.DataSource = null;
            gainedItemsGrid.DataSource = null;
        }

        void ViewUnacknowledgedAssets_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (UserAccount.Settings != null)
            {
                UserAccount.Settings.StoreFormSizeLoc(this);
                UserAccount.Settings.StoreColumnWidths(this.Name, gainedItemsGrid);
                UserAccount.Settings.StoreColumnWidths(this.Name, lostItemsGrid);
            }
        }


        public void CrossCheckAssetChanges()
        {
            this.Invoke(new Callback(HideData));

            try
            {
                if (_lostAssets == null) { _lostAssets = new AssetList(); }
                if (_gainedAssets == null) { _gainedAssets = new AssetList(); }

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

                EMMADataSet.AssetsDataTable assetChanges = new EMMADataSet.AssetsDataTable();

                List<int> gIndiciesToRemove = new List<int>();
                for (int i = 0; i < _gainedAssets.Count; i++)
                {
                    Asset gainedAsset = _gainedAssets[i];
                    decimal totalCost = 0;
                    long qRemaining = gainedAsset.Quantity;
                    _lostAssets.ItemFilter = "ItemID = " + gainedAsset.ItemID;
                    List<int> indiciesToRemove = new List<int>();
                    for (int j = 0; j < _lostAssets.FiltredItems.Count; j++)
                    {
                        Asset lostAsset = (Asset)_lostAssets.FiltredItems[j];

                        if (lostAsset.Quantity < 0 && gainedAsset.Quantity > 0)
                        {
                            long deltaQ = Math.Min(qRemaining, Math.Abs(lostAsset.Quantity));
                            totalCost += lostAsset.UnitBuyPrice * deltaQ;
                            qRemaining -= deltaQ;

                            lostAsset.Quantity += deltaQ;
                        }
                        if (lostAsset.Quantity == 0) { indiciesToRemove.Add(j); }
                    }
                    foreach (int index in indiciesToRemove)
                    {
                        _lostAssets.FiltredItems.RemoveAt(index);
                    }

                    if (qRemaining < gainedAsset.Quantity)
                    {
                        long assetID = 0;
                        Assets.AssetExists(assetChanges, gainedAsset.OwnerID, gainedAsset.CorpAsset,
                            gainedAsset.LocationID, gainedAsset.ItemID, gainedAsset.StatusID,
                            gainedAsset.ContainerID != 0, gainedAsset.ContainerID, gainedAsset.IsContainer,
                            false, true, gainedAsset.AutoConExclude, ref assetID);
                        EMMADataSet.AssetsRow assetRow = assetChanges.FindByID(assetID);
                        long totalQ = gainedAsset.Quantity - qRemaining;
                        if (assetRow.Quantity > gainedAsset.Quantity && assetRow.CostCalc)
                        {
                            totalCost += assetRow.Cost * assetRow.Quantity;
                            totalQ += assetRow.Quantity;
                        }
                        assetRow.Cost = totalCost / totalQ;
                        assetRow.CostCalc = true;
                    }

                    gainedAsset.Quantity = qRemaining;
                    if (gainedAsset.Quantity == 0)
                    {
                        gIndiciesToRemove.Add(i);
                    }
                }

                foreach (int index in gIndiciesToRemove)
                {
                    _gainedAssets.RemoveAt(index);
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


            this.Invoke(new Callback(ShowData));
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (AssetChangesAcknowledged != null)
            {
                AssetChangesAcknowledged(this, null);
            }
        }


    }
}
