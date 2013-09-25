namespace Barista.Automation.Selenium
{

  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using OpenQA.Selenium;
  using System;

  [Serializable]
  public class ByConstructor : ClrFunction
  {
    public ByConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "By", new ByInstance(engine.Object.InstancePrototype))
    {
      this.PopulateFunctions();
    }

    [JSConstructorFunction]
    public ByInstance Construct()
    {
      return new ByInstance(this.InstancePrototype);
    }

    [JSFunction(Name = "className")]
    public ByInstance ClassName(string classNameToFind)
    {
      return new ByInstance(this.Engine.Object.InstancePrototype, By.ClassName(classNameToFind));
    }

    [JSFunction(Name = "cssSelector")]
    public ByInstance CssSelector(string cssSelectorToFind)
    {
      return new ByInstance(this.Engine.Object.InstancePrototype, By.CssSelector(cssSelectorToFind));
    }

    [JSFunction(Name = "id")]
    public ByInstance Id(string idToFind)
    {
      return new ByInstance(this.Engine.Object.InstancePrototype, By.Id(idToFind));
    }

    [JSFunction(Name = "linkText")]
    public ByInstance LinkText(string linkTextToFind)
    {
      return new ByInstance(this.Engine.Object.InstancePrototype, By.LinkText(linkTextToFind));
    }

    [JSFunction(Name = "name")]
    public ByInstance ByName(string nameToFind)
    {
      return new ByInstance(this.Engine.Object.InstancePrototype, By.Name(nameToFind));
    }

    [JSFunction(Name = "partialLinkText")]
    public ByInstance PartialLinkText(string partialLinkTextToFind)
    {
      return new ByInstance(this.Engine.Object.InstancePrototype, By.PartialLinkText(partialLinkTextToFind));
    }

    [JSFunction(Name = "tagName")]
    public ByInstance TagName(string tagNameToFind)
    {
      return new ByInstance(this.Engine.Object.InstancePrototype, By.TagName(tagNameToFind));
    }

    [JSFunction(Name = "xpath")]
    public ByInstance XPath(string xpathToFind)
    {
      return new ByInstance(this.Engine.Object.InstancePrototype, By.XPath(xpathToFind));
    }
  }

  [Serializable]
  public class ByInstance : ObjectInstance
  {
    private readonly By m_by;

    public ByInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ByInstance(ObjectInstance prototype, By by)
      : this(prototype)
    {
      if (by == null)
        throw new ArgumentNullException("by");

      m_by = by;
    }

    public By By
    {
      get { return m_by; }
    }

    [JSFunction(Name = "findElement")]
    public WebElementInstance FindElement(SearchContextInstance searchContext)
    {
      if (searchContext == null)
        throw new JavaScriptException(this.Engine, "Error", "A Search Context must be specified.");

      var result = m_by.FindElement(searchContext.SearchContext);
      return new WebElementInstance(this.Engine.Object.InstancePrototype, result);
    }

    [JSFunction(Name = "findElements")]
    public ArrayInstance FindElements(SearchContextInstance searchContext)
    {
      if (searchContext == null)
        throw new JavaScriptException(this.Engine, "Error", "A Search Context must be specified.");

      var result = m_by.FindElements(searchContext.SearchContext);
      var resultArray = this.Engine.Array.Construct();
      foreach (var r in result)
        ArrayInstance.Push(resultArray, new WebElementInstance(this.Engine.Object.InstancePrototype, r));

      return resultArray;
    }

    [JSFunction(Name = "toString")]
    public override string ToString()
    {
      return m_by.ToString();
    }
  }
}
