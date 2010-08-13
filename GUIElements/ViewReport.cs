using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

using EveMarketMonitorApp.Reporting;
using EveMarketMonitorApp.AbstractionClasses;
using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.DatabaseClasses;

namespace EveMarketMonitorApp.GUIElements
{
    partial class ViewReport : Form
    {
        private IReport report;
        private DataTable table;
        private DataView view;
        private DataGridViewCellStyle ALT_ROW_STYLE;
        private DataGridViewCellStyle ROW_HEADER_STYLE;
        private DataGridViewCellStyle COL_HEADER_STYLE;
        private DataGridViewCellStyle DATA_STYLE;
        private DataGridViewCellStyle ISK_STYLE;
        private DataGridViewCellStyle PERCENTAGE_STYLE;
        private DataGridViewCellStyle NUMERIC_STYLE;
        private DataGridViewCellStyle NEGATIVE_DATA_STYLE;
        private DataGridViewCellStyle SECTION_HEADER_STYLE;

        private DataGridViewCellStyle GOOD_DATA_STYLE;
        private DataGridViewCellStyle WARNING_DATA_STYLE;
        private DataGridViewCellStyle DANGER_DATA_STYLE;

        private const int MARGIN_SIZE = 8;
        private IskMultiplier valuesShown;
        private bool allowSort = false;

        private bool controlPressed = false;
        private bool customFormating = true;

        public ViewReport(IReport report, IskMultiplier valuesShown)
        {
            InitializeComponent();

            this.valuesShown = valuesShown;
            this.report = report;
            allowSort = report.GetAllowSort();

            BuildDataTable();

            view = new DataView(table);
            view.RowFilter = "Visible = 'True'";

            // Set a few things to improve performance...
            reportGrid.EnableHeadersVisualStyles = false;
            reportGrid.CellBorderStyle = DataGridViewCellBorderStyle.None;
            reportGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            reportGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            reportGrid.DataSource = view;
            reportGrid.VirtualMode = true;

            // Clear the header text on the row headers column.
            reportGrid.Columns["RowHeaders"].HeaderText = "";
            // Make the row names, visible and expanded columns invisible.
            reportGrid.Columns["RowNames"].Visible = false;
            reportGrid.Columns["Visible"].Visible = false;
            reportGrid.Columns["Expanded"].Visible = false;
            // Set column header text.
            foreach (DataGridViewColumn column in reportGrid.Columns)
            {
                column.HeaderText = table.Columns[column.Name].Caption;
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }
            // Add a column to hold the expand/collapse icons
            DataGridViewImageColumn expandersColumn = new DataGridViewImageColumn();
            expandersColumn.Name = "Expanders";
            expandersColumn.HeaderText = "";
            expandersColumn.DisplayIndex = 1;
            reportGrid.Columns.Add(expandersColumn);

            reportGrid.CellValueNeeded += new DataGridViewCellValueEventHandler(reportGrid_CellValueNeeded);
            reportGrid.CellClick += new DataGridViewCellEventHandler(reportGrid_CellClick);
            reportGrid.KeyDown += new KeyEventHandler(reportGrid_KeyDown);
            reportGrid.KeyUp += new KeyEventHandler(reportGrid_KeyUp);


            // Check if a logo is to be used and load it if needed.
            string logoFile = UserAccount.CurrentGroup.Settings.Rpt_LogoFile;
            if (!logoFile.Equals("") && File.Exists(logoFile))
            {
                imgLogo.Image = Image.FromFile(logoFile);
                int sideLength = Math.Min(180, Math.Max(imgLogo.Image.Width, imgLogo.Image.Height));
                imgLogo.Width = sideLength;
                imgLogo.Height = sideLength;
                imgLogo.Visible = true;
            }
            else
            {
                imgLogo.Visible = false;
            }
        }

        private void ViewReport_Load(object sender, EventArgs e)
        {
            InitStyles();
            RefreshDisplay();
            reportGrid.Sorted += new EventHandler(reportGrid_Sorted);
            reportGrid.CellFormatting+=new DataGridViewCellFormattingEventHandler(reportGrid_CellFormatting);
        }

        void reportGrid_Sorted(object sender, EventArgs e)
        {
            PaintStyles();
            reportGrid.AutoResizeColumns();
            reportGrid.AutoResizeRows();
        }

        /// <summary>
        /// Build the data table used to hold data for the grid.
        /// </summary>
        public void BuildDataTable()
        {
            string[] colHeaders;
            string[] colNames;
            string[] rowHeaders;
            string[] rowNames;

            colHeaders = report.GetColumnHeaders();
            colNames = report.GetColumnNames();
            rowHeaders = report.GetRowHeaders();
            rowNames = report.GetRowNames();
            // We actually have two more columns in the table we use to display the report 
            // than in the report object. The first is used to hold row names and is invisible.
            // The second is used to display the row header text.
            int numColumns = colHeaders.Length + 4;
            int numRows = rowHeaders.Length;

            table = new DataTable();

            // Setup columns.
            DataColumn[] dataColumns = new DataColumn[numColumns];

            dataColumns[0] = new DataColumn();
            dataColumns[0].ColumnName = "RowNames";
            dataColumns[0].Caption = "";
            dataColumns[0].DataType = Type.GetType("System.String");

            dataColumns[1] = new DataColumn();
            dataColumns[1].ColumnName = "Expanded";
            dataColumns[1].Caption = "";
            dataColumns[1].DataType = Type.GetType("System.Boolean");

            dataColumns[2] = new DataColumn();
            dataColumns[2].ColumnName = "Visible";
            dataColumns[2].Caption = "";
            dataColumns[2].DataType = Type.GetType("System.String");

            dataColumns[3] = new DataColumn();
            dataColumns[3].ColumnName = "RowHeaders";
            dataColumns[3].Caption = "";
            dataColumns[3].DataType = Type.GetType("System.String");

            for (int i = 4; i < numColumns; i++)
            {
                dataColumns[i] = new DataColumn();
                dataColumns[i].ColumnName = colNames[i - 4];
                dataColumns[i].Caption = colHeaders[i - 4];
                dataColumns[i].DataType = Type.GetType("System.Decimal");
            }
            table.Columns.AddRange(dataColumns);

            // Fill rows with data and add them to the table.
            for (int row = 0; row < numRows; row++)
            {
                DataRow newRow = table.NewRow();
                bool emptyRow = false;

                for (int col = 0; col < numColumns; col++)
                {
                    string cellValue = "";

                    if (col == 0)
                    {
                        // Get row name
                        cellValue = rowNames[row];
                        newRow[col] = cellValue;
                    }
                    else if (col == 1)
                    {
                        // Set expanded
                        newRow[col] = report.SectionExpanded(rowNames[row]);
                    }
                    else if (col == 2)
                    {
                        // Set visible
                        newRow[col] = report.RowVisible(rowNames[row]).ToString();
                    }
                    else if (col == 3)
                    {
                        // Get row header text
                        if (rowHeaders[row].Trim().Equals(""))
                        {
                            emptyRow = true;
                        }
                        else
                        {
                            int indent = report.RowIndentLevel(rowNames[row]);
                            for (int i = 0; i < indent; i++)
                            {
                                cellValue = cellValue + "   ";
                            }
                        }
                        cellValue = cellValue + rowHeaders[row];
                        newRow[col] = cellValue;
                    }
                    else
                    {
                        // Get data
                        if (!emptyRow)
                        {
                            newRow[col] = report.GetValue(colNames[col - 4], rowNames[row]);
                        }
                    }
                }

                table.Rows.Add(newRow);
            }

        }


        /// <summary>
        /// Set the cell styles for the data grid view.
        /// </summary>
        public void InitStyles()
        {
            // Setup styles based upon user settings
            ROW_HEADER_STYLE = new DataGridViewCellStyle();
            ROW_HEADER_STYLE.Alignment = DataGridViewContentAlignment.MiddleLeft;
            ROW_HEADER_STYLE.Font = UserAccount.CurrentGroup.Settings.Rpt_RowHeaderFont;
            ROW_HEADER_STYLE.ForeColor = UserAccount.CurrentGroup.Settings.Rpt_RowHeaderTextColour;
            ROW_HEADER_STYLE.SelectionForeColor = UserAccount.CurrentGroup.Settings.Rpt_RowHeaderTextColour;
            ROW_HEADER_STYLE.BackColor = UserAccount.CurrentGroup.Settings.Rpt_RowHeaderBackColour;
            ROW_HEADER_STYLE.SelectionBackColor = UserAccount.CurrentGroup.Settings.Rpt_RowHeaderBackColour;

            COL_HEADER_STYLE = new DataGridViewCellStyle();
            COL_HEADER_STYLE.Alignment = DataGridViewContentAlignment.MiddleCenter;
            COL_HEADER_STYLE.Font = UserAccount.CurrentGroup.Settings.Rpt_ColHeaderFont;
            COL_HEADER_STYLE.BackColor = UserAccount.CurrentGroup.Settings.Rpt_ColHeaderBackColour;
            COL_HEADER_STYLE.ForeColor = UserAccount.CurrentGroup.Settings.Rpt_ColHeaderTextColour;
            COL_HEADER_STYLE.SelectionBackColor = UserAccount.CurrentGroup.Settings.Rpt_ColHeaderBackColour;
            COL_HEADER_STYLE.SelectionForeColor = UserAccount.CurrentGroup.Settings.Rpt_ColHeaderTextColour;

            ALT_ROW_STYLE = new DataGridViewCellStyle();
            ALT_ROW_STYLE.BackColor = UserAccount.CurrentGroup.Settings.Rpt_AltRowBackColour;
            ALT_ROW_STYLE.ForeColor = UserAccount.CurrentGroup.Settings.Rpt_AltRowTextColour;
            ALT_ROW_STYLE.SelectionBackColor = UserAccount.CurrentGroup.Settings.Rpt_AltRowBackColour;
            ALT_ROW_STYLE.SelectionForeColor = UserAccount.CurrentGroup.Settings.Rpt_AltRowTextColour;

            DATA_STYLE = new DataGridViewCellStyle();
            DATA_STYLE.Alignment = DataGridViewContentAlignment.MiddleRight;
            DATA_STYLE.Font = UserAccount.CurrentGroup.Settings.Rpt_DataFont;
            Color col = UserAccount.CurrentGroup.Settings.Rpt_DataBackColour;
            Color col2 = UserAccount.CurrentGroup.Settings.Rpt_DataTextColour;
            DATA_STYLE.BackColor = col;
            DATA_STYLE.ForeColor = col2;
            DATA_STYLE.SelectionBackColor = col;
            DATA_STYLE.SelectionForeColor = col2;

            NEGATIVE_DATA_STYLE = new DataGridViewCellStyle();
            NEGATIVE_DATA_STYLE.ForeColor = UserAccount.CurrentGroup.Settings.Rpt_NegDataTextColour;
            NEGATIVE_DATA_STYLE.SelectionForeColor = UserAccount.CurrentGroup.Settings.Rpt_NegDataTextColour;

            GOOD_DATA_STYLE = new DataGridViewCellStyle();
            GOOD_DATA_STYLE.ForeColor = UserAccount.CurrentGroup.Settings.Rpt_GoodDataTextColour;
            GOOD_DATA_STYLE.SelectionForeColor = UserAccount.CurrentGroup.Settings.Rpt_GoodDataTextColour;

            WARNING_DATA_STYLE = new DataGridViewCellStyle();
            WARNING_DATA_STYLE.ForeColor = UserAccount.CurrentGroup.Settings.Rpt_WarningDataTextColour;
            WARNING_DATA_STYLE.SelectionForeColor = UserAccount.CurrentGroup.Settings.Rpt_WarningDataTextColour;

            DANGER_DATA_STYLE = new DataGridViewCellStyle();
            DANGER_DATA_STYLE.ForeColor = UserAccount.CurrentGroup.Settings.Rpt_DangerDataTextColour;
            DANGER_DATA_STYLE.SelectionForeColor = UserAccount.CurrentGroup.Settings.Rpt_DangerDataTextColour;

            ISK_STYLE = new DataGridViewCellStyle();
            string iskFormat = "#,##0.00 ISK;(#,##0.00 ISK);-";
            switch (valuesShown)
            {
                case IskMultiplier.ISK:
                    iskFormat = "#,##0.00 ISK;(#,##0.00 ISK);-";
                    break;
                case IskMultiplier.Thousands:
                    iskFormat = "#,##0,.00K ISK;(#,##0,.00K ISK);-";
                    break;
                case IskMultiplier.Millions:
                    iskFormat = "#,##0,,.00M ISK;(#,##0,,.00M ISK);-";
                    break;
                case IskMultiplier.Billions:
                    iskFormat = "#,##0,,,.00B ISK;(#,##0,,,.00B ISK);-";
                    break;
                default:
                    break;
            }
            ISK_STYLE.Format = iskFormat;

            PERCENTAGE_STYLE = new DataGridViewCellStyle();
            PERCENTAGE_STYLE.Format = "0.00 %;-0.00 %;-";

            NUMERIC_STYLE = new DataGridViewCellStyle();
            NUMERIC_STYLE.Format = "0;-0;-";

            FontStyle SectionHeaderFontStyle =
                (UserAccount.CurrentGroup.Settings.Rpt_SectionHeaderFont.Bold ? FontStyle.Bold :
                FontStyle.Regular) |
                (UserAccount.CurrentGroup.Settings.Rpt_SectionHeaderFont.Underline ? FontStyle.Underline :
                FontStyle.Regular);
            SECTION_HEADER_STYLE = new DataGridViewCellStyle();
            SECTION_HEADER_STYLE.Font = new Font(DATA_STYLE.Font, SectionHeaderFontStyle);

            PaintStyles();
        }

        private void PaintStyles() 
        {
            reportGrid.SuspendLayout();

            // Now apply the styles to the appropriate locations.
            // First set grid level styles...
            reportGrid.DefaultCellStyle = DATA_STYLE;
            reportGrid.RowsDefaultCellStyle = null;
            reportGrid.Columns["RowHeaders"].DefaultCellStyle = ROW_HEADER_STYLE;
            reportGrid.Columns["Expanders"].DefaultCellStyle = ROW_HEADER_STYLE;
            reportGrid.BackgroundColor = DATA_STYLE.BackColor;
            // Also set the actual form background colour to this.
            this.BackColor = DATA_STYLE.BackColor;

            if (UserAccount.CurrentGroup.Settings.Rpt_AltRowInUse)
            {
                reportGrid.AlternatingRowsDefaultCellStyle = ALT_ROW_STYLE;
            }

            // Now set column header styles.
            reportGrid.ColumnHeadersDefaultCellStyle = COL_HEADER_STYLE;


            bool usingRowFormat = report.GetUseRowFormatting();

            if (!usingRowFormat)
            {
                for (int i = 5; i < reportGrid.Columns.Count; i++)
                {
                    // Set formatting rules for the numeric data..
                    ReportDataType dataType = report.ColumnDataType(table.Columns[i - 1].ColumnName);
                    switch (dataType)
                    {
                        case ReportDataType.ISKAmount:
                            reportGrid.Columns[i].DefaultCellStyle.ApplyStyle(ISK_STYLE);
                            break;
                        case ReportDataType.Percentage:
                            reportGrid.Columns[i].DefaultCellStyle.ApplyStyle(PERCENTAGE_STYLE);
                            break;
                        case ReportDataType.Number:
                            reportGrid.Columns[i].DefaultCellStyle.ApplyStyle(NUMERIC_STYLE);
                            break;
                        default:
                            break;
                    }
                }
            }

            for (int i = 0; i < reportGrid.Rows.Count; i++)
            {
                if (usingRowFormat)
                {
                    // Set formatting rules for the numeric data..
                    ReportDataType dataType = report.RowDataType(reportGrid["RowNames", i].Value.ToString());
                    switch (dataType)
                    {
                        case ReportDataType.ISKAmount:
                            reportGrid.Rows[i].DefaultCellStyle.ApplyStyle(ISK_STYLE);
                            break;
                        case ReportDataType.Percentage:
                            reportGrid.Rows[i].DefaultCellStyle.ApplyStyle(PERCENTAGE_STYLE);
                            break;
                        case ReportDataType.Number:
                            reportGrid.Rows[i].DefaultCellStyle.ApplyStyle(NUMERIC_STYLE);
                            break;
                        default:
                            break;
                    }
                }

                if (reportGrid["RowNames", i].Value.ToString().StartsWith("SECTIONROW"))
                {
                    reportGrid.Rows[i].DefaultCellStyle.ApplyStyle(SECTION_HEADER_STYLE);
                }
            }

            reportGrid.ResumeLayout();

            // Do a quick resize of rows and columns
            reportGrid.AutoResizeColumns();
            reportGrid.AutoResizeRows();
        }


        private void reportGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 5)
            {
                // Finally go through and set cell specific styles.
                if (e.Value.ToString().StartsWith("(") ||
                    e.Value.ToString().StartsWith("-"))
                {
                    // Set Negative value style
                    e.CellStyle.ApplyStyle(NEGATIVE_DATA_STYLE);
                }

                if (customFormating)
                {
                    // Report specific formatting...
                    if (report.GetName().Equals("Item Report"))
                    {
                        if (reportGrid.Columns[e.ColumnIndex].Name.Equals("Average Buy Price"))
                        {
                            if (!reportGrid["RowNames", e.RowIndex].Value.ToString().StartsWith("SECTIONROW"))
                            {
                                try
                                {
                                    decimal oldBuyCost = (decimal)reportGrid["Cost Of Units Sold", e.RowIndex].Value;
                                    decimal buyCost = (decimal)e.Value;
                                    decimal sellCost = (decimal)reportGrid["Average Sell Price", e.RowIndex].Value;
                                    if (buyCost != 0 && buyCost < (oldBuyCost * 0.95m))
                                    {
                                        e.CellStyle.ApplyStyle(GOOD_DATA_STYLE);
                                    }
                                    if (oldBuyCost != 0 && buyCost > (oldBuyCost * 1.05m))
                                    {
                                        e.CellStyle.ApplyStyle(WARNING_DATA_STYLE);
                                    }
                                    if (sellCost != 0 && buyCost > sellCost)
                                    {
                                        e.CellStyle.ApplyStyle(DANGER_DATA_STYLE);
                                    }
                                }
                                catch (ArgumentException)
                                {
                                    // This occurs if the columns we're trying to get data from have been 
                                    // disabled by the user. Just turn custom formatting off so it does
                                    // not keep happening.
                                    customFormating = false;
                                }
                            }
                        }
                    }
                    if (report.GetName().Equals("Assets Report"))
                    {
                        if (reportGrid.Columns[e.ColumnIndex].Name.Equals("Reprocess Value"))
                        {
                            if (!reportGrid["RowNames", e.RowIndex].Value.ToString().StartsWith("SECTIONROW"))
                            {
                                try
                                {
                                    decimal sellVal = (decimal)reportGrid["Total Est. Value", e.RowIndex].Value;
                                    decimal reproVal = (decimal)e.Value;
                                    if (reproVal > sellVal && sellVal != 0)
                                    {
                                        e.CellStyle.ApplyStyle(GOOD_DATA_STYLE);
                                    }
                                }
                                catch (ArgumentException)
                                {
                                    // This occurs if the column we're trying to get data from have been 
                                    // disabled by the user. Just turn custom formatting off so it does
                                    // not keep happening.
                                    customFormating = false;
                                }
                            }
                        }
                    }
                }
            }
        }


        private void RefreshDisplay()
        {
            // Set title label's colour and font
            Font titleFont = UserAccount.CurrentGroup.Settings.Rpt_TitleFont;
            Color titleBackCol = UserAccount.CurrentGroup.Settings.Rpt_TitleBackColour;
            Color titleTextCol = UserAccount.CurrentGroup.Settings.Rpt_TitleTextColour;
            lblTitle1.Font = titleFont;
            lblTitle1.ForeColor = titleTextCol;
            lblTitle1.BackColor = titleBackCol;
            lblTitle2.Font = titleFont;
            lblTitle2.ForeColor = titleTextCol;
            lblTitle2.BackColor = titleBackCol;
            lblTitle3.Font = new Font(titleFont.FontFamily, titleFont.SizeInPoints - 4, titleFont.Style);
            lblTitle3.ForeColor = titleTextCol;
            lblTitle3.BackColor = titleBackCol;

            // Set title label's text
            lblTitle1.Text = UserAccount.CurrentGroup.Name;
            lblTitle2.Text = report.GetTitle();
            lblTitle3.Text = report.GetSubTitle();

            // Position the title labels vertically
            lblTitle1.Location = new Point(lblTitle1.Location.X, MARGIN_SIZE);
            lblTitle2.Location = new Point(lblTitle2.Location.X, lblTitle1.Height + MARGIN_SIZE * 2);
            lblTitle3.Location = new Point(lblTitle3.Location.X, lblTitle2.Location.Y + lblTitle2.Height + MARGIN_SIZE);

            // Set the location of the report grid based upon the title label and logo positions
            // and sizes.
            int newYLocation = Math.Max(lblTitle3.Location.Y + lblTitle3.Height + MARGIN_SIZE, 
                (imgLogo.Visible ? imgLogo.Height + imgLogo.Location.Y * 2 : 0));
            int newXLocation = MARGIN_SIZE;
            if (imgLogo.Visible && reportGrid.Columns[0].Width < imgLogo.Width + MARGIN_SIZE * 2) 
            {
                newXLocation = imgLogo.Width + MARGIN_SIZE * 2 - reportGrid.Columns[0].Width ;
            }
            reportGrid.Location = new Point(newXLocation, newYLocation);

            // Set the width and height of the report grid. Cannot be greater than screen size.
            int newWidth = 20;
            int newHeight = reportGrid.ColumnHeadersHeight + 20;
            foreach (DataGridViewColumn column in reportGrid.Columns)
            {
                if (column.Visible)
                {
                    newWidth += column.Width;
                }
            }
            foreach (DataGridViewRow row in reportGrid.Rows)
            {
                if (row.Visible)
                {
                    newHeight += row.Height;
                }
            }
            reportGrid.Height = Math.Min(newHeight, this.MdiParent.ClientSize.Height - reportGrid.Location.Y - 160);
            reportGrid.Width = Math.Min(newWidth, this.MdiParent.ClientSize.Width - reportGrid.Location.X - 100);

            ReposTitle();

            reportGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            // Finally set the size of the form based upon the size and location of the report grid.
            int windowBorder = this.Width - this.ClientSize.Width;
            this.Size = new Size(
                reportGrid.Location.X + reportGrid.Width + Math.Max(MARGIN_SIZE + windowBorder, reportGrid.Location.X + windowBorder),
                reportGrid.Location.Y + reportGrid.Height + MARGIN_SIZE + 42);
            reportGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            this.CenterToScreen();
        }

        private void printReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap reportImage = new Bitmap(this.Bounds.Width, this.Bounds.Height);
            this.DrawToBitmap(reportImage, this.Bounds);
            reportImage.Save("C:/tst.bmp");
        }

        private void ViewReport_Resize(object sender, EventArgs e)
        {
            ReposTitle();
        }

        private void ReposTitle()
        {
            // Position the title labels horizontally based upon the report grid size and location.
            lblTitle1.Location = new Point(reportGrid.Width / 2 - lblTitle1.Width / 2 + reportGrid.Location.X, lblTitle1.Location.Y);
            lblTitle2.Location = new Point(reportGrid.Width / 2 - lblTitle2.Width / 2 + reportGrid.Location.X, lblTitle2.Location.Y);
            lblTitle3.Location = new Point(reportGrid.Width / 2 - lblTitle3.Width / 2 + reportGrid.Location.X, lblTitle3.Location.Y);
        }

        private void reportGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (reportGrid.Columns[e.ColumnIndex].Name.Equals("Expanders"))
            {
                if (e.RowIndex >= 0)
                {
                    int row = e.RowIndex;
                    ToggleExpand(row, controlPressed);
                }
            }
            else if (allowSort)
            {
                DataGridViewColumn oldSorted = reportGrid.SortedColumn;
                DataGridViewColumn newSorted = reportGrid.Columns[e.ColumnIndex];
                ListSortDirection newDirection = ListSortDirection.Ascending;

                if (oldSorted != null && oldSorted.Equals(newSorted))
                {
                    SortOrder oldDirection = reportGrid.SortOrder;
                    newDirection = oldDirection == SortOrder.Descending ?
                        ListSortDirection.Ascending : ListSortDirection.Descending;
                }

                reportGrid.Sort(newSorted, newDirection);
            }
        }

        private void ToggleExpand(int row, bool subsections)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                string rowName = reportGrid["RowNames", row].Value.ToString();
                bool collapse = (bool)reportGrid["Expanded", row].Value;
                if (collapse)
                {
                    report.CollapseSection(rowName, subsections);
                }
                else
                {
                    report.ExpandSection(rowName, subsections);
                }
                UpdateRows();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        void reportGrid_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (reportGrid.Columns[e.ColumnIndex].Name.Equals("Expanders"))
                {
                    string row = reportGrid["RowNames", e.RowIndex].Value.ToString();
                    if (row.StartsWith("SECTIONROW"))
                    {
                        bool expanded = (bool)reportGrid["Expanded", e.RowIndex].Value;

                        if (expanded == true)
                        {
                            e.Value = gridIcons.Images["collapse.gif"];
                        }
                        else
                        {
                            e.Value = gridIcons.Images["expand.gif"];
                        }
                    }
                    else
                    {
                        e.Value = gridIcons.Images["blank.gif"];
                    }
                }
            }
        }

        void reportGrid_KeyUp(object sender, KeyEventArgs e)
        {
            controlPressed = false;
        }

        void reportGrid_KeyDown(object sender, KeyEventArgs e)
        {
            controlPressed = e.Control;
        }

        private void UpdateRows()
        {
            reportGrid.SuspendLayout();

            int nameColumnPos = table.Columns.IndexOf("RowNames");
            int visibleColumnPos = table.Columns.IndexOf("Visible");
            int expandedColumnPos = table.Columns.IndexOf("Expanded");
            for (int i = 0; i < table.Rows.Count; i++)
            {
                string rowName = table.Rows[i].ItemArray.GetValue(nameColumnPos).ToString();

                object[] tmp = table.Rows[i].ItemArray;
                tmp[visibleColumnPos] = report.RowVisible(rowName).ToString();
                tmp[expandedColumnPos] = report.SectionExpanded(rowName);
                table.Rows[i].ItemArray = tmp;
            }

            reportGrid.ResumeLayout();
            PaintStyles();
        }


        private void exportToCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CSVExport.Export(table, report.GetTitle());   
        }




       
    }
}