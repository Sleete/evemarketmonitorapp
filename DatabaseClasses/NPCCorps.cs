using System;
using System.Collections.Generic;
using System.Text;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public static class NPCCorps
    {
        private static EveDataSetTableAdapters.crpNPCCorporationsTableAdapter tableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EveDataSetTableAdapters.crpNPCCorporationsTableAdapter();


        public static EveDataSet.crpNPCCorporationsRow GetCorp(long id)
        {
            EveDataSet.crpNPCCorporationsDataTable table = new EveDataSet.crpNPCCorporationsDataTable();
            EveDataSet.crpNPCCorporationsRow retVal = null;

            lock (tableAdapter)
            {
                tableAdapter.FillByID(table, (int)id);
            }

            if (table.Count > 0)
            {
                retVal = table[0];
            }

            return retVal;
        }
    }
}
