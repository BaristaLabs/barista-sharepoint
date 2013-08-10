namespace Barista.SharePoint.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Microsoft.SharePoint.Administration;
  using System;

  [Serializable]
  public class SPWebApplicationConstructor : ClrFunction
  {
    public SPWebApplicationConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPWebApplication", new SPWebApplicationInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPWebApplicationInstance Construct()
    {
      return new SPWebApplicationInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPWebApplicationInstance : ObjectInstance
  {
    private readonly SPWebApplication m_webApplication;

    public SPWebApplicationInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPWebApplicationInstance(ObjectInstance prototype, SPWebApplication webApplication)
      : this(prototype)
    {
      if (webApplication == null)
        throw new ArgumentNullException("webApplication");

      m_webApplication = webApplication;
    }

    public SPWebApplication SPWebApplication
    {
      get { return m_webApplication; }
    }

    [JSProperty(Name = "displayName")]
    public string DisplayName
    {
      get
      {
        return m_webApplication.DisplayName;
      }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get
      {
        return m_webApplication.Name;
      }
      set
      {
        m_webApplication.Name = value;
      }
    }
  }
}
