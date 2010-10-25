using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlTypes;

using EveMarketMonitorApp.AbstractionClasses;
using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.DatabaseClasses
{
    static class APICharacters
    {
        private static EMMADataSetTableAdapters.APICharactersTableAdapter apiCharTableAdapter = 
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.APICharactersTableAdapter();
        private static Cache<string, APICharacter> _cache = new Cache<string, APICharacter>(20);
        private static bool _initalised = false;


        /// <summary>
        /// Store the specified API Character record in the database.
        /// </summary>
        /// <param name="apiChar"></param>
        public static void Store(APICharacter apiChar)
        {
            EMMADataSet.APICharactersDataTable table = new EMMADataSet.APICharactersDataTable();
            lock (apiCharTableAdapter)
            {
                EMMADataSet.APICharactersRow data = LoadCharFromDB(apiChar.CharID);

                bool newRow = false;

                if (data == null)
                {
                    newRow = true;

                    data = table.NewAPICharactersRow();
                    data.ID = apiChar.CharID;
                }
                data.CharSheet = apiChar.CharSheet.InnerXml;
                data.CorpSheet = apiChar.CorpSheet.InnerXml;
                data.LastCharSheetUpdate = apiChar.CharSheetXMLLastUpdate;
                data.LastCorpSheetUpdate = apiChar.CorpSheetXMLLastUpdate;
                data.LastCharAssetsUpdate = apiChar.GetLastAPIUpdateTime(CharOrCorp.Char, APIDataType.Assets);
                data.LastCharJournalUpdate = apiChar.GetLastAPIUpdateTime(CharOrCorp.Char, APIDataType.Journal);
                data.LastCharOrdersUpdate = apiChar.GetLastAPIUpdateTime(CharOrCorp.Char, APIDataType.Orders);
                data.LastCharTransUpdate = apiChar.GetLastAPIUpdateTime(CharOrCorp.Char, APIDataType.Transactions);
                data.LastCorpAssetsUpdate = apiChar.GetLastAPIUpdateTime(CharOrCorp.Corp, APIDataType.Assets);
                data.LastCorpJournalUpdate = apiChar.GetLastAPIUpdateTime(CharOrCorp.Corp, APIDataType.Journal);
                data.LastCorpOrdersUpdate = apiChar.GetLastAPIUpdateTime(CharOrCorp.Corp, APIDataType.Orders);
                data.LastCorpTransUpdate = apiChar.GetLastAPIUpdateTime(CharOrCorp.Corp, APIDataType.Transactions);
                data.CorpFinanceAccess = apiChar.CorpFinanceAccess;
                data.HighestCharJournalID = apiChar.GetHighestID(CharOrCorp.Char, APIDataType.Journal);
                data.HighestCharTransID = apiChar.GetHighestID(CharOrCorp.Char, APIDataType.Transactions);
                data.HighestCorpJournalID = apiChar.GetHighestID(CharOrCorp.Corp, APIDataType.Journal);
                data.HighestCorpTransID = apiChar.GetHighestID(CharOrCorp.Corp, APIDataType.Transactions);

                try
                {
                    if (newRow)
                    {
                        table.AddAPICharactersRow(data);
                        apiCharTableAdapter.Update(table);
                    }
                    else
                    {
                        apiCharTableAdapter.Update(data);
                    }
                }
                catch (Exception ex)
                {
                    throw new EMMADataException(ExceptionSeverity.Critical, "Error storing eve character " +
                        "data in the EMMA database.", ex);
                }
            }
        }

        /// <summary>
        /// Get the specified API Character
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="apiKey"></param>
        /// <param name="charID"></param>
        /// <returns></returns>
        public static APICharacter GetCharacter(long userID, string apiKey, long charID)
        {
            APICharacter retVal = null;
            if (!_initalised) { Initialise(); }
            string key = userID + "|" + apiKey + "|" + charID;
            retVal = _cache.Get(key);
            return retVal;
        }

        /// <summary>
        /// Fired when the cache needs data from the database.
        /// </summary>
        /// <param name="myObject"></param>
        /// <param name="args"></param>
        static void Cache_DataUpdateNeeded(object myObject, DataUpdateNeededArgs<string, APICharacter> args)
        {
            char[] delim = {'|'};
            string[] keyData = args.Key.Split(delim);
            EMMADataSet.APICharactersRow rowData = LoadCharFromDB(long.Parse(keyData[2]));
            if (rowData != null)
            {
                args.Data = new APICharacter(long.Parse(keyData[0]), keyData[1], rowData);
            }
            else
            {
                args.Data = null;
            }
        }

        /// <summary>
        /// Return the specified API character row direct from the EMMA database
        /// </summary>
        /// <returns></returns>
        private static EMMADataSet.APICharactersRow LoadCharFromDB(long charID)
        {
            EMMADataSet.APICharactersRow retVal = null;
            EMMADataSet.APICharactersDataTable charData = new EMMADataSet.APICharactersDataTable();

            lock (apiCharTableAdapter)
            {
                apiCharTableAdapter.ClearBeforeFill = true;
                apiCharTableAdapter.FillByID(charData, charID);
                if (charData != null)
                {
                    if (charData.Count == 1)
                    {
                        retVal = charData[0];
                    }
                }
            }

            return retVal;
        }

        /// <summary>
        /// Setup the data update event handler on the cache.
        /// </summary>
        private static void Initialise()
        {
            if (!_initalised)
            {
                _cache.DataUpdateNeeded +=
                    new Cache<string, APICharacter>.DataUpdateNeededHandler(Cache_DataUpdateNeeded);
                _initalised = true;
            }
        }

    }
}
