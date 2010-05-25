using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Data.Sql;
using System.Data;
using System.Xml;

using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.AbstractionClasses;

namespace EveMarketMonitorApp.DatabaseClasses
{
    class Transactions : IProvideStatus
    {
        private static EMMADataSetTableAdapters.TransactionsTableAdapter tableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.TransactionsTableAdapter();
        private static EMMADataSetTableAdapters.IDTableTableAdapter IDTableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.IDTableTableAdapter();

        public event StatusChangeHandler StatusChange;

        /// <summary>
        /// This is used for calculating per unit profit when adding sell transactions from the API. 
        /// It should not be used outside of that context.
        /// </summary>
        /// <param name="transData"></param>
        /// <param name="newRow"></param>
        /// <returns></returns>
        public static decimal CalcProfit(int charID, bool corp, EMMADataSet.TransactionsDataTable transData,
            EMMADataSet.TransactionsRow newRow)
        {
            decimal retVal = 0;
            EMMADataSet.AssetsDataTable existingAssets = new EMMADataSet.AssetsDataTable();
            int stationID = newRow.StationID;

            try
            {
                // If there are matching assets for the specified character or corp at the 
                // transaction location then use the cost of those assets to calculate profit. 
                List<AssetAccessParams> assetAccessParams = new List<AssetAccessParams>();
                assetAccessParams.Add(new AssetAccessParams(charID, !corp, corp));
                Assets.GetAssets(existingAssets, assetAccessParams, stationID, 
                    Stations.GetStation(stationID).solarSystemID, newRow.ItemID);
                if (existingAssets != null)
                {
                    decimal totalBuyPrice = 0;
                    long qToFind = newRow.Quantity;
                    foreach (EMMADataSet.AssetsRow existingAsset in existingAssets)
                    {
                        // If possible, use data from assets that are currently for sale via the market.
                        if (existingAsset.Status == (int)AssetStatus.States.ForSaleViaMarket && 
                            existingAsset.Quantity > 0)
                        {
                            Asset asset = new Asset(existingAsset, null);
                            long q = Math.Min(qToFind, asset.Quantity);
                            qToFind -= q;
                            totalBuyPrice += asset.UnitBuyPrice * q;

                            // Adjust assets data in accordance with items that were sold.
                            long deltaQuantity = -1 * q;
                            // Note, since we're removing assets, the cost and costcalc parameters
                            // will be ignored.
                            Assets.ChangeAssets(charID, corp, newRow.StationID, newRow.ItemID, 
                                existingAsset.ContainerID, existingAsset.Status, existingAsset.AutoConExclude,
                                deltaQuantity, 0, false);
                        }
                    }
                    // If we could not find enough assets 'ForSaleViaMarket' to match the 
                    // sell transaction then look at assets that are in transit or just sat 
                    // in the hanger.
                    // (Don't use assets that are containers!)
                    if (qToFind > 0)
                    {
                        foreach (EMMADataSet.AssetsRow existingAsset in existingAssets)
                        {
                            if (existingAsset.Status != (int)AssetStatus.States.ForSaleViaMarket &&
                                existingAsset.Status != (int)AssetStatus.States.ForSaleViaContract &&
                                existingAsset.Quantity > 0 &&
                                !existingAsset.IsContainer)
                            {
                                Asset asset = new Asset(existingAsset, null);
                                long q = Math.Min(qToFind, asset.Quantity);
                                qToFind -= q;
                                totalBuyPrice += asset.UnitBuyPrice * q;

                                // Adjust assets data in accordance with items that were sold.
                                long deltaQuantity = -1 * q;
                                // Note, since we're removing assets, the cost and costcalc parameters
                                // will be ignored.
                                Assets.ChangeAssets(charID, corp, newRow.StationID, newRow.ItemID, 
                                    existingAsset.ContainerID, existingAsset.Status, existingAsset.AutoConExclude, 
                                    deltaQuantity, 0, false);
                            }
                        }
                    }
                    if (qToFind < newRow.Quantity)
                    {
                        decimal unitBuyPrice = totalBuyPrice / (newRow.Quantity - qToFind);
                        retVal = newRow.Price - unitBuyPrice;
                    }
                }
                else
                {
                    // If there are no assets that match what was sold then look at other
                    // buy transactions that are in the process of being added to the database.
                    DataRow[] purchases = transData.Select("ItemID = " + newRow.ItemID +
                        " AND StationID = " + newRow.StationID);
                    int buyQuantity = 0;
                    decimal buyPrice = 0;
                    if (purchases.Length > 0)
                    {
                        foreach (DataRow purchase in purchases)
                        {
                            EMMADataSet.TransactionsRow trans = purchase as EMMADataSet.TransactionsRow;
                            buyQuantity += trans.Quantity;
                            buyPrice += trans.Quantity * trans.Price;
                        }
                        buyPrice /= buyQuantity;
                        retVal = newRow.Price - buyPrice;
                    }
                    else
                    {
                        // If there are no assets at the station where the sell took place and no 
                        // buy transactions waiting to be added for that station either, then flag
                        // the transaction to have it's profit calculated when the next assets 
                        // update is performed.
                        newRow.CalcProfitFromAssets = true;
                    }
                }
            }
            catch (Exception ex)
            {
                new EMMADataException(ExceptionSeverity.Error, 
                    "Problem calculating profit for new sell transaction.", ex);
            }

            return retVal;
        }

        public static void BuildResults(List<FinanceAccessParams> accessParams, List<int> itemIDs,
            List<int> regionIDs, List<int> stationIDs, DateTime startDate, DateTime endDate, string transType)
        {
            // Make sure start/end dates are within the allowed ranges
            startDate = startDate.ToUniversalTime();
            endDate = endDate.ToUniversalTime();
            if (startDate.CompareTo(SqlDateTime.MinValue.Value) < 0) startDate = SqlDateTime.MinValue.Value;
            if (endDate.CompareTo(SqlDateTime.MinValue.Value) < 0) endDate = SqlDateTime.MinValue.Value;
            if (startDate.CompareTo(SqlDateTime.MaxValue.Value) > 0) startDate = SqlDateTime.MaxValue.Value;
            if (endDate.CompareTo(SqlDateTime.MaxValue.Value) > 0) endDate = SqlDateTime.MaxValue.Value;

            if (itemIDs.Count == 0) { itemIDs.Add(0); }
            if (regionIDs.Count == 0) { regionIDs.Add(0); }
            if (stationIDs.Count == 0) { stationIDs.Add(0); }

            string itemString = "";
            string stationString = "";
            string regionString = "";
            foreach (int item in itemIDs) { itemString = itemString + (itemString.Length == 0 ? "" : ",") + item; }
            foreach (int station in stationIDs) { stationString = stationString + (stationString.Length == 0 ? "" : ",") + station; }
            foreach (int region in regionIDs) { regionString = regionString + (regionString.Length == 0 ? "" : ",") + region; }

            //lock (tableAdapter)
            //{
                //tableAdapter.BuildResults(FinanceAccessParams.BuildAccessList(accessParams), itemString,
                //    stationString, regionString, startDate, endDate, transType);

                // Need a longer timeout than usual. This can take quite a while in extreme cases...
                // Go for 2 minutes.
                SqlConnection connection = new SqlConnection(
                    Properties.Settings.Default.EMMA_DatabaseConnectionString + ";Connection Timeout=180");

                try
                {
                    SqlCommand command = null;
                    connection.Open();

                    command = new SqlCommand("TransBuildResults", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.CommandTimeout = 180;

                    SqlParameter param = new SqlParameter("@accessParams", SqlDbType.VarChar, 2147483647,
                        ParameterDirection.Input, 0, 0, null, DataRowVersion.Current,
                        false, FinanceAccessParams.BuildAccessList(accessParams), "", "", "");
                    command.Parameters.Add(param);

                    SqlParameter param2 = new SqlParameter("@itemIDs", SqlDbType.VarChar, 2147483647,
                        ParameterDirection.Input, 0, 0, null, DataRowVersion.Current,
                        false, itemString, "", "", "");
                    command.Parameters.Add(param2);

                    SqlParameter param3 = new SqlParameter("@stationIDs", SqlDbType.VarChar, 2147483647,
                        ParameterDirection.Input, 0, 0, null, DataRowVersion.Current,
                        false, stationString, "", "", "");
                    command.Parameters.Add(param3);

                    SqlParameter param4 = new SqlParameter("@regionIDs", SqlDbType.VarChar, 2147483647,
                        ParameterDirection.Input, 0, 0, null, DataRowVersion.Current,
                        false, regionString, "", "", "");
                    command.Parameters.Add(param4);

                    SqlParameter param5 = new SqlParameter("@startDate", SqlDbType.DateTime, 8,
                        ParameterDirection.Input, 23, 3, null, DataRowVersion.Current,
                        false, startDate, "", "", "");
                    command.Parameters.Add(param5);

                    SqlParameter param6 = new SqlParameter("@endDate", SqlDbType.DateTime, 8,
                        ParameterDirection.Input, 23, 3, null, DataRowVersion.Current,
                        false, endDate, "", "", "");
                    command.Parameters.Add(param6);

                    SqlParameter param7 = new SqlParameter("@transType", SqlDbType.VarChar, 4,
                        ParameterDirection.Input, 0, 0, null, DataRowVersion.Current,
                        false, transType, "", "", "");
                    command.Parameters.Add(param7);

                    command.ExecuteNonQuery();
                }
                finally
                {
                    if (connection != null) { connection.Close(); }
                }
            //}
        }

        public static bool GetResultsPage(int startPos, int pageSize, ref TransactionList transactions)
        {
            if (startPos <= 0) startPos = 1;
            EMMADataSet.TransactionsDataTable table = new EMMADataSet.TransactionsDataTable(); 
            lock (tableAdapter)
            {
                tableAdapter.FillByResultsPage(table, startPos, pageSize);
            }
            foreach (EMMADataSet.TransactionsRow transaction in table)
            {
                transactions.Add(new Transaction(transaction));
            }

            return table.Count == pageSize;
        }


        public static void AddTransByCalcProfitFromAssets(EMMADataSet.TransactionsDataTable trans,
            List<FinanceAccessParams> accessParams, int itemID, bool calcProfitFromAssets)
        {
            lock(tableAdapter)
            {
                bool oldClearBeforeFill = tableAdapter.ClearBeforeFill;
                tableAdapter.ClearBeforeFill = false;
                EMMADataSet.TransactionsDataTable tmpTable = new EMMADataSet.TransactionsDataTable();
                tableAdapter.FillByCalcProfitFromAssets(tmpTable, FinanceAccessParams.BuildAccessList(accessParams),
                    itemID, calcProfitFromAssets);
                foreach (EMMADataSet.TransactionsRow tmpTrans in tmpTable)
                {
                    EMMADataSet.TransactionsRow match = trans.FindByID(tmpTrans.ID);
                    if (match == null)
                    {
                        trans.ImportRow(tmpTrans);
                    }
                }
                tableAdapter.ClearBeforeFill = oldClearBeforeFill;
            }
        }

        /// <summary>
        /// Used for creating transactions that are not part of the data recieved from the API,
        /// e.g. transactions created from item exchange contracts.
        /// </summary>
        public static void NewTransaction(DateTime datetime, int quantity, int itemID, decimal price, 
            int buyerID, int sellerID, int buyerCharID, int sellerCharID, int stationID, int regionID,
            bool buyerForCorp, bool sellerForCorp, short buyerWalletID, short sellerWalletID, 
            decimal sellerUnitProfit, bool calcProfitFromAssets, ref long newID)
        {
            long? ID = 0;
            lock (tableAdapter)
            {
                tableAdapter.New(datetime, quantity,itemID, price, buyerID, sellerID, buyerCharID, sellerCharID,
                    stationID, regionID, buyerForCorp, sellerForCorp, buyerWalletID, sellerWalletID, 
                    sellerUnitProfit, calcProfitFromAssets, ref ID);
            }
            newID = ID.HasValue ? ID.Value : -1;
        }

        public static void GetItemTransData(List<FinanceAccessParams> accessParams, List<int> itemIDs,
             List<int> regionIDs, List<int> stationIDs, DateTime startDate, DateTime endDate,
            ref decimal avgSellPrice, ref decimal avgBuyPrice, ref long unitsBought, ref long unitsSold,
            ref decimal brokerBuyFees, ref decimal brokerSellFees, ref decimal transactionTax,
            ref decimal transportCosts, bool calcBrokerFees, bool calcTransTax, bool useReprocessData)
        {
            decimal blank1 = 0, blank2 = 0;
            GetItemTransData(accessParams, itemIDs, regionIDs, stationIDs, startDate, endDate, 0, 0,
                ref avgSellPrice, ref blank1, ref avgBuyPrice, ref blank2, ref unitsBought, ref unitsSold,
                ref brokerBuyFees, ref brokerSellFees, ref transactionTax, ref transportCosts,
                calcBrokerFees, calcTransTax, true, true, false, true, useReprocessData);
        }

        public static void GetItemTransData(List<FinanceAccessParams> accessParams, List<int> itemIDs,
            long quantity, long recentBuyUnitsToIgnore,
            ref decimal avgSellPrice, ref decimal avgBuyPrice, ref long unitsBought, ref long unitsSold,
            ref decimal brokerBuyFees, ref decimal brokerSellFees, ref decimal transactionTax,
            bool calcBrokerFees, bool calcTransTax, bool useReprocessData)
        {
            decimal blank1 = 0, blank2 = 0, blank3 = 0;
            DateTime startDate = SqlDateTime.MinValue.Value;
            DateTime endDate = SqlDateTime.MaxValue.Value;

            GetItemTransData(accessParams, itemIDs, new List<int>(), new List<int>(), startDate, endDate,
                quantity, recentBuyUnitsToIgnore, ref avgSellPrice, ref blank1, ref avgBuyPrice, ref blank2,
                ref unitsBought, ref unitsSold, ref brokerBuyFees, ref brokerSellFees, ref transactionTax,
                ref blank3, calcBrokerFees, calcTransTax, true, true, false, false, useReprocessData);
        }

        public static void GetAverageBuyPrice(List<FinanceAccessParams> accessParams, List<int> itemIDs,
            List<int> stationIDs, List<int> regionIDs, long quantity, 
            long recentBuyUnitsToIgnore, ref decimal avgBuyPrice, ref decimal brokerBuyFees,
            bool useReprocessData)
        {
            decimal blank1 = 0, blank5 = 0, blank6 = 0, blank7 = 0, blank8 = 0, blank9 = 0;
            long blank3 = 0, blank4 = 0;
            DateTime startDate = SqlDateTime.MinValue.Value;
            DateTime endDate = SqlDateTime.MaxValue.Value;

            GetItemTransData(accessParams, itemIDs, regionIDs, stationIDs, startDate, endDate,
                quantity, recentBuyUnitsToIgnore, ref blank1, ref blank7, ref avgBuyPrice, ref blank8,
                ref blank3, ref blank4, ref brokerBuyFees, ref blank5, ref blank6, ref blank9,
                true, false, true, false, false, false, useReprocessData);
        }

        public static void GetAverageBuyPrice(List<FinanceAccessParams> accessParams, int itemID,
            List<int> stationIDs, List<int> regionIDs, long quantity,
            long recentBuyUnitsToIgnore, ref decimal avgBuyPrice)
        {
            List<int> itemIDs = new List<int>();
            itemIDs.Add(itemID);
            decimal blank1 = 0, blank5 = 0, blank6 = 0, blank7 = 0, blank8 = 0, blank9 = 0, blank10 = 0;
            long blank3 = 0, blank4 = 0;
            DateTime startDate = SqlDateTime.MinValue.Value;
            DateTime endDate = SqlDateTime.MaxValue.Value;

            GetItemTransData(accessParams, itemIDs, regionIDs, stationIDs, startDate, endDate,
                quantity, recentBuyUnitsToIgnore, ref blank1, ref blank7, ref avgBuyPrice, ref blank8,
                ref blank3, ref blank4, ref blank10, ref blank5, ref blank6, ref blank9,
                false, false, true, false, false, false, false);
        }

        public static void GetMedianSellPrice(List<FinanceAccessParams> accessParams, List<int> itemIDs,
            List<int> regionIDs, DateTime startDate, DateTime endDate, ref decimal medianSellPrice)
        {
            decimal blank1 = 0, blank2 = 0, blank5 = 0, blank6 = 0, blank7 = 0, blank8 = 0, blank9 = 0;
            long blank3 = 0, blank4 = 0;

            GetItemTransData(accessParams, itemIDs, regionIDs, new List<int>(), startDate, endDate,
                0, 0, ref blank1, ref medianSellPrice, ref blank2, ref blank5, ref blank3, ref blank4,
                ref blank6, ref blank7, ref blank8, ref blank9,
                false, false, false, true, true, false, true);
        }

        public static void GetMedianBuyPrice(List<FinanceAccessParams> accessParams, List<int> itemIDs,
            List<int> regionIDs, DateTime startDate, DateTime endDate, ref decimal medianBuyPrice,
            bool useReprocessData)
        {
            decimal blank1 = 0, blank2 = 0, blank5 = 0, blank6 = 0, blank7 = 0, blank8 = 0, blank9 = 0;
            long blank3 = 0, blank4 = 0;

            GetItemTransData(accessParams, itemIDs, regionIDs, new List<int>(), startDate, endDate,
                0, 0, ref blank1, ref blank5, ref blank2, ref medianBuyPrice, ref blank3, ref blank4,
                ref blank6, ref blank7, ref blank8, ref blank9,
                false, false, true, false, true, false, useReprocessData);
        }

        private static void GetItemTransData(List<FinanceAccessParams> accessParams, List<int> itemIDs,
             List<int> regionIDs, List<int> stationIDs, DateTime startDate, DateTime endDate,
            long quantity, long recentBuyUnitsToIgnore,
            ref decimal avgSellPrice, ref decimal medianSellPrice, ref decimal avgBuyPrice,
            ref decimal medianBuyPrice, ref long unitsBought, ref long unitsSold,
            ref decimal brokerBuyFees, ref decimal brokerSellFees, ref decimal transactionTax,
            ref decimal transportCosts,
            bool calcBrokerFees, bool calcTransTax, bool getBuyData, bool getSellData, bool getMedians,
            bool calcTransportCosts, bool useReprocessData)
        {
            long totBuy = 0, totSell = 0;
            decimal totIskBuy = 0, totIskSell = 0;
            brokerBuyFees = 0;
            brokerSellFees = 0;
            transactionTax = 0;
            transportCosts = 0;
            // Used for working out transport costs for sell transactions.
            Dictionary<int, Dictionary<int, long>> quantities = new Dictionary<int, Dictionary<int, long>>();
            long quantityRemaining = quantity;
            bool ignoreQuantity = quantity == 0;
            Dictionary<int, int> brokerRelations = new Dictionary<int, int>();
            Dictionary<int, int> accounting = new Dictionary<int, int>();
            SortedList<decimal, long> priceFrequencies = new SortedList<decimal, long>();

            startDate = startDate.ToUniversalTime();
            endDate = endDate.ToUniversalTime();

            Diagnostics.ResetTimer("Transactions.GetBuyTrans");
            Diagnostics.ResetTimer("Transactions.ProcessBuyTrans");
            Diagnostics.ResetTimer("Transactions.CalcBuyBrokerFees");
            Diagnostics.ResetTimer("Transactions.GetBkrRelLvl");
            Diagnostics.ResetTimer("Transactions.CalcBuyBrokerFeesGetOrder");
            Diagnostics.ResetTimer("Transactions.GetStanding");
            Diagnostics.ResetTimer("Transactions.CalculateBuyBkr");
            Diagnostics.ResetTimer("Transactions.CalcBuyMedian");
            Diagnostics.ResetTimer("Transactions.GetSellTrans");
            Diagnostics.ResetTimer("Transactions.ProcessSellTrans");
            Diagnostics.ResetTimer("Transactions.CalcSellBrokerFees");
            Diagnostics.ResetTimer("Transactions.CalcSellTransTax");
            Diagnostics.ResetTimer("Transactions.CalcSellTransportCosts");
            Diagnostics.ResetTimer("Transactions.CalcSellMedian");

            EMMADataSet.TransactionsDataTable transactions = new EMMADataSet.TransactionsDataTable();
            if (getBuyData)
            {
                Diagnostics.StartTimer("Transactions.GetBuyTrans");
                // Retrieve buy transactions that match our criteria
                transactions = GetTransData(accessParams, itemIDs, regionIDs, stationIDs, startDate, endDate, "Buy");
                Diagnostics.StopTimer("Transactions.GetBuyTrans");
                ReprocessResultList reprocessResults = new ReprocessResultList();
                if (itemIDs.Count == 1 && useReprocessData)
                {
                    reprocessResults = ReprocessJobs.GetItemResults(itemIDs[0], UserAccount.CurrentGroup.ID);
                }

                priceFrequencies = new SortedList<decimal, long>();

                Diagnostics.StartTimer("Transactions.ProcessBuyTrans");
                int transIndex = -1;
                int reprocIndex = (reprocessResults.Count == 0 ? -2 : -1);
                bool useReproc = false;
                bool useTrans = true;
                for (int i = 0; i < transactions.Count + reprocessResults.Count; i++)
                {
                    decimal currentUnitPrice = 0.0m;
                    long currentQuantity = 0;

                    if (reprocIndex != -2)
                    {
                        DateTime nextTransDate = DateTime.MinValue;
                        DateTime nextReprocDate = DateTime.MinValue;
                        if (transactions.Count > transIndex + 1)
                        {
                            nextTransDate = transactions[transIndex + 1].DateTime;
                        }
                        if (reprocessResults.Count > reprocIndex + 1)
                        {
                            nextReprocDate = reprocessResults[reprocIndex + 1].JobDate;
                        }
                        if (nextTransDate.CompareTo(nextReprocDate) < 0)
                        {
                            useReproc = true;
                            useTrans = false;
                            reprocIndex++;
                        }
                        else
                        {
                            useReproc = false;
                            useTrans = true;
                            transIndex++;
                        }
                    }
                    else
                    {
                        transIndex++;
                    }

                    bool includeTrans = true;
                    if (useTrans)
                    {
                        EMMADataSet.TransactionsRow trans = transactions[transIndex];
                        currentQuantity = trans.Quantity;
                        currentUnitPrice = trans.Price;
                    }
                    else if (useReproc)
                    {
                        ReprocessResult result = reprocessResults[reprocIndex];
                        currentQuantity = result.Quantity;
                        currentUnitPrice = result.EffectiveBuyPrice / currentQuantity;
                    }

                    if (recentBuyUnitsToIgnore > 0)
                    {
                        // If we're ignoring the first x units then first reduce the quantity we have 
                        // to ignore by the quantity of the current transaction
                        int quantityToUse = (int)(recentBuyUnitsToIgnore < currentQuantity ?
                            recentBuyUnitsToIgnore : currentQuantity);
                        recentBuyUnitsToIgnore -= quantityToUse;
                        if (recentBuyUnitsToIgnore == 0)
                        {
                            // If the current transaction has a greater quantity than we are ignoring
                            // then reduce the quantity on the transaction by whatever we have left to ignore
                            currentQuantity -= quantityToUse;
                        }
                        else
                        {
                            // otherwise, just move to the next transaction.
                            includeTrans = false;
                        }
                    }

                    if (includeTrans)
                    {
                        int quantityToUse = 1;

                        if (quantityRemaining > 0 || ignoreQuantity)
                        {
                            quantityToUse = (int)(ignoreQuantity ? currentQuantity :
                                (quantityRemaining < currentQuantity ? quantityRemaining : currentQuantity));

                            // Increase total buy units and total isk on buy transactions by
                            // the appropriate amounts.
                            totBuy += quantityToUse;
                            decimal transTot = currentUnitPrice * quantityToUse;
                            totIskBuy += transTot;

                            if (getMedians)
                            {
                                if (priceFrequencies.ContainsKey(currentUnitPrice))
                                {
                                    priceFrequencies[currentUnitPrice] = priceFrequencies[currentUnitPrice] + quantityToUse;
                                }
                                else
                                {
                                    priceFrequencies.Add(currentUnitPrice, quantityToUse);
                                }
                            }

                            #region Calculate broker fees
                            if (calcBrokerFees && useTrans)
                            {
                                Diagnostics.StartTimer("Transactions.CalcBuyBrokerFees");
                                Order buyOrder = null, empty = null;

                                Diagnostics.StartTimer("Transactions.CalcBuyBrokerFeesGetOrder");
                                EMMADataSet.TransactionsRow trans = transactions[transIndex];
                                Orders.GetOrder(new Transaction(trans), out buyOrder, out empty);
                                Diagnostics.StopTimer("Transactions.CalcBuyBrokerFeesGetOrder");
                                if (buyOrder != null)
                                {
                                    Diagnostics.StartTimer("Transactions.GetBkrRelLvl");
                                    int id = trans.BuyerForCorp ? trans.BuyerCharacterID : trans.BuyerID;
                                    int bkrrellvl = 0;
                                    decimal corpStanding = 0;
                                    decimal factionStanding = 0;

                                    if (brokerRelations.ContainsKey(id))
                                    {
                                        bkrrellvl = brokerRelations[id];
                                    }
                                    else
                                    {
                                        bool corpID = false;
                                        bkrrellvl = UserAccount.CurrentGroup.GetCharacter(id, ref corpID).BrokerRelationsLvl;
                                        brokerRelations.Add(id, bkrrellvl);
                                    }
                                    Diagnostics.StopTimer("Transactions.GetBkrRelLvl");

                                    Diagnostics.StartTimer("Transactions.GetStanding");
                                    EveDataSet.staStationsRow station = Stations.GetStation(buyOrder.StationID);
                                    if (station != null && !station.IscorporationIDNull())
                                    {
                                        int stationCorp = station.corporationID;
                                        EveDataSet.crpNPCCorporationsRow npcCorp =
                                            NPCCorps.GetCorp(stationCorp);
                                        if (npcCorp != null)
                                        {
                                            factionStanding = Standings.GetStanding(trans.BuyerID,
                                                npcCorp.factionID);
                                        }
                                        corpStanding = Standings.GetStanding(trans.BuyerID, stationCorp);
                                    }
                                    Diagnostics.StopTimer("Transactions.GetStanding");

                                    Diagnostics.StartTimer("Transactions.CalculateBuyBkr");
                                    decimal fee = transTot * (decimal)(1 /
                                        Math.Exp((double)(0.1m * factionStanding + 0.04m * corpStanding)) *
                                        1 - (0.05 * bkrrellvl)) / 100.0m;
                                    Diagnostics.StopTimer("Transactions.CalculateBuyBkr");

                                    brokerBuyFees += fee;
                                }

                                Diagnostics.StopTimer("Transactions.CalcBuyBrokerFees");
                            }
                            #endregion
                        }
                        else
                        {
                            // if we're only retrieving data for x units and have already got that
                            // many then just jump out of the loop.
                            i = transactions.Count + reprocessResults.Count;
                        }

                        quantityRemaining -= quantityToUse;
                    }
                }
                Diagnostics.StopTimer("Transactions.ProcessBuyTrans");

                #region Calculate median buy price
                if (getMedians && priceFrequencies.Count > 0)
                {
                    Diagnostics.StartTimer("Transactions.CalcBuyMedian");

                    float tmp = (totBuy + 1) / 2;
                    long limit = (long)Math.Round(tmp, MidpointRounding.AwayFromZero);
                    if (limit != tmp) { limit -= 1; }
                    int index = 0;
                    long quantitySoFar = 0;

                    while (quantitySoFar < limit)
                    {
                        quantitySoFar += priceFrequencies[priceFrequencies.Keys[index]];
                        index++;
                    }

                    if (limit == 0)
                    {
                        medianSellPrice = priceFrequencies.Keys[0];
                    }
                    else if (quantitySoFar > limit || limit == tmp)
                    {
                        medianBuyPrice = priceFrequencies.Keys[index - 1];
                    }
                    else
                    {
                        medianBuyPrice = (priceFrequencies.Keys[index - 1] + priceFrequencies[index]) / 2;
                    }

                    /*decimal[] priceArray = prices.ToArray();
                    Array.Sort<decimal>(priceArray);

                    int rem = 0;
                    int result = Math.DivRem(priceArray.Length, 2, out rem);
                    decimal median = 0;
                    if (rem == 0)
                    {
                        median = (priceArray[result - 1] + priceArray[result]) / 2;
                    }
                    else
                    {
                        median = priceArray[result];
                    }
                    medianBuyPrice = median;*/
                    Diagnostics.StopTimer("Transactions.CalcBuyMedian");
                }
                #endregion
            }

            if (getSellData)
            {
                Diagnostics.StartTimer("Transactions.GetSellTrans");
                // Retrieve sell transactions that match our criteria.
                transactions = GetTransData(accessParams, itemIDs, regionIDs, stationIDs, startDate, endDate, "Sell");
                Diagnostics.StopTimer("Transactions.GetSellTrans");

                priceFrequencies = new SortedList<decimal, long>();

                quantityRemaining = quantity;
                Diagnostics.StartTimer("Transactions.ProcessSellTrans");
                for (int i = 0; i < transactions.Count; i++)
                {
                    EMMADataSet.TransactionsRow trans = transactions[i];
                    int quantityToUse = 1;

                    if (quantityRemaining > 0 || ignoreQuantity)
                    {
                        quantityToUse = (int)(ignoreQuantity ? trans.Quantity :
                            (quantityRemaining < trans.Quantity ? quantityRemaining : trans.Quantity));

                        // Increase total sell units and total isk on sell transactions by
                        // the appropriate amounts.
                        totSell += quantityToUse;
                        decimal transTot = trans.Price * quantityToUse;
                        totIskSell += transTot;

                        if (getMedians)
                        {
                            decimal price = trans.Price;
                            if (priceFrequencies.ContainsKey(price))
                            {
                                priceFrequencies[price] = priceFrequencies[price] + quantityToUse;
                            }
                            else
                            {
                                priceFrequencies.Add(price, quantityToUse);
                            }
                        }

                        #region Calculate broker fees
                        if (calcBrokerFees)
                        {
                            // Only add the broker fee if we've got a matching journal record.
                            // If we don't it was probably a quick buy (i.e. no broker fee)
                            Order sellOrder = null, blank = null;
                            Orders.GetOrder(new Transaction(trans), out blank, out sellOrder);
                            if (sellOrder != null)
                            {
                                Diagnostics.StartTimer("Transactions.CalcSellBrokerFees");
                                int id = trans.SellerForCorp ? trans.SellerCharacterID : trans.SellerID;
                                int bkrrellvl = 0;
                                decimal factionStanding = 0, corpStanding = 0;

                                if (brokerRelations.ContainsKey(id))
                                {
                                    bkrrellvl = brokerRelations[id];
                                }
                                else
                                {
                                    bool corpID = false;
                                    bkrrellvl = UserAccount.CurrentGroup.GetCharacter(id, ref corpID).BrokerRelationsLvl;
                                    brokerRelations.Add(id, bkrrellvl);
                                }

                                EveDataSet.staStationsRow station = Stations.GetStation(trans.StationID);
                                if (station != null && !station.IscorporationIDNull())
                                {
                                    int stationCorp = station.corporationID;
                                    EveDataSet.crpNPCCorporationsRow npcCorp =
                                        NPCCorps.GetCorp(stationCorp);
                                    if (npcCorp != null)
                                    {
                                        factionStanding = Standings.GetStanding(trans.BuyerID,
                                            npcCorp.factionID);
                                    }
                                    corpStanding = Standings.GetStanding(trans.SellerID, stationCorp);
                                }

                                decimal fee = transTot * (decimal)(1 /
                                    Math.Exp((double)(0.1m * factionStanding + 0.04m * corpStanding)) *
                                    1 - (0.05 * bkrrellvl)) / 100.0m;

                                brokerSellFees += fee;
                            }
                            Diagnostics.StopTimer("Transactions.CalcSellBrokerFees");
                        }
                        #endregion
                        #region Calculate transaction tax
                        if (calcTransTax)
                        {
                            Diagnostics.StartTimer("Transactions.CalcSellTransTax");
                            int id = trans.SellerForCorp ? trans.SellerCharacterID : trans.SellerID;
                            int acclvl = 0;
                            if (accounting.ContainsKey(id))
                            {
                                acclvl = accounting[id];
                            }
                            else
                            {
                                bool corpID = false;
                                acclvl = UserAccount.CurrentGroup.GetCharacter(id, ref corpID).AccountingLvl;
                                accounting.Add(id, acclvl);
                            }
                            transactionTax += transTot * (decimal)(0.01 - 0.001 * acclvl);
                            Diagnostics.StopTimer("Transactions.CalcSellTransTax");
                        }
                        #endregion
                        #region Record quantities for working out transport costs.
                        Dictionary<int, long> itemQuantities;
                        if (quantities.ContainsKey(trans.StationID))
                        {
                            itemQuantities = quantities[trans.StationID];
                        }
                        else
                        {
                            itemQuantities = new Dictionary<int, long>();
                            quantities.Add(trans.StationID, itemQuantities);
                        }

                        if (itemQuantities.ContainsKey(trans.ItemID))
                        {
                            long qSoFar = itemQuantities[trans.ItemID];
                            qSoFar += trans.Quantity;
                            itemQuantities[trans.ItemID] = qSoFar;
                        }
                        else
                        {
                            itemQuantities.Add(trans.ItemID, trans.Quantity);
                        }
                        #endregion
                    }
                    else
                    {
                        // if we're only retrieving data for x units and have already got that
                        // many then just jump out of the loop.
                        i = transactions.Count;
                    }

                    quantityRemaining -= quantityToUse;
                }
                Diagnostics.StopTimer("Transactions.ProcessSellTrans");

            }

            #region Calculate transport costs
            if (calcTransportCosts)
            {
                Diagnostics.StartTimer("Transactions.CalcSellTransportCosts");
                Dictionary<int, Dictionary<int, long>>.Enumerator enumerator = quantities.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Dictionary<int, long>.Enumerator enumerator2 = enumerator.Current.Value.GetEnumerator();
                    while (enumerator2.MoveNext())
                    {
                        // Get cost of contracts using this item occuring up to 14 days before
                        // the beginning of the report.
                        transportCosts += Contracts.GetTransportCosts(enumerator2.Current.Key,
                            enumerator.Current.Key, enumerator2.Current.Value, startDate.AddDays(-14));
                    }
                }
                Diagnostics.StopTimer("Transactions.CalcSellTransportCosts");
            }
            #endregion
            #region Calculate median sell price
            if (getMedians && priceFrequencies.Count > 0)
            {
                Diagnostics.StartTimer("Transactions.CalcSellMedian");

                float tmp = (totSell + 1) / 2;
                long limit = (long)Math.Round(tmp, MidpointRounding.AwayFromZero);
                if (limit != tmp) { limit -= 1; }
                int index = 0;
                long quantitySoFar = 0;

                while (quantitySoFar < limit)
                {
                    quantitySoFar += priceFrequencies[priceFrequencies.Keys[index]];
                    index++;
                }

                if (limit == 0)
                {
                    medianSellPrice = priceFrequencies.Keys[0];
                }
                else if (quantitySoFar > limit || limit == tmp)
                {
                    medianSellPrice = priceFrequencies.Keys[index - 1];
                }
                else
                {
                    medianSellPrice = (priceFrequencies.Keys[index - 1] + priceFrequencies[index]) / 2;
                }
                
                /*decimal[] priceArray = prices.ToArray();
                Array.Sort<decimal>(priceArray);

                int rem = 0;
                int result = Math.DivRem(priceArray.Length, 2, out rem);
                decimal median = 0;
                if (rem == 0)
                {
                    median = (priceArray[result - 1] + priceArray[result]) / 2;
                }
                else
                {
                    median = priceArray[result];
                }
                medianSellPrice = median;*/
                Diagnostics.StopTimer("Transactions.CalcSellMedian");
            }
            #endregion


            // Set values for return parameters. (note, broker fees and transaction tax are already set for us.)
            avgSellPrice = (totSell == 0 ? 0 : totIskSell / totSell);
            avgBuyPrice = (totBuy == 0 ? 0 : totIskBuy / totBuy);
            unitsBought = totBuy;
            unitsSold = totSell;
        }

        /// <summary>
        /// Get a datatable containing the IDs of items that are involved in transactions that meet
        /// the specifeid criteria
        /// </summary>
        /// <param name="accessParams"></param>
        /// <returns></returns>
        public static EMMADataSet.IDTableDataTable GetInvolvedItemIDs(List<FinanceAccessParams> accessParams, 
            int minVolume)
        {
            return GetInvolvedItemIDs(accessParams, minVolume, SqlDateTime.MinValue.Value,
                SqlDateTime.MaxValue.Value);
        }

        /// <summary>
        /// Get a datatable containing the IDs of items that are involved in transactions that meet
        /// the specifeid criteria
        /// </summary>
        /// <param name="accessParams"></param>
        /// <returns></returns>
        public static EMMADataSet.IDTableDataTable GetInvolvedItemIDs(List<FinanceAccessParams> accessParams,
            int minVolume, DateTime minDate, DateTime maxDate)
        {
            EMMADataSet.IDTableDataTable table = new EMMADataSet.IDTableDataTable();
            lock (tableAdapter)
            {
                IDTableAdapter.FillItemIDsByTrans(table, FinanceAccessParams.BuildAccessList(accessParams), 
                    minVolume, minDate, maxDate);
            }
            return table;
        }

        /// <summary>
        /// Get a datatable containing the IDs of items that are involved in transactions that meet
        /// the specifeid criteria.
        /// </summary>
        /// <param name="accessParams"></param>
        /// <returns></returns>
        public static EMMADataSet.IDTableDataTable GetInvolvedItemIDs(List<FinanceAccessParams> accessParams,
            int minVolume, DateTime minDate, DateTime maxDate, int minBuy, int minSell, List<int> buyStations,
            List<int> sellStations)
        {
            EMMADataSet.IDTableDataTable retVal = new EMMADataSet.IDTableDataTable();
            EMMADataSet.TransactionsDataTable table = GetTransData(accessParams, null, null, null,
                minDate, maxDate, "");
            Dictionary<int, long> qTotalBuy = new Dictionary<int, long>();
            Dictionary<int, long> qTotalSell = new Dictionary<int, long>();
            List<int> allItems = new List<int>();

            foreach (EMMADataSet.TransactionsRow trans in table)
            {
                int itemID = trans.ItemID;
                if (!allItems.Contains(itemID)) { allItems.Add(itemID); }
                int stationID = trans.StationID;
                bool buyTrans = false;
                bool sellTrans = false;
                // Determine if this transaction is buy, sell or both for the characters passed in
                foreach (FinanceAccessParams accessDetails in accessParams)
                {
                    if (accessDetails.OwnerID == trans.BuyerID || accessDetails.OwnerID == trans.BuyerCharacterID)
                    {
                        buyTrans = true;
                    }
                    if (accessDetails.OwnerID == trans.SellerID || accessDetails.OwnerID == trans.SellerCharacterID)
                    {
                        sellTrans = true;
                    }
                }

                if (buyTrans)
                {
                    // Keep a count of the total amount of each item that has been bought in the 
                    // specified buy stations
                    if (buyStations.Count == 0 || buyStations.Contains(trans.StationID))
                    {
                        if (qTotalBuy.ContainsKey(itemID))
                        {
                            qTotalBuy[itemID] = qTotalBuy[itemID] + trans.Quantity;
                        }
                        else
                        {
                            qTotalBuy.Add(itemID, trans.Quantity);
                        }
                    }
                }
                if (sellTrans)
                {
                    // Keep a count of the total amount of each item that has been sold in the 
                    // specified sell stations
                    if (sellStations.Count == 0 || sellStations.Contains(stationID))
                    {
                        if (qTotalSell.ContainsKey(itemID))
                        {
                            qTotalSell[itemID] = qTotalSell[itemID] + trans.Quantity;
                        }
                        else
                        {
                            qTotalSell.Add(itemID, trans.Quantity);
                        }
                    }
                }
            }

            foreach (int itemID in allItems)
            {
                long qBuy = 0;
                long qSell = 0;
                if (qTotalBuy.ContainsKey(itemID)) { qBuy = qTotalBuy[itemID]; }
                if (qTotalSell.ContainsKey(itemID)) { qSell = qTotalSell[itemID]; }
                if (qBuy + qSell >= minVolume && qBuy >= minBuy && qSell >= minSell)
                {
                    EMMADataSet.IDTableRow newID = retVal.NewIDTableRow();
                    newID.ID = itemID;
                    retVal.AddIDTableRow(newID);
                }                
            }

            return retVal;
        }


        /// <summary>
        /// Get a datatable containing the IDs of stations that are involved in transactions that meet
        /// the specifeid criteria
        /// </summary>
        /// <param name="accessParams"></param>
        /// <returns></returns>
        public static EMMADataSet.IDTableDataTable GetInvolvedStationIDs(List<FinanceAccessParams> accessParams)
        {
            EMMADataSet.IDTableDataTable table = new EMMADataSet.IDTableDataTable();
            lock (tableAdapter)
            {
                IDTableAdapter.FillStationIDsByTrans(table, FinanceAccessParams.BuildAccessList(accessParams));
            }
            return table;
        }


        /// <summary>
        /// Get a list containing all transactions that meet the specified criteria.
        /// </summary>
        /// <param name="accessParams"></param>
        /// <param name="itemIDs"></param>
        /// <param name="stationIDs"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static TransactionList LoadTransactions(List<FinanceAccessParams> accessParams, List<int> itemIDs,
            List<int> stationIDs, DateTime startDate, DateTime endDate, string type)
        {
            TransactionList retVal = new TransactionList();

            //---------------------------------------------------------------------------------------------------

            EMMADataSet.TransactionsDataTable table = new EMMADataSet.TransactionsDataTable();
            table = GetTransData(accessParams, itemIDs, new List<int>(), stationIDs, startDate, endDate, type);

            foreach (EMMADataSet.TransactionsRow row in table)
            {
                Transaction trans = new Transaction(row);
                retVal.Add(trans);
            }

            //---------------------------------------------------------------------------------------------------
            // This was an attempt to speed up loading of data by only loading IDs initally and
            // then retrieving other information as required.
            // It was not much faster on the inital load and gave worse stuttering during operation
            // so it's no longer used. Instead we simply limit the inital data retrieval on
            // the view form to the last week's worth of data.
            //---------------------------------------------------------------------------------------------------

            /*string itemString = "";
            string stationString = "";
            foreach (int item in itemIDs) { itemString = itemString + (itemString.Length == 0 ? "" : ",") + item; }
            foreach (int station in stationIDs) { stationString = stationString + (stationString.Length == 0 ? "" : ",") + station; }

            SqlConnection connection = new SqlConnection(Properties.Settings.Default.EMMA_DatabaseConnectionString);
            SqlDataAdapter adapter = null;
            SqlCommand command = null;
            connection.Open();

            command = new SqlCommand("TransCountByItemAndLoc", connection);
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@accessParams", FinanceAccessParams.BuildAccessList(accessParams)));
            command.Parameters.Add(new SqlParameter("@itemIDs", itemString));
            command.Parameters.Add(new SqlParameter("@stationIDs", stationString));
            command.Parameters.Add(new SqlParameter("@regionIDs", ""));
            command.Parameters.Add(new SqlParameter("@startDate", SqlDateTime.MinValue.Value));
            command.Parameters.Add(new SqlParameter("@endDate", SqlDateTime.MaxValue.Value));
            command.Parameters.Add(new SqlParameter("@transType", ""));
            adapter = new SqlDataAdapter(command);

            // lock on the transactions table adapter, even though we're not actually using it, we're still
            // accessing the same database table.
            lock (tableAdapter)
            {
                SqlDataReader reader = adapter.SelectCommand.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        Transaction trans = new Transaction(reader.GetInt64(0));
                        retVal.Add(trans);
                    }
                }
                finally
                {
                    reader.Close();
                }
            }*/

            //---------------------------------------------------------------------------------------------------


            return retVal;
        }

        /// <summary>
        /// Get the specified transaction
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static EMMADataSet.TransactionsRow GetTransaction(long id)
        {
            EMMADataSet.TransactionsRow retVal = null;
            EMMADataSet.TransactionsDataTable table = new EMMADataSet.TransactionsDataTable();
            lock (tableAdapter)
            {
                tableAdapter.FillByID(table, id);
            }
            if (table.Count > 0) retVal = table[0];
            return retVal;
        }

        public static void DeleteTransaction(long id)
        {
            EMMADataSet.TransactionsDataTable table = new EMMADataSet.TransactionsDataTable();
            lock (tableAdapter)
            {
                tableAdapter.FillByID(table, id);
                if (table.Count > 0) { table[0].Delete(); }
                tableAdapter.Update(table);
            }            
        }

        /// <summary>
        /// Check if the specified transaction exists, if it does that it is added to the supplied
        /// datatable object.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool TransactionExists(EMMADataSet.TransactionsDataTable table, long id) 
        {
            bool? exists = false;
            lock (tableAdapter)
            {
                tableAdapter.ClearBeforeFill = false;
                tableAdapter.FillTransExists(table, id, ref exists);
            }
            return (exists.HasValue ? exists.Value : false);
        }


        /// <summary>
        /// Commit changes made in the specified datatable to the database.
        /// </summary>
        /// <param name="table"></param>
        public static void Store(EMMADataSet.TransactionsDataTable table)
        {
            lock (tableAdapter)
            {
                tableAdapter.Update(table);
            }
        }


        public static EMMADataSet.TransactionsDataTable GetTransData(int ownerID, bool includeCorporate,
            int itemID, int regionID, int stationID, long minTransID, string type)
        {
            EMMADataSet.TransactionsDataTable retVal = new EMMADataSet.TransactionsDataTable();

            lock (tableAdapter)
            {
                tableAdapter.FillByAnySingleAndID(retVal, ownerID, includeCorporate, itemID, stationID,
                    regionID, minTransID, type);
            }
            return retVal;
        }

        /// <summary>
        /// Retrieves transaction data from the database.
        /// The parameters supplied are used to determine the correct stored procedure to use
        /// to give us the best performance possible.
        /// </summary>
        /// <param name="accessList"></param>
        /// <param name="itemIDs"></param>
        /// <param name="regionIDs"></param>
        /// <param name="stationIDs"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="type">If value is 'buy' or 'sell' then only the relevant transactions are returned. Any other values returns both buy and sell transactions</param>
        /// <returns></returns>
        public static EMMADataSet.TransactionsDataTable GetTransData(List<FinanceAccessParams> accessList,
            List<int> itemIDs, List<int> regionIDs, List<int> stationIDs, 
            DateTime startDate, DateTime endDate, string type)
        {
            EMMADataSet.TransactionsDataTable retVal = new EMMADataSet.TransactionsDataTable();
            int charID = 0;
            List<short> walletIDs = new List<short>();

            // Make sure start/end dates are within the allowed ranges
            startDate = startDate.ToUniversalTime();
            endDate = endDate.ToUniversalTime();
            if (startDate.CompareTo(SqlDateTime.MinValue.Value) < 0) startDate = SqlDateTime.MinValue.Value;
            if (endDate.CompareTo(SqlDateTime.MinValue.Value) < 0) endDate = SqlDateTime.MinValue.Value;
            if (startDate.CompareTo(SqlDateTime.MaxValue.Value) > 0) startDate = SqlDateTime.MaxValue.Value;
            if (endDate.CompareTo(SqlDateTime.MaxValue.Value) > 0) endDate = SqlDateTime.MaxValue.Value;

            if (itemIDs == null) { itemIDs = new List<int>(); }
            if (regionIDs == null) { regionIDs = new List<int>(); }
            if (stationIDs == null) { stationIDs = new List<int>(); }

            if (itemIDs.Count == 0) { itemIDs.Add(0); }
            if (regionIDs.Count == 0) { regionIDs.Add(0); }
            if (stationIDs.Count == 0) { stationIDs.Add(0); }

            if (accessList.Count == 1)
            {
                charID = accessList[0].OwnerID;
                bool includeCorporate = accessList[0].IncludeCorporate;
                foreach (short wallet in accessList[0].WalletIDs)
                {
                    if (walletIDs.Count == 0 && wallet != 0) { walletIDs = accessList[0].WalletIDs; }
                }

                if (walletIDs.Count == 0 && itemIDs.Count == 1 && regionIDs.Count == 1 && stationIDs.Count == 1)
                {
                    lock (tableAdapter)
                    {
                        // Retrieve transactions for a single character (or corp) and only one
                        // of each item, station and region paramter. (note these could be the '0' paramter
                        // that will include anything.)
                        // e.g. getting all transactions for a single character involving tritanium
                        // at any location.
                        tableAdapter.FillByAnySingle(retVal, charID, includeCorporate, itemIDs[0], stationIDs[0], 
                            regionIDs[0], startDate, endDate, type);
                    }
                }
                else if (itemIDs.Count == 1 && regionIDs.Count == 1 && stationIDs.Count == 1)
                {
                    short w1 = accessList[0].WalletIDs[0], w2 = accessList[0].WalletIDs[1],
                        w3 = accessList[0].WalletIDs[2], w4 = accessList[0].WalletIDs[3],
                        w5 = accessList[0].WalletIDs[4], w6 = accessList[0].WalletIDs[5];
                    lock (tableAdapter)
                    {
                        // Retrieve transactions for a single corporation, specific wallets and only one
                        // of each item, station and region paramter
                        // e.g. getting all transactions in wallet 3 and 4 of specified corp involving
                        // tritanium at any location.
                        tableAdapter.FillBySingleAndWallets(retVal, charID, w1, w2, w3, w4, w5, w6, itemIDs[0],
                            stationIDs[0], regionIDs[0], startDate, endDate, type);
                    }
                }
            }
            else
            {
                if (itemIDs.Count == 1 && regionIDs.Count == 1 && stationIDs.Count == 1)
                {
                    lock (tableAdapter)
                    {
                        // Retrieve transactions for multiple characters and/or corporations where only one
                        // of each item, station and region paramter.
                        tableAdapter.FillByOwnersAndSingle(retVal, FinanceAccessParams.BuildAccessList(accessList),
                            itemIDs[0], stationIDs[0], regionIDs[0], startDate, endDate, type);
                    }
                }
                else
                {
                    string itemString = "";
                    string stationString = "";
                    string regionString = "";
                    foreach (int item in itemIDs) { itemString = itemString + (itemString.Length == 0 ? "" : ",") + item; }
                    foreach (int station in stationIDs) { stationString = stationString + (stationString.Length == 0 ? "" : ",") + station; }
                    foreach (int region in regionIDs) { regionString = regionString + (regionString.Length == 0 ? "" : ",") + region; }

                    lock (tableAdapter)
                    {
                        // Retrieve transactions for multiple characters and/or corporations where multiple
                        // items, stations or regions are specified.
                        tableAdapter.FillByAny(retVal, FinanceAccessParams.BuildAccessList(accessList), itemString,
                             stationString, regionString, startDate, endDate, type);
                    }
                }
            }

            return retVal;
        }

        #region Instance methods
        public void LoadOldEmmaXML(string filename, int charID, int corpID)
        {
            EMMADataSet.TransactionsDataTable table = new EMMADataSet.TransactionsDataTable();
            XmlDocument xml = new XmlDocument();
            //UpdateStatus(0, 0, "", "Loading file", false);
            xml.Load(filename);

            XmlNodeList nodes = xml.SelectNodes("/DocumentElement/Transactions");

            int counter = 0;
            UpdateStatus(0, 0, "", "Extracting data from XML", false);
            foreach (XmlNode node in nodes)
            {
                long transID = long.Parse(node.SelectSingleNode("ID").FirstChild.Value);

                if (!Transactions.TransactionExists(table, transID) &&
                    table.FindByID(transID) == null)
                {
                    // Actually create the line and add it to the data table
                    EMMADataSet.TransactionsRow newRow = BuildTransRow(transID, table, node, corpID, charID);

                    table.AddTransactionsRow(newRow);
                }
                else
                {
                    // We've got a transaction that already exists in the database,
                    // update the row with additional data if available. 
                    EMMADataSet.TransactionsRow newRow = BuildTransRow(transID, table, node, corpID, charID);
                    EMMADataSet.TransactionsRow oldRow = table.FindByID(transID);

                    //if (newRow.BuyerWalletID != oldRow.BuyerWalletID && newRow.BuyerWalletID != 0)
                    //{
                    //    oldRow.BuyerWalletID = newRow.BuyerWalletID;
                    //}
                    //if (newRow.SellerWalletID != oldRow.SellerWalletID && newRow.SellerWalletID != 0)
                    //{
                    //    oldRow.SellerWalletID = newRow.SellerWalletID;
                    //}
                    // If a corp sells somthing to another corp (or itself) then we will get into 
                    // the position of having the other party set as a character when in fact
                    // it is that character's corp.
                    // We check for this here and correct it if required.

                    // Change to just always update the database with the data from the import.
                    //if (oldRow.BuyerID == charID && newRow.BuyerID == corpID)
                    //{
                        oldRow.BuyerID = newRow.BuyerID;
                        oldRow.BuyerCharacterID = newRow.BuyerCharacterID;
                        oldRow.BuyerWalletID = newRow.BuyerWalletID;
                        oldRow.BuyerForCorp = newRow.BuyerForCorp;
                    //}
                    //if (oldRow.SellerID == charID && newRow.SellerID == corpID)
                    //{
                        oldRow.SellerID = newRow.SellerID;
                        oldRow.SellerCharacterID = newRow.SellerCharacterID;
                        oldRow.SellerWalletID = newRow.SellerWalletID;
                        oldRow.SellerForCorp = newRow.SellerForCorp;
                    //}

                        oldRow.DateTime = newRow.DateTime;
                        oldRow.Price = newRow.Price;
                        oldRow.Quantity = newRow.Quantity;
                        oldRow.ItemID = newRow.ItemID;
                        oldRow.StationID = newRow.StationID;
                }
                counter++;
                UpdateStatus(counter, nodes.Count, "", "", false);

                // If we've got 1000 rows then update the database and move on to the next batch.
                if (table.Count >= 1000)
                {
                    UpdateStatus(0, 0, "", "Updating database", false);
                    lock (tableAdapter)
                    {
                        tableAdapter.Update(table);
                        table.Clear();
                    }
                }
            }

            UpdateStatus(0, 0, "", "Updating database", false);
            lock (tableAdapter)
            {
                tableAdapter.Update(table);
            }
        }

        private EMMADataSet.TransactionsRow BuildTransRow(long transID, 
            EMMADataSet.TransactionsDataTable table, XmlNode node, int corpID, int charID)
        {
            EMMADataSet.TransactionsRow newRow = table.NewTransactionsRow();

            newRow.ID = transID;
            // Set the simple data. i.e. direct conversion from XML field to database field.
            newRow.DateTime = DateTime.Parse(node.SelectSingleNode("DateTime").FirstChild.Value);
            newRow.Quantity = int.Parse(node.SelectSingleNode("Quantity").FirstChild.Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            newRow.ItemID = int.Parse(node.SelectSingleNode("ItemID").FirstChild.Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            newRow.Price = Decimal.Parse(node.SelectSingleNode("Price").FirstChild.Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            newRow.StationID = int.Parse(node.SelectSingleNode("StationID").FirstChild.Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            newRow.RegionID = Stations.GetStation(newRow.StationID).regionID;

            // Get the data to work out the more complicated fields..
            string transType = node.SelectSingleNode("TransactionType").FirstChild.Value;
            int clientID = int.Parse(node.SelectSingleNode("ClientID").FirstChild.Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            XmlNode charNode = node.SelectSingleNode("CharacterID").FirstChild;
            int charID2 = 0;
            if (charNode != null)
            {
                charID2 = int.Parse(charNode.Value,
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            bool forCorp = bool.Parse(node.SelectSingleNode("ForCorp").FirstChild.Value);

            if (transType.Trim().ToLower().Equals("buy"))
            {
                newRow.BuyerID = forCorp ? corpID : charID;
                newRow.BuyerForCorp = forCorp;
                newRow.BuyerCharacterID = forCorp ? charID : 0;
                newRow.BuyerWalletID = short.Parse(node.SelectSingleNode("WalletID").FirstChild.Value);
                newRow.SellerID = clientID;
                newRow.SellerForCorp = charID2 != 0;
                newRow.SellerCharacterID = charID2;
                newRow.SellerWalletID = 0;
            }
            else
            {
                newRow.BuyerID = clientID;
                newRow.BuyerForCorp = charID2 != 0;
                newRow.BuyerCharacterID = charID2;
                newRow.BuyerWalletID = 0;
                newRow.SellerID = forCorp ? corpID : charID;
                newRow.SellerForCorp = forCorp;
                newRow.SellerCharacterID = forCorp ? charID : 0;
                newRow.SellerWalletID = short.Parse(node.SelectSingleNode("WalletID").FirstChild.Value);
            }

            return newRow;
        }

        public void UpdateStatus(int progress, int maxProgress, string section, string sectionStatus, bool done)
        {
            if (StatusChange != null)
            {
                StatusChange(null, new StatusChangeArgs(progress, maxProgress, section, sectionStatus, done));
            }
        }

        [System.Diagnostics.Conditional("DIAGNOSTICS")]
        public void DiagnosticUpdate(string section, string sectionStatus)
        {
            if (StatusChange != null)
            {
                StatusChange(null, new StatusChangeArgs(-1, -1, section, sectionStatus, false));
            }
        }
        #endregion
    }
}
