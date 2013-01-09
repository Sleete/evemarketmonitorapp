using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public static class IndustryJobs
    {
        private static EMMADataSetTableAdapters.IndustryJobsTableAdapter _tableAdapter = 
            new EMMADataSetTableAdapters.IndustryJobsTableAdapter();

        public static bool GetJob(EMMADataSet.IndustryJobsDataTable table, long jobID) 
        {
            _tableAdapter.ClearBeforeFill = false;
            lock (_tableAdapter)
            {
                _tableAdapter.FillByID(table, jobID);
            }
            EMMADataSet.IndustryJobsRow row = table.FindByID(jobID);
            return row != null;
        }

        public static EMMADataSet.IndustryJobsDataTable GetJobs()
        {
            EMMADataSet.IndustryJobsDataTable table = new EMMADataSet.IndustryJobsDataTable();
            lock (_tableAdapter)
            {
                _tableAdapter.Fill(table);
            }
            return table;
        }

        public static void Store(EMMADataSet.IndustryJobsDataTable table)
        {
            lock (_tableAdapter)
            {
                _tableAdapter.Update(table);
            }
        }
    }
}
