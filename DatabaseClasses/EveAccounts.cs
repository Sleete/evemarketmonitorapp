using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlTypes;

using EveMarketMonitorApp.AbstractionClasses;
using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public static class EveAccounts
    {
        private static EMMADataSetTableAdapters.EveAccountsTableAdapter eveAccountsTableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.EveAccountsTableAdapter();
        private static Cache<int, EVEAccount> _cache = new Cache<int, EVEAccount>(50);
        private static bool _initalised = false;

        public static bool AccountInUse(int accountID)
        {
            bool? retVal = false;
            eveAccountsTableAdapter.InUse(accountID, ref retVal);
            return (retVal.HasValue ? retVal.Value : false);
        }

        /// <summary>
        /// Setup the data update event handler on the cache.
        /// </summary>
        private static void Initialise() 
        {
            if (!_initalised)
            {
                _cache.DataUpdateNeeded +=
                    new Cache<int, EVEAccount>.DataUpdateNeededHandler(Cache_DataUpdateNeeded);
                _initalised = true;
            }
        }

        /// <summary>
        /// Get a list of the Eve accounts that are part of the specified report group
        /// </summary>
        /// <param name="rptGroupID"></param>
        /// <returns></returns>
        public static List<EVEAccount> GetGroupAccounts(int rptGroupID)
        {
            EMMADataSet.EveAccountsDataTable accountsData = new EMMADataSet.EveAccountsDataTable();
            List<EVEAccount> retVal = new List<EVEAccount>();

            eveAccountsTableAdapter.ClearBeforeFill = true;
            eveAccountsTableAdapter.FillByReportGroup(accountsData, rptGroupID);
            foreach (EMMADataSet.EveAccountsRow account in accountsData)
            {
                retVal.Add(new EVEAccount(account));
            }
            return retVal;
        }


        /// <summary>
        /// Store the specified Eve account in the EMMA database.
        /// </summary>
        /// <param name="account"></param>
        public static void Store(EVEAccount account)
        {
            EMMADataSet.EveAccountsDataTable table = new EMMADataSet.EveAccountsDataTable();
            EMMADataSet.EveAccountsRow data = LoadAccountFromDB(account.UserID);
            bool newRow = false;

            if (data == null)
            {
                newRow = true;

                data = table.NewEveAccountsRow();
                data.UserID = account.UserID;
            }
            data.APIKey = account.ApiKey;
            data.CharList = account.CharList.InnerXml;
            data.LastCharListUpdate = account.LastcharListUpdate;
            if (data.LastCharListUpdate.CompareTo(SqlDateTime.MinValue.Value) < 0)
            {
                data.LastCharListUpdate = SqlDateTime.MinValue.Value;
            }

            try
            {
                if (newRow)
                {
                    table.AddEveAccountsRow(data);
                    eveAccountsTableAdapter.Update(table);
                }
                else
                {
                    eveAccountsTableAdapter.Update(data);
                }
            }
            catch (Exception ex)
            {
                throw new EMMADataException(ExceptionSeverity.Critical, "Error storing eve account data in the " +
                    "EMMA database.", ex);
            }
        }

        /// <summary>
        /// Return the specified Eve Account.
        /// If the account does not exist in the database then the returned object is null.
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public static EVEAccount GetAccount(int userID)
        {
            EVEAccount retVal = null;
            if (!_initalised) { Initialise(); }
            retVal = _cache.Get(userID);
            return retVal;
        }

        /// <summary>
        /// Fired when the cache needs data from the database.
        /// </summary>
        /// <param name="myObject"></param>
        /// <param name="args"></param>
        static void Cache_DataUpdateNeeded(object myObject, DataUpdateNeededArgs<int, EVEAccount> args)
        {
            EMMADataSet.EveAccountsRow rowData = LoadAccountFromDB(args.Key);
            if (rowData != null)
            {
                args.Data = new EVEAccount(rowData);
            }
            else
            {
                args.Data = null;
            }
        }

        /// <summary>
        /// Return the specified account row direct from the EMMA database
        /// </summary>
        /// <returns></returns>
        private static EMMADataSet.EveAccountsRow LoadAccountFromDB(long userID)
        {
            EMMADataSet.EveAccountsRow retVal = null;
            EMMADataSet.EveAccountsDataTable accountsData = new EMMADataSet.EveAccountsDataTable();

            eveAccountsTableAdapter.ClearBeforeFill = true;
            eveAccountsTableAdapter.FillByID(accountsData, userID);
            if (accountsData != null)
            {
                if (accountsData.Count == 1)
                {
                    retVal = accountsData[0];
                }
            }
            return retVal;
        }

    }
}
