using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading;

using EveMarketMonitorApp.AbstractionClasses;
using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public class AutoContractor : IProvideStatus
    {
        public event StatusChangeHandler StatusChange;

        public ContractList GenerateContracts(int assetOwnerID)
        {
            ContractList retVal = new ContractList();

            try
            {
                Diagnostics.ResetAllTimers();
                bool corp = false;
                APICharacter character = UserAccount.CurrentGroup.GetCharacter(assetOwnerID, ref corp);

                // Get a list of assets for this char/corp that are enabled for auto contracting.
                // the list is sorted by locationID
                bool exclude = UserAccount.CurrentGroup.Settings.AutoCon_ExcludeContainers;
                EMMADataSet.AssetsDataTable assets = Assets.GetAutoConAssets(character.CharID, corp,
                    UserAccount.CurrentGroup.Settings.AutoCon_PickupLocations, exclude);
                int stationID = 0;
                decimal collateralTotal = 0;
                decimal volumeTotal = 0;
                int counter = 0;
                //long nextID = long.MaxValue;

                ItemValues itemsTraded = UserAccount.CurrentGroup.ItemValues;
                decimal collateralPerc = UserAccount.CurrentGroup.Settings.CollateralPercentage;
                decimal minCollateral = UserAccount.CurrentGroup.Settings.AutoCon_MinCollateral * 0.95m;
                decimal minReward = UserAccount.CurrentGroup.Settings.AutoCon_MinReward * 0.95m;
                decimal minVolume = UserAccount.CurrentGroup.Settings.AutoCon_MinVolume;
                int destination = UserAccount.CurrentGroup.Settings.AutoCon_DestiantionStation;

                Diagnostics.StartTimer("AutoConGenerate");
                Diagnostics.StartTimer("AutoConGenerate.Station");
                TimeSpan longest = new TimeSpan();
                string longestName = "";
                UpdateStatus(counter, assets.Count, "Processing Assets..", "", false);
                foreach (EMMADataSet.AssetsRow asset in assets)
                {
                    // Make sure we're dealing with assets inside a station.
                    if (asset.LocationID >= 60000000 && asset.LocationID < 70000000 &&
                        asset.LocationID != destination)
                    {
                        if (stationID != asset.LocationID && stationID != 0)
                        {
                            UpdateStatus(counter, assets.Count, "", Stations.GetStationName(asset.LocationID), false);
                            // When we get to the next station in the list, check if it will be
                            // excluded, then generate the contract if it wont be.
                            if (collateralTotal * (collateralPerc / 100) > minCollateral &&
                                collateralTotal * 0.1m > minReward &&
                                volumeTotal > minVolume)
                            {

                                DiagnosticUpdate("", "Station accepted");
                                DiagnosticUpdate("", "Approx collateral = " +
                                    (collateralTotal * (collateralPerc / 100)));
                                DiagnosticUpdate("", "Approx reward = " + (collateralTotal * 0.1m));
                                DiagnosticUpdate("", "Volume = " + volumeTotal);
                                ContractList newContracts = GenerateContracts(assetOwnerID, stationID, destination, true);
                                foreach (Contract newContract in newContracts)
                                {
                                    //newContract.ID = nextID;
                                    retVal.Add(newContract);
                                    //nextID--;
                                }
                            }
                            else
                            {
                                DiagnosticUpdate("", "Station rejected");
                                DiagnosticUpdate("", "Approx collateral = " +
                                    (collateralTotal * (collateralPerc / 100)));
                                DiagnosticUpdate("", "Approx reward = " + (collateralTotal * 0.1m));
                                DiagnosticUpdate("", "Volume = " + volumeTotal);
                            }
                            collateralTotal = 0;
                            volumeTotal = 0;
                            Diagnostics.StopTimer("AutoConGenerate.Station");
                            TimeSpan timetaken = Diagnostics.GetRunningTime("AutoConGenerate.Station");
                            DiagnosticUpdate("", "Total station time taken: " + timetaken.ToString());
                            if (timetaken.CompareTo(longest) > 0)
                            {
                                longest = timetaken;
                                longestName = Stations.GetStationName(stationID);
                            }
                            Diagnostics.ResetTimer("AutoConGenerate.Station");
                            Diagnostics.StartTimer("AutoConGenerate.Station");
                        }

                        // increase the rough totals counters for each asset at this station.
                        collateralTotal += itemsTraded.GetItemValue(asset.ItemID) * asset.Quantity;
                        volumeTotal += (decimal)(asset.Quantity * Items.GetItemVolume(asset.ItemID));
                        stationID = asset.LocationID;
                    }
                    counter++;
                    UpdateStatus(counter, assets.Count, "", "", false);
                }

                // Do this one last time for the final station...
                if (collateralTotal * (collateralPerc / 100) > minCollateral &&
                    collateralTotal * 0.1m > minReward &&
                    volumeTotal > minVolume)
                {
                    ContractList newContracts = GenerateContracts(assetOwnerID, stationID, destination, true);
                    foreach (Contract newContract in newContracts)
                    {
                        //newContract.ID = nextID;
                        retVal.Add(newContract);
                        //nextID--;
                    }
                }

                Diagnostics.StopTimer("AutoConGenerate");

                DiagnosticUpdate("", "Total time taken: " + Diagnostics.GetRunningTime("AutoConGenerate"));
                DiagnosticUpdate("", "Longest time taken: " + longestName + " " + longest.ToString());

                UpdateStatus(-1, -1, "Complete", "", true);
            }
            catch (ThreadAbortException)
            {
                // User has closed the progress dialog so just allow the execution to fall out of this loop.
            }
            catch (EMMAException)
            {
                // Occurs when an infinite loop is caught during execution, this will have already
                // generated a log entry and updated the user so just fall out of this method.
            }
            catch (Exception ex)
            {
                UpdateStatus(-1, -1, "Error", ex.Message, true);
                new EMMAException(ExceptionSeverity.Error, "Exception occured while running auto-contractor", ex);
            }

            return retVal;
        }


        public ContractList GenerateContracts(int ownerID, int pickupStation, int destinationStation, bool autoCon)
        {
            ContractList retVal = new ContractList();
            ContractItemList items = new ContractItemList();
            Dictionary<int, long> assetsUsed = new Dictionary<int, long>();
            decimal totVolume = 0, totCollateral = 0, totProfit = 0;
            ReportGroupSettings settings = UserAccount.CurrentGroup.Settings;
            decimal maxVolume = settings.AutoCon_MaxVolume;
            decimal maxCollateral = settings.AutoCon_MaxCollateral;
            bool allowStackSpliting = settings.AutoCon_AllowStackSplitting;
            string collateralBasedOn = settings.CollateralBasedOn;
            decimal collateralPerc = settings.CollateralPercentage;
            string rewardBasedOn = settings.RewardBasedOn;
            decimal minReward = settings.MinReward;
            decimal minRewardPerc = settings.MinRewardPercentage;
            decimal minAllowedCollateral = settings.AutoCon_MinCollateral;
            decimal minAllowedReward = settings.AutoCon_MinReward;
            decimal minAllowedVolume = settings.AutoCon_MinVolume;
            decimal maxReward = settings.MaxReward;
            decimal maxRewardPerc = settings.MaxRewardPercentage;
            decimal lowSecRewardBonus = settings.LowSecPickupBonusPerc;
            decimal rewardPerJump = settings.RewardPercPerJump;
            decimal rewardPerVol = settings.VolumeBasedRewardPerc;
            
            EveDataSet.mapSolarSystemsRow pickupSystemData = SolarSystems.GetSystem(
                Stations.GetStation(pickupStation).solarSystemID);

            int diag_contracts = 0;

            bool corp = false;
            bool complete = true;
            bool relax = false;
            APICharacter character = UserAccount.CurrentGroup.GetCharacter(ownerID, ref corp);
            // Get the list of assets available for auto-contracting at this station.
            Diagnostics.StartTimer("GenerateContract");
            Diagnostics.StartTimer("GenerateContract.Part1");
            EMMADataSet.AssetsDataTable assets = Assets.GetAutoConAssets(
                character.CharID, corp, pickupStation, true);
            Diagnostics.StopTimer("GenerateContract.Part1");
            int lastAssetCount = -1;

            while (assets.Count > 0 && lastAssetCount != assets.Count)
            {
                complete = true;
                relax = false;
                totVolume = 0;
                totProfit = 0;
                totCollateral = 0;
                assetsUsed = new Dictionary<int, long>();
                items = new ContractItemList();
                Contract contract = new Contract(ownerID, 5, pickupStation, destinationStation, 0,
                    0, 0, DateTime.UtcNow, new ContractItemList(), ContractType.Courier);
                lastAssetCount = assets.Count;

                foreach (EMMADataSet.AssetsRow asset in assets)
                {
                    decimal buyPrice = 0, sellPrice = 0, unitCollateral = 0;
                    int itemID = asset.ItemID;
                    decimal itemVolume = (decimal)Items.GetItemVolume(itemID);
                    long quantity = asset.Quantity;

                    // Check if we can use the full volume of these items without breaching the max volume
                    // limit.
                    if (autoCon && totVolume + (decimal)(quantity * itemVolume) > maxVolume && 
                        totVolume < maxVolume)
                    {
                        // Reduce the quantity of items being addded if they would exceed the 
                        // volume limit
                        quantity = (int)Math.Round((maxVolume - totVolume) / (decimal)itemVolume,
                            0, MidpointRounding.AwayFromZero) - 1;
                        // This check will prevent infinite loops when the volume of a single item is greater
                        // than the max allowed contract volume.
                        if (maxVolume < itemVolume && totVolume == 0) { relax = true; quantity = 1; }
                        complete = false;
                    }
                    else if (totVolume >= maxVolume) { complete = false; }

                    Diagnostics.StartTimer("GenerateContract.Part2");
                    // Get estimated sell price for the items being added to the contract.
                    sellPrice = GetSellPrice(itemID, destinationStation);
                    Diagnostics.StopTimer("GenerateContract.Part2");
                    Diagnostics.StartTimer("GenerateContract.Part6");
                    // Get average buy price for the items being added to the contract.
                    buyPrice = GetBuyPrice(ownerID, itemID, pickupStation, quantity, asset.Quantity - quantity);
                    Diagnostics.StopTimer("GenerateContract.Part6");
                    Diagnostics.StartTimer("GenerateContract.Part3");
                    // calculate the collateral value of the items being added.
                    unitCollateral = CalcCollateral(collateralBasedOn, collateralPerc, buyPrice, sellPrice);

                    long lastQuantity = quantity;
                    while (autoCon && totCollateral + (unitCollateral * quantity) > maxCollateral && !relax &&
                        totCollateral < maxCollateral)
                    {
                        // Reduce the quantity of items being addded if they would exceed the 
                        // collateral limit
                        quantity = (int)Math.Round((maxCollateral - totCollateral) / (decimal)unitCollateral,
                            MidpointRounding.AwayFromZero) - 1;
                        // This check will prevent infinite loops when the collateral for a single item is greater
                        // than the max allowed collateral.
                        if (maxCollateral < unitCollateral && totCollateral == 0) { relax = true; quantity = 1; }
                        complete = false;

                        // Catch a possible infinite loop.
                        if (lastQuantity == quantity) { relax = true; quantity = 1; }

                        lastQuantity = quantity;
                        // Recalculate buy price and collateral.
                        //buyPrice = GetBuyPrice(ownerID, itemID, pickupStation, quantity, asset.Quantity - quantity);
                        //unitCollateral = CalcCollateral(collateralBasedOn, collateralPerc, buyPrice, sellPrice);
                    }
                    if (totCollateral >= maxCollateral) { complete = false; }
                    Diagnostics.StopTimer("GenerateContract.Part3");

                    if ((quantity == asset.Quantity || allowStackSpliting || relax) && quantity > 0)
                    {
                        // Get up the values for the items being added and add them to the contract totals.
                        totVolume += (decimal)(quantity * itemVolume);
                        totCollateral += quantity * unitCollateral;
                        totProfit += quantity * sellPrice - quantity * buyPrice;
                        if (assetsUsed.ContainsKey(itemID))
                        {
                            // If we've already added an item with this id then just add the new quantity
                            // to the old contract item object...
                            assetsUsed[itemID] += quantity;
                            for (int i = 0; i < items.Count;i++ )
                            {
                                ContractItem itemData = items[i];
                                if (itemData.ItemID == itemID)
                                {
                                    itemData.Quantity += (int)quantity;
                                    i = items.Count;
                                }
                            }
                        }
                        else
                        {
                            // ...Otherwise create a new contract item and add it to the list.
                            assetsUsed.Add(itemID, quantity);
                            items.Add(new ContractItem(itemID, (int)quantity, sellPrice, buyPrice, contract));
                        }
                    }
                }

                Diagnostics.StartTimer("GenerateContract.Part4");
                // Get the distance between the pickup and destination stations
                int numJumps = SolarSystemDistances.GetDistanceBetweenStations(pickupStation, destinationStation);
                Diagnostics.StopTimer("GenerateContract.Part4");
                // Calculate the reward that will be given from completing the contract.
                decimal reward = CalcReward(rewardBasedOn, totCollateral, totProfit, minReward, maxReward, 
                    minRewardPerc, maxRewardPerc, rewardPerJump, rewardPerVol, lowSecRewardBonus, 
                    numJumps, totVolume, Stations.IsLowSec(pickupStation));
                
                contract.Items = items;
                contract.Reward = reward;
                contract.Collateral = totCollateral;
                contract.ExpectedProfit = totProfit;
                diag_contracts++;
                // Add the contract to the list
                if (!autoCon || (reward > minAllowedReward && totCollateral > minAllowedCollateral &&
                    totVolume > minAllowedVolume))
                {
                    retVal.Add(contract);
                }

                Diagnostics.StartTimer("GenerateContract.Part5");
                if (!complete)
                {
                    // If we have not used all items at this location then
                    // remove the items  we have used from the list of current assets.
                    Dictionary<int, long>.Enumerator enumerator = assetsUsed.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        DataRow[] rows = assets.Select("ItemID = " + enumerator.Current.Key);
                        if (rows.Length > 0)
                        {
                            long q_remain = enumerator.Current.Value;
                            for (int i = 0; i < rows.Length; i++)
                            {
                                if (q_remain > 0)
                                {
                                    EMMADataSet.AssetsRow assetData = rows[i] as EMMADataSet.AssetsRow;
                                    long diff = q_remain - assetData.Quantity;
                                    if (diff >= 0)
                                    {
                                        q_remain -= assetData.Quantity;
                                        assets.RemoveAssetsRow(assetData);
                                    }
                                    else
                                    {
                                        assetData.Quantity -= (int)q_remain;
                                        q_remain = 0;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    // otherwise, just clear the list of assets.
                    assets.Clear();
                }
                Diagnostics.StopTimer("GenerateContract.Part5");
            }

            #region Diagnostic dump if we've caught an infinite loop.
            if (assets.Count != 0)
            {
                UpdateStatus(0, 0, "Error: Infinite loop detected",
                    "Please send your 'logging/exceptionlog.txt' file to ambo.emma@googlemail.com. " +
                    "Warning, this file will contain information about your in-game assets.", true);
                StringBuilder errorText = new StringBuilder(
                    "Infinite loop detected during auto-contrator execution, diagnostics to follow:");
                errorText.Append("\r\n\t");
                errorText.Append("Assets Remaining:");
                foreach (EMMADataSet.AssetsRow asset in assets)
                {
                    errorText.Append("\r\n\t\t");
                    errorText.Append(Items.GetItemName(asset.ItemID).Replace(",", " "));
                    errorText.Append(",");
                    errorText.Append(asset.Quantity);
                    errorText.Append(",");
                    errorText.Append(Items.GetItemVolume(asset.ItemID));
                }
                errorText.Append("\r\n\t");
                errorText.Append("Contracts created:");
                foreach (Contract contract in retVal)
                {
                    errorText.Append("\r\n\t\t");
                    errorText.Append("ID: ");
                    errorText.Append(contract.ID);
                    errorText.Append("\r\n\t\t\t");
                    errorText.Append("Collateral: ");
                    errorText.Append(contract.Collateral);
                    errorText.Append("\r\n\t\t\t");
                    errorText.Append("Reward: ");
                    errorText.Append(contract.Reward);
                    errorText.Append("\r\n\t\t\t");
                    errorText.Append("Total Volume: ");
                    errorText.Append(contract.TotalVolume);
                    errorText.Append("\r\n\t\t\t");
                    errorText.Append("Profit: ");
                    errorText.Append(contract.ExpectedProfit);
                    errorText.Append("\r\n\t\t\t");
                    errorText.Append("Contract Items:");
                    foreach (ContractItem item in contract.Items)
                    {
                        errorText.Append("\r\n\t\t\t\t");
                        errorText.Append(item.Item);
                        errorText.Append(",");
                        errorText.Append(item.Quantity);
                        errorText.Append(",");
                        errorText.Append(item.ItemVolume);
                    }
                }
                errorText.Append("\r\n\t");
                errorText.Append("Max Volume: ");
                errorText.Append(maxVolume);
                errorText.Append("\r\n\t");
                errorText.Append("Max Collateral: ");
                errorText.Append(maxCollateral);
                errorText.Append("\r\n\t");
                errorText.Append("Max Reward: ");
                errorText.Append(maxReward);
                errorText.Append("\r\n\t");
                errorText.Append("Max Reward Percentage: ");
                errorText.Append(maxRewardPerc);
                errorText.Append("\r\n\t");
                errorText.Append("Min Allowed Collateral: ");
                errorText.Append(minAllowedCollateral);
                errorText.Append("\r\n\t");
                errorText.Append("Min Allowed Reward: ");
                errorText.Append(minAllowedReward);
                errorText.Append("\r\n\t");
                errorText.Append("Min Allowed Volume: ");
                errorText.Append(minAllowedVolume);
                errorText.Append("\r\n\t");
                errorText.Append("Min Reward: ");
                errorText.Append(minReward);
                errorText.Append("\r\n\t");
                errorText.Append("Min Reward Percentage: ");
                errorText.Append(minRewardPerc);
                errorText.Append("\r\n\t");
                errorText.Append("Allow stack splitting: ");
                errorText.Append(allowStackSpliting);

                throw new EMMAException(ExceptionSeverity.Error, errorText.ToString());
            }
            #endregion

            DiagnosticUpdate("", "Complete time: " + Diagnostics.GetRunningTime("GenerateContract").ToString());
            DiagnosticUpdate("", "Time loading assets: " + 
                Diagnostics.GetRunningTime("GenerateContract.Part1").ToString());
            DiagnosticUpdate("", "Get item sell prices: " + 
                Diagnostics.GetRunningTime("GenerateContract.Part2").ToString());
            DiagnosticUpdate("", "Get item buy prices: " + 
                Diagnostics.GetRunningTime("GenerateContract.Part6").ToString());
            DiagnosticUpdate("", "Calc and restrict collateral: " + 
                Diagnostics.GetRunningTime("GenerateContract.Part3").ToString());
            DiagnosticUpdate("", "Get route length: " + 
                Diagnostics.GetRunningTime("GenerateContract.Part4").ToString());
            DiagnosticUpdate("", "Remove used assets: " + 
                Diagnostics.GetRunningTime("GenerateContract.Part5").ToString());
            DiagnosticUpdate("", "Total contracts generated: " + diag_contracts);

            return retVal;
        }


        public static decimal GetSellPrice(int itemID, int destinationStation)
        {
            decimal sellPrice = UserAccount.CurrentGroup.ItemValues.GetItemValue(itemID, destinationStation);
            return sellPrice;
        }

        public static decimal GetBuyPrice(int ownerID, int itemID, int stationID,
            long quantity, long recentPurchasesToIgnore)
        {
            decimal retVal = 0, blank1 = 0;
            List<FinanceAccessParams> financeAccessParams = new List<FinanceAccessParams>();
            financeAccessParams.Add(new FinanceAccessParams(ownerID));
            List<int> itemIDs = new List<int>();
            itemIDs.Add(itemID);
            List<int> stationIDs = new List<int>();
            stationIDs.Add(stationID);

            if (!UserAccount.CurrentGroup.ItemValues.UseReprocessValGet(itemID) && 
                !UserAccount.CurrentGroup.ItemValues.ForceDefaultBuyPriceGet(itemID))
            {
                Transactions.GetAverageBuyPrice(financeAccessParams, itemIDs, stationIDs, new List<int>(), quantity,
                    recentPurchasesToIgnore, ref retVal, ref blank1, true);
            }

            if (retVal == 0 || UserAccount.CurrentGroup.ItemValues.ForceDefaultBuyPriceGet(itemID))
            {
                retVal = UserAccount.CurrentGroup.ItemValues.GetBuyPrice(itemID, 
                    Stations.GetStation(stationID).regionID);
            }
            //if (retVal == 0)
            //{
            //    Transactions.GetAverageBuyPrice(financeAccessParams, itemIDs, new List<int>(), new List<int>(),
            //        quantity, 0, ref retVal, ref blank1, true);
            //}
            return retVal;
        }    

        public static decimal CalcCollateral(string collateralBasedOn, decimal percentage,
            decimal purchasePrice, decimal salePrice)
        {
            decimal retVal = 0;
            if (collateralBasedOn.Equals("Buy"))
            {
                retVal = purchasePrice * (percentage / 100.0m);
            }
            else
            {
                retVal = salePrice * (percentage / 100.0m);
            }
            return retVal;
        }

        public static decimal CalcReward(string rewardBasedOn, decimal collateral, decimal profit,
            decimal minReward, decimal maxReward, decimal minPercentage, decimal maxPercentage, 
            decimal percentagePerJump, decimal percentagePerVol, decimal percentageLowSecBonus,
            int jumps, decimal volume, bool lowSecPickup)
        {
            decimal retVal = 0;
            decimal percentage = 0;
            if (rewardBasedOn.Equals("Profit"))
            {
                retVal = profit;
            }
            else
            {
                retVal = collateral;
            }
            percentage = jumps * percentagePerJump;
            percentage += volume * (percentagePerVol / 100.0m);
            percentage += lowSecPickup ? percentageLowSecBonus : 0;
            if (percentage < minPercentage && minPercentage != 0) { percentage = minPercentage; }
            if (percentage > maxPercentage && maxPercentage != 0) { percentage = maxPercentage; }
            retVal *= (percentage / 100.0m);
            if (retVal < minReward && minReward != 0) { retVal = minReward; }
            if (retVal > maxReward && maxReward != 0) { retVal = maxReward; }
            return retVal;
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
    }

}
