namespace Barista.Search.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;

    [Serializable]
    public class MultiFieldQueryParserQueryConstructor : ClrFunction
    {
        public MultiFieldQueryParserQueryConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "MultiFieldQueryParserQuery", new MultiFieldQueryParserQueryInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public MultiFieldQueryParserQueryInstance Construct()
        {
            return new MultiFieldQueryParserQueryInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class MultiFieldQueryParserQueryInstance : ObjectInstance, IQuery<MultiFieldQueryParserQuery>
    {
        private readonly MultiFieldQueryParserQuery m_multiFieldQueryParserQuery;

        public MultiFieldQueryParserQueryInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public MultiFieldQueryParserQueryInstance(ObjectInstance prototype, MultiFieldQueryParserQuery multiFieldQueryParserQuery)
            : this(prototype)
        {
            if (multiFieldQueryParserQuery == null)
                throw new ArgumentNullException("multiFieldQueryParserQuery");

            m_multiFieldQueryParserQuery = multiFieldQueryParserQuery;
        }

        public MultiFieldQueryParserQuery Query
        {
            get { return m_multiFieldQueryParserQuery; }
        }
    }
}
