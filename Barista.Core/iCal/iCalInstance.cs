namespace Barista.iCal
{
    using Barista.DDay.iCal;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;

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
    }
}
