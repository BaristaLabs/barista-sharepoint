namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;
    using Microsoft.SharePoint.Administration;

    [Serializable]
    public class SPIisWebServiceApplicationConstructor : ClrFunction
    {
        public SPIisWebServiceApplicationConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPIisWebServiceApplication", new SPIisWebServiceApplicationInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPIisWebServiceApplicationInstance Construct()
        {
            return new SPIisWebServiceApplicationInstance(InstancePrototype);
        }
    }

    [Serializable]
    public class SPIisWebServiceApplicationInstance : ObjectInstance
    {
        private readonly SPIisWebServiceApplication m_iisWebServiceApplication;

        public SPIisWebServiceApplicationInstance(ObjectInstance prototype)
            : base(prototype)
        {
            PopulateFields();
            PopulateFunctions();
        }

        public SPIisWebServiceApplicationInstance(ObjectInstance prototype, SPIisWebServiceApplication iisWebServiceApplication)
            : this(prototype)
        {
            if (iisWebServiceApplication == null)
                throw new ArgumentNullException("iisWebServiceApplication");

            m_iisWebServiceApplication = iisWebServiceApplication;
        }

        public SPIisWebServiceApplication SPIisWebServiceApplication
        {
            get { return m_iisWebServiceApplication; }
        }

        [JSProperty(Name = "id")]
        public GuidInstance Id
        {
            get { return new GuidInstance(Engine.Object.InstancePrototype, m_iisWebServiceApplication.Id); }
            set { m_iisWebServiceApplication.Id = value.Value; }
        }

        [JSProperty(Name = "name")]
        public string Name
        {
            get { return m_iisWebServiceApplication.Name; }
            set { m_iisWebServiceApplication.Name = value; }
        }

        [JSProperty(Name = "applicationPool")]
        public SPIisWebServiceApplicationPoolInstance ApplicationPool
        {
            get
            {
                if (m_iisWebServiceApplication.ApplicationPool == null)
                    return null;
                return new SPIisWebServiceApplicationPoolInstance(Engine.Object.InstancePrototype,
                  m_iisWebServiceApplication.ApplicationPool);
            }
        }

        [JSProperty(Name = "displayName")]
        public string DisplayName
        {
            get { return m_iisWebServiceApplication.DisplayName; }
        }

        [JSProperty(Name = "manageLink")]
        public string ManageLink
        {
            get { return m_iisWebServiceApplication.ManageLink.Url; }
        }

        [JSProperty(Name = "propertiesLink")]
        public string PropertiesLink
        {
            get { return m_iisWebServiceApplication.PropertiesLink.Url; }
        }

        [JSFunction(Name = "getPropertyKeyValue")]
        public object GetFarmKeyValueAsObject(string key)
        {
            string val = Convert.ToString(m_iisWebServiceApplication.Properties[key]);

            object result;

            //Attempt to convert the string into a JSON Object.
            try
            {
                result = JSONObject.Parse(Engine, val, null);
            }
            catch
            {
                result = val;
            }

            return result;
        }

        [JSFunction(Name = "setPropertyKeyValue")]
        public void SetPropertyKeyValue(string key, object value)
        {
            if (value == null || value == Undefined.Value || value == Null.Value)
                throw new ArgumentNullException("value");

            string stringValue;
            if (value is ObjectInstance)
            {
                stringValue = JSONObject.Stringify(Engine, value, null, null);
            }
            else
            {
                stringValue = value.ToString();
            }

            if (m_iisWebServiceApplication.Properties.ContainsKey(key))
                m_iisWebServiceApplication.Properties[key] = stringValue;
            else
                m_iisWebServiceApplication.Properties.Add(key, stringValue);

            m_iisWebServiceApplication.Update();
        }

        [JSFunction(Name = "update")]
        public void Update()
        {
            m_iisWebServiceApplication.Update();
        }
    }
}
