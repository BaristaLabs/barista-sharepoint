namespace Barista.iCal
{
    using Barista.DDay.iCal;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using System.Linq;

    [Serializable]
// ReSharper disable once InconsistentNaming
    public class iCalendarCollectionConstructor : ClrFunction
    {
        public iCalendarCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "CalendarCollection", new iCalendarCollectionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public iCalendarCollectionInstance Construct()
        {
            return new iCalendarCollectionInstance(this.InstancePrototype);
        }
    }

    [Serializable]
// ReSharper disable once InconsistentNaming
    public class iCalendarCollectionInstance : ObjectInstance
    {
        private readonly iCalendarCollection m_iCalendarCollection;

        public iCalendarCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public iCalendarCollectionInstance(ObjectInstance prototype, iCalendarCollection iCalendarCollection)
            : this(prototype)
        {
            if (iCalendarCollection == null)
                throw new ArgumentNullException("iCalendarCollection");

            m_iCalendarCollection = iCalendarCollection;
        }

// ReSharper disable once InconsistentNaming
        public iCalendarCollection iCalendarCollection
        {
            get { return m_iCalendarCollection; }
        }

        [JSFunction(Name = "toArray")]
        public ArrayInstance ToArray()
        {
            var result = this.Engine.Array.Construct();
            foreach (var calendar in m_iCalendarCollection.OfType<iCalendar>())
            {
                var iCal = new iCalendarInstance(this.Engine.Object.InstancePrototype, calendar);
                ArrayInstance.Push(this.Engine.Object.InstancePrototype, iCal);
            }

            return result;
        }
    }
}
