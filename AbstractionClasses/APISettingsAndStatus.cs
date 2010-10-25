using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlTypes;
using System.Xml;

using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters;

namespace EveMarketMonitorApp.AbstractionClasses
{
    public class APISettingsAndStatus
    {
        private APICharSettingsTableAdapter settingsTableAdapter = new APICharSettingsTableAdapter();
        private APICharSettings _settings;
        private long _charID;

        #region Class Variables
        private DateTime _lastCharTransUpdate = SqlDateTime.MinValue.Value;
        private DateTime _lastCharJournalUpdate = SqlDateTime.MinValue.Value;
        private DateTime _lastCharAssetsUpdate = SqlDateTime.MinValue.Value;
        private DateTime _lastCharOrdersUpdate = SqlDateTime.MinValue.Value;
        private DateTime _lastCorpTransUpdate = SqlDateTime.MinValue.Value;
        private DateTime _lastCorpJournalUpdate = SqlDateTime.MinValue.Value;
        private DateTime _lastCorpAssetsUpdate = SqlDateTime.MinValue.Value;
        private DateTime _lastCorpOrdersUpdate = SqlDateTime.MinValue.Value;

        private bool _autoUpdateCharTrans = true;
        private bool _autoUpdateCharJournal = true;
        private bool _autoUpdateCharAssets = true;
        private bool _autoUpdateCharOrders = true;
        private bool _autoUpdateCharIndustryJobs = true;
        private bool _autoUpdateCorpTrans = true;
        private bool _autoUpdateCorpJournal = true;
        private bool _autoUpdateCorpAssets = true;
        private bool _autoUpdateCorpOrders = true;
        private bool _autoUpdateCorpIndustryJobs = true;

        private string _lastCharTransUpdateError = "";
        private string _lastCharJournalUpdateError = "";
        private string _lastCharAssetsUpdateError = "";
        private string _lastCharOrdersUpdateError = "";
        private string _lastCharIndustryJobsUpdateError = "";
        private string _lastCorpTransUpdateError = "";
        private string _lastCorpJournalUpdateError = "";
        private string _lastCorpAssetsUpdateError = "";
        private string _lastCorpOrdersUpdateError = "";
        private string _lastCorpIndustryJobsUpdateError = "";

        private long _highestCharTransID = 0;
        private long _highestCorpTransID = 0;
        private long _highestCharJournalID = 0;
        private long _highestCorpJournalID = 0;
        #endregion

        public APISettingsAndStatus(long charID)
        {
            _charID = charID;
            InitSettings();
        }

        public APICharSettings Settings
        {
            get { return _settings; }
        }

        /// <summary>
        /// Initialise the settings object based upon the current character ID
        /// </summary>
        private void InitSettings()
        {
            EMMADataSet.APICharSettingsDataTable settingsTable = new EMMADataSet.APICharSettingsDataTable();
            settingsTableAdapter.FillByChar(settingsTable, _charID);
            if (settingsTable.Count > 0)
            {
                XmlDocument settingsDoc = new XmlDocument();
                settingsDoc.LoadXml(settingsTable[0].Settings);
                _settings = new APICharSettings(settingsDoc);
            }
            else
            {
                _settings = new APICharSettings(_charID);
            }
            _settings.SettingsUpdated += new SettingsUpdatedHandler(_settings_SettingsUpdated);
        }

        void _settings_SettingsUpdated(object myObject, EventArgs args)
        {
            StoreSettings();
        }

        /// <summary>
        /// Store the character's settings in the database
        /// </summary>
        public void StoreSettings()
        {
            // If it's null then it's not been accessed and there are no changes to store..
            if (_settings != null)
            {
                if (_settings.Changed)
                {
                    EMMADataSet.APICharSettingsDataTable settingsTable = new EMMADataSet.APICharSettingsDataTable();
                    lock (settingsTableAdapter)
                    {
                        settingsTableAdapter.FillByChar(settingsTable, _charID);
                        if (settingsTable.Count == 0)
                        {
                            EMMADataSet.APICharSettingsRow newRow = settingsTable.NewAPICharSettingsRow();
                            newRow.CharID = _charID;
                            // Just make this blank temporarilly so we are allowed to add it to the table.
                            newRow.Settings = "";
                            settingsTable.AddAPICharSettingsRow(newRow);
                        }
                        settingsTable[0].Settings = _settings.Xml.InnerXml;

                        settingsTableAdapter.Update(settingsTable);
                    }
                    _settings.Changed = false;
                }
            }
        }

        public DateTime GetLastUpdateTime(CharOrCorp corc, APIDataType type)
        {
            DateTime retVal = SqlDateTime.MinValue.Value;
            switch (corc)
            {
                case CharOrCorp.Char:
                    switch (type)
                    {
                        case APIDataType.Transactions:
                            retVal = _lastCharTransUpdate;
                            break;
                        case APIDataType.Journal:
                            retVal = _lastCharJournalUpdate;
                            break;
                        case APIDataType.Assets:
                            retVal = _lastCharAssetsUpdate;
                            break;
                        case APIDataType.Orders:
                            retVal = _lastCharOrdersUpdate;
                            break;
                        case APIDataType.IndustryJobs:
                            retVal = _settings.LastCharIndustryJobsUpdate;
                            break;
                        default:
                            break;
                    }
                    break;
                case CharOrCorp.Corp:
                    switch (type)
                    {
                        case APIDataType.Transactions:
                            retVal = _lastCorpTransUpdate;
                            break;
                        case APIDataType.Journal:
                            retVal = _lastCorpJournalUpdate;
                            break;
                        case APIDataType.Assets:
                            retVal = _lastCorpAssetsUpdate;
                            break;
                        case APIDataType.Orders:
                            retVal = _lastCorpOrdersUpdate;
                            break;
                        case APIDataType.IndustryJobs:
                            retVal = _settings.LastCorpIndustryJobsUpdate;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            return retVal;
        }
        public void SetLastUpdateTime(CharOrCorp corc, APIDataType type, DateTime time)
        {
            switch (corc)
            {
                case CharOrCorp.Char:
                    switch (type)
                    {
                        case APIDataType.Transactions:
                            _lastCharTransUpdate = time;
                            break;
                        case APIDataType.Journal:
                            _lastCharJournalUpdate = time;
                            break;
                        case APIDataType.Assets:
                            _lastCharAssetsUpdate = time;
                            break;
                        case APIDataType.Orders:
                            _lastCharOrdersUpdate = time;
                            break;
                        case APIDataType.IndustryJobs:
                            _settings.LastCharIndustryJobsUpdate = time;
                            break;
                        default:
                            break;
                    }
                    break;
                case CharOrCorp.Corp:
                    switch (type)
                    {
                        case APIDataType.Transactions:
                            _lastCorpTransUpdate = time;
                            break;
                        case APIDataType.Journal:
                            _lastCorpJournalUpdate = time;
                            break;
                        case APIDataType.Assets:
                            _lastCorpAssetsUpdate = time;
                            break;
                        case APIDataType.Orders:
                            _lastCorpOrdersUpdate = time;
                            break;
                        case APIDataType.IndustryJobs:
                            _settings.LastCorpIndustryJobsUpdate = time;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        public bool GetAutoUpdateFlag(CharOrCorp corc, APIDataType type)
        {
            bool retVal = false;
            switch (corc)
            {
                case CharOrCorp.Char:
                    switch (type)
                    {
                        case APIDataType.Transactions:
                            retVal = _autoUpdateCharTrans;
                            break;
                        case APIDataType.Journal:
                            retVal = _autoUpdateCharJournal;
                            break;
                        case APIDataType.Assets:
                            retVal = _autoUpdateCharAssets;
                            break;
                        case APIDataType.Orders:
                            retVal = _autoUpdateCharOrders;
                            break;
                        case APIDataType.IndustryJobs:
                            retVal = _autoUpdateCharIndustryJobs;
                            break;
                        default:
                            break;
                    }
                    break;
                case CharOrCorp.Corp:
                    switch (type)
                    {
                        case APIDataType.Transactions:
                            retVal = _autoUpdateCorpTrans;
                            break;
                        case APIDataType.Journal:
                            retVal = _autoUpdateCorpJournal;
                            break;
                        case APIDataType.Assets:
                            retVal = _autoUpdateCorpAssets;
                            break;
                        case APIDataType.Orders:
                            retVal = _autoUpdateCorpOrders;
                            break;
                        case APIDataType.IndustryJobs:
                            retVal = _autoUpdateCorpIndustryJobs;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            return retVal;
        }
        public void SetAutoUpdateFlag(CharOrCorp corc, APIDataType type, bool auto)
        {
            switch (corc)
            {
                case CharOrCorp.Char:
                    switch (type)
                    {
                        case APIDataType.Transactions:
                            _autoUpdateCharTrans = auto;
                            break;
                        case APIDataType.Journal:
                            _autoUpdateCharJournal = auto;
                            break;
                        case APIDataType.Assets:
                            _autoUpdateCharAssets = auto;
                            break;
                        case APIDataType.Orders:
                            _autoUpdateCharOrders = auto;
                            break;
                        case APIDataType.IndustryJobs:
                            _autoUpdateCharIndustryJobs = auto;
                            break;
                        default:
                            break;
                    }
                    break;
                case CharOrCorp.Corp:
                    switch (type)
                    {
                        case APIDataType.Transactions:
                            _autoUpdateCorpTrans = auto;
                            break;
                        case APIDataType.Journal:
                            _autoUpdateCorpJournal = auto;
                            break;
                        case APIDataType.Assets:
                            _autoUpdateCorpAssets = auto;
                            break;
                        case APIDataType.Orders:
                            _autoUpdateCorpOrders = auto;
                            break;
                        case APIDataType.IndustryJobs:
                            _autoUpdateCorpIndustryJobs = auto;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        public string GetLastUpdateError(CharOrCorp corc, APIDataType type)
        {
            string retVal = "";
            switch (corc)
            {
                case CharOrCorp.Char:
                    switch (type)
                    {
                        case APIDataType.Transactions:
                            retVal = _lastCharTransUpdateError;
                            break;
                        case APIDataType.Journal:
                            retVal = _lastCharJournalUpdateError;
                            break;
                        case APIDataType.Assets:
                            retVal = _lastCharAssetsUpdateError;
                            break;
                        case APIDataType.Orders:
                            retVal = _lastCharOrdersUpdateError;
                            break;
                        case APIDataType.IndustryJobs:
                            retVal = _lastCharIndustryJobsUpdateError;
                            break;
                        default:
                            break;
                    }
                    break;
                case CharOrCorp.Corp:
                    switch (type)
                    {
                        case APIDataType.Transactions:
                            retVal = _lastCorpTransUpdateError;
                            break;
                        case APIDataType.Journal:
                            retVal = _lastCorpJournalUpdateError;
                            break;
                        case APIDataType.Assets:
                            retVal = _lastCorpAssetsUpdateError;
                            break;
                        case APIDataType.Orders:
                            retVal = _lastCorpOrdersUpdateError;
                            break;
                        case APIDataType.IndustryJobs:
                            retVal = _lastCorpIndustryJobsUpdateError;
                            break;
                        default:
                            break;
                    } 
                    break;
                default:
                    break;
            }

            return retVal;
        }
        public void SetLastUpdateError(CharOrCorp corc, APIDataType type, string error)
        {
            switch (corc)
            {
                case CharOrCorp.Char:
                    switch (type)
                    {
                        case APIDataType.Transactions:
                            _lastCharTransUpdateError = error;
                            break;
                        case APIDataType.Journal:
                            _lastCharJournalUpdateError = error;
                            break;
                        case APIDataType.Assets:
                            _lastCharAssetsUpdateError = error;
                            break;
                        case APIDataType.Orders:
                            _lastCharOrdersUpdateError = error;
                            break;
                        case APIDataType.IndustryJobs:
                            _lastCharIndustryJobsUpdateError = error;
                            break;
                        default:
                            break;
                    }
                    break;
                case CharOrCorp.Corp:
                    switch (type)
                    {
                        case APIDataType.Transactions:
                            _lastCorpTransUpdateError = error;
                            break;
                        case APIDataType.Journal:
                            _lastCorpJournalUpdateError = error;
                            break;
                        case APIDataType.Assets:
                            _lastCorpAssetsUpdateError = error;
                            break;
                        case APIDataType.Orders:
                            _lastCorpOrdersUpdateError = error;
                            break;
                        case APIDataType.IndustryJobs:
                            _lastCorpIndustryJobsUpdateError = error;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }


        public long GetHighestID(CharOrCorp corc, APIDataType type)
        {
            long retVal = 0;
            switch (corc)
            {
                case CharOrCorp.Char:
                    switch (type)
                    {
                        case APIDataType.Transactions:
                            retVal = _highestCharTransID;
                            break;
                        case APIDataType.Journal:
                            retVal = _highestCharJournalID;
                            break;
                        default:
                            break;
                    }
                    break;
                case CharOrCorp.Corp:
                    switch (type)
                    {
                        case APIDataType.Transactions:
                            retVal = _highestCorpTransID;
                            break;
                        case APIDataType.Journal:
                            retVal = _highestCorpJournalID;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            return retVal;
        }
        public void SetHighestID(CharOrCorp corc, APIDataType type, long id)
        {
            switch (corc)
            {
                case CharOrCorp.Char:
                    switch (type)
                    {
                        case APIDataType.Transactions:
                            _highestCharTransID = id;
                            break;
                        case APIDataType.Journal:
                            _highestCharJournalID = id;
                            break;
                        default:
                            break;
                    }
                    break;
                case CharOrCorp.Corp:
                    switch (type)
                    {
                        case APIDataType.Transactions:
                            _highestCorpTransID = id;
                            break;
                        case APIDataType.Journal:
                            _highestCorpJournalID = id;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }
    }

    
    public enum APIDataType
    {
        Transactions,
        Journal,
        Assets,
        Orders,
        IndustryJobs,
        Unknown,
        Full
    }

    public enum CharOrCorp
    {
        Char,
        Corp
    }
}
