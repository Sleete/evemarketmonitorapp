using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.AbstractionClasses;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public class Asset : SortableObject
    {
        #region Class variables
        private long _id;
        private long _ownerID;
        private bool _gotOwner = false;
        private string _owner;
        private bool _corpAsset = false;
        private long _locationID;
        private bool _gotLocation = false;
        private string _location;
        private long _regionID;
        private bool _gotRegion = false;
        private string _region;
        private long _systemID;
        private bool _gotSystem = false;
        private string _system;
        private int _itemID;
        private bool _gotItem = false;
        private string _item;
        private long _quantity;
        private bool _autoConExclude;
        private bool _reprocExclude;
        private bool _processed;
        private int _statusID;
        private bool _gotStatus = false;
        private string _status;
        private long _eveItemInstanceID;
        private bool _isContainer;
        private Asset _container;
        private long _containerID;
        private AssetList _contents;

        private decimal _unitValue;
        private bool _gotUnitValue = false;
        private decimal _unitBuyPrice;
        private bool _gotUnitBuyPrice = false;
        private decimal _pureUnitBuyPrice;
        private bool _unitBuyPricePrecalc = false;
        private bool _selected = false;
        private decimal _reprocessValue;
        private bool _gotReprocessValue = false;
        private Dictionary<int, decimal> _reprocessPrices = new Dictionary<int, decimal>();
        private bool _forceNoReproValAsUnitVal;

        private AssetChangeTypes.ChangeType _changeTypeID;
        private bool _changeTypeSet = false;
        private bool _gotChangeType = false;
        private string _changeType = "";
        private bool _changeTypeLocked = false;

        private bool _expanded = false;
        #endregion

        #region Constructors
        public Asset(EMMADataSet.AssetsRow dataRow, Asset container)
        {
            if (dataRow != null)
            {
                _id = dataRow.ID;
                _ownerID = dataRow.OwnerID;
                if (dataRow.CorpAsset)
                {
                    _corpAsset = true;
                    _ownerID = UserAccount.CurrentGroup.GetCharacter(_ownerID).CorpID;
                }
                _locationID = dataRow.LocationID;
                _itemID = dataRow.ItemID;
                _quantity = dataRow.Quantity;
                _autoConExclude = dataRow.AutoConExclude;
                _reprocExclude = dataRow.ReprocExclude;
                _processed = dataRow.Processed;
                _statusID = dataRow.Status;
                _isContainer = dataRow.IsContainer;
                _container = container;
                _containerID = dataRow.ContainerID;
                _regionID = dataRow.RegionID;
                _systemID = dataRow.SystemID;
                _contents = new AssetList();
                _unitBuyPrice = dataRow.Cost;
                _pureUnitBuyPrice = _unitBuyPrice;
                _gotUnitBuyPrice = dataRow.CostCalc;
                _unitBuyPricePrecalc = dataRow.CostCalc;
                _eveItemInstanceID = dataRow.EveItemID;
            }
        }
        public Asset(EMMADataSet.AssetsRow dataRow)
        {
            if (dataRow != null)
            {
                _id = dataRow.ID;
                _ownerID = dataRow.OwnerID;
                if (dataRow.CorpAsset)
                {
                    _corpAsset = true;
                    _ownerID = UserAccount.CurrentGroup.GetCharacter(_ownerID).CorpID;
                }
                _locationID = dataRow.LocationID;
                _itemID = dataRow.ItemID;
                _quantity = dataRow.Quantity;
                _autoConExclude = dataRow.AutoConExclude;
                _reprocExclude = dataRow.ReprocExclude;
                _processed = dataRow.Processed;
                _statusID = dataRow.Status;
                _isContainer = dataRow.IsContainer;
                _container = null;
                _containerID = dataRow.ContainerID;
                _regionID = dataRow.RegionID;
                _systemID = dataRow.SystemID;
                _contents = new AssetList();
                _unitBuyPrice = dataRow.Cost;
                _pureUnitBuyPrice = _unitBuyPrice;
                _gotUnitBuyPrice = dataRow.CostCalc;
                _unitBuyPricePrecalc = dataRow.CostCalc;
                _eveItemInstanceID = dataRow.EveItemID;
            }
        }

        public Asset(XmlNode apiAssetData, int ownerID, bool corpAsset, Asset container)
        {
            _id = 0;
            _ownerID = ownerID;
            _corpAsset = corpAsset;
            XmlNode locationNode = apiAssetData.SelectSingleNode("@locationID");
            if (locationNode != null)
            {
                _locationID = int.Parse(locationNode.Value);

                // Translate location ID from a corporate office to a station ID if required.
                if (_locationID >= 66000000 && _locationID < 67000000)
                {
                    // NPC station.
                    _locationID -= 6000001;
                }
                if (_locationID >= 67000000 && _locationID < 68000000)
                {
                    // Conquerable station.
                    _locationID -= 6000000;
                }
            }
            _itemID = int.Parse(apiAssetData.SelectSingleNode("@typeID").Value);
            _eveItemInstanceID = long.Parse(apiAssetData.SelectSingleNode("@itemID").Value);
            _quantity = int.Parse(apiAssetData.SelectSingleNode("@quantity").Value);
            if (apiAssetData.LastChild != null && apiAssetData.LastChild.Name.Equals("rowset"))
            {
                _isContainer = true;
            }
            _autoConExclude = false;
            _reprocExclude = false;
            _processed = false;
            _statusID = 1;
            _contents = new AssetList();
            _container = container;
            _containerID = container.ID;
            _contents = new AssetList();
            _unitBuyPrice = 0;
            _pureUnitBuyPrice = _unitBuyPrice;
            _gotUnitBuyPrice = false;
            _unitBuyPricePrecalc = false;

            if (_isContainer)
            {
                XmlNodeList contents = apiAssetData.SelectNodes("rowset/row");
                foreach (XmlNode asset in contents)
                {
                    _contents.Add(new Asset(asset, ownerID, corpAsset, this));
                }
            }
        }

        /// <summary>
        /// !!Use this empty constructor with caution!!
        /// </summary>
        public Asset()
        {
        }
        #endregion

        #region Property accessors
        public long ContainerID
        {
            get { return _containerID; }
        }
        public Asset Container
        {
            get
            {
                if (_container == null && _containerID != 0)
                {
                    _container = new Asset(Assets.GetAssetDetail(_containerID));
                }
                return _container;
            }
            set { _container = value; if (_container != null) { _containerID = _container.ID; } }
        }

        public AssetList Contents
        {
            get
            {
                if (_isContainer)
                {
                    if (_contents == null || _contents.Count == 0)
                    {
                        _contents = Assets.LoadAssets(this);
                    }
                }
                return _contents;
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
            set { _owner = value; }
        }
        public long OwnerID
        {
            get { return _ownerID; }
            set 
            { 
                _ownerID = value;
                bool corp = false;
                UserAccount.CurrentGroup.GetCharacter(_ownerID, ref corp);
                _corpAsset = corp;
                _gotOwner = false; 
            }
        }

        public bool CorpAsset
        {
            get { return _corpAsset; }
        }

        public long LocationID
        {
            get { return _locationID; }
            set
            {
                _locationID = value; _gotLocation = false;
                _systemID = 0; _regionID = 0;
                _gotSystem = false; _gotRegion = false;
            }
        }
        

        public string Location
        {
            get
            {
                if (!_gotLocation)
                {
                    string locType = "Location";
                    try
                    {
                        if (_locationID >= 30000000 && _locationID < 40000000)
                        {
                            locType = "Solar System";
                            _location = SolarSystems.GetSystemName(_locationID);
                        }
                        if (_locationID >= 60000000 && _locationID < 70000000)
                        {
                            locType = "Station";
                            _location = Stations.GetStationName(_locationID);
                        }
                    }
                    catch (EMMADataMissingException)
                    {
                        _location = "Unknown " + locType;
                    }
                    _gotLocation = true;
                }
                return _location;
            }
            set { _item = value; }
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
                    catch (EMMADataMissingException)
                    {
                        _system = "Unknown System (" + SystemID + ")";
                    }
                    _gotSystem = true;
                }
                return _system;
            }
        }

        public long SystemID
        {
            get
            {
                if (_systemID == 0)
                {
                    try
                    {
                        if (_locationID >= 30000000 && _locationID < 40000000)
                        {
                            _systemID = SolarSystems.GetSystem(_locationID).solarSystemID;
                        }
                        if (_locationID >= 60000000 && _locationID < 70000000)
                        {
                            _systemID = Stations.GetStation(_locationID).solarSystemID;
                        }
                    }
                    catch (EMMADataMissingException)
                    {
                        _systemID = 0;
                    }
                }
                return _systemID;
            }
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
                    catch (EMMADataMissingException)
                    {
                        _region = "Unknown Region (" + RegionID + ")";
                    }
                    _gotRegion = true;
                }
                return _region;
            }
        }

        public long RegionID
        {
            get
            {
                if (_regionID == 0)
                {
                    try
                    {
                        EveDataSet.mapSolarSystemsRow system = SolarSystems.GetSystem(SystemID);
                        if (system != null) { _regionID = system.regionID; }
                    }
                    catch (EMMADataMissingException)
                    {
                        _regionID = 0;
                    }
                }
                return _regionID;
            }
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
                    if (_isContainer)
                    {
                        _item = _item + " (" + _id + ")";
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
            set
            {
                _itemID = value;
                _gotItem = false;
            }
        }

        public long EveItemInstanceID
        {
            get { return _eveItemInstanceID; }
            set { _eveItemInstanceID = value; }
        }

        public int StatusID
        {
            get { return _statusID; }
            set { _statusID = value; _gotStatus = false; }
        }
        public string Status
        {
            get
            {
                if (!_gotStatus)
                {
                    try
                    {
                        _status = AssetStatus.GetDescription(_statusID);
                    }
                    catch
                    {
                        _status = "Unknown";
                    }
                    _gotStatus = true;
                }
                return _status;
            }
            set { _status = value; }
        }

        public long ID
        {
            get { return _id; }
            set { _id = value; }
        }

        public long Quantity
        {
            get { return _quantity; }
            set 
            {
                _quantity = value;
                _gotReprocessValue = false;
            }
        }

        public bool AutoConExclude
        {
            get { return _autoConExclude; }
            set { _autoConExclude = value; }
        }

        public bool ReprocessorExclude
        {
            get { return _reprocExclude; }
            set { _reprocExclude = value; }
        }

        public bool Processed
        {
            get { return _processed; }
            set
            {
                _processed = value;
                foreach (Asset asset in Contents)
                {
                    asset.Processed = value;
                }
            }
        }

        public bool IsContainer
        {
            get { return _isContainer; }
            set { _isContainer = value; }
        }

        public decimal UnitValue
        {
            get
            {
                if (!_gotUnitValue)
                {
                    try
                    {
                        bool tmp = false;
                        if (_forceNoReproValAsUnitVal)
                        {
                            tmp = UserAccount.CurrentGroup.ItemValues.UseReprocessValGet(_itemID);
                            if (tmp)
                            {
                                UserAccount.CurrentGroup.ItemValues.UseReprocessValSet(_itemID, false);
                            }
                        }
                        _unitValue = UserAccount.CurrentGroup.ItemValues.GetItemValue(_itemID, 10000002, false);
                        if (_forceNoReproValAsUnitVal && tmp)
                        {
                            UserAccount.CurrentGroup.ItemValues.UseReprocessValSet(_itemID, tmp);
                        }
                    }
                    catch
                    {
                        _unitValue = 0;
                    }
                    _gotUnitValue = true;
                }
                return _unitValue;
            }
        }

        public bool ForceNoReproValAsUnitVal
        {
            get { return _forceNoReproValAsUnitVal; }
            set { _forceNoReproValAsUnitVal = value; }
        }

        public decimal TotalValue
        {
            get { return UnitValue * _quantity; }
        }

        // This returns the unit buy price value that is stored in the database
        // If the value is not precalculated, etc then no attempt is made to calculate
        // a buy price from other data.
        public decimal PureUnitBuyPrice
        {
            get { return _pureUnitBuyPrice; }
        }

        public decimal UnitBuyPrice
        {
            get
            {
                /// Usually, unit buy price will have been obtained when the asset was originally added 
                /// to the user's database.
                /// However, any assets added before version 1.4.2.0 will not have this data. Even those
                /// added after may be missing it in some cases.
                /// Consequently, we need to try and work out what was paid for the item.
                if (!_gotUnitBuyPrice)
                {
                    try
                    {
                        List<long> stationIDs = new List<long>();
                        stationIDs.Add(_locationID);
                        // First try getting the price by looking at the most recent buy transactions
                        // at the asset's location.
                        Transactions.GetAverageBuyPrice(UserAccount.CurrentGroup.GetFinanceAccessParams(
                            APIDataType.Transactions), _itemID, stationIDs, new List<long>(),
                            Math.Abs(_quantity), 0, ref _unitBuyPrice);

                        // If we don't find anything then just use the most recent buy transaction 
                        // at any location.
                        if (_unitBuyPrice == 0)
                        {
                            Transactions.GetAverageBuyPrice(UserAccount.CurrentGroup.GetFinanceAccessParams(
                                APIDataType.Transactions), _itemID, new List<long>(), new List<long>(),
                                Math.Abs(_quantity), 0, ref _unitBuyPrice);
                        }

                        // If we still don't have anything then use the ItemValues object
                        if (_unitBuyPrice == 0)
                        {
                            if (_locationID >= 60000000 && _locationID < 70000000)
                            {
                                _unitBuyPrice = UserAccount.CurrentGroup.ItemValues.GetBuyPrice(_itemID, _locationID, true);
                            }
                            else
                            {
                                _unitBuyPrice = UserAccount.CurrentGroup.ItemValues.GetBuyPrice(_itemID, 0);
                            }
                        }
                    }
                    catch
                    {
                        _unitBuyPrice = 0;
                    }

                    _gotUnitBuyPrice = true;
                }
                return _unitBuyPrice;
            }
            set
            {
                _unitBuyPrice = value;
                _gotUnitBuyPrice = true;
            }
        }

        public bool UnitBuyPricePrecalculated
        {
            get { return _unitBuyPricePrecalc; }
            set { _unitBuyPricePrecalc = value; }
        }

        public decimal TotalBuyPrice
        {
            get { return UnitBuyPrice * _quantity; }
        }

        public bool Selected
        {
            get { return _selected; }
            set { _selected = value; }
        }

        public bool Expanded
        {
            get { return _expanded; }
            set { _expanded = value; }
        }

        public decimal ReprocessValue
        {
            get
            {
                if (!_gotReprocessValue && _reprocessPrices != null)
                {
                    ReprocessJob job = new ReprocessJob(0, 0, 0);
                    job.AddItem(_itemID, Math.Abs(_quantity), TotalBuyPrice);
                    job.SetDefaultResultPrices(_reprocessPrices);
                    job.UpdateResults();
                    _reprocessValue = job.TotalResultsValue;
                    _gotReprocessValue = true;
                }
                return _reprocessValue;
            }
        }

        public void ClearReprocessValue()
        {
            _gotReprocessValue = false;
        }

        public void SetReprocessPrices(Dictionary<int, decimal> prices)
        {
            _reprocessPrices = prices;
        }


        public int ChangeTypeIntID
        {
            get { return (int)ChangeTypeID; }
            set
            {
                if (!_changeTypeLocked)
                {
                    ChangeTypeID = (AssetChangeTypes.ChangeType)value;
                    _changeTypeSet = true;
                }
            }
        }
        public AssetChangeTypes.ChangeType ChangeTypeID
        {
            get 
            {
                if (!_changeTypeSet)
                {
                    if (UserAccount.Settings.ManufacturingMode)
                    {
                        _changeTypeID = AssetChangeTypes.ChangeType.Unknown;
                    }
                    else
                    {
                        if (Quantity > 0) { _changeTypeID = AssetChangeTypes.ChangeType.Found; }
                        else { _changeTypeID = AssetChangeTypes.ChangeType.DestroyedOrUsed; }
                    }
                }
                return _changeTypeID; 
            }
            set { _changeTypeID = value; }
        }

        public string ChangeType
        {
            get
            {
                if (!_gotChangeType)
                {
                    _changeType = AssetChangeTypes.GetChangeTypeDesc(ChangeTypeID);
                }
                return _changeType;
            }
        }
        public bool ChangeTypeLocked
        {
            get { return _changeTypeLocked; }
            set { _changeTypeLocked = value; }
        }

        public AssetList GetBillOfMaterials()
        {
            return Assets.GetBillOfMaterials(this.ItemID, this.Quantity);
        }
        #endregion

        #region Overriden object methods
        public override bool Equals(object obj)
        {
            return Equals(obj, false);
        }
        public bool Equals(object obj, bool includeQuantity)
        {
            Asset other = obj as Asset;
            bool retVal = false;

            if (other != null)
            {
                retVal = this.GetHashCode(includeQuantity) == other.GetHashCode(includeQuantity);
            }

            return retVal;
        }

        public override int GetHashCode()
        {
            return GetHashCode(false);
        }
        public int GetHashCode(bool includeQuantity)
        {
            return ToString(includeQuantity).GetHashCode();
        }

        public override string ToString()
        {
            return ToString(false);
        }
        public string ToString(bool includeQuantity)
        {
            StringBuilder str = new StringBuilder();
            str.Append(_ownerID);
            str.Append(_locationID);
            str.Append(_itemID);
            str.Append(_statusID);
            if (includeQuantity) { str.Append(_quantity); }
            if (Contents != null) { str.Append(Contents.GetHashCode()); }
            return str.ToString();
        }
        #endregion
    }
}
