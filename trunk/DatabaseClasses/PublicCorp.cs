using System;
using System.Collections.Generic;
using System.Text;

using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public class PublicCorp : SortableObject
    {
        private int _id;
        private string _name;
        private string _ticker;
        private string _desc;
        private decimal _shareValue;
        private string _ceo;
        private decimal _expectedPayout;
        private CorpPayoutPeriod _payoutPeriod;
        private decimal _nav;
        private DateTime _navDate;
        private bool _bank;
        private RiskRating _riskRating;

        private int _sharesOwned;

        private bool _accountDetailsLoaded = false;
        private int _accountID = 0;
        private decimal _amountInAccount;
        private long _ownerID = -1;
        private bool _gotOwner = false;
        private string _ownerName = "";

        public PublicCorp()
        {
            _id = 0;
            _name = "";
            _ticker = "";
            _desc = "";
            _shareValue = 0;
            _ceo = "";
            _expectedPayout = 0;
            _payoutPeriod = CorpPayoutPeriod.Unspecified;
            _nav = 0;
            _navDate = DateTime.UtcNow;
            _bank = false;
            _riskRating = RiskRating.NotRated;
        }

        public PublicCorp(EMMADataSet.PublicCorpsRow data)
        {
            _id = data.CorpID;
            _name = data.CorpName;
            _ticker = data.Ticker;
            _desc = data.Description;
            _shareValue = data.ValuePerShare;
            _ceo = data.CEO;
            _expectedPayout = data.ExpectedPayoutPerShare;
            _payoutPeriod = (CorpPayoutPeriod)data.PayoutPeriodID;
            _nav = data.EstimatedNAV;
            _navDate = data.NAVTakenAt;
            if (UserAccount.Settings.UseLocalTimezone)
            {
                _navDate = _navDate.AddHours(Globals.HoursOffset);
            }
            _bank = data.Bank;
            _riskRating = (RiskRating)data.RiskRatingID;
        }

        public PublicCorp(EMMADataSet.InvestmentsRow data)
        {
            _id = data.CorpID;
            _name = data.CorpName;
            _ticker = data.Ticker;
            _desc = data.Description;
            _shareValue = data.ValuePerShare;
            _ceo = data.CEO;
            _expectedPayout = data.ExpectedPayoutPerShare;
            _payoutPeriod = (CorpPayoutPeriod)data.PayoutPeriodID;
            _nav = data.EstimatedNAV;
            _navDate = data.NAVTakenAt;
            if (!data.IsSharesOwnedNull())
            {
                _sharesOwned = data.SharesOwned;
            }
            else
            {
                _sharesOwned = 0;
            }
            _bank = data.Bank;
            _riskRating = (RiskRating)data.RiskRatingID;
        }

        private void GetBankAccountDetails()
        {
            if (_bank && !_accountDetailsLoaded)
            {
                if (_ownerID >= 0)
                {
                    EMMADataSet.BankAccountRow account = BankAccounts.GetSingleBankAccountData(
                        UserAccount.CurrentGroup.ID, _ownerID, _id);
                    if (account != null)
                    {
                        _amountInAccount = account.Balance;
                        _accountID = account.AccountID;
                    }
                    else
                    {
                        _amountInAccount = 0;
                        _accountID = 0;
                    }
                }
                else
                {
                    _amountInAccount = 0;
                    _accountID = 0;
                }
                _accountDetailsLoaded = true;
            }
        }

        public void ReloadBankAccountDetails()
        {
            _accountDetailsLoaded = false;
            GetBankAccountDetails();
        }

        public int ID
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Ticker
        {
            get { return _ticker; }
            set { _ticker = value; }
        }
        
        public string Description
        {
            get { return _desc; }
            set { _desc = value; }
        }

        public decimal ShareValue
        {
            get { return _shareValue; }
            set { _shareValue = value; }
        }

        public string CEO
        {
            get { return _ceo; }
            set { _ceo = value; }
        }

        public decimal ExpectedPayout
        {
            get { return _expectedPayout; }
            set { _expectedPayout = value; }
        }

        public CorpPayoutPeriod PayoutPeriod
        {
            get { return _payoutPeriod; }
            set { _payoutPeriod = value; }
        }

        public decimal NAV
        {
            get { return _nav; }
            set { _nav = value; }
        }

        public DateTime NAVDate
        {
            get { return _navDate; }
            set { _navDate = value; }
        }

        public int SharesOwned
        {
            get { return _sharesOwned; }
        }

        public bool Bank
        {
            get { return _bank; }
            set { _bank = value; }
        }

        public decimal SharesOwnedValue
        {
            get { return _sharesOwned * _shareValue; }
        }

        public RiskRating CorpRiskRating
        {
            get { return _riskRating; }
            set { _riskRating = value; }
        }

        public decimal CalcSharesOwnedValue(DateTime date)
        {
            decimal retVal = 0;
            if (!_bank)
            {
                retVal = ShareTransactions.GetSharesOwned(date, UserAccount.CurrentGroup.ID, _id) *
                    ShareValueHistory.GetShareValue(_id, date);
            }
            else
            {
                retVal = AmountInAccount;
            }
            return retVal;
        }

        public decimal AmountInAccount
        {
            get
            {
                GetBankAccountDetails();
                BankAccounts.PayOutstandingInterest(_accountID);
                ReloadBankAccountDetails();
                return _amountInAccount;
            }
        }

        public decimal TotalInterest
        {
            get
            {
                GetBankAccountDetails();
                return BankAccounts.GetTotalTransactionIsk(_accountID, 
                    BankTransactionType.InterestPayment); 
            }
        }

        public int BankAccountID
        {
            get
            {
                GetBankAccountDetails();
                return _accountID;
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

        public string Owner
        {
            get
            {
                if (!_gotOwner)
                {
                    if (_ownerID > 0)
                    {
                        try
                        {
                            _ownerName = Names.GetName(_ownerID);
                        }
                        catch (EMMADataMissingException)
                        {
                            _ownerName = "Unknown Char/Corp";
                        }
                    }
                    _gotOwner = true;
                }
                return _ownerName;
            }
            set { _ownerName = value; }
        }

        public override bool Equals(object obj)
        {
            bool retVal = false;
            PublicCorp other = obj as PublicCorp;
            if (other != null)
            {
                retVal = obj.GetHashCode() == GetHashCode();
            }
            return retVal;
        }

        public override string ToString()
        {
            return _id.ToString();
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
    }
}
