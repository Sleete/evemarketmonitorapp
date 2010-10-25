using System;
using System.Collections.Generic;
using System.Text;

using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public class Contract : SortableObject
    {
        private long _id;

        private long _ownerID;
        private bool _gotOwner = false;
        private string _owner;
        private short _statusID;
        private bool _gotStatus = false;
        private string _status;
        private long _pickupStationID;
        private bool _gotPickupStation = false;
        private string _pickupStation;
        private long _destinationStationID;
        private bool _gotDestinationStation = false;
        private string _destinationStation;
        private ContractType _type;

        private decimal _collateral;
        private decimal _reward;
        private decimal _expectedProfit;
        private decimal _totalVolume;
        private DateTime _issueDate;

        private ContractItemList _items = null;

        public Contract(long ownerID, short statusID)
        {
            _ownerID = ownerID;
            _statusID = statusID;
            _pickupStationID = 0;
            _destinationStationID = 0;
            _collateral = 0;
            _reward = 0;
            _expectedProfit = 0;
            _totalVolume = 0;
            _issueDate = DateTime.Now;
            _items = new ContractItemList();
            _type = ContractType.Courier;
        }

        public Contract(long ownerID, short statusID, long pickupStationID, long destinationStationID,
            decimal collateral, decimal reward, decimal expectedProfit, DateTime issueDate, ContractItemList items,
            ContractType type)
        {
            _ownerID = ownerID;
            _statusID = statusID;
            _pickupStationID = pickupStationID;
            _destinationStationID = destinationStationID;
            _collateral = collateral;
            _reward = reward;
            _expectedProfit = expectedProfit;
            _issueDate = issueDate;
            _items = items;
            _type = type;
        }

        public Contract(EMMADataSet.ContractsRow data)
        {
            _id = data.ID;
            _ownerID = data.OwnerID;
            _statusID = data.Status;
            _pickupStationID = data.PickupStationID;
            _destinationStationID = data.DestinationStationID;
            _collateral = data.Collateral;
            _reward = data.Reward;
            _issueDate = data.DateTime;
            if (UserAccount.Settings.UseLocalTimezone)
            {
                _issueDate = _issueDate.AddHours(Globals.HoursOffset);
            }
            _type = (ContractType)data.Type;
            _expectedProfit = 0;
        }


        #region Direct property accessors
        public decimal Collateral
        {
            get { return _collateral; }
            set { _collateral = value; }
        }

        public decimal Reward
        {
            get { return _reward; }
            set { _reward = value; }
        }

        public long ID
        {
            get { return _id; }
            set { _id = value; }
        }

        public DateTime IssueDate
        {
            get { return _issueDate; }
            set { _issueDate = value; }
        }

        public long PickupStationID
        {
            get { return _pickupStationID; }
            set
            {
                _pickupStationID = value;
                _gotPickupStation = false;
            }
        }

        public long DestinationStationID
        {
            get { return _destinationStationID; }
            set
            {
                _destinationStationID = value;
                _gotDestinationStation = false;
            }
        }

        public short StatusID
        {
            get { return _statusID; }
            set
            {
                _statusID = value;
                _gotStatus = false;
            }
        }

        public long OwnerID
        {
            get { return _ownerID; }
            set
            {
                _ownerID = value;
                _gotOwner = false;
            }
        }

        public ContractType Type
        {
            get { return _type; }
            set { _type = value; }
        }
        #endregion

        #region Property accessors requiring a lookup
        /// <summary>
        /// Read only property, use PickupStationID to set the value.
        /// </summary>
        public string PickupStation
        {
            get
            {
                if (!_gotPickupStation)
                {
                    try
                    {
                        _pickupStation = Stations.GetStationName(_pickupStationID);
                    }
                    catch (EMMADataMissingException)
                    {
                        _pickupStation = "Unknown Station";
                    }
                    _gotPickupStation = true;
                }
                return _pickupStation;
            }
        }

        /// <summary>
        /// Read only property, use DestinationStationID to set the value.
        /// </summary>
        public string DestinationStation
        {
            get
            {
                if (!_gotDestinationStation)
                {
                    try
                    {
                        _destinationStation = Stations.GetStationName(_destinationStationID);
                    }
                    catch (EMMADataMissingException)
                    {
                        _destinationStation = "Unknown Station";
                    }
                    _gotDestinationStation = true;
                }
                return _destinationStation;
            }
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
        }

        /// <summary>
        /// Read only property, use StatusID to set the value.
        /// </summary>
        public string Status
        {
            get
            {
                if (!_gotStatus)
                {
                    try
                    {
                        _status = ContractStatus.GetDescription(_statusID);
                    }
                    catch (EMMADataMissingException)
                    {
                        _status = "Unknown";
                    }
                    _gotStatus = true;
                }
                return _status;
            }
        }
        #endregion

        #region Property lookups requiring calculation and/or more expensive database access
        public ContractItemList Items
        {
            get
            {
                if (_items == null)
                {
                    _items = Contracts.GetContractItems(this);
                }
                return _items;
            }
            set
            {
                _items = value;
                _expectedProfit = 0;
                _totalVolume = 0;
            }
        }

        public decimal ExpectedProfit
        {
            get
            {
                if (_expectedProfit == 0)
                {
                    foreach (ContractItem item in Items)
                    {
                        _expectedProfit += (item.SellPrice - item.BuyPrice) * item.Quantity;
                    }
                }
                return _expectedProfit;
            }
            set { _expectedProfit = value; }
        }

        public decimal TotalVolume
        {
            get
            {
                if (_totalVolume == 0)
                {
                    foreach (ContractItem item in Items)
                    {
                        _totalVolume += item.ItemVolume * item.Quantity;
                    }
                }
                return _totalVolume;
            }
            set { _totalVolume = value; }
        }

        public decimal CostOfTransport(int itemID, out long quantity)
        {
            decimal retVal = 0;
            decimal itemValue = 0;
            decimal contractValue = 0;
            quantity = 0;
            bool profitBased = UserAccount.CurrentGroup.Settings.RewardBasedOn.Equals("Profit");

            contractValue = (profitBased ? ExpectedProfit : Collateral);
            for (int i = 0; i < Items.Count; i++)
            {
                ContractItem item = Items[i];
                if (item.ItemID == itemID)
                {
                    itemValue += (profitBased ? item.Profit : item.Collateral);
                    quantity += item.Quantity;
                    i = Items.Count;
                }
            }

            retVal = (contractValue > 0 ? ((itemValue /  contractValue) * _reward) : 0);

            return retVal;
        }
        #endregion
    }

    public class ContractItem : SortableObject
    {
        private int _itemID;
        private bool _gotItem = false;
        private string _item;
        private int _quantity;
        private decimal _sellPrice;
        private decimal _buyPrice;
        private decimal _volume;
        private decimal _profit = 0;
        private decimal _collateral = 0;
        private Contract _contract;
        private decimal _percOfTotalProfit = 0;
        private decimal _percOfTotalVolume = 0;
        private long _transactionID = 0;
        private bool _forcePrice = false;

        public ContractItem(int itemID, int quantity, decimal sellPrice, decimal buyPrice, Contract contract)
        {
            _itemID = itemID;
            _quantity = quantity;
            _sellPrice = sellPrice;
            _buyPrice = buyPrice;
            _contract = contract;
        }

        public ContractItem(EMMADataSet.ContractItemRow data, Contract contract)
        {
            _itemID = data.ItemID;
            _quantity = data.Quantity;
            _sellPrice = data.SellPrice;
            _buyPrice = data.BuyPrice;
            _contract = contract;
            _transactionID = data.TransactionID;
            _forcePrice = data.ForcePrice;
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
        }

        public int ItemID
        {
            get { return _itemID; }
            set
            {
                _itemID = value;
                _gotItem = false;
            }
        }

        public int Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                _contract.ExpectedProfit = 0;
                _contract.Collateral = 0;
                _contract.Reward = 0;
                _percOfTotalProfit = 0;
                _percOfTotalVolume = 0;
                _profit = 0;
                _collateral = 0;
            }
        }

        public decimal SellPrice
        {
            get { return _sellPrice; }
            set
            {
                _sellPrice = value;
                _contract.ExpectedProfit = 0;
                _contract.Collateral = 0;
                _contract.Reward = 0;
                _percOfTotalProfit = 0;
                _percOfTotalVolume = 0;
                _profit = 0;
                _collateral = 0;
            }
        }

        public decimal BuyPrice
        {
            get { return _buyPrice; }
            set
            {
                _buyPrice = value;
                _contract.ExpectedProfit = 0;
                _contract.Collateral = 0;
                _contract.Reward = 0;
                _percOfTotalProfit = 0;
                _percOfTotalVolume = 0;
                _profit = 0;
                _collateral = 0;
            }
        }

        public decimal Profit
        {
            get
            {
                if (_profit == 0)
                {
                    _profit = (_sellPrice - _buyPrice) * _quantity;
                }
                return _profit;
            }
        }

        public decimal Collateral
        {
            get
            {
                if (_collateral == 0)
                {
                    _collateral = AutoContractor.CalcCollateral(
                            UserAccount.CurrentGroup.Settings.CollateralBasedOn,
                            UserAccount.CurrentGroup.Settings.CollateralPercentage,
                            _buyPrice * _quantity, _sellPrice * _quantity);
                }
                return _collateral;
            }
        }

        public decimal ItemVolume
        {
            get
            {
                if (_volume == 0)
                {
                    _volume = (decimal)Items.GetItemVolume(_itemID);
                }
                return _volume;
            }
        }
        
        public decimal PercOfTotalProfit
        {
            get
            {
                if (_percOfTotalProfit == 0)
                {
                    if (_contract.ExpectedProfit == 0)
                    {
                        _percOfTotalProfit = 0;
                    }
                    else
                    {
                        _percOfTotalProfit = Profit / _contract.ExpectedProfit;
                    }
                }
                return _percOfTotalProfit;
            }
        }

        public decimal PercOfTotalVolume
        {
            get
            {
                if (_percOfTotalVolume == 0)
                {
                    if (_contract.TotalVolume == 0) 
                    {
                        _percOfTotalVolume = 0; 
                    }
                    else
                    {
                        _percOfTotalVolume = (this.ItemVolume * _quantity) / Math.Abs(_contract.TotalVolume);
                    }
                }
                return _percOfTotalVolume;
            }
        }

        public long TransactionID
        {
            get { return _transactionID; }
            set { _transactionID = value; }
        }

        public bool ForcePrice
        {
            get { return _forcePrice; }
            set { _forcePrice = value; }
        }
    }

}
