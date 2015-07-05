namespace Barista.WkHtmlToPdf.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Newtonsoft.Json;
    using Barista.TuesPechkin;

    [Serializable]
    public class HeaderSettingsConstructor : ClrFunction
    {
        public HeaderSettingsConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "HeaderSettings", new HeaderSettingsInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public HeaderSettingsInstance Construct()
        {
            return new HeaderSettingsInstance(InstancePrototype, new HeaderSettings());
        }
    }

    [Serializable]
    public class HeaderSettingsInstance : ObjectInstance
    {
        private readonly HeaderSettings m_headerSettings;

        public HeaderSettingsInstance(ObjectInstance prototype)
            : base(prototype)
        {
            PopulateFunctions();
        }

        public HeaderSettingsInstance(ObjectInstance prototype, HeaderSettings headerSettings)
            : this(prototype)
        {
            if (headerSettings == null)
                throw new ArgumentNullException("headerSettings");

            m_headerSettings = headerSettings;
        }

        public HeaderSettings HeaderSettings
        {
            get
            {
                return m_headerSettings;
            }
        }

        [JSProperty(Name = "centerText")]
        [JsonProperty("centerText")]
        public string CenterText
        {
            get
            {
                return m_headerSettings.CenterText;
            }
            set
            {
                m_headerSettings.CenterText = value;
            }
        }

        [JSProperty(Name = "contentSpacing")]
        [JSDoc("ternPropertyType", "number?")]
        [JsonProperty("contentSpacing")]
        public object ContentSpacing
        {
            get
            {
                if (m_headerSettings.ContentSpacing.HasValue == false)
                    return Null.Value;

                return m_headerSettings.ContentSpacing.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_headerSettings.ContentSpacing = null;
                else
                    m_headerSettings.ContentSpacing = TypeConverter.ToNumber(value);
            }
        }
        

        [JSProperty(Name = "fontName")]
        [JsonProperty("fontName")]
        public string FontName
        {
            get
            {
                return m_headerSettings.FontName;
            }
            set
            {
                m_headerSettings.FontName = value;
            }
        }

        [JSProperty(Name = "fontSize")]
        [JSDoc("ternPropertyType", "number?")]
        [JsonProperty("fontSize")]
        public object FontSize
        {
            get
            {
                if (m_headerSettings.FontSize.HasValue == false)
                    return Null.Value;

                return m_headerSettings.FontSize.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_headerSettings.FontSize = null;
                else
                    m_headerSettings.FontSize = TypeConverter.ToNumber(value);
            }
        }

        [JSProperty(Name = "htmlUrl")]
        [JsonProperty("htmlUrl")]
        public string HtmlUrl
        {
            get
            {
                return m_headerSettings.HtmlUrl;
            }
            set
            {
                m_headerSettings.HtmlUrl = value;
            }
        }

        [JSProperty(Name = "rightText")]
        [JsonProperty("rightText")]
        public string RightText
        {
            get
            {
                return m_headerSettings.RightText;
            }
            set
            {
                m_headerSettings.RightText = value;
            }
        }

        [JSProperty(Name = "leftText")]
        [JsonProperty("leftText")]
        public string LeftText
        {
            get
            {
                return m_headerSettings.LeftText;
            }
            set
            {
                m_headerSettings.LeftText = value;
            }
        }

        [JSProperty(Name = "useLineSeparator")]
        [JSDoc("ternPropertyType", "bool?")]
        [JsonProperty("useLineSeparator")]
        public object UseLineSeparator
        {
            get
            {
                if (m_headerSettings.UseLineSeparator.HasValue == false)
                    return Null.Value;

                return m_headerSettings.UseLineSeparator.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_headerSettings.UseLineSeparator = null;
                else
                    m_headerSettings.UseLineSeparator = TypeConverter.ToBoolean(value);
            }
        }
    }
}
