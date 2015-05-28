namespace Barista.TeamFoundationServer
{
  using System;
  using System.Net;
  using Barista.Jurassic;

  public class BaristaNetworkCredentialsProvider
  {
    private readonly ScriptEngine m_engine;

    public BaristaNetworkCredentialsProvider(ScriptEngine engine)
    {
      if (engine == null)
        throw new ArgumentNullException("engine");

      m_engine = engine;
    }

    public ScriptEngine Engine
    {
      get { return m_engine; }
    }

    public ICredentials Credential
    {
      get;
      set;
    }

    public ICredentials GetCredentials(Uri uri, ICredentials iCredentials)
    {
      return Credential;
    }

    public void NotifyCredentialsAuthenticated(Uri uri)
    {
      throw new JavaScriptException(m_engine, "Error", "Unable to authenticate with target TFS server.");
    }
  }
}
