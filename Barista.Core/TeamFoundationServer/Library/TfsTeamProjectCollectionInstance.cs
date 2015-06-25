namespace Barista.TeamFoundationServer.Library
{
  using System.Linq;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using Microsoft.TeamFoundation.Client;
  using System;
  using Microsoft.TeamFoundation.WorkItemTracking.Client;

  [Serializable]
  public class TfsTeamProjectCollectionConstructor : ClrFunction
  {
    public TfsTeamProjectCollectionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "TfsTeamProjectCollection", new TfsTeamProjectCollectionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public TfsTeamProjectCollectionInstance Construct()
    {
      return new TfsTeamProjectCollectionInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class TfsTeamProjectCollectionInstance : ObjectInstance
  {
    private readonly TfsTeamProjectCollection m_tfsTeamProjectCollection;

    public TfsTeamProjectCollectionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public TfsTeamProjectCollectionInstance(ObjectInstance prototype, TfsTeamProjectCollection tfsTeamProjectCollection)
      : this(prototype)
    {
      if (tfsTeamProjectCollection == null)
        throw new ArgumentNullException("tfsTeamProjectCollection");

      m_tfsTeamProjectCollection = tfsTeamProjectCollection;
    }

    public TfsTeamProjectCollection TfsTeamProjectCollection
    {
      get { return m_tfsTeamProjectCollection; }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_tfsTeamProjectCollection.Name; }
    }

    [JSProperty(Name = "uri")]
    public UriInstance Uri
    {
      get { return new UriInstance(this.Engine.Object.InstancePrototype, m_tfsTeamProjectCollection.Uri); }
    }

    [JSFunction(Name = "listProjects")]
    public ArrayInstance ListProjects()
    {
      //Not sure if just getting the workitemstore and iterating it's projects is preferable to the following:

      //var catalogNode = m_tfsTeamProjectCollection.CatalogNode;

      //var teamProjects = catalogNode.QueryChildren(
      //  new[] { CatalogResourceTypes.TeamProject },
      //  false, CatalogQueryOptions.None);


      var workItemStore = m_tfsTeamProjectCollection.GetService<WorkItemStore>();

      var result = this.Engine.Array.Construct();
      foreach (var project in workItemStore.Projects.OfType<Project>())
      {
        ArrayInstance.Push(result, new TfsProjectInstance(this.Engine.Object.InstancePrototype, project));
      }

      return result;
    }

    [JSFunction(Name = "getProject")]
    public object GetProject(string projectName)
    {
      var workItemStore = m_tfsTeamProjectCollection.GetService<WorkItemStore>();
      var project = workItemStore.Projects.OfType<Project>().FirstOrDefault(p => p.Name == projectName);

      if (project == null)
        return Null.Value;

      return new TfsProjectInstance(this.Engine.Object.InstancePrototype, project);
    }

    [JSFunction(Name = "getWorkItemStore")]
    public WorkItemStoreInstance GetWorkItemStore()
    {
      var workItemStore = m_tfsTeamProjectCollection.GetService<WorkItemStore>();
      return new WorkItemStoreInstance(this.Engine.Object.InstancePrototype, workItemStore);
    }
  }
}
