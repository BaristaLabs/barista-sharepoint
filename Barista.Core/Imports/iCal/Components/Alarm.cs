using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Barista.DDay.iCal
{
    /// <summary>
    /// A class that represents an RFC 2445 VALARM component.
    /// FIXME: move GetOccurrences() logic into an AlarmEvaluator.
    /// </summary>    
    [Serializable]
    public class Alarm :
        CalendarComponent,
        IAlarm
    {
        #region Private Fields

        private List<AlarmOccurrence> m_occurrences;

        #endregion

        #region Public Properties

        virtual public AlarmAction Action
        {
            get { return Properties.Get<AlarmAction>("ACTION"); }
            set { Properties.Set("ACTION", value); }
        }

        virtual public IAttachment Attachment
        {
            get { return Properties.Get<IAttachment>("ATTACH"); }
            set { Properties.Set("ATTACH", value); }
        }

        virtual public IList<IAttendee> Attendees
        {
            get { return Properties.GetMany<IAttendee>("ATTENDEE"); }
            set { Properties.Set("ATTENDEE", value); }
        }

        virtual public string Description
        {
            get { return Properties.Get<string>("DESCRIPTION"); }
            set { Properties.Set("DESCRIPTION", value); }
        }

        virtual public TimeSpan Duration
        {
            get { return Properties.Get<TimeSpan>("DURATION"); }
            set { Properties.Set("DURATION", value); }
        }

        virtual public int Repeat
        {
            get { return Properties.Get<int>("REPEAT"); }
            set { Properties.Set("REPEAT", value); }
        }

        virtual public string Summary
        {
            get { return Properties.Get<string>("SUMMARY"); }
            set { Properties.Set("SUMMARY", value); }
        }

        virtual public ITrigger Trigger
        {
            get { return Properties.Get<ITrigger>("TRIGGER"); }
            set { Properties.Set("TRIGGER", value); }
        }

        #endregion

        #region Protected Properties

        virtual protected List<AlarmOccurrence> Occurrences
        {
            get { return m_occurrences; }
            set { m_occurrences = value; }
        }

        #endregion

        #region Constructors

        public Alarm()
        {
            Initialize();
        }

        void Initialize()
        {
            Name = Components.ALARM;
            Occurrences = new List<AlarmOccurrence>();
        }

        #endregion                

        #region Public Methods

        /// <summary>
        /// Gets a list of alarm occurrences for the given recurring component, <paramref name="rc"/>
        /// that occur between <paramref name="fromDate"/> and <paramref name="toDate"/>.
        /// </summary>
        virtual public IList<AlarmOccurrence> GetOccurrences(IRecurringComponent rc, IDateTime fromDate, IDateTime toDate)
        {
            Occurrences.Clear();

            if (Trigger != null)
            {
                // If the trigger is relative, it can recur right along with
                // the recurring items, otherwise, it happens once and
                // only once (at a precise time).
                if (Trigger.IsRelative)
                {
                    // Ensure that "FromDate" has already been set
                    if (fromDate == null)
                        fromDate = rc.Start.Copy<IDateTime>();

                    TimeSpan d = default(TimeSpan);
                    foreach (Occurrence o in rc.GetOccurrences(fromDate, toDate))
                    {
                        IDateTime dt = o.Period.StartTime;
                        if (Trigger.Related == TriggerRelation.End)
                        {
                            if (o.Period.EndTime != null)
                            {
                                dt = o.Period.EndTime;
                                if (d == default(TimeSpan))
                                    d = o.Period.Duration;
                            }
                            // Use the "last-found" duration as a reference point
                            else if (d != default(TimeSpan))
                                dt = o.Period.StartTime.Add(d);
                            else throw new ArgumentException("Alarm trigger is relative to the END of the occurrence; however, the occurence has no discernible end.");
                        }

                        if (Trigger.Duration != null)
                            Occurrences.Add(new AlarmOccurrence(this, dt.Add(Trigger.Duration.Value), rc));
                    }
                }
                else
                {
                    var dt = Trigger.DateTime.Copy<IDateTime>();
                    dt.AssociatedObject = this;
                    Occurrences.Add(new AlarmOccurrence(this, dt, rc));
                }

                // If a REPEAT and DURATION value were specified,
                // then handle those repetitions here.
                AddRepeatedItems();
            }

            return Occurrences;
        }

        /// <summary>
        /// Polls the <see cref="Alarm"/> component for alarms that have been triggered
        /// since the provided <paramref name="start"/> date/time.  If <paramref name="start"/>
        /// is null, all triggered alarms will be returned.
        /// </summary>
        /// <param name="start">The earliest date/time to poll trigerred alarms for.</param>
        /// <param name="end"></param>
        /// <returns>A list of <see cref="AlarmOccurrence"/> objects, each containing a triggered alarm.</returns>
        virtual public IList<AlarmOccurrence> Poll(IDateTime start, IDateTime end)
        {
            var results = new List<AlarmOccurrence>();

            // Evaluate the alarms to determine the recurrences
            RecurringComponent rc = Parent as RecurringComponent;
            if (rc != null)
            {
                results.AddRange(GetOccurrences(rc, start, end));
                results.Sort();
            }
            return results;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Handles the repetitions that occur from the <c>REPEAT</c> and
        /// <c>DURATION</c> properties.  Each recurrence of the alarm will
        /// have its own set of generated repetitions.
        /// </summary>
        virtual protected void AddRepeatedItems()
        {
            var len = Occurrences.Count;
            for (var i = 0; i < len; i++)
            {
                var ao = Occurrences[i];
                var alarmTime = ao.DateTime.Copy<IDateTime>();

                for (var j = 0; j < Repeat; j++)
                {
                    alarmTime = alarmTime.Add(Duration);
                    Occurrences.Add(new AlarmOccurrence(this, alarmTime.Copy<IDateTime>(), ao.Component));
                }
            }
        }

        #endregion

        #region Overrides

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        #endregion
    }
}
