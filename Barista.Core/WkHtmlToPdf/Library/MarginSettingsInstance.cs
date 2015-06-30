namespace Barista.WkHtmlToPdf.Library
{

    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Extensions;
    using Barista.TuesPechkin;
    using Barista.Newtonsoft.Json;

    [Serializable]
    public class MarginSettingsConstructor : ClrFunction
    {
        public MarginSettingsConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "MarginSettings", new MarginSettingsInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public MarginSettingsInstance Construct()
        {
            return new MarginSettingsInstance(InstancePrototype, new MarginSettings());
        }
    }

    [Serializable]
    public class MarginSettingsInstance : ObjectInstance
    {
        private readonly MarginSettings m_marginSettings;

        public MarginSettingsInstance(ObjectInstance prototype)
            : base(prototype)
        {
            PopulateFunctions();
        }

        public MarginSettingsInstance(ObjectInstance prototype, MarginSettings marginSettings)
            : this(prototype)
        {
            if (marginSettings == null)
                throw new ArgumentNullException("marginSettings");

            m_marginSettings = marginSettings;
        }

        public MarginSettings MarginSettings
        {
            get
            {
                return m_marginSettings;
            }
        }

        [JSProperty(Name = "all")]
        [JSDoc("ternPropertyType", "number")]
        [JsonProperty("all")]
        public object OutlineDepth
        {
            set
            {
                m_marginSettings.All = TypeConverter.ToNumber(value);
            }
        }

        [JSProperty(Name = "top")]
        [JSDoc("ternPropertyType", "number?")]
        [JsonProperty("top")]
        public object Top
        {
            get
            {
                if (m_marginSettings.Top.HasValue == false)
                    return Null.Value;

                return m_marginSettings.Top.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_marginSettings.Top = null;
                else
                    m_marginSettings.Top = TypeConverter.ToNumber(value);
            }
        }

        [JSProperty(Name = "bottom")]
        [JSDoc("ternPropertyType", "number?")]
        [JsonProperty("bottom")]
        public object Bottom
        {
            get
            {
                if (m_marginSettings.Bottom.HasValue == false)
                    return Null.Value;

                return m_marginSettings.Bottom.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_marginSettings.Bottom = null;
                else
                    m_marginSettings.Bottom = TypeConverter.ToNumber(value);
            }
        }

        [JSProperty(Name = "left")]
        [JSDoc("ternPropertyType", "number?")]
        [JsonProperty("left")]
        public object Left
        {
            get
            {
                if (m_marginSettings.Left.HasValue == false)
                    return Null.Value;

                return m_marginSettings.Left.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_marginSettings.Left = null;
                else
                    m_marginSettings.Left = TypeConverter.ToNumber(value);
            }
        }

        [JSProperty(Name = "right")]
        [JSDoc("ternPropertyType", "number?")]
        [JsonProperty("right")]
        public object Right
        {
            get
            {
                if (m_marginSettings.Right.HasValue == false)
                    return Null.Value;

                return m_marginSettings.Right.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_marginSettings.Right = null;
                else
                    m_marginSettings.Right = TypeConverter.ToNumber(value);
            }
        }

        [JSProperty(Name = "unit")]
        [JsonProperty("unit")]
        public string Unit
        {
            get
            {
                return m_marginSettings.Unit.ToString();
            }
            set
            {
                Unit unit;
                if (value.TryParseEnum(true, out unit))
                    m_marginSettings.Unit = unit;
            }
        }
    }
}
