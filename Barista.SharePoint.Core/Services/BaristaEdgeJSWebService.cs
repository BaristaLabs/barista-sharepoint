namespace Barista.SharePoint.Services
{
    using Barista.Framework;
    using Microsoft.SharePoint.Client.Services;
    using System;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;

    /// <summary>
    /// Represents the Barista WCF service endpoint that responds to REST requests.
    /// </summary>
    [BasicHttpBindingServiceMetadataExchangeEndpoint]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true,
      InstanceContextMode = InstanceContextMode.PerCall,
      ConcurrencyMode = ConcurrencyMode.Multiple)]
    [RawJsonRequestBehavior]
    public class BaristaEdgeJSWebService : IBaristaRestService
    {
        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        [DynamicResponseType(RestOnly = true)]
        public void Exec(Stream requestBody)
        {
            ExecWild(requestBody);
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        [DynamicResponseType(RestOnly = true)]
        public void ExecWild(Stream requestBody)
        {
            BaristaServiceRequestPipeline.TakeOrder();

            string codePath;
            var code = BaristaServiceRequestPipeline.Grind(requestBody);

            code = BaristaServiceRequestPipeline.Tamp(code, out codePath);

            BaristaServiceRequestPipeline.Brew(code, codePath, requestBody, scriptEngineFactory: "Barista.SharePoint.SPBaristaEdgeJSScriptEngineFactory, Barista.SharePoint, Version=1.0.0.0, Culture=neutral, PublicKeyToken=a2d8064cb9226f52");
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        [DynamicResponseType(RestOnly = true)]
        public Message Eval(Stream requestBody)
        {
            return EvalWild(requestBody);
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        [DynamicResponseType(RestOnly = true)]
        public Message EvalWild(Stream requestBody)
        {
            BaristaServiceRequestPipeline.TakeOrder();

            string codePath;
            var code = BaristaServiceRequestPipeline.Grind(requestBody);

            code = BaristaServiceRequestPipeline.Tamp(code, out codePath);

            return BaristaServiceRequestPipeline.Pull(code, codePath, requestBody, scriptEngineFactory: "Barista.SharePoint.SPBaristaEdgeJSScriptEngineFactory, Barista.SharePoint, Version=1.0.0.0, Culture=neutral, PublicKeyToken=a2d8064cb9226f52");
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        [DynamicResponseType(RestOnly = true)]
        public Message Status()
        {
            throw new NotImplementedException();
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        [DynamicResponseType(RestOnly = true)]
        public Message ListPackages()
        {
            throw new NotImplementedException();
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        [DynamicResponseType(RestOnly = true)]
        public Message DeployPackage(Stream requestBody)
        {
            throw new NotImplementedException();
        }
    }
}
