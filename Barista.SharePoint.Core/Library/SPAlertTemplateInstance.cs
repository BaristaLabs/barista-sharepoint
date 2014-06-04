namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Library;
    using Microsoft.SharePoint;
    using System;

    [Serializable]
    public class SPAlertTemplateConstructor : ClrFunction
    {
        public SPAlertTemplateConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPAlertTemplate", new SPAlertTemplateInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPAlertTemplateInstance Construct()
        {
            return new SPAlertTemplateInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPAlertTemplateInstance : ObjectInstance
    {
        private readonly SPAlertTemplate m_alertTemplate;

        public SPAlertTemplateInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPAlertTemplateInstance(ObjectInstance prototype, SPAlertTemplate alertTemplate)
            : this(prototype)
        {
            if (alertTemplate == null)
                throw new ArgumentNullException("alertTemplate");

            m_alertTemplate = alertTemplate;
        }

        public SPAlertTemplate SPAlertTemplate
        {
            get
            {
                return m_alertTemplate;
            }
        }

        [JSProperty(Name = "displayName")]
        public string DisplayName
        {
            get
            {
                return m_alertTemplate.DisplayName;
            }
        }

        [JSProperty(Name = "id")]
        public object Id
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_alertTemplate.Id);
            }
            set
            {
                var guidValue = GuidInstance.ConvertFromJsObjectToGuid(value);
                m_alertTemplate.Id = guidValue;
            }
        }

        [JSProperty(Name = "name")]
        public string Name
        {
            get
            {
                return m_alertTemplate.Name;
            }
            set
            {
                m_alertTemplate.Name = value;
            }
        }

        [JSProperty(Name = "propertyBag")]
        public HashtableInstance PropertyBag
        {
            get
            {
                return m_alertTemplate.Properties == null
                    ? null
                    : new HashtableInstance(this.Engine.Object.InstancePrototype, m_alertTemplate.Properties);
            }
        }

        [JSProperty(Name = "status")]
        public string Status
        {
            get
            {
                return m_alertTemplate.Status.ToString();
            }
        }


        [JSProperty(Name = "xml")]
        public string Xml
        {
            get
            {
                return m_alertTemplate.Xml;
            }
            set
            {
                m_alertTemplate.Xml = value;
            }
        }

        [JSFunction(Name = "delete")]
        public void Delete()
        {
            m_alertTemplate.Delete();
        }

        [JSFunction(Name = "getLocalizedXml")]
        public string GetLocalizedXml(double lcid)
        {
            return m_alertTemplate.GetLocalizedXml((uint)lcid);
        }

        [JSFunction(Name = "update")]
        public void Update()
        {
            m_alertTemplate.Update();
        }
    }
}
