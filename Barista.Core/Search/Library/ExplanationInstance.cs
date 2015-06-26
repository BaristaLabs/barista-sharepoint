namespace Barista.Search.Library
{
  using Jurassic;
  using Jurassic.Library;
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
      return new ExplanationInstance(InstancePrototype);
    }
  }

  [Serializable]
  public class ExplanationInstance : ObjectInstance
  {
    private readonly Explanation m_explanation;

    public ExplanationInstance(ObjectInstance prototype)
      : base(prototype)
    {
      PopulateFields();
      PopulateFunctions();
    }

    public ExplanationInstance(ObjectInstance prototype, Explanation explanation)
      : this(prototype)
    {
      if (explanation == null)
        throw new ArgumentNullException("explanation");

      m_explanation = explanation;
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

    [JSProperty(Name = "details")]
    [JSDoc("ternPropertyType", "[+Explanation]")]
    public ArrayInstance Details
    {
      get
      {
        if (m_explanation.Details == null)
          return Engine.Array.Construct();

        var details = m_explanation.Details
                                   .Select(d => new ExplanationInstance(Engine.Object.InstancePrototype, d))
                                   .ToArray();
        return
          // ReSharper disable CoVariantArrayConversion
          Engine.Array.Construct(details);
        // ReSharper restore CoVariantArrayConversion
      }
    }

    [JSProperty(Name = "explanationHtml")]
    public string ExplanationHtml
    {
      get { return m_explanation.ExplanationHtml; }
    }
  }
}
