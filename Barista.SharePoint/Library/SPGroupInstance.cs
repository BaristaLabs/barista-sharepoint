namespace Barista.SharePoint.Library
{
  using System;
  using System.Linq;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using System.Collections.Generic;

  public class SPGroupConstructor : ClrFunction
  {
    public SPGroupConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPGroup", new SPGroupInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPUserInstance Construct(string loginName)
    {
      SPUser user;
      if (SPHelper.TryGetSPUserFromLoginName(loginName, out user) == false)
      {
        throw new JavaScriptException(this.Engine, "Error", "No user with the specified name exists in the current context.");
      }

      return new SPUserInstance(this.InstancePrototype, user);
    }

    public SPUserInstance Construct(SPUser user)
    {
      if (user == null)
        throw new ArgumentNullException("user");

      return new SPUserInstance(this.InstancePrototype, user);
    }
  }

  public class SPGroupInstance : ObjectInstance
  {
    private SPGroup m_group;

    public SPGroupInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPGroupInstance(ObjectInstance prototype, SPGroup group)
      : this(prototype)
    {
      this.m_group = group;
    }

    internal SPGroup Group
    {
      get { return m_group; }
    }

    #region Properties
    [JSProperty(Name = "allowMembersEditMembership")]
    public bool AllowMembersEditMembership
    {
      get { return m_group.AllowMembersEditMembership; }
      set { m_group.AllowMembersEditMembership = value; }
    }

    [JSProperty(Name = "allowRequestToJoinLeave")]
    public bool AllowRequestToJoinLeave
    {
      get { return m_group.AllowRequestToJoinLeave; }
      set { m_group.AllowRequestToJoinLeave = value; }
    }

    [JSProperty(Name = "autoAcceptRequestToJoinLeave")]
    public bool AutoAcceptRequestToJoinLeave
    {
      get { return m_group.AutoAcceptRequestToJoinLeave; }
      set { m_group.AutoAcceptRequestToJoinLeave = value; }
    }

    [JSProperty(Name = "canCurrentUserEditMembership")]
    public bool CanCurrentUserEditMembership
    {
      get { return m_group.CanCurrentUserEditMembership; }
    }

    [JSProperty(Name = "canCurrentUserManageGroup")]
    public bool CanCurrentUserManageGroup
    {
      get { return m_group.CanCurrentUserManageGroup; }
    }

    [JSProperty(Name = "canCurrentUserViewMembership")]
    public bool CanCurrentUserViewMembership
    {
      get { return m_group.CanCurrentUserViewMembership; }
    }

    [JSProperty(Name = "containsCurrentUser")]
    public bool ContainsCurrentUser
    {
      get { return m_group.ContainsCurrentUser; }
    }

    [JSProperty(Name = "description")]
    public string Description
    {
      get { return m_group.Description; }
      set { m_group.Description = value; }
    }

    [JSProperty(Name = "distributionGroupAlias")]
    public string DistributionGroupAlias
    {
      get { return m_group.DistributionGroupAlias; }
    }

    [JSProperty(Name = "distributionGroupEmail")]
    public string DistributionGroupEmail
    {
      get { return m_group.DistributionGroupEmail; }
    }

    [JSProperty(Name = "distributionGroupErrorMessage")]
    public string DistributionGroupErrorMessage
    {
      get { return m_group.DistributionGroupErrorMessage; }
    }

    [JSProperty(Name = "explicitlyContainsCurrentUser")]
    public bool ExplicitlyContainsCurrentUser
    {
      get { return m_group.ExplicitlyContainsCurrentUser; }
    }

    [JSProperty(Name = "id")]
    public int Id
    {
      get { return m_group.ID; }
    }

    [JSProperty(Name = "loginName")]
    public string LoginName
    {
      get { return m_group.LoginName; }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_group.Name; }
      set { m_group.Name = value; }
    }

    [JSProperty(Name = "onlyAllowMembersViewMembership")]
    public bool OnlyAllowMembersViewMembership
    {
      get { return m_group.OnlyAllowMembersViewMembership; }
      set { m_group.OnlyAllowMembersViewMembership = value; }
    }

    [JSProperty(Name = "requestToJoinLeaveEmailSetting")]
    public string RequestToJoinLeaveEmailSetting
    {
      get { return m_group.RequestToJoinLeaveEmailSetting; }
      set { m_group.RequestToJoinLeaveEmailSetting = value; }
    }

    [JSProperty(Name = "users")]
    public ArrayInstance Users
    {
      get
      {
        var result = this.Engine.Array.Construct();
        foreach (var user in m_group.Users.OfType<SPUser>())
        {
          ArrayInstance.Push(result, new SPUserInstance(this.Engine.Object.InstancePrototype, user));
        }
        return result;
      }
    }
    #endregion

    #region Functions
    [JSFunction(Name = "addUser")]
    public void AddUser(string loginName)
    {
      SPUser user;
      if (SPHelper.TryGetSPUserFromLoginName(loginName, out user) == false)
        throw new JavaScriptException(this.Engine, "Error", "A user with the specified login name does not exist.");

      m_group.AddUser(user);
    }

    public void AddUser(SPUserInstance user)
    {
      m_group.AddUser(user.User);
    }

    [JSFunction(Name = "clearDistributionGroupErrorMessage")]
    public void ClearDistributionGroupErrorMessage()
    {
      m_group.ClearDistributionGroupErrorMessage();
    }

    [JSFunction(Name = "createDistributionGroup")]
    public void CreateDistributionGroup(string dlAlias)
    {
      m_group.CreateDistributionGroup(dlAlias);
    }

    [JSFunction(Name = "deleteDistributionGroup")]
    public void DeleteDistributionGroup()
    {
      m_group.DeleteDistributionGroup();
    }

    [JSFunction(Name = "getDistributionGroupArchives")]
    public ArrayInstance GetDistributionGroupArchives()
    {
      var result = this.Engine.Array.Construct();
      foreach (var list in m_group.GetDistributionGroupArchives(null))
      {
        ArrayInstance.Push(result, new SPListInstance(this.Engine.Object.InstancePrototype, list));
      }
      return result;
    }

    [JSFunction(Name = "getParentWeb")]
    public SPWebInstance GetParentWeb()
    {
      return new SPWebInstance(this.Engine.Object.InstancePrototype, m_group.ParentWeb);
    }

    [JSFunction(Name = "getOwner")]
    public ObjectInstance GetOwner()
    {
      ObjectInstance result = null;

      var owner = m_group.Owner;
      if (owner is SPUser)
      {
        result = new SPUserInstance(this.Engine.Object.InstancePrototype, (owner as SPUser));
      }
      else if (owner is SPGroup)
      {
        result = new SPGroupInstance(this.Engine.Object.InstancePrototype, (owner as SPGroup));
      }
      
      return result;
    }

    [JSFunction(Name = "removeUser")]
    public void RemoveUser(string loginName)
    {
      SPUser user;
      if (SPHelper.TryGetSPUserFromLoginName(loginName, out user) == false)
        throw new JavaScriptException(this.Engine, "Error", "A user with the specified login name does not exist.");

      m_group.RemoveUser(user);
    }

    [JSFunction(Name = "removeUser")]
    public void RemoveUser(SPUserInstance user)
    {
      m_group.RemoveUser(user.User);
    }

    [JSFunction(Name = "renameDistributionGroup")]
    public void RenameDistributionGroup(string newAlias)
    {
      m_group.RenameDistributionGroup(newAlias);
    }

    [JSFunction(Name = "resynchronizeDistributionGroup")]
    public void ResynchronizeDistributionGroup()
    {
      m_group.ResynchronizeDistributionGroup();
    }

    [JSFunction(Name = "setDistributionGroupArchives")]
    public void SetDistributionGroupArchives(ArrayInstance lists)
    {
      List<SPList> m_lists = new List<SPList>();
      for (int i = 0; i < lists.Length; i++)
      {
        m_group.SetDistributionGroupArchives(m_lists.AsReadOnly(), null);
      }
    }

    [JSFunction(Name = "update")]
    public void Update()
    {
      m_group.Update();
    }

    [JSFunction(Name = "updateDistributionGroupStatus")]
    public void UpdateDistributionGroupStatus()
    {
      m_group.UpdateDistributionGroupStatus();
    }

    [JSFunction(Name = "getXml")]
    public string GetXml()
    {
      return m_group.Xml;
    }
    #endregion
  }
}
