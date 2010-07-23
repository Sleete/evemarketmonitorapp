using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.Reporting;
using EveMarketMonitorApp.AbstractionClasses;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class ViewUnacknowledgedOrders : Form
    {
        private OrdersList _orders;
        //private BindingSource _ordersBindingSource;
        private List<int> _owners;
        //private List<int> _corporateOwners;
        //private List<AssetAccessParams> _accessParams;
        private int _lastNumberOfOrders = 0;

        public ViewUnacknowledgedOrders()
        {
            InitializeComponent();

            //_owners = new List<int>();
            //_corporateOwners = new List<int>();
            //List<CharCorpOption> charcorps = UserAccount.CurrentGroup.GetCharCorpOptions(APIDataType.Orders);
            //foreach (CharCorpOption chop in charcorps)
            //{
            //    if (chop.Corp)
            //    {
            //        _corporateOwners.Add(chop.CharacterObj.CorpID);
            //    }
            //    else
            //    {
            //        _owners.Add(chop.CharacterObj.CharID);
            //    }
            //}

            _orders = new OrdersList();
            //_ordersBindingSource = new BindingSource();
            //_ordersBindingSource.DataSource = _orders;
            UserAccount.Settings.GetFormSizeLoc(this);

            ordersGrid.Tag = "Unacknowledged Orders Data";
            if (Globals.calculator != null)
            {
                Globals.calculator.BindGrid(ordersGrid);
            }
        }

        private void ViewUnacknowledgedOrders_Load(object sender, EventArgs e)
        {
            try
            {
                _orders = new OrdersList();
                //_ordersBindingSource = new BindingSource();
                //_ordersBindingSource.DataSource = _orders;

                DataGridViewCellStyle iskStyle = new DataGridViewCellStyle(PriceColumn.DefaultCellStyle);
                iskStyle.Format = IskAmount.FormatString();
                PriceColumn.DefaultCellStyle = iskStyle;
                DataGridViewCellStyle dayStyle = new DataGridViewCellStyle(DurationColumn.DefaultCellStyle);
                dayStyle.Format = "# Days;-# Days;# Days";
                DurationColumn.DefaultCellStyle = dayStyle;

                ordersGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                ordersGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                ordersGrid.AutoGenerateColumns = false;

                //ordersGrid.DataSource = _ordersBindingSource;
                DateTimeColmun.DataPropertyName = "Date";
                OwnerColumn.DataPropertyName = "Owner";
                ItemColumn.DataPropertyName = "Item";
                PriceColumn.DataPropertyName = "Price";
                StationColumn.DataPropertyName = "Station";
                TypeColumn.DataPropertyName = "Type";
                QuantityColumn.DataPropertyName = "TotalVol";
                RangeColumn.DataPropertyName = "RangeText";
                DurationColumn.DataPropertyName = "Duration";
                AcknowledgeColumn.Image = icons.Images["tick.gif"];
                ordersGrid.CellContentClick += new DataGridViewCellEventHandler(ordersGrid_CellContentClick);

                //ordersGrid.Sort(DateTimeColmun, ListSortDirection.Ascending);
                List<SortInfo> sort = new List<SortInfo>();
                sort.Add(new SortInfo(0, "Date"));
                ordersGrid.GridSortInfo = sort;

                UserAccount.Settings.GetColumnWidths(this.Name, ordersGrid);

                _lastNumberOfOrders = 0;
                this.FormClosing += new FormClosingEventHandler(ViewUnacknowledgedOrders_FormClosing);
                DisplayOrders();
            }
            catch (Exception ex)
            {
                // Creating new EMMAexception will cause error to be logged.
                EMMAException emmaex = ex as EMMAException;
                if (emmaex == null)
                {
                    emmaex = new EMMAException(ExceptionSeverity.Critical, "Error setting up orders form", ex);
                }
                MessageBox.Show("Problem setting up unacknowledged orders view.\r\nCheck " + Globals.AppDataDir + "Logging\\ExceptionLog.txt" +
                    " for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void ViewUnacknowledgedOrders_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (UserAccount.Settings != null && UserAccount.Settings != null)
            {
                UserAccount.Settings.StoreColumnWidths(this.Name, ordersGrid);
                UserAccount.Settings.StoreFormSizeLoc(this);
            }
            if (Globals.calculator != null)
            {
                Globals.calculator.RemoveGrid(ordersGrid);
            }
        }

        public int LastNumberOfOrders()
        {
            return _lastNumberOfOrders;
        }

        public void DisplayOrders()
        {
            // Only refresh the display if the number of unacknowledged orders has changed.
            OrdersList unack = Orders.LoadOrders(UserAccount.CurrentGroup.GetAssetAccessParams(APIDataType.Orders),
                new List<int>(), new List<int>(), (int)OrderState.ExpiredOrFilledAndUnacknowledged, "Any");
            if (unack.Count != _lastNumberOfOrders)
            {

                //List<int> items = new List<int>();
                //items.Add(0);
                //List<int> stations = new List<int>();
                //stations.Add(0);

                //_accessParams = new List<AssetAccessParams>();

                ////List<int> ignore = new List<int>();
                //foreach (int id in _owners)
                //{
                //    _accessParams.Add(new AssetAccessParams(id));
                //    //_accessParams.Add(new AssetAccessParams(id, true, _corporateOwners.Contains(id)));
                //    //ignore.Add(id);
                //}
                //foreach (int id in _corporateOwners)
                //{
                //    _accessParams.Add(new AssetAccessParams(id));
                ////    if (!ignore.Contains(id))
                ////    {
                ////        _accessParams.Add(new AssetAccessParams(id, false, true));
                ////    }
                //}

                //ListSortDirection sortDirection = ListSortDirection.Descending;
                //DataGridViewColumn sortColumn = ordersGrid.SortedColumn;
                //if (ordersGrid.SortOrder == SortOrder.Ascending) sortDirection = ListSortDirection.Ascending;
                List<SortInfo> sortinfo = ordersGrid.GridSortInfo;

                //_orders = Orders.LoadOrders(_accessParams, items, stations,
                //    (int)OrderState.ExpiredOrFilledAndUnacknowledged, "Any");
                _orders = unack;
                //_ordersBindingSource.DataSource = _orders;
                ordersGrid.DataSource = _orders;

                //ordersGrid.AutoResizeColumns();
                //ordersGrid.AutoResizeRows();

                //if (sortColumn != null)
                //{
                //    ordersGrid.Sort(sortColumn, sortDirection);
                //}
                ordersGrid.GridSortInfo = sortinfo;

                _lastNumberOfOrders = _orders.Count;
                Text = _lastNumberOfOrders + " unacknowledged orders";
            }
        }

        private void ordersGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (ordersGrid.Rows[e.RowIndex] != null)
            {
                if (ordersGrid.Columns[e.ColumnIndex].Name.Equals("PriceColumn"))
                {
                    bool buyOrder = ordersGrid["TypeColumn", e.RowIndex].Value.ToString().Equals("Buy");
                    DataGridViewCellStyle style = e.CellStyle;
                    if (buyOrder)
                    {
                        style.ForeColor = Color.Red;
                    }
                    else
                    {
                        style.ForeColor = Color.Green;
                    }

                    e.CellStyle = style;
                }
            }

        }

        void ordersGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                if (ordersGrid.Columns[e.ColumnIndex] == AcknowledgeColumn)
                {
                    Order orderData = (Order)ordersGrid.Rows[e.RowIndex].DataBoundItem;
                    orderData.StateID = (short)OrderState.ExpiredOrFilledAndAcknowledged;
                    Orders.Store(orderData);
                    DisplayOrders();
                }
            }
        }

        private void btnAcknowledgeAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow orderRow in ordersGrid.Rows)
            {
                Order order = (Order)orderRow.DataBoundItem;
                order.StateID = (short)OrderState.ExpiredOrFilledAndAcknowledged;
                Orders.Store(order);
            }
            DisplayOrders();
        }

        private void btnAckSelected_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow orderRow in ordersGrid.SelectedRows)
            {
                Order order = (Order)orderRow.DataBoundItem;
                order.StateID = (short)OrderState.ExpiredOrFilledAndAcknowledged;
                Orders.Store(order);
            }
            DisplayOrders();

        }


    }
}