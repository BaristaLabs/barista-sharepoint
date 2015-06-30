namespace Barista.WkHtmlToPdf.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Extensions;
    using Barista.Newtonsoft.Json;
    using Barista.TuesPechkin;

    [Serializable]
    public class LoadSettingsConstructor : ClrFunction
    {
        public LoadSettingsConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "LoadSettings", new LoadSettingsInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public LoadSettingsInstance Construct()
        {
            return new LoadSettingsInstance(InstancePrototype, new LoadSettings());
        }
    }

    [Serializable]
    public class LoadSettingsInstance : ObjectInstance
    {
        private readonly LoadSettings m_loadSettings;

        public LoadSettingsInstance(ObjectInstance prototype)
            : base(prototype)
        {
            PopulateFunctions();
        }

        public LoadSettingsInstance(ObjectInstance prototype, LoadSettings loadSettings)
            : this(prototype)
        {
            if (loadSettings == null)
                throw new ArgumentNullException("loadSettings");

            m_loadSettings = loadSettings;
        }

        public LoadSettings LoadSettings
        {
            get
            {
                return m_loadSettings;
            }
        }

        [JSProperty(Name = "blockLocalFileAccess")]
        [JSDoc("ternPropertyType", "bool?")]
        [JsonProperty("blockLocalFileAccess")]
        public object BlockLocalFileAccess
        {
            get
            {
                if (m_loadSettings.BlockLocalFileAccess.HasValue == false)
                    return Null.Value;

                return m_loadSettings.BlockLocalFileAccess.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_loadSettings.BlockLocalFileAccess = null;
                else
                    m_loadSettings.BlockLocalFileAccess = TypeConverter.ToBoolean(value);
            }
        }

        //cookies
        //customheaders

        [JSProperty(Name = "debugJavascript")]
        [JSDoc("ternPropertyType", "bool?")]
        [JsonProperty("debugJavascript")]
        public object DebugJavascript
        {
            get
            {
                if (m_loadSettings.DebugJavascript.HasValue == false)
                    return Null.Value;

                return m_loadSettings.DebugJavascript.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_loadSettings.DebugJavascript = null;
                else
                    m_loadSettings.DebugJavascript = TypeConverter.ToBoolean(value);
            }
        }

        [JSProperty(Name = "errorHandling")]
        [JsonProperty("errorHandling")]
        public string ErrorHandling
        {
            get
            {
                if (m_loadSettings.ErrorHandling.HasValue == false)
                    return null;

                return m_loadSettings.ErrorHandling.ToString();
            }
            set
            {
                if (value.IsNullOrWhiteSpace())
                    m_loadSettings.ErrorHandling = null;
                else
                {
                    LoadSettings.ContentErrorHandling errorHandling;
                    if (value.TryParseEnum(true, out errorHandling))
                        m_loadSettings.ErrorHandling = errorHandling;
                }
            }
        }

        [JSProperty(Name = "password")]
        [JsonProperty("password")]
        public string Password
        {
            get
            {
                return m_loadSettings.Password;
            }
            set
            {
                m_loadSettings.Password = value;
            }
        }

        [JSProperty(Name = "proxy")]
        [JsonProperty("proxy")]
        public string Proxy
        {
            get
            {
                return m_loadSettings.Proxy;
            }
            set
            {
                m_loadSettings.Proxy = value;
            }
        }

        [JSProperty(Name = "renderDelay")]
        [JSDoc("ternPropertyType", "number?")]
        [JsonProperty("renderDelay")]
        public object RenderDelay
        {
            get
            {
                if (m_loadSettings.RenderDelay.HasValue == false)
                    return Null.Value;

                return m_loadSettings.RenderDelay.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_loadSettings.RenderDelay = null;
                else
                    m_loadSettings.RenderDelay = TypeConverter.ToInt32(value);
            }
        }

        [JSProperty(Name = "repeatCustomHeaders")]
        [JSDoc("ternPropertyType", "bool?")]
        [JsonProperty("repeatCustomHeaders")]
        public object RepeatCustomHeaders
        {
            get
            {
                if (m_loadSettings.RepeatCustomHeaders.HasValue == false)
                    return Null.Value;

                return m_loadSettings.RepeatCustomHeaders.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_loadSettings.RepeatCustomHeaders = null;
                else
                    m_loadSettings.RepeatCustomHeaders = TypeConverter.ToBoolean(value);
            }
        }

        [JSProperty(Name = "stopSlowScript")]
        [JSDoc("ternPropertyType", "bool?")]
        [JsonProperty("stopSlowScript")]
        public object StopSlowScript
        {
            get
            {
                if (m_loadSettings.StopSlowScript.HasValue == false)
                    return Null.Value;

                return m_loadSettings.StopSlowScript.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_loadSettings.StopSlowScript = null;
                else
                    m_loadSettings.StopSlowScript = TypeConverter.ToBoolean(value);
            }
        }

        [JSProperty(Name = "username")]
        [JsonProperty("username")]
        public string Username
        {
            get
            {
                return m_loadSettings.Username;
            }
            set
            {
                m_loadSettings.Username = value;
            }
        }

        [JSProperty(Name = "windowStatus")]
        [JsonProperty("windowStatus")]
        public string WindowStatus
        {
            get
            {
                return m_loadSettings.WindowStatus;
            }
            set
            {
                m_loadSettings.WindowStatus = value;
            }
        }

        [JSProperty(Name = "zoomFactor")]
        [JSDoc("ternPropertyType", "number?")]
        [JsonProperty("zoomFactor")]
        public object ZoomFactor
        {
            get
            {
                if (m_loadSettings.ZoomFactor.HasValue == false)
                    return Null.Value;

                return m_loadSettings.ZoomFactor.Value;
            }
            set
            {
                if (value == Null.Value || value == Undefined.Value || value == null)
                    m_loadSettings.ZoomFactor = null;
                else
                    m_loadSettings.ZoomFactor = TypeConverter.ToNumber(value);
            }
        }
    }
}
