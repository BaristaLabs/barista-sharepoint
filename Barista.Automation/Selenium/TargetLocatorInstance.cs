namespace Barista.Automation.Selenium
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using OpenQA.Selenium;
  using System;

  [Serializable]
  public class TargetLocatorConstructor : ClrFunction
  {
    public TargetLocatorConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "TargetLocator", new TargetLocatorInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public TargetLocatorInstance Construct()
    {
      return new TargetLocatorInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class TargetLocatorInstance : ObjectInstance
  {
    private readonly ITargetLocator m_targetLocator;

    public TargetLocatorInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public TargetLocatorInstance(ObjectInstance prototype, ITargetLocator targetLocator)
      : this(prototype)
    {
      if (targetLocator == null)
        throw new ArgumentNullException("targetLocator");

      m_targetLocator = targetLocator;
    }

    public ITargetLocator TargetLocator
    {
      get { return m_targetLocator; }
    }

    [JSFunction(Name = "activeElement")]
    public WebElementInstance ActiveElement()
    {
      return new WebElementInstance(this.Engine.Object.InstancePrototype, m_targetLocator.ActiveElement());
    }

    [JSFunction(Name = "alert")]
    public AlertInstance Alert()
    {
      return new AlertInstance(this.Engine.Object.InstancePrototype, m_targetLocator.Alert());
    }

    [JSFunction(Name = "defaultContent")]
    public WebDriverInstance DefaultContent()
    {
      return new WebDriverInstance(this.Engine.Object.InstancePrototype, m_targetLocator.DefaultContent());
    }

    [JSFunction(Name = "frame")]
    public WebDriverInstance Frame(object frameArg)
    {
      if (frameArg == null || frameArg == Null.Value || frameArg == Undefined.Value)
        return new WebDriverInstance(this.Engine.Object.InstancePrototype, m_targetLocator.Frame(String.Empty));

      if (frameArg is WebElementInstance)
      {
        var webElement = (frameArg as WebElementInstance).WebElement;
        return new WebDriverInstance(this.Engine.Object.InstancePrototype, m_targetLocator.Frame(webElement));
      }
      
      if (TypeUtilities.IsNumeric(frameArg))
      {
        var value = TypeConverter.ToInteger(frameArg);
        return new WebDriverInstance(this.Engine.Object.InstancePrototype, m_targetLocator.Frame(value));
      }

      var strValue = TypeConverter.ToString(frameArg);
      return new WebDriverInstance(this.Engine.Object.InstancePrototype, m_targetLocator.Frame(strValue));
    }

    [JSFunction(Name = "window")]
    public WebDriverInstance Window(string windowName)
    {
      return new WebDriverInstance(this.Engine.Object.InstancePrototype, m_targetLocator.Window(windowName));
    }
  }
}
