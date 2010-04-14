using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;
using System.Threading;

//[assembly: FileIOPermission(SecurityAction., Unrestricted = true)]

namespace AutoUpdater
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            string homeDir = @"";
            int parentProcess = -1;
            string server = "";
            bool betaUpdates = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("/h"))
                {
                    homeDir = args[i + 1];
                }
                if (args[i].Equals("/p"))
                {
                    parentProcess = int.Parse(args[i + 1]);
                }
                if (args[i].Equals("/s"))
                {
                    server = args[i + 1];
                }
                if (args[i].Equals("/b"))
                {
                    try
                    {
                        betaUpdates = bool.Parse(args[i + 1]);
                    }
                    catch
                    {
                        betaUpdates = false;
                    }
                }
            }

            if (!homeDir.Equals("") && !server.Equals("") && parentProcess != -1)
            {
                try
                {
                    Application.EnableVisualStyles();
                    if (homeDir.EndsWith("\""))
                    {
                        homeDir = homeDir.Substring(0, homeDir.Length - 1);
                    }
                    if (server.EndsWith("\""))
                    {
                        server = server.Substring(0, server.Length - 1);
                    }

                    Form1 mainForm = new Form1(homeDir, server, betaUpdates);
                    //if (mainForm.UpdateNeeded)
                    //{
                        try
                        {
                            Process p = Process.GetProcessById(parentProcess);
                            if (p != null)
                            {
                                p.Kill();
                            }
                        }
                        catch (Exception ex)
                        { 
                            // If we can't kill the parent process then just try continuing anyway
                            MessageBox.Show("Unable to kill EMMA process (" + ex.Message + 
                                ").\r\nThis may cause the update to fail " +
                                "but we'll try to continue anyway.\r\n" +
                                "If the update does fail then restart EMMA and use task manager to kill " +
                                "the EMMA process if this warning appears again.",
                                "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }

                        //if (mainForm.UserHasAccess())
                        //{
                            Application.Run(mainForm);
                        //}
                        //else
                        //{
                        //    MessageBox.Show("Updates are available and you do not appear to have write " +
                        //        "access to the EMMA directory.\r\nPlease start EMMA with administrator " +
                        //        "privilages (right click the icon then choose 'run as administrator').\r\n" +
                        //        "This will allow the update to be completed.", "Permissions",
                        //        MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //}
                    //}
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Problem running auto-updater.\r\n" + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("This component should not be run manually.");
            }

        }
    }
}