﻿namespace Barista.iCal
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
            return new iCalendarCollectionInstance(InstancePrototype);
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
            PopulateFields();
            PopulateFunctions();
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
        [JSDoc("ternReturnType", "[+iCalendar]")]
        public ArrayInstance ToArray()
        {
            var result = Engine.Array.Construct();
            foreach (var calendar in m_iCalendarCollection.OfType<iCalendar>())
            {
                var iCal = new iCalendarInstance(Engine.Object.InstancePrototype, calendar);
                ArrayInstance.Push(result, iCal);
            }

            return result;
        }
    }
}
