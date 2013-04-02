using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Barista.SharePoint.IndexService
{
  using Topshelf;

  class Program
  {
    static void Main(string[] args)
    {
      HostFactory.Run(x =>
      {
        x.Service<WinnowService>(s =>
        {
          s.ConstructUsing(name => new WinnowService());
          s.WhenStarted(wa => wa.Start());
          s.WhenStopped(wa => wa.Stop());
        });
        x.RunAsLocalSystem();

        x.SetDescription("Service that allows for indexing and queries.");
        x.SetDisplayName("Barista Index Service");
        x.SetServiceName("BaristaIndexWindowsService");
      });     
    }
  }
}
