using System;
using System.Collections.Generic;
using System.Text;

using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public enum BankTransactionType : short
    {
        Deposit = 1,
        InterestPayment = 2,
        Withdrawl = 3,
        ManualAdjustment = 4
    }

    public static class BankTransactionTypes
    {
        private static Cache<short, string> _descriptions = new Cache<short,string>(10);
        private static EMMADataSetTableAdapters.BankTransTypesTableAdapter _tableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.BankTransTypesTableAdapter();
        private static bool _initalised = false;


        private static void Initalise()
        {
            if (!_initalised)
            {
                _descriptions.DataUpdateNeeded +=
                    new Cache<short, string>.DataUpdateNeededHandler(Descriptions_DataUpdateNeeded);
                _initalised = true;
            }
        }

        public static string GetDescription(BankTransactionType type)
        {
            if (!_initalised) { Initalise(); }
            return _descriptions.Get((short)type);
        }

        static void Descriptions_DataUpdateNeeded(object myObject,
            DataUpdateNeededArgs<short, string> args)
        {
            string desc = "";
            _tableAdapter.GetDesc(args.Key, ref desc);
            args.Data = desc;
        }

        public static EMMADataSet.BankTransTypesDataTable GetAll()
        {
            EMMADataSet.BankTransTypesDataTable retVal =
                new EMMADataSet.BankTransTypesDataTable();

            _tableAdapter.Fill(retVal);

            return retVal;
        }
    }


    public enum CorpPayoutPeriod : short
    {
        Dailey = 1,
        Weekly = 2,
        BiWeekly = 3,
        Monthly30 = 4,
        Monthly28 = 5,
        BiMonthly = 6,
        Quaterly = 7,
        Unspecified = 8
    }

    public static class CorpPayoutPeriods
    {
        private static Cache<short, string> _descriptions = new Cache<short, string>(10);
        private static EMMADataSetTableAdapters.PublicCorpPayoutPeriodTableAdapter _tableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.PublicCorpPayoutPeriodTableAdapter();
        private static bool _initalised = false;


        private static void Initalise()
        {
            if (!_initalised)
            {
                _descriptions.DataUpdateNeeded +=
                    new Cache<short, string>.DataUpdateNeededHandler(Descriptions_DataUpdateNeeded);
                _initalised = true;
            }
        }

        public static string GetDescription(CorpPayoutPeriod type)
        {
            if (!_initalised) { Initalise(); }
            return _descriptions.Get((short)type);
        }

        static void Descriptions_DataUpdateNeeded(object myObject,
            DataUpdateNeededArgs<short, string> args)
        {
            string desc = "";
            _tableAdapter.GetDesc(args.Key, ref desc);
            args.Data = desc;
        }

        public static EMMADataSet.PublicCorpPayoutPeriodDataTable GetAll()
        {
            EMMADataSet.PublicCorpPayoutPeriodDataTable retVal =
                new EMMADataSet.PublicCorpPayoutPeriodDataTable();

            _tableAdapter.Fill(retVal);

            return retVal;
        }
    }

    public enum OrderState : short
    {
        Closed = 1,
        ExpiredOrFilled = 2,
        Cancelled = 3,
        Pending = 4,
        CharacterDeleted = 5,
        Active = 999,
        ExpiredOrFilledAndUnacknowledged = 1000,
        OverbidAndUnacknowledged = 1001,
        ExpiredOrFilledAndAcknowledged = 2000
    }

    public static class OrderStates
    {
        private static EMMADataSet.OrderStatesDataTable _statesTable = new EMMADataSet.OrderStatesDataTable();
        private static EMMADataSetTableAdapters.OrderStatesTableAdapter _tableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.OrderStatesTableAdapter();


        public static EMMADataSet.OrderStatesDataTable GetAllStates()
        {
            EMMADataSet.OrderStatesDataTable table = new EMMADataSet.OrderStatesDataTable();
            lock (_tableAdapter)
            {
                _tableAdapter.Fill(table);
            }
            return table;
        }

        public static string GetStateDescription(short stateID)
        {
            string retVal = "";
            if (_statesTable.Count == 0) { LoadData(); }
            EMMADataSet.OrderStatesRow stateRow = _statesTable.FindByStateID(stateID);
            if (stateRow != null)
            {
                retVal = stateRow.Description;
            }
            return retVal;
        }


        private static void LoadData()
        {
            try
            {
                _tableAdapter.Fill(_statesTable);
            }
            catch (Exception ex)
            {
                throw new EMMADataException(ExceptionSeverity.Critical, "Unable to load order states " +
                    "from EMMA database", ex);
            }
        }

    }


    public enum RiskRating : short
    {
        NotRated = 1,
        LowRisk = 2,
        MediumRisk = 3,
        HighRisk = 4,
        Scam = 5
    }

    public static class RiskRatings
    {
        private static Cache<short, string> _descriptions = new Cache<short, string>(10);
        private static EMMADataSetTableAdapters.RiskRatingTableAdapter _tableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.RiskRatingTableAdapter();
        private static bool _initalised = false;


        private static void Initalise()
        {
            if (!_initalised)
            {
                _descriptions.DataUpdateNeeded +=
                    new Cache<short, string>.DataUpdateNeededHandler(Descriptions_DataUpdateNeeded);
                _initalised = true;
            }
        }

        public static string GetDescription(RiskRating type)
        {
            if (!_initalised) { Initalise(); }
            return _descriptions.Get((short)type);
        }

        static void Descriptions_DataUpdateNeeded(object myObject,
            DataUpdateNeededArgs<short, string> args)
        {
            string desc = "";
            _tableAdapter.GetDesc(args.Key, ref desc);
            args.Data = desc;
        }

        public static EMMADataSet.RiskRatingDataTable GetAll()
        {
            EMMADataSet.RiskRatingDataTable retVal =
                new EMMADataSet.RiskRatingDataTable();

            _tableAdapter.Fill(retVal);

            return retVal;
        }
    }


    public enum ContractType : short
    {
        Any = 0,
        Courier = 1,
        ItemExchange = 2,
        Cargo = 3
    }

    public static class ContractTypes
    {
        private static Cache<short, string> _descriptions = new Cache<short, string>(10);
        private static EMMADataSetTableAdapters.ContractTypeTableAdapter _tableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.ContractTypeTableAdapter();
        private static bool _initalised = false;


        private static void Initalise()
        {
            if (!_initalised)
            {
                _descriptions.DataUpdateNeeded +=
                    new Cache<short, string>.DataUpdateNeededHandler(Descriptions_DataUpdateNeeded);
                _initalised = true;
            }
        }

        public static string GetDescription(ContractType type)
        {
            if (!_initalised) { Initalise(); }
            return _descriptions.Get((short)type);
        }

        static void Descriptions_DataUpdateNeeded(object myObject,
            DataUpdateNeededArgs<short, string> args)
        {
            string desc = "";
            _tableAdapter.GetDesc(args.Key, ref desc);
            args.Data = desc;
        }

        public static EMMADataSet.ContractTypeDataTable GetAll()
        {
            EMMADataSet.ContractTypeDataTable retVal =
                new EMMADataSet.ContractTypeDataTable();

            _tableAdapter.Fill(retVal);

            return retVal;
        }
    }

}
