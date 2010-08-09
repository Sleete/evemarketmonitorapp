using System;
using System.Collections.Generic;
using System.Text;

using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.AbstractionClasses;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public static class ReportGroups
    {
        private static EMMADataSetTableAdapters.ReportGroupsTableAdapter reportGroupsTableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.ReportGroupsTableAdapter();
        private static EMMADataSetTableAdapters.RptGroupCharsTableAdapter groupCharsTableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.RptGroupCharsTableAdapter();
        private static EMMADataSetTableAdapters.RptGroupCorpsTableAdapter groupCorpsTableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.RptGroupCorpsTableAdapter();


/*        public static void LinkTutorialGroup(string userName)
        {
            EMMADataSetTableAdapters.UserRptGroupsTableAdapter adapter = 
                new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.UserRptGroupsTableAdapter();
            EMMADataSet.UserRptGroupsDataTable table = new EMMADataSet.UserRptGroupsDataTable();
            EMMADataSet.UserRptGroupsRow rowData = table.NewUserRptGroupsRow();
            rowData.UserName = userName;
            rowData.RptGroupID = 1;
            table.AddUserRptGroupsRow(rowData);
            adapter.Update(table);
        }
*/

        public static void SetGroupAccounts(int reportGroupID, List<EVEAccount> accounts)
        {
            StringBuilder accountIDs = new StringBuilder("");
            foreach(EVEAccount account in accounts) 
            {
                if(accountIDs.Length != 0) 
                {
                    accountIDs.Append(",");
                }
                accountIDs.Append(account.UserID.ToString());
            }
            lock (reportGroupsTableAdapter)
            {
                reportGroupsTableAdapter.SetAccounts(reportGroupID, accountIDs.ToString());
            }
        }

        public static void SetCharGroupSettings(int reportGroupID, int APICharID, bool included, bool autoTrans,
            bool autoJournal, bool autoAssets, bool autoOrders, bool autoIndustryJobs)
        {
            lock (groupCharsTableAdapter)
            {
                groupCharsTableAdapter.Store(reportGroupID, APICharID, included, autoTrans, autoJournal,
                    autoAssets, autoOrders, autoIndustryJobs);
            }
        }
        public static void SetCorpGroupSettings(int reportGroupID, int APICorpID, bool included, bool autoTrans,
            bool autoJournal, bool autoAssets, bool autoOrders, bool autoIndustryJobs, int APICharID)
        {
            lock (groupCorpsTableAdapter)
            {
                groupCorpsTableAdapter.Store(reportGroupID, APICorpID, included, autoTrans, autoJournal,
                    autoAssets, autoOrders, autoIndustryJobs, APICharID);
            }
        }

        /// <summary>
        /// Get the the report group level settings for the specified character
        /// </summary>
        /// <param name="reportGroupID"></param>
        /// <param name="APICharID"></param>
        /// <param name="autoUpdateTrans"></param>
        /// <param name="autoUpdateJournal"></param>
        /// <param name="autoUpdateAssets"></param>
        /// <param name="autoUpdateOrders"></param>
        /// <returns>True if the char is in the report group, false otherwise</returns>
        public static bool GroupCharSettings(int reportGroupID, int APICharID, ref bool autoUpdateTrans,
            ref bool autoUpdateJournal, ref bool autoUpdateAssets, ref bool autoUpdateOrders,
            ref bool autoUpdateIndustryJobs)
        {
            bool retVal = true;
            EMMADataSet.RptGroupCharsDataTable table = new EMMADataSet.RptGroupCharsDataTable();

            lock (groupCharsTableAdapter)
            {
                groupCharsTableAdapter.FillByIDs(table, reportGroupID, APICharID);
            }
            if (table == null || table.Count == 0)
            {
                retVal = false;
            }
            else
            {
                autoUpdateTrans = table[0].AutoUpdateTrans;
                autoUpdateOrders = table[0].AutoUpdateOrders;
                autoUpdateJournal = table[0].AutoUpdateJournal;
                autoUpdateAssets = table[0].AutoUpdateAssets;
                autoUpdateIndustryJobs = table[0].AutoUpdateIndustryJobs;
            }

            return retVal;
        }
        /// <summary>
        /// Get the the report group level settings for the specified corp
        /// </summary>
        /// <param name="reportGroupID"></param>
        /// <param name="APICorpID"></param>
        /// <param name="autoUpdateTrans"></param>
        /// <param name="autoUpdateJournal"></param>
        /// <param name="autoUpdateAssets"></param>
        /// <param name="autoUpdateOrders"></param>
        /// <returns>True if the corp is in the report group, false otherwise</returns>
        public static bool GroupCorpSettings(int reportGroupID, int APICorpID, ref bool autoUpdateTrans,
            ref bool autoUpdateJournal, ref bool autoUpdateAssets, ref bool autoUpdateOrders,
            ref bool autoUpdateIndustryJobs, int APICharID)
        {
            bool retVal = true;
            EMMADataSet.RptGroupCorpsDataTable table = new EMMADataSet.RptGroupCorpsDataTable();

            lock (groupCorpsTableAdapter)
            {
                groupCorpsTableAdapter.FillByIDs(table, reportGroupID, APICorpID, APICharID);
            }
            if (table == null || table.Count == 0)
            {
                retVal = false;
            }
            else
            {
                autoUpdateTrans = table[0].AutoUpdateTrans;
                autoUpdateOrders = table[0].AutoUpdateOrders;
                autoUpdateJournal = table[0].AutoUpdateJournal;
                autoUpdateAssets = table[0].AutoUpdateAssets;
                autoUpdateIndustryJobs = table[0].AutoUpdateIndustryJobs;
            }

            return retVal;
        }

        /// <summary>
        /// Create a new Report Group linked to the specified user account.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="groupName"></param>
        /// <param name="publicAccess"></param>
        public static void NewGroup(string username, string groupName, bool publicAccess)
        {
            List<ReportGroup> currentGroups = GetUsersGroups(username, false);
            if (currentGroups.Count >= 6)
            {
                throw new EMMAException(ExceptionSeverity.Warning, "User '" + username + "' already has " +
                    "6 report groups. Unable to create more.");
            }

            lock (reportGroupsTableAdapter)
            {
                reportGroupsTableAdapter.New(username, groupName, publicAccess);
            }
        }


        /// <summary>
        /// Get the specified user's access level to the specified report group.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="groupID"></param>
        /// <returns></returns>
        public static GroupAccess GetAccessLevel(string username, int groupID)
        {
            GroupAccess retVal = GroupAccess.ReadOnly;
            bool? fullAccess = false;
            lock (reportGroupsTableAdapter)
            {
                reportGroupsTableAdapter.UserHasFullAccess(username, groupID, ref fullAccess);
            }
            if (fullAccess.HasValue)
            {
                if (fullAccess.Value)
                {
                    retVal = GroupAccess.Full;
                }
            }
            return retVal;
        }

        /// <summary>
        /// Get a list of the report groups associated with the specified user account.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static List<ReportGroup> GetUsersGroups(string username, bool includePublic)
        {
            List<ReportGroup> retVal = new List<ReportGroup>();
            EMMADataSet.ReportGroupsDataTable data = new EMMADataSet.ReportGroupsDataTable();

            lock (reportGroupsTableAdapter)
            {
                reportGroupsTableAdapter.ClearBeforeFill = true;
                reportGroupsTableAdapter.FillByUser(data, username.Trim(), includePublic);
            }
            foreach (EMMADataSet.ReportGroupsRow grp in data)
            {
                retVal.Add(new ReportGroup(grp));
            }
            return retVal;
        }


        /// <summary>
        /// Delete the specified reporting group from the database
        /// </summary>
        /// <param name="ID"></param>
        static public void Delete(int ID)
        {
            try
            {
                lock (reportGroupsTableAdapter)
                {
                    reportGroupsTableAdapter.Delete1(ID);
                }
            }
            catch (Exception ex)
            {
                throw new EMMADataException(ExceptionSeverity.Error, "Problem removing reporting" +
                    " group data for group " + ID + ".", ex);
            }
        }


        /// <summary>
        /// Load group data from the database
        /// </summary>
        /*static private void LoadData(int ID)
        {
            try
            {
                reportGroupsTableAdapter.ClearBeforeFill = false;
                // Retrieve data
                reportGroupsTableAdapter.FillByID(data, ID);
            }
            catch (Exception ex)
            {
                throw new EMMADataException(ExceptionSeverity.Critical, "Problem loading reporting group " +
                    "data for group " + ID + " from the EMMA database.", ex);
            }
        }*/

    }
}
