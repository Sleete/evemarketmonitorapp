using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.Reporting;
using EveMarketMonitorApp.AbstractionClasses;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class ViewContracts : Form
    {
        private ContractViewMode _mode;
        private static ContractList _contracts;
        private static AutoContractor _autocon;
        private BindingSource _contractsBindingSource;
        private List<int> _owners;

        private string _lastPickup = "", _lastDest = "";

        private List<string> _recentStations;

        public ViewContracts()
        {
            InitializeComponent();
            _mode = ContractViewMode.ViewContracts;
            _autocon = new AutoContractor();
            UserAccount.Settings.GetFormSizeLoc(this);
            contractsGrid.Tag = "Contracts Data";
            if (Globals.calculator != null)
            {
                Globals.calculator.BindGrid(contractsGrid);
            }
        }

        private void Contracts_Load(object sender, EventArgs e)
        {
            try
            {
                EMMADataSet.ContractTypeDataTable types = ContractTypes.GetAll();
                // Remove the 'Cargo' type.
                // This is used by the Delivery  planner and we don't want users either viewing
                // or creating that type of contract.
                /*DataRow[] cargoType = types.Select("ID = 3");
                if (cargoType != null && cargoType.Length > 0)
                {
                    types.RemoveContractTypeRow((EMMADataSet.ContractTypeRow)cargoType[0]);
                }*/
                cmbType.DisplayMember = "Description";
                cmbType.ValueMember = "ID";
                cmbType.DataSource = types;
                cmbType.SelectedValue = (short)ContractType.Courier;
                cmbType.SelectedIndexChanged += new EventHandler(cmbType_SelectedIndexChanged);

                _contracts = new ContractList();
                _contractsBindingSource = new BindingSource();
                _contractsBindingSource.DataSource = _contracts;

                DataGridViewCellStyle iskStyle = new DataGridViewCellStyle(RewardColumn.DefaultCellStyle);
                iskStyle.Format = IskAmount.FormatString();
                RewardColumn.DefaultCellStyle = iskStyle;
                CollateralColumn.DefaultCellStyle = iskStyle;
                ExpectedProfitColumn.DefaultCellStyle = iskStyle;

                contractsGrid.AutoGenerateColumns = false;
                contractsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                contractsGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

                contractsGrid.DataSource = _contractsBindingSource;
                ContractIDColumn.DataPropertyName = "ID";
                OwnerColumn.DataPropertyName = "Owner";
                IssueDateColumn.DataPropertyName = "IssueDate";
                PickupStationColumn.DataPropertyName = "PickupStation";
                DestinationStationColumn.DataPropertyName = "DestinationStation";
                RewardColumn.DataPropertyName = "Reward";
                CollateralColumn.DataPropertyName = "Collateral";
                ExpectedProfitColumn.DataPropertyName = "ExpectedProfit";
                StatusColumn.DataPropertyName = "Status";
                CompletedColumn.Image = icons.Images["tick.gif"];
                FailedColumn.Image = icons.Images["cross.gif"];
                ExpiredColumn.Image = icons.Images["expired.gif"];
                contractsGrid.CellClick += new DataGridViewCellEventHandler(contractsGrid_CellClick);
                contractsGrid.CellDoubleClick += new DataGridViewCellEventHandler(contractsGrid_CellDoubleClick);
                UserAccount.Settings.GetColumnWidths(this.Name, contractsGrid);

                List<CharCorpOption> charcorps = UserAccount.CurrentGroup.GetCharCorpOptions();
                _owners = new List<int>();
                foreach (CharCorpOption chop in charcorps)
                {
                    _owners.Add(chop.Corp ? chop.CharacterObj.CorpID : chop.CharacterObj.CharID);
                }
                cmbOwner.DisplayMember = "Name";
                cmbOwner.ValueMember = "Data";
                charcorps.Sort();
                cmbOwner.DataSource = charcorps;
                cmbOwner.SelectedValue = 0;
                cmbOwner.Enabled = false;
                cmbOwner.SelectedIndexChanged += new EventHandler(cmbOwner_SelectedIndexChanged);
                chkIngoreOwner.Checked = true;
                chkIngoreOwner.CheckedChanged += new EventHandler(chkIngoreOwner_CheckedChanged);

                _recentStations = UserAccount.CurrentGroup.Settings.RecentStations;
                _recentStations.Sort();
                cmbDestination.Tag = 0;
                cmbDestination.Items.AddRange(_recentStations.ToArray());
                cmbDestination.AutoCompleteSource = AutoCompleteSource.ListItems;
                cmbDestination.AutoCompleteMode = AutoCompleteMode.Suggest;
                cmbDestination.KeyDown += new KeyEventHandler(cmbDestination_KeyDown);
                cmbDestination.SelectedIndexChanged += new EventHandler(cmbDestination_SelectedIndexChanged);

                cmbPickup.Tag = 0;
                cmbPickup.Items.AddRange(_recentStations.ToArray());
                cmbPickup.AutoCompleteSource = AutoCompleteSource.ListItems;
                cmbPickup.AutoCompleteMode = AutoCompleteMode.Suggest;
                cmbPickup.KeyDown += new KeyEventHandler(cmbPickup_KeyDown);
                cmbPickup.SelectedIndexChanged += new EventHandler(cmbPickup_SelectedIndexChanged);

                EMMADataSet.ContractStatesDataTable states = ContractStatus.GetAll();
                BindingSource statusSource = new BindingSource();
                statusSource.DataSource = states;
                statusSource.Sort = "Description";
                cmbStatus.DisplayMember = "Description";
                cmbStatus.ValueMember = "ID";
                cmbStatus.DataSource = statusSource;
                cmbStatus.SelectedValue = 1;
                cmbStatus.SelectedIndexChanged += new EventHandler(cmbStatus_SelectedIndexChanged);

                this.FormClosing += new FormClosingEventHandler(ViewContracts_FormClosing);
                RefreshGUI();
                DisplayContracts();
            }
            catch (Exception ex)
            {
                // Creating new EMMAexception will cause error to be logged.
                EMMAException emmaex = ex as EMMAException;
                if (emmaex == null)
                {
                    emmaex = new EMMAException(ExceptionSeverity.Critical, "Error setting up contracts form", ex);
                }
                MessageBox.Show("Problem setting up contracts view.\r\nCheck " + Globals.AppDataDir + "Logging\\ExceptionLog.txt" +
                    " for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbType.SelectedValue != null)
            {
                ContractType type = (ContractType)cmbType.SelectedValue;
                if (type == ContractType.Courier)
                {
                    _mode = ContractViewMode.ViewContracts;
                }
                else if (type == ContractType.ItemExchange)
                {
                    _mode = ContractViewMode.ItemExchangeContracts;
                }

                RefreshGUI();
                DisplayContracts();
            }
        }

        void ViewContracts_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (UserAccount.Settings != null)
            {
                UserAccount.Settings.StoreColumnWidths(this.Name, contractsGrid);
                UserAccount.Settings.StoreFormSizeLoc(this);
            }
            if (Globals.calculator != null)
            {
                Globals.calculator.RemoveGrid(contractsGrid);
            }
        }


        void cmbPickup_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                SetSelectedPickup();
            }
        }

        void cmbPickup_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetSelectedPickup();
        }

        private void SetSelectedPickup()
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (!cmbPickup.Text.Equals(_lastPickup))
                {
                    cmbPickup.Tag = 0;
                    if (!cmbPickup.Text.Equals(""))
                    {
                        try
                        {
                            EveDataSet.staStationsRow station = Stations.GetStation(cmbPickup.Text);
                            if (station != null)
                            {
                                cmbPickup.Tag = station.stationID;
                                string name = station.stationName;
                                cmbPickup.Text = name;
                                if (!cmbPickup.Items.Contains(name))
                                {
                                    cmbPickup.Items.Add(name);
                                    _recentStations.Add(name);
                                }
                            }
                        }
                        catch (EMMADataException) { }

                        _lastPickup = cmbPickup.Text;
                        DisplayContracts();
                    }
                    if ((int)cmbPickup.Tag == 0) { cmbPickup.Text = ""; }
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        void cmbDestination_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                SetSelectedDest();
            }
        }

        void cmbDestination_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetSelectedDest();
        }

        private void SetSelectedDest()
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (!cmbDestination.Text.Equals(_lastDest))
                {
                    cmbDestination.Tag = 0;
                    if (!cmbDestination.Text.Equals(""))
                    {
                        try
                        {
                            EveDataSet.staStationsRow station = Stations.GetStation(cmbDestination.Text);
                            if (station != null)
                            {
                                cmbDestination.Tag = station.stationID;
                                string name = station.stationName;
                                cmbDestination.Text = name;
                                if (!cmbDestination.Items.Contains(name))
                                {
                                    cmbDestination.Items.Add(name);
                                    _recentStations.Add(name);
                                }
                            }
                        }
                        catch (EMMADataException) { }

                        _lastDest = cmbDestination.Text;
                        DisplayContracts();
                    }
                    if ((int)cmbDestination.Tag == 0) { cmbDestination.Text = ""; }
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        void chkIngoreOwner_CheckedChanged(object sender, EventArgs e)
        {
            cmbOwner.Enabled = !chkIngoreOwner.Checked;
            if (chkIngoreOwner.Checked)
            {
                DisplayContracts();
            }
        }

        void cmbOwner_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayContracts();
        }

        void cmbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayContracts();
        }

        public void RefreshGUI()
        {
            switch (_mode)
            {
                case ContractViewMode.ViewContracts:
                    btnAutoContractor.Text = "Auto-contractor";
                    btnAutoContractor.Enabled = true;
                    this.Text = "Courier Contracts";
                    grpFilters.Enabled = true;
                    cmbDestination.Enabled = true;
                    lblDestStation.Enabled = true;
                    lblPickupStation.Text = "Pickup Station";
                    cmbStatus.Enabled = true;
                    lblStatus.Enabled = true;
                    break;
                case ContractViewMode.AutoContractor:
                    btnAutoContractor.Text = "Recalc All";
                    btnAutoContractor.Enabled = true;
                    this.Text = "Auto-contractor";
                    grpFilters.Enabled = false;
                    cmbDestination.Enabled = true;
                    lblDestStation.Enabled = true;
                    lblPickupStation.Text = "Pickup Station";
                    cmbStatus.Enabled = true;
                    lblStatus.Enabled = true;
                    break;
                case ContractViewMode.ItemExchangeContracts:
                    btnAutoContractor.Text = "Auto-contractor";
                    btnAutoContractor.Enabled = false;
                    this.Text = "Item Exchange Contracts";
                    grpFilters.Enabled = true;
                    cmbDestination.Enabled = false;
                    lblDestStation.Enabled = false;
                    lblPickupStation.Text = "Station";
                    cmbStatus.Enabled = false;
                    lblStatus.Enabled = false;
                    break;
                default:
                    break;
            }
        }

        public void DisplayContracts()
        {

            switch (_mode)
            {
                case ContractViewMode.ViewContracts:
                    RefreshList();
                    break;
                case ContractViewMode.AutoContractor:
                    RefreshAutoCon();
                    break;
                case ContractViewMode.ItemExchangeContracts:
                    RefreshList();
                    break;
                default:
                    break;
            }

        }

        private void RefreshAutoCon()
        {
            CompletedColumn.Visible = false;
            ExpiredColumn.Visible = false;
            FailedColumn.Visible = false;
        }

        private void RefreshList()
        {
            List<int> ownerIDs = new List<int>();
            int pickupStation = 0, destinationStation = 0;
            short status = 0;
            ContractType type = ContractType.Any;

            if(cmbType.SelectedValue != null) 
            {
                type = (ContractType)cmbType.SelectedValue;
            }
            if (cmbOwner.SelectedValue != null && !chkIngoreOwner.Checked)
            {
                CharCorp data = (CharCorp)cmbOwner.SelectedValue;
                ownerIDs.Add(data.corp ? data.characterObj.CorpID : data.characterObj.CharID);
            }
            if (ownerIDs.Count == 0)
            {
                ownerIDs = _owners;
            }

            pickupStation = int.Parse(cmbPickup.Tag.ToString());
            destinationStation = int.Parse(cmbDestination.Tag.ToString());
            if (cmbStatus.SelectedValue != null) { status = (short)cmbStatus.SelectedValue; }

            CompletedColumn.Visible = (type == ContractType.Courier && status == 1);
            ExpiredColumn.Visible = (type == ContractType.Courier && status == 1);
            FailedColumn.Visible = (type == ContractType.Courier && status == 1);
            DestinationStationColumn.Visible = type == ContractType.Courier;
            StatusColumn.Visible = type == ContractType.Courier;
            RewardColumn.Visible = type == ContractType.Courier;
            ExpectedProfitColumn.Visible = type == ContractType.Courier;
            PickupStationColumn.HeaderText = type == ContractType.Courier ? "Pickup Station" : "Station";
            CollateralColumn.HeaderText = type == ContractType.Courier ? "Collateral" : "Price";

            //ListSortDirection sortDirection = ListSortDirection.Descending;
            //DataGridViewColumn sortColumn = contractsGrid.SortedColumn;
            //if (contractsGrid.SortOrder == SortOrder.Ascending) sortDirection = ListSortDirection.Ascending;
            List<SortInfo> sortinfo = contractsGrid.GridSortInfo;

            _contracts = Contracts.GetContracts(ownerIDs, status, pickupStation, destinationStation, type);
            _contractsBindingSource.DataSource = _contracts;

            //if (sortColumn != null)
            //{
            //    contractsGrid.Sort(sortColumn, sortDirection);
            //}
            //else
            //{
            //    contractsGrid.Sort(IssueDateColumn, ListSortDirection.Descending);
            //}
            if (sortinfo.Count == 0)
            {
                DataGridViewColumn column = contractsGrid.Columns["IssueDateColumn"];
                sortinfo.Add(new SortInfo(column.Index, column.DataPropertyName));
            }
            contractsGrid.GridSortInfo = sortinfo;

            Text = "Viewing " + _contractsBindingSource.Count + " contracts";
        }

        private void RecalculateAutoCon()
        {
            SortedList<object, string> options = new SortedList<object, string>();
            List<CharCorpOption> charcorps = UserAccount.CurrentGroup.GetCharCorpOptions(APIDataType.Assets);
            charcorps.Sort();
            foreach (CharCorpOption opt in charcorps)
            {
                options.Add(opt.Corp ? opt.CharacterObj.CorpID : opt.CharacterObj.CharID, opt.Name);
            }
            OptionPicker picker = new OptionPicker("Select Owner", "Select a character or corporation to generate" +
                " courier contracts for.", options);
            if (picker.ShowDialog() == DialogResult.OK)
            {
                ProgressDialog progress = new ProgressDialog("Generating Contracts...", _autocon);
                object param = picker.SelectedItem;
                Thread t0 = new Thread(new ParameterizedThreadStart(AutoCon));
                t0.SetApartmentState(ApartmentState.STA);
                t0.Start(param);
                DialogResult result = progress.ShowDialog();
                if (result == DialogResult.OK)
                {
                    _contractsBindingSource.DataSource = _contracts;

                    Text = "Viewing " + _contractsBindingSource.Count + " contracts";
                }
                else if (result == DialogResult.Cancel)
                {
                    // Stop our worker thread if the user has cancelled out of the progress dialog.
                    t0.Abort();
                    t0.Join();

                    MessageBox.Show("Auto-contractor was cancelled, the contracts window will now close.",
                        "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
            }

        }

        private void AutoCon(object ownerID)
        {
            int id = (int)ownerID;
            _contracts = _autocon.GenerateContracts(id);
        }

        private void btnNewContract_Click(object sender, EventArgs e)
        {
            ContractType type = ContractType.Any;
            if(cmbType.SelectedValue != null) 
            {
                type = (ContractType)cmbType.SelectedValue;
            }

            SortedList<object, string> options = new SortedList<object, string>();
            List<CharCorpOption> charcorps = UserAccount.CurrentGroup.GetCharCorpOptions(APIDataType.Assets);
            charcorps.Sort();
            foreach (CharCorpOption opt in charcorps)
            {
                options.Add(opt.Corp ? opt.CharacterObj.CorpID : opt.CharacterObj.CharID, opt.Name);
            }
            OptionPicker picker = new OptionPicker("Select Owner", "Select a character or corporation to create" +
                " a contract for.", options);
            if (picker.ShowDialog() == DialogResult.OK)
            {
                Cursor = Cursors.WaitCursor;
                try
                {
                    CourierCalc courierCalc = new CourierCalc((int)picker.SelectedItem, type);
                    courierCalc.ShowDialog();
                    RefreshList();
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private void btnAutoContractor_Click(object sender, EventArgs e)
        {
            _mode = ContractViewMode.AutoContractor;
            RefreshGUI();
            RecalculateAutoCon();
            DisplayContracts();
        }

        private void btnCourierSettings_Click(object sender, EventArgs e)
        {
            CourierSettings settings = new CourierSettings();
            settings.ShowDialog();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        void contractsGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                Contract contract = (Contract)contractsGrid.Rows[e.RowIndex].DataBoundItem;
                if (contractsGrid.Columns[e.ColumnIndex].Name.Equals("CompletedColumn"))
                {
                    SetContractState(contract, NewContractState.Completed);
                }
                if (contractsGrid.Columns[e.ColumnIndex].Name.Equals("FailedColumn"))
                {
                    SetContractState(contract, NewContractState.Failed);
                }
                if (contractsGrid.Columns[e.ColumnIndex].Name.Equals("ExpiredColumn"))
                {
                    SetContractState(contract, NewContractState.Expired);
                }
            }
        }

        private void SetContractState(Contract contract, NewContractState newState)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                switch (newState)
                {
                    case NewContractState.Completed:
                        Contracts.CompleteContract(contract);
                        break;
                    case NewContractState.Failed:
                        Contracts.FailContract(contract);
                        break;
                    case NewContractState.Expired:
                        Contracts.ContractExpired(contract);
                        break;
                    default:
                        break;
                }
                RefreshList();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        void contractsGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                Cursor = Cursors.WaitCursor;
                try
                {
                    CourierCalc calc = new CourierCalc((Contract)contractsGrid.Rows[e.RowIndex].DataBoundItem,
                        _mode != ContractViewMode.AutoContractor);
                    calc.ShowDialog();
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private void contractsGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (_mode == ContractViewMode.ItemExchangeContracts)
            {
                if (e.KeyCode == Keys.Delete)
                {
                    List<Contract> contractsToRemove = new List<Contract>();
                    foreach (DataGridViewRow row in contractsGrid.SelectedRows)
                    {
                        contractsToRemove.Add((Contract)row.DataBoundItem);
                    }

                    foreach (Contract contract in contractsToRemove)
                    {
                        Contracts.Delete(contract, false);
                    }
                    RefreshList();
                }
            }
        }



        private enum ContractViewMode
        {
            ViewContracts,
            AutoContractor,
            ItemExchangeContracts
        }

        private enum NewContractState
        {
            Completed,
            Failed,
            Expired
        }


    }
}