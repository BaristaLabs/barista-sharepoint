namespace Barista.TeamFoundationServer.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using Microsoft.TeamFoundation.WorkItemTracking.Client;
  using System;

  [Serializable]
  public class TfsProjectConstructor : ClrFunction
  {
    public TfsProjectConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "Project", new TfsProjectInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public TfsProjectInstance Construct()
    {
      return new TfsProjectInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class TfsProjectInstance : ObjectInstance
  {
    private readonly Project m_project;

    public TfsProjectInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public TfsProjectInstance(ObjectInstance prototype, Project project)
      : this(prototype)
    {
      if (project == null)
        throw new ArgumentNullException("project");

      m_project = project;
    }

    public Project Project
    {
      get { return m_project; }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_project.Name; }
    }

    [JSProperty(Name = "guid")]
    public GuidInstance Guid
    {
      get { return new GuidInstance(this.Engine.Object.InstancePrototype, m_project.Guid); }
    }

    [JSProperty(Name = "uri")]
    public UriInstance Uri
    {
      get { return new UriInstance(this.Engine.Object.InstancePrototype, m_project.Uri); }
    }

    [JSFunction(Name = "getAllIterations")]
    public ArrayInstance GetAllIterations()
    {
      var result = this.Engine.Array.Construct();

      foreach (Node node in m_project.IterationRootNodes)
      {
        ArrayInstance.Push(result, node.Name);
        GetAllIterationsRecursive(node, result);
      }

      return result;
    }

    [JSFunction(Name = "getWorkItemTypes")]
    public ArrayInstance GetWorkItemTypes()
    {
      var result = this.Engine.Array.Construct();
      foreach (WorkItemType workItemType in m_project.WorkItemTypes)
      {
        ArrayInstance.Push(result, new TfsWorkItemTypeInstance(this.Engine.Object.InstancePrototype, workItemType));
      }
      return result;
    }

    [JSFunction(Name = "queryWorkItems")]
    public ArrayInstance QueryWorkItems(object query)
    {
      var result = this.Engine.Array.Construct();

      var wiql = TypeConverter.ToString(query);
      var wiQuery = new Query(m_project.Store, wiql);
      foreach (WorkItem workItem in wiQuery.RunQuery())
      {
        ArrayInstance.Push(result, new TfsWorkItemInstance(this.Engine.Object.InstancePrototype, workItem));
      }
      return result;
    }

    private void GetAllIterationsRecursive(Node node, ArrayInstance array)
    {
      foreach (Node item in node.ChildNodes)
      {
        ArrayInstance.Push(array, node.Name);
        GetAllIterationsRecursive(item, array);
      }
    }
  }
}
