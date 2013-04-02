namespace Barista.SharePoint.SearchService
{
  using Barista.Services;
  using System.ServiceModel;

  public class BaristaSearchWindowsService
  {
    private ServiceHost m_serviceHost;

    public void Start()
    {
      m_serviceHost = new ServiceHost(typeof(BaristaSearchService));

      m_serviceHost.Open();
    }

    public void Stop()
    {
      if (m_serviceHost != null)
      {
        m_serviceHost.Close();
        m_serviceHost = null;
      }
    }
  }
}
