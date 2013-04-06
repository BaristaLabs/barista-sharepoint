namespace Barista.SharePoint.K2.Library
{
  using Barista.SharePoint.K2Services;
  using Jurassic;
  using Jurassic.Library;
  using System;

  [Serializable]
  public class ProcessInstanceConstructor : ClrFunction
  {
    public ProcessInstanceConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ProcessInstance", new ProcessInstanceInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ProcessInstanceInstance Construct()
    {
      return new ProcessInstanceInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class ProcessInstanceInstance : ObjectInstance
  {
    private readonly ProcessInstance m_processInstance;

    public ProcessInstanceInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ProcessInstanceInstance(ObjectInstance prototype, ProcessInstance processInstance)
      : this(prototype)
    {
      if (processInstance == null)
        throw new ArgumentNullException("processInstance");

      m_processInstance = processInstance;
    }

    public ProcessInstance ProcessInstance
    {
      get { return m_processInstance; }
    }

    //[JSProperty(Name = "dataField")]
    //public ArrayInstance DataField
    //{
    //  get { return m_processInstance.DataField; }
    //}

    [JSProperty(Name = "description")]
    public string Description
    {
      get { return m_processInstance.Description; }
    }

    [JSProperty(Name = "expectedDuration")]
    public double ExpectedDuration
    {
      get { return m_processInstance.ExpectedDuration; }
    }

    [JSProperty(Name = "folder")]
    public string Folder
    {
      get { return m_processInstance.Folder; }
    }

    [JSProperty(Name = "folio")]
    public string Folio
    {
      get { return m_processInstance.Folio; }
    }

    [JSProperty(Name = "fullName")]
    public string FullName
    {
      get { return m_processInstance.FullName; }
    }

    [JSProperty(Name = "guid")]
    public string Guid
    {
      get { return m_processInstance.Guid; }
    }

    [JSProperty(Name = "Id")]
    public double Id
    {
      get { return m_processInstance.ID; }
    }

    [JSProperty(Name = "metadata")]
    public string Metadata
    {
      get { return m_processInstance.Metadata; }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_processInstance.Name; }
    }

    [JSProperty(Name = "priority")]
    public int Priority
    {
      get { return m_processInstance.Priority; }
    }

    [JSProperty(Name = "startDate")]
    public DateInstance StartDate
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_processInstance.StartDate); }
    }

    [JSProperty(Name = "status")]
    public string Status
    {
      get { return m_processInstance.Status.ToString(); }
    }

    //[JSProperty(Name = "xmlField")]
    //public ArrayInstance XmlField
    //{
    //  get { return m_processInstance.XmlField; }
    //}
  }
}
