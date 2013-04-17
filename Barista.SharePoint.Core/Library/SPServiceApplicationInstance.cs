namespace Barista.SharePoint.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using Microsoft.SharePoint.Administration;
  using System;

  [Serializable]
  public class SPServiceApplicationConstructor : ClrFunction
  {
    public SPServiceApplicationConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPServiceApplication", new SPServiceApplicationInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPServiceApplicationInstance Construct()
    {
      return new SPServiceApplicationInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPServiceApplicationInstance : ObjectInstance
  {
    private readonly SPServiceApplication m_serviceApplication;

    public SPServiceApplicationInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPServiceApplicationInstance(ObjectInstance prototype, SPServiceApplication sPServiceApplication)
      : this(prototype)
    {
      if (sPServiceApplication == null)
        throw new ArgumentNullException("sPServiceApplication");

      m_serviceApplication = sPServiceApplication;
    }

    public SPServiceApplication SPServiceApplication
    {
      get { return m_serviceApplication; }
    }

    [JSProperty(Name = "id")]
    public GuidInstance Id
    {
      get { return new GuidInstance(this.Engine.Object.InstancePrototype, m_serviceApplication.Id); }
      set { m_serviceApplication.Id = value.Value; }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_serviceApplication.Name; }
      set { m_serviceApplication.Name = value; }
    }

    [JSProperty(Name = "displayName")]
    public string DisplayName
    {
      get { return m_serviceApplication.DisplayName; }
    }

    [JSProperty(Name = "manageLink")]
    public string ManageLink
    {
      get { return m_serviceApplication.ManageLink.Url; }
    }

    [JSProperty(Name = "propertiesLink")]
    public string PropertiesLink
    {
      get { return m_serviceApplication.PropertiesLink.Url; }
    }

    [JSFunction(Name = "getPropertyKeyValue")]
    public object GetFarmKeyValueAsObject(string key)
    {
      string val = Convert.ToString(m_serviceApplication.Properties[key]);

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

      if (m_serviceApplication.Properties.ContainsKey(key))
        m_serviceApplication.Properties[key] = stringValue;
      else
        m_serviceApplication.Properties.Add(key, stringValue);

      m_serviceApplication.Update();
    }

    [JSFunction(Name = "update")]
    public void Update()
    {
      m_serviceApplication.Update();
    }
  }
}
