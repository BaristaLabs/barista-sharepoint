namespace Barista.SharePoint.Framework
{
  using System;
  using System.Data.Services;
  using System.Diagnostics;
  using System.ServiceModel;
  using System.ServiceModel.Description;
  using Microsoft.SharePoint.Client.Services;
  using System.Net;

  public class MultipleHeaderDataServiceHostFactory : DataServiceHostFactory
  {
    protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
    {
      ServiceUtility.ConfigureServiceHost();
      return new MultipleHeaderDataServiceHost(serviceType, ServiceUtility.FilterBaseAddresses(baseAddresses));
    }
  }

  public class MultipleHeaderDataServiceHost : DataServiceHost
  {
    // Fields
    private Uri[] m_baseAddresses;

    // Methods
    public MultipleHeaderDataServiceHost(Type serviceType, params Uri[] baseAddresses)
      : base(serviceType, ServiceUtility.GetBaseAddressesWithUniqueScheme(baseAddresses))
    {
      this.m_baseAddresses = baseAddresses;
    }

    private void CreateEndpoints()
    {
      Type contractType = ServiceUtility.GetContractType(base.ImplementedContracts);
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
        AddServiceEndpoint(contractType, binding, baseAddress);
      }
    }

    protected override void OnOpening()
    {
      this.CreateEndpoints();

      //if (String.IsNullOrEmpty(this.Description.Namespace) || this.Description.Namespace == "http://tempuri.org/")
      //    this.Description.Namespace = Constants.ServiceNamespace;

      if (this.Description.Behaviors.Find<ServiceDebugBehavior>() == null)
      {
        ServiceDebugBehavior item = new ServiceDebugBehavior();
        item.IncludeExceptionDetailInFaults = true;
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
  }
}
