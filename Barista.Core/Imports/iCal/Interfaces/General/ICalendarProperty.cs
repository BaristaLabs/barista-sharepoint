using System;
using System.Collections.Generic;
using System.Text;
using Barista.DDay.Collections;

namespace Barista.DDay.iCal
{
    public interface ICalendarProperty :        
        ICalendarParameterCollectionContainer,
        ICalendarObject,
        IValueObject<object>
    {
        object Value { get; set; }
    }
}
