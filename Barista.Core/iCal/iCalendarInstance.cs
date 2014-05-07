namespace Barista.iCal
{
    using System.IO;
    using System.Text;
    using Barista.DDay.iCal;
    using Barista.DDay.iCal.Serialization.iCalendar;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;

    [Serializable]
// ReSharper disable once InconsistentNaming
    public class iCalendarConstructor : ClrFunction
    {
        public iCalendarConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "iCalendar", new iCalendarInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public iCalendarInstance Construct()
        {
            return new iCalendarInstance(this.InstancePrototype);
        }
    }

    [Serializable]
// ReSharper disable once InconsistentNaming
    public class iCalendarInstance : ObjectInstance
    {
        private readonly iCalendar m_iCalendar;

        public iCalendarInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public iCalendarInstance(ObjectInstance prototype, iCalendar iCalendar)
            : this(prototype)
        {
            if (iCalendar == null)
                throw new ArgumentNullException("iCalendar");

            m_iCalendar = iCalendar;
        }

// ReSharper disable once InconsistentNaming
        public iCalendar iCalendar
        {
            get { return m_iCalendar; }
        }

        [JSProperty(Name = "name")]
        public string Name
        {
            get { return m_iCalendar.Name; }
            set { m_iCalendar.Name = value; }
        }

        [JSFunction(Name = "addLocalTimeZone")]
        public TimeZoneInstance AddLocalTimeZone()
        {
            var result = m_iCalendar.AddLocalTimeZone();
            return new TimeZoneInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "createEvent")]
        public EventInstance CreateEvent()
        {
            var result = m_iCalendar.Create<Event>();
            return new EventInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getBytes")]
        public Base64EncodedByteArrayInstance GetBytes(object fileName)
        {
            var serializer = new iCalendarSerializer();
            byte[] data;
            using (var ms = new MemoryStream())
            {
                serializer.Serialize(m_iCalendar, ms, Encoding.UTF8);
                data = ms.ToArray();
            }

            var result = new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, data)
            {
                MimeType = "text/calendar"
            };

            if (fileName != null && fileName != Undefined.Value)
                result.FileName = TypeConverter.ToString(fileName);

            return result;
        }
    }
}
