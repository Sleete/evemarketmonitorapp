using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlTypes;

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

        public static EMMADataSet.AssetsProducedDataTable GetAssetsProduced(int ownerID, DateTime startDate, DateTime endDate)
        {
            EMMADataSet.AssetsProducedDataTable retVal = new EMMADataSet.AssetsProducedDataTable();

            if (startDate.CompareTo(SqlDateTime.MinValue.Value) < 0) startDate = SqlDateTime.MinValue.Value;
            if (endDate.CompareTo(SqlDateTime.MinValue.Value) < 0) endDate = SqlDateTime.MinValue.Value;
            if (startDate.CompareTo(SqlDateTime.MaxValue.Value) > 0) startDate = SqlDateTime.MaxValue.Value;
            if (endDate.CompareTo(SqlDateTime.MaxValue.Value) > 0) endDate = SqlDateTime.MaxValue.Value;

            lock (_tableAdapter)
            {
                _tableAdapter.FillByDate(retVal, ownerID, startDate, endDate);
            }

            return retVal;
        }

    }
}
