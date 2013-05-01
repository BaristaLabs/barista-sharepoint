namespace Barista.SharePoint.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using Microsoft.SharePoint.Administration;
  using System;

  [Serializable]
  public class SPManagedAccountConstructor : ClrFunction
  {
    public SPManagedAccountConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPManagedAccount", new SPManagedAccountInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPManagedAccountInstance Construct()
    {
      return new SPManagedAccountInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPManagedAccountInstance : ObjectInstance
  {
    private readonly SPManagedAccount m_managedAccount;

    public SPManagedAccountInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPManagedAccountInstance(ObjectInstance prototype, SPManagedAccount managedAccount)
      : this(prototype)
    {
      if (managedAccount == null)
        throw new ArgumentNullException("managedAccount");

      m_managedAccount = managedAccount;
    }

    public SPManagedAccount SPManagedAccount
    {
      get { return m_managedAccount; }
    }

    [JSProperty(Name = "automaticChange")]
    public bool AutomaticChange
    {
      get { return m_managedAccount.AutomaticChange; }
    }

    [JSProperty(Name = "canChangePassword")]
    public bool CanChangePassword
    {
      get { return m_managedAccount.CanChangePassword; }
    }

    [JSProperty(Name = "displayName")]
    public string DisplayName
    {
      get { return m_managedAccount.DisplayName; }
    }

    [JSProperty(Name = "enableEmailBeforePasswordChange")]
    public bool EnableEmailBeforePasswordChange
    {
      get { return m_managedAccount.EnableEmailBeforePasswordChange; }
    }

    [JSProperty(Name = "id")]
    public GuidInstance Id
    {
      get { return new GuidInstance(this.Engine.Object.InstancePrototype, m_managedAccount.Id); }
    }

    [JSProperty(Name = "passwordExpiration")]
    public DateInstance PasswordExpiration
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_managedAccount.PasswordExpiration); }
    }

    [JSProperty(Name = "passwordLastChanged")]
    public DateInstance PasswordLastChanged
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_managedAccount.PasswordLastChanged); }
    }

    [JSProperty(Name = "sid")]
    public string Sid
    {
      get { return m_managedAccount.Sid.ToString(); }
    }

    [JSProperty(Name = "typeName")]
    public string TypeName
    {
      get { return m_managedAccount.TypeName; }
    }

    [JSProperty(Name = "username")]
    public string Username
    {
      get { return m_managedAccount.Username; }
    }
  }
}
