namespace Barista.SharePoint.Library
{
    using System.Linq;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint.Utilities;

    [Serializable]
    public class SPPropertyBagConstructor : ClrFunction
    {
        public SPPropertyBagConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPPropertyBag", new SPPropertyBagInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPPropertyBagInstance Construct()
        {
            return new SPPropertyBagInstance(InstancePrototype);
        }
    }

    [Serializable]
    public class SPPropertyBagInstance : ObjectInstance
    {
        private readonly SPPropertyBag m_propertyBag;

        public SPPropertyBagInstance(ObjectInstance prototype)
            : base(prototype)
        {
            PopulateFields();
            PopulateFunctions();
        }

        public SPPropertyBagInstance(ObjectInstance prototype, SPPropertyBag propertyBag)
            : this(prototype)
        {
            if (propertyBag == null)
                throw new ArgumentNullException("propertyBag");

            m_propertyBag = propertyBag;
        }

        public SPPropertyBag SPPropertyBag
        {
            get { return m_propertyBag; }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return m_propertyBag.Count;
            }
        }

        [JSProperty(Name = "isSynchronized")]
        public bool IsSynchronized
        {
            get
            {
                return m_propertyBag.IsSynchronized;
            }
        }

        [JSFunction(Name = "add")]
        public void Add(string key, string value)
        {
            m_propertyBag.Add(key, value);
        }

        [JSFunction(Name = "containsKey")]
        public bool ContainsKey(string key)
        {
            return m_propertyBag.ContainsKey(key);
        }

        [JSFunction(Name = "containsValue")]
        public bool ContainsValue(string value)
        {
            return m_propertyBag.ContainsValue(value);
        }

        [JSFunction(Name = "getKeys")]
        [JSDoc("ternReturnType", "[string]")]
        public ArrayInstance GetKeys()
        {
            var result = Engine.Array.Construct();
            foreach (var key in m_propertyBag.Keys.OfType<string>())
                ArrayInstance.Push(result, key);

            return result;
        }

        [JSFunction(Name = "getValueByKey")]
        public string GetValueByKey(string key)
        {
            return m_propertyBag[key];
        }

        [JSFunction(Name = "getValues")]
        [JSDoc("ternReturnType", "[string]")]
        public ArrayInstance GetValues()
        {
            var result = Engine.Array.Construct();
            foreach (var value in m_propertyBag.Values.OfType<string>())
                ArrayInstance.Push(result, value);

            return result;
        }

        [JSFunction(Name = "setValueByKey")]
        public void SetValueByKey(string key, string value)
        {
            m_propertyBag[key] = value;
        }

        [JSFunction(Name = "remove")]
        public void Remove(string key)
        {
            m_propertyBag.Remove(key);
        }
        
        [JSFunction(Name = "update")]
        public void Update()
        {
            m_propertyBag.Update();
        }

        [JSFunction(Name = "toObject")]
        public ObjectInstance ToObject()
        {
            var result = Engine.Object.Construct();
            foreach(var key in m_propertyBag.Keys.OfType<string>())
                result.SetPropertyValue(key, m_propertyBag[key], false);

            return result;
        }
    }
}
