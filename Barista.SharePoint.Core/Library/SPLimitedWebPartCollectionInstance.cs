namespace Barista.SharePoint.Library
{
    using System.Linq;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;
    using Microsoft.SharePoint.WebPartPages;

    [Serializable]
    public class SPLimitedWebPartCollectionConstructor : ClrFunction
    {
        public SPLimitedWebPartCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPLimitedWebPartCollection", new SPLimitedWebPartCollectionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPLimitedWebPartCollectionInstance Construct()
        {
            return new SPLimitedWebPartCollectionInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPLimitedWebPartCollectionInstance : ObjectInstance
    {
        private readonly SPLimitedWebPartCollection m_limitedWebPartCollection;

        public SPLimitedWebPartCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPLimitedWebPartCollectionInstance(ObjectInstance prototype, SPLimitedWebPartCollection limitedWebPartCollection)
            : this(prototype)
        {
            if (limitedWebPartCollection == null)
                throw new ArgumentNullException("limitedWebPartCollection");

            m_limitedWebPartCollection = limitedWebPartCollection;
        }

        public SPLimitedWebPartCollection SPLimitedWebPartCollection
        {
            get
            {
                return m_limitedWebPartCollection;
            }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return m_limitedWebPartCollection.Count;
            }
        }

        [JSFunction(Name = "contains")]
        public bool Contains(SPWebPartInstance value)
        {
            if (value == null)
                throw new JavaScriptException(this.Engine, "Error", "A web part must be provided as the first argument.");

            return m_limitedWebPartCollection.Contains(value.WebPart);
        }

        [JSFunction(Name = "indexOf")]
        public int IndexOf(SPWebPartInstance value)
        {
            if (value == null)
                throw new JavaScriptException(this.Engine, "Error", "A web part must be provided as the first argument.");

            return m_limitedWebPartCollection.IndexOf(value.WebPart);
        }

        [JSFunction(Name = "getWebPartByStorageKey")]
        public SPWebPartInstance GetWebPartByStorageKey(object storageKey)
        {
            var guidId = GuidInstance.ConvertFromJsObjectToGuid(storageKey);
            var result = m_limitedWebPartCollection[guidId] as WebPart;

            return result == null
                ? null
                : new SPWebPartInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getWebPartById")]
        public SPWebPartInstance GetWebPartById(string id)
        {
            var result = m_limitedWebPartCollection[id] as WebPart;

            return result == null
                ? null
                : new SPWebPartInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getWebPartByIndex")]
        public SPWebPartInstance GetWebPartByIndex(int index)
        {
            var result = m_limitedWebPartCollection[index] as WebPart;

            return result == null
                ? null
                : new SPWebPartInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "toArray")]
        public ArrayInstance ToArray()
        {
            var result = this.Engine.Array.Construct();

            foreach (var webPart in m_limitedWebPartCollection.OfType<WebPart>())
            {
                ArrayInstance.Push(result, new SPWebPartInstance(this.Engine.Object.InstancePrototype, webPart));
            }

            return result;
        }
    }
}
