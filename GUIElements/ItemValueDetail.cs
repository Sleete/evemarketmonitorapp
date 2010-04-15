using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class ItemValueDetail : Form
    {
        private ItemValues _itemValues = null;
        private List<string> _recentItems = new List<string>();
        private string _lastItem = "";
        private int _defaultItem = 0;
        private static string _message = "";
        private static int _messagesRecieved = 0;
        private BindingSource regionsBindingSource;

        delegate void UpdateViewCallback();

        public ItemValueDetail(int itemID)
        {
            InitializeComponent();
            _defaultItem = itemID;
            _itemValues = UserAccount.CurrentGroup.ItemValues;
        }

        private void ItemValueDetail_Load(object sender, EventArgs e)
        {
            _itemValues.ValueCalculationEvent += new ItemValueCalcEvent(ItemsTraded_ValueCalculationEvent);

            EveDataSet.mapRegionsDataTable regions = Regions.GetAllRegions();
            EveDataSet.mapRegionsRow newRow = regions.NewmapRegionsRow();
            newRow.regionID = -1;
            newRow.regionName = "All Regions";
            regions.AddmapRegionsRow(newRow);
            regionsBindingSource = new BindingSource();
            regionsBindingSource.DataSource = regions;
            regionsBindingSource.Sort = "regionName";
            cmbLocation.ValueMember = "regionID";
            cmbLocation.DisplayMember = "regionName";
            cmbLocation.DataSource = regionsBindingSource;
            cmbLocation.SelectedValue = -1;

            _recentItems = UserAccount.CurrentGroup.Settings.RecentItems;
            _recentItems.Sort();
            AutoCompleteStringCollection items = new AutoCompleteStringCollection();
            items.AddRange(_recentItems.ToArray());
            txtItem.AutoCompleteCustomSource = items;
            txtItem.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtItem.AutoCompleteMode = AutoCompleteMode.Suggest;
            txtItem.Leave += new EventHandler(txtItem_Leave);
            txtItem.KeyDown += new KeyEventHandler(txtItem_KeyDown);
            EveDataSet.invTypesRow item = Items.GetItem(_defaultItem);
            if (item != null)
            {
                txtItem.Tag = _defaultItem;
                txtItem.Text = item.typeName;
            }

            this.FormClosing += new FormClosingEventHandler(ItemValueDetail_FormClosing);
        }

        void txtItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Enter)
            {
                SetSelectedItem();
            }
        }

        void txtItem_Leave(object sender, EventArgs e)
        {
            SetSelectedItem();
        }

        private void SetSelectedItem()
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (!txtItem.Text.Equals(_lastItem))
                {
                    txtItem.Tag = 0;
                    if (!txtItem.Text.Equals(""))
                    {
                        try
                        {
                            EveDataSet.invTypesRow item = Items.GetItem(txtItem.Text);
                            if (item != null)
                            {
                                txtItem.Tag = (int)item.typeID;
                                string name = item.typeName;
                                txtItem.Text = name;
                                if (!_recentItems.Contains(name))
                                {
                                    _recentItems.Add(name);
                                    txtItem.AutoCompleteCustomSource.Add(name);
                                }
                            }
                        }
                        catch (EMMADataException) { }

                        _lastItem = txtItem.Text;
                    }

                    if ((int)txtItem.Tag == 0)
                    {
                        txtItem.Text = "";
                    }
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        void ItemValueDetail_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (UserAccount.CurrentGroup != null)
            {
                UserAccount.CurrentGroup.Settings.RecentItems = _recentItems;
            }
            _itemValues.ValueCalculationEvent -= this.ItemsTraded_ValueCalculationEvent;
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            lstDetails.Items.Clear();
            _messagesRecieved = 0;
            int itemID, regionID;
            bool buyPrice;

            try { itemID = (int)txtItem.Tag; }
            catch { itemID = 0; }
            try { regionID = (int)cmbLocation.SelectedValue; }
            catch { regionID = 0; }
            buyPrice = false;
            if (cmbBuySell.Text.ToUpper().Equals("BUY")) { buyPrice = true; }

            if (itemID != 0 && regionID != 0)
            {
                valueParams values = new valueParams(itemID, regionID, buyPrice);

                Thread t0 = new Thread(new ParameterizedThreadStart(Calculate));
                t0.Start(values);
            }
        }

        private void Calculate(object parameters)
        {
            valueParams values = (valueParams)parameters;

            if (values.buyPrice)
            {
                _itemValues.GetBuyPrice(values.itemID, values.regionID);
            }
            else
            {
                _itemValues.GetItemValue(values.itemID, values.regionID, false);
            }
        }
        
        void ItemsTraded_ValueCalculationEvent(object myObject, ValueCalcEventArgs args)
        {
            _messagesRecieved++;
            string mess = _messagesRecieved + ". " + args.Message;
            lock (_message)
            {
                _message = mess;
                UpdateList();
            }
        }

        private void UpdateList()
        {
            if (this.InvokeRequired)
            {
                UpdateViewCallback callback = new UpdateViewCallback(UpdateList);
                this.Invoke(callback, null);
            }
            else
            {
                lstDetails.Items.Insert(lstDetails.Items.Count, _message);  
            }
        }

        private void btnClearCache_Click(object sender, EventArgs e)
        {
            _itemValues.ClearValueCache();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        struct valueParams
        {
            public int itemID;
            public int regionID;
            public bool buyPrice;

            public valueParams(int itemID, int regionID, bool buyPrice)
            {
                this.itemID = itemID;
                this.regionID = regionID;
                this.buyPrice = buyPrice;
            }
        }
    }
}