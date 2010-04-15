using System;
using System.Collections.Generic;
using System.Text;

using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.DatabaseClasses
{
    class Order : SortableObject
    {
        private int _id;
        private int _ownerID;
        private bool _gotOwner = false;
        private string _owner;
        private int _stationID;
        private bool _gotStation = false;
        private string _station;
        private bool _gotSystem = false;
        private string _system;
        private bool _gotRegion = false;
        private string _region;
        private int _totalVol;
        private int _remainingVol;
        private int _minVol;
        private short _stateID;
        private bool _gotState = false;
        private string _state;
        private int _itemID;
        private bool _gotItem = false;
        private string _item;
        private short _range;
        private short _walletID;
        private short _duration;
        private decimal _escrow;
        private decimal _price;
        private bool _buyOrder;
        private DateTime _date;


        private long _twelveHourMovement;
        private bool _gotTwelveHourMovement = false;
        private long _twoDayMovement;
        private bool _gotTwoDayMovement = false;
        private long _sevenDayMovement;
        private bool _gotSevenDayMovement = false;

        /// <summary>
        /// This contsructor is used by the itemdetail form 
        /// </summary>
        public Order(int ownerID, int itemID, bool buyOrder)
        {
            _ownerID = ownerID;
            _itemID = itemID;
            _buyOrder = buyOrder;

            _stationID = 0;
            _gotStation = true;
            _station = "NO ORDER";
            _gotState = true;
            _state = "NO ORDER";
            _gotSystem = true;
            _system = "NO ORDER";
            _gotRegion = true;
            _region = "NO ORDER";
            _walletID = 0;
            _date = DateTime.Now;
            _price = 0;
            _remainingVol = 0;
            _totalVol = 0;
            _minVol = 0;
            _twelveHourMovement = 0;
            _gotTwelveHourMovement = true;
            _twoDayMovement = 0;
            _gotTwoDayMovement = true;
            _sevenDayMovement = 0;
            _gotSevenDayMovement = true;
        }

        public Order(EMMADataSet.OrdersRow dataRow)
        {
            if (dataRow != null)
            {
                _id = dataRow.ID;
                _ownerID = dataRow.OwnerID;
                if (dataRow.ForCorp)
                {
                    _ownerID = UserAccount.CurrentGroup.GetCharacter(_ownerID).CorpID;
                }
                _stationID = dataRow.StationID;
                _totalVol = dataRow.TotalVol;
                _remainingVol = dataRow.RemainingVol;
                _minVol = dataRow.MinVolume;
                _stateID = dataRow.OrderState;
                _itemID = dataRow.ItemID;
                _range = dataRow.Range;
                _duration = dataRow.Duration;
                _walletID = dataRow.WalletID;
                _escrow = dataRow.Escrow;
                _price = dataRow.Price;
                _buyOrder = dataRow.BuyOrder;
                _date = dataRow.Issued;
                if (UserAccount.Settings.UseLocalTimezone)
                {
                    _date = _date.AddHours(Globals.HoursOffset);
                }
            }
        }

        public int ID
        {
            get { return _id; }
        }

        public string Owner
        {
            get
            {
                if (!_gotOwner)
                {
                    try
                    {
                        _owner = Names.GetName(_ownerID);
                    }
                    catch (EMMADataMissingException)
                    {
                        _owner = "Unknown Char/Corp";
                    }
                    _gotOwner = true;
                }
                return _owner;
            }
            set { _owner = value; }
        }

        public int OwnerID
        {
            get { return _ownerID; }
        }

        public string Station
        {
            get
            {
                if (!_gotStation)
                {
                    try
                    {
                        _station = Stations.GetStationName(_stationID);
                    }
                    catch (EMMADataMissingException)
                    {
                        _station = "Unknown Station";
                    }
                    _gotStation = true;
                }
                return _station;
            }
            set { _station = value; }
        }

        public int StationID
        {
            get { return _stationID; }
        }

        public string System
        {
            get
            {
                if (!_gotSystem)
                {
                    try
                    {
                        _system = SolarSystems.GetSystemName(SystemID);
                    }
                    catch
                    {
                        _system = "Unknown System";
                    }
                    _gotStation = true;
                }
                return _system;
            }
            set { _system = value; }
        }

        public int SystemID
        {
            get { return Stations.GetStation(_stationID).solarSystemID; }
        }

        public string Region
        {
            get
            {
                if (!_gotRegion)
                {
                    try
                    {
                        _region = Regions.GetRegionName(RegionID);
                    }
                    catch
                    {
                        _region = "Unknown Region";
                    }
                    _gotRegion = true;
                }
                return _region;
            }
            set { _region = value; }
        }

        public int RegionID
        {
            get { return Stations.GetStation(_stationID).regionID; }
        }

        public string Item
        {
            get
            {
                if (!_gotItem)
                {
                    try
                    {
                        _item = Items.GetItemName(_itemID);
                    }
                    catch (EMMADataMissingException)
                    {
                        _item = "Unknown Item";
                    }
                    _gotItem = true;
                }
                return _item;
            }
            set { _item = value; }
        }

        public int ItemID
        {
            get { return _itemID; }
        }

        public string State
        {
            get
            {
                if (!_gotState)
                {
                    try
                    {
                        _state = OrderStates.GetStateDescription(_stateID);
                    }
                    catch (EMMADataException)
                    {
                        _state = "Unknown order state";
                    }
                    _gotState = true;
                }
                return _state;
            }
            set { _state = value; }
        }

        public short StateID
        {
            get { return _stateID; }
            set
            {
                _stateID = value;
                _gotState = false;
            }
        }


        public int TotalVol
        {
            get { return _totalVol; }
            set { _totalVol = value; }
        }

        public int RemainingVol
        {
            get { return _remainingVol; }
            set { _remainingVol = value; }
        }

        public int MinVol
        {
            get { return _minVol; }
            set { _minVol = value; }
        }

        public short Range
        {
            get { return _range; }
            set { _range = value; }
        }

        public string RangeText
        {
            get
            {
                string retVal = "";
                retVal = OrderRange.GetRangeText((int)_range);
                return retVal;
            }
        }

        public short WalletID
        {
            get { return _walletID; }
            set { _walletID = value; }
        }

        public short Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        public decimal Escrow
        {
            get { return _escrow; }
            set { _escrow = value; }
        }

        public decimal Price
        {
            get { return _price; }
            set { _price = value; }
        }

        public decimal TotalValue
        {
            get { return _price * _totalVol; }
        }

        public decimal RemainingValue
        {
            get { return _price * _remainingVol; }
        }

        public bool BuyOrder
        {
            get { return _buyOrder; }
            set { _buyOrder = value; }
        }

        public string Type
        {
            get
            {
                return _buyOrder ? "Buy" : "Sell";
            }
        }

        public DateTime Date
        {
            get { return _date; }
            set { _date = value; }
        }

        public long TwelveHourMovement
        {
            get
            {
                if (!_gotTwelveHourMovement)
                {
                    _twelveHourMovement = CalculateMovement(new TimeSpan(12, 0, 0));
                    _gotTwelveHourMovement = true;
                }
                return _twelveHourMovement;
            }
        }
        public long TwoDayMovement
        {
            get
            {
                if (!_gotTwoDayMovement)
                {
                    _twoDayMovement = CalculateMovement(new TimeSpan(2, 0, 0, 0));
                    _gotTwoDayMovement = true;
                }
                return _twoDayMovement;
            }
        }
        public long SevenDayMovement
        {
            get
            {
                if (!_gotSevenDayMovement)
                {
                    _sevenDayMovement = CalculateMovement(new TimeSpan(7, 0, 0, 0));
                    _gotSevenDayMovement = true;
                }
                return _sevenDayMovement;
            }
        }

        /// <summary>
        /// Calculates the movement on the market order over the period covered by the specified time span,
        /// prior to the current time.
        /// </summary>
        /// <param name="timespan"></param>
        /// <returns></returns>
        private long CalculateMovement(TimeSpan timespan)
        {
            long retVal = 0;

            List<FinanceAccessParams> accessParams = new List<FinanceAccessParams>();
            accessParams.Add(new FinanceAccessParams(_ownerID));
            List<int> itemIDs = new List<int>();
            itemIDs.Add(_itemID);
            List<int> regionIDs = new List<int>();
            List<int> stationIDs = new List<int>();
            if (_buyOrder)
            {
                short range = Range;
                List<int> systemIDs = new List<int>();
                if (range == 32767)
                {
                    regionIDs.Add(RegionID);
                }
                else
                {
                    if (range > 0)
                    {
                        // If range is greater than 0 then find the IDs of any systems within range.
                        systemIDs = SolarSystemDistances.GetSystemsInRange(SystemID, range);
                    }

                    if (range != -1)
                    {
                        // If range is anything except -1 then add the stations in all of the solar systems
                        // in range to the list of stations in the filter.
                        if (!systemIDs.Contains(SystemID)) { systemIDs.Add(SystemID); }
                        foreach (int systemID in systemIDs)
                        {
                            stationIDs.AddRange(Stations.GetStationsInSystem(systemID));
                        }
                    }
                    else
                    {
                        // If the range is -1 then we just want the one station...
                        stationIDs.Add(_stationID);
                    }
                }
            }
            else
            {
                stationIDs.Add(StationID);
            }

            DateTime timeLimit = DateTime.UtcNow.AddMinutes(timespan.TotalMinutes * -1);
            if(timeLimit.CompareTo(_date) < 0) { timeLimit = _date;}
            EMMADataSet.TransactionsDataTable transactions = Transactions.GetTransData(
                accessParams, itemIDs, regionIDs, stationIDs,
                timeLimit, DateTime.UtcNow, _buyOrder ? "buy" : "sell");

            foreach (EMMADataSet.TransactionsRow trans in transactions)
            {
                retVal += trans.Quantity;
            }
            if (retVal > _totalVol - _remainingVol) { retVal = _totalVol - _remainingVol; }

            return retVal;
        }
    }
}
