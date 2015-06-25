using System;
using System.Collections.Generic;
using System.Text;

namespace Barista.DDay.iCal
{
    public interface IPeriod :
        IEncodableDataType,
        IComparable<IPeriod>
    {
        IDateTime StartTime { get; set; }
        IDateTime EndTime { get; set; }
        TimeSpan Duration { get; set; }
        bool MatchesDateOnly { get; set; }

        bool Contains(IDateTime dt);
        bool CollidesWith(IPeriod period);
    }
}
