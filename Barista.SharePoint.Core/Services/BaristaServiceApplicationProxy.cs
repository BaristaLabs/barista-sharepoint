namespace Barista.SharePoint.Services
{
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Administration;
  using Microsoft.SharePoint.Utilities;
  using System;
  using System.Runtime.InteropServices;
  using System.ServiceModel;
  using System.ServiceModel.Channels;
  using System.ServiceModel.Configuration;

  [Guid("45ABD9F3-051B-48B6-958D-E5505848FABC")]
  public class BaristaServiceApplicationProxy : SPIisWebServiceApplicationProxy
  {
    private ChannelFactory<IBaristaServiceApplication> m_channelFactory;
    private readonly object m_channelFactoryLock = new object();
    private string m_endpointConfigName;

    [Persisted]
// ReSharper disable FieldCanBeMadeReadOnly.Local
    private SPServiceLoadBalancer m_loadBalancer;
// ReSharper restore FieldCanBeMadeReadOnly.Local

    public BaristaServiceApplicationProxy()
    {
    }

    public BaristaServiceApplicationProxy(string name, BaristaServiceProxy proxy, Uri serviceAddress)
      : base(name, proxy, serviceAddress)
    {
      // create instance of a new load balancer
      m_loadBalancer = new SPRoundRobinServiceLoadBalancer(serviceAddress);
    }

    #region Service Application Proxy Plumbing
    private ChannelFactory<T> CreateChannelFactory<T>(string endpointConfigName)
    {
      // open the client.config
      var clientConfigPath = SPUtility.GetGenericSetupPath(@"WebClients\Barista");
      var clientConfig = OpenClientConfiguration(clientConfigPath);
      var factory = new ConfigurationChannelFactory<T>(endpointConfigName, clientConfig, null);
      
      // configure the channel factory
      factory.ConfigureCredentials(SPServiceAuthenticationMode.Claims);
      
      return factory;
    }

    internal static void Invoke(SPServiceContext serviceContext, Action<BaristaServiceApplicationProxy> codeBlock)
    {
      if (serviceContext == null)
        throw new ArgumentNullException("serviceContext");

      // get service app proxy from the context
      var proxy = (BaristaServiceApplicationProxy)serviceContext.GetDefaultProxy(typeof(BaristaServiceApplicationProxy));
      
      if (proxy == null)
        throw new InvalidOperationException("Unable to obtain object reference to Barista service proxy.");

      // run the code block on the proxy
      using (new SPServiceContextScope(serviceContext))
      {
        codeBlock(proxy);
      }
    }

    private static string GetEndpointConfigName(Uri address)
    {
      string configName;

      // get the the config name for the provided address
      if (address.Scheme == Uri.UriSchemeHttp)
        configName = "http";
      else if (address.Scheme == Uri.UriSchemeHttps)
        configName = "https";
      else
        throw new NotSupportedException("Unsupported endpoint address.");

      return configName;
    }

    private IBaristaServiceApplication GetChannel(Uri address)
    {
      var endpointConfig = GetEndpointConfigName(address);

      // if there's a cached channel, use that
      if ((m_channelFactory == null) || (endpointConfig != m_endpointConfigName))
      {
        lock (m_channelFactoryLock)
        {
          // create a channel factory using the endpoint name
          m_channelFactory = CreateChannelFactory<IBaristaServiceApplication>(endpointConfig);
          
          // cache the created channel
          m_endpointConfigName = endpointConfig;
        }
      }

      // create a channel that acts as the logged on user when authenticating with the service
      var channel = m_channelFactory.CreateChannelActingAsLoggedOnUser(new EndpointAddress(address));
      return channel;
    }

    private void ExecuteOnChannel(Action<IBaristaServiceApplication> codeBlock)
    {
      var loadBalancerContext = m_loadBalancer.BeginOperation();
      
      try
      {
        // get a channel to the service app endpoint
        
// ReSharper disable SuspiciousTypeConversion.Global
        var channel = (IChannel)GetChannel(loadBalancerContext.EndpointAddress);
// ReSharper restore SuspiciousTypeConversion.Global

        try
        {
          // execute the code block
// ReSharper disable SuspiciousTypeConversion.Global
          codeBlock((IBaristaServiceApplication)channel);
// ReSharper restore SuspiciousTypeConversion.Global
          channel.Close();
        }
        catch (TimeoutException)
        {
          loadBalancerContext.Status = SPServiceLoadBalancerStatus.Failed;
          throw;
        }
        catch (EndpointNotFoundException)
        {
          loadBalancerContext.Status = SPServiceLoadBalancerStatus.Failed;
          throw;
        }
        finally
        {
          if (channel.State != CommunicationState.Closed)
            channel.Abort();
        }
      }
      finally
      {
        loadBalancerContext.EndOperation();
      }
    }
    #endregion

    // assign the custom app proxy type name
    public override string TypeName
    {
      get { return "Barista Service Application Proxy"; }
    }

    // provisioning the app proxy requires creating a new load balancer
    public override void Provision()
    {
      m_loadBalancer.Provision();
      base.Provision();
      this.Update();
    }

    // unprovisioning the app proxy requires deleting the load balancer
    public override void Unprovision(bool deleteData)
    {
      m_loadBalancer.Unprovision();
      base.Unprovision(deleteData);
      this.Update();
    }

    #region Service Application Methods
    public BrewResponse Eval(BrewRequest request)
    {
      BrewResponse result = null;

      // execute the call against the service app
      ExecuteOnChannel(channel => result = channel.Eval(request));

      return result;
    }

    public void Exec(BrewRequest request)
    {
      // execute the call against the service app
      ExecuteOnChannel(channel => channel.Exec(request));
    }
    #endregion
  }
}
