using System;
using System.Collections.Generic;
using System.Text;

namespace Barista.DDay.iCal
{
    public interface ICalendarDataType :
        ICalendarParameterCollectionContainer,
        ICopyable,
        IServiceProvider
    {
        Type GetValueType();
        void SetValueType(string type);
        ICalendarObject AssociatedObject { get; set; }
        IICalendar Calendar { get; }

        string Language { get; set; }
    }
}
