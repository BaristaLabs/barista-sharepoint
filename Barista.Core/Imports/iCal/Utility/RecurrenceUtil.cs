namespace Barista.DDay.iCal
{
    using System.Collections.Generic;

    public class RecurrenceUtil
    {
        static public void ClearEvaluation(IRecurrable recurrable)
        {
            IEvaluator evaluator = recurrable.GetService(typeof(IEvaluator)) as IEvaluator;
            if (evaluator != null)
                evaluator.Clear();
        }

        static public IList<Occurrence> GetOccurrences(IRecurrable recurrable, IDateTime dt, bool includeReferenceDateInResults)
        {
            return GetOccurrences(
                recurrable, 
                new iCalDateTime(dt.Local.Date), 
                new iCalDateTime(dt.Local.Date.AddDays(1).AddSeconds(-1)),
                includeReferenceDateInResults);
        }

        static public IList<Occurrence> GetOccurrences(IRecurrable recurrable, IDateTime periodStart, IDateTime periodEnd, bool includeReferenceDateInResults)
        {
            var occurrences = new List<Occurrence>();

            var evaluator = recurrable.GetService(typeof(IEvaluator)) as IEvaluator;
            if (evaluator == null)
                return occurrences;

            // Ensure the start time is associated with the object being queried
            var start = recurrable.Start.Copy<IDateTime>();
            start.AssociatedObject = recurrable as ICalendarObject;

            // Change the time zone of periodStart/periodEnd as needed 
            // so they can be used during the evaluation process.
            periodStart = DateUtil.MatchTimeZone(start, periodStart);
            periodEnd = DateUtil.MatchTimeZone(start, periodEnd);

            IList<IPeriod> periods = evaluator.Evaluate(
                start,
                DateUtil.GetSimpleDateTimeData(periodStart),
                DateUtil.GetSimpleDateTimeData(periodEnd),
                includeReferenceDateInResults);

            foreach (IPeriod p in periods)
            {
                // Filter the resulting periods to only contain those 
                // that occur sometime between startTime and endTime.
                // NOTE: fixes bug #3007244 - GetOccurences not returning long spanning all-day events 
                var endTime = p.EndTime ?? p.StartTime;
                if (endTime.GreaterThan(periodStart) && p.StartTime.LessThanOrEqual(periodEnd))
                    occurrences.Add(new Occurrence(recurrable, p));
            }

            occurrences.Sort();
            return occurrences;
        }

        static public bool?[] GetExpandBehaviorList(IRecurrencePattern p)
        {
            // See the table in RFC 5545 Section 3.3.10 (Page 43).
            switch (p.Frequency)
            {                
                case FrequencyType.Minutely: return new bool?[] { false, null, false, false, false, false, false, true, false };
                case FrequencyType.Hourly:   return new bool?[] { false, null, false, false, false, false, true, true, false };
                case FrequencyType.Daily:    return new bool?[] { false, null, null, false, false, true, true, true, false };
                case FrequencyType.Weekly:   return new bool?[] { false, null, null, null, true, true, true, true, false };
                case FrequencyType.Monthly:
                    {
                        bool?[] row = { false, null, null, true, true, true, true, true, false };

                        // Limit if BYMONTHDAY is present; otherwise, special expand for MONTHLY.
                        if (p.ByMonthDay.Count > 0)
                            row[4] = false;

                        return row;
                    }
                case FrequencyType.Yearly:
                    {
                        bool?[] row = { true, true, true, true, true, true, true, true, false };

                        // Limit if BYYEARDAY or BYMONTHDAY is present; otherwise,
                        // special expand for WEEKLY if BYWEEKNO present; otherwise,
                        // special expand for MONTHLY if BYMONTH present; otherwise,
                        // special expand for YEARLY.
                        if (p.ByYearDay.Count > 0 || p.ByMonthDay.Count > 0)
                            row[4] = false;

                        return row;
                    }
                default:
                    return new bool?[] { false, null, false, false, false, false, false, false, false };
            }
        }
    }
}
