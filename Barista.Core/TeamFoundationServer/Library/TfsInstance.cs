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

    [JSFunction(Name = "allowUntrustedCertificates")]
    public void AllowUntrusedCertificates()
    {
      //Trust all certificates
      System.Net.ServicePointManager.ServerCertificateValidationCallback =
          ((sender, certificate, chain, sslPolicyErrors) => true);
    }

    [JSFunction(Name = "listTeamProjectCollections")]
    [JSDoc("ternReturnType", "[+TfsTeamProjectCollection]")]
    public ArrayInstance ListTeamProjectCollections()
    {
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

    [JSDoc("Returns the Team Project Collection with the specified display name. If no name is specified, gets the default project collection for the specified url.")]
    [JSFunction(Name="getTeamProjectCollection")]
    public object GetTeamProjectCollection(object name)
    {
      if (name == null || name == Null.Value || name == Undefined.Value)
      {
        var tfs = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(m_tfsUri);
        tfs.Credentials = m_credentialsProvider.Credential;

        tfs.EnsureAuthenticated();
        return new TfsTeamProjectCollectionInstance(this.Engine.Object, tfs);
      }

      var strName = TypeConverter.ToString(name);
      var configurationServer = TfsConfigurationServerFactory.GetConfigurationServer(m_tfsUri);
      configurationServer.Credentials = m_credentialsProvider.Credential;

      configurationServer.EnsureAuthenticated();

      var catalogNode = configurationServer.CatalogNode;

      var tpcNodes = catalogNode.QueryChildren(
        new[] { CatalogResourceTypes.ProjectCollection },
        false, CatalogQueryOptions.None);

      var teamProjectCollection = tpcNodes.FirstOrDefault(p => p.Resource.DisplayName == strName);

      if (teamProjectCollection == null)
        return Null.Value;

      return new TfsTeamProjectCollectionInstance(this.Engine.Object.InstancePrototype,
        configurationServer.GetTeamProjectCollection(teamProjectCollection.Resource.Identifier));
    }
  }
}
