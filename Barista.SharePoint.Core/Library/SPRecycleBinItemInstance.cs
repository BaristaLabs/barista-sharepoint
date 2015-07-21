namespace Barista.SharePoint.Library
{
  using System;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;

  [Serializable]
  public class SPRecycleBinItemConstructor : ClrFunction
  {
    public SPRecycleBinItemConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPRecycleBinItem", new SPRecycleBinItemInstance(engine.Object.InstancePrototype))
    {
    }

    public SPRecycleBinItemInstance Construct(SPRecycleBinItem recycleBinItem)
    {
      if (recycleBinItem == null)
        throw new ArgumentNullException("recycleBinItem");

      return new SPRecycleBinItemInstance(this.InstancePrototype, recycleBinItem);
    }
  }

  [Serializable]
  public class SPRecycleBinItemInstance : ObjectInstance
  {
    private readonly SPRecycleBinItem m_recycleBinItem;

    public SPRecycleBinItemInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPRecycleBinItemInstance(ObjectInstance prototype, SPRecycleBinItem recycleBinItem)
      : this(prototype)
    {
      this.m_recycleBinItem = recycleBinItem;
    }

    #region Properties

    [JSProperty(Name = "author")]
    public SPUserInstance Author
    {
      get
      {
        return new SPUserInstance(Engine, m_recycleBinItem.Author);
      }
    }

    [JSProperty(Name = "authorEmail")]
    public string AuthorEmail
    {
      get { return m_recycleBinItem.AuthorEmail; }
    }

    [JSProperty(Name = "authorId")]
    public int AuthorId
    {
      get { return m_recycleBinItem.AuthorId; }
    }

    [JSProperty(Name = "authorName")]
    public string AuthorName
    {
      get { return m_recycleBinItem.AuthorName; }
    }

    [JSProperty(Name = "deletedBy")]
    public SPUserInstance DeletedBy
    {
        get { return new SPUserInstance(Engine, m_recycleBinItem.DeletedBy); }
    }

    [JSProperty(Name = "deletedByEmail")]
    public string DeletedByEmail
    {
      get { return m_recycleBinItem.DeletedByEmail; }
    }

    [JSProperty(Name = "deletedById")]
    public int DeletedById
    {
      get { return m_recycleBinItem.DeletedById; }
    }

    [JSProperty(Name = "deletedByName")]
    public string DeletedByName
    {
      get { return m_recycleBinItem.DeletedByName; }
    }

    [JSProperty(Name = "deletedDate")]
    public DateInstance DeletedDate
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_recycleBinItem.DeletedDate); }
    }

    [JSDoc("Gets the site-relative url of the list or folder that originally contained the item.")]
    [JSProperty(Name = "dirName")]
    public string DirName
    {
      get { return m_recycleBinItem.DirName; }
    }

    [JSProperty(Name = "id")]
    public string Id
    {
      get { return m_recycleBinItem.ID.ToString(); }
    }

    [JSProperty(Name = "imageUrl")]
    public string ImageUrl
    {
      get { return m_recycleBinItem.ImageUrl; }
    }

    [JSProperty(Name = "itemState")]
    public string ItemState
    {
      get { return m_recycleBinItem.ItemState.ToString(); }
    }

    [JSProperty(Name = "itemType")]
    public string ItemType
    {
      get { return m_recycleBinItem.ItemType.ToString(); }
    }

    [JSProperty(Name = "leafName")]
    public string LeafName
    {
      get { return m_recycleBinItem.LeafName; }
    }

    [JSProperty(Name = "progId")]
    public string ProgId
    {
      get { return m_recycleBinItem.ProgId; }
    }

    [JSProperty(Name = "size")]
    public double Size
    {
      get { return m_recycleBinItem.Size; }
    }

    [JSProperty(Name = "title")]
    public string Title
    {
      get { return m_recycleBinItem.Title; }
    }
    #endregion

    [JSFunction(Name = "delete")]
    public void Delete()
    {
      m_recycleBinItem.Delete();
    }

    [JSFunction(Name = "getWeb")]
    public SPWebInstance GetWeb()
    {
      return new SPWebInstance(this.Engine, m_recycleBinItem.Web);
    }

    [JSFunction(Name = "moveToSecondStage")]
    public void MoveToSecondStage()
    {
      m_recycleBinItem.MoveToSecondStage();
    }

    [JSFunction(Name = "restore")]
    public void Restore()
    {
      m_recycleBinItem.Restore();
    }
  }
}
