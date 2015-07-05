namespace Barista.iCal
{
    using System.Collections.Generic;
    using Barista.DDay.iCal;
    using Barista.Extensions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;

    [Serializable]
    public class EventConstructor : ClrFunction
    {
        public EventConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "Event", new EventInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public EventInstance Construct()
        {
            return new EventInstance(InstancePrototype);
        }
    }

    [Serializable]
    public class EventInstance : ObjectInstance
    {
        private readonly IEvent m_iEvent;

        public EventInstance(ObjectInstance prototype)
            : base(prototype)
        {
            PopulateFields();
            PopulateFunctions();
        }

        public EventInstance(ObjectInstance prototype, IEvent iEvent)
            : this(prototype)
        {
            if (iEvent == null)
                throw new ArgumentNullException("iEvent");

            m_iEvent = iEvent;
        }

        public IEvent Event
        {
            get { return m_iEvent; }
        }

        #region Properties
        private ArrayInstance m_categories;

        [JSProperty(Name = "categories")]
        [JSDoc("ternPropertyType", "[string]")]
        public ArrayInstance Categories
        {
            get
            {
                if (m_iEvent.Categories == null)
                    return null;

                return m_categories ?? (m_categories = Engine.Array.Construct(m_iEvent.Categories));
            }
            set
            {
                if (value == null)
                {
                    m_categories = null;
                    m_iEvent.Categories = null;
                    return;
                }

                if (m_categories == null)
                    m_categories = Engine.Array.Construct();
                else
                {
                    while (m_categories.Length > 0)
                        m_categories.Pop();
                }

                m_iEvent.Categories = new List<string>();
                foreach (var instance in value.ElementValues)
                {
                    var v = TypeConverter.ToString(instance);
                    m_iEvent.Categories.Add(v);
                    m_categories.Push(v);
                }
            }
        }

        private ArrayInstance m_comments;

        [JSProperty(Name = "comments")]
        [JSDoc("ternPropertyType", "[string]")]
        public ArrayInstance Comments
        {
            get
            {
                if (m_iEvent.Comments == null)
                    return null;

                return m_comments ?? (m_comments = Engine.Array.Construct(m_iEvent.Comments));
            }
            set
            {
                if (value == null)
                {
                    m_comments = null;
                    m_iEvent.Comments = null;
                    return;
                }

                if (m_comments == null)
                    m_comments = Engine.Array.Construct();
                else
                {
                    while (m_comments.Length > 0)
                        m_comments.Pop();
                }

                m_iEvent.Comments = new List<string>();
                foreach (var instance in value.ElementValues)
                {
                    var v = TypeConverter.ToString(instance);
                    m_iEvent.Comments.Add(v);
                    m_comments.Push(v);
                }
            }
        }

        [JSProperty(Name = "description")]
        public string Description
        {
            get
            {
                return m_iEvent.Description;
            }
            set
            {
                m_iEvent.Description = value;
            }
        }

        [JSProperty(Name = "duration")]
        public object Duration
        {
            get
            {
                return m_iEvent.Duration.ToString();
            }
            set
            {
                var strValue = TypeConverter.ToString(value);
                TimeSpan ts;
                if (TimeSpan.TryParse(strValue, out ts))
                {
                    m_iEvent.Duration = ts;
                }
                else
                {
                    throw new JavaScriptException(Engine, "Error", "Value could not be converted to a time span:" + strValue);
                }
            }
        }

        [JSProperty(Name = "isAllDay")]
        public bool IsAllDay
        {
            get
            {
                return m_iEvent.IsAllDay;
            }
            set
            {
                m_iEvent.IsAllDay = value;
            }
        }

        [JSProperty(Name = "location")]
        public string Location
        {
            get
            {
                return m_iEvent.Location;
            }
            set
            {
                m_iEvent.Location = value;
            }
        }

        [JSProperty(Name = "organizer")]
        public object Organizer
        {
            get
            {
                return new OrganizerInstance(Engine.Object.InstancePrototype, m_iEvent.Organizer);
            }
            set
            {
                if (value == null)
                {
                    m_iEvent.Organizer = null;
                    return;
                }

                if (value is OrganizerInstance)
                {
                    m_iEvent.Organizer = ((OrganizerInstance) value).Organizer;
                }
                else
                {
                    m_iEvent.Organizer = new Organizer(TypeConverter.ToString(value));
                }
            }
        }

        [JSProperty(Name = "priority")]
        public int Priority
        {
            get
            {
                return m_iEvent.Priority;
            }
            set
            {
                m_iEvent.Priority = value;
            }
        }

        [JSProperty(Name = "properties")]
        public CalendarPropertyListInstance CalendarProperties
        {
            get
            {
                return new CalendarPropertyListInstance(Engine.Object.InstancePrototype, m_iEvent.Properties);
            }
        }

        private ArrayInstance m_resources;
        [JSProperty(Name = "resources")]
        [JSDoc("ternPropertyType", "[string]")]
        public ArrayInstance Resources
        {
            get
            {
                if (m_iEvent.Resources == null)
                    return null;

                return m_resources ?? (m_resources = Engine.Array.Construct(m_iEvent.Resources));
            }
            set
            {
                if (value == null)
                {
                    m_resources = null;
                    m_iEvent.Resources = null;
                    return;
                }

                if (m_resources == null)
                    m_resources = Engine.Array.Construct();
                else
                {
                    while (m_resources.Length > 0)
                        m_resources.Pop();
                }

                m_iEvent.Resources = new List<string>();
                foreach (var instance in value.ElementValues)
                {
                    var v = TypeConverter.ToString(instance);
                    m_iEvent.Resources.Add(v);
                    m_resources.Push(v);
                }
            }
        }

        [JSProperty(Name = "status")]
        public string Status
        {
            get
            {
                return m_iEvent.Status.ToString();
            }
            set
            {
                EventStatus status;
                if (value.TryParseEnum(true, out status))
                    m_iEvent.Status = status;
            }
        }

        [JSProperty(Name = "start")]
        public object Start
        {
            get
            {
                return new iCalDateTimeInstance(Engine.Object.InstancePrototype, (iCalDateTime)m_iEvent.Start);
            }
            set
            {
                if (value == null)
                {
                    m_iEvent.Start = null;
                    return;
                }

                if (value is iCalDateTimeInstance)
                    m_iEvent.Start = ((iCalDateTimeInstance)value).iCalDateTime;
                else if (value is DateTime)
                    m_iEvent.Start = new iCalDateTime((DateTime) value);
                else if (value is DateInstance)
                    m_iEvent.Start = new iCalDateTime(((DateInstance)value).Value);
                else
                {
                    var strValue = TypeConverter.ToString(value);
                    var dt = new iCalDateTime(strValue);
                    m_iEvent.Start = dt;
                }
            }
        }

        [JSProperty(Name = "summary")]
        public string Summary
        {
            get
            {
                return m_iEvent.Summary;
            }
            set
            {
                m_iEvent.Summary = value;
            }
        }

        [JSProperty(Name = "uid")]
        public string Uid
        {
            get
            {
                return m_iEvent.UID;
            }
            set
            {
                m_iEvent.UID = value;
            }
        }

        [JSProperty(Name = "url")]
        public string Url
        {
            get
            {
                return m_iEvent.Url.ToString();
            }
            set {
                m_iEvent.Url = value == null ? null : new Uri(value);
            }
        }
        #endregion

        [JSFunction(Name = "getCalendar")]
        public iCalendarInstance GetCalendar()
        {
            return new iCalendarInstance(Engine.Object.InstancePrototype, (iCalendar)m_iEvent.iCalendar);
        }
    }
}
