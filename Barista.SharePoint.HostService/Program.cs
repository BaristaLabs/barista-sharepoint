namespace Barista.SharePoint.HostService
{
  using System.ServiceProcess;

  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static void Main()
    {
      ServiceBase[] servicesToRun = new ServiceBase[] 
        { 
          new BaristaSearchWindowsService(), 
          new BaristaWebSocketsWindowsService()
        };
      ServiceBase.Run(servicesToRun);
    }
  }
}
