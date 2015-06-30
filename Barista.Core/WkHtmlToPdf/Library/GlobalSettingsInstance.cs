namespace Barista.WkHtmlToPdf.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.TuesPechkin;

    [Serializable]
    public class GlobalSettingsConstructor : ClrFunction
    {
        public GlobalSettingsConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "GlobalSettings", new GlobalSettingsInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public GlobalSettingsInstance Construct()
        {
            return new GlobalSettingsInstance(InstancePrototype);
        }
    }

    [Serializable]
    public class GlobalSettingsInstance : ObjectInstance
    {
        private readonly GlobalSettings m_globalSettings;

        public GlobalSettingsInstance(ObjectInstance prototype)
            : base(prototype)
        {
            PopulateFunctions();
        }

        public GlobalSettingsInstance(ObjectInstance prototype, GlobalSettings globalSettings)
            : this(prototype)
        {
            if (globalSettings == null)
                throw new ArgumentNullException("globalSettings");

            m_globalSettings = globalSettings;
        }

        public GlobalSettings GlobalSettings
        {
            get
            {
                return m_globalSettings;
            }
        }

        [JSProperty(Name = "collate")]
        public object Collate
        {
            get
            {
                if (m_globalSettings.Collate.HasValue == false)
                    return Null.Value;

                return m_globalSettings.Collate.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_globalSettings.Collate = null;

                m_globalSettings.Collate = Convert.ToBoolean(value);
            }
        }
    }
}
