namespace Barista.Automation.Selenium
{
  using System.Drawing;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using OpenQA.Selenium;
  using System;

  [Serializable]
  public class WebElementConstructor : ClrFunction
  {
    public WebElementConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "WebElement", new WebElementInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public WebElementInstance Construct()
    {
      return new WebElementInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class WebElementInstance : ObjectInstance
  {
    private readonly IWebElement m_webElement;

    public WebElementInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public WebElementInstance(ObjectInstance prototype, IWebElement webElement)
      : this(prototype)
    {
      if (webElement == null)
        throw new ArgumentNullException("webElement");

      m_webElement = webElement;
    }

    public IWebElement WebElement
    {
      get { return m_webElement; }
    }

    #region Properties
    [JSProperty(Name = "displayed")]
    public bool Displayed
    {
      get { return m_webElement.Displayed; }
    }

    [JSProperty(Name = "enabled")]
    public bool Enabled
    {
      get { return m_webElement.Enabled; }
    }

    [JSProperty(Name = "location")]
    public PointInstance Location
    {
      get
      {
        return new PointInstance(this.Engine.Object.InstancePrototype, m_webElement.Location);
      }
    }

    [JSProperty(Name = "selected")]
    public object Selected
    {
      get
      {
        try
        {
          return m_webElement.Selected;
        }
        catch (InvalidElementStateException)
        {
          return Null.Value;
        }
      }
    }

    [JSProperty(Name = "size")]
    public SizeInstance Size
    {
      get { return new SizeInstance(this.Engine.Object.InstancePrototype, m_webElement.Size); }
    }

    [JSProperty(Name = "tagName")]
    public string TagName
    {
      get { return m_webElement.TagName; }
    }

    [JSProperty(Name = "text")]
    public string Text
    {
      get { return m_webElement.Text; }
    }
    #endregion


    [JSFunction(Name = "clear")]
    public void Clear()
    {
      m_webElement.Clear();
    }

    [JSFunction(Name = "click")]
    public void Click()
    {
      m_webElement.Click();
    }

    [JSFunction(Name = "getAttribute")]
    public string GetAttribute(string attributeName)
    {
      return m_webElement.GetAttribute(attributeName);
    }

    [JSFunction(Name = "getCssValue")]
    public string GetCssValue(string propertyName)
    {
      return m_webElement.GetCssValue(propertyName);
    }

    [JSFunction(Name = "sendKeys")]
    public void SendKeys(string text)
    {
      m_webElement.SendKeys(text);
    }

    [JSFunction(Name = "submit")]
    public void Submit()
    {
      m_webElement.Submit();
    }

    [JSFunction(Name = "findElement")]
    public WebElementInstance FindElement(ByInstance by)
    {
      if (by == null)
        throw new JavaScriptException(this.Engine, "Error", "The first argument cannot be null.");

      var byby = by.By;
      var result = m_webElement.FindElement(byby);
      return new WebElementInstance(this.Engine.Object.InstancePrototype, result);
    }

    [JSFunction(Name = "findElements")]
    public ArrayInstance FindElements(ByInstance by)
    {
      if (by == null)
        throw new JavaScriptException(this.Engine, "Error", "The first argument cannot be null.");

      var byby = by.By;
      var result = m_webElement.FindElements(byby);

      var arr = this.Engine.Array.Construct();
      foreach (var r in result)
      {
        ArrayInstance.Push(new WebElementInstance(this.Engine.Object.InstancePrototype, r));
      }
      return arr;
    }
  }
}
