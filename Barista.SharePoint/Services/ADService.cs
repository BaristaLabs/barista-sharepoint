namespace Barista.SharePoint.Services
{
  using Barista.Framework;
  using Barista.DirectoryServices;
  using Barista.SharePoint.Framework;

  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Client.Services;

  using System;
  using System.Collections.Generic;
  using System.DirectoryServices;
  using System.Linq;
  using System.ServiceModel;
  using System.ServiceModel.Activation;
  using System.ServiceModel.Web;
  using System.Web;

  /// <summary>
  /// Represents a service to retrieve people in the system.
  /// </summary>
  /// <remarks>
  /// This is a layer of abstraction over the Active Directory.
  /// </remarks>
  [SilverlightFaultBehavior]
  [BasicHttpBindingServiceMetadataExchangeEndpoint]
  [ServiceContract(Namespace = Barista.Constants.ServiceNamespace)]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
  public sealed class ADService
  {
    [OperationContract]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [WebGet(BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
    [DynamicResponseType]
    public ADUser GetADUser(string loginName)
    {
      if (String.IsNullOrEmpty(loginName))
        loginName = System.Threading.Thread.CurrentPrincipal.Identity.Name;

      ADUser result = null;
      SPSecurity.RunWithElevatedPrivileges(() =>
      {
        DirectoryEntry ldapRoot = new DirectoryEntry(GetCurrentLdapPath());
        using (ADContext ctx = new ADContext(ldapRoot))
        {
          string domainName = ldapRoot.Path.Replace("LDAP://", "");
          string preWin2kLoginName = loginName.Replace(domainName + "\\", "");

          ctx.Users.Searcher.SizeLimit = 1;

          result = (from usr in ctx.Users
                    where usr.PreWin2kLogonName == preWin2kLoginName
                    select usr).ToList().FirstOrDefault();
        }
      });

      return result;
    }

    [OperationContract]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [WebGet(BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
    [DynamicResponseType]
    public ADGroup GetADGroup(string groupName)
    {
      if (String.IsNullOrEmpty(groupName))
        return null;

      ADGroup result = null;
      SPSecurity.RunWithElevatedPrivileges(() =>
      {
        DirectoryEntry ldapRoot = new DirectoryEntry(GetCurrentLdapPath());
        using (ADContext ctx = new ADContext(ldapRoot))
        {
          ctx.Groups.Searcher.SizeLimit = 1;

          result = (from grp in ctx.Groups
                    where grp.Name == groupName
                    select grp).ToList().FirstOrDefault();
        }
      });

      return result;
    }

    [OperationContract]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [WebGet(BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
    [DynamicResponseType]
    [ServiceKnownType(typeof(List<DirectoryEntity>))]
    [ServiceKnownType(typeof(List<ADUser>))]
    [ServiceKnownType(typeof(List<ADGroup>))]
    public IEnumerable<DirectoryEntity> SearchAllDirectoryEntities(string searchText, int maxResults, PrincipalType principalType)
    {
      List<DirectoryEntity> result = new List<DirectoryEntity>();
      string domain = Utilities.GetFarmKeyValue("WindowsDomainShortName");

      searchText = searchText.Trim();

      SPSecurity.RunWithElevatedPrivileges(() =>
      {
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

            foreach (var user in users)
              result.Add(user);
          }
          else if ((principalType & PrincipalType.SecurityGroup) == PrincipalType.SecurityGroup)
          {
            var groups = from grp in ctx.Groups
                         where grp.Name == "*" + searchText + "*"
                         select grp;

            foreach (var group in groups)
              result.Add(group);
          }
          else
          {
            throw new NotImplementedException();
          }
        }
      });

      return result;
    }


    [OperationContract]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [WebGet(BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
    [DynamicResponseType]
    [ServiceKnownType(typeof(List<ADUser>))]
    public IEnumerable<ADUser> SearchAllUsers(string searchText, int maxResults)
    {
      return SearchAllDirectoryEntities(searchText, maxResults, PrincipalType.User).OfType<ADUser>().ToList();
    }

    [OperationContract]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [WebGet(BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
    [DynamicResponseType]
    [ServiceKnownType(typeof(List<ADGroup>))]
    public IEnumerable<ADGroup> SearchAllGroups(string searchText, int maxResults)
    {
      return SearchAllDirectoryEntities(searchText, maxResults, PrincipalType.SecurityGroup).OfType<ADGroup>().ToList();
    }

    private string GetCurrentLdapPath()
    {
      string domain = Utilities.GetFarmKeyValue("WindowsDomainShortName");
      return "LDAP://" + domain;
    }
  }
}