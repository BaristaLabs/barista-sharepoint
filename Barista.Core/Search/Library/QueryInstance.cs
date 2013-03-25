namespace Barista.Search.Library
{
  using Jurassic.Library;
  using Lucene.Net.Search;
  using System;

  [Serializable]
  public abstract class QueryInstance<T> : ObjectInstance
    where T : Query
  {
    protected QueryInstance(ObjectInstance prototype)
      : base(prototype)
    {
    }

    public abstract T Query
    {
      get;
    }

    [JSFunction(Name = "ToString")]
    public string ToStringJS()
    {
      return this.ToString();
    }

    [JSFunction(Name = "toString")]
    public override string ToString()
    {
      return this.Query.ToString();
    }
  }

  [Serializable]
  public class GenericQueryInstance : QueryInstance<Query> 
  {
    private readonly Query m_query;

    public GenericQueryInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public GenericQueryInstance(ObjectInstance prototype, Query query)
      : this(prototype)
    {
      if (query == null)
        throw new ArgumentNullException("query");

      m_query = query;
    }

    public override Query Query
    {
      get { return m_query; }
    }
  }

}