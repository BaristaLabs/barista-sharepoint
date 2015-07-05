namespace Barista.SharePoint.Services
{
  using Barista.Framework;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Client.Services;
  using System;
  using System.IO;
  using System.ServiceModel;
  using System.ServiceModel.Activation;
  using System.ServiceModel.Web;
  using System.Web;

  [SilverlightFaultBehavior]
  [BasicHttpBindingServiceMetadataExchangeEndpoint]
  [ServiceContract(Namespace = Barista.Constants.ServiceNamespace)]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  [ServiceBehavior(IncludeExceptionDetailInFaults = true, ConcurrencyMode = ConcurrencyMode.Multiple)]
  public class BaristaTestService
  {
    #region Service Operations

    /// <summary>
    /// Executes the specified script and does not return a result.
    /// </summary>
    [OperationContract(Name = "ExecRest")]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [WebGet(UriTemplate = "testme", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
    [DynamicResponseType(RestOnly = true)]
    public Stream TestMe()
    {
      if (WebOperationContext.Current == null)
        throw new InvalidOperationException("This service operation is intended to be invoked only by REST clients.");

      //If this call fails, ensure that the Service Application is started, and then do an IISReset (Yoda Says: !!Important, the last part, it is.)
      BaristaServiceClient client = new BaristaServiceClient(SPServiceContext.Current);

      var request = BrewRequest.CreateServiceApplicationRequestFromHttpRequest(HttpContext.Current.Request);
      request.ScriptEngineFactory = "Barista.SharePoint.SPBaristaJurassicScriptEngineFactory, Barista.SharePoint, Version=1.0.0.0, Culture=neutral, PublicKeyToken=a2d8064cb9226f52";
      request.Code = "6*7";

      var result = client.Eval(request);

      var setHeaders = true;
      if (WebOperationContext.Current != null)
      {
        result.ModifyOutgoingWebResponse(WebOperationContext.Current.OutgoingResponse);
        setHeaders = false;
      }

      result.ModifyHttpResponse(HttpContext.Current.Response, setHeaders);

      var resultStream = new MemoryStream(result.Content);
      return resultStream;
    }
    #endregion
  }
}
