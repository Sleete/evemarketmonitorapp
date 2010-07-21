using System;
using System.Collections.Generic;
using System.Text;

using EveMarketMonitorApp.AbstractionClasses;
using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.DatabaseClasses
{
    static class Orders
    {
        private static EMMADataSetTableAdapters.OrdersTableAdapter tableAdapter = 
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.OrdersTableAdapter();

        private static int _lastBuyerID;
        private static EMMADataSet.OrdersDataTable _lastBuyerOrders;
        private static int _lastSellerID;
        private static EMMADataSet.OrdersDataTable _lastSellerOrders;
        private static int _lastItemID;


        public static void MigrateOrdersToCorpID(int charID, int corpID)
        {
            lock (tableAdapter)
            {
                tableAdapter.OrdersMigrateToCorpID(charID, corpID);
            }
        }

        public static void Store(Order orderData)
        {
            EMMADataSet.OrdersDataTable table = new EMMADataSet.OrdersDataTable();
            EMMADataSet.OrdersRow orderRow = null;
            bool newRow = false;

            lock (tableAdapter)
            {
                tableAdapter.FillByID(table, orderData.ID);
            }

            if (table.Count == 0)
            {
                orderRow = table.NewOrdersRow();
                newRow = true;
            }
            else
            {
                orderRow = table[0];
            }

            bool corp = false;
            APICharacter charData = UserAccount.CurrentGroup.GetCharacter(orderData.OwnerID, ref corp);
            orderRow.OwnerID = charData.CharID;
            orderRow.ForCorp = corp;
            orderRow.StationID = orderData.StationID;
            orderRow.TotalVol = orderData.TotalVol;
            orderRow.RemainingVol = orderData.RemainingVol;
            orderRow.MinVolume = orderData.MinVol;
            orderRow.OrderState = orderData.StateID;
            orderRow.ItemID = orderData.ItemID;
            orderRow.Range = orderData.Range;
            orderRow.WalletID = orderData.WalletID;
            orderRow.Duration = orderData.Duration;
            orderRow.Escrow = orderData.Escrow;
            orderRow.Price = orderData.Price;
            orderRow.BuyOrder = orderData.BuyOrder;
            orderRow.Issued = orderData.Date;
            orderRow.Processed = false;

            if (newRow)
            {
                table.AddOrdersRow(orderRow);
            }

            lock (tableAdapter)
            {
                tableAdapter.Update(table);
            }
        }


        public static void SetProcessed(int ownerID, bool processed)
        {
            lock (tableAdapter)
            {
                tableAdapter.SetProcessed(ownerID, processed);
            }
        }
        
        public static void SetProcessedByID(int orderID, bool processed)
        {
            lock (tableAdapter)
            {
                tableAdapter.SetProcessedByID(orderID, processed);
            }
        }

        public static void FinishUnProcessed(int ownerID)
        {
            lock (tableAdapter)
            {
                bool notify = UserAccount.CurrentGroup.Settings.OrdersNotifyEnabled;
                bool notifyBuy = UserAccount.CurrentGroup.Settings.OrdersNotifyBuy;
                bool notifySell = UserAccount.CurrentGroup.Settings.OrdersNotifySell;
                tableAdapter.FinishUnProcessed(ownerID, notify, notifyBuy, notifySell);
            }
        }



        public static EMMADataSet.OrdersDataTable GetOrdersByIssueDate(int ownerID, short walletID,
            DateTime earliestIsssueDate, DateTime latestIssueDate)
        {
            EMMADataSet.OrdersDataTable retVal = new EMMADataSet.OrdersDataTable();
            earliestIsssueDate = earliestIsssueDate.ToUniversalTime();
            latestIssueDate = latestIssueDate.ToUniversalTime();
            lock (tableAdapter)
            {
                tableAdapter.FillByIssueDate(retVal, ownerID, walletID, earliestIsssueDate, latestIssueDate);
            }

            return retVal;
        }


        public static decimal GetSellOrderValue(int ownerID)
        {
            return GetSellOrderValue(ownerID, 0);
        }
        public static decimal GetSellOrderValue(int ownerID, short walletID)
        {
            decimal retVal = 0;
            EMMADataSet.OrdersDataTable table = new EMMADataSet.OrdersDataTable();


            lock (tableAdapter)
            {
                tableAdapter.FillByAnySingle(table, ownerID, walletID, 0, 0, 
                    (int)OrderState.Active, "Sell");
            }
            foreach (EMMADataSet.OrdersRow order in table)
            {
                retVal += order.Price * order.RemainingVol;
            }

            table.Clear();
            lock (tableAdapter)
            {
                tableAdapter.FillByAnySingle(table, ownerID, walletID, 0, 0, 
                    (int)OrderState.OverbidAndUnacknowledged, "Sell");
            }
            foreach (EMMADataSet.OrdersRow order in table)
            {
                retVal += order.Price * order.RemainingVol;
            }

            return retVal;
        }

        public static decimal GetCashInEscrow(int ownerID, bool forCorp, short walletID)
        {
            decimal retVal = 0;
            EMMADataSet.OrdersDataTable table = new EMMADataSet.OrdersDataTable();

            lock (tableAdapter)
            {
                tableAdapter.FillByAnySingle(table, ownerID, walletID, 0, 0, (int)OrderState.Active, "buy");
            }

            foreach (EMMADataSet.OrdersRow order in table)
            {
                retVal += order.Escrow;
            }
            return retVal;
        }


        public static OrdersList LoadOrders(List<AssetAccessParams> accessParams, List<int> itemIDs,
            List<int> stationIDs, int state, string type)
        {
            OrdersList retVal = new OrdersList();
            EMMADataSet.OrdersDataTable table = new EMMADataSet.OrdersDataTable();
            if (itemIDs.Count == 0) { itemIDs.Add(0); }
            if (stationIDs.Count == 0) { stationIDs.Add(0); }
            string itemString = "";
            string stationString = "";
            foreach (int item in itemIDs) { itemString = itemString + (itemString.Length == 0 ? "" : ",") + item; }
            foreach (int station in stationIDs) { stationString = stationString + (stationString.Length == 0 ? "" : ",") + station; }
            lock (tableAdapter)
            {
                tableAdapter.FillByAny(table, AssetAccessParams.BuildAccessList(accessParams), itemString,
                    stationString, state, type);
            }

            foreach (EMMADataSet.OrdersRow row in table)
            {
                Order order = new Order(row);
                retVal.Add(order);
            }
            return retVal;
        }



        public static void Store(EMMADataSet.OrdersDataTable ordersTable)
        {
            lock (tableAdapter)
            {
                tableAdapter.Update(ordersTable);
            }
        }


        /// <summary>
        /// Checks if the specified row exists in the database.
        /// If it does then the table will contain the row (as well as anything else that was in it
        /// before this method was called)
        /// </summary>
        /// <param name="ordersTable"></param>
        /// <param name="orderRow"></param>
        /// <param name="ID">The ID of the order that matches the supplied one</param>
        /// <returns></returns>
        public static bool Exists(EMMADataSet.OrdersDataTable ordersTable, EMMADataSet.OrdersRow orderRow,
            ref int ID)
        {
            bool? exists = false;
            int? orderID = 0;
            tableAdapter.ClearBeforeFill = false;

            lock (tableAdapter)
            {
                tableAdapter.FillOrderExists(ordersTable, orderRow.OwnerID, orderRow.WalletID,
                    orderRow.StationID, orderRow.ItemID, orderRow.TotalVol, orderRow.RemainingVol,
                    orderRow.Range, orderRow.OrderState, orderRow.BuyOrder, orderRow.Price, orderRow.EveOrderID,
                    ref exists, ref orderID);
            }

            ID = orderID.HasValue ? orderID.Value : 0;
            return exists.HasValue ? exists.Value : false;
        }


        /// <summary>
        /// Get the order used for the specified transaction. 
        /// Note that many transactions will not have orders related to them. In this case
        /// the return value will be false and the two out parameters will be null.
        /// </summary>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static bool GetOrder(Transaction trans, out Order buyOrder, out Order sellOrder)
        {
            bool retVal = false;
            bool buyerForCorp = false, sellerForCorp = false;
            int buyerID =0, sellerID = 0;
            buyOrder = null;
            sellOrder = null;

            buyerID = trans.BuyerID;
            sellerID = trans.SellerID;
            APICharacter buyChar = UserAccount.CurrentGroup.GetCharacter(buyerID, ref buyerForCorp);
            APICharacter sellChar = UserAccount.CurrentGroup.GetCharacter(sellerID, ref sellerForCorp);

            EMMADataSet.OrdersDataTable table = new EMMADataSet.OrdersDataTable();
            if (buyChar != null)
            {
                if (buyerID == _lastBuyerID && trans.ItemID == _lastItemID)
                {
                    table = _lastBuyerOrders;
                }
                else
                {
                    lock (tableAdapter)
                    {
                        tableAdapter.FillByAnySingle(table, trans.BuyerID, 0, trans.ItemID, 0, 0, "Any");
                        _lastBuyerID = buyerID;
                        _lastItemID = trans.ItemID;
                        _lastBuyerOrders = table;
                    }
                }
                buyOrder = MatchOrder(table, trans);
            }
            if (sellChar != null)
            {
                table.Clear();
                if (sellerID == _lastSellerID && trans.ItemID == _lastItemID)
                {
                    table = _lastSellerOrders;
                }
                else
                {
                    lock (tableAdapter)
                    {
                        tableAdapter.FillByAnySingle(table, sellerID, 0, trans.ItemID, 0, 0, "Any");
                        _lastSellerID = sellerID;
                        _lastItemID = trans.ItemID;
                        _lastSellerOrders = table;
                    }
                }
                sellOrder = MatchOrder(table, trans);
            }

            retVal = buyOrder != null || sellOrder != null; 
            return retVal;
        }

        /// <summary>
        /// Find the order in the provided table that matches the specified transaction.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        private static Order MatchOrder(EMMADataSet.OrdersDataTable table, Transaction trans)
        {
            Order retVal = null;
            decimal priceDiff = decimal.MaxValue;

            foreach (EMMADataSet.OrdersRow order in table)
            {
                if (retVal == null || order.Issued.CompareTo(retVal.Date) > 0)
                {
                    if (order.Issued.CompareTo(trans.Datetime) <= 0)
                    {
                        bool inRange = false;

                        if (order.Range == OrderRange.GetRangeFromText("Region"))
                        {
                            if (Stations.GetStation(order.StationID).regionID ==
                                Stations.GetStation(trans.StationID).regionID) { inRange = true; }
                        }
                        else if (order.Range == OrderRange.GetRangeFromText("Station"))
                        {
                            if (order.StationID == trans.StationID) { inRange = true; }
                        }
                        else if (order.Range == OrderRange.GetRangeFromText("Solar System"))
                        {
                            if (Stations.GetStation(order.StationID).solarSystemID ==
                                Stations.GetStation(trans.StationID).solarSystemID) { inRange = true; }
                        }
                        else
                        {
                            List<int> systemsInRange = SolarSystemDistances.GetSystemsInRange(
                                order.StationID, (int)order.Range);
                            if (systemsInRange.Contains(Stations.GetStation(trans.StationID).solarSystemID)) 
                            { 
                                inRange = true; 
                            }
                        }

                        if (inRange && Math.Abs(order.Price - trans.Price) < priceDiff)
                        {
                            retVal = new Order(order);
                            priceDiff = Math.Abs(order.Price - trans.Price);
                        }
                    }
                }
            }

            return retVal;
        }

    }
}
