using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlTypes;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public static class BankAccounts
    {        
        private static EMMADataSetTableAdapters.BankAccountTableAdapter bankAccountTableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.BankAccountTableAdapter();
        private static EMMADataSetTableAdapters.BankTransactionTableAdapter bankTransactionTableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.BankTransactionTableAdapter();

        public static void DeleteTransactions(BankTransactionsList transactions)
        {
            EMMADataSet.BankTransactionDataTable table = new EMMADataSet.BankTransactionDataTable();
            bankTransactionTableAdapter.ClearBeforeFill = false;

            foreach (BankTransaction trans in transactions)
            {
                bankTransactionTableAdapter.FillByID(table, trans.ID);
                EMMADataSet.BankTransactionRow row = table.FindByTransactionID(trans.ID);
                if (row != null) { row.Delete(); }
            }

            bankTransactionTableAdapter.Update(table);
            bankTransactionTableAdapter.ClearBeforeFill = true;
        }

        public static void StoreAccount(PublicCorp data, int reportGroupID, int ownerID)
        {
            EMMADataSet.BankAccountRow account = null;
            EMMADataSet.BankAccountDataTable table = new EMMADataSet.BankAccountDataTable();
            bool newRow = false;

            if (data.BankAccountID != 0)
            {
                bankAccountTableAdapter.FillByID(table, data.BankAccountID);
                account = table.FindByAccountID(data.BankAccountID);
            }

            if (account == null)
            {
                account = table.NewBankAccountRow();
                newRow = true;
            }

            account.PublicCorpID = data.ID;
            account.ReportGroupID = reportGroupID;
            account.Balance = 0;
            account.OwnerID = ownerID;
            if (newRow)
            {
                table.AddBankAccountRow(account);
            }
            bankAccountTableAdapter.Update(table);
        }

        public static void DeleteAccount(int corpID, int ownerID)
        {
            PublicCorp corp = PublicCorps.GetCorp(corpID);
            if (corp != null)
            {
                corp.OwnerID = ownerID;
                corp.ReloadBankAccountDetails();

                if (corp.BankAccountID > 0)
                {
                    DeleteTransactions(GetAccountTransactions(corp.BankAccountID));

                    EMMADataSet.BankAccountRow account = null;
                    EMMADataSet.BankAccountDataTable table = new EMMADataSet.BankAccountDataTable();

                    bankAccountTableAdapter.FillByID(table, corp.BankAccountID);
                    account = table.FindByAccountID(corp.BankAccountID);

                    if (account != null)
                    {
                        account.Delete();
                        bankAccountTableAdapter.Update(table);
                    }
                }
            }
        }

        public static double DaysAccountActive(int accountID)
        {
            double retVal = 0;
            int depositsPointer = 0;
            int withdrawlsPointer = 0;
            DateTime activeDateTime = DateTime.MinValue;

            // Get tables containing the deposits and withdrawls that have occured on this account..
            EMMADataSet.BankTransactionDataTable deposits = new EMMADataSet.BankTransactionDataTable();
            bankTransactionTableAdapter.FillByAny(deposits, accountID, 
                (short)BankTransactionType.Deposit, false);
            EMMADataSet.BankTransactionDataTable withdrawls = new EMMADataSet.BankTransactionDataTable();
            bankTransactionTableAdapter.FillByAny(withdrawls, accountID, 
                (short)BankTransactionType.Withdrawl, false);

            if (deposits.Count > 0)
            {
                activeDateTime = deposits[0].DateTime;

                // These tables will be ordered by date/time so we can just process sequentially...
                while (withdrawlsPointer < withdrawls.Count)
                {
                    // If the balance after the next withdrawl is 0 or less then add the
                    // days between the last deposit and the withdrawl to the total and
                    // skip to the next deposit.
                    // Otherwise, just move to the next withdrawl
                    if (BankAccounts.GetAccountBalance(accountID,
                        withdrawls[withdrawlsPointer].DateTime.AddSeconds(1)) <= 0)
                    {
                        DateTime inactiveDateTime = withdrawls[withdrawlsPointer].DateTime;
                        retVal += ((TimeSpan)inactiveDateTime.Subtract(activeDateTime)).TotalDays;
                        while (activeDateTime.CompareTo(inactiveDateTime) < 0 && 
                            depositsPointer + 1 < deposits.Count)
                        {
                            depositsPointer++;
                            activeDateTime = deposits[depositsPointer].DateTime;
                        }
                    }
                    withdrawlsPointer++;
                }
                // No more withdrawls to process, just add the number of days from
                // the last deposit to the current date if there is currently cash
                // in the account.
                if (BankAccounts.GetAccountBalance(accountID, DateTime.UtcNow) > 0)
                {
                    retVal += ((TimeSpan)DateTime.UtcNow.Subtract(activeDateTime)).TotalDays;
                }
            }

            return retVal;
        }

        public static BankTransactionsList GetAccountTransactions(int accountID)
        {
            BankTransactionsList retVal = new BankTransactionsList();
            EMMADataSet.BankTransactionDataTable table = new EMMADataSet.BankTransactionDataTable();
            bankTransactionTableAdapter.FillByAny(table, accountID, 0, false);
            foreach (EMMADataSet.BankTransactionRow trans in table)
            {
                retVal.Add(new BankTransaction(trans));
            }
            return retVal;
        }

        public static decimal GetTotalTransactionIsk(int accountID, BankTransactionType type)
        {
            decimal? retVal = 9999999999999999.99m;
            bankTransactionTableAdapter.GetTotal(accountID, (short)type, ref retVal);
            return (retVal.HasValue ? (retVal.Value == 9999999999999999.99m ? 0 : retVal.Value) : 0);
        }

        public static void ClearAllInterestPayments(int corpID)
        {
            bankTransactionTableAdapter.ClearAfterDate(0, corpID, SqlDateTime.MinValue.Value,
                (short)BankTransactionType.InterestPayment);
        }

        public static void RecalculateInterestAfter(int accountID, DateTime date)
        {
            // clear existing interest after the specified date.
            bankTransactionTableAdapter.ClearAfterDate(accountID, 0, date, 
                (short)BankTransactionType.InterestPayment);
            // Pay any outstanding interest on the account.
            PayOutstandingInterest(accountID);
        }

        public static void PayOutstandingInterest(int accountID)
        {
            bool cancel = false;
            EMMADataSet.BankAccountRow accountData = null;
            EMMADataSet.BankAccountDataTable accountTable = new EMMADataSet.BankAccountDataTable();
            CorpPayoutPeriod interestPeriod = CorpPayoutPeriod.Unspecified;
            decimal interestPercentage = 0;
            decimal balance = 0;

            bankAccountTableAdapter.FillByID(accountTable, accountID);
            if (accountTable.Count > 0)
            {
                accountData = accountTable[0];
                PublicCorp corpData = PublicCorps.GetCorp(accountData.PublicCorpID);
                if (corpData == null)
                {
                    cancel = true;
                }
                else
                {
                    interestPeriod = corpData.PayoutPeriod;
                    interestPercentage = corpData.ExpectedPayout;
                }
            }
            else
            {
                cancel = true;
            }

            EMMADataSet.BankTransactionDataTable table = new EMMADataSet.BankTransactionDataTable();

            // First work out when the last interest payment was made.
            // If there isn't one then use the oldest deposit/withdrawl/manual adjustment date.
            // If there's still nothing then we just need to clear the account balance.
            DateTime lastInterestPaymentDate = DateTime.MaxValue;
            bankTransactionTableAdapter.FillByAny(table, accountID,
                (short)BankTransactionType.InterestPayment, true);
            if (table.Count > 0)
            {
                lastInterestPaymentDate = table[0].DateTime;
            }
            else
            {
                bankTransactionTableAdapter.FillByAny(table, accountID,
                    0, false);
                if (table.Count > 0)
                {
                    foreach (EMMADataSet.BankTransactionRow transaction in table)
                    {
                        if (lastInterestPaymentDate.CompareTo(transaction.DateTime) > 0)
                        {
                            lastInterestPaymentDate = transaction.DateTime;
                        }
                    }
                }
                else
                {
                    cancel = true;
                }
            }

            if (cancel)
            {
                // We have no transactions so set account balance to zero.
                if (accountData != null)
                {
                    accountData.Balance = 0;
                    bankAccountTableAdapter.Update(accountData);
                }
            }
            else
            {
                // Now get the time interval between interest payments...
                TimeSpan interestPaymentInterval = new TimeSpan();
                EMMADataSet.PublicCorpPayoutPeriodDataTable periods = CorpPayoutPeriods.GetAll();
                EMMADataSet.PublicCorpPayoutPeriodRow period = periods.FindByID((short)interestPeriod);

                if (interestPeriod != CorpPayoutPeriod.Unspecified && period != null)
                {
                    if (!period.IsDaysNull())
                    {
                        interestPaymentInterval = new TimeSpan(period.Days, 0, 0, 0);
                    }
                }

                if (interestPaymentInterval.TotalDays >= 1)
                {
                    balance = GetAccountBalance(accountID,
                        lastInterestPaymentDate.Add(interestPaymentInterval));

                    // Create any missing interest payments.
                    while (DateTime.UtcNow.CompareTo(
                        lastInterestPaymentDate.Date.Add(interestPaymentInterval)) > 0)
                    {
                        lastInterestPaymentDate = lastInterestPaymentDate.Date.Add(
                            interestPaymentInterval);
                        decimal change = Math.Round(balance * (interestPercentage / 100.0m), 2);

                        if (change >= 0.01m)
                        {
                            StoreBankTransaction(new BankTransaction(lastInterestPaymentDate,
                                accountID, change, BankTransactionType.InterestPayment));
                            balance += change;
                        }

                        // Apply any deposits or withdrawls made between the last payment date and the
                        // next interest interval.
                        if (table.Count > 0)
                        {
                            foreach (EMMADataSet.BankTransactionRow transaction in table)
                            {
                                if (transaction.DateTime.CompareTo(lastInterestPaymentDate) > 0 &&
                                    transaction.DateTime.CompareTo(lastInterestPaymentDate.Add(interestPaymentInterval)) < 0)
                                {
                                    balance += transaction.Change;
                                }
                            }
                        }
                    }

                    // Update the current balance figure on the account.
                    accountData.Balance = balance;
                    bankAccountTableAdapter.Update(accountData);
                }
            }
        }

        public static decimal GetAccountBalance(int corpID, int reportGroupID, int ownerID)
        {
            int accountID = GetSingleBankAccountData(reportGroupID, ownerID, corpID).AccountID;
            return GetAccountBalance(accountID, DateTime.UtcNow);
        }

        public static decimal GetAccountBalance(int accountID)
        {
            return GetAccountBalance(accountID, DateTime.UtcNow);
        }

        public static decimal GetAccountBalance(int accountID, DateTime date)
        {
            decimal? retVal = 9999999999999999.99m;
            bankTransactionTableAdapter.GetAccountBalance(accountID, date, ref retVal);
            return (retVal.HasValue ? (retVal.Value == 9999999999999999.99m ? 0 : retVal.Value) : 0);
        }

        public static void StoreBankTransaction(BankTransaction data)
        {
            EMMADataSet.BankTransactionRow transaction = null;
            EMMADataSet.BankTransactionDataTable table = new EMMADataSet.BankTransactionDataTable();

            if (data.ID == 0)
            {
                transaction = table.NewBankTransactionRow();
            }
            transaction.AccountID = data.AccountID;
            transaction.DateTime = data.Date;
            transaction.Change = data.Change;
            transaction.Type = (short)data.Type;
            if (data.ID == 0)
            {
                table.AddBankTransactionRow(transaction);
            }
            bankTransactionTableAdapter.Update(table);
        }


        public static EMMADataSet.BankAccountRow GetSingleBankAccountData(int reportGroupID, int ownerID, int corpID)
        {
            EMMADataSet.BankAccountRow retVal = null;
            EMMADataSet.BankAccountDataTable table = GetBankAccountData(reportGroupID, ownerID, corpID);
            if (table.Count > 0)
            {
                retVal = table[0];
            }
            return retVal;
        }
        public static EMMADataSet.BankAccountDataTable GetBankAccountData(int reportGroupID, int ownerID, int corpID)
        {
            EMMADataSet.BankAccountDataTable table = new EMMADataSet.BankAccountDataTable();
            bankAccountTableAdapter.FillByCorpAndOwner(table, corpID, reportGroupID, ownerID);
            return table;
        }
    }
}
