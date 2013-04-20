namespace Barista.Library
{
  using System;
  using System.Security.Principal;
  using Barista.Extensions;
  using Jurassic;
  using Jurassic.Library;
  using Barista.DirectoryServices;

  [Serializable]
  public class ActiveDirectoryInstance : ObjectInstance
  {
    public ActiveDirectoryInstance(ScriptEngine engine)
      : base(engine)
    {
      var currentWindowsIdentity = WindowsIdentity.GetCurrent();
      if (currentWindowsIdentity != null)
        this.CurrentUserLoginName = currentWindowsIdentity.Name;

      this.PopulateFields();
      this.PopulateFunctions();
    }

    /// <summary>
    /// Gets or sets the username of the current user.
    /// </summary>
    public string CurrentUserLoginName
    {
      get;
      set;
    }

    [JSProperty(Name = "currentDomain")]
    [JSDoc("Gets the current domain, if the current machine is not joined to a domain, null is returned.")]
    public string CurrentDomain
    {
      get { return ADHelper.GetJoinedDomain(); }
    }

    [JSFunction(Name = "getADUser")]
    [JSDoc("Returns an object representating the specified user. If no login name is specified, returns the current user.")]
    public ADUserInstance GetADUser(object loginName)
    {
      ADUser user;
      if (loginName == null || loginName == Undefined.Value || loginName == Null.Value || TypeConverter.ToString(loginName).IsNullOrWhiteSpace())
        user = ADHelper.GetADUser(CurrentUserLoginName);
      else
        user = ADHelper.GetADUser(TypeConverter.ToString(loginName));

      if (user == null)
        throw new InvalidOperationException("The current user is not an AD user: " + CurrentUserLoginName);

      return new ADUserInstance(this.Engine.Object.InstancePrototype, user);
    }

    [JSFunction(Name = "getADGroup")]
    [JSDoc("Returns an object representating the specified group.")]
    public ADGroupInstance GetADGroup(string groupName)
    {
      var group = ADHelper.GetADGroup(groupName);

      return new ADGroupInstance(this.Engine.Object.InstancePrototype, group);
    }

    [JSFunction(Name = "searchAllDirectoryEntries")]
    [JSDoc("Searches all directory entries for the specified search text, optionally indicating a maximium number of results and to limit to the specified principal type.")]
    public ArrayInstance SearchAllDirectoryEntities(string searchText, int maxResults, string principalType)
    {
      var principalTypeEnum = PrincipalType.All;
      if (String.IsNullOrEmpty(principalType) == false)
        principalTypeEnum = (PrincipalType)Enum.Parse(typeof(PrincipalType), principalType);

      var entities = ADHelper.SearchAllDirectoryEntities(searchText, maxResults, principalTypeEnum);

      var result = this.Engine.Array.Construct();

      foreach (var entity in entities)
      {
        if (entity is ADGroup)
        {
          ArrayInstance.Push(result, new ADGroupInstance(this.Engine.Object.InstancePrototype, entity as ADGroup));
        }
        else if (entity is ADUser)
        {
          ArrayInstance.Push(result, new ADUserInstance(this.Engine.Object.InstancePrototype, entity as ADUser));
        }
      }

      return result;
    }

    [JSFunction(Name = "searchAllGroups")]
    [JSDoc("Searches all groups for the specified search text, optionally indicating a maximium number of results.")]
    public ArrayInstance SearchAllGroups(string searchText, int maxResults)
    {
      var groups = ADHelper.SearchAllGroups(searchText, maxResults);

      var result = this.Engine.Array.Construct();
      foreach (var group in groups)
      {
        ArrayInstance.Push(result, new ADGroupInstance(this.Engine.Object.InstancePrototype, group));
      }
      return result;
    }

    [JSFunction(Name = "searchAllUsers")]
    [JSDoc("Searches all users for the specified search text, optionally indicating a maximium number of results.")]
    public ArrayInstance SearchAllUsers(string searchText, int maxResults)
    {
      var users = ADHelper.SearchAllUsers(searchText, maxResults);

      var result = this.Engine.Array.Construct();
      foreach (var user in users)
      {
        ArrayInstance.Push(result, new ADUserInstance(this.Engine.Object.InstancePrototype, user));
      }
      return result;
    }

  }
}
