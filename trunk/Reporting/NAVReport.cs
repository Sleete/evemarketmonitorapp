using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.AbstractionClasses;

namespace EveMarketMonitorApp.Reporting
{
    /// <summary>
    /// The income statement is built up buy going through the journal and transaction data in the EMMA database.
    /// All entries are grouped by type and the total isk value for each type is added as an item on the statement.
    /// </summary>
    class NAVReport : ReportBase
    {
        private bool _includeInvestments = false;

        public NAVReport()
        {
            _name = "NAV Report";
            _title = "NAV Report";
            _subtitle = "";

            _expectedParams = new string[5];
            _expectedParams[0] = "ColumnPeriod";
            _expectedParams[1] = "StartDate";
            _expectedParams[2] = "TotalColumns";
            _expectedParams[3] = "FinanceAccessParams";
            _expectedParams[4] = "IncludeInvestments";
        }

        /// <summary>
        /// Initialise column array, set names and header text, etc.
        /// </summary>
        /// <param name="parameters"></param>
        public override void InitColumns(Dictionary<string, object> parameters)
        {
            //ReportPeriod period = ReportPeriod.Year;
            //DateTime startDate = DateTime.UtcNow;
            //int totColumns = 1;

            // Extract parameters...
            try
            {
                for (int i = 0; i < _expectedParams.Length; i++)
                {
                    object paramValue = null;
                    paramValue = parameters[_expectedParams[i]];
                    // These parameters will be retrieved by the base method, no need to get them here.
                    //if (expectedParams[i].Equals("ColumnPeriod")) period = (ReportPeriod)paramValue;
                    //if (expectedParams[i].Equals("StartDate")) startDate = (DateTime)paramValue;
                    //if (expectedParams[i].Equals("TotalColumns")) totColumns = (int)paramValue;
                    if (_expectedParams[i].Equals("FinanceAccessParams")) _financeAccessParams =
                        (List<FinanceAccessParams>)paramValue;
                    if (_expectedParams[i].Equals("IncludeInvestments")) _includeInvestments =
                        (bool)paramValue;
                }
            }
            catch (Exception ex)
            {
                string message = "Unable to parse parameters for report '" +
                    _title + "'.\r\nExpected";
                for (int i = 0; i < _expectedParams.Length; i++)
                {
                    message = message + " " + _expectedParams[i] + (i == _expectedParams.Length - 1 ? "." : ",");
                }
                UpdateStatus(0, 0, "Error", message, false);
                throw new EMMAReportingException(ExceptionSeverity.Error, message, ex);
            }

            base.InitColumns(parameters);
        }

        protected override void InitSections()
        {
            _sections = new ReportSections();
            ReportSection root = new ReportSection(_columns.Length, "NAV", "NAV", this);
            _sections.Add(root);
            ReportSection walletSection = new ReportSection(_columns.Length, "W", "Wallet Balance", this);
            ReportSection escrowSection = new ReportSection(_columns.Length, "E", "Cash in Escrow", this);
            ReportSection assetsSection = new ReportSection(_columns.Length, "A", "Assets", this);
            //ReportSection sellOrdersSection = new ReportSection(_columns.Length, "S", "Sell Orders", this);

            root.AddSection(walletSection);
            root.AddSection(escrowSection);
            root.AddSection(assetsSection);
            //root.AddSection(sellOrdersSection);

            foreach (FinanceAccessParams accessParams in _financeAccessParams)
            {
                int ownerID = accessParams.OwnerID;
                bool corp = false;

                APICharacter character = UserAccount.CurrentGroup.GetCharacter(ownerID, ref corp);
                //ReportSection ownerAssets = null;

                if (corp)
                {
                    walletSection.AddSection(new ReportSection(_columns.Length, "W" + ownerID.ToString(),
                        character.CorpName, this));
                    escrowSection.AddSection(new ReportSection(_columns.Length, "E" + ownerID.ToString(),
                        character.CorpName, this));
                    //ownerAssets = new ReportSection(_columns.Length, "A" + ownerID.ToString(),
                    //    character.CorpName, this);
                }
                else
                {
                    //ownerAssets = new ReportSection(_columns.Length, "A" + ownerID.ToString(),
                    //    character.CharName, this);
                }
                //assetsSection.AddSection(ownerAssets);
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
            int progressCounter = 0;
            int maxProgress = 4 * _columns.Length * _financeAccessParams.Count;
            UpdateStatus(0, maxProgress, "", "Getting Report Data...", false);

            Diagnostics.ResetAllTimers();
            Diagnostics.StartTimer("NAVReport");

            foreach (FinanceAccessParams accessParams in _financeAccessParams)
            {
                int ownerID = accessParams.OwnerID;
                Diagnostics.StartTimer("NAVReport." + ownerID.ToString());
                bool corp = false;
                APICharacter character = UserAccount.CurrentGroup.GetCharacter(ownerID, ref corp);
                List<short> walletIDs = new List<short>();

                // sort out what wallet IDs we actually want...
                foreach (short id in accessParams.WalletIDs)
                {
                    if (id != 0 && !walletIDs.Contains(id)) { walletIDs.Add(id); }
                    else
                    {
                        if (corp)
                        {
                            for (short i = 1000; i <= 1006; i++)
                            {
                                if (!walletIDs.Contains(i)) { walletIDs.Add(i); }
                            }
                        }
                        else
                        {
                            if (!walletIDs.Contains(1000)) { walletIDs.Add(1000); }
                        }
                    }
                }

                EMMADataSet.WalletDivisionsDataTable walletNames = character.WalletDivisions;

                Diagnostics.ResetTimer("NAVReport.GetWalletCash");
                foreach (short walletID in walletIDs)
                {
                    string walletName = "";
                    foreach (EMMADataSet.WalletDivisionsRow row in walletNames)
                    {
                        if (row.ID == walletID) { walletName = row.Name; }
                    }

                    /// Create new rows etc as needed.
                    _sections.GetSection((corp ? "W" + ownerID.ToString() : "W")).AddRow(
                        _columns.Length,
                        "W" + ownerID.ToString() + walletID.ToString(),
                        (corp ? walletName : character.CharName));

                    for (int columnNo = 0; columnNo < _columns.Length; columnNo++)
                    {
                        if (walletID == walletIDs[0])
                        {
                            progressCounter++;
                            UpdateStatus(progressCounter, maxProgress, 
                                (corp ? character.CorpName : character.CharName), 
                                "Wallet Data", false);
                        }

                        decimal cashInWallet;

                        Diagnostics.StartTimer("NAVReport.GetWalletCash");
                        cashInWallet = NAVHistory.GetWalletCash(ownerID, walletID,
                            _columns[columnNo].EndDate);
                        Diagnostics.StopTimer("NAVReport.GetWalletCash");
                        SetValue(_columns[columnNo].Name, "W" + ownerID.ToString() + walletID.ToString(), cashInWallet);
                    }
                }

                Diagnostics.ResetTimer("NAVReport.GetEscrowCash");
                foreach (short walletID in walletIDs)
                {
                    string walletName = "";
                    foreach (EMMADataSet.WalletDivisionsRow row in walletNames)
                    {
                        if (row.ID == walletID) { walletName = row.Name; }
                    }

                    _sections.GetSection((corp ? "E" + ownerID.ToString() : "E")).AddRow(
                        _columns.Length,
                        "E" + ownerID.ToString() + walletID.ToString(),
                        (corp ? walletName : character.CharName));

                    for (int columnNo = 0; columnNo < _columns.Length; columnNo++)
                    {
                        if (walletID == walletIDs[0])
                        {
                            progressCounter++;
                            UpdateStatus(progressCounter, maxProgress,
                                (corp ? character.CorpName : character.CharName),
                                "Cash in escrow", false);
                        }

                        decimal cashInEscrow;

                        Diagnostics.StartTimer("NAVReport.GetEscrowCash");
                        if (columnNo == 0)
                        {
                            cashInEscrow = NAVHistory.GetEscrowCash(ownerID, walletID);
                        }
                        else
                        {
                            cashInEscrow = NAVHistory.GetNextEscrowCash(ownerID, walletID, _columns[columnNo].EndDate);
                        }
                        Diagnostics.StopTimer("NAVReport.GetEscrowCash");

                        SetValue(_columns[columnNo].Name, "E" + ownerID.ToString() + walletID.ToString(), cashInEscrow);
                    }
                }

                _sections.GetSection("A").AddRow(
                    _columns.Length,
                    "A" + ownerID.ToString(),
                    (corp ? character.CorpName : character.CharName));


                Diagnostics.ResetTimer("NAVReport.GetAssetsValue");
                for (int columnNo = 0; columnNo < _columns.Length; columnNo++)
                {
                    progressCounter++;
                    UpdateStatus(progressCounter, maxProgress,
                        (corp ? character.CorpName : character.CharName),
                        "Assets value", false);

                    Diagnostics.StartTimer("NAVReport.GetAssetsValue");
                    /// Get Assets value and add to report.
                    decimal assetsValue = NAVHistory.GetAssetsValue(ownerID, _columns[columnNo].EndDate);
                    Diagnostics.StopTimer("NAVReport.GetAssetsValue");
                    SetValue(_columns[columnNo].Name, "A" + ownerID.ToString(), assetsValue);
                }

                //_sections.GetSection("S").AddRow(
                //    _columns.Length,
                //    "S" + ownerID.ToString(),
                //    (corp ? character.CorpName : character.CharName));


                //Diagnostics.ResetTimer("NAVReport.GetSellOrderValue");
                //for (int columnNo = 0; columnNo < _columns.Length; columnNo++)
                //{
                //    progressCounter++;
                //    UpdateStatus(progressCounter, maxProgress,
                //        (corp ? character.CorpName : character.CharName),
                //        "Value of sell orders", false);

                //    /// Get sell orders value and add to report.
                //    Diagnostics.StartTimer("NAVReport.GetSellOrderValue");
                //    decimal sellOrdersValue = NAVHistory.GetSellOrdersValue(ownerID, _columns[columnNo].EndDate);
                //    Diagnostics.StopTimer("NAVReport.GetSellOrderValue");
                //    SetValue(_columns[columnNo].Name, "S" + ownerID.ToString(), sellOrdersValue);
                //}
                Diagnostics.StopTimer("NAVReport." + ownerID.ToString());

                DiagnosticUpdate("", "--------------------------------------------");
                DiagnosticUpdate("", "------------ Timing diagnostics ------------");
                DiagnosticUpdate("", "Total time for " + (corp ? character.CorpName : character.CharName) + ": " +
                    Diagnostics.GetRunningTime("NAVReport." + ownerID.ToString()));
                DiagnosticUpdate("", "\tGet wallet cash " +
                    Diagnostics.GetRunningTime("NAVReport.GetWalletCash"));
                DiagnosticUpdate("", "\tGet escrow cash " +
                    Diagnostics.GetRunningTime("NAVReport.GetEscrowCash"));
                DiagnosticUpdate("", "\tGet assets value " +
                    Diagnostics.GetRunningTime("NAVReport.GetAssetsValue"));
                DiagnosticUpdate("", "\tGet sell order value " +
                    Diagnostics.GetRunningTime("NAVReport.GetSellOrderValue"));

            }

            if (_includeInvestments)
            {
                _sections.GetSection("NAV").AddRow(_columns.Length, "I", "Investments");
                Diagnostics.StartTimer("NAVReport.Investments");

                for (int columnNo = 0; columnNo < _columns.Length; columnNo++)
                {
                    decimal investmentsValue = 0;
                    PublicCorpsList investments = PublicCorps.GetReportGroupInvestments(
                        UserAccount.CurrentGroup.ID, false);
                    foreach (PublicCorp corp in investments)
                    {
                        investmentsValue += corp.CalcSharesOwnedValue(_columns[columnNo].EndDate);
                    }
                    PublicCorpsList banks = PublicCorps.GetReportGroupInvestments(
                        UserAccount.CurrentGroup.ID, true);
                    foreach (PublicCorp corp in banks)
                    {
                        EMMADataSet.BankAccountDataTable accounts = BankAccounts.GetBankAccountData(
                            UserAccount.CurrentGroup.ID, 0, corp.ID);
                        foreach (EMMADataSet.BankAccountRow account in accounts)
                        {
                            corp.OwnerID = account.OwnerID;
                            corp.ReloadBankAccountDetails();

                            BankAccounts.PayOutstandingInterest(corp.BankAccountID);
                            investmentsValue += BankAccounts.GetAccountBalance(corp.BankAccountID,
                                _columns[columnNo].EndDate);
                        }
                    }

                    SetValue(_columns[columnNo].Name, "I", investmentsValue);
                }
                Diagnostics.StopTimer("NAVReport.Investments");
            }

            Diagnostics.StopTimer("NAVReport");

            DiagnosticUpdate("", "--------------------------------------------");
            DiagnosticUpdate("", "------------ Timing diagnostics ------------");
            DiagnosticUpdate("", "Total time building report: " + Diagnostics.GetRunningTime("NAVReport"));
            foreach (FinanceAccessParams accessParams in _financeAccessParams)
            {
                int ownerID = accessParams.OwnerID;
                bool corp = false;
                APICharacter character = UserAccount.CurrentGroup.GetCharacter(ownerID, ref corp);
                DiagnosticUpdate("", "\t" + (corp ? character.CorpName : character.CharName) + ": " +
                    Diagnostics.GetRunningTime("NAVReport." + ownerID.ToString()));
            }
            DiagnosticUpdate("", "\tInvestments: " + Diagnostics.GetRunningTime("NAVReport.Investments"));


        }
        


    }

}
