namespace Barista.SharePoint.WebParts
{
  using HtmlAgilityPack;
  using Microsoft.SharePoint;
  using System;
  using System.ComponentModel;
  using System.IO;
  using System.Net;
  using System.Net.Cache;
  using System.Text;
  using System.Web;
  using System.Web.UI;
  using System.Web.UI.WebControls.WebParts;

  [ToolboxItemAttribute(false)]
  public class HtmlViewerWebPart : WebPart
  {
    #region Fields
    private const string DefaultTargetUrl = "";
    private const string DefaultProxyAddress = "";
    private const string DefaultUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; .NET CLR 1.0.3705)";
    private const int DefaultTimeout = 60000;
    private const int DefaultRetries = 3;
    private const bool DefaultConvertRelativeUrlsToAbsolute = false;
    private const bool DefaultRenderResultWithinIFrame = false;
    private const bool DefaultUseProxy = true;
    #endregion

    #region Constructor
    public HtmlViewerWebPart()
    {
      this.ConvertRelativeUrlsToAbsolute = DefaultConvertRelativeUrlsToAbsolute;
      this.ProxyAddress = DefaultProxyAddress;
      this.TargetUrl = String.Empty;
      this.Timeout = DefaultTimeout;
      this.Retries = DefaultRetries;
      this.RenderResultWithinIFrame = DefaultRenderResultWithinIFrame;
      this.UseProxy = DefaultUseProxy;
      this.UserAgent = DefaultUserAgent;
    }
    #endregion

    #region Web Part Properties
    [WebBrowsable(true)]
    [WebDisplayName("Convert Relative Urls To Absolute?")]
    [WebDescription("True to convert all relative urls to absolute urls in the response.")]
    [Personalizable(PersonalizationScope.Shared)]
    [Category("Settings")]
    [DefaultValue(DefaultConvertRelativeUrlsToAbsolute)]
    public bool ConvertRelativeUrlsToAbsolute
    {
      get;
      set;
    }

    [WebBrowsable(true)]
    [WebDisplayName("Proxy Address")]
    [WebDescription("Please enter the proxy address to use when making the web request.")]
    [Personalizable(PersonalizationScope.Shared)]
    [Category("Settings")]
    [DefaultValue(DefaultProxyAddress)]
    public string ProxyAddress
    {
      get;
      set;
    }

    [WebBrowsable(true)]
    [WebDisplayName("Render result within IFrame")]
    [WebDescription("True if the result should be contained as the contents of a IFrame - Use this to resove scripting issues.")]
    [Personalizable(PersonalizationScope.Shared)]
    [Category("Settings")]
    [DefaultValue(DefaultRenderResultWithinIFrame)]
    public bool RenderResultWithinIFrame
    {
      get;
      set;
    }

    [WebBrowsable(true)]
    [WebDisplayName("Retries")]
    [WebDescription("Please enter the number of retries to attempt.")]
    [Personalizable(PersonalizationScope.Shared)]
    [Category("Settings")]
    [DefaultValue(DefaultRetries)]
    public int Retries
    {
      get;
      set;
    }

    [WebBrowsable(true)]
    [WebDisplayName("Target Url")]
    [WebDescription("Please enter the url of the web page to retrieve.")]
    [Personalizable(PersonalizationScope.Shared)]
    [Category("Settings")]
    [DefaultValue(DefaultTargetUrl)]
    public string TargetUrl
    {
      get;
      set;
    }

    [WebBrowsable(true)]
    [WebDisplayName("Request Timeout")]
    [WebDescription("Please enter time-out in milliseconds to wait for the response.")]
    [Personalizable(PersonalizationScope.Shared)]
    [Category("Settings")]
    [DefaultValue(DefaultTimeout)]
    public int Timeout
    {
      get;
      set;
    }

    [WebBrowsable(true)]
    [WebDisplayName("Use Proxy?")]
    [WebDescription("True to use proxy settings.")]
    [Personalizable(PersonalizationScope.Shared)]
    [Category("Settings")]
    [DefaultValue(DefaultUseProxy)]
    public bool UseProxy
    {
      get;
      set;
    }

    [WebBrowsable(true)]
    [WebDisplayName("User Agent")]
    [WebDescription("Please enter the User Agent to use when making the web request.")]
    [Personalizable(PersonalizationScope.Shared)]
    [Category("Settings")]
    [DefaultValue(DefaultUserAgent)]
    public string UserAgent
    {
      get;
      set;
    }
    #endregion

    protected override void CreateChildControls()
    {
      string noUrlSetHtml = string.Format("<div style='width:100%; text-align: center; padding: 4px;'><a id='MsoFrameworkToolpartDefmsg_{0}' href=\"javascript:MSOTlPn_ShowToolPane2Wrapper('Edit','129','{0}');\">Click here to add url.</a></div>", this.ID);

      if (String.IsNullOrEmpty(this.TargetUrl) || (this.TargetUrl != null && this.TargetUrl.Trim() == String.Empty))
      {
        LiteralControl lctrl = new LiteralControl(noUrlSetHtml);
        this.Controls.Add(lctrl);
        return;
      }

      try
      {
        int retries = DefaultRetries;
        if (this.Retries > 0)
          retries = this.Retries;

        String result;

        if (this.RenderResultWithinIFrame)
        {
          string iFrameId = String.Format("{0}_htmlViewerIFrame", this.ID);
          string code;
          if (String.IsNullOrEmpty(this.ProxyAddress))
            code = String.Format(@"
var web = require(""Web"");
var response = web.ajax('{0}');
web.response.contentType = ""text/html"";
response;", 
          this.TargetUrl.Trim());

          else
            code = String.Format(@"
var web = require(""Web"");
var response = web.ajax('{0}', {{ useDefaultCredentials: true, proxy: {{ useDefaultCredentials: true, address: {1} }});
web.response.contentType = ""text/html"";
response;",
          this.TargetUrl.Trim(), this.ProxyAddress.Trim());

          string src = SPContext.Current.Web.Url + "/_vti_bin/Barista/v1/Barista.svc/eval?c=" + HttpUtility.UrlEncode(code);

          StringBuilder resultBuilder = new StringBuilder();
          resultBuilder.AppendFormat("<iframe id=\"{0}\" seamless=\"seamless\" src=\"{1}\" style=\"width: 100%; height: 100%;\" marginwidth=\"0\" marginheight=\"0\" frameborder=\"0\" vspace=\"0\" hspace=\"0\"></iframe>", iFrameId, HttpUtility.HtmlAttributeEncode(src));
          result = resultBuilder.ToString();
        }
        else
        {
          result = RetriveResponse(this.TargetUrl.Trim(), retries);
        }
        
        LiteralControl cntrl = new LiteralControl(result);
        Controls.Add(cntrl);
      }
      catch (Exception ex)
      {
        this.Controls.Add(new LiteralControl("<div id='IdOFSRSSFeedMainDiv' class='OFSBulLstMainDiv'>" + ex.Message + "</div>"));
      }
    }

    private string RetriveResponse(string url, int retries = 3)
    {
      Uri targetUri;
      if (SPHelper.TryCreateWebAbsoluteUri(url, out targetUri) == false)
        throw new InvalidOperationException("Unable to convert target url to absolute uri: " + url);

      bool success = false;
      string result = string.Empty;

      for(int i = 1; i <= retries; i++)
      {
        try
        {
          var noCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);

          var httpWebRequest = (HttpWebRequest)WebRequest.Create(targetUri);
          httpWebRequest.UseDefaultCredentials = true;
          httpWebRequest.Credentials = CredentialCache.DefaultNetworkCredentials;
          httpWebRequest.CachePolicy = noCachePolicy;

          if (this.Timeout > 0)
          {
            httpWebRequest.Timeout = this.Timeout;
          }

          httpWebRequest.UserAgent = String.IsNullOrEmpty(this.UserAgent) == false
            ? this.UserAgent
            : DefaultUserAgent;
          
          //Use The Default Proxy
          if (this.UseProxy)
          {
            var farmProxyAddress = Utilities.GetFarmKeyValue(Constants.FarmProxyAddressKey);
            if (String.IsNullOrEmpty(this.ProxyAddress) == false)
            {
              httpWebRequest.Proxy = new WebProxy(this.ProxyAddress, true, null, CredentialCache.DefaultNetworkCredentials);
            }
            else if (String.IsNullOrEmpty(farmProxyAddress) == false)
            {
              httpWebRequest.Proxy = new WebProxy(farmProxyAddress, true, null, CredentialCache.DefaultNetworkCredentials);
            }
            else
            {
              httpWebRequest.Proxy = WebRequest.GetSystemWebProxy();
            }

            if (httpWebRequest.Proxy != null)
            {
              httpWebRequest.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
            }
          }

          try
          {
            using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
              StreamReader reader = new StreamReader(httpWebResponse.GetResponseStream());
              result = reader.ReadToEnd();
            }
          }
          catch (WebException webException)
          {
            StreamReader reader = new StreamReader(webException.Response.GetResponseStream());
            result = reader.ReadToEnd();
          }
          success = true;
        }
        catch { /* Do Nothing */ }
      }

      if (success == false)
        result = String.Format("Unable to connect to {0}. Maximum number of retries exceeded ({1})", targetUri, retries);

      //Convert relative urls to absolute:
      if (this.ConvertRelativeUrlsToAbsolute && String.IsNullOrEmpty(result) == false)
      {
        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(result);

        // And and now queue up all the links on this page
        foreach (HtmlNode link in htmlDoc.DocumentNode.SelectNodes(@"//a[@href]"))
        {
          HtmlAttribute att = link.Attributes["href"];
          if (att == null) continue;
          string href = att.Value;
          if (href.StartsWith("javascript", StringComparison.InvariantCultureIgnoreCase)) continue;      // ignore javascript on buttons using a tags

          Uri currentUri = new Uri(href, UriKind.RelativeOrAbsolute);

          // Make it absolute if it's relative
          if (!currentUri.IsAbsoluteUri)
          {
            currentUri = new Uri(targetUri, currentUri);
            att.Value = currentUri.ToString();
          }
        }

        foreach (HtmlNode img in htmlDoc.DocumentNode.SelectNodes(@"//img[@src]"))
        {
          HtmlAttribute att = img.Attributes["src"];
          if (att == null) continue;
          string href = att.Value;
          if (href.StartsWith("javascript", StringComparison.InvariantCultureIgnoreCase)) continue;      // ignore javascript on buttons using a tags

          Uri currentUri = new Uri(href, UriKind.RelativeOrAbsolute);

          // Make it absolute if it's relative
          if (!currentUri.IsAbsoluteUri)
          {
            currentUri = new Uri(targetUri, currentUri);
            att.Value = currentUri.ToString();
          }
        }

        StringWriter sw = new StringWriter();
        htmlDoc.Save(sw);
        result = sw.ToString();
      }

      if (String.IsNullOrEmpty(result))
        result = "The response contained no data.";
      return result;
    }
  }
}
