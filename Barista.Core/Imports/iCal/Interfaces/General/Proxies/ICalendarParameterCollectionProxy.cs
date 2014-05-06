using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Barista.DDay.Collections;

namespace Barista.DDay.iCal
{
    public interface ICalendarParameterCollectionProxy :
        ICalendarParameterCollection,
        IGroupedCollectionProxy<string, ICalendarParameter, ICalendarParameter>
    {                
    }
}
