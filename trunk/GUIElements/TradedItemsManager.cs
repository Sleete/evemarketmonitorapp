using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.AbstractionClasses;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class TradedItemsManager : Form
    {
        EveDataSet.invTypesDataTable _itemsList = new EveDataSet.invTypesDataTable();
        TradedItems _tradedItems;
        
        public TradedItemsManager()
        {
            InitializeComponent();
        }

        private void TradedItemsManager_Load(object sender, EventArgs e)
        {
            _tradedItems = UserAccount.CurrentGroup.TradedItems;
            _itemsList = _tradedItems.GetAllItems();

            lstItems.DisplayMember = "typeName";
            lstItems.ValueMember = "typeID";
            lstItems.DataSource = _itemsList;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _tradedItems.CancelChanges();
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
            _tradedItems.Store();
        }

        private void btnAutoAdd_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                EveDataSet.invTypesDataTable newItems = Items.GetItemsTraded(
                    UserAccount.CurrentGroup.GetFinanceAccessParams(APIDataType.Full),
                    UserAccount.CurrentGroup.Settings.AutoAddMin);
                foreach (EveDataSet.invTypesRow item in newItems)
                {
                    EveDataSet.invTypesRow existing = _itemsList.FindBytypeID(item.typeID);
                    if (existing == null)
                    {
                        _itemsList.ImportRow(item);
                        _tradedItems.AddItem(item.typeID);
                    }
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnAutoAddConfig_Click(object sender, EventArgs e)
        {
            AutoAddConfig addConfig = new AutoAddConfig();
            addConfig.ShowDialog();
        }


        private void btnClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Really clear the traded items list?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _itemsList.Clear();
                _tradedItems.ClearAllItems();
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

                _tradedItems.RemoveItem(item.typeID);
                _itemsList.RemoveinvTypesRow(_itemsList.FindBytypeID(item.typeID));
            }
        }

        private void AddItem(string name)
        {
            try
            {
                EveDataSet.invTypesRow newItem = Items.GetItem(name);

                if (newItem != null)
                {
                    EveDataSet.invTypesRow existing = _itemsList.FindBytypeID(newItem.typeID);
                    if (existing == null)
                    {
                        _itemsList.ImportRow(newItem);
                        _tradedItems.AddItem(newItem.typeID);
                        lstItems.SelectedValue = newItem.typeID;
                        lstItems.Focus();
                    }
                }
            }
            catch (EMMADataException emmaDataEx)
            {
                MessageBox.Show(emmaDataEx.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
