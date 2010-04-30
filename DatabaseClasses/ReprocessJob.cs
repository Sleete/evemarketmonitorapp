using System;
using System.Collections.Generic;
using System.Text;

using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.AbstractionClasses;

namespace EveMarketMonitorApp.DatabaseClasses
{
    class ReprocessJob : SortableObject
    {
        private int _id;
        private DateTime _date;
        private int _stationID;
        private bool _gotStation = false;
        private string _stationName = "";
        private int _reportGroupID;
        private int _ownerID;
        private bool _gotOwner = false;
        private string _ownerName = "";

        private ReprocessItemList _items;
        private bool _gotItems = false;
        private ReprocessResultList _results;
        private bool _gotResults = false;
        private List<int> _resultIDs = new List<int>();

        private bool _gotResultsValue = false;
        private decimal _resultsValue;
        private bool _gotItemsBuyPrice = false;
        private decimal _itemsBuyPrice;

        private Dictionary<int, decimal> _defaultResultPrices = new Dictionary<int, decimal>();

        public ReprocessJob(EMMADataSet.ReprocessJobRow data)
        {
            _id = data.ID;
            _date = data.JobDate;
            _stationID = data.StationID;
            _reportGroupID = data.GroupID;
            _ownerID = data.OwnerID;
        }

        public ReprocessJob(int stationID, int groupID, int ownerID)
        {
            _id = 0;
            _date = DateTime.UtcNow;
            _stationID = stationID;
            _reportGroupID = groupID;
            _ownerID = ownerID;
            _items = new ReprocessItemList();
            _gotItems = true;
            _results = new ReprocessResultList();
            _gotResults = true;
        }

        public void SetDefaultResultPrices(Dictionary<int, decimal> prices)
        {
            _defaultResultPrices = prices;
        }

        public void CompleteJob()
        {
            bool corp = false;
            APICharacter charData = UserAccount.CurrentGroup.GetCharacter(_ownerID, ref corp);

            foreach (ReprocessItem item in _items)
            {
                // Note 'addedItemsCost' parameter is ignored because we're removing assets
                // rather than adding them.
                Assets.ChangeAssets(charData.CharID, corp, _stationID, item.ItemID,
                    0, 1, false, -1 * item.Quantity, 0);
            }
            foreach (ReprocessResult result in _results)
            {
                Assets.ChangeAssets(charData.CharID, corp, _stationID, result.ItemID,
                    0, 1, false, result.Quantity, result.EffectiveBuyPrice);
            }
        }

        public void ReverseJob()
        {
            bool corp = false;
            APICharacter charData = UserAccount.CurrentGroup.GetCharacter(_ownerID, ref corp);

            foreach (ReprocessItem item in SourceItems)
            {
                Assets.ChangeAssets(charData.CharID, corp, _stationID, item.ItemID,
                    0, 1, false, item.Quantity, item.BuyPrice);
            }
            foreach (ReprocessResult result in Results)
            {
                // Note 'addedItemsCost' parameter is ignored because we're removing assets
                // rather than adding them.
                Assets.ChangeAssets(charData.CharID, corp, _stationID, result.ItemID,
                    0, 1, false, -1 * result.Quantity, 0);
            }
        }

        public void UpdateResults()
        {
            foreach (ReprocessResult result in _results)
            {
                if (_defaultResultPrices.ContainsKey(result.ItemID))
                {
                    _defaultResultPrices[result.ItemID] = result.UnitSellPrice;
                }
                else
                {
                    _defaultResultPrices.Add(result.ItemID, result.UnitSellPrice);
                }
            }
            _results = new ReprocessResultList();
            _resultIDs.Clear();
            foreach (ReprocessItem item in _items)
            {
                Dictionary<int, double> results = Items.GetItemMaxReprocessResults(item.ItemID);
                Dictionary<int, double>.Enumerator enumerator = results.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    AddResult(enumerator.Current.Key, 
                        (long)(Math.Round(enumerator.Current.Value * item.Quantity)));
                }
            }

            _gotResultsValue = false;
            SetEffectiveBuyPrices();
        }

        private void SetEffectiveBuyPrices()
        {
            decimal totalVal = TotalResultsValue;
            long totalQuantity = 0;
            if (totalVal == 0 && _results.Count > 0)
            {
                foreach (ReprocessResult result in _results)
                {
                    totalVal += result.EstSellPrice;
                    totalQuantity += result.Quantity;
                }
            }

            if (totalQuantity == 0) { totalQuantity = 1; }
            foreach (ReprocessResult result in _results)
            {
                decimal multiplier = 0;
                if (totalVal == 0)
                {
                    multiplier = result.Quantity / totalQuantity;
                }
                else
                {
                    multiplier = result.EstSellPrice / totalVal;
                }
                result.EffectiveBuyPrice = this.ItemsTotalBuyPrice * multiplier;
            }
        }

        public void AddItem(int itemID, long quantity, decimal totalBuyPrice)
        {
            ReprocessItem itemData = null;
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].ItemID == itemID)
                {
                    itemData = _items[i];
                    i = _items.Count;
                }
            }
            if (itemData == null)
            {
                itemData = new ReprocessItem(_id, itemID, quantity, totalBuyPrice);
                _items.Add(itemData);
            }
            else
            {
                itemData.Quantity += quantity;
                itemData.BuyPrice += totalBuyPrice;
            }
            _gotItemsBuyPrice = false;
        }

        public void RemoveItem(int itemID, long quantity, decimal totalBuyPrice)
        {
            ReprocessItem itemData = null;
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].ItemID == itemID)
                {
                    itemData = _items[i];
                    i = _items.Count;
                }
            }
            if (itemData != null)
            {
                itemData.Quantity -= quantity;
                itemData.BuyPrice -= totalBuyPrice;
                if (itemData.Quantity == 0)
                {
                    _items.Remove(itemData);
                }
            }
            _gotItemsBuyPrice = false;
        }

        public void ClearSourceItems()
        {
            _items = new ReprocessItemList();
            _gotItemsBuyPrice = false;
        }

        public void AddResult(int itemID, long maxQuantity)
        {
            if (_resultIDs.Contains(itemID))
            {
                for (int i = 0; i < _results.Count; i++)
                {
                    if (_results[i].ItemID == itemID)
                    {
                        _results[i].MaxQuantity += maxQuantity;
                        i = _results.Count;
                    }
                }
            }
            else
            {
                ReprocessResult newResult = new ReprocessResult(_id, itemID, maxQuantity);
                if (_defaultResultPrices.ContainsKey(itemID))
                {
                    newResult.UnitSellPrice = _defaultResultPrices[itemID];
                }
                _results.Add(newResult);
                _resultIDs.Add(itemID);
            }
        }

        public ReprocessItemList SourceItems
        {
            get
            {
                if (!_gotItems)
                {
                    _items = ReprocessJobs.GetJobItems(_id);
                }
                return _items;
            }
        }

        public ReprocessResultList Results
        {
            get
            {
                if (!_gotResults)
                {
                    _results = ReprocessJobs.GetJobResults(_id);
                }
                return _results;
            }
        }

        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }

        public DateTime Date
        {
            get { return _date;}
            set { _date = value; }
        }

        public string Station
        {
            get
            {
                if (!_gotStation)
                {
                    try
                    {
                        _stationName = Stations.GetStationName(_stationID);
                    }
                    catch (EMMADataMissingException)
                    {
                        _stationName = "Unknown Station";
                    }
                    _gotStation = true;
                }
                return _stationName;
            }
        }

        public int StationID
        {
            get { return _stationID; }
            set
            {
                _stationID = value;
                _gotStation = false;
                _stationName = "";
            }
        }

        public int ReportGroupID
        {
            get { return _reportGroupID; }
            set { _reportGroupID = value; }
        }

        public string Owner
        {
            get
            {
                if (!_gotOwner)
                {
                    try
                    {
                        _ownerName = Names.GetName(_ownerID);
                    }
                    catch (EMMADataMissingException)
                    {
                        _ownerName = "Unknown Char/Corp";
                    }
                    _gotOwner = true;
                }
                return _ownerName;
            }
        }

        public int OwnerID
        {
            get { return _ownerID; }
            set { _ownerID = value; }
        }


        public decimal TotalResultsValue
        {
            get
            {
                if (!_gotResultsValue)
                {
                    _resultsValue = 0;
                    foreach (ReprocessResult result in _results)
                    {
                        _resultsValue += result.EstSellPrice;
                    }
                    _gotResultsValue = true;
                }
                return _resultsValue;
            }
        }

        public decimal ItemsTotalBuyPrice
        {
            get
            {
                if (!_gotItemsBuyPrice)
                {
                    _itemsBuyPrice = 0;
                    foreach (ReprocessItem item in _items)
                    {
                        _itemsBuyPrice += item.BuyPrice;
                    }
                    _gotItemsBuyPrice = true;
                }
                return _itemsBuyPrice;
            }
        }

        public void ClearTotalResultsValue()
        {
            _gotResultsValue = false;
        }

    }
}
