namespace Barista.SharePoint.ServiceManagement
{
  using Microsoft.SharePoint.Administration;

  public class BaristaSearchServiceInstance : SPWindowsServiceInstance
  {
    public const string ServiceInstanceName = "BaristaSearchServiceInstance";

    public BaristaSearchServiceInstance()
    {
    }

    public BaristaSearchServiceInstance(SPWindowsService spWinService)
      : this(SPServer.Local, spWinService)
    {
    }

    public BaristaSearchServiceInstance(SPServer spServer, SPWindowsService spWinService)
      : this(ServiceInstanceName, spServer, spWinService)
    {
    }

    public BaristaSearchServiceInstance(string siName, SPServer spServer, SPWindowsService spWinService)
      : base(siName, spServer, spWinService)
    {
    }
  }
}
