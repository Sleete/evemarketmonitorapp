using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public class AssetProduced : SortableObject
    {
        private int _itemID;
        private long _quantity;
        private DateTime _productionDateTime;
        private int _ownerID;
        private bool _corpAsset;
        private decimal _cost;

        public AssetProduced(int itemID, long quantity, DateTime productionDateTime, int ownerID,
            bool corpAsset, decimal cost)
        {
            _itemID = itemID;
            _quantity = quantity;
            _productionDateTime = productionDateTime;
            _ownerID = ownerID;
            _corpAsset = corpAsset;
            _cost = cost;
        }


    }
}
