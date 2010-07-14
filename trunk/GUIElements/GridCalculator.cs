using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.Reporting;
using EveMarketMonitorApp.DatabaseClasses;

namespace EveMarketMonitorApp.GUIElements
{
    //public delegate void GridCalculatorHidden(object myObject, EventArgs args);
    //public delegate void GridCalculatorShown(object myObject, EventArgs args);

    /// <summary>
    /// *** HOW TO USE THIS CLASS ***
    /// 
    /// In the constructor of the form containing the DataGridView to bind, simply add 
    /// the following code snippet:
    /// 
    /// datagrid.Tag = "Grid Name";
    /// if (Globals.calculator != null)
    /// {
    ///    Globals.calculator.BindGrid(datagrid);
    /// }
    /// 
    /// Also, catch the 'FormClosing' event and call the RemoveGrid method.
    /// </summary>
    public partial class GridCalculator : Form
    {
        private Dictionary<DataGridView, GridData> _bindings = new Dictionary<DataGridView, GridData>();

        private FrequencyTable<decimal> _frequencyTable;

        //public event GridCalculatorHidden Hidden;
        //public event GridCalculatorShown Shown;
        delegate void UpdateLabelCallback(Calculations labelID, object data);

        private Label[] _calcValueLabels = new Label[7];
        private static int _dotCounter = 0;


        /// <summary>
        /// Constructor
        /// </summary>
        public GridCalculator()
        {
            InitializeComponent();
            _frequencyTable = new FrequencyTable<decimal>();

            _calcValueLabels[(int)Calculations.SelectedValues] = lblNumValues;
            _calcValueLabels[(int)Calculations.Sum] = lblSum;
            _calcValueLabels[(int)Calculations.Average] = lblAverage;
            _calcValueLabels[(int)Calculations.Median] = lblMedian;
            _calcValueLabels[(int)Calculations.Max] = lblMaximum;
            _calcValueLabels[(int)Calculations.Min] = lblMinimum;
            _calcValueLabels[(int)Calculations.StdDev] = lblStdDev;

            UserAccount.Settings.GetFormSizeLoc(this);
            this.FormClosing += new FormClosingEventHandler(GridCalculator_FormClosing);
        }

        /// <summary>
        /// Fired when the form is closing.
        /// Do not allow the form to close unless it's parent is being closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GridCalculator_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.MdiFormClosing)
            {
                e.Cancel = true;
            }
        }

        public void StoreSizeAndPos()
        {
            if (UserAccount.Settings != null)
            {
                UserAccount.Settings.StoreFormSizeLoc(this);
            }
        }
        

        /// <summary>
        /// Bind the specified datagridview to the calculator.
        /// This means that when the user changes what is selected in the datagridview, the 
        /// calculator will automatically be updated with the results.
        /// </summary>
        /// <param name="grid"></param>
        public void BindGrid(DataGridView grid)
        {
            if (!_bindings.ContainsKey(grid) || (_bindings.ContainsKey(grid) && !_bindings[grid].Bound))
            {
                if (!_bindings.ContainsKey(grid))
                {
                    _bindings.Add(grid, new GridData(grid, true));
                }
                else
                {
                    _bindings[grid].Bound = true;
                }
                grid.SelectionChanged += new EventHandler(grid_SelectionChanged);
                RefreshGridsList();
                AddSelectedData(grid);
                Recalculate();

                if (!Visible && UserAccount.Settings.GridCalcEnabled)
                {
                    this.Show();
                }
            }
        }

        /// <summary>
        /// Unbind the specified grid from the calculator.
        /// This means that items already selected in the grid will be removed from the 
        /// data used by the calculator, also the user changing selected items in the 
        /// datagridview will no longer update to calculator.
        /// </summary>
        /// <param name="grid"></param>
        private void UnBindGrid(DataGridView grid)
        {
            if (_bindings.ContainsKey(grid) && _bindings[grid].Bound)
            {
                _bindings[grid].Bound = false;
                grid.SelectionChanged -= grid_SelectionChanged;
                RefreshGridsList();
                RemovePreviouslySelectedData(grid);
                Recalculate();
            }
        }

        /// <summary>
        /// Remove the datagridview from the list of potentially bound grids.
        /// This will also remove items already selected in the grid from the 
        /// data used by the calculator and the user changing selected items in the 
        /// datagridview will no longer update to calculator.
        /// </summary>
        /// <param name="grid"></param>
        public void RemoveGrid(DataGridView grid)
        {
            if (_bindings.ContainsKey(grid))
            {
                if (_bindings[grid].Bound)
                {
                    grid.SelectionChanged -= grid_SelectionChanged;
                    RemovePreviouslySelectedData(grid);
                }
                _bindings.Remove(grid);
                RefreshGridsList();
                Recalculate();

                if(_bindings.Count == 0) 
                {
                    this.Hide();
                }
            }
        }

        /// <summary>
        /// Fired when the user checkes/unchecks a line in the list of potentially bound grids.
        /// This will bind or unbind the appropriate grid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void boundGrids_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            GridData data = boundGrids.Items[e.Index] as GridData;
            if (data != null)
            {
                if (e.NewValue == CheckState.Checked)
                {
                    BindGrid(data.Grid);
                }
                else if (e.NewValue == CheckState.Unchecked)
                {
                    UnBindGrid(data.Grid);
                }
            }
        }

        /// <summary>
        /// Fired when a bound grid has it's selected cells changed by the user.
        /// This will remove the data that was previously selected by teh user for this grid
        /// and add the newly selected data before recalculating the results.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grid_SelectionChanged(object sender, EventArgs e)
        {
            DataGridView grid = sender as DataGridView;
            if (grid != null)
            {
                // Just make sure that the grid is definatley bound.
                if (_bindings.ContainsKey(grid) && _bindings[grid].Bound)
                {
                    RemovePreviouslySelectedData(grid);
                    AddSelectedData(grid);

                    if (Visible)
                    {
                        Recalculate();
                    }
                }
            }
        }

        /// <summary>
        /// Add data from the selected cells of the specified datagrid view to our
        /// master frequency table. 
        /// </summary>
        /// <param name="grid"></param>
        private void AddSelectedData(DataGridView grid)
        {
            bool gridHasQuantityColumn = grid.Columns.Contains("QuantityColumn");

            List<ValueFrequency> valueFrequncies = _bindings[grid].SelectedValues;
            valueFrequncies.Clear();
            foreach (DataGridViewCell cell in grid.SelectedCells)
            {
                // Only interested in isk values, ignore anything else.
                if (cell.FormattedValue.ToString().ToUpper().Contains(" ISK"))
                {
                    try
                    {
                        // Get the values of the newly selected cells and add them to the frequency table.
                        decimal value = decimal.Parse(cell.Value.ToString());
                        if (cell.Style.ForeColor == Color.Red && value > 0) { value *= -1; }
                        long quantity = 1;
                        if (chkQuantityAsFrequency.Checked && gridHasQuantityColumn)
                        {
                            try
                            {
                                quantity = long.Parse(grid["QuantityColumn", cell.RowIndex].Value.ToString());
                            }
                            catch { }
                        }
                        valueFrequncies.Add(new ValueFrequency(value, quantity));
                        lock (_frequencyTable)
                        {
                            _frequencyTable.Add(value, quantity);
                        }
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Remove the values that were last added for this datagridview from the 
        /// master frequency table
        /// </summary>
        /// <param name="grid"></param>
        private void RemovePreviouslySelectedData(DataGridView grid)
        {
            // Remove the values from the previously selected cells from the frequency table.
            List<ValueFrequency> valueFrequncies = _bindings[grid].SelectedValues;
            foreach (ValueFrequency valueFrequncy in valueFrequncies)
            {
                lock (_frequencyTable)
                {
                    _frequencyTable.Remove(valueFrequncy.value, valueFrequncy.frequency);
                }
            }
        }

        /// <summary>
        /// Fired when the 'use quantity column value as frequency' checkbox is ticked/unticked
        /// This will clear the master frequency table and re-add data from all bound grids.
        /// The results will then be recalculated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkQuantityAsFrequency_CheckedChanged(object sender, EventArgs e)
        {
            // Clear all current data.
            lock (_frequencyTable)
            {
                _frequencyTable = new FrequencyTable<decimal>();
            }

            // Get all bound grid's selected values
            Dictionary<DataGridView, GridData>.Enumerator enumerator = _bindings.GetEnumerator();
            while (enumerator.MoveNext())
            {
                DataGridView grid = enumerator.Current.Key;
                // If this grid is not bound then don't worry about it.
                if (_bindings.ContainsKey(grid) && _bindings[grid].Bound)
                {
                    AddSelectedData(grid);
                }
            }

            Recalculate();
        }

        /// <summary>
        /// Clear previous results and start a new thread to calculate the latest values from
        /// the master freequency table.
        /// </summary>
        private void Recalculate()
        {
            // Clear calculation result labels and get new values.
            foreach (Label label in _calcValueLabels)
            {
                label.Tag = null;
                label.Text = "";
            }
            _dotCounter = 0;
            calculationTimer.Start();
            // If the form is visible then recalculate the displayed data.
            Thread t0 = new Thread(new ThreadStart(RefreshCalculationLabels));
            t0.Start();
        }

        /// <summary>
        /// Refresh the list of potentially bound grids.
        /// </summary>
        private void RefreshGridsList()
        {
            boundGrids.Items.Clear();
            Dictionary<DataGridView, GridData>.Enumerator enumerator = _bindings.GetEnumerator();
            while (enumerator.MoveNext())
            {
                boundGrids.Items.Add(enumerator.Current.Value, enumerator.Current.Value.Bound);
            }
        }

        /// <summary>
        /// Update the tags of the result labels with the results of calculations
        /// from the frequency table.
        /// </summary>
        private void RefreshCalculationLabels()
        {
            lock (_frequencyTable)
            {
                _calcValueLabels[(int)Calculations.SelectedValues].Tag = (decimal)_frequencyTable.SampleSize;
                _calcValueLabels[(int)Calculations.Sum].Tag = (decimal)_frequencyTable.Sum;
                double mean = _frequencyTable.Mean;
                _calcValueLabels[(int)Calculations.Average].Tag = 
                    double.IsInfinity(mean) || double.IsNaN(mean) ? 0.0m : (decimal)mean;
                double median = _frequencyTable.Median;
                _calcValueLabels[(int)Calculations.Median].Tag = double.IsNaN(median) ? 0.0m : (decimal)median;
                _calcValueLabels[(int)Calculations.Max].Tag = (decimal)_frequencyTable.Maximum;
                _calcValueLabels[(int)Calculations.Min].Tag = (decimal)_frequencyTable.Minimum;
                double stdDev = _frequencyTable.StandardDevPop;
                _calcValueLabels[(int)Calculations.StdDev].Tag = 
                    double.IsInfinity(stdDev) || double.IsNaN(stdDev) ? 0.0m : (decimal)stdDev;

            }
        }

        /// <summary>
        /// Set the text of the specified results label (thread safe)
        /// </summary>
        /// <param name="labelID"></param>
        /// <param name="data"></param>
        private void SetLabelValue(Calculations labelID, object data)
        {
            if (this.InvokeRequired)
            {
                UpdateLabelCallback callback = new UpdateLabelCallback(SetLabelValue);
                object[] args = new object[2];
                args[0] = labelID;
                args[1] = data;
                this.Invoke(callback, args);
            }
            else
            {
                Label label = _calcValueLabels[(int)labelID];

                string labelText = "";
                try
                {
                    decimal value = (decimal)data;
                    if (labelID == Calculations.SelectedValues)
                    {
                        labelText = value.ToString();
                    }
                    else
                    {
                        labelText = new IskAmount(value).ToString();
                    }
                }
                catch
                {
                    labelText = data.ToString();
                }
                label.Text = labelText;
            }
        }

        /// <summary>
        /// Fired when the timer ticks over. 
        /// This will update the result labels with the data from thier tags, if the tags are null
        /// then the tag will display a sequence of full stops ('.') until the tag data
        /// is populated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void calculationTimer_Tick(object sender, EventArgs e)
        {
            string dottext = "";
            bool complete = true;

            for (int i = 0; i < _dotCounter; i++)
            {
                dottext = dottext + ".";
            }
            for (int i = 0; i < _calcValueLabels.Length; i++)
            {
                Label label = _calcValueLabels[i];
                if (label.Tag == null)
                {
                    SetLabelValue((Calculations)i, dottext);
                    complete = false;
                }
                else
                {
                    SetLabelValue((Calculations)i, label.Tag);
                }
            }

            _dotCounter++;
            if (_dotCounter > 3)
            {
                _dotCounter = 0;
            }
            if (complete)
            {
                calculationTimer.Stop();
            }
        }


        #region 'GridData' Inner class and 'ValueFrequency' struct
        private class GridData
        {
            private bool _bound;
            private DataGridView _grid;
            private List<ValueFrequency> _selectedValues;
            
            public GridData(DataGridView grid, bool bound)
            {
                _grid = grid;
                _bound = bound;
                _selectedValues = new List<ValueFrequency>();
            }

            public bool Bound
            {
                get { return _bound; }
                set { _bound = value; }
            }

            public List<ValueFrequency> SelectedValues
            {
                get { return _selectedValues; }
            }

            public DataGridView Grid
            {
                get { return _grid; }
            }

            public override string ToString()
            {
                return _grid.Tag.ToString();
            }
        }

        private struct ValueFrequency
        {
            public decimal value;
            public long frequency;

            public ValueFrequency(decimal value, long frequency)
            {
                this.value = value;
                this.frequency = frequency;
            }
        }
        #endregion


        private enum Calculations
        {
            SelectedValues,
            Sum,
            Average,
            Median,
            Max,
            Min,
            StdDev
        }


    }
}