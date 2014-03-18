namespace Barista.DocumentStore.Library
{
  using Barista.DocumentStore;
  using Jurassic;
  using Jurassic.Library;
  using System;

  [Serializable]
  public class FolderInstance : ObjectInstance, IFolder
  {
    private readonly IFolder m_folder;

    public FolderInstance(ScriptEngine engine, IFolder folder)
      : base(engine)
    {
      if (folder == null)
        throw new ArgumentNullException("folder");

      m_folder = folder;

      this.PopulateFields();
      this.PopulateFunctions();
    }

    public IFolder Folder
    {
      get { return m_folder; }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_folder.Name; }
      set { m_folder.Name = value; }
    }

     [JSProperty(Name = "fullPath")]
    public string FullPath
    {
      get { return m_folder.FullPath; }
      set { m_folder.FullPath = value; }
    }

    [JSProperty(Name = "created")]
    public DateInstance Created
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_folder.Created); }
      set { m_folder.Created = DateTime.Parse(value.ToIsoString()); }
    }

    DateTime IFolder.Created
    {
      get { return m_folder.Created; }
      set { m_folder.Created = value; }
    }

    [JSProperty(Name = "createdBy")]
    public object CreatedBy
    {
      get
      {
        if (m_folder.CreatedBy == null)
          return Null.Value;

        return m_folder.CreatedBy.LoginName;
      }
    }

    IUser IFolder.CreatedBy
    {
      get { return m_folder.CreatedBy; }
      set { m_folder.CreatedBy = value; }
    }

    [JSProperty(Name = "modified")]
    public DateInstance Modified
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_folder.Modified); }
      set { m_folder.Modified = DateTime.Parse(value.ToIsoString()); }
    }

    DateTime IFolder.Modified
    {
      get { return m_folder.Modified; }
      set { m_folder.Modified = value; }
    }

    [JSProperty(Name = "modifiedBy")]
    public object ModifiedBy
    {
      get
      {
        if (m_folder.ModifiedBy == null)
          return Null.Value;

        return m_folder.ModifiedBy.LoginName;
      }
    }

    IUser IFolder.ModifiedBy
    {
      get { return m_folder.ModifiedBy; }
      set { m_folder.ModifiedBy = value; }
    }


    [JSProperty(Name = "entityCount")]
    public int EntityCount
    {
      get { return m_folder.EntityCount; }
    }
  }
}
