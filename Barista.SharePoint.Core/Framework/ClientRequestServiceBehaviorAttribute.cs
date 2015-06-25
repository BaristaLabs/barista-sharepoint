namespace Barista.SharePoint.Framework
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.DirectoryServices;
  using System.Net;
  using System.ServiceModel;
  using System.ServiceModel.Channels;
  using System.ServiceModel.Description;
  using System.Web.Configuration;

  public sealed class ClientRequestServiceBehaviorAttribute : Attribute, IServiceBehavior
  {
    private static AuthenticationInfo s_authInfo;

    public static AuthenticationSchemes GetAllAuthenticationSchemes(out AuthenticationSchemes defaultOne)
    {
      AuthenticationInfo authInfo = AuthInfo;
      if (authInfo == null)
      {
        defaultOne = AuthenticationSchemes.Negotiate;
        return AuthenticationSchemes.Negotiate;
      }
      AuthenticationSchemes none = AuthenticationSchemes.None;
      defaultOne = AuthenticationSchemes.None;
      if ((authInfo.AuthFlags & AuthFlags.Anonymous) != AuthFlags.None)
      {
        defaultOne = AuthenticationSchemes.Anonymous;
        none |= AuthenticationSchemes.Anonymous;
      }
      if ((authInfo.AuthFlags & AuthFlags.Basic) != AuthFlags.None)
      {
        defaultOne = AuthenticationSchemes.Basic;
        none |= AuthenticationSchemes.Basic;
      }
      if ((authInfo.AuthFlags & AuthFlags.MD5) != AuthFlags.None)
      {
        defaultOne = AuthenticationSchemes.Digest;
        none |= AuthenticationSchemes.Digest;
      }
      if ((authInfo.AuthFlags & AuthFlags.NTLM) != AuthFlags.None)
      {
        defaultOne = AuthenticationSchemes.Ntlm;
        none |= AuthenticationSchemes.Ntlm;
        if ((authInfo.NTAuthenticationProviders != null) && (authInfo.NTAuthenticationProviders.IndexOf("Negotiate", StringComparison.OrdinalIgnoreCase) >= 0))
        {
          defaultOne = AuthenticationSchemes.Negotiate;
          none |= AuthenticationSchemes.Negotiate;
        }
      }
      if (none == AuthenticationSchemes.None)
      {
        none = AuthenticationSchemes.Anonymous;
      }
      if (defaultOne == AuthenticationSchemes.None)
      {
        defaultOne = AuthenticationSchemes.Anonymous;
      }
      if ((authInfo.AuthMode != AuthenticationMode.Windows) && ((authInfo.AuthFlags & AuthFlags.Anonymous) != AuthFlags.None))
      {
        defaultOne = AuthenticationSchemes.Anonymous;
      }
      return none;
    }

    void IServiceBehavior.AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
    {
    }

    void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
    {
    }

    void IServiceBehavior.Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
    {
      this.UpdateMaxReceivedMessageSize(serviceDescription.Endpoints);
    }

    private void UpdateMaxReceivedMessageSize(IEnumerable<ServiceEndpoint> endpoints)
    {
      int maxReceivedMessageSize = Int32.MaxValue;
      if (maxReceivedMessageSize == 0)
      {
        maxReceivedMessageSize = 0x400000;
      }
      if (maxReceivedMessageSize > 0)
      {
        foreach (ServiceEndpoint endpoint in endpoints)
        {
          CustomBinding binding = endpoint.Binding as CustomBinding;
          if (binding != null)
          {
            foreach (BindingElement element in binding.Elements)
            {
              HttpTransportBindingElement element2 = element as HttpTransportBindingElement;
              if (element2 != null)
              {
                element2.MaxReceivedMessageSize = maxReceivedMessageSize;
              }
            }
          }
        }
      }
    }

    // Properties
    private static AuthenticationInfo AuthInfo
    {
      get
      {
        if (s_authInfo == null)
        {
          AuthenticationInfo authInfo = new AuthenticationInfo();
          SecurityContext.RunAsProcess(delegate
          {
            AuthenticationMode mode = AuthenticationMode.Windows;
            try
            {
              AuthenticationSection section = (AuthenticationSection)WebConfigurationManager.GetSection("system.web/authentication", "/");
              mode = section.Mode;
            }
            catch (InvalidOperationException)
            {
            }
            DirectoryEntry entry = new DirectoryEntry("IIS://localhost/w3svc/" + IisUtility.CurrentInstanceId + "/Root");
            authInfo.AuthMode = mode;
            authInfo.AuthFlags = (AuthFlags)entry.Properties["AuthFlags"].Value;
            authInfo.NTAuthenticationProviders = (string)entry.Properties["NTAuthenticationProviders"].Value;
            PropertyValueCollection values = entry.Properties["ServerBindings"];
            authInfo.ServerBindings = new string[values.Count];
            values.CopyTo(authInfo.ServerBindings, 0);
            values = entry.Properties["SecureBindings"];
            authInfo.SecureBindings = new string[values.Count];
            values.CopyTo(authInfo.SecureBindings, 0);
          });
          s_authInfo = authInfo;
        }
        return s_authInfo;
      }
    }

    // Nested Types
    private class AuthenticationInfo
    {
      // Fields
      public ClientRequestServiceBehaviorAttribute.AuthFlags AuthFlags;
      public AuthenticationMode AuthMode;
      public string NTAuthenticationProviders;
      public string[] SecureBindings;
      public string[] ServerBindings;
    }

    [Flags]
    private enum AuthFlags
    {
      Anonymous = 1,
      Basic = 2,
      MD5 = 0x10,
      None = 0,
      NTLM = 4,
      Passport = 0x40
    }
  }
}
