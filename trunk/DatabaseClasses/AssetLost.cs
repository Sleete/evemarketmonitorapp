using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public class AssetLost : SortableObject
    {
        private int _itemID;
        private long _quantity;
        private DateTime _lossDateTime;
        private int _ownerID;
        private bool _corpAsset;

        public AssetLost(int itemID, long quantity, DateTime lossDateTime, int ownerID,
            bool corpAsset)
        {
            _itemID = itemID;
            _quantity = quantity;
            _lossDateTime = lossDateTime;
            _ownerID = ownerID;
            _corpAsset = corpAsset;
        }


    }
}
