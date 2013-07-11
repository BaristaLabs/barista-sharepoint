namespace Barista.SharePoint.SPSearch
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Microsoft.Office.Server.Search.Query;
  using System;
  using System.Linq;

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

    [JSProperty(Name="queryText")]
    public string QueryText
    {
      get { return m_query.QueryText; }
      set { m_query.QueryText = value; }
    }

    [JSFunction(Name="execute")]
    public ArrayInstance Execute()
    {
      var results = m_query.Execute();
      foreach (var result in results.OfType<ResultTable>())
      {
        var resultObj = this.Engine.Object.Construct();
        
      }
      throw new NotImplementedException();
    }
  }
}
