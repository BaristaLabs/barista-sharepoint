namespace Barista.DirectoryServices
{
  using System;
  using System.Runtime.Serialization;
  using System.Security.Principal;
  using Interop.ActiveDs;

  [DataContract(Namespace = Constants.ServiceNamespace)]
  [DirectorySchema("user", typeof(IADsUser))]
  public class ADUser : DirectoryEntity
  {
    #region General
    [DataMember]
    [DirectoryAttribute("objectSid")]
    public object RawsID
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("objectSid")]
    public object sID
    {
      get
      {
        SecurityIdentifier s = null;

        try
        {
          if (RawsID != null)
            s = new SecurityIdentifier((Byte[])RawsID, 0);
        }
        catch { }

        return s.ToString();
      }
      set { }
    }

    [DataMember]
    [DirectoryAttribute("cn")]
    public string Name
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("givenName")]
    public string FirstName
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("initials")]
    public string Initials
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("sn")]
    public string LastName
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("displayName")]
    public string DisplayName
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("description")]
    public string Description
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("physicalDeliveryOfficeName")]
    public string Office
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("mail")]
    public string Email
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("wWWHomePage")]
    public string HomePage
    {
      get;
      set;
    }
    #endregion

    #region Address

    [DataMember]
    [DirectoryAttribute("streetAddress")]
    public string Street
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("postOfficeBox")]
    public string POBox
    {
      get;
      set;
    }
    [DataMember]
    [DirectoryAttribute("l")]
    public string City
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("st")]
    public string State
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("postalCode")]
    public string Zip
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("c")]
    public string Country
    {
      get;
      set;
    }
    #endregion

    #region Account
    [DataMember]
    [DirectoryAttribute("userPrincipalName")]
    public string UserLogonName
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("sAMAccountName")]
    public string PreWin2kLogonName
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("userAccountControl")]
    public Int32 AccountDisabled
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("logonCount")]
    public int LogonCount
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("pwdLastSet")]
    public DateTime PasswordLastSet
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("lastLogon")]
    public DateTime LastLogon
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("lastLogoff")]
    public DateTime LastLogoff
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("badPasswordTime")]
    public DateTime BadPasswordTime
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("badPwdCount")]
    public int BadPasswordCount
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("msDS-LastSuccessfulInteractiveLogonTime")]
    public DateTime LastSuccessfulInteractiveLogonTime
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("msDS-LastFailedInteractiveLogonTime")]
    public DateTime LastFailedInteractiveLogonTime
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("msDS-FailedInteractiveLogonCount")]
    public int FailedInteractiveLogonCount
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("msDS-FailedInteractiveLogonCountAtLastSuccessfulLogon")]
    public int FailedInteractiveLogonCountAtLastSuccessfulLogon
    {
      get;
      set;
    }
    #endregion

    #region Phone

    [DataMember]
    [DirectoryAttribute("homePhone")]
    public string HomePhone
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("telephoneNumber")]
    public string PhoneNumber
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("mobile")]
    public string MobileNumber
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("facsimileTelephoneNumber")]
    public string FaxNumber
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("pager")]
    public string Pager
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("ipPhone")]
    public string IpPhone
    {
      get;
      set;
    }
    #endregion

    #region Organization
    [DataMember]
    [DirectoryAttribute("title")]
    public string Title
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("department")]
    public string Department
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("company")]
    public string Company
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("manager")]
    public string ManagerLdap
    {
      get;
      set;
    }

    [DataMember]
    public string ManagerName
    {
      get
      {
        string o = ManagerLdap;
        if (!string.IsNullOrEmpty(o))
        {
          string[] arr = o.Split(',');

          return arr[0].Replace("CN=", string.Empty);
        }

        return null;
      }
      set { }

    }
    #endregion

    [DataMember]
    [DirectoryAttribute("memberOf")]
    public string[] Groups
    {
      get;
      set;
    }

    //private string GetPrimaryGroup()
    //{
    //  DirectoryEntry aDomainEntry = System.DirectoryServices.ActiveDirectory.Domain.GetCurrentDomain().GetDirectoryEntry();

    //  int primaryGroupID = PrimaryGroupID;
    //  byte[] objectSid = (byte[])ObjectSid;

    //  StringBuilder escapedGroupSid = new StringBuilder();

    //  // Copy over everything but the last four bytes(sub-authority)
    //  // Doing so gives us the RID of the domain
    //  for (uint i = 0; i < objectSid.Length - 4; i++)
    //  {
    //    escapedGroupSid.AppendFormat("\\{0:x2}", objectSid[i]);
    //  }

    //  //Add the primaryGroupID to the escape string to build the SID of the primaryGroup
    //  for (uint i = 0; i < 4; i++)
    //  {
    //    escapedGroupSid.AppendFormat("\\{0:x2}", (primaryGroupID & 0xFF));
    //    primaryGroupID >>= 8;
    //  }

    //  //Search the directory for a group with this SID
    //  DirectorySearcher searcher = new DirectorySearcher();
    //  if (aDomainEntry != null)
    //  {
    //    searcher.SearchRoot = aDomainEntry;
    //  }

    //  searcher.Filter = "(&(objectCategory=Group)(objectSID=" + escapedGroupSid.ToString() + "))";
    //  searcher.PropertiesToLoad.Add("distinguishedName");

    //  string o = searcher.FindOne().Properties["distinguishedName"][0].ToString();
    //  if (!string.IsNullOrEmpty(o))
    //  {
    //    string[] arr = o.Split(',');

    //    return arr[0].Replace("CN=", string.Empty);
    //  }

    //  return null;
    //}
  }
}
