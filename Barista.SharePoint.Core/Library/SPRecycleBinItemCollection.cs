namespace Barista.SharePoint.Library
{
  using System;
  using System.Linq;
  using Barista.Library;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;

  [Serializable]
  public class SPRecycleBinItemCollectionConstructor : ClrFunction
  {
    public SPRecycleBinItemCollectionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPRecycleBinItemCollection", new SPRecycleBinItemCollectionInstance(engine.Object.InstancePrototype))
    {
    }

    public SPRecycleBinItemCollectionInstance Construct(SPRecycleBinItemCollection recycleBinItemCollection)
    {
      if (recycleBinItemCollection == null)
        throw new ArgumentNullException("recycleBinItemCollection");

      return new SPRecycleBinItemCollectionInstance(this.InstancePrototype, recycleBinItemCollection);
    }
  }

  [Serializable]
  public class SPRecycleBinItemCollectionInstance : ObjectInstance
  {
    private readonly SPRecycleBinItemCollection m_recycleBinItemCollection;

    public SPRecycleBinItemCollectionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPRecycleBinItemCollectionInstance(ObjectInstance prototype, SPRecycleBinItemCollection recycleBinItemCollection)
      : this(prototype)
    {
      this.m_recycleBinItemCollection = recycleBinItemCollection;
    }

    #region Properties

    [JSProperty(Name = "binType")]
    public string BinType
    {
      get { return m_recycleBinItemCollection.BinType.ToString(); }
    }

    [JSProperty(Name = "count")]
    public object Count
    {
        get
        {
            try
            {
                return m_recycleBinItemCollection.Count;
            }
            catch
            {
                return Undefined.Value;
            }
            
        }
    }

    //TODO: RecycleBinListItemCollectionPosition...

    [JSProperty(Name = "lastProcessedId")]
    public string LastProcessedId
    {
      get { return m_recycleBinItemCollection.LastProcessedId.ToString(); }
    }
    #endregion

    [JSFunction(Name = "deleteAll")]
    public void DeleteAll()
    {
      m_recycleBinItemCollection.DeleteAll();
    }

    [JSFunction(Name = "getItemById")]
    public SPRecycleBinItemInstance GetItemById(object id)
    {
      var guid = GuidInstance.ConvertFromJsObjectToGuid(id);
      return new SPRecycleBinItemInstance(this.Engine.Object.InstancePrototype, m_recycleBinItemCollection.GetItemById(guid));
    }

    [JSFunction(Name = "toArray")]
    public ArrayInstance ToArray()
    {
      var result = this.Engine.Array.Construct();
      foreach (var item in m_recycleBinItemCollection.OfType<SPRecycleBinItem>())
      {
        ArrayInstance.Push(result, new SPRecycleBinItemInstance(this.Engine.Object.InstancePrototype, item));
      }
      return result;
    }

    [JSFunction(Name = "moveAllToSecondStage")]
    public void MoveAllToSecondStage()
    {
      m_recycleBinItemCollection.MoveAllToSecondStage();
    }

    [JSFunction(Name = "restoreAll")]
    public void RestoreAll()
    {
      m_recycleBinItemCollection.RestoreAll();
    }
  }
}
