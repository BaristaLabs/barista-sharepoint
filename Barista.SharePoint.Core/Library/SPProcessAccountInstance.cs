namespace Barista.SharePoint.Library
{
  using System.Linq;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Microsoft.SharePoint.Administration;

  [Serializable]
  public class SPProcessAccountConstructor : ClrFunction
  {
    public SPProcessAccountConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPProcessAccount", new SPProcessAccountInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPProcessAccountInstance Construct()
    {
      return new SPProcessAccountInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPProcessAccountInstance : ObjectInstance
  {
    private readonly SPProcessAccount m_processAccount;

    public SPProcessAccountInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPProcessAccountInstance(ObjectInstance prototype, SPProcessAccount processAccount)
      : this(prototype)
    {
      if (processAccount == null)
        throw new ArgumentNullException("processAccount");

      m_processAccount = processAccount;
    }

    public SPProcessAccount SPProcessAccount
    {
      get { return m_processAccount; }
    }

    [JSProperty(Name = "managedAccount")]
    public SPManagedAccountInstance ManagedAccount
    {
      get
      {
        var farmManagedAccountCollection = new SPFarmManagedAccountCollection(SPFarm.Local);
        var managedAccount = farmManagedAccountCollection.FirstOrDefault(ma => ma.Sid == m_processAccount.SecurityIdentifier);
        return managedAccount == null
          ? null
          : new SPManagedAccountInstance(this.Engine.Object.InstancePrototype, managedAccount);
      }
    }

    [JSProperty(Name = "securityIdentifier")]
    public string SecurityIdentifier
    {
      get
      {
        return m_processAccount.SecurityIdentifier.ToString();
      }
    }
  }
}
