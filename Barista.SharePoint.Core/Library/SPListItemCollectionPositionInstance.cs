namespace Barista.SharePoint.Library
{
  using System;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;

  [Serializable]
  public class SPListItemCollectionPositionConstructor : ClrFunction
  {
    public SPListItemCollectionPositionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPListItemCollectionPosition", new SPListItemCollectionPositionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPListItemCollectionPositionInstance Construct(string data)
    {
      if (String.IsNullOrEmpty(data) || data == Undefined.Value.ToString())
       throw new JavaScriptException(this.Engine, "Error", "Position data must be specified.");

      return new SPListItemCollectionPositionInstance(this.Engine.Object.InstancePrototype, new SPListItemCollectionPosition(data));
    }

    public SPListItemCollectionPositionInstance Construct(SPListItemCollectionPosition position)
    {
      if (position == null)
        throw new ArgumentNullException("position");

      return new SPListItemCollectionPositionInstance(this.InstancePrototype, position);
    }
  }

  [Serializable]
  public class SPListItemCollectionPositionInstance : ObjectInstance
  {
    private SPListItemCollectionPosition m_position;

    public SPListItemCollectionPositionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPListItemCollectionPositionInstance(ObjectInstance prototype, SPListItemCollectionPosition position)
      : this(prototype)
    {
      this.m_position = position;
    }

    #region Properties
    public SPListItemCollectionPosition ListItemCollectionPosition
    {
      get { return m_position; }
    }

    [JSProperty(Name= "pagingInfo")]
    public string PagingInfo
    {
      get { return m_position.PagingInfo; }
      set { m_position.PagingInfo = value; }
    }
    #endregion
  }
}
