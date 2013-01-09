using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using EveMarketMonitorApp.DatabaseClasses;
using System.IO;
using EveMarketMonitorApp.AbstractionClasses;
using System.Xml;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class ExportData : Form
    {
        public ExportData()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (rdbCSV.Checked)
            {
                SortedList<object, string> options = new SortedList<object, string>();
                Type enumType = typeof(Table);
                foreach (string value in Enum.GetNames(enumType))
                {
                    options.Add(Enum.Parse(enumType, value), value);
                }
                OptionPicker dialog = new OptionPicker("Data to export", "Choose the data that " +
                    "you wish to export from EMMA.\r\nNote that you can also create CSV files " +
                    "from any report by right clicking it and choosing the 'export to CSV option'",
                    options);
                if (dialog.ShowDialog() != DialogResult.Cancel)
                {
                    Table result = (Table)dialog.SelectedItem;

                    ExportCSV(result);

                    this.Close();
                }
            }
            else if (rdbAPIXML.Checked)
            {
                ExportXML();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ExportCSV(Table table)
        {
            //DataTable tableData = null;

            switch (table)
            {
                case Table.Transactions:
                    break;
                case Table.Journal:
                    break;
                case Table.Orders:
                    break;
                case Table.Assets:
                    break;
                case Table.Contracts:
                    break;
                default:
                    break;
            }
        }

        private void ExportXML()
        {
            DialogResult result = MessageBox.Show("This process will export all data for the current EMMA user group in API-compatible XML format. " +
                "EMMA will generate XML files that can be imported into abother application (e.g. EMMA 2).", "Confirm", MessageBoxButtons.OKCancel);
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                FolderBrowserDialog dlg = new FolderBrowserDialog();
                dlg.Description = "Select the folder to export to";
                dlg.ShowNewFolderButton = true;
                string dir =  @"%APPDATA%\EMMA\Exports\EMMA_1_6_Data";
                if(!Directory.Exists(dir)) { Directory.CreateDirectory(dir);}
                dlg.SelectedPath = dir;
                if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
                {
                    dir = dlg.SelectedPath;

                    foreach (EVEAccount account in UserAccount.CurrentGroup.Accounts)
                    {
                        foreach(APICharacter character in account.Chars)
                        {
                            if (character.CharIncWithRptGroup)
                            {
                                ExportTransactionXML(dir, character, false);
                                ExportJournalXML(dir, character, false);
                                ExportMarketOrderXML(dir, character, false);
                                ExportIndustryJobXML(dir, character);
                            }
                            if (character.CorpIncWithRptGroup)
                            {
                                ExportTransactionXML(dir, character, true);
                                ExportJournalXML(dir, character, true);
                                ExportMarketOrderXML(dir, character, true);
                            }
                        }
                    }
                }
            }
        }

        private enum Table
        {
            Transactions,
            Journal,
            Orders,
            Assets,
            Contracts
        }

        #region Transactions
        private void ExportTransactionXML(string dir, APICharacter character, bool forCorp)
        {
            int wallet = 1000;
            int maxWallet = !forCorp ? 1000 : 1007;

            while (wallet <= maxWallet)
            {
                string filename = Path.Combine(dir, (!forCorp ? character.CharName : character.CorpName) + " [" +
                    (!forCorp ? character.CharID : character.CorpID) + "]" + (!forCorp ? "" : " Wallet " + wallet) + " Transactions.xml");
                EveMarketMonitorApp.DatabaseClasses.EMMADataSet.TransactionsDataTable transactions = Transactions.GetTransData(character.CharID, forCorp, 0, 0, 0, 0, "");

                XmlDocument xml = new XmlDocument();

                XmlNode eveAPINode = xml.CreateNode(XmlNodeType.Element, "eveapi", "");
                XmlAttribute versionNode = xml.CreateAttribute("version", "");
                versionNode.Value = "2";
                eveAPINode.Attributes.Append(versionNode);

                XmlNode currentTimeNode = xml.CreateNode(XmlNodeType.Element, "currentTime", "");
                currentTimeNode.InnerText = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                eveAPINode.AppendChild(currentTimeNode);

                XmlNode resultNode = xml.CreateNode(XmlNodeType.Element, "result", "");
                XmlNode rootRowSetNode = AddTransactionsToXML(xml, transactions, !forCorp ? character.CharID : character.CorpID, forCorp, wallet);
                resultNode.AppendChild(rootRowSetNode);
                eveAPINode.AppendChild(resultNode);

                xml.AppendChild(eveAPINode);
                xml.Save(filename);

                wallet++;
            }            
        }

        private XmlNode AddTransactionsToXML(XmlDocument xml, EveMarketMonitorApp.DatabaseClasses.EMMADataSet.TransactionsDataTable transactions, 
            long APIEntityID, bool forCorp, int walletID)
        {
            XmlNode rowSet = NewTransactionsRowset(xml);

            foreach (EveMarketMonitorApp.DatabaseClasses.EMMADataSet.TransactionsRow transaction in transactions)
            {
                if (transaction.ID < 9000000000000000000)
                {
                    if (transaction.BuyerID == APIEntityID)
                    {
                        if (!forCorp || transaction.BuyerWalletID == walletID)
                        {
                            XmlNode node = AddTransRow(xml, APIEntityID, transaction, true, forCorp);
                            rowSet.AppendChild(node);
                        }
                    }
                    if (transaction.SellerID == APIEntityID)
                    {
                        if (!forCorp || transaction.SellerWalletID == walletID)
                        {
                            XmlNode node = AddTransRow(xml, APIEntityID, transaction, false, forCorp);
                            rowSet.AppendChild(node);
                        }
                    }
                }
            }

            return rowSet;                
        }

        private XmlNode AddTransRow(XmlDocument xml, long APIEntityID, EveMarketMonitorApp.DatabaseClasses.EMMADataSet.TransactionsRow transaction, bool clientSeller, bool forCorp)
        {
            XmlNode row = xml.CreateNode(XmlNodeType.Element, "row", "");

            XmlAttribute datetimeAtt = xml.CreateAttribute("transactionDateTime", "");
            datetimeAtt.Value = transaction.DateTime.ToString(System.Globalization.CultureInfo.InvariantCulture);
            row.Attributes.Append(datetimeAtt);
            XmlAttribute idAtt = xml.CreateAttribute("transactionID", "");
            idAtt.Value = transaction.ID.ToString();
            row.Attributes.Append(idAtt);
            XmlAttribute quantityAtt = xml.CreateAttribute("quantity", "");
            quantityAtt.Value = transaction.Quantity.ToString();
            row.Attributes.Append(quantityAtt);
            XmlAttribute typeNameAtt = xml.CreateAttribute("typeName", "");
            typeNameAtt.Value = Items.GetItemName(transaction.ItemID);
            row.Attributes.Append(typeNameAtt);
            XmlAttribute typeIDAtt = xml.CreateAttribute("typeID", "");
            typeIDAtt.Value = transaction.ItemID.ToString();
            row.Attributes.Append(typeIDAtt);
            XmlAttribute priceAtt = xml.CreateAttribute("price", "");
            priceAtt.Value = transaction.Price.ToString(System.Globalization.CultureInfo.InvariantCulture);
            row.Attributes.Append(priceAtt);
            XmlAttribute clientIDAtt = xml.CreateAttribute("clientID", "");
            clientIDAtt.Value = clientSeller ? transaction.SellerID.ToString() : transaction.BuyerID.ToString();
            row.Attributes.Append(clientIDAtt);
            XmlAttribute clientNameAtt = xml.CreateAttribute("clientName", "");
            clientNameAtt.Value = Names.GetName(clientSeller ? transaction.SellerID : transaction.BuyerID);
            row.Attributes.Append(clientNameAtt);
            XmlAttribute stationIDAtt = xml.CreateAttribute("stationID", "");
            stationIDAtt.Value = transaction.StationID.ToString();
            row.Attributes.Append(stationIDAtt);
            XmlAttribute stationNameAtt = xml.CreateAttribute("stationName", "");
            stationNameAtt.Value = Stations.GetStationName(transaction.StationID);
            row.Attributes.Append(stationNameAtt);
            XmlAttribute transTypeAtt = xml.CreateAttribute("transactionType", "");
            transTypeAtt.Value = clientSeller ? "buy" : "sell";
            row.Attributes.Append(transTypeAtt);
            XmlAttribute transForAtt = xml.CreateAttribute("transactionFor", "");
            transForAtt.Value = !forCorp ? "personal" : "corporation";
            row.Attributes.Append(transForAtt);
            XmlAttribute journalIDAtt = xml.CreateAttribute("journalTransactionID", "");
            journalIDAtt.Value = "0";
            row.Attributes.Append(journalIDAtt);

            return row;
        }

        private XmlNode NewTransactionsRowset(XmlDocument xml)
        {
            XmlNode rowSet = xml.CreateNode(XmlNodeType.Element, "rowset", "");

            XmlAttribute nameAtt = xml.CreateAttribute("name", "");
            nameAtt.Value = "transactions";
            rowSet.Attributes.Append(nameAtt);
            XmlAttribute keyAtt = xml.CreateAttribute("key", "");
            keyAtt.Value = "transactionID";
            rowSet.Attributes.Append(keyAtt);
            XmlAttribute colAtt = xml.CreateAttribute("columns", "");
            colAtt.Value = "transactionDateTime,transactionID,quantity,typeName,typeID,price,clientID,clientName,stationID,stationName,transactionType,transactionFor";
            rowSet.Attributes.Append(colAtt);

            return rowSet;
        }
        #endregion

        #region Journal
        private void ExportJournalXML(string dir, APICharacter character, bool forCorp)
        {
            short wallet = 1000;
            int maxWallet = !forCorp ? 1000 : 1007;

            while (wallet <= maxWallet)
            {
                string filename = Path.Combine(dir, (!forCorp ? character.CharName : character.CorpName) + " [" +
                    (!forCorp ? character.CharID : character.CorpID) + "]" + (!forCorp ? "" : " Wallet " + wallet) + " Journal.xml");
                List<FinanceAccessParams> accessParams = new List<FinanceAccessParams>();
                if (!forCorp)
                {
                    accessParams.Add(new FinanceAccessParams(character.CharID));
                }
                else
                {
                    accessParams.Add(new FinanceAccessParams(character.CorpID, new List<short>() { wallet }));
                }
                EveMarketMonitorApp.DatabaseClasses.EMMADataSet.JournalDataTable journalEntries = Journal.LoadEntriesData(
                    accessParams, new List<short>(), DateTime.MinValue, DateTime.MaxValue, null);

                XmlDocument xml = new XmlDocument();

                XmlNode eveAPINode = xml.CreateNode(XmlNodeType.Element, "eveapi", "");
                XmlAttribute versionNode = xml.CreateAttribute("version", "");
                versionNode.Value = "2";
                eveAPINode.Attributes.Append(versionNode);

                XmlNode currentTimeNode = xml.CreateNode(XmlNodeType.Element, "currentTime", "");
                currentTimeNode.InnerText = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                eveAPINode.AppendChild(currentTimeNode);

                XmlNode resultNode = xml.CreateNode(XmlNodeType.Element, "result", "");
                XmlNode rootRowSetNode = AddJournalToXML(xml, journalEntries, !forCorp ? character.CharID : character.CorpID, forCorp, wallet);
                resultNode.AppendChild(rootRowSetNode);
                eveAPINode.AppendChild(resultNode);

                xml.AppendChild(eveAPINode);
                xml.Save(filename);

                wallet++;
            }
        }

        private XmlNode AddJournalToXML(XmlDocument xml, EveMarketMonitorApp.DatabaseClasses.EMMADataSet.JournalDataTable journalEntries,
            long APIEntityID, bool forCorp, int walletID)
        {
            XmlNode rowSet = NewJournalRowset(xml);

            foreach (EveMarketMonitorApp.DatabaseClasses.EMMADataSet.JournalRow journalEntry in journalEntries)
            {
                if (journalEntry.SenderID == APIEntityID || (forCorp && journalEntry.SCorpID == APIEntityID))
                {
                    if (!forCorp || journalEntry.SWalletID == walletID)
                    {
                        XmlNode node = AddJournalRow(xml, APIEntityID, journalEntry, true, forCorp);
                        rowSet.AppendChild(node);
                    }
                }
                if (journalEntry.RecieverID == APIEntityID || (forCorp && journalEntry.RCorpID == APIEntityID))
                {
                    if (!forCorp || journalEntry.RWalletID == walletID)
                    {
                        XmlNode node = AddJournalRow(xml, APIEntityID, journalEntry, false, forCorp);
                        rowSet.AppendChild(node);
                    }
                }
            }

            return rowSet;
        }

        private XmlNode AddJournalRow(XmlDocument xml, long APIEntityID, EveMarketMonitorApp.DatabaseClasses.EMMADataSet.JournalRow journalEntry, bool currentUserIsSender, bool forCorp)
        {
            XmlNode row = xml.CreateNode(XmlNodeType.Element, "row", "");

            XmlAttribute idAtt = xml.CreateAttribute("refID", "");
            idAtt.Value = journalEntry.ID.ToString();
            row.Attributes.Append(idAtt);
            XmlAttribute dateAtt = xml.CreateAttribute("date", "");
            dateAtt.Value = journalEntry.Date.ToString(System.Globalization.CultureInfo.InvariantCulture);
            row.Attributes.Append(dateAtt);
            XmlAttribute typeAtt = xml.CreateAttribute("refTypeID", "");
            typeAtt.Value = journalEntry.TypeID.ToString();
            row.Attributes.Append(typeAtt);
            XmlAttribute fromNameAtt = xml.CreateAttribute("ownerName1", "");
            fromNameAtt.Value = Names.GetName(journalEntry.SenderID);
            row.Attributes.Append(fromNameAtt);
            XmlAttribute fromIDAtt = xml.CreateAttribute("ownerID1", "");
            fromIDAtt.Value = journalEntry.SenderID.ToString();
            row.Attributes.Append(fromIDAtt);
            XmlAttribute toNameAtt = xml.CreateAttribute("ownerName2", "");
            toNameAtt.Value = Names.GetName(journalEntry.RecieverID);
            row.Attributes.Append(toNameAtt);
            XmlAttribute toIDAtt = xml.CreateAttribute("ownerID2", "");
            toIDAtt.Value = journalEntry.RecieverID.ToString();
            row.Attributes.Append(toIDAtt);
            XmlAttribute argNameAtt = xml.CreateAttribute("argName1", "");
            argNameAtt.Value = currentUserIsSender ? journalEntry.SArgName : journalEntry.RArgName;
            row.Attributes.Append(argNameAtt);
            XmlAttribute argIDAtt = xml.CreateAttribute("argID1", "");
            argIDAtt.Value = currentUserIsSender ? journalEntry.SArgID.ToString() : journalEntry.RArgID.ToString();
            row.Attributes.Append(argIDAtt);
            XmlAttribute amountAtt = xml.CreateAttribute("amount", "");
            amountAtt.Value = journalEntry.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture);
            row.Attributes.Append(amountAtt);
            XmlAttribute balanceAtt = xml.CreateAttribute("balance", "");
            balanceAtt.Value = currentUserIsSender ? journalEntry.SBalance.ToString() : journalEntry.RBalance.ToString();
            row.Attributes.Append(balanceAtt);
            XmlAttribute reasonAtt = xml.CreateAttribute("reason", "");
            reasonAtt.Value = journalEntry.Reason;
            row.Attributes.Append(reasonAtt);

            return row;
        }

        private XmlNode NewJournalRowset(XmlDocument xml)
        {
            XmlNode rowSet = xml.CreateNode(XmlNodeType.Element, "rowset", "");

            XmlAttribute nameAtt = xml.CreateAttribute("name", "");
            nameAtt.Value = "transactions";
            rowSet.Attributes.Append(nameAtt);
            XmlAttribute keyAtt = xml.CreateAttribute("key", "");
            keyAtt.Value = "refID";
            rowSet.Attributes.Append(keyAtt);
            XmlAttribute colAtt = xml.CreateAttribute("columns", "");
            colAtt.Value = "date,refID,refTypeID,ownerName1,ownerID1,ownerName2,ownerID2,argName1,argID1,amount,balance,reason,taxReceiverID,taxAmount";
            rowSet.Attributes.Append(colAtt);

            return rowSet;
        }
        #endregion

        #region Orders
        private void ExportMarketOrderXML(string dir, APICharacter character, bool forCorp)
        {
                string filename = Path.Combine(dir, (!forCorp ? character.CharName : character.CorpName) + " [" +
                    (!forCorp ? character.CharID : character.CorpID) + "]" + " MarketOrders.xml");
                List<AssetAccessParams> accessParams = new List<AssetAccessParams>();
                if (!forCorp)
                {
                    accessParams.Add(new AssetAccessParams(character.CharID));
                }
                else
                {
                    accessParams.Add(new AssetAccessParams(character.CorpID));
                }
                EveMarketMonitorApp.DatabaseClasses.EMMADataSet.OrdersDataTable marketOrders = Orders.LoadOrdersData(accessParams, new List<int>(), new List<long>(), 0, "");
                    
                XmlDocument xml = new XmlDocument();

                XmlNode eveAPINode = xml.CreateNode(XmlNodeType.Element, "eveapi", "");
                XmlAttribute versionNode = xml.CreateAttribute("version", "");
                versionNode.Value = "2";
                eveAPINode.Attributes.Append(versionNode);

                XmlNode currentTimeNode = xml.CreateNode(XmlNodeType.Element, "currentTime", "");
                currentTimeNode.InnerText = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                eveAPINode.AppendChild(currentTimeNode);

                XmlNode resultNode = xml.CreateNode(XmlNodeType.Element, "result", "");
                XmlNode rootRowSetNode = AddMarketOrderToXML(xml, marketOrders);
                resultNode.AppendChild(rootRowSetNode);
                eveAPINode.AppendChild(resultNode);

                xml.AppendChild(eveAPINode);
                xml.Save(filename);

        }

        private XmlNode AddMarketOrderToXML(XmlDocument xml, EveMarketMonitorApp.DatabaseClasses.EMMADataSet.OrdersDataTable marketOrders)
        {
            XmlNode rowSet = NewMarketOrderRowset(xml);

            foreach (EveMarketMonitorApp.DatabaseClasses.EMMADataSet.OrdersRow order in marketOrders)
            {
                XmlNode node = AddMarketOrderRow(xml, order);
                rowSet.AppendChild(node);
            }

            return rowSet;
        }

        private XmlNode AddMarketOrderRow(XmlDocument xml, EveMarketMonitorApp.DatabaseClasses.EMMADataSet.OrdersRow order)
        {
            XmlNode row = xml.CreateNode(XmlNodeType.Element, "row", "");

            XmlAttribute idAtt = xml.CreateAttribute("orderID", "");
            idAtt.Value = order.EveOrderID.ToString();
            row.Attributes.Append(idAtt);
            XmlAttribute charAtt = xml.CreateAttribute("charID", "");
            charAtt.Value = order.OwnerID.ToString();
            row.Attributes.Append(charAtt);
            XmlAttribute stationAtt = xml.CreateAttribute("stationID", "");
            stationAtt.Value = order.StationID.ToString();
            row.Attributes.Append(stationAtt);
            XmlAttribute volEnteredAtt = xml.CreateAttribute("volEntered", "");
            volEnteredAtt.Value = order.TotalVol.ToString();
            row.Attributes.Append(volEnteredAtt);
            XmlAttribute volRemainingAtt = xml.CreateAttribute("volRemaining", "");
            volRemainingAtt.Value = order.RemainingVol.ToString();
            row.Attributes.Append(volRemainingAtt);
            XmlAttribute minVolumeAtt = xml.CreateAttribute("minVolume", "");
            minVolumeAtt.Value = order.MinVolume.ToString();
            row.Attributes.Append(minVolumeAtt);
            XmlAttribute stateAtt = xml.CreateAttribute("orderState", "");
            if(order.OrderState == 1) { stateAtt.Value = "1"; }
            if(order.OrderState == 2) { stateAtt.Value = "2"; }
            if(order.OrderState == 3) { stateAtt.Value = "3"; }
            if(order.OrderState == 4) { stateAtt.Value = "4"; }
            if(order.OrderState == 5) { stateAtt.Value = "5"; }
            if(order.OrderState == 999) { stateAtt.Value = "0"; }
            if(order.OrderState == 1000) { stateAtt.Value = "2"; }
            if(order.OrderState == 1001) { stateAtt.Value = "0"; }
            if(order.OrderState == 2000) { stateAtt.Value = "2"; }
            row.Attributes.Append(stateAtt);
            XmlAttribute typeAtt = xml.CreateAttribute("typeID", "");
            typeAtt.Value = order.ItemID.ToString();
            row.Attributes.Append(typeAtt);
            XmlAttribute rangeAtt = xml.CreateAttribute("range", "");
            rangeAtt.Value = order.Range.ToString();
            row.Attributes.Append(rangeAtt);
            XmlAttribute walletAtt = xml.CreateAttribute("accountKey", "");
            walletAtt.Value = order.WalletID.ToString();
            row.Attributes.Append(walletAtt);
            XmlAttribute durationAtt = xml.CreateAttribute("duration", "");
            durationAtt.Value = order.Duration.ToString();
            row.Attributes.Append(durationAtt);
            XmlAttribute escrowAtt = xml.CreateAttribute("escrow", "");
            escrowAtt.Value = order.Escrow.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
            row.Attributes.Append(escrowAtt);
            XmlAttribute priceAtt = xml.CreateAttribute("price", "");
            priceAtt.Value = order.Price.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
            row.Attributes.Append(priceAtt);
            XmlAttribute bidAtt = xml.CreateAttribute("bid", "");
            bidAtt.Value = order.BuyOrder ? "1" : "0";
            row.Attributes.Append(bidAtt);
            XmlAttribute issuedAtt = xml.CreateAttribute("issued", "");
            issuedAtt.Value = order.Issued.ToString(System.Globalization.DateTimeFormatInfo.InvariantInfo);
            row.Attributes.Append(issuedAtt);

            return row;
        }

        private XmlNode NewMarketOrderRowset(XmlDocument xml)
        {
            XmlNode rowSet = xml.CreateNode(XmlNodeType.Element, "rowset", "");

            XmlAttribute nameAtt = xml.CreateAttribute("name", "");
            nameAtt.Value = "orders";
            rowSet.Attributes.Append(nameAtt);
            XmlAttribute keyAtt = xml.CreateAttribute("key", "");
            keyAtt.Value = "orderID";
            rowSet.Attributes.Append(keyAtt);
            XmlAttribute colAtt = xml.CreateAttribute("columns", "");
            colAtt.Value = "orderID,charID,stationID,volEntered,volRemaining,minVolume,orderState,typeID,range,accountKey,duration,escrow,price,bid,issued";
            rowSet.Attributes.Append(colAtt);

            return rowSet;
        }
        #endregion

        #region Industry Jobs
        private void ExportIndustryJobXML(string dir, APICharacter character)
        {
            string filename = Path.Combine(dir, character.CharName + " [" +
                character.CharID + "]" + " IndustryJobs.xml");
            EMMADataSet.IndustryJobsDataTable industryJobs = IndustryJobs.GetJobs();

            XmlDocument xml = new XmlDocument();

            XmlNode eveAPINode = xml.CreateNode(XmlNodeType.Element, "eveapi", "");
            XmlAttribute versionNode = xml.CreateAttribute("version", "");
            versionNode.Value = "2";
            eveAPINode.Attributes.Append(versionNode);

            XmlNode currentTimeNode = xml.CreateNode(XmlNodeType.Element, "currentTime", "");
            currentTimeNode.InnerText = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            eveAPINode.AppendChild(currentTimeNode);

            XmlNode resultNode = xml.CreateNode(XmlNodeType.Element, "result", "");
            XmlNode rootRowSetNode = AddIndustryJobsToXML(xml, industryJobs.Where(j => j.InstallerID == character.CharID));
            resultNode.AppendChild(rootRowSetNode);
            eveAPINode.AppendChild(resultNode);

            xml.AppendChild(eveAPINode);
            xml.Save(filename);

        }

        private XmlNode AddIndustryJobsToXML(XmlDocument xml, EnumerableRowCollection<EMMADataSet.IndustryJobsRow> industryJobs)
        {
            XmlNode rowSet = NewIndustryJobRowset(xml);

            foreach (EveMarketMonitorApp.DatabaseClasses.EMMADataSet.IndustryJobsRow job in industryJobs)
            {
                XmlNode node = AddIndustryJobRow(xml, job);
                rowSet.AppendChild(node);
            }

            return rowSet;
        }

        private XmlNode AddIndustryJobRow(XmlDocument xml, EveMarketMonitorApp.DatabaseClasses.EMMADataSet.IndustryJobsRow job)
        {
            XmlNode row = xml.CreateNode(XmlNodeType.Element, "row", "");

            XmlAttribute idAtt = xml.CreateAttribute("jobID", "");
            idAtt.Value = job.ID.ToString();
            row.Attributes.Append(idAtt);
            XmlAttribute assemblyLineIDAtt = xml.CreateAttribute("assemblyLineID", "");
            assemblyLineIDAtt.Value = job.AssemblyLineID.ToString();
            row.Attributes.Append(assemblyLineIDAtt);
            XmlAttribute containerIDAtt = xml.CreateAttribute("containerID", "");
            containerIDAtt.Value = job.ContainerID.ToString();
            row.Attributes.Append(containerIDAtt);
            XmlAttribute installedItemIDAtt = xml.CreateAttribute("installedItemID", "");
            installedItemIDAtt.Value = job.InstalledItemID.ToString();
            row.Attributes.Append(installedItemIDAtt);
            XmlAttribute installedItemLocationIDAtt = xml.CreateAttribute("installedItemLocationID", "");
            installedItemLocationIDAtt.Value = job.InstalledItemLocationID.ToString();
            row.Attributes.Append(installedItemLocationIDAtt);
            XmlAttribute installedItemQuantityAtt = xml.CreateAttribute("installedItemQuantity", "");
            installedItemQuantityAtt.Value = job.InstalledItemQuantity.ToString();
            row.Attributes.Append(installedItemQuantityAtt);
            XmlAttribute installedItemProductivityLevelAtt = xml.CreateAttribute("installedItemProductivityLevel", "");
            installedItemProductivityLevelAtt.Value = job.InstalledItemPL.ToString();
            row.Attributes.Append(installedItemProductivityLevelAtt);
            XmlAttribute installedItemMaterialLevelAtt = xml.CreateAttribute("installedItemMaterialLevel", "");
            installedItemMaterialLevelAtt.Value = job.InstalledItemME.ToString();
            row.Attributes.Append(installedItemMaterialLevelAtt);
            XmlAttribute installedItemLicensedProductionRunsRemainingAtt =
                xml.CreateAttribute("installedItemLicensedProductionRunsRemaining", "");
            installedItemLicensedProductionRunsRemainingAtt.Value = job.InstalledItemRunsRemaining.ToString();
            row.Attributes.Append(installedItemLicensedProductionRunsRemainingAtt);
            XmlAttribute outputLocationIDAtt = xml.CreateAttribute("outputLocationID", "");
            outputLocationIDAtt.Value = job.OutputLcoationID.ToString();
            row.Attributes.Append(outputLocationIDAtt);
            XmlAttribute installerIDAtt = xml.CreateAttribute("installerID", "");
            installerIDAtt.Value = job.InstallerID.ToString();
            row.Attributes.Append(installerIDAtt);
            XmlAttribute runsAtt = xml.CreateAttribute("runs", "");
            runsAtt.Value = job.JobRuns.ToString();
            row.Attributes.Append(runsAtt);
            XmlAttribute licensedProductionRunsAtt = xml.CreateAttribute("licensedProductionRuns", "");
            licensedProductionRunsAtt.Value = job.OutputRuns.ToString();
            row.Attributes.Append(licensedProductionRunsAtt);
            EveDataSet.staStationsRow station = Stations.GetStation(job.OutputLcoationID);
            if (station != null)
            {
                XmlAttribute installedInSolarSystemIDAtt = xml.CreateAttribute("installedInSolarSystemID", "");
                installedInSolarSystemIDAtt.Value = station.solarSystemID.ToString();
                row.Attributes.Append(installedInSolarSystemIDAtt);
            }
            XmlAttribute containerLocationIDAtt = xml.CreateAttribute("containerLocationID", "");
            containerLocationIDAtt.Value = job.OutputLcoationID.ToString();
            row.Attributes.Append(containerLocationIDAtt);
            XmlAttribute materialMultiplierAtt = xml.CreateAttribute("materialMultiplier", "");
            materialMultiplierAtt.Value = job.MaterialModifier.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
            row.Attributes.Append(materialMultiplierAtt);
            XmlAttribute charMaterialMultiplierAtt = xml.CreateAttribute("charMaterialMultiplier", "");
            charMaterialMultiplierAtt.Value = job.CharMaterialModifier.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
            row.Attributes.Append(charMaterialMultiplierAtt);
            XmlAttribute timeMultiplierAtt = xml.CreateAttribute("timeMultiplier", "");
            timeMultiplierAtt.Value = job.TimeMultiplier.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
            row.Attributes.Append(timeMultiplierAtt);
            XmlAttribute charTimeMultiplierAtt = xml.CreateAttribute("charTimeMultiplier", "");
            charTimeMultiplierAtt.Value = job.CharTimeMultiplier.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
            row.Attributes.Append(charTimeMultiplierAtt);
            XmlAttribute installedItemTypeIDAtt = xml.CreateAttribute("installedItemTypeID", "");
            installedItemTypeIDAtt.Value = job.InstalledItemTypeID.ToString();
            row.Attributes.Append(installedItemTypeIDAtt);
            XmlAttribute outputTypeIDAtt = xml.CreateAttribute("outputTypeID", "");
            outputTypeIDAtt.Value = job.OutputTypeID.ToString();
            row.Attributes.Append(outputTypeIDAtt);
            XmlAttribute containerTypeIDAtt = xml.CreateAttribute("containerTypeID", "");
            containerTypeIDAtt.Value = job.ContainerTypeID.ToString();
            row.Attributes.Append(containerTypeIDAtt);
            XmlAttribute installedItemCopyAtt = xml.CreateAttribute("installedItemCopy", "");
            installedItemCopyAtt.Value = job.InstalledItemCopy ? "1" : "0";
            row.Attributes.Append(installedItemCopyAtt);
            XmlAttribute completedAtt = xml.CreateAttribute("completed", "");
            completedAtt.Value = job.Completed ? "1" : "0";
            row.Attributes.Append(completedAtt);
            XmlAttribute completedSuccessfullyAtt = xml.CreateAttribute("completedSuccessfully", "");
            completedSuccessfullyAtt.Value = job.CompletedSuccessfully ? "1" : "0";
            row.Attributes.Append(completedSuccessfullyAtt);
            XmlAttribute installedItemFlagAtt = xml.CreateAttribute("installedItemFlag", "");
            installedItemFlagAtt.Value = job.InstalledItemFlag.ToString();
            row.Attributes.Append(installedItemFlagAtt);
            XmlAttribute outputFlagAtt = xml.CreateAttribute("outputFlag", "");
            outputFlagAtt.Value = job.OutputFlag.ToString();
            row.Attributes.Append(outputFlagAtt);
            XmlAttribute activityAtt = xml.CreateAttribute("activityID", "");
            activityAtt.Value = job.ActivityID.ToString();
            row.Attributes.Append(activityAtt);
            XmlAttribute completedStatusAtt = xml.CreateAttribute("completedStatus", "");
            completedStatusAtt.Value = job.CompletedStatus.ToString();
            row.Attributes.Append(completedStatusAtt);
            XmlAttribute installTimeAtt = xml.CreateAttribute("installTime", "");
            installTimeAtt.Value = job.InstallTime.ToString(System.Globalization.DateTimeFormatInfo.InvariantInfo);
            row.Attributes.Append(installTimeAtt);
            XmlAttribute beginProductionTimeAtt = xml.CreateAttribute("beginProductionTime", "");
            beginProductionTimeAtt.Value = job.BeginProductionTime.ToString(System.Globalization.DateTimeFormatInfo.InvariantInfo);
            row.Attributes.Append(beginProductionTimeAtt);
            XmlAttribute endProductionTimeAtt = xml.CreateAttribute("endProductionTime", "");
            endProductionTimeAtt.Value = job.EndProductionTime.ToString(System.Globalization.DateTimeFormatInfo.InvariantInfo);
            row.Attributes.Append(endProductionTimeAtt);
            XmlAttribute pauseProductionTimeAtt = xml.CreateAttribute("pauseProductionTime", "");
            pauseProductionTimeAtt.Value = job.PauseProductionTime.ToString(System.Globalization.DateTimeFormatInfo.InvariantInfo);
            row.Attributes.Append(pauseProductionTimeAtt);

            return row;
        }

        private XmlNode NewIndustryJobRowset(XmlDocument xml)
        {
            XmlNode rowSet = xml.CreateNode(XmlNodeType.Element, "rowset", "");

            XmlAttribute nameAtt = xml.CreateAttribute("name", "");
            nameAtt.Value = "jobs";
            rowSet.Attributes.Append(nameAtt);
            XmlAttribute keyAtt = xml.CreateAttribute("key", "");
            keyAtt.Value = "jobID";
            rowSet.Attributes.Append(keyAtt);
            XmlAttribute colAtt = xml.CreateAttribute("columns", "");
            colAtt.Value = "jobID,assemblyLineID,containerID,installedItemID,installedItemLocationID,installedItemQuantity,installedItemProductivityLevel,installedItemMaterialLevel,installedItemLicensedProductionRunsRemaining,outputLocationID,installerID,runs,licensedProductionRuns,installedInSolarSystemID,containerLocationID,materialMultiplier,charMaterialMultiplier,timeMultiplier,charTimeMultiplier,installedItemTypeID,outputTypeID,containerTypeID,installedItemCopy,completed,completedSuccessfully,installedItemFlag,outputFlag,activityID,completedStatus,installTime,beginProductionTime,endProductionTime,pauseProductionTime";
            rowSet.Attributes.Append(colAtt);

            return rowSet;
        }
        #endregion

        #region Contracts
        // Note - cannot export these contracts as they don't have half the data needed.
        // when I put them in EMMA there was no contracts API so all the data in EMMA has been 
        // entered by users and it's just not enough for EMMA 2 to work with.
        private void ExportContractsXML(string dir, APICharacter character, bool forCorp)
        {
            string filename = Path.Combine(dir, (!forCorp ? character.CharName : character.CorpName) + " [" +
                (!forCorp ? character.CharID : character.CorpID) + "]" + " Contracts.xml");
            List<long> ownerIDs  = new List<long>();
            if (!forCorp){  ownerIDs.Add(character.CharID);}
            else{ ownerIDs.Add(character.CorpID);}
            ContractList contracts = Contracts.GetContracts(ownerIDs, 0, 0, 0, ContractType.Any);

            XmlDocument xml = new XmlDocument();

            XmlNode eveAPINode = xml.CreateNode(XmlNodeType.Element, "eveapi", "");
            XmlAttribute versionNode = xml.CreateAttribute("version", "");
            versionNode.Value = "2";
            eveAPINode.Attributes.Append(versionNode);

            XmlNode currentTimeNode = xml.CreateNode(XmlNodeType.Element, "currentTime", "");
            currentTimeNode.InnerText = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            eveAPINode.AppendChild(currentTimeNode);

            XmlNode resultNode = xml.CreateNode(XmlNodeType.Element, "result", "");
            XmlNode rootRowSetNode = AddContractsToXML(xml, contracts);
            resultNode.AppendChild(rootRowSetNode);
            eveAPINode.AppendChild(resultNode);

            xml.AppendChild(eveAPINode);
            xml.Save(filename);

        }

        private XmlNode AddContractsToXML(XmlDocument xml, ContractList contracts)
        {
            XmlNode rowSet = NewContractRowset(xml);

            foreach (Contract contract in contracts)
            {
                //XmlNode node = AddContractRow(xml, contract);
                //rowSet.AppendChild(node);
            }

            return rowSet;
        }

        private XmlNode AddContractRow(XmlDocument xml, Contract contract, long charID, long? corpID)
        {
            XmlNode row = xml.CreateNode(XmlNodeType.Element, "row", "");

            XmlAttribute idAtt = xml.CreateAttribute("contractID", "");
            idAtt.Value = contract.ID.ToString();
            row.Attributes.Append(idAtt);
            XmlAttribute issuerAtt = xml.CreateAttribute("issuerID", "");
            issuerAtt.Value = charID.ToString();
            row.Attributes.Append(issuerAtt);
            if (corpID.HasValue)
            {
                XmlAttribute issuerCorpAtt = xml.CreateAttribute("issuerCorpID", "");
                issuerCorpAtt.Value = corpID.ToString();
                row.Attributes.Append(issuerCorpAtt);
            }
            //if (contract.Assignee.HasValue)
            //{
            //    XmlAttribute assigneeAtt = xml.CreateAttribute("assigneeID", "");
            //    assigneeAtt.Value = contract.AssigneeID.ToString();
            //    row.Attributes.Append(assigneeAtt);
            //}
            //if (contract.AcceptorID.HasValue)
            //{
            //    XmlAttribute acceptorAtt = xml.CreateAttribute("acceptorID", "");
            //    acceptorAtt.Value = contract.AcceptorID.ToString();
            //    row.Attributes.Append(acceptorAtt);
            //}
            //XmlAttribute startStationAtt = xml.CreateAttribute("startStationID", "");
            //startStationAtt.Value = contract.StartStationID.ToString();
            //row.Attributes.Append(startStationAtt);
            //if (contract.EndStationID.HasValue)
            //{
            //    XmlAttribute endStationAtt = xml.CreateAttribute("endStationID", "");
            //    endStationAtt.Value = contract.EndStationID.ToString();
            //    row.Attributes.Append(endStationAtt);
            //}
            //XmlAttribute typeAtt = xml.CreateAttribute("type", "");
            //typeAtt.Value = contract.ContractType.ToString();
            //row.Attributes.Append(typeAtt);
            //XmlAttribute statusAtt = xml.CreateAttribute("status", "");
            //statusAtt.Value = contract.ContractStatus.ToString();
            //row.Attributes.Append(statusAtt);
            //XmlAttribute titleAtt = xml.CreateAttribute("title", "");
            //titleAtt.Value = contract.Title;
            //row.Attributes.Append(titleAtt);
            //XmlAttribute forCorpAtt = xml.CreateAttribute("forCorp", "");
            //forCorpAtt.Value = contract.ForCorp ? "1" : "0";
            //row.Attributes.Append(forCorpAtt);
            //XmlAttribute availabilityAtt = xml.CreateAttribute("availability", "");
            //availabilityAtt.Value = contract.ContractAvailability.ToString();
            //row.Attributes.Append(availabilityAtt);
            //XmlAttribute dateIssuedAtt = xml.CreateAttribute("dateIssued", "");
            //dateIssuedAtt.Value = contract.DateIssued.ToString(System.Globalization.DateTimeFormatInfo.InvariantInfo);
            //row.Attributes.Append(dateIssuedAtt);
            //if (contract.DateExpired.HasValue)
            //{
            //    XmlAttribute dateExpiredAtt = xml.CreateAttribute("dateExpired", "");
            //    dateExpiredAtt.Value = contract.DateExpired.Value.ToString(System.Globalization.DateTimeFormatInfo.InvariantInfo);
            //    row.Attributes.Append(dateExpiredAtt);
            //}
            //if (contract.DateAccepted.HasValue)
            //{
            //    XmlAttribute dateAcceptedAtt = xml.CreateAttribute("dateAccepted", "");
            //    dateAcceptedAtt.Value = contract.DateAccepted.Value.ToString(System.Globalization.DateTimeFormatInfo.InvariantInfo);
            //    row.Attributes.Append(dateAcceptedAtt);
            //}
            //if (contract.DateCompleted.HasValue)
            //{
            //    XmlAttribute dateCompletedAtt = xml.CreateAttribute("dateCompleted", "");
            //    dateCompletedAtt.Value = contract.DateCompleted.Value.ToString(System.Globalization.DateTimeFormatInfo.InvariantInfo);
            //    row.Attributes.Append(dateCompletedAtt);
            //}
            //XmlAttribute numDaysAtt = xml.CreateAttribute("numDays", "");
            //numDaysAtt.Value = contract.Days.ToString();
            //row.Attributes.Append(numDaysAtt);
            //if (contract.Price.HasValue)
            //{
            //    XmlAttribute priceAtt = xml.CreateAttribute("price", "");
            //    priceAtt.Value = contract.Price.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
            //    row.Attributes.Append(priceAtt);
            //}
            //if (contract.Reward.HasValue)
            //{
            //    XmlAttribute rewardAtt = xml.CreateAttribute("reward", "");
            //    rewardAtt.Value = contract.Reward.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
            //    row.Attributes.Append(rewardAtt);
            //}
            //if (contract.Collateral.HasValue)
            //{
            //    XmlAttribute collateralAtt = xml.CreateAttribute("collateral", "");
            //    collateralAtt.Value = contract.Collateral.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
            //    row.Attributes.Append(collateralAtt);
            //}
            //if (contract.Buyout.HasValue)
            //{
            //    XmlAttribute buyoutAtt = xml.CreateAttribute("buyout", "");
            //    buyoutAtt.Value = contract.Buyout.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
            //    row.Attributes.Append(buyoutAtt);
            //}
            //XmlAttribute volumeAtt = xml.CreateAttribute("volume", "");
            //volumeAtt.Value = contract.Volume.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
            //row.Attributes.Append(volumeAtt);

            return row;
        }

        private XmlNode NewContractRowset(XmlDocument xml)
        {
            XmlNode rowSet = xml.CreateNode(XmlNodeType.Element, "rowset", "");

            XmlAttribute nameAtt = xml.CreateAttribute("name", "");
            nameAtt.Value = "orders";
            rowSet.Attributes.Append(nameAtt);
            XmlAttribute keyAtt = xml.CreateAttribute("key", "");
            keyAtt.Value = "orderID";
            rowSet.Attributes.Append(keyAtt);
            XmlAttribute colAtt = xml.CreateAttribute("columns", "");
            colAtt.Value = "orderID,charID,stationID,volEntered,volRemaining,minVolume,orderState,typeID,range,accountKey,duration,escrow,price,bid,issued";
            rowSet.Attributes.Append(colAtt);

            return rowSet;
        }
        #endregion
    }
}