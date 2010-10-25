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
    public partial class AutoAddConfig : Form
    {
        EveDataSet.staStationsDataTable _buyStations = new EveDataSet.staStationsDataTable();
        EveDataSet.staStationsDataTable _sellStations = new EveDataSet.staStationsDataTable();

        public AutoAddConfig()
        {
            InitializeComponent();
        }

        private void AutoAddConfig_Load(object sender, EventArgs e)
        {
            //cmbAddBasedOn.Text = UserAccount.CurrentGroup.Settings.AutoAddItemsBy;
            txtMinRequired.Text = UserAccount.CurrentGroup.Settings.AutoAddMin.ToString();
            txtMinPurchases.Text = UserAccount.CurrentGroup.Settings.AutoAddBuyMin.ToString();
            txtMinSales.Text = UserAccount.CurrentGroup.Settings.AutoAddSellMin.ToString();
            dtpTransactionStartDate.Value = UserAccount.CurrentGroup.Settings.AutoAddStartDate;

            _buyStations = new EveDataSet.staStationsDataTable();
            lstBuyStations.Items.Clear();
            lstBuyStations.DisplayMember = "stationName";
            lstBuyStations.ValueMember = "stationID";
            lstBuyStations.DataSource = _buyStations;
            foreach (int stationID in UserAccount.CurrentGroup.Settings.AutoAddBuyStations)
            {
                _buyStations.ImportRow(Stations.GetStation(stationID));
            }

            _sellStations = new EveDataSet.staStationsDataTable();
            lstSellStations.Items.Clear();
            lstSellStations.DisplayMember = "stationName";
            lstSellStations.ValueMember = "stationID";
            lstSellStations.DataSource = _sellStations;
            foreach (int stationID in UserAccount.CurrentGroup.Settings.AutoAddSellStations)
            {
                _sellStations.ImportRow(Stations.GetStation(stationID));
            }
        }
        

        private void txtMinRequired_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int.Parse(txtMinRequired.Text);
            }
            catch
            {
                txtMinRequired.Text = "";
            }

        }


        private void btnOk_Click(object sender, EventArgs e)
        {
            SaveSettings();
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void txtBuyStation_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                AddBuyStation();
            }
        }

        private void txtSellStation_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                AddSellStation();
            }
        }

        private void btnAddBuyStation_Click(object sender, EventArgs e)
        {
            AddBuyStation();
        }

        private void btnAddSellStation_Click(object sender, EventArgs e)
        {
            AddSellStation();
        }

        private void btnClearBuyStations_Click(object sender, EventArgs e)
        {
            _buyStations.Clear();
        }

        private void btnClearSellStations_Click(object sender, EventArgs e)
        {
            _sellStations.Clear();
        }

        private void lstBuyStations_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete)
            {
                _buyStations.RemovestaStationsRow(_buyStations.FindBystationID((int)lstBuyStations.SelectedValue));
            }
        }

        private void lstSellStations_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete)
            {
                _sellStations.RemovestaStationsRow(_sellStations.FindBystationID((int)lstSellStations.SelectedValue));
            }
        }


        private void AddBuyStation()
        {
            if (!txtBuyStation.Text.Equals(""))
            {
                try
                {
                    EveDataSet.staStationsRow station = Stations.GetStation(txtBuyStation.Text);
                    if (station != null)
                    {
                        if (_buyStations.FindBystationID(station.stationID) == null)
                        {
                            _buyStations.ImportRow(station);
                        }
                    }
                }
                catch (EMMADataException)
                {
                    MessageBox.Show("No station could be found matching the entered name.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void AddSellStation()
        {
            if (!txtSellStation.Text.Equals(""))
            {
                try
                {
                    EveDataSet.staStationsRow station = Stations.GetStation(txtSellStation.Text);
                    if (station != null)
                    {
                        if (_sellStations.FindBystationID(station.stationID) == null)
                        {
                            _sellStations.ImportRow(station);
                        }
                    }
                }
                catch (EMMADataException)
                {
                    MessageBox.Show("No station could be found matching the entered name.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SaveSettings()
        {
            //UserAccount.CurrentGroup.Settings.AutoAddItemsBy = cmbAddBasedOn.Text;
            UserAccount.CurrentGroup.Settings.AutoAddMin = int.Parse(txtMinRequired.Text);
            UserAccount.CurrentGroup.Settings.AutoAddBuyMin = int.Parse(txtMinPurchases.Text);
            UserAccount.CurrentGroup.Settings.AutoAddSellMin = int.Parse(txtMinSales.Text);
            UserAccount.CurrentGroup.Settings.AutoAddStartDate = dtpTransactionStartDate.Value;
            List<long> stationIDs = new List<long>();
            foreach (EveDataSet.staStationsRow station in _buyStations)
            {
                stationIDs.Add(station.stationID);
            }
            UserAccount.CurrentGroup.Settings.AutoAddBuyStations = stationIDs;
            stationIDs = new List<long>();
            foreach (EveDataSet.staStationsRow station in _sellStations)
            {
                stationIDs.Add(station.stationID);
            }
            UserAccount.CurrentGroup.Settings.AutoAddSellStations = stationIDs;
            UserAccount.CurrentGroup.StoreSettings();
        }




    }
}