namespace Barista.SharePoint.Library
{
  using Barista.Extensions;
  using Barista.Library;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.IdentityModel.Claims;
  using Microsoft.IdentityModel.WindowsTokenService;
  using Newtonsoft.Json;
  using System;
  using System.IO;
  using System.Linq;
  using System.Net;
  using System.Net.Cache;
  using System.Security.Principal;
  using System.Text;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Web;
  using System.Xml.Linq;

  public class WebInstance : ObjectInstance
  {
    private HttpRequestInstance m_httpRequest;
    private HttpResponseInstance m_httpResponse;

    public WebInstance(ScriptEngine engine, BrewRequest request, BrewResponse response)
      : base(engine)
    {
      m_httpRequest = new HttpRequestInstance(engine, request);
      m_httpResponse = new HttpResponseInstance(engine, response);
      
      this.PopulateFields();
      this.PopulateFunctions();
    }

    [JSProperty(Name = "request")]
    public HttpRequestInstance Request
    {
      get { return m_httpRequest; }
    }

    [JSProperty(Name = "response")]
    public HttpResponseInstance Response
    {
      get { return m_httpResponse; }
    }

    [JSFunction(Name = "ajax")]
    public object Ajax(string url, object settings)
    {
      var ajaxSettings = JurassicHelper.Coerce<AjaxSettingsInstance>(this.Engine, settings);

      //If we're running under Claims authentication, impersonate the thread user
      //by calling the Claims to Windows Token Service and call the remote site using
      //the impersonated credentials. NOTE: The Claims to Windows Token Service must be running.
      WindowsImpersonationContext ctxt = null;
      if (Thread.CurrentPrincipal.Identity is ClaimsIdentity)
      {
        IClaimsIdentity identity = (ClaimsIdentity)System.Threading.Thread.CurrentPrincipal.Identity;
        string upn = identity.Claims.Where(c => c.ClaimType == ClaimTypes.Upn).First().Value;

        if (String.IsNullOrEmpty(upn))
        {
          throw new InvalidOperationException("No UPN claim found");
        }

        var currentIdentity = S4UClient.UpnLogon(upn);
        ctxt = currentIdentity.Impersonate();
      }

      try
      {
        HttpRequestCachePolicy noCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);

        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
        request.CachePolicy = noCachePolicy; //TODO: Make this configurable.

        var farmProxyAddress = Utilities.GetFarmKeyValue(Constants.FarmProxyAddressKey);
        if (String.IsNullOrEmpty(farmProxyAddress) == false)
        {
          request.Proxy = new WebProxy(farmProxyAddress, true, null, CredentialCache.DefaultNetworkCredentials);
        }
        else
        {
          request.Proxy = WebRequest.GetSystemWebProxy();
        }

        if (request.Proxy != null)
        {
          request.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
        }

        if (ajaxSettings != null)
        {
          if (ajaxSettings.UseDefaultCredentials == false)
          {
            if (String.IsNullOrEmpty(ajaxSettings.Username) == false || String.IsNullOrEmpty(ajaxSettings.Password) == false || String.IsNullOrEmpty(ajaxSettings.Domain) == false)
              request.Credentials = new NetworkCredential(ajaxSettings.Username, ajaxSettings.Password, ajaxSettings.Domain);
          }
          else
          {
            request.UseDefaultCredentials = true;
            request.Credentials = CredentialCache.DefaultNetworkCredentials;
          }

          if (String.IsNullOrEmpty(ajaxSettings.Accept))
            request.Accept = ajaxSettings.Accept;

          if (ajaxSettings.Proxy != null)
          {
            var proxySettings = JurassicHelper.Coerce<ProxySettingsInstance>(this.Engine, ajaxSettings.Proxy);

            if (String.IsNullOrEmpty(proxySettings.Address) == false)
            {
              try
              {
                var proxy = new WebProxy(proxySettings.Address, true);
                request.Proxy = proxy;
              }
              catch { /* do nothing */ }
            }

            if (proxySettings.UseDefaultCredentials)
              request.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
          }
        }
        else
        {
          request.Accept = "application/json";
        }

        if (ajaxSettings != null && ajaxSettings.Async)
        {
          var tcs = new TaskCompletionSource<object>();
          try
          {
            request.BeginGetResponse(iar =>
            {
              HttpWebResponse response = null;
              try
              {
                response = (HttpWebResponse)request.EndGetResponse(iar);
                var resultObject = GetResultFromResponse(response);
                tcs.SetResult(resultObject);
              }
              catch (Exception exc) { tcs.SetException(exc); }
              finally { if (response != null) response.Close(); }
            }, null);
          }
          catch (Exception exc) { tcs.SetException(exc); }

          return new DeferredInstance(this.Engine.Object.InstancePrototype, tcs.Task);
        }

        object result = null;
        try
        {

          var syncResponse = (HttpWebResponse)request.GetResponse();
          result = GetResultFromResponse(syncResponse);
        }
        catch (WebException e)
        {
          //The request failed -- usually a 400
          using (WebResponse response = e.Response)
          {
            HttpWebResponse httpResponse = (HttpWebResponse)response;
            var responseJson = JsonConvert.SerializeObject(httpResponse);
            result = JSONObject.Parse(this.Engine, responseJson, null);
          }
        }
        catch (Exception)
        {
          //TODO: Log this.
          throw;
        }

        return result;
      }
      finally
      {
        if (ctxt != null)
          ctxt.Dispose();
      }
    }

    private object GetResultFromResponse(HttpWebResponse response)
    {
      object resultObject = null;

      if (response.StatusCode == HttpStatusCode.OK)
      {
        using (var stream = response.GetResponseStream())
        {
          byte[] resultData = stream.ToByteArray();
          var result = Encoding.UTF8.GetString(resultData);

          bool success = false;

          //If there is no contents, return undefined.
          if (resultData.Length == 0)
          {
            resultObject = Undefined.Value;
            success = true;
          }

          //Attempt to convert the result into Json
          if (!success)
          {
            try
            {
              resultObject = JSONObject.Parse(this.Engine, result, null);
              success = true;
            }
            catch { /* Do Nothing. */ }
          }

          if (!success)
          {
            //Attempt to convert the result into Xml
            try
            {
              XDocument doc = XDocument.Parse(result);
              var jsonDocument = JsonConvert.SerializeXmlNode(doc.Root.GetXmlNode());
              resultObject = JSONObject.Parse(this.Engine, jsonDocument, null);
              success = true;
            }
            catch { /* Do Nothing. */ }
          }

          if (!success)
          {
            //If we couldn't convert as json or xml, use it as a byte array.
            resultObject = new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, resultData);
            success = true;
          }
        }
      }

      return resultObject;
    }
  }
}
