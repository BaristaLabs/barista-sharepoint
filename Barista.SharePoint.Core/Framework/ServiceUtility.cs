﻿namespace Barista.SharePoint.Framework
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Net;
  using System.ServiceModel;
  using System.ServiceModel.Description;
  using System.Web;
  using System.ServiceModel.Channels;

  public static class ServiceUtility
  {
    /// <summary>
    /// For the given collection of Uris, returns the Uri that is associated with the current request.
    /// </summary>
    /// <param name="baseAddresses"></param>
    /// <returns></returns>
    public static Uri[] FilterBaseAddresses(Uri[] baseAddresses)
    {
      Uri result = null;

      //If there's only one base address, use that one.
      if (baseAddresses.Length == 1)
        result = baseAddresses[0];

      //Attempt to find the non-fully qualified host name from the base addresses.
      if (result == null)
      {
        var currentHostName = HttpContext.Current.Request.Url.Host;

        result = baseAddresses.FirstOrDefault(item => String.Compare(item.Host, currentHostName, StringComparison.OrdinalIgnoreCase) == 0);
      }

      //Attempt to find the fully qualified host name
      if (result == null)
      {
        var fullyQualifiedHostName = System.Net.Dns.GetHostEntry("").HostName;

        result = baseAddresses.FirstOrDefault(item => String.Compare(item.Host, fullyQualifiedHostName, StringComparison.OrdinalIgnoreCase) == 0);
      }

      if (result == null)
        return null;

      return new[] { result };
    }

    public static void ConfigureServiceHost()
    {
      var config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");

      var serviceModelGroup = System.ServiceModel.Configuration.ServiceModelSectionGroup.GetSectionGroup(config);

      if (serviceModelGroup != null)
        serviceModelGroup.ServiceHostingEnvironment.AspNetCompatibilityEnabled = true;
    }

    /// <summary>
    /// Can only call before opening the host
    /// </summary>
    public static void EnableMetadataExchange(ServiceHostBase serviceHost, Uri baseAddress, AuthenticationSchemes schemes, bool enableHttpGet)
    {
      if (serviceHost.State == CommunicationState.Opened)
      {
        throw new InvalidOperationException("Host is already opened");
      }

      var metadataBehavior = serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
      var address = new Uri(baseAddress, "/" + baseAddress.GetComponents(UriComponents.Path, UriFormat.Unescaped) + "/mex");

      if (metadataBehavior == null)
      {
        metadataBehavior = new ServiceMetadataBehavior();
        serviceHost.Description.Behaviors.Add(metadataBehavior);

        if (object.ReferenceEquals(address.Scheme, Uri.UriSchemeHttp))
        {
          metadataBehavior.HttpGetEnabled = enableHttpGet;
          metadataBehavior.HttpGetUrl = address;
        }
        else
        {
          metadataBehavior.HttpsGetEnabled = enableHttpGet;
          metadataBehavior.HttpGetUrl = address;
        }
      }
      AddMexEndpoint(serviceHost, address, schemes);
    }

    public static void AddMexEndpoint(ServiceHostBase serviceHost, Uri baseAddress, AuthenticationSchemes schemes)
    {
      HttpTransportBindingElement httpTransport;
      if (baseAddress.Scheme == Uri.UriSchemeHttp)
      {
        httpTransport = new HttpTransportBindingElement();
      }
      else
      {
        httpTransport = new HttpsTransportBindingElement();
      }
      httpTransport.AuthenticationScheme = schemes;
      Binding binding = new CustomBinding(new BindingElement[] { httpTransport });
      serviceHost.AddServiceEndpoint("IMetadataExchange", binding, baseAddress);
    }

    public static HttpClientCredentialType ClientCredentialTypeFromAuthenticationScheme(AuthenticationSchemes oneAuthScheme)
    {
      const HttpClientCredentialType none = HttpClientCredentialType.None;
      switch (oneAuthScheme)
      {
        case AuthenticationSchemes.Digest:
          return HttpClientCredentialType.Digest;

        case AuthenticationSchemes.Negotiate:
        case AuthenticationSchemes.IntegratedWindowsAuthentication:
          return HttpClientCredentialType.Windows;

        case (AuthenticationSchemes.Negotiate | AuthenticationSchemes.Digest):
        case (AuthenticationSchemes.Ntlm | AuthenticationSchemes.Digest):
        case (AuthenticationSchemes.IntegratedWindowsAuthentication | AuthenticationSchemes.Digest):
          return none;

        case AuthenticationSchemes.Ntlm:
          return HttpClientCredentialType.Ntlm;

        case AuthenticationSchemes.Basic:
          return HttpClientCredentialType.Basic;

        case AuthenticationSchemes.Anonymous:
          return HttpClientCredentialType.None;
      }
      return none;
    }

    public static Uri[] GetBaseAddressesWithUniqueScheme(Uri[] baseAddresses)
    {
      Dictionary<string, Uri> dictionary = new Dictionary<string, Uri>();

      if (baseAddresses == null)
        throw new ArgumentNullException("baseAddresses", @"No base addresses were supplied.");

      foreach (Uri uri in baseAddresses)
      {
        if (!dictionary.ContainsKey(uri.Scheme))
        {
          dictionary.Add(uri.Scheme, uri);
        }
      }
      Uri[] uriArray = new Uri[dictionary.Values.Count];
      int num = 0;
      foreach (Uri uri2 in dictionary.Values)
      {
        uriArray[num++] = uri2;
      }
      return uriArray;
    }

    public static Type GetContractType(IDictionary<string, ContractDescription> implementedContracts)
    {
      return implementedContracts.Values.Select(description => description.ContractType).FirstOrDefault();
    }
  }
}