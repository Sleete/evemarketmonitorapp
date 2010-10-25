using System;
using System.Collections.Generic;
using System.Text;

using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.DatabaseClasses
{
    static class SolarSystemDistances
    {
        private static EveDataSetTableAdapters.SolarSystemDistancesTableAdapter tableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EveDataSetTableAdapters.SolarSystemDistancesTableAdapter();
        private static Cache<RouteKey, int> _cache = new Cache<RouteKey, int>(100000);
        private static bool _initalised = false;
        private static Dictionary<RouteKey, int> _hitLevel = new Dictionary<RouteKey, int>();

        public static void PopulateJumpsArray(List<long> fromSystemIDs, List<long> toSystemIDs,
            ref short[,] jumps, Dictionary<long, int> idMapper, ref int nextFreeIndex)
        {
            StringBuilder fromString = new StringBuilder("");
            StringBuilder toString = new StringBuilder("");
            List<long> addedIds = new List<long>();

            foreach (int id in fromSystemIDs)
            {
                if (!addedIds.Contains(id))
                {
                    if (fromString.Length > 0) { fromString.Append(","); }
                    fromString.Append(id);
                    addedIds.Add(id);
                }
            }
            addedIds = new List<long>();
            foreach (int id in toSystemIDs)
            {
                if (!addedIds.Contains(id))
                {
                    if (toString.Length > 0) { toString.Append(","); }
                    toString.Append(id);
                    addedIds.Add(id);
                }
            }

            EveDataSet.SolarSystemDistancesDataTable table = new EveDataSet.SolarSystemDistancesDataTable();
            tableAdapter.FillByMultipleIDs(table, fromString.ToString(), toString.ToString());

            foreach (EveDataSet.SolarSystemDistancesRow route in table)
            {
                int startSystemID = route.FromSolarSystemID;
                int endSystemID = route.ToSolarSystemID;
                if (startSystemID > endSystemID)
                {
                    int tmp = startSystemID;
                    startSystemID = endSystemID;
                    endSystemID = tmp;
                }

                int startIndex = 0;
                if (idMapper.ContainsKey(startSystemID))
                {
                    startIndex = idMapper[startSystemID];
                }
                else
                {
                    idMapper.Add(startSystemID, nextFreeIndex);
                    startIndex = nextFreeIndex;
                    nextFreeIndex++;
                }

                int endIndex = 0;
                if (idMapper.ContainsKey(endSystemID))
                {
                    endIndex = idMapper[endSystemID];
                }
                else
                {
                    idMapper.Add(endSystemID, nextFreeIndex);
                    endIndex = nextFreeIndex;
                    nextFreeIndex++;
                }

                jumps[startIndex, endIndex] = (short)route.Distance;
            }
        }

        /// <summary>
        /// Get a list of system IDs for systems that are within the specified number of jumps
        /// of the given system.
        /// </summary>
        /// <param name="systemID"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static List<long> GetSystemsInRange(long systemID, int range)
        {
            List<long> retVal = new List<long>();
            EveDataSet.SolarSystemDistancesDataTable table = new EveDataSet.SolarSystemDistancesDataTable();
            tableAdapter.FillBySystemAndRange(table, (int)systemID, range);
            foreach (EveDataSet.SolarSystemDistancesRow distance in table)
            {
                retVal.Add(distance.ToSolarSystemID);
            }
            return retVal;
        }

        /// <summary>
        /// Set distance data for the specified route
        /// </summary>
        /// <param name="startSystemID"></param>
        /// <param name="endSystemID"></param>
        /// <returns></returns>
        public static void SetDistance(int startSystemID, int endSystemID, int numJumps)
        {
            EveDataSet.SolarSystemDistancesDataTable distances = new EveDataSet.SolarSystemDistancesDataTable();
            LoadDistanceData(startSystemID, endSystemID, distances);
            EveDataSet.SolarSystemDistancesRow newRow = distances.FindByFromSolarSystemIDToSolarSystemID(
                startSystemID, endSystemID);

            if (newRow == null)
            {
                newRow = distances.NewSolarSystemDistancesRow();
                newRow.FromSolarSystemID = startSystemID;
                newRow.ToSolarSystemID = endSystemID;
                newRow.Distance = numJumps;
                distances.AddSolarSystemDistancesRow(newRow);
                lock (tableAdapter)
                {
                    tableAdapter.Update(distances);
                }
                distances.AcceptChanges();
            }
            else
            {
                newRow.Distance = numJumps;
                lock (tableAdapter)
                {
                    tableAdapter.Update(newRow);
                }
                distances.AcceptChanges();
            }
        }

        /// <summary>
        /// Get distance data for the specified route
        /// </summary>
        /// <param name="startSystemID"></param>
        /// <param name="endSystemID"></param>
        /// <returns></returns>
        public static int GetDistance(int startSystemID, int endSystemID)
        {
            int[] tmp = new int[4];
            return GetDistance(startSystemID, endSystemID, ref tmp);
        }

        public static int GetDistance(int startSystemID, int endSystemID, ref int[] diagnostics)
        {
            int retVal = -1;
            if (!_initalised) { Initalise(); }

            RouteKey key = new RouteKey(startSystemID, endSystemID);
            _hitLevel.Add(key, 1);
            retVal = _cache.Get(key);

            if (_hitLevel.ContainsKey(key))
            {
                diagnostics[_hitLevel[key]]++;
                _hitLevel.Remove(key);
            }

            return retVal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startStationID"></param>
        /// <param name="endStationID"></param>
        /// <returns></returns>
        public static int GetDistanceBetweenStations(long startStationID, long endStationID)
        {
            EveDataSet.staStationsRow startStation = Stations.GetStation(startStationID);
            EveDataSet.staStationsRow endStation = Stations.GetStation(endStationID);

            return GetDistance(startStation.solarSystemID, endStation.solarSystemID);
        }

        /// <summary>
        /// Initalise cache structure
        /// </summary>
        private static void Initalise()
        {
            if (!_initalised)
            {
                _cache.DataUpdateNeeded += new Cache<RouteKey, int>.DataUpdateNeededHandler(
                    _cache_DataUpdateNeeded);
                _initalised = true;
            }
        }

        /// <summary>
        /// Called when the cache does not contain the requested route length.
        /// Attempt to get length from the database, if it's not there then calculate it and
        /// store the result in the database.
        /// </summary>
        /// <param name="myObject"></param>
        /// <param name="args"></param>
        static void _cache_DataUpdateNeeded(object myObject, DataUpdateNeededArgs<RouteKey, int> args)
        {
            int jumps = int.MaxValue;
            int startSystemID = args.Key.StartSystem;
            int endSystemID = args.Key.EndSystem;

            if (_hitLevel.ContainsKey(args.Key))
            {
                _hitLevel.Remove(args.Key);
            }

            EveDataSet.SolarSystemDistancesDataTable distances = new EveDataSet.SolarSystemDistancesDataTable();
            
            // try the database...
            LoadDistanceData(startSystemID, endSystemID, distances);
            EveDataSet.SolarSystemDistancesRow distanceData =
                distances.FindByFromSolarSystemIDToSolarSystemID(startSystemID, endSystemID);

            if (distanceData != null)
            {
                jumps = distanceData.Distance;
                _hitLevel.Add(args.Key, 2);
            }
            else
            {
                LoadDistanceData(endSystemID, startSystemID, distances);
                distanceData = distances.FindByFromSolarSystemIDToSolarSystemID(endSystemID, startSystemID);
                if (distanceData != null)
                {
                    jumps = distanceData.Distance;
                    SetDistance(startSystemID, endSystemID, jumps);
                    _hitLevel.Add(args.Key, 2);
                }
                else
                {
                    // If it's not in the database then we need to calculate it.
                    Map.SetCosts(1, 1, 1);
                    jumps = Map.CalcRouteLength(startSystemID, endSystemID);
                    SetDistance(startSystemID, endSystemID, jumps);
                    _hitLevel.Add(args.Key, 3);
                }
            }

            args.Data = jumps;
        }

        /// <summary>
        /// Load the data for the specified route into the specified data table
        /// </summary>
        /// <param name="startSystem"></param>
        /// <param name="endSystem"></param>
        /// <param name="distances"></param>
        private static void LoadDistanceData(int startSystem, int endSystem, 
            EveDataSet.SolarSystemDistancesDataTable distances)
        {
            lock (tableAdapter)
            {
                try
                {
                    tableAdapter.FillByIDs(distances, startSystem, endSystem);
                }
                catch (Exception ex)
                {
                    throw new EMMADataException(ExceptionSeverity.Critical, "Problem loading distance " +
                        "data from the EMMA database. (" + SolarSystems.GetSystemName(startSystem) +
                        " -> " + SolarSystems.GetSystemName(endSystem) + ")", ex);
                }
            }
        }



    }

        /// <summary>
        /// 
        /// </summary>
        public class RouteKey
        {
            private int _startSystemID;
            private int _endSystemID;

            public RouteKey(int startSystemID, int endSystemID)
            {
                _startSystemID = startSystemID;
                _endSystemID = endSystemID;
            }

            public override bool Equals(object obj)
            {
                bool retVal = false;
                RouteKey other = obj as RouteKey;
                if (other != null)
                {
                    if (other.StartSystem == _startSystemID && other.EndSystem == _endSystemID)
                    {
                        retVal = true;
                    }
                }

                return retVal;
            }

            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }

            public override string ToString()
            {
                return "start=" + _startSystemID.ToString() + ",end=" + _endSystemID.ToString();
            }

            public int StartSystem
            {
                get { return _startSystemID; }
            }

            public int EndSystem
            {
                get { return _endSystemID; }
            }
        }
}
