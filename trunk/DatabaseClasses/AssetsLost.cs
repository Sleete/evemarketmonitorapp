using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public static class AssetsLost
    {
        private static EMMADataSetTableAdapters.AssetsLostTableAdapter _tableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.AssetsLostTableAdapter();

        public static void Add(Asset lostAsset)
        {
            long? lostAssetID = 0;
            _tableAdapter.New(lostAsset.OwnerID, lostAsset.CorpAsset, lostAsset.ItemID, DateTime.UtcNow,
                Math.Abs(lostAsset.Quantity), lostAsset.UnitBuyPrice, lostAsset.UnitValue, ref lostAssetID);
        }
    }
}
