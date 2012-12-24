namespace Barista.SharePoint.Search.Library
{
  using Barista.Extensions;
  using Jurassic;
  using Jurassic.Library;
  using Lucene.Net.Search;
  using System;
  using System.Reflection;

  [Serializable]
  public class BooleanQueryConstructor : ClrFunction
  {
    public BooleanQueryConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "BooleanQuery", new BooleanQueryInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public BooleanQueryInstance Construct()
    {
      return new BooleanQueryInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class BooleanQueryInstance : QueryInstance<BooleanQuery>
  {
    private readonly BooleanQuery m_booleanQuery;

    public BooleanQueryInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public BooleanQueryInstance(ObjectInstance prototype, BooleanQuery booleanQuery)
      : this(prototype)
    {
      if (booleanQuery == null)
        throw new ArgumentNullException("booleanQuery");

      m_booleanQuery = booleanQuery;
    }

    public override BooleanQuery Query
    {
      get { return m_booleanQuery; }
    }

    [JSFunction(Name = "add")]
    public void Add(object searchQuery, object occur)
    {
      Occur lOccur;
      if (occur == Null.Value || occur == Undefined.Value || occur == null || (occur is string) == false)
        lOccur = Occur.MUST;
      else
        lOccur = (Occur) Enum.Parse(typeof (Occur), occur as string);

      var searchQueryType = searchQuery.GetType();

      Query query;
      if (searchQueryType.IsSubclassOfRawGeneric(typeof(QueryInstance<>)))
      {
        var queryProperty = searchQueryType.GetProperty("Query", BindingFlags.Instance | BindingFlags.Public);
        query = queryProperty.GetValue(searchQuery, null) as Query;
      }
      else
        throw new JavaScriptException(this.Engine, "Error", "The first parameter must be a query object.");
      
      m_booleanQuery.Add(new BooleanClause(query, lOccur));
    }
  }
}
