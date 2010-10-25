using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.AbstractionClasses;

namespace EveMarketMonitorApp.DatabaseClasses
{
    static class Names
    {
        static private EMMADataSetTableAdapters.NamesTableAdapter namesTableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.NamesTableAdapter();
        static private Cache<long, string> _cache = new Cache<long, string>(1000);
        private static bool _initalised = false;


        private static void InitCache()
        {
            if (!_initalised)
            {
                _cache.DataUpdateNeeded += new Cache<long, string>.DataUpdateNeededHandler(Cache_DataUpdateNeeded);
                _initalised = true;
            }
        }

        /// <summary>
        /// Get the name associated with the specified ID
        /// The caller should catch EMMADataMissingException and should make a call to  
        /// AddName(id, name) if appropriate.
        /// </summary>
        /// <param name="characterID"></param>
        /// <returns></returns>
        static public string GetName(long ID)
        {
            string retVal = "";
            if (!_initalised) { InitCache(); }

            if (ID != 0)
            {
                retVal = _cache.Get(ID);

                if (retVal == null)
                {
                    throw new EMMADataMissingException(ExceptionSeverity.Warning, "Supplied name ID (" +
                        ID + ") cannot be resolved.", "Names", ID.ToString());
                }
            }
            return retVal;

        }


        static void Cache_DataUpdateNeeded(object myObject, DataUpdateNeededArgs<long, string> args)
        {
            try
            {
                string name = "";
                lock (namesTableAdapter)
                {
                    namesTableAdapter.GetName(args.Key, ref name);
                }
                if (name == null || name.Equals(""))
                {
                    name = EveAPI.GetName(args.Key);
                    if (!name.Equals("")) { AddName(args.Key, name); }
                }
                args.Data = name;
            }
            catch (Exception)
            {
                args.Data = null;
            }
        }


        /// <summary>
        /// Add a new character to the characters table with the specified ID and name.
        /// </summary>
        /// <param name="characterID"></param>
        /// <param name="characterName"></param>
        static public void AddName(long ID, string Name)
        {
            try
            {
                // First make sure this character is not already in the database.
                string currentName = "";
                lock (namesTableAdapter)
                {
                    namesTableAdapter.GetName(ID, ref currentName);
                    bool newName = false;

                    if (currentName == null)
                    {
                        newName = true;
                    }
                    //else
                    //{
                    //    if (currentName.Equals("") && ID != 0)
                    //    {
                    //        newName = true;
                    //    }
                    //}

                    if (newName)
                    {
                        EMMADataSet.NamesDataTable names = new EMMADataSet.NamesDataTable();
                        EMMADataSet.NamesRow newRow = names.NewNamesRow();
                        newRow.ID = ID;
                        newRow.Name = Name;
                        names.AddNamesRow(newRow);

                        namesTableAdapter.Update(names);
                        names.AcceptChanges();
                    }
                    else
                    {
                        /*throw new EMMADataException(ExceptionSeverity.Error, "Name with ID " + ID +
                            " already exists in database.");*/
                    }

                }

            }
            catch (Exception ex)
            {
                throw new EMMADataException(ExceptionSeverity.Error, "Problem adding name - " + ID +
                    " " + Name + " to database.", ex);
            }
        }


    }
}
