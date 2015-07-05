namespace Barista.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Newtonsoft.Json;

    [Serializable]
    public class BaristaCookieConstructor : ClrFunction
    {
        public BaristaCookieConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "BaristaCookie", new BaristaCookieInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public BaristaCookieInstance Construct(string name, string value)
        {
            return new BaristaCookieInstance(this.InstancePrototype, new Biscotti());
        }
    }

    [Serializable]
    public class BaristaCookieInstance : ObjectInstance
    {
        private readonly IBaristaCookie m_baristaCookie;

        public BaristaCookieInstance(ObjectInstance prototype)
            : base(prototype)
        {
            m_baristaCookie = new Biscotti();
            this.PopulateFunctions();
        }

        public BaristaCookieInstance(ObjectInstance prototype, IBaristaCookie baristaCookie)
            : this(prototype)
        {
            if (baristaCookie == null)
                throw new ArgumentNullException("baristaCookie");

            m_baristaCookie = baristaCookie;
        }

        public IBaristaCookie BaristaCookie
        {
            get
            {
                return m_baristaCookie;
            }
        }

        [JSProperty(Name = "domain")]
        [JsonProperty("domain")]
        public string Domain
        {
            get
            {
                return m_baristaCookie.Domain;
            }
            set
            {
                m_baristaCookie.Domain = value;
            }
        }

        [JSProperty(Name = "encodedName")]
        [JsonProperty("encodedName")]
        public string EncodedName
        {
            get
            {
                return m_baristaCookie.EncodedName;
            }
        }

        [JSProperty(Name = "encodedValue")]
        [JsonProperty("encodedValue")]
        public string EncodedValue
        {
            get
            {
                return m_baristaCookie.EncodedValue;
            }
        }

        [JSProperty(Name = "expires")]
        [JsonProperty("expires")]
        public object Expires
        {
            get
            {
                if (m_baristaCookie.Expires.HasValue)
                    return JurassicHelper.ToDateInstance(Engine, m_baristaCookie.Expires.Value);

                return null;
            }
            set
            {
                if (value == null || value == Null.Value || value == Undefined.Value)
                {
                    m_baristaCookie.Expires = null;
                    return;
                }

                var valueDate = value as DateInstance;
                if (valueDate != null)
                    m_baristaCookie.Expires = valueDate.Value;
            }
        }

        [JSProperty(Name = "httpOnly")]
        [JsonProperty("httpOnly")]
        public bool HttpOnly
        {
            get
            {
                return m_baristaCookie.HttpOnly;
            }
            set
            {
                m_baristaCookie.HttpOnly = value;
            }
        }

        [JSProperty(Name = "name")]
        [JsonProperty("name")]
        public string Name
        {
            get
            {
                return m_baristaCookie.Name;
            }
            set
            {
                m_baristaCookie.Name = value;
            }
        }

        [JSProperty(Name = "path")]
        [JsonProperty("path")]
        public string Path
        {
            get
            {
                return m_baristaCookie.Path;
            }
            set
            {
                m_baristaCookie.Path = value;
            }
        }

        [JSProperty(Name = "secure")]
        [JsonProperty("secure")]
        public bool Secure
        {
            get
            {
                return m_baristaCookie.Secure;
            }
            set
            {
                m_baristaCookie.Secure = value;
            }
        }

        [JSProperty(Name = "value")]
        [JsonProperty("value")]
        public string Value
        {
            get
            {
                return m_baristaCookie.Value;
            }
            set
            {
                m_baristaCookie.Value = value;
            }
        }
    }
}
