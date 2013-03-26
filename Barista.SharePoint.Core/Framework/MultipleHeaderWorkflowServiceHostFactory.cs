namespace Barista.SharePoint.Framework
{
  using System;
  using System.Net;
  using System.Reflection;
  using System.ServiceModel;
  using System.ServiceModel.Activation;
  using System.ServiceModel.Description;
  using System.Web.Compilation;
  using System.Web.Hosting;
  using System.Workflow.ComponentModel.Compiler;

  public class MultipleHeaderWorkflowServiceHostFactory : WorkflowServiceHostFactory
  {
    private TypeProvider m_typeProvider;
    public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
    {
      return new MultipleHeaderWorkflowServiceHost(GetTypeFromString(constructorString, ServiceUtility.FilterBaseAddresses(baseAddresses)), ServiceUtility.FilterBaseAddresses(baseAddresses));
    }

    private Type GetTypeFromString(string typeString, Uri[] baseAddresses)
    {
      if (baseAddresses == null)
        throw new ArgumentNullException("baseAddresses");
      if (baseAddresses.Length == 0)
        throw new InvalidOperationException("No Base Addresses were provided.");

      Type type = Type.GetType(typeString, false);
      if (type == null)
      {
        this.m_typeProvider = new TypeProvider(null);
        string compiledCustomString;
        IDisposable disposable = null;
        try
        {
          try
          {
          }
          finally
          {
            disposable = HostingEnvironment.Impersonate();
          }
          compiledCustomString = BuildManager.GetCompiledCustomString(baseAddresses[0].AbsolutePath);
        }
        finally
        {
          if (disposable != null)
          {
            disposable.Dispose();
          }
        }
        if (string.IsNullOrEmpty(compiledCustomString))
        {
          throw new InvalidOperationException("Invalid Compiled String.");
        }
        string[] array = compiledCustomString.Split(new[]
                {
                    '|'
                });
        if (array.Length < 3)
        {
          throw new InvalidOperationException("Invalid Compiled String.");
        }
        for (int i = array.Length - 1; i > 2; i--)
        {
          Assembly assembly = Assembly.Load(array[i]);
          this.m_typeProvider.AddAssembly(assembly);
          type = assembly.GetType(typeString, false);
          if (type != null)
          {
            break;
          }
        }
        if (type == null)
        {
          Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
          foreach (Assembly assembly2 in assemblies)
          {
            this.m_typeProvider.AddAssembly(assembly2);
            type = assembly2.GetType(typeString, false);
            if (type != null)
            {
              break;
            }
          }
        }
      }
      return type;
    }
  }

  public class MultipleHeaderWorkflowServiceHost : WorkflowServiceHost
  {
    private readonly Uri[] m_baseAddresses;

    public MultipleHeaderWorkflowServiceHost(Type serviceType, params Uri[] baseAddresses)
      : base(serviceType, new Uri[0])
    {
      this.m_baseAddresses = baseAddresses;
    }

    private void CreateEndpoints()
    {
      Type contractType = ServiceUtility.GetContractType(ImplementedContracts);
      AuthenticationSchemes oneAuthScheme;
      AuthenticationSchemes allAuthenticationSchemes = ClientRequestServiceBehaviorAttribute.GetAllAuthenticationSchemes(out oneAuthScheme);

      foreach (Uri baseAddress in this.m_baseAddresses)
      {
        BasicHttpContextBinding binding = new BasicHttpContextBinding
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
        ServiceEndpoint endpoint = this.AddServiceEndpoint(contractType, binding, baseAddress);
        ServiceUtility.EnableMetadataExchange(this, baseAddress, oneAuthScheme, true);

        //TODO: Add a REST endpoint (Workflow instantiated/consumed by a REST based client = AWESOME!)
        //The following is a start -- but the problem is that the custom binding 1) needs to have behavior configured to be httpGet enabled, as well as the service host have webHttp 
        //Uri restAddress = new Uri(baseAddress, "/" + baseAddress.GetComponents(UriComponents.Path, UriFormat.Unescaped) + "/rest");

        //ContextBindingElement context = new ContextBindingElement();
        //context.ContextExchangeMechanism = ContextExchangeMechanism.HttpCookie;
        //context.ProtectionLevel = System.Net.Security.ProtectionLevel.None;

        //WebMessageEncodingBindingElement webMessageEncoding = new WebMessageEncodingBindingElement();

        //HttpTransportBindingElement httpTransport;
        //if (baseAddress.Scheme == Uri.UriSchemeHttp)
        //{
        //    httpTransport = new HttpTransportBindingElement();

        //}
        //else
        //{
        //    httpTransport = new HttpsTransportBindingElement();
        //}
        //httpTransport.AuthenticationScheme = oneAuthScheme;

        //BindingElementCollection elements = new BindingElementCollection();
        //elements.Add(context);
        //elements.Add(webMessageEncoding);
        //elements.Add(httpTransport);

        //CustomBinding restEndpointBinding = new CustomBinding(elements);
        //this.AddServiceEndpoint(contractType, restEndpointBinding, restAddress);
      }
    }

    protected override void OnOpening()
    {
      this.CreateEndpoints();

      if (this.Description.Behaviors.Find<AspNetCompatibilityRequirementsAttribute>() == null)
      {
        this.Description.Behaviors.Add(new AspNetCompatibilityRequirementsAttribute { RequirementsMode = AspNetCompatibilityRequirementsMode.Required });
      }

      //if (String.IsNullOrEmpty(this.Description.Namespace) || this.Description.Namespace == "http://tempuri.org/")
      //    this.Description.Namespace = Constants.ServiceNamespace;

      foreach (var endpoint in this.Description.Endpoints)
      {
        foreach (var operation in endpoint.Contract.Operations)
        {
          bool wasCreated = false;
          var behavior = operation.Behaviors.Find<OperationBehaviorAttribute>();
          if (behavior == null)
          {
            wasCreated = true;
            behavior = new OperationBehaviorAttribute();
          }
          behavior.Impersonation = ImpersonationOption.Allowed;

          if (wasCreated)
            operation.Behaviors.Add(behavior);
        }
      }

      if (this.Description.Behaviors.Find<ServiceDebugBehavior>() == null)
      {
        var item = new ServiceDebugBehavior
        {
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
