namespace Barista.iCal
{
    using System.Linq;
    using Barista.DDay.iCal;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;

    [Serializable]
    public class CalendarPropertyListConstructor : ClrFunction
    {
        public CalendarPropertyListConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "CalendarPropertyList", new CalendarPropertyListInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public CalendarPropertyListInstance Construct()
        {
            return new CalendarPropertyListInstance(this.InstancePrototype, new CalendarPropertyList());
        }
    }

    [Serializable]
    public class CalendarPropertyListInstance : ObjectInstance
    {
        private readonly ICalendarPropertyList m_calendarPropertyList;

        public CalendarPropertyListInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public CalendarPropertyListInstance(ObjectInstance prototype, ICalendarPropertyList calendarPropertyList)
            : this(prototype)
        {
            if (calendarPropertyList == null)
                throw new ArgumentNullException("calendarPropertyList");

            m_calendarPropertyList = calendarPropertyList;
        }

        public ICalendarPropertyList CalendarPropertyList
        {
            get { return m_calendarPropertyList; }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return m_calendarPropertyList.Count;
            }
        }

        [JSFunction(Name = "add")]
        public void Add(CalendarPropertyInstance calendarProperty)
        {
            if (calendarProperty == null)
                throw new JavaScriptException(this.Engine, "Error", "calendarProperty argument cannot be null.");

            m_calendarPropertyList.Add(calendarProperty.CalendarProperty);
            
        }

        [JSFunction(Name = "clear")]
        public void Clear()
        {
            m_calendarPropertyList.Clear();
        }

        [JSFunction(Name = "toArray")]
        public ArrayInstance ToArray()
        {
            var result = this.Engine.Array.Construct();

            foreach (var calendarProperty in m_calendarPropertyList.OfType<CalendarProperty>())
            {
                ArrayInstance.Push(result, new CalendarPropertyInstance(this.Engine.Object.InstancePrototype, calendarProperty));
            }

            return result;
        }
    }
}
