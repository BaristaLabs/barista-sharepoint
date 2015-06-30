namespace Barista.WkHtmlToPdf.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Newtonsoft.Json;
    using Barista.TuesPechkin;

    [Serializable]
    public class ObjectSettingsConstructor : ClrFunction
    {
        public ObjectSettingsConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "ObjectSettings", new ObjectSettingsInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public ObjectSettingsInstance Construct()
        {
            return new ObjectSettingsInstance(InstancePrototype, new ObjectSettings());
        }
    }

    [Serializable]
    public class ObjectSettingsInstance : ObjectInstance
    {
        private readonly ObjectSettings m_objectSettings;

        public ObjectSettingsInstance(ObjectInstance prototype)
            : base(prototype)
        {
            PopulateFunctions();
            m_objectSettings = new ObjectSettings();
        }

        public ObjectSettingsInstance(ObjectInstance prototype, ObjectSettings objectSettings)
            : this(prototype)
        {
            if (objectSettings == null)
                throw new ArgumentNullException("objectSettings");

            m_objectSettings = objectSettings;
        }

        public ObjectSettings ObjectSettings
        {
            get
            {
                return m_objectSettings;
            }
        }

        [JSProperty(Name = "htmlText")]
        [JsonProperty("HtmlText")]
        public string HtmlText
        {
            get
            {
                return m_objectSettings.HtmlText;
            }
            set
            {
                m_objectSettings.HtmlText = value;
            }
        }

        [JSProperty(Name = "pageUrl")]
        [JsonProperty("pageUrl")]
        public string PageUrl
        {
            get
            {
                return m_objectSettings.PageUrl;
            }
            set
            {
                m_objectSettings.PageUrl = value;
            }
        }
    }
}
