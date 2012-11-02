namespace Barista.SharePoint.Services
{
  using Barista.Framework;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Administration;
  using Microsoft.SharePoint.Client.Services;
  using Microsoft.SharePoint.Utilities;
  using Newtonsoft.Json.Linq;
  using System;
  using System.IO;
  using System.Linq;
  using System.Net;
  using System.ServiceModel;
  using System.ServiceModel.Activation;
  using System.ServiceModel.Web;
  using System.Web;

  [SilverlightFaultBehavior]
  [BasicHttpBindingServiceMetadataExchangeEndpoint]
  [ServiceContract(Namespace = Barista.Constants.ServiceNamespace)]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  [ServiceBehavior(IncludeExceptionDetailInFaults = true,
    InstanceContextMode = InstanceContextMode.PerSession,
    ConcurrencyMode = ConcurrencyMode.Multiple)]
  public class BaristaWebService
  {
    #region Service Operations

    /// <summary>
    /// Executes the specified script and does not return a result.
    /// </summary>
    /// <param name="code"></param>
    [OperationContract(Name = "ExecRest")]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [WebGet(UriTemplate = "exec", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
    [DynamicResponseType(RestOnly = true)]
    public void Coffee()
    {
      if (WebOperationContext.Current == null)
        throw new InvalidOperationException("This service operation is intended to be invoked only by REST clients.");

      TakeOrder();

      string code;
      string xml;

      PurifyWater(out code, out xml);

      CoffeeAuLait(code, xml);
    }

    /// <summary>
    /// Overload for coffee to allow http POSTS.
    /// </summary>
    /// <param name="code"></param>
    [OperationContract(Name = "Exec")]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [WebInvoke(Method = "POST", UriTemplate = "exec", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
    [DynamicResponseType]
    public void CoffeeAuLait(string code, string xml)
    {
      TakeOrder();

      Grind(ref code, ref xml);

      code = Tamp(code);

      Brew(code);
    }

    [OperationContract(Name = "Exec2")]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [WebInvoke(Method = "POST", UriTemplate = "exec2?c={code}&x={xml}", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
    [DynamicResponseType]
    public void CoffeeAuLait2(string code, string xml, Stream stream)
    {
      TakeOrder();

      Grind(ref code, ref xml);

      code = Tamp(code);

      Brew(code);
    }

    /// <summary>
    /// Evaluates the specified script.
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [OperationContract(Name = "EvalRest")]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [WebGet(UriTemplate = "eval", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
    [DynamicResponseType(RestOnly = true)]
    public Stream Espresso()
    {
      if (WebOperationContext.Current == null)
        throw new InvalidOperationException("This service operation is intended to be invoked only by REST clients.");

      TakeOrder();

      string code;
      string xml;

      PurifyWater(out code, out xml);

      return Latte(code, xml);
    }

    /// <summary>
    /// Expresso overload to support having code contained in the body of a http POST.
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [OperationContract(Name = "Eval")]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [WebInvoke(Method = "POST", UriTemplate = "eval", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
    [DynamicResponseType]
    public Stream Latte(string code, string xml)
    {
      TakeOrder();

      Grind(ref code, ref xml);

      code = Tamp(code);

      return Pull(code);
    }

    /// <summary>
    /// Expresso overload to support having code contained in the body of a http POST.
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [OperationContract(Name = "Eval2")]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [WebInvoke(Method = "POST", UriTemplate = "eval2?c={code}&x={xml}", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
    [DynamicResponseType]
    public Stream Latte2(string code, string xml, Stream stream)
    {
      TakeOrder();

      Grind(ref code, ref xml);

      code = Tamp(code);

      return Pull(code);
    }

    /// <summary>
    /// Supports multi-part uploads. Does not return a result.
    /// </summary>
    /// <param name="scriptUrl"></param>
    /// <param name="xml"></param>
    /// <returns></returns>
    [OperationContract(Name = "ExecUpload")]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [WebInvoke(Method = "POST", UriTemplate = "execUpload", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
    [DynamicResponseType(RestOnly = true)]
    public void IcedCoffee(Stream stream)
    {
      if (WebOperationContext.Current == null)
        throw new InvalidOperationException("This service operation is intended to be invoked only by REST clients.");

      TakeOrder();

      MultipartContentProcessor parser = new MultipartContentProcessor(stream);

      if (parser.IsValid == false)
      {
        WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.UnsupportedMediaType;
        throw new WebException("The posted file was not recognised.");
      }

      string code = null;
      string xml = null;

      Grind(ref code, ref xml);

      code = Tamp(code);

      Brew(code);
    }

    /// <summary>
    /// Supports multi-part uploads. Returns a result.
    /// </summary>
    /// <param name="scriptUrl"></param>
    /// <param name="xml"></param>
    /// <returns></returns>
    [OperationContract(Name = "EvalUpload")]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [WebInvoke(Method = "POST", UriTemplate = "evalUpload", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
    [DynamicResponseType(RestOnly = true)]
    public Stream Frappuccino(Stream stream)
    {
      if (WebOperationContext.Current == null)
        throw new InvalidOperationException("This service operation is intended to be invoked only by REST clients.");

      TakeOrder();

      MultipartContentProcessor parser = new MultipartContentProcessor(stream);

      if (parser.IsValid == false)
      {
        WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.UnsupportedMediaType;
        throw new WebException("The posted file was not recognised.");
      }

      string code = null;
      string xml = null;

      Grind(ref code, ref xml);

      code = Tamp(code);

      //If a 'redirect' part is contained in the request, redirect to that location, otherwise, return the result.
      if (String.IsNullOrEmpty(parser.Redirect))
      {
        return Pull(code);
      }

      throw new NotImplementedException();

      //var evalResult = Pull(code);
      //var stringResult = JSONObject.Stringify(engine, evalResult);

      //WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Found;
      //WebOperationContext.Current.OutgoingResponse.Location = parser.Redirect.Replace("%s", HttpUtility.UrlEncode(stringResult.Replace("\r", "").Replace("\n", "")));
      //return null;
    }
    #endregion

    #region Private Methods

    /// <summary>
    /// Takes an order. E.g. Asserts that the current web is valid, the user has read permissions on the current web, and the current web is a trusted location.
    /// </summary>
    private void TakeOrder()
    {
      if (SPContext.Current.Web == null)
        throw new InvalidOperationException("Cannot execute Barista: Barista must execute within the context of a SharePoint Web.");

      if (SPContext.Current.Web.DoesUserHavePermissions(SPBasePermissions.Open) == false)
        throw new InvalidOperationException("Cannot execute Barista: Access Denied - The current user does not have access to the current web.");

      EnsureExecutionInTrustedLocation();
    }

    /// <summary>
    /// Purifies the water. E.g. For REST Get requests, asserts that the proper query string parameters are present.
    /// </summary>
    private void PurifyWater(out string code, out string xml)
    {
      code = String.Empty;
      xml = String.Empty;

      var requestQueryParameters = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;

      var codeKey = requestQueryParameters.AllKeys.Where(k => k.ToLowerInvariant() == "c").FirstOrDefault();
      if (codeKey != null)
        code = requestQueryParameters[codeKey];

      var xmlKey = requestQueryParameters.AllKeys.Where(k => k.ToLowerInvariant() == "x").FirstOrDefault();
      if (xmlKey != null)
        xml = requestQueryParameters[xmlKey];

      if (String.IsNullOrEmpty(code))
        throw new InvalidOperationException("A query parameter named 'c' must be specified that contains either a literal script declaration or a relative or absolute path to a script file.");
    }
    /// <summary>
    /// Grinds the beans. E.g. Validates the code and xml parameters, if not present, attempts to retrieve them from the query string parameter.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="xml"></param>
    private void Grind(ref string code, ref string xml)
    {
      if (String.IsNullOrEmpty(code) && WebOperationContext.Current != null)
      {
        var requestQueryParameters = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;

        var codeKey = requestQueryParameters.AllKeys.Where(k => k.ToLowerInvariant() == "c").FirstOrDefault();
        if (codeKey != null)
          code = requestQueryParameters[codeKey];
      }

      if (String.IsNullOrEmpty(code))
        throw new ArgumentNullException("code", "Code must be specified either through the 'c' query string parameter or the 'code' operation parameter that contains either a literal script declaration or a relative or absolute path to a script file.");


      if (String.IsNullOrEmpty(xml) && WebOperationContext.Current != null)
      {
        var requestQueryParameters = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;

        var xmlKey = requestQueryParameters.AllKeys.Where(k => k.ToLowerInvariant() == "x").FirstOrDefault();
        if (xmlKey != null)
          xml = requestQueryParameters[xmlKey];
      }
    }

    /// <summary>
    /// Tamps the ground coffee. E.g. Parses the code and makes it ready to be executed (brewed).
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    private string Tamp(string code)
    {
      //If the code looks like a uri, attempt to retrieve a code file and use the contents of that file as the code.
      if (Uri.IsWellFormedUriString(code, UriKind.RelativeOrAbsolute))
      {
        Uri codeUri;
        if (Uri.TryCreate(code, UriKind.RelativeOrAbsolute, out codeUri))
        {

          bool isHiveFile;
          String codeFromfile;
          if (TryGetSPFileAsString(code, out codeFromfile, out isHiveFile))
          {
            if (isHiveFile == false)
            {
              string lockDownMode = SPContext.Current.Web.GetProperty("BaristaLockdownMode") as string;
              if (String.IsNullOrEmpty(lockDownMode) == false && lockDownMode.ToLowerInvariant() == "BaristaContentLibraryOnly")
              {
                //TODO: implement this.
              }
            }

            code = codeFromfile;
          }
        }
      }

      //Replace any tokens in the code.
      code = ReplaceTokens(code);

      return code;
    }

    /// <summary>
    /// Brews a cup of coffee. E.g. Executes the specified script.
    /// </summary>
    /// <param name="engine"></param>
    /// <param name="code"></param>
    private void Brew(string code)
    {
      BaristaServiceClient client = new BaristaServiceClient(SPServiceContext.Current);

      var request = BrewRequest.CreateServiceApplicationRequestFromHttpRequest(HttpContext.Current.Request);
      request.Code = code;

      SetExtendedPropertiesFromCurrentSPContext(request);
      
      client.Exec(request);
    }

    /// <summary>
    /// Checks the current context against the trusted locations.
    /// </summary>
    private void EnsureExecutionInTrustedLocation()
    {
      var currentUri = new Uri(SPContext.Current.Web.Url);

      //CA is always trusted.
      if (SPAdministrationWebApplication.Local.AlternateUrls.Any( u => u != null && u.Uri != null && u.Uri.IsBaseOf(currentUri)))
        return;

      bool trusted = false;
      var trustedLocations = Utilities.GetFarmKeyValue("BaristaTrustedLocations");

      if (trustedLocations != null && trustedLocations != string.Empty)
      {
        var trustedLocationsCollection = JArray.Parse(trustedLocations);
        foreach (var trustedLocation in trustedLocationsCollection.OfType<JObject>())
        {
          Uri trustedLocationUrl = new Uri(trustedLocation["Url"].ToString(), UriKind.Absolute);
          bool trustChildren = trustedLocation["TrustChildren"].ToObject<Boolean>();

          if (trustChildren == true)
          {
            if (trustedLocationUrl.IsBaseOf(currentUri))
            {
              trusted = true;
              break;
            }
          }
          else
          {
            if (trustedLocationUrl == currentUri)
            {
              trusted = true;
              break;
            }
          }
        }
      }

      if (trusted == false)
        throw new InvalidOperationException("Cannot execute Barista: Current location is not trusted. Add the current location to the trusted Urls in the management section of the Barista service application.");
    }

    /// <summary>
    /// Pulls a shot of espresso. E.g. Executes the specified script and sets the appropriate values on the response object.
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    private Stream Pull(string code)
    {
      BaristaServiceClient client = new BaristaServiceClient(SPServiceContext.Current);

      var request = BrewRequest.CreateServiceApplicationRequestFromHttpRequest(HttpContext.Current.Request);
      request.Code = code;

      SetExtendedPropertiesFromCurrentSPContext(request);

      var result = client.Eval(request);

      if (WebOperationContext.Current != null)
      {
        result.ModifyWebOperationContext(WebOperationContext.Current.OutgoingResponse);
      }

      result.ModifyHttpResponse(HttpContext.Current.Response);

      var resultStream = new MemoryStream(result.Content);
      return resultStream;
    }

    public void SetExtendedPropertiesFromCurrentSPContext(BrewRequest request)
    {
      if (SPContext.Current != null)
      {
        if (SPContext.Current.Site != null)
          request.ExtendedProperties.Add("SPSiteId", SPContext.Current.Site.ID.ToString());

        if (SPContext.Current.Web != null)
          request.ExtendedProperties.Add("SPWebId", SPContext.Current.Web.ID.ToString());

        if (SPContext.Current.ListId != null && SPContext.Current.ListId != Guid.Empty)
          request.ExtendedProperties.Add("SPListId", SPContext.Current.ListId.ToString());

        if (String.IsNullOrEmpty(SPContext.Current.ListItemServerRelativeUrl) == false)
          request.ExtendedProperties.Add("SPListItemUrl", SPContext.Current.ListItemServerRelativeUrl);

        if (SPContext.Current.ViewContext != null && SPContext.Current.ViewContext.ViewId != Guid.Empty)
          request.ExtendedProperties.Add("SPViewId", SPContext.Current.ViewContext.ViewId.ToString());

        if (SPContext.Current.File != null && SPContext.Current.File.UniqueId != Guid.Empty)
          request.ExtendedProperties.Add("SPFileId", SPContext.Current.File.UniqueId.ToString());
      }
    }

    /// <summary>
    /// Replaces SharePoint (and custom) tokens in the specified string with their corresponding values.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public string ReplaceTokens(string text)
    {
      if (SPContext.Current == null)
        return text;

      string result = text;
      if (SPContext.Current.List != null)
        result = result.Replace("{ListUrl}", SPContext.Current.Web.Url + "/" + SPContext.Current.List.RootFolder.Url);

      if (SPContext.Current.Web != null)
      {
        result = result.Replace("{WebUrl}", SPContext.Current.Web.Url);
        result = result.Replace("~site", SPContext.Current.Web.ServerRelativeUrl);
      }

      if (SPContext.Current.Site != null)
      {
        result = result.Replace("{SiteUrl}", SPContext.Current.Site.Url);
        result = result.Replace("~sitecollection", SPContext.Current.Site.ServerRelativeUrl);
      }

      return result;
    }

    /// <summary>
    /// Attemps to create an absolute Uri based on the specified string. If the passed string is relative, uses the current web's uri as the base uri.
    /// </summary>
    /// <param name="uriString"></param>
    /// <param name="uri"></param>
    /// <returns></returns>
    public bool TryCreateWebAbsoluteUri(string uriString, out Uri uri)
    {
      if (Uri.IsWellFormedUriString(uriString, UriKind.RelativeOrAbsolute) == false)
      {
        uri = null;
        return false;
      }

      Uri finalUri;
      if (Uri.TryCreate(uriString, UriKind.Absolute, out finalUri) == false)
      {
        if (Uri.TryCreate(new Uri(SPContext.Current.Web.Url), uriString, out finalUri) == false)
        {
          uri = null;
          return false;
        }
      }

      uri = finalUri;
      return true;
    }

    /// <summary>
    /// Attempts to get the contents of the specified file at the specified url. Attempts to be smart about finding the location of the file.
    /// </summary>
    /// <param name="fileUrl"></param>
    /// <param name="fileContents"></param>
    /// <returns></returns>
    private bool TryGetSPFileAsString(string fileUrl, out string fileContents, out bool isHiveFile)
    {
      isHiveFile = false;

      Uri fileUri;
      if (TryCreateWebAbsoluteUri(fileUrl, out fileUri) == false)
      {
        fileContents = null;
        return false;
      }

      try
      {
        using (SPSite sourceSite = new SPSite(fileUri.ToString()))
        {
          using (SPWeb sourceWeb = sourceSite.OpenWeb())
          {
            if (sourceWeb == null)
              throw new InvalidOperationException("The specified script url is invalid.");

            fileContents = sourceWeb.GetFileAsString(fileUri.ToString());
            return true;
          }
        }
      }
      catch { /* Do Nothing... */ }

      //Attempt to get the file relative to the url referrer.
      if (HttpContext.Current.Request.UrlReferrer != null)
      {
        try
        {
          string url = SPUtility.ConcatUrls(SPUtility.GetUrlDirectory(HttpContext.Current.Request.UrlReferrer.ToString()), fileUrl);

          using (SPSite sourceSite = new SPSite(url))
          {
            using (SPWeb sourceWeb = sourceSite.OpenWeb())
            {
              if (sourceWeb == null)
                throw new InvalidOperationException("The specified script url is invalid.");

              fileContents = sourceWeb.GetFileAsString(url.ToString());
              return true;
            }
          }
        }
        catch { /* Do Nothing... */ }
      }

      //Attempt to get the file relative to the sharepoint hive.
      try
      {
        string hiveFileContents = String.Empty;
        bool hiveFileResult = false;
        SPSecurity.RunWithElevatedPrivileges(() =>
        {
          string path = HttpContext.Current.Request.MapPath(fileUrl);
          if (File.Exists(path))
          {
            hiveFileContents = File.ReadAllText(path);
            hiveFileResult = true;
          }
        });

        if (hiveFileResult == true)
        {
          fileContents = hiveFileContents;
          isHiveFile = true;
          return true;
        }
      }
      catch { /* Do Nothing... */ }

      fileContents = null;
      return false;
    }
    #endregion
  }
}
