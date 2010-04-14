using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.DatabaseClasses;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class GroupLocationList : Form
    {
        private GroupLocationsList _locations;
        private BindingSource _locationsBindingSource;

        public GroupLocationList()
        {
            InitializeComponent();
            this.Load += new EventHandler(GroupLocationList_Load);
        }

        void GroupLocationList_Load(object sender, EventArgs e)
        {
            try
            {
                _locations = GroupLocations.GetGroupLocationsData();
                _locationsBindingSource = new BindingSource();
                _locationsBindingSource.DataSource = _locations;

                locationsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                locationsGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

                locationsGrid.AutoGenerateColumns = false;
                locationsGrid.DataSource = _locationsBindingSource;
                NameColumn.DataPropertyName = "Name";
                DescriptionColumn.DataPropertyName = "Description";
            }
            catch (Exception ex)
            {
                // Creating new EMMAexception will cause error to be logged.
                EMMAException emmaex = ex as EMMAException;
                if (emmaex == null)
                {
                    emmaex = new EMMAException(ExceptionSeverity.Critical, "Error setting up group location form", ex);
                }
                MessageBox.Show("Problem setting up group location view.\r\nCheck " + Globals.AppDataDir + "Logging\\ExceptionLog.txt" +
                    " for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReloadData()
        {
            _locations = GroupLocations.GetGroupLocationsData();
            _locationsBindingSource.DataSource = _locations;
        }


        private void locationsGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                GroupLocationMaint maint = new GroupLocationMaint(
                    locationsGrid["NameColumn", e.RowIndex].Value.ToString());
                maint.ShowDialog();
                ReloadData();
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            GroupLocationMaint maint = new GroupLocationMaint();
            maint.ShowDialog();
            ReloadData();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (locationsGrid.SelectedRows[0] != null)
            {
                GroupLocation selectedLocation;
                selectedLocation = (GroupLocation)locationsGrid.SelectedRows[0].DataBoundItem;
                GroupLocations.DeleteLocation(selectedLocation.Name);
                ReloadData();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }


    }
}