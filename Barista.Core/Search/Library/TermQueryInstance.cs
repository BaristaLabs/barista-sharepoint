namespace Barista.Search.Library
{
  using Jurassic;
  using Jurassic.Library;
  using Lucene.Net.Search;
  using System;

  [Serializable]
  public class TermQueryConstructor : ClrFunction
  {
    public TermQueryConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "TermQuery", new TermQueryInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public TermQueryInstance Construct()
    {
      return new TermQueryInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class TermQueryInstance : QueryInstance<TermQuery>
  {
    private readonly TermQuery m_termQuery;

    public TermQueryInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public TermQueryInstance(ObjectInstance prototype, TermQuery termQuery)
      : this(prototype)
    {
      if (termQuery == null)
        throw new ArgumentNullException("termQuery");

      m_termQuery = termQuery;
    }

    public override TermQuery Query
    {
      get { return m_termQuery; }
    }
  }
}
