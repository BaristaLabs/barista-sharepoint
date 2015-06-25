namespace Barista.Search.Library
{
  using Barista.Jurassic;
  using Jurassic.Library;
  using System;

  [Serializable]
  public abstract class QueryInstance<T> : ObjectInstance
    where T : Query
  {
    protected QueryInstance(ObjectInstance prototype)
      : base(prototype)
    {
    }

    [JSProperty(Name = "boost")]
    public object Boost
    {
      get
      {
        if (Query.Boost.HasValue == false)
          return Null.Value;

        return (double)Query.Boost.Value;
      }
      set
      {
        if (value == Null.Value || value == Undefined.Value)
        {
          Query.Boost = null;
          return;
        }
        Query.Boost = (float)TypeConverter.ToNumber(value);
      }
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