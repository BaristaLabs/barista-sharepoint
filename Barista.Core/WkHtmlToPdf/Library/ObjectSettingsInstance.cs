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

        [JSProperty(Name = "includeInOutline")]
        [JSDoc("ternPropertyType", "bool?")]
        [JsonProperty("includeInOutline")]
        public object IncludeInOutline
        {
            get
            {
                if (m_objectSettings.IncludeInOutline.HasValue == false)
                    return Null.Value;

                return m_objectSettings.IncludeInOutline.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_objectSettings.IncludeInOutline = null;
                else
                    m_objectSettings.IncludeInOutline = TypeConverter.ToBoolean(value);
            }
        }

        [JSProperty(Name = "countPages")]
        [JSDoc("ternPropertyType", "bool?")]
        [JsonProperty("countPages")]
        public object CountPages
        {
            get
            {
                if (m_objectSettings.CountPages.HasValue == false)
                    return Null.Value;

                return m_objectSettings.CountPages.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_objectSettings.CountPages = null;
                else
                    m_objectSettings.CountPages = TypeConverter.ToBoolean(value);
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

        [JSProperty(Name = "produceForms")]
        [JSDoc("ternPropertyType", "bool?")]
        [JsonProperty("produceForms")]
        public object ProduceForms
        {
            get
            {
                if (m_objectSettings.ProduceForms.HasValue == false)
                    return Null.Value;

                return m_objectSettings.ProduceForms.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_objectSettings.ProduceForms = null;
                else
                    m_objectSettings.ProduceForms = TypeConverter.ToBoolean(value);
            }
        }

        [JSProperty(Name = "produceExternalLinks")]
        [JSDoc("ternPropertyType", "bool?")]
        [JsonProperty("produceExternalLinks")]
        public object ProduceExternalLinks
        {
            get
            {
                if (m_objectSettings.ProduceExternalLinks.HasValue == false)
                    return Null.Value;

                return m_objectSettings.ProduceExternalLinks.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_objectSettings.ProduceExternalLinks = null;
                else
                    m_objectSettings.ProduceExternalLinks = TypeConverter.ToBoolean(value);
            }
        }

        [JSProperty(Name = "htmlText")]
        [JsonProperty("htmlText")]
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

        [JSProperty(Name = "headerSettings")]
        [JsonProperty("headerSettings")]
        public object HeaderSettings
        {
            get
            {
                if (m_objectSettings.HeaderSettings == null)
                    return null;

                return new HeaderSettingsInstance(Engine.Object.InstancePrototype, m_objectSettings.HeaderSettings);
            }
            set
            {
                if (value == null)
                {
                    m_objectSettings.HeaderSettings = null;
                    return;
                }

                var ws = value as HeaderSettingsInstance;
                if (ws != null)
                {
                    m_objectSettings.HeaderSettings = ws.HeaderSettings;
                    return;
                }

                var ws2 = JurassicHelper.Coerce<HeaderSettingsInstance>(Engine, value);
                m_objectSettings.HeaderSettings = ws2.HeaderSettings;
            }
        }

        [JSProperty(Name = "loadSettings")]
        [JsonProperty("loadSettings")]
        public object LoadSettings
        {
            get
            {
                if (m_objectSettings.LoadSettings == null)
                    return null;

                return new LoadSettingsInstance(Engine.Object.InstancePrototype, m_objectSettings.LoadSettings);
            }
            set
            {
                if (value == null)
                {
                    m_objectSettings.LoadSettings = null;
                    return;
                }

                var ws = value as LoadSettingsInstance;
                if (ws != null)
                {
                    m_objectSettings.LoadSettings = ws.LoadSettings;
                    return;
                }

                var ws2 = JurassicHelper.Coerce<LoadSettingsInstance>(Engine, value);
                m_objectSettings.LoadSettings = ws2.LoadSettings;
            }
        }

        [JSProperty(Name = "webSettings")]
        [JsonProperty("webSettings")]
        public object WebSettings
        {
            get
            {
                if (m_objectSettings.WebSettings == null)
                    return null;

                return new WebSettingsInstance(Engine.Object.InstancePrototype, m_objectSettings.WebSettings);
            }
            set
            {
                if (value == null)
                {
                    m_objectSettings.WebSettings = null;
                    return;
                }

                var ws = value as WebSettingsInstance;
                if (ws != null)
                {
                    m_objectSettings.WebSettings = ws.WebSettings;
                    return;
                }

                var ws2 = JurassicHelper.Coerce<WebSettingsInstance>(Engine, value);
                 m_objectSettings.WebSettings = ws2.WebSettings;
            }
        }
    }
}
