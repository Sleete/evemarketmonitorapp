using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using EveMarketMonitorApp.AbstractionClasses;
using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.Reporting;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class ItemValuesManager : Form
    {
        EveDataSet.invTypesDataTable itemsList = new EveDataSet.invTypesDataTable();
        EMMADataSet.ItemValuesRow currentItem;
        ItemValues itemValues;
        bool _resetCache = false;

        public ItemValuesManager()
        {
            InitializeComponent();
            lblCalculatedSellPrice.Text = "";
            lblLastUpdated.Text = "";
        }

        private void SelectItemsTraded_Load(object sender, EventArgs e)
        {
            txtItemSellPrice.Enabled = false;
            txtDefaultBuyPrice.Enabled = false;
            cmbStation.Enabled = false;
            chkUseReprocessVal.Enabled = false;
            chkForceDefaultBuyPrice.Enabled = false;
            chkForceDefaultSellPrice.Enabled = false;

            EveDataSet.mapRegionsDataTable regions = Regions.GetAllRegions();
            EveDataSet.mapRegionsRow newRow = regions.NewmapRegionsRow();
            newRow.regionID = -1;
            newRow.regionName = "All Regions";
            regions.AddmapRegionsRow(newRow);

            cmbStation.ValueMember = "regionID";
            cmbStation.DisplayMember = "regionName";
            cmbStation.DataSource = regions;
            cmbStation.SelectedValue = -1;

            itemValues = UserAccount.CurrentGroup.ItemValues;
            itemsList = itemValues.GetAllItems();

            lstItems.DisplayMember = "typeName";
            lstItems.ValueMember = "typeID";
            lstItems.DataSource = itemsList;

            chkEveMarketPrices.Checked = (UserAccount.CurrentGroup.Settings.UseEveCentral || UserAccount.CurrentGroup.Settings.UseEveMetrics);
            ReportGroupSettings.EveMarketValueToUse value = UserAccount.CurrentGroup.Settings.EveMarketType;
            rdbMaxBuy.Checked = false;
            rdbMedianBuy.Checked = false;
            switch (value)
            {
                case ReportGroupSettings.EveMarketValueToUse.medianBuy:
                    rdbMedianBuy.Checked = true;
                    break;
                case ReportGroupSettings.EveMarketValueToUse.maxBuy:
                    rdbMaxBuy.Checked = true;
                    break;
                default:
                    break;
            }
            rdbEveCentral.Checked = UserAccount.CurrentGroup.Settings.UseEveCentral;
            rdbEveMetrics.Checked = UserAccount.CurrentGroup.Settings.UseEveMetrics;
            txtDaysBetweenUpdates.Text = UserAccount.CurrentGroup.Settings.ItemValueWebExpiryDays.ToString();
            //RefreshList();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            itemValues.CancelChanges();
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            SaveAll();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void SaveAll()
        {
            StoreDefaultPrices();
            itemValues.Store();
            bool oldEveCentralVal = UserAccount.CurrentGroup.Settings.UseEveCentral;
            bool oldEveMetricsVal = UserAccount.CurrentGroup.Settings.UseEveMetrics;

            // Reset the cache if they flipped the option between eve metrics and eve central
            if (rdbEveMetrics.Checked != oldEveMetricsVal || rdbEveCentral.Checked != oldEveCentralVal) 
            { 
                _resetCache = true;
            }

            // Reset the cache if they changed whether they want any prices at all
            bool oldUseMarketValues = (UserAccount.CurrentGroup.Settings.UseEveCentral || UserAccount.CurrentGroup.Settings.UseEveMetrics);
            if ((rdbEveMetrics.Checked || rdbEveCentral.Checked) != oldUseMarketValues)
            {
                _resetCache = true;
            }

            // If use market prices is unchecked, then we need to clear both eve central and eve metrics. Otherwise, set them to their selection value
            if (chkEveMarketPrices.Checked)
            {
                UserAccount.CurrentGroup.Settings.UseEveCentral = rdbEveCentral.Checked;
                UserAccount.CurrentGroup.Settings.UseEveMetrics = rdbEveMetrics.Checked;
            }
            else
            {
                UserAccount.CurrentGroup.Settings.UseEveCentral = false;
                UserAccount.CurrentGroup.Settings.UseEveMetrics = false;
            }
            ReportGroupSettings.EveMarketValueToUse oldVal2 = UserAccount.CurrentGroup.Settings.EveMarketType;
            if (rdbMedianBuy.Checked)
            {
                if (oldVal2 == ReportGroupSettings.EveMarketValueToUse.maxBuy) { _resetCache = true; }
                UserAccount.CurrentGroup.Settings.EveMarketType = ReportGroupSettings.EveMarketValueToUse.medianBuy;
            }
            else if (rdbMaxBuy.Checked)
            {
                if (oldVal2 == ReportGroupSettings.EveMarketValueToUse.medianBuy) { _resetCache = true; }
                UserAccount.CurrentGroup.Settings.EveMarketType = ReportGroupSettings.EveMarketValueToUse.maxBuy;
            }
            if (_resetCache)
            {
                itemValues.ClearValueCache();
                _resetCache = false;
            }

            int itemValueWebExpiryDays = int.MinValue;
            try
            {
                itemValueWebExpiryDays = int.Parse(txtDaysBetweenUpdates.Text);
            }
            catch { }
            if (itemValueWebExpiryDays > 0)
            {
                UserAccount.CurrentGroup.Settings.ItemValueWebExpiryDays = itemValueWebExpiryDays;
            }
            else if (itemValueWebExpiryDays == int.MinValue)
            {
                MessageBox.Show("Invalid value specified for number of days between item value web updates." +
                    " Value not modified.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show("Value specified for number of days between item value web updates" +
                    " cannot be negative. It has not been modified.", "Warning", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /*private void RefreshList()
        {
            if (lstItems.SelectedIndex < 0 && itemsList.Count > 0)
            {
                lstItems.SelectedIndex = 0;
            }
            else if (lstItems.SelectedIndex >= 0 && itemsList.Count > 1)
            {
                lstItems.BeginUpdate();
                lstItems.SelectedIndex = lstItems.SelectedIndex + 
                    (lstItems.SelectedIndex == itemsList.Count - 1 ? -1 : 1);
                lstItems.SelectedIndex = lstItems.SelectedIndex +
                    (lstItems.SelectedIndex == itemsList.Count - 1 ? 1 : -1);
            }
            cmbStation.SelectedValue = -1;
        }*/

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Really clear all item value data?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                currentItem = null;
                itemsList.Clear();
                itemValues.ClearAllItems();
                //RefreshList();
                RefreshDefaultPrices();
            }
        }


        private void txtItemName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Return)
            {
                AddItem(txtItemName.Text);
            }
        }

        private void lstItems_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete)
            {
                EveDataSet.invTypesRow item = (EveDataSet.invTypesRow)((DataRowView)lstItems.SelectedItem).Row;

                currentItem = null;
                itemValues.RemoveItem(item.typeID);
                itemsList.RemoveinvTypesRow(itemsList.FindBytypeID(item.typeID));
                //RefreshList();
                RefreshDefaultPrices();
            }
        }

        private void AddItem(string name) 
        {
            try
            {
                EveDataSet.invTypesRow newItem = Items.GetItem(name);

                if (newItem != null)
                {
                    EveDataSet.invTypesRow existing = itemsList.FindBytypeID(newItem.typeID);
                    if (existing == null)
                    {
                        itemsList.ImportRow(newItem);
                        itemValues.AddItem(newItem.typeID);
                        lstItems.SelectedValue = newItem.typeID;
                        //RefreshList();
                        lstItems.Focus();
                    }
                }
            }
            catch (EMMADataException emmaDataEx)
            {
                MessageBox.Show(emmaDataEx.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lstItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            StoreDefaultPrices();
            RefreshDefaultPrices();
        }

        private void cmbStation_SelectedIndexChanged(object sender, EventArgs e)
        {
            StoreDefaultPrices();
            RefreshDefaultPrices();
        }

        private void txtItemSellPrice_Enter(object sender, EventArgs e)
        {
            if (currentItem != null)
            {
                txtItemSellPrice.Text = currentItem.DefaultSellPrice.ToString();
            }
        }

        private void txtItemSellPrice_Leave(object sender, EventArgs e)
        {
            try
            {
                currentItem.DefaultSellPrice = decimal.Parse(txtItemSellPrice.Text);
            }
            catch
            {
                currentItem.DefaultSellPrice = 0;
            }

            txtItemSellPrice.Text = new IskAmount(currentItem.DefaultSellPrice).ToString();
        }

        private void txtItemBuyPrice_Enter(object sender, EventArgs e)
        {
            if (currentItem != null)
            {
                txtDefaultBuyPrice.Text = currentItem.DefaultBuyPrice.ToString();
            }
        }

        private void txtItemBuyPrice_Leave(object sender, EventArgs e)
        {
            try
            {
                currentItem.DefaultBuyPrice = decimal.Parse(txtDefaultBuyPrice.Text);
            }
            catch
            {
                currentItem.DefaultBuyPrice = 0;
            }

            txtDefaultBuyPrice.Text = new IskAmount(currentItem.DefaultBuyPrice).ToString();
        }

        private void StoreDefaultPrices()
        {
            if (currentItem != null)
            {
                itemValues.UseReprocessValSet(currentItem.ItemID, chkUseReprocessVal.Checked);
                bool oldVal = itemValues.ForceDefaultBuyPriceGet(currentItem.ItemID);
                if (oldVal != chkForceDefaultBuyPrice.Checked) { _resetCache = true; }
                itemValues.ForceDefaultBuyPriceSet(currentItem.ItemID, chkForceDefaultBuyPrice.Checked);
                bool oldVal2 = itemValues.ForceDefaultSellPriceGet(currentItem.ItemID);
                if (oldVal2 != chkForceDefaultSellPrice.Checked) { _resetCache = true; }
                itemValues.ForceDefaultSellPriceSet(currentItem.ItemID, chkForceDefaultSellPrice.Checked);
                decimal oldVal3 = itemValues.DefaultPriceGet(currentItem.ItemID, currentItem.RegionID);
                if (oldVal3 != currentItem.DefaultSellPrice) { _resetCache = true; }
                itemValues.DefaultPriceSet(currentItem.ItemID, currentItem.RegionID, currentItem.DefaultSellPrice);
                decimal oldVal4 = itemValues.DefaultBuyPriceGet(currentItem.ItemID, currentItem.RegionID);
                if (oldVal4 != currentItem.DefaultBuyPrice) { _resetCache = true; }
                itemValues.DefaultBuyPriceSet(currentItem.ItemID, currentItem.RegionID, currentItem.DefaultBuyPrice);
            }
        }

        private void RefreshDefaultPrices()
        {
            if (lstItems.SelectedValue != null)
            {
                cmbStation.Enabled = true;
                txtItemSellPrice.Enabled = true;
                txtDefaultBuyPrice.Enabled = true;
                chkUseReprocessVal.Enabled = true;
                chkForceDefaultBuyPrice.Enabled = true;
                chkForceDefaultSellPrice.Enabled = true;

                int itemID = ((EveDataSet.invTypesRow)(((DataRowView)lstItems.SelectedItem).Row)).typeID;
                currentItem = itemValues.GetItem(itemID, (int)cmbStation.SelectedValue);

                txtItemSellPrice.Text = new IskAmount(currentItem.DefaultSellPrice).ToString();
                txtDefaultBuyPrice.Text = new IskAmount(currentItem.DefaultBuyPrice).ToString();
                lblCalculatedSellPrice.Text = new IskAmount(currentItem.CurrentSellPrice).ToString();
                lblLastUpdated.Text = currentItem.LastSellPriceCalc.ToString();
                // For this setting, get the value from the static class as it must always be for 
                // the -1 'Any region' setting.
                chkUseReprocessVal.Checked = itemValues.UseReprocessValGet(itemID);
                chkForceDefaultBuyPrice.Checked = itemValues.ForceDefaultBuyPriceGet(itemID);
                chkForceDefaultSellPrice.Checked = itemValues.ForceDefaultSellPriceGet(itemID);
            }
            else
            {
                cmbStation.Enabled = false;
                txtItemSellPrice.Enabled = false;
                txtDefaultBuyPrice.Enabled = false;
                chkUseReprocessVal.Enabled = false;
                chkForceDefaultBuyPrice.Enabled = false;
                chkForceDefaultSellPrice.Enabled = false;
            }
        }

        private void btnValueHist_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Currently under construction", "Coming soon", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            //ItemValueHistory history = new ItemValueHistory();
            //history.Show();
        }

        private void btnValueCalculationDetail_Click(object sender, EventArgs e)
        {
            SaveAll();
            ItemValueDetail detail = new ItemValueDetail(currentItem == null ? 24 : currentItem.ItemID);
            detail.Show();
        }

        private void lnkEveCentral_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("www.eve-central.com");
        }

        private void lnkEveMetrics_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("www.eve-metrics.com");
        }

    }
}