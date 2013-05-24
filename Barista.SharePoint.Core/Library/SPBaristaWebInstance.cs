namespace Barista.SharePoint.Library
{
  using System;
  using System.Reflection;
  using Jurassic;
  using Jurassic.Library;
  using Barista.Library;

  /// <summary>
  /// Represents a SharePoint-specific implementation of a WebInstance.
  /// </summary>
  public class SPBaristaWebInstance : WebInstanceBase
  {
    private HttpRequestInstance m_httpRequest;
    private HttpResponseInstance m_httpResponse;

    public SPBaristaWebInstance(ScriptEngine engine)
      : base(engine)
    {
      this.PopulateFunctions(this.GetType(), BindingFlags.Instance | BindingFlags.Public);
    }

    [JSProperty(Name = "request")]
    public override HttpRequestInstance Request
    {
      get
      {
        if (m_httpRequest == null || (m_httpRequest != null && Object.Equals(m_httpRequest.Request, SPBaristaContext.Current.Request) == false))
        {
          m_httpRequest = new HttpRequestInstance(this.Engine, SPBaristaContext.Current.Request);
        }

        return m_httpRequest;
      }
      set { m_httpRequest = value; }
    }

    [JSProperty(Name = "response")]
    public override HttpResponseInstance Response
    {
      get
      {
        if (m_httpResponse == null || (m_httpResponse != null && Object.Equals(m_httpResponse.Response, SPBaristaContext.Current.Response) == false))
        {
          m_httpResponse = new HttpResponseInstance(this.Engine, SPBaristaContext.Current.Response);
        }

        return m_httpResponse;
      }
      set { m_httpResponse = value; }
    }

    protected override Uri ObtainTargetUri(string url)
    {
      Uri targetUri;
      if (SPHelper.TryCreateWebAbsoluteUri(url, out targetUri) == false)
        throw new InvalidOperationException("Unable to convert target url to absolute uri: " + url);
      return targetUri;
    }

    protected override string ObtainDefaultProxyAddress()
    {
      //Get the proxy address from the farm property bag setting and use it as the default proxy object.
      return Utilities.GetFarmKeyValue(Constants.FarmProxyAddressKey);
    }

    protected override void LogAjaxException(Exception ex)
    {
      BaristaDiagnosticsService.Local.LogException(ex, BaristaDiagnosticCategory.Runtime, "Error in web.ajax.call");
    }
  }
}
