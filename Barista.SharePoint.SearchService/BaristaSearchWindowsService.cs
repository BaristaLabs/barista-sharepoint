namespace Barista.SharePoint.SearchService
{
  using Barista.Services;
  using System.ServiceModel;

  /// <summary>
  /// Represents a Barista Search Service hosted within a windows service.
  /// </summary>
  /// <remarks>
  /// Since .Net 3.5 WCF services do not support a shutdown event, a windows service
  /// that hosts Lucene and cleanly closes indexes on shutdown limits corruption of
  /// Lucene indexes due to IISResets or Worker Process Recycles.
  /// </remarks>
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
      if (m_serviceHost == null)
        return;

      m_serviceHost.Close();
      m_serviceHost = null;

      //Ensure that all indexes have been closed.
      BaristaSearchService.CloseAllIndexes();
    }
  }
}
