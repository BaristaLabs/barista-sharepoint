namespace Barista.SharePoint.Search.Library
{
  using Jurassic;
  using Jurassic.Library;
  using Lucene.Net.Search;
  using System;

  [Serializable]
  public class QueryConstructor : ClrFunction
  {
    public QueryConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "Query", new QueryInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public QueryInstance Construct()
    {
      return new QueryInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class QueryInstance : ObjectInstance
  {
    private readonly Query m_query;

    public QueryInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public QueryInstance(ObjectInstance prototype, Query query)
      : this(prototype)
    {
      if (query == null)
        throw new ArgumentNullException("query");

      m_query = query;
    }

    public Query Query
    {
      get { return m_query; }
    }
  }
}
