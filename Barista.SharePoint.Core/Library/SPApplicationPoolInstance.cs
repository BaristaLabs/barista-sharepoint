namespace Barista.SharePoint.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using Microsoft.SharePoint.Administration;
  using System;

  [Serializable]
  public class SPApplicationPoolConstructor : ClrFunction
  {
    public SPApplicationPoolConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPApplicationPool", new SPApplicationPoolInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPApplicationPoolInstance Construct()
    {
      return new SPApplicationPoolInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPApplicationPoolInstance : ObjectInstance
  {
    private readonly SPApplicationPool m_applicationPool;

    public SPApplicationPoolInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPApplicationPoolInstance(ObjectInstance prototype, SPApplicationPool applicationPool)
      : this(prototype)
    {
      if (applicationPool == null)
        throw new ArgumentNullException("applicationPool");

      m_applicationPool = applicationPool;
    }

    public SPApplicationPool SPApplicationPool
    {
      get { return m_applicationPool; }
    }

    //TODO: CurrentIdentityType, CurrentSecurityIdentifier

    [JSProperty(Name = "displayName")]
    public string DisplayName
    {
      get
      {
        return m_applicationPool.DisplayName;
      }
    }

    [JSProperty(Name = "id")]
    public GuidInstance Id
    {
      get
      {
        return new GuidInstance(this.Engine.Object.InstancePrototype, m_applicationPool.Id);
      }
    }

    [JSProperty(Name = "isCredentialDeploymentEnabled")]
    public bool IsCredentialDeploymentEnabled
    {
      get
      {
        return m_applicationPool.IsCredentialDeploymentEnabled;
      }
      set
      {
        m_applicationPool.IsCredentialDeploymentEnabled = value;
      }
    }

    [JSProperty(Name = "isCredentialUpdateEnabled")]
    public bool IsCredentialUpdateEnabled
    {
      get
      {
        return m_applicationPool.IsCredentialUpdateEnabled;
      }
      set
      {
        m_applicationPool.IsCredentialUpdateEnabled = value;
      }
    }

    [JSProperty(Name = "managedAccount")]
    public SPManagedAccountInstance ManagedAcount
    {
      get
      {
        return new SPManagedAccountInstance(this.Engine.Object.InstancePrototype, m_applicationPool.ManagedAccount);
      }
      set
      {
        if (value != null)
        {
          m_applicationPool.ManagedAccount = value.SPManagedAccount;
        }
      }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get
      {
        return m_applicationPool.Name;
      }
      set
      {
        m_applicationPool.Name = value;
      }
    }

    //Properties, ProcessAccount

    [JSProperty(Name = "typeName")]
    public string TypeName
    {
      get
      {
        return m_applicationPool.TypeName;
      }
    }
    
    //UpgradedPersistedProperties

    [JSProperty(Name = "username")]
    public string Username
    {
      get
      {
        return m_applicationPool.Username;
      }
      set
      {
        m_applicationPool.Username = value;
      }
    }

    [JSProperty(Name = "version")]
    public double Version
    {
      get
      {
        return m_applicationPool.Version;
      }
    }

    [JSFunction(Name = "delete")]
    public void Delete()
    {
      m_applicationPool.Delete();
    }

    [JSFunction(Name = "deploy")]
    public void Deploy()
    {
      m_applicationPool.Deploy();
    }

    [JSFunction(Name = "getFarm")]
    public SPFarmInstance GetFarm()
    {
      var farm = m_applicationPool.Farm;
      return farm == null
        ? null
        : new SPFarmInstance(this.Engine.Object.InstancePrototype, farm);
    }

    [JSFunction(Name = "provision")]
    public void Provision()
    {
      m_applicationPool.Provision();
    }

    [JSFunction(Name = "uncache")]
    public void Uncache()
    {
      m_applicationPool.Uncache();
    }

    [JSFunction(Name = "unprovision")]
    public void Unprovision()
    {
      m_applicationPool.Unprovision();
    }

    [JSFunction(Name = "update")]
    public void Update(object ensure)
    {
      if (ensure == null || ensure == Null.Value || ensure == Undefined.Value)
        m_applicationPool.Update();
      else
        m_applicationPool.Update(TypeConverter.ToBoolean(ensure));
    }
  }
}
