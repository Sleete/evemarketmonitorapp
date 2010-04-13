using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using EveMarketMonitorApp.GUIElements;

namespace EveMarketMonitorApp.Common
{
    static class Globals
    {
        private static string _appDataDir = string.Format("{0}{1}EMMA{1}", 
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Path.DirectorySeparatorChar);
        private static string _autoLoginFile =
            string.Format("{0}AutoLogin.xml", _appDataDir);

        public static string AutoLoginFile
        {
            get { return Globals._autoLoginFile; }
        }
        public static string AppDataDir
        {
            get { return Globals._appDataDir; }
        }

        // Put a few global locks to resources in here to control multi-threaded access to
        // files, etc.
        // Note: locks for database commands, etc are stored in the appropriate database 
        // access class.
        public static object APIErrorFileLock = new object();
        public static object ExceptionFileLock = new object();
        public static object JournalAPIUpdateLock = new object();
        public static object TransactionAPIUpdateLock = new object();


        public static bool EveAPIDown = false;
        public static bool EveCentralDown = false;
        public static bool EvePricesDown = false;
        public static bool EveMetricsDown = false;
        public static string EMMAUpdateServer = "";

        private static bool _gotTimeOffset = false;
        private static double _hoursOffset = 0;

        public static double HoursOffset
        {
            get
            {
                double retVal = 0;
                if (!_gotTimeOffset)
                {
                    TimeSpan difference = DateTime.Now.Subtract(DateTime.UtcNow);
                    _hoursOffset = difference.TotalHours;
                    _gotTimeOffset = true;
                }
                retVal = _hoursOffset;
                return retVal;
            }
        }

        public static GridCalculator calculator = null;

        public static char[] letters = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
        public static char[] numbers = { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
        


    }
}
