namespace Barista.SharePoint.Library
{
  using Barista.DocumentStore;
  using Jurassic;
  using Jurassic.Library;
  using System;

  [Serializable]
  public class ContainerInstance : ObjectInstance
  {
    Container m_container;

    public ContainerInstance(ScriptEngine engine, Container container)
      : base(engine)
    {
      if (container == null)
        throw new ArgumentNullException("container");

      m_container = container;

      this.PopulateFields();
      this.PopulateFunctions();
    }

    #region Properties

    [JSProperty(Name = "id")]
    public string Id
    {
      get { return m_container.Id.ToString(); }
      set { m_container.Id = new Guid(value); }
    }

    [JSProperty(Name = "description")]
    public string Description
    {
      get;
      set;
    }

    [JSProperty(Name = "entityCount")]
    public int EntityCount
    {
      get;
      set;
    }

    [JSProperty(Name = "title")]
    public string Title
    {
      get { return m_container.Title; }
      set { m_container.Title = value; }
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
      set { m_container.Created = DateTime.Parse(value.ToISOString()); }
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

    [JSProperty(Name = "modified")]
    public DateInstance Modified
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_container.Modified); }
      set { m_container.Modified = DateTime.Parse(value.ToISOString()); }
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
    #endregion
  }
}
