namespace Barista.Search.Library
{
  using Jurassic;
  using Jurassic.Library;
  using System;

  [Serializable]
  public class QueryWrapperFilterConstructor : ClrFunction
  {
    public QueryWrapperFilterConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "QueryWrapperFilter", new QueryWrapperFilterInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public QueryWrapperFilterInstance Construct()
    {
      return new QueryWrapperFilterInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class QueryWrapperFilterInstance : FilterInstance<QueryWrapperFilter>
  {
    private readonly QueryWrapperFilter m_queryWrapperFilter;

    public QueryWrapperFilterInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public QueryWrapperFilterInstance(ObjectInstance prototype, QueryWrapperFilter queryWrapperFilter)
      : this(prototype)
    {
      if (queryWrapperFilter == null)
        throw new ArgumentNullException("queryWrapperFilter");

      m_queryWrapperFilter = queryWrapperFilter;
    }

    public override QueryWrapperFilter Filter
    {
      get { return m_queryWrapperFilter; }
    }
  }
}
