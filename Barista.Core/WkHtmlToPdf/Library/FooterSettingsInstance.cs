namespace Barista.WkHtmlToPdf.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Newtonsoft.Json;
    using Barista.TuesPechkin;

    [Serializable]
    public class FooterSettingsConstructor : ClrFunction
    {
        public FooterSettingsConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "FooterSettings", new FooterSettingsInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public FooterSettingsInstance Construct()
        {
            return new FooterSettingsInstance(InstancePrototype, new FooterSettings());
        }
    }

    [Serializable]
    public class FooterSettingsInstance : ObjectInstance
    {
        private readonly FooterSettings m_footerSettings;

        public FooterSettingsInstance(ObjectInstance prototype)
            : base(prototype)
        {
            PopulateFunctions();
        }

        public FooterSettingsInstance(ObjectInstance prototype, FooterSettings footerSettings)
            : this(prototype)
        {
            if (footerSettings == null)
                throw new ArgumentNullException("footerSettings");

            m_footerSettings = footerSettings;
        }

        public FooterSettings FooterSettings
        {
            get
            {
                return m_footerSettings;
            }
        }

        [JSProperty(Name = "centerText")]
        [JsonProperty("centerText")]
        public string CenterText
        {
            get
            {
                return m_footerSettings.CenterText;
            }
            set
            {
                m_footerSettings.CenterText = value;
            }
        }

        [JSProperty(Name = "contentSpacing")]
        [JSDoc("ternPropertyType", "number?")]
        [JsonProperty("contentSpacing")]
        public object ContentSpacing
        {
            get
            {
                if (m_footerSettings.ContentSpacing.HasValue == false)
                    return Null.Value;

                return m_footerSettings.ContentSpacing.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_footerSettings.ContentSpacing = null;
                else
                    m_footerSettings.ContentSpacing = TypeConverter.ToNumber(value);
            }
        }


        [JSProperty(Name = "fontName")]
        [JsonProperty("fontName")]
        public string FontName
        {
            get
            {
                return m_footerSettings.FontName;
            }
            set
            {
                m_footerSettings.FontName = value;
            }
        }

        [JSProperty(Name = "fontSize")]
        [JSDoc("ternPropertyType", "number?")]
        [JsonProperty("fontSize")]
        public object FontSize
        {
            get
            {
                if (m_footerSettings.FontSize.HasValue == false)
                    return Null.Value;

                return m_footerSettings.FontSize.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_footerSettings.FontSize = null;
                else
                    m_footerSettings.FontSize = TypeConverter.ToNumber(value);
            }
        }

        [JSProperty(Name = "htmlUrl")]
        [JsonProperty("htmlUrl")]
        public string HtmlUrl
        {
            get
            {
                return m_footerSettings.HtmlUrl;
            }
            set
            {
                m_footerSettings.HtmlUrl = value;
            }
        }

        [JSProperty(Name = "rightText")]
        [JsonProperty("rightText")]
        public string RightText
        {
            get
            {
                return m_footerSettings.RightText;
            }
            set
            {
                m_footerSettings.RightText = value;
            }
        }

        [JSProperty(Name = "leftText")]
        [JsonProperty("leftText")]
        public string LeftText
        {
            get
            {
                return m_footerSettings.LeftText;
            }
            set
            {
                m_footerSettings.LeftText = value;
            }
        }

        [JSProperty(Name = "useLineSeparator")]
        [JSDoc("ternPropertyType", "bool?")]
        [JsonProperty("useLineSeparator")]
        public object UseLineSeparator
        {
            get
            {
                if (m_footerSettings.UseLineSeparator.HasValue == false)
                    return Null.Value;

                return m_footerSettings.UseLineSeparator.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_footerSettings.UseLineSeparator = null;
                else
                    m_footerSettings.UseLineSeparator = TypeConverter.ToBoolean(value);
            }
        }
    }
}
