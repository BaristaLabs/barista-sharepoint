namespace Barista.SharePoint.ServiceManagement
{
  using System;
  using Microsoft.SharePoint.Administration;

  public class BaristaWebSocketsService : SPWindowsService
  {
    public const string NtServiceName = "BaristaWebSocketsWindowsService";

    public BaristaWebSocketsService()
    {
    }

    public BaristaWebSocketsService(SPFarm spFarm, SPManagedAccount managedAccount)
      : base(NtServiceName, spFarm)
    {
      if (managedAccount == null)
        throw new ArgumentNullException("managedAccount");

      ProcessIdentity.ProcessAccount = SPProcessAccount.LookupManagedAccount(managedAccount.Sid);
      ProcessIdentity.ManagedAccount = managedAccount;
      ProcessIdentity.IsCredentialDeploymentEnabled = true;
      ProcessIdentity.IsCredentialUpdateEnabled = true;
    }

    public override string TypeName
    {
      get
      {
        return "Barista Web Sockets Service";
      }
    }
  }
}
