using System;
using System.Collections.Generic;
using System.Text;
using Barista.DDay.Collections;

namespace Barista.DDay.iCal
{
    public interface ICalendarParameter :
        ICalendarObject,
        IValueObject<string>
    {
        string Value { get; set; }
    }
}
