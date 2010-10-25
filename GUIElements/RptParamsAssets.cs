using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using EveMarketMonitorApp.Reporting;
using EveMarketMonitorApp.DatabaseClasses;

namespace EveMarketMonitorApp.GUIElements
{
    /// <summary>
    /// Note: To edit this form in the designer, replace ": RptParamsBase" with ": Form".
    /// Make sure to change it back once you've completed and saved your changes though!
    /// </summary>
    public partial class RptParamsAssets : RptParamsBase
    {
        BindingSource regionsBindingSource;

        public RptParamsAssets()
        {
            InitializeComponent();
            this.Text = "Assets Report Parameters";
        }

        private void AssetsReportParams_Load(object sender, EventArgs e)
        {
            chkItemsByGroup.Checked = false;
            chkInTransit.Checked = true;
            chkIncludeContainers.Checked = false;
            
            string[] columns = AssetsReport.GetPossibleColumns();
            chkColumns.Items.AddRange(columns);
            for (int i = 0; i < chkColumns.Items.Count; i++)
            {
                chkColumns.SetItemChecked(i, true);
            }

            EveDataSet.mapRegionsDataTable regions = Regions.GetAllRegions();
            regionsBindingSource = new BindingSource();
            regionsBindingSource.DataSource = regions;
            regionsBindingSource.Sort = "regionName";
            cmbValueRegion.ValueMember = "regionID";
            cmbValueRegion.DisplayMember = "regionName";
            cmbValueRegion.DataSource = regionsBindingSource;
            // Default to The Forge
            cmbValueRegion.SelectedValue = 10000002;

            List<string> locations = GroupLocations.GetLocationNames();
            if (!locations.Contains("All Regions"))
            {
                locations.Add("All Regions");
            }
            cmbLocation.Items.AddRange(locations.ToArray());
            cmbLocation.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbLocation.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbLocation.SelectedItem = "All Regions";

            btnNewLocation.Click += new EventHandler(btnNewLocation_Click);
            btnModifyLocation.Click += new EventHandler(btnModifyLocation_Click);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (_parameters == null)
            {
                _parameters = new Dictionary<string, object>();
            }

            string[] columnNames = AssetsReport.GetPossibleColumns();
            bool[] colsVisible = new bool[columnNames.Length];
            for (int i = 0; i < columnNames.Length; i++)
            {
                colsVisible[i] = chkColumns.CheckedItems.Contains(columnNames[i]);
            }
            _parameters.Add("ColumnsVisible", colsVisible);
            _parameters.Add("ValueRegion", (long)cmbValueRegion.SelectedValue);
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
            _parameters.Add("IncludeInTransit", chkInTransit.Checked);
            _parameters.Add("IncludeContainers", chkIncludeContainers.Checked);

            _report = new AssetsReport(chkItemsByGroup.Checked);

            this.DialogResult = DialogResult.OK;
            this.Close();
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
                maintLocation.ShowDialog();
            }
            else
            {
                MessageBox.Show("The currently entered location filter name does not exist in the database.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}