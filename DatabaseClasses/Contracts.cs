using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.AbstractionClasses;

namespace EveMarketMonitorApp.DatabaseClasses
{
    class Contracts : IProvideStatus
    {
        private static EMMADataSetTableAdapters.ContractsTableAdapter contractsTableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.ContractsTableAdapter();
        private static EMMADataSetTableAdapters.ContractItemTableAdapter contractItemsTableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.ContractItemTableAdapter();
        private static EMMADataSetTableAdapters.IDTableTableAdapter IDTableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.IDTableTableAdapter();

        private static Cache<long, Contract> _cache = new Cache<long, Contract>(1000);
        private static  bool _initalised = false;

        public event StatusChangeHandler StatusChange;

        public static decimal GetTransportCosts(int itemID, int destinationStationID,
            long quantity, DateTime oldestDate)
        {
            decimal retVal = 0;
            if (!_initalised) { InitCache(); }
            if (quantity > 0)
            {
                long quantityRemaining = quantity;
                EMMADataSet.ContractsDataTable contracts = new EMMADataSet.ContractsDataTable();

                contractsTableAdapter.FillByItem(contracts, itemID, destinationStationID, oldestDate, 
                    (short)ContractType.Courier);
                for (int i = 0; i < contracts.Count; i++)
                {
                    long quantityInContract = 0;
                    Contract contract = _cache.Get(contracts[i].ID);
                    decimal cost = contract.CostOfTransport(itemID, out quantityInContract);
                    if (quantityRemaining - quantityInContract <= 0)
                    {
                        cost /= quantityInContract;
                        cost *= quantityRemaining;
                        quantityInContract = quantityRemaining;
                        i = contracts.Count;
                    }
                    quantityRemaining -= quantityInContract;
                    retVal += cost;
                }
            }
            return retVal;
        }

        private static void InitCache()
        {
            if (!_initalised)
            {
                _cache.DataUpdateNeeded += new Cache<long, Contract>.DataUpdateNeededHandler(Cache_DataUpdateNeeded);
            }
        }

        static void Cache_DataUpdateNeeded(object myObject, DataUpdateNeededArgs<long, Contract> args)
        {
            EMMADataSet.ContractsDataTable contracts = new EMMADataSet.ContractsDataTable();
            contractsTableAdapter.FillByID(contracts, args.Key);
            if (contracts.Count > 0)
            {
                args.Data = new Contract(contracts[0]);
            }
        }

        public static void CompleteContract(Contract contract)
        {
            EMMADataSet.ContractsDataTable contracts = new EMMADataSet.ContractsDataTable();
            contractsTableAdapter.FillByID(contracts, contract.ID);
            ContractItemList items = GetContractItems(contract);
            bool corp = false;
            UserAccount.CurrentGroup.GetCharacter(contract.OwnerID, ref corp);

            foreach (ContractItem item in items)
            {
                // When removing assets from a stack, cost is not affected so just use zero..
                Assets.ChangeAssets(contract.OwnerID, contract.DestinationStationID, item.ItemID, 
                    0, 2, false, -1 * item.Quantity, 0, false);
                Assets.ChangeAssets(contract.OwnerID, contract.DestinationStationID, item.ItemID,
                    0, 1, false, item.Quantity, item.BuyPrice, true);
            }

            if (contracts.Count > 0)
            {
                contracts[0].Status = 2;
                contractsTableAdapter.Update(contracts);
            }
        }

        public static void ContractExpired(Contract contract)
        {
            EMMADataSet.ContractsDataTable contracts = new EMMADataSet.ContractsDataTable();
            contractsTableAdapter.FillByID(contracts, contract.ID);

            MoveContractItems(contract, true, true);

            if (contracts.Count > 0)
            {
                contracts[0].Status = 4;
                contractsTableAdapter.Update(contracts);
            }
        }

        public static void FailContract(Contract contract)
        {
            EMMADataSet.ContractsDataTable contracts = new EMMADataSet.ContractsDataTable();
            contractsTableAdapter.FillByID(contracts, contract.ID);
            ContractItemList items = GetContractItems(contract);
            bool corp = false;
            UserAccount.CurrentGroup.GetCharacter(contract.OwnerID, ref corp);

            foreach (ContractItem item in items)
            {
                Assets.ChangeAssets(contract.OwnerID, contract.DestinationStationID, item.ItemID,
                    0, 2, false, -1 * item.Quantity, 0, false);
            }

            if (contracts.Count > 0)
            {
                contracts[0].Status = 3;
                contractsTableAdapter.Update(contracts);
            }
        }


        //public static bool OwnerHasContractsInProgress(int ownerID)
        //{
        //    EMMADataSet.ContractsDataTable table = new EMMADataSet.ContractsDataTable();
        //    contractsTableAdapter.FillByStatus(table, ownerID.ToString(), 1);
        //    return table.Count > 0;
        //}

        public static EMMADataSet.IDTableDataTable GetInvolvedStationIDs(List<int> ownerIDs, 
            ContractStationType type)
        {
            StringBuilder ownerIDString = new StringBuilder("");
            foreach (int ownerID in ownerIDs)
            {
                if (ownerIDString.Length != 0) { ownerIDString.Append(","); }
                ownerIDString.Append(ownerID);
            }

            EMMADataSet.IDTableDataTable table = new EMMADataSet.IDTableDataTable();
            lock (contractsTableAdapter)
            {
                if (type == ContractStationType.Pickup)
                {
                    IDTableAdapter.FillStationIDsByContractPickup(table, ownerIDString.ToString());
                }
                else
                {
                    IDTableAdapter.FillStationIDsByContractDest(table, ownerIDString.ToString());
                }
            }
            return table;
        }

        public static ContractList GetContracts(List<int> ownerIDs, short status, int pickupStationID,
            int destinationStationID, ContractType contractType)
        {
            ContractList retVal = new ContractList();
            EMMADataSet.ContractsDataTable table = new EMMADataSet.ContractsDataTable();
            StringBuilder ownerIDString = new StringBuilder("");
            foreach (int ownerID in ownerIDs)
            {
                if (ownerIDString.Length != 0) { ownerIDString.Append(","); }
                ownerIDString.Append(ownerID);
            }
            contractsTableAdapter.FillByAny(table, ownerIDString.ToString(), pickupStationID,
                destinationStationID, status, (short)contractType);
            foreach (EMMADataSet.ContractsRow contract in table)
            {
                retVal.Add(new Contract(contract));
            }
            return retVal;
        }


        public static ContractItemList GetContractItems(Contract contract)
        {
            ContractItemList retVal = new ContractItemList();
            EMMADataSet.ContractItemDataTable table = new EMMADataSet.ContractItemDataTable();
            contractItemsTableAdapter.FillByContractID(table, contract.ID);
            foreach (EMMADataSet.ContractItemRow item in table)
            {
                retVal.Add(new ContractItem(item, contract));
            }
            return retVal;
        }


        static public void Delete(Contract contract)
        {
            MoveContractItems(contract, true, false);
            if (contract.Type == ContractType.ItemExchange)
            {
                DeleteTransactions(contract);
            }
            contractsTableAdapter.ClearByID(contract.ID);
        }

        static public void DeleteTransactions(Contract contract)
        {
            foreach (ContractItem item in contract.Items)
            {
                Transactions.DeleteTransaction(item.TransactionID);
            }
        }

        /// <summary>
        /// Move the items in the specified contract from the pickup station to the destination station
        /// in the assets table.
        /// For item exchange contract, remove items from 'forSaleViaContract' asset stack.
        /// </summary>
        /// <param name="contractID"></param>
        static private void MoveContractItems(Contract contract, bool reverse, bool useItemsInMemory)
        {
            if (contract.Type == ContractType.Courier)
            {
                // We use GetContractItems rather than the items collection on the contract object because
                // we want the original items on the contract... not whatever is in memory right now.
                // (I.e. when reversing a contract, we need to pickup the original items on the contract
                // not whatever it has in memory right now as that may have been altered by the user)
                ContractItemList items;
                if (useItemsInMemory) { items = contract.Items; }
                else { items = GetContractItems(contract); }
                bool corp = false;
                UserAccount.CurrentGroup.GetCharacter(contract.OwnerID, ref corp);

                foreach (ContractItem item in items)
                {
                    Assets.ChangeAssets(contract.OwnerID, contract.PickupStationID, item.ItemID,
                        0, (int)AssetStatus.States.Normal, false, (reverse ? 1 : -1) * item.Quantity, 
                        0, false);
                    Assets.ChangeAssets(contract.OwnerID, contract.DestinationStationID, item.ItemID,
                        0, (int)AssetStatus.States.InTransit, false, (reverse ? -1 : 1) * item.Quantity, 
                        item.BuyPrice, !reverse);
                }
            }
            else if (contract.Type == ContractType.ItemExchange)
            {
                ContractItemList items = null;
                if (useItemsInMemory) { items = contract.Items; }
                else { items = GetContractItems(contract); }

                // For sold items, we want to find assets with status 'ForSaleViaContract'.
                // For purchased items, we want to find assets marked with the BoughtViaContract flag.

                bool buy = false;
                bool corporate = false;
                bool updateAssets = false;
                APICharacter character = UserAccount.CurrentGroup.GetCharacter(contract.OwnerID, ref corporate);
                // Note, since item exchange contracts were really shoe-horned into the older courier
                // contract system, if the contract has collateral > 0 it's a sell contract, otherwise
                // it's a buy contract... (yeah, it's shit, I should really sort it out, in fact the
                // whole contracts sub system needs modifying and refactoring... much like everything 
                // else then.)
                if (contract.Collateral < 0)
                {
                    buy = true;
                }
                DateTime assetsEffectiveDate = corporate ? character.Settings.CorpAssetsEffectiveDate :
                    character.Settings.CharAssetsEffectiveDate;
                // We only need to worry about updating assets if the last assets update was
                // before the date of the contract (note 'IssueDate' = item exchange completed date)
                // We also don't want to move the assets if we're not reversing.
                // In that case, this will already have been done by 'UpdateExchangeAssets'.
                if (assetsEffectiveDate.CompareTo(contract.IssueDate) < 0) { updateAssets = reverse; }

                foreach (ContractItem item in items)
                {
                    if (reverse)
                    {
                        if (updateAssets)
                        {
                            // If this is a sell contract then try and find the asset that we are selling.
                            // we can use this to set the profit value.
                            if (!buy)
                            {
                                // Reversing a sell contract...
                                // First put the asset back into the database.
                                Assets.ChangeAssets(contract.OwnerID,
                                    contract.PickupStationID, item.ItemID,
                                    0, (int)AssetStatus.States.Normal, false,
                                    item.Quantity, item.BuyPrice, true);
                            }
                            else
                            {
                                // Reversing a buy contract...
                                Assets.ChangeAssets(contract.OwnerID,
                                    contract.PickupStationID, item.ItemID,
                                    0, (int)AssetStatus.States.Normal, false,
                                    -1 * item.Quantity, 0, false);
                            }
                        }
                        // We also need to remove the transaction record that was created.
                        Transactions.DeleteTransaction(item.TransactionID);
                    }
                    else
                    {
                        // Note that assets have already been moved in 'UpdateExchangeAssets'
                        decimal profit = 0;
                        if (!buy) { profit = item.SellPrice - item.BuyPrice; }
                        // Create the transaction record.
                        long transID = 0;
                        Transactions.NewTransaction(contract.IssueDate, item.Quantity, item.ItemID,
                            Math.Abs(item.SellPrice), buy ? contract.OwnerID : 0, buy ? 0 : contract.OwnerID,
                            corporate && buy ? character.CharID : 0, corporate && !buy ? character.CharID : 0,
                            contract.PickupStationID, Stations.GetStation(contract.PickupStationID).regionID,
                            buy && corporate, !buy && corporate, 1000, 1000, profit, false, ref transID);
                        item.TransactionID = transID;
                    }
                }
            }
        }

        /// <summary>
        /// Create a new contract with the specified parameters.
        /// Assets will automatically be moved from the pickup station to the destination station.
        /// </summary>
        /// <param name="pickupStationID"></param>
        /// <param name="destStationID"></param>
        /// <param name="collateral"></param>
        /// <param name="reward"></param>
        /// <param name="items"></param>
        static public void Create(Contract contract)
        {
            EMMADataSet.ContractsDataTable contracts = new EMMADataSet.ContractsDataTable();

            long? ID = 0;
            contractsTableAdapter.CreateNew(contract.OwnerID, 1, contract.PickupStationID,
                contract.DestinationStationID, contract.Collateral, contract.Reward, contract.IssueDate, 
                (short)contract.Type, ref ID);

            if (ID.HasValue)
            {
                if (contract.Type == ContractType.ItemExchange)
                {
                    UpdateExchangeAssets(contract);
                    // This is now done within 'MoveContractItems'
                    // CreateTransactions(contract);
                }

                foreach (ContractItem item in contract.Items)
                {
                    contractItemsTableAdapter.CreateNew(ID, item.ItemID, item.Quantity, item.BuyPrice,
                        item.SellPrice, contract.IssueDate, item.TransactionID, item.ForcePrice);
                }

                contractsTableAdapter.FillByID(contracts, ID);
                MoveContractItems(new Contract(contracts[0]), false, true);
            }
        }

        static public void UpdateExchangeAssets(Contract contract)
        {
            bool buy = false;
            if (contract.Collateral < 0)
            {
                buy = true;
            }

            bool corporate = false;
            bool updateAssets = false;
            APICharacter character = UserAccount.CurrentGroup.GetCharacter(contract.OwnerID, ref corporate);
            DateTime assetsEffectiveDate = corporate ? character.Settings.CorpAssetsEffectiveDate :
                character.Settings.CharAssetsEffectiveDate;
            // We only need to worry about updating assets if the last assets update was
            // before the date of the contract (note 'IssueDate' = item exchange completed date)
            if (assetsEffectiveDate.CompareTo(contract.IssueDate) < 0) { updateAssets = true; }


            foreach (ContractItem item in contract.Items)
            {
                if (!buy)
                {
                    long qToFind = item.Quantity;
                    decimal itemBuyPrice = 0;

                    // Look for assets that are marked as 'for sale via contract'
                    AssetList assets = Assets.LoadAssets(
                        UserAccount.CurrentGroup.GetAssetAccessParams(APIDataType.Assets),
                        new List<int>(), item.ItemID, 0, 0, false,
                        (int)AssetStatus.States.ForSaleViaContract, true, true);

                    LookForAssetMatch(contract, updateAssets, ref qToFind, ref itemBuyPrice,
                        assets, false);
                    if (qToFind > 0)
                    {
                        LookForAssetMatch(contract, updateAssets, ref qToFind, ref itemBuyPrice,
                            assets, true);
                    }

                    if (qToFind > 0)
                    {
                        // Can't find any/enough assets 'for sale via contract' so check normal
                        // assets
                        assets = Assets.LoadAssets(
                            UserAccount.CurrentGroup.GetAssetAccessParams(APIDataType.Assets),
                            new List<int>(), item.ItemID, 0, 0, false,
                            (int)AssetStatus.States.Normal, true, false);

                        LookForAssetMatch(contract, updateAssets, ref qToFind, ref itemBuyPrice,
                            assets, false);
                        if (qToFind > 0)
                        {
                            LookForAssetMatch(contract, updateAssets, ref qToFind, ref itemBuyPrice,
                               assets, false);
                        }
                    }

                    long qFound = (item.Quantity - qToFind);
                    item.BuyPrice = (qFound > 0 ? (itemBuyPrice / qFound) : 0);
                }
                else
                {
                    // buy contract, first look for assets that are marked as 'bought via contract'
                    // If there are none then just add the asset directly.
                    long qToFind = item.Quantity;
                    bool updates = false;
                    EMMADataSet.AssetsDataTable assetData = new EMMADataSet.AssetsDataTable();
                    Assets.GetAssetsBoughtViaContract(assetData,
                        UserAccount.CurrentGroup.GetAssetAccessParams(APIDataType.Assets), item.ItemID);

                    foreach (EMMADataSet.AssetsRow asset in assetData)
                    {
                        if (qToFind > 0 && contract.OwnerID == asset.OwnerID)
                        {
                            long deltaQ = Math.Min(asset.Quantity, qToFind);
                            asset.Cost = item.SellPrice;
                            asset.CostCalc = true;
                            asset.BoughtViaContract = false;
                            updates = true;
                            // Note, we don't update asset quantities because the asset is 
                            // clearly already there. (We found it by searching the database)
                            qToFind -= deltaQ;
                        }
                    }
                    if (qToFind > 0)
                    {
                        // We havn't found enough assets to match the contract.
                        // Check assets 'bought via contract' for all corps/chars in the report group
                        foreach (EMMADataSet.AssetsRow asset in assetData)
                        {
                            if (qToFind > 0)
                            {
                                long deltaQ = Math.Min(asset.Quantity, qToFind);
                                asset.Cost = item.SellPrice;
                                asset.CostCalc = true;
                                asset.BoughtViaContract = false;
                                updates = true;
                                qToFind -= deltaQ;
                            }
                        }
                    }
                    if (qToFind > 0)
                    {
                        // Still havn't managed to match everything. If we're updating assets 
                        // (contract date later than asset effective date) then just create
                        // the new asset records directly.
                        // If we're not updating assets then look for an asset of the same type
                        // that does not have it's cost calculated and set that.
                        if (updateAssets)
                        {
                            Assets.ChangeAssets(contract.OwnerID, contract.PickupStationID,
                                item.ItemID, 0, (int)AssetStatus.States.Normal, false, qToFind,
                                -1 * item.SellPrice, true);
                        }
                        else
                        {
                            // Try to find existing assets that are the same type and do not have
                            // a calculated cost.
                            AssetList assets = Assets.LoadAssets(
                                UserAccount.CurrentGroup.GetAssetAccessParams(APIDataType.Assets),
                                new List<int>(), item.ItemID, 0, 0, false,
                                (int)AssetStatus.States.Normal, true, false);
                            foreach (Asset asset in assets)
                            {
                                if (qToFind > 0 && contract.OwnerID == asset.OwnerID && 
                                    (asset.UnitBuyPrice == 0 || !asset.UnitBuyPricePrecalculated))
                                {
                                    long deltaQ = Math.Min(asset.Quantity, qToFind);
                                    Assets.AddAssetToTable(assetData, asset.ID);
                                    EMMADataSet.AssetsRow assetRow = assetData.FindByID(asset.ID);
                                    assetRow.Cost = item.SellPrice;
                                    assetRow.CostCalc = true;
                                    updates = true;
                                    qToFind -= deltaQ;
                                }
                            }
                            if (qToFind > 0)
                            {
                                foreach (Asset asset in assets)
                                {
                                    if (qToFind > 0 && 
                                        (asset.UnitBuyPrice == 0 || !asset.UnitBuyPricePrecalculated))
                                    {
                                        long deltaQ = Math.Min(asset.Quantity, qToFind);
                                        Assets.AddAssetToTable(assetData, asset.ID);
                                        EMMADataSet.AssetsRow assetRow = assetData.FindByID(asset.ID);
                                        assetRow.Cost = item.SellPrice;
                                        assetRow.CostCalc = true;
                                        updates = true;
                                        qToFind -= deltaQ;
                                    }
                                }
                            }
                        }
                    }

                    if (updates) { Assets.UpdateDatabase(assetData); }

                }


            }
        }

        private static void LookForAssetMatch(Contract contract, bool updateAssets, ref long qToFind,
            ref decimal itemBuyPrice, AssetList assets, bool ignoreOwner)
        {
            foreach (Asset a in assets)
            {
                if (ignoreOwner || a.OwnerID == contract.OwnerID)
                {
                    if (qToFind > 0)
                    {
                        long deltaQ = Math.Min(qToFind, a.Quantity);
                        itemBuyPrice += a.UnitBuyPrice * deltaQ;
                        qToFind -= deltaQ;
                        a.Quantity -= deltaQ;

                        // Update the database with changes to the asset row.
                        if (updateAssets)
                        {
                            EMMADataSet.AssetsRow data = Assets.GetAssetDetail(a.ID);
                            data.Quantity -= deltaQ;
                            if (data.Quantity == 0) { data.Delete(); }
                            Assets.UpdateDatabase(data);
                        }
                    }
                }
            }
        }
    

/*
        static public EMMADataSet.ContractsRow GetContract(int contractID)
        {
            contracts.Clear();
            LoadData(contractID);

            EMMADataSet.ContractsRow retVal = contracts.FindByID(contractID);

            return retVal;
        }

        /// <summary>
        /// Return all contracts in the database
        /// </summary>
        /// <returns></returns>
        static public EMMADataSet.ContractsDataTable GetAllContracts()
        {
            return GetContracts(0, 0);
        }

        /// <summary>
        /// Get contracts 
        /// </summary>
        /// <param name="pickupStationID"></param>
        /// <param name="destStationID"></param>
        /// <returns></returns>
        static public EMMADataSet.ContractsDataTable GetContracts(int pickupStationID, int destStationID)
        {
            EMMADataSet.ContractsDataTable retVal = new EMMADataSet.ContractsDataTable();

            if (pickupStationID > 0 && destStationID > 0)
            {
                contractsTableAdapter.FillByPickupAndDest(retVal, pickupStationID, destStationID);
            }
            else if (pickupStationID > 0)
            {
                contractsTableAdapter.FillByPickup(retVal, pickupStationID);
            }
            else if (destStationID > 0)
            {
                contractsTableAdapter.FillByDest(retVal, destStationID);
            }
            else
            {
                contractsTableAdapter.Fill(retVal);
            }

            return retVal;
        }


        static public void MoveAllContractItems()
        {
            EMMADataSet.ContractsDataTable allContracts = new EMMADataSet.ContractsDataTable();
            contractsTableAdapter.Fill(allContracts);

            foreach (EMMADataSet.ContractsRow contract in allContracts)
            {
                MoveContractItems(contract.ID, false);
            }
        }



        /// <summary>
        /// Cancel the specified contract.
        /// Assets that were previously moved to the destination station will be moved back to the pickup station.
        /// </summary>
        /// <param name="contractID"></param>
        /// <param name="failed"></param>
        static public void Cancel(int contractID, bool failed)
        {
            contracts.Clear();
            LoadData(contractID);
            EMMADataSet.ContractsRow contract = contracts.FindByID(contractID);
            EMMADataSet.ContractItemDataTable items = ContractItems.GetItemsInContract(contractID);

            foreach (EMMADataSet.ContractItemRow item in items)
            {
                Assets.ChangeAssets(contract.PickupStationID, item.ItemID, item.Quantity);
                Assets.ChangeAssets(contract.DestinationStationID, item.ItemID, -1 * item.Quantity);
            }

            contract.Status = (int) (failed ? ContractStates.Failed : ContractStates.Cancelled);
            contractsTableAdapter.Update(contracts);
            contracts.Clear();
        }

        /// <summary>
        /// Creates an xml document containing all contracts in the database.
        /// </summary>
        /// <returns>The filename of the resulting xml file.</returns>
        static public string SaveAll(string dstDir)
        {
            string fileName = string.Format("{0}Contracts.xml", dstDir);

            LoadAllData();

            try
            {
                contracts.WriteXml(fileName);
            }
            catch (Exception ex)
            {
                throw new EMMADataException(ExceptionSeverity.Error, "Cannot create contracts xml file.", ex);
            }

            contracts.Clear();

            return fileName;
        }

        /// <summary>
        /// Loads data from the specified XML file into the EMMA contracts table.
        /// </summary>
        /// <param name="fileName"></param>
        static public void LoadXML(string fileName)
        {
            try
            {
                // load the XML data into the in-memory data table.
                contracts.ReadXml(fileName);
                try
                {
                    // Update the database.
                    contractsTableAdapter.Update(contracts);

                    // Clear all the data we've just added out of memory.
                    contracts.Clear();
                }
                catch (Exception ex)
                {
                    throw new EMMADataException(ExceptionSeverity.Error, "Unable commit new contract data to " +
                        "the EMMA database.", ex);
                }
            }
            catch (Exception ex)
            {
                throw new EMMADataException(ExceptionSeverity.Error, "Unable to read contract data from the" +
                    " specified XML file - " + fileName, ex);
            }
        }

        static private int FreeID()
        {
            int? maxID = contractsTableAdapter.GetMaxContractID();
            return (maxID.HasValue ? maxID.Value : 0) + 1;
        }

        static public void ClearDatabase()
        {
            contractsTableAdapter.ClearContracts();
            contracts.Clear();
            ContractItems.ClearDatabase();
        }

        /// <summary>
        /// Load data for the specified contract from the database
        /// </summary>
        static private void LoadData(int contractID)
        {
            try
            {
                // Retrieve all data
                contractsTableAdapter.FillByID(contracts, contractID);
            }
            catch (Exception ex)
            {
                throw new EMMADataException(ExceptionSeverity.Critical, "Problem loading contract " +
                    "data for contract " + contractID + " from the EMMA database.", ex);
            }
        }
        
        /// <summary>
        /// Loads all data from the contracts database table into the dataset.
        /// This method should not be used unless absolutely necessary as it will eat memory.
        /// </summary>
        static private void LoadAllData()
        {
            try
            {
                // Retrieve all data
                contractsTableAdapter.Fill(contracts);
            }
            catch (Exception ex)
            {
                throw new EMMADataException(ExceptionSeverity.Critical, "Problem loading contract " +
                    "data from the EMMA database.", ex);
            }
        }

        public enum ContractStates
        {
            Open,
            Completed,
            Cancelled,
            Failed
        }
    }


    static class ContractItems
    {
        private static EMMADataSetTableAdapters.ContractItemTableAdapter contractItemsTableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.ContractItemTableAdapter();

        static public void DeleteItemsInContract(int contractID)
        {
            EMMADataSet.ContractItemDataTable items = GetItemsInContract(contractID);
            foreach (EMMADataSet.ContractItemRow item in items)
            {
                item.Delete();
            }
            contractItemsTableAdapter.Update(items);
        }

        /// <summary>
        /// Get the items that are part of the specified contract
        /// </summary>
        /// <param name="contractID"></param>
        /// <returns></returns>
        static public EMMADataSet.ContractItemDataTable GetItemsInContract(int contractID)
        {
            EMMADataSet.ContractItemDataTable items = new EMMADataSet.ContractItemDataTable();
            contractItemsTableAdapter.FillByContractID(items, contractID);
            return items;
        }

        /// <summary>
        /// Create a new contract with the specified parameters
        /// </summary>
        /// <param name="contractID"></param>
        /// <param name="itemID"></param>
        /// <param name="quantity"></param>
        static public void Create(int contractID, int itemID, int quantity, decimal buyPrice, decimal sellPrice)
        {
            contractItems = new EMMADataSet.ContractItemDataTable();
            EMMADataSet.ContractItemRow contractItem = contractItems.NewContractItemRow();
            contractItem.ContractID = contractID;
            contractItem.ItemID = itemID;
            contractItem.Quantity = quantity;
            contractItem.BuyPrice = buyPrice;
            contractItem.SellPrice = sellPrice;

            contractItems.AddContractItemRow(contractItem);
            contractItemsTableAdapter.Update(contractItems);
        }

        /// <summary>
        /// Creates an xml document containing all contract items in the database.
        /// </summary>
        /// <returns>The filename of the resulting xml file.</returns>
        static public string SaveAll(string dstDir)
        {
            string fileName = string.Format("{0}ContractItems.xml", dstDir);

            LoadAllData();

            try
            {
                contractItems.WriteXml(fileName);
            }
            catch (Exception ex)
            {
                throw new EMMADataException(ExceptionSeverity.Error, "Cannot create contract items xml file.", ex);
            }

            contractItems.Clear();

            return fileName;
        }

        /// <summary>
        /// Loads data from the specified XML file into the EMMA contract items table.
        /// </summary>
        /// <param name="fileName"></param>
        static public void LoadXML(string fileName)
        {
            try
            {
                // load the XML data into the in-memory data table.
                contractItems.ReadXml(fileName);
                try
                {
                    // Update the database.
                    contractItemsTableAdapter.Update(contractItems);

                    // Clear all the data we've just added out of memory.
                    contractItems.Clear();
                }
                catch (Exception ex)
                {
                    throw new EMMADataException(ExceptionSeverity.Error, "Unable commit new contract items data to " +
                        "the EMMA database.", ex);
                }
            }
            catch (Exception ex)
            {
                throw new EMMADataException(ExceptionSeverity.Error, "Unable to read contract items data from the" +
                    " specified XML file - " + fileName, ex);
            }
        }

        static public void ClearDatabase()
        {
            contractItems.Clear();
        }

        /// <summary>
        /// Load items for the specified contract from the database 
        /// </summary>
        static private void LoadData(int contractID)
        {
            try
            {
                // Retrieve all data
                contractItemsTableAdapter.FillByContractID(contractItems, contractID);
            }
            catch (Exception ex)
            {
                throw new EMMADataException(ExceptionSeverity.Critical, "Problem loading contract items " +
                    "data for contract " + contractID + " from the EMMA database.", ex);
            }
        }

        /// <summary>
        /// Loads all data from the contract items database table into the dataset.
        /// This method should not be used unless absolutely necessary as it will eat memory.
        /// </summary>
        static private void LoadAllData()
        {
            try
            {
                // Retrieve all data
                contractItemsTableAdapter.Fill(contractItems);
            }
            catch (Exception ex)
            {
                throw new EMMADataException(ExceptionSeverity.Critical, "Problem loading contract items " +
                    "data from the EMMA database.", ex);
            }
        }
*/

        public void LoadOldEmmaXML(string contractFile, string contractItemFile, int ownerID)
        {
            Dictionary<long, long> idTranslation = new Dictionary<long,long>();
            Dictionary<long, DateTime> contractTime = new Dictionary<long,DateTime>();
            EMMADataSet.ContractsDataTable table = new EMMADataSet.ContractsDataTable();
            XmlDocument xml = new XmlDocument();
            UpdateStatus(0, 0, "", "Loading contracts file", false);
            xml.Load(contractFile);

            XmlNodeList nodes = xml.SelectNodes("/DocumentElement/Contracts");

            int counter = 0;
            UpdateStatus(0, 0, "", "Extracting contract data from XML", false);
            foreach (XmlNode node in nodes)
            {
                long? newID = 0;
                long oldID = long.Parse(node.SelectSingleNode("ID").FirstChild.Value,
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                int status = 2;
                int pickupID = int.Parse(node.SelectSingleNode("PickupStationID").FirstChild.Value,
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                int destID = int.Parse(node.SelectSingleNode("DestinationStationID").FirstChild.Value,
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                decimal collateral = decimal.Parse(node.SelectSingleNode("Collateral").FirstChild.Value,
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                decimal reward = decimal.Parse(node.SelectSingleNode("Reward").FirstChild.Value,
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                XmlNode datenode = node.SelectSingleNode("IssueDate");
                DateTime datetime = new DateTime(2000, 1, 1);
                if (datenode != null)
                {
                    datetime = DateTime.Parse(datenode.FirstChild.Value);
                }
                lock (contractsTableAdapter)
                {
                    contractsTableAdapter.CreateNew(ownerID, status, pickupID, destID, collateral, reward,
                        datetime, (short)ContractType.Courier, ref newID);
                }

                idTranslation.Add(oldID, newID.Value);
                contractTime.Add(newID.Value, datetime);

                UpdateStatus(counter, nodes.Count, "", "", false);
                counter++;
            }


            EMMADataSet.ContractItemDataTable itemTable = new EMMADataSet.ContractItemDataTable();
            xml = new XmlDocument();
            UpdateStatus(0, 0, "", "Loading contract items file", false);
            xml.Load(contractItemFile);

            nodes = xml.SelectNodes("/DocumentElement/ContractItem");

            counter = 0;
            UpdateStatus(0, 0, "", "Extracting contract item data from XML", false);
            foreach (XmlNode node in nodes)
            {
                long oldID = long.Parse(node.SelectSingleNode("ContractID").FirstChild.Value,
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                if (idTranslation.ContainsKey(oldID))
                {
                    long newID = idTranslation[oldID];
                    int itemID = int.Parse(node.SelectSingleNode("ItemID").FirstChild.Value,
                        System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    int quantity = int.Parse(node.SelectSingleNode("Quantity").FirstChild.Value,
                        System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    decimal buyPrice = decimal.Parse(node.SelectSingleNode("BuyPrice").FirstChild.Value,
                        System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    decimal sellPrice = decimal.Parse(node.SelectSingleNode("SellPrice").FirstChild.Value,
                        System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    DateTime datetime = contractTime[newID];
                    lock (contractItemsTableAdapter)
                    {
                        contractItemsTableAdapter.CreateNew(newID, itemID, quantity, buyPrice, sellPrice, 
                            datetime, 0, false);
                    }
                }
                UpdateStatus(counter, nodes.Count, "", "", false);
                counter++;
            }
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

    public enum ContractStationType
    {
        Pickup,
        Destination
    }

}
