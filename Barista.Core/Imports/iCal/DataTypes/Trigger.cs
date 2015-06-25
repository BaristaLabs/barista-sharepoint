namespace Barista.DDay.iCal
{
    using Barista.DDay.iCal.Serialization.iCalendar;
    using System;
    using System.IO;

    /// <summary>
    /// A class that is used to specify exactly when an <see cref="Alarm"/> component will trigger.
    /// Usually this date/time is relative to the component to which the Alarm is associated.
    /// </summary>    
    [Serializable]
    public sealed class Trigger : 
        EncodableDataType,
        ITrigger
    {
        private bool Equals(Trigger other)
        {
            return Equals(m_dateTime, other.m_dateTime) && m_duration.Equals(other.m_duration) && m_related == other.m_related;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (m_dateTime != null ? m_dateTime.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ m_duration.GetHashCode();
                hashCode = (hashCode*397) ^ (int) m_related;
                return hashCode;
            }
        }

        #region Private Fields

        private IDateTime m_dateTime;
        private TimeSpan? m_duration;
        private TriggerRelation m_related = TriggerRelation.Start;

        #endregion

        #region Public Properties

        public IDateTime DateTime
        {
            get { return m_dateTime; }
            set
            {
                m_dateTime = value;
                if (m_dateTime != null)
                {
                    // NOTE: this, along with the "Duration" setter, fixes the bug tested in
                    // TODO11(), as well as this thread: https://sourceforge.net/forum/forum.php?thread_id=1926742&forum_id=656447

                    // DateTime and Duration are mutually exclusive
                    Duration = null;

                    // Do not allow timeless date/time values
                    m_dateTime.HasTime = true;
                }
            }
        }

        public TimeSpan? Duration
        {
            get { return m_duration; }
            set
            {
                m_duration = value;
                if (m_duration != null)
                {
                    // NOTE: see above.

                    // DateTime and Duration are mutually exclusive
                    DateTime = null;
                }
            }
        }

        public TriggerRelation Related
        {
            get { return m_related; }
            set { m_related = value; }
        }

        public bool IsRelative
        {
            get { return m_duration != null; }
        }

        #endregion

        #region Constructors

        public Trigger() { }
        public Trigger(TimeSpan ts)
        {
            Duration = ts;
        }
        public Trigger(string value)
            : this()
        {
            TriggerSerializer serializer = new TriggerSerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        #endregion

        #region Overrides

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            var trigger = obj as ITrigger;
            if (trigger == null)
                return;

            DateTime = trigger.DateTime;
            Duration = trigger.Duration;
            Related = trigger.Related;
        }

        public override bool Equals(object obj)
        {
            var t = obj as ITrigger;
            if (t == null)
                return base.Equals(obj);

            if (DateTime != null && !object.Equals(DateTime, t.DateTime))
                return false;
            if (Duration != null && !object.Equals(Duration, t.Duration))
                return false;
            return object.Equals(Related, t.Related);
        }

        #endregion
    }
}
