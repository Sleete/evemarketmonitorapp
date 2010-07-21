using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using EveMarketMonitorApp.Reporting;
using EveMarketMonitorApp.AbstractionClasses;
using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class CreateReport : Form
    {
        private List<ReportType> types;
        private IReport reportToRun;
        private RptParamsBase paramFormToRun;
        private IskMultiplier showAmountsIn;
        private CharCorpOption lastSelectedOwner;
        private Dictionary<int, List<int>> useDataFrom;
        private bool _selectAll = false;

        static Dictionary<string, object> parameters;

        public CreateReport()
        {
            InitializeComponent();
            useDataFrom = new Dictionary<int, List<int>>();

            SetupReportTypes();
            SetupGUI();
        }

        private void SetupGUI()
        {
            cmbReportTypes.Items.Clear();
            cmbReportTypes.Items.AddRange(types.ToArray());
            cmbReportTypes.SelectedIndex = 0;

            cmbValuesIn.Items.Clear();
            cmbValuesIn.Items.AddRange(Enum.GetNames(Type.GetType("EveMarketMonitorApp.Reporting.IskMultiplier")));
            cmbValuesIn.Text = IskMultiplier.ISK.ToString();

            RefreshCharList();
        }

        private void RefreshCharList()
        {
            lstOwners.Items.Clear();
            lstOwners.Enabled = false;
            if (cmbReportTypes.SelectedItem != null)
            {
                ReportType rpt = (ReportType)cmbReportTypes.SelectedItem;
                List<CharCorpOption> owners = UserAccount.CurrentGroup.GetCharCorpOptions(rpt.ApiAccessType);
                foreach (CharCorpOption owner in owners)
                {
                    // Note that 'useDataFrom' is automatically initalised as the items are added 
                    // to the list by the 'lstOwners_ItemCheck' method.
                    lstOwners.Items.Add(owner, true);
                }
                lstOwners.Enabled = true;
            }
            lstOwners.Sorted = true;
                
            RefreshWalletList();
        }

        private void RefreshWalletList()
        {
            CharCorpOption selectedOwner = (CharCorpOption)lstOwners.SelectedItem;
            chkWallets.Items.Clear();
            chkWallets.Enabled = false;

            if (selectedOwner != null)
            {
                int id = selectedOwner.Corp ? selectedOwner.CharacterObj.CorpID : selectedOwner.CharacterObj.CharID;
                if (useDataFrom.ContainsKey(id))
                {
                    if (selectedOwner.Corp)
                    {
                        foreach (EMMADataSet.WalletDivisionsRow wallet in selectedOwner.CharacterObj.WalletDivisions)
                        {
                            chkWallets.Items.Add(wallet.ID + (wallet.ID == 1000 ? " (Master)" : "") + 
                                " - " + wallet.Name, useDataFrom[id].Contains(wallet.ID));
                        }
                        chkWallets.Enabled = true;
                    }
                    else
                    {
                        chkWallets.Items.Add("Main Wallet", true);
                    }
                }
            }
        }

        private void SetupReportTypes()
        {
            types = new List<ReportType>();

            types.Add(new ReportType(new IncomeStatement(), "Provides a breakdown of your revenue and expenses" +
                " by journal entry type over the specified period.", new RptParamsIncomeStatement(), true, false,
                APIDataType.Journal));
            types.Add(new ReportType(new ItemReport(false), "Provides various financial metrics broken down by item" +
                " over the specified period.", new RptParamsItems(), true, true, APIDataType.Full));
            types.Add(new ReportType(new AssetsReport(false), "Provides financial details of total assets held",
                new RptParamsAssets(), true, true, APIDataType.Assets));
            types.Add(new ReportType(new SharesReport(), "Provides financial details of past and present investments" +
                " in public corporations, banks, etc.", new RptParamsShareReport(), false, false,
                APIDataType.Unknown));
            types.Add(new ReportType(new NAVReport(), "Display a breakdown of the selected corp's & character's" +
                " total ISK value", new RptParamsNAVReport(), true, false, APIDataType.Full));
            /*types.Add(new ReportType(new ItemDetailReport(), "Provides various financial metrics broken down by time" +
                " for the specified item or group of items.", new RptParamsItemDetail(), true));*/
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (useDataFrom.Count > 0)
            {
                parameters = new Dictionary<string, object>();
                List<FinanceAccessParams> finParams = new List<FinanceAccessParams>();
                List<AssetAccessParams> assetParams = new List<AssetAccessParams>();

                this.Visible = false;
                this.DialogResult = DialogResult.Cancel;

                ReportType thisType = (ReportType)cmbReportTypes.SelectedItem;

                if (thisType.NeedFinanceAccessParams || paramFormToRun.NeedFinanceParams)
                {
                    Dictionary<int, List<int>>.Enumerator enumerator = useDataFrom.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        // If we are accessing all wallets for a corp then no need to bether with this 
                        // as the default is to access everything when a blank list is supplied.
                        List<short> walletIDs = new List<short>();
                        if (enumerator.Current.Value.Count < 7)
                        {
                            foreach (short walletId in enumerator.Current.Value)
                            {
                                walletIDs.Add(walletId);
                            }
                        }

                        finParams.Add(new FinanceAccessParams(enumerator.Current.Key, walletIDs));
                    }
                    parameters.Add("FinanceAccessParams", finParams);
                    paramFormToRun.Parameters = parameters;
                    if (paramFormToRun.NeedFinanceParams) { paramFormToRun.FinanceParams = finParams; }
                }
                if (thisType.NeedAssetAccessParams || paramFormToRun.NeedAssetParams)
                {
                    foreach (object item in lstOwners.CheckedItems)
                    {
                        bool done = false;
                        CharCorpOption owner = (CharCorpOption)item;

                        foreach (AssetAccessParams character in assetParams)
                        {
                            if (character.OwnerID == owner.Data.ID)
                            {
                                done = true;
                            }
                        }

                        if (!done)
                        {
                            assetParams.Add(new AssetAccessParams(owner.Data.ID));
                        }
                    }
                    parameters.Add("AssetAccessParams", assetParams);
                    paramFormToRun.Parameters = parameters;
                    if (paramFormToRun.NeedAssetParams) { paramFormToRun.AssetParams = assetParams; }
                }

                // Go off to appropriate parameter form now.
                if (paramFormToRun.ShowDialog() != DialogResult.Cancel)
                {
                    parameters = paramFormToRun.Parameters;
                    reportToRun = paramFormToRun.Report;

                    Thread t1 = new Thread(BuildReport);
                    t1.SetApartmentState(ApartmentState.STA);
                    ProgressDialog prgDialog = new ProgressDialog("Building Report...", reportToRun);
                    Thread.Sleep(100);
                    t1.Start();
                    if (prgDialog.ShowDialog() == DialogResult.OK)
                    {
                        ShowReport(reportToRun, showAmountsIn);
                        this.DialogResult = DialogResult.OK;
                    }
                    else
                    {
                        // Stop our worker thread if the user has cancelled out of the progress dialog.
                        t1.Abort();
                        t1.Join();

                        MessageBox.Show("Report creation was cancelled.",
                            "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                this.Close();
            }
            else
            {
                MessageBox.Show("No characters or corporations selected", "Warning", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void cmbReportTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            ReportType type = (ReportType)cmbReportTypes.SelectedItem;

            if (type != null)
            {
                txtDescription.Text = type.Description;
                reportToRun = type.Report;
                paramFormToRun = type.ParameterForm;
                RefreshCharList();
            }
        }

        private void BuildReport()
        {
            try
            {
                reportToRun.CreateReport(parameters);
            }
            catch (ThreadAbortException)
            {
                // User has closed the progress dialog so just allow the execution to fall out of this loop.
            }
        }

        private void ShowReport(IReport report, IskMultiplier showValues)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                ViewReport viewRptForm = new ViewReport(report, showValues);
                viewRptForm.MdiParent = this.MdiParent;
                viewRptForm.Show();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        public IReport ReportToRun
        {
            get { return reportToRun; }
            set { reportToRun = value; }
        }

        public IskMultiplier ShowAmountsIn
        {
            get { return showAmountsIn; }
            set { showAmountsIn = value; }
        }

        private void cmbValuesIn_SelectedIndexChanged(object sender, EventArgs e)
        {
            showAmountsIn = (IskMultiplier)Enum.Parse(Type.GetType("EveMarketMonitorApp.Reporting.IskMultiplier"),
                cmbValuesIn.Text);
        }

        private void lstOwners_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lastSelectedOwner != null)
            {
                if (lastSelectedOwner.Corp && !lastSelectedOwner.Equals((CharCorpOption)lstOwners.SelectedItem))
                {
                    int id = lastSelectedOwner.Corp ? 
                        lastSelectedOwner.CharacterObj.CorpID : lastSelectedOwner.CharacterObj.CharID;

                    if (useDataFrom.ContainsKey(id))
                    {
                        useDataFrom.Remove(id);
                    }

                    if (chkWallets.CheckedItems.Count > 0)
                    {
                        useDataFrom.Add(id, new List<int>());
                        foreach (object wallet in chkWallets.CheckedItems)
                        {
                            string walletText = wallet.ToString();
                            int walletID = int.Parse(walletText.Remove(walletText.IndexOf(" ")));
                            useDataFrom[id].Add(walletID);
                        }
                    }
                }
            }

            lastSelectedOwner = (CharCorpOption)lstOwners.SelectedItem;
            RefreshWalletList();
        }

        private void lstOwners_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            CharCorpOption item = (CharCorpOption)lstOwners.Items[e.Index];
            if (item != null)
            {
                int id = item.Corp ? item.CharacterObj.CorpID : item.CharacterObj.CharID;
                if (e.NewValue == CheckState.Unchecked && useDataFrom.ContainsKey(id))
                {
                    useDataFrom.Remove(id);
                }
                if (e.NewValue == CheckState.Checked && !useDataFrom.ContainsKey(id))
                {
                    useDataFrom.Add(id, new List<int>());
                    if (item.Corp)
                    {
                        int[] walletIDs = { 1000, 1001, 1002, 1003, 1004, 1005, 1006 };
                        useDataFrom[id].AddRange(walletIDs);
                    }
                    else
                    {
                        useDataFrom[id].Add(1000);
                    }
                }
                RefreshWalletList();
            }
        }

        private void btnToggleAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < lstOwners.Items.Count; i++)
            {
                lstOwners.SetItemChecked(i, _selectAll);
            }

            _selectAll = !_selectAll;
            RefreshWalletList();
        }


    }


    public class ReportType
    {
        private IReport _report;
        private string _name;
        private string _description;
        private RptParamsBase _parameterForm;
        private bool _needFinanceAccessParams;
        private bool _needAssetAccessParams;
        private APIDataType _accessType;

        public ReportType(IReport report, string description, RptParamsBase parameterForm,
            bool needFinanceAccessParams, bool needAssetAccessParams, APIDataType accessType)
        {
            _report = report;
            _name = report.GetTitle();
            _description = description;
            _parameterForm = parameterForm;
            _needFinanceAccessParams = needFinanceAccessParams;
            _needAssetAccessParams = needAssetAccessParams;
            _accessType = accessType;
        }

        public IReport Report
        {
            get { return _report; }
            set { _report = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public RptParamsBase ParameterForm
        {
            get { return _parameterForm; }
            set { _parameterForm = value; }
        }
        public bool NeedAssetAccessParams
        {
            get { return _needAssetAccessParams; }
            set { _needAssetAccessParams = value; }
        }
        public bool NeedFinanceAccessParams
        {
            get { return _needFinanceAccessParams; }
            set { _needFinanceAccessParams = value; }
        }
        public APIDataType ApiAccessType
        {
            get { return _accessType; }
            set { _accessType = value; }
        }

        public override string ToString()
        {
            return _name;
        }
    }


        
}