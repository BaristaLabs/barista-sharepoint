namespace Barista.DDay.iCal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    /// A class that contains time zone information, and is usually accessed
    /// from an iCalendar object using the <see cref="DDay.iCal.iCalendar.GetTimeZone"/> method.        
    /// </summary>
    [Serializable]
    public sealed class iCalTimeZoneInfo : 
        CalendarComponent,
        ITimeZoneInfo
    {
        #region Private Fields

        TimeZoneInfoEvaluator m_evaluator;

        #endregion

        #region Constructors

        public iCalTimeZoneInfo()
        {
            // FIXME: how do we ensure SEQUENCE doesn't get serialized?
            //base.Sequence = null;
            // iCalTimeZoneInfo does not allow sequence numbers
            // Perhaps we should have a custom serializer that fixes this?

            Initialize();
        }
        public iCalTimeZoneInfo(string name) : this()
        {
            this.Name = name;
        }

        void Initialize()
        {
            m_evaluator = new TimeZoneInfoEvaluator(this);
            SetService(m_evaluator);
        }

        #endregion

        #region Overrides
        
        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        public override bool Equals(object obj)
        {
            var tzi = obj as iCalTimeZoneInfo;
            if (tzi != null)
            {
                return object.Equals(TimeZoneName, tzi.TimeZoneName) &&
                    object.Equals(OffsetFrom, tzi.OffsetFrom) &&
                    object.Equals(OffsetTo, tzi.OffsetTo);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (m_evaluator != null ? m_evaluator.GetHashCode() : 0);
            }
        }
                              
        #endregion

        #region ITimeZoneInfo Members

        public string TZID
        {
            get
            {
                ITimeZone tz = Parent as ITimeZone;
                if (tz != null)
                    return tz.TZID;
                return null;
            }
        }

        /// <summary>
        /// Returns the name of the current Time Zone.
        /// <example>
        ///     The following are examples:
        ///     <list type="bullet">
        ///         <item>EST</item>
        ///         <item>EDT</item>
        ///         <item>MST</item>
        ///         <item>MDT</item>
        ///     </list>
        /// </example>
        /// </summary>
        public string TimeZoneName
        {
            get
            {
                if (TimeZoneNames.Count > 0)
                    return TimeZoneNames[0];
                return null;
            }
            set
            {
                TimeZoneNames.Clear();
                TimeZoneNames.Add(value);
            }
        }

        public IUTCOffset TZOffsetFrom
        {
            get { return OffsetFrom; }
            set { OffsetFrom = value; }
        }

        public IUTCOffset OffsetFrom
        {
            get { return Properties.Get<IUTCOffset>("TZOFFSETFROM"); }
            set { Properties.Set("TZOFFSETFROM", value); }
        }

        public IUTCOffset OffsetTo
        {
            get { return Properties.Get<IUTCOffset>("TZOFFSETTO"); }
            set { Properties.Set("TZOFFSETTO", value); }
        }

        public IUTCOffset TZOffsetTo
        {
            get { return OffsetTo; }
            set { OffsetTo = value; }
        }

        public IList<string> TimeZoneNames
        {
            get { return Properties.GetMany<string>("TZNAME"); }
            set { Properties.Set("TZNAME", value); }
        }

        public TimeZoneObservance? GetObservance(IDateTime dt)
        {
            if (Parent == null)
                throw new Exception("Cannot call GetObservance() on a TimeZoneInfo whose Parent property is null.");
                        
            if (string.Equals(dt.TZID, TZID))
            {
                // Normalize date/time values within this time zone to a local value.
                DateTime normalizedDt = dt.Value;

                // Let's evaluate our time zone observances to find the 
                // observance that applies to this date/time value.
                IEvaluator parentEval = Parent.GetService(typeof(IEvaluator)) as IEvaluator;
                if (parentEval != null)
                {
                    // Evaluate the date/time in question.
                    parentEval.Evaluate(Start, DateUtil.GetSimpleDateTimeData(Start), normalizedDt, true);

                    // NOTE: We avoid using period.Contains here, because we want to avoid
                    // doing an inadvertent time zone lookup with it.
                    var period = m_evaluator
                        .Periods
                        .FirstOrDefault(p =>
                            p.StartTime.Value <= normalizedDt &&
                            p.EndTime.Value > normalizedDt
                        );

                    if (period != null)
                    {
                        return new TimeZoneObservance(period, this);
                    }
                }
            }

            return null;            
        }

        public bool Contains(IDateTime dt)
        {
            var retval = GetObservance(dt);
            return (retval.HasValue);
        }

        #endregion

        #region IRecurrable Members

        public IDateTime DTStart
        {
            get { return Start; }
            set { Start = value; }
        }

        public IDateTime Start
        {
            get { return Properties.Get<IDateTime>("DTSTART"); }
            set { Properties.Set("DTSTART", value); }
        }

        public IList<IPeriodList> ExceptionDates
        {
            get { return Properties.GetMany<IPeriodList>("EXDATE"); }
            set { Properties.Set("EXDATE", value); }
        }

        public IList<IRecurrencePattern> ExceptionRules
        {
            get { return Properties.GetMany<IRecurrencePattern>("EXRULE"); }
            set { Properties.Set("EXRULE", value); }
        }

        public IList<IPeriodList> RecurrenceDates
        {
            get { return Properties.GetMany<IPeriodList>("RDATE"); }
            set { Properties.Set("RDATE", value); }
        }

        public IList<IRecurrencePattern> RecurrenceRules
        {
            get { return Properties.GetMany<IRecurrencePattern>("RRULE"); }
            set { Properties.Set("RRULE", value); }
        }

        public IDateTime RecurrenceID
        {
            get { return Properties.Get<IDateTime>("RECURRENCE-ID"); }
            set { Properties.Set("RECURRENCE-ID", value); }
        }

        #endregion

        #region IRecurrable Members

        public void ClearEvaluation()
        {
            RecurrenceUtil.ClearEvaluation(this);
        }

        public IList<Occurrence> GetOccurrences(IDateTime dt)
        {
            return RecurrenceUtil.GetOccurrences(this, dt, true);
        }

        public IList<Occurrence> GetOccurrences(DateTime dt)
        {
            return RecurrenceUtil.GetOccurrences(this, new iCalDateTime(dt), true);
        }

        public IList<Occurrence> GetOccurrences(IDateTime startTime, IDateTime endTime)
        {
            return RecurrenceUtil.GetOccurrences(this, startTime, endTime, true);
        }

        public IList<Occurrence> GetOccurrences(DateTime startTime, DateTime endTime)
        {
            return RecurrenceUtil.GetOccurrences(this, new iCalDateTime(startTime), new iCalDateTime(endTime), true);
        }

        #endregion
    }    
}
