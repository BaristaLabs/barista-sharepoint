namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Library;
    using Microsoft.SharePoint;
    using System;

    [Serializable]
    public class SPWebInfoConstructor : ClrFunction
    {
        public SPWebInfoConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPWebInfo", new SPWebInfoInstance(engine))
        {
        }

        [JSConstructorFunction]
        public SPWebInfoInstance Construct()
        {
            return new SPWebInfoInstance(this.Engine);
        }
    }

    [Serializable]
    public class SPWebInfoInstance : ObjectInstance
    {
        private readonly SPWebInfo m_webInfo;

        internal SPWebInfoInstance(ScriptEngine engine)
            : base(engine)
        {
            this.PopulateFunctions();
        }

        public SPWebInfoInstance(ScriptEngine engine, SPWebInfo webInfo)
            : this(engine)
        {
            if (webInfo == null)
                throw new JavaScriptException(engine, "Error", "$camelCasedName must be specified.");

            m_webInfo = webInfo;
        }

        protected SPWebInfoInstance(ObjectInstance prototype, SPWebInfo webInfo)
            : base(prototype)
        {
            if (webInfo == null)
                throw new ArgumentNullException("webInfo");

            m_webInfo = webInfo;
        }

        public SPWebInfo SPWebInfo
        {
            get
            {
                return m_webInfo;
            }
        }

        [JSProperty(Name = "configuration")]
        public int Configuration
        {
            get
            {
                return m_webInfo.Configuration;
            }
        }

        [JSProperty(Name = "customMasterUrl")]
        public string CustomMasterUrl
        {
            get
            {
                return m_webInfo.CustomMasterUrl;
            }
        }

        [JSProperty(Name = "description")]
        public string Description
        {
            get
            {
                return m_webInfo.Description;
            }
        }

        [JSProperty(Name = "id")]
        public GuidInstance Id
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_webInfo.Id);
            }
        }

        [JSProperty(Name = "language")]
        public double Language
        {
            get
            {
                return m_webInfo.Language;
            }
        }

        [JSProperty(Name = "dastItemModifiedDate")]
        public DateInstance LastItemModifiedDate
        {
            get
            {
                return JurassicHelper.ToDateInstance(this.Engine, m_webInfo.LastItemModifiedDate);
            }
        }

        [JSProperty(Name = "masterUrl")]
        public string MasterUrl
        {
            get
            {
                return m_webInfo.MasterUrl;
            }
        }

        [JSProperty(Name = "serverRelativeUrl")]
        public string ServerRelativeUrl
        {
            get
            {
                return m_webInfo.ServerRelativeUrl;
            }
        }

        [JSProperty(Name = "title")]
        public string Title
        {
            get
            {
                return m_webInfo.Title;
            }
        }

        [JSProperty(Name = "uiVersion")]
        public int UIVersion
        {
            get
            {
                return m_webInfo.UIVersion;
            }
        }

        [JSProperty(Name = "uiVersionConfigurationEnabled")]
        public bool UIVersionConfigurationEnabled
        {
            get
            {
                return m_webInfo.UIVersionConfigurationEnabled;
            }
        }

        [JSProperty(Name = "webTemplateId")]
        public int WebTemplateId
        {
            get
            {
                return m_webInfo.WebTemplateId;
            }
        }
    }
}
