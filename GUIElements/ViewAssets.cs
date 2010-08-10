using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.AbstractionClasses;
using EveMarketMonitorApp.Reporting;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class ViewAssets : Form
    {
        private AssetList _assets;
        private BindingSource _assetsBindingSource;
        private List<int> _personalOwners;
        private List<int> _corporateOwners;
        private List<AssetAccessParams> _accessParams;
        private int _itemID;
        private List<string> _recentItems;

        private string _lastItem = "";
        private bool _change = false;
        private List<int> _modifiedIndicies = new List<int>();

        private DataGridViewRow _clickedRow;
        private DataGridViewCell _clickedCell;

        //private DataGridViewCellStyle _inTransitStyle;
        //private DataGridViewCellStyle _regularStyle;

        public ViewAssets()
        {
            InitializeComponent();
            AssetsGrid.AutoGenerateColumns = false;

            _recentItems = UserAccount.CurrentGroup.Settings.RecentItems;
            UserAccount.Settings.GetFormSizeLoc(this);
            AssetsGrid.Tag = "Assets Data";
            if (Globals.calculator != null)
            {
                Globals.calculator.BindGrid(AssetsGrid);
            }
        }

        private void ViewAssets_Load(object sender, EventArgs e)
        {
            Diagnostics.ResetAllTimers();
            try
            {
                Diagnostics.StartTimer("ViewAssets");
                Diagnostics.StartTimer("ViewAssets.Part1");
                DataGridViewCellStyle style = new DataGridViewCellStyle(CostColumn.DefaultCellStyle);
                style.Format = IskAmount.FormatString();
                CostColumn.DefaultCellStyle = style;

                _assets = new AssetList();
                _assetsBindingSource = new BindingSource();
                _assetsBindingSource.DataSource = _assets;

                AssetsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                AssetsGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

                AssetsGrid.DataSource = _assetsBindingSource;
                OwnerColumn.DataPropertyName = "Owner";
                ItemColumn.DataPropertyName = "Item";
                LocationColumn.DataPropertyName = "Location";
                QuantityColumn.DataPropertyName = "Quantity";
                AutoConExcludeColumn.DataPropertyName = "AutoConExclude";
                ReprocessorExcludeColumn.DataPropertyName = "ReprocessorExclude";
                StatusColumn.DataPropertyName = "Status";
                CostColumn.DataPropertyName = UserAccount.Settings.CalcCostInAssetView ? "UnitBuyPrice" : "PureUnitBuyPrice";
                //_regularStyle = OwnerColumn.DefaultCellStyle.Clone();
                //_inTransitStyle = OwnerColumn.DefaultCellStyle.Clone();
                //_inTransitStyle.BackColor = Color.Yellow;

                UserAccount.Settings.GetColumnWidths(this.Name, AssetsGrid);

                _recentItems.Sort();
                cmbItem.Items.AddRange(_recentItems.ToArray());
                cmbItem.AutoCompleteSource = AutoCompleteSource.ListItems;
                cmbItem.AutoCompleteMode = AutoCompleteMode.Suggest;
                cmbItem.KeyDown += new KeyEventHandler(cmbItem_KeyDown);
                cmbItem.SelectedIndexChanged += new EventHandler(cmbItem_SelectedIndexChanged);
                cmbItem.Tag = 0;
                Diagnostics.StopTimer("ViewAssets.Part1");

                Diagnostics.StartTimer("ViewAssets.Part2");
                List<CharCorpOption> charcorps = UserAccount.CurrentGroup.GetCharCorpOptions(APIDataType.Assets);
                _corporateOwners = new List<int>(); 
                _personalOwners = new List<int>();
                foreach (CharCorpOption chop in charcorps)
                {
                    if (chop.Corp)
                    {
                        _corporateOwners.Add(chop.CharacterObj.CorpID);
                    }
                    else
                    {
                        _personalOwners.Add(chop.CharacterObj.CharID);
                    }
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
                chkCalcCost.Checked = UserAccount.Settings.CalcCostInAssetView;
                chkCalcCost.CheckedChanged += new EventHandler(chkCalcCost_CheckedChanged);
                Diagnostics.StopTimer("ViewAssets.Part2");

                Diagnostics.StartTimer("ViewAssets.Part3");
                //DisplayAssets();
                BuildAccessList();
                DisplayTree();
                Diagnostics.StopTimer("ViewAssets.Part3");

                assetsTree.AfterExpand += new TreeViewEventHandler(assetsTree_AfterExpand);
                assetsTree.AfterSelect += new TreeViewEventHandler(assetsTree_AfterSelect);
                this.FormClosing += new FormClosingEventHandler(ViewAssets_FormClosing);


                Diagnostics.StopTimer("ViewAssets");
                Diagnostics.DisplayDiag("View assets setup time: " +
                    Diagnostics.GetRunningTime("ViewAssets").ToString() +
                    "\r\n  Initalise: " + Diagnostics.GetRunningTime("ViewAssets.Part1").ToString() +
                    "\r\n  Setup owners: " + Diagnostics.GetRunningTime("ViewAssets.Part2").ToString() +
                    "\r\n  Display Tree: " + Diagnostics.GetRunningTime("ViewAssets.Part3").ToString());
            }
            catch (Exception ex)
            {
                // Creating new EMMAexception will cause error to be logged.
                EMMAException emmaex = ex as EMMAException;
                if (emmaex == null)
                {
                    emmaex = new EMMAException(ExceptionSeverity.Critical, "Error setting up assets form", ex);
                }
                MessageBox.Show("Problem setting up assets view.\r\nCheck " + Globals.AppDataDir + "Logging\\ExceptionLog.txt" +
                    " for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        void chkCalcCost_CheckedChanged(object sender, EventArgs e)
        {
            UserAccount.Settings.CalcCostInAssetView = chkCalcCost.Checked;
            CostColumn.DataPropertyName = UserAccount.Settings.CalcCostInAssetView ? "UnitBuyPrice" : "PureUnitBuyPrice";
        }


        void assetsTree_AfterExpand(object sender, TreeViewEventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                if (e.Node.Nodes.Count == 1)
                {
                    if (e.Node.Nodes[0].Text.Equals("Temp"))
                    {
                        e.Node.Nodes.Clear();
                        BuildTree(e.Node);
                    }
                }
                foreach (TreeNode child in e.Node.Nodes)
                {
                    if (child.Nodes.Count == 0)
                    {
                        AssetViewNodeType type = ((AssetViewNode)child.Tag).Type;
                        if (type == AssetViewNodeType.Station || type == AssetViewNodeType.Container)
                        {
                            BuildTree(child);
                        }
                        else
                        {
                            child.Nodes.Add("Temp");
                        }
                    }
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        void assetsTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            AssetViewNode node = (AssetViewNode)e.Node.Tag;
            AssetViewNodeType type = node.Type;
            bool displayAssets = true;
            if (type != AssetViewNodeType.All && type != AssetViewNodeType.Region)
            {
            }
            else
            {
                /*if (MessageBox.Show("Warning, displaying assets for " + node.Text +
                    " may take a long time, really display these assets?", "Confirm",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    DisplayAssets();
                }*/
                string warningDefault = UserAccount.Settings.AssetsViewWarning;
                if (warningDefault.Equals("WARN"))
                {
                    ChkMessageBox message = new ChkMessageBox();
                    DialogResult result = message.Show("Don't ask me again", 
                        "Warning, displaying assets for " + node.Text +
                        " may take a long time, really display these assets?", "Confirm", 
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.No)
                    {
                        displayAssets = false;
                        if (message.Checked) { UserAccount.Settings.AssetsViewWarning = "FORCE NO"; }
                    }
                    if (result == DialogResult.Yes && message.Checked)
                    {
                        UserAccount.Settings.AssetsViewWarning = "FORCE YES";
                    }
                }
                else
                {
                    if (warningDefault.Equals("FORCE NO")) { displayAssets = false; }
                }
            }

            if (displayAssets)
            {
                DisplayAssets();
            }
        }

        void chkIngoreOwner_CheckedChanged(object sender, EventArgs e)
        {
            cmbOwner.Enabled = !chkIngoreOwner.Checked;
            if (chkIngoreOwner.Checked)
            {
                DisplayAssets();
                DisplayTree();
            }
        }

        void cmbOwner_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayAssets();
            DisplayTree();
        }

        private void cmbStation_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayAssets();
            DisplayTree();
        }

        void cmbItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Tab)
            {
                SetSelectedItem();
            }
        }

        void cmbItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetSelectedItem();
        }

        private void SetSelectedItem()
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (!cmbItem.Text.Equals(_lastItem))
                {
                    cmbItem.Tag = (short)0;
                    if (!cmbItem.Text.Equals(""))
                    {
                        try
                        {
                            EveDataSet.invTypesRow item = Items.GetItem(cmbItem.Text);
                            if (item != null)
                            {
                                cmbItem.Tag = item.typeID;
                                string name = item.typeName;
                                cmbItem.Text = name;
                                if (!cmbItem.Items.Contains(name))
                                {
                                    cmbItem.Items.Add(name);
                                    _recentItems.Add(name);
                                }
                            }
                        }
                        catch (EMMADataException) { }

                        if ((short)cmbItem.Tag != 0)
                        {
                            _lastItem = cmbItem.Text;
                            DisplayAssets();
                            DisplayTree();
                        }
                    }
                    if ((short)cmbItem.Tag == 0) 
                    { 
                        cmbItem.Text = "";
                        DisplayTree();
                        ClearAssets();
                    }
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }



        private void ClearAssets()
        {
            List<SortInfo> sortInfo = AssetsGrid.GridSortInfo;
            _assets = new AssetList();
            _assetsBindingSource.DataSource = _assets;
            AssetsGrid.GridSortInfo = sortInfo;

            Text = "Viewing " + _assetsBindingSource.Count + " assets";
        }

        private void DisplayAssets()
        {
            if (ContinueWithoutSave())
            {
                Cursor = Cursors.WaitCursor;

                try
                {
                    _change = false;
                    _modifiedIndicies = new List<int>();

                    _itemID = short.Parse(cmbItem.Tag.ToString());
                    int locationID = 0, systemID = 0;
                    List<int> regionIDs = new List<int>();
                    Asset container = null;

                    BuildAccessList();

                    if (assetsTree.SelectedNode != null)
                    {
                        AssetViewNode tag = assetsTree.SelectedNode.Tag as AssetViewNode;
                        if (tag != null)
                        {
                            switch (tag.Type)
                            {
                                case AssetViewNodeType.All:
                                    break;
                                case AssetViewNodeType.Region:
                                    regionIDs.Add((int)tag.Id);
                                    break;
                                case AssetViewNodeType.System:
                                    systemID = (int)tag.Id;
                                    break;
                                case AssetViewNodeType.Station:
                                    locationID = (int)tag.Id;
                                    break;
                                case AssetViewNodeType.Container:
                                    container = tag.Data as Asset;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                    //ListSortDirection sortDirection = ListSortDirection.Descending;
                    //DataGridViewColumn sortColumn = AssetsGrid.SortedColumn;
                    //if (AssetsGrid.SortOrder == SortOrder.Ascending) sortDirection = ListSortDirection.Ascending;
                    List<SortInfo> sortInfo = AssetsGrid.GridSortInfo;

                    if (container == null)
                    {
                        _assets = Assets.LoadAssets(_accessParams, regionIDs, _itemID, locationID, systemID, 
                            false, 0, true, false);
                    }
                    else
                    {
                        _assets = Assets.LoadAssets(container, _itemID);
                    }

                    _assetsBindingSource.DataSource = _assets;

                    //AssetsGrid.AutoResizeColumns();
                    //AssetsGrid.AutoResizeRows();

                    //if (sortColumn != null)
                    //{
                    //    AssetsGrid.Sort(sortColumn, sortDirection);
                    //}
                    AssetsGrid.GridSortInfo = sortInfo;

                    Text = "Viewing " + _assetsBindingSource.Count + " assets";
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private void AssetsGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                //string status = (string)AssetsGrid["StatusColumn", e.RowIndex].Value;
                //if (status.Equals("In Transit"))
                //{
                //    AssetsGrid.Rows[e.RowIndex].DefaultCellStyle = _inTransitStyle;
                //}
                //else
                //{
                //    AssetsGrid.Rows[e.RowIndex].DefaultCellStyle = _regularStyle;
                //}

                if (AssetsGrid.Columns[e.ColumnIndex].Name.Equals("CostColumn"))
                {
                    Asset a = (Asset)AssetsGrid.Rows[e.RowIndex].DataBoundItem;
                    DataGridViewCellStyle style = e.CellStyle;
                    if (a.UnitBuyPricePrecalculated)
                    {
                        style.ForeColor = Color.Black;
                    }
                    else
                    {
                        style.ForeColor = Color.Orange;
                    }

                    e.CellStyle = style;
                }
            }
        }

        private void BuildAccessList()
        {
            int ownerID = 0;
            bool corpAssets = false;
            if (cmbOwner.SelectedValue != null && !chkIngoreOwner.Checked)
            {
                CharCorp data = (CharCorp)cmbOwner.SelectedValue;
                ownerID = data.ID; //data.characterObj.CharID;
                corpAssets = data.corp;
            }

            _accessParams = new List<AssetAccessParams>();
            if (ownerID == 0)
            {
                foreach (int id in _personalOwners)
                {
                    _accessParams.Add(new AssetAccessParams(id));
                }
                foreach (int id in _corporateOwners)
                {
                    _accessParams.Add(new AssetAccessParams(id));
                }
            }
            else
            {
                _accessParams.Add(new AssetAccessParams(ownerID));
            }
        }

        private void DisplayTree()
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                assetsTree.Nodes.Clear();
                TreeNode rootNode = new TreeNode("All Assets");
                rootNode.Tag = new AssetViewNode(0, "All Assets", AssetViewNodeType.All);
                rootNode.Nodes.Add("Temp");
                //BuildTree(rootNode);
                assetsTree.Nodes.Add(rootNode);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void BuildTree(TreeNode node)
        {
            assetsTree.BeginUpdate();
            AssetViewNode nodeData = node.Tag as AssetViewNode;

            switch (nodeData.Type)
            {
                case AssetViewNodeType.All:
                    // This is the top level node so display the regions which contain assets
                    // as the next level.
                    EveDataSet.mapRegionsDataTable regions = Regions.GetAssetRegions(_accessParams, _itemID);
                    foreach (EveDataSet.mapRegionsRow region in regions)
                    {
                        TreeNode rnode = new TreeNode(region.regionName);
                        rnode.Tag = new AssetViewNode(region.regionID, region.regionName, AssetViewNodeType.Region);
                        node.Nodes.Add(rnode);
                    }
                    break;
                case AssetViewNodeType.Region:
                    // This is a region level node so the next level will be solar systems
                    // within the region that contain assets.
                    EveDataSet.mapSolarSystemsDataTable systems = SolarSystems.GetAssetSystems(_accessParams,
                        _itemID, (int)nodeData.Id);
                    foreach(EveDataSet.mapSolarSystemsRow system in systems)
                    {
                        TreeNode snode = new TreeNode(system.solarSystemName);
                        snode.Tag = new AssetViewNode(system.solarSystemID, system.solarSystemName, 
                            AssetViewNodeType.System);
                        node.Nodes.Add(snode);
                    }
                    break;
                case AssetViewNodeType.System:
                    // This is a system level node so the next level will be both stations
                    // within the system that contain assets and any containers in space in the system.
                    EveDataSet.staStationsDataTable stations = Stations.GetAssetStations(_accessParams, 
                        _itemID, (int)nodeData.Id);
                    foreach(EveDataSet.staStationsRow station in stations) 
                    {
                        TreeNode stnode = new TreeNode(station.stationName);
                        stnode.Tag = new AssetViewNode(station.stationID, station.stationName, 
                            AssetViewNodeType.Station);
                        node.Nodes.Add(stnode);
                    }

                    // Note the locationID param is the system ID and the systemID param is 0.
                    // This is because we only want assets that are in space in the system.
                    // NOT those in stations in the system.
                    AssetList systemAssets = Assets.LoadAssets(_accessParams, new List<int>(), _itemID,
                        (int)nodeData.Id, 0, true, 0, false, true);
                    foreach(Asset asset in systemAssets) 
                    {
                        TreeNode anode = new TreeNode(asset.Item);
                        anode.Tag = new AssetViewNode(asset.ID, asset.Item, AssetViewNodeType.Container, asset);
                        node.Nodes.Add(anode);
                    }
                    break;
                case AssetViewNodeType.Station:
                    // This is a station level node so the next level will be any containers in
                    // the station that contain assets.
                    AssetList stationContainers = Assets.LoadAssetsByItemAndContainersOfItem(_accessParams,
                        new List<int>(), _itemID, (int)nodeData.Id, 0, true);
                    foreach (Asset asset in stationContainers)
                    {
                        TreeNode cnode = new TreeNode(asset.Item);
                        cnode.Tag = new AssetViewNode(asset.ID, asset.Item, AssetViewNodeType.Container, asset);
                        node.Nodes.Add(cnode);
                    }
                    break;
                case AssetViewNodeType.Container:
                    // Containers may contain other containers so add them...
                    AssetList subContainers = Assets.LoadAssets(nodeData.Data as Asset);
                    foreach (Asset asset in subContainers)
                    {
                        bool display = true;
                        if (asset.IsContainer)
                        {
                            if (_itemID != 0)
                            {
                                asset.Contents.ItemFilter = "ItemID = " + _itemID;
                                display = asset.Contents.FiltredItems.Count > 0;
                            }
                            if (display)
                            {
                                TreeNode cnode = new TreeNode(asset.Item);
                                cnode.Tag = new AssetViewNode(asset.ID, asset.Item, AssetViewNodeType.Container, asset);
                                node.Nodes.Add(cnode);
                            }
                        }
                    }

                    break;
                default:
                    break;
            }
             
            assetsTree.EndUpdate();
        }

        void ViewAssets_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ContinueWithoutSave())
            {
                e.Cancel = true;
            }
            if (UserAccount.Settings != null)
            {
                if (UserAccount.CurrentGroup != null)
                {
                    UserAccount.CurrentGroup.Settings.RecentItems = _recentItems;
                }
                UserAccount.Settings.StoreColumnWidths(this.Name, AssetsGrid);
                UserAccount.Settings.StoreFormSizeLoc(this);
            }
            if (Globals.calculator != null)
            {
                Globals.calculator.RemoveGrid(AssetsGrid);
            }
        }

        private bool ContinueWithoutSave()
        {
            bool retVal = true;
            if (_change)
            {
                if (MessageBox.Show("Changes have been made to this data that have not been saved." +
                    "\r\nContinue without saving?", "Question", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.No)
                {
                    retVal = false;
                }
            }
            return retVal;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnStore_Click(object sender, EventArgs e)
        {
            if (_change)
            {
                foreach (int index in _modifiedIndicies)
                {
                    if (AssetsGrid.Rows[index] != null)
                    {
                        Asset assetData = AssetsGrid.Rows[index].DataBoundItem as Asset;
                        if (assetData != null)
                        {
                            Assets.SetAutoConExcludeFlag(assetData.ID, assetData.AutoConExclude);
                            Assets.SetReprocExcludeFlag(assetData.ID, assetData.ReprocessorExclude);
                        }
                    }
                }

                _change = false;
            }
        }

        private void AssetsGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0)
            {
                if (AssetsGrid.Columns[e.ColumnIndex] == AutoConExcludeColumn)
                {
                    _change = true;
                    _modifiedIndicies.Add(e.RowIndex);
                }
                else if (AssetsGrid.Columns[e.ColumnIndex] == ReprocessorExcludeColumn)
                {
                    _change = true;
                    _modifiedIndicies.Add(e.RowIndex);
                }
            }
        }

        private void AssetsGrid_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                DataGridView.HitTestInfo Hti;
                Hti = AssetsGrid.HitTest(e.X, e.Y);

                if (Hti.Type == DataGridViewHitTestType.Cell)
                {
                    // store a reference to the cell and row that the user has clicked on.
                    _clickedRow = AssetsGrid.Rows[Hti.RowIndex];
                    _clickedCell = AssetsGrid[Hti.ColumnIndex, Hti.RowIndex];
                    _clickedRow.Selected = true;
                }
            }

        }
        
        private void copyCellDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_clickedCell != null && _clickedCell.Value != null)
            {
                Clipboard.SetText(_clickedCell.Value.ToString());
            }
        }

        private void copyRowDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_clickedRow != null)
            {
                StringBuilder stringData = new StringBuilder();
                foreach (DataGridViewCell cell in _clickedRow.Cells)
                {
                    if (cell.Value != null)
                    {
                        if (stringData.Length > 0) { stringData.Append(","); }
                        stringData.Append(cell.Value.ToString());
                    }
                }
                Clipboard.SetText(stringData.ToString(), TextDataFormat.Text);
            }
        }

        private void copyCellTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_clickedCell != null && _clickedCell.FormattedValue != null)
            {
                Clipboard.SetText(_clickedCell.FormattedValue.ToString());
            }
        }

        private void copyRowTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_clickedRow != null)
            {
                StringBuilder stringData = new StringBuilder();
                foreach (DataGridViewCell cell in _clickedRow.Cells)
                {
                    if (cell.FormattedValue != null)
                    {
                        if (stringData.Length > 0) { stringData.Append(","); }
                        stringData.Append(cell.FormattedValue.ToString());
                    }
                }
                Clipboard.SetText(stringData.ToString(), TextDataFormat.Text);
            }
        }



        private class AssetViewNode 
        {
            private string _text;
            private long _id;
            private AssetViewNodeType _type;
            private object _data;

            public AssetViewNode(long id, string text, AssetViewNodeType type)
            {
                _text = text;
                _id = id;
                _type = type;
            }

            public AssetViewNode(long id, string text, AssetViewNodeType type, object data)
            {
                _text = text;
                _id = id;
                _type = type;
                _data = data;
            }

            public string Text
            {
                get { return _text; }
                set { _text = value; }
            }

            public long Id
            {
                get { return _id; }
                set { _id = value; }
            }

            public AssetViewNodeType Type
            {
                get { return _type; }
                set { _type = value; }
            }

            public object Data
            {
                get { return _data; }
                set { _data = value; }
            }
        }

        private enum AssetViewNodeType
        {
            All,
            Region,
            System,
            Station,
            Container
        }

        private void btnCSV_Click(object sender, EventArgs e)
        {
            CSVExport.Export(AssetsGrid, "assets");
        }


    }
}