namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint;

    [Serializable]
    public class SPFieldMultiColumnValueConstructor : ClrFunction
    {
        public SPFieldMultiColumnValueConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPFieldMultiColumnValue", new SPFieldMultiColumnValueInstance(engine.Object.InstancePrototype))
        {
            this.PopulateFunctions();
        }

        [JSConstructorFunction]
        public SPFieldMultiColumnValueInstance Construct(object fieldValue)
        {
            if (fieldValue == Undefined.Value)
                return new SPFieldMultiColumnValueInstance(this.InstancePrototype, new SPFieldMultiColumnValue());

            return new SPFieldMultiColumnValueInstance(this.Engine.Object.InstancePrototype, new SPFieldMultiColumnValue(TypeConverter.ToString(fieldValue)));
        }

        [JSProperty(Name = "delimiter")]
        public string Delimiter
        {
            get
            {
                return SPFieldMultiColumnValue.Delimiter;
            }
        }

        [JSFunction(Name = "createWithNumberOfSubColumns")]
        public SPFieldMultiColumnValueInstance CreateWithNumberOfSubColumns(int numberOfSubColumns)
        {
            return new SPFieldMultiColumnValueInstance(this.Engine.Object.InstancePrototype, new SPFieldMultiColumnValue(numberOfSubColumns));
        }
    }

    [Serializable]
    public class SPFieldMultiColumnValueInstance : ObjectInstance
    {
        private readonly SPFieldMultiColumnValue m_fieldMultiColumnValue;

        public SPFieldMultiColumnValueInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFunctions();
        }

        public SPFieldMultiColumnValueInstance(ObjectInstance prototype, SPFieldMultiColumnValue fieldMultiColumnValue)
            : this(prototype)
        {
            if (fieldMultiColumnValue == null)
                throw new ArgumentNullException("fieldMultiColumnValue");

            m_fieldMultiColumnValue = fieldMultiColumnValue;
        }

        public SPFieldMultiColumnValue SPFieldMultiColumnValue
        {
            get
            {
                return m_fieldMultiColumnValue;
            }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return m_fieldMultiColumnValue.Count;
            }
        }

        [JSFunction(Name = "add")]
        public void Add(string subColumnValue)
        {
            m_fieldMultiColumnValue.Add(subColumnValue);
        }

        [JSFunction(Name = "getValueByIndex")]
        public string GetValueByIndex(int index)
        {
            return m_fieldMultiColumnValue[index];
        }

        [JSFunction(Name = "toString")]
        public override string ToString()
        {
            return m_fieldMultiColumnValue.ToString();
        }
    }
}
