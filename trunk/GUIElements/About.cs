using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Data.Sql;
using System.Data.SqlClient;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class About : Form
    {
        List<ComponentData> _componentList;

        public About()
        {
            InitializeComponent();
        }

        private void About_Load(object sender, EventArgs e)
        {
            _componentList = new List<ComponentData>();
            _componentList.Add(new ComponentData("EveMarketMonitorApp.exe", ""));
            _componentList.Add(new ComponentData("AutoUpdater.exe", ""));
            _componentList.Add(new ComponentData("Enforcer.dll", ""));
            _componentList.Add(new ComponentData("EMMA Database.mdf", "Data"));
            _componentList.Add(new ComponentData("EveData.mdf", "Data"));
            _componentList.Add(new ComponentData("Tutorial_default.xml", "Data"));
            _componentList.Add(new ComponentData("Tutorial_en-GB.xml", "Data"));

            componentVersionsGrid.AutoGenerateColumns = false;
            componentVersionsGrid.DataSource = _componentList;
            NameColumn.DataPropertyName = "Name";
            VersionColumn.DataPropertyName = "currentVersion";
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lnkSourceForge_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            lnkSourceForge.Links[lnkSourceForge.Links.IndexOf(e.Link)].Visited = true;

            string target = @"http://code.google.com/p/evemarketmonitorapp/";
            System.Diagnostics.Process.Start(target);
        }

        private void lnkEMMAThread_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            lnkEMMAThread.Links[lnkEMMAThread.Links.IndexOf(e.Link)].Visited = true;

            string target = @"http://www.eveonline.com/ingameboard.asp?a=topic&threadID=1180576";
            System.Diagnostics.Process.Start(target);
        }

        private class ComponentData
        {
            private string _name;
            private string _fullPath;
            private Version _latestVersion;
            private Version _currentVersion;
            private string _description;
            private bool _exists;

            public ComponentData(string name, string subpath)
            {
                bool error = false;
                _name = name;
                _fullPath = System.Environment.CurrentDirectory + Path.DirectorySeparatorChar + subpath + 
                    Path.DirectorySeparatorChar + _name;
                
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
                                "Integrated Security=True;User Instance=True");
                            SqlCommand command = null;
                            connection.Open();

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
                    if (error) { _currentVersion = new Version(0, 0); }
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
}