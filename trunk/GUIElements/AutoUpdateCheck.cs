using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;

using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class AutoUpdateCheck : Form
    {
        [DllImport("user32")]
        public static extern UInt32 SendMessage
            (IntPtr hWnd, UInt32 msg, UInt32 wParam, UInt32 lParam);

        internal const int BCM_FIRST = 0x1600; //Normal button
        internal const int BCM_SETSHIELD = (BCM_FIRST + 0x000C); //Elevated button

        private static string _homeDir = "";
        private static List<ComponentData> _components = new List<ComponentData>();
        private static List<ComponentData> _updateComponents = new List<ComponentData>();
        private static string _updateURL = "http://go-dl.eve-files.com/media/corp/Ambo/";
        private static bool _betaUpdates = false;

        public AutoUpdateCheck(string homeDirectory, string server, bool betaUpdates)
        {
            InitializeComponent();
            _homeDir = homeDirectory;
            _betaUpdates = betaUpdates;
            _updateURL = "http://" + server;
            if (server.Equals("go-dl.eve-files.com"))
            {
                _updateURL = _updateURL + "/media/corp/Ambo/";
            }
            if (server.Equals("www.starfreeze.com"))
            {
                _updateURL = _updateURL + "/emma/";
            }

            string xmlFile = DownloadFile("summary.xml");
            //string xmlFile = @"C:\testing.xml";

            if (xmlFile.Length > 0)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFile);
                XmlNodeList nodes = xmlDoc.SelectNodes("/emma/component");
                foreach (XmlNode node in nodes)
                {
                    _components.Add(new ComponentData(node, _homeDir));
                }

                foreach (ComponentData component in _components)
                {
                    if (!component.Exists)
                    {
                        _updateComponents.Add(component);
                    }
                    else if (component.latestVersion.CompareTo(component.currentVersion) > 0)
                    {
                        _updateComponents.Add(component);
                    }
                }
            }

        }

        
        public bool UpdateNeeded
        {
            get { return _updateComponents.Count != 0; }
        }

        private void AutoUpdateCheck_Load(object sender, EventArgs e)
        {
            AddShieldToButton(btnUpdate);
            lblInfo.Text = "An update for EMMA is available";
        }

        private void btnDetails_Click(object sender, EventArgs e)
        {
            AutoUpdateDetails details = new AutoUpdateDetails(_components, this);
            details.ShowDialog();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            string exeFile = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "AutoUpdater.exe";
            //string uacHelper = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + 
            //    "UacHelpers.UserAccountControl.dll";
            if (File.Exists(exeFile))// && File.Exists(uacHelper))
            {
                string tmpDir = Globals.AppDataDir + "Update";
                string exeTmp = tmpDir + Path.DirectorySeparatorChar + "AutoUpdater.exe";
                //string uacHelperTmp = tmpDir + Path.DirectorySeparatorChar + "UacHelpers.UserAccountControl.dll";
                if (Directory.Exists(tmpDir))
                {
                    Directory.Delete(tmpDir, true);
                }
                Directory.CreateDirectory(tmpDir);
                File.Copy(exeFile, exeTmp);
                //File.Copy(uacHelper, uacHelperTmp);

                string parameters = "/p " + System.Diagnostics.Process.GetCurrentProcess().Id +
                    " /s " + Globals.EMMAUpdateServer +
                    " /b " + Properties.Settings.Default.BetaUpdates.ToString() +
                    " /h \"" + AppDomain.CurrentDomain.BaseDirectory + "\"";
                try
                {
                    ProcessStartInfo updateProcessInfo = new ProcessStartInfo();
                    //updateProcessInfo.Verb = "runas";
                    updateProcessInfo.FileName = exeTmp;
                    updateProcessInfo.Arguments = parameters;
                    System.Diagnostics.Process updateProcess = System.Diagnostics.Process.Start(updateProcessInfo);
                    while (!updateProcess.HasExited) { }
                }
                catch (Win32Exception ex)
                {
                    if (ex.Message.Contains("The operation was canceled by the user"))
                    {
                        // If this happens then we just ignore it and continue.
                        // The user has cancelled the auto-update program for some reason
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }
            else
            {
                MessageBox.Show("Auto-updater cannot be found. EMMA cannot complete auto update operation.", 
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public string DownloadFile(string filename)
        {
            if (_betaUpdates) { filename = "Beta_" + filename; }
            string tmpDir = Globals.AppDataDir + Path.DirectorySeparatorChar +
                "Temp" + Path.DirectorySeparatorChar;
            string tmpFile = tmpDir + filename;
            string tmpZip = tmpDir + filename.Remove(filename.LastIndexOf(".")) + ".zip";
            string fileURL = _updateURL + filename.Remove(filename.LastIndexOf(".")) + ".zip";
            WebClient client = new WebClient();
            Stream strResponse = null;
            Stream strLocal = null;
            HttpWebRequest webRequest = null;
            HttpWebResponse webResponse = null;

            if (!Directory.Exists(tmpDir)) { Directory.CreateDirectory(tmpDir); }
            if (File.Exists(tmpZip)) { File.Delete(tmpZip); }

            try
            {
                try
                {
                    webRequest = (HttpWebRequest)WebRequest.Create(fileURL);
                    webRequest.Credentials = CredentialCache.DefaultCredentials;
                    webResponse = (HttpWebResponse)webRequest.GetResponse();

                    strResponse = webResponse.GetResponseStream(); //client.OpenRead(fileURL);
                    strLocal = new FileStream(tmpZip, FileMode.Create, FileAccess.Write, FileShare.None);

                    int bytesSize = 0;
                    byte[] downBuffer = new byte[2048];

                    while ((bytesSize = strResponse.Read(downBuffer, 0, downBuffer.Length)) > 0)
                    {
                        strLocal.Write(downBuffer, 0, bytesSize);
                    }
                }
                finally
                {
                    if (webResponse != null) { webResponse.Close(); }
                    if (strResponse != null) { strResponse.Close(); }
                    if (strLocal != null) { strLocal.Close(); }
                }


                if (File.Exists(tmpZip))
                {
                    try
                    {
                        Compression.DecompressFile(tmpZip, tmpFile);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Problem decompressing file '" + filename + "'.\r\n" + ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        tmpFile = "";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problem downloading file '" + filename + "'.\r\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                tmpFile = "";
            }

            return tmpFile;
        }

        private void AddShieldToButton(Button b)
        {
            b.FlatStyle = FlatStyle.System;
            SendMessage(b.Handle, BCM_SETSHIELD, 0, 0xFFFFFFFF);
        }

    }


    public class ComponentData
    {
        private string _name;
        private string _fullPath;
        private Version _latestVersion;
        private Version _currentVersion;
        private string _description;
        private bool _exists;

        public ComponentData(XmlNode xml, string homeDir)
        {
            bool error = false;
            _name = xml.SelectSingleNode("@name").Value;
            XmlNode subpathnode = xml.SelectSingleNode("@subpath");
            string subpath = (subpathnode == null ? "" : subpathnode.Value + Path.DirectorySeparatorChar);
            _fullPath = homeDir + subpath + _name;
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
