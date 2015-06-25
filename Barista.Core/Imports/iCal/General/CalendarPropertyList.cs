namespace Barista.DDay.iCal
{
    using Barista.DDay.Collections;
    using System;
    using System.Linq;

    [Serializable]
    public class CalendarPropertyList :
        GroupedValueList<string, ICalendarProperty, CalendarProperty, object>,
        ICalendarPropertyList
    {
        #region Private Fields

        private readonly ICalendarObject m_parent;
        private readonly bool m_caseInsensitive;

        #endregion

        #region Constructors

        public CalendarPropertyList()
        {
        }

        public CalendarPropertyList(ICalendarObject parent, bool caseInsensitive)
        {
            m_parent = parent;
            m_caseInsensitive = caseInsensitive;

            ItemAdded += CalendarPropertyList_ItemAdded;
            ItemRemoved += CalendarPropertyList_ItemRemoved;
        }

        #endregion
        
        #region Event Handlers

        void CalendarPropertyList_ItemRemoved(object sender, ObjectEventArgs<ICalendarProperty, int> e)
        {
            e.First.Parent = null;
        }

        void CalendarPropertyList_ItemAdded(object sender, ObjectEventArgs<ICalendarProperty, int> e)
        {
            e.First.Parent = m_parent;
        }

        #endregion

        protected override string GroupModifier(string group)
        {
            if (m_caseInsensitive && group != null)
                return group.ToUpper();
            return group;
        }

        public ICalendarProperty this[string name]
        {
            get
            {
                if (ContainsKey(name))
                {
                    return AllOf(name)
                        .FirstOrDefault();
                }
                return null;
            }            
        }
    }
}
