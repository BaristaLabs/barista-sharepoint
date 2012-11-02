namespace Barista.SharePoint.Services
{
  using System;
  using System.IO;
  using System.Linq;
  using System.ServiceModel;
  using System.ServiceModel.Activation;
  using System.ServiceModel.Web;
  using System.Text;
  using System.Web;
  using System.Xml.Linq;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Client.Services;
  using Newtonsoft.Json;
  using Barista.Framework;
  using Barista.Extensions;
  using Barista.SharePoint.Services;
  using Barista.SharePoint.Framework;
  using Barista.SharePoint.Library;
  using System.Net;

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
    /// <param name="code"></param>
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
      request.Code = "6*7";

      var result = client.Eval(request);

      if (WebOperationContext.Current != null)
        result.ModifyWebOperationContext(WebOperationContext.Current.OutgoingResponse);

      result.ModifyHttpResponse(HttpContext.Current.Response);

      var resultStream = new MemoryStream(result.Content);
      return resultStream;
    }
    #endregion
  }
}
