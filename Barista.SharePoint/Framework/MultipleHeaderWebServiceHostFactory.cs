namespace Barista.SharePoint.Framework
{
  using Barista.Framework;

  using System;
  using System.Net;
  using System.ServiceModel;
  using System.ServiceModel.Activation;
  using System.ServiceModel.Channels;
  using System.ServiceModel.Description;
  using System.ServiceModel.Web;

  public class MultipleHeaderWebServiceHostFactory : WebServiceHostFactory
  {
    protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
    {
      ServiceUtility.ConfigureServiceHost();
      return new MultipleHeaderWebServiceHost(serviceType, ServiceUtility.FilterBaseAddresses(baseAddresses));
    }
  }

  public class MultipleHeaderWebServiceHost : WebServiceHost
  {
    // Fields
    private Uri[] m_baseAddresses;

    // Methods
    public MultipleHeaderWebServiceHost(Type serviceType, params Uri[] baseAddresses)
      : base(serviceType, ServiceUtility.GetBaseAddressesWithUniqueScheme(baseAddresses))
    {
      this.m_baseAddresses = baseAddresses;
    }

    private void CreateEndpoints()
    {
      AuthenticationSchemes oneAuthScheme;
      AuthenticationSchemes allAuthenticationSchemes = ClientRequestServiceBehaviorAttribute.GetAllAuthenticationSchemes(out oneAuthScheme);

      foreach (Uri baseAddress in this.m_baseAddresses)
      {
        WebHttpBinding binding = new WebHttpBinding()
        {
          AllowCookies = true,
          ReceiveTimeout = TimeSpan.FromHours(1),
          SendTimeout = TimeSpan.FromHours(1),
          OpenTimeout = TimeSpan.FromHours(1),
          CloseTimeout = TimeSpan.FromHours(1),
          MaxReceivedMessageSize = int.MaxValue,
          TransferMode = TransferMode.Streamed,
          ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas()
          {
            MaxArrayLength = int.MaxValue,
            MaxBytesPerRead = 2048,
            MaxDepth = int.MaxValue,
            MaxNameTableCharCount = int.MaxValue,
            MaxStringContentLength = int.MaxValue,
          }
        };

        if (object.ReferenceEquals(baseAddress.Scheme, Uri.UriSchemeHttps))
        {
          binding.Security.Mode = WebHttpSecurityMode.Transport;
        }
        else if (object.ReferenceEquals(baseAddress.Scheme, Uri.UriSchemeHttp))
        {
          binding.Security.Mode = WebHttpSecurityMode.TransportCredentialOnly;
        }
        else if ((oneAuthScheme != AuthenticationSchemes.None) && (oneAuthScheme != AuthenticationSchemes.Anonymous))
        {
          binding.Security.Mode = WebHttpSecurityMode.TransportCredentialOnly;
        }
        else
        {
          binding.Security.Mode = WebHttpSecurityMode.None;
        }

        binding.Security.Transport.ClientCredentialType = ServiceUtility.ClientCredentialTypeFromAuthenticationScheme(oneAuthScheme);

        //Set the content type mapper on the binding to return raw elements (for json.)
        CustomBinding cb = new CustomBinding(binding);
        var webMEBE = cb.Elements.Find<WebMessageEncodingBindingElement>();

        var rawMapper = new RawMapper();
        if (this.Description.Behaviors.Find<RawJsonRequestBehaviorAttribute>() != null)
          rawMapper.UseRawForJson = true;

        webMEBE.ContentTypeMapper = rawMapper;

        Type contractType = ServiceUtility.GetContractType(base.ImplementedContracts);
        AddServiceEndpoint(contractType, cb, baseAddress);
      }
    }

    protected override void OnOpening()
    {
      this.CreateEndpoints();

      //if (String.IsNullOrEmpty(this.Description.Namespace) || this.Description.Namespace == "http://tempuri.org/")
      //    this.Description.Namespace = Constants.ServiceNamespace;

      foreach (var endpoint in this.Description.Endpoints)
      {
        if (endpoint.Behaviors.Find<WebHttpBehavior>() != null)
        {
          endpoint.Behaviors.Remove<WebHttpBehavior>();
        }
        endpoint.Behaviors.Add(new DynamicWebHttpBehavior());
      }

      if (this.Description.Behaviors.Find<ServiceDebugBehavior>() == null)
      {
        ServiceDebugBehavior item = new ServiceDebugBehavior();
        item.IncludeExceptionDetailInFaults = true;
        this.Description.Behaviors.Add(item);
      }

      if (this.Description.Behaviors.Find<ServiceAuthorizationBehavior>() == null)
      {
        ServiceAuthorizationBehavior item = new ServiceAuthorizationBehavior();
        item.ImpersonateCallerForAllOperations = true;
        this.Description.Behaviors.Add(item);
      }

      if (this.Description.Behaviors.Find<ServiceMetadataBehavior>() == null)
      {
        ServiceMetadataBehavior item = new ServiceMetadataBehavior();
        item.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
        this.Description.Behaviors.Add(item);
      }
      else
      {
        var item = this.Description.Behaviors.Find<ServiceMetadataBehavior>();
        item.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
      }

      base.OnOpening();
    }

    private class RawMapper : WebContentTypeMapper
    {
      public bool UseRawForJson
      {
        get;
        set;
      }

      public override WebContentFormat GetMessageFormatForContentType(string contentType)
      {
        if (String.IsNullOrEmpty(contentType))
          return WebContentFormat.Default;
        else if (IsJsonContentType(contentType) && UseRawForJson == false)
          return WebContentFormat.Json;
        else if (IsXmlContentType(contentType))
          return WebContentFormat.Xml;
        else
          return WebContentFormat.Raw;
      }

      private bool IsJsonContentType(string contentType)
      {
        var contentTypeLower = contentType.Trim().ToLowerInvariant();

        if (contentTypeLower.StartsWith("application/json") ||
        contentTypeLower.StartsWith("application/x-javascript") ||
        contentTypeLower.StartsWith("text/javascript") ||
        contentTypeLower.StartsWith("text/x-javascript") ||
        contentTypeLower.StartsWith("text/x-json"))
          return true;

        return false;
      }

      private bool IsXmlContentType(string contentType)
      {
        var contentTypeLower = contentType.Trim().ToLowerInvariant();

        if (contentTypeLower.StartsWith("application/xml") ||
            contentTypeLower.StartsWith("application/soap+xml") ||
            contentTypeLower.StartsWith("text/xml"))
          return true;

        return false;
      }
    }
  }
}
