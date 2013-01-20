namespace Barista.SharePoint.ServiceManagement
{
  using System;
  using Microsoft.SharePoint.Administration;

  internal class BaristaSearchService : SPWindowsService
  {
    public const string NtServiceName = "BaristaSearchWindowsService";

    public BaristaSearchService()
    {
    }

    public BaristaSearchService(SPFarm spFarm, SPManagedAccount managedAccount)
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
        return "Barista Search Service";
      }
    }
  }
}
