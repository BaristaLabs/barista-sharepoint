namespace Barista.TeamFoundationServer.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Microsoft.TeamFoundation.WorkItemTracking.Client;
  using System;

  [Serializable]
  public class TfsWorkItemConstructor : ClrFunction
  {
    public TfsWorkItemConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "WorkItem", new TfsWorkItemInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public TfsWorkItemInstance Construct()
    {
      return new TfsWorkItemInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class TfsWorkItemInstance : ObjectInstance
  {
    private readonly WorkItem m_workItem;

    public TfsWorkItemInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public TfsWorkItemInstance(ObjectInstance prototype, WorkItem workItem)
      : this(prototype)
    {
      if (workItem == null)
        throw new ArgumentNullException("workItem");

      m_workItem = workItem;
    }

    public WorkItem WorkItem
    {
      get { return m_workItem; }
    }

    [JSProperty(Name = "areaId")]
    public int AreaId
    {
      get { return m_workItem.AreaId; }
      set { m_workItem.AreaId = value; }
    }

    [JSProperty(Name = "attachedFileCount")]
    public int AttachedFileCount
    {
      get { return m_workItem.AttachedFileCount; }
    }

    [JSProperty(Name = "externalLinkCount")]
    public int ExternalLinkCount
    {
      get { return m_workItem.ExternalLinkCount; }
    }

    [JSProperty(Name = "hyperLinkCount")]
    public int HyperLinkCount
    {
      get { return m_workItem.HyperLinkCount; }
    }

    [JSProperty(Name = "id")]
    public int Id
    {
      get { return m_workItem.Id; }
    }

    [JSProperty(Name = "internalVersion")]
    public int InternalVersion
    {
      get { return m_workItem.InternalVersion; }
    }

    [JSProperty(Name = "iterationId")]
    public int IterationId
    {
      get { return m_workItem.IterationId; }
      set { m_workItem.IterationId = value; }
    }

    [JSProperty(Name = "relatedLinkCount")]
    public int RelatedLinkCount
    {
      get { return m_workItem.RelatedLinkCount; }
    }

    [JSProperty(Name = "rev")]
    public int Rev
    {
      get { return m_workItem.Rev; }
    }

    [JSProperty(Name = "revision")]
    public int Revision
    {
      get { return m_workItem.Revision; }
    }

    [JSProperty(Name = "temporaryId")]
    public int TemporaryId
    {
      get { return m_workItem.TemporaryId; }
    }

    [JSProperty(Name = "watermark")]
    public int Watermark
    {
      get { return m_workItem.Watermark; }
    }

    [JSProperty(Name = "areaPath")]
    public string AreaPath
    {
      get { return m_workItem.AreaPath; }
      set { m_workItem.AreaPath = value; }
    }

    [JSProperty(Name = "changedBy")]
    public string ChangedBy
    {
      get { return m_workItem.ChangedBy; }
    }

    [JSProperty(Name = "createdBy")]
    public string CreatedBy
    {
      get { return m_workItem.CreatedBy; }
    }

    [JSProperty(Name = "description")]
    public string Description
    {
      get { return m_workItem.Description; }
      set { m_workItem.Description = value; }
    }

    [JSProperty(Name = "displayForm")]
    public string DisplayForm
    {
      get { return m_workItem.DisplayForm; }
    }

    [JSProperty(Name = "history")]
    public string History
    {
      get { return m_workItem.History; }
      set { m_workItem.History = value; }
    }

    [JSProperty(Name = "iterationPath")]
    public string IterationPath
    {
      get { return m_workItem.IterationPath; }
      set { m_workItem.IterationPath = value; }
    }

    [JSProperty(Name = "nodeName")]
    public string NodeName
    {
      get { return m_workItem.NodeName; }
    }

    [JSProperty(Name = "reason")]
    public string Reason
    {
      get { return m_workItem.Reason; }
      set { m_workItem.Reason = value; }
    }

    [JSProperty(Name = "state")]
    public string State
    {
      get { return m_workItem.State; }
      set { m_workItem.State = value; }
    }

    [JSProperty(Name = "tags")]
    public string Tags
    {
      get { return m_workItem.Tags; }
    }

    [JSProperty(Name = "title")]
    public string Title
    {
      get { return m_workItem.Title; }
      set { m_workItem.Title = value; }
    }

    [JSProperty(Name = "authorizedDate")]
    public DateInstance AuthorizedDate
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_workItem.AuthorizedDate); }
    }

    [JSProperty(Name = "changedDate")]
    public DateInstance ChangedDate
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_workItem.ChangedDate); }
    }

    [JSProperty(Name = "createdDate")]
    public DateInstance CreatedDate
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_workItem.CreatedDate); }
    }

    [JSProperty(Name = "revisedDate")]
    public DateInstance RevisedDate
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_workItem.RevisedDate); }
    }

    [JSProperty(Name = "fields")]
    public ArrayInstance Fields
    {
      get
      {
        var result = this.Engine.Array.Construct();
        foreach (Field field in m_workItem.Fields)
        {
          ArrayInstance.Push(result, new TfsWorkItemFieldInstance(this.Engine.Object.InstancePrototype, field));
        }
        return result;
      }
    }

    [JSFunction(Name = "save")]
    [JSDoc("Saves any changes on the work item.")]
    public void Save()
    {
      m_workItem.Save();
    }
  }
}
