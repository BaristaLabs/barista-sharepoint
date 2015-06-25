namespace Barista.iCal
{
    using Barista.DDay.iCal;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;

    [Serializable]
    public class TimeZoneConstructor : ClrFunction
    {
        public TimeZoneConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "TimeZone", new TimeZoneInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public TimeZoneInstance Construct()
        {
            return new TimeZoneInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class TimeZoneInstance : ObjectInstance
    {
        private readonly ITimeZone m_iTimeZone;

        public TimeZoneInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public TimeZoneInstance(ObjectInstance prototype, ITimeZone iTimeZone)
            : this(prototype)
        {
            if (iTimeZone == null)
                throw new ArgumentNullException("iTimeZone");

            m_iTimeZone = iTimeZone;
        }

        public ITimeZone TimeZone
        {
            get { return m_iTimeZone; }
        }
    }
}
