namespace Barista.SharePoint.Services
{
  using Barista.Extensions;
  using Barista.Framework;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Administration;
  using Microsoft.SharePoint.Client.Services;
  using Microsoft.SharePoint.Utilities;
  using Newtonsoft.Json;
  using Newtonsoft.Json.Linq;
  using System;
  using System.IO;
  using System.Linq;
  using System.Net;
  using System.ServiceModel;
  using System.ServiceModel.Activation;
  using System.ServiceModel.Web;
  using System.Text;
  using System.Web;

  [SilverlightFaultBehavior]
  [BasicHttpBindingServiceMetadataExchangeEndpoint]
  [ServiceContract(Namespace = Barista.Constants.ServiceNamespace)]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  [ServiceBehavior(IncludeExceptionDetailInFaults = true,
    InstanceContextMode = InstanceContextMode.PerSession,
    ConcurrencyMode = ConcurrencyMode.Multiple)]
  [RawJsonRequestBehavior]
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

      var code = Grind();

      code = Tamp(code);

      Brew(code);
    }

    /// <summary>
    /// Overload for coffee to allow http POSTS.
    /// </summary>
    /// <param name="code"></param>
    [OperationContract(Name = "Exec")]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [WebInvoke(Method = "POST", UriTemplate = "exec", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
    [DynamicResponseType]
    public void CoffeeAuLait(Stream stream)
    {
      TakeOrder();

      var code = Grind();

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

      var code = Grind();

      code = Tamp(code);

      return Pull(code);
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
    public Stream Latte(Stream stream)
    {
      TakeOrder();

      var code = Grind();

      code = Tamp(code);

      return Pull(code);
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
    /// Grinds the beans. E.g. Attempts to retrieve the code value from the request.
    /// </summary>
    private string Grind()
    {
      string code = null;

      //If the request has a header named "c" use that first.
      var requestHeaders = WebOperationContext.Current.IncomingRequest.Headers;
      var codeHeaderKey = requestHeaders.AllKeys.Where(k => k.ToLowerInvariant() == "c").FirstOrDefault();
      if (codeHeaderKey != null)
        code = requestHeaders[codeHeaderKey];

      //If the request has a query string parameter named "c" use that.
      if (String.IsNullOrEmpty(code))
      {
        var requestQueryParameters = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;

        var codeKey = requestQueryParameters.AllKeys.Where(k => k.ToLowerInvariant() == "c").FirstOrDefault();
        if (codeKey != null)
          code = requestQueryParameters[codeKey];
      }

      //If the request contains a form variable named "c" or "code" use that.
      if (String.IsNullOrEmpty(code) && HttpContext.Current.Request.HttpMethod == "POST")
      {
        var form = HttpContext.Current.Request.Form;
        var formKey = form.AllKeys.Where(k => k.ToLowerInvariant() == "c" || k.ToLowerInvariant() == "code").FirstOrDefault();
        if (formKey != null)
          code = form[formKey];
      }

      //Otherwise, use the body of the post as code.
      if (String.IsNullOrEmpty(code) && HttpContext.Current.Request.HttpMethod == "POST")
      {
        var body = HttpContext.Current.Request.InputStream.ToByteArray();
        var bodyString = Encoding.UTF8.GetString(body);

        //Try using JSON encoding.
        try
        {
          var jsonFormBody = JObject.Parse(bodyString);
          var codeProperty = jsonFormBody.Properties().Where(p => p.Name.ToLowerInvariant() == "code" || p.Name.ToLowerInvariant() == "c").FirstOrDefault();
          if (codeProperty != null)
            code = codeProperty.Value.ToObject<string>();
        }
        catch { /* Do Nothing */ }

        //Try using form encoding
        if (String.IsNullOrEmpty(code))
        {
          try
          {
            var form = HttpUtility.ParseQueryString(bodyString);
            var formKey = form.AllKeys.Where(k => k.ToLowerInvariant() == "c" || k.ToLowerInvariant() == "code").FirstOrDefault();
            if (formKey != null)
              code = form[formKey];
          }
          catch { /* Do Nothing */ }
        }
      }

      if (String.IsNullOrEmpty(code))
        throw new ArgumentNullException("code", "Code must be specified either through the 'c' header, a 'c' query string parameter or the 'code' or 'c' form field that contains either a literal script declaration or a relative or absolute path to a script file.");

      return code;
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
          if (SPHelper.TryGetSPFileAsString(code, out codeFromfile, out isHiveFile))
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
      code = SPHelper.ReplaceTokens(code);

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
    #endregion
  }
}
