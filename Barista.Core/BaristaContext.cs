namespace Barista
{
  using System;
  using System.Web;

  [Serializable]
  public class BaristaContext : IDisposable
  {
    private bool m_disposed;
    private readonly object m_disposeLock = new object();

    protected BaristaContext()
    {
    }

    public BaristaContext(BrewRequest request, BrewResponse response)
      : this()
    {
      if (request == null)
        throw new ArgumentNullException("request");

      if (response == null)
        response = new BrewResponse();

      this.Request = request;
      this.Response = response;
    }

    /// <summary>
    /// Gets the BrewRequest associated with the context.
    /// </summary>
    public BrewRequest Request
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the BrewResponse associated with the context.
    /// </summary>
    public BrewResponse Response
    {
      get;
      private set;
    }

    public void Dispose()
    {
      Dispose(true);

      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (m_disposed)
        return;

      if (m_disposed == false)
      {
        lock (m_disposeLock)
        {
          if (!m_disposed)
          {
            this.Request = null;
            this.Response = null;
            m_disposed = true;
          }
        }
      }
    }

    #region Static Members
    [ThreadStatic]
    private static BaristaContext s_currentContext;

    /// <summary>
    /// Gets or sets the current Barista Context. If there is no current context one will be created using the current HttpContext.
    /// </summary>
    public static BaristaContext Current
    {
      get
      {
        if (s_currentContext == null && HttpContext.Current != null)
        {
          s_currentContext = BaristaContext.CreateContextFromHttpContext(HttpContext.Current);
        }
        return s_currentContext;
      }
      set { s_currentContext = value; }
    }

    /// <summary>
    /// Gets a value that indicates if a current Barista context is available.
    /// </summary>
    public static bool HasCurrentContext
    {
      get { return s_currentContext != null; }
    }

    /// <summary>
    /// Using the specified HttpContext, returns a new Barista Context.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static BaristaContext CreateContextFromHttpContext(HttpContext context)
    {
      var result = new BaristaContext();

      return result;
    }
    #endregion

  }
}
