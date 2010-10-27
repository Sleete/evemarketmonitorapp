using System;
using System.Collections.Generic;
using System.Text;

using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public class GroupLocation : SortableObject
    {
        private string _name;
        private List<long> _regions;
        private List<long> _stations;
        private long _stationID;
        private int _range;
        private int _reportGroupID;

        public GroupLocation()
        {
            _name = "";
            _regions = new List<long>();
            _stations = new List<long>();
            _stationID = 0;
            _range = 0;
            _reportGroupID = UserAccount.CurrentGroup.ID;
        }

        public GroupLocation(EMMADataSet.GroupLocationRow data)
        {
            if (data != null)
            {
                _name = data.LocationName.Trim();

                char[] delim = {','};
                string[] regions = data.RegionIDs.Split(delim, StringSplitOptions.RemoveEmptyEntries);
                _regions = new List<long>();
                foreach (string region in regions)
                {
                    _regions.Add(long.Parse(region));
                }

                string[] stations = data.StationIDs.Split(delim, StringSplitOptions.RemoveEmptyEntries);
                _stations = new List<long>();
                foreach (string station in stations)
                {
                    _stations.Add(long.Parse(station));
                }

                _stationID = data.StationID;
                _range = data.Range;
                _reportGroupID = data.ReportGroupID;
            }
        }


        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public List<long> Regions
        {
            get { return _regions; }
            set { _regions = value; }
        }

        public List<long> Stations
        {
            get { return _stations; }
            set { _stations = value; }
        }

        public long StationID
        {
            get { return _stationID; }
            set { _stationID = value; }
        }
        
        public int Range
        {
            get { return _range; }
            set { _range = value; }
        }

        public int ReportGroupID
        {
            get { return _reportGroupID; }
            set { _reportGroupID = value; }
        }

        public string Description
        {
            get
            {
                StringBuilder retVal = new StringBuilder("");
                if (_regions.Count > 0)
                {
                    foreach (int region in _regions)
                    {
                        if (retVal.Length > 0) { retVal.Append(", "); }
                        retVal.Append(EveMarketMonitorApp.DatabaseClasses.Regions.GetRegionName(region));
                    }
                }
                else if (_stationID != 0)
                {
                    retVal.Append(EveMarketMonitorApp.DatabaseClasses.Stations.GetStationName(_stationID));
                    if (_range != -1)
                    {
                        retVal.Append(" + All Stations Within ");
                        retVal.Append(OrderRange.GetRangeText(_range));
                    }
                }
                else
                {
                    foreach (int station in _stations)
                    {
                        if (retVal.Length > 0) { retVal.Append(", "); }
                        retVal.Append(EveMarketMonitorApp.DatabaseClasses.Stations.GetStationName(station));
                    }
                }
                return retVal.ToString();
            }
        }
    }
}
