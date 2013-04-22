namespace Barista.Search.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;

  [Serializable]
  public class QueryParserQueryConstructor : ClrFunction
  {
    public QueryParserQueryConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "QueryParserQuery", new QueryParserQueryInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public QueryParserQueryInstance Construct()
    {
      return new QueryParserQueryInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class QueryParserQueryInstance : ObjectInstance
  {
    private readonly QueryParserQuery m_queryParserQuery;

    public QueryParserQueryInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public QueryParserQueryInstance(ObjectInstance prototype, QueryParserQuery queryParserQuery)
      : this(prototype)
    {
      if (queryParserQuery == null)
        throw new ArgumentNullException("queryParserQuery");

      m_queryParserQuery = queryParserQuery;
    }

    public QueryParserQuery QueryParserQuery
    {
      get { return m_queryParserQuery; }
    }
  }
}
