namespace Barista.TeamFoundationServer.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Microsoft.TeamFoundation.WorkItemTracking.Client;
  using System;

  [Serializable]
  public class WorkItemStoreConstructor : ClrFunction
  {
    public WorkItemStoreConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "WorkItemStore", new WorkItemStoreInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public WorkItemStoreInstance Construct()
    {
      return new WorkItemStoreInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class WorkItemStoreInstance : ObjectInstance
  {
    private readonly WorkItemStore m_workItemStore;

    public WorkItemStoreInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public WorkItemStoreInstance(ObjectInstance prototype, WorkItemStore workItemStore)
      : this(prototype)
    {
      if (workItemStore == null)
        throw new ArgumentNullException("workItemStore");

      m_workItemStore = workItemStore;
    }

    public WorkItemStore WorkItemStore
    {
      get { return m_workItemStore; }
    }

    public string Something
    {
      get
      {
        var p = m_workItemStore.Projects[0];
        return null;
      }
    }
  }
}
