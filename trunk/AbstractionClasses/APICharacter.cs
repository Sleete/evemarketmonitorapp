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
using EveMarketMonitorApp.GUIElements.Interfaces;

namespace EveMarketMonitorApp.AbstractionClasses
{
    public class APICharacter : IProvideStatus
    {
        public event StatusChangeHandler StatusChange;

        private long _userID;
        private string _apiKey;

        private string _charName;
        private long _charID;

        private string _corpName;
        private long _corpID;
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

        private Queue<string> _unprocessedXMLFiles = new Queue<string>();
        private bool _processingQueue = false;
        private DateTime _lastQueueProcessingDT = new DateTime(2000, 1, 1);

        private int _downloadsInProgress = 0;
        private object _syncDownloadsInProg = new object();

        #region Public properties
        public long CharID
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

        public long CorpID
        {
            get { return _corpID; }
        }

        public long UserID
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

        public APICharacter(long userID, string apiKey, EMMADataSet.APICharactersRow data)
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
            // Note - don't need to set industry job update dates as they are stored in the settings xml
            // instead of directly on the character record.

            SetHighestID(CharOrCorp.Char, APIDataType.Transactions, data.HighestCharTransID);
            SetHighestID(CharOrCorp.Corp, APIDataType.Transactions, data.HighestCorpTransID);
            SetHighestID(CharOrCorp.Char, APIDataType.Journal, data.HighestCharJournalID);
            SetHighestID(CharOrCorp.Corp, APIDataType.Journal, data.HighestCorpJournalID);

            if (!Settings.UpdatedOwnerIDToCorpID)
            {
                UpdateOwnerIDToCorpID();
            }
        }

        public APICharacter(long userID, string apiKey, long charID)
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

            if (!Settings.UpdatedOwnerIDToCorpID)
            {
                UpdateOwnerIDToCorpID();
            }
        }


        /// <summary>
        /// This is called to migrate legacy corporate market orders and assets that were stored against
        /// the character ID rather than the corporate ID.
        /// </summary>
        public void UpdateOwnerIDToCorpID() 
        {
            if (!Settings.UpdatedOwnerIDToCorpID)
            {
                try
                {
                    Assets.MigrateAssetsToCorpID(_charID, _corpID);
                    Orders.MigrateOrdersToCorpID(_charID, _corpID);

                    Settings.UpdatedOwnerIDToCorpID = true;
                }
                catch (Exception ex) 
                {
                    EMMAException emmaEx = ex as EMMAException;
                    if (emmaEx == null)
                    {
                        throw new EMMAException(ExceptionSeverity.Critical,
                            "Problem when migrating data to new format", ex);
                    }
                }
            }
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
                    _apiSettings.GetAutoUpdateFlag(CharOrCorp.Char, APIDataType.Orders),
                    _apiSettings.GetAutoUpdateFlag(CharOrCorp.Char, APIDataType.IndustryJobs));
                _oldCharIncWithRptGroup = _charIncWithRptGroup;
            }
            if (type == SettingsStoreType.Corp | type == SettingsStoreType.Both)
            {
                ReportGroups.SetCorpGroupSettings(_corpLastRptGroupID, _corpID, _corpIncWithRptGroup,
                    _apiSettings.GetAutoUpdateFlag(CharOrCorp.Corp, APIDataType.Transactions),
                    _apiSettings.GetAutoUpdateFlag(CharOrCorp.Corp, APIDataType.Journal),
                    _apiSettings.GetAutoUpdateFlag(CharOrCorp.Corp, APIDataType.Assets),
                    _apiSettings.GetAutoUpdateFlag(CharOrCorp.Corp, APIDataType.Orders),
                    _apiSettings.GetAutoUpdateFlag(CharOrCorp.Corp, APIDataType.IndustryJobs),
                    _charID);
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

            if (DateTime.UtcNow.AddHours(-48).CompareTo(_charSheetXMLLastUpdate) > 0)
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
            long id = (corc == CharOrCorp.Char ? _charID : _corpID);


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


        public void DownloadXMLFromAPI(CharOrCorp corc, APIDataType type)
        {
            switch (type)
            {
                case APIDataType.Transactions:
                    SetLastAPIUpdateError(corc, type, "QUEUED");
                    ThreadPool.QueueUserWorkItem(RetrieveAPIXML, new APIUpdateInfo(corc, type));
                    break;
                case APIDataType.Journal:
                    SetLastAPIUpdateError(corc, type, "QUEUED");
                    ThreadPool.QueueUserWorkItem(RetrieveAPIXML, new APIUpdateInfo(corc, type));
                    break;
                case APIDataType.Assets:
                    SetLastAPIUpdateError(corc, type, "QUEUED");
                    ThreadPool.QueueUserWorkItem(RetrieveAPIXML, new APIUpdateInfo(corc, type));
                    break;
                case APIDataType.Orders:
                    SetLastAPIUpdateError(corc, type, "QUEUED");
                    ThreadPool.QueueUserWorkItem(RetrieveAPIXML, new APIUpdateInfo(corc, type));
                    break;
                case APIDataType.IndustryJobs:
                    SetLastAPIUpdateError(corc, type, "QUEUED");
                    ThreadPool.QueueUserWorkItem(RetrieveAPIXML, new APIUpdateInfo(corc, type));
                    break;
                case APIDataType.Unknown:
                    break;
                case APIDataType.Full:
                    SetLastAPIUpdateError(corc, APIDataType.Transactions, "QUEUED");
                    SetLastAPIUpdateError(corc, APIDataType.Orders, "QUEUED");
                    SetLastAPIUpdateError(corc, APIDataType.IndustryJobs, "QUEUED");
                    SetLastAPIUpdateError(corc, APIDataType.Journal, "QUEUED");
                    SetLastAPIUpdateError(corc, APIDataType.Assets, "QUEUED");
                    ThreadPool.QueueUserWorkItem(RetrieveAPIXML, new APIUpdateInfo(corc, APIDataType.Transactions));
                    ThreadPool.QueueUserWorkItem(RetrieveAPIXML, new APIUpdateInfo(corc, APIDataType.Orders));
                    ThreadPool.QueueUserWorkItem(RetrieveAPIXML, new APIUpdateInfo(corc, APIDataType.IndustryJobs));
                    ThreadPool.QueueUserWorkItem(RetrieveAPIXML, new APIUpdateInfo(corc, APIDataType.Journal));
                    ThreadPool.QueueUserWorkItem(RetrieveAPIXML, new APIUpdateInfo(corc, APIDataType.Assets));
                    break;
                default:
                    break;
            }
        }



        //public void UpdateDataFromAPI(CharOrCorp corc, APIDataType type)
        //{
        //    SetLastAPIUpdateError(corc, type, "QUEUED");

        //    switch (type)
        //    {
        //        case APIDataType.Transactions:
        //            ThreadPool.QueueUserWorkItem(RetrieveAPITrans, corc);
        //            break;
        //        case APIDataType.Journal:
        //            ThreadPool.QueueUserWorkItem(RetrieveAPIJournal, corc);
        //            break;
        //        case APIDataType.Assets:
        //            ThreadPool.QueueUserWorkItem(RetrieveAPIAssets, corc);
        //            break;
        //        case APIDataType.Orders:
        //            ThreadPool.QueueUserWorkItem(RetrieveAPIOrders, corc);
        //            break;
        //        default:
        //            break;
        //    }

        //}

        private void RetrieveAPIXML(object param)
        {
            APIUpdateInfo updateInfo = param as APIUpdateInfo;
            CharOrCorp corc = updateInfo.Corc;
            APIDataType type = updateInfo.Type;

            lock (_syncDownloadsInProg) { _downloadsInProgress++; }
            try
            {
                SetLastAPIUpdateError(corc, type, "DOWNLOADING");
                RetrieveAPIXML(corc, type);
            }
            finally
            {
                lock (_syncDownloadsInProg) { _downloadsInProgress--; }
            }

            try
            {
                APICharacters.Store(this);
            }
            catch (Exception) { }

            if (GetLastAPIUpdateError(corc, type).Equals("DOWNLOADING"))
            {
                SetLastAPIUpdateError(corc, type, "QUEUED");
            }
        }

        /// <summary>
        /// Download XML from the Eve API
        /// </summary>
        /// <param name="corc"></param>
        /// <param name="type"></param>
        private void RetrieveAPIXML(CharOrCorp corc, APIDataType type)
        {
            TimeSpan timeBetweenUpdates = UserAccount.Settings.GetAPIUpdatePeriod(type);
            DateTime earliestUpdate = GetLastAPIUpdateTime(corc, type).Add(timeBetweenUpdates);
            DateTime dataDate = DateTime.MinValue;

            short walletID = corc == CharOrCorp.Corp ? (short)1000 : (short)0;
            decimal beforeID = 0;
            bool finishedDownloading = false;
            bool walletExhausted = false;
            bool noData = true;
            bool abort = false;
            string xmlFile = "";
            XmlDocument xml = null;

            try
            {
                // Make sure we don't download if we've already done so recently.
                if (earliestUpdate.CompareTo(DateTime.UtcNow) > 0)
                {
                    throw new EMMAEveAPIException(ExceptionSeverity.Warning, 1000, "Cannot get " + 
                        type.ToString() + " data so soon after the last update. Wait until at least " + 
                        earliestUpdate.ToLongTimeString() + " before updating.");
                }

                while (!finishedDownloading)
                {
                    try
                    {
                        // Set parameters that will be passed to the API
                        #region Set parameters
                        StringBuilder parameters = new StringBuilder();
                        parameters.Append("userid=");
                        parameters.Append(_userID);
                        parameters.Append("&apiKey=");
                        parameters.Append(_apiKey);
                        parameters.Append("&characterID=");
                        parameters.Append(_charID);
                        if (type == APIDataType.Journal || type == APIDataType.Transactions)
                        {
                            if (walletID != 0)
                            {
                                parameters.Append("&accountKey=");
                                parameters.Append(walletID);
                            }
                            if (type == APIDataType.Journal) { parameters.Append("&beforeRefID="); }
                            if (type == APIDataType.Transactions) { parameters.Append("&beforeTransID="); }
                            parameters.Append(beforeID);
                        }
                        if (type == APIDataType.Assets || type == APIDataType.Orders)
                        {
                            parameters.Append("&version=2");
                        }
                        #endregion

                        xml = EveAPI.GetXml(EveAPI.URL_EveApiBase + EveAPI.GetURL(corc, type), 
                            parameters.ToString(), ref xmlFile);
                        XmlNodeList tmp = EveAPI.GetResults(xml);
                        if (xmlFile.Length > 0)
                        {
                            lock (_unprocessedXMLFiles) { _unprocessedXMLFiles.Enqueue(xmlFile); }
                            noData = false;
                        }
                        if (type == APIDataType.Journal || type == APIDataType.Transactions)
                        {
                            if (tmp.Count < 1000) { walletExhausted = true; }
                        }

                        // Set the last update time based upon the 'cached until'
                        // time rather than the actual time the update occured
                        // Note we could modify the API update period timer instead but
                        // that would cause other issues. It's better for the user if 
                        // we just do things this way.
                        if (type == APIDataType.Transactions)
                        {
                            // Transactions XML often gives a cache expiry date time that is too soon.
                            // If we try and update again when it says then it will fail so just wait
                            // for the usual 1 hour. (Or whatever the user has it set to)
                            SetLastAPIUpdateTime(corc, type, DateTime.UtcNow);
                        }
                        else
                        {
                            DateTime nextAllowed = EveAPI.GetCachedUntilTime(xml);
                            SetLastAPIUpdateTime(corc, type, nextAllowed.Subtract(
                                UserAccount.Settings.GetAPIUpdatePeriod(type)));
                        }

                        // If we've been successfull in getting data and this is a corporate data request
                        // then make sure we've got access set to true;
                        if (corc == CharOrCorp.Corp)
                        {
                            Settings.SetCorpAPIAccess(type, true);
                        }

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
                            if (!noData)
                            {
                                walletExhausted = true;
                                //SetLastAPIUpdateError(corc, type, "Eve API Error 100");
                            }
                            else
                            {
                                throw emmaApiEx;
                            }
                        }
                        else if (emmaApiEx.EveCode == 101 || emmaApiEx.EveCode == 102 ||
                            emmaApiEx.EveCode == 103 || emmaApiEx.EveCode == 116 || emmaApiEx.EveCode == 117)
                        {
                            // Data already retrieved
                            string err = emmaApiEx.EveDescription;

                            // If there is a cachedUntil tag, dont try and get data again until
                            // after it has expired.
                            DateTime nextAllowed = EveAPI.GetCachedUntilTime(xml);
                            SetLastAPIUpdateTime(corc, type, nextAllowed.Subtract(
                                UserAccount.Settings.GetAPIUpdatePeriod(type)));
                            if (noData)
                            {
                                SetLastAPIUpdateError(corc, type,
                                    "The Eve API reports that this data has already been retrieved, no update has occured.");
                            }
                            walletExhausted = true;
                        }
                        else if (emmaApiEx.EveCode == 200)
                        {
                            // Security level not high enough
                            SetLastAPIUpdateError(corc, type,
                                "You must enter your FULL api key to retrieve financial and asset data.\r\n" +
                                "Use the 'manage group' button to correct this.");
                            abort = true;
                        }
                        else if (emmaApiEx.EveCode == 206 || emmaApiEx.EveCode == 208 ||
                            emmaApiEx.EveCode == 209 || emmaApiEx.EveCode == 213)
                        {
                            // Character does not have required corporate role.
                            Settings.SetCorpAPIAccess(type, false);
                            SetAPIAutoUpdate(corc, type, false);
                            SetLastAPIUpdateError(corc, type, emmaApiEx.Message);
                            abort = true;
                        }
                        else
                        {
                            throw emmaApiEx;
                        }
                        #endregion
                    }


                    /// By default, we're now done..
                    finishedDownloading = true;

                    // However, for some update types, we'll want to go round a few more times
                    #region Determine if we should access API again with different variables
                    if (!abort)
                    {
                        if (type == APIDataType.Journal || type == APIDataType.Transactions)
                        {
                            XmlNode lastRowNode = xml.SelectSingleNode(@"/eveapi/result/rowset/row[last()]");
                            if (lastRowNode != null)
                            {
                                string idAttribName = "";
                                if (type == APIDataType.Journal) { idAttribName = "@refID"; }
                                if (type == APIDataType.Transactions) { idAttribName = "@transactionID"; }
                                beforeID = decimal.Parse(lastRowNode.SelectSingleNode(idAttribName).Value);
                            }
                            else { walletExhausted = true; }
                            if (!walletExhausted) { finishedDownloading = false; }
                            if (walletExhausted && corc == CharOrCorp.Corp && walletID < 1006)
                            {
                                walletID++;
                                beforeID = 0;
                                finishedDownloading = false;
                            }
                        }
                    }
                    #endregion
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
                    emmaEx = new EMMAException(ExceptionSeverity.Error,
                        "Error when downloading " + type.ToString() + " from Eve API", ex);
                }

                SetLastAPIUpdateError(corc, type, ex.Message);
                noData = true;
            }

            // If we have not retrieved any data at all then mark the update as completed.
            if (noData)
            {
                if (UpdateEvent != null)
                {
                    UpdateEvent(this, new APIUpdateEventArgs(type,
                        corc == CharOrCorp.Char ? _charID : _corpID,
                        APIUpdateEventType.UpdateCompleted));
                }
            }
        }


        public void ProcessQueuedXML()
        {
            if (!_processingQueue)
            {
                _processingQueue = true;
                // Don't process the queue more than once every 10 seconds.
                if (_lastQueueProcessingDT.AddSeconds(10).CompareTo(DateTime.UtcNow) < 0)
                {
                    _lastQueueProcessingDT = DateTime.UtcNow;

                    // Limit this loop to running for no more than 1/2 a second
                    while (_unprocessedXMLFiles.Count > 0 && 
                        _lastQueueProcessingDT.AddMilliseconds(500).CompareTo(DateTime.UtcNow) > 0)
                    {
                        bool process = true;
                        string xmlFile = "";
                        lock (_unprocessedXMLFiles) { xmlFile = _unprocessedXMLFiles.Dequeue(); }
                        // If the next file is an asset data file and we still have other files waiting
                        // or being downloaded then move the asset data file to the back of the queue.
                        if (xmlFile.ToUpper().Contains("ASSETS") &&
                            (_unprocessedXMLFiles.Count > 0 || _downloadsInProgress > 0))
                        {
                            if (_unprocessedXMLFiles.Count == 1 && 
                                _unprocessedXMLFiles.Peek().ToUpper().Contains("ASSETS"))
                            {
                                // If this character also gets the corp updates then there may be another 
                                // asset file waiting. If this is the case, allow the xml to be processed.
                            }
                            else
                            {
                                lock (_unprocessedXMLFiles) { _unprocessedXMLFiles.Enqueue(xmlFile); }
                                process = false;
                                // If only the single assets file is left and we're still waiting on other 
                                // files to download then just break out of the loop for now.
                                if (_unprocessedXMLFiles.Count == 1 && _downloadsInProgress > 0) { break; }
                            }
                        }

                        if (process)
                        {
                            DataImportParams parameters = new DataImportParams();
                            CharOrCorp corc = CharOrCorp.Char;
                            short walletID = 0;
                            if (xmlFile.ToUpper().Contains("CORP")) { corc = CharOrCorp.Corp; }
                            if (xmlFile.ToUpper().Contains("ACCOUNTKEY="))
                            {
                                walletID = short.Parse(xmlFile.Substring(
                                    xmlFile.ToUpper().IndexOf("ACCOUNTKEY=") + 11, 4));
                            }
                            parameters.corc = corc;
                            parameters.walletID = walletID;
                            parameters.xmlData = new XmlDocument();
                            parameters.xmlData.Load(xmlFile);


                            if (xmlFile.ToUpper().Contains("ASSETS"))
                            {
                                ThreadPool.QueueUserWorkItem(UpdateAssetsFromXML, parameters);
                            }
                            else if (xmlFile.ToUpper().Contains("TRANSACTIONS"))
                            {
                                ThreadPool.QueueUserWorkItem(UpdateTransactionsFromXML, parameters);
                            }
                            else if (xmlFile.ToUpper().Contains("JOURNAL ENTRIES"))
                            {
                                ThreadPool.QueueUserWorkItem(UpdateJournalFromXML, parameters);
                            }
                            else if (xmlFile.ToUpper().Contains("MARKET ORDERS"))
                            {
                                ThreadPool.QueueUserWorkItem(UpdateOrdersFromXML, parameters);
                            }
                            else if (xmlFile.ToUpper().Contains("INDUSTRY JOBS"))
                            {
                                ThreadPool.QueueUserWorkItem(UpdateIndustryJobsFromXML, parameters);
                            }
                        }
                    }

                    if (_unprocessedXMLFiles.Count > 0)
                    {
                        // If we weren't able to process everything then dump some diagnostics.
                        TimeSpan processingTime = DateTime.UtcNow.Subtract(_lastQueueProcessingDT);

                        StringBuilder message= new StringBuilder(
                            "Warning, not all queued XML files were processed.\r\n");
                        message.Append("Files not processed:\r\n");
                        foreach (string file in _unprocessedXMLFiles)
                        {
                            message.Append(file);
                            message.Append("\r\n");
                        }
                        message.Append("Downloads active: ");
                        message.Append(_downloadsInProgress);
                        message.Append("\r\nApprox processing time: ");
                        message.Append(processingTime.TotalMilliseconds);
                        message.Append(" milliseconds");

                        new EMMAException(ExceptionSeverity.Warning, message.ToString(), true);  
                    }
                }

                _processingQueue = false;
            }
        }

        #region Update Assets
        public void ProcessAssetXML(XmlDocument fileXml, CharOrCorp corc)
        {
            DataImportParams parameters = new DataImportParams();
            parameters.xmlData = fileXml;
            parameters.corc = corc;
            parameters.walletID = 0;
            ThreadPool.QueueUserWorkItem(UpdateAssetsFromXML, parameters);
        }

        private void UpdateAssetsFromXML(object parameters)
        {
            // Just wait a moment to allow the caller to display the 
            // progress dialog if importing from a file.
            Thread.Sleep(200);

            DataImportParams data = (DataImportParams)parameters;
            SetLastAPIUpdateError(data.corc, APIDataType.Assets, "UPDATING");

            lock (_syncLock)
            {
                UpdateAssetsFromXML(data.corc, data.xmlData);
            }

            APICharacters.Store(this);
            if (GetLastAPIUpdateError(data.corc, APIDataType.Assets).Equals("UPDATING"))
            {
                SetLastAPIUpdateError(data.corc, APIDataType.Assets, "");
            }
        }



        private void UpdateAssetsFromXML(CharOrCorp corc, XmlDocument xml)
        {
            DateTime earliestUpdate = GetLastAPIUpdateTime(corc, APIDataType.Assets).AddHours(23);
            EMMADataSet.AssetsDataTable assetData = new EMMADataSet.AssetsDataTable();
            DateTime dataDate = DateTime.MinValue;

            try
            {
                XmlNodeList assetList = null;

                UpdateStatus(0, 1, "Getting asset data from file", "", false);

                dataDate = EveAPI.GetCachedUntilTime(xml);
                dataDate = dataDate.AddHours(-23);
                DateTime assetsEffectiveDate = corc == CharOrCorp.Char ?
                    Settings.CharAssetsEffectiveDate : Settings.CorpAssetsEffectiveDate;
                if (dataDate.CompareTo(assetsEffectiveDate) < 0)
                {
                    UpdateStatus(1, 1, "Error", "This data in this file is from " + dataDate.ToString() +
                        ". EMMA has already imported asset data dated " + assetsEffectiveDate + " therefore the" +
                        " database will not be updated.", true);
                    assetList = null;
                }
                else
                {
                    assetList = EveAPI.GetResults(xml);
                    UpdateStatus(1, 1, "", assetList.Count + " asset data lines found.", false);
                }

                if (assetList != null)
                {
                    // Set the 'processed' flag to false for all of this char/corp's assets.
                    Assets.SetProcessedFlag(corc == CharOrCorp.Corp ? _corpID : _charID, (int)AssetStatus.States.Normal, false);
                    Assets.SetProcessedFlag(corc == CharOrCorp.Corp ? _corpID : _charID, (int)AssetStatus.States.ForSaleViaMarket, false);
                    Assets.SetProcessedFlag(corc == CharOrCorp.Corp ? _corpID : _charID, (int)AssetStatus.States.ForSaleViaContract, false);
                    Assets.SetProcessedFlag(corc == CharOrCorp.Corp ? _corpID : _charID, (int)AssetStatus.States.InTransit, false);

                    AssetList changes = new AssetList();

                    // Create an in-memory datatable with all of the changes required to the assets 
                    // database in order to reflect the data in the xml file.
                    UpdateAssets(assetData, assetList, 0, corc, 0, changes);
                    // Use the currently active sell order to account for assets that appear to be
                    // missing.
                    UpdateStatus(0, 0, "Processing active sell orders", "", false);
                    Assets.ProcessSellOrders(assetData, changes, corc == CharOrCorp.Corp ? _corpID : _charID);
                    UpdateStatus(0, 0, "", "Complete", false);
                    // Use transactions that occured after the effective date of the asset data file
                    // to ensure that the asset list is as up-to-date as possible.
                    UpdateStatus(0, 0, "Updating assets from transactions that occur after " +
                        "the asset file's effective date", "", false);
                    long maxID = Assets.UpdateFromTransactions(assetData, changes, _charID, _corpID,
                        corc == CharOrCorp.Corp, dataDate);
                    if (corc == CharOrCorp.Char) { Settings.CharAssetsTransUpdateID = maxID; }
                    else { Settings.CorpAssetsTransUpdateID = maxID; }
                    UpdateStatus(0, 0, "", "Complete", false);

                    AssetList gained = new AssetList();
                    AssetList lost = new AssetList();
                    if ((corc == CharOrCorp.Char && Settings.FirstUpdateDoneAssetsChar) ||
                        (corc == CharOrCorp.Corp && Settings.FirstUpdateDoneAssetsCorp))
                    {
                        UpdateStatus(0, 0, "Analysing changes to assets", "", false);
                        Assets.AnalyseChanges(assetData, corc == CharOrCorp.Corp ? _corpID : _charID,
                            changes, out gained, out lost);
                        UpdateStatus(0, 0, "", "Complete", false);
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

                    UpdateStatus(0, 0, "Updating assets database", "", false);
                    Assets.UpdateDatabase(assetData);
                    UpdateStatus(0, 0, "", "Complete", false);

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
                    Assets.SetProcessedFlag(corc == CharOrCorp.Corp ? _corpID : _charID,
                        (int)AssetStatus.States.ForSaleViaContract, true);
                    Assets.SetProcessedFlag(corc == CharOrCorp.Corp ? _corpID : _charID,
                        (int)AssetStatus.States.InTransit, true);
                    // Clear any remaining assets that have not been processed.
                    Assets.ClearUnProcessed(corc == CharOrCorp.Corp ? _corpID : _charID, false);
                    Assets.SetProcessedFlag(corc == CharOrCorp.Corp ? _corpID : _charID, 0, false);

                    UpdateStatus(0, 0, assetData.Count + " asset database entries modified.", "", false);

                    // Update the assets effective date setting.
                    // Also set the 'FirstUpdateDone' flag
                    if (corc == CharOrCorp.Char)
                    {
                        Settings.CharAssetsEffectiveDate = dataDate;
                        Settings.FirstUpdateDoneAssetsChar = true;
                    }
                    else
                    {
                        Settings.CorpAssetsEffectiveDate = dataDate;
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


                    UpdateStatus(1, 1, "", "Complete", true);
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
                    emmaEx = new EMMAException(ExceptionSeverity.Error, "Error when processing assets data", ex);
                }

                UpdateStatus(-1, -1, "Error", ex.Message, true);
                SetLastAPIUpdateError(corc, APIDataType.Assets, ex.Message);
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

                // Note that if a match is not found for the specific eve instance ID we pass in then
                // EMMA will automatically search for an asset matching all the other parameters.
                if (Assets.AssetExists(assetData, corc == CharOrCorp.Corp ? _corpID : _charID, locationID,
                    itemID, (int)AssetStatus.States.Normal, containerID != 0, containerID, isContainer,
                    false, !isContainer, false, true, eveInstanceID, ref assetID))
                {
                    needNewRow = false;
                }
                else if(!isContainer)
                {
                    // We havn't actually updated the database with anything yet so we may already have a
                    // matching item stack in memory but not in the database. Check for that here.
                    DataRow[] data =
                        assetData.Select("ItemID = " + itemID + " AND OwnerID = " + _charID + " AND CorpAsset = " +
                        (corc == CharOrCorp.Corp ? 1 : 0) + " AND LocationID = " + locationID +
                        " AND Status = " + (int)AssetStatus.States.Normal + " AND ContainerID = " + containerID +
                        " AND EveItemID = " + eveInstanceID);
                    if (data != null && data.Length > 0)
                    {
                        needNewRow = false;
                        assetID = ((EMMADataSet.AssetsRow)data[0]).ID;
                    }
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
                        // We're stacking multiple eve item instances so just set the eve item ID to zero.
                        assetRow.EveItemID = 0;

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
                            change.EveItemInstanceID = 0;
                            if (change.Quantity == 0) { changes.ItemFilter = ""; changes.Remove(change); }
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
                            if (assetRow.EveItemID != 0)
                            {
                                Assets.SetProcessedFlag(assetID, true);
                                assetData.RemoveAssetsRow(assetRow);
                            }
                            else
                            {
                                // If Eve instance ID is not yet set then set it.
                                Assets.SetProcessedFlag(assetID, true);
                                assetRow.Processed = true;
                                assetRow.EveItemID = eveInstanceID;
                            }
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
                            change = new Asset(assetRow);
                            change.Quantity = quantity - assetRow.Quantity;
                            change.EveItemInstanceID = eveInstanceID;
                            change.Processed = false;
                            changes.Add(change);
                            #endregion

                            // All we need to do is update the quantity and set the processed flag.
                            assetRow.Quantity = quantity;
                            assetRow.Processed = true;
                            assetRow.EveItemID = eveInstanceID;
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
                    //assetRow.OwnerID = _charID;
                    assetRow.OwnerID = corc == CharOrCorp.Corp ? _corpID : _charID;
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
                    assetRow.BoughtViaContract = false;

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
                        try
                        {
                            station = Stations.GetStation(locationID);
                        }
                        catch (EMMADataMissingException) { }

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
                    change = new Asset(assetRow);
                    if (isContainer) { change.ID = assetID; }
                    change.Quantity = quantity;
                    change.Processed = false;
                    changes.Add(change);
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

        }
        #endregion

        #region Update Journal
        public void ProcessJournalXML(XmlDocument fileXml, CharOrCorp corc, short walletID)
        {
            DataImportParams parameters = new DataImportParams();
            parameters.xmlData = fileXml;
            parameters.corc = corc;
            parameters.walletID = walletID;
            ThreadPool.QueueUserWorkItem(UpdateAssetsFromXML, parameters);
        }

        private void UpdateJournalFromXML(object parameters)
        {
            // Just wait a moment to allow the caller to display the 
            // progress dialog if importing from a file.
            Thread.Sleep(200);

            DataImportParams data = (DataImportParams)parameters;

            lock (Globals.JournalAPIUpdateLock)
            {
                SetLastAPIUpdateError(data.corc, APIDataType.Journal, "UPDATING");
                UpdateJournalFromXML(data.corc, data.xmlData, data.walletID);
            }

            APICharacters.Store(this);
            if (GetLastAPIUpdateError(data.corc, APIDataType.Journal).Equals("UPDATING"))
            {
                SetLastAPIUpdateError(data.corc, APIDataType.Journal, "");
            }
        }
        
        /// <summary>
        /// Add journal entries from the supplied XML to the database.
        /// </summary>
        /// <param name="corc"></param>
        /// <param name="fileXML"></param>
        /// <returns>The number of rows added to the journal table.</returns>
        private int UpdateJournalFromXML(CharOrCorp corc, XmlDocument fileXML, short walletID)
        {
            int retVal = 0;
            int updatedEntries = 0;
            EMMADataSet.JournalDataTable journalData = new EMMADataSet.JournalDataTable();
            long highestIDSoFar = _apiSettings.GetHighestID(corc, APIDataType.Journal);
            long oldHighestID = _apiSettings.GetHighestID(corc, APIDataType.Journal);
            DateTime dataDate = DateTime.UtcNow;

            try
            {
                XmlNodeList journEntries = null;
                XmlDocument xml = new XmlDocument();

                UpdateStatus(0, 1, "Getting journal entries from file", "", false);
                journEntries = EveAPI.GetResults(fileXML);
                dataDate = EveAPI.GetCachedUntilTime(fileXML).AddHours(-1);
                UpdateStatus(1, 1, "", journEntries.Count + " entries found in file.", false);

                if (journEntries != null && journEntries.Count > 0)
                {
                    // Offset will always be the same since CCP switched over to 64 bit IDs
                    // (At least until we break the 64bit limit... As of mid 2010, we're using 
                    // around 2 billion IDs a year so breaking the 64 bit limit will take around 
                    // 9 billion years... Don't think it will be a problem :))
                    long offset = 2591720933;
                    int batchPrg = 0;

                    UpdateStatus(0, journEntries.Count, "Processing entries", "", false);


                    // Loop through the results returned from this call to the API and add the line 
                    // to the data table.
                    foreach (XmlNode journEntry in journEntries)
                    {
                        bool tryUpdate = false;
                        long id = long.Parse(journEntry.SelectSingleNode("@refID").Value) + offset;
                        long recieverID = 0;
                        if (corc == CharOrCorp.Corp)
                        {
                            // This is a special case.
                            // When bounty prizes are received by the player, corp tax is applied.
                            // This corp tax does not appear as a seperate journal entry for the
                            // character. It is specified by the taxReceiverID and taxAmount fields
                            // on the bounty prize entry itself in the XML.
                            // On the corp side, there is a specifc entry for the tax but it has
                            // the same journalentryID and ownerID2 as the character entry. 
                            // This means that EMMA does not differentiate between them and the
                            // corp tax part is lost.
                            // In order to resolve this we simply set receiver ID to be the corp
                            // instead of character in these cases.
                            // Note that 'BuildJournalEntry' has similar processing.
                            if (int.Parse(journEntry.SelectSingleNode("@refTypeID").Value) == 85)
                            {
                                recieverID = _corpID;
                            }
                        }
                        if (recieverID == 0)
                        {
                            recieverID = int.Parse(journEntry.SelectSingleNode("@ownerID2").Value);
                        }


                        if (id - offset > oldHighestID)
                        {
                            if (id - offset > highestIDSoFar) { highestIDSoFar = id - offset; }
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
                                        BuildJournalEntry(journalData, journEntry, offset, walletID, corc);

                                    journalData.AddJournalRow(newRow);
                                    retVal++;

                                    // This section searches the character and journal ref type tables 
                                    // for the values used in this new journal entry.
                                    // If they are not present in the tables then they are added.
                                    #region Check other tables and add values if needed.
                                    SortedList<long, string> entityIDs = new SortedList<long, string>();
                                    entityIDs.Add(newRow.SenderID, journEntry.SelectSingleNode("@ownerName1").Value);
                                    if (!entityIDs.ContainsKey(newRow.RecieverID))
                                    {
                                        entityIDs.Add(newRow.RecieverID, journEntry.SelectSingleNode("@ownerName2").Value);
                                    }
                                    foreach (KeyValuePair<long, string> checkName in entityIDs)
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
                                    BuildJournalEntry(journalData, journEntry, offset, walletID, corc);
                                EMMADataSet.JournalRow oldRow = journalData.FindByIDRecieverID(newRow.ID,
                                        newRow.RecieverID);
                                bool updated = false;

                                if (oldRow != null)
                                {
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
                                }

                                if (updated)
                                {
                                    updatedEntries++;
                                }
                            }

                        }

                        batchPrg++;
                        UpdateStatus(batchPrg, journEntries.Count, "", "", false);
                    }

                    SetHighestID(corc, APIDataType.Journal, highestIDSoFar);
                }

                UpdateStatus(0, 0, retVal + " journal entries added to database.", "", false);
                UpdateStatus(0, 0, updatedEntries + " existing journal entries updated.", "", true);

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
                    // If we've caught a standard exception rather than an EMMA one then log 
                    // it by creating a new exception.
                    // Note that we don't need to actually throw it..
                    emmaEx = new EMMAException(ExceptionSeverity.Error, "Error when adding journal data", ex);
                }

                SetLastAPIUpdateError(corc, APIDataType.Journal, ex.Message);
                UpdateStatus(-1, 0, "Error", ex.Message, true);
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
            // Entries of type 42 (market escrow) do not have a reciever ID whene retrieved from the API.
            // ID 1000132 is the secure commerce commission.
            // If the sender is the secure commerce commission then the receiver must be the current
            // char/corp and vice versa.
            if (retVal.TypeID == 42 && retVal.RecieverID == 0)
            {
                if (retVal.SenderID == 1000132)
                {
                    retVal.RecieverID = corc == CharOrCorp.Char ? _charID : _corpID;
                }
                else
                {
                    retVal.RecieverID = 1000132;
                }
            }

            if (corc == CharOrCorp.Corp)
            {
                // This is a special case.
                // When bounty prizes are received by the player, corp tax is applied.
                // This corp tax does not appear as a seperate journal entry for the
                // character. It is specified by the taxReceiverID and taxAmount fields
                // on the bounty prize entry itself in the XML.
                // On the corp side, there is a specifc entry for the tax but it has
                // the same journalentryID and ownerID2 as the character entry. 
                // This means that EMMA does not differentiate between them and the
                // corp tax part is lost.
                // In order to resolve this we simply set receiver ID to be the corp
                // instead of character and the sender to be the character instead
                // of concord.
                if (int.Parse(journEntry.SelectSingleNode("@refTypeID").Value) == 85)
                {
                    retVal.SenderID = retVal.RecieverID;
                    retVal.RecieverID = _corpID;
                }
            }


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
        public void ProcessTransactionsXML(XmlDocument fileXml, CharOrCorp corc, short walletID)
        {
            DataImportParams parameters = new DataImportParams();
            parameters.xmlData = fileXml;
            parameters.corc = corc;
            parameters.walletID = walletID;
            ThreadPool.QueueUserWorkItem(UpdateTransactionsFromXML, parameters);
        }

        private void UpdateTransactionsFromXML(object parameters)
        {
            // Just wait a moment to allow the caller to display the 
            // progress dialog if importing from a file.
            Thread.Sleep(200);

            DataImportParams data = (DataImportParams)parameters;

            lock (Globals.TransactionAPIUpdateLock)
            {
                lock (_syncLock)
                {
                    SetLastAPIUpdateError(data.corc, APIDataType.Transactions, "UPDATING");
                    UpdateTransactionsFromXML(data.corc, data.xmlData, data.walletID);
                }
            }

            APICharacters.Store(this);
            if (GetLastAPIUpdateError(data.corc, APIDataType.Transactions).Equals("UPDATING"))
            {
                SetLastAPIUpdateError(data.corc, APIDataType.Transactions, "");
            }
        }


        /// <summary>
        /// Update the database transactions table from the specified XML.
        /// </summary>
        /// <param name="corc"></param>
        /// <param name="fileXML"></param>
        /// <returns></returns>
        private int UpdateTransactionsFromXML(CharOrCorp corc, XmlDocument fileXML, short walletID)
        {
            int retVal = 0;
            EMMADataSet.TransactionsDataTable transData = new EMMADataSet.TransactionsDataTable();
            long highestIDSoFar = _apiSettings.GetHighestID(corc, APIDataType.Transactions);
            long highestID = 0;
            DateTime ticker = DateTime.UtcNow.AddSeconds(-10);

            try
            {
                int updated = 0;

                XmlNodeList transEntries = null;
                XmlDocument xml = new XmlDocument();

                UpdateStatus(0, 1, "Getting transactions from file", "", false);
                transEntries = EveAPI.GetResults(fileXML);
                UpdateStatus(1, 1, "", transEntries.Count + " entries found in file.", false);


                if (transEntries != null && transEntries.Count > 0)
                {
                    int batchPrg = 0;
                    UpdateStatus(0, transEntries.Count, "Processing transactions", "", false);


                    XmlNode entryIDNode = transEntries[0].SelectSingleNode("@transactionID");
                    long fileMaxID = long.Parse(entryIDNode.Value,
                        System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    if (fileMaxID > highestID) { highestID = fileMaxID; }

                    // Loop through the results returned from this call to the API and add the line to
                    // the data table if the transactionID is not already in the database.
                    foreach (XmlNode transEntry in transEntries)
                    {
                        XmlNode transIDNode = transEntry.SelectSingleNode("@transactionID");
                        long transID = long.Parse(transIDNode.Value,
                            System.Globalization.CultureInfo.InvariantCulture.NumberFormat);

                        if (transID > highestIDSoFar)
                        {
                            if (!Transactions.TransactionExists(transData, transID) &&
                                transData.FindByID(transID) == null)
                            {
                                // Actually create the line and add it to the data table
                                SortedList<int, string> nameIDs = new SortedList<int, string>();
                                EMMADataSet.TransactionsRow newRow = BuildTransRow(transID, transData,
                                    transEntry, walletID, nameIDs, false);

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
                                    BuildTransRow(transID, transData, transEntry, walletID, nameIDs, true);
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

                        batchPrg++;
                        UpdateStatus(batchPrg, transEntries.Count, "", "", false);
                    }
                }

                if (highestID > highestIDSoFar)
                {
                    SetHighestID(corc, APIDataType.Transactions, highestID);
                }

                UpdateStatus(0, 0, retVal + " transactions added to database.", "", false);
                UpdateStatus(0, 0, updated + " transactions updated.", "", false);


                if (transData.Count > 0)
                {
                    Transactions.Store(transData);

                    UpdateStatus(1, 1, "", "Complete", true);
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
                    emmaEx = new EMMAException(ExceptionSeverity.Error, "Error when adding transactions", ex);
                }

                SetLastAPIUpdateError(corc, APIDataType.Transactions, ex.Message);
                UpdateStatus(-1, 0, "Error", ex.Message, true);
            }

            if (UpdateEvent != null)
            {
                UpdateEvent(this, new APIUpdateEventArgs(APIDataType.Transactions,
                    corc == CharOrCorp.Char ? _charID : _corpID,
                    APIUpdateEventType.UpdateCompleted));
            }

            return retVal;
        }


        private EMMADataSet.TransactionsRow BuildTransRow(long transID, EMMADataSet.TransactionsDataTable transData,
            XmlNode transEntry, short walletID, SortedList<int, string> nameIDs, bool rowInDatabase)
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
                if (!rowInDatabase)
                {
                    Assets.BuyAssets(forCorp ? _corpID : _charID, newRow.StationID, newRow.ItemID, 
                        newRow.Quantity, newRow.Price, 
                        forCorp ? Settings.CorpAssetsEffectiveDate : Settings.CharAssetsEffectiveDate,
                        newRow.DateTime);
                }
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
                if (!rowInDatabase)
                {
                    newRow.SellerUnitProfit = Transactions.CalcProfit(forCorp ? _corpID : _charID, 
                        transData, newRow, 
                        forCorp ? Settings.CorpAssetsEffectiveDate : Settings.CharAssetsEffectiveDate);
                }
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
        public void ProcessOrdersXML(XmlDocument fileXml, CharOrCorp corc)
        {
            DataImportParams parameters = new DataImportParams();
            parameters.xmlData = fileXml;
            parameters.corc = corc;
            parameters.walletID = 0;
            ThreadPool.QueueUserWorkItem(UpdateOrdersFromXML, parameters);
        }

        private void UpdateOrdersFromXML(object parameters)
        {
            // Just wait a moment to allow the caller to display the 
            // progress dialog if importing from a file.
            Thread.Sleep(200);

            DataImportParams data = (DataImportParams)parameters;

            // Note, the sync lock is used to make sure that a transaction, assets or orders update do
            // not run at the same time for a character. 
            lock (_syncLock)
            {
                SetLastAPIUpdateError(data.corc, APIDataType.Orders, "UPDATING");
                UpdateOrdersFromXML(data.corc, data.xmlData);
            }

            APICharacters.Store(this);
            if (GetLastAPIUpdateError(data.corc, APIDataType.Orders).Equals("UPDATING"))
            {
                SetLastAPIUpdateError(data.corc, APIDataType.Orders, "");
            }
        }


        private void UpdateOrdersFromXML(CharOrCorp corc, XmlDocument fileXML)
        {
            EMMADataSet.OrdersDataTable orderData = new EMMADataSet.OrdersDataTable();
            int added = 0;
            int updated = 0;

            try
            {

                Orders.SetProcessed(corc == CharOrCorp.Corp ? _corpID : _charID, false);

                XmlNodeList orderEntries = null;
                XmlDocument xml = new XmlDocument();

                UpdateStatus(0, 1, "Getting orders from file", "", false);
                orderEntries = EveAPI.GetResults(fileXML);
                UpdateStatus(1, 1, "", orderEntries.Count + " orders found in file.", false);


                if (orderEntries != null && orderEntries.Count > 0)
                {
                    UpdateStatus(0, orderEntries.Count, "Processing orders", "", false);

                    foreach (XmlNode orderEntry in orderEntries)
                    {
                        EMMADataSet.OrdersRow orderRow = BuildOrdersRow(orderData, orderEntry, corc);
                        int id = 0;

                        if (!Orders.Exists(orderData, orderRow, ref id, _corpID, _charID))
                        {
                            // Order does not exist in the database so add it.
                            orderData.AddOrdersRow(orderRow);
                            if (orderRow.OrderState == (short)OrderState.ExpiredOrFilled)
                            {
                                bool notify = false;
                                notify = UserAccount.CurrentGroup.Settings.OrdersNotifyEnabled &&
                                    ((UserAccount.CurrentGroup.Settings.OrdersNotifyBuy && orderRow.BuyOrder) ||
                                    (UserAccount.CurrentGroup.Settings.OrdersNotifySell && !orderRow.BuyOrder));
                                if (notify)
                                {
                                    orderRow.OrderState = (short)OrderState.ExpiredOrFilledAndUnacknowledged;
                                }
                                else
                                {
                                    orderRow.OrderState = (short)OrderState.ExpiredOrFilledAndAcknowledged;
                                }
                            }
                            added++;
                        }
                        else
                        {
                            EMMADataSet.OrdersRow oldRow = orderData.FindByID(id);

                            if (oldRow.TotalVol == orderRow.TotalVol &&
                                oldRow.RemainingVol == orderRow.RemainingVol &&
                                oldRow.MinVolume == orderRow.MinVolume && oldRow.Range == orderRow.Range &&
                                oldRow.Duration == orderRow.Duration && oldRow.Escrow == orderRow.Escrow &&
                                oldRow.Price == orderRow.Price && oldRow.OrderState == orderRow.OrderState &&
                                oldRow.EveOrderID == orderRow.EveOrderID)
                            {
                                // If the order from the XML exactly matches what we have in the database
                                // then just set the processed flag and remove it from the orderData table
                                // without setting it to be removed from the database.
                                //Orders.SetProcessedByID(oldRow.ID, true);
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
                                //Orders.SetProcessedByID(oldRow.ID, true);

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
                                    oldRow.Price != orderRow.Price || oldRow.EveOrderID != orderRow.EveOrderID)
                                {
                                    oldRow.TotalVol = orderRow.TotalVol;
                                    oldRow.RemainingVol = orderRow.RemainingVol;
                                    oldRow.MinVolume = orderRow.MinVolume;
                                    oldRow.Range = orderRow.Range;
                                    oldRow.Duration = orderRow.Duration;
                                    oldRow.Escrow = orderRow.Escrow;
                                    oldRow.Price = orderRow.Price;
                                    oldRow.EveOrderID = orderRow.EveOrderID;
                                    // Note, only other fields are 'buyOrder' and 'issued'. Neither of which we want to change.
                                    updated++;
                                }
                            }
                        }

                        UpdateStatus(added + updated, orderEntries.Count, "", "", false);
                    }
                }

                UpdateStatus(0, 0, added + " orders added to database.", "", false);
                UpdateStatus(0, 0, updated + " orders updated.", "", true);

                if (orderData.Count > 0)
                {
                    Orders.Store(orderData);
                }

                Orders.FinishUnProcessed(corc == CharOrCorp.Corp ? _corpID : _charID);
            }
            catch (Exception ex)
            {
                EMMAException emmaEx = ex as EMMAException;
                if (emmaEx == null)
                {
                    // If we've caught a standard exception rather than an EMMA one then log it by creating a 
                    // new exception.
                    // Note that we don't need to actually throw it..
                    emmaEx = new EMMAException(ExceptionSeverity.Error, "Error when adding market orders", ex);
                }

                SetLastAPIUpdateError(corc, APIDataType.Orders, ex.Message);
                UpdateStatus(-1, 0, "Error", ex.Message, true);
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

            newRow.EveOrderID = long.Parse(orderEntry.SelectSingleNode("@orderID").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            //newRow.OwnerID = int.Parse(orderEntry.SelectSingleNode("@charID").Value,
            //    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            newRow.OwnerID = corc == CharOrCorp.Corp ? _corpID : _charID;
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
                System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
            newRow.Processed = true;

            return newRow;
        }
        #endregion


        #region Update Industry Jobs

        public void ProcessIndustryJobsXML(XmlDocument fileXml, CharOrCorp corc)
        {
            DataImportParams parameters = new DataImportParams();
            parameters.xmlData = fileXml;
            parameters.corc = corc;
            parameters.walletID = 0;
            ThreadPool.QueueUserWorkItem(UpdateIndustryJobsFromXML, parameters);
        }

        private void UpdateIndustryJobsFromXML(object parameters)
        {
            // Just wait a moment to allow the caller to display the 
            // progress dialog if importing from a file.
            Thread.Sleep(200);

            DataImportParams data = (DataImportParams)parameters;

            // Note, the sync lock is used to make sure that a transaction, assets, orders or 
            // industry jobs update do not run at the same time for a character. 
            lock (_syncLock)
            {
                SetLastAPIUpdateError(data.corc, APIDataType.IndustryJobs, "UPDATING");
                UpdateIndustryJobsFromXML(data.corc, data.xmlData);
            }

            APICharacters.Store(this);
            if (GetLastAPIUpdateError(data.corc, APIDataType.IndustryJobs).Equals("UPDATING"))
            {
                SetLastAPIUpdateError(data.corc, APIDataType.IndustryJobs, "");
            }
        }

        private void UpdateIndustryJobsFromXML(CharOrCorp corc, XmlDocument fileXML)
        {
            EMMADataSet.IndustryJobsDataTable jobsData = new EMMADataSet.IndustryJobsDataTable();

            try
            {
                XmlNodeList jobEntries = null;

                UpdateStatus(0, 1, "Getting industry jobs from file", "", false);
                jobEntries = EveAPI.GetResults(fileXML);
                UpdateStatus(1, 1, "", jobEntries.Count + " industry jobs found in file.", false);


                if (jobEntries != null && jobEntries.Count > 0)
                {
                    UpdateStatus(0, jobEntries.Count, "Processing jobs", "", false);

                    foreach (XmlNode jobEntry in jobEntries)
                    {
                        EMMADataSet.IndustryJobsRow jobRow = BuildIndustryJobRow(jobsData, jobEntry);
                        if (IndustryJobs.GetJob(jobsData, jobRow.ID))
                        {
                            // The job already exists in the database. Update if needed.
                            EMMADataSet.IndustryJobsRow oldJobRow = jobsData.FindByID(jobRow.ID);
                            if (oldJobRow.Completed != jobRow.Completed ||
                                oldJobRow.CompletedStatus != jobRow.CompletedStatus ||
                                oldJobRow.CompletedSuccessfully != jobRow.CompletedSuccessfully ||
                                oldJobRow.EndProductionTime.CompareTo(jobRow.EndProductionTime) != 0 ||
                                oldJobRow.PauseProductionTime.CompareTo(jobRow.PauseProductionTime) != 0)
                            {
                                oldJobRow.Completed = jobRow.Completed;
                                oldJobRow.CompletedStatus = jobRow.CompletedStatus;
                                oldJobRow.CompletedSuccessfully = jobRow.CompletedSuccessfully;
                                oldJobRow.EndProductionTime = jobRow.EndProductionTime;
                                oldJobRow.PauseProductionTime = jobRow.PauseProductionTime;
                            }
                            else
                            {
                                // No changes
                            }
                        }
                        else
                        {
                            // This is a new job. Add it to the database.
                            jobsData.AddIndustryJobsRow(jobRow);
                        }

                    }
                }

                if (jobsData != null && jobsData.Count > 0)
                {
                    IndustryJobs.Store(jobsData);
                }
            }
            catch (Exception ex)
            {
                EMMAException emmaEx = ex as EMMAException;
                if (emmaEx == null)
                {
                    // If we've caught a standard exception rather than an EMMA one then log it by creating a 
                    // new exception.
                    // Note that we don't need to actually throw it..
                    emmaEx = new EMMAException(ExceptionSeverity.Error, "Error when adding industry jobs", ex);
                }

                SetLastAPIUpdateError(corc, APIDataType.IndustryJobs, ex.Message);
                UpdateStatus(-1, 0, "Error", ex.Message, true);
            }


            if (UpdateEvent != null)
            {
                UpdateEvent(this, new APIUpdateEventArgs(APIDataType.IndustryJobs,
                    corc == CharOrCorp.Char ? _charID : _corpID,
                    APIUpdateEventType.UpdateCompleted));
            }

        }

        private EMMADataSet.IndustryJobsRow BuildIndustryJobRow(EMMADataSet.IndustryJobsDataTable dataTable,
            XmlNode xmlData)
        {
            EMMADataSet.IndustryJobsRow jobRow = dataTable.NewIndustryJobsRow();
            jobRow.ID = long.Parse(xmlData.SelectSingleNode("@jobID").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            jobRow.ActivityID = int.Parse(xmlData.SelectSingleNode("@activityID").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            jobRow.AssemblyLineID = int.Parse(xmlData.SelectSingleNode("@assemblyLineID").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            jobRow.BeginProductionTime = DateTime.Parse(xmlData.SelectSingleNode("@beginProductionTime").Value,
                System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
            jobRow.CharMaterialModifier = double.Parse(xmlData.SelectSingleNode("@charMaterialMultiplier").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            jobRow.CharTimeMultiplier = double.Parse(xmlData.SelectSingleNode("@charTimeMultiplier").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            jobRow.Completed = int.Parse(xmlData.SelectSingleNode("@completed").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat) == 1;
            jobRow.CompletedStatus = int.Parse(xmlData.SelectSingleNode("@completedStatus").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            jobRow.CompletedSuccessfully = int.Parse(xmlData.SelectSingleNode("@completedSuccessfully").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat) == 1;
            jobRow.ContainerID = int.Parse(xmlData.SelectSingleNode("@containerID").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            jobRow.ContainerTypeID = int.Parse(xmlData.SelectSingleNode("@containerTypeID").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            jobRow.EndProductionTime = DateTime.Parse(xmlData.SelectSingleNode("@endProductionTime").Value,
                System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
            jobRow.InstalledItemCopy = int.Parse(xmlData.SelectSingleNode("@installedItemCopy").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat) == 1;
            jobRow.InstalledItemFlag = int.Parse(xmlData.SelectSingleNode("@installedItemFlag").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            jobRow.InstalledItemID = int.Parse(xmlData.SelectSingleNode("@installedItemID").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            jobRow.InstalledItemLocationID = int.Parse(xmlData.SelectSingleNode("@installedItemLocationID").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            jobRow.InstalledItemME = int.Parse(xmlData.SelectSingleNode("@installedItemMaterialLevel").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            jobRow.InstalledItemPL = int.Parse(xmlData.SelectSingleNode("@installedItemProductivityLevel").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            jobRow.InstalledItemQuantity = int.Parse(xmlData.SelectSingleNode("@installedItemQuantity").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            jobRow.InstalledItemRunsRemaining = int.Parse(xmlData.SelectSingleNode("@installedItemLicensedProductionRunsRemaining").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            jobRow.InstalledItemTypeID = int.Parse(xmlData.SelectSingleNode("@installedItemTypeID").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            jobRow.InstallerID = int.Parse(xmlData.SelectSingleNode("@installerID").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            jobRow.InstallTime = DateTime.Parse(xmlData.SelectSingleNode("@installTime").Value,
                System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
            jobRow.JobRuns = int.Parse(xmlData.SelectSingleNode("@runs").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            jobRow.MaterialModifier = double.Parse(xmlData.SelectSingleNode("@materialMultiplier").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            jobRow.OutputFlag = int.Parse(xmlData.SelectSingleNode("@outputFlag").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            jobRow.OutputLcoationID = int.Parse(xmlData.SelectSingleNode("@outputLocationID").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            jobRow.OutputRuns = int.Parse(xmlData.SelectSingleNode("@licensedProductionRuns").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            jobRow.OutputTypeID = int.Parse(xmlData.SelectSingleNode("@outputTypeID").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            jobRow.PauseProductionTime = DateTime.Parse(xmlData.SelectSingleNode("@pauseProductionTime").Value,
                System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
            jobRow.TimeMultiplier = double.Parse(xmlData.SelectSingleNode("@timeMultiplier").Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);

            if (jobRow.PauseProductionTime.CompareTo(SqlDateTime.MinValue.Value) < 0) { jobRow.PauseProductionTime = SqlDateTime.MinValue.Value; }
            if (jobRow.PauseProductionTime.CompareTo(SqlDateTime.MaxValue.Value) > 0) { jobRow.PauseProductionTime = SqlDateTime.MaxValue.Value; }
            if (jobRow.EndProductionTime.CompareTo(SqlDateTime.MinValue.Value) < 0) { jobRow.PauseProductionTime = SqlDateTime.MinValue.Value; }
            if (jobRow.EndProductionTime.CompareTo(SqlDateTime.MaxValue.Value) > 0) { jobRow.PauseProductionTime = SqlDateTime.MaxValue.Value; }
            if (jobRow.BeginProductionTime.CompareTo(SqlDateTime.MinValue.Value) < 0) { jobRow.PauseProductionTime = SqlDateTime.MinValue.Value; }
            if (jobRow.BeginProductionTime.CompareTo(SqlDateTime.MaxValue.Value) > 0) { jobRow.PauseProductionTime = SqlDateTime.MaxValue.Value; }
            if (jobRow.InstallTime.CompareTo(SqlDateTime.MinValue.Value) < 0) { jobRow.PauseProductionTime = SqlDateTime.MinValue.Value; }
            if (jobRow.InstallTime.CompareTo(SqlDateTime.MaxValue.Value) > 0) { jobRow.PauseProductionTime = SqlDateTime.MaxValue.Value; }

            return jobRow;
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
                    long oldCorp = _corpID;
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
            if (DateTime.UtcNow.AddHours(-48).CompareTo(_corpSheetXMLLastUpdate) > 0)
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
                bool autoTrans = false, autoJournal = false, autoAssets = false, autoOrders = false, 
                    autoIndustry = false;
                _charLastRptGroupID = UserAccount.CurrentGroup.ID;
                _charIncWithRptGroup = ReportGroups.GroupCharSettings(_charLastRptGroupID, _charID,
                    ref autoTrans, ref autoJournal, ref autoAssets, ref autoOrders, ref autoIndustry);
                _apiSettings.SetAutoUpdateFlag(CharOrCorp.Char, APIDataType.Transactions, autoTrans);
                _apiSettings.SetAutoUpdateFlag(CharOrCorp.Char, APIDataType.Journal, autoJournal);
                _apiSettings.SetAutoUpdateFlag(CharOrCorp.Char, APIDataType.Orders, autoOrders);
                _apiSettings.SetAutoUpdateFlag(CharOrCorp.Char, APIDataType.Assets, autoAssets);
                _apiSettings.SetAutoUpdateFlag(CharOrCorp.Char, APIDataType.IndustryJobs, autoIndustry);
                _oldCharIncWithRptGroup = _charIncWithRptGroup;
            }
        }

        private void GetGroupLevelCorpSettings()
        {
            if (UserAccount.CurrentGroup.ID != _corpLastRptGroupID)
            {
                bool autoTrans = false, autoJournal = false, autoAssets = false, autoOrders = false,
                    autoIndustry = false; 
                _corpLastRptGroupID = UserAccount.CurrentGroup.ID;
                _corpIncWithRptGroup = ReportGroups.GroupCorpSettings(_corpLastRptGroupID, _corpID,
                    ref autoTrans, ref autoJournal, ref autoAssets, ref autoOrders, ref autoIndustry, _charID);
                _apiSettings.SetAutoUpdateFlag(CharOrCorp.Corp, APIDataType.Transactions, autoTrans);
                _apiSettings.SetAutoUpdateFlag(CharOrCorp.Corp, APIDataType.Journal, autoJournal);
                _apiSettings.SetAutoUpdateFlag(CharOrCorp.Corp, APIDataType.Orders, autoOrders);
                _apiSettings.SetAutoUpdateFlag(CharOrCorp.Corp, APIDataType.Assets, autoAssets);
                _apiSettings.SetAutoUpdateFlag(CharOrCorp.Corp, APIDataType.IndustryJobs, autoIndustry);
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
                case APIDataType.IndustryJobs:
                    retVal = Settings.CorpIndustryJobsAPIAccess;
                    break;
                case APIDataType.Unknown:
                    break;
                case APIDataType.Full:
                    retVal = Settings.CorpTransactionsAPIAccess && Settings.CorpJournalAPIAccess &&
                        Settings.CorpAssetsAPIAccess && Settings.CorpOrdersAPIAccess && 
                        Settings.CorpIndustryJobsAPIAccess;
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

    public class APIUpdateInfo
    {
        public CharOrCorp Corc { get; set; }
        public APIDataType Type { get; set; }

        public APIUpdateInfo(CharOrCorp corc, APIDataType type)
        {
            Corc = corc;
            Type = type;
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
