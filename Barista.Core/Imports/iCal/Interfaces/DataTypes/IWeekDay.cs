using System;
using System.Collections.Generic;
using System.Text;

namespace Barista.DDay.iCal
{
    public interface IWeekDay :
        IEncodableDataType,
        IComparable
    {
        int Offset { get; set; }
        DayOfWeek DayOfWeek { get; set; }
    }
}
