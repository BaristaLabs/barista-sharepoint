namespace Barista.SharePoint.Services
{
  using System.Linq;
  using Barista.Extensions;
  using Barista.Framework;
  using Barista.SharePoint.Extensions;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Client.Services;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.ServiceModel;
  using System.ServiceModel.Activation;
  using System.ServiceModel.Web;
  using System.Text;
  using System.Web;

  /// <summary>
  /// Represents the Barista WCF service endpoint that responds to WCF based requests.
  /// </summary>
  [BasicHttpBindingServiceMetadataExchangeEndpoint]
  [ServiceContract(Namespace = Barista.Constants.ServiceNamespace)]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  [ServiceBehavior(IncludeExceptionDetailInFaults = true,
    InstanceContextMode = InstanceContextMode.PerSession,
    ConcurrencyMode = ConcurrencyMode.Multiple)]
  public class BaristaWcfService
  {
    #region Service Operations
    [OperationContract(Name = "Exec")]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    public void Coffee(string code)
    {
      if (code.IsNullOrWhiteSpace())
        throw new ArgumentNullException("code");

      TakeOrder();

      string codePath;
      code = Tamp(code, out codePath);

      Brew(code, codePath, null, null, null);
    }

    [OperationContract(Name = "ExecWithBody")]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    public void CoffeeAuLait(string code, byte[] body)
    {
      if (code.IsNullOrWhiteSpace())
        throw new ArgumentNullException("code");

      TakeOrder();

      string codePath;
      code = Tamp(code, out codePath);

      Brew(code, codePath, null, body, null);
    }

    [OperationContract(Name = "ExecWithStringBody")]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    public void CoffeeAuLait(string code, string body)
    {
      if (code.IsNullOrWhiteSpace())
        throw new ArgumentNullException("code");

      TakeOrder();

      string codePath;
      code = Tamp(code, out codePath);

      var bodyBytes = Encoding.UTF8.GetBytes(body);
      Brew(code, codePath, null, bodyBytes, null);
    }

    [OperationContract(Name = "ExecWithQueryString")]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    public void CoffeeAuLait(string code, IDictionary<string, string> queryString)
    {
      if (code.IsNullOrWhiteSpace())
        throw new ArgumentNullException("code");

      TakeOrder();

      string codePath;
      code = Tamp(code, out codePath);

      Brew(code, codePath, queryString, null, null);
    }

    [OperationContract(Name = "ExecFull")]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    public void RedEye(string code, IDictionary<string, string> queryString, byte[] body, IDictionary<string, PostedFile> files)
    {
      if (code.IsNullOrWhiteSpace())
        throw new ArgumentNullException("code");

      TakeOrder();

      string codePath;
      code = Tamp(code, out codePath);

      Brew(code, codePath, queryString, body, files);
    }

    [OperationContract(Name = "Eval")]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    public Stream Espresso(string code)
    {
      if (code.IsNullOrWhiteSpace())
        throw new ArgumentNullException("code");

      TakeOrder();

      string codePath;
      code = Tamp(code, out codePath);

      return Pull(code, codePath, null, null, null);
    }

    [OperationContract(Name = "EvalWithBody")]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    public Stream Latte(string code, byte[]  body)
    {
      if (code.IsNullOrWhiteSpace())
        throw new ArgumentNullException("code");

      TakeOrder();

      string codePath;
      code = Tamp(code, out codePath);

      return Pull(code, codePath, null, body, null);
    }

    [OperationContract(Name = "EvalWithStringBody")]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    public string Cappuccino(string code, string body)
    {
      if (code.IsNullOrWhiteSpace())
        throw new ArgumentNullException("code");

      TakeOrder();

      string codePath;
      code = Tamp(code, out codePath);

      var bodyBytes = Encoding.UTF8.GetBytes(body);
      var result = Pull(code, codePath, null, bodyBytes, null);
      var resultBytes = result.ToByteArray();
      return Encoding.UTF8.GetString(resultBytes);
    }

    [OperationContract(Name = "EvalWithQueryString")]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    public Stream Mocha(string code, IDictionary<string, string> queryString)
    {
      if (code.IsNullOrWhiteSpace())
        throw new ArgumentNullException("code");

      TakeOrder();

      string codePath;
      code = Tamp(code, out codePath);

      return Pull(code, codePath, queryString, null, null);
    }

    [OperationContract(Name = "EvalWithQueryStringStringResult")]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    public string WhiteChocolateMocha(string code, IDictionary<string, string> queryString)
    {
      if (code.IsNullOrWhiteSpace())
        throw new ArgumentNullException("code");

      TakeOrder();

      string codePath;
      code = Tamp(code, out codePath);

      var result = Pull(code, codePath, queryString, null, null);
      var resultBytes = result.ToByteArray();
      return Encoding.UTF8.GetString(resultBytes);
    }

    [OperationContract(Name = "EvalFull")]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    public Stream CaramelMacchiato(string code, IDictionary<string, string> queryString, byte[] body, IDictionary<string, PostedFile> files)
    {
      if (code.IsNullOrWhiteSpace())
        throw new ArgumentNullException("code");

      TakeOrder();

      string codePath;
      code = Tamp(code, out codePath);

      return Pull(code, codePath, queryString, body, files);
    }
    #endregion

    #region Private Methods

    /// <summary>
    /// Takes an order. E.g. Asserts that the current web is valid, the user has read permissions on the current web, and the current web is a trusted location.
    /// </summary>
    private static void TakeOrder()
    {
      if (SPContext.Current.Web == null)
        throw new InvalidOperationException("Cannot execute Barista: Barista must execute within the context of a SharePoint Web.");

      if (SPContext.Current.Web.DoesUserHavePermissions(SPBasePermissions.Open) == false)
        throw new InvalidOperationException("Cannot execute Barista: Access Denied - The current user does not have access to the current web.");

      BaristaHelper.EnsureExecutionInTrustedLocation();
    }

    /// <summary>
    /// Tamps the ground coffee. E.g. Parses the code and makes it ready to be executed (brewed).
    /// </summary>
    /// <param name="code"></param>
    /// <param name="scriptPath"></param>
    /// <returns></returns>
    private static string Tamp(string code, out string scriptPath)
    {
      scriptPath = String.Empty;

      //If the code looks like a uri, attempt to retrieve a code file and use the contents of that file as the code.
      if (Uri.IsWellFormedUriString(code, UriKind.RelativeOrAbsolute))
      {
        Uri codeUri;
        if (Uri.TryCreate(code, UriKind.RelativeOrAbsolute, out codeUri))
        {
          string scriptFilePath;
          bool isHiveFile;
          String codeFromfile;
          if (SPHelper.TryGetSPFileAsString(code, out scriptFilePath, out codeFromfile, out isHiveFile))
          {
            if (isHiveFile == false)
            {
              var lockDownMode = SPContext.Current.Web.GetProperty("BaristaLockdownMode") as string;
              if (String.IsNullOrEmpty(lockDownMode) == false && lockDownMode.ToLowerInvariant() == "BaristaContentLibraryOnly")
              {
                //TODO: implement this.
              }
            }

            scriptPath = scriptFilePath;
            code = codeFromfile;
          }
        }
      }

      //Replace any tokens in the code.
      code = SPHelper.ReplaceTokens(SPContext.Current, code);

      return code;
    }

    /// <summary>
    /// Brews a cup of coffee. E.g. Executes the specified script.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="codePath"></param>
    /// <param name="queryString"></param>
    /// <param name="body"></param>
    /// <param name="files"></param>
    private static void Brew(string code, string codePath, IDictionary<string, string> queryString, byte[] body, IDictionary<string, PostedFile> files)
    {
      var client = new BaristaServiceClient(SPServiceContext.Current);

      var request = BrewRequest.CreateServiceApplicationRequestFromHttpRequest(HttpContext.Current.Request);
      request.Code = code;
      request.CodePath = codePath;

      if (queryString != null && queryString.Any())
        request.QueryString = queryString;

      if (body != null && body.Length > 0)
        request.Body = body;

      if (files != null && files.Count > 0)
        request.Files = files;

      if (String.IsNullOrEmpty(request.InstanceInitializationCode) == false)
      {
        string instanceInitializationCodePath;
        request.InstanceInitializationCode = Tamp(request.InstanceInitializationCode, out instanceInitializationCodePath);
        request.InstanceInitializationCodePath = instanceInitializationCodePath;
      }

      request.SetExtendedPropertiesFromCurrentSPContext();
      
      client.Exec(request);
    }

    /// <summary>
    /// Pulls a shot of espresso. E.g. Executes the specified script and sets the appropriate values on the response object.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="codePath"></param>
    /// <param name="queryString"></param>
    /// <param name="body"></param>
    /// <param name="files"></param>
    /// <returns></returns>
    private static Stream Pull(string code, string codePath, IDictionary<string, string> queryString, byte[] body, IDictionary<string, PostedFile> files)
    {
      var client = new BaristaServiceClient(SPServiceContext.Current);

      var request = BrewRequest.CreateServiceApplicationRequestFromHttpRequest(HttpContext.Current.Request);
      request.Code = code;
      request.CodePath = codePath;

      if (queryString != null && queryString.Any())
        request.QueryString = queryString;

      if (body != null && body.Length > 0)
        request.Body = body;

      if (files != null && files.Count > 0)
        request.Files = files;

      if (String.IsNullOrEmpty(request.InstanceInitializationCode) == false)
      {
        string instanceInitializationCodePath;
        request.InstanceInitializationCode = Tamp(request.InstanceInitializationCode, out instanceInitializationCodePath);
        request.InstanceInitializationCodePath = instanceInitializationCodePath;
      }

      request.SetExtendedPropertiesFromCurrentSPContext();

      var result = client.Eval(request);

      var setHeaders = true;
      if (WebOperationContext.Current != null)
      {
        result.ModifyWebOperationContext(WebOperationContext.Current.OutgoingResponse);
        setHeaders = false;
      }

      result.ModifyHttpResponse(HttpContext.Current.Response, setHeaders);

      var resultStream = new MemoryStream(result.Content);
      return resultStream;
    }
    #endregion
  }
}
