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
                    break;
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
}
