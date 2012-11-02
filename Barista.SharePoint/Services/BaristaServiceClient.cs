namespace Barista.SharePoint.Services
{
  using Microsoft.SharePoint;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Text;

  public sealed class BaristaServiceClient
  {
    private SPServiceContext m_serviceContext;

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
