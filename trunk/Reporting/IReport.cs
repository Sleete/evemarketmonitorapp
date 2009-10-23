using System;
using System.Collections.Generic;
using System.Text;

using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.DatabaseClasses;

namespace EveMarketMonitorApp.Reporting
{
    /// <summary>
    /// The IReport interface defines the methods that all reports must implement to be used
    /// by the ViewReport form.
    /// If you are creating a report then inherit from ReportBase which implements some basic functionality
    /// </summary>
    public interface IReport : IProvideStatus
    {
        string[] GetColumnHeaders();
        string[] GetRowHeaders();

        string[] GetColumnNames();
        string[] GetRowNames();


        //decimal GetValue(int column, int row);
        decimal GetValue(string columnName, string rowName);
        void SetValue(string columnName, string rowName, decimal value);
        ReportDataType ColumnDataType(string columnName);
        ReportDataType RowDataType(string rowName);
        int RowIndentLevel(string rowName);

        bool RowVisible(string rowName);
        bool SectionExpanded(string rowName);
        void ExpandSection(string rowName, bool subsections);
        void CollapseSection(string rowName, bool subsections);

        string GetName();
        string GetTitle();
        string GetSubTitle();
        bool GetAllowSort();
        bool GetUseRowFormatting();

        void CreateReport(Dictionary<string, object> parameters);
    }

    /// <summary>
    /// The Report Base implements some basic functionality that will be common to the majority of reports.
    /// Columns will be time periods, if you want different columns then override the InitColunms method
    /// with your own code. 
    /// 
    /// All reports kick off with the CreateReport method in ReportBase. This then calls 
    /// InitColumns(paramters), InitSections() and FillReport() to contruct the report and
    /// fill in the data.
    /// 
    /// FillReport and InitSections must be overridden, InitColumns can be overridden as required.
    /// See IncomeStatement for an example of a report that does not override InitColumns (well
    /// actually it does but it still calls the base method...)
    /// See ItemReport for an example of completly overriding All methods. 
    /// </summary>
    public abstract class ReportBase : IReport
    {
        protected ReportColumn[] _columns;
        //protected ReportSection[] sections;
        protected ReportSections _sections;
        protected string[] _expectedParams = {"ColumnPeriod", "StartDate", "TotalColumns", "Wallets"};

        protected DateTime _startDate;
        protected DateTime _endDate;
        protected int _periodYears;
        protected int _periodMonths;
        protected int _periodDays;
        protected List<FinanceAccessParams> _financeAccessParams;
        protected List<AssetAccessParams> _assetAccessParams;
        protected string _name;
        protected string _title;
        protected string _subtitle;
        protected bool _allowSort = false;
        protected bool _useRowFormatting = false;

        public event StatusChangeHandler StatusChange;

        /// <summary>
        /// The overriding method must fill the sections and rows with the required data.
        /// </summary>
        public abstract void FillReport();

        protected abstract void InitSections();



        /// <summary>
        /// Get the header text for the columns in this report.
        /// </summary>
        /// <returns></returns>
        public string[] GetColumnHeaders()
        {
            string[] retVal = new string[_columns.Length];

            for (int i = 0; i < _columns.Length; i++)
            {
                retVal[i] = _columns[i].Text;
            }
            return retVal;
        }

        /// <summary>
        /// Get the header text for the rows in this report.
        /// </summary>
        /// <returns></returns>
        public string[] GetRowHeaders()
        {
            List<string> rowHeaders = new List<string>();
            foreach (ReportSection section in _sections)
            {
                rowHeaders.AddRange(section.GetRowHeaders());
            }
            return rowHeaders.ToArray();
        }

        /// <summary>
        /// Get the names of the columns in this report.
        /// </summary>
        /// <returns></returns>
        public string[] GetColumnNames()
        {
            string[] retVal = new string[_columns.Length];

            for (int i = 0; i < _columns.Length; i++)
            {
                retVal[i] = _columns[i].Name;
            }
            return retVal;
        }

        /// <summary>
        /// Get the names of the rows in this report.
        /// </summary>
        /// <returns></returns>
        public string[] GetRowNames()
        {
            List<string> rowNames = new List<string>();
            foreach (ReportSection section in _sections)
            {
                rowNames.AddRange(section.GetRowNames());
            }
            return rowNames.ToArray();
        }
        public string[] GetRowNames(string sectionName)
        {
            List<string> rowNames = new List<string>();
            rowNames = _sections.GetSection(sectionName).GetRowNames();
            return rowNames.ToArray();
        }

        public ReportDataType ColumnDataType(string columnName)
        {
            return _columns[GetColumnByName(columnName)].DataType;
        }
        public ReportDataType ColumnDataType(int colNo)
        {
            return _columns[colNo].DataType;
        }

        public SectionRowBehavior ColumnSectionRowBehavior(string columnName)
        {
            return _columns[GetColumnByName(columnName)].SectionRowBehavior;
        }
        public SectionRowBehavior ColumnSectionRowBehavior(int colNo)
        {
            return _columns[colNo].SectionRowBehavior;
        }

        public ReportDataType RowDataType(string rowName)
        {
            return GetRowByName(rowName).DataType;
        }

        public int RowIndentLevel(string rowName)
        {
            return GetRowByName(rowName).NestingLevel;
        }

        public bool RowVisible(string rowName)
        {
            return GetRowByName(rowName).Visible;
        }

        public bool SectionExpanded(string rowName)
        {
            bool retVal = true;
            ReportSection section = _sections.GetSectionByRowName(rowName);
            if (section != null)
            {
                retVal = section.Expanded;
            }
            return retVal;
        }

        public void ExpandSection(string rowName, bool subsections)
        {
            _sections.GetSectionByRowName(rowName).Expand(subsections, 0);
        }
        public void CollapseSection(string rowName, bool subsections)
        {
            _sections.GetSectionByRowName(rowName).Collapse(subsections, 0);
        }

        /// <summary>
        /// Initialise column array, set names and header text, etc.
        /// </summary>
        /// <param name="parameters"></param>
        public virtual void InitColumns(Dictionary<string, object> parameters)
        {
            ReportPeriod period = ReportPeriod.Year;
            DateTime startDate = DateTime.UtcNow;
            int totColumns = 1;
            bool paramsOk = true;

            // Extract parameters...
            try
            {
                for (int i = 0; i < _expectedParams.Length; i++)
                {
                    object paramValue = null;
                    paramsOk = paramsOk && parameters.TryGetValue(_expectedParams[i], out paramValue);
                    if (_expectedParams[i].Equals("ColumnPeriod")) period = (ReportPeriod)paramValue;
                    if (_expectedParams[i].Equals("StartDate")) startDate = (DateTime)paramValue;
                    if (_expectedParams[i].Equals("TotalColumns")) totColumns = (int)paramValue;
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

            // Start building the columns.
            try
            {
                this._startDate = startDate;
                startDate = startDate.AddHours(23 - startDate.Hour);
                startDate = startDate.AddMinutes(59 - startDate.Minute);
                startDate = startDate.AddSeconds(59 - startDate.Second);
                startDate = startDate.AddMilliseconds(-startDate.Millisecond);

                Period = period;
                if (period == ReportPeriod.AllTime) totColumns = 1;

                _columns = new ReportColumn[totColumns];

                UpdateStatus(0, totColumns, "", "Building Report Columns...", false);

                for (int i = 0; i < totColumns; i++)
                {
                    int years, months, days;
                    DateTime columnStartDate;
                    DateTime columnEndDate;

                    // Set the start and end dates for the current column
                    years = _periodYears * i;
                    months = _periodMonths * i;
                    days = _periodDays * i;
                    columnEndDate = startDate.AddYears(-years);
                    columnEndDate = columnEndDate.AddMonths(-months);
                    columnEndDate = columnEndDate.AddDays(-days);
                    years = _periodYears * (i + 1);
                    months = _periodMonths * (i + 1);
                    days = _periodDays * (i + 1);
                    columnStartDate = startDate.AddYears(-years);
                    columnStartDate = columnStartDate.AddMonths(-months);
                    columnStartDate = columnStartDate.AddDays(-days);

                    _columns[i] = new ReportColumn("Column " + i, "");
                    _columns[i].StartDate = columnStartDate;
                    _columns[i].EndDate = columnEndDate;

                    switch (Period)
                    {
                        case ReportPeriod.Year:
                            _columns[i].Text = string.Format("{0:dd} {0:MMM} {0:yy} - {1:dd} {1:MMM} {1:yy}",
                                _columns[i].StartDate, _columns[i].EndDate);
                            break;
                        case ReportPeriod.Quarter:
                            _columns[i].Text = string.Format("{0:dd} {0:MMM} {0:yy} - {1:dd} {1:MMM} {1:yy}",
                                _columns[i].StartDate, _columns[i].EndDate);
                            break;
                        case ReportPeriod.Month:
                            _columns[i].Text = string.Format("{0:dd} {0:MMM} {0:yy} - {1:dd} {1:MMM} {1:yy}",
                                _columns[i].StartDate, _columns[i].EndDate);
                            break;
                        case ReportPeriod.Week:
                            _columns[i].Text = "Week Beginning " + string.Format("{0:ddd} {0:dd} {0:MMM}", _columns[i].EndDate);
                            break;
                        case ReportPeriod.Day:
                            _columns[i].Text = string.Format("{0:ddd} {0:dd} {0:MMM}", _columns[i].EndDate);
                            break;
                        case ReportPeriod.AllTime:
                            _columns[i].Text = "All Time";
                            break;
                        default:
                            _columns[i].Text = "";
                            break;
                    }

                    UpdateStatus(i + 1, totColumns, "", "", false);
                }

                _endDate = _columns[totColumns - 1].EndDate;
            }
            catch (Exception ex)
            {
                throw new EMMAReportingException(ExceptionSeverity.Error, "Problem creating columns: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Create the report with the specified parameters and fill with data.
        /// </summary>
        /// <param name="paramters"></param>
        /// <exception cref="EMMAReportingException"></exception>
        public void CreateReport(Dictionary<string, object> paramters)
        {
            try
            {
                UpdateStatus(0, 10, "Building Report", "", false);

                InitColumns(paramters);
                InitSections();
                FillReport();

                UpdateStatus(10, 10, "Report Complete", "", true);
            }
            catch (Exception ex)
            {
                UpdateStatus(-1, 0, "Problem creating report", ex.Message, true);
                EMMAException emmaEx = ex as EMMAException;
                if (emmaEx == null)
                {
                    new EMMAReportingException(ExceptionSeverity.Error, "Problem creating report.", ex);
                }
            }
        }

        public string GetName()
        {
            return _name == null ? "" : _name;
        }

        /// <summary>
        /// Get the title of the report
        /// </summary>
        public string GetTitle()
        {
            return _title;
        }

        public string GetSubTitle()
        {
            return _subtitle;
        }

        public bool GetAllowSort()
        {
            return _allowSort;
        }

        public bool GetUseRowFormatting()
        {
            return _useRowFormatting;
        }

        /// <summary>
        /// Add the specified row to the specified section.
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="newRow"></param>
        /*protected void AddRow(string sectionName, string rowName, string rowText)
        {
            sections.GetSection(sectionName).AddRow(columns.Length, rowName, rowText);
        }*/

       /* /// <summary>
        /// Add the specified row to the specified section.
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="newRow"></param>
        protected void AddRow(ReportSection section, string rowName, string rowText, ReportDataType dataType)
        {
            section.AddRow(_columns.Length, rowName, rowText, dataType);
        }

        /// <summary>
        /// Add the specified row to the specified section.
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="newRow"></param>
        protected void AddRow(ReportSection section, string rowName, string rowText)
        {
            AddRow(section, rowName, rowText, ReportDataType.Default);
        }*/


        /// <summary>
        /// Get the amount in the specified row and column.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="rowName"></param>
        /// <param name="value"></param>
        public decimal GetValue(string columnName, string rowName)
        {
            int colPos = -1;
            ReportRow row = null;
            colPos = GetColumnByName(columnName);
            row = GetRowByName(rowName);

            if (row != null && colPos >= 0)
            {
                return row.GetData(colPos);
            }
            else
            {
                throw new EMMAReportingException(ExceptionSeverity.Error, "Cannot find specified row and column in" +
                    " report.\r\nRow: " + rowName + "\r\nColumn: " + columnName);
            }
        }

        /// <summary>
        /// Set the amount in the specified row and column to the specified value.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="rowName"></param>
        /// <param name="value"></param>
        public void SetValue(string columnName, string rowName, decimal value)
        {
            SetValue(columnName, rowName, value, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="rowName"></param>
        /// <param name="value"></param>
        /// <param name="ignoreIfMissing"></param>
        public void SetValue(string columnName, string rowName, decimal value, bool ignoreIfMissing)
        {
            int colPos = -1;
            ReportRow row = null;
            colPos = GetColumnByName(columnName);
            row = GetRowByName(rowName);

            if (row != null && colPos >= 0)
            {
                row.SetData(colPos, value);
            }
            else
            {
                if (!ignoreIfMissing)
                {
                    throw new EMMAReportingException(ExceptionSeverity.Error, "Cannot find specified row and column in" +
                        " report.\r\nRow: " + rowName + "\r\nColumn: " + columnName);
                }
            }
        }

        /// <summary>
        /// Get the specified row
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected ReportRow GetRowByName(string name)
        {
            ReportRow retVal = _sections.GetRow(name.Trim());
            return retVal;
        }

        /// <summary>
        /// Get the index of the specified column
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected int GetColumnByName(string name)
        {
            int retVal = -1;

            for(int i = 0; i < _columns.Length ; i++) 
            {
                if (_columns[i].Name != null && _columns[i].Name.Equals(name))
                {
                    retVal = i;
                }
            }

            return retVal;
        }

        protected void UpdateStatus(int currentProgress, int maxProgress, string section, string sectionStatus, bool done)
        {
            if (StatusChange != null)
            {
                StatusChange(this, new StatusChangeArgs(currentProgress, maxProgress, section, sectionStatus, done));
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

        /// <summary>
        /// Get/Set the period of this report
        /// </summary>
        protected ReportPeriod Period
        {
            get
            {
                ReportPeriod retVal = ReportPeriod.AllTime;
                if (_periodYears == 1000)
                {
                    retVal = ReportPeriod.AllTime;
                }
                if (_periodYears == 1)
                {
                    retVal = ReportPeriod.Year;
                }
                else if (_periodMonths == 4)
                {
                    retVal = ReportPeriod.Quarter;
                }
                else if (_periodMonths == 1)
                {
                    retVal = ReportPeriod.Month;
                }
                else if (_periodDays == 7)
                {
                    retVal = ReportPeriod.Week;
                }
                else if (_periodDays == 1)
                {
                    retVal = ReportPeriod.Day;
                }

                return retVal;
            }
            set
            {
                _periodYears = 0;
                _periodMonths = 0;
                _periodDays = 0;
                switch (value)
                {
                    case ReportPeriod.AllTime:
                        _periodYears = 1000;
                        break;
                    case ReportPeriod.Year:
                        _periodYears = 1;
                        break;
                    case ReportPeriod.Quarter:
                        _periodMonths = 4;
                        break;
                    case ReportPeriod.Month:
                        _periodMonths = 1;
                        break;
                    case ReportPeriod.Week:
                        _periodDays = 7;
                        break;
                    case ReportPeriod.Day:
                        _periodDays = 1;
                        break;
                    default:
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Holds data about a column in a report
    /// </summary>
    public class ReportColumn
    {
        private string _name;
        private string _text;
        private DateTime _startDate;
        private DateTime _endDate;
        private ReportDataType _dataType;
        private SectionRowBehavior _sectionRowBehaviour;

        public ReportColumn(string name, string text)
        {
            this._name = name;
            this._text = text;
            this._dataType = ReportDataType.ISKAmount;
            this._sectionRowBehaviour = SectionRowBehavior.Sum;
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public DateTime StartDate
        {
            get { return _startDate; }
            set { _startDate = value; }
        }

        public DateTime EndDate
        {
            get { return _endDate; }
            set { _endDate = value; }
        }

        public ReportDataType DataType
        {
            get { return _dataType; }
            set { _dataType = value; }
        }

        public SectionRowBehavior SectionRowBehavior
        {
            get { return _sectionRowBehaviour; }
            set { _sectionRowBehaviour = value; }
        }
    }

    /// <summary>
    /// Holds data about a row in a report
    /// </summary>
    public class ReportRow
    {
        protected ReportSection _parent;
        protected string _name;
        protected string _text;
        protected decimal[] _data;
        protected ReportDataType _dataType;
        protected int _nestingLevel;
        protected bool _visible;

        public ReportRow(int numColumns, string name, string text)
        {
            _data = new decimal[numColumns];
            this._parent = null;
            this._name = name;
            this._text = text;
            _dataType = ReportDataType.Default;
            _nestingLevel = 0;
            _visible = true;
        }

        public ReportRow(int numColumns, string name, string text, ReportDataType dataType)
        {
            _data = new decimal[numColumns];
            this._parent = null;
            this._name = name;
            this._text = text;
            this._dataType = dataType;
            _nestingLevel = 0;
            _visible = true;
        }

        public void SetData(int column, decimal value)
        {
            _data[column] = value;
        }

        public decimal GetData(int column)
        {
            decimal retVal = 0;
            ReportSection section = this as ReportSection;
            if (section != null)
            {
                // If we're trying to get data for a section rather than a normal row
                // then first build the data values if required.
                section.BuildDataValues();
            }
            if (column >= 0 && column < _data.Length)
            {
                retVal = _data[column];
            }
            return retVal;
        }

        public ReportSection Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public ReportDataType DataType
        {
            get { return _dataType; }
            set { _dataType = value; }
        }

        public int NestingLevel
        {
            get { return _nestingLevel; }
            set { _nestingLevel = value; }
        }

        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }
    }

    /// <summary>
    /// A report section can contain multiple report rows and/or sub-sections.
    /// Note that it also counts as a row in itself.
    /// </summary>
    public class ReportSection : ReportRow
    {
        private List<string> _containedSections;
        private List<string> _containedRows;
        private ReportSections _childSections;
        private List<ReportRow> _rows;
        private ReportBase _report;
        private bool _dataValuesBuilt = false;
        private bool _expanded = true;

        public ReportSection(int numColumns, string name, string text, ReportBase report) 
            : base(numColumns, name, text)
        {    
            _containedSections = new List<string>();
            _containedRows = new List<string>();
            _containedRows.Add("SECTIONROW" + _name);
            _rows = new List<ReportRow>();
            _childSections = new ReportSections();
            _report = report;
        }

        /// <summary>
        /// Build the section's data values based upon the values of the sub-sections 
        /// and rows that it contains
        /// </summary>
        public void BuildDataValues()
        {
            if (!_dataValuesBuilt)
            {
                decimal total = 0;
                decimal count = 0;

                for (int i = 0; i < _data.Length; i++)
                {
                    total = 0;
                    count = 0;

                    if (_report.ColumnSectionRowBehavior(i) != SectionRowBehavior.Blank)
                    {
                        foreach (ReportSection section in _childSections)
                        {
                            total += section.GetData("SECTIONROW" + section.Name, i);
                            count++;
                        }
                        foreach (ReportRow row in _rows)
                        {
                            total += row.GetData(i);
                            count++;
                        }

                        decimal value = 0;
                        switch (_report.ColumnSectionRowBehavior(i))
                        {
                            case SectionRowBehavior.Sum:
                                value = total;
                                break;
                            case SectionRowBehavior.Average:
                                value = (count == 0 ? 0 : total / count);
                                break;
                            case SectionRowBehavior.Blank:
                                break;
                            default:
                                break;
                        }
                        SetData(i, value);
                    }
                    else
                    {
                        _data[i] = 0;
                    }
                }

                _dataValuesBuilt = true;
            }
        }

        /// <summary>
        /// Add the specified section
        /// </summary>
        /// <param name="section"></param>
        public void AddSection(ReportSection section)
        {
            section.NestingLevel = _nestingLevel + 1;
            section.Parent = this;
            _containedSections.AddRange(section.ContainedSections);
            AddToContainedSections(section.Name);
            _childSections.Add(section);
        }

        /// <summary>
        /// Add the specified section name to the list of contained sections and call the same method on 
        /// the parent section.
        /// </summary>
        /// <param name="name"></param>
        private void AddToContainedSections(string name)
        {
            _containedSections.Add(name);
            _containedRows.Add("SECTIONROW" + name);
            if (_parent != null)
            {
                _parent.AddToContainedSections(name);
            }
        }
 
        /// <summary>
        /// Remove this section from the tree.
        /// </summary>
        public void RemoveSection()
        {
            _containedSections.Add(_name);
            if (_parent != null)
            {
                _parent.Remove(this);
            }
        }
        private void Remove(ReportSection section)
        {
            foreach (string sectionName in section.ContainedSections)
            {
                _containedSections.Remove(sectionName);
            }
            foreach (string row in section.ContainedRows)
            {
                _containedRows.Remove(row);
            }

            if (_parent != null)
            {
                _parent.Remove(section);
            }

            if (_childSections.Contains(section))
            {
                _childSections.Remove(section);
            }
        }

        public void Expand(bool subsections, int level)
        {
            // Set to expanded if this is the section that was clicked on
            // and/or we're expanding subsections.
            if (level == 0 || subsections)
            {
                _expanded = true;
            }
            // Make directly contained rows visible if this section is expanded.
            foreach (ReportRow row in _rows)
            {
                row.Visible = _expanded;
            }
            // Make this section visible.
            this.Visible = true;
            // If this section itself is expanded then call the same method on
            // child sections.
            if (_expanded)
            {
                foreach (ReportSection section in _childSections)
                {
                    section.Expand(subsections, level + 1);
                }
            }
        }

        public void Collapse(bool subsections, int level)
        {
            // Set to not expanded if this is the section that was clicked on
            // and/or we're collapsing subsections.
            if (level == 0 || subsections)
            {
                _expanded = false;
            }
            // Make directly contained rows invisible.
            foreach (ReportRow row in _rows)
            {
                row.Visible = false;
            }
            // If this is not the section that was clicked on then make it invisible.
            if (level > 0)
            {
                this.Visible = false;
            }
            // Call the same method on all child sections.
            foreach (ReportSection section in _childSections)
            {
                section.Collapse(subsections, level + 1);
            }
        }

        /// <summary>
        /// Check if this section contains the specified section
        /// </summary>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public bool Contains(string sectionName)
        {
            return _containedSections.Contains(sectionName);
        }

        /// <summary>
        /// Get the header text for the rows in this section.
        /// </summary>
        /// <returns></returns>
        public List<string> GetRowHeaders()
        {
            List<string> retVal = new List<string>();

            retVal.Add(_text);
            foreach (ReportSection section in _childSections)
            {
                retVal.AddRange(section.GetRowHeaders());
            }
            for (int i = 0; i < _rows.Count; i++)
            {
                retVal.Add(_rows[i].Text);
            }

            return retVal;
        }

        /// <summary>
        /// Get the names of the rows in this section.
        /// </summary>
        /// <returns></returns>
        public List<string> GetRowNames()
        {
            List<string> retVal = new List<string>();
            // Want to make sure these are in the right order so can't just use _containedRows
            retVal.Add("SECTIONROW" + _name);
            foreach (ReportSection section in _childSections)
            {
                retVal.AddRange(section.GetRowNames());
            }
            for (int i = 0; i < _rows.Count; i++)
            {
                retVal.Add(_rows[i].Name);
            }

            return retVal;
        }

        /// <summary>
        /// Return the total number of rows this section contains
        /// </summary>
        /// <returns></returns>
        public int NumRows()
        {
            /*int retVal = 0;
            if (_totalRows == 0)
            {
                foreach (ReportSection section in _childSections)
                {
                    retVal += section.NumRows();
                }

                retVal += _rows.Count;
                retVal += 1;
            }
            else
            {
                retVal = _totalRows;
            }*/

            return _containedRows.Count;
        }

        /// <summary>
        /// Add a new row to this section.
        /// </summary>
        /// <param name="numColumns"></param>
        /// <param name="rowName"></param>
        /// <param name="rowText"></param>
        public void AddRow(int numColumns, string rowName, string rowText)
        {
            AddRow(numColumns, rowName, rowText, ReportDataType.Default);
        }
        public void AddRow(int numColumns, string rowName, string rowText, ReportDataType dataType)
        {
            ReportRow newRow = new ReportRow(numColumns, rowName.Trim(), rowText, dataType);
            newRow.NestingLevel = _nestingLevel + 1;
            newRow.Parent = this;
            AddToContainedRows(rowName);
            _rows.Add(newRow);
        }

        /// <summary>
        /// Add the specified row name to '_containedRows' and call recursively for parent sections.
        /// </summary>
        /// <param name="name"></param>
        private void AddToContainedRows(string name)
        {
            _containedRows.Add(name);
            if (_parent != null)
            {
                _parent.AddToContainedRows(name);
            }
        }

        /// <summary>
        /// Check if this section contains the specified row.
        /// (Note this only checks this section's rows, not any sub-sections.)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasRow(string name)
        {
            if (name.Equals("SECTIONROW" + _name))
            {
                return true;
            }
            foreach (ReportRow row in _rows)
            {
                if (row.Name.Equals(name)) return true;
            }
            return false;
        }

        /// <summary>
        /// Set the data value for the specified row and column
        /// </summary>
        /// <param name="rowName"></param>
        /// <param name="column"></param>
        /// <param name="value"></param>
        public void SetData(string rowName, int column, decimal value)
        {
            GetRowByName(rowName).SetData(column, value);
        }

        /// <summary>
        /// Get the data value for the specified row and column
        /// </summary>
        /// <param name="rowName"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public decimal GetData(string rowName, int column)
        {
            return GetRowByName(rowName).GetData(column);
        }

        /// <summary>
        /// Get the specified row object
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ReportRow GetRowByName(string name)
        {
            ReportRow retVal = null;

            if (name.Equals("SECTIONROW" + _name))
            {
                retVal = this;
            } 
            else 
            {
                foreach (ReportRow row in _rows)
                {
                    if (row.Name != null && row.Name.Equals(name))
                    {
                        retVal = row;
                    }
                }
            }

            return retVal;
        }

        public List<string> ContainedSections
        {
            get { return _containedSections; }
        }

        public List<string> ContainedRows
        {
            get { return _containedRows; }
        }

        public ReportSections ChildSections
        {
            get { return _childSections; }
        }

        public bool Expanded
        {
            get { return _expanded; }
            set { _expanded = value; }
        }
    }

    /// <summary>
    /// A report section type definition
    /// </summary>
/*    public class ReportSectionType
    {
        private string _name;
        private string _displayName;
        private int _nestingLevel;
        private ReportSectionTypes _childSections;

        public ReportSectionType(string typeName, string displayName)
        {
            this._name = typeName;
            this._displayName = displayName;
            this._nestingLevel = 0;
            this._childSections = new ReportSectionTypes();
        }

        public void Add(ReportSectionType section)
        {
            section.NestingLevel = _nestingLevel + 1;
            _childSections.Add(section);
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }

        public int NestingLevel
        {
            get { return _nestingLevel; }
            set { _nestingLevel = value; }
        }

        public ReportSectionTypes ChildSections
        {
            get { return _childSections; }
        }
    }
*/

    /// <summary>
    /// A report section collection definition
    /// This contains a collection of ReportSection objects
    /// </summary>
    public class ReportSections : List<ReportSection>
    {
        public ReportSections() : base()
        {
        }

        public ReportSection GetSection(string name)
        {
            ReportSection retVal = null;

            foreach (ReportSection type in this)
            {
                if (type.Name.Equals(name))
                {
                    retVal = type;
                }
                else if (type.ContainedSections.Contains(name))
                {
                    retVal = type.ChildSections.GetSection(name);
                }
            }
            return retVal;
        }

        public ReportSection GetSectionByRowName(string name)
        {
            ReportSection retVal = GetRow(name) as ReportSection;
            return retVal;
        }

        public ReportRow GetRow(string name)
        {
            ReportRow retVal = null;

            foreach (ReportSection type in this)
            {
                if (type.HasRow(name))
                {
                    retVal = type.GetRowByName(name);
                }
                else if (type.ContainedRows.Contains(name))
                {
                    retVal = type.ChildSections.GetRow(name);
                }
            }

            return retVal;
        }

        public bool ContiansSection(string name) 
        {
            bool retVal = false;
            foreach (ReportSection type in this)
            {
                if (type.ContainedSections.Contains(name)) { retVal = true; }
            }
            return retVal;
        }
    }

    /// <summary>
    /// Valid income statement periods
    /// </summary>
    public enum ReportPeriod
    {
        Year,
        Quarter,
        Month,
        Week,
        Day,
        AllTime
    }

    /// <summary>
    /// Types of data that can be stored in reports
    /// </summary>
    public enum ReportDataType
    {
        ISKAmount,
        Percentage,
        Number,
        Default
    }

    /// <summary>
    /// Different ways of displaying data in aggregated section totals rows.
    /// </summary>
    public enum SectionRowBehavior
    {
        Sum,
        Average,
        Blank
    }

}
