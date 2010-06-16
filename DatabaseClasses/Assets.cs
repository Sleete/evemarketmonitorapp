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
        /// 
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="quantity"></param>
        /// <returns>Return the assets required to build the specified item. (Assuming perfect skills, 
        /// efficiency, etc).</returns>
        public static AssetList GetBillOfMaterials(int itemID, long quantity)
        {
            AssetList retVal = new AssetList();

            throw new NotImplementedException();

            return retVal;
        }

        /// <summary>
        /// This is called after an import of XML asset data but before the changes are commited
        /// to the database.
        /// The aim is to try to work out which assets have simply moved location and which have 
        /// been added or lost. Additionally, if items have moved, update the cost of the item 
        /// stack appropriately.
        /// </summary>
        /// <param name="assetData">The datatable containing the changes that will be applied to the assets database</param>
        /// <param name="charID"></param>
        /// <param name="corp"></param>
        /// <param name="changes">Contains changes in quantities of item stacks that were in the XML.</param>
        /// <param name="gained">A list of items that have been gained from 'nowhere'</param>
        /// <param name="lost">A list of items that have been totally lost from the owner's asset data.</param>
        public static void AnalyseChanges(EMMADataSet.AssetsDataTable assetData, int charID, bool corp,
            AssetList changes, out AssetList gained, out AssetList lost)
        {
            // Note that 'changes' will only include items that exist in the XML from the API. i.e. Any
            // stacks that have been completely removed from a location will not show up.
            // To get these removed assets, we need to retrieve any 'unprocessed' assets from the 
            // assetData table.
            gained = new AssetList();
            lost = new AssetList();

            // Add any unprocessed asset stacks to the 'changes' collection.
            // Although the main data changes have not yet been supplied to the database,
            // the processed flags have been set for the relevant asset rows.
            // This means that we can get the list of unprocessed assets direct from
            // the database.
            List<AssetAccessParams> accessParams = new List<AssetAccessParams>();
            accessParams.Add(new AssetAccessParams(charID, !corp, corp));
            EMMADataSet.AssetsDataTable unprocMatches = new EMMADataSet.AssetsDataTable();
            Assets.GetAssets(unprocMatches, accessParams, 0, 0, 0, 0, false);

            foreach (EMMADataSet.AssetsRow unprocMatch in unprocMatches)
            {
                Asset change = new Asset(unprocMatch);
                // Ignore any assets 'for sale via contract' or 'in transit' 
                if (change.Quantity != 0 && change.StatusID != (int)AssetStatus.States.ForSaleViaContract && 
                    change.StatusID != (int)AssetStatus.States.InTransit)
                {
                    change.Quantity *= -1; // These assets are effectively missing so invert the quantity.
                    changes.Add(change);
                }
            }

            // Note 'changes2' is a list of the same items as 'changes'.
            // It is needed because the filter functionallity changes the list which we cannot 
            // do within the main foreach loop.
            AssetList changes2 = new AssetList();
            foreach (Asset change in changes)
            {
                changes2.Add(change);
            }

            // See if any of our 'missing' assets match transactions that are marked to have thier profit
            // calculated during an assets update.
            #region Update transactions with CalcProfitFromAssets flag
            EMMADataSet.TransactionsDataTable transData = new EMMADataSet.TransactionsDataTable();
            Transactions.AddTransByCalcProfitFromAssets(transData,
                UserAccount.CurrentGroup.GetFinanceAccessParams(APIDataType.Transactions), 0, true);

            foreach (EMMADataSet.TransactionsRow trans in transData)
            {
                long quantityRemaining = trans.Quantity;
                decimal totalBuyPrice = 0;

                changes.ItemFilter = "ItemID = " + trans.ItemID + " AND Quantity < 0";
                foreach (Asset change in changes.FiltredItems)
                {
                    // If we get a match then use the missing items's cost to calculate the 
                    // transaction's profit.
                    // Note that we don't need to adjust any item quantities since either the
                    // changes have already been made or the item stack is 'unprocessed' and will
                    // be cleared out anyway.
                    long deltaQ = Math.Min(Math.Abs(change.Quantity), quantityRemaining);
                    // Adjust the quantity of the 'missing' items.
                    change.Quantity += deltaQ;
                    quantityRemaining -= deltaQ;
                    // Because this transaction's asset quantity change will have been made
                    // back when the transaction was processed, we will have some unexpected
                    // added items as well.
                    // 
                    // E.g. Consider station A and B. At A, there are 2 units of item X, at B 
                    // there is nothing. 
                    // At a later date, the player moves 2 of X from A to B and sells it.
                    // When the sell transaction is processed, station B will be set to -2 
                    // units of X and the transaction will be set as 'CalcProfitFromAssets'.
                    // When the asset update occurs, it will show zero for both locations so 
                    // they will be unprocessed.
                    // This will result in 2 unexplained units gained at station B and 2 lost 
                    // at station A. (We've gone from A=2,B=-2 to A=0 B=0)
                    changes2.ItemFilter = "ItemID = " + trans.ItemID + " AND LocationID = " + trans.StationID +
                        " AND Status = 1";
                    if (changes2.FiltredItems.Count > 0)
                    {
                        // We've already accounted for the cost of items, etc so just reduce the 
                        // quantity.
                        changes2[0].Quantity -= deltaQ;
                    }

                    totalBuyPrice += (change.UnitBuyPrice * deltaQ);

                    if (quantityRemaining != trans.Quantity)
                    {
                        // We've found enough missing items to match this transaction either completely
                        // or partially so calculate it's profit and set it as completed.
                        trans.CalcProfitFromAssets = false;
                        trans.SellerUnitProfit = trans.Price - (totalBuyPrice / trans.Quantity);
                    }
                }
            }
            /*
            foreach (Asset change in changes)
            {
                // Note: because we are changing quantity within the foreach loop
                // we don't use the filter on the AssetList.
                // Instead, just use this if condition each time around.
                if (change.Quantity < 0 && !noTransItemIDs.Contains(change.ItemID))
                {
                    // If we get a match then use the missing items's cost to calculate the 
                    // transaction's profit.
                    // Note that we don't need to adjust any item quantities since either the
                    // changes have already been made or the item stack is 'unprocessed' and will
                    // be cleared out anyway.
                    Transactions.AddTransByCalcProfitFromAssets(transData,
                        UserAccount.CurrentGroup.GetFinanceAccessParams(APIDataType.Transactions),
                        change.ItemID, true);
                    if (transData.Count == 0) { noTransItemIDs.Add(change.ItemID); }
                    foreach (EMMADataSet.TransactionsRow trans in transData)
                    {
                        if (trans.ItemID == change.ItemID && !completedTrans.Contains(trans.ID))
                        {
                            long quantityRemaining = trans.Quantity;
                            TransProcessData data = new TransProcessData();
                            if (processData.ContainsKey(trans.ID))
                            {
                                data = processData[trans.ID];
                                quantityRemaining = data.QuantityRemaining;
                            }
                            else
                            {
                                data.QuantityMatched = 0;
                                data.QuantityRemaining = trans.Quantity;
                                data.TotalBuyPrice = 0;
                                processData.Add(trans.ID, data);
                            }

                            long deltaQ = Math.Min(Math.Abs(change.Quantity), quantityRemaining);
                            // Adjust the quantity of the 'missing' items.
                            change.Quantity += deltaQ;
                            // Because this transaction's asset quantity change will have been made
                            // back when the transaction was processed, we will have some unexpected
                            // added items as well.
                            // 
                            // E.g. Consider station A and B. At A, there are 2 units of item X, at B 
                            // there is nothing. 
                            // At a later date, the player moves 2 of X from A to B and sells it.
                            // When the sell transaction is processed, station B will be set to -2 
                            // units of X and the transaction will be set as 'CalcProfitFromAssets'.
                            // When the asset update occurs, it will show zero for both locations so 
                            // they will be unprocessed.
                            // This will result in 2 unexplained units gained at station B and 2 lost 
                            // at station A. (We've gone from A=2,B=-2 to A=0 B=0)
                            changes2.ItemFilter = "ItemID = " + trans.ItemID + " AND LocationID = " + trans.StationID;
                            foreach (Asset change2 in changes2)
                            {
                                if (change2.Quantity > 0)
                                {
                                    // We've already accounted for the cost of items, etc so just reduce the 
                                    // quantity.
                                    change2.Quantity -= deltaQ;
                                }
                            }

                            data.QuantityRemaining = data.QuantityRemaining - deltaQ;
                            data.QuantityMatched = data.QuantityMatched + deltaQ;
                            data.TotalBuyPrice = data.TotalBuyPrice + (change.UnitBuyPrice * deltaQ);

                            if (data.QuantityRemaining == 0)
                            {
                                // We've found enough missing items to match this transaction completely
                                // so calculate it's profit and set it as completed.
                                trans.CalcProfitFromAssets = false;
                                trans.SellerUnitProfit = trans.Price - (data.TotalBuyPrice / data.QuantityMatched);
                                completedTrans.Add(trans.ID);
                                if (uncompletedTrans.Contains(trans.ID)) { uncompletedTrans.Remove(trans.ID); }
                            }
                            else
                            {
                                // We havn't found enough missing items to completely match this transaction
                                // yet so add to to the list of uncompleted transactions.
                                uncompletedTrans.Add(trans.ID);
                            }
                        }
                    }
                }
            }

            // Calculate profits as best we can for any 'uncompleted' transactions.
            // i.e. those that we did not have enough missing items to match completely.
            foreach (long transID in uncompletedTrans)
            {
                EMMADataSet.TransactionsRow trans = transData.FindByID(transID);
                if (trans != null && processData.ContainsKey(transID))
                {
                    TransProcessData data = processData[transID];
                    trans.CalcProfitFromAssets = false;
                    trans.SellerUnitProfit = trans.Price - (data.TotalBuyPrice / data.QuantityMatched);
                }
            }
             * */
            // Update transactions database
            Transactions.Store(transData);
            #endregion

            // Work through the list of changes.
            // 
            // If items have been added then see if there is a matching quantity that has been removed
            // somwhere. If so, use the old asset stack cost to set the cost for the new asset stack.
            // If we can't find a match for any added items then add them to the 'gained' list.
            // 
            // By definititon, any items that have been removed and match stacks added elsewhere will
            // be matched by the above process. Any removed items that are left over are added to the 
            // 'lost' list of assets.
            changes.ItemFilter = "";

            // If we're in manufacturing mode then we don't want to try and match 
            // lost assets with gained assets until after the user has selected 
            // what has been built, etc.
            if (!UserAccount.Settings.ManufacturingMode)
            {
                #region Try and match gains in one place against losses in another.
                List<int> zeroLossItemIDs = new List<int>();
                foreach (Asset change in changes)
                {
                    if (change.Quantity > 0 && !zeroLossItemIDs.Contains(change.ItemID))
                    {
                        int itemID = change.ItemID;
                        int locationID = change.LocationID;

                        changes2.ItemFilter = "ItemID = " + itemID + " AND Quantity < 0";
                        if (changes2.FiltredItems.Count == 0)
                        {
                            if (!zeroLossItemIDs.Contains(change.ItemID)) { zeroLossItemIDs.Add(change.ItemID); }
                        }

                        foreach (Asset change2 in changes2.FiltredItems)
                        {
                            if (change.Quantity > 0 && change2.Quantity < 0)
                            {
                                // Get the asset data lines associated with the two changes in asset quantities 
                                // that we have found.
                                //bool got1 = Assets.AssetExists(assetData, charID, corp, locationID, itemID,
                                //    change.StatusID, change.ContainerID != 0, change.ContainerID, change.IsContainer,
                                //    true, true, change.AutoConExclude, ref assetID1);
                                //bool got2 = Assets.AssetExists(assetData, charID, corp, change2.LocationID, itemID,
                                //    change2.StatusID, change2.ContainerID != 0, change2.ContainerID, change2.IsContainer,
                                //    true, true, change2.AutoConExclude, ref assetID2);
                                Assets.AddAssetToTable(assetData, change.ID);
                                Assets.AddAssetToTable(assetData, change2.ID);

                                EMMADataSet.AssetsRow row1 = assetData.FindByID(change.ID);
                                EMMADataSet.AssetsRow row2 = assetData.FindByID(change2.ID);
                                Asset a1 = new Asset(row1, null);
                                Asset a2 = new Asset(row2, null);
                                long thisAbsDeltaQ = Math.Min(Math.Abs(change.Quantity), Math.Abs(change2.Quantity));

                                // If the rows are processed then the actual movement of items has already happened
                                // so we don't need to adjust quantities.
                                // If they are not processed then we need to adjust the quantities now.
                                if (!row1.Processed) { row1.Quantity += thisAbsDeltaQ; }
                                if (!row2.Processed) { row2.Quantity -= thisAbsDeltaQ; }

                                // If the stack we're adding to now has zero items in it then we can just 
                                // remove it. Otherwise we need to work out the value of the new stack.
                                if (row1.Quantity > 0)
                                {
                                    // Calculate the cost of items in the new stack based on the 
                                    // cost of items from the old stack.
                                    decimal newCost = 0;
                                    newCost = ((a1.TotalBuyPrice) + (a2.UnitBuyPrice * thisAbsDeltaQ)) /
                                        (a1.Quantity + thisAbsDeltaQ);
                                    row1.Cost = newCost;
                                    row1.CostCalc = true;
                                }
                                else if (row1.Quantity == 0) { row1.Delete(); }
                                if (row2.Quantity == 0) { row2.Delete(); }

                                change.Quantity -= thisAbsDeltaQ;
                                change2.Quantity += thisAbsDeltaQ;
                            }
                        }

                    }
                }
                #endregion
            }

            // We have some items left over that could not be accounted for from losses/gains
            // elsewhere. Add these to the appropriate 'lost' or 'gained' list.
            #region Add remaining unexplained changes to the appropriate lost/gained list
            foreach (Asset change in changes)
            {
                if (change.Quantity != 0)
                {
                    Asset unexplained = new Asset();
                    unexplained.ID = change.ID;
                    unexplained.ItemID = change.ItemID;
                    unexplained.LocationID = change.LocationID;
                    unexplained.Quantity = change.Quantity;
                    unexplained.StatusID = change.StatusID;
                    unexplained.IsContainer = change.IsContainer;
                    unexplained.Container = change.Container;
                    unexplained.AutoConExclude = change.AutoConExclude;
                    unexplained.OwnerID = change.OwnerID;
                    if (change.UnitBuyPricePrecalculated)
                    {
                        unexplained.UnitBuyPrice = change.UnitBuyPrice;
                        unexplained.UnitBuyPricePrecalculated = true;
                    }
                    else
                    {
                        unexplained.UnitBuyPrice = 0;
                        unexplained.UnitBuyPricePrecalculated = false;
                    }
                    if (change.Quantity > 0)
                    {
                        // If the unexplained assets are for sale via contract or in transit then we
                        // would expect them not to show up if they are in the same state as before.
                        // This being the case, we do not need to add them to the list of unexplained items.
                        if (change.StatusID != (int)AssetStatus.States.ForSaleViaContract &&
                            change.StatusID != (int)AssetStatus.States.InTransit) { gained.Add(unexplained); }
                    }
                    else
                    {
                        // If the unexplained assets are for sale via contract or in transit then we
                        // would expect them not to show up if they are in the same state as before.
                        // This being the case, we do not need to add them to the list of unexplained items.
                        if (change.StatusID != (int)AssetStatus.States.ForSaleViaContract &&
                            change.StatusID != (int)AssetStatus.States.InTransit) { lost.Add(unexplained); }
                    }
                }
            }
            #endregion
        }

        /*private class TransProcessData
        {
            // Used by AnalyseChanges method.
            public long QuantityRemaining { get; set; }
            public long QuantityMatched { get; set; }
            public decimal TotalBuyPrice { get; set; }
        }*/

        private struct AssetInfo
        {
            public long assetID;
            public long quantity;
        }


        /// <summary>
        /// This ensures that items in sell orders appear in the list of the player's assets.
        /// It is called just after new asset XML from the API has been processed but before
        /// the update is applied to the database.
        /// </summary>
        public static void ProcessSellOrders(EMMADataSet.AssetsDataTable assetData, AssetList changes,
            int charID, int corpID, bool corp)
        {
            List<int> itemIDs = new List<int>();
            itemIDs.Add(0);
            List<int> stationIDs = new List<int>();
            stationIDs.Add(0);
            List<AssetAccessParams> accessParams = new List<AssetAccessParams>();
            accessParams.Add(new AssetAccessParams(charID, !corp, corp));
            // Get active sell orders
            OrdersList sellOrders = Orders.LoadOrders(accessParams, itemIDs, stationIDs, 999, "Sell");
            EMMADataSet.AssetsRow changedAsset = null;
            // Note that modifiedAssets is indexed first by itemID and second by stationID
            Dictionary<int, Dictionary<int, AssetInfo>> modifiedAssets = new Dictionary<int, Dictionary<int, AssetInfo>>();

            foreach (Order sellOrder in sellOrders)
            {
                bool foundMatch = false;
                long assetID = 0;


                // If there is already an asset row with a state of 'ForSaleViaMarket' in the same location,
                // and with the same item type then check quantity.
                // If it matches then just set to processed and move on.
                // If it does not then record the difference in quantities and go to the next order.
                // If we can't find a match then add a new asset row and record the items gained.
                if (Assets.AssetExists(assetData, charID, corp, sellOrder.StationID, sellOrder.ItemID,
                    (int)AssetStatus.States.ForSaleViaMarket, false, 0, false, false, true, true,
                    true, 0, ref assetID))
                {
                    foundMatch = true;
                }
                else
                {
                    DataRow[] data =
                        assetData.Select("ItemID = " + sellOrder.ItemID + " AND OwnerID = " + charID + " AND CorpAsset = " +
                        (corp ? 1 : 0) + " AND LocationID = " + sellOrder.StationID +
                        " AND Status = " + (int)AssetStatus.States.ForSaleViaMarket);
                    if (data != null && data.Length > 0)
                    {
                        foundMatch = true;
                        assetID = ((EMMADataSet.AssetsRow)data[0]).ID;
                    }
                }

                if (foundMatch)
                {
                    changedAsset = assetData.FindByID(assetID);
                    if (changedAsset.Quantity != sellOrder.RemainingVol)
                    {
                        // If the quantities do not match then store how many units we are removing 
                        // from the stack, the most likely cause is more than one sell order for 
                        // this item in this location and if we know how many units we've removed 
                        // We can make sure that the other order(s) quantity matches up.
                        Dictionary<int, AssetInfo> itemDeltaVol = new Dictionary<int, AssetInfo>();
                        if (modifiedAssets.ContainsKey(sellOrder.ItemID))
                        {
                            itemDeltaVol = modifiedAssets[sellOrder.ItemID];
                        }
                        else
                        {
                            modifiedAssets.Add(sellOrder.ItemID, itemDeltaVol);
                        }
                        if (itemDeltaVol.ContainsKey(sellOrder.StationID))
                        {
                            AssetInfo info = itemDeltaVol[sellOrder.StationID];
                            info.quantity += sellOrder.RemainingVol - changedAsset.Quantity;
                            itemDeltaVol[sellOrder.StationID] = info;
                            changedAsset.Quantity += sellOrder.RemainingVol;
                        }
                        else
                        {
                            AssetInfo info = new AssetInfo();
                            info.quantity = sellOrder.RemainingVol - changedAsset.Quantity;
                            info.assetID = changedAsset.ID;
                            itemDeltaVol.Add(sellOrder.StationID, info);
                            changedAsset.Quantity = sellOrder.RemainingVol;
                        }
                    }
                    changedAsset.Processed = true;
                    // Also set it to processed in the database.
                    SetProcessedFlag(changedAsset.ID, true);
                }

                // We havn't managed to match the order to an existing 'ForSaleViaMarket' stack in
                // the database or in memory.
                // As such, we need to create a new one.
                if (!foundMatch)
                {
                    // Create the new asset row..
                    changedAsset = assetData.NewAssetsRow();
                    changedAsset.AutoConExclude = true;
                    changedAsset.ContainerID = 0;
                    changedAsset.CorpAsset = corp;
                    // Set cost to zero for now, it will be worked out later when gains/losses are reconciled.
                    changedAsset.Cost = 0;
                    changedAsset.CostCalc = false;
                    changedAsset.IsContainer = false;
                    changedAsset.ItemID = sellOrder.ItemID;
                    changedAsset.EveItemID = 0;
                    changedAsset.LocationID = sellOrder.StationID;
                    changedAsset.OwnerID = sellOrder.OwnerID;
                    changedAsset.Processed = true;
                    changedAsset.Quantity = sellOrder.RemainingVol;
                    changedAsset.RegionID = sellOrder.RegionID;
                    changedAsset.ReprocExclude = true;
                    changedAsset.SystemID = sellOrder.SystemID;
                    changedAsset.Status = (int)AssetStatus.States.ForSaleViaMarket;

                    assetData.AddAssetsRow(changedAsset);

                    // Store the changes we are making to quantities
                    Dictionary<int, AssetInfo> itemDeltaVol = new Dictionary<int, AssetInfo>();
                    if (modifiedAssets.ContainsKey(sellOrder.ItemID))
                    {
                        itemDeltaVol = modifiedAssets[sellOrder.ItemID];
                    }
                    else
                    {
                        modifiedAssets.Add(sellOrder.ItemID, itemDeltaVol);
                    }
                    if (itemDeltaVol.ContainsKey(sellOrder.StationID))
                    {
                        AssetInfo info = itemDeltaVol[sellOrder.StationID];
                        info.quantity += sellOrder.RemainingVol;
                        itemDeltaVol[sellOrder.StationID] = info;
                    }
                    else
                    {
                        AssetInfo info = new AssetInfo();
                        info.quantity = sellOrder.RemainingVol;
                        info.assetID = changedAsset.ID;
                        itemDeltaVol.Add(sellOrder.StationID, info);
                    }
                } 
                 
            }


            // Once we've finished processing all the orders, store the overall quantity changes. 
            Dictionary<int, Dictionary<int, AssetInfo>>.Enumerator enumerator = modifiedAssets.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Dictionary<int, AssetInfo>.Enumerator enumerator2 = enumerator.Current.Value.GetEnumerator();
                while(enumerator2.MoveNext())
                {
                    Asset change = new Asset();
                    change.ID = enumerator2.Current.Value.assetID;
                    change.ItemID = enumerator.Current.Key;
                    change.LocationID = enumerator2.Current.Key;
                    change.Quantity = enumerator2.Current.Value.quantity;
                    change.StatusID = (int)AssetStatus.States.ForSaleViaMarket;
                    change.IsContainer = false;
                    change.Container = null;
                    change.AutoConExclude = true;
                    change.OwnerID = corp ? corpID : charID;
                    change.UnitBuyPrice = 0;
                    change.UnitBuyPricePrecalculated = false;    
     
                    changes.Add(change);
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
        public static long UpdateFromTransactions(EMMADataSet.AssetsDataTable assetsData,
            AssetList changes, int charID, int corpID, bool useCorp, long minimumTransID)
        {
            return UpdateFromTransactions(assetsData, changes, charID, corpID, useCorp, minimumTransID, DateTime.MaxValue);
        }
        /// <summary>
        /// Update the assets table based on the transactions meeting the specified criteria.
        /// </summary>
        /// <param name="charID"></param>
        /// <param name="corpID"></param>
        /// <param name="useCorp"></param>
        /// <param name="includeTransAfter"></param>
        /// <returns></returns>
        public static long UpdateFromTransactions(EMMADataSet.AssetsDataTable assetsData,
            AssetList changes, int charID, int corpID, bool useCorp, DateTime includeTransAfter)
        {
            return UpdateFromTransactions(assetsData, changes, charID, corpID, useCorp, -1, includeTransAfter);
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
        private static long UpdateFromTransactions(EMMADataSet.AssetsDataTable assetsData,
            AssetList changes, int charID, int corpID, bool useCorp, long minimumTransID,
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

                    // Change this to not actually make the change in the database. Instead, use the 
                    // asset data table passed in and record the changes made in the 'changes' list.

                    /*ChangeAssets(charID, useCorp, trans.StationID, trans.ItemID, 0, (int)AssetStatus.States.Normal, 
                        false, deltaQuantity, (deltaQuantity > 0 ? trans.Price : 0), deltaQuantity > 0);*/
                    long assetID = 0;
                    EMMADataSet.AssetsRow asset = null;

                    AssetExists(assetsData, ownerID, useCorp, trans.StationID, trans.ItemID,
                        (int)AssetStatus.States.Normal, false, 0, false, false, true, false, true, 0, ref assetID);

                    if (assetID != 0)
                    {
                        // Asset stack already exists in database and/or datatable, modify it 
                        // based upon the transaction data.
                        asset = assetsData.FindByID(assetID);
                        asset.Quantity += deltaQuantity;
                        if (deltaQuantity > 0)
                        {
                            Asset logicalAsset = new Asset(asset);
                            asset.CostCalc = true;
                            asset.Cost = (logicalAsset.TotalBuyPrice + (trans.Price * trans.Quantity)) / 
                                (logicalAsset.Quantity + trans.Quantity);
                        }
                        Asset chg = new Asset(asset);
                        chg.Quantity = deltaQuantity;
                        changes.Add(chg);
                    }
                    else
                    {
                        // Asset does not exist in database so add it to the datatable. 
                        asset = assetsData.NewAssetsRow();
                        asset.Quantity = deltaQuantity;
                        asset.AutoConExclude = false;
                        asset.ContainerID = 0;
                        asset.CorpAsset = useCorp;
                        asset.Cost = trans.Price;
                        asset.CostCalc = true;
                        asset.IsContainer = false;
                        asset.ItemID = trans.ItemID;
                        asset.EveItemID = 0;
                        asset.LocationID = trans.StationID;
                        asset.OwnerID = ownerID;
                        asset.Processed = true;
                        asset.RegionID = trans.RegionID;
                        asset.ReprocExclude = false;
                        asset.Status = (int)AssetStatus.States.Normal;
                        asset.SystemID = Stations.GetStation(trans.StationID).solarSystemID;
                        assetsData.AddAssetsRow(asset);
                        changes.Add(new Asset(asset));
                    }

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
            bool processed, bool ignoreProcessed, bool autoConExclude, bool ignoreAutoConEx, 
            long eveItemInstanceID, ref long assetID)
        {
            bool? exists = false;
            long? assetRowID = 0;

            lock (assetsTableAdapter)
            {
                // Have to be carefull here cos the row could well already exist in our table...
                EMMADataSet.AssetsDataTable tmpTable = new EMMADataSet.AssetsDataTable();
                assetsTableAdapter.FillAssetExists(tmpTable, ownerID, corpAsset, locationID, itemID, status,
                    isContained, containerID, isContainer, processed, ignoreProcessed, autoConExclude,
                    ignoreAutoConEx, eveItemInstanceID, ref exists, ref assetRowID);
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
                    row.IsContainer, row.ReprocExclude, row.Cost, row.CostCalc, row.EveItemID, out retVal);
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
            long containerID, int status, bool autoConExclude, long deltaQuantity, decimal addedItemsCost,
            bool costCalculated)
        {
            int systemID = 0, regionID = 0;
            try
            {
                EveDataSet.staStationsRow station = Stations.GetStation(locationID);
                if (station != null)
                {
                    systemID = station.solarSystemID;
                    regionID = station.regionID;
                }
            }
            catch (EMMADataMissingException)
            {
                EveDataSet.mapSolarSystemsRow system = SolarSystems.GetSystem(locationID);
                if (system != null)
                {
                    systemID = system.solarSystemID;
                    regionID = system.regionID;
                }
            }


            lock (assetsTableAdapter)
            {
                assetsTableAdapter.AddQuantity(ownerID, corpAsset, itemID, locationID, systemID,
                    regionID, status, containerID, autoConExclude, deltaQuantity, addedItemsCost, costCalculated);
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

        /// <summary>
        /// Return the assets owned by the specified characters and corps, at the specified location and
        /// of the specified types.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="accessParams"></param>
        /// <param name="locationID">If locationID = 0 then any location will be returned</param>
        /// <param name="itemID">If itemID = 0 then any item will be returned</param>
        /// <param name="status">If status = 0 then any item will be returned</param>
        /// <param name="processed"></param>
        static public void GetAssets(EMMADataSet.AssetsDataTable table,
            List<AssetAccessParams> accessParams, int locationID, int systemID, int itemID, int status, 
            bool processed)
        {
            lock (assetsTableAdapter)
            {
                assetsTableAdapter.FillByProcessed(table, AssetAccessParams.BuildAccessList(accessParams), 
                    systemID, locationID, itemID, status, processed);
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

        /// <summary>
        /// Add the specified asset row to the specified data table
        /// </summary>
        /// <param name="assets"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        static public void AddAssetToTable(EMMADataSet.AssetsDataTable assets, long ID)
        {
            // Check if the row is already in the table
            EMMADataSet.AssetsRow row = assets.FindByID(ID);
            if (row == null)
            {
                lock (assetsTableAdapter)
                {
                    // If not then retrieve it from the database and place into the table.
                    bool previousClearBeforeFill = assetsTableAdapter.ClearBeforeFill;

                    assetsTableAdapter.ClearBeforeFill = false;
                    assetsTableAdapter.FillByID(assets, ID);

                    assetsTableAdapter.ClearBeforeFill = previousClearBeforeFill;
                }

            }
        }

    }


}
