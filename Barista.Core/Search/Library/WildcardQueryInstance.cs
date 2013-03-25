namespace Barista.Search.Library
{
  using Jurassic;
  using Jurassic.Library;
  using Lucene.Net.Search;
  using System;

  [Serializable]
  public class WildcardQueryConstructor : ClrFunction
  {
    public WildcardQueryConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "WildcardQuery", new WildcardQueryInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public WildcardQueryInstance Construct()
    {
      return new WildcardQueryInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class WildcardQueryInstance : QueryInstance<WildcardQuery>
  {
    private readonly WildcardQuery m_wildcardQuery;

    public WildcardQueryInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public WildcardQueryInstance(ObjectInstance prototype, WildcardQuery wildcardQuery)
      : this(prototype)
    {
      if (wildcardQuery == null)
        throw new ArgumentNullException("wildcardQuery");

      m_wildcardQuery = wildcardQuery;
    }

    public override WildcardQuery Query
    {
      get { return m_wildcardQuery; }
    }
  }
}
