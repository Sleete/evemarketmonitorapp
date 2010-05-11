using System;
using System.Collections.Generic;
using System.Text;

using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.AbstractionClasses;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public class Transaction : SortableObject
    {
        // For values that are an id, the text value corresponding to the id is
        // retrieved as required (eg item, station, etc)
        private long _id;

        private DateTime _datetime;
        private int _quantity;
        private int _itemID;
        private bool _gotItem = false;
        private string _item;
        private decimal _price;
        private int _buyerID;
        private bool _gotBuyer = false;
        private string _buyer;
        private int _sellerID;
        private bool _gotSeller = false;
        private string _seller;
        private int _sellerCharID;
        private bool _gotSellerChar = false;
        private string _sellerChar;
        private int _buyerCharID;
        private bool _gotBuyerChar = false;
        private string _buyerChar;
        private int _stationID;
        private bool _gotStation = false;
        private string _station;
        private int _regionID;
        private bool _buyerForCorp;
        private bool _sellerForCorp;
        private short _buyerWalletID;
        private bool _gotBuyerWallet = false;
        private string _buyerWallet = "";
        private short _sellerWalletID;
        private bool _gotSellerWallet = false;
        private string _sellerWallet = "";

        private TransactionType _type;
        private bool _gotType = false;
        private decimal _profit = 0;
        private bool _gotProfit = false;
        private bool _calcProfitFromAssets = false;

        public Transaction(EMMADataSet.TransactionsRow dataRow)
        {
            if (dataRow != null)
            {
                _id = dataRow.ID;
                Populate(dataRow);
            }
        }

        private void Populate(EMMADataSet.TransactionsRow dataRow)
        {
            _datetime = dataRow.DateTime;
            if (UserAccount.Settings.UseLocalTimezone)
            {
                _datetime = _datetime.AddHours(Globals.HoursOffset);
            }
            _quantity = dataRow.Quantity;
            _itemID = dataRow.ItemID;
            _price = dataRow.Price;
            _buyerID = dataRow.BuyerID;
            _sellerID = dataRow.SellerID;
            _buyerCharID = dataRow.BuyerCharacterID;
            _sellerCharID = dataRow.SellerCharacterID;
            _stationID = dataRow.StationID;
            _regionID = dataRow.RegionID;
            _buyerForCorp = dataRow.BuyerForCorp;
            _sellerForCorp = dataRow.SellerForCorp;
            _buyerWalletID = dataRow.BuyerWalletID;
            _sellerWalletID = dataRow.SellerWalletID;
            _profit = dataRow.SellerUnitProfit;
            _gotProfit = _profit != 0;
            _calcProfitFromAssets = dataRow.CalcProfitFromAssets;
        }


        public long Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public DateTime Datetime
        {
            get { return _datetime; }
            set { _datetime = value; }
        }

        public int Quantity
        {
            get { return _quantity; }
            set { _quantity = value; }
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

        public decimal Price
        {
            get { return _price; }
            set { _price = value; }
        }

        public decimal Total
        {
            get { return _price * _quantity; }
        }

        public string Buyer
        {
            get
            {
                if (!_gotBuyer)
                {
                    try
                    {
                        _buyer = Names.GetName(_buyerID);
                    }
                    catch (EMMADataMissingException)
                    {
                        _buyer = "Unknown Char/Corp";
                    }
                    _gotBuyer = true;
                }
                return _buyer;
            }
            set { _buyer = value; }
        }

        public string Seller
        {
            get
            {
                if (!_gotSeller)
                {
                    try
                    {
                        _seller = Names.GetName(_sellerID);
                    }
                    catch (EMMADataMissingException)
                    {
                        _seller = "Unknown Char/Corp";
                    }
                    _gotSeller = true;
                }
                return _seller;
            }
            set { _seller = value; }
        }

        public string SellerChar
        {
            get
            {
                if (!_gotSellerChar)
                {
                    try
                    {
                        _sellerChar = Names.GetName(_sellerCharID);
                    }
                    catch (EMMADataMissingException)
                    {
                        _sellerChar = "Unknown Char/Corp";
                    }
                    _gotSellerChar = true;
                }
                return _sellerChar;
            }
            set { _sellerChar = value; }
        }

        public string BuyerChar
        {
            get
            {
                if (!_gotBuyerChar)
                {
                    try
                    {
                        _buyerChar = Names.GetName(_buyerCharID);
                    }
                    catch (EMMADataMissingException)
                    {
                        _buyerChar = "Unknown Char/Corp";
                    }
                    _gotBuyerChar = true;
                }
                return _buyerChar;
            }
            set { _buyerChar = value; }
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

        public int RegionID
        {
            get { return _regionID; }
            set { _regionID = value; }
        }

        public short BuyerWalletID
        {
            get { return _buyerWalletID; }
            set { _buyerWalletID = value; }
        }

        public string BuyerWallet
        {
            get
            {
                if (!_gotBuyerWallet)
                {
                    if (_buyerForCorp)
                    {
                        if (UserAccount.CurrentGroup.Accounts != null)
                        {
                            foreach (EVEAccount account in UserAccount.CurrentGroup.Accounts)
                            {
                                if (account.Chars != null)
                                {
                                    foreach (APICharacter character in account.Chars)
                                    {
                                        if (character.CorpID == _buyerID)
                                        {
                                            foreach (EMMADataSet.WalletDivisionsRow division in character.WalletDivisions)
                                            {
                                                if (division.ID == _buyerWalletID)
                                                {
                                                    _buyerWallet = division.Name;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (_buyerWallet.Equals(""))
                        {
                            _buyerWallet = "Unknown Wallet Division";
                        }
                    }
                    else
                    {
                        _buyerWallet = "Personal";
                    }
                    _gotBuyerWallet = true;
                }
                return _buyerWallet;
            }
            set { _buyerWallet = value; }
        }

        public short SellerWalletID
        {
            get { return _sellerWalletID; }
            set { _sellerWalletID = value; }
        }

        public string SellerWallet
        {
            get
            {
                if (!_gotSellerWallet)
                {
                    if (_sellerForCorp)
                    {
                        if (UserAccount.CurrentGroup.Accounts != null)
                        {
                            foreach (EVEAccount account in UserAccount.CurrentGroup.Accounts)
                            {
                                if (account.Chars != null)
                                {
                                    foreach (APICharacter character in account.Chars)
                                    {
                                        if (character.CorpID == _sellerID)
                                        {
                                            foreach (EMMADataSet.WalletDivisionsRow division in character.WalletDivisions)
                                            {
                                                if (division.ID == _sellerWalletID)
                                                {
                                                    _sellerWallet = division.Name;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (_sellerWallet.Equals(""))
                        {
                            _sellerWallet = "Unknown Wallet Division";
                        }
                    }
                    else
                    {
                        _sellerWallet = "Personal";
                    }
                    _gotSellerWallet = true;
                }
                return _sellerWallet;
            }
            set { _sellerWallet = value; }
        }


        public int BuyerID
        {
            get { return _buyerID; }
            set { _buyerID = value; }
        }

        public int SellerID
        {
            get { return _sellerID; }
            set { _sellerID = value; }
        }

        public int BuyerCharID
        {
            get {return _buyerCharID; }
            set { _buyerCharID = value; }
        }

        public int SellerCharID
        {
            get {  return _sellerCharID; }
            set { _sellerCharID = value; }
        }

        public TransactionType Type
        {
            get
            {
                if (!_gotType)
                {
                    bool isBuyTransaction = false;
                    bool isSellTransaction = false;
                    _type = TransactionType.None;

                    foreach (EVEAccount account in UserAccount.CurrentGroup.Accounts)
                    {
                        foreach (APICharacter character in account.Chars)
                        {
                            if (!isSellTransaction && ((character.CharIncWithRptGroup && 
                                (_sellerID == character.CharID || _sellerCharID == character.CharID)) ||
                                (character.CorpIncWithRptGroup && _sellerID == character.CorpID)))
                            {
                                isSellTransaction = true;
                            }
                            if (!isBuyTransaction && ((character.CharIncWithRptGroup && 
                                (_buyerID == character.CharID || _buyerCharID == character.CharID)) ||
                                (character.CorpIncWithRptGroup && _buyerID == character.CorpID)))
                            {
                                isBuyTransaction = true;
                            }
                        }
                    }

                    if (isBuyTransaction && isSellTransaction)
                    {
                        _type = TransactionType.Both;
                    }
                    else if (isBuyTransaction)
                    {
                        _type = TransactionType.Buy;
                    }
                    else if (isSellTransaction)
                    {
                        _type = TransactionType.Sell;
                    }

                    _gotType = true;
                }
                return _type;
            }
        }

        public decimal GrossUnitProfit
        {
            get
            {
                if (!_gotProfit)
                {
                    List<AssetAccessParams> assetAccessParams = 
                        UserAccount.CurrentGroup.GetAssetAccessParams(APIDataType.Assets);
                    List<FinanceAccessParams> financeAccessParams = 
                        UserAccount.CurrentGroup.GetFinanceAccessParams(APIDataType.Transactions);

                    if (Type == TransactionType.Sell)
                    {
                        long historyQuantity = Assets.GetTotalQuantity(assetAccessParams, financeAccessParams, 
                            _itemID, _datetime.AddSeconds(-1));
                        List<int> itemIDList = new List<int>();
                        itemIDList.Add(_itemID);

                        decimal buyPrice = 0, blank1 = 0;
                        Transactions.GetAverageBuyPrice(financeAccessParams, itemIDList, new List<int>(),
                            new List<int>(), _quantity, historyQuantity,
                            ref buyPrice, ref blank1, true);

                        _profit = _price - buyPrice;
                    }
                    else
                    {
                        _profit = 0;
                    }

                    _gotProfit = true;
                }
                return _profit;
            }
            set
            {
                _profit = value;
                _gotProfit = true;
            }
        }

        public decimal GrossProfit
        {
            get
            {
                return GrossUnitProfit * _quantity;
            }
        }

        public bool CalcProfitFromAssets
        {
            get { return _calcProfitFromAssets; }
            set { _calcProfitFromAssets = value; }
        }
    }

    public enum TransactionType
    {
        Buy,
        Sell,
        Both,
        None
    }
}
