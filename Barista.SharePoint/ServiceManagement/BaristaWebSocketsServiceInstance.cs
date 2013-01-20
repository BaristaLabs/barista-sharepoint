namespace Barista.SharePoint.ServiceManagement
{
  using Microsoft.SharePoint.Administration;

  internal class BaristaWebSocketsServiceInstance : SPWindowsServiceInstance
  {
    public const string ServiceInstanceName = "BaristaWebSocketsServiceInstance";

    public BaristaWebSocketsServiceInstance()
    {
    }

    public BaristaWebSocketsServiceInstance(SPWindowsService spWinService)
      : this(SPServer.Local, spWinService)
    {
    }

    public BaristaWebSocketsServiceInstance(SPServer spServer, SPWindowsService spWinService)
      : this(ServiceInstanceName, spServer, spWinService)
    {
    }

    public BaristaWebSocketsServiceInstance(string siName, SPServer spServer, SPWindowsService spWinService)
      : base(siName, spServer, spWinService)
    {
    }
  }
}
