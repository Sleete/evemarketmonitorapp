using System;
using System.Collections.Generic;
using System.Text;

using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.AbstractionClasses;
using EveMarketMonitorApp.GUIElements;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public delegate void ItemValueCalcEvent(object myObject, ValueCalcEventArgs args);
    
    public class ItemValues
    {
        private static EMMADataSetTableAdapters.ItemValuesTableAdapter tableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.ItemValuesTableAdapter();
        private static EMMADataSet.ItemValuesDataTable table = new EMMADataSet.ItemValuesDataTable();

        private int _reportGroupID;

        private Cache<PriceCacheKey, decimal> _sellPriceCache;
        private Cache<PriceCacheKey, decimal> _buyPriceCache;

        public event ItemValueCalcEvent ValueCalculationEvent;

        public ItemValues(int reportGroupID)
        {
            _reportGroupID = reportGroupID;
            lock (tableAdapter)
            {
                tableAdapter.FillByID(table, _reportGroupID, 0, 0);
            }
            ClearValueCache();
        }


        public void ClearValueCache()
        {
            _sellPriceCache = new Cache<PriceCacheKey, decimal>(10000);
            _sellPriceCache.DataUpdateNeeded += new Cache<PriceCacheKey, decimal>.DataUpdateNeededHandler(
                PriceCache_DataUpdateNeeded);
            _buyPriceCache = new Cache<PriceCacheKey, decimal>(10000);
            _buyPriceCache.DataUpdateNeeded += new Cache<PriceCacheKey, decimal>.DataUpdateNeededHandler(
                PriceCache_DataUpdateNeeded);
        }

        void PriceCache_DataUpdateNeeded(object myObject, DataUpdateNeededArgs<PriceCacheKey, decimal> args)
        {
            int itemID = args.Key.ItemID;
            int regionID = args.Key.RegionID;
            DateTime valueDate = args.Key.ValueDate;
            bool buyPrice = myObject == _buyPriceCache;

            args.Data = GetItemValue(itemID, regionID, valueDate, buyPrice);
        }

        public decimal GetItemValue(int itemID, int regionID, DateTime valueDate, bool buyPrice)
        {
            decimal? value = 0;
            decimal? webValue = 0;
            DateTime? trueDate = new DateTime?();
            DateTime? webDate = new DateTime?();
            decimal wrongDateVal = 0;
            valueDate = valueDate.Date;
            bool webPrice = false;
            bool done = false;
            bool storeValue = true;
            int wrongDateDaysOut = 0;

            if (this.ValueCalculationEvent != null)
            {
                ValueCalculationEvent(this, new ValueCalcEventArgs("Getting " + (buyPrice ? "buy" : "sell") + " value for: " + Items.GetItemName(itemID) + " in " + (regionID == -1 ? "All Regions" : Regions.GetRegionName(regionID)) + " on " + valueDate.Date.ToShortDateString()));
            }

            // If the price is being forced to default then just return that value.
            if (buyPrice && ForceDefaultBuyPriceGet(itemID))
            {
                value = DefaultBuyPriceGet(itemID);
                if (this.ValueCalculationEvent != null)
                {
                    ValueCalculationEvent(this, new ValueCalcEventArgs("Value forced to default: " + value.ToString()));
                }
            }
            if (!buyPrice && ForceDefaultSellPriceGet(itemID))
            {
                value = DefaultPriceGet(itemID);
                if (this.ValueCalculationEvent != null)
                {
                    ValueCalculationEvent(this, new ValueCalcEventArgs("Value forced to default: " + value.ToString()));
                }
            }


            if (value.Value != 0)
            {
                done = true;
            }
            else
            {
                lock (tableAdapter)
                {
                    if (this.ValueCalculationEvent != null)
                    {
                        ValueCalculationEvent(this, new ValueCalcEventArgs("Checking historical price data"));
                    }

                    // Start by trying to get a value from the value history table
                    value = 999999999999999.99m;

                    tableAdapter.ItemValueGet(valueDate, itemID, regionID, _reportGroupID, buyPrice, false,
                        ref value, ref trueDate);
                    if (((!value.HasValue || (value.HasValue && value.Value == 999999999999999.99m)) ||
                        (trueDate.HasValue && trueDate.Value.Date.CompareTo(valueDate.Date) != 0)))
                    {
                        if (this.ValueCalculationEvent != null)
                        {
                            ValueCalculationEvent(this, new ValueCalcEventArgs("No matching price entry found"));
                        }
                        if (value.HasValue && value.Value != 999999999999999.99m) 
                        {
                            if (this.ValueCalculationEvent != null)
                            {
                                ValueCalculationEvent(this, new ValueCalcEventArgs("Closest match is for " + trueDate.Value.ToShortDateString() + ", price: " + value.ToString()));
                            }
                            wrongDateVal = value.Value; 
                        }
                        if (UserAccount.CurrentGroup.Settings.UseEveCentral)
                        {
                            if (this.ValueCalculationEvent != null)
                            {
                                ValueCalculationEvent(this, new ValueCalcEventArgs("Checking web price history"));
                            }
                            // If we don't get a value for the exact day we want then try getting a value 
                            // from the web value history table
                            webValue = 999999999999999.99m;

                            tableAdapter.ItemValueGet(valueDate, itemID, regionID, _reportGroupID, buyPrice, true,
                                ref webValue, ref webDate);
                            if ((webValue.HasValue && webValue.Value != 999999999999999.99m) &&
                                (webDate.HasValue && webDate.Value.Date.CompareTo(valueDate.Date) == 0))
                            {
                                value = webValue;
                                done = true;
                                storeValue = false;
                                if (this.ValueCalculationEvent != null)
                                {
                                    ValueCalculationEvent(this, new ValueCalcEventArgs("Matching price entry found: " + value.ToString()));
                                }
                            }
                            else if ((webValue.HasValue && webValue.Value != 999999999999999.99m) &&
                                wrongDateVal == 0) 
                            { 
                                wrongDateVal = webValue.Value;
                                if (this.ValueCalculationEvent != null)
                                {
                                    ValueCalculationEvent(this, new ValueCalcEventArgs("Closest match is for " + webDate.Value.ToShortDateString() + ", price: " + webValue.ToString()));
                                }
                            }
                            else
                            {
                                if (this.ValueCalculationEvent != null)
                                {
                                    ValueCalculationEvent(this, new ValueCalcEventArgs("No matching price entry found"));
                                }
                            }
                        }
                    }
                    else
                    {
                        if (this.ValueCalculationEvent != null)
                        {
                            ValueCalculationEvent(this, new ValueCalcEventArgs("Matching price entry found: " + value.ToString()));
                        }
                        done = true;
                        storeValue = false;
                    }
                }

                if (!value.HasValue || (value.HasValue && value.Value == 999999999999999.99m)) 
                {
                    value = 0;
                    done = false;
                    storeValue = true;
                }

                // If we're looking for a value for today then do not use the value from the database
                // note that we still want to keep the 'wrongDateVal' as a backup if we found one though.
                if (valueDate.Date.CompareTo(DateTime.UtcNow.Date) == 0)
                {
                    if (this.ValueCalculationEvent != null && value.HasValue && value.Value != 0)
                    {
                        ValueCalculationEvent(this, new ValueCalcEventArgs("Since we're looking for a price for today, ignore the database value for now and just keep it as a backup incase we can't find anything else."));
                    }
                    if (wrongDateVal == 0 && value.HasValue) { wrongDateVal = value.Value; }
                    value = 0;
                    done = false;
                    storeValue = true;
                }
            }

            if (!done)
            {
                // If we couldn't find anything matching in the value history tables then we need to
                // calculate the value some other way...
                decimal tmpVal = 0;

                List<int> itemIDs = new List<int>();
                itemIDs.Add(itemID);
                List<int> regionIDs = new List<int>();
                regionIDs.Add(regionID == -1 ? 0 : regionID);
                if (buyPrice)
                {
                    Transactions.GetMedianBuyPrice(
                        UserAccount.CurrentGroup.GetFinanceAccessParams(APIDataType.Transactions),
                        itemIDs, regionIDs, valueDate.AddDays(-2), valueDate, ref tmpVal, false);
                    value = tmpVal;
                }
                else
                {
                    Transactions.GetMedianSellPrice(
                        UserAccount.CurrentGroup.GetFinanceAccessParams(APIDataType.Transactions),
                        itemIDs, regionIDs, valueDate.AddDays(-2), valueDate, ref tmpVal);
                    value = tmpVal;
                }
                if (this.ValueCalculationEvent != null)
                {
                    ValueCalculationEvent(this, new ValueCalcEventArgs("Median sell price based on your report group's transactions for the last 2 days: " + value.ToString()));
                }

                if (value.Value == 0)
                {
                    // If we've still not found anything then try going further back in time.
                    if (buyPrice)
                    {
                        Transactions.GetMedianBuyPrice(
                            UserAccount.CurrentGroup.GetFinanceAccessParams(APIDataType.Transactions),
                            itemIDs, regionIDs, valueDate.AddDays(-7), valueDate, ref tmpVal, false);
                        value = tmpVal;
                    }
                    else
                    {
                        Transactions.GetMedianSellPrice(
                            UserAccount.CurrentGroup.GetFinanceAccessParams(APIDataType.Transactions),
                            itemIDs, regionIDs, valueDate.AddDays(-7), valueDate, ref tmpVal);
                        value = tmpVal;
                    }
                    if (this.ValueCalculationEvent != null)
                    {
                        ValueCalculationEvent(this, new ValueCalcEventArgs("Median sell price based on your report group's transactions for the last 7 days: " + value.ToString()));
                    }


                    if (value.Value == 0)
                    {
                        EMMADataSet.ItemValuesRow itemData = table.FindByReportGroupIDItemIDRegionID(
                            _reportGroupID, itemID, regionID);
                        if (this.ValueCalculationEvent != null)
                        {
                            ValueCalculationEvent(this, new ValueCalcEventArgs("Checking default values"));
                        }

                        if (itemData != null)
                        {
                            if (!buyPrice)
                            {
                                value = itemData.DefaultSellPrice;
                            }
                            else
                            {
                                value = itemData.DefaultBuyPrice;
                            }
                        }

                        if (value == 0 && regionID != 0)
                        {
                            EMMADataSet.ItemValuesRow itemData2 = table.FindByReportGroupIDItemIDRegionID(
                                _reportGroupID, itemID, 0);

                            if (itemData != null)
                            {
                                if (!buyPrice)
                                {
                                    value = itemData.DefaultSellPrice;
                                }
                                else
                                {
                                    value = itemData.DefaultBuyPrice;
                                }
                            }
                        }

                        if (value.Value == 0)
                        {
                            if (this.ValueCalculationEvent != null)
                            {
                                ValueCalculationEvent(this, new ValueCalcEventArgs("No default values found"));
                            }
                        }

                        if (value.Value == 0 && (UserAccount.CurrentGroup.Settings.UseEveCentral || UserAccount.CurrentGroup.Settings.UseEveMetrics) &&
                            valueDate.CompareTo(DateTime.UtcNow.Date) == 0)
                        {
                            if (this.ValueCalculationEvent != null)
                            {
                                if (UserAccount.CurrentGroup.Settings.UseEveMetrics)
                                {
                                    ValueCalculationEvent(this, new ValueCalcEventArgs("Check eve-metrics.com or eve-prices.net"));
                                }
                                else // must be eve-central
                                {
                                    ValueCalculationEvent(this, new ValueCalcEventArgs("Check eve-central.com or eve-prices.net"));
                                }
                            }

                            // Make sure we don't grab the same data more than ever x days.
                            if ((buyPrice && CalculatedBuyPriceLastUpdateGet(itemID, regionID).CompareTo(
                                DateTime.UtcNow.AddDays(-1 * 
                                UserAccount.CurrentGroup.Settings.ItemValueWebExpiryDays)) < 0) ||
                                (!buyPrice && CalculatedSellPriceLastUpdateGet(itemID, regionID).CompareTo(
                                DateTime.UtcNow.AddDays(-1 *
                                UserAccount.CurrentGroup.Settings.ItemValueWebExpiryDays)) < 0))
                            {
                                // Need to get a price update from eve central or eve metrics, first make sure this is 
                                // actually a market item. 
                                // (Non-market items are not supported by Eve-Central or Eve-Metrics)
                                EveDataSet.invTypesRow eveItemData = Items.GetItem(itemID);
                                if (eveItemData != null)
                                {
                                    if (!eveItemData.IsmarketGroupIDNull())
                                    {
                                        // Note if we're trying to find the buy price of an item then we 
                                        // actually want to use the lowest sell order.
                                        // If we're looking for a sell price then we want to use the
                                        // highest buy order. Consequently, we reverse the buy/sell boolean.
                                        if (UserAccount.CurrentGroup.Settings.UseEveMetrics)
                                        {
                                            value = EveMetrics.GetPrice(itemID, regionID, !buyPrice);
                                            if (this.ValueCalculationEvent != null)
                                            {
                                                ValueCalculationEvent(this, new ValueCalcEventArgs("eve-metrics.com price: " + value.Value));
                                            }
                                        }
                                        else
                                        {
                                            value = EveCentral.GetPrice(itemID, regionID, !buyPrice);
                                            if (this.ValueCalculationEvent != null)
                                            {
                                                ValueCalculationEvent(this, new ValueCalcEventArgs("eve-central.com price: " + value.Value));
                                            }
                                        }
                                    }
                                    webPrice = true;

                                    // Update the last update times.
                                    if (buyPrice)
                                    {
                                        CalculatedBuyPriceLastUpdateSet(itemID, regionID, DateTime.UtcNow);
                                    }
                                    else
                                    {
                                        CalculatedSellPriceLastUpdateSet(itemID, regionID, DateTime.UtcNow);
                                    }
                                }
                            }
                            else
                            {
                                if (this.ValueCalculationEvent != null)
                                {
                                    ValueCalculationEvent(this, new ValueCalcEventArgs("We've already checked the web value for this item/region in the last 24 hours so don't try again."));
                                }
                                // If we've got a historic web value and are not allowed to check
                                // eve-central directly then just use the wrong date value since it 
                                // will be from less than 24 hours ago.
                                if (webValue.HasValue && wrongDateVal == webValue.Value)
                                {
                                    if (this.ValueCalculationEvent != null)
                                    {
                                        ValueCalculationEvent(this, new ValueCalcEventArgs("Use the historic web value we saved from earlier - " + wrongDateVal));
                                    }
                                    value = wrongDateVal;
                                }
                            }

                        }

                    }

                    if (value.Value == 0)
                    {
                        // Try just getting any buy or sell data for the item in the last three months.
                        // (We could try going further back but any value we got would be pretty useless)
                        if (buyPrice)
                        {
                            Transactions.GetMedianBuyPrice(
                                UserAccount.CurrentGroup.GetFinanceAccessParams(APIDataType.Transactions),
                                itemIDs, regionIDs, valueDate.AddMonths(-3), valueDate, ref tmpVal, false);
                            value = tmpVal;
                        }
                        else
                        {
                            Transactions.GetMedianSellPrice(
                                UserAccount.CurrentGroup.GetFinanceAccessParams(APIDataType.Transactions),
                                itemIDs, regionIDs, valueDate.AddMonths(-3), valueDate, ref tmpVal);
                            value = tmpVal;
                        }
                        if (this.ValueCalculationEvent != null)
                        {
                            ValueCalculationEvent(this, new ValueCalcEventArgs("Median sell price based on your report group's transactions for the last 3 months: " + value.ToString()));
                        }
                    }

                    if (value.Value == 0 && regionID > 0)
                    {
                        // If there is still nothing then try getting data for any region if we're
                        // not already doing so.
                        value = GetItemValue(itemID, -1, valueDate, buyPrice);
                    }
                }

                if (value.Value == 0 && wrongDateVal != 0)
                {
                    // If we couldn't get a value for the current data but we do have a value for a different
                    // date from the database then we may as well just use that one.
                    value = wrongDateVal;
                }
                if (value.Value == 0 && !buyPrice)
                {
                    // Finally, if we STILL have no value and we want a sell value then try using 
                    // a buy value instead.
                    // (Note this would not really work the other way around)
                    value = GetItemValue(itemID, regionID, valueDate, true);
                }

                if (value.Value == 0)
                {
                    storeValue = false;
                }
            }

            if (storeValue)
            {
                lock (tableAdapter)
                {
                    tableAdapter.ItemValueSet(valueDate, itemID, regionID, _reportGroupID,
                        buyPrice, webPrice, value);
                }
            }

            if (this.ValueCalculationEvent != null)
            {
                ValueCalculationEvent(this, new ValueCalcEventArgs("Final price stored in cache: " + value.ToString()));
            }

            return value.HasValue ? value.Value : 0;
        }

        #region Old Buy/Sell price cache updates
        /*
        void BuyPriceCache_DataUpdateNeeded(object myObject, DataUpdateNeededArgs<PriceCacheKey, decimal> args)
        {
            decimal value = 0;
            int itemID = args.Key.ItemID;
            int regionID = args.Key.RegionID;
            // First just try and get the data for the specified item in the specified region...
            EMMADataSet.ItemValuesRow itemData = table.FindByReportGroupIDItemIDRegionID(
                _reportGroupID, itemID, regionID);

            if (itemData == null)
            {
                // If we find nothing then try the 'all regions' setting for the same item...
                if (regionID != -1)
                {
                    itemData = table.FindByReportGroupIDItemIDRegionID(
                        _reportGroupID, itemID, -1);
                }
            }

            if (itemData != null)
            {
                // We've got some data from the database.
                // If possible we want to use a buy price calculated from this user's data
                // so try that first.
                if (itemData.LastBuyPriceCalc.AddDays(1).CompareTo(DateTime.UtcNow) > 0)
                {
                    value = itemData.CurrentBuyPrice;
                }
            }

            if (value == 0)
            {
                // If the last claculated price is out of date or we don't have one then 
                // try and calculate it now.
                decimal blank1 = 0;
                List<int> itemIDs = new List<int>();
                itemIDs.Add(itemID);
                List<int> regionIDs = new List<int>();
                regionIDs.Add(regionID == -1 ? 0 : regionID);
                Transactions.GetAverageBuyPrice(UserAccount.CurrentGroup.GetFinanceAccessParams(APIDataType.Transactions),
                    itemIDs, new List<int>(), regionIDs, 10, 0, ref value, ref blank1);

                CalculatedBuyPriceLastUpdateSet(itemID, regionID, DateTime.UtcNow);
                if (value != 0)
                {
                    // We've got a value. Set the data and return what we've found.
                    CalculatedBuyPriceSet(itemID, regionID, value);
                }
            }

            args.Data = value;
        }


        void SellPriceCache_DataUpdateNeeded(object myObject, DataUpdateNeededArgs<PriceCacheKey, decimal> args)
        {
            decimal value = 0;
            bool updateFromEveCentral = false;
            bool useEveCentral = UserAccount.CurrentGroup.Settings.UseEveCentral;
            int itemID = args.Key.ItemID;
            int regionID = args.Key.RegionID;
            // First just try and get the data for the specified item in the specified region...
            EMMADataSet.ItemValuesRow itemData = table.FindByReportGroupIDItemIDRegionID(
                _reportGroupID, itemID, regionID);

            if (itemData == null)
            {
                // If we find nothing then try the 'all regions' setting for the same item...
                if (regionID != -1)
                {
                    itemData = table.FindByReportGroupIDItemIDRegionID(
                        _reportGroupID, itemID, -1);
                }
            }

            if (itemData != null)
            {
                // We've got some data from the database.
                // If possible we want to use a sell price calculated from this user's sell data
                // so try that first.
                if (itemData.LastSellPriceCalc.AddDays(1).CompareTo(DateTime.UtcNow) > 0)
                {
                    value = itemData.CurrentSellPrice;
                }
            }

            if (value == 0)
            {
                // If the last claculated price is out of date or we don't have one then 
                // try and calculate it now.
                decimal medianSellPrice = 0;
                List<int> itemIDs = new List<int>();
                itemIDs.Add(itemID);
                List<int> regionIDs = new List<int>();
                regionIDs.Add(regionID == -1 ? 0 : regionID);
                DateTime date = DateTime.UtcNow;
                Transactions.GetMedianSellPrice(UserAccount.CurrentGroup.GetFinanceAccessParams(APIDataType.Transactions), itemIDs,
                    regionIDs, date.AddDays(-5), date, ref medianSellPrice);

                value = medianSellPrice;
                if (value != 0)
                {
                    // We've got a value. Set the data and return what we've found.
                    CalculatedSellPriceSet(itemID, regionID, value);
                    CalculatedSellPriceLastUpdateSet(itemID, regionID, DateTime.UtcNow);
                }
                else
                {
                    Transactions.GetMedianSellPrice(UserAccount.CurrentGroup.GetFinanceAccessParams(APIDataType.Transactions), itemIDs,
                        regionIDs, date.AddMonths(-1), date, ref medianSellPrice);

                    value = medianSellPrice;
                    if (value != 0)
                    {
                        // We've got a value. Set the data and return what we've found.
                        CalculatedSellPriceSet(itemID, regionID, value);
                    }
                    else if (itemData != null)
                    {
                        // We've got no sales data in range.
                        if (itemData.DefaultSellPrice != 0)
                        {
                            // Use the default sell price if one is specified.
                            value = itemData.DefaultSellPrice;
                        }
                        else
                        {
                            // Going to have to get an update from eve central.
                            if (itemData.LastSellPriceCalc.AddDays(1).CompareTo(DateTime.UtcNow) < 0)
                            {
                                updateFromEveCentral = true;
                            }
                        }
                    }
                    else
                    {
                        updateFromEveCentral = true;
                    }
                    CalculatedSellPriceLastUpdateSet(itemID, regionID, DateTime.UtcNow);
                }
            }            

            if (updateFromEveCentral && useEveCentral)
            {
                // Need to get a price update from eve central, first make sure this is actually a market
                // item. (Non-market items are not supported by Eve-Central)
                if (!Items.GetItem(itemID).IsmarketGroupIDNull())
                {
                    value = EveCentral.GetPrice(itemID, regionID);
                }
                else
                {
                    // Use 'Eve-Prices.net'
                    value = EvePrices.GetValue(itemID);
                }
                CalculatedSellPriceSet(itemID, regionID, value);
                CalculatedSellPriceLastUpdateSet(itemID, regionID, DateTime.UtcNow);
            }

            args.Data = value;
        }
        */
        #endregion

        public decimal GetReprocessValue(int itemID)
        {
            if (this.ValueCalculationEvent != null)
            {
                ValueCalculationEvent(this, new ValueCalcEventArgs("Calculating reprocess value of " + Items.GetItemName(itemID)));
            }

            ReprocessJob job = new ReprocessJob(0, 0, 0);
            job.AddItem(itemID, 1, 1000);
            job.UpdateResults();

            if (this.ValueCalculationEvent != null)
            {
                ValueCalculationEvent(this, new ValueCalcEventArgs("Reprocess value = " + job.TotalResultsValue));
            }

            return job.TotalResultsValue;
        }

        /// <summary>
        /// Get the estimated value of an item. 
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public decimal GetItemValue(int itemID)
        {
            return GetItemValue(itemID, -1, DateTime.UtcNow.Date);
        }
        /// <summary>
        /// Get the estimated value of an item. 
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="stationID"></param>
        /// <returns></returns>
        public decimal GetItemValue(int itemID, int stationID)
        {
            decimal retVal = 0;
            int regionID = Stations.GetStation(stationID).regionID;
            retVal = GetItemValue(itemID, regionID, DateTime.UtcNow.Date);
            return retVal;
        }
        /// <summary>
        /// Get the estimated value of an item. 
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="regionID"></param>
        /// <returns></returns>
        public decimal GetItemValue(int itemID, int regionID, bool nothing)
        {
            decimal retVal = 0;
            retVal = GetItemValue(itemID, regionID, DateTime.UtcNow.Date);
            return retVal;
        }
        /// <summary>
        /// Get the estimated value of an item. 
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public decimal GetItemValue(int itemID, DateTime date)
        {
            decimal retVal = 0;
            retVal = GetItemValue(itemID, -1, date.Date);
            return retVal;
        }
        /// <summary>
        /// Get the estimated value of an item. 
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="stationID"></param>
        /// <returns></returns>
        public decimal GetItemValue(int itemID, int regionID, DateTime date)
        {
            decimal retVal = 0;
            if (this.UseReprocessValGet(itemID))
            {
                // Temporarilly disable setting to make sure no infinite loops occur.
                this.UseReprocessValSet(itemID, false);
                retVal = GetReprocessValue(itemID);
                this.UseReprocessValSet(itemID, true);
            }
            else
            {
                if (regionID == 0) { regionID = -1; }
                retVal = _sellPriceCache.Get(new PriceCacheKey(itemID, regionID, date.Date));
                if (this.ValueCalculationEvent != null)
                {
                    ValueCalculationEvent(this, new ValueCalcEventArgs("Price retrieved from cache: " + retVal.ToString()));
                }
            }
            return retVal;
        }

        /// <summary> 
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="stationID"></param>
        /// <returns></returns>
        public decimal GetBuyPrice(int itemID, int stationID, bool nothing)
        {
            decimal retVal = 0;
            int regionID = Stations.GetStation(stationID).regionID;
            retVal = GetBuyPrice(itemID, regionID, DateTime.UtcNow.Date);
            return retVal;
        }

        /// <summary> 
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="stationID"></param>
        /// <returns></returns>
        public decimal GetBuyPrice(int itemID, int regionID)
        {
            decimal retVal = 0;
            retVal = GetBuyPrice(itemID, regionID, DateTime.UtcNow.Date);
            return retVal;
        }

        /// <summary> 
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="stationID"></param>
        /// <returns></returns>
        public decimal GetBuyPrice(int itemID, int regionID, DateTime date)
        {
            decimal retVal = 0;
            if (regionID == 0) { regionID = -1; }
            retVal = _buyPriceCache.Get(new PriceCacheKey(itemID, regionID, date.Date));
            if (this.ValueCalculationEvent != null)
            {
                ValueCalculationEvent(this, new ValueCalcEventArgs("Price retrieved from cache: " + retVal.ToString()));
            }
            return retVal;
        }

        public bool IsEmpty()
        {
            return table.Count == 0;
        }

        public EveDataSet.invTypesDataTable GetAllItems()
        {
            EveDataSet.invTypesDataTable retVal = new EveDataSet.invTypesDataTable();
            foreach (EMMADataSet.ItemValuesRow item in table)
            {
                EveDataSet.invTypesRow existing = retVal.FindBytypeID((short)item.ItemID);
                if (existing == null)
                {
                    retVal.ImportRow(Items.GetItem(item.ItemID));
                }
            }
            return retVal;
        }

        public List<int> GetAllItemIDs()
        {
            List<int> retVal = new List<int>();
            foreach (EMMADataSet.ItemValuesRow item in table)
            {
                if (!retVal.Contains(item.ItemID))
                {
                    retVal.Add(item.ItemID);
                }
            }
            return retVal;
        }

        public EMMADataSet.ItemValuesRow GetItem(int itemID, int stationID)
        {
            EMMADataSet.ItemValuesRow retVal;
            retVal = table.FindByReportGroupIDItemIDRegionID(_reportGroupID, itemID, stationID);
            if (retVal == null)
            {
                AddItem(itemID, stationID);
                retVal = table.FindByReportGroupIDItemIDRegionID(_reportGroupID, itemID, stationID);
            }
            return retVal;
        }

        public void Store()
        {
            try
            {
                lock (tableAdapter)
                {
                    tableAdapter.Update(table);
                }
            }
            catch (Exception ex)
            {
                throw new EMMADataException(ExceptionSeverity.Critical, "Unable to store changes to " +
                    "Traded Items table.", ex);
            }
            table.AcceptChanges();
        }

        public void CancelChanges()
        {
            table.RejectChanges();
        }

        public void AddItem(int itemID)
        {
            AddItem(itemID, -1);
        }
        private void AddItem(int itemID, int regionID)
        {
            EMMADataSet.ItemValuesRow item = table.FindByReportGroupIDItemIDRegionID(_reportGroupID, itemID, regionID);
            if (item == null)
            {
                item = table.NewItemValuesRow();
                item.ReportGroupID = _reportGroupID;
                item.ItemID = itemID;
                item.RegionID = regionID;
                item.LastSellPriceCalc = new DateTime(1990, 1, 1);
                item.DefaultSellPrice = 0;
                item.DefaultBuyPrice = 0;
                item.CurrentSellPrice = 0;
                item.CurrentBuyPrice = 0;
                item.LastBuyPriceCalc = new DateTime(1990, 1, 1);
                item.UseReprocessVal = false;
                item.ForceDefaultBuyPrice = false;
                item.ForceDefaultSellPrice = false;
                table.AddItemValuesRow(item);
            }
            else if (item.RowState == System.Data.DataRowState.Deleted)
            {
                item.RejectChanges();
            }
        }

        public void RemoveItem(int itemID)
        {
            EMMADataSet.ItemValuesDataTable toBeRemoved = new EMMADataSet.ItemValuesDataTable();
            tableAdapter.FillByID(toBeRemoved, _reportGroupID, 0, itemID);
            foreach (EMMADataSet.ItemValuesRow item in toBeRemoved)
            {
                table.FindByReportGroupIDItemIDRegionID(_reportGroupID, itemID, item.RegionID).Delete();
            }
        }

        public void ClearAllItems()
        {
            for (int i = 0; i < table.Count; i++)
            {
                EMMADataSet.ItemValuesRow item = table[i];
                item.Delete();
            }
        }

        public decimal DefaultPriceGet(int itemID)
        {
            decimal retVal = 0;
            int regionID = -1;
            EMMADataSet.ItemValuesRow tradedItem = table.FindByReportGroupIDItemIDRegionID(
                _reportGroupID, itemID, regionID);
            if (tradedItem != null) { retVal = tradedItem.DefaultSellPrice; }
            return retVal;
        }
        public void DefaultPriceSet(int itemID, decimal newPrice)
        {
            int regionID = -1; 
            EMMADataSet.ItemValuesRow tradedItem = table.FindByReportGroupIDItemIDRegionID(
                _reportGroupID, itemID, regionID);
            if (tradedItem == null)
            {
                AddItem(itemID, regionID);
                tradedItem = table.FindByReportGroupIDItemIDRegionID(
                    _reportGroupID, itemID, regionID);
            }

            tradedItem.DefaultSellPrice = newPrice;
        }

        public decimal DefaultBuyPriceGet(int itemID)
        {
            decimal retVal = 0;
            int regionID = -1;
            EMMADataSet.ItemValuesRow tradedItem = table.FindByReportGroupIDItemIDRegionID(
                _reportGroupID, itemID, regionID);
            if (tradedItem != null) { retVal = tradedItem.DefaultBuyPrice; }
            return retVal;
        }
        public void DefaultBuyPriceSet(int itemID, decimal newPrice)
        {
            int regionID = -1;
            EMMADataSet.ItemValuesRow tradedItem = table.FindByReportGroupIDItemIDRegionID(
                _reportGroupID, itemID, regionID);
            if (tradedItem == null)
            {
                AddItem(itemID, regionID);
                tradedItem = table.FindByReportGroupIDItemIDRegionID(
                    _reportGroupID, itemID, regionID);
            }

            tradedItem.DefaultBuyPrice = newPrice;
        }

        public decimal CalculatedSellPriceGet(int itemID, int regionID)
        {
            decimal retVal = 0;
            if (regionID == 0) { regionID = -1; }
            EMMADataSet.ItemValuesRow tradedItem = table.FindByReportGroupIDItemIDRegionID(
                _reportGroupID, itemID, regionID);
            if (tradedItem != null) { retVal = tradedItem.CurrentSellPrice; }
            return retVal;
        }
        public void CalculatedSellPriceSet(int itemID, int regionID, decimal newPrice)
        {
            if (regionID == 0) { regionID = -1; }
            EMMADataSet.ItemValuesRow tradedItem = table.FindByReportGroupIDItemIDRegionID(
                _reportGroupID, itemID, regionID);
            if (tradedItem == null)
            {
                AddItem(itemID, regionID);
                tradedItem = table.FindByReportGroupIDItemIDRegionID(
                    _reportGroupID, itemID, regionID);
            }
            tradedItem.CurrentSellPrice = newPrice;
        }

        public DateTime CalculatedSellPriceLastUpdateGet(int itemID, int regionID)
        {
            DateTime retVal= new DateTime(2000, 1, 1);
            if (regionID == 0) { regionID = -1; }
            EMMADataSet.ItemValuesRow tradedItem = table.FindByReportGroupIDItemIDRegionID(
                _reportGroupID, itemID, regionID);
            if (tradedItem != null) { retVal = tradedItem.LastSellPriceCalc; }
            return retVal;
        }
        public void CalculatedSellPriceLastUpdateSet(int itemID, int regionID, DateTime newDateTime)
        {
            if (regionID == 0) { regionID = -1; }
            EMMADataSet.ItemValuesRow tradedItem = table.FindByReportGroupIDItemIDRegionID(
                _reportGroupID, itemID, regionID);
            if (tradedItem == null)
            {
                AddItem(itemID, regionID);
                tradedItem = table.FindByReportGroupIDItemIDRegionID(
                    _reportGroupID, itemID, regionID);
            }
            tradedItem.LastSellPriceCalc = newDateTime;
        }

        public decimal CalculatedBuyPriceGet(int itemID, int regionID)
        {
            decimal retVal = 0;
            if (regionID == 0) { regionID = -1; }
            EMMADataSet.ItemValuesRow tradedItem = table.FindByReportGroupIDItemIDRegionID(
                _reportGroupID, itemID, regionID);
            if (tradedItem != null) { retVal = tradedItem.CurrentBuyPrice; }
            return retVal;
        }
        public void CalculatedBuyPriceSet(int itemID, int regionID, decimal newPrice)
        {
            if (regionID == 0) { regionID = -1; }
            EMMADataSet.ItemValuesRow tradedItem = table.FindByReportGroupIDItemIDRegionID(
                _reportGroupID, itemID, regionID);
            if (tradedItem == null)
            {
                AddItem(itemID, regionID);
                tradedItem = table.FindByReportGroupIDItemIDRegionID(
                    _reportGroupID, itemID, regionID);
            }
            tradedItem.CurrentBuyPrice = newPrice;
        }

        public DateTime CalculatedBuyPriceLastUpdateGet(int itemID, int regionID)
        {
            DateTime retVal = new DateTime(2000, 1, 1);
            if (regionID == 0) { regionID = -1; }
            EMMADataSet.ItemValuesRow tradedItem = table.FindByReportGroupIDItemIDRegionID(
                _reportGroupID, itemID, regionID);
            if (tradedItem != null) { retVal = tradedItem.LastBuyPriceCalc; }
            return retVal;
        }
        public void CalculatedBuyPriceLastUpdateSet(int itemID, int regionID, DateTime newDateTime)
        {
            if (regionID == 0) { regionID = -1; }
            EMMADataSet.ItemValuesRow tradedItem = table.FindByReportGroupIDItemIDRegionID(
                _reportGroupID, itemID, regionID);
            if (tradedItem == null)
            {
                AddItem(itemID, regionID);
                tradedItem = table.FindByReportGroupIDItemIDRegionID(
                    _reportGroupID, itemID, regionID);
            }
            tradedItem.LastBuyPriceCalc = newDateTime;
        }

        public bool UseReprocessValGet(int itemID)
        {
            bool retVal = false;
            EMMADataSet.ItemValuesRow tradedItem = table.FindByReportGroupIDItemIDRegionID(
                _reportGroupID, itemID, -1);
            if (tradedItem != null) { retVal = tradedItem.UseReprocessVal; }
            return retVal;
        }
        public void UseReprocessValSet(int itemID, bool setting)
        {
            EMMADataSet.ItemValuesRow tradedItem = table.FindByReportGroupIDItemIDRegionID(
                _reportGroupID, itemID, -1);
            if (tradedItem == null)
            {
                AddItem(itemID, -1);
                tradedItem = table.FindByReportGroupIDItemIDRegionID(
                    _reportGroupID, itemID, -1);
            }

            tradedItem.UseReprocessVal = setting;
        }

        public bool ForceDefaultSellPriceGet(int itemID)
        {
            bool retVal = false;
            EMMADataSet.ItemValuesRow tradedItem = table.FindByReportGroupIDItemIDRegionID(
                _reportGroupID, itemID, -1);
            if (tradedItem != null) { retVal = tradedItem.ForceDefaultSellPrice; }
            return retVal;
        }
        public void ForceDefaultSellPriceSet(int itemID, bool setting)
        {
            EMMADataSet.ItemValuesRow tradedItem = table.FindByReportGroupIDItemIDRegionID(
                _reportGroupID, itemID, -1);
            if (tradedItem == null)
            {
                AddItem(itemID, -1);
                tradedItem = table.FindByReportGroupIDItemIDRegionID(
                    _reportGroupID, itemID, -1);
            }

            tradedItem.ForceDefaultSellPrice = setting;
        }
        public bool ForceDefaultBuyPriceGet(int itemID)
        {
            bool retVal = false;
            EMMADataSet.ItemValuesRow tradedItem = table.FindByReportGroupIDItemIDRegionID(
                _reportGroupID, itemID, -1);
            if (tradedItem != null) { retVal = tradedItem.ForceDefaultBuyPrice; }
            return retVal;
        }
        public void ForceDefaultBuyPriceSet(int itemID, bool setting)
        {
            EMMADataSet.ItemValuesRow tradedItem = table.FindByReportGroupIDItemIDRegionID(
                _reportGroupID, itemID, -1);
            if (tradedItem == null)
            {
                AddItem(itemID, -1);
                tradedItem = table.FindByReportGroupIDItemIDRegionID(
                    _reportGroupID, itemID, -1);
            }

            tradedItem.ForceDefaultBuyPrice = setting;
        }
       
        private class PriceCacheKey
        {
            private int _itemID;
            private int _regionID;
            private DateTime _valueDate;

            //public PriceCacheKey(int itemID, int regionID)
            public PriceCacheKey(int itemID, int regionID, DateTime valueDate)
            {
                _itemID = itemID;
                _regionID = regionID;
                _valueDate = valueDate;
            }

            public int ItemID
            {
                get { return _itemID; }
                set { _itemID = value; }
            }

            public int RegionID
            {
                get { return _regionID; }
                set { _regionID = value; }
            }

            public DateTime ValueDate
            {
                get { return _valueDate; }
                set { _valueDate = value; }
            }

            public override bool Equals(object obj)
            {
                bool retVal = false;
                PriceCacheKey other = obj as PriceCacheKey;
                if (other != null)
                {
                    //retVal = other.ItemID == _itemID && other.RegionID == _regionID 
                    retVal = other.ItemID == _itemID && other.RegionID == _regionID && 
                        other.ValueDate.CompareTo(_valueDate) == 0;
                }
                return retVal;
            }

            public override string ToString()
            {
                //return _itemID.ToString() + _regionID.ToString();
                return _itemID.ToString() + _regionID.ToString() + _valueDate.ToString();
            }

            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }
        }
    }

    public class ValueCalcEventArgs : EventArgs
    {
        private string _message;

        public ValueCalcEventArgs(string message)
        {
            _message = message;
        }

        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

    }
}
