namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint;

    [Serializable]
    public class SPFieldLookupValueConstructor : ClrFunction
    {
        public SPFieldLookupValueConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPFieldLookupValue", new SPFieldLookupValueInstance(engine, null))
        {
        }

        [JSConstructorFunction]
        public SPFieldLookupValueInstance Construct(object valueOrId, object value)
        {
            if (valueOrId == Undefined.Value && value == Undefined.Value)
                return new SPFieldLookupValueInstance(this.Engine, new SPFieldLookupValue());

            if (value == Undefined.Value)
                return new SPFieldLookupValueInstance(this.Engine, new SPFieldLookupValue(TypeConverter.ToString(valueOrId)));

            return new SPFieldLookupValueInstance(this.Engine, new SPFieldLookupValue(TypeConverter.ToInt32(valueOrId), TypeConverter.ToString(value)));
        }
    }

    [Serializable]
    public class SPFieldLookupValueInstance : ObjectInstance
    {
        private readonly SPFieldLookupValue m_fieldLookupValue;

        public SPFieldLookupValueInstance(ScriptEngine engine, SPFieldLookupValue fieldLookupValue)
            : base(engine)
        {
            m_fieldLookupValue = fieldLookupValue;

            this.PopulateFunctions();
        }

        protected SPFieldLookupValueInstance(ObjectInstance prototype, SPFieldLookupValue fieldLookupValue)
            : base(prototype)
        {
            m_fieldLookupValue = fieldLookupValue;
        }

        public SPFieldLookupValue SPFieldLookupValue
        {
            get
            {
                return m_fieldLookupValue;
            }
        }

        [JSProperty(Name = "lookupId")]
        public int LookupId
        {
            get
            {
                return m_fieldLookupValue.LookupId;
            }
            set
            {
                m_fieldLookupValue.LookupId = value;
            }
        }

        [JSProperty(Name = "lookupValue")]
        public string LookupValue
        {
            get
            {
                return m_fieldLookupValue.LookupValue;
            }
        }

        [JSFunction(Name = "toStringJS")]
        public override string ToString()
        {
            return m_fieldLookupValue.ToString();
        }
    }
}
