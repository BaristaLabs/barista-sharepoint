namespace Barista.DDay.iCal
{
    using Barista.DDay.Collections;
    using System;

    /// <summary>
    /// A collection of calendar objects.
    /// </summary>
    [Serializable]
    public class CalendarObjectList :
        GroupedList<string, ICalendarObject>,
        ICalendarObjectList<ICalendarObject>
    {
        ICalendarObject m_parent;

        public CalendarObjectList(ICalendarObject parent)
        {
            m_parent = parent;
        }
    }
}
