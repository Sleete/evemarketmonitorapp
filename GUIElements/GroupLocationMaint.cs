using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class GroupLocationMaint : Form
    {
        private GroupLocation _filterDetail;
        private EveDataSet.staStationsDataTable _stations;

        public string FilterName
        {
            get { return _filterDetail.Name; }
        }

        public GroupLocationMaint()
        {
            InitializeComponent();
            _filterDetail = new GroupLocation();
            DisplayFilter();
        }

        public GroupLocationMaint(string locationFilterName)
        {
            InitializeComponent();

            _filterDetail = GroupLocations.GetLocationDetail(locationFilterName);
            DisplayFilter();
        }

        private void DisplayFilter()
        {
            EveDataSet.mapRegionsDataTable regions = Regions.GetAllRegions();

            _stations = new EveDataSet.staStationsDataTable();
            lstStations.Items.Clear();
            lstStations.DisplayMember = "stationName";
            lstStations.ValueMember = "stationID";
            lstStations.DataSource = _stations;

            string[] ranges = OrderRange.GetStandardRanges();
            cmbRange.Items.AddRange(ranges);
            cmbRange.SelectedIndex = 0;

            if (!_filterDetail.Name.Equals(""))
            {
                txtLocationName.Text = _filterDetail.Name;
                txtLocationName.Enabled = false;

                foreach (EveDataSet.mapRegionsRow region in regions)
                {
                    chkRegions.Items.Add(new MiniRegion(region.regionID, region.regionName),
                        _filterDetail.Regions.Contains(region.regionID));
                }

                foreach (int station in _filterDetail.Stations)
                {
                    _stations.ImportRow(Stations.GetStation(station));
                }
                txtStation.Text = Stations.GetStationName(_filterDetail.StationID);
                cmbRange.SelectedIndex = cmbRange.Items.IndexOf(OrderRange.GetRangeText(_filterDetail.Range));
            }
            else
            {
                foreach (EveDataSet.mapRegionsRow region in regions)
                {
                    chkRegions.Items.Add(new MiniRegion(region.regionID, region.regionName));
                }
            }

            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (chkRegions.CheckedItems.Count > 0)
            {
                lstStations.Enabled = false;
                txtStation.Enabled = false;
            }
            else
            {
                lstStations.Enabled = true;
                txtStation.Enabled = true;
            }

            if (_stations.Count == 1)
            {
                chkRegions.Enabled = false;
                cmbRange.Enabled = true;
            }
            else if (_stations.Count > 1)
            {
                chkRegions.Enabled = false;
                cmbRange.SelectedIndex = 0;
                cmbRange.Enabled = false;
            }
            else
            {
                chkRegions.Enabled = true;
                cmbRange.SelectedIndex = 0;
                cmbRange.Enabled = false;
            }
        }


        private void chkRegions_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            RefreshDisplay();
        }

        private void txtLocationName_Leave(object sender, EventArgs e)
        {
            if (!txtLocationName.Text.Equals(""))
            {
                if (GroupLocations.NameExists(txtLocationName.Text))
                {
                    MessageBox.Show("The location filter '" + txtLocationName.Text + "' already exists in" +
                        " the database. If you save it, the old filter will be overwritten." +
                        " If you do not wish this to happen then either change the filter name or cancel.",
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void txtStation_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                if (!txtStation.Text.Equals(""))
                {
                    try
                    {
                        EveDataSet.staStationsRow station = Stations.GetStation(txtStation.Text);
                        if (_stations.FindBystationID(station.stationID) == null)
                        {
                            _stations.ImportRow(station);
                            RefreshDisplay();
                        }
                    }
                    catch (EMMADataException)
                    {
                        MessageBox.Show("No station could be found matching the entered name.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (SaveFilter())
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private bool SaveFilter()
        {
            bool done = false;

            if (txtLocationName.Text.Equals(""))
            {
                MessageBox.Show("You must enter a name for this location filter.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                try
                {
                    _filterDetail.Name = txtLocationName.Text;
                    _filterDetail.Range = OrderRange.GetRangeFromText(cmbRange.Items[cmbRange.SelectedIndex].ToString());
                    // If range is region wide then remove the station and just add it's region to the filter 
                    if (_filterDetail.Range == OrderRange.GetRangeFromText("Region"))
                    {
                        int region = Stations.GetStation(_stations[0].stationID).regionID;
                        _stations.Clear();
                        for (int i = 0; i < chkRegions.Items.Count; i++)
                        {
                            chkRegions.SetSelected(i, true);
                            if (((MiniRegion)chkRegions.SelectedItem)._regionID == region)
                            {
                                chkRegions.SetItemChecked(i, true);
                            }
                        }
                    }
                    List<long> regionIDs = new List<long>();

                    foreach (object region in chkRegions.CheckedItems)
                    {
                        MiniRegion regionData = ((MiniRegion)region);
                        regionIDs.Add(regionData._regionID);
                    }
                    _filterDetail.Regions = regionIDs;

                    List<long> stationIDs = new List<long>();
                    if (_stations.Count > 1)
                    {
                        // If more than one station selected then simply add thier IDs to the filter
                        foreach (EveDataSet.staStationsRow station in _stations)
                        {
                            stationIDs.Add(station.stationID);
                        }
                        _filterDetail.Stations = stationIDs;
                        _filterDetail.StationID = 0;
                    }
                    else if (_stations.Count == 1)
                    {
                        // If only one station selected then we need to set the station ID list based upon
                        // the selected range.
                        _filterDetail.Stations = new List<long>();
                        _filterDetail.StationID = _stations[0].stationID;
                        int range = _filterDetail.Range;
                        List<long> systemIDs = new List<long>();
                        if (range > 0)
                        {
                            // If range is greater than 0 then find the IDs of any systems within range.
                            systemIDs = SolarSystemDistances.GetSystemsInRange(
                               Stations.GetStation(_filterDetail.StationID).solarSystemID, range);
                        }
                        if (range != -1)
                        {
                            // If range is anything except -1 then add the stations in all of the solar systems
                            // in range to the list of stations in the filter.
                            systemIDs.Add(Stations.GetStation(_filterDetail.StationID).solarSystemID);
                            foreach (long systemID in systemIDs)
                            {
                                stationIDs.AddRange(Stations.GetStationsInSystem(systemID));
                            }
                        }
                        else
                        {
                            // If the range is -1 then we just want the one station...
                            stationIDs.Add(_filterDetail.StationID);
                        }
                        _filterDetail.Stations = stationIDs;
                    }

                    GroupLocations.StoreLocation(_filterDetail);
                    done = true;
                }
                catch (Exception ex)
                {
                    EMMAException emmaEx = ex as EMMAException;
                    if (emmaEx == null)
                    {
                        emmaEx = new EMMAException(ExceptionSeverity.Error, "Problem storing custom location.", ex);
                    }
                    MessageBox.Show(emmaEx.Message + ": " + ex.Message, "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return done;
        }

        private void lstStations_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete)
            {
                _stations.RemovestaStationsRow(_stations.FindBystationID((int)lstStations.SelectedValue));
            }
        }


        private class MiniRegion
        {
            public int _regionID;
            public string _regionName;

            public MiniRegion(int regionID, string regionName)
            {
                _regionID = regionID;
                _regionName = regionName;
            }

            public override string ToString()
            {
                return _regionName;
            }
        }


    }
}