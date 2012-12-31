namespace Barista.Library
{
  using Jurassic;
  using Jurassic.Library;
  using System;
  using System.Net;
  using System.Collections.Generic;

  [Serializable]
  public class HttpWebResponseConstructor : ClrFunction
  {
    public HttpWebResponseConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "HttpWebResponse", new HttpWebResponseInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public HttpWebResponseInstance Construct()
    {
      return new HttpWebResponseInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class HttpWebResponseInstance : ObjectInstance, IDisposable
  {
    private readonly HttpWebResponse m_httpWebResponse;

    public HttpWebResponseInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public HttpWebResponseInstance(ObjectInstance prototype, HttpWebResponse httpWebResponse)
      : this(prototype)
    {
      if (httpWebResponse == null)
        throw new ArgumentNullException("httpWebResponse");

      m_httpWebResponse = httpWebResponse;
    }

    public HttpWebResponse HttpWebResponse
    {
      get { return m_httpWebResponse; }
    }

    [JSProperty(Name = "characterSet")]
    public string CharacterSet
    {
      get { return m_httpWebResponse.CharacterSet; }
    }

    [JSProperty(Name = "contentEncoding")]
    public string ContentEncoding
    {
      get { return m_httpWebResponse.ContentEncoding; }
    }

    [JSProperty(Name = "contentLength")]
    public double ContentLength
    {
      get { return m_httpWebResponse.ContentLength; }
    }

    [JSProperty(Name = "contentType")]
    public string ContentType
    {
      get { return m_httpWebResponse.ContentType; }
    }

    [JSProperty(Name = "cookies")]
    public ArrayInstance Cookies
    {
      get
      {
        var cookieInstances = new List<CookieInstance>();
        for (var i = 0; i < m_httpWebResponse.Cookies.Count; i++)
        {
          var cookie = m_httpWebResponse.Cookies[i];
          cookieInstances.Add(new CookieInstance(this.Engine.Object.InstancePrototype, cookie));
        }

// ReSharper disable CoVariantArrayConversion
        return this.Engine.Array.Construct(cookieInstances.ToArray());
// ReSharper restore CoVariantArrayConversion
      }
    }

    [JSProperty(Name = "headers")]
    public ObjectInstance Headers
    {
      get
      {
        var result = this.Engine.Object.Construct();
        foreach (var key in m_httpWebResponse.Headers.AllKeys)
        {
          result.SetPropertyValue(key, m_httpWebResponse.Headers[key], false);
        }
        return result;
      }
    }

    [JSProperty(Name = "isMutuallyAuthenticated")]
    public bool IsMutallyAuthenticated
    {
      get { return m_httpWebResponse.IsMutuallyAuthenticated; }
    }

    [JSProperty(Name = "lastModified")]
    public DateInstance LastModified
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_httpWebResponse.LastModified); }
    }

    [JSProperty(Name = "method")]
    public string Method
    {
      get { return m_httpWebResponse.Method; }
    }

    [JSProperty(Name = "protocolVersion")]
    public string ProtocolVersion
    {
      get { return m_httpWebResponse.ProtocolVersion.ToString(); }
    }

    [JSProperty(Name = "responseUri")]
    public UriInstance ResponseUri
    {
      get { return new UriInstance(this.Engine.Object.InstancePrototype, m_httpWebResponse.ResponseUri); }
    }

    [JSProperty(Name = "server")]
    public string Server
    {
      get { return m_httpWebResponse.Server; }
    }

    [JSProperty(Name = "statusCode")]
    public int StatusCode
    {
      get { return (int)m_httpWebResponse.StatusCode; }
    }

    [JSProperty(Name = "statusDescription")]
    public string StatusDescription
    {
      get { return m_httpWebResponse.StatusDescription; }
    }

    public void Dispose()
    {
      if (m_httpWebResponse != null)
        ((IDisposable)m_httpWebResponse).Dispose();
    }
  }
}
