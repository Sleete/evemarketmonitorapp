using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Security.Permissions;

using EveMarketMonitorApp.GUIElements;
using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.DatabaseClasses;

//[assembly: FileIOPermission(SecurityAction.RequestMinimum, Unrestricted = true)]
//[assembly: RegistryPermission(SecurityAction.RequestMinimum, Unrestricted = true)]

namespace EveMarketMonitorApp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            bool checkForUpdates = true;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("/u"))
                {
                    try
                    {
                        checkForUpdates = bool.Parse(args[i + 1]);
                    }
                    catch
                    {
                        checkForUpdates = true;
                    }
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Main main = new Main(checkForUpdates);
                if(!main.IsDisposed) 
                {
                    Application.Run(main);
                }
            }
            catch (Exception ex)
            {
                // Creating new exception will cause error to be logged.
                EMMAException emmaex = ex as EMMAException;
                if (emmaex == null)
                {
                    emmaex = new EMMAException(ExceptionSeverity.Critical, "Unhandled exception", ex);
                }
                MessageBox.Show("An unexpected error has occured.\r\nCheck " + Globals.AppDataDir + "Logging\\ExceptionLog.txt" +
                    " for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                try
                {
                    // Attempt to logout. This will save all settings, etc.
                    UserAccount.Logout();
                }
                catch (Exception ex2)
                {
                    EMMAException emmaex2 = ex2 as EMMAException;
                    if (emmaex2 == null)
                    {
                        emmaex2 = new EMMAException(ExceptionSeverity.Critical, 
                            "Error during automatic log out", ex2);
                    }
                }

                // Close the application. In some cases it may still be able to continue but it's
                // much safer to just shut things down.
                Application.Exit();
            }
        }
    }
}