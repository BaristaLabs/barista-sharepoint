namespace Barista.SharePoint.WebSocketsService
{
  using Topshelf;

  class Program
  {
    static void Main(string[] args)
    {
      HostFactory.Run(x =>
      {
        x.Service<BaristaWebSocketsWindowsService>(s =>
        {
          s.ConstructUsing(name => new BaristaWebSocketsWindowsService());
          s.WhenStarted(wa => wa.Start());
          s.WhenStopped(wa => wa.Stop());
        });
        x.RunAsLocalService();

        x.SetDescription("Service that allows for hosting Web Sockets Instances.");
        x.SetDisplayName("Barista Web Sockets Service");
        x.SetServiceName("BaristaWebSocketsWindowsService");
      });     
    }
  }
}
