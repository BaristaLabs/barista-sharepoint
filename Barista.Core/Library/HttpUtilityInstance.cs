namespace Barista.Library
{
  using System.Web;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;

  [Serializable]
  public class HttpUtilityConstructor : ClrFunction
  {
    public HttpUtilityConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "HttpUtility", new HttpUtilityInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public HttpUtilityInstance Construct()
    {
      return new HttpUtilityInstance(this.InstancePrototype);
    }

    [JSFunction(Name = "urlEncode")]
    public string UrlEncode(string str)
    {
      return HttpUtility.UrlEncode(str);
    }

    [JSFunction(Name = "urlDecode")]
    public string UrlDecode(string str)
    {
      return HttpUtility.UrlDecode(str);
    }
  }

  [Serializable]
  public class HttpUtilityInstance : ObjectInstance
  {
    private readonly HttpUtility m_httpUtility;

    public HttpUtilityInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public HttpUtilityInstance(ObjectInstance prototype, HttpUtility httpUtility)
      : this(prototype)
    {
      if (httpUtility == null)
        throw new ArgumentNullException("httpUtility");

      m_httpUtility = httpUtility;
    }

    public HttpUtility HttpUtility
    {
      get { return m_httpUtility; }
    }
  }
}
