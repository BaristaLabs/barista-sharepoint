namespace Barista.TeamFoundationServer.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Microsoft.TeamFoundation.WorkItemTracking.Client;
  using System;

  [Serializable]
  public class TfsWorkItemTypeConstructor : ClrFunction
  {
    public TfsWorkItemTypeConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "TfsWorkItemType", new TfsWorkItemTypeInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public TfsWorkItemTypeInstance Construct()
    {
      return new TfsWorkItemTypeInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class TfsWorkItemTypeInstance : ObjectInstance
  {
    private readonly WorkItemType m_workItemType;

    public TfsWorkItemTypeInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public TfsWorkItemTypeInstance(ObjectInstance prototype, WorkItemType workItemType)
      : this(prototype)
    {
      if (workItemType == null)
        throw new ArgumentNullException("workItemType");

      m_workItemType = workItemType;
    }

    public WorkItemType WorkItemType
    {
      get { return m_workItemType; }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get
      {
        return m_workItemType.Name;
      }
    }

    [JSProperty(Name = "descripton")]
    public string Description
    {
      get
      {
        return m_workItemType.Description;
      }
    }
  }
}
