using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using EveMarketMonitorApp.Reporting;
using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.AbstractionClasses;

namespace EveMarketMonitorApp.GUIElements
{
    /// <summary>
    /// Note: To edit this form in the designer, replace ": RptParamsBase" with ": Form".
    /// Make sure to change it back once you've completed and saved your changes though!
    /// </summary>
    public partial class RptParamsItems : RptParamsBase
    {
        public RptParamsItems()
        {
            InitializeComponent();
            _needAssetParams = true;
            _needFinanceParams = true;

            cmbLocation.Sorted = true;
        }

        private void ItemReportParams_Load(object sender, EventArgs e)
        {
            bool cancel = false;

            //if (UserAccount.CurrentGroup.ItemsTraded.IsEmpty())
            //{
            //    MessageBox.Show("You have nothing in your items traded list. You must set this up before producing an " +
            //        "item report. You will now be taken to the items traded list setup screen.", "Information",
            //        MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    SelectItemsTraded itemsTraded = new SelectItemsTraded();
            //    if (itemsTraded.ShowDialog() == DialogResult.Cancel)
            //    {
            //        cancel = true;
            //    }
            //}

            if (!cancel)
            {
                dtpStartDate.Value = DateTime.Now.AddDays(-7);
                dtpEndDate.Value = DateTime.Now;
                //dtpEndDate.Enabled = false;
                chkItemsByGroup.Checked = false;

                string[] columns = ItemReport.GetPossibleColumns();
                chkColumns.Items.AddRange(columns);
                for (int i = 0; i < chkColumns.Items.Count; i++)
                {
                    chkColumns.SetItemChecked(i, true);
                }

                List<string> locations = GroupLocations.GetLocationNames();
                if (!locations.Contains("All Regions"))
                {
                    locations.Add("All Regions");
                }
                cmbLocation.Items.AddRange(locations.ToArray());
                cmbLocation.AutoCompleteSource = AutoCompleteSource.ListItems;
                cmbLocation.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                cmbLocation.SelectedItem = "All Regions";
            }
            else
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void btnNewLocation_Click(object sender, EventArgs e)
        {
            GroupLocationMaint newLocation = new GroupLocationMaint();
            if (newLocation.ShowDialog() == DialogResult.OK)
            {
                cmbLocation.Items.Add(newLocation.FilterName);
                cmbLocation.SelectedItem = newLocation.FilterName;
            }
        }

        private void btnModifyLocation_Click(object sender, EventArgs e)
        {
            if (GroupLocations.NameExists(cmbLocation.Text))
            {
                GroupLocationMaint maintLocation = new GroupLocationMaint(cmbLocation.Text);
            }
            else
            {
                MessageBox.Show("The currently entered location filter name does not exist in the database.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (!cmbLocation.Items.Contains(cmbLocation.Text))
            {
                MessageBox.Show("The location filter must be chosen from the list of those available." +
                    " To setup a new filter, click the 'Create New' button", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (_parameters == null)
                {
                    _parameters = new Dictionary<string, object>();
                }

                _parameters.Add("StartDate", dtpStartDate.Value.ToUniversalTime());
                _parameters.Add("EndDate", dtpEndDate.Value.ToUniversalTime());
                List<long> regions = new List<long>();
                List<long> stations = new List<long>();
                if (cmbLocation.Text.Equals("All Regions"))
                {
                    regions.Add(0);
                    stations.Add(0);
                }
                else
                {
                    GroupLocation locationFilter = GroupLocations.GetLocationDetail(cmbLocation.Text);
                    regions = locationFilter.Regions;
                    stations = locationFilter.Stations;
                }
                _parameters.Add("RegionIDs", regions);
                _parameters.Add("StationIDs", stations);

                List<int> itemIds = new List<int>();
                EMMADataSet.IDTableDataTable ids = Transactions.GetInvolvedItemIDs(this._financeAccessParams,
                    1, dtpStartDate.Value.ToUniversalTime(), dtpEndDate.Value.ToUniversalTime());
                List<int> tradedItemIDs = UserAccount.CurrentGroup.TradedItems.GetAllItemIDs();
                foreach (EMMADataSet.IDTableRow id in ids)
                {
                    if (!chkTradedItems.Checked || tradedItemIDs.Contains((int)id.ID))
                    {
                        itemIds.Add((int)id.ID);
                    }
                }
                _parameters.Add("ItemIDs", itemIds);

                string[] columnNames = ItemReport.GetPossibleColumns();
                bool[] colsVisible = new bool[columnNames.Length];
                for (int i = 0; i < columnNames.Length; i++)
                {
                    colsVisible[i] = chkColumns.CheckedItems.Contains(columnNames[i]);
                }
                _parameters.Add("ColumnsVisible", colsVisible);
                _parameters.Add("UseMostRecentBuyPrice", chkUseRecentBuyPrices.Checked);
                _parameters.Add("RestrictedCostCalc", chkRestrictedCostCalc.Checked);
                _parameters.Add("TradedItemsOnly", chkTradedItems.Checked);

                _report = new ItemReport(chkItemsByGroup.Checked);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }


    }
}