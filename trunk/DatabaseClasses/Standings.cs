using System;
using System.Collections.Generic;
using System.Text;

using EveMarketMonitorApp.AbstractionClasses;
using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public static class Standings
    {
        private static EMMADataSetTableAdapters.StandingsTableAdapter tableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.StandingsTableAdapter();
        private const int CACHE_SIZE = 500;
        private static Cache<string, decimal> _cache = new Cache<string, decimal>(CACHE_SIZE);
        private static bool _initalised = false;

        /// <summary>
        /// Clear all standings that match the specified filter.
        /// setting an parameter to zero will include any id for that column.
        /// </summary>
        /// <param name="toID"></param>
        /// <param name="fromID"></param>
        public static void ClearStandings(long toID, long fromID)
        {
            lock (tableAdapter)
            {
                tableAdapter.ClearByIDs(toID, fromID);
            }
        }

        /// <summary>
        /// Get the standing from the fromID to toID
        /// </summary>
        /// <param name="toID"></param>
        /// <param name="fromID"></param>
        /// <returns></returns>
        public static decimal GetStanding(long toID, long fromID) 
        {
            decimal retVal = 0;
            if (!_initalised) { Initalise(); }

            retVal = _cache.Get(toID + "|" + fromID);
            return retVal;
        }

        /// <summary>
        /// Set standing in the database to the specified value.
        /// </summary>
        /// <param name="toID"></param>
        /// <param name="fromID"></param>
        /// <param name="standing"></param>
        public static void SetStanding(long toID, long fromID, decimal standing)
        {
            EMMADataSet.StandingsDataTable table = new EMMADataSet.StandingsDataTable();
            tableAdapter.FillByIDs(table, toID, fromID);
            EMMADataSet.StandingsRow dataRow = table.FindBytoIDfromID(toID, fromID);
            bool newRow = false;
            if (dataRow == null)
            {
                dataRow = table.NewStandingsRow();
                dataRow.toID = toID;
                dataRow.fromID = fromID;
                newRow = true;
            }

            dataRow.standing = standing;

            if (newRow) { table.AddStandingsRow(dataRow); }
            tableAdapter.Update(table);
        }


        /// <summary>
        /// Update standings for the supplied id with the latest from the eve API.
        /// </summary>
        /// <param name="id"></param>
        public static void UpdateStandings(long id)
        {
            bool corp = false;

            APICharacter character = UserAccount.CurrentGroup.GetCharacter(id, ref corp);
            if (character != null)
            {
                character.UpdateStandings(corp ? CharOrCorp.Corp : CharOrCorp.Char);
                _cache = new Cache<string, decimal>(CACHE_SIZE);
                Initalise();
            }
            else
            {
                throw new EMMAException(ExceptionSeverity.Error, "The specified id (" + id +
                    ") is not part of the current report group.");
            }

        }


        private static void Initalise()
        {
            if (!_initalised)
            {
                _cache.DataUpdateNeeded += new Cache<string, decimal>.DataUpdateNeededHandler(Cache_DataUpdateNeeded);
                _initalised = true;
            }
        }

        static void Cache_DataUpdateNeeded(object myObject, DataUpdateNeededArgs<string, decimal> args)
        {
            EMMADataSet.StandingsDataTable table = new EMMADataSet.StandingsDataTable();
            lock (tableAdapter)
            {
                char[] delim = { '|' };
                long toID = 0, fromID = 0;
                string[] ids = args.Key.Split(delim);
                toID = long.Parse(ids[0]);
                fromID = long.Parse(ids[1]);
                tableAdapter.FillByIDs(table, toID, fromID);
            }

            if (table.Count > 0)
            {
                args.Data = table[0].standing;
            }
            else
            {
                args.Data = 0;
            }
        }
    }
}
