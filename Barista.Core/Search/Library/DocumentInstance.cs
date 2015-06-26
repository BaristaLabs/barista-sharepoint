namespace Barista.Search.Library
{
    using Jurassic;
    using Jurassic.Library;
    using Lucene.Net.Documents;
    using System;
    using System.Linq;

    [Serializable]
    public sealed class DocumentInstance : ObjectInstance
    {
        private readonly Document m_document;

        public DocumentInstance(ScriptEngine engine, Document document)
            : base(engine)
        {
            if (document == null)
                throw new ArgumentNullException("document");

            m_document = document;

            PopulateFunctions();
        }

        public Document Document
        {
            get { return m_document; }
        }

        [JSProperty(Name = "fields")]
        [JSDoc("ternPropertyType", "[+SearchField]")]
        public ArrayInstance Fields
        {
            get
            {
                var fields = m_document.GetFields()
                    .OfType<Field>()
                    .Select(f => new SearchFieldInstance(Engine, f))
                    .ToArray();

// ReSharper disable once CoVariantArrayConversion
                var result = Engine.Array.Construct(fields);
                return result;
            }
        }
    }
}
