namespace Barista.SharePoint.Library
{
    using System.Linq;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint.WebPartPages;

    [Serializable]
    public class SPWebPartConnectionCollectionConstructor : ClrFunction
    {
        public SPWebPartConnectionCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPWebPartConnectionCollection", new SPWebPartConnectionCollectionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPWebPartConnectionCollectionInstance Construct()
        {
            return new SPWebPartConnectionCollectionInstance(InstancePrototype);
        }
    }

    [Serializable]
    public class SPWebPartConnectionCollectionInstance : ObjectInstance
    {
        private readonly SPWebPartConnectionCollection m_webPartConnectionCollection;

        public SPWebPartConnectionCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            PopulateFields();
            PopulateFunctions();
        }

        public SPWebPartConnectionCollectionInstance(ObjectInstance prototype, SPWebPartConnectionCollection webPartConnectionCollection)
            : this(prototype)
        {
            if (webPartConnectionCollection == null)
                throw new ArgumentNullException("webPartConnectionCollection");

            m_webPartConnectionCollection = webPartConnectionCollection;
        }

        public SPWebPartConnectionCollection SPWebPartConnectionCollection
        {
            get
            {
                return m_webPartConnectionCollection;
            }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return m_webPartConnectionCollection.Count;
            }
        }

        [JSProperty(Name = "isReadOnly")]
        public bool IsReadOnly
        {
            get
            {
                return m_webPartConnectionCollection.IsReadOnly;
            }
        }

        [JSFunction(Name = "add")]
        public int Add(SPWebPartConnectionInstance value)
        {
            if (value == null)
                throw new JavaScriptException(Engine, "Error", "A web part connection must be specified as the first argument");

            return m_webPartConnectionCollection.Add(value.SPWebPartConnection);
        }

        [JSFunction(Name = "contains")]
        public bool Contains(SPWebPartConnectionInstance value)
        {
            if (value == null)
                throw new JavaScriptException(Engine, "Error", "A web part connection must be specified as the first argument");

            return m_webPartConnectionCollection.Contains(value.SPWebPartConnection);
        }

        [JSFunction(Name = "clear")]
        public void Clear()
        {
            m_webPartConnectionCollection.Clear();
        }

        [JSFunction(Name = "getConnectionById")]
        public SPWebPartConnectionInstance GetConnectionById(string id)
        {
            var result = m_webPartConnectionCollection[id];
            return result == null
                ? null
                : new SPWebPartConnectionInstance(Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getConnectionByIndex")]
        public SPWebPartConnectionInstance GetConnectionByIndex(int index)
        {
            var result = m_webPartConnectionCollection[index];
            return result == null
                ? null
                : new SPWebPartConnectionInstance(Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "indexOf")]
        public int IndexOf(SPWebPartConnectionInstance value)
        {
            if (value == null)
                throw new JavaScriptException(Engine, "Error", "A web part connection must be specified as the first argument");

            return m_webPartConnectionCollection.IndexOf(value.SPWebPartConnection);
        }

        [JSFunction(Name = "insert")]
        public void Insert(int index, SPWebPartConnectionInstance value)
        {
            if (value == null)
                throw new JavaScriptException(Engine, "Error", "A web part connection must be specified as the first argument");

            m_webPartConnectionCollection.Insert(index, value.SPWebPartConnection);
        }

        [JSFunction(Name = "remove")]
        public void Remove(SPWebPartConnectionInstance value)
        {
            if (value == null)
                throw new JavaScriptException(Engine, "Error", "A web part connection must be specified as the first argument");

            m_webPartConnectionCollection.Remove(value.SPWebPartConnection);
        }

        [JSFunction(Name = "removeAt")]
        public void RemoveAt(int index)
        {
            m_webPartConnectionCollection.RemoveAt(index);
        }

        [JSFunction(Name = "toArray")]
        [JSDoc("ternReturnType", "[+SPWebPartConnection]")]
        public ArrayInstance ToArray()
        {
            var result = Engine.Array.Construct();

            foreach (var conn in m_webPartConnectionCollection.OfType<SPWebPartConnection>())
            {
                ArrayInstance.Push(result, new SPWebPartConnectionInstance(Engine.Object.InstancePrototype, conn));
            }

            return result;
        }
    }
}
