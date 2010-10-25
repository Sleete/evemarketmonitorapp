using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.AbstractionClasses;
using EveMarketMonitorApp.GUIElements;

namespace EveMarketMonitorApp.DatabaseClasses
{
    static class Stations
    {
        static private EveDataSetTableAdapters.staStationsTableAdapter stationsTableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EveDataSetTableAdapters.staStationsTableAdapter();
        static private EveDataSet.staStationsDataTable _stations = new EveDataSet.staStationsDataTable();
        static private Cache<long, string> _nameCache = new Cache<long, string>(1000);
        static private Cache<long, double> _securityCache = new Cache<long, double>(1000);
        static private bool _initalised = false;


        /// <summary>
        /// Add a new station to the stations table with the specified ID and name.
        /// </summary>
        /// <param name="stationID"></param>
        /// <param name="stationName"></param>
        static public void AddStation(int stationID, string stationName, int solarSystemID, int corpID)
        {
            try
            {
                EveDataSet.staStationsDataTable table = new EveDataSet.staStationsDataTable();
                // First make sure this station is not already in the database.
                lock (stationsTableAdapter)
                {
                    stationsTableAdapter.FillByIDs(table, stationID.ToString());
                }
                EveDataSet.staStationsRow data = table.FindBystationID(stationID);

                if (data == null)
                {
                    EveDataSet.staStationsRow newRow = table.NewstaStationsRow();
                    newRow.stationID = stationID;
                    newRow.stationName = stationName;
                    newRow.corporationID = corpID;
                    newRow.solarSystemID = solarSystemID;
                    EveDataSet.mapSolarSystemsRow systemData = SolarSystems.GetSystem(solarSystemID);
                    newRow.constellationID = systemData.constellationID;
                    newRow.regionID = systemData.regionID;

                    table.AddstaStationsRow(newRow);

                    stationsTableAdapter.Update(table);
                }
                else
                {
                    bool update = false;
                    if (!data.stationName.Equals(stationName))
                    {
                        data.stationName = stationName;
                        update = true;
                    }
                    if (data.IscorporationIDNull() || data.corporationID != corpID)
                    {
                        data.corporationID = corpID;
                        update = true;
                    }
                    if (update)
                    {
                        stationsTableAdapter.Update(data);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new EMMADataException(ExceptionSeverity.Error, "Problem adding station to database.", ex);
            }
        }

        /// <summary>
        /// Get a list of the IDs of all stations in the specified solar system.
        /// </summary>
        /// <param name="systemID"></param>
        /// <returns></returns>
        static public List<long> GetStationsInSystem(long systemID)
        {
            EveDataSet.staStationsDataTable table = new EveDataSet.staStationsDataTable();
            List<long> retVal = new List<long>();
            lock (stationsTableAdapter)
            {
                stationsTableAdapter.FillBySystem(table, (int)systemID);
            }
            foreach (EveDataSet.staStationsRow station in table)
            {
                retVal.Add(station.stationID);
            }
            return retVal;
        }

        /// <summary>
        /// Get the specified station. stationName can be the whole name or just part of a name.
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        static public EveDataSet.staStationsRow GetStation(string stationName)
        {
            EveDataSet.staStationsDataTable table = new EveDataSet.staStationsDataTable();
            EveDataSet.staStationsRow retVal = null;

            lock (stationsTableAdapter)
            {
                stationsTableAdapter.FillByName(table, "%" + stationName + "%");
            }

            if (table.Count < 1)
            {
                throw new EMMADataException(ExceptionSeverity.Error, "No station found matching '" + stationName + "'");
            }
            else if (table.Count > 1)
            {
                SortedList<object, string> options = new SortedList<object, string>();
                foreach (EveDataSet.staStationsRow station in table)
                {
                    options.Add(station.stationID, station.stationName);
                }
                OptionPicker picker = new OptionPicker("Select Station", "Choose the specific station you " +
                    "want from those listed below.", options);
                if (picker.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
                {
                    retVal = table.FindBystationID((int)picker.SelectedItem);
                }
            }
            else
            {
                retVal = table[0];
            }

            return retVal;
        }

        /// <summary>
        /// Return a datatable containing all stations in the database
        /// </summary>
        /// <returns></returns>
        static public EveDataSet.staStationsDataTable GetAllStations()
        {
            EveDataSet.staStationsDataTable retVal = new EveDataSet.staStationsDataTable();
            lock (stationsTableAdapter)
            {
                stationsTableAdapter.FillAll(retVal, false);
            }
            return retVal;
        }

        public static EveDataSet.staStationsDataTable GetContractStations(List<long> ownerIDs,
            ContractStationType type)
        {
            StringBuilder stationIDs = new StringBuilder("");
            EMMADataSet.IDTableDataTable idTable = Contracts.GetInvolvedStationIDs(ownerIDs, type);
            foreach (EMMADataSet.IDTableRow id in idTable)
            {
                stationIDs.Append(" ");
                stationIDs.Append(id.ID);
            }
            EveDataSet.staStationsDataTable retVal = new EveDataSet.staStationsDataTable();
            lock (stationsTableAdapter)
            {
                stationsTableAdapter.FillByIDs(retVal, stationIDs.ToString());
            }
            return retVal;
        }

        /// <summary>
        /// Get a datatable containing all stations where assets are stored that match the 
        /// specified parameters
        /// </summary>
        /// <param name="accessParams"></param>
        /// <param name="itemID"></param>
        /// <param name="systemID"></param>
        /// <returns></returns>
        static public EveDataSet.staStationsDataTable GetAssetStations(
            List<AssetAccessParams> accessParams, int itemID, int systemID)
        {
            StringBuilder stationIDs = new StringBuilder("");
            EMMADataSet.IDTableDataTable idTable = Assets.GetInvolvedStationIDs(accessParams, itemID, systemID);
            foreach (EMMADataSet.IDTableRow id in idTable)
            {
                stationIDs.Append(" ");
                stationIDs.Append(id.ID);
            }
            EveDataSet.staStationsDataTable retVal = new EveDataSet.staStationsDataTable();
            lock (stationsTableAdapter)
            {
                stationsTableAdapter.FillByIDs(retVal, stationIDs.ToString());
            }
            return retVal;
        }

        /// <summary>
        /// Get a datatable containing all stations where transactions have taken place that 
        /// match the specified parameters
        /// </summary>
        /// <param name="accessParams"></param>
        /// <returns></returns>
        static public EveDataSet.staStationsDataTable GetStationsTradedIn(List<FinanceAccessParams> accessParams)
        {
            StringBuilder stationIDs = new StringBuilder("");
            EMMADataSet.IDTableDataTable idTable = Transactions.GetInvolvedStationIDs(accessParams);
            foreach (EMMADataSet.IDTableRow id in idTable)
            {
                stationIDs.Append(" ");
                stationIDs.Append(id.ID);
            }
            EveDataSet.staStationsDataTable retVal = new EveDataSet.staStationsDataTable();
            lock (stationsTableAdapter)
            {
                stationsTableAdapter.FillByIDs(retVal, stationIDs.ToString());
            }
            return retVal;
        }


        /// <summary>
        /// Return true if the station is in a system with security rating less than 0.4 and false otherwise.
        /// </summary>
        /// <param name="stationID"></param>
        /// <returns></returns>
        static public bool IsLowSec(long stationID)
        {
            if (!_initalised) { InitialiseCache(); }
            return _securityCache.Get(stationID) <= 0.45;
        }

        /// <summary>
        /// Get the row containing the specified station.
        /// </summary>
        /// <param name="stationID"></param>
        /// <returns></returns>
        static public EveDataSet.staStationsRow GetStation(long stationID)
        {
            EveDataSet.staStationsRow retVal;

            int intStationID = (int)stationID;
            retVal = _stations.FindBystationID(intStationID);

            if (retVal == null)
            {
                stationsTableAdapter.ClearBeforeFill = false;
                lock (stationsTableAdapter)
                {
                    stationsTableAdapter.FillByIDs(_stations, stationID.ToString());
                }
                retVal = _stations.FindBystationID(intStationID);
            }
            
            if (retVal == null)
            {
                throw new EMMADataMissingException(ExceptionSeverity.Warning, "Supplied station ID (" +
                    stationID + ") is not in the EMMA database.", _stations.TableName, stationID.ToString());
            }

            return retVal;
        }

        /// <summary>
        /// Get the name of the specified station.
        /// </summary>
        /// <param name="stationID"></param>
        /// <returns></returns>
        static public string GetStationName(long stationID)
        {
            if (!_initalised) { InitialiseCache(); }
            return _nameCache.Get(stationID);
        }

        /// <summary>
        /// Add a new station to the stations table with the specified ID and name.
        /// </summary>
        /// <param name="stationID"></param>
        /// <param name="stationName"></param>
        /*static public void AddStation(int stationID, string stationName)
        {
            // First make sure this station is not already in the database.
            EveDataSet.staStationsDataTable table = new EveDataSet.staStationsDataTable();
            EveDataSet.staStationsRow data;

            table = GetStations(stationID.ToString);
            data = table.FindBystationID(stationID);

            if (data == null)
            {
                MessageBox.Show("A station is refered to that is not in EMMA's database." +
                    "\r\nPlease add details for this station now", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                NewStation newStation = new NewStation(stationID, stationName);

                if (newStation.ShowDialog() == DialogResult.OK)
                {
                    AddStation(stationID, stationName, newStation.SolarSystemID);
                }
            }
        }*/

        /// <summary>
        /// Add a new station to the stations table with the specified ID and name.
        /// </summary>
        /// <param name="stationID"></param>
        /// <param name="stationName"></param>
        /*static public void AddStation(int stationID, string stationName, int solarSystemID)
        {
            try
            {
                // First make sure this station is not already in the database.
                LoadData(stationID);
                EMMADataSet.StationsRow data = stations.FindByID(stationID);

                if (data == null)
                {
                    EMMADataSet.StationsRow newRow = stations.NewStationsRow();
                    newRow.ID = stationID;
                    newRow.Name = stationName;
                    newRow.SolarSystemID = solarSystemID;
                    EMMADataSet.MapSolarSystemsRow systemData = SolarSystems.GetSystem(solarSystemID);
                    newRow.ConstellationID = systemData.ConstellationID;
                    newRow.RegionID = systemData.RegionID;

                    stations.AddStationsRow(newRow);

                    lock (stationsTableAdapter)
                    {
                        stationsTableAdapter.Update(stations);
                    }
                    stations.AcceptChanges();
                }
                else
                {
                    if (data.Name.Equals(stationName))
                    {
                        throw new EMMADataException(ExceptionSeverity.Warning, "Station with ID " + stationID +
                            " already exists in database.");
                    }
                    else
                    {
                        data.Name = stationName;
                        lock (stationsTableAdapter)
                        {
                            stationsTableAdapter.Update(data);
                        }
                        data.AcceptChanges();
                    }
                }
            }
            catch (EMMADataException)
            {
                // If it's the station already exists warning then just ignore it, we're fine with that.
            }
            catch (Exception ex)
            {
                throw new EMMADataException(ExceptionSeverity.Error, "Problem adding station to database.", ex);
            }
        }*/

        #region Private Methods
        /// <summary>
        /// Initialise the cache holding station names
        /// </summary>
        static private void InitialiseCache()
        {
            if (!_initalised)
            {
                _nameCache.DataUpdateNeeded += new Cache<long, string>.DataUpdateNeededHandler(NameCache_DataUpdateNeeded);
                _securityCache.DataUpdateNeeded += new Cache<long, double>.DataUpdateNeededHandler(SecurityCache_DataUpdateNeeded);
                _initalised = true;
            }
        }

        static void SecurityCache_DataUpdateNeeded(object myObject, DataUpdateNeededArgs<long, double> args)
        {
            double security = 0;
            EveDataSet.staStationsRow station = GetStation(args.Key);
            if (station != null)
            {
                EveDataSet.mapSolarSystemsRow system = SolarSystems.GetSystem(station.solarSystemID);
                if(system != null) 
                {
                    security = (double)system.security;
                }
            }
            args.Data = security;
        }

        /// <summary>
        /// Called when the cache requires a station name to be read from the database
        /// </summary>
        /// <param name="myObject"></param>
        /// <param name="args"></param>
        static void NameCache_DataUpdateNeeded(object myObject, DataUpdateNeededArgs<long, string> args)
        {
            string name = "";

            lock (stationsTableAdapter)
            {
                stationsTableAdapter.GetName((int)args.Key, ref name);
            }

            if (name.Equals(""))
            {
                name = "Unknown Station (" + args.Key + ")";
            }

            args.Data = name;
        }

        /// <summary>
        /// Get a data table containing all the items specified in the supplied list of IDs 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        private static EveDataSet.staStationsDataTable GetStations(List<int> ids)
        {
            StringBuilder idList = new StringBuilder("");
            foreach (int id in ids)
            {
                idList.Append(" ");
                idList.Append(id);
            }
            return GetStations(idList.ToString());
        }

        /// <summary>
        /// Get a data table containing all the items specified in the supplied list of IDs 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        private static EveDataSet.staStationsDataTable GetStations(string ids)
        {
            EveDataSet.staStationsDataTable stations = new EveDataSet.staStationsDataTable();
            lock (stationsTableAdapter)
            {
                stationsTableAdapter.FillByIDs(stations, ids);
            }
            return stations;
        }
        #endregion

    }
}
