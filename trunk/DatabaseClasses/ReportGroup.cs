using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using EveMarketMonitorApp.AbstractionClasses;
using EveMarketMonitorApp.GUIElements;
using EveMarketMonitorApp.Common;
using System.Windows.Forms;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public class ReportGroup
    {
        private EMMADataSetTableAdapters.RptGroupSettingsTableAdapter rptGroupSettingsTableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.RptGroupSettingsTableAdapter();

        private string _name;
        private int _id;
        private List<EVEAccount> _accounts;
        private int _accHashCode;
        private bool _public;
        private GroupAccess _accessLevel;
        private ReportGroupSettings _settings = null;
        private ItemValues _itemValues = null;
        private TradedItems _tradedItems = null;

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="data"></param>
        public ReportGroup(EMMADataSet.ReportGroupsRow data) 
        {
            _name = data.Name.Trim();
            _id = data.ID;
            _public = data.PublicGroup;
            _accessLevel = ReportGroups.GetAccessLevel(UserAccount.Name, _id);
        }

        public bool HasCharOrCorp()
        {
            return GetCharCorpOptions().Count > 0;
        }

        public List<CharCorpOption> GetCharCorpOptions()
        {
            return GetCharCorpOptions(APIDataType.Unknown, false);
        }

        public List<CharCorpOption> GetCharCorpOptions(bool charsOnly)
        {
            return GetCharCorpOptions(APIDataType.Unknown, charsOnly);
        }

        public List<CharCorpOption> GetCharCorpOptions(APIDataType type)
        {
            return GetCharCorpOptions(type, false);
        }

        public List<CharCorpOption> GetCharCorpOptions(APIDataType type, bool charsOnly)
        {
            List<CharCorpOption> options = new List<CharCorpOption>();

            foreach (EVEAccount account in _accounts)
            {
                foreach (APICharacter apichar in account.Chars)
                {
                    if (apichar.CharIncWithRptGroup)
                    {
                        CharCorpOption opt = new CharCorpOption(apichar, false);
                        options.Add(opt);
                    }
                    if (!charsOnly && apichar.CorpIncWithRptGroup && apichar.CharHasCorporateAccess(type))
                    {
                        CharCorpOption opt = new CharCorpOption(apichar, true);
                        options.Add(opt);
                    }
                }
            }

            return options;
        }

        public List<AssetAccessParams> GetAssetAccessParams(APIDataType type)
        {
            List<AssetAccessParams> accessList = new List<AssetAccessParams>();
            foreach (EVEAccount account in _accounts)
            {
                foreach (APICharacter character in account.Chars)
                {
                    if (character.CharIncWithRptGroup)
                    {
                        accessList.Add(new AssetAccessParams(character.CharID));
                    }
                    if (character.CorpIncWithRptGroup && character.CharHasCorporateAccess(type))
                    {
                        accessList.Add(new AssetAccessParams(character.CorpID));
                    }
                }
            }
            return accessList;
        }

        public List<FinanceAccessParams> GetFinanceAccessParams(APIDataType type)
        {
            List<FinanceAccessParams> accessList = new List<FinanceAccessParams>();
            foreach (EVEAccount account in _accounts)
            {
                foreach (APICharacter character in account.Chars)
                {
                    if (character.CharIncWithRptGroup)
                    {
                        accessList.Add(new FinanceAccessParams(character.CharID));
                    }
                    if (character.CorpIncWithRptGroup && character.CharHasCorporateAccess(type))
                    {
                        accessList.Add(new FinanceAccessParams(character.CorpID));
                    }
                }
            }
            return accessList;
        }

        public void LoadEveAccounts()
        {
            Diagnostics.StartTimer("RptGrp.LoadEveAccounts");
            if (_accounts == null)
            {
                _accounts = EveAccounts.GetGroupAccounts(_id);
                if (_accounts == null)
                {
                    _accounts = new List<EVEAccount>();
                }
                _accHashCode = GetAccHashCode();

                foreach (EVEAccount account in _accounts)
                {
                    account.UpdateCharList(false);
                    account.PopulateChars();
                }

                if (Globals.License == Enforcer.LicenseType.Lite)
                {
                    // Check that only one character/corp is in this report group
                    int charTotal = 0;
                    int corpTotal = 0;
                    foreach (EVEAccount account in _accounts)
                    {
                        foreach (APICharacter character in account.Chars)
                        {
                            if (character.CharIncWithRptGroup) { charTotal++; }
                            if (character.CorpIncWithRptGroup) { corpTotal++; }
                        }
                    }
                    if (charTotal + corpTotal > 1)
                    {
                        throw new EMMALicensingException(ExceptionSeverity.Error,
                            "You have an EMMA 'lite' license. This only allows you to work with " +
                            "report groups containing a single character or corporation. The report group '" +
                            _name + "' contains " + charTotal + " characters and " + corpTotal + " corps." +
                            " You must create new report groups, each with only one character/corporation." +
                            " Note that you will still retain all existing data.");
                    }
                }
            }
            Diagnostics.StopTimer("RptGrp.LoadEveAccounts");
        }

        public void StoreEveAccounts()
        {
            if (AccountsChanged())
            {
                ReportGroups.SetGroupAccounts(_id, Accounts);
                foreach (EVEAccount account in Accounts) { EveAccounts.Store(account); }
            }
        }

        private bool AccountsChanged()
        {
            return _accHashCode != GetAccHashCode();
        }

        private int GetAccHashCode()
        {
            StringBuilder str = new StringBuilder();
            foreach (EVEAccount account in _accounts)
            {
                str.Append(account.UserID);
                str.Append(account.LastcharListUpdate.ToString());               
            }
            return str.ToString().GetHashCode();
        }

        public EVEAccount GetAccount(long userID)
        {
            EVEAccount retVal = null;
            foreach (EVEAccount account in _accounts)
            {
                if (account.UserID == userID) { retVal = account; }
            }
            return retVal;
        }

        public APICharacter GetCharacter(long entityID)
        {
            bool corp = false;
            return GetCharacter(entityID, ref corp);
        }
        public APICharacter GetCharacter(long entityID, ref bool corpID)
        {
            APICharacter retVal = null;
            corpID = false;
            bool corpInReportGroup = false;
            foreach (EVEAccount account in _accounts)
            {
                foreach (APICharacter character in account.Chars)
                {
                    if (character.CharID == entityID) { retVal = character; }
                    if (character.CorpID == entityID) 
                    {
                        // There may be several chars that are part of the same corp.
                        // Favour returning the character that is set as the primary
                        // API updater for the corp.
                        if (!corpInReportGroup && character.CorpIncWithRptGroup)
                        {
                            retVal = character;
                            corpID = true;
                            corpInReportGroup = character.CorpIncWithRptGroup;
                        }
                    }
                }
            }
            return retVal;
        }

        /// <summary>
        /// Initialise the settings object based upon the current report group ID
        /// </summary>
        private void InitSettings()
        {
            EMMADataSet.RptGroupSettingsDataTable settingsTable = new EMMADataSet.RptGroupSettingsDataTable();
            lock (rptGroupSettingsTableAdapter)
            {
                rptGroupSettingsTableAdapter.FillByID(settingsTable, _id);
            }
            if (settingsTable.Count > 0)
            {
                XmlDocument settingsDoc = new XmlDocument();
                settingsDoc.LoadXml(settingsTable[0].Settings);
                _settings = new ReportGroupSettings(settingsDoc);
            }
            else
            {
                _settings = new ReportGroupSettings(_id);
            }
        }

        /// <summary>
        /// Store the report group's settings in the database
        /// </summary>
        public void StoreSettings()
        {
            // If it's null then it's not been accessed and there are no changes to store..
            if (_settings != null)
            {
                if (_settings.Changed)
                {
                    EMMADataSet.RptGroupSettingsDataTable settingsTable = new EMMADataSet.RptGroupSettingsDataTable();
                    lock (rptGroupSettingsTableAdapter)
                    {
                        rptGroupSettingsTableAdapter.FillByID(settingsTable, _id);
                        if (settingsTable.Count == 0)
                        {
                            EMMADataSet.RptGroupSettingsRow newRow = settingsTable.NewRptGroupSettingsRow();
                            newRow.ReportGroupID = _id;
                            // Just make this blank temporarilly so we are allowed to add it to the table.
                            newRow.Settings = "";
                            settingsTable.AddRptGroupSettingsRow(newRow);
                        }
                        settingsTable[0].Settings = _settings.Xml.InnerXml;

                        rptGroupSettingsTableAdapter.Update(settingsTable);
                    }
                    _settings.Changed = false;
                }
            }
            // Store character level settings as well.
            foreach (EVEAccount account in _accounts)
            {
                foreach (APICharacter character in account.Chars)
                {
                    if (character.CharIncWithRptGroup || character.CorpIncWithRptGroup)
                    {
                        character.StoreSettings();
                    }
                }
            }
        }

        /// <summary>
        /// Store the report group's traded items data in the database
        /// </summary>
        public void StoreItemsTraded()
        {
            // If it's null then it's not been accessed and there are no changes to store..
            if (_itemValues != null)
            {
                _itemValues.Store();
            }
        }
       
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }

        public bool IsPublic
        {
            get { return _public; }
            set { _public = value; }
        }

        public string Type
        {
            get { return _public ? "Public" : "Private"; }
        }

        public GroupAccess AccessLevel
        {
            get { return _accessLevel; }
            set { _accessLevel = value; }
        }

        public string AccessLevelText
        {
            get { return _accessLevel == GroupAccess.Full ? "Full" : "Read Only"; }
        }

        public List<EVEAccount> Accounts
        {
            get { return _accounts; }
            set { _accounts = value; }
        }

        public ReportGroupSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    InitSettings();
                }
                return _settings;
            }
        }

        public ItemValues ItemValues
        {
            get
            {
                if (_itemValues == null)
                {
                    _itemValues = new ItemValues(_id);
                }
                return _itemValues;
            }
        }

        public TradedItems TradedItems
        {
            get
            {
                if (_tradedItems == null)
                {
                    _tradedItems = new TradedItems(_id);
                }
                return _tradedItems;
            }
        }
    }

    public enum GroupAccess
    {
        Full,
        ReadOnly
    }

    public class CharCorpOption : IComparable
    {
        CharCorp _data;

        public CharCorpOption(APICharacter characterObj, bool corp)
        {
            _data = new CharCorp(characterObj, corp);
        }

        public APICharacter CharacterObj
        {
            get { return _data.characterObj; }
            set { _data.characterObj = value; }
        }

        public bool Corp
        {
            get { return _data.corp; }
            set { _data.corp = value; }
        }

        public string Name
        {
            get { return _data.corp ? _data.characterObj.CorpName : _data.characterObj.CharName; }
        }

        public CharCorp Data
        {
            get { return _data; }
        }

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(object obj)
        {
            int retVal = 0;
            CharCorpOption other = obj as CharCorpOption;
            if (other != null)
            {
                retVal = Name.CompareTo(other.Name);
            }

            return retVal;
        }
    }

    public class CharCorp : IComparable
    {
        public APICharacter characterObj;
        public bool corp;

        public CharCorp(APICharacter charObj, bool corp) 
        {
            characterObj = charObj;
            this.corp = corp;
        }

        public long ID
        {
            get { return (corp ? characterObj.CorpID : characterObj.CharID);}
        }

        public int CompareTo(object obj)
        {
            int retVal = 0;
            CharCorp other = obj as CharCorp;
            if (other != null)
            {
                retVal =  ID.CompareTo(other.ID);
            }
            return retVal;
        }
    }
}
