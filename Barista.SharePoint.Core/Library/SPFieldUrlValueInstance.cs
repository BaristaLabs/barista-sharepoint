namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint;

    [Serializable]
    public class SPFieldUrlValueConstructor : ClrFunction
    {
        public SPFieldUrlValueConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPFieldUrlValue", new SPFieldUrlValueInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPFieldUrlValueInstance Construct(object fieldValue)
        {
            return fieldValue == Undefined.Value
                ? new SPFieldUrlValueInstance(this.InstancePrototype, new SPFieldUrlValue())
                : new SPFieldUrlValueInstance(this.InstancePrototype, new SPFieldUrlValue(TypeConverter.ToString(fieldValue)));
        }
    }

    [Serializable]
    public class SPFieldUrlValueInstance : ObjectInstance
    {
        private readonly SPFieldUrlValue m_fieldUrlValue;

        public SPFieldUrlValueInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPFieldUrlValueInstance(ObjectInstance prototype, SPFieldUrlValue fieldUrlValue)
            : this(prototype)
        {
            if (fieldUrlValue == null)
                throw new ArgumentNullException("fieldUrlValue");

            m_fieldUrlValue = fieldUrlValue;
        }

        public SPFieldUrlValue SPFieldUrlValue
        {
            get
            {
                return m_fieldUrlValue;
            }
        }

        [JSProperty(Name="description")]
        public string Description
        {
            get
            {
                return m_fieldUrlValue.Description;
            }
            set
            {
                m_fieldUrlValue.Description = value;
            }
        }

        [JSProperty(Name = "url")]
        public string Url
        {
            get
            {
                return m_fieldUrlValue.Url;
            }
            set
            {
                m_fieldUrlValue.Url = value;
            }
        }

        [JSFunction(Name = "toString")]
        public override string ToString()
        {
            return m_fieldUrlValue.ToString();
        }
    }
}
