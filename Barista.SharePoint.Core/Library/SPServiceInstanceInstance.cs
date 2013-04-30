namespace Barista.SharePoint.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;

  [Serializable]
  public class SPServiceInstanceConstructor : ClrFunction
  {
    public SPServiceInstanceConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPServiceInstance", new SPServiceInstanceInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPServiceInstanceInstance Construct()
    {
      return new SPServiceInstanceInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPServiceInstanceInstance : ObjectInstance
  {
    private readonly Microsoft.SharePoint.Administration.SPServiceInstance m_serviceInstance;

    public SPServiceInstanceInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPServiceInstanceInstance(ObjectInstance prototype, Microsoft.SharePoint.Administration.SPServiceInstance serviceInstance)
      : this(prototype)
    {
      if (serviceInstance == null)
        throw new ArgumentNullException("serviceInstance");

      m_serviceInstance = serviceInstance;
    }

    public Microsoft.SharePoint.Administration.SPServiceInstance SPServiceInstance
    {
      get { return m_serviceInstance; }
    }

    [JSProperty(Name = "description")]
    public string Description
    {
      get { return m_serviceInstance.Description; }
    }

    [JSProperty(Name = "displayName")]
    public string DisplayName
    {
      get { return m_serviceInstance.DisplayName; }
    }

    [JSProperty(Name = "hidden")]
    public bool Hidden
    {
      get { return m_serviceInstance.Hidden; }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_serviceInstance.Name; }
    }

    [JSProperty(Name = "manageLinkUrl")]
    public string ManageLink
    {
      get
      {
        if (m_serviceInstance.ManageLink != null)
          return m_serviceInstance.ManageLink.Url;
        return String.Empty;
      }
    }

    [JSProperty(Name = "provisionLinkUrl")]
    public string ProvisionLinkUrl
    {
      get
      {
        if (m_serviceInstance.ProvisionLink != null)
          return m_serviceInstance.ProvisionLink.Url;
        return string.Empty;
      }
    }

    [JSProperty(Name = "server")]
    public object Server
    {
      get
      {
        if (m_serviceInstance.Server == null)
          return Null.Value;

        return new SPServerInstance(this.Engine.Object.InstancePrototype, m_serviceInstance.Server);
      }
    }

    [JSProperty(Name = "status")]
    public string Status
    {
      get { return m_serviceInstance.Status.ToString(); }
    }

    [JSProperty(Name = "systemService")]
    public bool SystemService
    {
      get { return m_serviceInstance.SystemService; }
    }

    [JSProperty(Name = "typeName")]
    public string TypeName
    {
      get { return m_serviceInstance.TypeName; }
    }

    [JSProperty(Name = "unprovisionLinkUrl")]
    public string UnprovisionLinkUrl
    {
      get
      {
        if (m_serviceInstance.UnprovisionLink != null)
          return m_serviceInstance.UnprovisionLink.Url;
        return string.Empty;
      }
    }
  }
}
