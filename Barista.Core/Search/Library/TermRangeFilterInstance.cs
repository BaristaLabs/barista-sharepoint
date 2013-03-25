namespace Barista.Search.Library
{
  using Jurassic;
  using Jurassic.Library;
  using Lucene.Net.Search;
  using System;

  [Serializable]
  public class TermRangeFilterConstructor : ClrFunction
  {
    public TermRangeFilterConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "TermRangeFilter", new TermRangeFilterInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public TermRangeFilterInstance Construct()
    {
      return new TermRangeFilterInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class TermRangeFilterInstance : FilterInstance<TermRangeFilter>
  {
    private readonly TermRangeFilter m_termRangeFilter;

    public TermRangeFilterInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public TermRangeFilterInstance(ObjectInstance prototype, TermRangeFilter termRangeFilter)
      : this(prototype)
    {
      if (termRangeFilter == null)
        throw new ArgumentNullException("termRangeFilter");

      m_termRangeFilter = termRangeFilter;
    }

    public override TermRangeFilter Filter
    {
      get { return m_termRangeFilter; }
    }
  }
}
