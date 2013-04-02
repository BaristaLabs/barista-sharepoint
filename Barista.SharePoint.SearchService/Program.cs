namespace Barista.SharePoint.SearchService
{
  using Topshelf;

  class Program
  {
    static void Main(string[] args)
    {
      HostFactory.Run(x =>
      {
        x.Service<BaristaSearchWindowsService>(s =>
        {
          s.ConstructUsing(name => new BaristaSearchWindowsService());
          s.WhenStarted(wa => wa.Start());
          s.WhenStopped(wa => wa.Stop());
        });
        x.RunAsLocalSystem();

        x.SetDescription("Service that allows for indexing and queries.");
        x.SetDisplayName("Barista Index and Query Service");
        x.SetServiceName("BaristaIndexQueryWindowsService");
      });     
    }
  }
}
