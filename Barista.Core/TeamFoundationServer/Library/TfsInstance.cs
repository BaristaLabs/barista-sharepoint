namespace Barista.TeamFoundationServer.Library
{
  using Barista.Jurassic;
  using Barista.Library;
  using Jurassic.Library;
  using Microsoft.TeamFoundation.Client;
  using Microsoft.TeamFoundation.Framework.Common;
  using System;
  using System.Linq;

  public class TfsInstance : ObjectInstance
  {
    private readonly BaristaNetworkCredentialsProvider m_credentialsProvider;
    private Uri m_tfsUri;

    public TfsInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();

      m_credentialsProvider = new BaristaNetworkCredentialsProvider(this.Engine);
    }

    #region Properties

    [JSProperty(Name = "credential")]
    public object Credential
    {
      get { return m_credentialsProvider.Credential; }
      set
      {
        if (value is NetworkCredentialInstance)
        {
          m_credentialsProvider.Credential = (value as NetworkCredentialInstance).NetworkCredential;
          return;
        }

        throw new JavaScriptException(this.Engine, "Error", "Unknown Credentials type: " + value.GetType());
      }
    }

    [JSProperty(Name = "uri")]
    public object Uri
    {
      get
      {
        if (m_tfsUri == null)
          return Null.Value;
        return new UriInstance(this.Engine.Object.InstancePrototype, m_tfsUri);
      }
      set
      {
        if (value == null || value == Undefined.Value || value == Null.Value)
          m_tfsUri = null;
        else if (value is UriInstance)
          m_tfsUri = (value as UriInstance).Uri;
        else
          m_tfsUri = new Uri(TypeConverter.ToString(value));
      }
    }

    #endregion

    [JSFunction(Name = "listTeamProjectCollections")]
    public ArrayInstance ListTeamProjectCollections()
    {
      //Trust all certificates
      System.Net.ServicePointManager.ServerCertificateValidationCallback =
          ((sender, certificate, chain, sslPolicyErrors) => true);

      var configurationServer = TfsConfigurationServerFactory.GetConfigurationServer(m_tfsUri);
      configurationServer.Credentials = m_credentialsProvider.Credential;

      configurationServer.EnsureAuthenticated();

      var catalogNode = configurationServer.CatalogNode;
      
      var tpcNodes = catalogNode.QueryChildren(
        new[] {CatalogResourceTypes.ProjectCollection},
        false, CatalogQueryOptions.None);

      var result = this.Engine.Array.Construct();
      foreach (var teamProjectCollection in tpcNodes
        .Select(p => configurationServer.GetTeamProjectCollection(new Guid(p.Resource.Properties["InstanceId"])))
        .Where(teamProjectCollection => teamProjectCollection != null))
      {
        ArrayInstance.Push(result, new TfsTeamProjectCollectionInstance(this.Engine.Object.InstancePrototype, teamProjectCollection));
      }

      return result;
    }

    [JSFunction(Name="getTeamProjectCollection")]
    public object GetTeamProjectCollection(string name)
    {
      var configurationServer = TfsConfigurationServerFactory.GetConfigurationServer(m_tfsUri);
      configurationServer.Credentials = m_credentialsProvider.Credential;

      configurationServer.EnsureAuthenticated();

      var catalogNode = configurationServer.CatalogNode;

      var tpcNodes = catalogNode.QueryChildren(
        new[] { CatalogResourceTypes.ProjectCollection },
        false, CatalogQueryOptions.None);

      var teamProjectCollection = tpcNodes.FirstOrDefault(p => p.Resource.DisplayName == name);

      if (teamProjectCollection == null)
        return Null.Value;

      return new TfsTeamProjectCollectionInstance(this.Engine.Object.InstancePrototype,
        configurationServer.GetTeamProjectCollection(teamProjectCollection.Resource.Identifier));
    }
  }
}
