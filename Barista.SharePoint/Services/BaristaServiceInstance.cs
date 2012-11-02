namespace Barista.SharePoint.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using Microsoft.SharePoint.Administration;

  public class BaristaServiceInstance : SPIisWebServiceInstance
  {
    public BaristaServiceInstance()
    {
    }

    public BaristaServiceInstance(SPServer server, BaristaService service)
      : base(server, service)
    {
    }

    public override string DisplayName
    {
      get { return "Barista Service"; }
    }

    public override string Description
    {
      get { return "Barista Service providing scripted service capabilities."; }
    }

    public override string TypeName
    {
      get { return "Barista Service"; }
    }
  }
}
