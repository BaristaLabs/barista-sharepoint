namespace Barista.SharePoint.Library
{
  using Barista.DocumentStore;
  using Jurassic;
  using Jurassic.Library;
  using System;

  public class FolderInstance : ObjectInstance
  {
    Folder m_folder;

    public FolderInstance(ScriptEngine engine, Folder folder)
      : base(engine)
    {
      if (folder == null)
        throw new ArgumentNullException("folder");

      m_folder = folder;

      this.PopulateFields();
      this.PopulateFunctions();
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

    [JSProperty(Name = "entityCount")]
    public int EntityCount
    {
      get { return m_folder.EntityCount; }
      set { m_folder.EntityCount = value; }
    }
  }
}
