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
                //case ChangeType.Made:
                //    retVal = "Made";
                //    break;
                case ChangeType.DestroyedOrUsed:
                    retVal = "Destroyed or Used";
                    break;
                case ChangeType.ForSaleViaContract:
                    retVal = "For sale via contract";
                    break;
                case ChangeType.BoughtViaContract:
                    retVal = "Bought via contract";
                    break;
                case ChangeType.WasNeverMissing:
                    retVal = "Was never missing";
                    break;
                case ChangeType.CancelledContract:
                    retVal = "Sell contract cancelled";
                    break;
                case ChangeType.NotLost:
                    retVal = "Not lost";
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

        public static List<AssetChangeType> GetAllChangeTypes(ChangeMetaType metaType)
        {
            List<AssetChangeType> retVal = new List<AssetChangeType>();
            Array values = Enum.GetValues(typeof(ChangeType));
            for (int i = 0; i < values.Length; i++)
            {
                bool addit = false;
                switch (metaType)
                {
                    case ChangeMetaType.Any:
                        if ((UserAccount.Settings.ManufacturingMode || i != (int)ChangeType.Unknown))
                        {
                            addit = true;
                        }
                        break;
                    case ChangeMetaType.Loss:
                        if (i == (int)ChangeType.DestroyedOrUsed || i == (int)ChangeType.ForSaleViaContract ||
                            i == (int)ChangeType.NotLost ||
                            (UserAccount.Settings.ManufacturingMode && i == (int)ChangeType.Unknown))
                        {
                            addit = true;
                        }
                        break;
                    case ChangeMetaType.Gain:
                        if (i == (int)ChangeType.Found || /*i == (int)ChangeType.Made ||*/
                            i == (int)ChangeType.BoughtViaContract || i == (int)ChangeType.WasNeverMissing ||
                            i == (int)ChangeType.CancelledContract ||
                            (UserAccount.Settings.ManufacturingMode && i == (int)ChangeType.Unknown))
                        {
                            addit = true;
                        }
                        break;
                    default:
                        break;
                }
                if (addit) { retVal.Add(new AssetChangeType(i, GetChangeTypeDesc((ChangeType)i))); }
            }
            return retVal;
        }


        public enum ChangeType
        {
            Found,
            //Made,
            DestroyedOrUsed,
            ForSaleViaContract,
            BoughtViaContract,
            WasNeverMissing,
            CancelledContract,
            NotLost,
            Unknown
        }

        public enum ChangeMetaType
        {
            Any,
            Loss,
            Gain
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
