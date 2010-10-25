using System;
using System.Collections.Generic;
using System.Text;

using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public static class GroupLocations
    {
        private static EMMADataSetTableAdapters.GroupLocationTableAdapter tableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.GroupLocationTableAdapter();
        private static EMMADataSet.GroupLocationDataTable locations = new EMMADataSet.GroupLocationDataTable();
        private static int loadedGroup = 0;

        /// <summary>
        /// Does the specified location filter name exist in the database? (for the current report group)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool NameExists(string name)
        {
            LoadLocations();
            EMMADataSet.GroupLocationRow data = locations.FindByReportGroupIDLocationName(loadedGroup, name);
            return data != null;
        }

        /// <summary>
        /// Store the specified group location filter in the database.
        /// </summary>
        /// <param name="data"></param>
        public static void StoreLocation(GroupLocation data)
        {
            LoadLocations();
            EMMADataSet.GroupLocationRow rowData = locations.FindByReportGroupIDLocationName(loadedGroup, data.Name);
            bool newRow = false;

            if (rowData == null)
            {
                rowData = locations.NewGroupLocationRow();
                rowData.LocationName = data.Name;
                newRow = true;
            }
            rowData.ReportGroupID = data.ReportGroupID;
            rowData.Range = data.Range;
            rowData.StationID = data.StationID;
            List<long> regions = data.Regions;
            StringBuilder regionList = new StringBuilder();
            foreach (int region in regions)
            {
                if (regionList.Length != 0) { regionList.Append(","); }
                regionList.Append(region);
            }
            rowData.RegionIDs = regionList.ToString();
            List<long> stations = data.Stations;
            StringBuilder stationList = new StringBuilder();
            foreach (int station in stations)
            {
                if (stationList.Length != 0) { stationList.Append(","); }
                stationList.Append(station);
            }
            rowData.StationIDs = stationList.ToString();

            if (newRow) { locations.AddGroupLocationRow(rowData); }
            tableAdapter.Update(locations);
            locations.AcceptChanges();
        }

        /// <summary>
        /// Get the names of all location filters for the currently active report group
        /// </summary>
        /// <returns></returns>
        public static List<string> GetLocationNames() 
        {
            List<string> retVal = new List<string>();
            LoadLocations();

            foreach (EMMADataSet.GroupLocationRow location in locations)
            {
                retVal.Add(location.LocationName.Trim());
            }

            return retVal;
        }

        /// <summary>
        /// Get the data for all location filters for the currently active report group
        /// </summary>
        /// <returns></returns>
        public static GroupLocationsList GetGroupLocationsData()
        {
            GroupLocationsList retVal = new GroupLocationsList();
            LoadLocations();

            foreach (EMMADataSet.GroupLocationRow location in locations)
            {
                retVal.Add(new GroupLocation(location));
            }

            return retVal;
        }

        /// <summary>
        /// Get the detail of the specified location filter.
        /// Note that it must be a filter belonging to the currently active report group.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GroupLocation GetLocationDetail(string name)
        {
            LoadLocations();
            name.PadRight(50, ' ');
            EMMADataSet.GroupLocationRow data = locations.FindByReportGroupIDLocationName(loadedGroup, name);
            GroupLocation retVal = new GroupLocation(data);
            return retVal;
        }

        /// <summary>
        /// Permenantly remove the specified location from the database.
        /// </summary>
        /// <param name="name"></param>
        public static void DeleteLocation(string name)
        {
            LoadLocations();

            EMMADataSet.GroupLocationRow data = locations.FindByReportGroupIDLocationName(loadedGroup, name);
            data.Delete();
            tableAdapter.Update(locations);
            locations.AcceptChanges();
        }

        /// <summary>
        /// Load the location filters for the currently active report group from the Emma database
        /// if it is not already stored in memory.
        /// </summary>
        private static void LoadLocations()
        {
            int groupID = 0;
            if (UserAccount.CurrentGroup != null)
            {
                groupID = UserAccount.CurrentGroup.ID;
                if (loadedGroup != groupID)
                {
                    tableAdapter.ClearBeforeFill = true;
                    tableAdapter.FillByReportGroup(locations, groupID);
                    loadedGroup = groupID;
                }
            }
            else
            {
                throw new EMMAException(ExceptionSeverity.Error, "Cannot load group locations if a user " +
                    "is not logged in to a report group.");
            }
        }
    }
}
