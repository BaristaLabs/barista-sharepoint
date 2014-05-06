namespace Barista.DDay.iCal
{
    using Barista.DDay.iCal.Serialization.iCalendar;
    using System;
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// The iCalendar equivalent of the .NET <see cref="DateTime"/> class.
    /// <remarks>
    /// In addition to the features of the <see cref="DateTime"/> class, the <see cref="iCalDateTime"/>
    /// class handles time zone differences, and integrates seamlessly into the iCalendar framework.
    /// </remarks>
    /// </summary>
    [Serializable]
    public sealed class iCalDateTime :
        EncodableDataType,
        IDateTime
    {
        #region Static Public Properties

        static public iCalDateTime Now
        {
            get
            {
                return new iCalDateTime(DateTime.Now);
            }
        }

        static public iCalDateTime Today
        {
            get
            {
                return new iCalDateTime(DateTime.Today);
            }
        }

        #endregion

        #region Private Fields

        private DateTime m_value;
        private bool m_hasDate;
        private bool m_hasTime;
        private TimeZoneObservance? m_timeZoneObservance;
        private bool m_isUniversalTime;

        #endregion

        #region Constructors

        public iCalDateTime() { }
        public iCalDateTime(IDateTime value)
        {
            Initialize(value.Value, value.TZID, null);
        }
        public iCalDateTime(DateTime value) : this(value, null) { }
        public iCalDateTime(DateTime value, string tzid)
        {
            Initialize(value, tzid, null);
        }
        public iCalDateTime(DateTime value, TimeZoneObservance tzo)
        {
            Initialize(value, tzo);
        }
        public iCalDateTime(int year, int month, int day, int hour, int minute, int second)
        {
            Initialize(year, month, day, hour, minute, second, null, null);
            HasTime = true;
        }
        public iCalDateTime(int year, int month, int day, int hour, int minute, int second, string tzid)
        {
            Initialize(year, month, day, hour, minute, second, tzid, null);
            HasTime = true;
        }
        public iCalDateTime(int year, int month, int day, int hour, int minute, int second, string tzid, IICalendar iCal)
        {
            Initialize(year, month, day, hour, minute, second, tzid, iCal);
            HasTime = true;
        }
        public iCalDateTime(int year, int month, int day)
            : this(year, month, day, 0, 0, 0) { }
        public iCalDateTime(int year, int month, int day, bool hasTime)
            : this(year, month, day, 0, 0, 0) { HasTime = hasTime; }
        public iCalDateTime(int year, int month, int day, string tzid)
            : this(year, month, day, 0, 0, 0, tzid) { }
        public iCalDateTime(int year, int month, int day, string tzid, bool hasTime)
            : this(year, month, day, 0, 0, 0, tzid) { HasTime = hasTime; }

        public iCalDateTime(string value)
        {
            DateTimeSerializer serializer = new DateTimeSerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        private void Initialize(int year, int month, int day, int hour, int minute, int second, string tzid, IICalendar iCal)
        {
            Initialize(CoerceDateTime(year, month, day, hour, minute, second, DateTimeKind.Local), tzid, iCal);
        }

        private void Initialize(DateTime value, string tzid, IICalendar iCal)
        {
            if (value.Kind == DateTimeKind.Utc)
                this.IsUniversalTime = true;

            // Convert all incoming values to UTC.
            this.Value = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            this.HasDate = true;
            this.HasTime = (value.Second != 0 || value.Minute != 0 || value.Hour != 0);
            this.TZID = tzid;
            this.AssociatedObject = iCal;
        }

        private void Initialize(DateTime value, TimeZoneObservance tzo)
        {
            if (value.Kind == DateTimeKind.Utc)
                this.IsUniversalTime = true;

            // Convert all incoming values to UTC.
            this.Value = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            this.HasDate = true;
            this.HasTime = (value.Second != 0 || value.Minute != 0 || value.Hour != 0);
            if (tzo.TimeZoneInfo != null)
                this.TZID = tzo.TimeZoneInfo.TZID;
            this.TimeZoneObservance = tzo;
            this.AssociatedObject = tzo.TimeZoneInfo;
        }

        private DateTime CoerceDateTime(int year, int month, int day, int hour, int minute, int second, DateTimeKind kind)
        {
            DateTime dt = DateTime.MinValue;

            // NOTE: determine if a date/time value exceeds the representable date/time values in .NET.
            // If so, let's automatically adjust the date/time to compensate.
            // FIXME: should we have a parsing setting that will throw an exception
            // instead of automatically adjusting the date/time value to the
            // closest representable date/time?
            try
            {
                if (year > 9999)
                    dt = DateTime.MaxValue;
                else if (year > 0)
                    dt = new DateTime(year, month, day, hour, minute, second, kind);
            }
            catch
            {
            }

            return dt;
        }

        #endregion

        #region Protected Methods

        private TimeZoneObservance? GetTimeZoneObservance()
        {
            if (m_timeZoneObservance == null &&
                TZID != null &&
                Calendar != null)
            {
                ITimeZone tz = Calendar.GetTimeZone(TZID);
                if (tz != null)
                    m_timeZoneObservance = tz.GetTimeZoneObservance(this);
            }
            return m_timeZoneObservance;
        }

        #endregion

        #region Overrides

        public override ICalendarObject AssociatedObject
        {
            get
            {
                return base.AssociatedObject;
            }
            set
            {
                if (!object.Equals(AssociatedObject, value))
                {
                    base.AssociatedObject = value;
                }
            }
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);

            IDateTime dt = obj as IDateTime;
            if (dt != null)
            {
                m_value = dt.Value;
                m_isUniversalTime = dt.IsUniversalTime;
                m_hasDate = dt.HasDate;
                m_hasTime = dt.HasTime;

                AssociateWith(dt);
            }
        }

        public override bool Equals(object obj)
        {
            var time = obj as IDateTime;
            if (time != null)
            {
                this.AssociateWith(time);
                return time.UTC.Equals(UTC);
            }

            if (!(obj is DateTime))
                return false;

            var dt = new iCalDateTime((DateTime)obj);
            this.AssociateWith(dt);
            return object.Equals(dt.UTC, UTC);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return ToString(null, null);
        }

        #endregion

        #region Operators

        public static bool operator <(iCalDateTime left, IDateTime right)
        {
            left.AssociateWith(right);

            if (left.HasTime || right.HasTime)
                return left.UTC < right.UTC;
            return left.UTC.Date < right.UTC.Date;
        }

        public static bool operator >(iCalDateTime left, IDateTime right)
        {
            left.AssociateWith(right);

            if (left.HasTime || right.HasTime)
                return left.UTC > right.UTC;
            return left.UTC.Date > right.UTC.Date;
        }

        public static bool operator <=(iCalDateTime left, IDateTime right)
        {
            left.AssociateWith(right);

            if (left.HasTime || right.HasTime)
                return left.UTC <= right.UTC;
            return left.UTC.Date <= right.UTC.Date;
        }

        public static bool operator >=(iCalDateTime left, IDateTime right)
        {
            left.AssociateWith(right);

            if (left.HasTime || right.HasTime)
                return left.UTC >= right.UTC;
            return left.UTC.Date >= right.UTC.Date;
        }

        public static bool operator ==(iCalDateTime left, IDateTime right)
        {
            if (left == null && right == null)
                return true;

            if (left == null)
                return false;

            if (right == null)
                return false;

            left.AssociateWith(right);

            if (left.HasTime || right.HasTime)
                return left.UTC.Equals(right.UTC);
            return left.UTC.Date.Equals(right.UTC.Date);
        }

        public static bool operator !=(iCalDateTime left, IDateTime right)
        {
            if (left == null && right == null)
                return true;

            if (left == null)
                return false;

            if (right == null)
                return false;

            left.AssociateWith(right);

            if (left.HasTime || right.HasTime)
                return !left.UTC.Equals(right.UTC);
            return !left.UTC.Date.Equals(right.UTC.Date);
        }

        public static TimeSpan operator -(iCalDateTime left, IDateTime right)
        {
            left.AssociateWith(right);
            return left.UTC - right.UTC;
        }

        public static IDateTime operator -(iCalDateTime left, TimeSpan right)
        {
            var copy = left.Copy<IDateTime>();
            copy.Value -= right;
            return copy;
        }

        public static IDateTime operator +(iCalDateTime left, TimeSpan right)
        {
            var copy = left.Copy<IDateTime>();
            copy.Value += right;
            return copy;
        }

        public static implicit operator iCalDateTime(DateTime left)
        {
            return new iCalDateTime(left);
        }

        #endregion

        #region IDateTime Members

        /// <summary>
        /// Converts the date/time to local time, according to the time
        /// zone specified in the TZID property.
        /// </summary>
        public DateTime Local
        {
            get
            {
                if (IsUniversalTime &&
                    TZID != null)
                {
                    var value = HasTime ? Value : Value.Date;

                    // Get the Time Zone Observance, if possible
                    var tzi = TimeZoneObservance;
                    if (tzi.HasValue)
                        tzi = GetTimeZoneObservance();

                    if (!tzi.HasValue)
                        return DateTime.SpecifyKind(HasTime ? Value : Value.Date, DateTimeKind.Local);

                    Debug.Assert(tzi.Value.TimeZoneInfo.OffsetTo != null);
                    return DateTime.SpecifyKind(tzi.Value.TimeZoneInfo.OffsetTo.ToLocal(value), DateTimeKind.Local);
                }
                return DateTime.SpecifyKind(HasTime ? Value : Value.Date, DateTimeKind.Local);
            }
        }

        /// <summary>
        /// Converts the date/time to UTC (Coordinated Universal Time)
        /// </summary>
        public DateTime UTC
        {
            get
            {
                if (IsUniversalTime)
                    return DateTime.SpecifyKind(Value, DateTimeKind.Utc);

                if (TZID == null)
                    return DateTime.SpecifyKind(Value, DateTimeKind.Local).ToUniversalTime();

                var value = Value;

                // Get the Time Zone Observance, if possible
                var tzi = TimeZoneObservance;

                if (!tzi.HasValue)
                    tzi = GetTimeZoneObservance();

                if (!tzi.HasValue)
                    return DateTime.SpecifyKind(Value, DateTimeKind.Local).ToUniversalTime();

                Debug.Assert(tzi.Value.TimeZoneInfo.OffsetTo != null);
                return DateTime.SpecifyKind(tzi.Value.TimeZoneInfo.OffsetTo.ToUTC(value), DateTimeKind.Utc);

                // Fallback to the OS-conversion
            }
        }

        /// <summary>
        /// Gets/sets the <see cref="iCalTimeZoneInfo"/> object for the time
        /// zone set by <see cref="TZID"/>.
        /// </summary>
        public TimeZoneObservance? TimeZoneObservance
        {
            get
            {
                return m_timeZoneObservance;
            }
            set
            {
                m_timeZoneObservance = value;
                if (value.HasValue &&
                    value.Value.TimeZoneInfo != null)
                {
                    this.TZID = value.Value.TimeZoneInfo.TZID;
                }
                else
                {
                    this.TZID = null;
                }
            }
        }

        public bool IsUniversalTime
        {
            get { return m_isUniversalTime; }
            set { m_isUniversalTime = value; }
        }

        public string TimeZoneName
        {
            get
            {
                if (IsUniversalTime)
                    return "UTC";

                return m_timeZoneObservance.HasValue
                    ? m_timeZoneObservance.Value.TimeZoneInfo.TimeZoneName
                    : string.Empty;
            }
        }

        public DateTime Value
        {
            get { return m_value; }
            set
            {
                if (!object.Equals(m_value, value))
                {
                    m_value = value;

                    // Reset the time zone info if the new date/time doesn't
                    // fall within this time zone observance.
                    if (m_timeZoneObservance.HasValue &&
                        !m_timeZoneObservance.Value.Contains(this))
                        m_timeZoneObservance = null;
                }

            }
        }

        public bool HasDate
        {
            get { return m_hasDate; }
            set { m_hasDate = value; }
        }

        public bool HasTime
        {
            get { return m_hasTime; }
            set { m_hasTime = value; }
        }

        public string TZID
        {
            get { return Parameters.Get("TZID"); }
            set
            {
                if (!object.Equals(TZID, value))
                {
                    Parameters.Set("TZID", value);

                    // Set the time zone observance to null if the TZID
                    // doesn't match.
                    if (value != null &&
                        m_timeZoneObservance.HasValue &&
                        m_timeZoneObservance.Value.TimeZoneInfo != null &&
                        !object.Equals(m_timeZoneObservance.Value.TimeZoneInfo.TZID, value))
                        m_timeZoneObservance = null;
                }
            }
        }

        public int Year
        {
            get { return Value.Year; }
        }

        public int Month
        {
            get { return Value.Month; }
        }

        public int Day
        {
            get { return Value.Day; }
        }

        public int Hour
        {
            get { return Value.Hour; }
        }

        public int Minute
        {
            get { return Value.Minute; }
        }

        public int Second
        {
            get { return Value.Second; }
        }

        public int Millisecond
        {
            get { return Value.Millisecond; }
        }

        public long Ticks
        {
            get { return Value.Ticks; }
        }

        public DayOfWeek DayOfWeek
        {
            get { return Value.DayOfWeek; }
        }

        public int DayOfYear
        {
            get { return Value.DayOfYear; }
        }

        public IDateTime FirstDayOfYear
        {
            get
            {
                IDateTime dt = Copy<IDateTime>();
                dt.Value = Value.AddDays(-Value.DayOfYear + 1).Date;
                return dt;
            }
        }

        public IDateTime FirstDayOfMonth
        {
            get
            {
                IDateTime dt = Copy<IDateTime>();
                dt.Value = Value.AddDays(-Value.Day + 1).Date;
                return dt;
            }
        }

        public DateTime Date
        {
            get { return Value.Date; }
        }

        public TimeSpan TimeOfDay
        {
            get { return Value.TimeOfDay; }
        }

        public IDateTime ToTimeZone(TimeZoneObservance tzo)
        {
            ITimeZoneInfo tzi = tzo.TimeZoneInfo;
            if (tzi != null)
                return new iCalDateTime(tzi.OffsetTo.ToLocal(UTC), tzo);
            return null;
        }

        public IDateTime ToTimeZone(ITimeZone tz)
        {
            if (tz == null)
                throw new ArgumentException("You must provide a valid time zone to the ToTimeZone() method", "tz");

            var tzi = tz.GetTimeZoneObservance(this);
            return tzi.HasValue ? ToTimeZone(tzi.Value) : Copy<IDateTime>();

            // FIXME: if the time cannot be resolved, should we
            // just provide a copy?  Is this always appropriate?
        }

        public IDateTime ToTimeZone(string tzid)
        {
            if (tzid != null)
            {
                if (Calendar != null)
                {
                    ITimeZone tz = Calendar.GetTimeZone(tzid);
                    if (tz != null)
                        return ToTimeZone(tz);

                    // FIXME: sometimes a calendar is perfectly valid but the time zone
                    // could not be resolved.  What should we do here?
                    //throw new Exception("The '" + tzid + "' time zone could not be resolved.");
                    return Copy<IDateTime>();
                }
                throw new Exception("The iCalDateTime object must have an iCalendar associated with it in order to use TimeZones.");
            }
            throw new ArgumentException("You must provide a valid TZID to the ToTimeZone() method", "tzid");
        }

        public IDateTime SetTimeZone(ITimeZone tz)
        {
            if (tz != null)
                this.TZID = tz.TZID;
            return this;
        }

        public IDateTime Add(TimeSpan ts)
        {
            return this + ts;
        }

        public IDateTime Subtract(TimeSpan ts)
        {
            return this - ts;
        }

        public TimeSpan Subtract(IDateTime dt)
        {
            return this - dt;
        }

        public IDateTime AddYears(int years)
        {
            IDateTime dt = Copy<IDateTime>();
            dt.Value = Value.AddYears(years);
            return dt;
        }

        public IDateTime AddMonths(int months)
        {
            IDateTime dt = Copy<IDateTime>();
            dt.Value = Value.AddMonths(months);
            return dt;
        }

        public IDateTime AddDays(int days)
        {
            IDateTime dt = Copy<IDateTime>();
            dt.Value = Value.AddDays(days);
            return dt;
        }

        public IDateTime AddHours(int hours)
        {
            IDateTime dt = Copy<IDateTime>();
            if (!dt.HasTime && hours % 24 > 0)
                dt.HasTime = true;
            dt.Value = Value.AddHours(hours);
            return dt;
        }

        public IDateTime AddMinutes(int minutes)
        {
            IDateTime dt = Copy<IDateTime>();
            if (!dt.HasTime && minutes % 1440 > 0)
                dt.HasTime = true;
            dt.Value = Value.AddMinutes(minutes);
            return dt;
        }

        public IDateTime AddSeconds(int seconds)
        {
            IDateTime dt = Copy<IDateTime>();
            if (!dt.HasTime && seconds % 86400 > 0)
                dt.HasTime = true;
            dt.Value = Value.AddSeconds(seconds);
            return dt;
        }

        public IDateTime AddMilliseconds(int milliseconds)
        {
            IDateTime dt = Copy<IDateTime>();
            if (!dt.HasTime && milliseconds % 86400000 > 0)
                dt.HasTime = true;
            dt.Value = Value.AddMilliseconds(milliseconds);
            return dt;
        }

        public IDateTime AddTicks(long ticks)
        {
            IDateTime dt = Copy<IDateTime>();
            dt.HasTime = true;
            dt.Value = Value.AddTicks(ticks);
            return dt;
        }

        public bool LessThan(IDateTime dt)
        {
            return this < dt;
        }

        public bool GreaterThan(IDateTime dt)
        {
            return this > dt;
        }

        public bool LessThanOrEqual(IDateTime dt)
        {
            return this <= dt;
        }

        public bool GreaterThanOrEqual(IDateTime dt)
        {
            return this >= dt;
        }

        public void AssociateWith(IDateTime dt)
        {
            if (AssociatedObject == null && dt.AssociatedObject != null)
                AssociatedObject = dt.AssociatedObject;
            else if (AssociatedObject != null && dt.AssociatedObject == null)
                dt.AssociatedObject = AssociatedObject;

            // If these share the same TZID, then let's see if we
            // can share the time zone observance also!
            if (TZID != null && string.Equals(TZID, dt.TZID))
            {
                if (TimeZoneObservance != null && dt.TimeZoneObservance == null)
                {
                    IDateTime normalizedDt = new iCalDateTime(TimeZoneObservance.Value.TimeZoneInfo.OffsetTo.ToUTC(dt.Value));
                    if (TimeZoneObservance.Value.Contains(normalizedDt))
                        dt.TimeZoneObservance = TimeZoneObservance;
                }
                else if (dt.TimeZoneObservance != null && TimeZoneObservance == null)
                {
                    IDateTime normalizedDt = new iCalDateTime(dt.TimeZoneObservance.Value.TimeZoneInfo.OffsetTo.ToUTC(Value));
                    if (dt.TimeZoneObservance.Value.Contains(normalizedDt))
                        TimeZoneObservance = dt.TimeZoneObservance;
                }
            }
        }

        #endregion

        #region IComparable Members

        public int CompareTo(IDateTime dt)
        {
            if (this.Equals(dt))
                return 0;
            if (this < dt)
                return -1;
            if (this > dt)
                return 1;

            throw new Exception("An error occurred while comparing two IDateTime values.");
        }

        #endregion

        #region IFormattable Members

        public string ToString(string format)
        {
            return ToString(format, null);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            string tz = TimeZoneName;
            if (!string.IsNullOrEmpty(tz))
                tz = " " + tz;

            if (format != null)
                return Value.ToString(format, formatProvider) + tz;
            if (HasTime && HasDate)
                return Value + tz;
            if (HasTime)
                return Value.TimeOfDay + tz;
            
            return Value.ToShortDateString() + tz;
        }

        #endregion
    }
}
