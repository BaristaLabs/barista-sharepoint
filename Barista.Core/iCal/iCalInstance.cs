namespace Barista.iCal
{
    using System.IO;
    using Barista.DDay.iCal;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Library;

// ReSharper disable once InconsistentNaming
    public class iCalInstance : ObjectInstance
    {
        public iCalInstance(ScriptEngine engine)
            : base(engine)
        {
            this.PopulateFunctions();
        }

        [JSFunction(Name = "createCalendar")]
        public iCalendarInstance CreateCalendar()
        {
            var result = new iCalendar();
            return new iCalendarInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "loadCalendar")]
        public iCalendarCollectionInstance LoadCalendar(Base64EncodedByteArrayInstance data)
        {
            using (var ms = new MemoryStream(data.Data))
            {
                var result = iCalendar.LoadFromStream(ms);
                return new iCalendarCollectionInstance(this.Engine.Object.InstancePrototype, (iCalendarCollection)result);
            }
            
        }
    }
}
