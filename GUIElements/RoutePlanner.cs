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
    public partial class RoutePlanner : Form
    {
        private List<string> _recentSystems;
        private string _lastSystem = "";
        private short[,] _jumps;
        private Dictionary<int, int> _idMapper;
        private int _nextFreeIndex = 0;
        private string _lastStartSystem = "";
        private string _lastEndSystem = "";

        public RoutePlanner()
        {
            InitializeComponent();

            UserAccount.Settings.GetFormSizeLoc(this);
        }

        private void RoutePlanner_Load(object sender, EventArgs e)
        {
            try
            {
                _recentSystems = UserAccount.CurrentGroup.Settings.RecentSystems;
                _recentSystems.Sort();
                AutoCompleteStringCollection systems = new AutoCompleteStringCollection();
                systems.AddRange(_recentSystems.ToArray());
                txtSystem.AutoCompleteCustomSource = systems;
                txtSystem.AutoCompleteSource = AutoCompleteSource.CustomSource;
                txtSystem.AutoCompleteMode = AutoCompleteMode.Suggest;
                txtSystem.Leave += new EventHandler(txtSystem_Leave);
                txtSystem.KeyDown += new KeyEventHandler(txtSystem_KeyDown);
                txtSystem.Tag = 0;
                txtSystem.Text = "";

                txtStartSystem.AutoCompleteCustomSource = systems;
                txtStartSystem.AutoCompleteSource = AutoCompleteSource.CustomSource;
                txtStartSystem.AutoCompleteMode = AutoCompleteMode.Suggest;
                txtStartSystem.Leave += new EventHandler(txtSystem_Leave);
                txtStartSystem.KeyDown += new KeyEventHandler(txtSystem_KeyDown);
                txtStartSystem.Tag = 0;
                txtStartSystem.Text = "";

                txtEndSystem.AutoCompleteCustomSource = systems;
                txtEndSystem.AutoCompleteSource = AutoCompleteSource.CustomSource;
                txtEndSystem.AutoCompleteMode = AutoCompleteMode.Suggest;
                txtEndSystem.Leave += new EventHandler(txtSystem_Leave);
                txtEndSystem.KeyDown += new KeyEventHandler(txtSystem_KeyDown);
                txtEndSystem.Tag = 0;
                txtEndSystem.Text = "";

                List<string> locations = GroupLocations.GetLocationNames();
                if (!locations.Contains("All Regions"))
                {
                    locations.Add("All Regions");
                }
                locations.Sort();
                cmbLocation.Items.AddRange(locations.ToArray());
                cmbLocation.AutoCompleteSource = AutoCompleteSource.ListItems;
                cmbLocation.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                cmbLocation.SelectedItem = "All Regions";

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
                    emmaex = new EMMAException(ExceptionSeverity.Critical, "Error setting up route planner", ex);
                }
                MessageBox.Show("Problem setting up route planner.\r\nCheck " + Globals.AppDataDir + "Logging\\ExceptionLog.txt" +
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
                if (field == txtSystem) { lastSystem = _lastSystem; }
                else if (field == txtStartSystem) { lastSystem = _lastStartSystem; }
                else if (field == txtEndSystem) { lastSystem = _lastEndSystem; }

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

                        if (field == txtSystem) { _lastSystem = field.Text; }
                        else if (field == txtStartSystem) { _lastStartSystem = field.Text; }
                        else if (field == txtEndSystem) { _lastEndSystem = field.Text; }
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

        private void btnAdd_Click(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                SetSelectedSystem(textBox);
            }

            int systemID = (int)txtSystem.Tag;
            if (systemID != 0)
            {
                string systemName = SolarSystems.GetSystem(systemID).solarSystemName;
                string regionName = Regions.GetRegionName(SolarSystems.GetSystem(systemID).regionID);
                lstWaypoints.Items.Add(new SystemData(systemID, systemName, regionName, true));
            }
        }

        private void lstWaypoints_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && lstWaypoints.SelectedIndex >= 0)
            {
                lstWaypoints.Items.RemoveAt(lstWaypoints.SelectedIndex);
            }
        }

        private void btnAddAssets_Click(object sender, EventArgs e)
        {
            int ownerID = 0;
            bool corp = false;
            GroupLocation location = null;

            if (cmbOwner.SelectedItem != null)
            {
                ownerID = ((CharCorpOption)cmbOwner.SelectedItem).Data.ID;
                corp = ((CharCorpOption)cmbOwner.SelectedItem).Corp;
            }
            if (cmbLocation.Text != "")
            {
                location = GroupLocations.GetLocationDetail(cmbLocation.Text);
            }

            if (ownerID != 0 && location != null)
            {
                EMMADataSet.IDTableDataTable systemIDs = Assets.GetInvolvedSystemIDs(ownerID,
                    location.Regions, location.Stations, !chkExcludeContainers.Checked,
                    !chkExcludeContainers.Checked);

                lstWaypoints.BeginUpdate();
                lstWaypoints.Items.Clear();
                foreach (EMMADataSet.IDTableRow idRow in systemIDs)
                {
                    if (!chkHighSecAssetsOnly.Checked || !SolarSystems.IsLowSec(idRow.ID))
                    {   
                        string systemName = SolarSystems.GetSystem(idRow.ID).solarSystemName;
                        string regionName = Regions.GetRegionName(SolarSystems.GetSystem(idRow.ID).regionID);
                        lstWaypoints.Items.Add(new SystemData(idRow.ID, systemName, regionName, true));
                    }
                }
                lstWaypoints.EndUpdate();
            }
        }

        private void btnGenRoute_Click(object sender, EventArgs e)
        {
            WPRoute route;

            int startSystemID = (int)txtStartSystem.Tag;
            int endSystemID = (int)txtEndSystem.Tag;

            if (startSystemID == 0)
            {
                MessageBox.Show("You must specify a start system",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (endSystemID == 0)
            {
                MessageBox.Show("You must specify an end system",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            if (lstWaypoints.Items.Count < 3)
            {
                MessageBox.Show("You must have at least 3 waypoints specified or there is nothing to optimise.",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                Cursor = Cursors.WaitCursor;
                try
                {
                    _nextFreeIndex = 0;
                    _idMapper = new Dictionary<int, int>();
                    _jumps = new short[lstWaypoints.Items.Count + 2, lstWaypoints.Items.Count + 2];

                    List<int> waypoints = new List<int>();
                    waypoints.Add(startSystemID);
                    foreach (object item in lstWaypoints.Items)
                    {
                        SystemData system = item as SystemData;
                        waypoints.Add(system.ID);
                    }
                    waypoints.Add(endSystemID);
                    route = new WPRoute(waypoints, ref _nextFreeIndex, ref _jumps, _idMapper);

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

                    if (lstRoute.Items.Count > 0)
                    {
                        lblRouteInfo.Text = string.Format("Route - {0} Jumps", lstRoute.Items.Count);
                        lstRoute.Focus();
                    }
                    else
                        lblRouteInfo.Text = "Route";
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private void lstRoute_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            SystemData system = (SystemData)lstRoute.Items[e.Index];
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
                SystemData system = (SystemData)lstRoute.Items[e.Index];
                system.Draw(e.Graphics, e.Bounds, e.Font);
            }
        }


        private class SystemData
        {
            public int ID;
            public string SystemName;
            public string RegionName;
            public bool IsWaypoint;
            private float _security = -10;

            public SystemData(int id, string name, string regionName, bool isWaypoint)
            {
                ID = id;
                SystemName = name;
                RegionName = regionName;
                IsWaypoint = isWaypoint;
            }

            public override string ToString()
            {
                return SystemName;
            }

            public float Security
            {
                get
                {
                    if (_security == -10)
                    {
                        _security = SolarSystems.GetSystemSecurity(ID);
                    }
                    return _security;
                }
            }

            public void Draw(Graphics g, Rectangle bounds, Font font)
            {
                string text = null;
                string tabSystemName = null;
                string tabRegionName = null;
                string formatString = null;
                Color col = Color.Empty;

                SizeF systemNameTextSize = g.MeasureString(this.SystemName, font);
                SizeF regionNameTextSize = g.MeasureString(this.RegionName, font);

                tabSystemName = systemNameTextSize.Width < 47.2 ? this.SystemName + "\t\t" : this.SystemName + "\t";
                tabRegionName = regionNameTextSize.Width < 47.2 ? this.RegionName + "\t\t" : this.RegionName + "\t";

                formatString = IsWaypoint ? "WP\t{0}{2}\t\t{1}" : "\t{0}{2}\t\t{1}";
                text = string.Format(formatString, tabSystemName, this.RegionName, Math.Round(Security, 2));

                if (Security > 0.45)
                    col = Color.DarkGreen;
                else if (Security > 0.25)
                    col = Color.Orange;
                else
                    col = Color.Red;

                g.DrawString(text, font, new SolidBrush(col), bounds);
            }
        }

        private class WPRoute : List<int>, IProvideStatus
        {
            public event StatusChangeHandler StatusChange;
            private int _nextFreeIndex;
            private short[,] _jumps;
            private Dictionary<int, int> _idMapper;


            public WPRoute(List<int> waypoints, ref int nextFreeIndex, ref short[,] jumps,
                Dictionary<int, int> idMapper)
                : base(waypoints)
            {
                _nextFreeIndex = nextFreeIndex;
                _jumps = jumps;
                _idMapper = idMapper;
            }


            /// <summary>
            /// This algorithm will optimize a sequence of waypoints to try and find the sequence that
            /// will result in the fewest number of jumps required to visit all waypoints.
            /// (i.e. the travelling salesman problem: http://en.wikipedia.org/wiki/Travelling_salesman_problem)
            /// The approach used here is to run a genetic algorithm.
            /// This works by 'cloning' the current sequence of waypoints to create a population. 
            /// Each sequence then has different random changes made (mutations).
            /// Once all mutations are complete, the best (shortest) sequence of waypoints is used 
            /// as the basis for the next generation and the process repeats.
            /// We only stop if we go for 6 generations without improving the route at all. 
            /// At that point we probably have somthing close to the best sequence of waypoints though
            /// this is not guaranteed.
            /// </summary>
            public void Optimize()
            {
                bool improvement = true;
                int generation = 0;
                int noImprovementGen = 0;
                int populationSize = Math.Min(this.Count, 50);
                List<WPRoute> population = new List<WPRoute>();
                WPRoute bestRoute = null;
                // Just wait half a second to allow the caller to display the progress dialog.
                Thread.Sleep(500);
                UpdateStatus(0, 1, "Optimizing Route", "", false);
                UpdateStatus(0, 1, "", "Pre-caching data", false);
                SolarSystemDistances.PopulateJumpsArray(this, this, ref _jumps, _idMapper, ref _nextFreeIndex);
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
                            WPRoute currentRoute = new WPRoute(this, ref _nextFreeIndex, ref _jumps, _idMapper);
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
                int retVal = 0;
                int[] tmp = new int[4];
                for (int i = 0; i < this.Count - 1; i++)
                {
                    retVal += GetRouteLength(this[i], this[i + 1], ref tmp);
                }
                return retVal;
            }

            public List<SystemData> GetCompleteRoute()
            {
                List<SystemData> retVal = new List<SystemData>();
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
                            string systemName = SolarSystems.GetSystem(systemID).solarSystemName;
                            string regionName = Regions.GetRegionName(SolarSystems.GetSystem(systemID).regionID);

                            retVal.Add(new SystemData(systemID, systemName, regionName, systemIndex == route.Count - 1));
                        }
                    }
                }
                return retVal;
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

        private void btnAutopilotSettings_Click(object sender, EventArgs e)
        {
            RouteCalcSettings settings = new RouteCalcSettings();
            settings.ShowDialog();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            lstWaypoints.Items.Clear();
        }
    }
}