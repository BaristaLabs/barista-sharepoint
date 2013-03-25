namespace Barista.Search.Library
{
  using Jurassic;
  using Jurassic.Library;
  using Lucene.Net.Search;
  using System;

  [Serializable]
  public class TermRangeQueryConstructor : ClrFunction
  {
    public TermRangeQueryConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "TermRangeQuery", new TermRangeQueryInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public TermRangeQueryInstance Construct()
    {
      return new TermRangeQueryInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class TermRangeQueryInstance : QueryInstance<TermRangeQuery>
  {
    private readonly TermRangeQuery m_termRangeQuery;

    public TermRangeQueryInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public TermRangeQueryInstance(ObjectInstance prototype, TermRangeQuery termRangeQuery)
      : this(prototype)
    {
      if (termRangeQuery == null)
        throw new ArgumentNullException("termRangeQuery");

      m_termRangeQuery = termRangeQuery;
    }

    public override TermRangeQuery Query
    {
      get { return m_termRangeQuery; }
    }
  }
}
