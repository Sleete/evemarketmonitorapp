using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.DatabaseClasses
{
    static class Regions
    {
        static private EveDataSetTableAdapters.mapRegionsTableAdapter regionsTableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EveDataSetTableAdapters.mapRegionsTableAdapter();
        static private Cache<int, string> _nameCache = new Cache<int, string>(200);
        static private bool _initalised = false;

        /// <summary>
        /// Get the name of the specified region.
        /// </summary>
        /// <param name="stationID"></param>
        /// <returns></returns>
        static public string GetRegionName(int regionID)
        {
            if (!_initalised) { InitialiseCache(); }
            return _nameCache.Get(regionID);
        }
        
        
        static public EveDataSet.mapRegionsDataTable GetAssetRegions(List<AssetAccessParams> accessList,
            int itemID)
        {
            StringBuilder regionIDs = new StringBuilder("");
            EMMADataSet.IDTableDataTable idTable = Assets.GetInvolvedRegionIDs(accessList, itemID);
            foreach (EMMADataSet.IDTableRow id in idTable)
            {
                regionIDs.Append(" ");
                regionIDs.Append(id.ID);
            }
            EveDataSet.mapRegionsDataTable retVal = new EveDataSet.mapRegionsDataTable();
            lock (regionsTableAdapter)
            {
                regionsTableAdapter.FillByIDs(retVal, regionIDs.ToString());
            }
            Regions.SetWormholeRegionNames(ref retVal);
            return retVal;
        }

        static public EveDataSet.mapRegionsDataTable GetAllRegions()
        {
            EveDataSet.mapRegionsDataTable retVal = new EveDataSet.mapRegionsDataTable();
            regionsTableAdapter.Fill(retVal);
            Regions.SetWormholeRegionNames(ref retVal);
            return retVal;
        }

        static private void SetWormholeRegionNames(ref EveDataSet.mapRegionsDataTable table)
        {
            foreach (EveDataSet.mapRegionsRow region in table)
            {
                // Add region ID to wormhole regions
                if (region.regionName.Trim().ToUpper().Equals("UNKNOWN"))
                {
                    region.regionName = "Unknown (" + region.regionID + ")";
                }
            }
        }

        #region Private Methods
        /// <summary>
        /// Initialise the cache holding region names
        /// </summary>
        static private void InitialiseCache()
        {
            if (!_initalised)
            {
                _nameCache.DataUpdateNeeded += new Cache<int, string>.DataUpdateNeededHandler(NameCache_DataUpdateNeeded);
                _initalised = true;
            }
        }


        /// <summary>
        /// Called when the cache requires a region name to be read from the database
        /// </summary>
        /// <param name="myObject"></param>
        /// <param name="args"></param>
        static void NameCache_DataUpdateNeeded(object myObject, DataUpdateNeededArgs<int, string> args)
        {
            string name = "";

            lock (regionsTableAdapter)
            {
                EveDataSet.mapRegionsDataTable table = new EveDataSet.mapRegionsDataTable();
                regionsTableAdapter.FillByIDs(table, args.Key.ToString());
                if (table.Count > 0)
                {
                    name = table[0].regionName;
                }
            }

            if (name.Equals(""))
            {
                name = "Unknown Region (" + args.Key + ")";
            }
            // Add region ID to wormhole regions
            if (name.Trim().ToUpper().Equals("UNKNOWN"))
            {
                name = "Unknown (" + args.Key + ")";
            }

            args.Data = name;
        }
        #endregion
    }
}
