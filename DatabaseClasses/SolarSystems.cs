using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.GUIElements;

namespace EveMarketMonitorApp.DatabaseClasses
{
    static class SolarSystems
    {
        static private EveDataSetTableAdapters.mapSolarSystemsTableAdapter systemsTableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EveDataSetTableAdapters.mapSolarSystemsTableAdapter();
        static private Cache<long, string> _nameCache = new Cache<long, string>(5000);
        static private bool _initalised = false;

        /// <summary>
        /// Return true if the system is in a system with security rating less than 0.45 and false otherwise.
        /// </summary>
        /// <param name="systemID"></param>
        /// <returns></returns>
        static public bool IsLowSec(long systemID)
        {
            return Map.GetSecurity(systemID) <= 0.45;
        }

        static public float GetSystemSecurity(long systemID)
        {
            return Map.GetSecurity(systemID);
        }

        static public EveDataSet.mapSolarSystemsDataTable GetAssetSystems(
            List<AssetAccessParams> accessParams, int itemID, long regionID)
        {
            StringBuilder systemIDs = new StringBuilder("");
            EMMADataSet.IDTableDataTable idTable = Assets.GetInvolvedSystemIDs(accessParams, itemID, regionID);
            foreach (EMMADataSet.IDTableRow id in idTable)
            {
                systemIDs.Append(" ");
                systemIDs.Append(id.ID);
            }
            EveDataSet.mapSolarSystemsDataTable retVal = new EveDataSet.mapSolarSystemsDataTable();
            lock (systemsTableAdapter)
            {
                systemsTableAdapter.FillByIDs(retVal, systemIDs.ToString());
            }
            return retVal;
        }


        static public string GetSystemName(long systemID)
        {
            if (!_initalised) { InitialiseCache(); }
            string retVal = "";
            retVal = _nameCache.Get(systemID);
            return retVal;
        }


        static public EveDataSet.mapSolarSystemsRow GetSystem(long systemID)
        {
            EveDataSet.mapSolarSystemsDataTable table = new EveDataSet.mapSolarSystemsDataTable();
            EveDataSet.mapSolarSystemsRow retVal;
            table = GetSystems(systemID.ToString());
            retVal = table.FindBysolarSystemID((int)systemID);
            return retVal;
        }

        static public EveDataSet.mapSolarSystemsRow GetSystem(string systemName)
        {
            EveDataSet.mapSolarSystemsDataTable table = new EveDataSet.mapSolarSystemsDataTable();
            EveDataSet.mapSolarSystemsRow retVal = null;

            lock (systemsTableAdapter)
            {
                systemsTableAdapter.FillByName(table, systemName);
            }

            if (table.Count == 0)
            {
                lock (systemsTableAdapter)
                {
                    systemsTableAdapter.FillByName(table, "%" + systemName + "%");
                }

                if (table.Count < 1)
                {
                    throw new EMMADataException(ExceptionSeverity.Error, "No system found matching '" + systemName + "'");
                }
                else if (table.Count > 1)
                {
                    SortedList<object, string> options = new SortedList<object, string>();
                    foreach (EveDataSet.mapSolarSystemsRow system in table)
                    {
                        options.Add(system.solarSystemID, system.solarSystemName);
                    }
                    OptionPicker picker = new OptionPicker("Select System", "Choose the specific system you " +
                        "want from those listed below.", options);
                    if (picker.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
                    {
                        retVal = table.FindBysolarSystemID((int)picker.SelectedItem);
                    }
                }
                else
                {
                    retVal = table[0];
                }
            }
            else
            {
                retVal = table[0];
            }

            return retVal;
        }

        static public EveDataSet.mapSolarSystemsDataTable GetAllSystems()
        {
            EveDataSet.mapSolarSystemsDataTable table = new EveDataSet.mapSolarSystemsDataTable();
            systemsTableAdapter.Fill(table);
            return table;
        }

        #region Private Methods
        static private void InitialiseCache()
        {
            if (!_initalised)
            {
                _nameCache.DataUpdateNeeded +=
                    new Cache<long, string>.DataUpdateNeededHandler(NameCache_DataUpdateNeeded);
                _initalised = true;
            }
        }

        static void NameCache_DataUpdateNeeded(object myObject, DataUpdateNeededArgs<long, string> args)
        {
            string name = "";

            lock (systemsTableAdapter)
            {
                systemsTableAdapter.GetName((int)args.Key, ref name);
            }

            if (name.Equals(""))
            {
                name = "Unknown System (" + args.Key + ")";
            }

            args.Data = name;
        }

        /// <summary>
        /// Get a data table containing all the items specified in the supplied list of IDs 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        private static EveDataSet.mapSolarSystemsDataTable GetSystems(List<int> ids)
        {
            StringBuilder idList = new StringBuilder("");
            foreach (int id in ids)
            {
                idList.Append(" ");
                idList.Append(id);
            }
            return GetSystems(idList.ToString());
        }

        /// <summary>
        /// Get a data table containing all the items specified in the supplied list of IDs 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        private static EveDataSet.mapSolarSystemsDataTable GetSystems(string ids)
        {
            EveDataSet.mapSolarSystemsDataTable systems = new EveDataSet.mapSolarSystemsDataTable();
            lock (systemsTableAdapter)
            {
                systemsTableAdapter.FillByIDs(systems, ids);
            }
            return systems;
        }
        #endregion






    }
}
