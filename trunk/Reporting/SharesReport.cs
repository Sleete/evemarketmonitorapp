using System;
using System.Collections.Generic;
using System.Text;

using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.DatabaseClasses;

namespace EveMarketMonitorApp.Reporting
{
    class SharesReport : ReportBase
    {
        private static string[] _allColumnNames = { "Avg Buy Price", "Avg Sell Price", 
            "Units Bought", "Units Sold",
            "Avg Divs Per Share", "Share Value", "Total Dividends" , "Total Value of Shares", 
            "Profit", "Profit %", "Daily Profit %"};
        private int _reportGroupID;

        public SharesReport()
        {
            _name = "Investment Report";
            _title = "Investment Report";
            _allowSort = true;

            _expectedParams = new string[2];
            _expectedParams[0] = "ColumnsVisible";
            _expectedParams[1] = "ReportGroupID";

        }
        
        public static string[] GetPossibleColumns()
        {
            return _allColumnNames;
        }

        protected override void InitSections()
        {
            _sections = new ReportSections();
            _sections.Add(new ReportSection(_columns.Length, "All Corps", "All Corps", this));
        }

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
                    if (_expectedParams[i].Equals("ReportGroupID")) _reportGroupID = (int)paramValue;
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
                        if (_allColumnNames[i].Contains("%"))
                        {
                            _columns[colNum].DataType = ReportDataType.Percentage;
                        }
                        else if (_allColumnNames[i].Contains("Units"))
                        {
                            _columns[colNum].DataType = ReportDataType.Number;
                        }
                        else
                        {
                            _columns[colNum].DataType = ReportDataType.ISKAmount;
                        }

                        if (_allColumnNames[i].Contains("Total") || _allColumnNames[i].Equals("Profit"))
                        {
                            _columns[colNum].SectionRowBehavior = SectionRowBehavior.Sum;
                        }
                        else
                        {
                            _columns[colNum].SectionRowBehavior = SectionRowBehavior.Blank;
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
            PublicCorpsList pubCorps = PublicCorps.GetAll();

            int maxProgress = pubCorps.Count;
            UpdateStatus(0, maxProgress, "Getting Report Data...", "", false);

            // Cycle through all public corps
            for (int j = 0; j < pubCorps.Count; j++)
            {
                PublicCorp corpData = pubCorps[j];
                string corpName = corpData.Name;
                int corpID = corpData.ID;
                bool createRow = false;

                decimal avgBuyPrice = 0, avgSellPrice = 0, totBuyVolume = 0, totSellVolume = 0;
                decimal avgDivPerShare = 0, totDivs = 0, shareValue = 0/*, sharesIssued = 0*/       ;
                decimal sharesOwnedVal = 0, profitPerc = 0, profitPercIncValue = 0, dailyProfit = 0;
                decimal investedDays = 0, profit = 0;

                if (!corpData.Bank)
                {
                    ShareTransactions.GetCorpTransInfo(_reportGroupID, corpID, ref avgBuyPrice, ref avgSellPrice,
                        ref totBuyVolume, ref totSellVolume, ref investedDays);
                    createRow = investedDays > 0 &&
                        (avgBuyPrice * totBuyVolume) > 0;
                    if (createRow)
                    {
                        Dividends.GetCorpDivInfo(corpID, _reportGroupID, ref avgDivPerShare, ref totDivs);

                        // Calculate data for columns
                        shareValue = corpData.ShareValue;
                        sharesOwnedVal = shareValue * (totBuyVolume - totSellVolume);
                        profit = (((avgSellPrice * totSellVolume) + totDivs + sharesOwnedVal) - (avgBuyPrice * totBuyVolume));
                        profitPerc = (((avgSellPrice * totSellVolume) + totDivs) / (avgBuyPrice * totBuyVolume)) - 1.0m;
                        profitPercIncValue = (((avgSellPrice * totSellVolume) + totDivs + sharesOwnedVal) / (avgBuyPrice * totBuyVolume)) - 1.0m;
                        dailyProfit = profit / investedDays;
                    }

                    if (createRow)
                    {
                        // Add a row for this corp to the grid.
                        _sections[0].AddRow(_columns.Length, corpID.ToString(), corpName);

                        // Add the data for this row.
                        SetValue("Avg Buy Price", corpID.ToString(), avgBuyPrice, true);
                        SetValue("Avg Sell Price", corpID.ToString(), avgSellPrice, true);
                        SetValue("Units Bought", corpID.ToString(), totBuyVolume, true);
                        SetValue("Units Sold", corpID.ToString(), totSellVolume, true);
                        SetValue("Avg Divs Per Share", corpID.ToString(), avgDivPerShare, true);
                        SetValue("Share Value", corpID.ToString(), shareValue, true);
                        SetValue("Total Dividends", corpID.ToString(), totDivs, true);
                        SetValue("Total Value of Shares", corpID.ToString(), sharesOwnedVal, true);
                        SetValue("Profit", corpID.ToString(), profit, true);
                        SetValue("Profit %", corpID.ToString(), profitPercIncValue, true);
                        SetValue("Daily Profit %", corpID.ToString(), dailyProfit, true);
                    }
                }
                else
                {
                    List<int> accountOwners = new List<int>();
                    EMMADataSet.BankAccountDataTable accounts = BankAccounts.GetBankAccountData(
                        UserAccount.CurrentGroup.ID, 0, corpData.ID);
                    foreach (EMMADataSet.BankAccountRow account in accounts)
                    {
                        corpData.OwnerID = account.OwnerID;
                        corpData.ReloadBankAccountDetails();

                        createRow = BankAccounts.DaysAccountActive(corpData.BankAccountID) > 0 &&
                            BankAccounts.GetTotalTransactionIsk(corpData.BankAccountID,
                                BankTransactionType.Deposit) > 0;
                        if (createRow)
                        {
                            sharesOwnedVal = corpData.AmountInAccount;
                            profit = corpData.TotalInterest;
                            profitPerc = ((BankAccounts.GetTotalTransactionIsk(corpData.BankAccountID,
                                BankTransactionType.Withdrawl) * -1) /
                                BankAccounts.GetTotalTransactionIsk(corpData.BankAccountID,
                                BankTransactionType.Deposit)) - 1.0m;
                            profitPercIncValue = (((sharesOwnedVal +
                                BankAccounts.GetTotalTransactionIsk(corpData.BankAccountID,
                                BankTransactionType.Withdrawl) * -1)) /
                                BankAccounts.GetTotalTransactionIsk(corpData.BankAccountID,
                                BankTransactionType.Deposit)) - 1.0m;
                            dailyProfit = profitPercIncValue / (decimal)BankAccounts.DaysAccountActive(
                                corpData.BankAccountID);

                            // Add a row for this account to the grid.
                            string rowName = corpID.ToString() + corpData.OwnerID.ToString();
                            _sections[0].AddRow(_columns.Length, rowName, corpName + " (" + corpData.Owner + ")");

                            // Add the data for this row.
                            SetValue("Avg Buy Price", rowName, avgBuyPrice, true);
                            SetValue("Avg Sell Price", rowName, avgSellPrice, true);
                            SetValue("Units Bought", rowName, totBuyVolume, true);
                            SetValue("Units Sold", rowName, totSellVolume, true);
                            SetValue("Avg Divs Per Share", rowName, avgDivPerShare, true);
                            SetValue("Share Value", rowName, shareValue, true);
                            SetValue("Total Dividends", rowName, totDivs, true);
                            SetValue("Total Value of Shares", rowName, sharesOwnedVal, true);
                            SetValue("Profit", rowName, profit, true);
                            SetValue("Profit %", rowName, profitPercIncValue, true);
                            SetValue("Daily Profit %", rowName, dailyProfit, true);
                        }
                    }
                }

                UpdateStatus(j, maxProgress, "", "", false);

            }
        }
    }


}
