using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Threading;
using System.IO;

using EveMarketMonitorApp.AbstractionClasses;
using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class ImportData : Form, IProvideStatus
    {
        public event StatusChangeHandler StatusChange;

        public ImportData()
        {
            InitializeComponent();
            rdbAPIXML.Checked = true;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.Visible = false;

            if (rdbAPIXML.Checked)
            {
                System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
                dialog.Filter = "XML files|*.xml";
                dialog.Title = "Import EVE API Data";
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
                {
                    XmlDocument xml = new XmlDocument();
                    xml.Load(dialog.FileName);
                    APIDataType type = EveAPI.GetFileType(xml);
                    APICharacter character = UserAccount.CurrentGroup.Accounts[0].Chars[0];
                    int walletID = 0;
                    CharOrCorp corc = CharOrCorp.Char;
                    bool Cancel = false;

                    // Determine correct APICharacter instance to use when loading the data.
                    SortedList<object, string> options = new SortedList<object, string>();
                    List<CharCorpOption> charcorps = UserAccount.CurrentGroup.GetCharCorpOptions();
                    charcorps.Sort();
                    foreach (CharCorpOption opt in charcorps)
                    {
                        options.Add(opt.Data.ID, opt.Name);
                    }

                    OptionPicker picker = new OptionPicker("Select Char/Corp",
                        "Please select the character or corp to use when loading this " + type.ToString() +
                        " file into the database.", options);

                    if (picker.ShowDialog() != DialogResult.Cancel)
                    {
                        int chosenID = (int)picker.SelectedItem;
                        bool corp = false;
                        character = UserAccount.CurrentGroup.GetCharacter(chosenID, ref corp);
                        corc = corp ? CharOrCorp.Corp : CharOrCorp.Char;

                        if ((type == APIDataType.Journal || type == APIDataType.Transactions) && 
                            corc == CharOrCorp.Corp)
                        {
                            SortedList<object, string> options2 = new SortedList<object, string>();
                            foreach(EMMADataSet.WalletDivisionsRow wallet in character.WalletDivisions) 
                            {
                                options2.Add(wallet.ID, wallet.Name + " (" + wallet.ID + ")");
                            }
                            OptionPicker picker2 = new OptionPicker("Select Wallet",
                                "Please select the corporate wallet to be used when loading this journal data.",
                                options2);
                            if (picker2.ShowDialog() == DialogResult.Cancel)
                            {
                                Cancel = true;
                            }
                            else
                            {
                                walletID = (int)picker2.SelectedItem;
                            }
                        }

                        if (!Cancel)
                        {
                            IProgressDialog prgDialog = null;
                                
                            switch (type)
                            {
                                case APIDataType.Transactions:
                                    prgDialog = new ProgressDialog("Loading Transactions", character);
                                    character.ProcessTransactionsXML(xml, corc, (short)walletID);
                                    prgDialog.ShowDialog();
                                    break;
                                case APIDataType.Journal:
                                    prgDialog = new ProgressDialog("Loading Journal Data", character);
                                    character.ProcessJournalXML(xml, corc, (short)walletID);
                                    prgDialog.ShowDialog();
                                    break;
                                case APIDataType.Assets:
                                    prgDialog = new DetailProgressDialog("Loading Assets", character);
                                    character.ProcessAssetXML(xml, corc);
                                    prgDialog.ShowDialog();
                                    break;
                                case APIDataType.Orders:
                                    prgDialog = new ProgressDialog("Loading Orders", character);
                                    character.ProcessOrdersXML(xml, corc);
                                    prgDialog.ShowDialog();
                                    break;
                                case APIDataType.IndustryJobs:
                                    prgDialog = new ProgressDialog("Loading Industry Jobs", character);
                                    character.ProcessIndustryJobsXML(xml, corc);
                                    prgDialog.ShowDialog();
                                    break;
                                case APIDataType.Unknown:
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }

            }
            else if (rdbEveIncome.Checked)
            {

            }
            else if (rdbEMMA.Checked)
            {
                System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
                dialog.Filter = "EMMA data files (*.DAT, *.CDAT)|*.dat;*.cdat";
                dialog.Title = "Import EMMA Data";
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
                {
                    Thread t1 = new Thread(new ParameterizedThreadStart(LoadEMMAFile));
                    t1.SetApartmentState(ApartmentState.STA);
                    ProgressDialog progress = new ProgressDialog("Importing EMMA Data...", this);
                    t1.Start(dialog.FileName);
                    DialogResult result = progress.ShowDialog();
                    if (result == DialogResult.Cancel)
                    {
                        // Stop our worker thread if the user has cancelled out of the progress dialog.
                        t1.Abort();
                        t1.Join();

                        MessageBox.Show("Import was cancelled.\r\nAny data already committed to the " +
                            "database will still be there.", "Notification", MessageBoxButtons.OK, 
                            MessageBoxIcon.Information);
                    }
                }
            }


            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void LoadEMMAFile(object filenameData)
        {
            int maxStatus = 7;
            string filename = (string)filenameData;
            string tmpLoadDir = string.Format("{0}Temp{1}Load{1}",
                Globals.AppDataDir, Path.DirectorySeparatorChar);
            UpdateStatus(0, maxStatus, "", "Decompressing files", false);
            try
            {
                Directory.CreateDirectory(tmpLoadDir);
                float saveVersion = Compression.DecompressDirectory(filename, tmpLoadDir);

                if (saveVersion == 0)
                {
                    UpdateStatus(0, 0, "Error: File version is too old", "", true);
                }
                else if (saveVersion == 1)
                {
                    if (filename.ToUpper().EndsWith(".CDAT"))
                    {
                        Dictionary<int, int> IDChanges = new Dictionary<int, int>();
                        try
                        {
                            PublicCorps.LoadOldEmmaXML(tmpLoadDir + "CorpData.xml", ref IDChanges);
                            Dividends.LoadOldEmmaXML(tmpLoadDir + "Dividends.xml", IDChanges);
                            WebLinks.LoadOldEmmaXML(tmpLoadDir + "WebLinks.xml", IDChanges);
                            // Try to link journal entries to the dividends
                            Dividends.UpdateFromJournal(true); 
                            UpdateStatus(1, 1, "Complete", "", true);
                        }
                        catch (ThreadAbortException abortEx)
                        {
                            // User has closed the progress dialog so just throw the exception to the next level.
                            throw abortEx;
                        }
                        catch (Exception ex)
                        {
                            EMMAException emmaEx = ex as EMMAException;
                            if (emmaEx == null)
                            {
                                emmaEx = new EMMAException(ExceptionSeverity.Error, "Problem importing old EMMA public corp data", ex);
                            }
                            UpdateStatus(-1, -1, "Error", ex.Message, true);
                        }
                    }
                    else
                    {
                        APICharacter character = UserAccount.CurrentGroup.Accounts[0].Chars[0];
                        CharOrCorp corc = CharOrCorp.Char;

                        // Determine correct APICharacter instance to use when loading the data.
                        SortedList<object, string> options = new SortedList<object, string>();
                        List<CharCorpOption> charcorps = UserAccount.CurrentGroup.GetCharCorpOptions();
                        charcorps.Sort();
                        foreach (CharCorpOption opt in charcorps)
                        {
                            options.Add(opt.Data, opt.Name);
                        }

                        OptionPicker picker = new OptionPicker("Select Char/Corp",
                            "Please select the character or corp to use when loading this file into the database.",
                            options);

                        if (picker.ShowDialog() != DialogResult.Cancel)
                        {
                            CharCorp chosen = (CharCorp)picker.SelectedItem;
                            character = chosen.characterObj;
                            corc = chosen.corp ? CharOrCorp.Corp : CharOrCorp.Char;
                        }

                        LoadOldDataParams parameters = new LoadOldDataParams();
                        parameters.dataDirectory = tmpLoadDir;
                        parameters.corc = corc;
                        parameters.character = character;
                        LoadOldEMMAData(parameters);
                    }
                }
                else if (saveVersion == 2)
                {
                }
            }
            catch (ThreadAbortException)
            {
                // User has closed the progress dialog so just allow the execution to fall out of this loop.
            }

        }


        private void LoadOldEMMAData(object parameterData)
        {
            LoadOldDataParams parameters = (LoadOldDataParams)parameterData;
            string tmpLoadDir = parameters.dataDirectory;
            APICharacter character = parameters.character;
            CharOrCorp corc = parameters.corc;

            try
            {
                int maxStatus = 6;
                UpdateStatus(1, maxStatus, "Importing Data", "Loading Journal (" +
                    (new FileInfo(tmpLoadDir + "Journal.xml").Length / 1024).ToString() + " Kb)", false);
                Journal journ = new Journal();
                journ.StatusChange += new StatusChangeHandler(child_StatusChange);
                journ.LoadOldEmmaXML(tmpLoadDir + "Journal.xml", character.CharID, 
                    corc == CharOrCorp.Corp ? character.CorpID : 0);
                UpdateStatus(2, maxStatus, "Importing Data", "Loading Transactions (" +
                    (new FileInfo(tmpLoadDir + "Transactions.xml").Length / 1024).ToString() + " Kb)", false);
                Transactions trans = new Transactions();
                trans.StatusChange += new StatusChangeHandler(child_StatusChange);
                trans.LoadOldEmmaXML(tmpLoadDir + "Transactions.xml", character.CharID,
                    corc == CharOrCorp.Corp ? character.CorpID : 0);
                UpdateStatus(3, maxStatus, "Importing Data", "Loading Contracts (" +
                    (new FileInfo(tmpLoadDir + "Contracts.xml").Length / 1024).ToString() + " Kb) and Contract " +
                    "Items (" + (new FileInfo(tmpLoadDir + "ContractItems.xml").Length / 1024).ToString() + " Kb)", 
                    false);
                Contracts cont = new Contracts();
                cont.StatusChange += new StatusChangeHandler(child_StatusChange);
                cont.LoadOldEmmaXML(tmpLoadDir + "Contracts.xml", tmpLoadDir + "ContractItems.xml", 
                    corc == CharOrCorp.Char ? character.CharID : character.CorpID);
                // Just need this 'if' for backwards compatibility...
                if (File.Exists(tmpLoadDir + "ShareTrans.xml"))
                {
                    UpdateStatus(4, maxStatus, "Importing Data", "Loading Share Transactions (" +
                        (new FileInfo(tmpLoadDir + "ShareTrans.xml").Length / 1024).ToString() + " Kb)", false);
                    ShareTransactions shareTrans = new ShareTransactions();
                    shareTrans.StatusChange += new StatusChangeHandler(child_StatusChange);
                    shareTrans.LoadOldEmmaXML(tmpLoadDir + "ShareTrans.xml", UserAccount.CurrentGroup.ID);

                    /*
                    UpdateStatus(5, maxStatus, "Importing Data", "Loading Characters (" +
                        (new FileInfo(tmpLoadDir + "APIKeyInfo.xml").Length / 1024).ToString() + " Kb)", false);
                    Names.LoadOldEmmaXML(tmpLoadDir + "APIKeyInfo.xml");
                    UpdateStatus(6, maxStatus, "Importing Data", "Loading Traded Items (" +
                        (new FileInfo(tmpLoadDir + "TradedItems.xml").Length / 1024).ToString() + " Kb)", false);
                    UserAccount.CurrentGroup.ItemsTraded.LoadOldEmmaXML(tmpLoadDir + "TradedItems.xml");
                    */
                }

                UpdateStatus(maxStatus - 1, maxStatus, "Tidying up", "", false);
                Directory.Delete(tmpLoadDir, true);
                UpdateStatus(maxStatus, maxStatus, "Import Complete", "", true);

            }
            catch (ThreadAbortException abortEx)
            {
                // User has closed the progress dialog so just throw the exception to the next level.
                throw abortEx;
            }
            catch (Exception ex)
            {
                EMMAException emmaEx = ex as EMMAException;
                if (emmaEx == null)
                {
                    emmaEx = new EMMAException(ExceptionSeverity.Error, "Problem importing old EMMA data", ex);
                }
                UpdateStatus(-1, -1, "Error", ex.Message, true);
            }
        }

        void child_StatusChange(object myObject, StatusChangeArgs args)
        {
            if (StatusChange != null)
            {
                StatusChange(null, args);
            }
        }

        public void UpdateStatus(int progress, int maxProgress, string section, string sectionStatus, bool done)
        {
            if (StatusChange != null)
            {
                StatusChange(null, new StatusChangeArgs(progress, maxProgress, section, sectionStatus, done));
            }
        }

        private struct LoadOldDataParams
        {
            public string dataDirectory;
            public APICharacter character;
            public CharOrCorp corc;
        }

    }


}