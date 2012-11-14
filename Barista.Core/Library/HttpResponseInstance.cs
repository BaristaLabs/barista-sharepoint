namespace Barista.Library
{
  using Jurassic;
  using Jurassic.Library;
  using System;
  using System.IO;
  using System.Net;
  using System.Web;

  public class HttpResponseInstance : ObjectInstance
  {
    private string m_body;

    public HttpResponseInstance(ScriptEngine engine, BrewResponse response)
      : base(engine)
    {
      this.AutoDetectContentType = true;
      this.Response = response;
      this.PopulateFields();
      this.PopulateFunctions();
      this.StatusCode = 200;
    }

    internal BrewResponse Response
    {
      get;
      private set;
    }

    [JSProperty(Name = "autoDetectContentType")]
    public bool AutoDetectContentType
    {
      get;
      set;
    }

    [JSProperty(Name = "body")]
    public string Body
    {
      get
      {
        return m_body;
      }
      set
      {
        HasBodyBeenSet = true;
        m_body = value;
      }
    }

    [JSProperty(Name = "contentType")]
    public string ContentType
    {
      get
      {
        return Response.ContentType;
      }
      set
      {
        AutoDetectContentType = false;
        Response.ContentType = value;
      }
    }

    /// <summary>
    /// Gets or sets a value that indicates if the response body has been set.
    /// </summary>
    internal bool HasBodyBeenSet
    {
      get;
      set;
    }

    [JSProperty(Name = "isRaw")]
    public bool IsRaw
    {
      get;
      set;
    }

    [JSProperty(Name = "redirectLocation")]
    public string Location
    {
      get { return Response.RedirectLocation; }
      set { Response.RedirectLocation = value; }
    }

    [JSProperty(Name = "statusCode")]
    public int StatusCode
    {
      get { return (int)Response.StatusCode; }
      set { Response.StatusCode = (HttpStatusCode)value; }
    }

    [JSFunction(Name = "getHeaders")]
    public ObjectInstance GetHeaders()
    {
      var result = this.Engine.Object.Construct();
      foreach (var key in this.Response.Headers.Keys)
      {
        result.SetPropertyValue(key, this.Response.Headers[key], false);
      }
      return result;
    }

    [JSFunction(Name = "setHeader")]
    public string SetHeader(string name, string value)
    {
      this.Response.Headers[name] = value;
      return value;
    }
  }
}
