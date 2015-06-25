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
    public SPWindowsServiceInstance(ObjectInstance prototype)
      : base(prototype)
    {
    }

    public SPWindowsServiceInstance(ObjectInstance prototype, SPWindowsService windowsService)
      : base(prototype, windowsService)
    {
      if (windowsService == null)
        throw new ArgumentNullException("windowsService");
    }

    public SPWindowsService SPWindowsService
    {
      get { return SPService as SPWindowsService; }
    }

    [JSProperty(Name = "processIdentity")]
    public object ProcessIdentity
    {
      get
      {
        if (this.SPWindowsService.ProcessIdentity == null)
          return Null.Value;

        return new SPProcessIdentityInstance(this.Engine.Object.InstancePrototype, this.SPWindowsService.ProcessIdentity);
      }
    }
  }
}
