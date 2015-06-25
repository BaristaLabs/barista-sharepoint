namespace Barista.iCal
{
    using Barista.DDay.iCal;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;

    [Serializable]
    public class CalendarPropertyConstructor : ClrFunction
    {
        public CalendarPropertyConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "CalendarProperty", new CalendarPropertyInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public CalendarPropertyInstance Construct(string name)
        {
            return new CalendarPropertyInstance(this.InstancePrototype, new CalendarProperty(name));
        }
    }

    [Serializable]
    public class CalendarPropertyInstance : ObjectInstance
    {
        private readonly CalendarProperty m_calendarProperty;

        public CalendarPropertyInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public CalendarPropertyInstance(ObjectInstance prototype, CalendarProperty calendarProperty)
            : this(prototype)
        {
            if (calendarProperty == null)
                throw new ArgumentNullException("calendarProperty");

            m_calendarProperty = calendarProperty;
        }

        public ICalendarProperty CalendarProperty
        {
            get { return m_calendarProperty; }
        }

        [JSProperty(Name = "name")]
        public string Name
        {
            get
            {
                return m_calendarProperty.Name;
            }
            set
            {
                m_calendarProperty.Name = value;
            }
        }

        [JSProperty(Name = "value")]
        public string Value
        {
            get
            {
                return m_calendarProperty.Value.ToString();
            }
            set
            {
                m_calendarProperty.Value = value;
            }
        }

        [JSFunction(Name = "addParameter")]
        public void AddParameter(string name, string value)
        {
            m_calendarProperty.AddParameter(name, value);
        }
    }
}
