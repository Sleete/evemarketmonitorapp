using System;
using System.Collections.Generic;
using System.Text;

using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.AbstractionClasses;

namespace EveMarketMonitorApp.DatabaseClasses
{
    class ReprocessResult : SortableObject
    {
        private int _jobID;
        private DateTime _jobDate;
        private int _itemID;
        private bool _gotItem = false;
        private string _itemName = "";
        private bool _gotQuantity = false;
        private long _quantity;
        private decimal _effectiveBuyPrice;

        // -- non database fields used by reprocessor screen --
        private long _maxQuantity;
        private bool _gotActualQuantity = false;
        private long _actualQuantity;
        private bool _gotStationTakes = false;
        private long _stationTakes;
        private bool _gotUnitSellPrice = false;
        private decimal _unitSellPrice;

        public ReprocessResult(EMMADataSet.ReprocessResultRow data)
        {
            _jobID = data.JobID;
            _jobDate = data.JobDate;
            _itemID = data.ItemID;
            _gotQuantity = true;
            _quantity = data.Quantity;
            _effectiveBuyPrice = data.EffectiveBuyPrice;
            _gotUnitSellPrice = true;
            _unitSellPrice = data.EstSellPrice / data.Quantity;
        }

        public ReprocessResult(int jobID, int itemID, long maxQuantity)
        {
            _jobID = jobID;
            _itemID = itemID;
            _maxQuantity = maxQuantity;
        }

        public int JobID
        {
            get { return _jobID; }
            set { _jobID = value; }
        }

        public DateTime JobDate
        {
            get { return _jobDate; }
        }

        public string Item
        {
            get
            {
                if (!_gotItem)
                {
                    try
                    {
                        _itemName = Items.GetItemName(_itemID);
                    }
                    catch (EMMADataMissingException)
                    {
                        _itemName = "Unknown Item";
                    }
                    _gotItem = true;
                }
                return _itemName;
            }
            set { _itemName = value; }
        }

        public int ItemID
        {
            get { return _itemID; }
            set
            {
                _itemID = value;
                _gotItem = false;
            }
        }

        public long Quantity
        {
            get 
            {
                if (!_gotQuantity)
                {
                    _quantity = ActualQuantity - StationTakes;
                    _gotQuantity = true;
                }
                return _quantity; 
            }
            set { _quantity = value; }
        }

        public decimal EffectiveBuyPrice
        {
            get { return _effectiveBuyPrice; }
            set { _effectiveBuyPrice = value; }
        }

        public decimal EstSellPrice
        {
            get { return UnitSellPrice * Quantity; }
        }

        public decimal UnitSellPrice
        {
            get
            {
                if (!_gotUnitSellPrice)
                {
                    _unitSellPrice = UserAccount.CurrentGroup.ItemValues.GetItemValue(_itemID, 10000002, false);
                    _gotUnitSellPrice = true;
                }
                return _unitSellPrice;
            }
            set
            {
                _unitSellPrice = value;
                _gotUnitSellPrice = true;
            }
        }

        public long MaxQuantity
        {
            get { return _maxQuantity; }
            set
            {
                _maxQuantity = value;
                _gotActualQuantity = false;
                _gotStationTakes = false;
                _gotQuantity = false;
            }
        }

        public long ActualQuantity
        {
            get
            {
                if (!_gotActualQuantity)
                {
                    string itemName = Items.GetItemName(_itemID);
                    double reprocessPerc = 0;
                    string[] skills = Enum.GetNames(typeof(Skills));
                    foreach (string skill in skills)
                    {
                        if (skill.Contains("Processing"))
                        {
                            string oreName = skill.Remove(skill.IndexOf("Processing")); 
                            if (itemName.Contains(oreName))
                            {
                                Skills enumSkill = (Skills)Enum.Parse(typeof(Skills), skill);
                                reprocessPerc = GetReprocessPerc(
                                    UserAccount.CurrentGroup.Settings.GetReprocessSkillLevel(enumSkill));
                            }
                        }
                    }
                    if (reprocessPerc == 0)
                    {
                        reprocessPerc = GetReprocessPerc(
                            UserAccount.CurrentGroup.Settings.GetReprocessSkillLevel(Skills.ScrapmetalProcessing));
                    }
                    _actualQuantity = (long)Math.Round(_maxQuantity * (reprocessPerc / 100), MidpointRounding.ToEven);
                    _gotActualQuantity = true;
                }
                return _actualQuantity;
            }
        }

        public long StationTakes
        {
            get
            {
                if (!_gotStationTakes)
                {
                    _stationTakes = (long)Math.Round(ActualQuantity *
                        (UserAccount.CurrentGroup.Settings.ReprocessStationWillTakePerc / 100),
                        MidpointRounding.ToEven);
                    _gotStationTakes = true;
                }
                return _stationTakes;
            }
        }

        private double GetReprocessPerc(int specificSkillLevel)
        {
            float implantMod = ((float)UserAccount.CurrentGroup.Settings.ReprocessImplantPerc) / 100.0f;
            double percentage = (UserAccount.CurrentGroup.Settings.ReprocessStationYieldPerc / 100.0f) +
                (0.375 *
                (1 + (UserAccount.CurrentGroup.Settings.GetReprocessSkillLevel(Skills.Refining) * 0.02f)) *
                (1 + (UserAccount.CurrentGroup.Settings.GetReprocessSkillLevel(Skills.RefineryEfficiency) * 0.04f)) *
                (1 + (specificSkillLevel * 0.05)) *
                (1+ implantMod));
            percentage *= 100;
            if (percentage < 0) { percentage = 0; }
            if (percentage > 100) { percentage = 100; }
            return percentage;
        }
    }
}
