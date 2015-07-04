namespace Barista.Library
{
    using System.Globalization;
    using Jurassic;
  using Jurassic.Library;
  using System;
  using System.Net;

  [Serializable]
  public class HttpResponseInstance : ObjectInstance
  {
    private string m_body;

    public HttpResponseInstance(ScriptEngine engine, BrewResponse response)
      : base(engine)
    {
      this.AutoDetectContentType = true;
      this.Response = response;
      this.PopulateFunctions();
      this.StatusCode = 200;
    }

    public BrewResponse Response
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

    [JSProperty(Name = "lastModified")]
    public DateInstance LastModified
    {
      get
      {
          if (!Response.Headers.ContainsKey("last-modified"))
              return null;

          DateTime result;
          return DateTime.TryParseExact(Response.Headers["last-modified"], "R", CultureInfo.InvariantCulture, DateTimeStyles.None, out result)
              ? JurassicHelper.ToDateInstance(this.Engine, result)
              : null;
      }
      set
      {
          if (value == null)
              Response.Headers.Remove("last-modified");
          else
            Response.Headers["Last-Modified"] = value.Value.ToUniversalTime().ToString("R");
      }
    }

    [JSProperty(Name = "expires")]
    public DateInstance Expires
    {
      get
      {
          if (!Response.Headers.ContainsKey("expires"))
              return null;

          DateTime result;
          return DateTime.TryParseExact(Response.Headers["expires"], "R", CultureInfo.InvariantCulture, DateTimeStyles.None, out result)
              ? JurassicHelper.ToDateInstance(this.Engine, result)
              : null;
      }
      set
      {
          if (value == null)
              Response.Headers.Remove("expires");
          else
            Response.Headers["Expires"] = value.Value.ToUniversalTime().ToString("R");
      }
    }


    [JSProperty(Name = "redirectLocation")]
    public string Location
    {
        get
        {
            if (!Response.Headers.ContainsKey("location"))
                return null;

            return Response.Headers["location"];
        }
        set
        {
            Response.Headers["Location"] = value;
        }
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
