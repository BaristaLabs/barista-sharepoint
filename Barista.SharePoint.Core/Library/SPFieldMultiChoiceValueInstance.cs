namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Microsoft.SharePoint;
    using System;

    [Serializable]
    public class SPFieldMultiChoiceValueConstructor : ClrFunction
    {
        public SPFieldMultiChoiceValueConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPFieldMultiChoiceValue", new SPFieldMultiChoiceValueInstance(engine.Object.InstancePrototype))
        {
            this.PopulateFunctions();
        }

        [JSConstructorFunction]
        public SPFieldMultiChoiceValueInstance Construct(object fieldValue)
        {
            if (fieldValue == Undefined.Value)
                return new SPFieldMultiChoiceValueInstance(this.InstancePrototype, new SPFieldMultiChoiceValue());

            return new SPFieldMultiChoiceValueInstance(this.InstancePrototype, new SPFieldMultiChoiceValue(TypeConverter.ToString(fieldValue)));
        }

        [JSProperty(Name = "delimiter")]
        public string Delimiter
        {
            get
            {
                return SPFieldMultiChoiceValue.Delimiter;
            }
        }

    }

    [Serializable]
    public class SPFieldMultiChoiceValueInstance : ObjectInstance
    {
        private readonly SPFieldMultiChoiceValue m_fieldMultiChoiceValue;

        public SPFieldMultiChoiceValueInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFunctions();
        }

        public SPFieldMultiChoiceValueInstance(ObjectInstance prototype, SPFieldMultiChoiceValue fieldMultiChoiceValue)
            : this(prototype)
        {
            if (fieldMultiChoiceValue == null)
                throw new ArgumentNullException("fieldMultiChoiceValue");

            m_fieldMultiChoiceValue = fieldMultiChoiceValue;
        }

        public SPFieldMultiChoiceValue SPFieldMultiChoiceValue
        {
            get
            {
                return m_fieldMultiChoiceValue;
            }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return m_fieldMultiChoiceValue.Count;
            }
        }

        [JSFunction(Name = "add")]
        public void Add(string choiceValue)
        {
            m_fieldMultiChoiceValue.Add(choiceValue);
        }

        [JSFunction(Name = "getValueByIndex")]
        public string GetValueByIndex(int index)
        {
            return m_fieldMultiChoiceValue[index];
        }

        [JSFunction(Name = "toString")]
        public override string ToString()
        {
            return m_fieldMultiChoiceValue.ToString();
        }
    }
}
