namespace Barista.SharePoint.Library
{
  using Barista.Library;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using System;

  [Serializable]
  public class SPEventReceiverDefinitionConstructor : ClrFunction
  {
    public SPEventReceiverDefinitionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPEventReceiverDefinition", new SPEventReceiverDefinitionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPEventReceiverDefinitionInstance Construct()
    {
      return new SPEventReceiverDefinitionInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPEventReceiverDefinitionInstance : ObjectInstance
  {
    private readonly SPEventReceiverDefinition m_eventReceiverDefinition;

    public SPEventReceiverDefinitionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPEventReceiverDefinitionInstance(ObjectInstance prototype, SPEventReceiverDefinition sPEventReceiverDefinition)
      : this(prototype)
    {
      if (sPEventReceiverDefinition == null)
        throw new ArgumentNullException("sPEventReceiverDefinition");

      m_eventReceiverDefinition = sPEventReceiverDefinition;
    }

    public SPEventReceiverDefinition SPEventReceiverDefinition
    {
      get { return m_eventReceiverDefinition; }
    }

    [JSProperty(Name = "assembly")]
    public string Assembly
    {
      get { return m_eventReceiverDefinition.Assembly; }
      set { m_eventReceiverDefinition.Assembly = value; }
    }

    [JSProperty(Name = "class")]
    public string Class
    {
      get { return m_eventReceiverDefinition.Class; }
      set { m_eventReceiverDefinition.Class = value; }
    }

    [JSProperty(Name = "contextCollectionId")]
    public GuidInstance ContextCollectionId
    {
      get { return new GuidInstance(this.Engine.Object.InstancePrototype, m_eventReceiverDefinition.ContextCollectionId); }
      set
      {
        if (value != null)
          m_eventReceiverDefinition.ContextCollectionId = value.Value;
      }
    }

    
    [JSProperty(Name = "contextEventType")]
    public GuidInstance ContextEventType
    {
      get { return new GuidInstance(this.Engine.Object.InstancePrototype, m_eventReceiverDefinition.ContextEventType); }
      set
      {
        if (value != null)
          m_eventReceiverDefinition.ContextEventType = value.Value;
      }
    }

    [JSProperty(Name = "contextId")]
    public GuidInstance ContextId
    {
      get { return new GuidInstance(this.Engine.Object.InstancePrototype, m_eventReceiverDefinition.ContextId); }
      set
      {
        if (value != null)
          m_eventReceiverDefinition.ContextId = value.Value;
      }
    }

    [JSProperty(Name = "contextItemId")]
    public int ContextItemId
    {
      get { return m_eventReceiverDefinition.ContextItemId; }
      set { m_eventReceiverDefinition.ContextItemId = value; }
    }

    [JSProperty(Name = "contextItemUrl")]
    public string ContextItemUrl
    {
      get { return m_eventReceiverDefinition.ContextItemUrl; }
      set { m_eventReceiverDefinition.ContextItemUrl = value; }
    }

    [JSProperty(Name = "contextObjectId")]
    public GuidInstance ContextObjectId
    {
      get { return new GuidInstance(this.Engine.Object.InstancePrototype, m_eventReceiverDefinition.ContextObjectId); }
      set
      {
        if (value != null)
          m_eventReceiverDefinition.ContextObjectId = value.Value;
      }
    }

    [JSProperty(Name = "contextType")]
    public GuidInstance ContextType
    {
      get { return new GuidInstance(this.Engine.Object.InstancePrototype, m_eventReceiverDefinition.ContextType); }
      set
      {
        if (value != null)
          m_eventReceiverDefinition.ContextType = value.Value;
      }
    }

    [JSProperty(Name = "credential")]
    public int Credential
    {
      get { return m_eventReceiverDefinition.Credential; }
      set { m_eventReceiverDefinition.Credential = value; }
    }

    [JSProperty(Name = "data")]
    public string Data
    {
      get { return m_eventReceiverDefinition.Data; }
      set { m_eventReceiverDefinition.Data = value; }
    }

    [JSProperty(Name = "filter")]
    public string Filter
    {
      get { return m_eventReceiverDefinition.Filter; }
      set { m_eventReceiverDefinition.Filter = value; }
    }

    [JSProperty(Name = "id")]
    public GuidInstance Id
    {
      get { return new GuidInstance(this.Engine.Object.InstancePrototype, m_eventReceiverDefinition.Id); }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_eventReceiverDefinition.Name; }
      set { m_eventReceiverDefinition.Name = value; }
    }

    [JSProperty(Name = "parentHostId")]
    public GuidInstance ParentHostId
    {
      get { return new GuidInstance(this.Engine.Object.InstancePrototype, m_eventReceiverDefinition.ParentHostId); }
      set
      {
        if (value != null)
          m_eventReceiverDefinition.ParentHostId = value.Value;
      }
    }

    [JSProperty(Name = "parentHostType")]
    public string ParentHostType
    {
      get { return m_eventReceiverDefinition.ParentHostType.ToString(); }
      set { m_eventReceiverDefinition.ParentHostType = (SPEventHostType) Enum.Parse(typeof (SPEventHostType), value); }
    }

    [JSProperty(Name = "sequenceNumber")]
    public int SequenceNumber
    {
      get { return m_eventReceiverDefinition.SequenceNumber; }
      set { m_eventReceiverDefinition.SequenceNumber = value; }
    }

    [JSProperty(Name = "siteId")]
    public GuidInstance SiteId
    {
      get { return new GuidInstance(this.Engine.Object.InstancePrototype, m_eventReceiverDefinition.SiteId); }
    }

    [JSProperty(Name = "synchronization")]
    public string Synchronization
    {
      get { return m_eventReceiverDefinition.Synchronization.ToString(); }
      set { m_eventReceiverDefinition.Synchronization = (SPEventReceiverSynchronization)Enum.Parse(typeof(SPEventReceiverSynchronization), value); }
    }

    [JSProperty(Name = "type")]
    public string Type
    {
      get { return m_eventReceiverDefinition.Type.ToString(); }
      set { m_eventReceiverDefinition.Type = (SPEventReceiverType) Enum.Parse(typeof (SPEventReceiverType), value); }
    }

    [JSProperty(Name = "webId")]
    public GuidInstance WebId
    {
      get { return new GuidInstance(this.Engine.Object.InstancePrototype, m_eventReceiverDefinition.WebId); }
    }

    [JSFunction(Name = "delete")]
    public void Delete()
    {
      m_eventReceiverDefinition.Delete();
    }

    [JSFunction(Name = "fireContextEvent")]
    public void FireContextEvent(SPSiteInstance instance)
    {
      if (instance != null)
      {
        m_eventReceiverDefinition.FireContextEvent(instance.Site);
      }
    }

    [JSFunction(Name = "update")]
    public void Update()
    {
      m_eventReceiverDefinition.Update();
    }

    [JSFunction(Name = "ToString")]
    public string ToStringJS()
    {
      return this.ToString();
    }

    [JSFunction(Name = "toString")]
    public override string ToString()
    {
      return m_eventReceiverDefinition.ToString();
    }
  }
}
