using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;

namespace EveMarketMonitorApp.Common
{
    /// <summary>
    /// Base EMMA Exception. All other EMMA exceptions must inherit from this type
    /// CVS test comment
    /// </summary>
    public class EMMAException : ApplicationException
    {
        private ExceptionSeverity severity;
        public static string logFile = string.Format("{0}Logging{1}ExceptionLog.txt",
            Globals.AppDataDir, Path.DirectorySeparatorChar);

        public ExceptionSeverity Severity 
        {
            get { return severity; }
        }

        public EMMAException(ExceptionSeverity severity)
            : base("EMMA has encountered an unspecified error.")
        {
            this.severity = severity;
            WriteToLog();
        }
        public EMMAException(ExceptionSeverity severity, string message)
            : base(message)
        {
            this.severity = severity;
            WriteToLog();
        }
        public EMMAException(ExceptionSeverity severity, string message, Exception inner)
            : base(message, inner)
        {
            this.severity = severity;
            WriteToLog();
        }
        public EMMAException(ExceptionSeverity severity, string message, bool writeToLog)
            : base(message)
        {
            this.severity = severity;
            if (writeToLog) { WriteToLog(); }
        }

        private void WriteToLog()
        {
            lock (Globals.ExceptionFileLock)
            {
                StreamWriter writer;
                if (!File.Exists(logFile))
                {
                    writer = File.CreateText(logFile);
                    try
                    {
                        writer.Write("");
                    }
                    finally
                    {
                        writer.Close();
                    }
                }

                writer = File.AppendText(logFile);

                try
                {
                    string outputText = "";
                    outputText = outputText + "=====================================================================================\r\n";
                    outputText = outputText + "                                   EXCEPTION REPORT\r\n";
                    outputText = outputText + "=====================================================================================\r\n";
                    outputText = outputText + DateTime.Now + " " + severity + " - " + this.GetType().ToString() + " - " + Message;
                    string tabs = "\t";

                    Exception ex = this;
                    while (ex.InnerException != null)
                    {
                        outputText += "\r\n" + tabs + "Inner Exception: " + ex.InnerException.Message;
                        outputText += (ex.InnerException.StackTrace == null ? "" : "\r\n" + tabs +
                            "Stack Trace: " + ex.InnerException.StackTrace);
                        ex = ex.InnerException;
                        tabs = tabs + "\t";
                    }
                    outputText += (this.StackTrace == null ? "" : "\r\nStack Trace: " + this.StackTrace);

                    writer.WriteLine(outputText);
                }
                finally
                {
                    writer.Close();
                }
            }
        }

    }


    public class EMMASettingsException : EMMAException
    {
        public EMMASettingsException(ExceptionSeverity severity)
            : base(severity, "Error in EMMA's settings sub-system") { }
        public EMMASettingsException(ExceptionSeverity severity, string message)
            : base(severity, message) { }
        public EMMASettingsException(ExceptionSeverity severity, string message, Exception inner)
            : base(severity, message, inner) { }
    }

    public class EMMAEveAPIException : EMMAException
    {
        private int eveCode = 0;
        private string eveDescription = "";

        public EMMAEveAPIException(ExceptionSeverity severity)
            : base(severity, "Error in EMMA's Eve API communication sub-system") { }
        public EMMAEveAPIException(ExceptionSeverity severity, string message)
            : base(severity, message) { }
        public EMMAEveAPIException(ExceptionSeverity severity, string message, Exception inner)
            : base(severity, message, inner) { }
        public EMMAEveAPIException(ExceptionSeverity severity, int eveApiErrorCode, string eveApiErrorDesc)
            : base(severity, "Eve API error.\r\nCode: " + eveApiErrorCode + "\r\nDescription: " + eveApiErrorDesc,
            eveApiErrorCode != 101 && eveApiErrorCode != 102 && eveApiErrorCode != 103 && eveApiErrorCode != 117)
        {
            this.eveCode = eveApiErrorCode;
            this.eveDescription = eveApiErrorDesc;
        }

        public string EveDescription
        {
            get { return eveDescription; }
            set { eveDescription = value; }
        }

        public int EveCode
        {
            get { return eveCode; }
            set { eveCode = value; }
        }
    }

    public class EMMADataException : EMMAException
    {
        public EMMADataException(ExceptionSeverity severity)
            : base(severity, "Error in EMMA's database sub-system") { }
        public EMMADataException(ExceptionSeverity severity, string message)
            : base(severity, message) { }
        public EMMADataException(ExceptionSeverity severity, string message, Exception inner)
            : base(severity, message, inner) { }
    }

    public class EMMADataMissingException : EMMADataException
    {
        private string tableName = "";
        private string key = "";

        public EMMADataMissingException(ExceptionSeverity severity, string tableName, string key)
            : base(severity, "Data is missing from EMMA's database") 
        {
            this.tableName = tableName;
            this.key = key;
        }
        public EMMADataMissingException(ExceptionSeverity severity, string message, string tableName, string key)
            : base(severity, message)
        {
            this.tableName = tableName;
            this.key = key;
        }
        public EMMADataMissingException(ExceptionSeverity severity, string message, Exception inner, string tableName,
            string key)
            : base(severity, message, inner)
        {
            this.tableName = tableName;
            this.key = key;
        }

        public string TableName
        {
            get { return tableName; }
            set { tableName = value; }
        }

        public string Key
        {
            get { return key; }
            set { key = value; }
        }
    }

    public class EMMACompressionException : EMMAException
    {
        public EMMACompressionException(ExceptionSeverity severity)
            : base(severity, "Error in EMMA's file compression sub-system") { }
        public EMMACompressionException(ExceptionSeverity severity, string message)
            : base(severity, message) { }
        public EMMACompressionException(ExceptionSeverity severity, string message, Exception inner)
            : base(severity, message, inner) { }
    }

    public class EMMAReportingException : EMMAException
    {
        public EMMAReportingException(ExceptionSeverity severity)
            : base(severity, "Error in EMMA's reporting sub-system") { }
        public EMMAReportingException(ExceptionSeverity severity, string message)
            : base(severity, message) { }
        public EMMAReportingException(ExceptionSeverity severity, string message, Exception inner)
            : base(severity, message, inner) { }
    }

    public class EMMAInvalidPasswordException : EMMAException
    {
        public EMMAInvalidPasswordException(ExceptionSeverity severity)
            : base(severity, "Invalid password entered") { }
        public EMMAInvalidPasswordException(ExceptionSeverity severity, string message)
            : base(severity, message) { }
        public EMMAInvalidPasswordException(ExceptionSeverity severity, string message, Exception inner)
            : base(severity, message, inner) { }
    }

    
    public enum ExceptionSeverity
    {
        Critical,
        Error,
        Warning
    }



}
