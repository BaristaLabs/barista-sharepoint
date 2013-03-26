namespace Barista.SharePoint
{
  using Barista.DirectoryServices;

  using System;
  using System.Collections.Generic;
  using System.DirectoryServices;
  using System.Linq;

  /// <summary>
  /// Represents a class that 
  /// </summary>
  /// <remarks>
  /// This is a layer of abstraction over the Active Directory.
  /// </remarks>
  public static class ADHelper
  {
    public static ADUser GetADUser(string loginName)
    {
      if (String.IsNullOrEmpty(loginName))
        loginName = System.Threading.Thread.CurrentPrincipal.Identity.Name;

      ADUser result;
     
      var ldapRoot = new DirectoryEntry(GetCurrentLdapPath());
      using (var ctx = new ADContext(ldapRoot))
      {
        string domainName = ldapRoot.Path.Replace("LDAP://", "");
        string preWin2KLoginName = loginName.Replace(domainName + "\\", "");

        ctx.Users.Searcher.SizeLimit = 1;

        result = (from usr in ctx.Users
                  where usr.PreWin2kLogonName == preWin2KLoginName
                  select usr).ToList().FirstOrDefault();
      }

      return result;
    }

    public static ADGroup GetADGroup(string groupName)
    {
      if (String.IsNullOrEmpty(groupName))
        return null;

      ADGroup result;

      var ldapRoot = new DirectoryEntry(GetCurrentLdapPath());
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
      List<DirectoryEntity> result = new List<DirectoryEntity>();

      searchText = searchText.Trim();

      DirectoryEntry ldapRoot = new DirectoryEntry(GetCurrentLdapPath());
      using (ADContext ctx = new ADContext(ldapRoot))
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

    private static string GetCurrentLdapPath()
    {
      var domain = Utilities.GetFarmKeyValue("WindowsDomainShortName");
      return "LDAP://" + domain;
    }
  }
}