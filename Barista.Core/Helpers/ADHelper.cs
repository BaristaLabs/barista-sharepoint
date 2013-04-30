namespace Barista
{
  using System.Runtime.InteropServices;
  using Barista.DirectoryServices;

  using System;
  using System.Collections.Generic;
  using System.DirectoryServices;
  using System.Linq;
  using Barista.Extensions;

  /// <summary>
  /// Represents a class that 
  /// </summary>
  /// <remarks>
  /// This is a layer of abstraction over Active Directory.
  /// </remarks>
  public static class ADHelper
  {
    #region DLL Imports
    [DllImport("Netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern int NetGetJoinInformation(
      string server,
      out IntPtr domain,
      out NetJoinStatus status);

    [DllImport("Netapi32.dll")]
// ReSharper disable InconsistentNaming
    static extern int NetApiBufferFree(IntPtr Buffer);
// ReSharper restore InconsistentNaming

    // Win32 Result Code Constant
    const int ErrorSuccess = 0;

    // NetGetJoinInformation() Enumeration
    public enum NetJoinStatus
    {
      NetSetupUnknownStatus = 0,
      NetSetupUnjoined,
      NetSetupWorkgroupName,
      NetSetupDomainName
    } // NETSETUP_JOIN_STATUS
    #endregion

    public static ADUser GetADUser(string loginName)
    {
      if (String.IsNullOrEmpty(loginName))
        loginName = System.Threading.Thread.CurrentPrincipal.Identity.Name;

      ADUser result;

      var ldapPath = GetJoinedDomain();
      if (ldapPath.IsNullOrWhiteSpace())
        throw new InvalidOperationException("The current machine is not joined to a domain.");

      var ldapRoot = new DirectoryEntry("LDAP://" + ldapPath);
      using (var ctx = new ADContext(ldapRoot))
      {
        var domainName = ldapRoot.Path.Replace("LDAP://", "");
        var preWin2KLoginName = loginName.Replace(domainName + "\\", "");

        ctx.Users.Searcher.SizeLimit = 1;

        result = (from usr in ctx.Users
                  where usr.PreWin2kLogonName == preWin2KLoginName
                  select usr).ToList().FirstOrDefault();
      }

      return result;
    }

    public static string GetUserUpn(string loginName)
    {
      var user = GetADUser(loginName);
      if (user == null)
        return null;

      return user.UserLogonName;
    }

    public static ADGroup GetADGroup(string groupName)
    {
      if (String.IsNullOrEmpty(groupName))
        return null;

      ADGroup result;

      var ldapPath = GetJoinedDomain();
      if (ldapPath.IsNullOrWhiteSpace())
        throw new InvalidOperationException("The current machine is not joined to a domain.");

      var ldapRoot = new DirectoryEntry(ldapPath);

      using (var ctx = new ADContext(ldapRoot))
      {
        ctx.Groups.Searcher.SizeLimit = 1;

        result = (from grp in ctx.Groups
                  where grp.Name == groupName
                  select grp).ToList().FirstOrDefault();
      }

      return result;
    }

    public static IEnumerable<DirectoryEntity> SearchAllDirectoryEntities(string searchText, int maxResults, PrincipalType principalType)
    {
      var result = new List<DirectoryEntity>();

      searchText = searchText.Trim();

      var ldapPath = GetJoinedDomain();
      if (ldapPath.IsNullOrWhiteSpace())
        throw new InvalidOperationException("The current machine is not joined to a domain.");

      var ldapRoot = new DirectoryEntry(ldapPath);

      using (var ctx = new ADContext(ldapRoot))
      {
        ctx.Users.Searcher.SizeLimit = maxResults;
        ctx.Groups.Searcher.SizeLimit = maxResults;

        if ((principalType & PrincipalType.User) == PrincipalType.User)
        {
          var users = from usr in ctx.Users
                      where usr.FirstName == "*" + searchText + "*" ||
                            usr.LastName == "*" + searchText + "*" ||
                            usr.DisplayName == "*" + searchText + "*" ||
                            usr.Email == "*" + searchText + "*" ||
                            usr.PreWin2kLogonName == "*" + searchText + "*"
                      select usr;

          result.AddRange(Enumerable.Cast<DirectoryEntity>(users));
        }
        else if ((principalType & PrincipalType.SecurityGroup) == PrincipalType.SecurityGroup)
        {
          var groups = from grp in ctx.Groups
                        where grp.Name == "*" + searchText + "*"
                        select grp;

          result.AddRange(Enumerable.Cast<DirectoryEntity>(groups));
        }
        else
        {
          throw new NotImplementedException();
        }
      }

      return result;
    }

    public static IEnumerable<ADUser> SearchAllUsers(string searchText, int maxResults)
    {
      return SearchAllDirectoryEntities(searchText, maxResults, PrincipalType.User).OfType<ADUser>().ToList();
    }

    public static IEnumerable<ADGroup> SearchAllGroups(string searchText, int maxResults)
    {
      return SearchAllDirectoryEntities(searchText, maxResults, PrincipalType.SecurityGroup).OfType<ADGroup>().ToList();
    }

    // Returns the domain name the computer is joined to, or "" if not joined.
    public static string GetJoinedDomain()
    {
      string domain = null;
      var pDomain = IntPtr.Zero;
      try
      {
// ReSharper disable RedundantAssignment
        var status = NetJoinStatus.NetSetupUnknownStatus;
// ReSharper restore RedundantAssignment

        var result = NetGetJoinInformation(null, out pDomain, out status);
        if (result == ErrorSuccess &&
            status == NetJoinStatus.NetSetupDomainName)
        {
          domain = Marshal.PtrToStringAuto(pDomain);
        }
      }
      finally
      {
        if (pDomain != IntPtr.Zero)
          NetApiBufferFree(pDomain);
      }

      return domain;
    }
  }
}