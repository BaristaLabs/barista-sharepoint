namespace Barista.SharePoint.Framework
{
  using Microsoft.SharePoint.Client.Services;

  using System;
  using System.Net;
  using System.ServiceModel;
  using System.ServiceModel.Activation;
  using System.ServiceModel.Description;

  /// <summary>
  /// Represents a WCF service host factory that determines its base address from the current request.
  /// </summary>
  public class MultipleHeaderServiceHostFactory : ServiceHostFactory
  {
    protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
    {
      ServiceUtility.ConfigureServiceHost();
      return new MultipleHeaderServiceHost(serviceType, ServiceUtility.FilterBaseAddresses(baseAddresses));
    }
  }

  public class MultipleHeaderServiceHost : ServiceHost
  {
    // Fields
    private readonly Uri[] m_baseAddresses;

    // Methods
    public MultipleHeaderServiceHost(Type serviceType, params Uri[] baseAddresses)
      : base(serviceType, new Uri[0])
    {
      this.m_baseAddresses = baseAddresses;
    }

    private void CreateEndpoints()
    {
      var contractType = ServiceUtility.GetContractType(ImplementedContracts);
      AuthenticationSchemes oneAuthScheme;
      ClientRequestServiceBehaviorAttribute.GetAllAuthenticationSchemes(out oneAuthScheme);

      foreach (var baseAddress in this.m_baseAddresses)
      {
        var binding = new BasicHttpBinding
          {
          AllowCookies = true,
          ReceiveTimeout = TimeSpan.FromHours(1),
          SendTimeout = TimeSpan.FromHours(1),
          OpenTimeout = TimeSpan.FromHours(1),
          CloseTimeout = TimeSpan.FromHours(1),
          MaxReceivedMessageSize = int.MaxValue,
          ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas
            {
            MaxArrayLength = int.MaxValue,
            MaxBytesPerRead = 2048,
            MaxDepth = int.MaxValue,
            MaxNameTableCharCount = int.MaxValue,
            MaxStringContentLength = int.MaxValue,
          }
        };

        if (baseAddress.Scheme == Uri.UriSchemeHttps)
        {
          binding.Security.Mode = BasicHttpSecurityMode.Transport;
        }
        else if (baseAddress.Scheme == Uri.UriSchemeHttp)
        {
          binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
        }
        else if ((oneAuthScheme != AuthenticationSchemes.None) && (oneAuthScheme != AuthenticationSchemes.Anonymous))
        {
          binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
        }
        else
        {
          binding.Security.Mode = BasicHttpSecurityMode.None;
        }
        binding.Security.Transport.ClientCredentialType = ServiceUtility.ClientCredentialTypeFromAuthenticationScheme(oneAuthScheme);
        AddServiceEndpoint(contractType, binding, baseAddress);

        if ((Description.ServiceType != null) && Attribute.IsDefined(Description.ServiceType, typeof(BinaryEndpointBehaviorAttribute), true))
        {
          var binaryEndpointBehaviorAttribute = Attribute.GetCustomAttribute(Description.ServiceType, typeof(BinaryEndpointBehaviorAttribute), true) as BinaryEndpointBehaviorAttribute;

          if (binaryEndpointBehaviorAttribute != null)
          {
            var binaryBinding = new NetTcpBinding
              {
                ReceiveTimeout = TimeSpan.FromHours(1),
                SendTimeout = TimeSpan.FromHours(1),
                OpenTimeout = TimeSpan.FromHours(1),
                CloseTimeout = TimeSpan.FromHours(1),
                MaxReceivedMessageSize = int.MaxValue,
              };

            binaryBinding.Security.Mode = SecurityMode.Transport;
            binaryBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

            var builder = new UriBuilder(baseAddress.ToString())
              {
                Scheme = "net.tcp",
                Port = binaryEndpointBehaviorAttribute.PortNumber
              };

            AddServiceEndpoint(contractType, binaryBinding, builder.Uri);
          }
        }

        //Add a MEX endpoint if the attribute is defined on the service type.
        if ((Description.ServiceType != null) && Attribute.IsDefined(Description.ServiceType, typeof(BasicHttpBindingServiceMetadataExchangeEndpointAttribute), true))
        {
          ServiceUtility.EnableMetadataExchange(this, baseAddress, oneAuthScheme, true);
        }
      }
    }

    protected override void OnOpening()
    {
      this.CreateEndpoints();

      //if (String.IsNullOrEmpty(this.Description.Namespace) || this.Description.Namespace == "http://tempuri.org/")
      //    this.Description.Namespace = Constants.ServiceNamespace;

      if (this.Description.Behaviors.Find<ServiceDebugBehavior>() == null)
      {
        var item = new ServiceDebugBehavior {
          IncludeExceptionDetailInFaults = true
        };
        this.Description.Behaviors.Add(item);
      }

      if (this.Description.Behaviors.Find<ServiceAuthorizationBehavior>() == null)
      {
        var item = new ServiceAuthorizationBehavior
        {
          ImpersonateCallerForAllOperations = true
        };
        this.Description.Behaviors.Add(item);
      }
      else
      {
        this.Description.Behaviors.Find<ServiceAuthorizationBehavior>().ImpersonateCallerForAllOperations = true;
      }

      if (this.Description.Behaviors.Find<ServiceMetadataBehavior>() == null)
      {
        var item = new ServiceMetadataBehavior
          {
            MetadataExporter = {PolicyVersion = PolicyVersion.Policy15}
          };
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