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
        x.RunAsLocalService();

        x.SetDescription("Service that allows for adding documents to an index and querying on those documents.");
        x.SetDisplayName("Barista Search Service");
        x.SetServiceName("BaristaSearchWindowsService");
      });     
    }
  }
}
