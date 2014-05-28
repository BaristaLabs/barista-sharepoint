namespace Barista.Library
{
    using System.Collections.Specialized;
    using System.Linq;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;

    [Serializable]
    public class StringDictionaryConstructor : ClrFunction
    {
        public StringDictionaryConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "StringDictionary", new StringDictionaryInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public StringDictionaryInstance Construct()
        {
            return new StringDictionaryInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class StringDictionaryInstance : ObjectInstance
    {
        private readonly StringDictionary m_stringDictionary;

        public StringDictionaryInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public StringDictionaryInstance(ObjectInstance prototype, StringDictionary stringDictionary)
            : this(prototype)
        {
            if (stringDictionary == null)
                throw new ArgumentNullException("stringDictionary");

            m_stringDictionary = stringDictionary;
        }

        public StringDictionary StringDictionary
        {
            get { return m_stringDictionary; }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get { return m_stringDictionary.Count; }
        }

        [JSProperty(Name = "isSynchronized")]
        public bool IsSynchronized
        {
            get { return m_stringDictionary.IsSynchronized; }
        }

        [JSFunction(Name = "add")]
        public void Add(string key, string value)
        {
            m_stringDictionary.Add(key, value);
        }

        [JSFunction(Name = "clear")]
        public void Clear()
        {
            m_stringDictionary.Clear();
        }

        [JSFunction(Name = "containsKey")]
        public bool ContainsKey(string key)
        {
            return m_stringDictionary.ContainsKey(key);
        }

        [JSFunction(Name = "containsValue")]
        public bool ContainsValue(string value)
        {
            return m_stringDictionary.ContainsValue(value);
        }

        [JSFunction(Name = "getKeys")]
        public ArrayInstance GetKeys()
        {
// ReSharper disable once CoVariantArrayConversion
            return this.Engine.Array.Construct(m_stringDictionary.Keys.OfType<string>().ToArray());
        }

        [JSFunction(Name = "getValueByKey")]
        public ObjectInstance GetValueByKey(string key)
        {
            return TypeConverter.ToObject(this.Engine, m_stringDictionary[key]);
        }

        [JSFunction(Name = "getValues")]
        public ArrayInstance GetValues()
        {
// ReSharper disable once CoVariantArrayConversion
            return this.Engine.Array.Construct(m_stringDictionary.Values.OfType<string>().ToArray());
        }

        [JSFunction(Name = "setValueByKey")]
        public void SetValueByKey(string key, string value)
        {
            m_stringDictionary[key] = value;
        }

        [JSFunction(Name = "remove")]
        public void Remove(string value)
        {
            m_stringDictionary.Remove(value);
        }

        [JSFunction(Name = "toObject")]
        public ObjectInstance ToArray(ArrayInstance array, int index)
        {
            var result = this.Engine.Object.Construct();

            foreach(string key in m_stringDictionary.Keys)
                result.SetPropertyValue(key, m_stringDictionary[key], false);

            return result;
        }

    }
}
