namespace Barista.DocumentStore.Library
{
  using Barista.DocumentStore;
  using Barista.Library;
  using Jurassic;
  using Jurassic.Library;
  using System;

  [Serializable]
  public class ContainerInstance : ObjectInstance, IContainer
  {
    private readonly IContainer m_container;

    public ContainerInstance(ScriptEngine engine, IContainer container)
      : base(engine)
    {
      if (container == null)
        throw new ArgumentNullException("container");

      m_container = container;

      this.PopulateFields();
      this.PopulateFunctions();
    }

    public IContainer Container
    {
      get { return m_container; }
    }

    #region Properties

    [JSProperty(Name = "id")]
    public GuidInstance Id
    {
      get { return new GuidInstance(this.Engine.Object.InstancePrototype, m_container.Id); }
      set
      {
        if (value != null)
          m_container.Id = value.Value;
      }
    }

    Guid IContainer.Id
    {
      get { return m_container.Id; }
      set { m_container.Id = value; }
    }

    [JSProperty(Name = "title")]
    public string Title
    {
      get { return m_container.Title; }
      set { m_container.Title = value; }
    }

    [JSProperty(Name = "description")]
    public string Description
    {
      get { return m_container.Description; }
      set { m_container.Description = value; }
    }

    [JSProperty(Name = "rootFolder")]
    public IFolder RootFolder
    {
      get { return m_container.RootFolder; }
    }

    [JSProperty(Name = "url")]
    public string Url
    {
      get { return m_container.Url; }
      set { m_container.Url = value; }
    }

    [JSProperty(Name = "created")]
    public DateInstance Created
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_container.Created); }
      set { m_container.Created = DateTime.Parse(value.ToIsoString()); }
    }

    DateTime IContainer.Created
    {
      get { return m_container.Created; }
      set { m_container.Created = value; }
    }

    [JSProperty(Name = "createdBy")]
    public object CreatedBy
    {
      get
      {
        if (m_container.CreatedBy == null)
          return Null.Value;

        return m_container.CreatedBy.LoginName;
      }
    }

    IUser IContainer.CreatedBy
    {
      get { return m_container.CreatedBy; }
      set { m_container.CreatedBy = value; }
    }

    [JSProperty(Name = "modified")]
    public DateInstance Modified
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_container.Modified); }
      set { m_container.Modified = DateTime.Parse(value.ToIsoString()); }
    }

    DateTime IContainer.Modified
    {
      get { return m_container.Modified; }
      set { m_container.Modified = value; }
    }

    [JSProperty(Name = "modifiedBy")]
    public object ModifiedBy
    {
      get
      {
        if (m_container.ModifiedBy == null)
          return Null.Value;

        return m_container.ModifiedBy.LoginName;
      }
    }

    IUser IContainer.ModifiedBy
    {
      get { return m_container.ModifiedBy; }
      set { m_container.ModifiedBy = value; }
    }
    #endregion
  }
}
