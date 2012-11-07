namespace Barista.SharePoint.Library
{
  using Barista.DocumentStore;
  using Jurassic;
  using Jurassic.Library;
  using System;

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
      get;
      set;
    }


    #endregion
  }
}
