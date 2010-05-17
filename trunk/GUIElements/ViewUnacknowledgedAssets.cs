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


namespace EveMarketMonitorApp.GUIElements
{
    public delegate void AssetChangesAcknowledgedHandler(object myObject, EventArgs args);

    public partial class ViewUnacknowledgedAssets : Form
    {
        private AssetList _lostAssets = null;
        private AssetList _gainedAssets = null;

        public event AssetChangesAcknowledgedHandler AssetChangesAcknowledged; 

        public ViewUnacknowledgedAssets()
        {
            InitializeComponent();
        }

        private void ViewUnacknowledgedAssets_Load(object sender, EventArgs e)
        {
            Thread t0 = new Thread(new ThreadStart(CrossCheckAssetChanges));
            t0.Start();
        }

        public void CrossCheckAssetChanges()
        {
            if (_lostAssets == null) { _lostAssets = new AssetList(); }
            if (_gainedAssets == null) { _gainedAssets = new AssetList(); }

            foreach (EVEAccount account in UserAccount.CurrentGroup.Accounts)
            {
                foreach(APICharacter character in account.Chars) 
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

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (AssetChangesAcknowledged != null)
            {
                AssetChangesAcknowledged(this, null);
            }
        }

    }
}
