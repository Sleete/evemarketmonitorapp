using System;
using System.Collections.Generic;
using System.Text;

using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.AbstractionClasses;

namespace EveMarketMonitorApp.DatabaseClasses
{
    class JournalEntry : SortableObject
    {
        private long _id;
        private DateTime _date;
        private short _typeID;
        private bool _gotType = false;
        private string _type;
        private bool _ownerIsSender;
        private long _senderID;
        private bool _gotSender = false;
        private string _sender;
        private long _recieverID;
        private bool _gotReciever = false;
        private string _reciever;
        private string _argName;
        private long _argID;
        private decimal _amount;
        private decimal _balance;
        private string _reason;
        private int _sWalletID;
        private bool _gotSWallet = false;
        private string _sWallet = "";
        private int _rWalletID;
        private bool _gotRWallet = false;
        private string _rWallet = "";
        private long _rCorpID;
        private bool _gotRCorp = false;
        private string _rCorp;
        private long _sCorpID;
        private bool _gotSCorp = false;
        private string _sCorp;



        public JournalEntry(EMMADataSet.JournalRow dataRow, long ownerID)
        {
            if (dataRow != null)
            {
                _ownerIsSender = ownerID == dataRow.SenderID;
                _id = dataRow.ID;
                _date = dataRow.Date;
                if (UserAccount.Settings.UseLocalTimezone)
                {
                    _date = _date.AddHours(Globals.HoursOffset);
                }
                _typeID = dataRow.TypeID;
                _senderID = dataRow.SenderID;
                _recieverID = dataRow.RecieverID;
                _argName = _ownerIsSender ? dataRow.SArgName : dataRow.RArgName;
                _argID = _ownerIsSender ? dataRow.SArgID : dataRow.RArgID;
                _amount = dataRow.Amount;
                _balance = _ownerIsSender ? dataRow.SBalance : dataRow.RBalance;
                _reason = dataRow.Reason;
                _sWalletID = dataRow.SWalletID;
                _rWalletID = dataRow.RWalletID;
                _sCorpID = dataRow.SCorpID;
                _rCorpID = dataRow.RCorpID;
            }
        }


        public long Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public DateTime Date
        {
            get { return _date; }
            set { _date = value; }
        }

        public string Type
        {
            get 
            {
                if (!_gotType)
                {
                    try
                    {
                        _type = JournalRefTypes.GetReferenceDesc(_typeID);
                    }                    
                    catch (EMMADataMissingException)
                    {
                        _type = "Unknown Type";
                    }
                    _gotType = true;
                }
                return _type; 
            }
            set { _type = value; }
        }

        public bool OwnerIsSender
        {
            get { return _ownerIsSender; }
            set { _ownerIsSender = value; }
        }

        public string Sender
        {
            get
            {
                if (!_gotSender)
                {
                    try
                    {
                        _sender = Names.GetName(_senderID);
                    }
                    catch (EMMADataMissingException)
                    {
                        _sender = "Unknown Char/Corp";
                    }
                    _gotSender = true;
                }
                return _sender;
            }
            set { _sender = value; }
        }

        public long SenderID
        {
            get { return _senderID; }
            set { _senderID = value; }
        }

        public string Reciever
        {
            get
            {
                if (!_gotReciever)
                {
                    try
                    {
                        _reciever = Names.GetName(_recieverID);
                    }
                    catch (EMMADataMissingException)
                    {
                        _reciever = "Unknown Char/Corp";
                    }
                    _gotReciever = true;
                }
                return _reciever;
            }
            set { _reciever = value; }
        }

        public long RecieverID
        {
            get { return _recieverID; }
            set { _recieverID = value; }
        }

        public string ArgName
        {
            get { return _argName; }
            set { _argName = value; }
        }

        public long ArgID
        {
            get { return _argID; }
            set { _argID = value; }
        }

        public decimal Amount
        {
            get { return _amount; }
            set { _amount = value; }
        }

        public decimal Balance
        {
            get { return _balance; }
            set { _balance = value; }
        }

        public string Reason
        {
            get { return _reason; }
            set { _reason = value; }
        }

        public string SenderWallet
        {
            get
            {
                if (!_gotSWallet)
                {
                    if (_sCorpID != 0)
                    {
                        if (UserAccount.CurrentGroup.Accounts != null)
                        {
                            foreach (EVEAccount account in UserAccount.CurrentGroup.Accounts)
                            {
                                if (account.Chars != null)
                                {
                                    foreach (APICharacter character in account.Chars)
                                    {
                                        if (character.CorpID == _sCorpID)
                                        {
                                            foreach (EMMADataSet.WalletDivisionsRow division in character.WalletDivisions)
                                            {
                                                if (division.ID == _sWalletID)
                                                {
                                                    _sWallet = division.Name;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (_sWallet.Equals(""))
                        {
                            _sWallet = "Unknown Wallet Division";
                        }
                    }
                    else
                    {
                        _sWallet = "Personal";
                    }
                    _gotSWallet = true;
                }
                return _sWallet;
            }
            set { _sWallet = value; }
        }

        public string RecieverWallet
        {
            get
            {
                if (!_gotRWallet)
                {
                    if (_rCorpID != 0)
                    {
                        foreach (EVEAccount account in UserAccount.CurrentGroup.Accounts)
                        {
                            foreach (APICharacter character in account.Chars)
                            {
                                if (character.CorpID == _rCorpID)
                                {
                                    foreach (EMMADataSet.WalletDivisionsRow division in character.WalletDivisions)
                                    {
                                        if (division.ID == _rWalletID)
                                        {
                                            _rWallet = division.Name;
                                        }
                                    }
                                }
                            }
                        }

                        if (_rWallet.Equals(""))
                        {
                            _rWallet = "Unknown Wallet Division";
                        }
                    }
                    else
                    {
                        _rWallet = "Personal";
                    }
                    _gotRWallet = true;
                }
                return _rWallet;
            }
            set { _rWallet = value; }
        }

        public string SenderCorp
        {
            get
            {
                if (!_gotSCorp)
                {
                    try
                    {
                        _sCorp = Names.GetName(_sCorpID);
                    }
                    catch (EMMADataMissingException)
                    {
                        _sCorp = "Unknown Corp";
                    }
                    _gotSCorp = true;
                }
                return _sCorp;
            }
            set { _sCorp = value; }
        }

        public string RecieverCorp
        {
            get
            {
                if (!_gotRCorp)
                {
                    try
                    {
                        _rCorp = Names.GetName(_rCorpID);
                    }
                    catch (EMMADataMissingException)
                    {
                        _rCorp = "Unknown Corp";
                    }
                    _gotRCorp = true;
                }
                return _rCorp;
            }
            set { _rCorp = value; }
        }
    }
}
