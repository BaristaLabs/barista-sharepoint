namespace Barista.WkHtmlToPdf.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Newtonsoft.Json;
    using Barista.TuesPechkin;

    [Serializable]
    public class HtmlToImageDocumentConstructor : ClrFunction
    {
        public HtmlToImageDocumentConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "HtmlToImageDocument", new HtmlToImageDocumentInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public HtmlToImageDocumentInstance Construct()
        {
            return new HtmlToImageDocumentInstance(InstancePrototype, new HtmlToImageDocument());
        }
    }

    [Serializable]
    public class HtmlToImageDocumentInstance : ObjectInstance
    {
        private readonly HtmlToImageDocument m_htmlToImageDocument;

        public HtmlToImageDocumentInstance(ObjectInstance prototype)
            : base(prototype)
        {
            PopulateFunctions();
        }

        public HtmlToImageDocumentInstance(ObjectInstance prototype, HtmlToImageDocument htmlToImageDocument)
            : this(prototype)
        {
            if (htmlToImageDocument == null)
                throw new ArgumentNullException("htmlToImageDocument");

            m_htmlToImageDocument = htmlToImageDocument;
        }

        public HtmlToImageDocument HtmlToImageDocument
        {
            get
            {
                return m_htmlToImageDocument;
            }
        }

        [JSProperty(Name = "screenHeight")]
        [JsonProperty("screenHeight")]
        public double ScreenHeight
        {
            get
            {
                return m_htmlToImageDocument.ScreenHeight;
            }
            set
            {
                m_htmlToImageDocument.ScreenHeight = value;
            }
        }

        [JSProperty(Name = "screenWidth")]
        [JSDoc("ternPropertyType", "number?")]
        [JsonProperty("screenWidth")]
        public object ScreenWidth
        {
            get
            {
                if (m_htmlToImageDocument.ScreenWidth.HasValue == false)
                    return Null.Value;

                return m_htmlToImageDocument.ScreenWidth.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_htmlToImageDocument.ScreenWidth = null;

                m_htmlToImageDocument.ScreenWidth = TypeConverter.ToNumber(value);
            }
        }

        [JSProperty(Name = "quality")]
        [JSDoc("ternPropertyType", "number?")]
        [JsonProperty("quality")]
        public object Quality
        {
            get
            {
                if (m_htmlToImageDocument.Quality.HasValue == false)
                    return Null.Value;

                return m_htmlToImageDocument.Quality.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_htmlToImageDocument.Quality = null;

                m_htmlToImageDocument.Quality = TypeConverter.ToNumber(value);
            }
        }

        [JSProperty(Name = "format")]
        [JsonProperty("format")]
        public string Format
        {
            get
            {
                return m_htmlToImageDocument.Format;
            }
            set
            {
                m_htmlToImageDocument.Format = value;
            }
        }

        [JSProperty(Name = "out")]
        [JsonProperty("out")]
        public string Out
        {
            get
            {
                return m_htmlToImageDocument.Out;
            }
            set
            {
                m_htmlToImageDocument.Out = value;
            }
        }

        [JSProperty(Name = "in")]
        [JsonProperty("in")]
        public string In
        {
            get
            {
                return m_htmlToImageDocument.In;
            }
            set
            {
                m_htmlToImageDocument.In = value;
            }
        }

        [JSProperty(Name = "transparent")]
        [JSDoc("ternPropertyType", "bool?")]
        [JsonProperty("transparent")]
        public object Transparent
        {
            get
            {
                if (m_htmlToImageDocument.Transparent.HasValue == false)
                    return Null.Value;

                return m_htmlToImageDocument.Transparent.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_htmlToImageDocument.Transparent = null;

                m_htmlToImageDocument.Transparent = TypeConverter.ToBoolean(value);
            }
        }

        [JSProperty(Name = "cropSettings")]
        [JsonProperty("cropSettings")]
        public object CropSettings
        {
            get
            {
                if (m_htmlToImageDocument.CropSettings == null)
                    return null;

                return new CropSettingsInstance(Engine.Object.InstancePrototype, m_htmlToImageDocument.CropSettings);
            }
            set
            {
                if (value == null)
                {
                    m_htmlToImageDocument.CropSettings = null;
                    return;
                }

                var ws = value as CropSettingsInstance;
                if (ws != null)
                {
                    m_htmlToImageDocument.CropSettings = ws.CropSettings;
                    return;
                }

                var ws2 = JurassicHelper.Coerce<CropSettingsInstance>(Engine, value);
                m_htmlToImageDocument.CropSettings = ws2.CropSettings;
            }
        }

        [JSProperty(Name = "loadSettings")]
        [JsonProperty("loadSettings")]
        public object LoadSettings
        {
            get
            {
                if (m_htmlToImageDocument.LoadSettings == null)
                    return null;

                return new LoadSettingsInstance(Engine.Object.InstancePrototype, m_htmlToImageDocument.LoadSettings);
            }
            set
            {
                if (value == null)
                {
                    m_htmlToImageDocument.LoadSettings = null;
                    return;
                }

                var ws = value as LoadSettingsInstance;
                if (ws != null)
                {
                    m_htmlToImageDocument.LoadSettings = ws.LoadSettings;
                    return;
                }

                var ws2 = JurassicHelper.Coerce<LoadSettingsInstance>(Engine, value);
                m_htmlToImageDocument.LoadSettings = ws2.LoadSettings;
            }
        }

        [JSProperty(Name = "webSettings")]
        [JsonProperty("webSettings")]
        public object WebSettings
        {
            get
            {
                if (m_htmlToImageDocument.WebSettings == null)
                    return null;

                return new WebSettingsInstance(Engine.Object.InstancePrototype, m_htmlToImageDocument.WebSettings);
            }
            set
            {
                if (value == null)
                {
                    m_htmlToImageDocument.WebSettings = null;
                    return;
                }

                var ws = value as WebSettingsInstance;
                if (ws != null)
                {
                    m_htmlToImageDocument.WebSettings = ws.WebSettings;
                    return;
                }

                var ws2 = JurassicHelper.Coerce<WebSettingsInstance>(Engine, value);
                m_htmlToImageDocument.WebSettings = ws2.WebSettings;
            }
        }
    }
}
