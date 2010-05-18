using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public static class AssetChangeTypes
    {

        public static string GetChangeTypeDesc(ChangeType changeType)
        {
            string retVal = "";
            switch (changeType)
            {
                case ChangeType.Found:
                    retVal = "Found";
                    break;
                case ChangeType.Made:
                    retVal = "Made";
                    break;
                case ChangeType.DestroyedOrUsed:
                    retVal = "Destroyed or Used";
                    break;
                case ChangeType.ForSaleViaContract:
                    retVal = "For sale via contract";
                    break;
                case ChangeType.Unknown:
                    retVal = "Unknown";
                    break;
                default:
                    retVal = "-Undefined-";
                    break;
            }

            return retVal;
        }

        public static List<AssetChangeType> GetAllChangeTypes()
        {
            List<AssetChangeType> retVal = new List<AssetChangeType>();
            Array values = Enum.GetValues(typeof(ChangeType));
            for (int i = 0; i < values.Length; i++)
            {
                retVal.Add(new AssetChangeType(i, GetChangeTypeDesc((ChangeType)i)));
            }
            return retVal;
        }


        public enum ChangeType
        {
            Found,
            Made,
            DestroyedOrUsed,
            ForSaleViaContract,
            Unknown
        }
    }

    public class AssetChangeType
    {
        private int _id;
        private string _desc;

        public AssetChangeType(int id, string desc)
        {
            _id = id;
            _desc = desc;
        }

        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }
        public string Description
        {
            get { return _desc; }
            set { _desc = value; }
        }

    }
}
