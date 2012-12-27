namespace Barista.SharePoint.Search.Library
{
  using Jurassic;
  using Jurassic.Library;
  using Lucene.Net.Search;
  using System;

  [Serializable]
  public class PrefixFilterConstructor : ClrFunction
  {
    public PrefixFilterConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "PrefixFilter", new PrefixFilterInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public PrefixFilterInstance Construct()
    {
      return new PrefixFilterInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class PrefixFilterInstance : FilterInstance<PrefixFilter>
  {
    private readonly PrefixFilter m_prefixFilter;

    public PrefixFilterInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public PrefixFilterInstance(ObjectInstance prototype, PrefixFilter prefixFilter)
      : this(prototype)
    {
      if (prefixFilter == null)
        throw new ArgumentNullException("prefixFilter");

      m_prefixFilter = prefixFilter;
    }

    public override PrefixFilter Filter
    {
      get { return m_prefixFilter; }
    }
  }
}
