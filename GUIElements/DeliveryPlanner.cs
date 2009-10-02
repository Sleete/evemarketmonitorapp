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
using EveMarketMonitorApp.AbstractionClasses;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class DeliveryPlanner : Form
    {
        private List<string> _recentSystems;
        private string _lastSystem = "";
        private short[,] _jumps;
        private Dictionary<int, int> _idMapper;
        private int _nextFreeIndex = 0;
        private string _lastStartSystem = "";
        private string _lastEndSystem = "";
        private CargoHoldSpec _cargoHold;

        private ContractList _cargo = new ContractList();
        private BindingSource _cargoBindingSource = new BindingSource();

        public DeliveryPlanner()
        {
            InitializeComponent();

            UserAccount.Settings.GetFormSizeLoc(this);
            _cargoHold = new CargoHoldSpec();
        }
        
        private void RoutePlanner_Load(object sender, EventArgs e)
        {
            try
            {
                _recentSystems = UserAccount.CurrentGroup.Settings.RecentSystems;
                _recentSystems.Sort();
                AutoCompleteStringCollection systems = new AutoCompleteStringCollection();
                systems.AddRange(_recentSystems.ToArray());

                txtStartSystem.AutoCompleteCustomSource = systems;
                txtStartSystem.AutoCompleteSource = AutoCompleteSource.CustomSource;
                txtStartSystem.AutoCompleteMode = AutoCompleteMode.Suggest;
                txtStartSystem.Leave += new EventHandler(txtSystem_Leave);
                txtStartSystem.KeyDown += new KeyEventHandler(txtSystem_KeyDown);
                txtStartSystem.Tag = 0;
                txtStartSystem.Text = "";

                List<CharCorpOption> charcorps = UserAccount.CurrentGroup.GetCharCorpOptions(APIDataType.Assets);
                charcorps.Sort();
                cmbOwner.DisplayMember = "Name";
                cmbOwner.ValueMember = "Data";
                cmbOwner.DataSource = charcorps;
                if (charcorps.Count > 0)
                {
                    cmbOwner.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                // Creating new EMMAexception will cause error to be logged.
                EMMAException emmaex = ex as EMMAException;
                if (emmaex == null)
                {
                    emmaex = new EMMAException(ExceptionSeverity.Critical, "Error setting up delivery planner", ex);
                }
                MessageBox.Show("Problem setting up delivery planner.\r\nCheck \\Logging\\ExceptionLog.txt" +
                    " for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        void txtSystem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Enter)
            {
                TextBox textBox = sender as TextBox;
                if (textBox != null)
                {
                    SetSelectedSystem(textBox);
                }
            }
        }

        void txtSystem_Leave(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                SetSelectedSystem(textBox);
            }
        }

        private void SetSelectedSystem(TextBox field)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                string lastSystem = "";
                if (field == txtStartSystem) { lastSystem = _lastStartSystem; }

                if (!field.Text.Equals(lastSystem))
                {
                    field.Tag = 0;
                    if (!field.Text.Equals(""))
                    {
                        try
                        {
                            EveDataSet.mapSolarSystemsRow system = SolarSystems.GetSystem(field.Text);
                            if (system != null)
                            {
                                field.Tag = system.solarSystemID;
                                string name = system.solarSystemName;
                                field.Text = name;
                                if (!_recentSystems.Contains(name))
                                {
                                    _recentSystems.Add(name);
                                    // Only need to add this to the auto complete source for this field because
                                    // all the text boxes share the same auto complete source object.
                                    field.AutoCompleteCustomSource.Add(name);
                                }
                            }
                        }
                        catch (EMMADataException) { }

                        if (field == txtStartSystem) {  _lastStartSystem = field.Text; }
                    }
                    if ((int)field.Tag == 0) { field.Text = ""; }
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void RoutePlanner_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (UserAccount.Settings != null)
            {
                if (UserAccount.CurrentGroup != null)
                {
                    UserAccount.CurrentGroup.Settings.RecentSystems = _recentSystems;
                }
                UserAccount.Settings.StoreFormSizeLoc(this);
            }
        }


        private void cargoDataView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                List<Contract> contractsToRemove = new List<Contract>();
                foreach (DataGridViewRow row in cargoDataView.SelectedRows)
                {
                    Contract toRemove = row.DataBoundItem as Contract;
                    if (toRemove != null)
                    {
                        contractsToRemove.Add(toRemove);
                    }
                }
                RemoveCargoPickup(contractsToRemove);

            }
        }

        private void RemoveCargoPickup(List<Contract> contracts)
        {
            if (contracts.Count > 0)
            {
                foreach (Contract contract in contracts)
                {
                    _cargo.Remove(contract);
                }
                RefreshCargoList();
            }
        }

        private void btnAddAssets_Click(object sender, EventArgs e)
        {
            int ownerID = 0;

            if (cmbOwner.SelectedItem != null)
            {
                ownerID = ((CharCorpOption)cmbOwner.SelectedItem).Data.ID;
            }

            if (ownerID != 0)
            {
                AutoContractor contractor = new AutoContractor();
                ContractList contracts = contractor.GenerateContracts(ownerID);

                foreach (Contract contract in contracts)
                {
                    _cargo.Add(contract);
                }
            }
            else
            {
                MessageBox.Show("Please select an assets owner to generate cargo pickup points for");
            }
        }

        private void btnCourierSettings_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                CourierSettings settings = new CourierSettings();
                settings.ShowDialog();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void cargoDataView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                Cursor = Cursors.WaitCursor;
                try
                {
                    CourierCalc calc = new CourierCalc((Contract)cargoDataView.Rows[e.RowIndex].DataBoundItem, false);
                    calc.ShowDialog();
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private void btnGenRoute_Click(object sender, EventArgs e)
        {
            CargoRoute route;

            int startSystemID = (int)txtStartSystem.Tag;

            if (startSystemID == 0)
            {
                MessageBox.Show("You must specify a start system",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (_cargo.Count < 3)
            {
                MessageBox.Show("You must have at least 3 pickup points specified or there is nothing to optimise.",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (_cargoHold.Containers.Count <= 0)
            {
                MessageBox.Show("You must specifiy at least one container or you will not be able to hold any cargo!",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                Cursor = Cursors.WaitCursor;
                try
                {
                    _nextFreeIndex = 0;
                    _idMapper = new Dictionary<int, int>();
                    _jumps = new short[_cargo.Count + 2, _cargo.Count + 2];

                    List<int> waypoints = new List<int>();
                    waypoints.Add(startSystemID);

                    route = new CargoRoute(waypoints, ref _nextFreeIndex, ref _jumps, _idMapper, _cargo, _cargoHold);

                    Thread t0 = new Thread(new ThreadStart(route.Optimize));
                    ProgressDialog prg = new ProgressDialog("Generating route", route);
                    t0.SetApartmentState(ApartmentState.STA);
                    t0.Start();
                    if (prg.ShowDialog() == DialogResult.OK)
                    {
                        lstRoute.BeginUpdate();
                        lstRoute.Items.Clear();
                        lstRoute.Items.AddRange(route.GetCompleteRoute().ToArray());
                        lstRoute.EndUpdate();
                    }
                    else
                    {
                        lstRoute.Items.Clear();
                        t0.Abort();
                        t0.Join();

                        if (MessageBox.Show("Route optimizer was cancelled. Do you wish to see the best route" +
                            " so far? (" + route.GetLength() + " jumps)", "Notification",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                        {
                            lstRoute.Items.Clear();
                            lstRoute.Items.AddRange(route.GetCompleteRoute().ToArray());
                            lstRoute.EndUpdate();
                        }
                    }
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private void lstRoute_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            LocationData system = (LocationData)lstRoute.Items[e.Index];
            SizeF size = e.Graphics.MeasureString(system.ToString(), lstRoute.Font);
            e.ItemHeight = (int)size.Height;
            e.ItemWidth = (int)size.Width;
        }

        private void lstRoute_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.DrawFocusRectangle();

            if (e.Index > -1 && e.Index < lstRoute.Items.Count)
            {
                LocationData system = (LocationData)lstRoute.Items[e.Index];
                system.Draw(e.Graphics, e.Bounds, e.Font);
            }
        }
        

        private void btnAutopilotSettings_Click(object sender, EventArgs e)
        {
            RouteCalcSettings settings = new RouteCalcSettings();
            settings.ShowDialog();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            _cargo = new ContractList();
            RefreshCargoList();
        }

        private void btnAddCargo_Click(object sender, EventArgs e)
        {
            // Only need an owner if we're going to be storing the cargo information in the database at
            // some point. At the moment, that won't happen.
            /*int ownerID = 0;
            bool corp = false;

            if (cmbOwner.SelectedItem != null)
            {
                ownerID = ((CharCorpOption)cmbOwner.SelectedItem).CharacterObj.CharID;
                corp = ((CharCorpOption)cmbOwner.SelectedItem).Corp;
            }
            else 
            {
                MessageBox.Show("Please choose a character from the 'Owner of assets to use' dropdown list.\r\n" +
                    "This will be the character that the cargo contract is stored against.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }*/


            CourierCalc calc = new CourierCalc(0, ContractType.Cargo);
            if (calc.ShowDialog() == DialogResult.OK)
            {
                Contract newContract = calc.FinalContract;
                _cargo.Add(newContract);
                RefreshCargoList();
            }

        }


        private void btnAddContainer_Click(object sender, EventArgs e)
        {
            long volume = 0;
            try
            {
                volume = long.Parse(txtVolume.Text);
                if (volume < 0) { throw new Exception(); }

                _cargoHold.AddContainer(volume);
            }
            catch
            {
                MessageBox.Show("Invalid volume entered. Please enter a positive whole number", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void btnClearCotnainers_Click(object sender, EventArgs e)
        {
            _cargoHold = new CargoHoldSpec();
            RefreshContainerList();
        }


        private void RefreshCargoList()
        {
            if (_cargoBindingSource == null) { _cargoBindingSource = new BindingSource(); }
            _cargoBindingSource.DataSource = _cargo;

            if (cargoDataView.DataSource == null)
            {
                cargoDataView.AutoGenerateColumns = false;
                cargoDataView.DataSource = _cargoBindingSource;
                cargoDestinationColumn.DataPropertyName = "DestinationStation";
                cargoPickupColumn.DataPropertyName = "PickupStation";
                cargoVolumeColumn.DataPropertyName = "TotalVolume";
            }

            ListSortDirection sortDirection;
            if (cargoDataView.SortOrder == SortOrder.Descending) { sortDirection = ListSortDirection.Descending; }
            else { sortDirection = ListSortDirection.Ascending; }

            if (cargoDataView.SortedColumn == null)
            {
                cargoDataView.Sort(cargoPickupColumn, sortDirection);
            }
            else
            {
                cargoDataView.Sort(cargoDataView.SortedColumn, sortDirection);
            }

            cargoDataView.Refresh();
            cargoDataView.ClearSelection();
        }

        private void RefreshContainerList()
        {
            lstCargoHold.Items.Clear();
            foreach (CargoContainer container in _cargoHold.Containers)
            {
                lstCargoHold.Items.Add(container.TotalVolume + " m3");
            }
        }

        #region Inner Classes
        private class CargoHoldSpec
        {
            private List<CargoContainer> _containers;

            public CargoHoldSpec()
            {
                _containers = new List<CargoContainer>();
            }

            public void ClearAll() 
            {
                foreach (CargoContainer container in _containers)
                {
                    container.Clear();
                }
            }

            public void AddContainer(long volume)
            {
                _containers.Add(new CargoContainer(volume));
            }

            public List<CargoContainer> Containers
            {
                get { return _containers; }
            }
        }

        private class CargoContainer
        {
            private long _totalVolume;
            private List<ContractItem> _items;

            public CargoContainer(long volume)
            {
                _totalVolume = volume;
                _items = new List<ContractItem>();
            }

            public void Clear()
            {
                _items.Clear();
            }

            public long TotalVolume
            {
                get { return _totalVolume; }
                set { _totalVolume = value; }
            }
        }

        private class LocationData
        {
            public bool _isAStation;
            private int _stationID = 0;
            private int _systemID = 0;
            private string _systemName = "";
            private string _stationName = "";
            private bool _pickup;
            private bool _deliver;
            private float _security = -10;

            public LocationData(bool isAStation, int  id, string name, bool pickup, bool deliver)
            {
                _isAStation = isAStation;
                if (isAStation)
                {
                    _stationID = id;
                    _stationName = name;
                }
                else
                {
                    _systemID = id;
                    _systemName = name;
                }
                _pickup = pickup;
                _deliver = deliver;
            }

            public override string ToString()
            {
                return _stationName.Length == 0 ? _systemName : _stationName;
            }

            public int SystemID
            {
                get
                {
                    if (_systemID == 0 && _stationID != 0)
                    {
                        EveDataSet.staStationsRow station = Stations.GetStation(_stationID);
                        _systemID = station.solarSystemID;
                    }
                    return _systemID;
                }
                set
                {
                    _systemID = value;
                }
            }

            public string SystemName
            {
                get
                {
                    if (_systemName.Length == 0)
                    {
                        _systemName = SolarSystems.GetSystemName(SystemID);
                    }
                    return _systemName;
                }
                set
                {
                    _systemName = value;
                }
            }

            public int StationID
            {
                get
                {
                    return _stationID;
                }
                set
                {
                    _stationID = value;
                }
            }

            public string StationName
            {
                get
                {
                    if (_stationName.Length == 0 && _stationID != 0)
                    {
                        _stationName = Stations.GetStationName(_stationID);
                    }
                    return _stationName;
                }
                set
                {
                    _stationName = value;
                }
            }

            public float Security
            {
                get
                {
                    if (_security == -10)
                    {
                        _security = SolarSystems.GetSystemSecurity(SystemID);
                    }
                    return _security;
                }
            }

            public void Draw(Graphics g, Rectangle bounds, Font font)
            {
                string text = (_pickup ? ("PICKUP\t" + this.ToString() + "   (" + Math.Round(Security, 2) + ")") :
                    (_deliver ? ("DELIVER\t" + this.ToString() + "   (" + Math.Round(Security, 2) + ")") :
                    ("\t\t" + this.ToString() + "   (" + Math.Round(Security, 2) + ")")));
                Color col = Color.Green;
                if (Security > 0.45)
                {
                    col = Color.DarkGreen;
                }
                else if (Security > 0)
                {
                    col = Color.Orange;
                }
                else
                {
                    col = Color.Red;
                }
                g.DrawString(text, font, new SolidBrush(col), bounds);
            }
        }
        
        private class CargoRoute : List<int>, IProvideStatus
        {
            public event StatusChangeHandler StatusChange;
            private int _nextFreeIndex;
            private short[,] _jumps;
            // The ID Mapper matches a solar system ID to an index in the '_jumps' array.
            // This enables us to use an array, (which is much faster than other structures,) 
            // to cache jump distance data. 
            private Dictionary<int, int> _idMapper;
            private ContractList _contracts;
            private bool _initialised = false;
            private CargoHoldSpec _cargoHold;

            private List<int> _pickupStations;
            private List<int> _destinationStations;

            public CargoRoute(List<int> waypoints, ref int nextFreeIndex, ref short[,] jumps, 
                Dictionary<int, int> idMapper, ContractList contracts, CargoHoldSpec cargoHold) : base(waypoints)
            {
                _nextFreeIndex = nextFreeIndex;
                _jumps = jumps;
                _idMapper = idMapper;
                _contracts = contracts;
                _cargoHold = cargoHold;
                _destinationStations = new List<int>();
                _pickupStations = new List<int>();
            }


            /// <summary>
            /// 
            /// </summary>
            public void Optimize()
            {
                bool improvement = true;
                int generation = 0;
                int noImprovementGen = 0;
                int populationSize = Math.Min(this.Count, 50);
                List<CargoRoute> population = new List<CargoRoute>();
                CargoRoute bestRoute = null;
                // Just wait half a second to allow the caller to display the progress dialog.
                Thread.Sleep(500); 
                UpdateStatus(0, 1, "Optimizing Route", "", false);
                UpdateStatus(0, 1, "", "Pre-caching data", false);
                List<int> allSystems = new List<int>();
                foreach (Contract contract in _contracts)
                {
                    if (!_pickupStations.Contains(contract.PickupStationID))
                    {
                        _pickupStations.Add(contract.PickupStationID);
                    }
                    int system1 = Stations.GetStation(contract.PickupStationID).solarSystemID;
                    if (!allSystems.Contains(system1)) { allSystems.Add(system1); }
                    int system2 = Stations.GetStation(contract.DestinationStationID).solarSystemID;
                    if (!allSystems.Contains(system2)) { allSystems.Add(system2); }
                }
                SolarSystemDistances.PopulateJumpsArray(allSystems, allSystems, ref _jumps, _idMapper, 
                    ref _nextFreeIndex);
                allSystems.Clear();
                UpdateStatus(0, 1, "", "Calculating initial route length", false);
                UpdateStatus(0, 1, "", "Initial route length = " + GetLength() + " jumps", false);

                // Setup autopilot jump costs.
                Map.SetCosts(UserAccount.CurrentGroup.Settings.RouteHighSecWeight,
                    UserAccount.CurrentGroup.Settings.RouteLowSecWeight,
                    UserAccount.CurrentGroup.Settings.RouteNullSecWeight);

                try
                {
                    Random rand = new Random(DateTime.UtcNow.GetHashCode());

                    while (improvement)
                    {
                        int shortestLength = GetLength();
                        bestRoute = null;
                        improvement = false;
                        generation++;

                        int[] diagnostics = new int[4];

                        for (int r = 0; r < populationSize; r++)
                        {
                            // For each potential route in this population, we first start by creating a
                            // copy of the current route...
                            CargoRoute currentRoute = new CargoRoute(this, ref _nextFreeIndex, ref _jumps, _idMapper);
                            population.Add(currentRoute);

                            UpdateStatus(r, populationSize, "", "Processing generation " + generation, false);

                            // We then try some mutations to try and get a better (shorter) route.
                            for (int i = 0; i < 4; i++)
                            {
                                int oldLength, newLength;
                                // select a random system in the route (we'll call this system A) 
                                // Also select a random number of systems to after the selected system. 
                                // We'll call the system at the end of this sequence, system 'B'.
                                int startIndex = rand.Next(1, currentRoute.Count - 2);
                                int length = rand.Next(2, currentRoute.Count - startIndex);

                                oldLength = 0;
                                // Calculate the number of jumps in the route from the system before A to A
                                oldLength += GetRouteLength(currentRoute[startIndex - 1],
                                    currentRoute[startIndex], ref diagnostics);
                                // Add the number of jumps between B and the system after B
                                oldLength += GetRouteLength(currentRoute[startIndex + length - 1],
                                    currentRoute[startIndex + length], ref diagnostics);

                                // Reverse all the waypoints between A and B (inclusive)
                                currentRoute.Reverse(startIndex, length);

                                // When the section of the route is reversed, all the jumps between systems within
                                // that section will stay the same. Also, number of jumps between systems not 
                                // included in that section will stay the same.
                                // Therefore we only need to get the number of jumps between the systems at
                                // the edge of reversed section (A and B) and thier non-reversed neighbours. 
                                newLength = 0;
                                // Calculate the new number of jumps in the route from the system 
                                // before A's position to A's position
                                // (Note that system B is now in system A's old position)
                                newLength += GetRouteLength(currentRoute[startIndex - 1],
                                    currentRoute[startIndex], ref diagnostics);
                                // As before, add the number of jumps between B's position and the 
                                // position after B.
                                newLength += GetRouteLength(currentRoute[startIndex + length - 1],
                                    currentRoute[startIndex + length], ref diagnostics);

                                // If reversing the sequence of waypoints has given us a shorter route then keep
                                // it. Otherwise, discard it.
                                if (oldLength < newLength)
                                {
                                    currentRoute.Reverse(startIndex, length);
                                }
                            }

                            // If the mutations are better than the current best route then record it
                            if (currentRoute.GetLength() < shortestLength)
                            {
                                shortestLength = currentRoute.GetLength();
                                bestRoute = currentRoute;
                                improvement = true;
                            }
                        }

                        if (improvement)
                        {
                            // If there has been an improvement in this generation then convert the current
                            // route to the best route and start the process again.
                            this.Clear();
                            this.AddRange(bestRoute);
                            noImprovementGen = 0;

                            UpdateStatus(0, 0, "", "Route optimized to " + shortestLength +
                                " jumps", false);
                        }
                        else
                        {
                            // If we get no improvement for 6 consecutive generations then assume that
                            // we've got the best route we are going to get and leave it at that.
                            if (noImprovementGen == 0) { noImprovementGen = generation; }
                            if (generation == noImprovementGen + 6)
                            {
                                UpdateStatus(0, 0, "", "No further optimizations found", true);
                            }
                            else
                            {
                                improvement = true;
                            }
                        }

                        DiagnosticUpdate("", "  Fast cache hits = " + diagnostics[0]);
                        DiagnosticUpdate("", "  General cache hits = " + diagnostics[1]);
                        DiagnosticUpdate("", "  Database hits = " + diagnostics[2]);
                        DiagnosticUpdate("", "  Route calculations = " + diagnostics[3]);
                    }
                }
                catch (ThreadAbortException)
                {
                    this.Clear();
                    if (bestRoute != null)
                    {
                        this.AddRange(bestRoute);
                    }
                }
                catch (Exception ex)
                {
                    EMMAException emmaEx = ex as EMMAException;
                    if (emmaEx == null)
                    {
                        emmaEx = new EMMAException(ExceptionSeverity.Error, "Error optimising route", ex);
                    }
                    UpdateStatus(0, 0, "Error", ex.Message, true);
                }
            }

            private short GetRouteLength(int startSystemID, int endSystemID, ref int[] diagnostics)
            {
                short retVal = 0;

                if (startSystemID != endSystemID)
                {
                    if (startSystemID > endSystemID)
                    {
                        int tmp = startSystemID;
                        startSystemID = endSystemID;
                        endSystemID = tmp;
                    }
                    int startIndex = 0;
                    if (_idMapper.ContainsKey(startSystemID))
                    {
                        startIndex = _idMapper[startSystemID];
                    }
                    else
                    {
                        _idMapper.Add(startSystemID, _nextFreeIndex);
                        startIndex = _nextFreeIndex;
                        _nextFreeIndex++;
                    }

                    int endIndex = 0;
                    if (_idMapper.ContainsKey(endSystemID))
                    {
                        endIndex = _idMapper[endSystemID];
                    }
                    else
                    {
                        _idMapper.Add(endSystemID, _nextFreeIndex);
                        endIndex = _nextFreeIndex;
                        _nextFreeIndex++;
                    }

                    retVal = _jumps[startIndex, endIndex];

                    if (retVal == 0)
                    {
                        retVal = (short)Map.CalcRouteLength(startSystemID, endSystemID);
                        /*retVal = (short)SolarSystemDistances.GetDistance(startSystemID, endSystemID,
                            ref diagnostics);*/
                        _jumps[startIndex, endIndex] = retVal;
                    }
                    else
                    {
                        diagnostics[0]++;
                    }
                }

                return retVal;
            }

            public int GetLength()
            {
                if (!_initialised) { Initialise(); }
                int retVal = 0;
                int[] tmp = new int[4];
                for (int i = 0; i < this.Count - 1; i++)
                {
                    retVal += GetRouteLength(this[i], this[i + 1], ref tmp);
                }
                return retVal;
            }

            private void Initialise()
            {
                GenerateRoute(0);

                _initialised = true;
            }

            public List<LocationData> GetCompleteRoute()
            {
                List<LocationData> retVal = new List<LocationData>();
                for (int i = 0; i < this.Count - 1; i++)
                {
                    if (this[i] != this[i + 1])
                    {
                        List<int> route = Map.GetRoute(this[i], this[i + 1]);

                        // Remove the first system (i.e. the start system).
                        route.RemoveAt(0);

                        for (int systemIndex = 0; systemIndex < route.Count; systemIndex++)
                        {
                            int systemID = route[systemIndex];
                            /*retVal.Add(new LocationData(systemID,
                                SolarSystems.GetSystemName(systemID),
                                systemIndex == route.Count - 1));*/
                        }
                    }
                }
                return retVal;
            }

            public void GenerateRoute(long seed)
            {

            }

            private void PickupCargo(int stationID)
            {

            }


            public void UpdateStatus(int progress, int maxProgress, string section, string sectionStatus, bool done)
            {
                if (StatusChange != null)
                {
                    StatusChange(null, new StatusChangeArgs(progress, maxProgress, section, sectionStatus, done));
                }
            }

            [System.Diagnostics.Conditional("DIAGNOSTICS")]
            public void DiagnosticUpdate(string section, string sectionStatus)
            {
                if (StatusChange != null)
                {
                    StatusChange(null, new StatusChangeArgs(-1, -1, section, sectionStatus, false));
                }
            }

        }
        #endregion






    }


}