namespace Barista.iCal
{
    using Barista.DDay.iCal;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;

    [Serializable]
    public class OrganizerConstructor : ClrFunction
    {
        public OrganizerConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "Organizer", new OrganizerInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public OrganizerInstance Construct(string value)
        {
            return new OrganizerInstance(this.InstancePrototype, new Organizer(value));
        }
    }

    [Serializable]
    public class OrganizerInstance : ObjectInstance
    {
        private readonly IOrganizer m_organizer;

        public OrganizerInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public OrganizerInstance(ObjectInstance prototype, IOrganizer organizer)
            : this(prototype)
        {
            if (organizer == null)
                throw new ArgumentNullException("organizer");

            m_organizer = organizer;
        }

        public IOrganizer Organizer
        {
            get { return m_organizer; }
        }

        [JSProperty(Name = "commonName")]
        public string CommonName
        {
            get
            {
                return m_organizer.CommonName;
            }
            set
            {
                m_organizer.CommonName = value;
            }
        }
    }
}
