namespace Barista.Library
{
  using System.Linq;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Microsoft.Web.Administration;

  [Serializable]
  public class ServerManagerConstructor : ClrFunction
  {
    public ServerManagerConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ServerManager", new ServerManagerInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ServerManagerInstance Construct()
    {
      return new ServerManagerInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class ServerManagerInstance : ObjectInstance
  {
    private readonly ServerManager m_serverManager;

    public ServerManagerInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ServerManagerInstance(ObjectInstance prototype, ServerManager serverManager)
      : this(prototype)
    {
      if (serverManager == null)
        throw new ArgumentNullException("serverManager");

      m_serverManager = serverManager;
    }

    public ServerManager ServerManager
    {
      get { return m_serverManager; }
    }

    [JSProperty(Name = "applicationPools")]
    public ArrayInstance ApplicationPools
    {
      get
      {
        // ReSharper disable CoVariantArrayConversion
        return this.Engine.Array.Construct(m_serverManager.ApplicationPools
                                                          .Select(applicationPool =>
                                                                  new ApplicationPoolInstance(
                                                                    this.Engine.Object.InstancePrototype,
                                                                    applicationPool)
                                             )
                                                          .ToArray());
        // ReSharper restore CoVariantArrayConversion
      }
    }
  }
}
