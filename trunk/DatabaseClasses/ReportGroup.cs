using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using EveMarketMonitorApp.AbstractionClasses;
using EveMarketMonitorApp.GUIElements;
using EveMarketMonitorApp.Common;

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
        private ItemsTraded _itemsTraded = null;

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
                    accessList.Add(new AssetAccessParams(character.CharID, character.CharIncWithRptGroup,
                        character.CorpIncWithRptGroup && character.CharHasCorporateAccess(type)));
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
            }
            Diagnostics.StopTimer("RptGrp.LoadEveAccounts");
        }

        public void StoreEveAccounts()
        {
            if (AccountsChanged())
            {
                ReportGroups.SetGroupAccounts(_id, Accounts);
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
            }
            return str.ToString().GetHashCode();
        }

        public EVEAccount GetAccount(int userID)
        {
            EVEAccount retVal = null;
            foreach (EVEAccount account in _accounts)
            {
                if (account.UserID == userID) { retVal = account; }
            }
            return retVal;
        }

        public APICharacter GetCharacter(int entityID)
        {
            bool corp = false;
            return GetCharacter(entityID, ref corp);
        }
        public APICharacter GetCharacter(int entityID, ref bool corpID)
        {
            APICharacter retVal = null;
            corpID = false;
            foreach (EVEAccount account in _accounts)
            {
                foreach (APICharacter character in account.Chars)
                {
                    if (character.CharID == entityID) { retVal = character; }
                    if (character.CorpID == entityID) { retVal = character; corpID = true; }
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
            rptGroupSettingsTableAdapter.FillByID(settingsTable, _id);
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
            if (_itemsTraded != null)
            {
                _itemsTraded.Store();
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

        public ItemsTraded ItemsTraded
        {
            get
            {
                if (_itemsTraded == null)
                {
                    _itemsTraded = new ItemsTraded(_id);
                }
                return _itemsTraded;
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

        public int ID
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
