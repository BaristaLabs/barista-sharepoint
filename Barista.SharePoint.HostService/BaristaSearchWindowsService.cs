namespace Barista.SharePoint.HostService
{
  using System.ServiceModel;
  using System.ServiceProcess;

  partial class BaristaSearchWindowsService : ServiceBase
  {
    private ServiceHost m_serviceHost;

    public BaristaSearchWindowsService()
    {
      InitializeComponent();
    }

    protected override void OnStart(string[] args)
    {
    }

    protected override void OnStop()
    {
      // TODO: Add code here to perform any tear-down necessary to stop your service.
    }
  }
}
