namespace Barista.SharePoint.SearchService
{
  using System;
  using Barista.Search;
  using Barista.SharePoint.Search;
  using Microsoft.SharePoint.Administration;
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
      var localServer = SPServer.Local;

      if (localServer == null)
        throw new InvalidOperationException("Unable to locate a SharePoint farm. Ensure that the current machine is joined to a SharePoint farm and the farm is online.");
      
      m_serviceHost = new ServiceHost(typeof(SPBaristaSearchService));
      //m_serviceHost.AddServiceEndpoint(typeof (IBaristaSearch),
      //                                 new WSHttpBinding(SecurityMode.TransportWithMessageCredential,
      //                                                   true), localServer.Address);
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
