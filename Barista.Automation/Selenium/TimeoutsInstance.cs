namespace Barista.Automation.Selenium
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using OpenQA.Selenium;
  using System;

  [Serializable]
  public class TimeoutsConstructor : ClrFunction
  {
    public TimeoutsConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "Timeouts", new TimeoutsInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public TimeoutsInstance Construct()
    {
      return new TimeoutsInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class TimeoutsInstance : ObjectInstance
  {
    private readonly ITimeouts m_timeouts;

    public TimeoutsInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public TimeoutsInstance(ObjectInstance prototype, ITimeouts timeouts)
      : this(prototype)
    {
      if (timeouts == null)
        throw new ArgumentNullException("timeouts");

      m_timeouts = timeouts;
    }

    public ITimeouts Timeouts
    {
      get { return m_timeouts; }
    }

    [JSFunction(Name = "implicitlyWait")]
    public TimeoutsInstance ImplicitlyWait(int timeToWait)
    {
      return new TimeoutsInstance(this.Engine.Object.InstancePrototype, m_timeouts.ImplicitlyWait(TimeSpan.FromMilliseconds(timeToWait)));
    }

    [JSFunction(Name = "setPageLoadTimeout")]
    public TimeoutsInstance SetPageLoadTimeout(int timeToWait)
    {
      return new TimeoutsInstance(this.Engine.Object.InstancePrototype, m_timeouts.SetPageLoadTimeout(TimeSpan.FromMilliseconds(timeToWait)));
    }

    [JSFunction(Name = "setScriptTimeout")]
    public TimeoutsInstance SetScriptTimeout(int timeToWait)
    {
      return new TimeoutsInstance(this.Engine.Object.InstancePrototype, m_timeouts.SetScriptTimeout(TimeSpan.FromMilliseconds(timeToWait)));
    }
  }
}
