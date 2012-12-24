namespace Barista.SharePoint.Search.Library
{
  using Jurassic;
  using Jurassic.Library;
  using Lucene.Net.Search;
  using System;

  [Serializable]
  public class PrefixQueryConstructor : ClrFunction
  {
    public PrefixQueryConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "PrefixQuery", new PrefixQueryInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public PrefixQueryInstance Construct()
    {
      return new PrefixQueryInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class PrefixQueryInstance : QueryInstance<PrefixQuery>
  {
    private readonly PrefixQuery m_prefixQuery;

    public PrefixQueryInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public PrefixQueryInstance(ObjectInstance prototype, PrefixQuery prefixQuery)
      : this(prototype)
    {
      if (prefixQuery == null)
        throw new ArgumentNullException("prefixQuery");

      m_prefixQuery = prefixQuery;
    }

    public override PrefixQuery Query
    {
      get { return m_prefixQuery; }
    }
  }
}
