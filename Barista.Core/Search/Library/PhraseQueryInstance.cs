namespace Barista.Search.Library
{
  using Jurassic;
  using Jurassic.Library;
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
    public object Slop
    {
      get
      {
        if (m_phraseQuery.Slop.HasValue == false)
          return Null.Value;

        return m_phraseQuery.Slop.Value;
      }
      set
      {
        if (value == Null.Value || value == Undefined.Value)
          m_phraseQuery.Slop = null;

        m_phraseQuery.Slop = TypeConverter.ToInteger(value);
      }
    }

    [JSFunction(Name = "add")]
    public void Add(string fieldName, string text)
    {
      m_phraseQuery.Terms.Add(new Term { FieldName = fieldName, Value = text });
    }
  }
}
