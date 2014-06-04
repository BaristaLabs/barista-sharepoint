namespace Barista.SharePoint.Library
{
    using System.Linq;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint;

    [Serializable]
    public class SPDocTemplateCollectionConstructor : ClrFunction
    {
        public SPDocTemplateCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPDocTemplateCollection", new SPDocTemplateCollectionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPDocTemplateCollectionInstance Construct()
        {
            return new SPDocTemplateCollectionInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPDocTemplateCollectionInstance : ObjectInstance
    {
        private readonly SPDocTemplateCollection m_docTemplateCollection;

        public SPDocTemplateCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPDocTemplateCollectionInstance(ObjectInstance prototype, SPDocTemplateCollection docTemplateCollection)
            : this(prototype)
        {
            if (docTemplateCollection == null)
                throw new ArgumentNullException("docTemplateCollection");

            m_docTemplateCollection = docTemplateCollection;
        }

        public SPDocTemplateCollection SPDocTemplateCollection
        {
            get
            {
                return m_docTemplateCollection;
            }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return m_docTemplateCollection.Count;
            }
        }

        [JSFunction(Name = "getDocTemplateByIndex")]
        public SPDocTemplateInstance GetDocTemplateByIndex(int index)
        {
            var result = m_docTemplateCollection[index];
            return result == null
                ? null
                : new SPDocTemplateInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getSchemaXml")]
        public string GetSchemaXml()
        {
            return m_docTemplateCollection.SchemaXml;
        }

        [JSFunction(Name = "getWeb")]
        public SPWebInstance GetWeb()
        {
            return m_docTemplateCollection.Web == null
                ? null
                : new SPWebInstance(this.Engine, m_docTemplateCollection.Web);
        }

        [JSFunction(Name = "toArray")]
        public ArrayInstance ToArray()
        {
            var result = this.Engine.Array.Construct();
            foreach (var sol in m_docTemplateCollection
                .OfType<SPDocTemplate>()
                .Select(t => new SPDocTemplateInstance(this.Engine.Object.InstancePrototype, t)))
            {
                ArrayInstance.Push(result, sol);
            }

            return result;
        }
    }
}
