using System;
using System.Collections.Generic;
using System.Text;
using Barista.DDay.Collections;

namespace Barista.DDay.iCal
{    
    public interface ICalendarObjectList<TType> : 
        IGroupedCollection<string, TType>
        where TType : class, ICalendarObject
    {
        TType this[int index] { get; }
    }
}
