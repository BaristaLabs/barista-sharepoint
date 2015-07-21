namespace Barista.SharePoint.Library
{
    using System.Reflection;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint;

    [Serializable]
    public class SPFieldUserValueConstructor : ClrFunction
    {
        public SPFieldUserValueConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPFieldUserValue", new SPFieldUserValueInstance(engine, null))
        {
        }

        [JSConstructorFunction]
        public SPFieldUserValueInstance Construct(object web, object idOrValue, object value)
        {
            if (web == Undefined.Value && idOrValue == Undefined.Value && value == Undefined.Value)
                return new SPFieldUserValueInstance(this.Engine, new SPFieldUserValue());

            if ((web is SPWebInstance) == false)
                throw new JavaScriptException(this.Engine, "Error", "The first argument, when specified, must be an instance of an SPWeb.");

            var spWeb = web as SPWebInstance;

            if (idOrValue == Undefined.Value && value == Undefined.Value)
                return new SPFieldUserValueInstance(this.Engine, new SPFieldUserValue(spWeb.Web));

            return value == Undefined.Value
                ? new SPFieldUserValueInstance(this.Engine, new SPFieldUserValue(spWeb.Web, TypeConverter.ToString(idOrValue)))
                : new SPFieldUserValueInstance(this.Engine, new SPFieldUserValue(spWeb.Web, TypeConverter.ToInt32(idOrValue), TypeConverter.ToString(value)));
        }
    }

    [Serializable]
    public class SPFieldUserValueInstance : SPFieldLookupValueInstance
    {
        private readonly SPFieldUserValue m_fieldUserValue;

        public SPFieldUserValueInstance(ScriptEngine engine, SPFieldUserValue fieldUserValue)
            : base(new SPFieldLookupValueInstance(engine, fieldUserValue), fieldUserValue)
        {
            m_fieldUserValue = fieldUserValue;

            this.PopulateFunctions(this.GetType(), BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        }

        protected SPFieldUserValueInstance(ObjectInstance prototype, SPFieldUserValue fieldUserValue)
            : base(prototype, fieldUserValue)
        {
            m_fieldUserValue = fieldUserValue;
        }

        public SPFieldUserValue SPFieldUserValue
        {
            get
            {
                return m_fieldUserValue;
            }
        }

        [JSProperty(Name = "sipAddress")]
        public string SipAddress
        {
            get
            {
                return m_fieldUserValue.SipAddress;
            }
        }

        [JSProperty(Name = "user")]
        public SPUserInstance User
        {
            get
            {
                return m_fieldUserValue.User == null
                    ? null
                    : new SPUserInstance(Engine, m_fieldUserValue.User);
            }
        }

        public override string ToString()
        {
            return m_fieldUserValue.ToString();
        }
    }
}
