namespace Barista.Search.Library
{
  using Jurassic;
  using Jurassic.Library;
  using Lucene.Net.Search;
  using System;
  using System.Linq;

  [Serializable]
  public class ExplanationConstructor : ClrFunction
  {
    public ExplanationConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "Explanation", new ExplanationInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ExplanationInstance Construct()
    {
      return new ExplanationInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class ExplanationInstance : ObjectInstance
  {
    private readonly Explanation m_explanation;

    public ExplanationInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ExplanationInstance(ObjectInstance prototype, Explanation explination)
      : this(prototype)
    {
      if (explination == null)
        throw new ArgumentNullException("explination");

      m_explanation = explination;
    }

    public Explanation Explanation
    {
      get { return m_explanation; }
    }

    [JSProperty(Name = "description")]
    public string Description
    {
      get { return m_explanation.Description; }
    }

    [JSProperty(Name = "isMatch")]
    public bool IsMatch
    {
      get { return m_explanation.IsMatch; }
    }

    [JSProperty(Name = "value")]
    public double Value
    {
      get { return m_explanation.Value; }
    }

    [JSFunction(Name = "getDetails")]
    public ArrayInstance GetDetails()
    {
      var details = m_explanation.GetDetails()
                                 .Select(d => new ExplanationInstance(this.Engine.Object.InstancePrototype, d))
                                 .ToArray();
      return
        this.Engine.Array.Construct(details);
    }

    [JSFunction(Name = "toHtml")]
    public string ToHtml()
    {
      return m_explanation.ToHtml();
    }

    [JSFunction(Name = "ToString")]
    public string ToStringJS()
    {
      return this.ToString();
    }

    [JSFunction(Name = "toString")]
    public override string ToString()
    {
      return m_explanation.ToString();
    }
  }
}
