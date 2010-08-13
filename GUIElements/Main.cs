using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Threading;
using System.IO.Compression;
using System.Management;
using Microsoft.Win32;
using System.Net.NetworkInformation;

using EveMarketMonitorApp.AbstractionClasses;
using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.Reporting;

using Enforcer;
using EveMarketMonitorApp.GUIElements.Interfaces;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class Main : Form, IProvideStatus
    {
        private static ControlPanel _controlPanel;
        private static UpdateStatus _updateStatus;
        private static SystemStatus _status;
        private static Dictionary<APIDataType, List<int>> _updatesRunning;
        private static List<int> _assetUpdatesAwaitingAcknowledgement = new List<int>();
        private static Dictionary<int, short> _corpOrderUpdates = new Dictionary<int, short>();
        //private static bool _unackAssetsWaiting = false;
        private static SplashScreen splash;
        private static ViewUnacknowledgedOrders _unackOrders = null;
        private static ViewUnacknowledgedAssets _unackAssets = null;
        private static bool _tutorialActive = false;
        private static bool _forceClose = false;
        private static Thread _tutorialThread;
        private static LicenseType _license = LicenseType.Invalid;

        private bool _wastedTime = false;

        delegate void UpdateViewCallback();
        delegate void RefreshUnackOrdersCallback(bool forceDisplay);
        delegate void AcknowledgeAssetChangesCallback();

        public event StatusChangeHandler StatusChange;

        public Main(bool checkForUpdates)
        {
            Diagnostics.StartTimer("TotalStartupTimer");
            Diagnostics.StartTimer("DisplaySplash");
            splash = new SplashScreen(this);
            Thread t0 = new Thread(ShowSplash);
            t0.Start();
            Diagnostics.StopTimer("DisplaySplash");

            Diagnostics.StartTimer("InitGUI");
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(Main_FormClosing);
            // Set main window start state/position/size
            this.StartPosition = FormStartPosition.Manual;
            this.WindowState = EveMarketMonitorApp.Properties.Settings.Default.WindowState;
            if (EveMarketMonitorApp.Properties.Settings.Default.WindowPos.X > 0 &&
                EveMarketMonitorApp.Properties.Settings.Default.WindowPos.X < Screen.PrimaryScreen.WorkingArea.Width &&
                EveMarketMonitorApp.Properties.Settings.Default.WindowPos.Y > 0 &&
                EveMarketMonitorApp.Properties.Settings.Default.WindowPos.Y < Screen.PrimaryScreen.WorkingArea.Height)
            {
                this.Location = EveMarketMonitorApp.Properties.Settings.Default.WindowPos;
            }
            else
            {
                this.Location = new Point(0, 0);
            }
            this.Size = EveMarketMonitorApp.Properties.Settings.Default.WindowSize;
            Diagnostics.StopTimer("InitGUI");

            try
            {
                //splash.ShowMessage("Test Message", "Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DateTime start = DateTime.UtcNow;

                // DO ANY SETUP HERE.
                // The splash screen will be showing while these methods are executed.
                Diagnostics.StartTimer("Environment");
                UpdateStatus(0, 0, "Setting up environment", "", false);
                SetupEnvironment();
                Diagnostics.StopTimer("Environment");
                UpdateStatus(0, 0, "Checking Prerequesits", "", false);
                if (Prerequs())
                {
                    Diagnostics.StartTimer("PingChecks");
                    UpdateStatus(0, 0, "Checking remote servers", "", false);
                    PingServers();
                    Diagnostics.StopTimer("PingChecks");
                    Diagnostics.StartTimer("Updates");
                    // Update settings and user database if needed.
                    UpdateStatus(0, 0, "Initalising database", "", false);
                    //try
                    //{
                        Updater.Update();
                    //}
                    //catch (EMMAException)
                    //{
                        //UpdateStatus(0, 0, "Done", "", true);
                        //MessageBox.Show(splash, "Critical error updating EMMA database. For details, see " +
                        //    "\"Logging/ExceptionLog.txt\"", "Ciritcal error",
                        //    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //}
                    Updater.InitDBs();
                    CheckForDocumentation();
                    checkForUpdates = checkForUpdates && EveMarketMonitorApp.Properties.Settings.Default.AutoUpdate; 
                    if (checkForUpdates)
                    {
                        DateTime lastCheck = Properties.Settings.Default.LastEMMAUpdateCheck;
                        if (lastCheck.AddHours(5).CompareTo(DateTime.UtcNow) < 0)
                        {
                            UpdateStatus(0, 0, "Checking for updates", "", false);
                            // Check for updates to EMMA components
                            AutoUpdate();
                            Properties.Settings.Default.LastEMMAUpdateCheck = DateTime.UtcNow;
                            Properties.Settings.Default.Save();
                        }
                    }
                    Diagnostics.StopTimer("Updates");
                    Diagnostics.StartTimer("MapInit");
                    // Pre-load map data.
                    Map.InitaliseData();
                    UpdateStatus(0, 0, "Getting latest outpost data", "", false);
                    EveAPI.UpdateOutpostData();
                    Diagnostics.StopTimer("MapInit");
                    Diagnostics.StartTimer("AutoLogin");
                    // Log user in if they have auto login turned on
                    AutoLogin();
                    Diagnostics.StopTimer("AutoLogin");
                    //UpdateStatus(0, 0, "Starting program", "", false);

                    // make sure we show the splash screen for a minimum of one second.
                    // ... it looks wierd otherwise.
                    while (start.AddSeconds(1).CompareTo(DateTime.UtcNow) > 0) { }
                }
            }
            catch (Exception ex)
            {
                EMMAException emmaEx = ex as EMMAException;
                if (emmaEx == null)
                {
                    emmaEx = new EMMAException(ExceptionSeverity.Critical, "Error during startup", ex);
                }
                splash.ShowMessage("Problem during EMMA startup.\r\nCheck " + EMMAException.logFile + 
                    " for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus(0, 0, "Done", "", true);
                this.Close();
            }
            finally
            {
                Diagnostics.StartTimer("LoadMainForm");
                UpdateStatus(0, 0, "Done", "", true);
                splash = null;
            }
        }

        private void ShowSplash()
        {
            splash.ShowDialog();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            try
            {
                Diagnostics.StopTimer("LoadMainForm");
                this.Size = EveMarketMonitorApp.Properties.Settings.Default.WindowSize;

                Diagnostics.StartTimer("Init");
                Initialisation();
                _controlPanel.InitSettings();
                _controlPanel.Show();
                Diagnostics.StopTimer("Init");

                this.Refresh();

                if (EveMarketMonitorApp.Properties.Settings.Default.FirstRun)
                {
                    DisplayTutorial();

                    EveMarketMonitorApp.Properties.Settings.Default.FirstRun = false;
                    EveMarketMonitorApp.Properties.Settings.Default.Save();
                }

                if (_status == SystemStatus.NoUserLoggedIn)
                {
                    // Set these to make sure nothing thinks we're logged in when we're not.
                    UserAccount.Name = "";
                    UserAccount.CurrentGroup = null;
                    Diagnostics.StopTimer("TotalStartupTimer");
                    Diagnostics.DisplayDiag("Total Startup Time: " + Diagnostics.GetRunningTime("TotalStartupTimer") +
                        "\r\n  Display Splashscreen: " + Diagnostics.GetRunningTime("DisplaySplash") +
                        "\r\n  Environment Setup: " + Diagnostics.GetRunningTime("Environment") +
                        "\r\n  Ping Checks: " + Diagnostics.GetRunningTime("PingChecks") +
                        "\r\n  EMMA Update Check: " + Diagnostics.GetRunningTime("Updates") +
                        "\r\n  Map Init: " + Diagnostics.GetRunningTime("MapInit") +
                        "\r\n  Load Main Form: " + Diagnostics.GetRunningTime("LoadMainForm") +
                        "\r\n  Global Init: " + Diagnostics.GetRunningTime("Init"));
                    if (Login())
                    {
                        if (UserAccount.CurrentGroup == null)
                        {
                            SelectReportGroup();
                        }
                    }
                }
                else
                {
                    Diagnostics.StartTimer("RefreshDisplay");
                    RefreshDisplay();
                    Diagnostics.StopTimer("RefreshDisplay");

                    Diagnostics.StopTimer("TotalStartupTimer");
                    Diagnostics.DisplayDiag("Total Startup Time: " + Diagnostics.GetRunningTime("TotalStartupTimer") +
                        "\r\n  Display Splashscreen: " + Diagnostics.GetRunningTime("DisplaySplash") +
                        "\r\n  Init GUI: " + Diagnostics.GetRunningTime("InitGUI") +
                        "\r\n  Environment Setup: " + Diagnostics.GetRunningTime("Environment") +
                        "\r\n  Ping Checks: " + Diagnostics.GetRunningTime("PingChecks") +
                        "\r\n  EMMA Update Check: " + Diagnostics.GetRunningTime("Updates") +
                        "\r\n  Map Init: " + Diagnostics.GetRunningTime("MapInit") +
                        "\r\n  Auto Login: " + Diagnostics.GetRunningTime("AutoLogin") +
                        "\r\n    Open Account: " + Diagnostics.GetRunningTime("OpenAccount") +
                        "\r\n      Load User Account: " + Diagnostics.GetRunningTime("OpenAccount.LoadAccount") +
                        "\r\n      Load Report Groups: " + Diagnostics.GetRunningTime("OpenAccount.GetGroups") +
                        "\r\n      Load Eve Accounts: " + Diagnostics.GetRunningTime("RptGrp.LoadEveAccounts") +
                        "\r\n      Init User Settings: " + Diagnostics.GetRunningTime("OpenAccount.InitSettings") +
                        "\r\n  Load Main Form: " + Diagnostics.GetRunningTime("LoadMainForm") +
                        "\r\n  Global Init: " + Diagnostics.GetRunningTime("Init") +
                        "\r\n  Refresh Display: " + Diagnostics.GetRunningTime("RefreshDisplay"));
                }
            }
            catch (Exception ex)
            {
                EMMAException emmaEx = ex as EMMAException;
                if (emmaEx == null)
                {
                    emmaEx = new EMMAException(ExceptionSeverity.Error, "Problem loading main form.", ex);
                }
                try
                {
                    MessageBox.Show(null, "Problem loading EMMA:\r\n" + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception)
                {
                }
                finally
                {
                    _forceClose = true;
                    this.Close();
                }
            }
        }

        #region Initialisation Stuff
        private void Initialisation()
        {
            SettingsMenu settingsMenu = new SettingsMenu();
            settingsMenu.MdiParent = this;

            // Setup the control panel.
            _controlPanel = new ControlPanel(settingsMenu);
            _controlPanel.Location = new Point(this.Width - _controlPanel.Width - 20, 10);
            _controlPanel.MdiParent = this;
            // note these two settings don't actually work for MDI child forms...
            _controlPanel.TopMost = true;
            _controlPanel.Opacity = .6;
            _controlPanel.OptionSelected += new CPOptionSelected(ControlPanel_OptionSelected);
            _controlPanel.Move += new EventHandler(ControlPanel_Move);

            // Setup default UI state
            RefreshDisplay();

            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
            }

            ValidateInstall(false);
        }

        private void SetupEnvironment()
        {

            if (!Directory.Exists(Globals.AppDataDir))
            {
                Directory.CreateDirectory(Globals.AppDataDir);
            }

            // Make sure that required sub directories exist.
            if (!Directory.Exists(string.Format("{0}Logging", Globals.AppDataDir)))
            {
                Directory.CreateDirectory(string.Format("{0}Logging", Globals.AppDataDir));
            }
            if (!Directory.Exists(string.Format("{0}Logging{1}API Call History",
                    Globals.AppDataDir, Path.DirectorySeparatorChar)))
            {
                Directory.CreateDirectory(string.Format("{0}Logging{1}API Call History",
                    Globals.AppDataDir, Path.DirectorySeparatorChar));
            }
            if (!Directory.Exists(string.Format("{0}Logging{1}Eve Central History",
                    Globals.AppDataDir, Path.DirectorySeparatorChar)))
            {
                Directory.CreateDirectory(string.Format("{0}Logging{1}Eve Central History",
                    Globals.AppDataDir, Path.DirectorySeparatorChar));
            }

            if (Directory.Exists(string.Format("{0}Temp", Globals.AppDataDir)))
            {
                Directory.Delete(string.Format("{0}Temp", Globals.AppDataDir), true);
            }
            Directory.CreateDirectory(string.Format("{0}Temp", Globals.AppDataDir));

            if (!Directory.Exists(string.Format("{0}Logging", Globals.AppDataDir)) ||
                !Directory.Exists(string.Format("{0}Logging{1}API Call History",
                    Globals.AppDataDir, Path.DirectorySeparatorChar)) ||
                !Directory.Exists(string.Format("{0}Logging{1}Eve Central History",
                    Globals.AppDataDir, Path.DirectorySeparatorChar)) ||
                !Directory.Exists(string.Format("{0}Temp", Globals.AppDataDir)))
            {
                //UpdateStatus(0, 0, "Done", "", true);
                //MessageBox.Show((splash != null ? (IWin32Window)splash : (IWin32Window)this), 
                //    "Unable to create required sub-directories.", "Error",
                //   MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw new EMMAException(ExceptionSeverity.Critical, "Unable to create required " +
                    "sub-directories.");
            }

            // This directory is used by the auto updater to temporarilly store a copy of itself.
            // If it's present on the system then just remove it to keep things tidy.
            string tmpDir = Globals.AppDataDir + Path.DirectorySeparatorChar + "Update";
            if (Directory.Exists(tmpDir))
            {
                Directory.Delete(tmpDir, true);
            }

            if (!Directory.Exists(string.Format("{0}Data", Globals.AppDataDir)))
            {
                Directory.CreateDirectory(string.Format("{0}Data", Globals.AppDataDir));
            }

            if (Properties.Settings.Default.EMMA_DatabaseConnectionString.Contains("EMMA Database.mdf"))
            {
                string emmaDataFile = string.Format("{0}Data{1}EMMA Database.mdf",
                    Globals.AppDataDir, Path.DirectorySeparatorChar);
                string eveDataFile = string.Format("{0}Data{1}EveData.mdf",
                    Globals.AppDataDir, Path.DirectorySeparatorChar);
                if (!File.Exists(emmaDataFile))
                {
                    File.Copy(string.Format("{0}Data{1}EMMA Database.mdf",
                        AppDomain.CurrentDomain.BaseDirectory, Path.DirectorySeparatorChar), emmaDataFile);
                    File.Copy(string.Format("{0}Data{1}EMMA Database_log.ldf",
                        AppDomain.CurrentDomain.BaseDirectory, Path.DirectorySeparatorChar), 
                        string.Format("{0}Data{1}EMMA Database_log.ldf",
                        Globals.AppDataDir, Path.DirectorySeparatorChar));
                }
                if (!File.Exists(eveDataFile))
                {
                    File.Copy(string.Format("{0}Data{1}EveData.mdf",
                        AppDomain.CurrentDomain.BaseDirectory, Path.DirectorySeparatorChar), eveDataFile);
                    File.Copy(string.Format("{0}Data{1}EveData_log.ldf",
                        AppDomain.CurrentDomain.BaseDirectory, Path.DirectorySeparatorChar), 
                        string.Format("{0}Data{1}EveData_log.ldf",
                        Globals.AppDataDir, Path.DirectorySeparatorChar));
                }
            }

            AppDomain.CurrentDomain.SetData("DataDirectory", Globals.AppDataDir);


            // Initalise the updates running structure. This keeps track of which database tables
            // are being updated and is used to enable/disable main menu items. 
            InitaliseUpdatesRunning();

            // Database connections are limited so make sure that we can never be running more than 8
            // threads trying to access the database at once .
            // (tasks are queued in APICharacter.UpdateDataFromAPI)
            ThreadPool.SetMaxThreads(8, 8);

            // Set initial user status
            _status = SystemStatus.NoUserLoggedIn;
        }

        private void InitaliseUpdatesRunning()
        {
            _updatesRunning = new Dictionary<APIDataType, List<int>>();
            _updatesRunning.Add(APIDataType.Assets, new List<int>());
            _updatesRunning.Add(APIDataType.Journal, new List<int>());
            _updatesRunning.Add(APIDataType.Orders, new List<int>());
            _updatesRunning.Add(APIDataType.Transactions, new List<int>());
            _updatesRunning.Add(APIDataType.IndustryJobs, new List<int>());
        }

        private void AutoUpdate()
        {
            AutoUpdateCheck checker = new AutoUpdateCheck(AppDomain.CurrentDomain.BaseDirectory, 
                Globals.EMMAUpdateServer, Properties.Settings.Default.BetaUpdates);
            if (checker.UpdateNeeded)
            {
                checker.ShowDialog();
            }
            else
            {
                checker = null;
            }

            /*if (!Globals.EMMAUpdateServer.Equals(""))
            {
                string exeFile = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "AutoUpdater.exe";
                if (File.Exists(exeFile))
                {
                    string tmpDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                        Path.DirectorySeparatorChar + "EMMA";
                    string exeTmp = tmpDir + Path.DirectorySeparatorChar + "AutoUpdater.exe";
                    if (Directory.Exists(tmpDir))
                    {
                        Directory.Delete(tmpDir, true);
                    }
                    Directory.CreateDirectory(tmpDir);
                    File.Copy(exeFile, exeTmp);

                    string parameters = "/p " + System.Diagnostics.Process.GetCurrentProcess().Id +
                        " /s " + Globals.EMMAUpdateServer +
                        " /b " + Properties.Settings.Default.BetaUpdates.ToString() +
                        " /h \"" + AppDomain.CurrentDomain.BaseDirectory + "\"";
                    try
                    {
                        System.Diagnostics.Process updateProcess = System.Diagnostics.Process.Start(exeTmp, parameters);
                        while (!updateProcess.HasExited) { }
                    }
                    catch (Win32Exception ex)
                    {
                        if (ex.Message.Contains("The operation was canceled by the user"))
                        {
                            // If this happens then we just ignore it and continue.
                            // The user has cancelled the auto-update program for some reason
                            // (e.g. does not want to give the program admin rights on vista)
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Auto-updater cannot be found. EMMA will not be updated when new features " +
                        "become available.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }*/
        }

        private void CheckForDocumentation()
        {
            if (EveMarketMonitorApp.Properties.Settings.Default.DoDocCheck)
            {
                if (NewDocumentationForm.DocUpdateAvailable())
                {
                    NewDocumentationForm form = new NewDocumentationForm();
                    form.TopMost = true;
                    form.ShowDialog();
                }
            }
        }
        #endregion

        public void RebuildUpdatePanel()
        {
            if (_updateStatus != null)
            {
                _updateStatus.PopulatePanels();
            }
        }

        void ControlPanel_OptionSelected(object myObject, CPOptionSelectedArgs args)
        {
            switch (args.Option)
            {
                case ControlPanelOption.ManageGroup:
                    if (Globals.EveAPIDown)
                    {
                        MessageBox.Show("Cannot manage group while Eve API is down.", "Warning",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        if (!CheckForUpdatesInProg())
                        {
                            ManageReportGroup();
                            RebuildUpdatePanel();
                        }
                    }
                    break;
                case ControlPanelOption.SelectGroup:
                    if (!CheckForUpdatesInProg())
                    {
                        SelectReportGroup();
                    }
                    break;
                case ControlPanelOption.ChangeSettings:
                    // This is handled by the control panel form itself.
                    break;
                case ControlPanelOption.Tutorial:
                    DisplayTutorial();
                    break;
                case ControlPanelOption.ExportData:
                    if (!CheckForUpdatesInProg())
                    {
                        Export();
                    }
                    break;
                case ControlPanelOption.ImportData:
                    if (UserAccount.CurrentGroup.HasCharOrCorp())
                    {
                        if (!CheckForUpdatesInProg())
                        {
                            Import();
                        }
                    }
                    else
                    {
                        MessageBox.Show("You cannot import data until you have a character or " +
                            "corporation in this report group.", "Warning", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    break;
                case ControlPanelOption.VerifyDB:
                    RepairDatabase();
                    break;
                case ControlPanelOption.LicenseDetails:
                    ValidateInstall(true);
                    break;
                case ControlPanelOption.Logout:
                    if (!CheckForUpdatesInProg())
                    {
                        Logout();
                    }
                    break;
                case ControlPanelOption.Login:
                    if (Login())
                    {
                        if (_status == SystemStatus.NoReportGroupSelected)
                        {
                            SelectReportGroup();
                        }
                    }
                    break;
                case ControlPanelOption.About:
                    About aboutScreen = new About();
                    aboutScreen.MdiParent = this;
                    aboutScreen.Show();
                    break;
                case ControlPanelOption.Quit:
                    if (!CheckForUpdatesInProg())
                    {
                        this.Close();
                    }
                    break;
                default:
                    break;
            }
        }

        public bool CheckForUpdatesInProg()
        {
            bool retVal = false;
            bool updatesInProg = false;

            List<int> ids = _updatesRunning[APIDataType.Assets];
            if (ids.Count > 0) { updatesInProg = true; }
            ids = _updatesRunning[APIDataType.Journal];
            if (ids.Count > 0) { updatesInProg = true; }
            ids = _updatesRunning[APIDataType.Orders];
            if (ids.Count > 0) { updatesInProg = true; }
            ids = _updatesRunning[APIDataType.Transactions];
            if (ids.Count > 0) { updatesInProg = true; }
            ids = _updatesRunning[APIDataType.IndustryJobs];
            if (ids.Count > 0) { updatesInProg = true; }

            if (updatesInProg)
            {
                MessageBox.Show("API updates are currently in progress. Please wait until they are completed", 
                    "Cannot Continue", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                retVal = true;
            }

            return retVal;
        }


        void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool closeMain = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                EveMarketMonitorApp.Properties.Settings.Default.WindowState = this.WindowState;
                if (WindowState != FormWindowState.Minimized)
                {
                    EveMarketMonitorApp.Properties.Settings.Default.WindowPos = this.Location;
                    EveMarketMonitorApp.Properties.Settings.Default.WindowSize = this.Size;
                }
                EveMarketMonitorApp.Properties.Settings.Default.Save();

                Logout();
                if (_controlPanel == null && _updateStatus == null)
                {
                    closeMain = true;
                }
                if (_controlPanel != null)
                {
                    // Have to put this slightly wierd bit of code here to get the control panel to 
                    // close properly and allow the main window to close.
                    _controlPanel.AllowClose = true;
                    _controlPanel.Close();
                    _controlPanel = null;
                    closeMain = true;
                }
                if (_updateStatus != null)
                {
                    _updateStatus.AllowClose = true;
                    _updateStatus.Close();
                    _updateStatus = null;
                    closeMain = true;
                }
                if (_tutorialActive)
                {
                    _tutorialThread.Abort();
                    _tutorialThread.Join();
                }

                string tmpDir = Globals.AppDataDir + "Temp";
                if (Directory.Exists(tmpDir)) { Directory.Delete(tmpDir, true); }
                string updateDir = Globals.AppDataDir + "Update";
                if (Directory.Exists(updateDir)) { Directory.Delete(updateDir, true); }
            }
            finally
            {
                e.Cancel = !closeMain;
            }
        }

        #region UI Methods
        private void RefreshDisplay()
        {
            if (this.InvokeRequired)
            {
                UpdateViewCallback callback = new UpdateViewCallback(RefreshDisplay);
                this.Invoke(callback, null);
            }
            else
            {
                switch (_status)
                {
                    case SystemStatus.NoUserLoggedIn:
                        Text = "EMMA - Please login";
                        btnAssets.Enabled = false;
                        btnInvestments.Enabled = false;
                        btnJournal.Enabled = false;
                        btnOrders.Enabled = false;
                        btnReports.Enabled = false;
                        btnTransactions.Enabled = false;
                        btnItemDetail.Enabled = false;
                        btnContracts.Enabled = false;
                        btnRoutePlanner.Enabled = false;
                        btnReprocessor.Enabled = false;
                        if (_updateStatus != null)
                        {
                            _updateStatus.AllowClose = true;
                            _updateStatus.Close();
                            _updateStatus = null;
                        }
                        if (Globals.calculator != null)
                        {
                            Globals.calculator.Close();
                            Globals.calculator = null;
                        }
                        break;
                    case SystemStatus.NoReportGroupSelected:
                        Text = "EMMA - Please select a report group";
                        btnAssets.Enabled = false;
                        btnInvestments.Enabled = false;
                        btnJournal.Enabled = false;
                        btnOrders.Enabled = false;
                        btnReports.Enabled = false;
                        btnTransactions.Enabled = false;
                        btnItemDetail.Enabled = false;
                        btnContracts.Enabled = false;
                        btnRoutePlanner.Enabled = false;
                        btnReprocessor.Enabled = false;
                        if (_updateStatus != null)
                        {
                            _updateStatus.AllowClose = true;
                            _updateStatus.Close();
                            _updateStatus = null;
                        }
                        if (Globals.calculator != null)
                        {
                            Globals.calculator.Close();
                            Globals.calculator = null;
                        }
                        // Set control panel location.
                        Point ctrlPanelLoc = UserAccount.Settings.ControlPanelPos;
                        _controlPanel.Location = ctrlPanelLoc.X == 0 && ctrlPanelLoc.Y == 0 ?
                            _controlPanel.Location : ctrlPanelLoc;

                        break;
                    case SystemStatus.Complete:
                        WasteTime();
                        bool allEnabled = true;
                        Text = "EMMA - " + UserAccount.CurrentGroup.Name;

                        List<int> ids = _updatesRunning[APIDataType.Assets];
                        if (ids.Count == 0) { btnAssets.Enabled = true; }
                        else { btnAssets.Enabled = false; allEnabled = false; }
                        ids = _updatesRunning[APIDataType.Journal];
                        if (ids.Count == 0) { btnJournal.Enabled = true; }
                        else { btnJournal.Enabled = false; allEnabled = false; }
                        ids = _updatesRunning[APIDataType.Orders];
                        if (ids.Count == 0) { btnOrders.Enabled = true; }
                        else { btnOrders.Enabled = false; allEnabled = false; }
                        ids = _updatesRunning[APIDataType.Transactions];
                        if (ids.Count == 0) { btnTransactions.Enabled = true; }
                        else { btnTransactions.Enabled = false; allEnabled = false; }
                        if (allEnabled) { btnReports.Enabled = true; }
                        else { btnReports.Enabled = false; }
                        btnItemDetail.Enabled = btnAssets.Enabled && btnOrders.Enabled && btnTransactions.Enabled;
                        btnContracts.Enabled = btnAssets.Enabled && btnTransactions.Enabled;
                        btnReprocessor.Enabled = btnAssets.Enabled && btnTransactions.Enabled;
                        btnInvestments.Enabled = true;
                        btnRoutePlanner.Enabled = true;

                        if (_updateStatus == null)
                        {
                            _updateStatus = new UpdateStatus();
                            Point userUpdLoc = UserAccount.Settings.UpdatePanelPos;
                            Size userUpdSize = UserAccount.Settings.UpdatePanelSize;
                            if (userUpdLoc.X < 0 || userUpdLoc.X + userUpdSize.Width > this.Width) { userUpdLoc.X = 10; }
                            if (userUpdLoc.Y < 0 || userUpdLoc.Y + userUpdSize.Height > this.Height) { userUpdLoc.Y = 10; }
                            _updateStatus.Location = userUpdLoc.X == 0 && userUpdLoc.Y == 0 ?
                                new Point(10, 10) : userUpdLoc;
                            _updateStatus.Size = userUpdSize.Width == 0 && userUpdSize.Height == 0 ?
                                _updateStatus.Size : userUpdSize;
                            _updateStatus.MdiParent = this;
                            _updateStatus.ResizeEnd += new EventHandler(UpdateStatus_ResizeEnd);
                            _updateStatus.Move += new EventHandler(UpdateStatus_Move);
                            _updateStatus.UpdateEvent += new APIUpdateEvent(UpdateStatus_UpdateEvent);
                            _updateStatus.Show();
                        }
                        if (Globals.calculator == null)
                        {
                            GridCalculator calc = new GridCalculator();
                            calc.MdiParent = this;
                            Globals.calculator = calc;
                        }

                        RefreshUnackOrders(false);
                        break;
                    default:
                        break;
                }

                if (_controlPanel != null) { _controlPanel.Refresh(_status); }
            }
        }

        public void TryShowUnackOrders()
        {
            RefreshUnackOrders(true);
            if (_unackOrders == null || !_unackOrders.Visible)
            {
                MessageBox.Show("There are currently no unacknowledged orders to be dispalyed.",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void RefreshUnackOrders(bool forceDisplay)
        {
            if (this.InvokeRequired)
            {
                RefreshUnackOrdersCallback callback = new RefreshUnackOrdersCallback(RefreshUnackOrders);
                object[] parameters = new object[1];
                parameters[0] = forceDisplay;
                this.Invoke(callback, parameters);
            }
            else
            {
                bool updateForm = false;
                bool showForm = false;

                // If order notification window is not displayed and there is at least one unacknowledged
                // order then display it.
                // If the order notification window is displayed and the number of unacnowledged orders
                // has changed then refresh it.
                // _unackOrders.Visible will be false if EMMA is minimised but in this case we just
                // want to refresh the existing window, not display a new one.
                if (_unackOrders == null || (!_unackOrders.Visible && this.Visible))
                {
                    OrdersList unack = Orders.LoadOrders(UserAccount.CurrentGroup.GetAssetAccessParams(
                        APIDataType.Orders), new List<int>(), new List<int>(),
                        (int)OrderState.ExpiredOrFilledAndUnacknowledged, "Any");
                    if (_unackOrders == null && unack.Count > 0)
                    {
                        _unackOrders = new ViewUnacknowledgedOrders();
                        _unackOrders.MdiParent = this;
                        showForm = true;
                    }
                    else if (_unackOrders != null && !_unackOrders.Visible &&
                        ((_unackOrders.LastNumberOfOrders() != unack.Count) || forceDisplay))
                    {
                        // For some reason, calling 'Show' on the form after it's been closed will
                        // no longer work.
                        // Instead, we just have to create a new instance.
                        _unackOrders = new ViewUnacknowledgedOrders();
                        _unackOrders.MdiParent = this;
                        showForm = true;
                        //updateForm = true;
                    }
                }
                else { updateForm = true; }

                if (showForm)
                {
                    _unackOrders.Show();
                }
                if (updateForm)
                {
                    _unackOrders.DisplayOrders();
                }
            }
        }


        private void AcknowledgeAssetChanges()
        {
            if (this.InvokeRequired)
            {
                AcknowledgeAssetChangesCallback callback = new 
                    AcknowledgeAssetChangesCallback(AcknowledgeAssetChanges);
                this.Invoke(callback);
            }
            else
            {
                if (_unackAssets != null && _unackAssets.Visible)
                {
                    _unackAssets.PopulateLists();
                }
                else
                {
                    _unackAssets = new ViewUnacknowledgedAssets();
                    _unackAssets.MdiParent = this;
                    _unackAssets.AssetChangesAcknowledged +=
                        new AssetChangesAcknowledgedHandler(unack_AssetChangesAcknowledged);
                    _unackAssets.Show();
                }
            }
        }

        void unack_AssetChangesAcknowledged(object myObject, EventArgs args)
        {
            foreach (int id in _assetUpdatesAwaitingAcknowledgement)
            {
                bool isCorp = false;
                APICharacter character = UserAccount.CurrentGroup.GetCharacter(id, ref isCorp);
                List<int> idsUpdating = _updatesRunning[APIDataType.Assets];
                if (idsUpdating.Contains(id))
                {
                    idsUpdating.Remove(id);
                }
                character.SetLastAPIUpdateError(isCorp ? CharOrCorp.Corp : CharOrCorp.Char, APIDataType.Assets, ""); 
            }
            _assetUpdatesAwaitingAcknowledgement.Clear();
            RefreshDisplay();
        }


        void UpdateStatus_UpdateEvent(object myObject, APIUpdateEventArgs args)
        {
            List<int> idsUpdating;
            //bool isCorp = false;
            switch (args.EventType)
            {
                case APIUpdateEventType.UpdateStarted:
                    idsUpdating = _updatesRunning[args.UpdateType];
                    if (!idsUpdating.Contains(args.OwnerID))
                    {
                        idsUpdating.Add(args.OwnerID);
                    }
                    //isCorp = false;
                    //UserAccount.CurrentGroup.GetCharacter(args.OwnerID, ref isCorp);
                    //if (args.UpdateType == APIDataType.Orders && isCorp)
                    //{
                    //    if (_corpOrderUpdates.ContainsKey(args.OwnerID))
                    //    {
                    //        _corpOrderUpdates[args.OwnerID] = (short)(_corpOrderUpdates[args.OwnerID] + 1);
                    //    }
                    //    else
                    //    {
                    //        _corpOrderUpdates.Add(args.OwnerID, (short)1);
                    //    }
                    //}
                    RefreshDisplay();
                    break;
                case APIUpdateEventType.UpdateCompleted:
                    // If we're dealing with corporate orders API updates then
                    // there can be multiple updates running for the same corp.
                    // Make sure they are all finished before removing the corp
                    // ID from the list of those being updated.
                    //isCorp = false;
                    bool removeIDFromList = true;
                    //UserAccount.CurrentGroup.GetCharacter(args.OwnerID, ref isCorp);
                    //if (args.UpdateType == APIDataType.Orders && isCorp)
                    //{
                    //    removeIDFromList = false;
                    //    if (_corpOrderUpdates.ContainsKey(args.OwnerID))
                    //    {
                    //        _corpOrderUpdates[args.OwnerID] = (short)(_corpOrderUpdates[args.OwnerID] - 1);
                    //        if (_corpOrderUpdates[args.OwnerID] <= 0)
                    //        {
                    //            _corpOrderUpdates[args.OwnerID] = 0;
                    //            removeIDFromList = true;
                    //        }
                    //    }
                    //}
                    if (removeIDFromList)
                    {
                        idsUpdating = _updatesRunning[args.UpdateType];
                        if (idsUpdating.Contains(args.OwnerID))
                        {
                            idsUpdating.Remove(args.OwnerID);
                        }
                    }
                    if (args.UpdateType == APIDataType.Assets)
                    {
                        DisplayAssetChangeScreen();
                    }
                    RefreshDisplay();
                    break;
                case APIUpdateEventType.OrderHasExpiredOrCompleted:
                    // Note: this is no longer needed since we refresh unacknowledged orders
                    // in the RefreshDisplay method instead... 
                    RefreshUnackOrders(false);
                    break;
                case APIUpdateEventType.AssetsAwaitingAcknowledgement:
                    if (!_assetUpdatesAwaitingAcknowledgement.Contains(args.OwnerID))
                    {
                        _assetUpdatesAwaitingAcknowledgement.Add(args.OwnerID);
                    }
                    DisplayAssetChangeScreen();
                    break;
                default:
                    break;
            }
        }

        void UpdateStatus_Move(object sender, EventArgs e)
        {
            UserAccount.Settings.UpdatePanelPos = _updateStatus.Location;
        }

        void UpdateStatus_ResizeEnd(object sender, EventArgs e)
        {
            UserAccount.Settings.UpdatePanelSize = _updateStatus.Size;
        }

        void ControlPanel_Move(object sender, EventArgs e)
        {
            UserAccount.Settings.ControlPanelPos = _controlPanel.Location;
        }

        private void DisplayAssetChangeScreen()
        {
            if (_assetUpdatesAwaitingAcknowledgement.Count > 0)
            {
                // Check if there are any assets updates either in progress or pending
                // If there are none then show the 'AcknowledgeAssetChanges' screen
                List<int> idsUpdating = _updatesRunning[APIDataType.Assets];
                bool updatesStillRunning = false;
                foreach (int id in idsUpdating)
                {
                    // If there are any ids in the 'updates running' list but NOT in the 
                    // 'updates awaiting acknowledgement' list then there must still be
                    // processing going on for another character/corp.
                    if (!_assetUpdatesAwaitingAcknowledgement.Contains(id))
                    {
                        updatesStillRunning = true;
                    }
                }

                if (!updatesStillRunning)
                {
                    // If no asset updates are still running then display the form.
                    AcknowledgeAssetChanges();
                }
            }
        }

        private void DisplayTutorial()
        {
            if (!_tutorialActive)
            {
                _tutorialThread = new Thread(new ThreadStart(DisplayTutorial2));
                _tutorialThread.Start();
            }
        }
        private void DisplayTutorial2()
        {
            try
            {
                Tutorial tutorial = new Tutorial();
                _tutorialActive = true;
                tutorial.ShowDialog();
                _tutorialActive = false;
            }
            catch (ThreadAbortException)
            {
                // This may be forced to abort, just go with it...
            }
        }
        #endregion

        #region General functionality methods
        /// <summary>
        /// Login automatically using the settings from the auto login XML file.
        /// </summary>
        /// <returns></returns>
        private bool AutoLogin()
        {
            bool retVal = false;

            if (File.Exists(Globals.AutoLoginFile))
            {
                UpdateStatus(0, 0, "Performing automatic login", "", false);
                this.Text = "EMMA - Logging in...";
                XmlDocument autoLoginXML = new XmlDocument();
                try
                {
                    autoLoginXML.Load(Globals.AutoLoginFile);

                    XmlNode usernameNode = autoLoginXML.SelectSingleNode("/Settings/Username");
                    string username = usernameNode.FirstChild.Value;
                    XmlNode passwordNode = autoLoginXML.SelectSingleNode("/Settings/Password");
                    string password = "";
                    if (passwordNode.FirstChild != null)
                    {
                        password = passwordNode.FirstChild.Value;
                    }

                    Diagnostics.StartTimer("OpenAccount");
                    UserAccount.OpenAccount(username, password);
                    Diagnostics.StopTimer("OpenAccount");

                    retVal = true;
                    _status = UserAccount.CurrentGroup == null ?
                        SystemStatus.NoReportGroupSelected : SystemStatus.Complete;
                }
                catch(Exception ex)
                {
                    //UpdateStatus(0, 0, "Done", "", true);
                    splash.ShowMessage( 
                        "Problem during auto login: " + ex.Message, "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }                
            }

            return retVal;
        }

        /// <summary>
        /// Prompt the user to login.
        /// </summary>
        /// <returns>True if the user sucessfully logged in, false otherwise.</returns>
        private bool Login()
        {
            bool retVal = false;
            Login login = new Login();

            DialogResult result = login.ShowDialog(this);
            if (result == DialogResult.Cancel && UserAccount.Name.Equals(""))
            {
                _status = SystemStatus.NoUserLoggedIn;
                RefreshDisplay();
            }
            else if (result == DialogResult.Cancel)
            {
                _status = SystemStatus.NoReportGroupSelected;
                RefreshDisplay();
            }
            else 
            {
                _status = UserAccount.CurrentGroup == null ? 
                    SystemStatus.NoReportGroupSelected : SystemStatus.Complete;
                InitaliseUpdatesRunning();
                RefreshDisplay();
                retVal = true;
            }

            return retVal;
        }

        /// <summary>
        /// Logoff the current user
        /// </summary>
        private void Logout()
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                if (_updateStatus != null && !_forceClose) { _updateStatus.BlockUntilUpdated(); }
                if (Globals.calculator != null) { Globals.calculator.StoreSizeAndPos(); }
                UserAccount.Logout();
                _status = SystemStatus.NoUserLoggedIn;
                InitaliseUpdatesRunning();
                RefreshDisplay();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Prompt the user to select a report group.
        /// </summary>
        private void SelectReportGroup()
        {
            if (UserLoggedIn())
            {
                ManageReportGroups rptGroup = new ManageReportGroups();
                DialogResult result = rptGroup.ShowDialog();
                if (result == DialogResult.Cancel && UserAccount.CurrentGroup == null)
                {
                    _status = SystemStatus.NoReportGroupSelected;
                    InitaliseUpdatesRunning();
                    RefreshDisplay();
                }
                else if (result == DialogResult.Cancel && UserAccount.CurrentGroup != null) 
                {
                    _status = SystemStatus.Complete;
                    RefreshDisplay();
                }
                else if (result != DialogResult.Cancel)
                {
                    UserAccount.CurrentGroup = rptGroup.SelectedGroup;
                    UserAccount.Settings.FirstRun = false;
                    _status = SystemStatus.Complete;
                    InitaliseUpdatesRunning();
                    if (_updateStatus != null && _updateStatus.Visible)
                    {
                        _updateStatus.Close();
                        _updateStatus = null;
                    }
                    if (_unackOrders != null && _unackOrders.Visible)
                    {
                        _unackOrders.Close();
                        _unackOrders = null;
                    }
                    RefreshDisplay();
                }
            }
        }

        private void ManageReportGroup()
        {
            if (UserLoggedIn() && ReportGroupSelected())
            {
                ReportGroupSetup setup = new ReportGroupSetup();
                setup.ShowDialog();
            }
        }

        private void Import()
        {
            if (UserLoggedIn() && ReportGroupSelected())
            {
                ImportData importForm = new ImportData();
                importForm.ShowDialog(this);
            }
        }

        private void Export()
        {
            if (UserLoggedIn() && ReportGroupSelected())
            {
                ExportData exportForm = new ExportData();
                exportForm.ShowDialog(this);
            }
        }


        private bool UserLoggedIn()
        {
            bool retVal = true;
            if (UserAccount.Name.Equals(""))
            {
                MessageBox.Show("Error, No user currently logged in");
                retVal = false;
                _status = SystemStatus.NoUserLoggedIn;
                RefreshDisplay();
            }
            return retVal;
        }
        private bool ReportGroupSelected()
        {
            bool retVal = true;
            if (UserAccount.CurrentGroup == null)
            {
                MessageBox.Show("Error, No report group currently selected");
                retVal = false;
                _status = SystemStatus.NoReportGroupSelected;
                RefreshDisplay();
            }
            return retVal;
        }

        private void ValidateInstall(bool showDialogIfValid)
        {
            try
            {
                /*Enforcer.LicenseManagement licenseMgt = new LicenseManagement();
                _license = licenseMgt.GetLicenseType();

                if (showDialogIfValid || (_license != LicenseType.Full && _license != LicenseType.Lite))
                {
                    licenseMgt.ShowDialog();

                    _license = licenseMgt.GetLicenseType();

                    if (_license == LicenseType.Invalid || _license == LicenseType.TrialExpired)
                    {
                        MessageBox.Show("Your trial time has expired and you do not have a valid license key.");
                        this.Close();
                    }
                }*/
                _license = LicenseType.Full;
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was a problem validating your installation.\r\n" +
                    ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }
        #endregion

        #region Menu button handlers
        private void btnTransactions_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            Diagnostics.ResetAllTimers();
            Diagnostics.StartTimer("Main.ShowTrans");
            try
            {
                ViewTransactions viewTrans = new ViewTransactions();
                viewTrans.MdiParent = this;
                viewTrans.Show();
            }
            finally
            {
                Diagnostics.StopTimer("Main.ShowTrans");
                Cursor = Cursors.Default;
                Diagnostics.DisplayDiag(Diagnostics.GetRunningTime("Main.ShowTrans").ToString());
            }
        }

        private void btnJournal_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                ViewJournal viewJournal = new ViewJournal();
                viewJournal.MdiParent = this;
                viewJournal.Show();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnAssets_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                ViewAssets viewAssets = new ViewAssets();
                viewAssets.MdiParent = this;
                viewAssets.Show();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnOrders_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                ViewOrders viewOrders = new ViewOrders();
                viewOrders.MdiParent = this;
                viewOrders.Show();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnReports_Click(object sender, EventArgs e)
        {
            CreateReport createReport = new CreateReport();
            createReport.MdiParent = this;
            createReport.Show();
        }

        private void btnContracts_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                ViewContracts viewContracts = new ViewContracts();
                viewContracts.MdiParent = this;
                viewContracts.Show();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }


        private void btnInvestments_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                ViewInvestments viewInvestments = new ViewInvestments();
                viewInvestments.MdiParent = this;
                viewInvestments.Show();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }


        private void btnRoutePlanner_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                RoutePlanner planner = new RoutePlanner();
                planner.MdiParent = this;
                planner.Show();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnDeliveryPlanner_Click(object sender, EventArgs e)
        {
            /*Cursor = Cursors.WaitCursor;
            try
            {
                DeliveryPlanner planner = new DeliveryPlanner();
                planner.MdiParent = this;
                planner.Show();
            }
            finally
            {
                Cursor = Cursors.Default;
            }*/
        }

        private void btnReprocessor_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                Reprocessor reprocessor = new Reprocessor();
                reprocessor.MdiParent = this;
                reprocessor.Show();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        #endregion

        #region Prerequesite checks
        private bool Prerequs()
        {
            bool retVal = true;
            //try
            //{
                if (!EveMarketMonitorApp.Properties.Settings.Default.SkipExpressCheck)
                {
                    if (!isSQLExpressInstalled())
                    {
                        retVal = false;
                        /*MessageBox.Show("You do not have SQL Express 2005 SP2 installed.\r\nYou can download " +
                            "it from Microsoft for free: http://www.microsoft.com/express/2005/sql/download" +
                            "/default.aspx\r\nIf you wish to skip this check in the future then you can " +
                            "disable it in the 'EveMarketMonitorApp.exe.config' file.\r\nEMMA will now close.",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);*/
                    }
                    if (retVal && !isSQLExpressRunning())
                    {
                        retVal = false;
                        //UpdateStatus(0, 0, "Done", "", true);
                        throw new EMMAException(ExceptionSeverity.Critical, 
                            "It appears that the SQL Express service is not running and cannot be " +
                            "started.\r\nIf you wish to skip this " +
                            "check in the future then you can disable it in the "+
                            "'EveMarketMonitorApp.exe.config' file.");
                    }
                    if (retVal && !waitForSQLAcknowledge())
                    {
                        retVal = false;
                        //UpdateStatus(0, 0, "Done", "", true);
                        throw new EMMAException(ExceptionSeverity.Critical,
                            "EMMA is unable to connect to the database.");
                    }
                }
            //}
            //catch (EMMAException)
            //{
                //UpdateStatus(0, 0, "Done", "", true);
            //    MessageBox.Show((splash != null ? (IWin32Window)splash : (IWin32Window)this), 
            //        "There was an error while checking if SQL server express is installed and " +
            //        "running. (Check " + Globals.AppDataDir + "Logging\\exceptionlog.txt for details)\r\nEMMA will attempt to " +
            //        "continue but if things don't work then that's probably the reason.", 
            //        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
            return retVal;
        }

        private bool waitForSQLAcknowledge()
        {
            bool retVal = false;

            try
            {
                UpdateStatus(0, 0, "Testing database connection", "", false);
                Updater.WaitForAcknowledge();
                retVal = true;
            }
            catch (Exception ex)
            {
                EMMAException emmaEx = ex as EMMAException;
                if (emmaEx == null)
                {
                    new EMMAException(ExceptionSeverity.Critical, "Error connecting to database", ex);
                }
            }

            return retVal;
        }

        private bool isSQLExpressRunning()
        {
            bool retVal = true;
            try
            {
                System.ServiceProcess.ServiceController express =
                    new System.ServiceProcess.ServiceController("MSSQL$SQLEXPRESS");

                try
                {
                    if (express.Status != System.ServiceProcess.ServiceControllerStatus.Running)
                    {
                        if (express.Status == System.ServiceProcess.ServiceControllerStatus.Stopped)
                        {
                            express.Start();
                        }
                        UpdateStatus(0, 0, "Waiting for SQL Express Service", "", false);
                        express.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running);
                    }
                    if (express.Status != System.ServiceProcess.ServiceControllerStatus.Running)
                    {
                        retVal = false;
                    }
                }
                catch (Exception ex)
                {
                    throw new EMMAException(ExceptionSeverity.Critical, "Cannot start SQL Express service", ex);
                }
            }
            catch (Exception ex)
            {
                throw new EMMAException(ExceptionSeverity.Critical, "Problem accessing SQL Express service", ex);
            }

            return retVal;
        }

        private bool isSQLExpressInstalled()
        {
            bool retVal = false;

            try
            {
                SQLVersion version = SQLExpressChecks.GetSQLVersion();

                switch (version)
                {
                    case SQLVersion.None:
                        UpdateStatus(0, 0, "Done", "", true);
                        MessageBox.Show("EMMA requires SQL Express 2008. " +
                            "Please download and install it from here: http://www.microsoft.com/express/database/",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        
                        break;
                    case SQLVersion.SQL2005:
                        UpdateStatus(0, 0, "Done", "", true);
                        MessageBox.Show("You only have SQL Express 2005 installed. EMMA requires SQL Express 2008. " +
                            "Please download and install it from here: http://www.microsoft.com/express/database/",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case SQLVersion.SQL2008:
                        retVal = true;
                        break;
                    case SQLVersion.Later:
                        retVal = true;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new EMMAException(ExceptionSeverity.Error, "Problem detecting SQL installation", ex);
            }

            return retVal;

            /*try
            {
                using (RegistryKey Key = Registry.LocalMachine.OpenSubKey(
                    "Software\\Microsoft\\Microsoft SQL Server\\", false))
                {
                    if (Key == null) return false;
                    string[] strNames;
                    strNames = Key.GetSubKeyNames();

                    //If we cannot find a SQL Server registry key, we don't have SQL Server Express installed
                    if (strNames.Length == 0) return false;

                    foreach (string s in strNames)
                    {
                        if (s.Equals("SQLEXPRESS"))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
            catch (Exception ex)
            {
                throw new EMMAException(ExceptionSeverity.Error, "Problem detecting SQL Server Install", ex);
            }*/


        }
        #endregion

        private void RepairDatabase()
        {
            VerifyDatabase verify = new VerifyDatabase();
            Thread t0 = new Thread(new ThreadStart(verify.Run));
            ProgressDialog dialog = new ProgressDialog("Verifying Data", verify);
            t0.Start();
            if (dialog.ShowDialog() == DialogResult.Cancel)
            {
                t0.Abort();
                t0.Join();

                MessageBox.Show("Data integrity check was cancelled.",
                    "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void UpdateStatus(int progress, int maxProgress, string section, string sectionStatus, bool done)
        {
            if (StatusChange != null)
            {
                StatusChange(null, new StatusChangeArgs(progress, maxProgress, section, sectionStatus, done));
            }
        }

        private void Main_ResizeBegin(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
            {
                EveMarketMonitorApp.Properties.Settings.Default.NormalWinState = WindowState;
                //if (WindowState == FormWindowState.Normal)
                //{
                //    EveMarketMonitorApp.Properties.Settings.Default.WindowSize = Size;
                //    EveMarketMonitorApp.Properties.Settings.Default.WindowPos = Location;
                //}
            }
        }

        private void Main_ResizeEnd(object sender, EventArgs e)
        {
            if (_controlPanel != null)
            {
                //if (_controlPanel.Location.X < 0 ||
                //    this.Width - _controlPanel.Width - 20 < _controlPanel.Location.X)
                //{
                    _controlPanel.Location = new Point(this.Width - _controlPanel.Width - 20, 10);
                //}
            }
            if (WindowState == FormWindowState.Normal)
            {
                EveMarketMonitorApp.Properties.Settings.Default.WindowSize = Size;
                EveMarketMonitorApp.Properties.Settings.Default.WindowPos = Location;
            }
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            EveMarketMonitorApp.Properties.Settings.Default.WindowState = WindowState;
            if (WindowState == FormWindowState.Minimized)
            {
                if (!UserAccount.Settings.ShowInTaskbarWhenMinimised)
                {
                    Hide();
                }
            }
            else
            {
                EveMarketMonitorApp.Properties.Settings.Default.NormalWinState = WindowState;
                if (EveMarketMonitorApp.Properties.Settings.Default.NormalWinState != FormWindowState.Normal &&
                    WindowState == FormWindowState.Normal)
                {
                    Size = EveMarketMonitorApp.Properties.Settings.Default.WindowSize;
                    Location = EveMarketMonitorApp.Properties.Settings.Default.WindowPos;
                }
            }

            if (_controlPanel != null)
            {
                //if (_controlPanel.Location.X < 0 ||
                //    this.Width - _controlPanel.Width - 20 < _controlPanel.Location.X)
                //{
                    _controlPanel.Location = new Point(this.Width - _controlPanel.Width - 20, 10);
                //}
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            if (!Visible)
            {
                Show();
            }
            WindowState = (EveMarketMonitorApp.Properties.Settings.Default.WindowState == 
                FormWindowState.Minimized ? EveMarketMonitorApp.Properties.Settings.Default.NormalWinState : 
                EveMarketMonitorApp.Properties.Settings.Default.WindowState);
            if (WindowState == FormWindowState.Normal)
            {
                Size = EveMarketMonitorApp.Properties.Settings.Default.WindowSize;
                Location = EveMarketMonitorApp.Properties.Settings.Default.WindowPos;
            }
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForUpdatesInProg())
            {
                this.Close();
            }
        }

        private void PingServers()
        {
            if (!Properties.Settings.Default.SkipConnectionCheck)
            {
                try { Ping("api.eve-online.com"); }
                catch (EMMAException)
                {
                    Globals.EveAPIDown = true;
                    //UpdateStatus(0, 0, "Done", "", true);
                    splash.ShowMessage(
                        "Failed to contact the Eve API.\r\nAPI updates will be disabled until EMMA is restarted.\r\n" +
                        "(See " + EMMAException.logFile + " for more detailed information)",
                        "Communication failure", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                try { Ping("www.eve-central.com"); }
                catch (EMMAException)
                {
                    Globals.EveCentralDown = true;
                    //UpdateStatus(0, 0, "Done", "", true);
                    splash.ShowMessage(
                        "Failed to contact eve-central.\r\nPrice updates from eve-central will be disabled until " +
                        "EMMA is restarted.\r\n(See " + EMMAException.logFile + " for more " +
                        "detailed information)", "Communication failure",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                try { Ping("www.eve-metrics.com"); }
                catch (EMMAException)
                {
                    Globals.EveMetricsDown = true;
                    //UpdateStatus(0, 0, "Done", "", true);
                    splash.ShowMessage( 
                        "Failed to contact eve-metrics.\r\n" +
                        "Price updates from eve-metrics will be disabled until EMMA is restarted.\r\n" +
                        "(See " + EMMAException.logFile + " for more detailed information)", "Communication failure",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                Globals.EMMAUpdateServer = "";
                int serverIndex = 0;
                bool connectedOk = false;
                System.Collections.Specialized.StringCollection updateServers = 
                    EveMarketMonitorApp.Properties.Settings.Default.UpdateServers;
                while(!connectedOk && updateServers.Count > serverIndex) 
                {
                    string pingserver = updateServers[serverIndex];
                    Globals.EMMAUpdateServer = pingserver;
                    if (pingserver.Equals("www.eve-files.com"))
                    {
                        Globals.EMMAUpdateServer = "go-dl.eve-files.com";
                    }
                    try 
                    { 
                        Ping(pingserver);
                        connectedOk = true;
                    }
                    catch
                    {
                        Globals.EMMAUpdateServer = "";
                    }
                    serverIndex++;
                }
                if (Globals.EMMAUpdateServer.Equals("") &&
                    updateServers.Count > 0)
                {
                    //UpdateStatus(0, 0, "Done", "", true);
                    splash.ShowMessage(
                        "Failed to contact any auto-update server.\r\n" +
                        "EMMA will be unable to update until it is restarted.\r\n" +
                        "(See " + EMMAException.logFile + " for more detailed information)", "Communication failure",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }


                //if (Globals.EMMAUpdateServer.Equals("") && Globals.EveAPIDown &&
                //    Globals.EveCentralDown)
                //{
                    //UpdateStatus(0, 0, "Done", "", true);
                    //if (MessageBox.Show((splash != null ? (IWin32Window)splash : (IWin32Window)this), 
                    //    "Connection checks to ALL servers have failed.\r\n" +
                    //    "Do you wish to skip these checks in future?", "Communication failure",
                    //        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    //{
                    //    Properties.Settings.Default.SkipConnectionCheck = true;
                    //}
                //}
            }
        }

        private void Ping(string url)
        {
            Ping ping = new Ping();
            PingReply reply;
            try
            {
                reply = ping.Send(url, 1000);

                if (reply.Status != IPStatus.Success)
                {
                    reply = ping.Send(url, 1000);

                    if (reply.Status != IPStatus.Success)
                    {
                        throw new EMMAException(ExceptionSeverity.Error, "Failed to contact " + url + "\r\n" +
                            "Status - " + reply.Status.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                EMMAException emmaEx = ex as EMMAException;
                if (emmaEx == null)
                {
                    throw new EMMAException(ExceptionSeverity.Error, "Failed to contact " + url, ex);
                }
                else { throw ex; }
            } 
        }


        private void WasteTime()
        {
            if (!_wastedTime)
            {
                StringBuilder name1 = new StringBuilder();
                name1.Append(Globals.letters[18].ToString().ToUpper());
                name1.Append(Globals.letters[20]);
                name1.Append(Globals.letters[8]);
                name1.Append(Globals.letters[2]);
                name1.Append(Globals.letters[8]);
                name1.Append(Globals.letters[3]);
                name1.Append(Globals.numbers[2]);
                name1.Append(" ");
                name1.Append(Globals.letters[6].ToString().ToUpper());
                name1.Append(Globals.letters[0]);
                name1.Append(Globals.letters[13]);
                name1.Append(Globals.letters[10]);
                name1.Append(Globals.numbers[2]);
                name1.Append(Globals.letters[17]);

                StringBuilder name2 = new StringBuilder();
                name2.Append(Globals.letters[8]);
                name2.Append(Globals.letters[13]);
                name2.Append(Globals.letters[21].ToString().ToUpper());
                name2.Append(Globals.letters[8]);
                name2.Append(Globals.letters[2]);
                name2.Append(Globals.letters[19]);
                name2.Append(Globals.letters[20]);
                name2.Append(Globals.numbers[4]);

                if (UserAccount.CurrentGroup != null && UserAccount.CurrentGroup.Accounts != null)
                {
                    List<EVEAccount> accounts = UserAccount.CurrentGroup.Accounts;
                    foreach (EVEAccount account in accounts)
                    {
                        if (account != null && account.Chars != null)
                        {
                            List<APICharacter> characters = account.Chars;
                            foreach (APICharacter character in characters)
                            {
                                if (character != null && character.CharName != null)
                                {
                                    if (character.CharName.Equals(name1.ToString()) ||
                                        character.CharName.Equals(name2.ToString()))
                                    {
                                        this.Close();
                                    }
                                }
                            }
                        }
                    }
                }
                _wastedTime = true;
            }
        }

        private void btnItemDetail_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor; try
            {

                ViewItemDetail itemDetail = new ViewItemDetail();
                itemDetail.MdiParent = this;
                itemDetail.Show();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }


    }

    public enum SystemStatus
    {
        NoUserLoggedIn,
        NoReportGroupSelected,
        Complete
    }
}
