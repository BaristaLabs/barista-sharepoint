namespace Barista.Automation.Selenium
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using OpenQA.Selenium;
  using System;

  [Serializable]
  public class SearchContextConstructor : ClrFunction
  {
    public SearchContextConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SearchContext", new SearchContextInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SearchContextInstance Construct()
    {
      return new SearchContextInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SearchContextInstance : ObjectInstance
  {
    private readonly ISearchContext m_searchContext;

    public SearchContextInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SearchContextInstance(ObjectInstance prototype, ISearchContext searchContext)
      : this(prototype)
    {
      if (searchContext == null)
        throw new ArgumentNullException("searchContext");

      m_searchContext = searchContext;
    }

    public ISearchContext SearchContext
    {
      get { return m_searchContext; }
    }

    [JSFunction(Name = "findElement")]
    public WebElementInstance FindElement(ByInstance by)
    {
      if (by == null)
        throw new JavaScriptException(this.Engine, "Error", "A predicate to search by must be specified.");

      var result = m_searchContext.FindElement(by.By);
      return new WebElementInstance(this.Engine.Object.InstancePrototype, result);
    }

    [JSFunction(Name = "findElements")]
    public ArrayInstance FindElements(ByInstance by)
    {
      if (by == null)
        throw new JavaScriptException(this.Engine, "Error", "A predicate to search by must be specified.");

      var result = m_searchContext.FindElements(by.By);
      var resultArray = this.Engine.Array.Construct();
      foreach (var r in result)
        ArrayInstance.Push(resultArray, new WebElementInstance(this.Engine.Object.InstancePrototype, r));

      return resultArray;
    }
  }
}
