namespace Barista.Search.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;

  [Serializable]
  public class TermsFilterConstructor : ClrFunction
  {
    public TermsFilterConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "TermsFilter", new TermsFilterInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public TermsFilterInstance Construct()
    {
      return new TermsFilterInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class TermsFilterInstance : FilterInstance<TermsFilter>
  {
    private readonly TermsFilter m_termsFilter;

    public TermsFilterInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public TermsFilterInstance(ObjectInstance prototype, TermsFilter termsFilter)
      : this(prototype)
    {
      if (termsFilter == null)
        throw new ArgumentNullException("termsFilter");

      m_termsFilter = termsFilter;
    }

    public override TermsFilter Filter
    {
      get { return m_termsFilter; }
    }

    [JSFunction(Name = "addTerm")]
    public TermsFilterInstance AddTerm(string fieldName, string text)
    {
      var term = new Term {FieldName = fieldName, Value = text};
      m_termsFilter.Terms.Add(term);

      return this;
    }
  }
}
