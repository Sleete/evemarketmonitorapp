using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Data.SqlTypes;
using System.Threading;
using System.IO;

using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.GUIElements;
using System.Data;

namespace EveMarketMonitorApp.AbstractionClasses
{
    public class APICharacter : IProvideStatus
    {
        public event StatusChangeHandler StatusChange;

        private int _userID;
        private string _apiKey;

        private string _charName;
        private int _charID;

        private string _corpName;
        private int _corpID;
        private string _corpTag;
        private bool _corpFinanceAccess = false;

        private List<APICharacter> _otherCorpChars = new List<APICharacter>();

        //private int _brokerRelationsLvl = 0;
        //private int _accountingLvl = 0;
        //private int _marginTradingLvl = 0;

        private Dictionary<int, int> _skills = new Dictionary<int,int>();

        private EMMADataSet.WalletDivisionsDataTable _corpWalletDivisions;

        private XmlDocument _walletXMLCache = new XmlDocument();
        private DateTime _walletXMLLastUpdate = SqlDateTime.MinValue.Value;
        private XmlDocument _corpWalletXMLCache = new XmlDocument();
        private DateTime _corpWalletXMLLastUpdate = SqlDateTime.MinValue.Value;
        private XmlDocument _charSheetXMLCache = new XmlDocument();
        private DateTime _charSheetXMLLastUpdate = SqlDateTime.MinValue.Value;
        private XmlDocument _corpSheetXMLCache = new XmlDocument();
        private DateTime _corpSheetXMLLastUpdate = SqlDateTime.MinValue.Value;

        private bool _charIncWithRptGroup = false;
        private int _charLastRptGroupID = 0;
        private bool _oldCharIncWithRptGroup = false;
        private bool _corpIncWithRptGroup = false;
        private int _corpLastRptGroupID = 0;
        private bool _oldCorpIncWithRptGroup = false;

        private APISettingsAndStatus _apiSettings;
        private object _syncLock = new object();

        public event APIUpdateEvent UpdateEvent;

        private AssetList _unacknowledgedGains = new AssetList();
        private AssetList _unacknowledgedLosses = new AssetList();
        private AssetList _corpUnacknowledgedGains = new AssetList();
        private AssetList _corpUnacknowledgedLosses = new AssetList();

        #region Public properties
        public int CharID
        {
            get { return _charID; }
        }

        public string CharName
        {
            get { return _charName; }
        }

        public string CorpName
        {
            get { return _corpName; }
        }

        public string CorpTag
        {
            get { return _corpTag; }
        }

        public int CorpID
        {
            get { return _corpID; }
        }

        public int UserID
        {
            get { return _userID; }
        }

        public string APIKey
        {
            get { return _apiKey; }
            set { _apiKey = value; }
        }

        public bool CorpFinanceAccess
        {
            get { return _corpFinanceAccess; }
            set { _corpFinanceAccess = value; }
        }

        public XmlDocument CharSheet
        {
            get { return _charSheetXMLCache; }
            set { _charSheetXMLCache = value; }
        }

        public XmlDocument CorpSheet
        {
            get { return _corpSheetXMLCache; }
            set { _corpSheetXMLCache = value; }
        }

        public DateTime CharSheetXMLLastUpdate
        {
            get { return _charSheetXMLLastUpdate; }
            set { _charSheetXMLLastUpdate = value; }
        }

        public DateTime CorpSheetXMLLastUpdate
        {
            get { return _corpSheetXMLLastUpdate; }
            set { _corpSheetXMLLastUpdate = value; }
        }

        public bool CharIncWithRptGroup
        {
            get
            {
                GetGroupLevelCharSettings();
                return _charIncWithRptGroup;
            }
            set
            {
                _charIncWithRptGroup = value;
            }
        }

        public bool CorpIncWithRptGroup
        {
            get
            {
                GetGroupLevelCorpSettings();
                return _corpIncWithRptGroup;
            }
            set
            {
                _corpIncWithRptGroup = value;
            }
        }

        public int BrokerRelationsLvl
        {
            get { return GetSkillLvl(Skills.BrokerRelations); }
        }

        public int AccountingLvl
        {
            get { return GetSkillLvl(Skills.Accounting); }
        }

        public int MarginTradingLvl
        {
            get { return GetSkillLvl(Skills.MarginTrading); }
        }

        public int GetSkillLvl(Skills skill)
        {
            int retVal = 0;
            if (_skills.ContainsKey((int)skill))
            {
                retVal = _skills[(int)skill];
            }
            return retVal;
        }

        public APICharSettings Settings
        {
            get { return _apiSettings.Settings; }
        }

        public List<APICharacter> OtherCorpChars
        {
            get { return _otherCorpChars; }
            set { _otherCorpChars = value; }
        }

        public AssetList UnacknowledgedGains
        {
            get { return _unacknowledgedGains; }
            set { _unacknowledgedGains = value; }
        }
        public AssetList CorpUnacknowledgedGains
        {
            get { return _corpUnacknowledgedGains; }
            set { _corpUnacknowledgedGains = value; }
        }
        public AssetList UnacknowledgedLosses
        {
            get { return _unacknowledgedLosses ; }
            set { _unacknowledgedLosses = value; }
        }
        public AssetList CorpUnacknowledgedLosses
        {
            get { return _corpUnacknowledgedLosses; }
            set { _corpUnacknowledgedLosses = value; }
        }
        #endregion

        public APICharacter(int userID, string apiKey, EMMADataSet.APICharactersRow data)
        {
            _userID = userID;
            _apiKey = apiKey;
            _charID = data.ID;
            _apiSettings = new APISettingsAndStatus(_charID);
            if (data.CharSheet.Length > 0)
            {
                _charSheetXMLCache.LoadXml(data.CharSheet);
                _charSheetXMLLastUpdate = data.LastCharSheetUpdate;
                GetDataFromCharXML();
            }
            if (data.CorpSheet.Length > 0)
            {
                _corpSheetXMLCache.LoadXml(data.CorpSheet);
                _corpSheetXMLLastUpdate = data.LastCorpSheetUpdate;
                GetDataFromCorpXML();
            }

            _corpFinanceAccess = data.CorpFinanceAccess;

            try
            {
                RefreshCharXMLFromAPI();
            }
            catch { }
            try
            {
                RefreshCorpXMLFromAPI();
            }
            catch { }

            SetLastAPIUpdateTime(CharOrCorp.Char, APIDataType.Assets, data.LastCharAssetsUpdate);
            SetLastAPIUpdateTime(CharOrCorp.Char, APIDataType.Journal, data.LastCharJournalUpdate);
            SetLastAPIUpdateTime(CharOrCorp.Char, APIDataType.Orders, data.LastCharOrdersUpdate);
            SetLastAPIUpdateTime(CharOrCorp.Char, APIDataType.Transactions, data.LastCharTransUpdate);
            SetLastAPIUpdateTime(CharOrCorp.Corp, APIDataType.Assets, data.LastCorpAssetsUpdate);
            SetLastAPIUpdateTime(CharOrCorp.Corp, APIDataType.Journal, data.LastCorpJournalUpdate);
            SetLastAPIUpdateTime(CharOrCorp.Corp, APIDataType.Orders, data.LastCorpOrdersUpdate);
            SetLastAPIUpdateTime(CharOrCorp.Corp, APIDataType.Transactions, data.LastCorpTransUpdate);

            SetHighestID(CharOrCorp.Char, APIDataType.Transactions, data.HighestCharTransID);
            SetHighestID(CharOrCorp.Corp, APIDataType.Transactions, data.HighestCorpTransID);
            SetHighestID(CharOrCorp.Char, APIDataType.Journal, data.HighestCharJournalID);
            SetHighestID(CharOrCorp.Corp, APIDataType.Journal, data.HighestCorpJournalID);
        }

        public APICharacter(int userID, string apiKey, int charID)
        {
            _userID = userID;
            _charID = charID;
            _apiKey = apiKey;
            _apiSettings = new APISettingsAndStatus(_charID);

            try
            {
                RefreshCharXMLFromAPI();
            }
            catch { }
            try
            {
                RefreshCorpXMLFromAPI();
            }
            catch { }
        }

        public void ResetIncludedVars()
        {
            _charIncWithRptGroup = _oldCharIncWithRptGroup;
            _corpIncWithRptGroup = _oldCorpIncWithRptGroup;
        }

        public void StoreGroupLevelSettings(SettingsStoreType type)
        {
            if (type == SettingsStoreType.Char | type == SettingsStoreType.Both)
            {
                ReportGroups.SetCharGroupSettings(_charLastRptGroupID, _charID, _charIncWithRptGroup,
                    _apiSettings.GetAutoUpdateFlag(CharOrCorp.Char, APIDataType.Transactions),
                    _apiSettings.GetAutoUpdateFlag(CharOrCorp.Char, APIDataType.Journal),
                    _apiSettings.GetAutoUpdateFlag(CharOrCorp.Char, APIDataType.Assets),
                    _apiSettings.GetAutoUpdateFlag(CharOrCorp.Char, APIDataType.Orders));
                _oldCharIncWithRptGroup = _charIncWithRptGroup;
            }
            if (type == SettingsStoreType.Corp | type == SettingsStoreType.Both)
            {
                ReportGroups.SetCorpGroupSettings(_corpLastRptGroupID, _corpID, _corpIncWithRptGroup,
                    _apiSettings.GetAutoUpdateFlag(CharOrCorp.Corp, APIDataType.Transactions),
                    _apiSettings.GetAutoUpdateFlag(CharOrCorp.Corp, APIDataType.Journal),
                    _apiSettings.GetAutoUpdateFlag(CharOrCorp.Corp, APIDataType.Assets),
                    _apiSettings.GetAutoUpdateFlag(CharOrCorp.Corp, APIDataType.Orders), _charID);
                _oldCorpIncWithRptGroup = _corpIncWithRptGroup;
            }
        }

        public void StoreSettings()
        {
            _apiSettings.StoreSettings();
        }

        #region Accessors for data stored in _apiSettings
        public bool GetAPIAutoUpdate(CharOrCorp corc, APIDataType type)
        {
            GetGroupLevelCharSettings();
            return _apiSettings.GetAutoUpdateFlag(corc, type);
        }
        public void SetAPIAutoUpdate(CharOrCorp corc, APIDataType type, bool auto)
        {
            _apiSettings.SetAutoUpdateFlag(corc, type, auto);
            StoreGroupLevelSettings(corc == CharOrCorp.Char ? SettingsStoreType.Char : SettingsStoreType.Corp);
        }

        public DateTime GetLastAPIUpdateTime(CharOrCorp corc, APIDataType type)
        {
            return _apiSettings.GetLastUpdateTime(corc, type);
        }
        public void SetLastAPIUpdateTime(CharOrCorp corc, APIDataType type, DateTime time)
        {
            _apiSettings.SetLastUpdateTime(corc, type, time);
        }

        public string GetLastAPIUpdateError(CharOrCorp corc, APIDataType type)
        {
            return _apiSettings.GetLastUpdateError(corc, type);
        }
        public void SetLastAPIUpdateError(CharOrCorp corc, APIDataType type, string error)
        {
            _apiSettings.SetLastUpdateError(corc, type, error);
        }


        public long GetHighestID(CharOrCorp corc, APIDataType type)
        {
            return _apiSettings.GetHighestID(corc, type);
        }
        public void SetHighestID(CharOrCorp corc, APIDataType type, long id)
        {
            _apiSettings.SetHighestID(corc, type, id);
        }
        #endregion

        /// <summary>
        /// Return the balance of the master wallet.
        /// </summary>
        /// <param name="corp"></param>
        /// <returns></returns>
        public decimal GetWalletBalance(bool corp)
        {
            // By default just get master wallet balance
            List<int> ids = new List<int>();
            ids.Add(1000);
            return GetWalletBalance(corp, ids);
        }

        private void UpdateStatus(int progress, int maxprogress, string sectionName, string status, bool complete)
        {
            if (StatusChange != null)
            {
                StatusChange(this, new StatusChangeArgs(progress, maxprogress, sectionName, status, complete));
            }
        }
        private void UpdateStatus(int progress, int maxprogress, string sectionName, string status, bool complete,
            int currentSubProgress, int currentSubMax, string subDesc)
        {
            if (StatusChange != null)
            {
                StatusChange(this, new StatusChangeArgs(progress, maxprogress, sectionName, status, complete,
                    currentSubProgress, currentSubMax, subDesc));
            }
        }

        #region Methods calling the Eve API
        public void RefreshCharXMLFromAPI()
        {
            XmlDocument xml = null;

            if (DateTime.UtcNow.AddHours(-1).CompareTo(_charSheetXMLLastUpdate) > 0)
            {
                xml = EveAPI.GetXml(EveAPI.URL_EveApiBase + EveAPI.URL_CharDataApi,
                  "userid=" + _userID + "&apiKey=" + _apiKey + "&characterID=" + _charID);
            }
            else
            {
                xml = _charSheetXMLCache;
            }

            if (xml != null)
            {
                try
                {
                    if (!_charSheetXMLCache.Equals(xml))
                    {
                        // If there is some problem with the API or XML then this method will throw an exception.
                        // This will prevent _charList from being set as we don't want to overwrite good data
                        // with an error or invalid data.
                        XmlNodeList results = EveAPI.GetResults(xml);
                        _charSheetXMLCache = xml;
                        _charSheetXMLLastUpdate = DateTime.UtcNow;
                    }
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {
                    GetDataFromCharXML();
                }
            }

        }

        /// <summary>
        /// Return the total balance of the specified wallets.
        /// </summary>
        /// <param name="corp"></param>
        /// <param name="walletIDs"></param>
        /// <returns></returns>
        public decimal GetWalletBalance(bool corp, List<int> walletIDs)
        {
            decimal retVal = 0;
            XmlDocument xml = corp ? _corpWalletXMLCache : _walletXMLCache;

            // Only request new data from the API if our cached version is more than 1 hour old.
            if (xml == null || DateTime.UtcNow.AddHours(-1).CompareTo(
                (corp ? _corpWalletXMLLastUpdate : _walletXMLLastUpdate)) > 0)
            {
                xml = EveAPI.GetXml(EveAPI.URL_EveApiBase +
                    (corp ? EveAPI.URL_WalletCorpApi : EveAPI.URL_WalletApi),
                    "userid=" + _userID + "&apiKey=" + _apiKey +
                    "&characterID=" + _charID);
            }

            if (corp)
            {
                _corpWalletXMLCache = xml;
                _corpWalletXMLLastUpdate = DateTime.UtcNow;
            }
            else
            {
                _walletXMLCache = xml;
                _walletXMLLastUpdate = DateTime.UtcNow;
            }

            XmlNodeList walletsData = EveAPI.GetResults(xml);

            foreach (XmlNode walletData in walletsData)
            {
                XmlNode walletID = walletData.SelectSingleNode("@accountKey");
                XmlNode walletBalance = walletData.SelectSingleNode("@balance");

                if (walletIDs.Contains(int.Parse(walletID.Value,
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat)))
                {
                    retVal += decimal.Parse(walletBalance.Value,
                        System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                }
            }

            return retVal;
        }

        /// <summary>
        /// Update standings for this character or corporation with the latest from the Eve API
        /// </summary>
        /// <param name="corc"></param>
        public void UpdateStandings(CharOrCorp corc)
        {
            int id = (corc == CharOrCorp.Char ? _charID : _corpID);


            XmlDocument xml = EveAPI.GetXml(EveAPI.URL_EveApiBase +
                (corc == CharOrCorp.Corp ? EveAPI.URL_CorpStandingsApi : EveAPI.URL_CharStandingsApi),
                "userid=" + _userID + "&apiKey=" + _apiKey + "&characterID=" + _charID);

            // Standings xml does not have the normal format so can't use EveAPI.GetResults...
            if (xml != null)
            {
                // First check if we've been returned an error.
                XmlNode errorNode = xml.SelectSingleNode("/eveapi/error");
                if (errorNode != null)
                {
                    string file = string.Format("{0}Logging{1}APIError.xml",
                        Globals.AppDataDir, Path.DirectorySeparatorChar);
                    lock (Globals.APIErrorFileLock)
                    {
                        xml.Save(file);
                    }

                    XmlNode errCodeNode = errorNode.SelectSingleNode("@code");
                    XmlNode errTextNode = errorNode.FirstChild;

                    throw new EMMAEveAPIException(ExceptionSeverity.Error,
                        int.Parse(errCodeNode.Value), errTextNode.Value);
                }

                // If there is no error then clear current standings, get the data we need
                // and update with the new standings as we go along.
                Standings.ClearStandings(id, 0);
                Standings.ClearStandings(0, id);

                XmlNodeList xmlNodes = null;
                // First we do standings set by this char/corp towards others...
                xmlNodes = xml.SelectNodes("/eveapi/result/standingsTo/rowset/row");
                foreach (XmlNode node in xmlNodes)
                {
                    int toID = int.Parse(node.Attributes.GetNamedItem("toID").Value);
                    string toName = node.Attributes.GetNamedItem("toName").Value;
                    decimal standing = decimal.Parse(node.Attributes.GetNamedItem("standing").Value, 
                        System.Globalization.CultureInfo.InvariantCulture.NumberFormat) * 10;
                    if (standing > 10) { standing = 10; }
                    if (standing < -10) { standing = -10; }

                    Names.AddName(toID, toName);
                    Standings.SetStanding(toID, id, standing);
                }

                // ...then standings set by others towrds this char/corp
                xmlNodes = xml.SelectNodes("/eveapi/result/standingsFrom/rowset/row");
                foreach (XmlNode node in xmlNodes)
                {
                    int fromID = int.Parse(node.Attributes.GetNamedItem("fromID").Value);
                    string fromName = node.Attributes.GetNamedItem("fromName").Value;
                    decimal standing = decimal.Parse(node.Attributes.GetNamedItem("standing").Value,
                        System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //* 10;
                    if (standing > 10) { standing = 10; }
                    if (standing < -10) { standing = -10; }

                    Names.AddName(fromID, fromName);
                    Standings.SetStanding(id, fromID, standing);
                }
                
            }
            else
            {
                throw new EMMAEveAPIException(ExceptionSeverity.Critical, "No XML document to process");
            }
        }


        public void UpdateDataFromAPI(CharOrCorp corc, APIDataType type)
        {
            SetLastAPIUpdateError(corc, type, "QUEUED");

            switch (type)
            {
                case APIDataType.Transactions:
                    ThreadPool.QueueUserWorkItem(RetrieveAPITrans, corc);
                    break;
                case APIDataType.Journal:
                    ThreadPool.QueueUserWorkItem(RetrieveAPIJournal, corc);
                    break;
                case APIDataType.Assets:
                    ThreadPool.QueueUserWorkItem(RetrieveAPIAssets, corc);
                    break;
                case APIDataType.Orders:
                    ThreadPool.QueueUserWorkItem(RetrieveAPIOrders, corc);
                    break;
                default:
                    break;
            }

        }

        #region Update Assets
        private void RetrieveAPIAssets(object param)
        {
            CharOrCorp corc = (CharOrCorp)param;

            bool updateAllowed = false;
            DateTime startTryUpdate = DateTime.Now;
            bool askUser = false;
            while (!updateAllowed)
            {
                // Only allow the assets update to run if there has been an update to transactions and 
                // orders within the last x minutes, the number of minutes is specified by the user.
                TimeSpan timeSinceTransUpdate = DateTime.UtcNow.Subtract(
                    GetLastAPIUpdateTime(corc, APIDataType.Transactions));
                TimeSpan timeSinceOrdersUpdate = DateTime.UtcNow.Subtract(
                    GetLastAPIUpdateTime(corc, APIDataType.Orders));
                int minutes = UserAccount.Settings.AssetsUpdateMaxMinutes;

                if (minutes == 0 ||
                    (timeSinceOrdersUpdate.TotalMinutes < minutes && timeSinceTransUpdate.TotalMinutes < minutes))
                {
                    updateAllowed = true;
                    // Note, the sync lock is used to make sure that a transaction, assets or orders update do
                    // not run at the same time for a character. 
                    lock (_syncLock)
                    {
                        SetLastAPIUpdateError(corc, APIDataType.Assets, "UPDATING");
                        RetrieveAssets(corc, null);
                    }
                }
                else
                {
                    // Wait for five seconds before checking order/transaction update times again.
                    Thread.Sleep(5000);
                    if (!askUser && startTryUpdate.AddMinutes(2).CompareTo(DateTime.Now) < 0)
                    {
                        SetLastAPIUpdateError(corc, APIDataType.Assets, "BLOCKED");                       
                        askUser = true;
                    }
                }
            }

            try
            {
                APICharacters.Store(this);
            }
            catch (Exception) { }

            if (GetLastAPIUpdateError(corc, APIDataType.Assets).Equals("UPDATING"))
            {
                SetLastAPIUpdateError(corc, APIDataType.Assets, "");
            }
        }

        public void RetrieveAssets(XmlDocument fileXml, CharOrCorp corc)
        {
            DataImportParams parameters = new DataImportParams();
            parameters.xmlData = fileXml;
            parameters.corc = corc;
            parameters.walletID = 0;
            Thread t0 = new Thread(new ParameterizedThreadStart(RetrieveAssets));
            t0.Start(parameters);
        }

        private void RetrieveAssets(object parameters)
        {
            // Just wait a moment to allow the caller to display the 
            // progress dialog if importing from a file.
            Thread.Sleep(200);
            DataImportParams data = (DataImportParams)parameters;
            RetrieveAssets(data.corc, data.xmlData);
        }

        private void RetrieveAssets(CharOrCorp corc, XmlDocument fileXml)
        {
            DateTime earliestUpdate = GetLastAPIUpdateTime(corc, APIDataType.Assets).AddHours(23);
            EMMADataSet.AssetsDataTable assetData = new EMMADataSet.AssetsDataTable();
            bool fromFile = fileXml != null;
            DateTime fileDate = DateTime.MinValue;

            try
            {
                // Set the 'processed' flag to false for all of this char/corp's assets.
                Assets.SetProcessedFlag(_charID, corc == CharOrCorp.Corp, (int)AssetStatus.States.Normal, false);
                Assets.SetProcessedFlag(_charID, corc == CharOrCorp.Corp, (int)AssetStatus.States.ForSaleViaMarket, false);
                Assets.SetProcessedFlag(_charID, corc == CharOrCorp.Corp, (int)AssetStatus.States.ForSaleViaContract, false);
                Assets.SetProcessedFlag(_charID, corc == CharOrCorp.Corp, (int)AssetStatus.States.InTransit, false);
                // Remove containers and all thier contents
                //Assets.ClearUnProcessed(_charID, corc == CharOrCorp.Char, corc == CharOrCorp.Corp, true);

                if (!fromFile & earliestUpdate.CompareTo(DateTime.UtcNow) > 0)
                {
                    throw new EMMAEveAPIException(ExceptionSeverity.Warning, 1000, "Cannot get asset data so soon" +
                        " after the last update. Wait until at least " + earliestUpdate.ToLongTimeString() +
                        " before retrieving the asset list.");
                }

                XmlNodeList assetList = null;
                XmlDocument xml = new XmlDocument();

                try
                {
                    if (fromFile)
                    {
                        UpdateStatus(0, 1, "Getting asset data from file", "", false);

                        fileDate = EveAPI.GetAssetDataTime(fileXml);
                        DateTime assetsEffectiveDate = corc == CharOrCorp.Char ? Settings.CharAssetsEffectiveDate :
                            Settings.CorpAssetsEffectiveDate;
                        if (fileDate.CompareTo(assetsEffectiveDate) < 0)
                        {
                            UpdateStatus(1, 1, "Error", "This data in this file is from " + fileDate.ToString() +
                                ". EMMA has imaport asset data dated " + assetsEffectiveDate + " therefore the" +
                                " database will not be updated.", true);
                            assetList = null;
                        }
                        else
                        {
                            assetList = EveAPI.GetResults(fileXml);
                            UpdateStatus(1, 1, "", assetList.Count + " asset data lines found.", false);
                        }
                    }
                    else
                    {
                        xml = EveAPI.GetXml(
                            EveAPI.URL_EveApiBase + (corc == CharOrCorp.Corp ? EveAPI.URL_AssetCorpApi :
                            EveAPI.URL_AssetApi),
                            "userid=" + _userID + "&apiKey=" + _apiKey + "&version=2" +
                            "&characterID=" + _charID);

                        fileDate = EveAPI.GetAssetDataTime(xml);
                        DateTime assetsEffectiveDate = corc == CharOrCorp.Char ? Settings.CharAssetsEffectiveDate :
                            Settings.CorpAssetsEffectiveDate;
                        if (fileDate.CompareTo(assetsEffectiveDate) < 0)
                        {
                            // If the assets file we're loading is dated before what we have previously loaded
                            // then don't bother.
                            assetList = null;
                        }
                        else
                        {
                            assetList = EveAPI.GetResults(xml);
                            SetLastAPIUpdateTime(corc, APIDataType.Assets, fileDate);
                        }

                        // If we've been successfull in getting data and this is a corporate data request
                        // then make sure we've got access set to true;
                        if (corc == CharOrCorp.Corp)
                        {
                            Settings.CorpAssetsAPIAccess = true;
                        }
                    }

                    //UpdateStatus(1, 1, "", assetList.Count + " asset data lines retrieved.", false);
                }
                catch (EMMAEveAPIException emmaApiEx)
                {
                    if (emmaApiEx.EveCode == 102)
                    {
                        string err = emmaApiEx.EveDescription;

                        // If there is a cachedUntil tag, dont try and get data again until
                        // after it has expired.
                        XmlNode nextTime = xml.SelectSingleNode("/eveapi/cachedUntil");
                        XmlNode eveTime = xml.SelectSingleNode("/eveapi/currentTime");
                        TimeSpan difference = DateTime.UtcNow.Subtract(DateTime.Parse(eveTime.FirstChild.Value,
                            System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat));
                        DateTime nextAllowed = DateTime.Parse(nextTime.FirstChild.Value,
                            System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat).Add(difference);

                        SetLastAPIUpdateTime(corc, APIDataType.Assets, nextAllowed.Subtract(
                            UserAccount.Settings.APIAssetUpdatePeriod));
                        SetLastAPIUpdateError(corc, APIDataType.Assets,
                            "The Eve API reports that this data has already been retrieved, no update has occured.");

                        //UpdateStatus(0, 0, "Warning", "Asset data already retrieved in the last 24 hours.", false);
                        assetList = null;
                    }
                    else if (emmaApiEx.EveCode == 200)
                    {
                        // Security level not high enough
                        SetLastAPIUpdateError(corc, APIDataType.Assets,
                            "You must enter your FULL api key to retrieve financial data.\r\n" +
                            "Use the 'manage group' button to correct this.");
                    }
                    else if (emmaApiEx.EveCode == 206 || emmaApiEx.EveCode == 208 ||
                        emmaApiEx.EveCode == 209)
                    {
                        // Character does not have required corporate role.
                        Settings.CorpAssetsAPIAccess = false;
                        SetAPIAutoUpdate(corc, APIDataType.Assets, false);
                        SetLastAPIUpdateError(corc, APIDataType.Assets, emmaApiEx.Message);
                    }
                    else
                    {
                        throw emmaApiEx;
                    }
                }

                if (assetList != null)
                {
                    AssetList changes = new AssetList();

                    // Create an in-memory datatable with all of the changes required to the assets 
                    // database in order to reflect the data in the xml file.
                    UpdateAssets(assetData, assetList, 0, corc, 0, changes);
                    // Use the currently active sell order to account for assets that appear to be
                    // missing.
                    if (fromFile) { UpdateStatus(0, 0, "Processing active sell orders", "", false); }
                    Assets.ProcessSellOrders(assetData, changes, _charID, corc == CharOrCorp.Corp);
                    if (fromFile) { UpdateStatus(0, 0, "", "Complete", false); }
                    // Use transactions that occured after the effective date of the asset data file
                    // to ensure that the asset list is as up-to-date as possible.
                    if (fromFile) { UpdateStatus(0, 0, "Updating assets from transactions that occur after " +
                        "the asset file's effective date", "", false); }
                    long maxID = Assets.UpdateFromTransactions(assetData, changes, _charID, _corpID, 
                        corc == CharOrCorp.Corp, fileDate);
                    if (corc == CharOrCorp.Char) { Settings.CharAssetsTransUpdateID = maxID; }
                    else { Settings.CorpAssetsTransUpdateID = maxID; }
                    if (fromFile) { UpdateStatus(0, 0, "", "Complete", false); }

                    AssetList gained = new AssetList();
                    AssetList lost = new AssetList();
                    if ((corc == CharOrCorp.Char && Settings.FirstUpdateDoneAssetsChar) ||
                        (corc == CharOrCorp.Corp && Settings.FirstUpdateDoneAssetsCorp))
                    {
                        if (fromFile) { UpdateStatus(0, 0, "Analysing changes to assets", "", false);}
                        Assets.AnalyseChanges(assetData, _charID, corc == CharOrCorp.Corp, changes, 
                            out gained, out lost);
                        if (fromFile) { UpdateStatus(0, 0, "", "Complete", false); }
                    }

                    if (corc == CharOrCorp.Char)
                    {
                        _unacknowledgedGains = gained;
                        _unacknowledgedLosses = lost;
                    }
                    else
                    {
                        _corpUnacknowledgedGains = gained;
                        _corpUnacknowledgedLosses = lost;
                    }

                    if (fromFile) { UpdateStatus(0, 0, "Updating assets database", "", false); }
                    Assets.UpdateDatabase(assetData);
                    if (fromFile) { UpdateStatus(0, 0, "", "Complete", false); }

                    // Set all 'for sale via contract' and 'in transit' assets in the database to processed.
                    // These types of assets would not be expected to show up in either the XML from the
                    // API or the list of current market orders.
                    // Any assets of these types that have been moved to a different state (e.g. in transit 
                    // items that have arrived or contracts that have expired) will have been updated already 
                    // in this method or ProcessSellOrders.
                    // Therefore, the ones that are left are still in the same situation as before.
                    // i.e. either 'for sale via contract' or 'in transit'.
                    // We set them to processed to prevent them from being removed along with other
                    // unprocessed assets.
                    Assets.SetProcessedFlag(_charID, corc == CharOrCorp.Corp, 
                        (int)AssetStatus.States.ForSaleViaContract, true);
                    Assets.SetProcessedFlag(_charID, corc == CharOrCorp.Corp, 
                        (int)AssetStatus.States.InTransit, true);                    
                    // Clear any assets remaining assets that have not been processed.
                    Assets.ClearUnProcessed(_charID, corc == CharOrCorp.Char, corc == CharOrCorp.Corp, false);
                    Assets.SetProcessedFlag(_charID, corc == CharOrCorp.Corp, 0, false);

                    if (fromFile)
                    {
                        UpdateStatus(0, 0, assetData.Count + " asset database entries modified.", "", false);
                    }

                    // Update the assets effective date setting.
                    // Also set the 'FirstUpdateDone' flag
                    if (corc == CharOrCorp.Char)
                    {
                        Settings.CharAssetsEffectiveDate = fileDate;
                        Settings.FirstUpdateDoneAssetsChar = true;
                    }
                    else
                    {
                        Settings.CorpAssetsEffectiveDate = fileDate;
                        if (!Settings.FirstUpdateDoneAssetsCorp)
                        {
                            Settings.FirstUpdateDoneAssetsCorp = true;
                            foreach (EVEAccount account in UserAccount.CurrentGroup.Accounts)
                            {
                                foreach (APICharacter character in account.Chars)
                                {
                                    if (character.CharID != _charID && character.CorpID == _corpID)
                                    {
                                        Settings.FirstUpdateDoneAssetsCorp = true;
                                    }
                                }
                            }
                        }
                    }


                    if (fromFile)
                    {
                        UpdateStatus(1, 1, "", "Complete", true);
                    }
                }
            }
            catch (Exception ex)
            {
                EMMAException emmaEx = ex as EMMAException;
                if (emmaEx == null)
                {
                    // If we've caught a standard exception rather than an EMMA one then log it be creating a 
                    // new exception.
                    // Note that we don't need to actually throw it..
                    emmaEx = new EMMAException(ExceptionSeverity.Error, "Error when adding assets" +
                        " from api" + (fromFile ? " file" : ""), ex);
                }

                if (fromFile)
                {
                    UpdateStatus(-1, -1, "Error", ex.Message, true);
                }
                else
                {
                    SetLastAPIUpdateError(corc, APIDataType.Assets, ex.Message);
                }
            }

            if (UpdateEvent != null)
            {
                if (_unacknowledgedLosses == null) { _unacknowledgedLosses = new AssetList(); }
                if (_unacknowledgedGains == null) { _unacknowledgedGains = new AssetList(); }
                if (_corpUnacknowledgedLosses == null) { _corpUnacknowledgedLosses = new AssetList(); }
                if (_corpUnacknowledgedGains == null) { _corpUnacknowledgedGains = new AssetList(); }

                if ((corc == CharOrCorp.Char && _unacknowledgedLosses.Count + _unacknowledgedGains.Count == 0) ||
                    (corc == CharOrCorp.Corp && _corpUnacknowledgedGains.Count + _corpUnacknowledgedLosses.Count == 0))
                {
                    UpdateEvent(this, new APIUpdateEventArgs(APIDataType.Assets,
                        corc == CharOrCorp.Char ? _charID : _corpID,
                        APIUpdateEventType.UpdateCompleted));
                }
                else
                {
                    SetLastAPIUpdateError(corc, APIDataType.Assets, "AWAITING ACKNOWLEDGEMENT");
                    UpdateEvent(this, new APIUpdateEventArgs(APIDataType.Assets,
                        corc == CharOrCorp.Char ? _charID : _corpID,
                        APIUpdateEventType.AssetsAwaitingAcknowledgement));
                }
            }
        }

        /// <summary>
        /// Recursive method to update the supplied assets data table based upon the supplied xml node list.
        /// </summary>
        /// <param name="assetData"></param>
        /// <param name="assetList"></param>
        /// <param name="locationID"></param>
        /// <param name="corc"></param>
        /// <param name="containerID"></param>
        /// <param name="expectedChanges"></param>
        private void UpdateAssets(EMMADataSet.AssetsDataTable assetData, XmlNodeList assetList, int locationID,
            CharOrCorp corc, long containerID, AssetList changes)
        {
            int counter = 0;
            if (containerID == 0)
            {
                UpdateStatus(counter, assetList.Count, "Getting asset data from file", "", false);
            }
            else
            {
                UpdateStatus(-1, -1, "Getting asset data from file", "", false, 
                    counter, assetList.Count, "Container progress");
            }

            foreach (XmlNode asset in assetList)
            {
                int itemID, quantity;
                long assetID = 0, eveInstanceID;
                bool isContainer = false, needNewRow = false;

                XmlNode locationNode = asset.SelectSingleNode("@locationID");
                if (locationNode != null)
                {
                    locationID = int.Parse(locationNode.Value);

                    // Translate location ID from a corporate office to a station ID if required.
                    if (locationID >= 66000000 && locationID < 67000000)
                    {
                        // NPC station.
                        locationID -= 6000001;
                    }
                    if (locationID >= 67000000 && locationID < 68000000)
                    {
                        // Conquerable station.
                        locationID -= 6000000;
                    }
                }
                itemID = int.Parse(asset.SelectSingleNode("@typeID").Value,
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                eveInstanceID = int.Parse(asset.SelectSingleNode("@itemID").Value,
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                quantity = int.Parse(asset.SelectSingleNode("@quantity").Value,
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                if (asset.LastChild != null && asset.LastChild.Name.Equals("rowset"))
                {
                    isContainer = true;
                }

                EMMADataSet.AssetsRow assetRow;
                needNewRow = true;

                // Note that if a match is not found for teh specific eve instance ID we pass in then
                // EMMA will automatically search for an asset matching all the other parameters.
                if (Assets.AssetExists(assetData, _charID, corc == CharOrCorp.Corp, locationID,
                    itemID, (int)AssetStatus.States.Normal, containerID != 0, containerID, isContainer,
                    false, !isContainer, false, true, eveInstanceID, ref assetID))
                {
                    needNewRow = false;
                }

                Asset change = null;
                if (!needNewRow)
                {
                    assetRow = assetData.FindByID(assetID);

                    if (assetRow.Processed)
                    {
                        // Row is already in the database but has been processed so just add the current
                        // quantity to the row.
                        // (i.e. there are multiple stacks of the same item in the same location in-game
                        // EMMA merges these since we don't care how things are stacked and it makes
                        // things a little easier.)
                        assetRow.Quantity = assetRow.Quantity + quantity;

                        // Store the changes that are being made to the quantity of 
                        // items here. 
                        // This means that once the update processing is complete, we
                        // can try and work out where these items came from.
                        #region Remember changes to item quantities
                        changes.ItemFilter = "ID = " + assetRow.ID;
                        if (changes.FiltredItems.Count > 0)
                        {
                            change = (Asset)changes.FiltredItems[0];
                            change.Quantity += quantity;
                        }
                        else
                        {
                            change = new Asset(assetRow);
                            change.Quantity = quantity;
                            change.Processed = false;
                            changes.Add(change);
                        }
                        #endregion
                    }
                    else
                    {
                        if (assetRow.Quantity == quantity)
                        {
                            // The row already exists in the database and quantity is the same so
                            // set the processed flag on the database directly and remove the row 
                            // from the dataset without setting it to be deleted when the database 
                            // is updated.
                            // Note the processed flag MUST be set on the database for later routines
                            // to work correctly. (e.g. Assets.ProcessSellOrders)
                            Assets.SetProcessedFlag(assetID, true);
                            assetData.RemoveAssetsRow(assetRow);
                        }
                        else if (assetRow.Quantity != quantity)
                        {
                            // The row already exists in the database, has not yet been processed
                            // and the quantity does not match what we've got from the XML.

                            // Store the changes that are being made to the quantity of 
                            // items here. 
                            // This means that once the update processing is complete, we
                            // can try and work out where these items came from.
                            #region Remember changes to item quantities
                            //changes.ItemFilter = "ItemID = " + itemID + " AND LocationID = " + locationID +
                            //    " AND ContainerID = " + containerID;
                            //if (changes.FiltredItems.Count > 0)
                            //{
                            //    change = (Asset)changes.FiltredItems[0];
                            //    change.Quantity = quantity - assetRow.Quantity;
                            //}
                            //else
                            //{
                                change = new Asset(assetRow);
                                change.Quantity = quantity - assetRow.Quantity;
                                change.Processed = false;
                                changes.Add(change);
                            //}
                            #endregion

                            // All we need to do is update the quantity and set the processed flag.
                            assetRow.Quantity = quantity;
                            assetRow.Processed = true;
                            // Also set the processed flag on the database directly. This will
                            // stop us from picking up this row later on (e.g. Assets.ProcessSellOrders)
                            Assets.SetProcessedFlag(assetID, true);
                        }
                    }
                }
                else
                {
                    // The row does not currently exist in the database so we need to create it. 
                    assetRow = assetData.NewAssetsRow();
                    assetRow.OwnerID = _charID;
                    assetRow.CorpAsset = corc == CharOrCorp.Corp;
                    assetRow.ItemID = itemID;
                    assetRow.EveItemID = eveInstanceID;
                    assetRow.LocationID = locationID;
                    assetRow.Status = 1;
                    assetRow.Processed = true;
                    assetRow.AutoConExclude = false;
                    assetRow.ReprocExclude = false;
                    assetRow.Cost = 0;
                    assetRow.CostCalc = false;

                    int systemID = 0, regionID = 0;
                    if (locationID >= 30000000 && locationID < 40000000)
                    {
                        systemID = locationID;
                        EveDataSet.mapSolarSystemsRow system = SolarSystems.GetSystem(locationID);
                        if (system != null)
                        {
                            regionID = system.regionID;
                        }
                        else
                        {
                            new EMMAEveAPIException(ExceptionSeverity.Warning, "Asset row added with unknown " +
                                "solar system ID (" + locationID + ")");
                        }
                    }
                    else
                    {
                        EveDataSet.staStationsRow station = null;
                        //try
                        //{
                            station = Stations.GetStation(locationID);
                        //}
                        //catch (EMMADataMissingException) { }

                        if (station != null)
                        {
                            systemID = station.solarSystemID;
                            regionID = station.regionID;
                        }
                        else
                        {
                            new EMMAEveAPIException(ExceptionSeverity.Warning, "Asset row added with unknown " +
                                "station ID (" + locationID + ")");
                        }
                    }
                    assetRow.SystemID = systemID;
                    assetRow.RegionID = regionID;
                    assetRow.Quantity = quantity;
                    assetRow.ContainerID = containerID;
                    assetRow.IsContainer = isContainer;
                    if (isContainer)
                    {
                        // If this asset is a container and has child assets then we must add it to the
                        // database now and get the correct ID number.
                        // (Because IDs are assigned by the database itself)
                        assetID = Assets.AddRowToDatabase(assetRow);
                    }
                    else
                    {
                        // Otherwise, we can just add it to the data table to be stored later along with 
                        // everything else.
                        assetData.AddAssetsRow(assetRow);
                    }

                    // Store the changes that are being made to the quantity of 
                    // items here. 
                    // This means that once the update processing is complete, we
                    // can try and work out where these items came from.
                    #region Remember changes to item quantities
                    //changes.ItemFilter = "ItemID = " + itemID + " AND LocationID = " + locationID +
                    //    " AND ContainerID = " + containerID;
                    //if (changes.FiltredItems.Count > 0)
                    //{
                    //    change = (Asset)changes.FiltredItems[0];
                    //    change.Quantity = quantity;
                    //}
                    //else
                    //{
                        change = new Asset(assetRow);
                        if (isContainer) { change.ID = assetID; }
                        change.Quantity = quantity;
                        change.Processed = false;
                        changes.Add(change);

                    //}
                    #endregion
                }

                if (isContainer)
                {
                    XmlNodeList contained = asset.SelectNodes("rowset/row");
                    UpdateAssets(assetData, contained, locationID, corc, assetID, changes);
                }

                counter++;
                if (containerID == 0)
                {
                    UpdateStatus(counter, assetList.Count, "Getting asset data from file", "", false);
                }
                else
                {
                    UpdateStatus(-1, -1, "Getting asset data from file", "", false,
                        counter, assetList.Count, "Container progress");
                }
            }

            // This is me experimenting with a different method for doing this...

            /*AssetList newAssets = new AssetList();
            List<AssetAccessParams> accessParams = new List<AssetAccessParams>();
            accessParams.Add(new AssetAccessParams(_charID, corc == CharOrCorp.Char, corc == CharOrCorp.Corp));
            AssetList oldAssets = Assets.LoadAssets(accessParams, new List<int>(), 0, 0);

            foreach (XmlNode asset in assetList)
            {
                newAssets.Add(new Asset(asset, corc == CharOrCorp.Char ? _charID : _corpID, null));
            }

            foreach (Asset asset in newAssets)
            {
                // IndexOf uses the Equals method so we're matching by owner, location, item and
                // contents but NOT quantity.
                int oldAssetIndex = oldAssets.IndexOf(asset);
                if (oldAssetIndex != 0)
                {
                    Asset oldAsset = oldAssets[oldAssetIndex];
                    if (oldAsset.Processed)
                    {
                        // Row is already in the database but has been processed (i.e. there has already
                        // been another asset row in the XML with the same item, location, etc).
                        // Just add the current quantity to the row.
                        oldAsset.Quantity += asset.Quantity;
                    }
                    else
                    {
                        if (oldAsset.Quantity == asset.Quantity)
                        {
                            // The row already exists in the database and quantity is the same so
                            // just set the processed flag.
                            oldAsset.Processed = true;
                        }
                        else
                        {
                            // The row already exists in the database, has not yet been processed
                            // and the quantity does not match what we've got from the XML.
                            // All we need to do is update the quantity and set the processed flag.
                            oldAsset.Quantity = quantity;
                            oldAsset.Processed = true;
                        }
                    }
                }
                else
                {
                    // The row does not currently exist in the database so we need to create it. 
                    oldAssets.Add(asset);
                }
                
            }*/

        }

        /*private static void UpdateExpectedChanges(Dictionary<int, Dictionary<int, int>> expectedChanges, 
            int locationID, int itemID, int deltaQuantity)
        {
            if (!expectedChanges.ContainsKey(locationID))
            {
                expectedChanges.Add(locationID, new Dictionary<int, int>());
            }
            if (!expectedChanges[locationID].ContainsKey(itemID))
            {
                expectedChanges[locationID].Add(itemID, 0);
            }
            expectedChanges[locationID][itemID] = deltaQuantity;
        }*/
        #endregion

        #region Update Journal
        private void RetrieveAPIJournal(object param)
        {
            CharOrCorp corc = (CharOrCorp)param;

            // Note, the JournalAPIUpdateLock is used to make sure that journal updates for different
            // characters and corps cannot run at the same time.
            // This is to prevent primary key conflicts when two characters in the report group transfer
            // cash to each other.
            lock (Globals.JournalAPIUpdateLock)
            {
                SetLastAPIUpdateError(corc, APIDataType.Journal, "UPDATING");

                RetrieveJournal(corc, null, 0);
            }

            APICharacters.Store(this);
            if (GetLastAPIUpdateError(corc, APIDataType.Journal).Equals("UPDATING"))
            {
                SetLastAPIUpdateError(corc, APIDataType.Journal, "");
            }
        }

        public void RetrieveJournal(XmlDocument fileXML, short walletID)
        {
            DataImportParams parameters = new DataImportParams();
            parameters.corc = CharOrCorp.Char; // Does not matter if we choose char or corp.
            parameters.xmlData = fileXML;
            parameters.walletID = walletID;
            Thread t0 = new Thread(new ParameterizedThreadStart(RetrieveJournal));
            t0.Start(parameters);
        }

        private void RetrieveJournal(object parameters) 
        {
            // Just wait a moment to allow the caller to display the 
            // progress dialog if importing from a file.
            Thread.Sleep(200);
            DataImportParams data = (DataImportParams)parameters;
            RetrieveJournal(data.corc, data.xmlData, data.walletID);
        }
        
        /// <summary>
        /// Retrieves journal entries from the Eve API or the specified xml document and adds any not already present to the database.
        /// </summary>
        /// <param name="corc"></param>
        /// <param name="fileXML"></param>
        /// <returns>The number of rows added to the journal table.</returns>
        private int RetrieveJournal(CharOrCorp corc, XmlDocument fileXML, short defaultWalletID)
        {
            decimal beforeRefID = 0;
            bool moreEntriesToAdd = true;
            bool errTryAgain = false;
            int retVal = 0;
            int updatedEntries = 0;
            //int batchNo = 0;
            short walletID = defaultWalletID == 0 ? (corc == CharOrCorp.Corp ? 
                (short)1000 : (short)0) : defaultWalletID;
            DateTime earliestUpdate = GetLastAPIUpdateTime(corc, APIDataType.Journal).AddHours(1);
            EMMADataSet.JournalDataTable journalData = new EMMADataSet.JournalDataTable();
            bool fromFile = fileXML != null;
            long highestIDSoFar = _apiSettings.GetHighestID(corc, APIDataType.Journal);
            long highestID = 0;
            DateTime fileDate = DateTime.UtcNow;
            bool noData = true;
            DateTime ticker = DateTime.UtcNow.AddSeconds(-10);

            try
            {
                if (!fromFile & earliestUpdate.CompareTo(DateTime.UtcNow) > 0)
                {
                    throw new EMMAEveAPIException(ExceptionSeverity.Warning, 1000, "Cannot get more journal" +
                        " entries so soon after the last update. Wait until at least " + 
                        earliestUpdate.ToLongTimeString() + " before updating the journal.");
                }

                // Loop through this while there are more entries left to add
                while (moreEntriesToAdd)
                {
                    XmlNodeList journEntries = null;
                    XmlDocument xml = new XmlDocument();

                    // Retrieve journal data from the Eve API. A maximum of 1000 results are returned in one call.
                    // If more results need to be added to the database then the program will loop round to call this
                    // again with the 'beforeRefID' parameter set.
                    // Looping will stop when:
                    // A) Less than 1000 results are returned - this indicates no more will be accessible (only up to
                    //      1 week in the past is accessible through the API).
                    // B) We read a journal line that has the same or lower refID than the specified 'latestEntryID'.
                    //      This would indicate that we have hit data that is already in the EMMA database and can stop 
                    //      requesting more.
                    // C) An error 101 or 103 is returned from the API service. This indicates there are no more 
                    //      transactions to be returned. 
                    // Note: Any other error will cause an exception to be thrown.
                    try
                    {
                        if (fromFile)
                        {
                            UpdateStatus(0, 1, "Getting journal entries from file", "", false);
                            journEntries = EveAPI.GetResults(fileXML);
                            fileDate = EveAPI.GetJournalDataTime(fileXML);
                            UpdateStatus(1, 1, "", journEntries.Count + " entries found in file.", false);
                        }
                        else
                        {
                            // This funny little bit of code is trying to prevent the API from prematurely firing
                            // error 100's.
                            // It seems that the second request for journal data will somtimes arrive
                            // at the API server before it has completed processing the first request.
                            // In this case, the server recieves a request with a before ref ID when it does 
                            // not think that it's been queried with a before ref id of zero yet, causing
                            // an error 100 to occur.
                            // This ensures that we wait 2 seconds between requests on the same wallet.
                            TimeSpan sleepTime = new TimeSpan(0,0,2);
                            sleepTime = sleepTime - DateTime.UtcNow.Subtract(ticker);
                            if (sleepTime.TotalMilliseconds > 0) { Thread.Sleep((int)sleepTime.TotalMilliseconds); }
                            ticker = DateTime.UtcNow;

                            //UpdateStatus(0, 1, "Getting journal entries from Eve API" +
                            //    (Settings.CorpMode ? " for corp wallet " + walletID + "" : "") +
                            //    (beforeRefID == 0 ? "." : " where journal ref ID < " + beforeRefID + "."), "", false);

                            xml = EveAPI.GetXml(
                                EveAPI.URL_EveApiBase + 
                                (corc == CharOrCorp.Corp ? EveAPI.URL_JournCorpApi : EveAPI.URL_JournApi),
                                "userid=" + _userID + "&apiKey=" + _apiKey + "&characterID=" + _charID +
                                (walletID == 0 ? "" : "&accountKey=" + walletID) +
                                "&beforeRefID=" + beforeRefID);
                            journEntries = EveAPI.GetResults(xml);
                            fileDate = EveAPI.GetJournalDataTime(xml);

                            SetLastAPIUpdateTime(corc, APIDataType.Journal, DateTime.UtcNow);

                            // If we've been successfull in getting data and this is a corporate data request
                            // then make sure we've got access set to true;
                            if (corc == CharOrCorp.Corp)
                            {
                                Settings.CorpJournalAPIAccess = true;
                            }
                        }

                        //UpdateStatus(1, 1, "", journEntries.Count + " entries retrieved.", false);
                    }
                    catch (EMMAEveAPIException emmaApiEx)
                    {
                        #region API Error Handling
                        if (emmaApiEx.EveCode == 100)
                        {
                            // Error code 100 indicates that a 'beforeRefID' has been passed in when the
                            // api was not expecting it. If we know for sure that we've already called
                            // the api once then have to abandon the data we have got so far.
                            // (No idea why the API does this, it just happens from time to time)
                            if (journalData.Count != 0)
                            {
                                journEntries = null;
                                moreEntriesToAdd = false;
                                journalData = new EMMADataSet.JournalDataTable();
                                retVal = 0;
                                updatedEntries = 0;
                                highestID = 0;
                                SetLastAPIUpdateError(corc, APIDataType.Journal, "Eve API Error 100");
                            }
                            else
                            {
                                if (!fromFile)
                                {
                                    SetLastAPIUpdateError(corc, APIDataType.Journal, emmaApiEx.Message);
                                    moreEntriesToAdd = false;
                                }
                                else
                                {
                                    throw emmaApiEx;
                                }
                            }
                        }
                        else if (emmaApiEx.EveCode == 101 || emmaApiEx.EveCode == 103)
                        {
                            //UpdateStatus(0, 0, "Error", "No more entries available", false);
                            moreEntriesToAdd = false;

                            try
                            {
                                // If there is a cachedUntil tag, dont try and get data again until
                                // after it has expired.
                                XmlNode nextTime = xml.SelectSingleNode("/eveapi/cachedUntil");
                                XmlNode eveTime = xml.SelectSingleNode("/eveapi/currentTime");
                                TimeSpan difference = DateTime.UtcNow.Subtract(DateTime.Parse(eveTime.FirstChild.Value,
                                    System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat));
                                DateTime nextAllowed = DateTime.Parse(nextTime.FirstChild.Value,
                                    System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat).Add(difference);

                                SetLastAPIUpdateTime(corc, APIDataType.Journal, nextAllowed.Subtract(
                                    UserAccount.Settings.APIJournUpdatePeriod));
                                if (noData)
                                {
                                    SetLastAPIUpdateError(corc, APIDataType.Journal,
                                        "The Eve API reports that this data has already been retrieved, no update has occured.");
                                }
                            }
                            catch (Exception) { }

                        }
                        else if (emmaApiEx.EveCode == 102)
                        {
                            string err = emmaApiEx.EveDescription;
                            beforeRefID = decimal.Parse(err.Substring(err.IndexOf("[") + 1,
                                err.IndexOf("]") - err.IndexOf("[") - 1));
                            //UpdateStatus(0, 0, "Warning", "Data already retrieved in the last hour, " +
                            //   "attemtping to retrieve older entries anyway.", false);
                            journEntries = null;
                            errTryAgain = true;
                        }
                        else if (emmaApiEx.EveCode == 200)
                        {
                            // Security level not high enough
                            SetLastAPIUpdateError(corc, APIDataType.Journal,
                                "You must enter your FULL api key to retrieve financial data.\r\n" +
                                "Use the 'manage group' button to correct this.");
                            moreEntriesToAdd = false;
                        }
                        else if (emmaApiEx.EveCode == 206 || emmaApiEx.EveCode == 208 ||
                            emmaApiEx.EveCode == 209)
                        {
                            // Character does not have required corporate role.
                            Settings.CorpJournalAPIAccess = false;
                            SetAPIAutoUpdate(corc, APIDataType.Journal, false);
                            SetLastAPIUpdateError(corc, APIDataType.Journal, emmaApiEx.Message);
                            moreEntriesToAdd = false;
                        }
                        else
                        {
                            if (!fromFile)
                            {
                                SetLastAPIUpdateError(corc, APIDataType.Journal, emmaApiEx.Message);
                                moreEntriesToAdd = false;
                            }
                            else
                            {
                                throw emmaApiEx;
                            }
                        }
                        #endregion
                    }

                    if (journEntries != null && journEntries.Count > 0)
                    {
                        noData = false;
                        if (journEntries.Count < 1000 || fromFile) { moreEntriesToAdd = false; }

                        // Get the ID offsets of the first and last entries.
                        XmlNode firstEntry = journEntries[0];
                        XmlNode lastEntry = journEntries[journEntries.Count - 1];

                        long offset1 = 0, offset2 = 0;
                        bool alreadyDone = false;
                        EMMADataSet.JournalRow newRow1 =
                            BuildJournalEntry(journalData, firstEntry, 0, walletID, corc);
                        XmlNode entryIDNode = firstEntry.SelectSingleNode("@refID");
                        long fileMaxID = long.Parse(entryIDNode.Value,
                            System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                        if (fileMaxID > highestID) { highestID = fileMaxID; }

                        if (fileMaxID < highestIDSoFar - 500000000)
                        {
                            // If the highest ID in this import is less than the highest ID stored
                            // for this char/corp then we're probably dealing with a new generation
                            // so need to reset..
                            SetHighestID(corc, APIDataType.Journal, 0);
                            highestIDSoFar = 0;
                        }
                        else if (fileMaxID < highestIDSoFar && !fromFile)
                        {
                            alreadyDone = true;
                            moreEntriesToAdd = false;
                        }

                        if (!alreadyDone)
                        {
                            try
                            {
                                offset1 = JournalGenerations.GetOffset(newRow1, fileDate);

                                EMMADataSet.JournalRow newRow2 =
                                    BuildJournalEntry(journalData, lastEntry, 0, walletID, corc);
                                try
                                {
                                    offset2 = JournalGenerations.GetOffset(newRow2, fileDate);
                                }
                                catch (Exception ex)
                                {
                                    EMMAException emmaEx = ex as EMMAException;
                                    if (emmaEx == null)
                                    {
                                        // If we've caught a standard exception rather than an EMMA one then log it be creating a 
                                        // new exception.
                                        // Note that we don't need to actually throw it..
                                        emmaEx = new EMMAException(ExceptionSeverity.Error, "Error when adding journal data" +
                                            " from api" + (fromFile ? " file" : ""), ex);
                                    }
                                    journEntries = null;

                                    if (!fromFile)
                                    {
                                        SetLastAPIUpdateError(corc, APIDataType.Journal, ex.Message);
                                    }
                                    else
                                    {
                                        UpdateStatus(0, journEntries.Count, "Error calculating ID offset", ex.Message, false);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                EMMAException emmaEx = ex as EMMAException;
                                if (emmaEx == null)
                                {
                                    // If we've caught a standard exception rather than an EMMA one then log it be creating a 
                                    // new exception.
                                    // Note that we don't need to actually throw it..
                                    emmaEx = new EMMAException(ExceptionSeverity.Error, "Error when adding journal data" +
                                        " from api" + (fromFile ? " file" : ""), ex);
                                }

                                if (!fromFile)
                                {
                                    SetLastAPIUpdateError(corc, APIDataType.Journal, ex.Message);
                                }
                                else
                                {
                                    UpdateStatus(0, journEntries.Count, "Error calculating ID offset", ex.Message, false);
                                }
                                journEntries = null;
                            }

                            if (offset1 == offset2)
                            {
                                //batchNo++;
                                int batchPrg = 0;
                                if (journEntries != null)
                                {
                                    //UpdateStatus(0, journEntries.Count, "Processing entries batch " + batchNo, "", false);
                                    if (fromFile)
                                    {
                                        UpdateStatus(0, journEntries.Count, "Processing entries", "", false);
                                    }

                                    // Loop through the results returned from this call to the API and add the line 
                                    // to the data table.
                                    foreach (XmlNode journEntry in journEntries)
                                    {
                                        bool tryUpdate = false;
                                        long id = long.Parse(journEntry.SelectSingleNode("@refID").Value) + offset1;
                                        int recieverID = int.Parse(journEntry.SelectSingleNode("@ownerID2").Value);
                                        beforeRefID = id - offset1;

                                        if (beforeRefID > highestIDSoFar || fromFile)
                                        {
                                            if (Journal.EntryExists(journalData, id, recieverID))
                                            {
                                                tryUpdate = true;
                                            }
                                            else
                                            {
                                                EMMADataSet.JournalRow tmpRow = journalData.FindByIDRecieverID(id, recieverID);
                                                if (tmpRow == null)
                                                {
                                                    EMMADataSet.JournalRow newRow =
                                                        BuildJournalEntry(journalData, journEntry, offset1, walletID, corc);

                                                    journalData.AddJournalRow(newRow);
                                                    retVal++;

                                                    // This section searches the character and journal ref type tables 
                                                    // for the values used in this new journal entry.
                                                    // If they are not present in the tables then they are added.
                                                    #region Check other tables and add values if needed.
                                                    //try
                                                    //{
                                                    //    JournalRefTypes.GetReferenceDesc(newRow.TypeID);
                                                    //}
                                                    //catch (EMMADataException)
                                                    //{
                                                    //    JournalRefTypes.AddUnspecifiedRefType(newRow.TypeID);
                                                    //}
                                                    SortedList<int, string> entityIDs = new SortedList<int, string>();
                                                    entityIDs.Add(newRow.SenderID, journEntry.SelectSingleNode("@ownerName1").Value);
                                                    if (!entityIDs.ContainsKey(newRow.RecieverID))
                                                    {
                                                        entityIDs.Add(newRow.RecieverID, journEntry.SelectSingleNode("@ownerName2").Value);
                                                    }
                                                    foreach (KeyValuePair<int, string> checkName in entityIDs)
                                                    {
                                                        Names.AddName(checkName.Key, checkName.Value);
                                                    }
                                                    #endregion
                                                }
                                                else
                                                {
                                                    tryUpdate = true;
                                                }
                                            }

                                            if (tryUpdate)
                                            {
                                                EMMADataSet.JournalRow newRow =
                                                    BuildJournalEntry(journalData, journEntry, offset1, walletID, corc);
                                                EMMADataSet.JournalRow oldRow = journalData.FindByIDRecieverID(newRow.ID,
                                                        newRow.RecieverID);
                                                bool updated = false;

                                                if ((newRow.RBalance > 0 && oldRow.RBalance == 0) ||
                                                    (newRow.RCorpID != 0 && oldRow.RCorpID == 0))
                                                {
                                                    oldRow.RBalance = newRow.RBalance;
                                                    oldRow.RCorpID = newRow.RCorpID;
                                                    oldRow.RArgID = newRow.RArgID;
                                                    oldRow.RArgName = newRow.RArgName;
                                                    oldRow.RWalletID = newRow.RWalletID;
                                                    updated = true;
                                                }
                                                if ((newRow.SBalance > 0 && oldRow.SBalance == 0) ||
                                                    (newRow.SCorpID != 0 && oldRow.SCorpID == 0))
                                                {
                                                    oldRow.SBalance = newRow.SBalance;
                                                    oldRow.SCorpID = newRow.SCorpID;
                                                    oldRow.SArgID = newRow.SArgID;
                                                    oldRow.SArgName = newRow.SArgName;
                                                    oldRow.SWalletID = newRow.SWalletID;
                                                    updated = true;
                                                }

                                                if (updated)
                                                {
                                                    updatedEntries++;
                                                }
                                            }

                                        }

                                        batchPrg++;

                                        if (fromFile)
                                        {
                                            UpdateStatus(batchPrg, journEntries.Count, "", "", false);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                throw new EMMAEveAPIException(ExceptionSeverity.Error, "It appears that the journal" +
                                        " data in this file is from two different ID generations.");
                            }

                        }

                    }
                    else
                    {
                        if (!errTryAgain)
                        {
                            moreEntriesToAdd = false;
                        }
                        else
                        {
                            errTryAgain = false;
                        }
                    }

                    if (!moreEntriesToAdd && highestID > highestIDSoFar && !fromFile)
                    {
                        SetHighestID(corc, APIDataType.Journal, highestID);
                    }

                    if (!moreEntriesToAdd && corc == CharOrCorp.Corp && walletID < 1006 && !fromFile)
                    {
                        walletID++;
                        beforeRefID = 0;
                        moreEntriesToAdd = true;
                        ticker = DateTime.UtcNow.AddSeconds(-10);
                    }

                    if (fromFile)
                    {
                        UpdateStatus(0, 0, retVal + " journal entries added to database.", "", false);
                        UpdateStatus(0, 0, updatedEntries + " existing journal entries updated.", "", true);
                    }
                }

                if (journalData.Count > 0)
                {
                    Journal.Store(journalData);
                }
            }
            catch (Exception ex)
            {
                EMMAException emmaEx = ex as EMMAException;
                if (emmaEx == null)
                {
                    // If we've caught a standard exception rather than an EMMA one then log it be creating a 
                    // new exception.
                    // Note that we don't need to actually throw it..
                    emmaEx = new EMMAException(ExceptionSeverity.Error, "Error when adding journal data" +
                        " from api" + (fromFile ? " file" : ""), ex);
                }
                
                if (!fromFile)
                {
                    SetLastAPIUpdateError(corc, APIDataType.Journal, ex.Message);
                }
                else
                {
                    UpdateStatus(-1, 0, "Error", ex.Message, true);
                }
            }

            if (UpdateEvent != null)
            {
                UpdateEvent(this, new APIUpdateEventArgs(APIDataType.Journal, 
                    corc == CharOrCorp.Char ? _charID : _corpID,
                    APIUpdateEventType.UpdateCompleted));
            }

            return retVal;
        }

        public EMMADataSet.JournalRow BuildJournalEntry(EMMADataSet.JournalDataTable journalData,
            XmlNode journEntry, long IDOffset, short walletID, CharOrCorp corc)
        {
            EMMADataSet.JournalRow retVal = null;
            XmlNode entryIDNode = journEntry.SelectSingleNode("@refID");
            long entryID = long.Parse(entryIDNode.Value,
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);

            // Actually create the line and add it to the data table
            retVal = journalData.NewJournalRow();
            retVal.ID = entryID + IDOffset;
            retVal.Date = DateTime.Parse(journEntry.SelectSingleNode("@date").Value);
            retVal.TypeID = short.Parse(journEntry.SelectSingleNode("@refTypeID").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            retVal.SenderID = int.Parse(journEntry.SelectSingleNode("@ownerID1").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            retVal.RecieverID = int.Parse(journEntry.SelectSingleNode("@ownerID2").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            decimal amount = decimal.Parse(journEntry.SelectSingleNode("@amount").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            retVal.Amount = Math.Abs(amount);
            if (amount < 0)
            {
                retVal.SBalance = decimal.Parse(journEntry.SelectSingleNode("@balance").Value,
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                retVal.SWalletID = walletID == 0 ? (short)1000 : walletID;
                retVal.SArgName = journEntry.SelectSingleNode("@argName1").Value;
                retVal.SArgID = int.Parse(journEntry.SelectSingleNode("@argID1").Value,
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                retVal.SCorpID = corc == CharOrCorp.Corp ? _corpID : 0;

                retVal.RBalance = 0;
                retVal.RWalletID = 0;
                retVal.RArgName = "";
                retVal.RArgID = 0;
                retVal.RCorpID = 0;
            }
            else
            {
                retVal.RBalance = decimal.Parse(journEntry.SelectSingleNode("@balance").Value,
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                retVal.RWalletID = walletID == 0 ? (short)1000 : walletID;
                retVal.RArgName = journEntry.SelectSingleNode("@argName1").Value;
                retVal.RArgID = int.Parse(journEntry.SelectSingleNode("@argID1").Value,
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                retVal.RCorpID = corc == CharOrCorp.Corp ? _corpID : 0;

                retVal.SBalance = 0;
                retVal.SWalletID = 0;
                retVal.SArgName = "";
                retVal.SArgID = 0;
                retVal.SCorpID = 0;
            }
            // Reason text can be longer than 50 chars so truncate it if needed...
            string reason = journEntry.SelectSingleNode("@reason").Value;
            retVal.Reason = (reason.Length > 50 ? reason.Remove(50) : reason);

            return retVal;
        }
        #endregion

        #region Update Transactions
        private void RetrieveAPITrans(object param)
        {
            CharOrCorp corc = (CharOrCorp)param;
            // The TransactionAPIUpdateLock is used to make sure that transaction updates for different
            // characters and corps cannot run at the same time.
            // This is to prevent primary key conflicts when two characters in the report group transfer
            // cash to each other.
            lock (Globals.TransactionAPIUpdateLock)
            {
                // Note, the sync lock is used to make sure that a transaction, assets or orders update do
                // not run at the same time for a character. 
                lock (_syncLock)
                {
                    SetLastAPIUpdateError(corc, APIDataType.Transactions, "UPDATING");

                    RetrieveTrans(corc, null, 0);
                }
            }

            APICharacters.Store(this);
            if (GetLastAPIUpdateError(corc, APIDataType.Transactions).Equals("UPDATING"))
            {
                SetLastAPIUpdateError(corc, APIDataType.Transactions, "");
            }
        }

        public void RetrieveTrans(XmlDocument fileXML, CharOrCorp corc, short walletID)
        {
            DataImportParams parameters = new DataImportParams();
            parameters.corc = corc;
            parameters.xmlData = fileXML;
            parameters.walletID = walletID;
            Thread t0 = new Thread(new ParameterizedThreadStart(RetrieveTrans));
            t0.Start(parameters);
        }

        private void RetrieveTrans(object parameters)
        {
            // Just wait a moment to allow the caller to display the 
            // progress dialog if importing from a file.
            Thread.Sleep(200);
            DataImportParams data = (DataImportParams)parameters;
            RetrieveTrans(data.corc, data.xmlData, data.walletID);
        }


        /// <summary>
        /// Update the database transactions table with data from the Eve API.
        /// </summary>
        /// <param name="corc"></param>
        /// <param name="fileXML"></param>
        /// <returns></returns>
        private int RetrieveTrans(CharOrCorp corc, XmlDocument fileXML, short defaultWalletID)
        {
            decimal beforeTransID = 0;
            bool moreEntriesToAdd = true;
            bool errTryAgain = false;
            int retVal = 0;
            short walletID = (short)(defaultWalletID == 0 ? (corc == CharOrCorp.Corp ? 1000 : 0) : defaultWalletID);
            DateTime earliestUpdate = GetLastAPIUpdateTime(corc, APIDataType.Transactions).AddHours(1);
            EMMADataSet.TransactionsDataTable transData = new EMMADataSet.TransactionsDataTable();
            bool fromFile = fileXML != null;
            long highestIDSoFar = _apiSettings.GetHighestID(corc, APIDataType.Transactions);
            long highestID = 0;
            DateTime ticker = DateTime.UtcNow.AddSeconds(-10);

            try
            {
                int updated = 0;

                if (earliestUpdate.CompareTo(DateTime.UtcNow) > 0 && !fromFile)
                {
                    throw new EMMAEveAPIException(ExceptionSeverity.Warning, 1000, "Cannot get more transactions " +
                        " so soon after the last update. Wait until at least " + earliestUpdate.ToLongTimeString() +
                        " before updating transactions.");
                }

                // Loop through this while there are more entries left to add
                while (moreEntriesToAdd)
                {
                    XmlNodeList transEntries = null;
                    XmlDocument xml = new XmlDocument();

                    // Retrieve transaction data from the Eve API. A maximum of 1000 results are returned in one call.
                    // If more results need to be added to the database then the program will loop round to call this
                    // again with the 'beforeRefID' parameter set.
                    // Looping will stop when:
                    // A) Less than 1000 results are returned - this indicates no more will be accessible (only up to
                    //      1 week in the past is accessible through the API).
                    // B) We read a transaction line that has the same or lower refID than the specified 'latestTransID'.
                    //      This would indicate that we have hit data that is already in the EMMA database and can stop 
                    //      requesting more.
                    // C) An error 101 or 103 is returned from the API service. This indicates there are no more 
                    //      transactions to be returned.
                    // Note: Any other error will cause an exception to be thrown.
                    try
                    {
                        Settings.CorpTransactionsAPIAccess = true;
                        if (fromFile)
                        {
                            UpdateStatus(0, 1, "Getting transactions from file", "", false);
                            transEntries = EveAPI.GetResults(fileXML);
                            UpdateStatus(1, 1, "", transEntries.Count + " entries found in file.", false);
                        }
                        else
                        {
                            // This funny little bit of code is trying to prevent the API from prematurely firing
                            // error 100's.
                            // It seems that the second request for transaction data will somtimes arrive
                            // at the API server before it has completed processing the first request.
                            // In this case, the server recieves a request with a before trans ID when it does 
                            // not think that it's been queried with a before trans id of zero yet, causing
                            // an error 100 to occur.
                            TimeSpan sleepTime = new TimeSpan(0, 0, 2);
                            sleepTime = sleepTime - DateTime.UtcNow.Subtract(ticker);
                            if (sleepTime.TotalMilliseconds > 0) { Thread.Sleep((int)sleepTime.TotalMilliseconds); }
                            ticker = DateTime.UtcNow;

                            xml = EveAPI.GetXml(
                                EveAPI.URL_EveApiBase +
                                (corc == CharOrCorp.Corp ? EveAPI.URL_TransCorpApi : EveAPI.URL_TransApi),
                                "userid=" + _userID + "&apiKey=" + _apiKey + "&characterID=" + _charID +
                                (walletID == 0 ? "" : "&accountKey=" + walletID) +
                                "&beforeTransID=" + beforeTransID);
                            transEntries = EveAPI.GetResults(xml);

                            // Set last transaction update time to now.
                            SetLastAPIUpdateTime(corc, APIDataType.Transactions, DateTime.UtcNow);

                            // If we've been successfull in getting data and this is a corporate data request
                            // then make sure we've got access set to true;
                            if (corc == CharOrCorp.Corp)
                            {
                                Settings.CorpTransactionsAPIAccess = true;
                            }
                        }

                    }
                    catch (EMMAEveAPIException emmaApiEx)
                    {
                        #region API Error Handing
                        if (emmaApiEx.EveCode == 100)
                        {
                            // Error code 100 indicates that a 'beforeTransID' has been passed in when the
                            // api was not expecting it. If we know for sure that we've already called
                            // the api once then have to abandon the data we have got so far.
                            // (No idea why the API does this, it just happens from time to time)
                            if (transData.Count != 0)
                            {
                                transEntries = null;
                                moreEntriesToAdd = false;
                                transData = new EMMADataSet.TransactionsDataTable();
                                retVal = 0;
                                updated = 0;
                                highestID = 0;
                                SetLastAPIUpdateError(corc, APIDataType.Transactions, "Eve API Error 100");
                            }
                            else
                            {
                                if (!fromFile)
                                {
                                    SetLastAPIUpdateError(corc, APIDataType.Transactions, emmaApiEx.Message);
                                    moreEntriesToAdd = false;
                                }
                                else
                                {
                                    throw emmaApiEx;
                                }
                            }
                        }
                        else if (emmaApiEx.EveCode == 101 || emmaApiEx.EveCode == 103)
                        {
                            // Wallet exhausted, retry only after specified time. 
                            moreEntriesToAdd = false;

                            try
                            {
                                // If there is a cachedUntil tag, dont try and get data again until
                                // after it has expired.
                                XmlNode nextTime = xml.SelectSingleNode("/eveapi/cachedUntil");
                                XmlNode eveTime = xml.SelectSingleNode("/eveapi/currentTime");
                                TimeSpan difference = DateTime.UtcNow.Subtract(DateTime.Parse(eveTime.FirstChild.Value,
                                    System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat));
                                DateTime nextAllowed = DateTime.Parse(nextTime.FirstChild.Value,
                                    System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat).Add(difference);

                                SetLastAPIUpdateTime(corc, APIDataType.Transactions,
                                    nextAllowed.Subtract(UserAccount.Settings.APITransUpdatePeriod));
                                if (transData.Count == 0)
                                {
                                    SetLastAPIUpdateError(corc, APIDataType.Transactions,
                                        "The Eve API reports that this data has already been retrieved, no update has occured.");
                                }
                            }
                            catch (Exception) { }
                        }
                        else if (emmaApiEx.EveCode == 102)
                        {
                            // API was expecting a different 'beforeTransID' value.
                            // get what it was expecting and try again using that value.
                            string err = emmaApiEx.EveDescription;
                            beforeTransID = Decimal.Parse(err.Substring(err.IndexOf("[") + 1,
                                err.IndexOf("]") - err.IndexOf("[") - 1),
                                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                            errTryAgain = true;
                            transEntries = null;
                        }
                        else if (emmaApiEx.EveCode == 200)
                        {
                            // Security level not high enough
                            SetLastAPIUpdateError(corc, APIDataType.Transactions, 
                                "You must enter your FULL api key to retrieve financial data.\r\n" +
                                "Use the 'manage group' button to correct this.");
                            moreEntriesToAdd = false;
                        }
                        else if (emmaApiEx.EveCode == 206 || emmaApiEx.EveCode == 208 ||
                            emmaApiEx.EveCode == 209)
                        {
                            // Character does not have required corporate role.
                            Settings.CorpTransactionsAPIAccess = false;
                            SetAPIAutoUpdate(corc, APIDataType.Transactions, false);
                            SetLastAPIUpdateError(corc, APIDataType.Transactions, emmaApiEx.Message);
                            moreEntriesToAdd = false;
                        }
                        else
                        {
                            if (!fromFile)
                            {
                                SetLastAPIUpdateError(corc, APIDataType.Transactions, emmaApiEx.Message);
                                moreEntriesToAdd = false;
                            }
                            else
                            {
                                throw emmaApiEx;
                            }
                        }
                        #endregion
                    }


                    if (transEntries != null && transEntries.Count > 0)
                    {
                        if (transEntries.Count < 1000 || fromFile) { moreEntriesToAdd = false; }

                        bool alreadyDone = false;
                        int batchPrg = 0;
                        if (fromFile)
                        {
                            UpdateStatus(0, transEntries.Count, "Processing transactions", "", false);
                        }

                        XmlNode entryIDNode = transEntries[0].SelectSingleNode("@transactionID");
                        long fileMaxID = long.Parse(entryIDNode.Value,
                            System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                        if (fileMaxID > highestID) { highestID = fileMaxID; }

                        if (fileMaxID < highestIDSoFar - 500000000)
                        {
                            // If the highest ID in this import is less than the highest ID stored
                            // for this char/corp - 1/2 a billion then we're probably dealing with 
                            // a new generation so need to reset..
                            SetHighestID(corc, APIDataType.Transactions, 0);
                            highestIDSoFar = 0;
                        }
                        else if (fileMaxID < highestIDSoFar && !fromFile)
                        {
                            alreadyDone = true;
                            moreEntriesToAdd = false;
                        }

                        if (!alreadyDone)
                        {
                            // Loop through the results returned from this call to the API and add the line to
                            // the data table if the transactionID is not already in the database.
                            foreach (XmlNode transEntry in transEntries)
                            {
                                XmlNode transIDNode = transEntry.SelectSingleNode("@transactionID");
                                long transID = long.Parse(transIDNode.Value,
                                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);

                                beforeTransID = transID;

                                if (beforeTransID > highestIDSoFar || fromFile)
                                {
                                    if (!Transactions.TransactionExists(transData, transID) &&
                                        transData.FindByID(transID) == null)
                                    {
                                        // Actually create the line and add it to the data table
                                        SortedList<int, string> nameIDs = new SortedList<int, string>();
                                        EMMADataSet.TransactionsRow newRow = BuildTransRow(transID, transData,
                                            transEntry, walletID, nameIDs);

                                        transData.AddTransactionsRow(newRow);
                                        retVal++;

                                        // This section searches the character, item and station ref type tables 
                                        // for the values used in this new transaction entry.
                                        // If they are not present in the table then they are added.
                                        #region Check other tables and add values if needed.
                                        foreach (KeyValuePair<int, string> checkName in nameIDs)
                                        {
                                            Names.AddName(checkName.Key, checkName.Value);
                                        }
                                        Items.AddItem(newRow.ItemID, transEntry.SelectSingleNode("@typeName").Value);
                                        #endregion
                                    }
                                    else
                                    {
                                        SortedList<int, string> nameIDs = new SortedList<int, string>();
                                        // We've got a transaction that already exists in the database,
                                        // update the row with additional data if available. 
                                        EMMADataSet.TransactionsRow newRow =
                                            BuildTransRow(transID, transData, transEntry, walletID, nameIDs);
                                        EMMADataSet.TransactionsRow oldRow = transData.FindByID(transID);
                                        bool updateDone = false;

                                        if (newRow.BuyerWalletID != oldRow.BuyerWalletID && newRow.BuyerWalletID != 0)
                                        {
                                            oldRow.BuyerWalletID = newRow.BuyerWalletID;
                                            updateDone = true;
                                        }
                                        if (newRow.SellerWalletID != oldRow.SellerWalletID && newRow.SellerWalletID != 0)
                                        {
                                            oldRow.SellerWalletID = newRow.SellerWalletID;
                                            updateDone = true;
                                        }
                                        // If a corp sells somthing to another corp (or itself) then we will get into 
                                        // the position of having the other party set as a character when in fact
                                        // it is that character's corp.
                                        // We check for this here and correct it if required.
                                        if (oldRow.BuyerID == _charID && newRow.BuyerID == _corpID)
                                        {
                                            oldRow.BuyerID = newRow.BuyerID;
                                            oldRow.BuyerCharacterID = newRow.BuyerCharacterID;
                                            oldRow.BuyerWalletID = newRow.BuyerWalletID;
                                            oldRow.BuyerForCorp = newRow.BuyerForCorp;
                                            updateDone = true;
                                        }
                                        if (oldRow.SellerID == _charID && newRow.SellerID == _corpID)
                                        {
                                            oldRow.SellerID = newRow.SellerID;
                                            oldRow.SellerCharacterID = newRow.SellerCharacterID;
                                            oldRow.SellerWalletID = newRow.SellerWalletID;
                                            oldRow.SellerForCorp = newRow.SellerForCorp;
                                            updateDone = true;
                                        }

                                        if (updateDone)
                                        {
                                            updated++;
                                        }
                                    }
                                }
                                else
                                {
                                    moreEntriesToAdd = false;
                                }

                                batchPrg++;
                                if (fromFile)
                                {
                                    UpdateStatus(batchPrg, transEntries.Count, "", "", false);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!errTryAgain)
                        {
                            moreEntriesToAdd = false;
                        }
                        else
                        {
                            errTryAgain = false;
                        }
                    }

                    if (!moreEntriesToAdd && highestID > highestIDSoFar && !fromFile)
                    {
                        SetHighestID(corc, APIDataType.Transactions, highestID);
                    }

                    if (!moreEntriesToAdd && corc == CharOrCorp.Corp && walletID < 1006 && !fromFile)
                    {
                        walletID++;
                        beforeTransID = 0;
                        moreEntriesToAdd = true;
                        ticker = DateTime.UtcNow.AddSeconds(-10);
                    }
                }

                if (fromFile)
                {
                    UpdateStatus(0, 0, retVal + " transactions added to database.", "", false);
                    UpdateStatus(0, 0, updated + " transactions updated.", "", false);
                }

                if (transData.Count > 0)
                {
                    Transactions.Store(transData);

                    //if (fromFile)
                    //{
                    //    UpdateStatus(0, 1, "Updating assets from new transactions", "", false);
                    //}

                    // Don't use this anymore, instead, assets are updated in the BuildTransRow method below
                    // or the Transactions.CalcProfit method that is called from there.

                    //long minID = (corc == CharOrCorp.Char ? Settings.CharAssetsTransUpdateID : Settings.CorpAssetsTransUpdateID);
                    //minID += 1;
                    //long maxID = 0;

                    // Need to update assets with the data from the transactions we've just added.
                    // (This is done because assets can only be updated once every 24 hours or so but
                    // transactions can be updated every hour, using those transactions to modify the 
                    // assets data allows EMMA to give a more up-to-date view)
                    //if (minID > 1)
                    //{
                    //    // If we've already updated assets from transactions since the last direct assets update
                    //    // then use the ID of the last transactions used to update the assets last time as
                    //    // the cutoff point.
                    //    maxID = Assets.UpdateFromTransactions(_charID, _corpID, corc == CharOrCorp.Corp, minID);
                    //}
                    //else
                    //{
                    //    // Otherwise, use the effective date of the last assets update as the cutoff point.
                    //    // i.e. update assets with any transactions after that date/time.
                    //    DateTime assetsValid = (corc == CharOrCorp.Char ? Settings.CharAssetsEffectiveDate : 
                    //        Settings.CorpAssetsEffectiveDate);
                    //    maxID = Assets.UpdateFromTransactions(_charID, _corpID, corc == CharOrCorp.Corp, assetsValid);
                    //}

                    //if (corc == CharOrCorp.Char) { Settings.CharAssetsTransUpdateID = maxID; }
                    //else { Settings.CorpAssetsTransUpdateID = maxID; }

                    if (fromFile)
                    {
                        UpdateStatus(1, 1, "", "Complete", true);
                    }
                }

            }
            catch (Exception ex)
            {
                EMMAException emmaEx = ex as EMMAException;
                if (emmaEx == null)
                {
                    // If we've caught a standard exception rather than an EMMA one then log it be creating a 
                    // new exception.
                    // Note that we don't need to actually throw it..
                    emmaEx = new EMMAException(ExceptionSeverity.Error, "Error when adding transactions" +
                        " from api" + (fromFile ? " file" : ""), ex);
                }

                if (!fromFile)
                {
                    SetLastAPIUpdateError(corc, APIDataType.Transactions, ex.Message);
                }
                else
                {
                    UpdateStatus(-1, 0, "Error", ex.Message, true);
                }
            }

            if (UpdateEvent != null)
            {
                UpdateEvent(this, new APIUpdateEventArgs(APIDataType.Transactions, 
                    corc == CharOrCorp.Char ? _charID : _corpID,
                    APIUpdateEventType.UpdateCompleted));
            }

            return retVal;
        }


        //private EMMADataSet.TransactionsRow BuildTransRow(long transID, EMMADataSet.TransactionsDataTable transData,
        //    XmlNode transEntry, SortedList<int, string> nameIDs, short walletID)
        private EMMADataSet.TransactionsRow BuildTransRow(long transID, EMMADataSet.TransactionsDataTable transData,
            XmlNode transEntry, short walletID, SortedList<int, string> nameIDs)
        {
            EMMADataSet.TransactionsRow newRow = transData.NewTransactionsRow();

            newRow.ID = transID;
            // Set the simple data. i.e. direct conversion from XML field to database field.
            newRow.DateTime = DateTime.Parse(transEntry.SelectSingleNode("@transactionDateTime").Value);
            newRow.Quantity = int.Parse(transEntry.SelectSingleNode("@quantity").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            newRow.ItemID = int.Parse(transEntry.SelectSingleNode("@typeID").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            newRow.Price = Decimal.Parse(transEntry.SelectSingleNode("@price").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            newRow.StationID = int.Parse(transEntry.SelectSingleNode("@stationID").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            newRow.RegionID = Stations.GetStation(newRow.StationID).regionID;

            // Get the data to work out the more complicated fields..
            string transType = transEntry.SelectSingleNode("@transactionType").Value;
            int clientID = int.Parse(transEntry.SelectSingleNode("@clientID").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            XmlNode node = transEntry.SelectSingleNode("@characterID");
            int charID = 0;
            if (node != null)
            {
                charID = int.Parse(node.Value,
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            bool forCorp =
                transEntry.SelectSingleNode("@transactionFor").Value.Equals("personal") ? false : true;

            newRow.CalcProfitFromAssets = false;
            if (transType.ToLower().Equals("buy"))
            {
                newRow.BuyerID = forCorp ? _corpID : _charID;
                newRow.BuyerForCorp = forCorp;
                newRow.BuyerCharacterID = forCorp ? _charID : 0;
                newRow.BuyerWalletID = (walletID == 0 ? (short)1000 : walletID);
                newRow.SellerID = clientID;
                newRow.SellerForCorp = charID != 0;
                newRow.SellerCharacterID = charID;
                newRow.SellerWalletID = 0;
                newRow.SellerUnitProfit = 0;
                // Update asset quantities.
                Assets.ChangeAssets(forCorp ? _corpID : _charID, forCorp, newRow.StationID, newRow.ItemID, 0,
                    (int)AssetStatus.States.Normal, false, newRow.Quantity, newRow.Price, true);
            }
            else
            {
                newRow.BuyerID = clientID;
                newRow.BuyerForCorp = charID != 0;
                newRow.BuyerCharacterID = charID;
                newRow.BuyerWalletID = 0;
                newRow.SellerID = forCorp ? _corpID : _charID;
                newRow.SellerForCorp = forCorp;
                newRow.SellerCharacterID = forCorp ? _charID : 0;
                newRow.SellerWalletID = (walletID == 0 ? (short)1000 : walletID);
                // Calculate transaction profit and update asset quantities.
                newRow.SellerUnitProfit = Transactions.CalcProfit(_charID, forCorp, transData, newRow);
            }

            // Get the IDs and associated names in this transaction.
            if (!nameIDs.ContainsKey(clientID))
            {
                nameIDs.Add(clientID, transEntry.SelectSingleNode("@clientName").Value);
            }
            if (charID != 0 && !nameIDs.ContainsKey(charID))
            {
                nameIDs.Add(charID, transEntry.SelectSingleNode("@characterName").Value);
            }

            return newRow;
        }

        #endregion

        #region Update Orders
        private void RetrieveAPIOrders(object param)
        {
            CharOrCorp corc = (CharOrCorp)param;

            // Note, the sync lock is used to make sure that a transaction, assets or orders update do
            // not run at the same time for a character. 
            lock (_syncLock)
            {
                SetLastAPIUpdateError(corc, APIDataType.Orders, "UPDATING");

                RetrieveOrders(corc, null);
            }

            APICharacters.Store(this);
            if (GetLastAPIUpdateError(corc, APIDataType.Orders).Equals("UPDATING"))
            {
                SetLastAPIUpdateError(corc, APIDataType.Orders, "");
            }
        }

        public void RetrieveOrders(XmlDocument fileXML, CharOrCorp corc)
        {
            DataImportParams parameters = new DataImportParams();
            parameters.corc = corc;
            parameters.xmlData = fileXML;
            parameters.walletID = 0; // No need to set wallet ID
            Thread t0 = new Thread(new ParameterizedThreadStart(RetrieveOrders));
            t0.Start(parameters);
        }

        private void RetrieveOrders(object parameters)
        {
            // Just wait a moment to allow the caller to display the 
            // progress dialog if importing from a file.
            Thread.Sleep(200);
            DataImportParams data = (DataImportParams)parameters;
            RetrieveOrders(data.corc, data.xmlData);
        }

        private void RetrieveOrders(CharOrCorp corc, XmlDocument fileXML)
        {
            DateTime earliestUpdate = GetLastAPIUpdateTime(corc, APIDataType.Orders).AddHours(1);
            EMMADataSet.OrdersDataTable orderData = new EMMADataSet.OrdersDataTable();
            bool fromFile = fileXML != null;
            int added = 0;
            int updated = 0;
            bool errorOccured = false;
            bool otherChars = true;
            int charIndex = 0;

            try
            {
                if (earliestUpdate.CompareTo(DateTime.UtcNow) > 0 && !fromFile)
                {
                    throw new EMMAEveAPIException(ExceptionSeverity.Warning, 1000, "Cannot update orders " +
                        " so soon after the last update. Wait until at least " + earliestUpdate.ToLongTimeString() +
                        " before updating orders.");
                }
                while(otherChars)
                {
                    APICharacter character = this;
                    int userID = _userID;
                    string apiKey = _apiKey;
                    int charID = _charID;
                    otherChars = false;

                    if (corc == CharOrCorp.Corp && !fromFile)
                    {
                        character = OtherCorpChars[charIndex];

                        userID = character.UserID;
                        apiKey = character.APIKey;
                        charID = character.CharID;
                        charIndex++;
                        if (OtherCorpChars.Count > charIndex) { otherChars = true; }
                    }
                    
                    XmlNodeList orderEntries = null;
                    XmlDocument xml = new XmlDocument();

                    // Retrieve orders from the Eve API or process the supplied XML. 
                    try
                    {
                        if (fromFile)
                        {
                            UpdateStatus(0, 1, "Getting orders from file", "", false);
                            orderEntries = EveAPI.GetResults(fileXML);
                            UpdateStatus(1, 1, "", orderEntries.Count + " orders found in file.", false);
                        }
                        else
                        {
                            xml = EveAPI.GetXml(
                                EveAPI.URL_EveApiBase +
                                (corc == CharOrCorp.Corp ? EveAPI.URL_CorpOrdersApi : EveAPI.URL_CharOrdersApi),
                                "userid=" + userID + "&apiKey=" + apiKey + "&characterID=" + charID +
                                "&version=2");
                            orderEntries = EveAPI.GetResults(xml);

                            // Set last orders update time to now.
                            character.SetLastAPIUpdateTime(corc, APIDataType.Orders, DateTime.UtcNow);

                            // If we've been successfull in getting data and this is a corporate data request
                            // then make sure we've got access set to true;
                            if (corc == CharOrCorp.Corp)
                            {
                                character.Settings.CorpOrdersAPIAccess = true;
                            }
                        }
                    }
                    catch (EMMAEveAPIException emmaApiEx)
                    {
                        errorOccured = true;
                        if (emmaApiEx.EveCode == 117)
                        {
                            try
                            {
                                // If there is a cachedUntil tag, dont try and get data again until
                                // after it has expired.
                                XmlNode nextTime = xml.SelectSingleNode("/eveapi/cachedUntil");
                                XmlNode eveTime = xml.SelectSingleNode("/eveapi/currentTime");
                                TimeSpan difference = DateTime.UtcNow.Subtract(DateTime.Parse(eveTime.FirstChild.Value,
                                    System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat));
                                DateTime nextAllowed = DateTime.Parse(nextTime.FirstChild.Value,
                                    System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat).Add(difference);

                                character.SetLastAPIUpdateTime(corc, APIDataType.Orders,
                                    nextAllowed.Subtract(UserAccount.Settings.APIOrderUpdatePeriod));
                                character.SetLastAPIUpdateError(corc, APIDataType.Orders,
                                    "The Eve API reports that this data has already been retrieved, no update has occured.");
                            }
                            catch (Exception) { }
                        }
                        else if (emmaApiEx.EveCode == 200)
                        {
                            // Security level not high enough
                            character.SetLastAPIUpdateError(corc, APIDataType.Orders,
                                "You must enter your FULL api key to retrieve financial data.\r\n" +
                                "Use the 'manage group' button to correct this.");
                        }
                        else if (emmaApiEx.EveCode == 206 || emmaApiEx.EveCode == 208 ||
                            emmaApiEx.EveCode == 209)
                        {
                            // Character does not have required corporate role.
                            character.Settings.CorpOrdersAPIAccess = false;
                            character.SetAPIAutoUpdate(corc, APIDataType.Orders, false);
                            character.SetLastAPIUpdateError(corc, APIDataType.Orders, emmaApiEx.Message);
                        }
                        else
                        {
                            if (!fromFile)
                            {
                                character.SetLastAPIUpdateError(corc, APIDataType.Orders, emmaApiEx.Message);
                            }
                            else
                            {
                                throw emmaApiEx;
                            }
                        }
                    }

                    if (orderEntries != null && orderEntries.Count > 0)
                    {
                        Orders.SetProcessed(charID, corc == CharOrCorp.Corp, false);

                        if (fromFile)
                        {
                            UpdateStatus(0, orderEntries.Count, "Processing orders", "", false);
                        }

                        foreach (XmlNode orderEntry in orderEntries)
                        {
                            EMMADataSet.OrdersRow orderRow = BuildOrdersRow(orderData, orderEntry, corc);
                            int id = 0;

                            if (!Orders.Exists(orderData, orderRow, ref id))
                            {
                                orderData.AddOrdersRow(orderRow);
                                added++;
                            }
                            else
                            {
                                EMMADataSet.OrdersRow oldRow = orderData.FindByID(id);

                                if (oldRow.TotalVol == orderRow.TotalVol &&
                                    oldRow.RemainingVol == orderRow.RemainingVol &&
                                    oldRow.MinVolume == orderRow.MinVolume && oldRow.Range == orderRow.Range &&
                                    oldRow.Duration == orderRow.Duration && oldRow.Escrow == orderRow.Escrow &&
                                    oldRow.Price == orderRow.Price && oldRow.OrderState == orderRow.OrderState)
                                {
                                    // If the order from the XML exactly matches what we have in the database
                                    // then just set the processed flag and remove it from the orderData table
                                    // without setting it to be removed from the database.
                                    Orders.SetProcessedByID(oldRow.ID, true);
                                    orderData.RemoveOrdersRow(oldRow);
                                }
                                else
                                {
                                    // Set the row to processed right now.
                                    oldRow.Processed = true;
                                    // Accept the changes to the row (will only be the processed flag at 
                                    // this point) and set the processed flag on the database.
                                    // This will prevent the row from being double matched with another
                                    // order later.
                                    // The 'accept changes' will prevent the concurency error that we 
                                    // would get if we only updated the processed flag on the database
                                    // side.
                                    oldRow.AcceptChanges();
                                    Orders.SetProcessedByID(oldRow.ID, true);
                                    
                                    // If the order was active and is now completed/expired then flag it for
                                    // the unacknowledged orders viewer to display.
                                    bool notify = false;
                                    notify = UserAccount.CurrentGroup.Settings.OrdersNotifyEnabled &&
                                        ((UserAccount.CurrentGroup.Settings.OrdersNotifyBuy && orderRow.BuyOrder) ||
                                        (UserAccount.CurrentGroup.Settings.OrdersNotifySell && !orderRow.BuyOrder));

                                    if (/*orderRow.RemainingVol == 0 &&*/
                                        orderRow.OrderState == (short)OrderState.ExpiredOrFilled &&
                                        (oldRow.OrderState == (short)OrderState.Active ||
                                        oldRow.OrderState == (short)OrderState.ExpiredOrFilled))
                                    {
                                        if (notify)
                                        {
                                            oldRow.OrderState = (short)OrderState.ExpiredOrFilledAndUnacknowledged;
                                            // No longer needed as the unacknowledged orders form is displayed/refreshed
                                            // as needed when refreshing the main form after an update is complete.
                                            //if (UpdateEvent != null)
                                            //{
                                            //    UpdateEvent(this, new APIUpdateEventArgs(APIDataType.Orders,
                                            //        corc == CharOrCorp.Corp ? _corpID : _charID,
                                            //        APIUpdateEventType.OrderHasExpiredOrCompleted));
                                            //}
                                        }
                                        else
                                        {
                                            oldRow.OrderState = (short)OrderState.ExpiredOrFilledAndAcknowledged;
                                        }
                                    }
                                    else if (orderRow.OrderState != (short)OrderState.ExpiredOrFilled)
                                    {
                                        oldRow.OrderState = orderRow.OrderState;
                                    }

                                    if (oldRow.TotalVol != orderRow.TotalVol ||
                                        oldRow.RemainingVol != orderRow.RemainingVol ||
                                        oldRow.MinVolume != orderRow.MinVolume || oldRow.Range != orderRow.Range ||
                                        oldRow.Duration != orderRow.Duration || oldRow.Escrow != orderRow.Escrow ||
                                        oldRow.Price != orderRow.Price)
                                    {
                                        oldRow.TotalVol = orderRow.TotalVol;
                                        oldRow.RemainingVol = orderRow.RemainingVol;
                                        oldRow.MinVolume = orderRow.MinVolume;
                                        oldRow.Range = orderRow.Range;
                                        oldRow.Duration = orderRow.Duration;
                                        oldRow.Escrow = orderRow.Escrow;
                                        oldRow.Price = orderRow.Price;
                                        // Note, only other fields are 'buyOrder' and 'issued'. Neither of which we want to change.
                                        updated++;
                                    }
                                }
                            }

                            if (fromFile)
                            {
                                UpdateStatus(added + updated, orderEntries.Count, "", "", false);
                            }

                        }
                    }

                    if (fromFile)
                    {
                        UpdateStatus(0, 0, added + " orders added to database.", "", false);
                        UpdateStatus(0, 0, updated + " orders updated.", "", true);
                    }

                    if (orderData.Count > 0)
                    {
                        Orders.Store(orderData);
                    }

                    if (!errorOccured)
                    {
                        Orders.FinishUnProcessed(charID, corc == CharOrCorp.Corp);
                    }
                }
            }
            catch (Exception ex)
            {
                EMMAException emmaEx = ex as EMMAException;
                if (emmaEx == null)
                {
                    // If we've caught a standard exception rather than an EMMA one then log it be creating a 
                    // new exception.
                    // Note that we don't need to actually throw it..
                    emmaEx = new EMMAException(ExceptionSeverity.Error, "Error when adding market orders" +
                        " from api" + (fromFile ? " file" : ""), ex);
                }

                if (!fromFile)
                {
                    SetLastAPIUpdateError(corc, APIDataType.Orders, ex.Message);
                }
                else
                {
                    UpdateStatus(-1, 0, "Error", ex.Message, true);
                }
            }


            if (UpdateEvent != null)
            {
                UpdateEvent(this, new APIUpdateEventArgs(APIDataType.Orders,
                    corc == CharOrCorp.Char ? _charID : _corpID,
                    APIUpdateEventType.UpdateCompleted));
            }

        }

        private EMMADataSet.OrdersRow BuildOrdersRow(EMMADataSet.OrdersDataTable orderData, XmlNode orderEntry,
            CharOrCorp corc)
        {
            EMMADataSet.OrdersRow newRow = orderData.NewOrdersRow();

            newRow.OwnerID = int.Parse(orderEntry.SelectSingleNode("@charID").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            newRow.ForCorp = corc == CharOrCorp.Corp;
            newRow.StationID = int.Parse(orderEntry.SelectSingleNode("@stationID").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            newRow.TotalVol = int.Parse(orderEntry.SelectSingleNode("@volEntered").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            newRow.RemainingVol = int.Parse(orderEntry.SelectSingleNode("@volRemaining").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            newRow.MinVolume = int.Parse(orderEntry.SelectSingleNode("@minVolume").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            newRow.OrderState = short.Parse(orderEntry.SelectSingleNode("@orderState").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            // We want to store 'Active' state code as 999, not 0. 
            if (newRow.OrderState == 0) { newRow.OrderState = (short)OrderState.Active; }
            newRow.ItemID = int.Parse(orderEntry.SelectSingleNode("@typeID").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            newRow.Range = short.Parse(orderEntry.SelectSingleNode("@range").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            newRow.WalletID = short.Parse(orderEntry.SelectSingleNode("@accountKey").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            newRow.Duration = short.Parse(orderEntry.SelectSingleNode("@duration").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            newRow.Escrow = decimal.Parse(orderEntry.SelectSingleNode("@escrow").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            newRow.Price = decimal.Parse(orderEntry.SelectSingleNode("@price").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            int buyOrder = int.Parse(orderEntry.SelectSingleNode("@bid").Value);
            newRow.BuyOrder = buyOrder == 1;
            newRow.Issued = DateTime.Parse(orderEntry.SelectSingleNode("@issued").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            newRow.Processed = true;

            return newRow;
        }
        #endregion
        #endregion

        #region Worker methods
        private void GetDataFromCharXML()
        {
            if (_charSheetXMLCache != null)
            {
                XmlNode charNameNode = _charSheetXMLCache.SelectSingleNode("/eveapi/result/name");
                if (charNameNode != null)
                {
                    _charName = charNameNode.LastChild.Value;
                }
                XmlNode corpNameNode = _charSheetXMLCache.SelectSingleNode("/eveapi/result/corporationName");
                if (corpNameNode != null)
                {
                    _corpName = corpNameNode.LastChild.Value;
                }
                XmlNode corpIDNode = _charSheetXMLCache.SelectSingleNode("/eveapi/result/corporationID");
                if (corpIDNode != null)
                {
                    int oldCorp = _corpID;
                    _corpID = int.Parse(corpIDNode.LastChild.Value,
                            System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    if (_corpID != oldCorp && oldCorp != 0)
                    {
                        Settings.FirstUpdateDoneAssetsCorp = false;
                        // We cannot do this here since this code is usually called when setting up the
                        // accounts within a group. I.e. the accounts collection is null.
                        // The only potential issue that might arrise is if character A changes corp,
                        // character B had already done assets updates for that corp and the
                        // user changes to use chanracter A for asset updates instead. In this case,
                        // EMMA would think it was updating for the first time and asset changes would
                        // be ignored. After this first update it would be back to normal though..
                        // Dosn't sound like a big deal really.
                        //foreach (EVEAccount account in UserAccount.CurrentGroup.Accounts)
                        //{
                        //    foreach (APICharacter character in account.Chars)
                        //    {
                        //        if (character.CharID != _charID && character.CorpID == _corpID)
                        //        {
                        //            if (character.Settings.FirstUpdateDoneAssetsCorp)
                        //            {
                        //                Settings.FirstUpdateDoneAssetsCorp = true;
                        //            }
                        //        }
                        //    }
                        //}
                    }
                }
                /*XmlNode accountingSkillNode = _charSheetXMLCache.SelectSingleNode(
                    "/eveapi/result/rowset[@name=\"skills\"]/row[@typeID=\"16622\"]");
                if (accountingSkillNode != null)
                {
                    _accountingLvl = int.Parse(accountingSkillNode.Attributes.GetNamedItem("level").Value);
                }
                XmlNode brokerRelSkillNode = _charSheetXMLCache.SelectSingleNode(
                    "/eveapi/result/rowset[@name=\"skills\"]/row[@typeID=\"3446\"]");
                if (brokerRelSkillNode != null)
                {
                    _brokerRelationsLvl = int.Parse(brokerRelSkillNode.Attributes.GetNamedItem("level").Value);
                }
                XmlNode marginTradingSkillNode = _charSheetXMLCache.SelectSingleNode(
                    "/eveapi/result/rowset[@name=\"skills\"]/row[@typeID=\"16597\"]");
                if (marginTradingSkillNode != null)
                {
                    _marginTradingLvl = int.Parse(marginTradingSkillNode.Attributes.GetNamedItem("level").Value);
                }*/
                XmlNodeList skillNodes = _charSheetXMLCache.SelectNodes("/eveapi/result/rowset[@name=\"skills\"]/row");
                _skills = new Dictionary<int,int>();
                foreach (XmlNode skillNode in skillNodes)
                {
                    try
                    {
                        int skillID = int.Parse(skillNode.Attributes.GetNamedItem("typeID").Value,
                            System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                        // Some skills (unpublished ones for example) don't have a 'level' attribute node.
                        XmlNode levelNode = skillNode.Attributes.GetNamedItem("level");
                        int skillLevel = 1;
                        if (levelNode != null)
                        {
                            skillLevel = int.Parse(skillNode.Attributes.GetNamedItem("level").Value,
                                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                        }

                        _skills.Add(skillID, skillLevel);
                    }
                    catch (Exception ex)
                    {
                        // Create a new exception to log the error but don't bother doing anything about it.
                        new EMMAException(ExceptionSeverity.Warning, "Problem getting skill node data", ex);
                    }
                }
            }
        }
        #endregion

        #region Corp methods

        public void RefreshCorpXMLFromAPI()
        {
            XmlDocument xml = null;
            if (DateTime.UtcNow.AddHours(-1).CompareTo(_corpWalletXMLLastUpdate) > 0)
            {
                xml = EveAPI.GetXml(EveAPI.URL_EveApiBase + EveAPI.URL_CorpDataApi,
                   "userid=" + _userID + "&apiKey=" + _apiKey + "&characterID=" + _charID);
            }
            else
            {
                xml = _corpSheetXMLCache;
            }

            if (xml != null)
            {
                try
                {
                    if (!_corpSheetXMLCache.Equals(xml))
                    {
                        // If there is some problem with the API or XML then this method will throw an exception.
                        // This will prevent _charList from being set as we don't want to put the wrong thing in it.
                        XmlNodeList results = EveAPI.GetResults(xml);
                        _corpSheetXMLCache = xml;
                        _corpSheetXMLLastUpdate = DateTime.UtcNow;
                    }
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {
                    GetDataFromCorpXML();
                }
            }

        }

        public EMMADataSet.WalletDivisionsDataTable WalletDivisions
        {
            get
            {
                if (_corpWalletDivisions == null)
                {
                    _corpWalletDivisions = new EMMADataSet.WalletDivisionsDataTable();
                }
                return _corpWalletDivisions;
            }
            //set { _corpName = value; }
        }
        #endregion

        #region Corp working methods
        private void GetDataFromCorpXML()
        {
            if (_corpSheetXMLCache != null)
            {
                _corpWalletDivisions = new EMMADataSet.WalletDivisionsDataTable();
                XmlNodeList walletDivisionNodes = _corpSheetXMLCache.SelectNodes("/eveapi/result/rowset[@name=\"walletDivisions\"]/row");

                if (walletDivisionNodes != null)
                {
                    foreach (XmlNode walletDivisionNode in walletDivisionNodes)
                    {
                        EMMADataSet.WalletDivisionsRow newDiv = _corpWalletDivisions.NewWalletDivisionsRow();
                        int walletID = int.Parse(walletDivisionNode.SelectSingleNode("@accountKey").Value,
                            System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                        string walletName = walletDivisionNode.SelectSingleNode("@description").Value;

                        newDiv.ID = walletID;
                        newDiv.Name = walletName;
                        _corpWalletDivisions.AddWalletDivisionsRow(newDiv);
                    }
                }

                XmlNode corpTagNode = _corpSheetXMLCache.SelectSingleNode("/eveapi/result/ticker");
                if (corpTagNode != null)
                {
                    _corpTag = corpTagNode.LastChild.Value;
                }

            }
        }
        #endregion

        #region Get report group level settings
        private void GetGroupLevelCharSettings()
        {
            if (UserAccount.CurrentGroup.ID != _charLastRptGroupID)
            {
                bool autoTrans = false, autoJournal = false, autoAssets = false, autoOrders = false;
                _charLastRptGroupID = UserAccount.CurrentGroup.ID;
                _charIncWithRptGroup = ReportGroups.GroupCharSettings(_charLastRptGroupID, _charID,
                    ref autoTrans, ref autoJournal, ref autoAssets, ref autoOrders);
                _apiSettings.SetAutoUpdateFlag(CharOrCorp.Char, APIDataType.Transactions, autoTrans);
                _apiSettings.SetAutoUpdateFlag(CharOrCorp.Char, APIDataType.Journal, autoJournal);
                _apiSettings.SetAutoUpdateFlag(CharOrCorp.Char, APIDataType.Orders, autoOrders);
                _apiSettings.SetAutoUpdateFlag(CharOrCorp.Char, APIDataType.Assets, autoAssets);
                _oldCharIncWithRptGroup = _charIncWithRptGroup;
            }
        }

        private void GetGroupLevelCorpSettings()
        {
            if (UserAccount.CurrentGroup.ID != _corpLastRptGroupID)
            {
                bool autoTrans = false, autoJournal = false, autoAssets = false, autoOrders = false; 
                _corpLastRptGroupID = UserAccount.CurrentGroup.ID;
                _corpIncWithRptGroup = ReportGroups.GroupCorpSettings(_corpLastRptGroupID, _corpID,
                    ref autoTrans, ref autoJournal, ref autoAssets, ref autoOrders, _charID);
                _apiSettings.SetAutoUpdateFlag(CharOrCorp.Corp, APIDataType.Transactions, autoTrans);
                _apiSettings.SetAutoUpdateFlag(CharOrCorp.Corp, APIDataType.Journal, autoJournal);
                _apiSettings.SetAutoUpdateFlag(CharOrCorp.Corp, APIDataType.Orders, autoOrders);
                _apiSettings.SetAutoUpdateFlag(CharOrCorp.Corp, APIDataType.Assets, autoAssets);
                _oldCorpIncWithRptGroup = _corpIncWithRptGroup;
            }
        }
        #endregion


        public bool CharHasCorporateAccess(APIDataType type)
        {
            bool retVal = true;
            switch (type)
            {
                case APIDataType.Transactions:
                    retVal = Settings.CorpTransactionsAPIAccess;
                    break;
                case APIDataType.Journal:
                    retVal = Settings.CorpJournalAPIAccess;
                    break;
                case APIDataType.Assets:
                    retVal = Settings.CorpAssetsAPIAccess;
                    break;
                case APIDataType.Orders:
                    retVal = Settings.CorpOrdersAPIAccess;
                    break;
                case APIDataType.Unknown:
                    break;
                case APIDataType.Full:
                    retVal = Settings.CorpTransactionsAPIAccess && Settings.CorpJournalAPIAccess &&
                        Settings.CorpAssetsAPIAccess && Settings.CorpOrdersAPIAccess;
                    break;
                default:
                    break;
            }

            return retVal;
        }


        // Used for passing parameters as a single object
        private struct DataImportParams
        {
            public CharOrCorp corc;
            public XmlDocument xmlData;
            public short walletID;
        }
    }

    public enum SettingsStoreType
    {
        Both,
        Corp,
        Char
    }

    public enum Skills : int
    {
        Refining = 3385,
        RefineryEfficiency = 3389,
        BrokerRelations = 3446,
        MarginTrading = 16597,
        Accounting = 16622,
        ArkonorProcessing = 12180,
        BistotProcessing = 12181,
        CrokiteProcessing = 12182,
        DarkOchreProcessing = 12183,
        GneissProcessing = 12184,
        HedbergiteProcessing = 12185,
        HemorphiteProcessing = 12186,
        JaspetProcessing = 12187,
        KerniteProcessing = 12188,
        MercoxitProcessing = 12189,
        OmberProcessing = 12190,
        PlagioclaseProcessing = 12191,
        PyroxeresProcessing = 12192,
        ScorditeProcessing = 12193,
        SpodumainProcessing = 12194,
        VeldsparProcessing = 12195,
        ScrapmetalProcessing = 12196
    }
}
