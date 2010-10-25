using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlTypes;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public static class AssetsLost
    {
        private static EMMADataSetTableAdapters.AssetsLostTableAdapter _tableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.AssetsLostTableAdapter();

        public static void Add(Asset lostAsset)
        {
            long? lostAssetID = 0;
            lock (_tableAdapter)
            {
                _tableAdapter.New(lostAsset.OwnerID, lostAsset.CorpAsset, lostAsset.ItemID, DateTime.UtcNow,
                    Math.Abs(lostAsset.Quantity), lostAsset.UnitBuyPrice, lostAsset.UnitValue, ref lostAssetID);
            }
        }


        public static EMMADataSet.AssetsLostDataTable GetAssetsLost(long ownerID, DateTime startDate, DateTime endDate)
        {
            EMMADataSet.AssetsLostDataTable retVal = new EMMADataSet.AssetsLostDataTable();

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
