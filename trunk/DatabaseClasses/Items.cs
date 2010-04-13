using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;

using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.GUIElements;

namespace EveMarketMonitorApp.DatabaseClasses
{
    static class Items
    {
        static private EveDataSetTableAdapters.invTypesTableAdapter tableAdapter = new
            EveMarketMonitorApp.DatabaseClasses.EveDataSetTableAdapters.invTypesTableAdapter();
        static private EveDataSetTableAdapters.invGroupsTableAdapter groupsTableAdapter = new
            EveMarketMonitorApp.DatabaseClasses.EveDataSetTableAdapters.invGroupsTableAdapter();
        static private EveDataSet.invGroupsDataTable groupTable = null;
        static private Cache<int, string> _nameCache = new Cache<int, string>(1000);
        static private Cache<int, double> _volCache = new Cache<int, double>(1000);
        static private Cache<int, Dictionary<int, double>> _reproCache = new Cache<int, Dictionary<int, double>>(1000);
        static private bool _initalised = false;


        static public Dictionary<int, double> GetItemMaxReprocessResults(int itemID)
        {
            Dictionary<int, double> retVal = new Dictionary<int, double>();
            if (!_initalised) { InitialiseCache(); }
            retVal = _reproCache.Get(itemID);
            return retVal;
        }

        /// <summary>
        /// Get the name of the specified item.
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        /// <exception cref="EMMADataMissingException"></exception>
        static public string GetItemName(int itemID)
        {
            string retVal = "";
            if (!_initalised) { InitialiseCache(); }
            retVal = _nameCache.Get(itemID);            
            return retVal;
        }

        /// <summary>
        /// Get the volume of the specified item.
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        /// <exception cref="EMMADataMissingException"></exception>
        static public double GetItemVolume(int itemID)
        {
            double retVal = 0;
            if (!_initalised) { InitialiseCache(); }
            retVal = _volCache.Get(itemID);
            return retVal;
        }

        /// <summary>
        /// Get the specified item.
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        /// <exception cref="EMMADataMissingException"></exception>
        static public EveDataSet.invTypesRow GetItem(int itemID)
        {
            EveDataSet.invTypesDataTable table = new EveDataSet.invTypesDataTable();
            table = GetItems(itemID.ToString());
            EveDataSet.invTypesRow data = table.FindBytypeID((short)itemID);
            return data;
        }

        /// <summary>
        /// Get the specified item. itemName can be the whole name or just part of a name.
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        static public EveDataSet.invTypesRow GetItem(string itemName)
        {
            EveDataSet.invTypesDataTable table = new EveDataSet.invTypesDataTable();
            EveDataSet.invTypesRow retVal = null;

            lock (tableAdapter)
            {
                tableAdapter.FillByName(table, itemName);
            }
            if (table.Count == 1)
            {
                retVal = table[0];
            }
            else
            {
                lock (tableAdapter)
                {
                    tableAdapter.FillByName(table, "%" + itemName + "%");
                }

                if (table.Count < 1)
                {
                    throw new EMMADataException(ExceptionSeverity.Error, "No item found matching '" + itemName + "'");
                }
                else if (table.Count > 1)
                {
                    SortedList<object, string> options = new SortedList<object, string>();
                    foreach (EveDataSet.invTypesRow item in table)
                    {
                        options.Add(item.typeID, item.typeName);
                    }
                    OptionPicker picker = new OptionPicker("Select Item", "Choose the specific item you want from " +
                        "those listed below.", options);
                    if (picker.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
                    {
                        retVal = table.FindBytypeID((short)picker.SelectedItem);
                    }
                }
                else
                {
                    retVal = table[0];
                }
            }

            return retVal;
        }


        static public EveDataSet.invTypesDataTable GetItemsWithTransactions(List<FinanceAccessParams> accessList)
        {
            return GetItemsTraded(accessList, 0);
        }
        
        static public EveDataSet.invTypesDataTable GetItemsTraded(List<FinanceAccessParams> accessList, int minTrans)
        {
            return GetItemsTraded(accessList, minTrans, 0, 0, new List<int>(), new List<int>(), 
                new DateTime(1990, 1, 1));
        }

        static public EveDataSet.invTypesDataTable GetItemsTraded(List<FinanceAccessParams> accessList, int minTrans,
            int minBuy, int minSell, List<int> buyStations, List<int> sellStations, DateTime startDate)
        {
            StringBuilder itemIDs = new StringBuilder("");
            EMMADataSet.IDTableDataTable idTable = Transactions.GetInvolvedItemIDs(accessList, minTrans);
            foreach (EMMADataSet.IDTableRow id in idTable)
            {
                itemIDs.Append(" ");
                itemIDs.Append(id.ID);
            }
            EveDataSet.invTypesDataTable retVal = new EveDataSet.invTypesDataTable();
            retVal = GetItems(itemIDs.ToString());
            return retVal;
        }

        static public List<int> GetItemIDsWithTransactions(List<FinanceAccessParams> accessList)
        {
            List<int> retVal = new List<int>();
            EMMADataSet.IDTableDataTable idTable = Transactions.GetInvolvedItemIDs(accessList, 0);
            foreach (EMMADataSet.IDTableRow id in idTable)
            {
                retVal.Add(id.ID);
            }
            return retVal;
        }


        static public EveDataSet.invTypesDataTable GetItemsThatAreAssets(List<AssetAccessParams> accessList)
        {
            StringBuilder itemIDs = new StringBuilder("");
            EMMADataSet.IDTableDataTable idTable = Assets.GetInvolvedItemIDs(accessList);
            foreach (EMMADataSet.IDTableRow id in idTable)
            {
                itemIDs.Append(" ");
                itemIDs.Append(id.ID);
            }
            EveDataSet.invTypesDataTable retVal = new EveDataSet.invTypesDataTable();
            retVal = GetItems(itemIDs.ToString());
            return retVal;
        }

        
        static public EveDataSet.invTypesDataTable GetMarketItems()
        {
            EveDataSet.invTypesDataTable retVal = new EveDataSet.invTypesDataTable();
            lock (tableAdapter)
            {
                tableAdapter.FillByMarketItems(retVal, false);
            }
            return retVal;
        }



        /// <summary>
        /// Add a new item to the items table with the specified ID and name.
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="itemName"></param>
        static public void AddItem(int itemID, string itemName)
        {
            try
            {
                // First make sure this item is not already in the database.
                lock (tableAdapter)
                {
                    EveDataSet.invTypesDataTable items = new EveDataSet.invTypesDataTable();

                    items = GetItems(itemID.ToString());
                    EveDataSet.invTypesRow data = items.FindBytypeID((short)itemID);

                    if (data == null)
                    {
                        EveDataSet.invTypesRow newRow = items.NewinvTypesRow();
                        newRow.typeID = (short)itemID;
                        newRow.typeName = itemName;
                        /// Just adding a placeholder row until an offical datadump arrives.
                        /// Can't expect the user to enter other details so just leave other rows
                        /// to default value (i.e. null)...
                        items.AddinvTypesRow(newRow);

                        tableAdapter.Update(items);
                    }
                    else
                    {
                        /*throw new EMMADataException(ExceptionSeverity.Error, "Item with ID " + itemID +
                            " already exists in database.");*/
                    }
                }
            }
            catch (Exception ex)
            {
                throw new EMMADataException(ExceptionSeverity.Error, "Problem adding item to database.", ex);
            }
        }

        #region Private Methods
        static private void InitialiseCache()
        {
            if (!_initalised)
            {
                _nameCache.DataUpdateNeeded += new Cache<int, string>.DataUpdateNeededHandler(NameCache_DataUpdateNeeded);
                _volCache.DataUpdateNeeded += new Cache<int, double>.DataUpdateNeededHandler(VolCache_DataUpdateNeeded);
                _reproCache.DataUpdateNeeded += new Cache<int, Dictionary<int, double>>.DataUpdateNeededHandler(ReproCache_DataUpdateNeeded);
                _initalised = true;
            }
        }


        static void ReproCache_DataUpdateNeeded(object myObject, DataUpdateNeededArgs<int, Dictionary<int, double>> args)
        {
            // decimal value = 0;
            Dictionary<int, double> list = new Dictionary<int, double>();

            try
            {
                EveDataSetTableAdapters.ReprocesResultsTableAdapter reproAdapter =
                    new EveMarketMonitorApp.DatabaseClasses.EveDataSetTableAdapters.ReprocesResultsTableAdapter();
                EveDataSet.ReprocesResultsDataTable reprocessResults = new EveDataSet.ReprocesResultsDataTable();

                reproAdapter.Fill(reprocessResults, args.Key);

                foreach (EveDataSet.ReprocesResultsRow row in reprocessResults)
                {
                    double quantity = row.quantity;
                    EveDataSet.invTypesRow item = Items.GetItem(args.Key);
                    int portion = (item.IsportionSizeNull() ? 0 : item.portionSize);
                    if (portion != 0)
                    {
                        quantity /= portion;
                    }
                    //value += quantity * UserAccount.CurrentGroup.ItemsTraded.GetItemValue(row.requiredTypeID);

                    list.Add(row.requiredTypeID, quantity);
                }
            }
            catch (Exception ex)
            {
                EMMAException emmaEx = ex as EMMAException;
                if (emmaEx == null)
                {
                    new EMMAException(ExceptionSeverity.Error, "Problem getting reprocessor update for item '" +
                        Items.GetItemName(args.Key) + "'", ex);
                }
            }

            //args.Data = value;
            args.Data = list;
        }


        static void VolCache_DataUpdateNeeded(object myObject, DataUpdateNeededArgs<int, double> args)
        {
            double vol = 0;

            EveDataSet.invTypesRow itemData = GetItem(args.Key);

            if (itemData == null || itemData.IsvolumeNull())
            {
                vol = 0;
            }
            else
            {
                short groupID = itemData.groupID;
                if (groupTable == null)
                {
                    groupTable = new EveDataSet.invGroupsDataTable();
                    groupsTableAdapter.Fill(groupTable);
                }
                EveDataSet.invGroupsRow groupData = groupTable.FindBygroupID(groupID);

                if (groupData.categoryID == 6)
                {
                    // If the item is a ship then use the groupID to get the unpackaged volume
                    // Shuttle
                    if (groupID == 31) { vol = 500; }
                    // Frigs, Interceptors, Covops, EAS, Assault Ships, Stealth Bombers
                    else if (groupID == 25 || groupID == 831 || groupID == 830 || groupID == 893 || 
                        groupID == 324 || groupID == 834) { vol = 2500; }
                    // Mining Barges, Exhumers
                    else if (groupID == 463 || groupID == 543) { vol = 3750; }
                    // Destroyers, Interdictors
                    else if (groupID == 420 || groupID == 541) { vol = 5000; }
                    // Cruiser, HACs, HICs, Logistics, Combat Recons, Force Recons
                    else if (groupID == 26 || groupID == 358 || groupID == 894 || groupID == 832 || 
                        groupID == 906 || groupID == 833) { vol = 10000; }
                    // Battlecruiser, Command Ships
                    else if (groupID == 419 || groupID == 540) { vol = 15000; }
                    // Industrials, Transport Ships
                    else if (groupID == 28 || groupID == 380) { vol = 20000; }
                    // Battleships, Blackops, Marauders
                    else if (groupID == 27 || groupID == 898 || groupID == 900) { vol = 50000; }
                    // Freighters, Jump Freighters
                    else if (groupID == 513 || groupID == 902) { vol = 1000000; }
                    // If the ship belongs to a different group we don't know about then just use
                    // it's unpackaged volume.
                    else { vol = itemData.volume; }
                }
                else
                {
                    vol = itemData.volume;
                }
            }

            args.Data = vol;
        }

        static void NameCache_DataUpdateNeeded(object myObject, DataUpdateNeededArgs<int, string> args)
        {
            string name = "";

            lock (tableAdapter)
            {
                tableAdapter.GetName(args.Key, ref name);
            }

            if (name.Equals(""))
            {
                AddItem(args.Key, "Unknown Item (" + args.Key + ")");
                name = "Unknown Item (" + args.Key + ")";
            }

            args.Data = name;
        }
        
        /// <summary>
        /// Get a data table containing all the items specified in the supplied list of IDs 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        private static EveDataSet.invTypesDataTable GetItems(List<int> ids)
        {
            StringBuilder idList = new StringBuilder("");
            foreach (int id in ids)
            {
                idList.Append(" ");
                idList.Append(id);
            }
            return GetItems(idList.ToString());
        }

        /// <summary>
        /// Get a data table containing all the items specified in the supplied list of IDs 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        private static EveDataSet.invTypesDataTable GetItems(string ids)
        {
            EveDataSet.invTypesDataTable items = new EveDataSet.invTypesDataTable();
            lock (tableAdapter)
            {
                tableAdapter.FillByIDs(items, ids);
            }
            return items;
        }
        #endregion
    }
}
