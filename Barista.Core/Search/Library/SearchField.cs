namespace Barista.Search.Library
{
    using Jurassic;
    using Jurassic.Library;
    using Lucene.Net.Documents;
    using System;

    [Serializable]
    public sealed class SearchFieldInstance : ObjectInstance
    {
        private readonly Field m_field;

        public SearchFieldInstance(ScriptEngine engine, Field field)
            : base(engine)
        {
            if (field == null)
                throw new ArgumentNullException("field");

            m_field = field;
            this.PopulateFunctions();
        }

        public Field Field
        {
            get { return m_field; }
        }

        [JSProperty(Name = "name")]
        public string Name
        {
            get { return m_field.Name; }
        }

        [JSProperty(Name = "value")]
        public string Value
        {
            get { return m_field.StringValue; }
        }
    }
}
