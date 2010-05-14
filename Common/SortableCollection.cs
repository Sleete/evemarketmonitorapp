using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Collections.Generic;


namespace EveMarketMonitorApp.Common
{
    /// <summary>
    /// Summary description for SortableCollection.
    /// </summary>
    public abstract class SortableCollection : CollectionBase, ICloneable, IBindingList
    {
        // We want to cache the property comparers we create as there is an overhead on creating them
        // that could cancel out the performance gain in certain situations...
        private Dictionary<string, ObjectPropertyComparer> comparerCache;

        public SortableCollection()
        {
            comparerCache = new Dictionary<string, ObjectPropertyComparer>();
        }


        #region IClonable
        public object Clone()
        {
            Object Rt = Activator.CreateInstance(this.GetType());
            for (int i = 0; i < this.Count; i++)
            {
                ((SortableCollection)Rt).InnerList.Add(((SortableObject)this[i]).Clone());
            }
            //((SortableCollection )Rt).FiltredItems = (SortableCollection) this.FiltredItems.Clone();
            //deleteditem à ajouter
            return Rt;
        }
        #endregion
        #region My Methods
        public bool IsDirty
        {
            get
            {
                bool Rt = false;
                if (((SortableCollection)this.GetChanges()).Count != 0)
                { Rt = true; }
                return Rt;
            }
        }
        private Type _ItemType;
        public Type ItemType
        {
            get { return _ItemType; }
            set { _ItemType = value; }
        }
        public void Sort(string Expression)
        {
            try
            {
                string[] SplittedExpr = Expression.Split(',');
                string PropertyName;
                ListSortDirection Direction;
                for (int i = 0; i <= SplittedExpr.GetUpperBound(0); i++)
                {

                    if (-SplittedExpr[i].ToUpper().IndexOf(" ASC") < 0)
                    {
                        PropertyName = SplittedExpr[i].Replace(" ASC", "").Trim();
                        Direction = ListSortDirection.Ascending;
                    }
                    else if (-SplittedExpr[i].ToUpper().IndexOf(" DESC") < 0)
                    {
                        PropertyName = SplittedExpr[i].Replace(" DESC", "").Trim();
                        Direction = ListSortDirection.Descending;
                    }
                    else
                    {
                        PropertyName = SplittedExpr[i].Trim();
                        Direction = ListSortDirection.Ascending;
                    }
                    base.InnerList.Sort(new ObjectPropertyComparer(PropertyName, _ItemType));
                    if (Direction == ListSortDirection.Descending) base.InnerList.Reverse();

                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        public void Sort(List<SortInfo> sortData)
        {
            try
            {
                base.InnerList.Sort(new ObjectPropertyComparer(sortData, _ItemType));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        private bool ischangescontainer = false;
        public bool IsChangesContainer
        {
            get { return ischangescontainer; }
            set { ischangescontainer = value; }
        }
        private bool _IsFiltering = false;
        public bool IsFiltering
        {
            get { return _IsFiltering; }
            set { _IsFiltering = value; }
        }

        public SortableCollection GetChanges()
        {
            try
            {
                object Rt = Activator.CreateInstance(this.GetType());

                //Création d'une collection complète
                object WholeCollection = Activator.CreateInstance(this.GetType());

                //clonage 
                WholeCollection = (SortableCollection)this.Clone();

                //Ajout des lments filtrs

                for (int i = 0; i < this.FiltredItems.Count; i++)
                {
                    ((SortableCollection)WholeCollection).Add(this.FiltredItems[i]);
                }


                //Eléments Ajoutés et modifiés
                for (int i = 0; i < ((SortableCollection)WholeCollection).Count; i++)
                {
                    if (((SortableObject)((SortableCollection)WholeCollection)[i]).ObjectState != SortableObject.ObjectStateType.Unchanged)
                    {
                        ((SortableCollection)Rt).Add(((SortableCollection)WholeCollection)[i]);
                    }
                }

                //Elements supprimés
                for (int i = 0; i < DeletedItems.Count; i++)
                {
                    if (((SortableObject)DeletedItems[i]).IsPersistent)
                    {
                        ((SortableObject)DeletedItems[i]).ObjectState = SortableObject.ObjectStateType.Deleted;
                        ((SortableCollection)Rt).Add(DeletedItems[i]);
                    }
                }

                ((SortableCollection)Rt).IsChangesContainer = true;
                return (SortableCollection)Rt;

            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        private SortableCollection deleteditems; // clone de la Collection Cre juste aprs le chargement de la base de donnes
        public SortableCollection DeletedItems
        {
            get
            {
                if (deleteditems == null)
                {
                    deleteditems = (SortableCollection)Activator.CreateInstance(this.GetType());
                }
                return deleteditems;
            }
            set
            {
                deleteditems = value;
            }
        }
        private string filter = string.Empty;
        internal SortableCollection filtreditems; // Collection contenant les lments filtrs (Supprims);
        public SortableCollection FiltredItems
        {
            get
            {
                if (filtreditems == null)
                {
                    filtreditems = (SortableCollection)Activator.CreateInstance(this.GetType());
                }
                return filtreditems;
            }
            set
            {
                filtreditems = value;
            }

        }
        public string ItemFilter
        {
            get
            {
                return filter;
            }
            set
            {
                try
                {
                    if (value == filter) return;
                    this.IsFiltering = true;
                    //2- restauration de la collection   l'etat d'origine 
                    for (int i = 0; i <= this.FiltredItems.Count - 1; i++)
                    {
                        this.Add(this.FiltredItems[i]);
                    }

                    //Reset de la liste des lements filtrs
                    this.FiltredItems = (SortableCollection)Activator.CreateInstance(this.GetType());

                    //3- application du filtre si non vide
                    if (value != null & value != string.Empty)
                    {
                        Filter MyFilter = new Filter(this, value);
                    }
                    filter = value;

                    //4-Restauration du rafraichissement auto
                    OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, 0, 0));
                    //ListChanged += onListChanged;

                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }
        #endregion
        #region IList Members

        public bool IsReadOnly
        {
            get { return false; }
        }

        public object this[int index]
        {
            get
            {
                return InnerList[index];
            }
            set
            {
                InnerList[index] = value;
            }
        }

        public new void RemoveAt(int index)
        {
            this.Remove(this[index]);
        }

        public void Insert(int index, object value)
        {
            ((SortableObject)value).Parent = this;
            InnerList.Insert(index, value);
        }

        public void Remove(object value)
        {
            this.DeletedItems.Add(value);
            int index = IndexOf(value);
            SortableObject obj = (SortableObject)value;
            obj.Parent = null;
            InnerList.Remove(value);
            OnItemDeleted(new System.EventArgs(), index);
            if (!this.IsFiltering)
                OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));

        }

        public bool Contains(object value)
        {
            return InnerList.Contains(value);
        }

        public new void Clear()
        {
            for (int i = 0; i < this.Count; i++)
            {
                this.Remove(this[i]);
            }
        }

        public int IndexOf(object value)
        {
            return InnerList.IndexOf(value);
        }

        public int Add(object value)
        {
            ((SortableObject)value).Parent = this;
            int i = InnerList.Add(value);
            if (!this.IsFiltering)
                OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, IndexOf(value)));
            return i;
        }

        public bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        #endregion
        #region ICollection Members

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public new int Count
        {
            get
            {
                return InnerList.Count;
            }
        }

        public void CopyTo(Array array, int index)
        {
            InnerList.CopyTo(array, index);
        }

        public object SyncRoot
        {
            get
            {
                return InnerList.SyncRoot;
            }
        }

        #endregion
        #region IEnumerable Members

        public new IEnumerator GetEnumerator()
        {
            return InnerList.GetEnumerator();
        }

        #endregion
        #region Collection Base
        protected override void OnClearComplete()
        {
            OnListChanged(resetEvent);
        }

        protected override void OnInsertComplete(int index, object value)
        {

            SortableObject obj = (SortableObject)value;
            obj.Parent = this;
            if (!this.IsFiltering)
                OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
        }

        protected override void OnSetComplete(int index, object oldValue, object newValue)
        {
            if (oldValue != newValue)
            {
                SortableObject oldobj = (SortableObject)oldValue;
                SortableObject newobj = (SortableObject)newValue;

                oldobj.Parent = null;
                newobj.Parent = this;
                if (!this.IsFiltering)
                    OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
            }
        }
        #endregion
        #region IBindingList Members

        // Implements IBindingList.
        bool IBindingList.AllowEdit
        {
            get { return true; }
        }

        bool IBindingList.AllowNew
        {
            get { return true; }
        }

        bool IBindingList.AllowRemove
        {
            get { return true; }
        }

        bool IBindingList.SupportsChangeNotification
        {
            get { return true; }
        }


        bool IBindingList.SupportsSearching
        {
            get { return false; }
        }

        bool IBindingList.SupportsSorting
        {
            get { return true; }
        }

        object IBindingList.AddNew()
        {
            object obj = Activator.CreateInstance(this.ItemType);
            this.Add(obj);
            return obj;
        }

        private bool isSorted = false;

        bool IBindingList.IsSorted
        {
            get { return isSorted; }
        }

        private ListSortDirection listSortDirection = ListSortDirection.Ascending;

        ListSortDirection IBindingList.SortDirection
        {
            get { return listSortDirection; }
        }

        PropertyDescriptor sortProperty = null;

        PropertyDescriptor IBindingList.SortProperty
        {
            get { return sortProperty; }
        }

        void IBindingList.AddIndex(PropertyDescriptor property)
        {
            throw new NotSupportedException();
        }
        int IBindingList.Find(PropertyDescriptor property, object key)
        {
            throw new NotSupportedException();
        }

        void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            isSorted = true;
            sortProperty = property;
            listSortDirection = direction;

            ArrayList a = new ArrayList();

            IComparer comparer;
            if (comparerCache.ContainsKey(property.Name))
            {
                comparer = comparerCache[property.Name];
            }
            else
            {
                comparer = new ObjectPropertyComparer(property.Name, _ItemType);
            }
            base.InnerList.Sort(comparer);
            if (direction == ListSortDirection.Descending) base.InnerList.Reverse();
        }

        void IBindingList.RemoveIndex(PropertyDescriptor property)
        {
            throw new NotSupportedException();
        }

        void IBindingList.RemoveSort()
        {
            isSorted = false;
            sortProperty = null;
        }
        #endregion
        #region ListChanged Events
        internal ListChangedEventArgs resetEvent = new ListChangedEventArgs(ListChangedType.Reset, -1);
        private ListChangedEventHandler onListChanged;

        protected virtual void OnListChanged(ListChangedEventArgs ev)
        {
            if (onListChanged != null)
            {
                onListChanged(this, ev);
            }
        }
        internal void CollectionChanged(object obj)
        {
            int index = List.IndexOf(obj);
            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, index));
        }
        // Events.
        public event ListChangedEventHandler ListChanged
        {
            add
            {
                onListChanged += value;
            }
            remove
            {
                onListChanged -= value;
            }
        }
        #endregion
        #region events
        private EventHandler _ItemDeleted;
        public event EventHandler ItemDeleted
        {
            add
            {
                _ItemDeleted += value;
            }
            remove
            {
                _ItemDeleted -= value;
            }
        }
        protected virtual void OnItemDeleted(EventArgs args, int index)
        {
            if (_ItemDeleted != null)
            {
                _ItemDeleted(this, args);
            }
        }
        #endregion
    }

    #region ObjectPropertyComparer. Used for sorting.
    /// <summary>
    /// Summary description for ObjectComparer.
    /// </summary>
    public class ObjectPropertyComparer : IComparer
    {
        const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        //private string PropertyName;
        private List<SortInfo> PropertyNames;

        private Type type = null;
        //private PropertyInfo propertyInfo = null;
        private Dictionary<string, PropertyInfo> propertyInfo = null;
        //private GetHandler getPropertyVal = null;
        private Dictionary<string, GetHandler> getPropertyVal = null;

        /// <summary>
        /// Provides Comparison opreations.
        /// </summary>
        /// <param name="propertyName">The property to compare</param>
        public ObjectPropertyComparer(List<SortInfo> propertyNames, Type objectType)
        {
            PropertyNames = propertyNames;
            type = objectType;
            propertyInfo = new Dictionary<string, PropertyInfo>();
            foreach (SortInfo info in propertyNames)
            {
                propertyInfo.Add(info.PropertyName, type.GetProperty(info.PropertyName, BINDING_FLAGS));
            }
        }

        public ObjectPropertyComparer(string propertyName, Type objectType)
        {
            PropertyNames = new List<SortInfo>();
            SortInfo info = new SortInfo(0, propertyName);
            // Always sort ascending, the order will be reversed later if needed.
            info.Direction = ListSortDirection.Ascending;
            PropertyNames.Add(info);

            type = objectType;
            propertyInfo = new Dictionary<string, PropertyInfo>();
            propertyInfo.Add(info.PropertyName, type.GetProperty(info.PropertyName, BINDING_FLAGS));

        }
        /// <summary>
        /// Compares 2 objects by their properties, given on the constructor
        /// </summary>
        /// <param name="x">First value to compare</param>
        /// <param name="y">Second value to compare</param>
        /// <returns></returns>
        public int Compare(object x, object y)
        {
            int retVal = 0;
            int propertyIndex = 0;

            if (getPropertyVal == null)
            {
                getPropertyVal = new Dictionary<string, GetHandler>();
                //getPropertyVal = DynamicMethodCompiler.CreateGetHandler(type, propertyInfo);
            }

            while (retVal == 0 && propertyIndex < PropertyNames.Count)
            {
                string property = PropertyNames[propertyIndex].PropertyName;
                if (!getPropertyVal.ContainsKey(property))
                {
                    getPropertyVal.Add(property,
                        DynamicMethodCompiler.CreateGetHandler(type, propertyInfo[property]));
                }


                object a = getPropertyVal[property](x);
                object b = getPropertyVal[property](y);

                //object a = x.GetType().GetProperty(PropertyName).GetValue(x, null);
                //object b = y.GetType().GetProperty(PropertyName).GetValue(y, null);
                if (a != null && b == null) { retVal = 1; }
                else if (a == null && b != null) { retVal = -1; }
                else if (a == null && b == null) { retVal = 0; }
                else
                {
                    string sa = a as string;
                    if (sa != null)
                    {
                        string sb = b as string;
                        retVal = string.Compare(sa, sb, StringComparison.Ordinal);
                    }
                    else
                    {
                        retVal = ((IComparable)a).CompareTo(b);
                    }
                }

                retVal *= (PropertyNames[propertyIndex].Direction == ListSortDirection.Ascending ? 1 : -1);
                propertyIndex++;
            }

            return retVal;
        }
    }
    #endregion
    #region Expression object and collection. Used for filtering.
    /// <summary>
    /// Summary description for Filter.
    /// </summary>
    public class Expressions : CollectionBase
    {
        public Expressions(string HoleFilterExpression)
        {
            try
            {
                this.SplitFilter(HoleFilterExpression);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        public Expressions()
        {

        }
        public void SplitFilter(string HoleFilterExpression)
        {
            try
            {
                int LastPosition = 0;

                //AND
                for (int j = 5; j <= HoleFilterExpression.Length - 5; j++)
                {

                    string FiveCurrentChars = HoleFilterExpression.Substring(j - 5, 5).ToUpper();
                    if (FiveCurrentChars == " AND ")
                    {


                        this.Add(new Expression(HoleFilterExpression.Substring(LastPosition, j - LastPosition - 5)));
                        LastPosition = j;
                    }
                }
                //OR
                for (int z = 4; z <= HoleFilterExpression.Length - 4; z++)
                {
                    string TowCurrentChars = HoleFilterExpression.Substring(z - 4, 4).ToUpper();
                    if (TowCurrentChars == " OR ")
                    {
                        this.Add(new Expression(HoleFilterExpression.Substring(LastPosition, z - LastPosition - 4)));
                        LastPosition = z;
                    }
                }

                //Ajouter le dernier élement ou le premier si aucun AND/OR
                this.Add(new Expression(HoleFilterExpression.Substring(LastPosition)));
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        public Expression Item(int Index)
        {
            return (Expression)List[Index];

        }

        public int Add(Expression value)
        {

            return List.Add(value);
        }



        public void Remove(Expression value)
        {

            List.Remove(value);
        }

        internal void ElementChanged(Expression cust)
        {

            int index = List.IndexOf(cust);


        }
    }
    public class Expression
    {
        private string TmpPropertyValue;
        private string TmpOperator;
        private string TmpUserValue;
        public Expression()
        {
        }
        public Expression(string PropValue, string Opr, string Usrvalue)
        {
            this.PropertyName = PropValue;
            this.Operator = Opr;
            this.UserValue = this.UserValue = Usrvalue;
        }
        public Expression(string HoleExpression)
        {
            try
            {
                string[] Words = new string[2];
                Words = HoleExpression.Split(new char[1] { ' ' }, 3);
                this.PropertyName = Words[0];
                this.Operator = Words[1].Trim();
                this.UserValue = Words[2].Trim();
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        public string PropertyName
        {
            get { return TmpPropertyValue; }
            set { TmpPropertyValue = value; }
        }
        public string Operator
        {
            get { return TmpOperator; }
            set { TmpOperator = value; }
        }
        public string UserValue
        {
            get { return TmpUserValue; }
            set { TmpUserValue = value; }
        }
    }
    #endregion
    #region Filter object
    public class Filter
    {

        public Filter()
        {
        }
        public Filter(object ObjToFilter, string Filter)
        {

            this.ApplyFilter(ObjToFilter, Filter);
        }
        private bool IsOk(string ObjectPropertyValue, string Operator, string UserValue)
        {
            bool Rt = false;
            try
            {
                if (Operator == "=")
                {
                    if (ObjectPropertyValue.TrimEnd() == UserValue.TrimEnd()) { Rt = true; }
                }
                else if (Operator == "<>" | Operator == "!=")
                {
                    if (ObjectPropertyValue.TrimEnd() != UserValue.TrimEnd()) { Rt = true; }
                }
                else if (Operator.ToUpper() == "LIKE")
                {
                    //recherche des étoiles
                    int LastStrar = UserValue.LastIndexOf("*");
                    int UserValueLenght = UserValue.Length;
                    string SearchedText = UserValue.Replace("*", "");
                    int TextPosition = ObjectPropertyValue.IndexOf(SearchedText);
                    if (TextPosition != -1)
                    {
                        { Rt = true; }
                    }
                }
                else
                {
                    throw new Exception("L'opérateur '" + Operator + "' n'est pas géré pour le type string !");
                }
                return Rt;
            }
            catch (System.Exception ex)
            { throw ex; }
        }
        private bool IsOk(int ObjectPropertyValue, string Operator, int UserValue)
        {
            bool Rt = false;
            try
            {
                if (Operator == "=")
                {
                    if (ObjectPropertyValue == UserValue) { Rt = true; }
                }
                else if (Operator == ">")
                {
                    if (ObjectPropertyValue > UserValue) { Rt = true; }
                }
                else if (Operator == ">=")
                {
                    if (ObjectPropertyValue >= UserValue) { Rt = true; }
                }
                else if (Operator == "<")
                {
                    if (ObjectPropertyValue < UserValue) { Rt = true; }
                }
                else if (Operator == "<=")
                {
                    if (ObjectPropertyValue <= UserValue) { Rt = true; }
                }

                else if (Operator == "<>" | Operator == "!=")
                {
                    if (ObjectPropertyValue != UserValue) { Rt = true; }
                }

                else
                {
                    throw new Exception("L'opérateur '" + Operator + "' n'est pas géré pour le type int !");
                }
                return Rt;

            }
            catch (System.Exception ex)
            { throw ex; }
        }

        private bool IsOk(decimal ObjectPropertyValue, string Operator, int UserValue)
        {
            bool Rt = false;
            try
            {
                if (Operator == "=")
                {
                    if (ObjectPropertyValue == UserValue) { Rt = true; }
                }
                else if (Operator == ">")
                {
                    if (ObjectPropertyValue > UserValue) { Rt = true; }
                }
                else if (Operator == ">=")
                {
                    if (ObjectPropertyValue >= UserValue) { Rt = true; }
                }
                else if (Operator == "<")
                {
                    if (ObjectPropertyValue < UserValue) { Rt = true; }
                }
                else if (Operator == "<=")
                {
                    if (ObjectPropertyValue <= UserValue) { Rt = true; }
                }

                else if (Operator == "<>" | Operator == "!=")
                {
                    if (ObjectPropertyValue != UserValue) { Rt = true; }
                }

                else
                {
                    throw new Exception("L'opérateur '" + Operator + "' n'est pas géré pour le type int !");
                }
                return Rt;

            }
            catch (System.Exception ex)
            { throw ex; }
        }

        private bool IsOk(Guid ObjectPropertyValue, string Operator, Guid UserValue)
        {
            bool Rt = false;
            try
            {
                if (Operator == "=")
                {
                    if (ObjectPropertyValue == UserValue) { Rt = true; }
                }
                else
                {
                    throw new Exception("L'opérateur '" + Operator + "' n'est pas géré pour le type Guid !");
                }
                return Rt;

            }
            catch (System.Exception ex)
            { throw ex; }
        }
        private bool IsOk(double ObjectPropertyValue, string Operator, double UserValue)
        {
            bool Rt = false;
            try
            {
                if (Operator == "=")
                {
                    if (ObjectPropertyValue == UserValue) { Rt = true; }
                }
                else if (Operator == ">")
                {
                    if (ObjectPropertyValue > UserValue) { Rt = true; }
                }
                else if (Operator == ">=")
                {
                    if (ObjectPropertyValue >= UserValue) { Rt = true; }
                }
                else if (Operator == "<")
                {
                    if (ObjectPropertyValue < UserValue) { Rt = true; }
                }
                else if (Operator == "<=")
                {
                    if (ObjectPropertyValue <= UserValue) { Rt = true; }
                }
                else
                {

                    throw new Exception("L'opérateur '" + Operator + "' n'est pas géré pour le type double !");
                }
                return Rt;
            }
            catch (System.Exception ex)
            { throw ex; }
        }
        private bool IsOk(long ObjectPropertyValue, string Operator, long UserValue)
        {
            bool Rt = false;
            try
            {
                if (Operator == "=")
                {
                    if (ObjectPropertyValue == UserValue) { Rt = true; }
                }
                else if (Operator == ">")
                {
                    if (ObjectPropertyValue > UserValue) { Rt = true; }
                }
                else if (Operator == ">=")
                {
                    if (ObjectPropertyValue >= UserValue) { Rt = true; }
                }
                else if (Operator == "<")
                {
                    if (ObjectPropertyValue < UserValue) { Rt = true; }
                }
                else if (Operator == "<=")
                {
                    if (ObjectPropertyValue <= UserValue) { Rt = true; }
                }
                else
                {
                    throw new Exception("L'opérateur '" + Operator + "' n'est pas géré pour le type long !");
                }
                return Rt;
            }
            catch (System.Exception ex)
            { throw ex; }
        }
        private bool IsOk(decimal ObjectPropertyValue, string Operator, decimal UserValue)
        {
            bool Rt = false;
            try
            {
                if (Operator == "=")
                {
                    if (ObjectPropertyValue == UserValue) { Rt = true; }
                }
                else if (Operator == ">")
                {
                    if (ObjectPropertyValue > UserValue) { Rt = true; }
                }
                else if (Operator == ">=")
                {
                    if (ObjectPropertyValue >= UserValue) { Rt = true; }
                }
                else if (Operator == "<")
                {
                    if (ObjectPropertyValue < UserValue) { Rt = true; }
                }
                else if (Operator == "<=")
                {
                    if (ObjectPropertyValue <= UserValue) { Rt = true; }
                }
                else
                {

                    throw new Exception("L'opérateur '" + Operator + "' n'est pas géré pour le type decimal !");
                }
                return Rt;
            }
            catch (System.Exception ex)
            { throw ex; }
        }
        private bool IsOk(DateTime ObjectPropertyValue, string Operator, DateTime UserValue)
        {
            bool Rt = false;
            try
            {
                if (Operator == "=")
                {
                    if (ObjectPropertyValue == UserValue) { Rt = true; }
                }
                else if (Operator == ">")
                {
                    if (ObjectPropertyValue > UserValue) { Rt = true; }
                }
                else if (Operator == ">=")
                {
                    if (ObjectPropertyValue >= UserValue) { Rt = true; }
                }
                else if (Operator == "<")
                {
                    if (ObjectPropertyValue < UserValue) { Rt = true; }
                }
                else if (Operator == "<=")
                {
                    if (ObjectPropertyValue <= UserValue) { Rt = true; }
                }
                else
                {

                    throw new Exception("L'opérateur '" + Operator + "' n'est pas géré pour le type DateTime !");
                }
                return Rt;
            }
            catch (System.Exception ex)
            { throw ex; }
        }
        private bool IsOk(bool ObjectPropertyValue, string Operator, bool UserValue)
        {
            bool Rt = false;
            try
            {
                if (Operator == "=")
                {
                    if (ObjectPropertyValue == UserValue) { Rt = true; }
                }
                else
                {
                    throw new Exception("L'opérateur '" + Operator + "' n'est pas géré pour le type string !");
                }
                return Rt;
            }
            catch (System.Exception ex)
            { throw ex; }
        }
        private bool IsOk(string Operator, object UserValue)
        {
            bool Rt = false;
            try
            {
                if (UserValue.ToString().ToUpper() == "NULL" & Operator == "=")
                {
                    Rt = true;
                }
                return Rt;
            }
            catch (System.Exception ex)
            { throw ex; }
        }
        private string CorrectUserValue(string UserValue)
        {
            try
            {
                if (UserValue.Substring(0, 1) == "'")
                {
                    UserValue = UserValue.Replace("'", "");
                }

                if (UserValue.Substring(0, 1) == "#")
                {
                    UserValue = UserValue.Replace("#", "");
                }

                return UserValue;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        private bool IsOk(object ObjectPropertyValue, string Operator, string UserValue)
        {

            bool Rt = false;
            try
            {
                //Type of the property value
                if (ObjectPropertyValue == null)
                {
                }
                Type TypeOfValue = ObjectPropertyValue.GetType();
                //Object FilterValue  = Convert.ChangeType(CorrectUserValue(UserValue),TypeOfValue) ;
                Object FilterValue = CorrectUserValue(UserValue);

                if (TypeOfValue == typeof(string))
                { Rt = IsOk((string)ObjectPropertyValue, Operator, (string)FilterValue); }

                else if (TypeOfValue == typeof(int))
                { Rt = IsOk((int)ObjectPropertyValue, Operator, (int)Convert.ToInt32(FilterValue)); }

                else if (TypeOfValue == typeof(double))
                { Rt = IsOk((double)ObjectPropertyValue, Operator, (double)Convert.ToDouble(FilterValue)); }

                else if (TypeOfValue == typeof(decimal))
                { Rt = IsOk((decimal)ObjectPropertyValue, Operator, (decimal)Convert.ToDecimal(FilterValue)); }

                else if (TypeOfValue == typeof(DateTime))
                { Rt = IsOk((DateTime)ObjectPropertyValue, Operator, (DateTime)Convert.ToDateTime(FilterValue)); }

                else if (TypeOfValue == typeof(bool))
                { Rt = IsOk((bool)ObjectPropertyValue, Operator, (bool)Convert.ToBoolean(FilterValue)); }

                else if (TypeOfValue == typeof(Guid))
                { Rt = IsOk((Guid)ObjectPropertyValue, Operator, new Guid(FilterValue.ToString())); }
                else if (TypeOfValue == typeof(decimal))
                {
                    { Rt = IsOk((decimal)ObjectPropertyValue, Operator, (decimal)Convert.ToDecimal(FilterValue)); }
                }
                else if (TypeOfValue == typeof(long))
                {
                    { Rt = IsOk((long)ObjectPropertyValue, Operator, (long)Convert.ToInt64(FilterValue)); }
                }
                else
                { throw new Exception("Filtrage impossible sur le Type " + TypeOfValue.ToString()); }
                return Rt;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }


        }
        public void ApplyFilter(object objToFilter, string StrFilter)
        {
            try
            {
                //DateTime D1 = DateTime.UtcNow;

                SortableCollection ObjectToFilter = (SortableCollection)objToFilter;
                int CountValue = ObjectToFilter.Count;
                Expressions ListOfExpressions = new Expressions(StrFilter);


                //Loading items of the collection
                bool[] Validations;
                bool AllIsOK;

                PropertyInfo[] PropsInfo = new PropertyInfo[ListOfExpressions.Count];
                for (int x = 0; x < ListOfExpressions.Count; x++)
                {
                    PropsInfo[x] = ObjectToFilter.ItemType.GetProperty(ListOfExpressions.Item(x).PropertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (PropsInfo[x] == null)
                    {
                        throw new Exception("property " + ListOfExpressions.Item(x).PropertyName + " does not exists!");
                    }
                }

                for (int f = 0; f <= (ObjectToFilter.Count - 1); f++)
                {
                    object CollectionItem = ObjectToFilter[f];
                    Type TypeOfItem = CollectionItem.GetType();

                    Validations = new bool[ListOfExpressions.Count];
                    AllIsOK = true;

                    for (int t = 0; t <= ListOfExpressions.Count - 1; t++)
                    {
                        PropertyInfo ItemProperty = PropsInfo[t];
                        object PropertyValue = ItemProperty.GetValue(CollectionItem, new object[0]);

                        if (PropertyValue == null)
                        {
                            Validations[t] = IsOk(ListOfExpressions.Item(t).Operator, ListOfExpressions.Item(t).UserValue);
                        }
                        else
                        {
                            Validations[t] = this.IsOk(PropertyValue, ListOfExpressions.Item(t).Operator, ListOfExpressions.Item(t).UserValue);
                        }
                        if (Validations[t] == false)
                        { AllIsOK = false; }
                        //System.Diagnostics.Trace.WriteLine("time : " + (DateTime.UtcNow - timeSt1).TotalMilliseconds.ToString() + " cicle : " + cicle.ToString());
                        //timeSt1 = DateTime.UtcNow;
                        //cicle++;
                    }

                    if (AllIsOK == true)
                    {
                        ObjectToFilter.Remove(CollectionItem);
                        ObjectToFilter.filtreditems.Add(CollectionItem);
                        CountValue -= 1;
                        f -= 1;
                    }
                }

                //DateTime D2 = DateTime.UtcNow;
                //Console.WriteLine((D2 - D1).ToString());


            }
            catch (System.Exception ex)
            {
                throw ex;
            }

        }
    }
    #endregion
}
