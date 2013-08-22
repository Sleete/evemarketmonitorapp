using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.AbstractionClasses;

namespace EveMarketMonitorApp.Reporting
{
    class ItemReport : ReportBase
    {
        private bool _byItemGroup;
        private bool _tradedItemsOnly;
        private EveDataSet.invMarketGroupsDataTable _marketGroups = new EveDataSet.invMarketGroupsDataTable();
        private static string[] _allColumnNames = { "Average Buy Price", "Average Sell Price", 
            "Units Bought", "Units Sold", "Cost Of Units Sold",
            "Gross Margin %", "Gross Margin per Item", "Gross Profit",
            "Broker Fees", "Transaction Fees", "Transport Costs", "Overheads %", "Net Profit"};
        private List<long> _regionIDs;
        private List<long> _stationIDs;
        private List<int> _itemIDs;
        private bool _useMostRecentBuyPrice;
        private bool _restrictedCostCalc;

        public ItemReport(bool byGroup)
        {
            _name = "Item Report";
            _title = "Item Report";
            _allowSort = !byGroup;

            _expectedParams = new string[11];
            _expectedParams[0] = "StartDate";
            _expectedParams[1] = "EndDate";
            _expectedParams[2] = "RegionIDs";
            _expectedParams[3] = "StationIDs";
            _expectedParams[4] = "ItemIDs";
            _expectedParams[5] = "ColumnsVisible";
            _expectedParams[6] = "UseMostRecentBuyPrice";
            _expectedParams[7] = "FinanceAccessParams";
            _expectedParams[8] = "AssetAccessParams";
            _expectedParams[9] = "TradedItemsOnly";
            _expectedParams[10] = "RestrictedCostCalc";

            _byItemGroup = byGroup;
        }

        public static string[] GetPossibleColumns()
        {
            return _allColumnNames;
        }

        protected override void InitSections()
        {
            _sections = new ReportSections();

            UpdateStatus(0, 1, "Building Report Sections...", "", false);

            if (_byItemGroup)
            {
                // Note that an additonal root section 'Non-market items' may be added during
                // GetDataFromDatabase if it is required.
                List<int> itemIDs = Items.GetItemIDsWithTransactions(_financeAccessParams);
                List<int> tmpItemIDs = new List<int>();
                if (_tradedItemsOnly)
                {
                    List<int> tradedItemIDs = UserAccount.CurrentGroup.TradedItems.GetAllItemIDs();
                    foreach (int itemID in itemIDs)
                    {
                        if (tradedItemIDs.Contains(itemID)) { tmpItemIDs.Add(itemID); }
                    }
                    itemIDs = tmpItemIDs;
                }
                _marketGroups = MarketGroups.GetGroupsForItems(itemIDs);
                DataRow[] rootGroups = _marketGroups.Select("parentGroupID IS null");
                int counter = 0;
                ReportSection rootSection = new ReportSection(_columns.Length, "All Items", "All Items", this);
                _sections.Add(rootSection);
                foreach (DataRow group in rootGroups)
                {
                    counter++;
                    EveDataSet.invMarketGroupsRow marketGroup = (EveDataSet.invMarketGroupsRow)group;
                    ReportSection section = new ReportSection(_columns.Length, marketGroup.marketGroupID.ToString(),
                        marketGroup.marketGroupName, this);
                    rootSection.AddSection(section);
                    BuildSection(section);
                    UpdateStatus(counter, rootGroups.Length, "", section.Text, false);
                }
            }
            else
            {
                _sections.Add(new ReportSection(_columns.Length, "All Items", "All Items", this));
            }
            UpdateStatus(1, 1, "", "", false);
        }

        private void BuildSection(ReportSection section)
        {
            DataRow[] childGroups = _marketGroups.Select("parentGroupID = " + section.Name);
            foreach (DataRow group in childGroups)
            {
                EveDataSet.invMarketGroupsRow marketGroup = (EveDataSet.invMarketGroupsRow)group;
                ReportSection childSection = new ReportSection(_columns.Length, marketGroup.marketGroupID.ToString(),
                    marketGroup.marketGroupName, this);
                section.AddSection(childSection);
                BuildSection(childSection);
            }
        }

        //public decimal GetTotalProfit()
        //{
        //    decimal retVal = 0;

        //    foreach (ReportSection section in sections)
        //    {
        //        for (int i = 0; i < section.NumRows(); i++)
        //        {
        //            string rowName = section.GetRow(i).Name;
        //            if (!rowName.Equals(section.Type.Name))
        //            {
        //                retVal += GetValue("Total Profit", rowName);
        //            }
        //        }
        //    }

        //    return retVal;
        //}

        //public string GetMostProfitableItem()
        //{
        //    string retVal = "";
        //    decimal highestProfit = -1;

        //    foreach (ReportSection section in sections)
        //    {
        //        for (int i = 0; i < section.NumRows(); i++)
        //        {
        //            string rowName = section.GetRow(i).Name;
        //            if (!rowName.Equals(section.Type.Name))
        //            {
        //                decimal thisProfit = GetValue("Total Profit", rowName);
        //                if (thisProfit > highestProfit)
        //                {
        //                    highestProfit = thisProfit;
        //                    retVal = section.GetRow(i).Text;
        //                }
        //            }
        //        }
        //    }

        //    return retVal.Trim();
        //}


        /// <summary>
        /// Initialise column array, set names and header text, etc.
        /// </summary>
        /// <param name="parameters"></param>
        public override void InitColumns(Dictionary<string, object> parameters)
        {
            bool[] columnsVisible = null;
            int totColumns = 0;
            bool paramsOk = true;

            // Extract parameters...
            try
            {
                for (int i = 0; i < _expectedParams.Length; i++)
                {
                    object paramValue = null;
                    paramsOk = paramsOk && parameters.TryGetValue(_expectedParams[i], out paramValue);
                    if (_expectedParams[i].Equals("StartDate")) _startDate = (DateTime)paramValue;
                    if (_expectedParams[i].Equals("EndDate")) _endDate = (DateTime)paramValue;
                    if (_expectedParams[i].Equals("RegionIDs")) _regionIDs = (List<long>)paramValue;
                    if (_expectedParams[i].Equals("StationIDs")) _stationIDs = (List<long>)paramValue;
                    if (_expectedParams[i].Equals("ItemIDs")) _itemIDs = (List<int>)paramValue;
                    if (_expectedParams[i].Equals("ColumnsVisible")) columnsVisible = (bool[])paramValue;
                    if (_expectedParams[i].Equals("UseMostRecentBuyPrice")) _useMostRecentBuyPrice = (bool)paramValue;
                    if (_expectedParams[i].Equals("RestrictedCostCalc")) _restrictedCostCalc = (bool)paramValue;
                    if (_expectedParams[i].Equals("FinanceAccessParams"))
                        _financeAccessParams = (List<FinanceAccessParams>)paramValue;
                    if (_expectedParams[i].Equals("AssetAccessParams"))
                        _assetAccessParams = (List<AssetAccessParams>)paramValue;
                    if (_expectedParams[i].Equals("TradedItemsOnly")) _tradedItemsOnly = (bool)paramValue;
                }
            }
            catch (Exception)
            {
                paramsOk = false;
            }

            if (!paramsOk)
            {
                // If parameters are wrong in some way then throw an exception. 
                string message = "Unable to parse parameters for report '" +
                    _title + "'.\r\nExpected";
                for (int i = 0; i < _expectedParams.Length; i++)
                {
                    message = message + " " + _expectedParams[i] + (i == _expectedParams.Length - 1 ? "." : ",");
                }
                UpdateStatus(0, 0, "Error", message, false);
                throw new EMMAReportingException(ExceptionSeverity.Error, message);
            }

            _subtitle = "Between " + _startDate.ToShortDateString() + " " + _startDate.ToShortTimeString() + 
                " and " + _endDate.ToShortDateString() + " " + _endDate.ToShortTimeString();

            totColumns = 0;
            for (int i = 0; i < columnsVisible.Length; i++)
            {
                if (columnsVisible[i]) totColumns++;
            }

            try
            {
                _columns = new ReportColumn[totColumns];

                UpdateStatus(0, totColumns, "", "Building Report Columns...", false);

                // Iterate through the columnsVisible array. Add any columns marked as visible to the report. 
                int colNum = 0;
                for (int i = 0; i < columnsVisible.Length; i++)
                {
                    if (columnsVisible[i])
                    {
                        _columns[colNum] = new ReportColumn(_allColumnNames[i], _allColumnNames[i]);
                        if (_allColumnNames[i].Contains("%"))
                        {
                            _columns[colNum].DataType = ReportDataType.Percentage;
                            _columns[colNum].SectionRowBehavior = SectionRowBehavior.Average;
                        }
                        else if (_allColumnNames[i].Equals("Units Bought") || 
                            _allColumnNames[i].Equals("Units Sold"))
                        {
                            _columns[colNum].DataType = ReportDataType.Number;
                            _columns[colNum].SectionRowBehavior = SectionRowBehavior.Blank;
                        }
                        else if (_allColumnNames[i].Contains("Fees") || _allColumnNames[i].Contains("Costs") ||
                            _allColumnNames[i].Contains("Profit"))
                        {
                            _columns[colNum].DataType = ReportDataType.ISKAmount;
                            _columns[colNum].SectionRowBehavior = SectionRowBehavior.Sum;
                        }
                        else
                        {
                            _columns[colNum].DataType = ReportDataType.ISKAmount;
                            _columns[colNum].SectionRowBehavior = SectionRowBehavior.Average;
                        }
                        colNum++;
                        UpdateStatus(colNum, totColumns, "", "", false);
                    }
                }

                UpdateStatus(0, 0, "", "Columns Complete", false);
            }
            catch (Exception ex)
            {
                throw new EMMAReportingException(ExceptionSeverity.Error, "Problem creating columns: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Fill report with data 
        /// </summary>
        public override void FillReport()
        {
            GetDataFromDatabase();
        }

        /// <summary>
        /// Gets the required data from the database
        /// </summary>
        public void GetDataFromDatabase()
        {
            int count2 = 0;

            int maxProgress = _itemIDs.Count;
            UpdateStatus(0, maxProgress, "Getting Report Data...", "", false);

            for (int i = 0; i < _itemIDs.Count; i++)
            {
                Diagnostics.ResetAllTimers();
                Diagnostics.StartTimer("ItemReport.Item");
                int itemID = _itemIDs[i];
                string itemName = Items.GetItemName(itemID);
                UpdateStatus(i, maxProgress, "", itemName, false);

                long totBuyVolume = 0, totSellVolume = 0;
                decimal avgBuyPrice = 0, avgSellPrice = 0;
                decimal marginPerc = 0, marginAbs = 0, netProfit = 0;
                decimal brokerBuyFees = 0, brokerSellFees = 0, displayedBrokerFees = 0;
                decimal transFees = 0, transportCosts = 0, grossProfit = 0, avgSellProfit = 0;
                decimal soldUnitsBuyPrice = 0, soldUnitsPurchaseBrokerFees = 0;
                decimal overheads = 0, overheadsPerc = 0;

                // Get values from database
                Diagnostics.StartTimer("ItemReport.Part1");
                List<int> itemIDList = new List<int>();
                itemIDList.Add(itemID);
                Transactions.GetItemTransData(
                    _financeAccessParams, itemIDList, _regionIDs, _stationIDs, _startDate, _endDate,
                    ref avgSellPrice, ref avgBuyPrice, ref totBuyVolume, ref totSellVolume,
                    ref brokerBuyFees, ref brokerSellFees, ref transFees, ref transportCosts, ref avgSellProfit,
                    true, true, false, _useMostRecentBuyPrice, _restrictedCostCalc);

                Diagnostics.StopTimer("ItemReport.Part1");

                // Only add the item if we actually have any purchases or sales during this time period..
                if (totBuyVolume > 0 || totSellVolume > 0)
                {
                    DiagnosticUpdate("", "--------------------------------------------");
                    DiagnosticUpdate("", "------------ Timing diagnostics ------------");
                    DiagnosticUpdate("", "\tTime getting data: " + Diagnostics.GetRunningTime("ItemReport.Part1"));
                    DiagnosticUpdate("", "\t\tGet buy transactions: " + Diagnostics.GetRunningTime("Transactions.GetBuyTrans"));
                    DiagnosticUpdate("", "\t\tProcess buy transactions: " + Diagnostics.GetRunningTime("Transactions.ProcessBuyTrans"));
                    //DiagnosticUpdate("", "\t\t\tCalc buy broker fees: " + Diagnostics.GetRunningTime("Transactions.CalcBuyBrokerFees"));
                    //DiagnosticUpdate("", "\t\t\t\tGet broker rel level: " + Diagnostics.GetRunningTime("Transactions.GetBkrRelLvl"));
                    //DiagnosticUpdate("", "\t\t\t\tGet order: " + Diagnostics.GetRunningTime("Transactions.CalcBuyBrokerFeesGetOrder"));
                    //DiagnosticUpdate("", "\t\t\t\tGet standing: " + Diagnostics.GetRunningTime("Transactions.GetStanding"));
                    //DiagnosticUpdate("", "\t\t\t\tCalculate: " + Diagnostics.GetRunningTime("Transactions.CalculateBuyBkr"));
                    DiagnosticUpdate("", "\t\tCalc buy median: " + Diagnostics.GetRunningTime("Transactions.CalcBuyMedian"));
                    DiagnosticUpdate("", "\t\tGet sell transactions: " + Diagnostics.GetRunningTime("Transactions.GetSellTrans"));
                    DiagnosticUpdate("", "\t\tProcess sell transactions: " + Diagnostics.GetRunningTime("Transactions.ProcessSellTrans"));
                    //DiagnosticUpdate("", "\t\t\tCalc sell broker fees: " + Diagnostics.GetRunningTime("Transactions.CalcSellBrokerFees"));
                    //DiagnosticUpdate("", "\t\t\tCalc sell transaction tax: " + Diagnostics.GetRunningTime("Transactions.CalcSellTransTax"));
                    DiagnosticUpdate("", "\t\tCalc sell transport costs: " + Diagnostics.GetRunningTime("Transactions.CalcSellTransportCosts"));
                    DiagnosticUpdate("", "\t\tCalc sell median: " + Diagnostics.GetRunningTime("Transactions.CalcSellMedian"));
     
                    count2++;
                    if (avgSellPrice != 0)
                    {
                        if (avgSellProfit == 0)
                        {
                            // If we don't have a profit value from the transaction record, fall back
                            // onto the old way of calulating profit.
                            Diagnostics.StartTimer("ItemReport.Part2");
                            // To calculate profit, we don't want to use the purchase price listed but instead the 
                            // average buy price of x units of this item, ignoring the most recent y units bought.
                            // where x = quantity sold and y = current units as assets.
                            // Also, don't want to restrict this to a specific region.
                            long quantityToIgnore = 0;
                            if (!_useMostRecentBuyPrice)
                            {
                                quantityToIgnore = Assets.GetTotalQuantity(_assetAccessParams, itemID);
                                OrdersList sellOrders = Orders.LoadOrders(_assetAccessParams, itemIDList,
                                    new List<long>(), (int)OrderState.Active, "sell");
                                foreach (Order sellOrder in sellOrders)
                                {
                                    quantityToIgnore += sellOrder.RemainingVol;
                                }
                            }
                            Transactions.GetAverageBuyPrice(_financeAccessParams, itemIDList, new List<long>(),
                                new List<long>(), (int)totSellVolume, quantityToIgnore,
                                ref soldUnitsBuyPrice, ref soldUnitsPurchaseBrokerFees, true);
                        }
                        else
                        {
                            soldUnitsBuyPrice = avgSellPrice - avgSellProfit;
                        }


                        //Diagnostics.StopTimer("ItemReport.Part2");

                        //DiagnosticUpdate("", "\tTime getting extended data: " + Diagnostics.GetRunningTime("ItemReport.Part2"));
                        //DiagnosticUpdate("", "\t\tGet buy transactions: " + Diagnostics.GetRunningTime("Transactions.GetBuyTrans"));
                        //DiagnosticUpdate("", "\t\tProcess buy transactions: " + Diagnostics.GetRunningTime("Transactions.ProcessBuyTrans"));
                        ////DiagnosticUpdate("", "\t\t\tCalc buy broker fees: " + Diagnostics.GetRunningTime("Transactions.CalcBuyBrokerFees"));
                        //DiagnosticUpdate("", "\t\tCalc buy median: " + Diagnostics.GetRunningTime("Transactions.CalcBuyMedian"));
                        //DiagnosticUpdate("", "\t\tGet sell transactions: " + Diagnostics.GetRunningTime("Transactions.GetSellTrans"));
                        //DiagnosticUpdate("", "\t\tProcess sell transactions: " + Diagnostics.GetRunningTime("Transactions.ProcessSellTrans"));
                        ////DiagnosticUpdate("", "\t\t\tCalc sell broker fees: " + Diagnostics.GetRunningTime("Transactions.CalcSellBrokerFees"));
                        ////DiagnosticUpdate("", "\t\t\tCalc sell transaction tax: " + Diagnostics.GetRunningTime("Transactions.CalcSellTransTax"));
                        //DiagnosticUpdate("", "\t\tCalc sell transport costs: " + Diagnostics.GetRunningTime("Transactions.CalcSellTransportCosts"));
                        //DiagnosticUpdate("", "\t\tCalc sell median: " + Diagnostics.GetRunningTime("Transactions.CalcSellMedian"));
                    }

                    if (totSellVolume > 0 && (soldUnitsBuyPrice == 0 || 
                        UserAccount.CurrentGroup.ItemValues.ForceDefaultBuyPriceGet(itemID)))
                    {
                        // If we couldn't find a buy price for the sold items then try and 
                        // get a buy price from other sources
                        bool tmp = UserAccount.CurrentGroup.Settings.UseEveCentral;
                        UserAccount.CurrentGroup.Settings.UseEveCentral = false;
                        soldUnitsBuyPrice = UserAccount.CurrentGroup.ItemValues.GetBuyPrice(itemID,
                            _regionIDs.Count == 1 ? _regionIDs[0] : -1);
                        UserAccount.CurrentGroup.Settings.UseEveCentral = tmp;
                    }
                    if (UserAccount.CurrentGroup.ItemValues.ForceDefaultSellPriceGet(itemID))
                    {
                        avgSellPrice = UserAccount.CurrentGroup.ItemValues.DefaultPriceGet(itemID);
                    }

                    Diagnostics.StartTimer("ItemReport.Part3");
                    // Calculate data for columns
                    if (avgSellPrice > 0 && soldUnitsBuyPrice > 0) marginAbs = avgSellPrice - soldUnitsBuyPrice;
                    if (soldUnitsBuyPrice > 0) marginPerc = (marginAbs / soldUnitsBuyPrice);
                    grossProfit = marginAbs * totSellVolume;

                    displayedBrokerFees = brokerSellFees + soldUnitsPurchaseBrokerFees;
                    overheads = displayedBrokerFees + transFees + transportCosts;
                    overheadsPerc = (grossProfit == 0 ? 0 : (overheads / grossProfit));
                    netProfit = grossProfit - overheads;
                    Diagnostics.StopTimer("ItemReport.Part3");

                    Diagnostics.StartTimer("ItemReport.Part4");
                    // Add a row for this item to the grid.
                    EveDataSet.invTypesRow itemData = Items.GetItem(itemID);

                    ReportSection section = null;
                    if (_byItemGroup)
                    {
                        section = _sections.GetSection(itemData.marketGroupID.ToString());
                    } 
                    else 
                    {
                        section = _sections[0];
                    }
                    section.AddRow(_columns.Length, itemID.ToString(), itemName);
                    Diagnostics.StopTimer("ItemReport.Part4");

                    Diagnostics.StartTimer("ItemReport.Part5");
                    // Add the data for this row.                    
                    SetValue("Average Buy Price", itemID.ToString(), decimal.Round(avgBuyPrice, 2), true);
                    SetValue("Average Sell Price", itemID.ToString(), decimal.Round(avgSellPrice, 2), true);
                    SetValue("Units Bought", itemID.ToString(), totBuyVolume, true);
                    SetValue("Units Sold", itemID.ToString(), totSellVolume, true);
                    SetValue("Cost Of Units Sold", itemID.ToString(), decimal.Round(soldUnitsBuyPrice, 2), true);
                    SetValue("Gross Margin %", itemID.ToString(), decimal.Round(marginPerc, 2), true);
                    SetValue("Gross Margin per Item", itemID.ToString(), decimal.Round(marginAbs, 2), true);
                    SetValue("Gross Profit", itemID.ToString(), decimal.Round(grossProfit, 2), true);
                    SetValue("Broker Fees", itemID.ToString(), decimal.Round(displayedBrokerFees, 2), true);
                    SetValue("Transaction Fees", itemID.ToString(), decimal.Round(transFees, 2), true);
                    SetValue("Transport Costs", itemID.ToString(), decimal.Round(transportCosts, 2), true);
                    SetValue("Overheads %", itemID.ToString(), decimal.Round(overheadsPerc, 2), true);
                    SetValue("Net Profit", itemID.ToString(), decimal.Round(netProfit, 2), true);
                    Diagnostics.StopTimer("ItemReport.Part5");
                    Diagnostics.StopTimer("ItemReport.Item");

                    DiagnosticUpdate("", "\tCalculation time: " + Diagnostics.GetRunningTime("ItemReport.Part3"));
                    DiagnosticUpdate("", "\tTime to add rows to grid: " + Diagnostics.GetRunningTime("ItemReport.Part4"));
                    DiagnosticUpdate("", "\tTime to set data values: " + Diagnostics.GetRunningTime("ItemReport.Part5"));
                    DiagnosticUpdate("", "Total time for item: " + Diagnostics.GetRunningTime("ItemReport.Item"));
                    DiagnosticUpdate("", "-----------------------------");
                }
            }

            // Remove any empty sections
            string[] rows = GetRowNames();
            foreach (string row in rows)
            {
                if (row.StartsWith("SECTIONROW"))
                {
                    ReportRow rowData = GetRowByName(row);
                    ReportSection section = rowData as ReportSection;
                    if (section != null)
                    {
                        decimal value1 = section.GetData(GetColumnByName("Average Buy Price"));
                        decimal value2 = section.GetData(GetColumnByName("Average Sell Price"));
                        if (value1 == 0 && value2 == 0)
                        {
                            section.RemoveSection();
                        }
                    }
                }
            }
        }



    }
}
