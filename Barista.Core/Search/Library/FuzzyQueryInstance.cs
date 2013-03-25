namespace Barista.Search.Library
{
  using Jurassic;
  using Jurassic.Library;
  using Lucene.Net.Search;
  using System;

  [Serializable]
  public class FuzzyQueryConstructor : ClrFunction
  {
    public FuzzyQueryConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "FuzzyQuery", new FuzzyQueryInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public FuzzyQueryInstance Construct()
    {
      return new FuzzyQueryInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class FuzzyQueryInstance : QueryInstance<FuzzyQuery>
  {
    private readonly FuzzyQuery m_fuzzyQuery;

    public FuzzyQueryInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public FuzzyQueryInstance(ObjectInstance prototype, FuzzyQuery fuzzyQuery)
      : this(prototype)
    {
      if (fuzzyQuery == null)
        throw new ArgumentNullException("fuzzyQuery");

      m_fuzzyQuery = fuzzyQuery;
    }

    public override FuzzyQuery Query
    {
      get { return m_fuzzyQuery; }
    }
  }
}
