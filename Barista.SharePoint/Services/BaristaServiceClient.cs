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
  }
}
