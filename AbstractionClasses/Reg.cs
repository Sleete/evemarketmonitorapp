using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.AbstractionClasses
{
    /*
    public static class Reg
    {
        public static string ReadLM(string KeyName)
        {
            string retVal = "";
            try
            {
                RegistryKey regkey = Registry.LocalMachine.OpenSubKey("Software").OpenSubKey("EMMA");
                retVal = regkey.GetValue(KeyName).ToString();
            }
            catch (KeyNotFoundException)
            {
                retVal = "NOT FOUND";
            }
            catch (Exception ex)
            {
                throw new EMMAException(ExceptionSeverity.Critical,
                    "Problem accessing registry settings", ex);
            }

            return retVal;
        }

        public static string WriteLM(string KeyName, string value)
        {
            string retVal = "";
            try
            {
                RegistryKey regkey = Registry.LocalMachine.OpenSubKey("Software").OpenSubKey("EMMA", true);
                regkey.SetValue(KeyName, value);
            }
            catch (Exception ex)
            {
                throw new EMMAException(ExceptionSeverity.Critical, 
                    "Problem storing registry settings", ex);
            }

            return retVal;
        }

        public static void Init()
        {
            try
            {
                try
                {
                    RegistryKey regkey = Registry.LocalMachine.OpenSubKey("Software").OpenSubKey("EMMA");
                }
                catch (KeyNotFoundException)
                {
                    Registry.LocalMachine.OpenSubKey("Software").CreateSubKey("EMMA");
                }
            }
            catch (Exception ex)
            {
                throw new EMMAException(ExceptionSeverity.Critical,
                    "Problem setting up values in registry", ex);
            }
        }
    }*/
}
