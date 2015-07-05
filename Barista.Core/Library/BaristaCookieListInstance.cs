namespace Barista.Library
{
    using System.Collections.Generic;
    using System.Reflection;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;

    public class BaristaCookieListInstance : ListInstance<ObjectInstance, IBaristaCookie>
    {
        public BaristaCookieListInstance(ScriptEngine engine, IList<IBaristaCookie> cookieList)
            : base(new ListInstance<BaristaCookieInstance, IBaristaCookie>(engine))
        {
            List = cookieList;
            PopulateFunctions(GetType(),
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        }

        public override void Add(ObjectInstance item)
        {
            //don't add null items, which blows up things.
            if (item == null)
                return;

            base.Add(item);
        }

        protected override ObjectInstance Wrap(IBaristaCookie cookie)
        {
            return cookie == null
                ? null
                : new BaristaCookieInstance(Engine.Object.InstancePrototype, cookie);
        }

        protected override IBaristaCookie Unwrap(ObjectInstance cookie)
        {
            if (cookie == null)
                return null;

            //If c is cookie, that's good for me.
            var c = cookie as BaristaCookieInstance;
            if (c != null)
                return c.BaristaCookie;

            var os2 = JurassicHelper.Coerce<BaristaCookieInstance>(Engine, cookie);
            return os2.BaristaCookie;
        }
    }
}
