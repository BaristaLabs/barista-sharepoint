namespace Barista.Library
{
    using Barista.Extensions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Newtonsoft.Json.Linq;

    [Serializable]
    public class JsonMergeSettingsConstructor : ClrFunction
    {
        public JsonMergeSettingsConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "JsonMergeSettings", new JsonMergeSettingsInstance(engine))
        {
        }

        [JSConstructorFunction]
        public JsonMergeSettingsInstance Construct()
        {
            return new JsonMergeSettingsInstance(this.Engine, new JsonMergeSettings());
        }
    }

    [Serializable]
    public class JsonMergeSettingsInstance : ObjectInstance
    {
        private readonly JsonMergeSettings m_jsonMergeSettings = new JsonMergeSettings();

        public JsonMergeSettingsInstance(ScriptEngine engine)
            : base(engine)
        {
            this.PopulateFunctions();
        }

        public JsonMergeSettingsInstance(ScriptEngine engine, JsonMergeSettings jsonMergeSettings)
            : this(engine)
        {
            if (jsonMergeSettings != null)
                m_jsonMergeSettings = jsonMergeSettings;
        }

        protected JsonMergeSettingsInstance(ObjectInstance prototype, JsonMergeSettings jsonMergeSettings)
            : base(prototype)
        {
            if (jsonMergeSettings == null)
                throw new ArgumentNullException("jsonMergeSettings");

            m_jsonMergeSettings = jsonMergeSettings;
        }

        public JsonMergeSettings JsonMergeSettings
        {
            get
            {
                return m_jsonMergeSettings;
            }
        }

        [JSProperty(Name = "mergeArrayHandling")]
        public string MergeArrayHandling
        {
            get
            {
                return m_jsonMergeSettings.MergeArrayHandling.ToString();
            }
            set
            {
                MergeArrayHandling handling;
                if (value.TryParseEnum(true, out handling))
                {
                    m_jsonMergeSettings.MergeArrayHandling = handling;
                }
            }
        }
    }
}
