namespace Barista.SharePoint.DocumentStore.Library
{
  using Barista.DocumentStore;
  using Barista.Library;
  using Jurassic;
  using Jurassic.Library;
  using System;

  [Serializable]
  public class EntityPartInstance : ObjectInstance
  {
    private readonly EntityPart m_entityPart;

    public EntityPartInstance(ScriptEngine engine, EntityPart entityPart)
      : base(engine)
    {
      if (entityPart == null)
        throw new ArgumentNullException("entityPart");

      m_entityPart = entityPart;

      this.PopulateFields();
      this.PopulateFunctions();
    }

    public EntityPart EntityPart
    {
      get { return m_entityPart; }
    }

    [JSProperty(Name = "entityId")]
    public GuidInstance EntityId
    {
      get { return new GuidInstance(this.Engine.Object.InstancePrototype, m_entityPart.EntityId); }
      set
      {
        if (value != null)
          m_entityPart.EntityId = value.Value;
      }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_entityPart.Name; }
      set { m_entityPart.Name = value; }
    }

    [JSProperty(Name = "category")]
    public string Category
    {
      get { return m_entityPart.Category; }
      set { m_entityPart.Category = value; }
    }

    [JSProperty(Name = "eTag")]
    public string ETag
    {
      get { return m_entityPart.ETag; }
      set { m_entityPart.ETag = value; }
    }

    [JSProperty(Name = "data")]
    public object Data
    {
      get
      {
        try
        {
          // ReSharper disable RedundantArgumentDefaultValue
          var result = JSONObject.Parse(this.Engine, m_entityPart.Data, null);
          // ReSharper restore RedundantArgumentDefaultValue
          return result;
        }
        catch (Exception)
        {
          //If there was an issue converting to a JSON object, just return the string value.
          return m_entityPart.Data;
        }
      }
      set
      {

        if (value == Null.Value || value == Undefined.Value || value == null)
          m_entityPart.Data = String.Empty;
        else if (value is string || value is StringInstance || value is ConcatenatedString)
          m_entityPart.Data = value.ToString();
        else if (value is ObjectInstance)
          // ReSharper disable RedundantArgumentDefaultValue
          m_entityPart.Data = JSONObject.Stringify(this.Engine, value, null, null);
        // ReSharper restore RedundantArgumentDefaultValue
        else
          m_entityPart.Data = value.ToString();
      }
    }

    [JSProperty(Name = "created")]
    public DateInstance Created
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_entityPart.Created); }
      set { m_entityPart.Created = DateTime.Parse(value.ToISOString()); }
    }

    [JSProperty(Name = "createdBy")]
    public object CreatedBy
    {
      get
      {
        if (m_entityPart.CreatedBy == null)
          return Null.Value;

        return m_entityPart.CreatedBy.LoginName;
      }
    }

    [JSProperty(Name = "modified")]
    public DateInstance Modified
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_entityPart.Modified); }
      set { m_entityPart.Modified = DateTime.Parse(value.ToISOString()); }
    }

    [JSProperty(Name = "modifiedBy")]
    public object ModifiedBy
    {
      get
      {
        if (m_entityPart.ModifiedBy == null)
          return Null.Value;

        return m_entityPart.ModifiedBy.LoginName;
      }
    }
  }
}
