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
        private string _otherComponent = "";
        private string _otherCompFullPath = "";
        private bool _exists;
        private bool _permenant;

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

            _permenant = true;
            XmlNode permenantnode = xml.SelectSingleNode("@permenant");
            try
            {
                if (permenantnode != null) { _permenant = bool.Parse(permenantnode.Value.ToString()); }
            }
            catch { }

            string subpath = (subpathnode == null ? "" : subpathnode.Value);
            _fullPath = Path.Combine(Path.Combine(homeDir, subpath), _name);
            // Non-permenant files need to go in the user's applciation directory location instead.
            if (!_permenant)
            {
                _fullPath = Path.Combine(Path.Combine(Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "EMMA"), subpath), _name);
            }
            _latestVersion = new Version(xml.SelectSingleNode("@version").Value);
            _description = xml.SelectSingleNode("@description").Value;

            // If we need to check this component's version number against another component
            // then work out where that component should be.
            XmlNode otherCompNode = xml.SelectSingleNode("@checkOtherComponentVersion");
            XmlNode otherCompSubPathNode = xml.SelectSingleNode("@otherComponentSubPath");
            XmlNode otherCompPermNode = xml.SelectSingleNode("@otherComponentPermenant");
            bool otherCompPerm = true;
            string otherCompSubPath = "";
            if (otherCompNode != null) { _otherComponent = otherCompNode.Value; }
            if (otherCompSubPathNode != null) { otherCompSubPath = otherCompSubPathNode.Value; }
            _otherCompFullPath = Path.Combine(Path.Combine(homeDir, otherCompSubPath), _otherComponent);
            try
            {
                if (otherCompPermNode != null) { otherCompPerm = bool.Parse(otherCompPermNode.Value.ToString()); }
            }
            catch { }
            if (!otherCompPerm)
            {
                _otherCompFullPath = Path.Combine(Path.Combine(Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "EMMA"), otherCompSubPath), _otherComponent);
            }


            // If we're comparing to another component and the new component already exists
            // then consider it the correct version.
            // This is to stop the updater continually thinking there are new updates available.
            // e.g. database is 1.5.2.0
            // we look for updates and databaseupdate_1_5_3_0 is available because the database is 
            // less than 1.5.3.0
            // The update is downloaded and the user told to restart EMMA.
            // Before the update is applied, EMMA will check for updates again, the database will 
            // still be 1.5.2.0 so if we don't stop it, it would download the same update again.
            if (File.Exists(_fullPath) && _otherComponent.Length > 0)
            {
                _exists = true;
                _currentVersion = _latestVersion;
            }
            // Work out what the version of the component on the users's machine is.
            // Note that if we are comparing the latest version number to a different component
            // then we must get the version number from that other component rather than the
            // component itself.
            else if (File.Exists(_fullPath) || (_otherComponent.Length > 0 && File.Exists(_otherCompFullPath)))
            {
                string comparisonName = _otherComponent.Length > 0 ? _otherComponent : _name;
                string comparisonFullPath = _otherComponent.Length > 0 ? _otherCompFullPath : _fullPath;

                // If we're checking another component's version but the actual component is already
                // on the user's machine then just check the component itself.
                // This is done to resolve the following situation:
                // Look for updates. database is 1.2.0.0
                // A script update xml is available to update the database to 1.3.0.0 so we download it.
                // When EMMA next starts, it checks for updates again and will again download the
                // xml script update since it has not yet been applied to the database (that's 
                // done by the Updater class in EMMA itself). This will continue indefinitely.
                if (_otherComponent.Length > 0 && File.Exists(_fullPath)) 
                {
                    comparisonName = _name;
                    comparisonFullPath = _fullPath;
                }
             
                if (comparisonName.ToLower().EndsWith(".xml"))
                {
                    // Get version from the xml file itself.
                    XmlDocument xmlDoc = new XmlDocument();
                    try
                    {
                        xmlDoc.Load(comparisonFullPath);
                        XmlNode versionNode = xmlDoc.FirstChild.NextSibling.SelectSingleNode("@version");
                        if (versionNode != null)
                        {
                            _currentVersion = new Version(versionNode.Value);
                        }
                    }
                    catch { error = true; }
                }
                else if (comparisonName.ToLower().EndsWith(".mdf"))
                {
                    try
                    {
                        SqlConnection connection = new SqlConnection(
                            @"Data Source=.\SQLEXPRESS;" +
                            @"AttachDbFilename=" + comparisonFullPath + ";" +
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
                        FileVersionInfo info = FileVersionInfo.GetVersionInfo(comparisonFullPath);
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

        public bool ComparingToOtherComponent
        {
            get { return _otherComponent.Length > 0; }
        }
    }

}
