using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;

using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.DatabaseClasses;

namespace EveMarketMonitorApp.Reporting
{
    class AssetsReport : ReportBase
    {
        private bool _byItemGroup;
        private EveDataSet.invMarketGroupsDataTable _marketGroups = new EveDataSet.invMarketGroupsDataTable();
        private static string[] _allColumnNames = { "Total Units", "Average Buy Price", "Est. Sell Price",
            "Total Est. Value", "Total Est. Gross Profit", "Reprocess Value", "Best Gross Profit"};
        private int _valueRegionID = 0;
        private List<int> _stationsIDs;
        private List<int> _regionIDs;
        private bool _includeInTransit = false;
        private bool _includeContainers = false;

        public AssetsReport(bool byGroup)
        {
            _name = "Assets Report";
            _title = "Assets Report";
            _allowSort = !byGroup;

            _expectedParams = new string[8];
            _expectedParams[0] = "ColumnsVisible";
            _expectedParams[1] = "ValueRegion";
            _expectedParams[2] = "StationIDs";
            _expectedParams[3] = "RegionIDs";
            _expectedParams[4] = "IncludeInTransit";
            _expectedParams[5] = "IncludeContainers";
            _expectedParams[6] = "AssetAccessParams";
            _expectedParams[7] = "FinanceAccessParams";

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
                EveDataSet.invTypesDataTable items = Items.GetItemsThatAreAssets(_assetAccessParams);
                List<int> itemIDs = new List<int>();
                foreach (EveDataSet.invTypesRow item in items)
                {
                    itemIDs.Add(item.typeID);
                }
                _marketGroups = MarketGroups.GetGroupsForItems(itemIDs);
                DataRow[] rootGroups = _marketGroups.Select("parentGroupID IS null");
                int counter = 0;
                ReportSection rootSection = new ReportSection(_columns.Length, "All Items", "All Items", this);
                _sections.Add(rootSection);
                ReportSection nonMarket = new ReportSection(_columns.Length, "Non-Market Items", "Non-Market Items", this);
                rootSection.AddSection(nonMarket);
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


/*        public decimal TotalBuyValue()
        {
            decimal retVal = 0;

            foreach (ReportSection section in sections)
            {
                for (int i = 0; i < section.NumRows(); i++)
                {
                    string rowName = section.GetRow(i).Name;
                    if (!rowName.Equals(section.Type.Name))
                    {
                        retVal += GetValue("Total Units", rowName) * GetValue("Average Buy Price", rowName);
                    }
                }
            }

            return retVal;
        }
*/
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
                    if (_expectedParams[i].Equals("ColumnsVisible")) columnsVisible = (bool[])paramValue;
                    if (_expectedParams[i].Equals("ValueRegion")) _valueRegionID = (int)paramValue;
                    if (_expectedParams[i].Equals("StationIDs")) _stationsIDs = (List<int>)paramValue;
                    if (_expectedParams[i].Equals("RegionIDs")) _regionIDs = (List<int>)paramValue;
                    if (_expectedParams[i].Equals("IncludeInTransit")) _includeInTransit = (bool)paramValue;
                    if (_expectedParams[i].Equals("IncludeContainers")) _includeContainers = (bool)paramValue;
                    if (_expectedParams[i].Equals("AssetAccessParams")) _assetAccessParams = 
                        (List<AssetAccessParams>)paramValue;
                    if (_expectedParams[i].Equals("FinanceAccessParams")) _financeAccessParams = 
                        (List<FinanceAccessParams>)paramValue;
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

            _subtitle = "Sell prices based upon data for " + Regions.GetRegionName(_valueRegionID);
            totColumns = 0;
            for (int i = 0; i < columnsVisible.Length; i++)
            {
                if(columnsVisible[i]) totColumns++;
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
                        if (_allColumnNames[i].Contains("Units"))
                        {
                            _columns[colNum].DataType = ReportDataType.Number;
                            _columns[colNum].SectionRowBehavior = SectionRowBehavior.Sum;
                        }
                        else if (_allColumnNames[i].Equals("Average Buy Price") ||
                            _allColumnNames[i].Equals("Est. Sell Price"))
                        {
                            _columns[colNum].DataType = ReportDataType.ISKAmount;
                            _columns[colNum].SectionRowBehavior = SectionRowBehavior.Blank;
                        }
                        else
                        {
                            _columns[colNum].DataType = ReportDataType.ISKAmount;
                            _columns[colNum].SectionRowBehavior = SectionRowBehavior.Sum;
                        }
                        colNum++;
                        UpdateStatus(colNum, totColumns, "", "", false);
                    }
                }
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
            EveDataSet.invTypesDataTable assetItems = Items.GetItemsThatAreAssets(_assetAccessParams);
            int maxProgress = assetItems.Count;
            UpdateStatus(0, maxProgress, "Getting Report Data...", "", false);
            //StreamWriter log = File.CreateText("C:\\AssetsReport.txt");
            //try
            //{
                // Cycle through all items that are assets and add thier data to the report
                for (int j = 0; j < assetItems.Count; j++)
                {
                    EveDataSet.invTypesRow item = assetItems[j];
                    int itemID = item.typeID;
                    long quantity = Assets.GetTotalQuantity(_assetAccessParams, item.typeID, _stationsIDs, _regionIDs,
                        _includeInTransit, _includeContainers);

                    decimal avgBuyPrice = 0, avgSellPrice = 0;
                    decimal marginAbs = 0, totalProfit = 0;
                    decimal blank1 = 0, reproValue = 0;
                    decimal bestProfit = 0;

                    List<int> itemIDs = new List<int>();
                    itemIDs.Add(itemID);

                    // Only add the item if we actually have any (although only items with positive amounts in a station
                    // are included, it would be possible to have included an item with positive and negative quantities
                    // in different stations that balance to 0 or less)..
                    if (quantity > 0)
                    {
                        // Get values from database
                        Transactions.GetAverageBuyPrice(_financeAccessParams, itemIDs, _stationsIDs, _regionIDs,
                            quantity, 0, ref avgBuyPrice, ref blank1, true);
                        avgSellPrice = UserAccount.CurrentGroup.ItemValues.GetItemValue(item.typeID, _valueRegionID, false);

                        // If we couldn't find a buy price for the sold items then try and 
                        // get a buy price from other sources
                        if (avgBuyPrice == 0 || UserAccount.CurrentGroup.ItemValues.ForceDefaultBuyPriceGet(itemID))
                        {
                            bool tmp = UserAccount.CurrentGroup.Settings.UseEveCentral;
                            UserAccount.CurrentGroup.Settings.UseEveCentral = false;
                            avgBuyPrice = UserAccount.CurrentGroup.ItemValues.GetBuyPrice(itemID,
                                _regionIDs.Count == 1 ? _regionIDs[0] : 0);
                            UserAccount.CurrentGroup.Settings.UseEveCentral = tmp;

                            if (avgBuyPrice == 0)
                            {
                                // If we still fail to get a buy price then just set it to the same as the
                                // sell price.
                                avgBuyPrice = avgSellPrice;
                            }
                        }


                        //log.WriteLine(Items.GetItemName(itemID) + ", " + quantity + ", " + avgSellPrice);


                        // Calculate data for columns
                        if (avgSellPrice > 0) marginAbs = avgSellPrice - avgBuyPrice;
                        totalProfit = marginAbs * quantity;
                        bestProfit = totalProfit;

                        // Get reprocess value
                        // reproValue = quantity * Items.GetItemReprocessValue(itemID);
                        ReprocessJob job = new ReprocessJob(0, 0, 0);
                        job.AddItem(itemID, quantity, avgBuyPrice * quantity);
                        job.UpdateResults();
                        reproValue = job.TotalResultsValue;
                        if (reproValue > quantity * avgSellPrice)
                        {
                            bestProfit = reproValue - (avgBuyPrice * quantity);
                        }

                        // Add a row for this item to the grid.
                        EveDataSet.invTypesRow itemData = Items.GetItem(itemID);
                        string itemName = itemData.typeName;
                        ReportSection section = null;
                        if (_byItemGroup)
                        {
                            if (itemData.IsmarketGroupIDNull())
                            {
                                section = _sections.GetSection("Non-Market Items");
                            }
                            else
                            {
                                section = _sections.GetSection(itemData.marketGroupID.ToString());
                            }
                        }
                        else
                        {
                            section = _sections[0];
                        }
                        section.AddRow(_columns.Length, itemID.ToString(), itemName);

                        // Add the data for this row.
                        SetValue("Average Buy Price", itemID.ToString(), avgBuyPrice, true);
                        SetValue("Est. Sell Price", itemID.ToString(), avgSellPrice, true);
                        SetValue("Total Units", itemID.ToString(), quantity, true);
                        SetValue("Total Est. Value", itemID.ToString(), quantity * avgSellPrice, true);
                        SetValue("Total Est. Gross Profit", itemID.ToString(), totalProfit, true);
                        SetValue("Reprocess Value", itemID.ToString(), reproValue, true);
                        SetValue("Best Gross Profit", itemID.ToString(), bestProfit, true);

                        UpdateStatus(j, maxProgress, "", itemName, false);
                    }
                }
            //}
            //finally
            //{
            //    log.Close();
            //}


        }
        
    }
}
