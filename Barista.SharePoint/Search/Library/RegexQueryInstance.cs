namespace Barista.SharePoint.Search.Library
{
  using Contrib.Regex;
  using Jurassic;
  using Jurassic.Library;
  using System;

  [Serializable]
  public class RegexQueryConstructor : ClrFunction
  {
    public RegexQueryConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "RegexQuery", new RegexQueryInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public RegexQueryInstance Construct()
    {
      return new RegexQueryInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class RegexQueryInstance : QueryInstance<RegexQuery>
  {
    private readonly RegexQuery m_regexQuery;

    public RegexQueryInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public RegexQueryInstance(ObjectInstance prototype, RegexQuery regexQuery)
      : this(prototype)
    {
      if (regexQuery == null)
        throw new ArgumentNullException("regexQuery");

      m_regexQuery = regexQuery;
    }

    public override RegexQuery Query
    {
      get { return m_regexQuery; }
    }
  }
}
