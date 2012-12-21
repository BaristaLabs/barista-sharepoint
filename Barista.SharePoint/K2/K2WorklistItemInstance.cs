namespace Barista.SharePoint.K2.Library
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using K2Services;

  public class K2WorklistItemConstructor : ClrFunction
  {
    public K2WorklistItemConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "K2WorklistItem", new K2WorklistItemInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public K2WorklistItemInstance Construct()
    {
      return new K2WorklistItemInstance(this.InstancePrototype);
    }

    public K2WorklistItemInstance Construct(WorklistItem workListItem)
    {
      return new K2WorklistItemInstance(this.InstancePrototype, workListItem);
    }
  }

  public class K2WorklistItemInstance : ObjectInstance
  {
    private WorklistItem m_workListItem;

    public K2WorklistItemInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public K2WorklistItemInstance(ObjectInstance prototype, WorklistItem workListItem)
      : this(prototype)
    {
      this.m_workListItem = workListItem;
    }

    public K2WorklistItemInstance(ScriptEngine engine, WorklistItem workListItem)
      : this(engine.Object.InstancePrototype, workListItem)
    {
    }

    [JSProperty(Name = "action")]
    public ArrayInstance Action
    {
      get
      {
        return this.Engine.Array.Construct();
      }
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
      get { return m_workListItem.AllocatedUser; }
    }

    [JSProperty(Name = "data")]
    public string Data
    {
      get { return m_workListItem.Data; }
    }

    [JSProperty(Name = "id")]
    public long Id
    {
      get { return m_workListItem.ID; }
    }

    [JSProperty(Name = "serialNumber")]
    public string SerialNumber
    {
      get { return m_workListItem.SerialNumber; }
    }

    [JSProperty(Name = "status")]
    public string Status
    {
      get { return m_workListItem.Status.ToString(); }
    }
  }
}
