using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace EveMarketMonitorApp.Common
{
    class MultisortDataGridView : DataGridView
    {
        private List<SortInfo> _sortIndex = new List<SortInfo>();
        private bool _controlPressed = false;

        public MultisortDataGridView()
        {
            this.ColumnHeaderMouseClick += new DataGridViewCellMouseEventHandler(MultisortDataGridView_ColumnHeaderMouseClick);
            this.KeyDown += new KeyEventHandler(MultisortDataGridView_KeyDown);
            this.KeyUp += new KeyEventHandler(MultisortDataGridView_KeyUp);
        }

        internal List<SortInfo> GridSortInfo
        {
            get { return _sortIndex; }
            set
            {
                _sortIndex = value;
                RefreshColHeaders();
                DoSort();
            }
        }

        void MultisortDataGridView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.LControlKey || e.KeyCode == Keys.RControlKey || !e.Control)
            {
                _controlPressed = false;
            }
        }

        void MultisortDataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.LControlKey || e.KeyCode == Keys.RControlKey || e.Control)
            {
                _controlPressed = true;
            }
        }

        void MultisortDataGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            bool done = false;

            if (e.ColumnIndex >= 0)
            {
                List<SortInfo> newSortIndex = new List<SortInfo>();

                if (_controlPressed)
                {
                    newSortIndex = _sortIndex;
                }

                // If the clicked column exists in our sort index then just reverse the sort order.
                foreach(SortInfo index in _sortIndex) 
                {
                    if (index.ColumnIndex == e.ColumnIndex)
                    {
                        index.Direction = index.Direction == ListSortDirection.Ascending ?
                            ListSortDirection.Descending : ListSortDirection.Ascending;
                        if (!newSortIndex.Contains(index))
                        {
                            newSortIndex.Add(index);
                        }
                        done = true;
                    }
                }

                // If the clicked column was not already in the sort index then add it.
                if (!done && !this.Columns[e.ColumnIndex].DataPropertyName.Equals(""))
                {
                    newSortIndex.Add(new SortInfo(e.ColumnIndex, this.Columns[e.ColumnIndex].DataPropertyName));
                }

                _sortIndex = newSortIndex;
                RefreshColHeaders();
                DoSort();
            }
        }

        private void RefreshColHeaders()
        {
            foreach (DataGridViewColumn column in this.Columns)
            {
                column.HeaderCell.SortGlyphDirection = SortOrder.None;
                int index = column.HeaderText.IndexOf("(");
                if (index > 0)
                {
                    column.HeaderText = column.HeaderText.Remove(index).Trim();
                }
            }
            int counter = 1;
            foreach (SortInfo index in _sortIndex)
            {
                if (index.ColumnIndex >= 0 && this.Columns.Count > index.ColumnIndex)
                {
                    DataGridViewColumn column = this.Columns[index.ColumnIndex];
                    column.HeaderCell.SortGlyphDirection = index.Direction == ListSortDirection.Ascending ?
                        SortOrder.Ascending : SortOrder.Descending;
                    if (_sortIndex.Count > 1)
                    {
                        column.HeaderText = column.HeaderText + " (" + counter + ")";
                        counter++;
                    }
                }
            }
        }

        private void DoSort()
        {
            SortableCollection data = this.DataSource as SortableCollection;
            if (data == null)
            {
                BindingSource bs = this.DataSource as BindingSource;
                if (bs != null)
                {
                    data = bs.DataSource as SortableCollection;
                }
            }

            if (data != null)
            {
                StringBuilder sortText = new StringBuilder();

                /*foreach (SortInfo sort in _sortIndex)
                {
                    DataGridViewColumn column = this.Columns[sort.ColumnIndex];
                    if (sortText.Length > 0) { sortText.Append(","); }
                    sortText.Append(column.DataPropertyName);
                    sortText.Append(sort.Direction == ListSortDirection.Ascending ? " ASC" : " DESC");
                }

                data.Sort(sortText.ToString());*/
                data.Sort(_sortIndex);
                this.Refresh();
            }
        }
    }

    [Serializable]
    public class SortInfo : ISerializable
    {
        private int _columnIndex;
        private string _propertyName;
        private ListSortDirection _direction;

        public SortInfo(int columnIndex, string propertyName)
        {
            _columnIndex = columnIndex;
            _propertyName = propertyName;
            _direction = ListSortDirection.Descending;
        }

         //Deserialization constructor.
        public SortInfo(SerializationInfo info, StreamingContext ctxt)
        {
            //Get the values from info and assign them to the appropriate properties
            _columnIndex = (int)info.GetValue("ColumnIndex", typeof(int));
            _propertyName = (String)info.GetValue("PropertyName", typeof(string));
            _direction = (ListSortDirection)info.GetValue("Direction", typeof(ListSortDirection));
        }
                
        public int ColumnIndex
        {
            get { return _columnIndex; }
            set { _columnIndex = value; }
        }

        public string PropertyName
        {
            get { return _propertyName; }
            set { _propertyName = value; }
        }

        public ListSortDirection Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

        #region ISerializable Members

        //Serialization function.
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("ColumnIndex", _columnIndex);
            info.AddValue("PropertyName", _propertyName);
            info.AddValue("Direction", _direction);
        }

        #endregion
    }



}
