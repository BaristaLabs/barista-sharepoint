namespace Barista.Search.Library
{
  using Jurassic;
  using Jurassic.Library;
  using Lucene.Net.Index;
  using Lucene.Net.Search;
  using System;

  [Serializable]
  public class PhraseQueryConstructor : ClrFunction
  {
    public PhraseQueryConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "PhraseQuery", new PhraseQueryInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public PhraseQueryInstance Construct()
    {
      return new PhraseQueryInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class PhraseQueryInstance : QueryInstance<PhraseQuery>
  {
    private readonly PhraseQuery m_phraseQuery;

    public PhraseQueryInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public PhraseQueryInstance(ObjectInstance prototype, PhraseQuery phraseQuery)
      : this(prototype)
    {
      if (phraseQuery == null)
        throw new ArgumentNullException("phraseQuery");

      m_phraseQuery = phraseQuery;
    }

    public override PhraseQuery Query
    {
      get { return m_phraseQuery; }
    }

    [JSProperty(Name = "slop")]
    public int Slop
    {
      get { return m_phraseQuery.Slop; }
      set { m_phraseQuery.Slop = value; }
    }

    [JSFunction(Name = "add")]
    public void Add(string fieldName, string text)
    {
      m_phraseQuery.Add(new Term(fieldName, text));
    }
  }
}
