
namespace Barista.Bundles
{
    using Barista.iCal;

// ReSharper disable once InconsistentNaming
    public class iCalBundle : IBundle
    {
        public bool IsSystemBundle
        {
            get { return true; }
        }

        public string BundleName
        {
            get { return "iCal"; }
        }

        public string BundleDescription
        {
            get { return "iCal Bundle. Provides a mechanism to parse and generate iCals"; }
        }

        public virtual object InstallBundle(Jurassic.ScriptEngine engine)
        {
            engine.SetGlobalValue("iCalendar", new iCalendarConstructor(engine));
            engine.SetGlobalValue("iCalDateTime", new iCalDateTimeConstructor(engine));
            engine.SetGlobalValue("iEvent", new EventConstructor(engine));
            engine.SetGlobalValue("iTodo", new TodoConstructor(engine));
            engine.SetGlobalValue("iCalendarProperty", new CalendarPropertyConstructor(engine));
            engine.SetGlobalValue("iOrganizer", new OrganizerConstructor(engine));
            engine.SetGlobalValue("iTimeZone", new TimeZoneConstructor(engine));

            var iCalInstance = new iCalInstance(engine);
            return iCalInstance;
        }
    }
}
