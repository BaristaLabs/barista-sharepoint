namespace Barista.SharePoint.Library
{
  using System;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint.Administration;

  [Serializable]
  public class SPContextInstance : ObjectInstance
  {
    private readonly SPBaristaContext m_context;

    public SPContextInstance(ScriptEngine engine, SPBaristaContext context)
      : base(engine)
    {
      m_context = context;

      if (m_context.Site != null)
        this.Site = new SPSiteInstance(this.Engine.Object.InstancePrototype, m_context.Site);

      if (m_context.Web != null)
        this.Web = new SPWebInstance(this.Engine.Object.InstancePrototype, m_context.Web);

      try
      {
        if (m_context.List != null)
          this.List = new SPListInstance(this.Engine.Object.InstancePrototype, null, null, m_context.List);
      }
      catch (NullReferenceException) { /* Do Nothing */ }

      try
      {
        if (m_context.ListItem != null)
          this.ListItem = new SPListItemInstance(this.Engine.Object.InstancePrototype, m_context.ListItem);
      }
      catch (NullReferenceException) { /* Do Nothing */ }

      try
      {
        if (m_context.View != null)
          this.View = new SPViewInstance(this.Engine.Object.InstancePrototype, m_context.View);
      }
      catch (NullReferenceException) { /* Do Nothing */ }
      
      this.PopulateFields();
      this.PopulateFunctions();
    }

    [JSProperty(Name = "serverVersion")]
    public string ServerVersion
    {
      get { return SPFarm.Local.BuildVersion.ToString(); }
    }

    [JSProperty(Name = "list")]
    public SPListInstance List
    {
      get;
      private set;
    }

    [JSProperty(Name = "listItem")]
    public SPListItemInstance ListItem
    {
      get;
      private set;
    }

    [JSProperty(Name = "site")]
    public SPSiteInstance Site
    {
      get;
      private set;
    }

    [JSProperty(Name = "view")]
    public SPViewInstance View
    {
      get;
      private set;
    }

    [JSProperty(Name = "web")]
    public SPWebInstance Web
    {
      get;
      private set;
    }
  }
}
