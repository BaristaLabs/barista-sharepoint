namespace Barista.WkHtmlToPdf.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Newtonsoft.Json;
    using Barista.TuesPechkin;

    [Serializable]
    public class WebSettingsConstructor : ClrFunction
    {
        public WebSettingsConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "WebSettings", new WebSettingsInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public WebSettingsInstance Construct()
        {
            return new WebSettingsInstance(InstancePrototype, new WebSettings());
        }
    }

    [Serializable]
    public class WebSettingsInstance : ObjectInstance
    {
        private readonly WebSettings m_webSettings;

        public WebSettingsInstance(ObjectInstance prototype)
            : base(prototype)
        {
            PopulateFunctions();
        }

        public WebSettingsInstance(ObjectInstance prototype, WebSettings webSettings)
            : this(prototype)
        {
            if (webSettings == null)
                throw new ArgumentNullException("webSettings");

            m_webSettings = webSettings;
        }

        public WebSettings WebSettings
        {
            get
            {
                return m_webSettings;
            }
        }

        [JSProperty(Name = "defaultEncoding")]
        [JsonProperty("defaultEncoding")]
        public string DefaultEncoding
        {
            get
            {
                return m_webSettings.DefaultEncoding;
            }
            set
            {
                m_webSettings.DefaultEncoding = value;
            }
        }

        [JSProperty(Name = "enableIntelligentShrinking")]
        [JSDoc("ternPropertyType", "bool?")]
        [JsonProperty("enableIntelligentShrinking")]
        public object EnableIntelligentShrinking
        {
            get
            {
                if (m_webSettings.EnableIntelligentShrinking.HasValue == false)
                    return Null.Value;

                return m_webSettings.EnableIntelligentShrinking.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_webSettings.EnableIntelligentShrinking = null;
                else
                    m_webSettings.EnableIntelligentShrinking = TypeConverter.ToBoolean(value);
            }
        }

        [JSProperty(Name = "enableJavascript")]
        [JSDoc("ternPropertyType", "bool?")]
        [JsonProperty("enableJavascript")]
        public object EnableJavascript
        {
            get
            {
                if (m_webSettings.EnableJavascript.HasValue == false)
                    return Null.Value;

                return m_webSettings.EnableJavascript.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_webSettings.EnableJavascript = null;
                else
                    m_webSettings.EnableJavascript = TypeConverter.ToBoolean(value);
            }
        }

        [JSProperty(Name = "enablePlugins")]
        [JSDoc("ternPropertyType", "bool?")]
        [JsonProperty("enablePlugins")]
        public object EnablePlugins
        {
            get
            {
                if (m_webSettings.EnablePlugins.HasValue == false)
                    return Null.Value;

                return m_webSettings.EnablePlugins.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_webSettings.EnablePlugins = null;
                else
                    m_webSettings.EnablePlugins = TypeConverter.ToBoolean(value);
            }
        }

        [JSProperty(Name = "loadImages")]
        [JSDoc("ternPropertyType", "bool?")]
        [JsonProperty("loadImages")]
        public object LoadImages
        {
            get
            {
                if (m_webSettings.LoadImages.HasValue == false)
                    return Null.Value;

                return m_webSettings.LoadImages.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_webSettings.LoadImages = null;
                else
                    m_webSettings.LoadImages = TypeConverter.ToBoolean(value);
            }
        }

        [JSProperty(Name = "minimumFontSize")]
        [JSDoc("ternPropertyType", "number?")]
        [JsonProperty("minimumFontSize")]
        public object MinimumFontSize
        {
            get
            {
                if (m_webSettings.MinimumFontSize.HasValue == false)
                    return Null.Value;

                return m_webSettings.MinimumFontSize.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_webSettings.MinimumFontSize = null;
                else
                    m_webSettings.MinimumFontSize = TypeConverter.ToNumber(value);
            }
        }

        [JSProperty(Name = "printBackground")]
        [JSDoc("ternPropertyType", "bool?")]
        [JsonProperty("printBackground")]
        public object PrintBackground
        {
            get
            {
                if (m_webSettings.PrintBackground.HasValue == false)
                    return Null.Value;

                return m_webSettings.PrintBackground.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_webSettings.PrintBackground = null;
                else
                    m_webSettings.PrintBackground = TypeConverter.ToBoolean(value);
            }
        }

        [JSProperty(Name = "printMediaType")]
        [JSDoc("ternPropertyType", "bool?")]
        [JsonProperty("printMediaType")]
        public object PrintMediaType
        {
            get
            {
                if (m_webSettings.PrintMediaType.HasValue == false)
                    return Null.Value;

                return m_webSettings.PrintMediaType.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_webSettings.PrintMediaType = null;
                else
                    m_webSettings.PrintMediaType = TypeConverter.ToBoolean(value);
            }
        }

        [JSProperty(Name = "userStyleSheet")]
        [JsonProperty("userStyleSheet")]
        public string UserStyleSheet
        {
            get
            {
                return m_webSettings.UserStyleSheet;
            }
            set
            {
                m_webSettings.UserStyleSheet = value;
            }
        }
    }
}
