namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Library;
    using Microsoft.SharePoint;
    using System;
    using System.Reflection;

    [Serializable]
    public class SPFieldUserValueCollectionConstructor : ClrFunction
    {
        public SPFieldUserValueCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPFieldUserValueCollection", new SPFieldUserValueCollectionInstance(engine, null))
        {
        }

        [JSConstructorFunction]
        public SPFieldUserValueCollectionInstance Construct(object web, object fieldValue)
        {
            if (web == Undefined.Value && fieldValue == Undefined.Value)
                return new SPFieldUserValueCollectionInstance(this.Engine, new SPFieldUserValueCollection());

            var spWeb = web as SPWebInstance;
            if (spWeb == null)
                throw new JavaScriptException(this.Engine, "Error", "Web argument, when specified, must be an instance of an SPWeb.");

            return new SPFieldUserValueCollectionInstance(this.Engine, new SPFieldUserValueCollection(spWeb.Web, TypeConverter.ToString(fieldValue)));
        }
    }

    [Serializable]
    public class SPFieldUserValueCollectionInstance : ListInstance<SPFieldUserValueInstance, SPFieldUserValue>
    {
        private readonly SPFieldUserValueCollection m_fieldUserValueCollection;
        public SPFieldUserValueCollectionInstance(ScriptEngine engine, SPFieldUserValueCollection fieldUserValueCollection)
            : base(new ListInstance<SPFieldUserValueCollectionInstance, SPFieldUserValue>(engine))
        {
            this.m_fieldUserValueCollection = fieldUserValueCollection;
            this.List = fieldUserValueCollection;
            this.PopulateFunctions(this.GetType(),
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        }

        protected override SPFieldUserValueInstance Wrap(SPFieldUserValue fieldUserValue)
        {
            return fieldUserValue == null
                ? null
                : new SPFieldUserValueInstance(this.Engine, fieldUserValue);
        }

        protected override SPFieldUserValue Unwrap(SPFieldUserValueInstance fieldUserValueInstance)
        {
            return fieldUserValueInstance == null
                ? null
                : fieldUserValueInstance.SPFieldUserValue;
        }

        public override string ToString()
        {
            return m_fieldUserValueCollection.ToString();
        }
    }
}
