namespace Barista.SharePoint.K2.Library
{
  using Barista.SharePoint.K2Services;
  using Jurassic;
  using Jurassic.Library;
  using System;

  [Serializable]
  public class WorklistItemConstructor : ClrFunction
  {
    public WorklistItemConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "WorklistItem", new WorklistItemInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public WorklistItemInstance Construct()
    {
      return new WorklistItemInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class WorklistItemInstance : ObjectInstance
  {
    private readonly WorklistItem m_worklistItem;

    public WorklistItemInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public WorklistItemInstance(ObjectInstance prototype, WorklistItem worklistItem)
      : this(prototype)
    {
      if (worklistItem == null)
        throw new ArgumentNullException("worklistItem");

      m_worklistItem = worklistItem;
    }

    public WorklistItem WorklistItem
    {
      get { return m_worklistItem; }
    }

    //[JSProperty(Name = "activityInstanceDestination")]
    //public K2EventInstanceInstance ActivityInstanceDestination
    //{
    //  get
    //  {
    //    //return m_workListItem.ActivityInstanceDestination;
    //  }
    //}

    //[JSProperty(Name = "activityInstanceDestination1")]
    //public K2EventInstanceInstance ActivityInstanceDestination1
    //{
    //  get
    //  {
    //    return m_workListItem.ActivityInstanceDestination1;
    //  }
    //}

    [JSProperty(Name = "allocatedUser")]
    public string AllocatedUser
    {
      get { return m_worklistItem.AllocatedUser; }
    }

    [JSProperty(Name = "data")]
    public string Data
    {
      get { return m_worklistItem.Data; }
    }

    [JSProperty(Name = "id")]
    public long Id
    {
      get { return m_worklistItem.ID; }
    }

    [JSProperty(Name = "serialNumber")]
    public string SerialNumber
    {
      get { return m_worklistItem.SerialNumber; }
    }

    [JSProperty(Name = "status")]
    public string Status
    {
      get { return m_worklistItem.Status.ToString(); }
    }
  }
}
