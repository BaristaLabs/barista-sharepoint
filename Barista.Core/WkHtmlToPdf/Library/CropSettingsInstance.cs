namespace Barista.WkHtmlToPdf.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Newtonsoft.Json;
    using Barista.TuesPechkin;

    [Serializable]
    public class CropSettingsConstructor : ClrFunction
    {
        public CropSettingsConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "CropSettings", new CropSettingsInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public CropSettingsInstance Construct()
        {
            return new CropSettingsInstance(InstancePrototype, new CropSettings());
        }
    }

    [Serializable]
    public class CropSettingsInstance : ObjectInstance
    {
        private readonly CropSettings m_cropSettings;

        public CropSettingsInstance(ObjectInstance prototype)
            : base(prototype)
        {
            PopulateFunctions();
        }

        public CropSettingsInstance(ObjectInstance prototype, CropSettings cropSettings)
            : this(prototype)
        {
            if (cropSettings == null)
                throw new ArgumentNullException("cropSettings");

            m_cropSettings = cropSettings;
        }

        public CropSettings CropSettings
        {
            get
            {
                return m_cropSettings;
            }
        }

        [JSProperty(Name = "top")]
        [JSDoc("ternPropertyType", "number?")]
        [JsonProperty("top")]
        public object Top
        {
            get
            {
                if (m_cropSettings.Top.HasValue == false)
                    return Null.Value;

                return m_cropSettings.Top.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_cropSettings.Top = null;
                else
                    m_cropSettings.Top = TypeConverter.ToNumber(value);
            }
        }

        [JSProperty(Name = "bottom")]
        [JSDoc("ternPropertyType", "number?")]
        [JsonProperty("bottom")]
        public object Bottom
        {
            get
            {
                if (m_cropSettings.Bottom.HasValue == false)
                    return Null.Value;

                return m_cropSettings.Bottom.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_cropSettings.Bottom = null;
                else
                    m_cropSettings.Bottom = TypeConverter.ToNumber(value);
            }
        }

        [JSProperty(Name = "width")]
        [JSDoc("ternPropertyType", "number?")]
        [JsonProperty("width")]
        public object Width
        {
            get
            {
                if (m_cropSettings.Width.HasValue == false)
                    return Null.Value;

                return m_cropSettings.Width.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_cropSettings.Width = null;
                else
                    m_cropSettings.Width = TypeConverter.ToNumber(value);
            }
        }

        [JSProperty(Name = "height")]
        [JSDoc("ternPropertyType", "number?")]
        [JsonProperty("height")]
        public object Height
        {
            get
            {
                if (m_cropSettings.Height.HasValue == false)
                    return Null.Value;

                return m_cropSettings.Height.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_cropSettings.Height = null;
                else
                    m_cropSettings.Height = TypeConverter.ToNumber(value);
            }
        }
    }
}
