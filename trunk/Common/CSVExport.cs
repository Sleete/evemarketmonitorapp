using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Windows.Forms;
using System.Threading;

using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.GUIElements;

namespace EveMarketMonitorApp.Common
{
    class CSVExport : IProvideStatus
    {
        public event StatusChangeHandler StatusChange;
        public static string _filename = "";

        public void UpdateStatus(int progress, int maxProgress, string section, string sectionStatus, bool done)
        {
            if (StatusChange != null)
            {
                StatusChange(null, new StatusChangeArgs(progress, maxProgress, section, sectionStatus, done));
            }
        }

        public CSVExport()
        {
        }

        public static void Export(DataTable data, string name)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Export " + name + " data to csv";
            dialog.Filter = "csv files (*.csv)|*.csv";
            dialog.AddExtension = true;
            dialog.DefaultExt = ".csv";
            dialog.OverwritePrompt = true;
            dialog.InitialDirectory = UserAccount.Settings.CSVExportDir;
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                _filename = dialog.FileName;
                CSVExport expObj = new CSVExport();
                Thread t1 = new Thread(new ParameterizedThreadStart(expObj.DoExport));
                t1.TrySetApartmentState(ApartmentState.STA);
                ProgressDialog prgDialog = new ProgressDialog("Building " + name + " CSV file...", expObj);
                t1.Start(data);
                prgDialog.ShowDialog();
            }
        }

        public static void Export(DataGridView data, string name)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Export " + name + " data to csv";
            dialog.Filter = "csv files (*.csv)|*.csv";
            dialog.AddExtension = true;
            dialog.DefaultExt = ".csv";
            dialog.OverwritePrompt = true;
            dialog.InitialDirectory = UserAccount.Settings.CSVExportDir;
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                _filename = dialog.FileName;

                CSVExport expObj = new CSVExport();
                Thread t1 = new Thread(new ParameterizedThreadStart(expObj.DoExport2));
                t1.TrySetApartmentState(ApartmentState.STA);
                ProgressDialog prgDialog = new ProgressDialog("Building " + name + " CSV file...", expObj);
                t1.Start(data);
                prgDialog.ShowDialog();
            }
        }

        private void DoExport2(object datagridview)
        {
            // Just wait to make sure the progress dialog is displayed
            Thread.Sleep(100);
            DataGridView table = datagridview as DataGridView;

            try
            {
                string filename = _filename;
                string directory = filename.Remove(filename.LastIndexOf(Path.DirectorySeparatorChar));
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                UserAccount.Settings.CSVExportDir = directory;
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }

                StringBuilder line = new StringBuilder();
                StreamWriter writer = File.CreateText(filename);
                try
                {
                    List<int> visibleColumns = new List<int>();

                    // Get column names..
                    UpdateStatus(0, table.Rows.Count + 1, "Writing column headers..", "", false);
                    for (int col = 0; col < table.Columns.Count; col++)
                    {
                        DataGridViewColumn column = table.Columns[col];
                        if (column.Visible)
                        {
                            if (line.Length > 0) { line.Append(","); }
                            line.Append(column.HeaderText);
                            visibleColumns.Add(col);
                        }
                    }
                    UpdateStatus(1, table.Rows.Count + 1, "Writing column headers..", "Done", false);
                    // ..then output the data.
                    for (int row = 0; row < table.Rows.Count; row++)
                    {
                        UpdateStatus(row + 1, table.Rows.Count + 1, "Writing data..", "", false);
                        line = new StringBuilder();
                        for (int i = 0; i < visibleColumns.Count; i++)
                        {
                            if (line.Length > 0) { line.Append(","); }
                            line.Append(table[visibleColumns[i], row].Value.ToString());
                        }
                        writer.WriteLine(line.ToString());
                    }
                    UpdateStatus(0, 0, "", "Done", false);
                }
                finally
                {
                    writer.Close();
                }

                UpdateStatus(0, 0, "CSV created successfully", "", true);
            }
            catch (Exception ex)
            {
                // Creating new EMMAexception will cause error to be logged.
                EMMAException emmaex = ex as EMMAException;
                if (emmaex == null)
                {
                    emmaex = new EMMAException(ExceptionSeverity.Error, "Error exporting to CSV", ex);
                }
                UpdateStatus(0, 0, "Error", "Problem exporting to CSV.\r\nCheck " + Globals.AppDataDir + "Logging\\ExceptionLog.txt" +
                    " for details.", true);
            }
        }


        private void DoExport(object datatable)
        {
            // Just wait to make sure the progress dialog is displayed
            Thread.Sleep(100);

            string filename = _filename;
            string directory = filename.Remove(filename.LastIndexOf(Path.DirectorySeparatorChar));
            UserAccount.Settings.CSVExportDir = directory;

            int rowNo = 0;
            
            try
            {
                DataTable data = (DataTable)datatable;

                try
                {
                    TableType tableType = TableType.Generic;

                    if (data.GetType().Equals(Type.GetType(
                        "EveMarketMonitorApp.DatabaseClasses.EMMADataSet+AssetsDataTable")))
                    {
                        tableType = TableType.Assets;
                    }
                    else if (data.GetType().Equals(Type.GetType(
                        "EveMarketMonitorApp.DatabaseClasses.EMMADataSet+JournalDataTable")))
                    {
                        tableType = TableType.Journal;
                    }
                    else if (data.GetType().Equals(Type.GetType(
                        "EveMarketMonitorApp.DatabaseClasses.EMMADataSet+TransactionsDataTable")))
                    {
                        tableType = TableType.Transactions;
                    }

                    FileInfo fInfo = new FileInfo(filename);

                    if (!Directory.Exists(fInfo.DirectoryName))
                    {
                        Directory.CreateDirectory(fInfo.DirectoryName);
                    }

                    FileStream fileStream = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.None);
                    try
                    {
                        StreamWriter writer = new StreamWriter(fileStream);

                        try
                        {
                            // first write column headers
                            StringBuilder rowData = new StringBuilder("");

                            UpdateStatus(0, data.Rows.Count + 1, "Writing column headers..", "", false);
                            for (int colNo = 0; colNo < data.Columns.Count; colNo++)
                            {
                                if (colNo != 0) rowData.Append(",");
                                rowData.Append(GetValue(data, tableType, colNo, 0, ValueType.ColumnName));
                            }
                            writer.WriteLine(rowData.ToString());
                            UpdateStatus(1, data.Rows.Count + 1, "Writing column headers..", "Done", false);

                            // Now write out the actual data
                            for (rowNo = 0; rowNo < data.Rows.Count; rowNo++)
                            {
                                UpdateStatus(rowNo + 1, data.Rows.Count + 1, "Writing data..", "", false);
                                rowData = new StringBuilder("");
                                for (int colNo = 0; colNo < data.Columns.Count; colNo++)
                                {
                                    string columnData = "";
                                    if (colNo != 0) rowData.Append(",");
                                    columnData = GetValue(data, tableType, colNo, rowNo, ValueType.Data);
                                    rowData.Append(columnData.Replace(',', '.'));
                                }
                                writer.WriteLine(rowData.ToString());
                            }
                            UpdateStatus(data.Rows.Count + 1, data.Rows.Count + 1, "Writing data..", "Done", false);

                        }
                        finally
                        {
                            writer.Close();
                        }
                        UpdateStatus(data.Rows.Count + 1, data.Rows.Count + 1, "File created succesfully.", "", true);
                    }
                    finally
                    {
                        fileStream.Close();
                    }
                }
                catch (Exception ex)
                {
                    UpdateStatus(rowNo + 1, data.Rows.Count + 1, "Error", ex.Message, true);
                    throw new EMMAException(ExceptionSeverity.Error, "Problem exporting to CSV", ex);
                }
            }
            catch (EMMAException)
            {
                // Just ignore this, the user will be notified by the status dialog and 
                // the error will have been logged.
            }
        }
            

        

        private string GetValue(DataTable data, TableType tableType, int colNo, int rowNo, ValueType type)
        {
            string columnData = "";
            string columnName = "";
            ColumnType columnType = ColumnType.Generic;
            string retVal = "";

            try
            {
                switch (tableType)
                {
                    case TableType.Generic:
                        columnType = ColumnType.Generic;
                        break;
                    case TableType.Journal:
                        if (data.Columns[colNo].ColumnName.Equals("TypeID"))
                        {
                            columnType = ColumnType.JournRefTypeID;
                            columnName = "Entry Type";
                        }
                        else if (data.Columns[colNo].ColumnName.Equals("OwnerID1"))
                        {
                            columnType = ColumnType.OwnerID;
                            columnName = "Owner1";
                        }
                        else if (data.Columns[colNo].ColumnName.Equals("OwnerID2"))
                        {
                            columnType = ColumnType.OwnerID;
                            columnName = "Owner2";
                        }
                        else
                        {
                            columnType = ColumnType.Generic;
                        }
                        break;
                    case TableType.Transactions:
                        if (data.Columns[colNo].ColumnName.Equals("ItemID"))
                        {
                            columnType = ColumnType.ItemID;
                            columnName = "Item";
                        }
                        else if (data.Columns[colNo].ColumnName.Equals("ClientID"))
                        {
                            columnType = ColumnType.OwnerID;
                            columnName = "Client";
                        }
                        else if (data.Columns[colNo].ColumnName.Equals("CharacterID"))
                        {
                            columnType = ColumnType.OwnerID;
                            columnName = "Character";
                        }
                        else if (data.Columns[colNo].ColumnName.Equals("StationID"))
                        {
                            columnType = ColumnType.StationID;
                            columnName = "Station";
                        }
                        else
                        {
                            columnType = ColumnType.Generic;
                        }
                        break;
                    case TableType.Assets:
                        if (data.Columns[colNo].ColumnName.Equals("StationID"))
                        {
                            string tmpData = data.Rows[rowNo].ItemArray[colNo].ToString();
                            if (tmpData.StartsWith("3"))
                            {
                                columnType = ColumnType.SolarSystemID;
                            }
                            else if (tmpData.StartsWith("6"))
                            {
                                columnType = ColumnType.StationID;
                            }
                            else
                            {
                                columnType = ColumnType.Generic;
                            }
                            columnName = "Location";
                        }
                        else if (data.Columns[colNo].ColumnName.Equals("ItemID"))
                        {
                            columnType = ColumnType.ItemID;
                            columnName = "Item";
                        }
                        else
                        {
                            columnType = ColumnType.Generic;
                        }
                        break;
                    default:
                        break;
                }


                switch (columnType)
                {
                    case ColumnType.Generic:
                        columnData = data.Rows[rowNo].ItemArray[colNo].ToString();
                        columnName = data.Columns[colNo].Caption;
                        break;
                    case ColumnType.ItemID:
                        try
                        {
                            columnData = Items.GetItemName(
                                int.Parse(data.Rows[rowNo].ItemArray[colNo].ToString()));
                        }
                        catch (Exception)
                        {
                            columnData = "Unknown Item";
                        }
                        break;
                    case ColumnType.OwnerID:
                        try
                        {
                            columnData = Names.GetName(
                                int.Parse(data.Rows[rowNo].ItemArray[colNo].ToString()));
                        }
                        catch (Exception)
                        {
                            columnData = "Unknown Entity";
                        }
                        break;
                    case ColumnType.JournRefTypeID:
                        try
                        {
                            columnData = JournalRefTypes.GetReferenceDesc(
                                short.Parse(data.Rows[rowNo].ItemArray[colNo].ToString()));
                        }
                        catch (Exception)
                        {
                            columnData = "Unknown Entry Type";
                        }
                        break;
                    case ColumnType.StationID:
                        try
                        {
                            columnData = Stations.GetStationName(
                                int.Parse(data.Rows[rowNo].ItemArray[colNo].ToString()));
                        }
                        catch (Exception)
                        {
                            columnData = "Unknown Station";
                        }
                        break;
                    case ColumnType.SolarSystemID:
                        try
                        {
                            columnData = SolarSystems.GetSystemName(
                                int.Parse(data.Rows[rowNo].ItemArray[colNo].ToString()));
                        }
                        catch (Exception)
                        {
                            columnData = "Unknown Solar System";
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                columnData = "Error: " + ex.Message;
                columnName = "Error: " + ex.Message;
            }

            switch (type)
            {
                case ValueType.ColumnName:
                    retVal = columnName;
                    break;
                case ValueType.Data:
                    retVal = columnData;
                    break;
                default:
                    break;
            }

            return retVal;
        }


        private enum ValueType
        {
            ColumnName,
            Data
        }

        private enum TableType
        {
            Generic,
            Journal,
            Transactions,
            Assets
        }

        private enum ColumnType
        {
            Generic,
            ItemID,
            OwnerID,
            JournRefTypeID,
            StationID,
            SolarSystemID
        }
    }
}
