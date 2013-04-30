namespace Barista.SharePoint.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Microsoft.SharePoint.Administration;
  using System;

  [Serializable]
  public class SPWindowsServiceConstructor : ClrFunction
  {
    public SPWindowsServiceConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPWindowsService", new SPWindowsServiceInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPWindowsServiceInstance Construct()
    {
      return new SPWindowsServiceInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPWindowsServiceInstance : SPServiceInstance
  {
    private readonly SPWindowsService m_windowsService;

    public SPWindowsServiceInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPWindowsServiceInstance(ObjectInstance prototype, SPWindowsService windowsService)
      : this(prototype)
    {
      if (windowsService == null)
        throw new ArgumentNullException("windowsService");

      m_windowsService = windowsService;
    }

    public SPWindowsService SPWindowsService
    {
      get { return m_windowsService; }
    }

    [JSProperty(Name = "processIdentity")]
    public SPProcessIdentityInstance ProcessIdentity
    {
      get { return new SPProcessIdentityInstance(this.Engine.Object.InstancePrototype, m_windowsService.ProcessIdentity); }
    }
  }
}
