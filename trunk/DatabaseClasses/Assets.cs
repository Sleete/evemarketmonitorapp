using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading;
using System.IO;

using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.AbstractionClasses;
using EveMarketMonitorApp.GUIElements;

namespace EveMarketMonitorApp.DatabaseClasses
{

    static class Assets
    {
        static private EMMADataSetTableAdapters.AssetsTableAdapter assetsTableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.AssetsTableAdapter();
        private static EMMADataSetTableAdapters.IDTableTableAdapter IDTableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.IDTableTableAdapter();

        /// <summary>
        /// Try to work out which assets have simply moved location and which have been added or lost.
        /// Additionally, if items have moved, make sure we update the cost of the item stack.
        /// 
        /// </summary>
        /// <param name="changes"></param>
        /// <param name="assetData"></param>
        public static void AnalyseChanges(EMMADataSet.AssetsDataTable assetData, 
            Dictionary<int, Dictionary<int, long>> changes)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// This ensures that items in sell orders appear in the list of the player's assets.
        /// It is called just after new asset XML from the API has been processed but before
        /// the update is applied to the database.
        /// 
        /// </summary>
        public static void ProcessSellOrders(EMMADataSet.AssetsDataTable assetData, int charID, bool corp)
        {
            List<int> itemIDs = new List<int>();
            itemIDs.Add(0);
            List<int> stationIDs = new List<int>();
            stationIDs.Add(0);
            List<AssetAccessParams> accessParams = new List<AssetAccessParams>();
            accessParams.Add(new AssetAccessParams(charID, !corp, corp));
            // Get active sell orders
            OrdersList sellOrders = Orders.LoadOrders(accessParams, itemIDs, stationIDs, 999, "Sell");
            EMMADataSet.AssetsDataTable assets = new EMMADataSet.AssetsDataTable();
            EMMADataSet.AssetsRow changedAsset = null;
            Dictionary<int, Dictionary<int, long>> removedAssets = new Dictionary<int, Dictionary<int, long>>();

            foreach (Order sellOrder in sellOrders)
            {
                bool orderDone = false;
                long assetID = 0;


                // If there is already an asset row with a state of 'ForSaleViaMarket' in the same location,
                // same item type and same quantity then just use that and set it to processed
                // (Note that transaction updates should have updated the asset values to match whatever
                // the remaining volume of the order is) 
                // Above is correct. However, if there are multiple sell orders in the same station for the 
                // same item then the asset quantity will be the combined total volume of all the orders.
                if (Assets.AssetExists(assets, charID, corp, sellOrder.StationID, sellOrder.ItemID,
                    (int)AssetStatus.States.ForSaleViaMarket, false, 0, false, false, true, true, ref assetID))
                {
                    changedAsset = assets.FindByID(assetID);
                    if (changedAsset.Quantity <= sellOrder.RemainingVol)
                    {
                        if (!changedAsset.Processed)
                        {
                            if (changedAsset.Quantity < sellOrder.RemainingVol)
                            {
                                // If the quantities do not match then store how many units
                                // we are removing, the most likely cause is more than
                                // one sell order for this item and this location.
                                Dictionary<int, long> itemVolRemoved = new Dictionary<int, long>();
                                if (removedAssets.ContainsKey(sellOrder.StationID))
                                {
                                    itemVolRemoved = removedAssets[sellOrder.StationID];
                                }
                                else
                                {
                                    removedAssets.Add(sellOrder.StationID, itemVolRemoved);
                                }
                                if (itemVolRemoved.ContainsKey(sellOrder.ItemID))
                                {
                                    itemVolRemoved[sellOrder.ItemID] = changedAsset.Quantity - sellOrder.RemainingVol;
                                }
                                else
                                {
                                    itemVolRemoved.Add(sellOrder.ItemID, changedAsset.Quantity - sellOrder.RemainingVol);
                                }
                                changedAsset.Quantity = sellOrder.RemainingVol;
                            }
                            changedAsset.Processed = true;
                            orderDone = true;
                        }
                        else
                        {
                            // The asset record has already been processed.
                            // This means that there must be more than one sell order for the same item 
                            // at this station.
                            // Use the 'removedAssets' dictionary to validate the amounts rather than just 
                            // adding them blindly.
                            if (removedAssets.ContainsKey(sellOrder.StationID))
                            {
                                Dictionary<int, long> itemVolRemoved = removedAssets[sellOrder.StationID];
                                if (itemVolRemoved.ContainsKey(sellOrder.ItemID))
                                {
                                    long remainingRemoved = itemVolRemoved[sellOrder.ItemID];
                                    if (remainingRemoved >= sellOrder.RemainingVol)
                                    {
                                        remainingRemoved -= sellOrder.RemainingVol;
                                        itemVolRemoved[sellOrder.ItemID] = remainingRemoved;
                                        changedAsset.Quantity += sellOrder.RemainingVol;
                                        orderDone = true;
                                    }
                                }
                            }
                        }
                    }
                }


                // We havn't managed to match the order to and existing 'ForSaleViaMarket' stack in
                // the database.
                // As such, we need to work out where the items in the sell order have come from in
                // order to calculate the correct 'cost' value.
                if(!orderDone)
                {
                    // Find any unprocessed asset stacks of the same item as the sell order
                    decimal assetCost = 0;
                    long qToFind = sellOrder.RemainingVol;
                    EMMADataSet.AssetsDataTable unprocMatches = new EMMADataSet.AssetsDataTable();
                    Assets.GetAssets(unprocMatches, accessParams, sellOrder.StationID, sellOrder.SystemID,
                        sellOrder.ItemID);
                    // Work through the unprocessed stacks until we've found enough items 
                    // to match the sell order.
                    foreach (EMMADataSet.AssetsRow unprocMatch in unprocMatches)
                    {
                        if(qToFind > 0)
                        {
                            long q = Math.Min(qToFind, unprocMatch.Quantity);
                            qToFind -= q;
                            Asset unproc = new Asset(unprocMatch, null);
                            assetCost += q * unproc.UnitBuyPrice;

                            // remove the matching unprocessed items from the database.
                            Assets.ChangeAssets(charID, corp, unprocMatch.LocationID, unprocMatch.ItemID,
                                unprocMatch.ContainerID, unprocMatch.Status, unprocMatch.AutoConExclude, -1 * q, 0);
                        }
                    }

                    // Create the new asset row..
                    changedAsset = assets.NewAssetsRow();
                    changedAsset.AutoConExclude = true;
                    changedAsset.ContainerID = 0;
                    changedAsset.CorpAsset = corp;
                    changedAsset.Cost = assetCost / (sellOrder.TotalVol - qToFind);
                    changedAsset.CostCalc = true;
                    changedAsset.IsContainer = false;
                    changedAsset.ItemID = sellOrder.ItemID;
                    changedAsset.LocationID = sellOrder.StationID;
                    changedAsset.OwnerID = sellOrder.OwnerID;
                    changedAsset.Processed = true;
                    changedAsset.Quantity = sellOrder.RemainingVol;
                    changedAsset.RegionID = sellOrder.RegionID;
                    changedAsset.ReprocExclude = true;
                    changedAsset.SystemID = sellOrder.SystemID;
                    changedAsset.Status = (int)AssetStatus.States.ForSaleViaMarket;

                    assets.AddAssetsRow(changedAsset);

                }                
            }
        }

        /// <summary>
        /// GroupBy can be 'Region', 'Owner', 'System' or 'None'.
        /// Gruoping by region or system will also group by owner.
        /// </summary>
        /// <param name="accessParams"></param>
        /// <param name="itemIDs"></param>
        /// <param name="groupBy"></param>
        public static void BuildResults(List<AssetAccessParams> accessParams, List<int> itemIDs, string groupBy)
        {
            if (itemIDs.Count == 0) { itemIDs.Add(0); }

            string itemString = "";
            foreach (int item in itemIDs) { itemString = itemString + (itemString.Length == 0 ? "" : ",") + item; }

            lock (assetsTableAdapter)
            {
                assetsTableAdapter.BuildResults(AssetAccessParams.BuildAccessList(accessParams), itemString,
                    0, groupBy);
            }
        }

        public static bool GetResultsPage(int startPos, int pageSize, ref AssetList assets)
        {
            if (startPos <= 0) startPos = 1;
            EMMADataSet.AssetsDataTable table = new EMMADataSet.AssetsDataTable();
            lock (assetsTableAdapter)
            {
                assetsTableAdapter.FillByResultsPage(table, startPos, pageSize);
            }
            foreach (EMMADataSet.AssetsRow asset in table)
            {
                assets.Add(new Asset(asset, null));
            }

            return table.Count == pageSize;
        }


        public static decimal GetAssetsValue(int charID, bool corp)
        {
            decimal retVal = 0;
            List<AssetAccessParams> accessParams = new List<AssetAccessParams>();
            accessParams.Add(new AssetAccessParams(charID, !corp, corp));
            EMMADataSet.AssetsDataTable assets = GetAssets(accessParams);
            //StreamWriter log = File.CreateText("C:\\NAVAssets.txt");
            //try
            //{
                foreach (EMMADataSet.AssetsRow asset in assets)
                {
                    // Use The forge to value items if possible.
                    decimal value = UserAccount.CurrentGroup.ItemValues.GetItemValue(asset.ItemID, 10000002, true);
                    retVal += value * asset.Quantity;
                    //log.WriteLine(Items.GetItemName(asset.ItemID) + ", " + asset.Quantity + ", " + value); 
                }
            //}
            //finally
            //{
            //    log.Close();
            //}

            return retVal;
        }


        
        /// <summary>
        /// Update the assets table based on the transactions meeting the specified criteria.
        /// </summary>
        /// <param name="charID"></param>
        /// <param name="corpID"></param>
        /// <param name="useCorp"></param>
        /// <param name="minimumTransID"></param>
        /// <returns></returns>
        public static long UpdateFromTransactions(int charID, int corpID, bool useCorp, long minimumTransID)
        {
            return UpdateFromTransactions(charID, corpID, useCorp, minimumTransID, DateTime.MaxValue);
        }
        /// <summary>
        /// Update the assets table based on the transactions meeting the specified criteria.
        /// </summary>
        /// <param name="charID"></param>
        /// <param name="corpID"></param>
        /// <param name="useCorp"></param>
        /// <param name="includeTransAfter"></param>
        /// <returns></returns>
        public static long UpdateFromTransactions(int charID, int corpID, bool useCorp, DateTime includeTransAfter)
        {
            return UpdateFromTransactions(charID, corpID, useCorp, -1, includeTransAfter);
        }
        /// <summary>
        /// Update the assets table based on the transactions meeting the specified criteria.
        /// </summary>
        /// <param name="charID"></param>
        /// <param name="corpID"></param>
        /// <param name="useCorp"></param>
        /// <param name="minimumTransID"></param>
        /// <param name="includeTransAfter"></param>
        /// <returns></returns>
        private static long UpdateFromTransactions(int charID, int corpID, bool useCorp, long minimumTransID,
            DateTime includeTransAfter)
        {
            long maxID = 0;
            int ownerID = useCorp ? corpID : charID;

            EMMADataSet.TransactionsDataTable transactions = new EMMADataSet.TransactionsDataTable();
            transactions = GetTransactions(charID, corpID, useCorp, minimumTransID, includeTransAfter);

            foreach (EMMADataSet.TransactionsRow trans in transactions)
            {
                // If the ID is greater than 9000000000000000000 then it must have been created by EMMA as part of
                // an item exchange contract. These should be ignored here.
                if (trans.ID < 9000000000000000000)
                {
                    int deltaQuantity = trans.Quantity;
                    if (trans.SellerID == ownerID) { deltaQuantity *= -1; }
                    // We just adjust the 'normal' assets pile even though the change (particularaly
                    // where items are being removed) is likely to affect a different pile 
                    // (e.g. 'ForSaleViaMarket')
                    ChangeAssets(charID, useCorp, trans.StationID, trans.ItemID, 0, (int)AssetStatus.States.Normal, 
                        false, deltaQuantity, trans.Price);
                    if (trans.ID > maxID) { maxID = trans.ID; }
                }
            }

            return maxID;
        }
        

        /// <summary>
        /// Get a list of changes that would be made to assets by the transactions meeting the 
        /// specified criteria. (i.e. What would change if 'UpdateFromTransactions' was called 
        /// with the same parameters).
        /// If the return structure, the outer dictionary is indexed by stationID, the inner by 
        /// itemID. The contained value is the change to be made to the quantity of the asset.
        /// </summary>
        /// <param name="charID"></param>
        /// <param name="corpID"></param>
        /// <param name="useCorp"></param>
        /// <param name="minimumTransID"></param>
        /// <param name="includeTransAfter"></param>
        /// <returns></returns>
        public static Dictionary<int, Dictionary<int, long>> GetQuantityChangesFromTransactions(
            int charID, int corpID, bool useCorp, long minimumTransID, DateTime includeTransAfter)
        {
            Dictionary<int, Dictionary<int, long>> retVal = new Dictionary<int, Dictionary<int, long>>();
            int ownerID = useCorp ? corpID : charID;

            EMMADataSet.TransactionsDataTable transactions = new EMMADataSet.TransactionsDataTable();
            transactions = GetTransactions(charID, corpID, useCorp, minimumTransID, includeTransAfter);

            foreach (EMMADataSet.TransactionsRow trans in transactions)
            {
                int stationID = trans.StationID;
                int itemID = trans.ItemID;
                int deltaQuantity = trans.Quantity;
                if (trans.SellerID == ownerID) { deltaQuantity *= -1; }
                if (!retVal.ContainsKey(stationID))
                {
                    retVal.Add(stationID, new Dictionary<int, long>());
                }
                if (!retVal[stationID].ContainsKey(itemID))
                {
                    retVal[stationID].Add(itemID, 0);
                }
                retVal[stationID][itemID] += deltaQuantity;
            }

            return retVal;
        }

        /// <summary>
        /// Get transactions meeting the specified parameters.
        /// </summary>
        /// <param name="charID"></param>
        /// <param name="corpID"></param>
        /// <param name="useCorp"></param>
        /// <param name="minimumTransID"></param>
        /// <param name="includeTransAfter"></param>
        /// <returns></returns>
        private static EMMADataSet.TransactionsDataTable GetTransactions(int charID, int corpID, bool useCorp,
            long minimumTransID, DateTime includeTransAfter)
        {
            int ownerID = useCorp ? corpID : charID;
            EMMADataSet.TransactionsDataTable transactions = new EMMADataSet.TransactionsDataTable();
            if (minimumTransID == -1)
            {
                List<FinanceAccessParams> accessParams = new List<FinanceAccessParams>();
                accessParams.Add(new FinanceAccessParams(ownerID, false));
                List<int> ids = new List<int>();
                ids.Add(0);
                transactions = Transactions.GetTransData(accessParams, ids, ids, ids, includeTransAfter, DateTime.MaxValue, "");
            }
            else
            {
                transactions = Transactions.GetTransData(ownerID, false, 0, 0, 0, minimumTransID, "");
            }
            return transactions;
        }
        
        
        public static EMMADataSet.IDTableDataTable GetInvolvedItemIDs(List<AssetAccessParams> accessParams)
        {
            EMMADataSet.IDTableDataTable table = new EMMADataSet.IDTableDataTable();
            lock (assetsTableAdapter)
            {
                IDTableAdapter.FillItemIDsByAssets(table, AssetAccessParams.BuildAccessList(accessParams), 0);
            }
            return table;
        }

        public static EMMADataSet.IDTableDataTable GetInvolvedStationIDs(List<AssetAccessParams> accessParams,
            int itemID, int systemID)
        {
            EMMADataSet.IDTableDataTable table = new EMMADataSet.IDTableDataTable();
            lock (assetsTableAdapter)
            {
                IDTableAdapter.FillStationIDsByAssets(table, AssetAccessParams.BuildAccessList(accessParams),
                    itemID, systemID);
            }
            return table;
        }

        public static EMMADataSet.IDTableDataTable GetInvolvedSystemIDs(List<AssetAccessParams> accessParams,
            int itemID, int regionID)
        {
            EMMADataSet.IDTableDataTable table = new EMMADataSet.IDTableDataTable();
            lock (assetsTableAdapter)
            {
                IDTableAdapter.FillSystemIDsByAssets(table, AssetAccessParams.BuildAccessList(accessParams),
                    itemID, regionID);
            }
            return table;
        }

        public static EMMADataSet.IDTableDataTable GetInvolvedSystemIDs(int ownerID, bool corpAsset,
            List<int> regionIDs, List<int> stationIDs, bool includeContainers, bool includeContents)
        {
            EMMADataSet.IDTableDataTable table = new EMMADataSet.IDTableDataTable();
            if (regionIDs == null) { regionIDs = new List<int>(); }
            if (stationIDs == null) { stationIDs = new List<int>(); }
            if (regionIDs.Count == 0) { regionIDs.Add(0);}
            if (stationIDs.Count == 0) { stationIDs.Add(0); }
            StringBuilder regionIDList = new StringBuilder();
            StringBuilder stationIDList = new StringBuilder();
            foreach (int id in regionIDs)
            {
                if (regionIDList.Length != 0) { regionIDList.Append(","); }
                regionIDList.Append(id);
            }
            foreach (int id in stationIDs)
            {
                if (stationIDList.Length != 0) { stationIDList.Append(","); }
                stationIDList.Append(id);
            }

            lock (assetsTableAdapter)
            {
                IDTableAdapter.FillLimitedSystemIDsByAssets(table,ownerID, corpAsset,
                    regionIDList.ToString(), stationIDList.ToString(), includeContainers, includeContents);
            }
            return table;
        }

        public static EMMADataSet.IDTableDataTable GetInvolvedRegionIDs(List<AssetAccessParams> accessParams,
            int itemID)
        {
            EMMADataSet.IDTableDataTable table = new EMMADataSet.IDTableDataTable();
            lock (assetsTableAdapter)
            {
                IDTableAdapter.FillRegionIDsByAssets(table, AssetAccessParams.BuildAccessList(accessParams),
                    itemID);
            }
            return table;
        }


        /// <summary>
        /// Retrieve assets that are stored in the specified container 
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static AssetList LoadAssets(Asset container)
        {
            AssetList retVal = new AssetList();
            EMMADataSet.AssetsDataTable table = new EMMADataSet.AssetsDataTable();
            lock (assetsTableAdapter)
            {
                assetsTableAdapter.FillByContainerID(table, container.ID);
            }

            foreach (EMMADataSet.AssetsRow row in table)
            {
                Asset asset = new Asset(row, container);
                retVal.Add(asset);
            }
            return retVal;
        }

        /// <summary>
        /// Return a list of assets that meet the specified parameters.
        /// Note that the list does NOT include assets stored within other containers, ships, etc.
        /// </summary>
        /// <param name="accessParams"></param>
        /// <param name="regionIDs"></param>
        /// <param name="itemID"></param>
        /// <param name="locationID"></param>
        /// <returns></returns>
        public static AssetList LoadAssets(List<AssetAccessParams> accessParams, List<int> regionIDs,
            int itemID, int locationID, int systemID, bool containersOnly)
        {
            return LoadAssets(accessParams, regionIDs, itemID, locationID, systemID, containersOnly, 0, false);
        }

        /// <summary>
        /// Return a list of assets that meet the specified parameters.
        /// Note that the list does NOT include assets stored within other containers, ships, etc.
        /// </summary>
        /// <param name="accessParams"></param>
        /// <param name="regionIDs"></param>
        /// <param name="itemID"></param>
        /// <param name="locationID"></param>
        /// <returns></returns>
        public static AssetList LoadAssets(List<AssetAccessParams> accessParams, List<int> regionIDs,
            int itemID, int locationID, int systemID, bool containersOnly, int status, bool excludeContainers)
        {
            AssetList retVal = new AssetList();
            EMMADataSet.AssetsDataTable table = new EMMADataSet.AssetsDataTable();
            string regionString = "";
            foreach (int region in regionIDs)
            {
                regionString = regionString + (regionString.Length == 0 ? "" : ",") + region;
            }
            lock (assetsTableAdapter)
            {
                assetsTableAdapter.FillByAny(table, AssetAccessParams.BuildAccessList(accessParams),
                    regionString, systemID, locationID, itemID, containersOnly, false, status);
            }

            foreach (EMMADataSet.AssetsRow row in table)
            {
                if (row.Quantity > 0 && (!row.IsContainer || !excludeContainers))
                {
                    Asset asset = new Asset(row, null);
                    retVal.Add(asset);
                }
            }
            return retVal;
        }

        /// <summary>
        /// Return a list of assets that meet the specified parameters.
        /// </summary>
        public static AssetList LoadReprocessableAssets(int ownerID, bool corpAssets, int stationID,
            int status, bool includeContainers, bool includeNonContainers)
        {
            AssetList retVal = new AssetList();
            EMMADataSet.AssetsDataTable table = new EMMADataSet.AssetsDataTable();

            lock (assetsTableAdapter)
            {
                assetsTableAdapter.FillByReprocessables(table, ownerID, corpAssets, stationID,
                    status, includeContainers, includeNonContainers);
            }

            foreach (EMMADataSet.AssetsRow row in table)
            {
                if (row.Quantity > 0)
                {
                    Asset asset = new Asset(row, null);
                    retVal.Add(asset);
                }
            }
            return retVal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="ownerID"></param>
        /// <param name="corpAsset"></param>
        /// <param name="locationID"></param>
        /// <param name="itemID"></param>
        /// <param name="containerID"></param>
        /// <param name="isContainer"></param>
        /// <param name="assetID"></param>
        /// <returns></returns>
        public static bool AssetExists(EMMADataSet.AssetsDataTable table, int ownerID, bool corpAsset, 
            int locationID, int itemID, int status, bool isContained, long containerID, bool isContainer, 
            bool processed, bool ignoreProcessed, bool autoConExclude, ref long assetID)
        {
            bool? exists = false;
            long? assetRowID = 0;

            lock (assetsTableAdapter)
            {
                // Have to be carefull here cos the row could well already exist in our table...
                EMMADataSet.AssetsDataTable tmpTable = new EMMADataSet.AssetsDataTable();
                assetsTableAdapter.FillAssetExists(tmpTable, ownerID, corpAsset, locationID, itemID, status,
                    isContained, containerID, isContainer, processed, ignoreProcessed, autoConExclude,
                    ref exists, ref assetRowID);
                long id = assetRowID.HasValue ? assetRowID.Value : 0;
                EMMADataSet.AssetsRow row = table.FindByID(id);
                if (row == null && id != 0)
                {
                    EMMADataSet.AssetsRow dbrow = tmpTable.FindByID(id);
                    table.ImportRow(dbrow);
                }
            }

            assetID = assetRowID.HasValue ? assetRowID.Value : 0;
            return exists.HasValue ? exists.Value : false;
        }


        /// <summary>
        /// Update the database as described in the supplied assets table parameter 
        /// </summary>
        /// <param name="table"></param>
        public static void UpdateDatabase(EMMADataSet.AssetsDataTable table)
        {
            lock (assetsTableAdapter)
            {
                try
                {
                    assetsTableAdapter.Update(table);
                }
                catch (System.Data.ConstraintException ex)
                {
                    throw new EMMADataException(ExceptionSeverity.Error, "Error updating database", ex);
                }
            }
        }

        /// <summary>
        /// Update the database as described in the supplied assets table parameter 
        /// </summary>
        /// <param name="table"></param>
        public static void UpdateDatabase(EMMADataSet.AssetsRow row)
        {
            lock (assetsTableAdapter)
            {
                try
                {
                    assetsTableAdapter.Update(row);
                }
                catch (System.Data.ConstraintException ex)
                {
                    throw new EMMADataException(ExceptionSeverity.Error, "Error updating database", ex);
                }
            }
        }


        public static long AddRowToDatabase(EMMADataSet.AssetsRow row)
        {
            long? retVal = 0;
            lock (assetsTableAdapter)
            {
                assetsTableAdapter.Insert(row.OwnerID, row.CorpAsset, row.LocationID, row.ItemID, row.SystemID,
                    row.RegionID, row.ContainerID, row.Quantity, row.Status, row.Processed, row.AutoConExclude,
                    row.IsContainer, row.ReprocExclude, row.Cost, row.CostCalc, out retVal);
            }
            return retVal.HasValue ? retVal.Value : 0;
        }

        /// <summary>
        /// Set the 'processed' flag for the assets meeting the specified criteria. 
        /// </summary>
        /// <param name="ownerID"></param>
        /// <param name="locationID">The location of assets to set the processed flag for. If it's zero then any location.</param>
        /// <param name="itemID">The item ID of assets to set the processed flag for. If it's zero then any item.</param>
        /// <param name="processed"></param>
        /// <param name="corpAsset"></param>
        public static void SetProcessedFlag(int ownerID, bool corpAsset, int status, bool processed)
        {
            lock (assetsTableAdapter)
            {
                assetsTableAdapter.SetProcFlag(0, ownerID, corpAsset, status, processed);
            }
        }
        public static void SetProcessedFlag(long assetID, bool processed)
        {
            lock (assetsTableAdapter)
            {
                assetsTableAdapter.SetProcFlag(assetID, 0, false, 0, processed);
            }
        }

        /// <summary>
        /// Set the Auto-contractor exclusion flag for the specified asset. 
        /// </summary>
        /// <param name="charID"></param>
        /// <param name="corpAsset"></param>
        /// <param name="stationID"></param>
        /// <param name="itemID"></param>
        /// <param name="exclude"></param>
        public static void SetAutoConExcludeFlag(int charID, bool corpAsset, int stationID, int itemID, bool exclude)
        {
            lock (assetsTableAdapter)
            {
                assetsTableAdapter.SetExcludeFlag(0, charID, stationID, itemID, 1, 0, exclude, corpAsset);
            }
        }
        public static void SetAutoConExcludeFlag(long assetID, bool exclude)
        {
            lock (assetsTableAdapter)
            {
                assetsTableAdapter.SetExcludeFlag(assetID, 0, 0, 0, 1, 0, exclude, false);
            }
        }

        public static void SetReprocExcludeFlag(long assetID, bool exclude)
        {
            lock (assetsTableAdapter)
            {
                assetsTableAdapter.SetReprocExclude(assetID, 0, 0, 0, 1, 0, exclude, false);
            }
        }


        /// <summary>
        /// Clear assets from the asset table that have the 'processed' flag set to zero and
        /// meet the other criteria specified.
        /// </summary>
        /// <param name="ownerID"></param>
        /// <param name="includePersonal"></param>
        /// <param name="includeCorporate"></param>
        public static void ClearUnProcessed(int ownerID, bool includePersonal, bool includeCorporate,
            bool onlyContainers)
        {
            lock (assetsTableAdapter)
            {
                assetsTableAdapter.ClearUnProc(ownerID, includePersonal, includeCorporate, onlyContainers);
            }
        }


       
        /// <summary>
        /// Get the total quantity of the item specified for all the corporations and characters 
        /// in the access list.
        /// </summary>
        /// <param name="charIDs"></param>
        /// <param name="itemID"></param>
        /// <returns></returns>
        static public long GetTotalQuantity(List<AssetAccessParams> accessParams, int itemID)
        {
            long? retVal = 0;
            lock (assetsTableAdapter)
            {
                assetsTableAdapter.TotalQuantity(AssetAccessParams.BuildAccessList(accessParams), itemID, 
                    "0", "0", true, true, ref retVal);
            }
            return retVal.HasValue ? retVal.Value : 0;
        }

        /// <summary>
        /// Get the total quantity of the item specified for all the corporations and characters 
        /// in the access list.
        /// </summary>
        /// <param name="charIDs"></param>
        /// <param name="itemID"></param>
        /// <returns></returns>
        static public long GetTotalQuantity(List<AssetAccessParams> accessParams, int itemID, List<int> stationIDs,
            List<int> regionIDs, bool includeItemsInTransit, bool includeContainers)
        {
            long? retVal = 0;
            string stationList = "0";
            string regionList = "0";

            if (regionIDs.Count > 0)
            {
                regionList = "";
                foreach (int regionID in regionIDs)
                {
                    regionList = regionList + (regionList.Length == 0 ? "" : ",") + regionID;
                }
            }
            if (stationIDs.Count > 0)
            {
                stationList = "";
                foreach (int stationID in stationIDs)
                {
                    stationList = stationList + (stationList.Length == 0 ? "" : ",") + stationID;
                }
            }

            lock (assetsTableAdapter)
            {
                assetsTableAdapter.TotalQuantity(AssetAccessParams.BuildAccessList(accessParams), itemID,
                    stationList, regionList, includeItemsInTransit, includeContainers, ref retVal);
            }
            return retVal.HasValue ? retVal.Value : 0;
        }

        static public long GetTotalQuantity(List<AssetAccessParams> accessParams, 
            List<FinanceAccessParams> financeParams, int itemID, DateTime dateTime)
        {
            long retVal = 0;
            List<int> itemIDList = new List<int>();
            itemIDList.Add(itemID);

            long quantityNow = GetTotalQuantity(accessParams, itemID);
            OrdersList sellOrders = Orders.LoadOrders(accessParams, itemIDList,
                new List<int>(), (int)OrderState.Active, "sell");
            foreach (Order sellOrder in sellOrders)
            {
                quantityNow += sellOrder.RemainingVol;
            }
            retVal = quantityNow;

            TransactionList transactions = Transactions.LoadTransactions(financeParams, 
                itemIDList, new List<int>(), dateTime, DateTime.UtcNow, "buy");
            foreach (Transaction trans in transactions)
            {
                retVal -= trans.Quantity;
            }

            transactions = Transactions.LoadTransactions(financeParams,
                itemIDList, new List<int>(), dateTime, DateTime.UtcNow, "sell");
            foreach (Transaction trans in transactions)
            {
                retVal += trans.Quantity;
            }

            return retVal;
        }

        /// <summary>
        /// Modify the quantity of the specified asset.
        /// </summary>
        /// <param name="ownerID"></param>
        /// <param name="corpAsset"></param>
        /// <param name="locationID"></param>
        /// <param name="itemID"></param>
        /// <param name="containerID"></param>
        /// <param name="status"></param>
        /// <param name="autoConExclude"></param>
        /// <param name="deltaQuatnity"></param>
        static public void ChangeAssets(int ownerID, bool corpAsset, int locationID, int itemID,
            long containerID, int status, bool autoConExclude, long deltaQuantity, decimal addedItemsCost)
        {
            int systemID = 0, regionID = 0;
            EveDataSet.staStationsRow station = Stations.GetStation(locationID);
            if (station != null)
            {
                systemID = station.solarSystemID;
                regionID = station.regionID;
            }
            else
            {
                throw new EMMADataMissingException(ExceptionSeverity.Critical, "A station is missing from the " +
                    "database. This is likley due to new systems being " +
                    "added to the game that are not in EMMA's database. A data update is required.",
                    "staStations", locationID.ToString());
            }
            lock (assetsTableAdapter)
            {
                assetsTableAdapter.AddQuantity(ownerID, corpAsset, itemID, locationID, systemID,
                    regionID, status, 0, autoConExclude, deltaQuantity, addedItemsCost, true);
            }
        }
        

        /// <summary>
        /// Return the assets stored at the specified station
        /// </summary>
        /// <param name="stationID"></param>
        /// <returns></returns>
        /*static public EMMADataSet.AssetsDataTable GetAssetsAt(List<AccessParams> accessParams, int stationID)
        {
            EMMADataSet.AssetsDataTable retVal = new EMMADataSet.AssetsDataTable();
            assetsTableAdapter.FillByItemAndLocation(retVal, BuildAccessList(accessParams), "", stationID, 0);
            return retVal;
        }*/

        /// <summary>
        /// Return the assets owned by the specified characters and corps
        /// </summary>
        /// <param name="accessParams"></param>
        /// <returns></returns>
        static public EMMADataSet.AssetsDataTable GetAssets(List<AssetAccessParams> accessParams)
        {
            EMMADataSet.AssetsDataTable retVal = new EMMADataSet.AssetsDataTable();
            lock (assetsTableAdapter)
            {
                assetsTableAdapter.FillByAny(retVal, AssetAccessParams.BuildAccessList(accessParams), "", 0, 0, 0, false, true, 0);
            }
            return retVal;
        }

        /// <summary>
        /// Return the assets owned by the specified characters and corps, at the specified location and
        /// of the specified types.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="accessParams"></param>
        /// <param name="locationID">If locationID = 0 then any location will be returned</param>
        /// <param name="itemID">If itemID = 0 then any item will be returned</param>
        static public void GetAssets(EMMADataSet.AssetsDataTable table, 
            List<AssetAccessParams> accessParams, int locationID, int systemID, int itemID)
        {
            lock (assetsTableAdapter)
            {
                assetsTableAdapter.FillByAny(table, AssetAccessParams.BuildAccessList(accessParams), "", 
                    systemID, locationID, itemID, false, true, 0);
            }
        }

        static public EMMADataSet.AssetsDataTable GetAutoConAssets(int charID, bool corpAssets, int stationID,
            bool excludeContainers)
        {
            EMMADataSet.AssetsDataTable retVal = new EMMADataSet.AssetsDataTable();

            List<int> itemIDs = new List<int>();
            if (itemIDs == null || itemIDs.Count == 0) { itemIDs = new List<int>(); itemIDs.Add(0); }
            if (UserAccount.CurrentGroup.Settings.AutoCon_TradedItems)
            {
                itemIDs = UserAccount.CurrentGroup.TradedItems.GetAllItemIDs();
            }
            StringBuilder itemsString = new StringBuilder("");
            foreach (int itemID in itemIDs)
            {
                if (itemsString.Length > 0) { itemsString.Append(","); }
                itemsString.Append(itemID);
            }
            
            lock (assetsTableAdapter)
            {
                assetsTableAdapter.FillByAutoCon(retVal, charID, corpAssets, stationID, 
                    itemsString.ToString(), excludeContainers);
            }
            return retVal;
        }

        static public EMMADataSet.AssetsDataTable GetAutoConAssets(int charID, bool corpAssets, string locationName,
            bool excludeContainers)
        {
            GroupLocation location = GroupLocations.GetLocationDetail(locationName);
            List<int> regionsIDs = location.Regions;
            List<int> stationIDs = location.Stations;
            if (regionsIDs == null || regionsIDs.Count == 0) { regionsIDs = new List<int>(); regionsIDs.Add(0); }
            if (stationIDs == null || stationIDs.Count == 0) { stationIDs = new List<int>(); stationIDs.Add(0); }

            StringBuilder regionsString = new StringBuilder("");
            StringBuilder stationsString = new StringBuilder("");
            foreach (int regionID in regionsIDs)
            {
                if (regionsString.Length > 0) { regionsString.Append(","); }
                regionsString.Append(regionID);
            }
            foreach (int stationID in stationIDs)
            {
                if (stationsString.Length > 0) { stationsString.Append(","); }
                stationsString.Append(stationID);
            }

            List<int> itemIDs = new List<int>();
            if (itemIDs == null || itemIDs.Count == 0) { itemIDs = new List<int>(); itemIDs.Add(0); }
            if (UserAccount.CurrentGroup.Settings.AutoCon_TradedItems)
            {
                itemIDs = UserAccount.CurrentGroup.TradedItems.GetAllItemIDs();
            }
            StringBuilder itemsString = new StringBuilder("");
            foreach (int itemID in itemIDs)
            {
                if (itemsString.Length > 0) { itemsString.Append(","); }
                itemsString.Append(itemID);
            }            

            EMMADataSet.AssetsDataTable retVal = new EMMADataSet.AssetsDataTable();
            lock (assetsTableAdapter)
            {
                assetsTableAdapter.FillByAutoConAny(retVal, charID, corpAssets, stationsString.ToString(),
                    regionsString.ToString(), itemsString.ToString(), excludeContainers);
            }
            return retVal;
        }

        /// <summary>
        /// Get detailed information on the specified asset
        /// </summary>
        /// <param name="stationID"></param>
        /// <param name="itemID"></param>
        /// <returns></returns>
        static public EMMADataSet.AssetsRow GetAssetDetail(long ID)
        {
            EMMADataSet.AssetsRow retVal = null;
            EMMADataSet.AssetsDataTable table = new EMMADataSet.AssetsDataTable();
            lock (assetsTableAdapter)
            {
                assetsTableAdapter.FillByID(table, ID);
            }

            if (table != null)
            {
                if (table.Count > 0)
                {
                    retVal = table[0];
                }
            }
            return retVal;
        }


    }


}
