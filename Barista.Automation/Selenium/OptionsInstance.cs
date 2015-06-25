namespace Barista.Automation.Selenium
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using OpenQA.Selenium;
  using System;

  [Serializable]
  public class OptionsConstructor : ClrFunction
  {
    public OptionsConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "Options", new OptionsInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public OptionsInstance Construct()
    {
      return new OptionsInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class OptionsInstance : ObjectInstance
  {
    private readonly IOptions m_options;

    public OptionsInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public OptionsInstance(ObjectInstance prototype, IOptions options)
      : this(prototype)
    {
      if (options == null)
        throw new ArgumentNullException("options");

      m_options = options;
    }

    public IOptions Options
    {
      get { return m_options; }
    }

    [JSProperty(Name = "cookies")]
    public CookieJarInstance Cookies
    {
      get { return new CookieJarInstance(this.Engine.Object.InstancePrototype, m_options.Cookies); }
    }

    [JSProperty(Name = "window")]
    public WindowInstance Window
    {
      get { return new WindowInstance(this.Engine.Object.InstancePrototype, m_options.Window); }
    }

    [JSFunction(Name = "timeouts")]
    public TimeoutsInstance Timeouts()
    {
      return new TimeoutsInstance(this.Engine.Object.InstancePrototype, m_options.Timeouts());
    }
  }
}
