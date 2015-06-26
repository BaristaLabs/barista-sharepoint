namespace Barista.Library
{
    using System.Collections;
    using System.Linq;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;

    [Serializable]
    public class HashtableConstructor : ClrFunction
    {
        public HashtableConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "Hashtable", new HashtableInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public HashtableInstance Construct()
        {
            return new HashtableInstance(InstancePrototype, new Hashtable());
        }
    }

    [Serializable]
    public class HashtableInstance : ObjectInstance
    {
        private readonly Hashtable m_hashtable;

        public HashtableInstance(ObjectInstance prototype)
            : base(prototype)
        {
            PopulateFields();
            PopulateFunctions();
        }

        public HashtableInstance(ObjectInstance prototype, Hashtable hashtable)
            : this(prototype)
        {
            if (hashtable == null)
                throw new ArgumentNullException("hashtable");

            m_hashtable = hashtable;
        }

        public Hashtable Hashtable
        {
            get { return m_hashtable; }
        }

        [JSProperty(Name="count")]
        public int Count
        {
            get
            {
                return m_hashtable.Count;
            }
        }

        [JSProperty(Name = "isFixedSize")]
        public bool IsFixedSize
        {
            get
            {
                return m_hashtable.IsFixedSize;
            }
        }

        [JSProperty(Name = "isReadOnly")]
        public bool IsReadOnly
        {
            get
            {
                return m_hashtable.IsReadOnly;
            }
        }

        [JSProperty(Name = "isSynchronized")]
        public bool IsSynchronized
        {
            get
            {
                return m_hashtable.IsSynchronized;
            }
        }

        [JSFunction(Name = "add")]
        public void Add(object key, object value)
        {
            m_hashtable.Add(key, value);
        }

        [JSFunction(Name = "clear")]
        public void Clear()
        {
            m_hashtable.Clear();
        }

        [JSFunction(Name = "clone")]
        public HashtableInstance Clone()
        {
            var result = m_hashtable.Clone() as Hashtable;
            if (result == null)
                return null;

            return new HashtableInstance(Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "contains")]
        public bool Contains(object key)
        {
            return m_hashtable.Contains(key);
        }

        [JSFunction(Name = "containsKey")]
        public bool ContainsKey(object key)
        {
            return m_hashtable.ContainsKey(key);
        }

        [JSFunction(Name = "containsValue")]
        public bool ContainsVallue(object value)
        {
            return m_hashtable.ContainsValue(value);
        }

        [JSFunction(Name = "getKeys")]
        [JSDoc("ternReturnType", "[string]")]
        public ArrayInstance GetKeys()
        {
// ReSharper disable once CoVariantArrayConversion
            return Engine.Array.Construct(m_hashtable.Keys.OfType<object>().Select(o => TypeConverter.ToObject(Engine, o)).ToArray());
        }

        [JSFunction(Name = "getValueByKey")]
        public ObjectInstance GetValueByKey(object key)
        {
            return TypeConverter.ToObject(Engine, m_hashtable[key]);
        }

        [JSFunction(Name = "getValues")]
        public ArrayInstance GetValues()
        {
// ReSharper disable once CoVariantArrayConversion
            return Engine.Array.Construct(m_hashtable.Values.OfType<object>().Select(o => TypeConverter.ToObject(Engine, o)).ToArray());
        }

        [JSFunction(Name = "setValueByKey")]
        public void SetValueByKey(object key, object value)
        {
            m_hashtable[key] = value;
        }

        [JSFunction(Name = "remove")]
        public void Remove(object key)
        {
            m_hashtable.Remove(key);
        }

        [JSFunction(Name = "toObject")]
        public ObjectInstance ToObject()
        {
            var result = Engine.Object.Construct();
            foreach (var key in m_hashtable.Keys)
            {
                result.SetPropertyValue(key.ToString(), TypeConverter.ToObject(Engine, m_hashtable[key]), false);
            }

            return result;
        }
    }
}
