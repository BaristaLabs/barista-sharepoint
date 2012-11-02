namespace Barista.SharePoint.Library
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Security.Principal;
  using System.ServiceModel;
  using System.Text;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using Barista.SharePoint.K2Services;

  public class K2Instance : ObjectInstance
  {
    private string m_servicesBaseUrl = String.Empty;
    private BasicHttpBinding m_k2Binding;

    private K2WorklistItemConstructor m_k2WorklistItemConstructor;

    public K2Instance(ScriptEngine engine)
      : base(engine)
    {
      this.PopulateFields();
      this.PopulateFunctions();

      m_k2WorklistItemConstructor = new K2WorklistItemConstructor(this.Engine);

      m_k2Binding = new BasicHttpBinding()
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

      m_k2Binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
      m_k2Binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
    }

    [JSProperty(Name="servicesBaseUrl")]
    public string ServicesBaseUrl
    {
      get { return m_servicesBaseUrl; }
      set { m_servicesBaseUrl = value; }
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
          ArrayInstance.Push(instance, m_k2WorklistItemConstructor.Construct(workListItem));
        }

        return instance;
      }
    }

    private WorklistNavigationServiceClient GetWorkListServiceClient()
    {
      var workListServiceClient = new K2Services.WorklistNavigationServiceClient(m_k2Binding, new EndpointAddress(m_servicesBaseUrl + "/Worklist"));
      workListServiceClient.ChannelFactory.Credentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
      workListServiceClient.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;

      return workListServiceClient;
    }
  }
}
