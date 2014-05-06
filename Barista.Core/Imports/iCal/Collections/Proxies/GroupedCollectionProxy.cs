namespace Barista.DDay.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A proxy for a keyed list.
    /// </summary>
    [Serializable]
    public class GroupedCollectionProxy<TGroup, TOriginal, TNew> :
        IGroupedCollectionProxy<TGroup, TOriginal, TNew>
        where TOriginal : class, IGroupedObject<TGroup>
        where TNew : class, TOriginal
    {
        #region Private Fields

        private IGroupedCollection<TGroup, TOriginal> m_realObject;
        private readonly Func<TNew, bool> m_predicate;

        #endregion

        #region Constructors

        public GroupedCollectionProxy(IGroupedCollection<TGroup, TOriginal> realObject)
            : this(realObject, null)
        {
        }

        public GroupedCollectionProxy(IGroupedCollection<TGroup, TOriginal> realObject, Func<TNew, bool> predicate)
        {
            m_predicate = predicate ?? (o => true);
            SetProxiedObject(realObject);

            m_realObject.ItemAdded += _RealObject_ItemAdded;
            m_realObject.ItemRemoved += _RealObject_ItemRemoved;
        }

        #endregion

        #region Event Handlers

        void _RealObject_ItemRemoved(object sender, ObjectEventArgs<TOriginal, int> e)
        {
            if (e.First is TNew)
                OnItemRemoved((TNew)e.First, e.Second);
        }

        void _RealObject_ItemAdded(object sender, ObjectEventArgs<TOriginal, int> e)
        {
            if (e.First is TNew)
                OnItemAdded((TNew)e.First, e.Second);
        }

        #endregion

        #region IGroupedCollection Members

        virtual public event EventHandler<ObjectEventArgs<TNew, int>> ItemAdded;
        virtual public event EventHandler<ObjectEventArgs<TNew, int>> ItemRemoved;

        protected void OnItemAdded(TNew item, int index)
        {
            if (ItemAdded != null)
                ItemAdded(this, new ObjectEventArgs<TNew, int>(item, index));
        }

        protected void OnItemRemoved(TNew item, int index)
        {
            if (ItemRemoved != null)
                ItemRemoved(this, new ObjectEventArgs<TNew, int>(item, index));
        }

        virtual public bool Remove(TGroup group)
        {
            return m_realObject.Remove(group);
        }

        virtual public void Clear(TGroup group)
        {
            m_realObject.Clear(group);
        }

        virtual public bool ContainsKey(TGroup group)
        {
            return m_realObject.ContainsKey(group);            
        }

        virtual public int CountOf(TGroup group)
        {
            return m_realObject
                .AllOf(group)
                .OfType<TNew>()
                .Where(m_predicate)
                .Count();            
        }

        virtual public IEnumerable<TNew> AllOf(TGroup group)
        {
            return m_realObject
                .AllOf(group)
                .OfType<TNew>()
                .Where(m_predicate);
        }
 
        public virtual void SortKeys()
        {
            SortKeys(null);
        }

        virtual public void SortKeys(IComparer<TGroup> comparer)
        {
            m_realObject.SortKeys(comparer);
        }

        virtual public void Add(TNew item)
        {
            m_realObject.Add(item);
        }

        virtual public void Clear()
        {
            // Only clear items of this type
            // that match the predicate.

            var items = m_realObject
                .OfType<TNew>()
                .Where(m_predicate)
                .ToArray();

            foreach (TNew item in items)
            {
                m_realObject.Remove(item);
            }
        }

        virtual public bool Contains(TNew item)
        {
            return m_realObject.Contains(item);
        }

        virtual public void CopyTo(TNew[] array, int arrayIndex)
        {
            int i = 0;
            foreach (TNew item in this)
            {
                array[arrayIndex + (i++)] = item;
            }
        }

        virtual public int Count
        {
            get 
            { 
                return m_realObject
                    .OfType<TNew>()
                    .Where(m_predicate)
                    .Count(); 
            }
        }

        virtual public bool IsReadOnly
        {
            get { return false; }
        }

        virtual public bool Remove(TNew item)
        {
            return m_realObject.Remove(item);
        }

        virtual public IEnumerator<TNew> GetEnumerator()
        {
            return m_realObject
                .OfType<TNew>()
                .Where(m_predicate)
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_realObject
                .OfType<TNew>()
                .Where(m_predicate)
                .GetEnumerator();
        }

        #endregion

        #region IGroupedCollectionProxy Members

        public IGroupedCollection<TGroup, TOriginal> RealObject
        {
            get { return m_realObject; }
        }

        virtual public void SetProxiedObject(IGroupedCollection<TGroup, TOriginal> realObject)
        {
            m_realObject = realObject;
        }

        #endregion
    }
}
