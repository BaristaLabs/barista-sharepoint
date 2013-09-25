namespace Barista.Automation.Selenium
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using OpenQA.Selenium;
  using System;

  [Serializable]
  public class NavigationConstructor : ClrFunction
  {
    public NavigationConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "Navigation", new NavigationInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public NavigationInstance Construct()
    {
      return new NavigationInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class NavigationInstance : ObjectInstance
  {
    private readonly INavigation m_navigation;

    public NavigationInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public NavigationInstance(ObjectInstance prototype, INavigation navigation)
      : this(prototype)
    {
      if (navigation == null)
        throw new ArgumentNullException("navigation");

      m_navigation = navigation;
    }

    public INavigation Navigation
    {
      get { return m_navigation; }
    }

    [JSFunction(Name = "back")]
    public void Back()
    {
      m_navigation.Back();
    }

    [JSFunction(Name = "forward")]
    public void Forward()
    {
      m_navigation.Forward();
    }

    [JSFunction(Name = "goToUrl")]
    public void GoToUrl(object url)
    {
      if (url == null || url == Undefined.Value || url == Null.Value)
        m_navigation.GoToUrl(String.Empty);
      else if (url is UriInstance)
        m_navigation.GoToUrl((url as UriInstance).Uri);
      else
        m_navigation.GoToUrl(TypeConverter.ToString(url));
    }

    [JSFunction(Name = "refresh")]
    public void Refresh()
    {
      m_navigation.Refresh();
    }
  }
}
