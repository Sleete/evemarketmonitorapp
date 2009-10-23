using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.Reporting
{
    /// <summary>
    /// The income statement is built up buy going through the journal and transaction data in the EMMA database.
    /// All entries are grouped by type and the total isk value for each type is added as an item on the statement.
    /// </summary>
    class IncomeStatement : ReportBase
    {
        List<int> excRefTypeIDs;

        public IncomeStatement()
        {
            _name = "Income Statement";
            _title = "Income Statement";
            _subtitle = "";

            _expectedParams = new string[5];
            _expectedParams[0] = "ColumnPeriod";
            _expectedParams[1] = "StartDate";
            _expectedParams[2] = "TotalColumns";
            _expectedParams[3] = "FinanceAccessParams";
            _expectedParams[4] = "ExcRefTypes";
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
                    if (_expectedParams[i].Equals("ExcRefTypes")) excRefTypeIDs = (List<int>)paramValue;
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
            _sections.Add(new ReportSection(_columns.Length, "Revenue", "Revenue", this));
            _sections.Add(new ReportSection(_columns.Length, "Expenses", "Expenses", this));
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
            List<short> refTypeIDs = JournalRefTypes.GetRefTypeIDs();

            int maxProgress = _sections.Count * (_columns.Length * refTypeIDs.Count);
            UpdateStatus(0, maxProgress, "", "Getting Report Data...", false);

            for (int sectionNo = 0; sectionNo < _sections.Count; sectionNo++)
            {
                ReportSection section = _sections[sectionNo];

                for (int i = 0; i < _columns.Length; i++)
                {
                    // Cycle through all journal reference IDs
                    foreach (short refTypeID in refTypeIDs)
                    {
                        if (refTypeID != 0 && !excRefTypeIDs.Contains((int)refTypeID))
                        {
                            // Get the total value from the amount column in the journal table for all 
                            // entries that match the current reference ID and are between this column's 
                            // start and end dates.
                            decimal amount = Journal.GetTotAmtByType(refTypeID, 
                                _columns[i].StartDate, _columns[i].EndDate, 
                                section.Name.Equals("Revenue") ? EntryType.Revenue : EntryType.Expense, 
                                _financeAccessParams);

                            // Add the data to the grid.
                            if (amount != 0)
                            {
                                string rowName = section.Name + " " + JournalRefTypes.GetReferenceDesc(refTypeID);
                                if (GetRowByName(rowName) == null)
                                {
                                    // If there is not already a row with this name then add it.
                                    section.AddRow(_columns.Length, rowName,
                                        JournalRefTypes.GetReferenceDesc(refTypeID).Trim());
                                }

                                // Add the data for this row & column.
                                amount = section.Name.Equals("Revenue") ? amount : amount * -1;
                                SetValue(_columns[i].Name, rowName, amount);
                                UpdateStatus(refTypeIDs.IndexOf(refTypeID) + i * refTypeIDs.Count +
                                    sectionNo * (_columns.Length * refTypeIDs.Count),
                                    maxProgress, "", "", false);
                            }
                        }
                    }
                }
                // Add a blank row at the bottom of this section...
                if (GetRowByName("Blank" + sectionNo) == null && sectionNo < _sections.Count - 1)
                {
                    section.AddRow(_columns.Length, "Blank" + sectionNo, "");
                }

            }
        }

        
    }

}
