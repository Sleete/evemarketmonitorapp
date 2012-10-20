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
                            }
                            if (character.CorpIncWithRptGroup)
                            {
                                ExportTransactionXML(dir, character, true);
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

    }
}