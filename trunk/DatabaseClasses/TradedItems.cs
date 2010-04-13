using System;
using System.Collections.Generic;
using System.Text;

using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.AbstractionClasses;
using EveMarketMonitorApp.GUIElements;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public class TradedItems
    {
        private static EMMADataSetTableAdapters.TradedItemsTableAdapter tableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.TradedItemsTableAdapter();
        private static EMMADataSet.TradedItemsDataTable table = new EMMADataSet.TradedItemsDataTable();

        private int _reportGroupID;

        public TradedItems(int reportGroupID)
        {
            _reportGroupID = reportGroupID;
            lock (tableAdapter)
            {
                tableAdapter.FillByID(table, _reportGroupID, 0);
            }
        }

        public EveDataSet.invTypesDataTable GetAllItems()
        {
            EveDataSet.invTypesDataTable retVal = new EveDataSet.invTypesDataTable();
            foreach (EMMADataSet.TradedItemsRow item in table)
            {
                EveDataSet.invTypesRow existing = retVal.FindBytypeID((short)item.ItemID);
                if (existing == null)
                {
                    retVal.ImportRow(Items.GetItem(item.ItemID));
                }
            }
            return retVal;
        }

        public List<int> GetAllItemIDs()
        {
            List<int> retVal = new List<int>();
            foreach (EMMADataSet.TradedItemsRow item in table)
            {
                if (!retVal.Contains(item.ItemID))
                {
                    retVal.Add(item.ItemID);
                }
            }
            return retVal;
        }

        public void Store()
        {
            try
            {
                lock (tableAdapter)
                {
                    tableAdapter.Update(table);
                }
            }
            catch (Exception ex)
            {
                throw new EMMADataException(ExceptionSeverity.Critical, "Unable to store changes to " +
                    "Traded Items table.", ex);
            }
            table.AcceptChanges();
        }

        public void CancelChanges()
        {
            table.RejectChanges();
        }

        public void AddItem(int itemID)
        {
            EMMADataSet.TradedItemsRow item = table.FindByReportGroupIDItemID(_reportGroupID, itemID);
            if (item == null)
            {
                item = table.NewTradedItemsRow();
                item.ReportGroupID = _reportGroupID;
                item.ItemID = itemID;
                table.AddTradedItemsRow(item);
            }
            else if (item.RowState == System.Data.DataRowState.Deleted)
            {
                item.RejectChanges();
            }
        }

        public void RemoveItem(int itemID)
        {
            EMMADataSet.TradedItemsDataTable toBeRemoved = new EMMADataSet.TradedItemsDataTable();
            tableAdapter.FillByID(toBeRemoved, _reportGroupID, itemID);
            foreach (EMMADataSet.TradedItemsRow item in toBeRemoved)
            {
                table.FindByReportGroupIDItemID(_reportGroupID, itemID).Delete();
            }
        }

        public void ClearAllItems()
        {
            for (int i = 0; i < table.Count; i++)
            {
                EMMADataSet.TradedItemsRow item = table[i];
                item.Delete();
            }
        }

    }

}
