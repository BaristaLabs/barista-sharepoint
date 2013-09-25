namespace Barista.Automation.Selenium
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using OpenQA.Selenium;
  using System;

  [Serializable]
  public class CookieConstructor : ClrFunction
  {
    public CookieConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "Cookie", new CookieInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public CookieInstance Construct(string name, string value, object arg3, object arg4, object arg5)
    {
      if (TypeUtilities.IsString(arg3))
      {
        if (arg4 == Undefined.Value && arg5 == Undefined.Value)
          return new CookieInstance(this.Engine.Object.InstancePrototype, new Cookie(name, value, TypeConverter.ToString(arg3)));

        if ((arg4 == null || arg4 == Null.Value || arg4 is DateInstance) && arg5 == Undefined.Value)
        {
          var date = arg4 as DateInstance;
          if (arg4 == null || arg4 == Null.Value)
            return new CookieInstance(this.Engine.Object.InstancePrototype,
                                      new Cookie(name, value, TypeConverter.ToString(arg3), (DateTime?) null));

          return new CookieInstance(this.Engine.Object.InstancePrototype,
                                    new Cookie(name, value, TypeConverter.ToString(arg3), date.Value));
        }

        DateTime? expiryDate;

        if (arg5 as DateInstance == null)
          expiryDate = null;
        else
        {
          var date = arg5 as DateInstance;
          expiryDate = date.Value;
        }

        return new CookieInstance(this.Engine.Object.InstancePrototype,
          new Cookie(name, value, TypeConverter.ToString(arg3), TypeConverter.ToString(arg4), expiryDate));
      }

      return new CookieInstance(this.InstancePrototype, new Cookie(name, value));
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

    [JSProperty(Name = "domain")]
    public string Domain
    {
      get { return m_cookie.Domain; }
    }

    [JSProperty(Name = "expiry")]
    public object Expiry
    {
      get
      {
        if (m_cookie.Expiry.HasValue == false)
          return Null.Value;
        
        return JurassicHelper.ToDateInstance(this.Engine, m_cookie.Expiry.Value);
      }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_cookie.Name; }
    }

    [JSProperty(Name = "path")]
    public string Path
    {
      get { return m_cookie.Path; }
    }

    [JSProperty(Name = "secure")]
    public bool Secure
    {
      get { return m_cookie.Secure; }
    }

    [JSProperty(Name = "value")]
    public string Value
    {
      get { return m_cookie.Value; }
    }

    [JSFunction(Name = "toString")]
    public override string ToString()
    {
      return m_cookie.ToString();
    }
  }
}
