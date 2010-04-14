using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Threading;


namespace AutoUpdater
{
    public partial class Form1 : Form
    {
        private static string _appDataDir = string.Format("{0}{1}EMMA", 
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), Path.DirectorySeparatorChar);

        private static string _homeDir = "";
        private static List<ComponentData> _components = new List<ComponentData>();
        private static List<ComponentData> _updateComponents = new List<ComponentData>();
        private static string _updateURL = "http://go-dl.eve-files.com/media/corp/Ambo/";
        private static bool _betaUpdates = false;

        private static long _currentFileSize = 1;
        private static long _currentFileProgress = 0;
        private static string _currentText = "";
        private static string _currentFile = "";
        private static int _currentFileNo = 0;
        private static bool _done = false;

        delegate void UpdateViewCallback();



        public Form1(string homeDirectory, string server, bool betaUpdates)
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

            // Add version history to list of files that are not up to date.
            // The latest copy of it will then be included in the update.
            if (_updateComponents.Count > 0)
            {
                _updateComponents.Add(new ComponentData("VersionHistory.rtf",
                    _homeDir + Path.DirectorySeparatorChar + "VersionHistory.rtf"));
            }
        }

        private void RefreshDisplay()
        {
            if (this.InvokeRequired)
            {
                UpdateViewCallback callback = new UpdateViewCallback(RefreshDisplay);
                this.Invoke(callback, null);
            }
            else
            {
                if (_done)
                {
                    this.Close();
                }
                else
                {
                    int progress = _currentFileProgress == 0 ? 0 :
                        (int)(((float)_currentFileProgress / (float)_currentFileSize) * 100.0f);
                    if (progress > 100) { progress = 100; }
                    prgProgress.Maximum = 100;
                    prgProgress.Value = progress;
                    lblInfo.Text = _currentText;
                }
            }
        }

        private void DoUpdate()
        {
            // wait for half a second to allow the form to finish loading 
            Thread.Sleep(500);
            try
            {
                bool error = false;

                for (int i = 0; i < _updateComponents.Count; i++)
                {
                    ComponentData component = _updateComponents[i];
                    _currentFileSize = 1;
                    _currentFileProgress = 0;
                    _currentFile = component.Name;
                    _currentFileNo = i + 1;
                    _currentText = _currentFile + " component (" + _currentFileNo + " of " + _updateComponents.Count + ")";
                    RefreshDisplay();

                    if (!UpdateFile(component))
                    {
                        error = true;
                    }
                }

                if (!error)
                {
                    MessageBox.Show("Update completed successfully. Please restart EMMA.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Update was not sucessfull. Please restart EMMA.", "Fail",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                
                /*UserAccountControl.CreateProcessAsStandardUser(
                    _homeDir + Path.DirectorySeparatorChar + "EveMarketMonitorApp.exe",
                    @"/u False");
                Process p2 = Process.Start(_homeDir + Path.DirectorySeparatorChar + "EveMarketMonitorApp.exe",
                    "/u False");*/
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problem running auto-updater.\r\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            _done = true;
            RefreshDisplay();
        }


        private bool UpdateFile(ComponentData data)
        {
            bool retVal = true;
            string tmpFile = DownloadFile(data.Name);
            string tmpFile2 = "";
            if (data.Name.ToLower().EndsWith(".mdf"))
            {
                _currentFile = data.Name.Remove(data.Name.LastIndexOf(".")) + "_log.ldf";
                _currentText = _currentFile + " component (" + _currentFileNo + " of " + _updateComponents.Count + ")";
                _currentFileSize = 1;
                _currentFileProgress = 0;
                RefreshDisplay();
                tmpFile2 = DownloadFile(_currentFile);
            }
            else if (data.Name.ToLower().Equals("evemarketmonitorapp.exe"))
            {
                _currentFile = data.Name + ".config";
                _currentText = _currentFile + " component (" + _currentFileNo + " of " + _updateComponents.Count + ")";
                _currentFileSize = 1;
                _currentFileProgress = 0;
                RefreshDisplay();
                tmpFile2 = DownloadFile(_currentFile);
            }

            if (tmpFile.Length > 0)
            {
                try
                {
                    _currentText = _currentFile + " component (" + _currentFileNo + " of " + _updateComponents.Count + 
                        ")\r\nCopying...";
                    RefreshDisplay();
                    File.Copy(tmpFile, data.FullPath, true);
                    if (tmpFile2.Length > 0)
                    {
                        if (data.Name.ToLower().EndsWith(".mdf"))
                        {
                            File.Copy(tmpFile2, data.FullPath.Remove(data.FullPath.LastIndexOf(".")) + "_log.ldf", true);
                        }
                        else if (data.Name.ToLower().Equals("evemarketmonitorapp.exe"))
                        {
                            File.Copy(tmpFile2, data.FullPath + ".config", true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Problem copying file '" + data.Name + "'.\r\n" + ex.Message, "Error",
                       MessageBoxButtons.OK, MessageBoxIcon.Error);
                    retVal = false;
                }
            }

            return retVal;
        }

        public string DownloadFile(string filename)
        {
            if (_betaUpdates) { filename = "Beta_" + filename; }
            string tmpDir = _appDataDir + Path.DirectorySeparatorChar +
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
                _currentText = _currentFile + " component (" + _currentFileNo + " of " + _updateComponents.Count + ")";
                _currentText = _currentText + "\r\nDownloading...";
                RefreshDisplay();

                try
                {
                    webRequest = (HttpWebRequest)WebRequest.Create(fileURL);
                    webRequest.Credentials = CredentialCache.DefaultCredentials;
                    webResponse = (HttpWebResponse)webRequest.GetResponse();
                    _currentFileSize = webResponse.ContentLength;

                    strResponse = webResponse.GetResponseStream(); //client.OpenRead(fileURL);
                    strLocal = new FileStream(tmpZip, FileMode.Create, FileAccess.Write, FileShare.None);

                    int bytesSize = 0;
                    byte[] downBuffer = new byte[2048];

                    while ((bytesSize = strResponse.Read(downBuffer, 0, downBuffer.Length)) > 0)
                    {
                        strLocal.Write(downBuffer, 0, bytesSize);
                        _currentFileProgress = strLocal.Length;
                        RefreshDisplay();
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
                        _currentText = _currentFile + " component (" + _currentFileNo + " of " + _updateComponents.Count + ")";
                        _currentText = _currentText + "\r\nExtracting...";
                        RefreshDisplay();
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

        /*public bool UserHasAccess()
        {
            bool retVal = false;

            string tmpFile = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar +
                "Temp" + Path.DirectorySeparatorChar + "Tutorial_default.xml";
            string existingFile = _homeDir + Path.DirectorySeparatorChar + "Data" + 
                Path.DirectorySeparatorChar + "Tutorial_default.xml";

            try
            {
                File.Copy(existingFile, tmpFile, true);
                File.Copy(tmpFile, existingFile, true);
                retVal = true;
            }
            catch (UnauthorizedAccessException) { }

            return retVal;
        }

        public bool UpdateNeeded
        {
            get { return _updateComponents.Count != 0; }
        }*/

        private void btnClose_Click(object sender, EventArgs e)
        {
            /*UserAccountControl.CreateProcessAsStandardUser(
                _homeDir + Path.DirectorySeparatorChar + "EveMarketMonitorApp.exe",
                @"/u False");*/
            /*Process p2 = Process.Start(_homeDir + Path.DirectorySeparatorChar + "EveMarketMonitorApp.exe",
                "/u False");*/
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            prgProgress.Visible = true;
            btnClose.Visible = false;
            Thread t1 = new Thread(new ThreadStart(DoUpdate));
            t1.Start();
        }


    }
}