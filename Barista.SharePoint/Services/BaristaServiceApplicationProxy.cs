namespace Barista.SharePoint.Services
{
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Administration;
  using Microsoft.SharePoint.Utilities;
  using System;
  using System.Collections.Generic;
  using System.Configuration;
  using System.IO;
  using System.Linq;
  using System.Net;
  using System.Runtime.InteropServices;
  using System.Security.Principal;
  using System.ServiceModel;
  using System.ServiceModel.Channels;
  using System.ServiceModel.Configuration;
  using System.Text;

  [Guid("45ABD9F3-051B-48B6-958D-E5505848FABC")]
  public class BaristaServiceApplicationProxy : SPIisWebServiceApplicationProxy
  {
    private ChannelFactory<IBaristaServiceApplication> m_channelFactory;
    private object m_channelFactoryLock = new object();
    private string m_endpointConfigName;

    [Persisted]
    private SPServiceLoadBalancer m_loadBalancer;

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
      string clientConfigPath = SPUtility.GetGenericSetupPath(@"WebClients\Barista");
      Configuration clientConfig = OpenClientConfiguration(clientConfigPath);
      ConfigurationChannelFactory<T> factory = new ConfigurationChannelFactory<T>(endpointConfigName, clientConfig, null);

      // configure the channel factory
      factory.ConfigureCredentials(SPServiceAuthenticationMode.Claims);

      return factory;
    }

    internal delegate void CodeToRunOnApplicationProxy(BaristaServiceApplicationProxy appProxy);

    internal static void Invoke(SPServiceContext serviceContext, CodeToRunOnApplicationProxy codeBlock)
    {
      if (serviceContext == null)
        throw new ArgumentNullException("serviceContext");

      // get service app proxy from the context
      BaristaServiceApplicationProxy proxy = (BaristaServiceApplicationProxy)serviceContext.GetDefaultProxy(typeof(BaristaServiceApplicationProxy));
      if (proxy == null)
        throw new InvalidOperationException("Unable to obtain object reference to Barista service proxy.");

      // run the code block on the proxy
      using (new SPServiceContextScope(serviceContext))
      {
        codeBlock(proxy);
      }
    }

    private string GetEndpointConfigName(Uri address)
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
      string endpointConfig = GetEndpointConfigName(address);

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

      IBaristaServiceApplication channel;

      // create a channel that acts as the logged on user when authenticating with the service
      channel = m_channelFactory.CreateChannelActingAsLoggedOnUser<IBaristaServiceApplication>(new EndpointAddress(address));
      return channel;
    }

    private delegate void CodeToRunOnChannel(IBaristaServiceApplication contract);
    private void ExecuteOnChannel(string operationName, CodeToRunOnChannel codeBlock)
    {
      SPServiceLoadBalancerContext loadBalancerContext = m_loadBalancer.BeginOperation();

      try
      {
        // get a channel to the service app endpoint
        IChannel channel = (IChannel)GetChannel(loadBalancerContext.EndpointAddress);
        try
        {
          // execute the code block
          codeBlock((IBaristaServiceApplication)channel);
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

    #region service application methods
    public BrewResponse Eval(BrewRequest request)
    {
      BrewResponse result = null;

      // execute the call against the service app
      ExecuteOnChannel("Eval",
          delegate(IBaristaServiceApplication channel)
          {
            result = channel.Eval(request);
          });

      return result;
    }
    public void Exec(BrewRequest request)
    {
      // execute the call against the service app
      ExecuteOnChannel("Exec",
          delegate(IBaristaServiceApplication channel)
          {
            channel.Exec(request);
          });
    }
    #endregion
  }
}
