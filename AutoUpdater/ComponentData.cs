using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data;

namespace AutoUpdater
{
    public class ComponentData
    {
        private string _name;
        private string _fullPath;
        private Version _latestVersion;
        private Version _currentVersion;
        private string _description;
        private bool _exists;

        public ComponentData(string name, string fullPath)
        {
            _name = name;
            _fullPath = fullPath;
        }

        public ComponentData(XmlNode xml, string homeDir)
        {
            bool error = false;
            _name = xml.SelectSingleNode("@name").Value;
            XmlNode subpathnode = xml.SelectSingleNode("@subpath");
            string subpath = (subpathnode == null ? "" : subpathnode.Value + Path.DirectorySeparatorChar);
            _fullPath = homeDir + Path.DirectorySeparatorChar + subpath + _name;
            // Eve Data files need to go in the user's applciation directory location instead.
            if (_name.ToUpper().Equals("EVEDATA.MDF") || _name.ToUpper().Equals("EVEDATA_LOG.LDF"))
            {
                _fullPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + 
                    Path.DirectorySeparatorChar + "EMMA" + Path.DirectorySeparatorChar + subpath + _name;
            }
            _latestVersion = new Version(xml.SelectSingleNode("@version").Value);
            _description = xml.SelectSingleNode("@description").Value;
            if (File.Exists(_fullPath))
            {
                if (_name.ToLower().EndsWith(".xml"))
                {
                    // Get version from the xml file itself.
                    XmlDocument xmlDoc = new XmlDocument();
                    try
                    {
                        xmlDoc.Load(_fullPath);
                        XmlNode versionNode = xmlDoc.FirstChild.NextSibling.SelectSingleNode("@version");
                        if (versionNode != null)
                        {
                            _currentVersion = new Version(versionNode.Value);
                        }
                    }
                    catch { error = true; }
                }
                else if (_name.ToLower().EndsWith(".mdf"))
                {
                    try
                    {
                        SqlConnection connection = new SqlConnection(
                            @"Data Source=.\SQLEXPRESS;" +
                            @"AttachDbFilename=" + _fullPath + ";" +
                            "Integrated Security=True;User Instance=True;Pooling=false");
                        SqlCommand command = null;
                        connection.Open();

                        try
                        {
                            command = new SqlCommand("_GetVersion", connection);
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            string value = "";
                            SqlParameter param = new SqlParameter("@version", SqlDbType.VarChar, 50,
                                ParameterDirection.InputOutput, 0, 0, null, DataRowVersion.Current,
                                false, value, "", "", "");
                            command.Parameters.Add(param);
                            command.ExecuteNonQuery();
                            string versionString = param.Value.ToString();
                            _currentVersion = new Version(versionString);
                        }
                        finally
                        {
                            if (connection != null) { connection.Close(); }
                        }
                    }
                    catch { error = true; }
                }
                else
                {
                    try
                    {
                        FileVersionInfo info = FileVersionInfo.GetVersionInfo(_fullPath);
                        if (info.FileVersion != null)
                        {
                            _currentVersion = new Version(info.FileVersion);
                        }
                        else
                        {
                            _currentVersion = new Version();
                        }
                    }
                    catch { error = true; }
                }
                _exists = true;
                if (error) { _currentVersion = new Version(0,0); }
            }
            else
            {
                _exists = false;
            }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public string FullPath
        {
            get { return _fullPath; }
            set { _fullPath = value; }
        }
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
        public Version latestVersion
        {
            get { return _latestVersion; }
            set { _latestVersion = value; }
        }
        public Version currentVersion
        {
            get { return _currentVersion; }
            set { _currentVersion = value; }
        }
        public bool Exists
        {
            get { return _exists; }
            set { _exists = value; }
        }
    }

}
