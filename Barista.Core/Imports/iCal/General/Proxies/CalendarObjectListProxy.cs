using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Barista.DDay.Collections;

namespace Barista.DDay.iCal
{
    public class CalendarObjectListProxy<TType> :
        GroupedCollectionProxy<string, ICalendarObject, TType>,
        ICalendarObjectList<TType>
        where TType : class, ICalendarObject
    {
        public CalendarObjectListProxy(IGroupedCollection<string, ICalendarObject> list) : base(list)
        {
        }

        virtual public TType this[int index]
        {
            get
            {
                return this.Skip(index).FirstOrDefault();
            }
        }
    }
}
