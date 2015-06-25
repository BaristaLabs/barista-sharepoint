using System;
using System.Collections.Generic;
using System.Text;
using Barista.DDay.Collections;

namespace Barista.DDay.iCal
{
    public interface ICalendarPropertyList :
        IGroupedValueList<string, ICalendarProperty, CalendarProperty, object>
    {
        ICalendarProperty this[string name] { get; }
    }
}
