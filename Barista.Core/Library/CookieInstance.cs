namespace Barista.Library
{
  using Jurassic;
  using Jurassic.Library;
  using System;
  using System.Net;

  [Serializable]
  public class CookieConstructor : ClrFunction
  {
    public CookieConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "Cookie", new CookieInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public CookieInstance Construct()
    {
      return new CookieInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class CookieInstance : ObjectInstance
  {
    private readonly Cookie m_cookie;

    public CookieInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public CookieInstance(ObjectInstance prototype, Cookie cookie)
      : this(prototype)
    {
      if (cookie == null)
        throw new ArgumentNullException("cookie");

      m_cookie = cookie;
    }

    public Cookie Cookie
    {
      get { return m_cookie; }
    }

    [JSProperty(Name = "comment")]
    public string Comment
    {
      get { return m_cookie.Comment; }
      set { m_cookie.Comment = value; }
    }

    [JSProperty(Name = "commentUri")]
    public UriInstance CommentUri
    {
      get { return new UriInstance(this.Engine.Object.InstancePrototype, m_cookie.CommentUri); }
      set
      {
        if (value != null)
          m_cookie.CommentUri = value.Uri;
      }
    }

    [JSProperty(Name = "discard")]
    public bool Discard
    {
      get { return m_cookie.Discard; }
      set { m_cookie.Discard = value; }
    }

    [JSProperty(Name = "domain")]
    public string Domain
    {
      get { return m_cookie.Domain; }
      set { m_cookie.Domain = value; }
    }

    [JSProperty(Name = "expired")]
    public bool Expired
    {
      get { return m_cookie.Expired; }
      set { m_cookie.Expired = value; }
    }

    [JSProperty(Name = "expires")]
    public DateInstance Expires
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_cookie.Expires); }
      set
      {
        if (value != null)
          m_cookie.Expires = DateTime.Parse(value.ToISOString());
      }
    }

    [JSProperty(Name = "httpOnly")]
    public bool HttpOnly
    {
      get { return m_cookie.HttpOnly; }
      set { m_cookie.HttpOnly = value; }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_cookie.Name; }
      set { m_cookie.Name = value; }
    }

    [JSProperty(Name = "path")]
    public string Path
    {
      get { return m_cookie.Path; }
      set { m_cookie.Path = value; }
    }

    [JSProperty(Name = "port")]
    public string Port
    {
      get { return m_cookie.Port; }
      set { m_cookie.Port = value; }
    }

    [JSProperty(Name = "secure")]
    public bool Secure
    {
      get { return m_cookie.Secure; }
      set { m_cookie.Secure = value; }
    }

    [JSProperty(Name = "timeStamp")]
    public DateInstance TimeStamp
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_cookie.TimeStamp); }
    }

    [JSProperty(Name = "value")]
    public string Value
    {
      get { return m_cookie.Value; }
      set { m_cookie.Value = value; }
    }

    [JSProperty(Name = "version")]
    public int Version
    {
      get { return m_cookie.Version; }
      set { m_cookie.Version = value; }
    }
  }
}
