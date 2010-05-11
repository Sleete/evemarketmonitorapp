using System;
using System.Collections.Generic;
using System.Text;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public static class AssetStatus
    {
        private static EMMADataSet.AssetStatusesDataTable table = new EMMADataSet.AssetStatusesDataTable();
        private static EMMADataSetTableAdapters.AssetStatusesTableAdapter tableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.AssetStatusesTableAdapter();

        public static string GetDescription(int status)
        {
            string retVal = "";
            if(table.Count == 0) {LoadData();}
            EMMADataSet.AssetStatusesRow data = table.FindByStatusID(status);
            if (data != null) { retVal = data.Description.Trim(); }
            return retVal;
        }

        private static void LoadData()
        {
            tableAdapter.Fill(table);
        }

        public enum States : int
        {
            Normal =1,
            InTransit =2,
            ForSaleViaContract = 3,
            ForSaleViaMarket = 4
        }
    }
}
