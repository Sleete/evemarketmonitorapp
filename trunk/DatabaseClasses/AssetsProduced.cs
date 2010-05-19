using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public static class AssetsProduced
    {
        private static EMMADataSetTableAdapters.AssetsProducedTableAdapter _tableAdapter = 
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.AssetsProducedTableAdapter();

        public static void Add(Asset producedAsset)
        {
            long? producedAssetID = 0;
            _tableAdapter.New(producedAsset.OwnerID, producedAsset.CorpAsset, producedAsset.ItemID, DateTime.UtcNow,
                producedAsset.UnitBuyPricePrecalculated ? producedAsset.UnitBuyPrice : 0,
                Math.Abs(producedAsset.Quantity), ref producedAssetID);
        }


    }
}
