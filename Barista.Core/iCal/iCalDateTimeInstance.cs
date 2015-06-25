namespace Barista.iCal
{
    using Barista.DDay.iCal;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;

    [Serializable]
// ReSharper disable once InconsistentNaming
    public class iCalDateTimeConstructor : ClrFunction
    {
        public iCalDateTimeConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "iCalDateTime", new iCalDateTimeInstance(engine.Object.InstancePrototype))
        {
            this.PopulateFunctions();
        }

        [JSConstructorFunction]
        public iCalDateTimeInstance Construct(params object[] args)
        {
            iCalDateTime dt = null;
            if (args.Length == 0)
                dt = new iCalDateTime();
            else if (args.Length == 1)
            {
                if (args[0] is DateTime)
                    dt = new iCalDateTime((DateTime)args[0]);
                else
                    dt = new iCalDateTime(TypeConverter.ToString(args[0]));
                
            }
            else if (args.Length >= 3)
            {
                var year = TypeConverter.ToInteger(args[0]);
                var month = TypeConverter.ToInteger(args[1]);
                var day = TypeConverter.ToInteger(args[2]);

                dt = new iCalDateTime(year, month, day);

                //TODO: The other params...
            }

            if (dt == null)
                throw new JavaScriptException(this.Engine, "Error",
                    "Cannot create an instance of a iCalDateTime with the supplied arguments.");

            return new iCalDateTimeInstance(this.InstancePrototype, dt);
        }

        [JSFunction(Name = "now")]
        public iCalDateTimeInstance Now()
        {
            return new iCalDateTimeInstance(this.InstancePrototype, iCalDateTime.Now);
        }

        [JSFunction(Name = "today")]
        public iCalDateTimeInstance Today()
        {
            return new iCalDateTimeInstance(this.InstancePrototype, iCalDateTime.Today);
        }
    }

    [Serializable]
// ReSharper disable once InconsistentNaming
    public class iCalDateTimeInstance : ObjectInstance
    {
        private readonly iCalDateTime m_iCalDateTime;

        public iCalDateTimeInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public iCalDateTimeInstance(ObjectInstance prototype, iCalDateTime iCalDateTime)
            : this(prototype)
        {
            if (iCalDateTime == null)
                throw new ArgumentNullException("iCalDateTime");

            m_iCalDateTime = iCalDateTime;
        }

// ReSharper disable once InconsistentNaming
        public iCalDateTime iCalDateTime
        {
            get { return m_iCalDateTime; }
        }
    }
}
