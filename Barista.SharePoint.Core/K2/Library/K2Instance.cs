namespace Barista.SharePoint.K2.Library
{
  using System;
  using System.Security.Principal;
  using System.ServiceModel;
  using Jurassic;
  using Jurassic.Library;
  using Barista.SharePoint.K2Services;

  public class K2Instance : ObjectInstance
  {
    private string m_servicesBaseUrl = String.Empty;
    private readonly BasicHttpBinding m_k2Binding;

    public K2Instance(ScriptEngine engine)
      : base(engine)
    {
      this.PopulateFields();
      this.PopulateFunctions();

      m_k2Binding = new BasicHttpBinding
        {
        AllowCookies = true,
        ReceiveTimeout = TimeSpan.FromMinutes(5),
        SendTimeout = TimeSpan.FromMinutes(5),
        OpenTimeout = TimeSpan.FromMinutes(5),
        CloseTimeout = TimeSpan.FromMinutes(5),
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

      m_k2Binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
      m_k2Binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
    }

    [JSProperty(Name="servicesBaseUrl")]
    public string ServicesBaseUrl
    {
      get { return m_servicesBaseUrl; }
      set { m_servicesBaseUrl = value; }
    }

    [JSFunction(Name = "openProcessInstance")]
    public ProcessInstanceInstance OpenProcessInstance(string processInstanceId, bool includeDataFields, bool includeXmlFields)
    {
      using (var processNavigationClient = GetProcessNavigationServiceClient())
      {
        var processInstance = processNavigationClient.OpenProcessInstance(processInstanceId, includeDataFields, includeXmlFields);
        return new ProcessInstanceInstance(this.Engine.Object.InstancePrototype, processInstance);
      }
    }

    [JSFunction(Name="openWorklist")]
    public ArrayInstance OpenWorklist(bool includeActivityData, bool includeActivityXml, bool includeProcessData, bool includeProcessXml)
    {
      using (var workListServiceClient = GetWorkListServiceClient())
      {
        var workListItems = workListServiceClient.OpenWorklist(includeActivityData, includeActivityXml, includeProcessData, includeProcessXml);

        var instance = this.Engine.Array.Construct();
        foreach (var workListItem in workListItems)
        {
          ArrayInstance.Push(instance, new WorklistItemInstance(this.Engine.Object.InstancePrototype, workListItem));
        }

        return instance;
      }
    }

    private ProcessNavigationServiceClient GetProcessNavigationServiceClient()
    {
      var processNavigationClient = new K2Services.ProcessNavigationServiceClient(m_k2Binding,
                                                                                  new EndpointAddress(
                                                                                    m_servicesBaseUrl + "/Process"));
      if (processNavigationClient.ChannelFactory.Credentials != null)
        processNavigationClient.ChannelFactory.Credentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
      
      if (processNavigationClient.ClientCredentials != null)
        processNavigationClient.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;

      return processNavigationClient;
    }

    private WorklistNavigationServiceClient GetWorkListServiceClient()
    {
      var workListServiceClient = new K2Services.WorklistNavigationServiceClient(m_k2Binding, new EndpointAddress(m_servicesBaseUrl + "/Worklist"));
      
      if (workListServiceClient.ChannelFactory.Credentials != null)
        workListServiceClient.ChannelFactory.Credentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
      
      if (workListServiceClient.ClientCredentials != null)
        workListServiceClient.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;

      return workListServiceClient;
    }
  }
}
