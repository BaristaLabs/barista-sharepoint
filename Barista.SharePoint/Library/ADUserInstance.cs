namespace Barista.SharePoint.Library
{
  using System;
  using System.IO;
  using System.Web;
  using Jurassic;
  using Jurassic.Library;
  using Barista.DirectoryServices;

  public class ADUserConstructor : ClrFunction
  {
    public ADUserConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ADUser", new ADUserInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ADUserInstance Construct()
    {
      ADUser user = new ADUser();

      return new ADUserInstance(this.InstancePrototype, user);
    }

    public ADUserInstance Construct(ADUser user)
    {
      if (user == null)
        throw new ArgumentNullException("user");

      return new ADUserInstance(this.InstancePrototype, user);
    }
  }

  public class ADUserInstance : ObjectInstance
  {
    private ADUser m_user;

    public ADUserInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ADUserInstance(ObjectInstance prototype, ADUser user)
      : this(prototype)
    {
      if (user == null)
        throw new ArgumentNullException("user");

      this.m_user = user;
    }

    #region General
    [JSProperty(Name = "rawSid")]
    public object RawsID
    {
      get { return StringHelper.ByteArrayToString((byte[])m_user.RawsID); }
    }

    [JSProperty(Name="sId")]
    public object sID
    {
      get { return m_user.sID; }
    }

    [JSProperty(Name="name")]
    public string Name
    {
      get { return m_user.Name; }
    }

    [JSProperty(Name="firstName")]
    public string FirstName
    {
      get { return m_user.FirstName; }
    }

    [JSProperty(Name="initials")]
    public string Initials
    {
      get { return m_user.Initials; }
    }

    [JSProperty(Name="lastName")]
    public string LastName
    {
      get { return m_user.LastName; }
    }

    [JSProperty(Name="displayName")]
    public string DisplayName
    {
      get { return m_user.DisplayName; }
    }

    [JSProperty(Name="description")]
    public string Description
    {
      get { return m_user.Description; }
    }

    [JSProperty(Name="office")]
    public string Office
    {
      get { return m_user.Office; }
    }

    [JSProperty(Name="email")]
    public string Email
    {
      get { return m_user.Email; }
    }

    [JSProperty(Name="homePage")]
    public string HomePage
    {
      get { return m_user.HomePage; }
    }
    #endregion

    #region Address

    [JSProperty(Name="street")]
    [DirectoryAttribute("streetAddress")]
    public string Street
    {
      get { return m_user.Street; }
    }

    [JSProperty(Name="poBox")]
    public string POBox
    {
      get { return m_user.POBox; }
    }
    [JSProperty(Name="city")]
    public string City
    {
      get { return m_user.City; }
    }

    [JSProperty(Name="state")]
    public string State
    {
      get { return m_user.State; }
    }

    [JSProperty(Name="zip")]
    public string Zip
    {
      get { return m_user.Zip; }
    }

    [JSProperty(Name="country")]
    public string Country
    {
      get { return m_user.Country; }
    }
    #endregion

    #region Account
    [JSProperty(Name="userLogonName")]
    public string UserLogonName
    {
      get { return m_user.UserLogonName; }
    }

    [JSProperty(Name="preWin2kLogonName")]
    public string PreWin2kLogonName
    {
      get { return m_user.PreWin2kLogonName; }
    }

    [JSProperty(Name="isAccountDisabled")]
    public int AccountDisabled
    {
      get { return m_user.AccountDisabled; }
    }

    [JSProperty(Name="logonCount")]
    public int LogonCount
    {
      get { return m_user.LogonCount; }
    }

    [JSProperty(Name="passwordLastSet")]
    public DateInstance PasswordLastSet
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_user.PasswordLastSet.ToLocalTime()); }
    }

    [JSProperty(Name="lastLogon")]
    public DateInstance LastLogon
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_user.LastLogon.ToLocalTime()); }
    }

    [JSProperty(Name="lastLogoff")]
    public DateInstance LastLogoff
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_user.LastLogoff.ToLocalTime()); }
    }

    [JSProperty(Name="badPasswordTime")]
    public DateInstance BadPasswordTime
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_user.BadPasswordTime.ToLocalTime()); }
    }

    [JSProperty(Name = "badPasswordCount")]
    public int BadPasswordCount
    {
      get { return m_user.BadPasswordCount; }
    }

    [JSProperty(Name="lastSuccessfulInteractiveLogonTime")]
    public DateInstance LastSuccessfulInteractiveLogonTime
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_user.LastSuccessfulInteractiveLogonTime.ToLocalTime()); }
    }

    [JSProperty(Name="lastFailedInteractiveLogonTime")]
    public DateInstance LastFailedInteractiveLogonTime
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_user.LastFailedInteractiveLogonTime.ToLocalTime()); }
    }

    [JSProperty(Name="failedInteractiveLogonCount")]
    public int FailedInteractiveLogonCount
    {
      get { return m_user.FailedInteractiveLogonCount;  }
    }

    [JSProperty(Name = "failedInteractiveLogonCountAtLastSuccessfulLogon")]
    public int FailedInteractiveLogonCountAtLastSuccessfulLogon
    {
      get { return m_user.FailedInteractiveLogonCountAtLastSuccessfulLogon; }
    }
    #endregion

    #region Phone

    [JSProperty(Name="homePhone")]
    public string HomePhone
    {
      get { return m_user.HomePhone; }
    }

    [JSProperty(Name="phoneNumber")]
    public string PhoneNumber
    {
      get { return m_user.PhoneNumber; }
    }

    [JSProperty(Name="mobileNumber")]
    public string MobileNumber
    {
      get { return m_user.MobileNumber; }
    }

    [JSProperty(Name="faxNumber")]
    [DirectoryAttribute("facsimileTelephoneNumber")]
    public string FaxNumber
    {
      get { return m_user.FaxNumber; }
    }

    [JSProperty(Name="pager")]
    public string Pager
    {
      get { return m_user.Pager; }
    }

    [JSProperty(Name="ipPhone")]
    public string IpPhone
    {
      get { return m_user.IpPhone; }
    }
    #endregion

    #region Organization
    [JSProperty(Name="title")]
    public string Title
    {
      get { return m_user.Title; }
    }

    [JSProperty(Name="department")]
    public string Department
    {
      get { return m_user.Department; }
    }

    [JSProperty(Name="company")]
    public string Company
    {
      get { return m_user.Company; }
    }

    [JSProperty(Name="managerLdap")]
    public string ManagerLdap
    {
      get { return m_user.ManagerLdap; }
    }

    [JSProperty(Name="managerName")]
    public string ManagerName
    {
      get { return m_user.ManagerName; }
    }
    #endregion
  }
}
