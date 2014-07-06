namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Library;
    using Microsoft.SharePoint;
    using System;
    using System.Reflection;

    [Serializable]
    public class SPFieldLookupValueCollectionConstructor : ClrFunction
    {
        public SPFieldLookupValueCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPFieldLookupValueCollection", new SPFieldLookupValueCollectionInstance(engine, null))
        {
        }

        [JSConstructorFunction]
        public SPFieldLookupValueCollectionInstance Construct(object fieldValue)
        {
            if (fieldValue == Undefined.Value)
                return new SPFieldLookupValueCollectionInstance(this.Engine, new SPFieldLookupValueCollection());

            return new SPFieldLookupValueCollectionInstance(this.Engine, new SPFieldLookupValueCollection(TypeConverter.ToString(fieldValue)));
        }
    }

    [Serializable]
    public class SPFieldLookupValueCollectionInstance : ListInstance<SPFieldLookupValueInstance, SPFieldLookupValue>
    {
        private readonly SPFieldLookupValueCollection m_fieldLookupValueCollection;
        public SPFieldLookupValueCollectionInstance(ScriptEngine engine, SPFieldLookupValueCollection fieldLookupValueCollection)
            : base(new ListInstance<SPFieldLookupValueCollectionInstance, SPFieldUserValue>(engine))
        {
            this.m_fieldLookupValueCollection = fieldLookupValueCollection;
            this.List = fieldLookupValueCollection;
            this.PopulateFunctions(this.GetType(),
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        }

        protected override SPFieldLookupValueInstance Wrap(SPFieldLookupValue fieldLookupValue)
        {
            return fieldLookupValue == null
                ? null
                : new SPFieldLookupValueInstance(this.Engine, fieldLookupValue);
        }

        protected override SPFieldLookupValue Unwrap(SPFieldLookupValueInstance fieldLookupValueInstance)
        {
            return fieldLookupValueInstance == null
                ? null
                : fieldLookupValueInstance.SPFieldLookupValue;
        }

        public override string ToString()
        {
            return m_fieldLookupValueCollection.ToString();
        }
    }
}
