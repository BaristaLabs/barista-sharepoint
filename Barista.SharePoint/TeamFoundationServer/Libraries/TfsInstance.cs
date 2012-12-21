namespace Barista.SharePoint.TeamFoundationServer.Library
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using Jurassic.Library;
  using Barista.Library;
  using Microsoft.TeamFoundation.Client;
  using Microsoft.TeamFoundation.WorkItemTracking.Client;

  public class TfsInstance : ObjectInstance
  {
    public TfsInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    #region Properties
    
    #endregion

    [JSFunction(Name="getTeamProjectCollection")]
    public ArrayInstance GetTeamProjectCollection(object serverUri)
    {
      Uri uri;
      if (serverUri is UriInstance)
        uri = (serverUri as UriInstance).Uri;
      else
        uri = new Uri(serverUri.ToString());

      var provider = new TfsCredentialsProvider();
      var tfs = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(uri, provider);
      tfs.Authenticate();

      var wiStore = tfs.GetService<WorkItemStore>();
      object[] projectNames = wiStore.Projects.OfType<Project>().Select(p => p.Name).ToArray();
      var projects = this.Engine.Array.Construct(projectNames);
      return projects;
    }
  }
}
