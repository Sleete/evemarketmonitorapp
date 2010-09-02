using System;
using System.Collections.Generic;
using System.Text;

using EveMarketMonitorApp.AbstractionClasses;
using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public static class NAVHistory
    {
        private static EMMADataSetTableAdapters.AssetsHistoryTableAdapter tableAdapter = 
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.AssetsHistoryTableAdapter();

        private static decimal _lastCashInEscrow;
        private static DateTime _lastEscrowDate;
        private static Dictionary<int, int> _escrowOrderQuantity;


        public static decimal GetWalletCash(int ownerID, short walletID, DateTime date)
        {
            decimal retVal = 0;

            date = date.ToUniversalTime();
            bool corp = false;
            APICharacter character = UserAccount.CurrentGroup.GetCharacter(ownerID, ref corp);

            if (date.CompareTo(DateTime.UtcNow.AddHours(-2)) >= 0)
            {
                if (character != null)
                {
                    List<int> wallets = new List<int>();
                    if (corp) { wallets.Add(walletID); }
                    else { wallets.Add(1000); }
                    retVal = character.GetWalletBalance(corp, wallets);
                }
            }
            else
            {
                EMMADataSet.JournalRow data = Journal.GetClosest(ownerID, corp, walletID, date);
                if (data != null)
                {
                    if (ownerID == data.RecieverID || ownerID == data.RCorpID)
                    {
                        retVal = data.RBalance;
                    }
                    else
                    {
                        retVal = data.SBalance;
                    }
                }
            }

            return retVal;
        }


        public static decimal GetEscrowCash(int ownerID, short walletID)
        {
            decimal retVal = 0;

            // Just get the cash currently in escrow.
            retVal = Orders.GetCashInEscrow(ownerID, walletID);

            _escrowOrderQuantity = new Dictionary<int, int>();
            _lastCashInEscrow = retVal;
            _lastEscrowDate = DateTime.UtcNow;

            return retVal;
        }



        public static decimal GetNextEscrowCash(int ownerID, short walletID, DateTime date)
        {
            decimal retVal = _lastCashInEscrow;
            APICharacter character = UserAccount.CurrentGroup.GetCharacter(ownerID);
            if (character != null)
            {
                date = date.ToUniversalTime();

                // Setup required variables
                DateTime startDate, endDate;
                int marginTradingLevel = 0;
                //double ordersMult = 1;
                List<FinanceAccessParams> accessParams = new List<FinanceAccessParams>();
                List<short> walletIDs = new List<short>();
                walletIDs.Add(walletID);
                accessParams.Add(new FinanceAccessParams(ownerID, walletIDs));
                //if (_escrowOrderQuantity == null)
                //{
                //    _escrowOrderQuantity = new Dictionary<int, int>();
                //}

                startDate = date;
                endDate = _lastEscrowDate;
                //ordersMult = -1;

                // *****************
                // This old method relied on trying to work out the historic cash in escrow based on
                // orders and transactions. This is far from reliable.
                // Instead, simply use journal data to get the exact values in and out of escrow.
                // We also need to include buy transactions since by buying something, 
                // cash goes out of escrow without the journal knowing about it. 
                // *****************

                // Get buy transactions that occured between the last date and the desired date.
                EMMADataSet.TransactionsDataTable transactions = Transactions.GetTransData(accessParams,
                    new List<int>(), new List<int>(), new List<int>(), startDate, endDate, "Buy");

                //// Get orders that were created between the last date and the desired date.
                //EMMADataSet.OrdersDataTable orders = Orders.GetOrdersByIssueDate(ownerID, walletID,
                //    startDate, endDate);

                marginTradingLevel = character.MarginTradingLvl;

                //ordersMult *= Math.Pow(0.75, (double)marginTradingLevel);
                //foreach (EMMADataSet.OrdersRow order in orders)
                //{
                //    // Any buy orders created within this time period will have increased the amount of 
                //    // cash in escrow. So take that amount away from the total.
                //    if (order.BuyOrder)
                //    {
                //        retVal += (order.Price * order.TotalVol) * (decimal)ordersMult;
                //    }
                //}

                //foreach (EMMADataSet.TransactionsRow trans in transactions)
                //{
                //    Order orderData = null, blank = null;
                //    if (Orders.GetOrder(new Transaction(trans), out orderData, out blank))
                //    {
                //        if (orderData != null)
                //        {
                //            if (!_escrowOrderQuantity.ContainsKey(orderData.ID))
                //            {
                //                _escrowOrderQuantity.Add(orderData.ID, orderData.RemainingVol);
                //            }

                //            decimal minimum = orderData.Price * orderData.TotalVol *
                //                (decimal)(1 - Math.Pow(0.75, (double)marginTradingLevel));
                //            if ((_escrowOrderQuantity[orderData.ID] + trans.Quantity) * trans.Price > minimum)
                //            {
                //                retVal += (_escrowOrderQuantity[orderData.ID] + trans.Quantity) * trans.Price -
                //                    Math.Max(minimum, _escrowOrderQuantity[orderData.ID] * trans.Price);
                //            }
                //            else
                //            {
                //                _escrowOrderQuantity[orderData.ID] += trans.Quantity;
                //            }
                //        }
                //    }
                //}

                List<short> typeIDs = new List<short>();
                typeIDs.Add(42); // 'Market Escrow'
                JournalList escrowJournalEntries = Journal.LoadEntries(accessParams, typeIDs, startDate, endDate);
                int multiplier = 0;

                foreach (JournalEntry entry in escrowJournalEntries)
                {
                    // Use market escrow journal entries to figure out how much has gone in/out of escrow 
                    // during this time period.
                    if (entry.RecieverID == ownerID) { multiplier = 1; }
                    else if (entry.SenderID == ownerID) { multiplier = -1; }
                    retVal += entry.Amount * multiplier;
                }

                // We also need to take account of buy transactions

                // Note: When the buy order is placed, an amount is placed into escrow. This amount
                // will be the complete cost of the buy order by default.
                // If the player has margin trading then the amount will be less.
                // As the order is filled, the amount in escrow will be used up. It is only when ALL
                // the cash in escrow is used up that cash will again be taken from the wallet
                // (and appear in the journal)

                // In practice, this just means that we always know that the cash in escrow drops by
                // the full cost of the transaction (rather than some percentage based on margin trading)

                foreach (EMMADataSet.TransactionsRow trans in transactions)
                {
                    // ignore any transactions created by contracts.
                    // Although these do use escrow, we can't know how much isk is in contract escrow
                    // at the start of the period so have to ignore them.
                    if (trans.ID < 90000000000000000)
                    {
                        retVal += trans.Quantity * trans.Price;
                    }
                }
            }
            else
            {
                retVal = 0;
            }
                
            _lastEscrowDate = date;
            _lastCashInEscrow = retVal;
            
            return retVal;
        }


        public static decimal GetSellOrdersValue(int ownerID, DateTime date)
        {
            decimal retVal = 0;

            //bool corp = false;
            //APICharacter character = UserAccount.CurrentGroup.GetCharacter(ownerID, ref corp);
            date = date.ToUniversalTime();

            if (date.CompareTo(DateTime.UtcNow.AddHours(-2)) >= 0)
            {
                retVal = Orders.GetSellOrderValue(ownerID);
            }
            else
            {
                // 1. ----- Get the value of sell orders now -----
                retVal = Orders.GetSellOrderValue(ownerID);

                // 2. ----- Then increase the value by using sell transactions -----
                List<FinanceAccessParams> accessParams = new List<FinanceAccessParams>();
                accessParams.Add(new FinanceAccessParams(ownerID));

                // Get transactions that occured between the current date and the 
                // desired date.
                EMMADataSet.TransactionsDataTable transactions = Transactions.GetTransData(accessParams,
                    new List<int>(), new List<int>(), new List<int>(), date, DateTime.UtcNow, "Sell");
                Dictionary<int, long> deltaQuantaties = new Dictionary<int, long>();

                // Use the sales quantity and price per item to calculate the change in sale order value
                foreach (EMMADataSet.TransactionsRow trans in transactions)
                {
                    retVal += trans.Quantity * trans.Price;
                }

                // 3. ----- Finally reduce the value by taking off the value of sell orders that were 
                // created between the current date and the date we are getting the value for -----
                List<AssetAccessParams> accessParams2 = new List<AssetAccessParams>();
                accessParams2.Add(new AssetAccessParams(ownerID));

                OrdersList orders = Orders.LoadOrders(accessParams2, new List<int>(), new List<int>(), 0, "sell");
                foreach (Order order in orders)
                {
                    if (order.Date.CompareTo(DateTime.UtcNow) < 0 && order.Date.CompareTo(date) > 0)
                    {
                        retVal -= order.TotalVol * order.Price;
                    }
                }
            }

            return retVal;
        }


        public static decimal GetAssetsValue(int ownerID, DateTime date)
        {
            decimal retVal = 0;
            bool storeValue = true;

            //bool corp = false;
            //APICharacter character = UserAccount.CurrentGroup.GetCharacter(ownerID, ref corp);
            date = date.ToUniversalTime();

            if (date.CompareTo(DateTime.UtcNow.AddHours(-2)) >= 0)
            {
                List<int> excludedStates = new List<int>();
                excludedStates.Add((int)AssetStatus.States.ForSaleViaMarket);
                retVal = Assets.GetAssetsValue(ownerID, excludedStates);
                //retVal += Orders.GetSellOrderValue(character.CharID, corp, 0);
            }
            else
            {
                // I've removed the total asset value history table stuff because it can cause 
                // incorrectly values items to persist even after the incorrect valuation has
                // been fixed by the user.
                // It'll take longer but it should be more accurate.

                //EMMADataSet.AssetsHistoryDataTable assetsHistory = new EMMADataSet.AssetsHistoryDataTable();
                DateTime startDate = date, endDate = DateTime.UtcNow;
                //lock (tableAdapter)
                //{
                //    tableAdapter.FillByClosest(assetsHistory, character.CharID, corp, date);
                //}

                //if (assetsHistory.Count < 0)
                //{
                    // If there is no data in the database then get the current value of all assets and use
                    // it to work from.
                    List<int> excludedStates = new List<int>();
                    excludedStates.Add((int)AssetStatus.States.ForSaleViaMarket);
                    retVal = Assets.GetAssetsValue(ownerID, excludedStates);
                    startDate = date;
                    endDate = DateTime.UtcNow;
                /*}
                else
                {
                    for (int i = 0; i < assetsHistory.Count; i++)
                    {
                        if (assetsHistory[i].Date.CompareTo(date) == 0)
                        {
                            // If the data from the database is for the exact date we want then we're done.
                            retVal = assetsHistory[0].Value;
                            storeValue = false;
                        }
                    }
                    if (retVal == 0)
                    {
                        // If we don't find an exact match then setup the vairables and 
                        // move on to the next part.
                        retVal = assetsHistory[0].Value;

                        if (date.CompareTo(assetsHistory[0].Date) > 0)
                        {
                            startDate = assetsHistory[0].Date;
                            endDate = date;
                        }
                        else
                        {
                            startDate = date;
                            endDate = assetsHistory[0].Date;
                        }
                    }
                }*/

                
                if (storeValue)
                {
                    // Get sell orders that have been created between the dates we are looking at.
                    List<AssetAccessParams> accessParams = new List<AssetAccessParams>();
                    accessParams.Add(new AssetAccessParams(ownerID));
                    Dictionary<int, long> deltaQuantaties = new Dictionary<int, long>();

                    /// Don't need to do this anymore since items in sell orders are included in the 
                    /// Asset data.
                    //OrdersList orders = Orders.LoadOrders(accessParams, new List<int>(), new List<int>(), 0, "sell");
                    //foreach (Order order in orders)
                    //{
                    //    if (order.Date.CompareTo(startDate) > 0 && order.Date.CompareTo(endDate) < 0)
                    //    {
                    //        if (deltaQuantaties.ContainsKey(order.ItemID))
                    //        {
                    //            deltaQuantaties[order.ItemID] += order.TotalVol;
                    //        }
                    //        else
                    //        {
                    //            deltaQuantaties.Add(order.ItemID, order.TotalVol);
                    //        }
                    //    }
                    //}


                    List<FinanceAccessParams> accessParams2 = new List<FinanceAccessParams>();
                    accessParams2.Add(new FinanceAccessParams(ownerID));

                    // Get transactions that occured between the closest data point we have and the 
                    // desired date.
                    EMMADataSet.TransactionsDataTable transactions = Transactions.GetTransData(accessParams2,
                        new List<int>(), new List<int>(), new List<int>(), startDate, endDate, "both");
                    int mult = 0;

                    // Total up the increase and decrease in quantity of each item due to transactions.
                    foreach (EMMADataSet.TransactionsRow trans in transactions)
                    {
                        if ((trans.BuyerID == ownerID /*|| trans.BuyerCharacterID == ownerID*/) &&
                            (trans.SellerID == ownerID /*|| trans.SellerCharacterID == ownerID*/))
                        {
                            mult = 0;
                        }
                        else if (trans.BuyerID == ownerID /*|| trans.BuyerCharacterID == ownerID*/)
                        {
                            mult = -1;
                        }
                        if (trans.SellerID == ownerID /*|| trans.SellerCharacterID == ownerID*/)
                        {
                            // Since we deal with the value of items in sell orders seperately,
                            // we need to figure out if this sell transaction relates to a sell
                            // order or was just a direct sale.
                            Order empty, sellOrder;
                            if (Orders.GetOrder(new Transaction(trans), out empty, out sellOrder))
                            {
                                // Found a matching order so ignore this transaction.
                                mult = 0;
                            }
                            else
                            {
                                // We can't find a matching sell order so assume it was a direct sale
                                // and include with our numbers here.
                                mult = 1;
                            }
                        }

                        if (mult != 0)
                        {
                            if (deltaQuantaties.ContainsKey(trans.ItemID))
                            {
                                deltaQuantaties[trans.ItemID] += trans.Quantity * mult;
                            }
                            else
                            {
                                deltaQuantaties.Add(trans.ItemID, trans.Quantity * mult);
                            }
                        }
                    }

                    // Get Items gained / lost during this time.
                    EMMADataSet.AssetsLostDataTable lostAssets = AssetsLost.GetAssetsLost(
                        ownerID, startDate, endDate);
                    foreach (EMMADataSet.AssetsLostRow asset in lostAssets)
                    {
                        if (deltaQuantaties.ContainsKey(asset.ItemID))
                        {
                            deltaQuantaties[asset.ItemID] += asset.Quantity;
                        }
                        else
                        {
                            deltaQuantaties.Add(asset.ItemID, asset.Quantity);
                        }
                    }
                    EMMADataSet.AssetsProducedDataTable gainedAssets = AssetsProduced.GetAssetsProduced(
                        ownerID, startDate, endDate);
                    foreach (EMMADataSet.AssetsProducedRow asset in gainedAssets)
                    {
                        if (deltaQuantaties.ContainsKey(asset.ItemID))
                        {
                            deltaQuantaties[asset.ItemID] -= asset.Quantity;
                        }
                        else
                        {
                            deltaQuantaties.Add(asset.ItemID, -1 * asset.Quantity);
                        }
                    }


                    // Use the total increase/decrease in items to calculate the change in asset value
                    Dictionary<int, long>.Enumerator enumerator = deltaQuantaties.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        // use the average of the item's value at the start and end of the period we're 
                        // looking at.
                        retVal += ((UserAccount.CurrentGroup.ItemValues.GetItemValue(enumerator.Current.Key, 
                            10000002, startDate) + UserAccount.CurrentGroup.ItemValues.GetItemValue(
                            enumerator.Current.Key, 10000002, endDate)) / 2) *
                            enumerator.Current.Value;
                    }
                }
            }

            if (storeValue) { SaveAssetsValue(ownerID, date, retVal); }

            return retVal;
        }


        private static void SaveAssetsValue(int ownerID, DateTime date, decimal value)
        {
            bool corp = false;
            APICharacter character = UserAccount.CurrentGroup.GetCharacter(ownerID, ref corp);

            EMMADataSet.AssetsHistoryDataTable assetsHistory = new EMMADataSet.AssetsHistoryDataTable();
            EMMADataSet.AssetsHistoryRow data = null;
            lock (tableAdapter)
            {
                tableAdapter.FillByClosest(assetsHistory, ownerID, date);
            }

            if (assetsHistory.Count > 0)
            {
                for (int i = 0; i < assetsHistory.Count; i++)
                {
                    if (assetsHistory[i].Date.Date.CompareTo(date.Date) == 0)
                    {
                        data = assetsHistory[i];
                    }
                }
            }

            if (data == null)
            {
                data = assetsHistory.NewAssetsHistoryRow();
                data.OwnerID = ownerID;
                data.Corp = corp;
                data.Date = date;
                data.Value = 0;
                assetsHistory.AddAssetsHistoryRow(data);
            }

            data.Value = value;

            lock (tableAdapter)
            {
                tableAdapter.Update(assetsHistory);
            }
        }
    }
}
