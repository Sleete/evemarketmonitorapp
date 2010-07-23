using System;
using System.Collections.Generic;
using System.Text;

namespace EveMarketMonitorApp.DatabaseClasses
{
    /// <summary>
    /// AssetAccessParams holds database access parameters in the form of a character id and 
    /// flags for corporate and/or personal data retrieval.
    /// The static 'BuildAccessList' method creates a string containing the supplied paramters 
    /// in the correct format to pass to various database stored procedures in order to retrieve
    /// assets or orders.
    /// </summary>
    public class AssetAccessParams
    {
        private int _ownerID;
        //private bool _includePersonal;
        //private bool _includeCorporate;

        public AssetAccessParams(int ownerID)//, bool includePersonal, bool includeCorporate)
        {
            _ownerID = ownerID;
            //_includePersonal = includePersonal;
            //_includeCorporate = includeCorporate;
        }

        public int OwnerID
        {
            get { return _ownerID; }
            set { _ownerID = value; }
        }

        //public bool IncludePersonal
        //{
        //    get { return _includePersonal; }
        //    set { _includePersonal = value; }
        //}

        //public bool IncludeCorporate
        //{
        //    get { return _includeCorporate; }
        //    set { _includeCorporate = value; }
        //}

        public static string BuildAccessList(List<AssetAccessParams> accessParams)
        {
            StringBuilder accessList = new StringBuilder("");
            List<int> done = new List<int>();
            foreach (AssetAccessParams entry in accessParams)
            {
                if (!done.Contains(entry.OwnerID))
                {
                    accessList.Append(entry.OwnerID);
                    done.Add(entry.OwnerID);
                    //accessList.Append(",");
                    //accessList.Append(entry.IncludePersonal);
                    //accessList.Append(",");
                    //accessList.Append(entry.IncludeCorporate);
                    if (accessList.Length != 0)
                    {
                        accessList.Append("|");
                    }
                }
            }
            return accessList.ToString();
        }
    }

    /// <summary>
    /// FinanceAccessParams holds database access parameters in the form of a char/corp id and 
    /// wallet IDs.
    /// The static 'BuildAccessList' method creates a string containing the supplied paramters 
    /// in the correct format to pass to various database stored procedures in order to retrieve
    /// journal entries or transaction.
    /// </summary>
    public class FinanceAccessParams
    {
        private int _ownerID;
        private List<short> _walletIDs;
        private bool _includeCorporate = true;

        public FinanceAccessParams(int ownerID)
        {
            _ownerID = ownerID;
            _walletIDs = new List<short>();
            _walletIDs.Add((short)0);
            InitWallets();
        }

        public FinanceAccessParams(int ownerID, List<short> walletIDs)
        {
            _ownerID = ownerID;
            _walletIDs = walletIDs;
            InitWallets();
        }

        public FinanceAccessParams(int ownerID, bool includeCorporate)
        {
            _ownerID = ownerID;
            _walletIDs = new List<short>();
            _walletIDs.Add((short)0);
            _includeCorporate = includeCorporate;
            InitWallets();
        }

        private void InitWallets()
        {
            if (_walletIDs.Count == 0)
            {
                _walletIDs = new List<short>();
                _walletIDs.Add((short)0);
            }

            for (int i = _walletIDs.Count; i < 6; i++)
            {
                _walletIDs.Add(_walletIDs[0]);
            }
        }


        public int OwnerID
        {
            get { return _ownerID; }
            set { _ownerID = value; }
        }

        public List<short> WalletIDs
        {
            get { return _walletIDs; }
            set { _walletIDs = value; }
        }

        public bool IncludeCorporate
        {
            get { return _includeCorporate; }
            set { _includeCorporate = value; }
        }


        public static string BuildAccessList(List<FinanceAccessParams> accessParams)
        {
            StringBuilder accessList = new StringBuilder("");
            foreach (FinanceAccessParams entry in accessParams)
            {
                accessList.Append(entry._ownerID);
                short spareWalletID = entry._walletIDs[0];
                int counter = 0;
                foreach (int wallet in entry.WalletIDs)
                {
                    if (!accessList.ToString().EndsWith(",")) { accessList.Append(","); }
                    accessList.Append(wallet.ToString());
                    counter++;
                }
                while (counter < 6)
                {
                    if (!accessList.ToString().EndsWith(",")) { accessList.Append(","); }
                    accessList.Append(spareWalletID.ToString());
                    counter++;
                }
                if (accessList.Length != 0)
                {
                    accessList.Append("|");
                }
            }
            return accessList.ToString();
        }
    }
}
