namespace Barista.Automation.Selenium
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using OpenQA.Selenium;

  [Serializable]
  public class CookieJarConstructor : ClrFunction
  {
    public CookieJarConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "CookieJar", new CookieJarInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public CookieJarInstance Construct()
    {
      return new CookieJarInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class CookieJarInstance : ObjectInstance
  {
    private readonly ICookieJar m_cookieJar;

    public CookieJarInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public CookieJarInstance(ObjectInstance prototype, ICookieJar cookieJar)
      : this(prototype)
    {
      if (cookieJar == null)
        throw new ArgumentNullException("cookieJar");

      m_cookieJar = cookieJar;
    }

    public ICookieJar CookieJar
    {
      get { return m_cookieJar; }
    }

    [JSFunction(Name = "addCookie")]
    public void AddCookie(CookieInstance cookie)
    {
      if (cookie == null)
        return;

      m_cookieJar.AddCookie(cookie.Cookie);
    }

    [JSProperty(Name = "allCookies")]
    public ArrayInstance AllCookies
    {
      get
      {
        var result = this.Engine.Array.Construct();

        foreach (var cookie in m_cookieJar.AllCookies)
        {
          ArrayInstance.Push(new CookieInstance(this.Engine.Object.InstancePrototype, cookie));
        }

        return result;
      }
    }

    [JSFunction(Name = "deleteAllCookies")]
    public void DeleteAllCookies()
    {
      m_cookieJar.DeleteAllCookies();
    }

    [JSFunction(Name = "deleteCookie")]
    public void DeleteCookie(CookieInstance cookie)
    {
      if (cookie == null)
        return;

      m_cookieJar.DeleteCookie(cookie.Cookie);
    }

    [JSFunction(Name = "deleteCookieNamed")]
    public void DeleteCookieNamed(string name)
    {
      m_cookieJar.DeleteCookieNamed(name);
    }

    [JSFunction(Name = "getCookieNamed")]
    public CookieInstance GetCookieNamed(string name)
    {
      return new CookieInstance(this.Engine.Object.InstancePrototype, m_cookieJar.GetCookieNamed(name));
    }
  }
}
