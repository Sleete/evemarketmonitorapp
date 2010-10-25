using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;

using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public static class Map
    {
        #region Class variables
        private const int _numSystemsInEve = 20000;
        private const int _maxNeighbours = 8;
        private static bool _initalised = false;
        private static long[][] _systemNeighbours = new long[_numSystemsInEve][];
        private static float[] _systemSec = new float[_numSystemsInEve];
        private static Dictionary<long, int> _idToIndex = new Dictionary<long, int>();

        private static int _highSecCost = 1;
        private static int _lowSecCost = 1;
        private static int _nullSecCost = 1;
        #endregion

        #region Public methods
        public static float GetSecurity(long systemID)
        {
            float retVal = 0;
            if (_idToIndex.ContainsKey(systemID))
            {
                retVal = _systemSec[_idToIndex[systemID]];
            }
            return retVal;
        }

        public static void SetCosts(int highSecCost, int lowSecCost, int nullSecCost)
        {
            _highSecCost = highSecCost;
            _lowSecCost = lowSecCost;
            _nullSecCost = nullSecCost;
        }

        /// <summary>
        /// Make sure to call SetCosts first to setup jump cost values.
        /// </summary>
        /// <param name="startSystemID"></param>
        /// <param name="endSystemID"></param>
        /// <returns></returns>
        public static List<long> GetRoute(long startSystemID, long endSystemID)
        {
            return GetRouteDijkstra(startSystemID, endSystemID);
        }

        /// <summary>
        /// Make sure to call SetCosts first to setup jump cost values.
        /// </summary>
        /// <param name="startStationID"></param>
        /// <param name="endStationID"></param>
        /// <returns></returns>
        public static int CalcRouteLengthBetweenStations(long startStationID, long endStationID)
        {
            EveDataSet.staStationsRow startStation = Stations.GetStation(startStationID);
            EveDataSet.staStationsRow endStation = Stations.GetStation(endStationID);

            return CalcRouteLength(startStation.solarSystemID, endStation.solarSystemID);
        }

        /// <summary>
        /// Make sure to call SetCosts first to setup jump cost values.
        /// </summary>
        /// <param name="startSystemID"></param>
        /// <param name="endSystemID"></param>
        /// <returns></returns>
        public static int CalcRouteLength(long startSystemID, long endSystemID)
        {
            List<long> route;
            //route = GetSystemRouteDijkstra(startSystemID, endSystemID);
            route = GetRouteDijkstra(startSystemID, endSystemID);

            // note: the route list contains the ids of all the systems in the route including
            // the start system, to get number of jumps, we need to remove 1.
            return route.Count - 1;
        }
        #endregion

        #region Dijkstra
        private static List<long> GetRouteDijkstra(long startSystemID, long endSystemID)
        {
            Dictionary<int, long> indexToId = new Dictionary<int, long>();
            Dictionary<long, int> idToIndex = new Dictionary<long, int>();
            int nextFreeIndex = 0;
            short[] distance = new short[_numSystemsInEve];
            int[] previous = new int[_numSystemsInEve];
            bool[] vistited = new bool[_numSystemsInEve];

            indexToId.Add(0, startSystemID);
            idToIndex.Add(startSystemID, 0);
            nextFreeIndex = 1;
            if (!idToIndex.ContainsKey(endSystemID))
            {
                indexToId.Add(1, endSystemID);
                idToIndex.Add(endSystemID, 1);
                nextFreeIndex = 2;
            }
            bool complete = false;

            distance[0] = 0;
            for (int i = 1; i < _numSystemsInEve; i++)
            {
                distance[i] = short.MaxValue;
                previous[i] = int.MaxValue;
                vistited[i] = false;
            }

            if (startSystemID == endSystemID) { complete = true; }

            int iterations = 0;
            while (!complete)
            {
                iterations++;
                int minIndex = 0;
                short minDistance = short.MaxValue;
                for (int i = 0; i < _numSystemsInEve; i++)
                {
                    if (distance[i] < minDistance && !vistited[i]) { minDistance = distance[i]; minIndex = i; }
                    // This optimisation can only be made because the distance between each node is 
                    // always 1 - it must be removed if using weighted paths.
                    //if (distance[i] == short.MaxValue) { i = _numSystemsInEve; }
                }

                if (minDistance >= distance[idToIndex[endSystemID]]) 
                { 
                    complete = true; 
                }
                else
                {
                    vistited[minIndex] = true;
                    long[] systems = GetNeighbourSystems(indexToId[minIndex]);
                    for (int i = 0; i < _maxNeighbours; i++)
                    {
                        long system = systems[i];
                        if (system != 0)
                        {
                            //if (SolarSystems.InConstellation(system, constellations))
                            //{
                            if (!idToIndex.ContainsKey(system))
                            {
                                idToIndex.Add(system, nextFreeIndex);
                                indexToId.Add(nextFreeIndex, system);
                                nextFreeIndex++;
                            }
                            int jumpCost = 1;
                            float security = 0;
                            //if (_idToIndex.ContainsKey(system))
                            //{
                                security = _systemSec[_idToIndex[system]];
                            //}
                            if (security <= 0)
                            {
                                jumpCost = _nullSecCost;
                            }
                            else if (security > 0.45)
                            {
                                jumpCost = _highSecCost;
                            }
                            else
                            {
                                jumpCost = _lowSecCost;
                            }
                            

                            short possible = (short)(distance[minIndex] + jumpCost);
                            if (possible < distance[idToIndex[system]])
                            {
                                distance[idToIndex[system]] = possible;
                                previous[idToIndex[system]] = minIndex;
                            }
                            // This optimisation can only be made because the distance between each node is 
                            // always 1 - it must be removed if using weighted paths. (need some other way
                            // to say the algorith is complete though.
                            //if (system == endSystemID) { complete = true; }
                            //}
                        }
                    }
                }
            }

            List<long> route = new List<long>();
            long systemId = endSystemID;
            route.Add(systemId);
            while (systemId != startSystemID)
            {
                long prevId = indexToId[previous[idToIndex[systemId]]];
                route.Add(prevId);
                systemId = prevId;
            }

            route.Reverse();
            return route;
        }

        private static long[] GetNeighbourSystems(long systemID)
        {
            int index = _idToIndex[systemID];
            return _systemNeighbours[index];
        }
        #endregion

        // Could use this but it requires holding more data and Dijkstra is plenty fast enough.
        // Also, It's difficult to come up with a decent heuristic.
        // number of constellation jumps is too low compared to the actual number of jumps and
        // results in far too many nodes being explored.
        // world-distance between systems works ok but has to be divided by the longest
        // jump in Eve if we want to ensure that the algorithm will always find the best route. 
        // This, again, results in far too many nodes being explored.
        #region A*
        /*
        private static List<int> GetRouteAStar(int startSystemID, int endSystemID)
        {
            List<int> retVal = null;
            short[] distance = new short[10000];
            double[] remaining = new double[10000];
            int[] previous = new int[10000];
            List<int> openSet = new List<int>();
            List<int> closedSet = new List<int>();
            bool complete = false;
            Dictionary<int, int> idToIndex = new Dictionary<int, int>();
            Dictionary<int, int> indexToID = new Dictionary<int, int>();
            int nextFreeIndex = 0;

            idToIndex.Add(startSystemID, 0);
            indexToID.Add(0, startSystemID);
            nextFreeIndex = 1;

            distance[0] = 0;
            EveDataSet.mapSolarSystemsRow endSystemData = SolarSystems.GetSystem(endSystemID);
            EveDataSet.mapSolarSystemsRow currentSystemData = SolarSystems.GetSystem(startSystemID);
            double realDistance = Math.Sqrt(
                Math.Pow(currentSystemData.x - endSystemData.x, 2) +
                Math.Pow(currentSystemData.y - endSystemData.y, 2) +
                Math.Pow(currentSystemData.z - endSystemData.z, 2));
            remaining[0] = realDistance / 344739065449487000;
            for (int i = 1; i < 10000; i++)
            {
                distance[i] = short.MaxValue;
                remaining[i] = short.MaxValue;
            }
            openSet.Add(0);

            int iterations = 0;

            Diagnostics.ResetTimer("MAP.AStar.1");
            Diagnostics.ResetTimer("MAP.AStar.2");
            Diagnostics.ResetTimer("MAP.AStar.3");
            Diagnostics.ResetTimer("MAP.AStar.4");

            Diagnostics.StartTimer("MAP.AStar");
            while (openSet.Count > 0 && !complete) 
            {
                iterations++;
                Diagnostics.StartTimer("MAP.AStar.1");
                double minimum = double.MaxValue;
                int minIndex = -1;
                foreach (int index in openSet)
                {
                    //if (distance[index] + remaining[index] < minimum)
                    if (remaining[index] < minimum)
                    {
                        //minimum = distance[index] + remaining[index];
                        minimum = remaining[index];
                        minIndex = index;
                    }
                }

                if (indexToID[minIndex] == endSystemID) { complete = true; }
                Diagnostics.StopTimer("MAP.AStar.1");


                if (!complete)
                {
                    Diagnostics.StartTimer("MAP.AStar.2");
                    closedSet.Add(minIndex);
                    openSet.Remove(minIndex);

                    int[] systems = GetNeighbourSystems(indexToID[minIndex]);
                    Diagnostics.StopTimer("MAP.AStar.2");

                    for (int i = 0; i < _maxNeighbours; i++)
                    {
                        int system = systems[i];
                        Diagnostics.StartTimer("MAP.AStar.3");

                        Diagnostics.StopTimer("MAP.AStar.3");

                        if (!closedSet.Contains(system))
                        {
                            Diagnostics.StartTimer("MAP.AStar.4");
                            // Note - change the 1 for other values based on sec status etc if required.
                            short tentative = (short)(distance[minIndex] + 1);
                            bool better = false;

                            if (!openSet.Contains(systemIndex))
                            {
                                openSet.Add(systemIndex);
                                currentSystemData = SolarSystems.GetSystem(system);
                                realDistance = Math.Sqrt(
                                    Math.Pow(currentSystemData.x - endSystemData.x, 2) +
                                    Math.Pow(currentSystemData.y - endSystemData.y, 2) +
                                    Math.Pow(currentSystemData.z - endSystemData.z, 2));
                                remaining[systemIndex] = realDistance / 344739065449487000;
                                better = true;
                            }
                            else if (tentative < distance[systemIndex])
                            {
                                better = true;
                            }

                            if (better)
                            {
                                previous[systemIndex] = minIndex;
                                distance[systemIndex] = tentative;
                            }
                            Diagnostics.StopTimer("MAP.AStar.4");
                        }
                    }
                }
            }
            Diagnostics.StopTimer("MAP.AStar");

            Diagnostics.DisplayDiag("A* search diagnostics:\r\n" +
                Diagnostics.GetRunningTime("MAP.AStar") + "\r\n\t" +
                Diagnostics.GetRunningTime("MAP.AStar.1") + "\r\n\t" +
                Diagnostics.GetRunningTime("MAP.AStar.2") + "\r\n\t" +
                Diagnostics.GetRunningTime("MAP.AStar.3") + "\r\n\t" +
                Diagnostics.GetRunningTime("MAP.AStar.4"));
            

            if (complete)
            {
                retVal = new List<int>();
                int systemId = endSystemID;
                retVal.Add(systemId);
                while (systemId != startSystemID)
                {
                    int prevId = indexToID[previous[idToIndex[systemId]]];
                    retVal.Add(prevId);
                    systemId = prevId;
                }

                retVal.Reverse();
            }

            return retVal;
        }
*/
        #endregion

        #region Test methods
        public static void TEST()
        {
            // First just run the algorithms to make sure caching does not affect results too much.
            //int route3 = CalcRouteLengthDijkstra(30000142, 30001443);
            //int route3 = CalcRouteLengthDijkstra(30001796, 30004069);
            //int route = CalcRouteLength(30000142, 30001443);
            //int route = CalcRouteLength(30001796, 30004069);


            /*           DateTime startTime = DateTime.UtcNow;
                       //int route2 = CalcRouteLength(30000142, 30001443);
                       int route2 = CalcRouteLength(30001796, 30004069);
                       DateTime endTime = DateTime.UtcNow;
            */
            DateTime startTime2 = DateTime.UtcNow;
            //int route4 = CalcRouteLengthDijkstra(30000142, 30001443);
            int route4 = CalcRouteLength(30001796, 30004069);
            DateTime endTime2 = DateTime.UtcNow;
            /*           DateTime startTime3 = DateTime.UtcNow;
                        //int route5 = GetRouteAStar(30000142, 30001443).Count - 1;
                        int route5 = GetRouteAStar(30001796, 30004069).Count - 1;
                        DateTime endTime3 = DateTime.UtcNow;
            */
            //            TimeSpan t1 = endTime.Subtract(startTime);
            TimeSpan t2 = endTime2.Subtract(startTime2);
            //            TimeSpan t3 = endTime3.Subtract(startTime3);
        }

        /*public static void TEST()
        {
            // Cloud ring to Outer passage (60 jumps)
            EveDataSet.mapSolarSystemsRow startSystem = SolarSystems.GetSystem(30001796);
            EveDataSet.mapSolarSystemsRow endSystem = SolarSystems.GetSystem(30004069);
            // Jita to Vuorrassi (7 Jumps)
            //EveDataSet.mapSolarSystemsRow startSystem = SolarSystems.GetSystem(30000142);
            //EveDataSet.mapSolarSystemsRow endSystem = SolarSystems.GetSystem(30001443);

            int startSystemID = startSystem.solarSystemID;
            int startConstID = startSystem.constellationID;
            int startRegionID = startSystem.regionID;
            int endSystemID = endSystem.solarSystemID;
            int endConstID = endSystem.constellationID;
            int endRegionID = endSystem.regionID;

            List<int> route = new List<int>();
            List<int> regionRoute = new List<int>();
            List<int> constRoute = new List<int>();

            systemJumpsAdapter.ClearBeforeFill = true;
            constJumpsAdapter.ClearBeforeFill = true;
            regionJumpsAdapter.ClearBeforeFill = true;

            regionRoute = GetRoute(startRegionID, endRegionID, MapLevel.Region, new List<int>());
            regionRoute = GetNeighbourRegions(regionRoute);
            StreamWriter writer = File.CreateText(@"C:\constResults.csv");
            for (int i = 1; i <= 10; i++)
            {
                for (int j = 1; j <= 10; j++)
                {
                    DateTime startTime = DateTime.UtcNow;
                    constRoute = GetRoute(startConstID, endConstID, MapLevel.Constellation, regionRoute, i, j);
                    DateTime endTime = DateTime.UtcNow;
                    writer.Write((j != 1 ? "," : "") + ((TimeSpan)endTime.Subtract(startTime)).ToString());
                }
                writer.WriteLine();
            }
            writer.Close();
            constRoute = GetNeighbourConstellations(constRoute);
            writer = File.CreateText(@"C:\sysResults.csv");
            for (int i = 1; i <= 10; i++)
            {
                for (int j = 1; j <= 10; j++)
                {
                    DateTime startTime = DateTime.UtcNow;
                    route = GetRoute(startSystemID, endSystemID, MapLevel.System, constRoute, i, j);
                    DateTime endTime = DateTime.UtcNow;
                    writer.Write((j != 1 ? "," : "") + ((TimeSpan)endTime.Subtract(startTime)).ToString());
                }
                writer.WriteLine();
            }
            writer.Close();
        }*/
        #endregion

        #region Initalisation
        public static void InitaliseData()
        {
            if (!_initalised)
            {
                //DateTime dt1 = DateTime.UtcNow;
                for (int i = 0; i < _numSystemsInEve; i++)
                {
                    _systemNeighbours[i] = new long[_maxNeighbours];
                }

                SqlConnection connection = new SqlConnection(Properties.Settings.Default.ebs_DATADUMPConnectionString);
                SqlCommand command = null;
                // Don't want to use zero...
                int nextFreeID = 1;
                connection.Open();

                try
                {
                    #region Setup _systemNeighbours
                    command = new SqlCommand("SolarSystemJumpsGetAll", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        SqlInt32 fromID = reader.GetInt32(0);
                        SqlInt32 toID = reader.GetInt32(1);
                        int fromIndex = 0;

                        if (!_idToIndex.ContainsKey(fromID.Value))
                        {
                            _idToIndex.Add(fromID.Value, nextFreeID);
                            nextFreeID++;
                        }
                        fromIndex = _idToIndex[fromID.Value];

                        for (int i = 0; i < 10; i++)
                        {
                            if (_systemNeighbours[fromIndex][i] == 0)
                            {
                                _systemNeighbours[fromIndex][i] = toID.Value;
                                i = 10;
                            }
                        }
                    }

                    reader.Close();
                    #endregion

                    #region Setup _systemSec
                    command = new SqlCommand("SELECT solarSystemID, security FROM mapSolarSystems", connection);
                    command.CommandType = System.Data.CommandType.Text;

                    reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int sysID = reader.GetInt32(0);
                        float security = (float)reader.GetDouble(1);
                        int sysIndex = 0;

                        if (!_idToIndex.ContainsKey(sysID))
                        {
                            _idToIndex.Add(sysID, nextFreeID);
                            nextFreeID++;
                        }
                        sysIndex = _idToIndex[sysID];

                        _systemSec[sysIndex] = security;
                    }

                    reader.Close();
                    #endregion
                }
                catch (Exception ex)
                {
                    throw new EMMADataException(ExceptionSeverity.Error, "Problem pre-loading map data", ex);
                }
                finally
                {
                    connection.Close();
                }

                //TimeSpan t1 = DateTime.UtcNow.Subtract(dt1);

                _initalised = true;
            }
        }
        #endregion
    }


}
