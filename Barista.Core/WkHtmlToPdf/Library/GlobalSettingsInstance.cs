namespace Barista.WkHtmlToPdf.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Extensions;
    using Barista.Newtonsoft.Json;
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
            return new GlobalSettingsInstance(InstancePrototype, new GlobalSettings());
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
        [JSDoc("ternPropertyType", "bool?")]
        [JsonProperty("collate")]
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
                else
                    m_globalSettings.Collate = TypeConverter.ToBoolean(value);
            }
        }

        [JSProperty(Name = "colorMode")]
        [JsonProperty("colorMode")]
        public string ColorMode
        {
            get
            {
                if (m_globalSettings.ColorMode.HasValue == false)
                    return null;

                return m_globalSettings.ColorMode.ToString();
            }
            set
            {
                if (value.IsNullOrWhiteSpace())
                    m_globalSettings.ColorMode = null;
                else
                {
                    GlobalSettings.DocumentColorMode documentColorMode;
                    if (value.TryParseEnum(true, out documentColorMode))
                        m_globalSettings.ColorMode = documentColorMode;
                }
            }
        }

        [JSProperty(Name = "cookieJar")]
        [JsonProperty("cookieJar")]
        public string CookieJar
        {
            get
            {
                return m_globalSettings.CookieJar;
            }
            set
            {
                m_globalSettings.CookieJar = value;
            }
        }

        [JSProperty(Name = "copies")]
        [JSDoc("ternPropertyType", "number?")]
        [JsonProperty("copies")]
        public object Copies
        {
            get
            {
                if (m_globalSettings.Copies.HasValue == false)
                    return Null.Value;

                return m_globalSettings.Copies.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_globalSettings.Copies = null;
                else
                    m_globalSettings.Copies = TypeConverter.ToInt32(value);
            }
        }

        [JSProperty(Name = "documentTitle")]
        [JsonProperty("documentTitle")]
        public string DocumentTitle
        {
            get
            {
                return m_globalSettings.DocumentTitle;
            }
            set
            {
                m_globalSettings.DocumentTitle = value;
            }
        }

        [JSProperty(Name = "dpi")]
        [JSDoc("ternPropertyType", "number?")]
        [JsonProperty("dpi")]
        public object Dpi
        {
            get
            {
                if (m_globalSettings.Dpi.HasValue == false)
                    return Null.Value;

                return m_globalSettings.Dpi.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_globalSettings.Dpi = null;
                else
                    m_globalSettings.Dpi = TypeConverter.ToInt32(value);
            }
        }

        [JSProperty(Name = "dumpOutline")]
        [JsonProperty("dumpOutline")]
        public string DumpOutline
        {
            get
            {
                return m_globalSettings.DumpOutline;
            }
            set
            {
                m_globalSettings.DumpOutline = value;
            }
        }

        [JSProperty(Name = "imageDpi")]
        [JSDoc("ternPropertyType", "number?")]
        [JsonProperty("imageDpi")]
        public object ImageDpi
        {
            get
            {
                if (m_globalSettings.ImageDpi.HasValue == false)
                    return Null.Value;

                return m_globalSettings.ImageDpi.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_globalSettings.ImageDpi = null;
                else
                    m_globalSettings.ImageDpi = TypeConverter.ToInt32(value);
            }
        }

        [JSProperty(Name = "imageQuality")]
        [JSDoc("ternPropertyType", "number?")]
        [JsonProperty("imageQuality")]
        public object ImageQuality
        {
            get
            {
                if (m_globalSettings.ImageQuality.HasValue == false)
                    return Null.Value;

                return m_globalSettings.ImageQuality.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_globalSettings.ImageQuality = null;
                else
                    m_globalSettings.ImageQuality = TypeConverter.ToInt32(value);
            }
        }

        [JSDoc("ternPropertyType", "+MarginSettings?")]
        [JSProperty(Name = "margins")]
        [JsonProperty("margins")]
        public object Margin
        {
            get
            {
                if (m_globalSettings.Margins == null)
                    return null;

                return new MarginSettingsInstance(Engine.Object.InstancePrototype, m_globalSettings.Margins);
            }
            set
            {
                var marginSettings = value as MarginSettingsInstance;
                if (marginSettings != null)
                    m_globalSettings.Margins = marginSettings.MarginSettings;
                else
                {
                    marginSettings = JurassicHelper.Coerce<MarginSettingsInstance>(Engine, value);
                    m_globalSettings.Margins = marginSettings.MarginSettings;
                }
            }
        }

        [JSProperty(Name = "orientation")]
        [JsonProperty("orientation")]
        public string Orientation
        {
            get
            {
                if (m_globalSettings.Orientation.HasValue)
                    return m_globalSettings.Orientation.ToString();
                return null;
            }
            set
            {
                if (value.IsNullOrWhiteSpace())
                    m_globalSettings.ImageQuality = null;
                else
                {
                    GlobalSettings.PaperOrientation orientation;
                    if (value.TryParseEnum(true, out orientation))
                        m_globalSettings.Orientation = orientation;
                }
            }
        }

        [JSProperty(Name = "outlineDepth")]
        [JSDoc("ternPropertyType", "number?")]
        [JsonProperty("outlineDepth")]
        public object OutlineDepth
        {
            get
            {
                if (m_globalSettings.OutlineDepth.HasValue == false)
                    return Null.Value;

                return m_globalSettings.OutlineDepth.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_globalSettings.OutlineDepth = null;

                m_globalSettings.OutlineDepth = TypeConverter.ToInt32(value);
            }
        }

        [JSProperty(Name = "outputFile")]
        [JsonProperty("outputFile")]
        public string OutputFile
        {
            get
            {
                return m_globalSettings.OutputFile;
            }
            set
            {
                m_globalSettings.OutputFile = value;
            }
        }

        [JSProperty(Name = "outputFormat")]
        [JsonProperty("outputFormat")]
        public string OutputFormat
        {
            get
            {
                if (m_globalSettings.OutputFormat == null)
                    return null;

                return m_globalSettings.OutputFormat.ToString();
            }
            set
            {
                if (value.IsNullOrWhiteSpace())
                    m_globalSettings.OutputFormat = null;
                else
                {
                    GlobalSettings.DocumentOutputFormat outputFormat;
                    if (value.TryParseEnum(true, out outputFormat))
                        m_globalSettings.OutputFormat = outputFormat;
                }
            }
        }

        [JSProperty(Name = "pageOffset")]
        [JSDoc("ternPropertyType", "number?")]
        [JsonProperty("pageOffset")]
        public object PageOffset
        {
            get
            {
                if (m_globalSettings.PageOffset.HasValue == false)
                    return Null.Value;

                return m_globalSettings.PageOffset.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_globalSettings.PageOffset = null;

                m_globalSettings.PageOffset = TypeConverter.ToInt32(value);
            }
        }

        [JSProperty(Name = "paperSize")]
        [JSDoc("ternPropertyType", "+PaperSize")]
        [JsonProperty("paperSize")]
        public object PaperSize
        {
            get
            {
                if (m_globalSettings.PaperSize == null)
                    return null;

                return new PaperSizeInstance(Engine.Object.InstancePrototype, m_globalSettings.PaperSize);
            }
            set
            {
                var paperSize = value as PaperSizeInstance;
                if (paperSize != null)
                    m_globalSettings.PaperSize = paperSize.PechkinPaperSize;
                else
                {
                    paperSize = JurassicHelper.Coerce<PaperSizeInstance>(Engine, value);
                    m_globalSettings.PaperSize = paperSize.PechkinPaperSize;
                }
            }
        }

        [JSProperty(Name = "produceOutline")]
        [JSDoc("ternPropertyType", "bool?")]
        [JsonProperty("produceOutline")]
        public object ProduceOutline
        {
            get
            {
                if (m_globalSettings.ProduceOutline.HasValue == false)
                    return Null.Value;

                return m_globalSettings.ProduceOutline.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_globalSettings.ProduceOutline = null;
                else
                    m_globalSettings.ProduceOutline = TypeConverter.ToBoolean(value);
            }
        }

        [JSProperty(Name = "useCompression")]
        [JSDoc("ternPropertyType", "bool?")]
        [JsonProperty("useCompression")]
        public object UseCompression
        {
            get
            {
                if (m_globalSettings.UseCompression.HasValue == false)
                    return Null.Value;

                return m_globalSettings.UseCompression.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_globalSettings.UseCompression = null;
                else
                    m_globalSettings.UseCompression = TypeConverter.ToBoolean(value);
            }
        }
    }
}
