namespace Barista.SharePoint.HostService
{
  using System.ComponentModel;
  using System.Configuration.Install;
  using System.ServiceProcess;

  [RunInstaller(true)]
  public partial class ProjectInstaller : Installer
  {
    private readonly ServiceProcessInstaller m_process;
    private readonly ServiceInstaller m_baristaSearchWindowsServiceInstaller;
    private readonly ServiceInstaller m_baristaWebSocketsWindowsServiceInstaller;

    public ProjectInstaller()
    {
      InitializeComponent();

      m_process = new ServiceProcessInstaller
      {
        Account = ServiceAccount.LocalSystem
      };

      m_baristaSearchWindowsServiceInstaller = new ServiceInstaller
      {
        ServiceName = "BaristaSearchWindowsService",
        DisplayName = "Barista Search Service",
        StartType = ServiceStartMode.Automatic,
      };

      m_baristaWebSocketsWindowsServiceInstaller = new ServiceInstaller
      {
        ServiceName = "BaristaWebSocketsWindowsService",
        DisplayName = "Barista Web Sockets Service",
        StartType = ServiceStartMode.Automatic,
      };

      Installers.Add(m_process);
      Installers.Add(m_baristaSearchWindowsServiceInstaller);
      Installers.Add(m_baristaWebSocketsWindowsServiceInstaller);
    }
  }
}
