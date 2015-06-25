namespace Barista.SharePoint.Migration.Library
{
  using Barista.Extensions;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using Barista.Newtonsoft.Json;
  using Microsoft.SharePoint.Deployment;
  using System;

  [Serializable]
  public class SPExportObjectConstructor : ClrFunction
  {
    public SPExportObjectConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPExportObject", new SPExportObjectInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPExportObjectInstance Construct()
    {
      return new SPExportObjectInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPExportObjectInstance : ObjectInstance
  {
    private readonly SPExportObject m_exportObject;

    public SPExportObjectInstance(ObjectInstance prototype)
      : base(prototype)
    {
      m_exportObject = new SPExportObject();

      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPExportObjectInstance(ObjectInstance prototype, SPExportObject exportObject)
      : this(prototype)
    {
      if (exportObject == null)
        throw new ArgumentNullException("exportObject");

      m_exportObject = exportObject;
    }

    public SPExportObject SPExportObject
    {
      get { return m_exportObject; }
    }

    [JSProperty(Name = "exportChildren")]
    [JsonProperty("exportChildren")]
    public bool ExcludeChildren
    {
      get
      {
        return m_exportObject.ExcludeChildren;
      }
      set
      {
        m_exportObject.ExcludeChildren = value;
      }
    }

    [JSProperty(Name = "exportChangeToken")]
    [JsonProperty("exportChangeToken")]
    public string ExportChangeToken
    {
      get
      {
        return m_exportObject.ExportChangeToken;
      }
      set
      {
        m_exportObject.ExportChangeToken = value;
      }
    }

    [JSProperty(Name = "id")]
    [JsonProperty("id")]
    public object Id
    {
      get
      {
        return new GuidInstance(this.Engine.Object.InstancePrototype, m_exportObject.Id);
      }
      set
      {
        var guid = GuidInstance.ConvertFromJsObjectToGuid(value);
        m_exportObject.Id = guid;
      }
    }

    [JSProperty(Name = "includeDescendants")]
    [JsonProperty("includeDescendants")]
    [JSDoc("Gets or sets a value that indicates what types of descendents to include. Possible values are: All, Content, None")]
    public string IncludeDescendants
    {
      get
      {
        return m_exportObject.IncludeDescendants.ToString();
      }
      set
      {
        SPIncludeDescendants incDesc;

        if (value.TryParseEnum(true, out incDesc))
          m_exportObject.IncludeDescendants = incDesc;
      }
    }

    [JSProperty(Name = "parentId")]
    [JsonProperty("parentId")]
    public object ParentId
    {
      get
      {
        return new GuidInstance(this.Engine.Object.InstancePrototype, m_exportObject.ParentId);
      }
      set
      {
        var guid = GuidInstance.ConvertFromJsObjectToGuid(value);
        m_exportObject.ParentId = guid;
      }
    }

    [JSProperty(Name = "type")]
    [JsonProperty("type")]
    [JSDoc("Gets or sets the type of object that the ID references. Possible Values: Invalid, List, ListItem, Site, Web")]
    public string Type
    {
      get
      {
        return m_exportObject.Type.ToString();
      }
      set
      {
        SPDeploymentObjectType deploymentObjectType;

        if (value.TryParseEnum(true, out deploymentObjectType))
          m_exportObject.Type = deploymentObjectType;
      }
    }

    [JSProperty(Name = "url")]
    [JsonProperty("url")]
    public string Url
    {
      get
      {
        return m_exportObject.Url;
      }
      set
      {
        m_exportObject.Url = value;
      }
    }
  }
}
