namespace Barista.SharePoint.Services
{
  using Microsoft.SharePoint;
  using System;

  public sealed class BaristaServiceClient
  {
    private readonly SPServiceContext m_serviceContext;

    public BaristaServiceClient(SPServiceContext serviceContext)
    {
	
	    if (serviceContext == null)
	        throw new ArgumentNullException("serviceContext");

      m_serviceContext = serviceContext;
    }

    public BrewResponse Eval(BrewRequest request)
    {
      BrewResponse result = null;
      BaristaServiceApplicationProxy.Invoke(
        m_serviceContext,
        proxy => result = proxy.Eval(request)
      );
      return result;
    }

    public void Exec(BrewRequest request)
    {
      BaristaServiceApplicationProxy.Invoke(
        m_serviceContext,
        proxy => proxy.Exec(request)
      );
    }

    public void AddObjectToIndex(string indexUrl, bool createIndex, string json)
    {
      //TODO: Change this (And add a seperate static method on the proxy
      //That obtains the corresponding proxy to the BaristaServiceApplication.IndexServerAffinityKey
      //defined in the folder property bag.
      //Not necessary now, as there is only one app server....
      BaristaServiceApplicationProxy.Invoke(
        m_serviceContext,
        proxy => proxy.AddObjectToIndex(indexUrl, createIndex, json)
      );
    }
  }
}
