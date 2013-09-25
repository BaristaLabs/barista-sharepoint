namespace Barista.Automation.Selenium
{
  using System.Linq;
  using System.Reflection;
  using Barista.Jurassic;
  using Jurassic.Library;
  using System;
  using OpenQA.Selenium;

  [Serializable]
  public class WebDriverInstance : ObjectInstance
  {
    public WebDriverInstance(ObjectInstance prototype, IWebDriver webDriver)
      : base(prototype)
    {
      if (webDriver == null)
        throw new ArgumentNullException("webDriver");

      this.WebDriver = webDriver;
// ReSharper disable DoNotCallOverridableMethodsInConstructor
      Initialize();
// ReSharper restore DoNotCallOverridableMethodsInConstructor
    }

    protected virtual void Initialize()
    {
      this.PopulateFields(this.GetType());
      this.PopulateFunctions(this.GetType(), BindingFlags.Instance | BindingFlags.Public);
    }

    public IWebDriver WebDriver
    {
      get;
      set;
    }

    #region IWebDriver
    [JSProperty(Name = "currentWindowHandle")]
    public string CurrentWindowHandle
    {
      get { return WebDriver.CurrentWindowHandle; }
    }

    [JSProperty(Name = "pageSource")]
    public string PageSource
    {
      get { return WebDriver.PageSource; }
    }

    [JSProperty(Name = "title")]
    public string Title
    {
      get { return WebDriver.Title; }
    }

    [JSProperty(Name = "url")]
    public string Url
    {
      get { return WebDriver.Url; }
      set { WebDriver.Url = value; }
    }

    [JSProperty(Name = "windowHandles")]
    public ArrayInstance WindowHandles
    {
      get
      {
        var handles = WebDriver.WindowHandles;
// ReSharper disable CoVariantArrayConversion
        return this.Engine.Array.Construct(handles.ToArray());
// ReSharper restore CoVariantArrayConversion
      }
    }

    [JSFunction(Name = "close")]
    public void Close()
    {
      WebDriver.Close();
    }

    [JSFunction(Name = "manage")]
    public OptionsInstance Manage()
    {
      return new OptionsInstance(this.Engine.Object.InstancePrototype, WebDriver.Manage());
    }

    [JSFunction(Name = "navigate")]
    public NavigationInstance Navigate()
    {
     return new NavigationInstance(this.Engine.Object.InstancePrototype, WebDriver.Navigate());
    }

    [JSFunction(Name = "quit")]
    public void Quit()
    {
      WebDriver.Quit();
    }

    [JSFunction(Name = "switchTo")]
    public TargetLocatorInstance SwitchTo()
    {
      return new TargetLocatorInstance(this.Engine.Object.InstancePrototype, WebDriver.SwitchTo());
    }

    [JSFunction(Name = "findElement")]
    public WebElementInstance FindElement(ByInstance by)
    {
      if (by == null)
        throw new JavaScriptException(this.Engine, "Error", "A predicate to search by must be specified.");

      var result = WebDriver.FindElement(by.By);
      return new WebElementInstance(this.Engine.Object.InstancePrototype, result);
    }

    [JSFunction(Name = "findElements")]
    public ArrayInstance FindElements(ByInstance by)
    {
      if (by == null)
        throw new JavaScriptException(this.Engine, "Error", "A predicate to search by must be specified.");

      var result = WebDriver.FindElements(by.By);
      var resultArray = this.Engine.Array.Construct();
      foreach (var r in result)
        ArrayInstance.Push(resultArray, new WebElementInstance(this.Engine.Object.InstancePrototype, r));

      return resultArray;
    }

    [JSFunction(Name = "dispose")]
    public void Dispose()
    {
      WebDriver.Dispose();
    }
    #endregion
  }
}
