namespace Barista.SharePoint.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Barista.Library;
  using Microsoft.SharePoint.Administration;

  [Serializable]
  public class SPIisWebServiceApplicationPoolConstructor : ClrFunction
  {
    public SPIisWebServiceApplicationPoolConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPIisWebServiceApplicationPool", new SPIisWebServiceApplicationPoolInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPIisWebServiceApplicationPoolInstance Construct()
    {
      return new SPIisWebServiceApplicationPoolInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPIisWebServiceApplicationPoolInstance : ObjectInstance
  {
    private readonly SPIisWebServiceApplicationPool m_iisWebServiceApplicationPool;

    public SPIisWebServiceApplicationPoolInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPIisWebServiceApplicationPoolInstance(ObjectInstance prototype, SPIisWebServiceApplicationPool iisWebServiceApplicationPool)
      : this(prototype)
    {
      if (iisWebServiceApplicationPool == null)
        throw new ArgumentNullException("iisWebServiceApplicationPool");

      m_iisWebServiceApplicationPool = iisWebServiceApplicationPool;
    }

    public SPIisWebServiceApplicationPool SPIisWebServiceApplicationPool
    {
      get { return m_iisWebServiceApplicationPool; }
    }

    [JSProperty(Name = "id")]
    public GuidInstance Id
    {
      get { return new GuidInstance(this.Engine.Object.InstancePrototype, m_iisWebServiceApplicationPool.Id); }
      set { m_iisWebServiceApplicationPool.Id = value.Value; }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_iisWebServiceApplicationPool.Name; }
    }

    [JSProperty(Name = "processAccount")]
    public SPProcessAccountInstance ProcessAccount
    {
      get
      {
        if (m_iisWebServiceApplicationPool.ProcessAccount == null)
          return null;
        return new SPProcessAccountInstance(this.Engine.Object.InstancePrototype,
          m_iisWebServiceApplicationPool.ProcessAccount);
      }
    }

    [JSProperty(Name = "displayName")]
    public string DisplayName
    {
      get { return m_iisWebServiceApplicationPool.DisplayName; }
    }

    [JSProperty(Name = "status")]
    public string Status
    {
      get { return m_iisWebServiceApplicationPool.Status.ToString(); }
    }

    [JSFunction(Name = "getPropertyKeyValue")]
    public object GetFarmKeyValueAsObject(string key)
    {
      string val = Convert.ToString(m_iisWebServiceApplicationPool.Properties[key]);

      object result;

      //Attempt to convert the string into a JSON Object.
      try
      {
        result = JSONObject.Parse(this.Engine, val, null);
      }
      catch
      {
        result = val;
      }

      return result;
    }

    [JSFunction(Name = "setPropertyKeyValue")]
    public void SetPropertyKeyValue(string key, object value)
    {
      if (value == null || value == Undefined.Value || value == Null.Value)
        throw new ArgumentNullException("value");

      string stringValue;
      if (value is ObjectInstance)
      {
        stringValue = JSONObject.Stringify(this.Engine, value, null, null);
      }
      else
      {
        stringValue = value.ToString();
      }

      if (m_iisWebServiceApplicationPool.Properties.ContainsKey(key))
        m_iisWebServiceApplicationPool.Properties[key] = stringValue;
      else
        m_iisWebServiceApplicationPool.Properties.Add(key, stringValue);

      m_iisWebServiceApplicationPool.Update();
    }

    [JSFunction(Name = "update")]
    public void Update()
    {
      m_iisWebServiceApplicationPool.Update();
    }
  }
}
