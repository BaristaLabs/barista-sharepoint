namespace Barista.SharePoint.Library
{
  using System.Linq;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Microsoft.SharePoint.Administration;
  using System;

  [Serializable]
  public class SPServerConstructor : ClrFunction
  {
    public SPServerConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPServer", new SPServerInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPServerInstance Construct()
    {
      return new SPServerInstance(InstancePrototype);
    }
  }

  [Serializable]
  public class SPServerInstance : ObjectInstance
  {
    private readonly SPServer m_server;

    public SPServerInstance(ObjectInstance prototype)
      : base(prototype)
    {
      PopulateFields();
      PopulateFunctions();
    }

    public SPServerInstance(ObjectInstance prototype, SPServer server)
      : this(prototype)
    {
      if (server == null)
        throw new ArgumentNullException("server");

      m_server = server;
    }

    public SPServer SPServer
    {
      get { return m_server; }
    }

    [JSProperty(Name = "address")]
    public string Address
    {
      get { return m_server.Address; }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_server.Name; }
    }

    [JSProperty(Name = "displayName")]
    public string DisplayName
    {
      get { return m_server.DisplayName; }
    }

    [JSProperty(Name = "role")]
    public string Role
    {
      get { return m_server.Role.ToString(); }
    }

    [JSFunction(Name = "getServiceInstances")]
    [JSDoc("ternReturnType", "[+SPService]")]
    public ArrayInstance GetServiceInstances()
    {
      var serviceInstances = m_server.ServiceInstances;
// ReSharper disable CoVariantArrayConversion
      return Engine.Array.Construct(serviceInstances
                                           .Select(
                                             si =>
                                             new SPServiceInstanceInstance(Engine.Object.InstancePrototype, si))
                                           .ToArray()
        );
// ReSharper restore CoVariantArrayConversion
    }
  }
}
