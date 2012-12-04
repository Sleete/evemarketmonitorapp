using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlTypes;
using System.Xml;
using System.Data;

using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.AbstractionClasses;

namespace EveMarketMonitorApp.DatabaseClasses
{
    class Journal : IProvideStatus
    {
        private static EMMADataSetTableAdapters.JournalTableAdapter tableAdapter = 
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.JournalTableAdapter();

        public event StatusChangeHandler StatusChange;
       

        public static bool HasBrokerFeePayment(long ownerID, DateTime date, decimal expectedFee)
        {
            EMMADataSet.JournalDataTable table = new EMMADataSet.JournalDataTable();
            bool retVal = false;
            DateTime startDate, endDate;
            date = date.ToUniversalTime();
            startDate = date.AddMinutes(-1);
            endDate = date.AddMinutes(1);
            lock (tableAdapter)
            {
                tableAdapter.FillByRefTypeAndTime(table, ownerID, startDate, endDate, 46);
            }
            foreach (EMMADataSet.JournalRow row in table)
            {
                if (row.Amount > expectedFee * 0.95m && row.Amount < expectedFee * 1.05m)
                {
                    retVal = true;
                }
            }

            return retVal;
        }

        public static EMMADataSet.JournalRow GetClosest(long ownerID, bool corp, short walletID, DateTime date)
        {
            EMMADataSet.JournalDataTable table = new EMMADataSet.JournalDataTable();
            EMMADataSet.JournalRow retVal = null;
            date = date.ToUniversalTime();
            lock (tableAdapter)
            {
                tableAdapter.FillByClosest(table, ownerID, corp, walletID, date);
            }
            if (table.Count > 0) { retVal = table[0]; }

            return retVal;
        }


        public static EMMADataSet.JournalDataTable GetUnprocessedDivs()
        {
            EMMADataSet.JournalDataTable table = new EMMADataSet.JournalDataTable();
            lock (tableAdapter)
            {
                tableAdapter.FillByUnprocessedDivs(table, FinanceAccessParams.BuildAccessList(
                    UserAccount.CurrentGroup.GetFinanceAccessParams(APIDataType.Journal)));
            }
            return table;
        }

        public static decimal GetTotAmtByType(int refTypeID, DateTime startDate, DateTime endDate,
            EntryType type, List<FinanceAccessParams> financeAccessParams)
        {
            // A bug in SQL/CLR interaction means that SQL will treat output parameters of
            // type decimal as decimal(s,p) where s and p are defined at run-time by the value
            // passed in rather than the definition in the actual procedure.
            // i.e. If sumVal is set to zero, errors will occur when SQL tries to put anything
            // bigger than 9 in it...
            decimal? sumVal = 1234567890123456.00m;
            startDate = startDate.ToUniversalTime();
            endDate = endDate.ToUniversalTime();
            if (startDate.CompareTo(SqlDateTime.MinValue.Value) < 0) startDate = SqlDateTime.MinValue.Value;
            if (endDate.CompareTo(SqlDateTime.MinValue.Value) < 0) endDate = SqlDateTime.MinValue.Value;
            if (startDate.CompareTo(SqlDateTime.MaxValue.Value) > 0) startDate = SqlDateTime.MaxValue.Value;
            if (endDate.CompareTo(SqlDateTime.MaxValue.Value) > 0) endDate = SqlDateTime.MaxValue.Value;

            tableAdapter.SumByType(FinanceAccessParams.BuildAccessList(financeAccessParams), refTypeID,
                startDate, endDate, (type == EntryType.Expense ? "expense" : "revenue"), ref sumVal);
            return sumVal.HasValue ? (sumVal.Value == 1234567890123456.00m ? 0 : sumVal.Value) : 0;
        }

        public static JournalList LoadEntries(List<FinanceAccessParams> accessParams, List<short> typeIDs,
            DateTime startDate, DateTime endDate)
        {
            return LoadEntries(accessParams, typeIDs, startDate, endDate, "");
        }

        public static JournalList LoadEntries(List<FinanceAccessParams> accessParams, List<short> typeIDs,
            DateTime startDate, DateTime endDate, string nameProfile)
        {
            JournalList retVal = new JournalList();

            EMMADataSet.JournalDataTable table = LoadEntriesData(accessParams, typeIDs, startDate, endDate, nameProfile);

            Diagnostics.StartTimer("Journal.LoadEntries.BuildList");
            foreach (EMMADataSet.JournalRow row in table)
            {
                // We need to build the journal rows differently depending on who the owner is...
                long sOwner = 0, rOwner = 0;
                foreach (FinanceAccessParams access in accessParams)
                {
                    if ((access.OwnerID == row.SenderID && sOwner == 0) || access.OwnerID == row.SCorpID)
                    {
                        sOwner = access.OwnerID;
                    }
                    if ((access.OwnerID == row.RecieverID && rOwner == 0) || access.OwnerID == row.RCorpID)
                    {
                        rOwner = access.OwnerID;
                    }
                }

                if (rOwner != 0) { retVal.Add(new JournalEntry(row, rOwner)); }
                if (sOwner != 0) { retVal.Add(new JournalEntry(row, sOwner)); }
            }
            Diagnostics.StopTimer("Journal.LoadEntries.BuildList");
            return retVal;
        }

        public static EMMADataSet.JournalDataTable LoadEntriesData(List<FinanceAccessParams> accessParams, 
            List<short> typeIDs, DateTime startDate, DateTime endDate, string nameProfile)
        {
            EMMADataSet.JournalDataTable table = new EMMADataSet.JournalDataTable();

            startDate = startDate.ToUniversalTime();
            endDate = endDate.ToUniversalTime();
            if (startDate.CompareTo(SqlDateTime.MinValue.Value) < 0) startDate = SqlDateTime.MinValue.Value;
            if (endDate.CompareTo(SqlDateTime.MinValue.Value) < 0) endDate = SqlDateTime.MinValue.Value;
            if (startDate.CompareTo(SqlDateTime.MaxValue.Value) > 0) startDate = SqlDateTime.MaxValue.Value;
            if (endDate.CompareTo(SqlDateTime.MaxValue.Value) > 0) endDate = SqlDateTime.MaxValue.Value;

            string typeString = "";
            foreach (short typeID in typeIDs) { typeString = typeString + (typeString.Length == 0 ? "" : ",") + typeID; }
            lock (tableAdapter)
            {
                Diagnostics.StartTimer("Journal.LoadEntries.Database");
                if (string.IsNullOrEmpty(nameProfile))
                {
                    tableAdapter.FillByAny(table, FinanceAccessParams.BuildAccessList(accessParams), typeString,
                        startDate, endDate);
                }
                else
                {
                    tableAdapter.FillByAnyAndName(table, FinanceAccessParams.BuildAccessList(accessParams), 
                        typeString, startDate, endDate, nameProfile);
                }
                Diagnostics.StopTimer("Journal.LoadEntries.Database");
            }
            return table;
        }
        
        public static void Store(EMMADataSet.JournalDataTable table)
        {
            lock (tableAdapter)
            {
                tableAdapter.Update(table);
            }
        }


        public static bool EntryExists(EMMADataSet.JournalDataTable table, long ID, long recieverID)
        {
            bool? exists = false;

            lock (tableAdapter)
            {
                tableAdapter.ClearBeforeFill = false;
                tableAdapter.FillEntryExists(table, ID, recieverID, ref exists);
            }

            return (exists.HasValue ? exists.Value : false);
        }


        public static EMMADataSet.JournalRow FindEntry(DateTime date, long senderID, long recieverID, decimal amount)
        {
            EMMADataSet.JournalDataTable table = new EMMADataSet.JournalDataTable();
            EMMADataSet.JournalRow retVal = null;
            date = date.ToUniversalTime();
            lock (tableAdapter)
            {
                tableAdapter.FindMatch(table, date, recieverID, senderID, amount);
            }

            // Only return a match if there is one and only one matching record found.
            if (table.Count == 1)
            {
                retVal = table[0];
            }

            return retVal;
        }


        public void LoadOldEmmaXML(string filename, long charID, long corpID) 
        {
            EMMADataSet.JournalDataTable table = new EMMADataSet.JournalDataTable();
            XmlDocument xml = new XmlDocument();
            //UpdateStatus(0, 0, "", "Loading file", false);
            xml.Load(filename);

            XmlNodeList nodes = xml.SelectNodes("/DocumentElement/Journal");

            int counter = 0;
            UpdateStatus(0, 0, "", "Extracting data from XML", false);
            foreach (XmlNode node in nodes)
            {
                bool tryUpdate = true;
                long id = long.Parse(node.SelectSingleNode("ID").FirstChild.Value) + 1085796677;
                long recieverID = long.Parse(node.SelectSingleNode("OwnerID2").FirstChild.Value);

                if (!EntryExists(table, id, recieverID))
                {
                    EMMADataSet.JournalRow tmpRow = table.FindByIDRecieverID(id, recieverID);
                    if (tmpRow == null)
                    {
                        EMMADataSet.JournalRow newRow = BuildRow(corpID, node, id, recieverID, table);
                        table.AddJournalRow(newRow);
                        tryUpdate = false;
                    }
                }
                if (tryUpdate)
                {
                    EMMADataSet.JournalRow newRow = BuildRow(corpID, node, id, recieverID, table);
                    EMMADataSet.JournalRow oldRow = table.FindByIDRecieverID(newRow.ID,
                        newRow.RecieverID);

                    if ((newRow.RBalance > 0 && oldRow.RBalance == 0) ||
                        (newRow.RCorpID != 0 && oldRow.RCorpID == 0))
                    {
                        oldRow.RBalance = newRow.RBalance;
                        oldRow.RCorpID = newRow.RCorpID;
                        oldRow.RArgID = newRow.RArgID;
                        oldRow.RArgName = newRow.RArgName;
                        oldRow.RWalletID = newRow.RWalletID;
                    }
                    if ((newRow.SBalance > 0 && oldRow.SBalance == 0) ||
                        (newRow.SCorpID != 0 && oldRow.SCorpID == 0))
                    {
                        oldRow.SBalance = newRow.SBalance;
                        oldRow.SCorpID = newRow.SCorpID;
                        oldRow.SArgID = newRow.SArgID;
                        oldRow.SArgName = newRow.SArgName;
                        oldRow.SWalletID = newRow.SWalletID;
                    }
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

        private EMMADataSet.JournalRow BuildRow(long corpID, XmlNode node, long id, long recieverID,
            EMMADataSet.JournalDataTable table)
        {
            EMMADataSet.JournalRow newRow = table.NewJournalRow();
            XmlNode child = null;
            newRow.ID = id;
            newRow.RecieverID = recieverID;
            newRow.SenderID = long.Parse(node.SelectSingleNode("OwnerID1").FirstChild.Value);
            newRow.Date = DateTime.Parse(node.SelectSingleNode("Date").FirstChild.Value,
                System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
            newRow.TypeID = short.Parse(node.SelectSingleNode("TypeID").FirstChild.Value);
            decimal amount = decimal.Parse(node.SelectSingleNode("Amount").FirstChild.Value,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            newRow.Amount = Math.Abs(amount);
            if (amount < 0)
            {
                newRow.SBalance = decimal.Parse(node.SelectSingleNode("Balance").FirstChild.Value,
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                newRow.SWalletID = short.Parse(node.SelectSingleNode("WalletID").FirstChild.Value);
                child = node.SelectSingleNode("ArgName1").FirstChild;
                newRow.SArgName = child == null ? "" : child.Value;
                child = node.SelectSingleNode("ArgID1").FirstChild;
                newRow.SArgID = long.Parse(child == null ? "0" : child.Value,
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                newRow.SCorpID = corpID;

                newRow.RBalance = 0;
                newRow.RWalletID = 0;
                newRow.RArgName = "";
                newRow.RArgID = 0;
                newRow.RCorpID = 0;
            }
            else
            {
                newRow.RBalance = decimal.Parse(node.SelectSingleNode("Balance").FirstChild.Value,
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                newRow.RWalletID = short.Parse(node.SelectSingleNode("WalletID").FirstChild.Value);
                child = node.SelectSingleNode("ArgName1").FirstChild;
                newRow.RArgName = child == null ? "" : child.Value;
                child = node.SelectSingleNode("ArgID1").FirstChild;
                newRow.RArgID = long.Parse(child == null ? "0" : child.Value,
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                newRow.RCorpID = corpID;

                newRow.SBalance = 0;
                newRow.SWalletID = 0;
                newRow.SArgName = "";
                newRow.SArgID = 0;
                newRow.SCorpID = 0;
            }
            // Reason text can be no longer than 50 chars so truncate it if needed...
            child = node.SelectSingleNode("Reason").FirstChild;
            string reason = child == null ? "" : child.Value;
            newRow.Reason = (reason.Length > 50 ? reason.Remove(50) : reason);
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


        private struct TmpDiv
        {
            public DateTime date;
            public decimal payoutPerShare;

            public TmpDiv(DateTime date, decimal payoutPerShare)
            {
                this.date = date;
                this.payoutPerShare = payoutPerShare;
            }
        }
    }

    public enum EntryType
    {
        Revenue,
        Expense
    }
}
