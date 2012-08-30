using System;
using System.Collections.Generic;
using System.Text;

namespace EveMarketMonitorApp.DatabaseClasses
{
    static class MarketGroups
    {
        private static EveDataSetTableAdapters.invMarketGroupsTableAdapter tableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EveDataSetTableAdapters.invMarketGroupsTableAdapter();

        /// <summary>
        /// Get a table containing all groups (including parents) that contain items that 
        /// have transactions matching the specified parameters.
        /// </summary>
        /// <param name="accessList"></param>
        /// <returns></returns>
        public static EveDataSet.invMarketGroupsDataTable GetGroupsForItems(List<int> itemIDs)
        {
            EveDataSet.invMarketGroupsDataTable retVal = new EveDataSet.invMarketGroupsDataTable();
            // Build a string contianing all the ids of items we want to get groups for.
            StringBuilder itemList = new StringBuilder();
            foreach (int id in itemIDs)
            {
                if (itemList.Length != 0) { itemList.Append(","); }
                itemList.Append(id);
            }
            tableAdapter.FillByItems(retVal, itemList.ToString());
            // The SQL procedure only returns the first level group that the item is in, we want
            // all the parent groups as well so use this recursive method to get them...
            EveDataSet.invMarketGroupsDataTable parentGroups = new EveDataSet.invMarketGroupsDataTable();
            foreach (EveDataSet.invMarketGroupsRow group in retVal)
            {
                if (!group.IsparentGroupIDNull())
                {
                    AddGroupParents(parentGroups, group.parentGroupID);
                }
            }
            // ..then we can add them all to the result datatable.
            foreach (EveDataSet.invMarketGroupsRow group in parentGroups)
            {
                EveDataSet.invMarketGroupsRow oldGroup = retVal.FindBymarketGroupID(group.marketGroupID);
                if (oldGroup == null)
                {
                    retVal.ImportRow(group);
                }
            }


            return retVal;
        }

        /// <summary>
        /// recursively add the parent groups of the specified group id to the given table.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="groupID"></param>
        private static void AddGroupParents(EveDataSet.invMarketGroupsDataTable table, int groupID)
        {
            EveDataSet.invMarketGroupsRow group = table.FindBymarketGroupID(groupID);
            if (group == null)
            {
                EveDataSet.invMarketGroupsDataTable resultTable = new EveDataSet.invMarketGroupsDataTable();
                tableAdapter.FillByID(resultTable, groupID);
                table.ImportRow(resultTable[0]);
                group = table.FindBymarketGroupID(groupID);
            }

            if (!group.IsparentGroupIDNull())
            {
                AddGroupParents(table, group.parentGroupID);
            }
        }

    }
}
