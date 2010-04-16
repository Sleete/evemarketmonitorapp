using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace EveMarketMonitorApp.Common
{
    public enum SQLVersion
    {
        None,
        SQL2005,
        SQL2008,
        Later
    }

    /// <summary>
    /// The code in this class is mostly adapted from http://msdn.microsoft.com/en-us/library/dd981032.aspx
    /// </summary>
    public static class SQLExpressChecks
    {
        /// <summary>
        /// Enumerates all SQL Server instances on the machine.
        /// </summary>
        /// <returns></returns>
        public static SQLVersion GetSQLVersion()
        {
            SQLVersion retVal = SQLVersion.None;
            string correctNamespace = GetCorrectWmiNameSpace();
            if (!string.Equals(correctNamespace, string.Empty))
            {
                string query = string.Format("select * from SqlServiceAdvancedProperty where SQLServiceType = 1 and PropertyName = 'instanceID'");
                ManagementObjectSearcher getSqlEngine = new ManagementObjectSearcher(correctNamespace, query);
                if (getSqlEngine.Get().Count > 0)
                {
                    string serviceName = string.Empty;
                    string version = string.Empty;
                    foreach (ManagementObject sqlEngine in getSqlEngine.Get())
                    {
                        serviceName = sqlEngine["ServiceName"].ToString();
                        version = GetWmiPropertyValueForEngineService(serviceName, correctNamespace, "Version");
                    }
                    Version v = new Version(version);
                    if (v.Major > 10 && retVal == SQLVersion.None || retVal == SQLVersion.SQL2005)
                    {
                        retVal = SQLVersion.Later;
                    }
                    if (v.Major == 10 && retVal == SQLVersion.None || retVal == SQLVersion.SQL2005 || retVal == SQLVersion.Later)
                    {
                        retVal = SQLVersion.SQL2008;
                    }
                    if (v.Major == 9 && retVal == SQLVersion.None)
                    {
                        retVal = SQLVersion.SQL2005;
                    }

                    //Console.WriteLine("SQL Server database instances discovered :");
                    //string instanceName = string.Empty;
                    //string serviceName = string.Empty;
                    //string version = string.Empty;
                    //string edition = string.Empty;
                    //Console.WriteLine("Instance Name \t ServiceName \t Edition \t Version \t");
                    //foreach (ManagementObject sqlEngine in getSqlEngine.Get())
                    //{
                    //    serviceName = sqlEngine["ServiceName"].ToString();
                    //    instanceName = GetInstanceNameFromServiceName(serviceName);
                    //    version = GetWmiPropertyValueForEngineService(serviceName, correctNamespace, "Version");
                    //    edition = GetWmiPropertyValueForEngineService(serviceName, correctNamespace, "SKUNAME");
                    //    Console.Write("{0} \t", instanceName);
                    //    Console.Write("{0} \t", serviceName);
                    //    Console.Write("{0} \t", edition);
                    //    Console.WriteLine("{0} \t", version);
                    //}
                }
            }

            return retVal;
        }
        /// <summary>
        /// Method returns the correct SQL namespace to use to detect SQL Server instances.
        /// </summary>
        /// <returns>namespace to use to detect SQL Server instances</returns>
        private static string GetCorrectWmiNameSpace()
        {
            String wmiNamespaceToUse = "root\\Microsoft\\sqlserver";
            List<string> namespaces = new List<string>();
            try
            {
                // Enumerate all WMI instances of
                // __namespace WMI class.
                ManagementClass nsClass =
                    new ManagementClass(
                    new ManagementScope(wmiNamespaceToUse),
                    new ManagementPath("__namespace"),
                    null);
                foreach (ManagementObject ns in nsClass.GetInstances())
                {
                    namespaces.Add(ns["Name"].ToString());
                }
            }
            catch (ManagementException e)
            {
                if (!e.Message.ToUpper().Contains("INVALID NAMESPACE"))
                {
                    throw new EMMAException(ExceptionSeverity.Error, "Error determining installed version of SQL", e);
                }
            }
            if (namespaces.Count > 0)
            {
                if (namespaces.Contains("ComputerManagement10"))
                {
                    //use katmai+ namespace
                    wmiNamespaceToUse = wmiNamespaceToUse + "\\ComputerManagement10";
                }
                else if (namespaces.Contains("ComputerManagement"))
                {
                    //use yukon namespace
                    wmiNamespaceToUse = wmiNamespaceToUse + "\\ComputerManagement";
                }
                else
                {
                    wmiNamespaceToUse = string.Empty;
                }
            }
            else
            {
                wmiNamespaceToUse = string.Empty;
            }
            return wmiNamespaceToUse;
        }
        /// <summary>
        /// method extracts the instance name from the service name
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        private static string GetInstanceNameFromServiceName(string serviceName)
        {
            if (!string.IsNullOrEmpty(serviceName))
            {
                if (string.Equals(serviceName, "MSSQLSERVER", StringComparison.OrdinalIgnoreCase))
                {
                    return serviceName;
                }
                else
                {
                    return serviceName.Substring(serviceName.IndexOf('$') + 1, serviceName.Length - serviceName.IndexOf('$')-1);
                }
            }
            else
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// Returns the WMI property value for a given property name for a particular SQL Server service Name
        /// </summary>
        /// <param name="serviceName">The service name for the SQL Server engine serivce to query for</param>
        /// <param name="wmiNamespace">The wmi namespace to connect to </param>
        /// <param name="propertyName">The property name whose value is required</param>
        /// <returns></returns>
        private static string GetWmiPropertyValueForEngineService(string serviceName, string wmiNamespace, string propertyName)
        {
            string propertyValue = string.Empty;
            string query = String.Format("select * from SqlServiceAdvancedProperty where SQLServiceType = 1 and PropertyName = '{0}' and ServiceName = '{1}'", propertyName, serviceName);
            ManagementObjectSearcher propertySearcher = new ManagementObjectSearcher(wmiNamespace, query);
            foreach (ManagementObject sqlEdition in propertySearcher.Get())
            {
                propertyValue = sqlEdition["PropertyStrValue"].ToString();
            }
            return propertyValue;
        }
    }
}
