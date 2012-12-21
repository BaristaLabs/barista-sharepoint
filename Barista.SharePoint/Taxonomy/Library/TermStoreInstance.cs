namespace Barista.SharePoint.Taxonomy.Library
{
  using Barista.SharePoint.Library;
  using Jurassic.Library;
  using Microsoft.SharePoint.Taxonomy;
  using System;

  [Serializable]
  public class TermStoreInstance : ObjectInstance
  {
    private readonly TermStore m_termStore;

    public TermStoreInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public TermStoreInstance(ObjectInstance prototype, TermStore termStore)
      : this(prototype)
    {
      this.m_termStore = termStore;
    }

    #region Properties
    [JSProperty(Name = "defaultLanguage")]
    public int DefaultLanguage
    {
      get { return m_termStore.DefaultLanguage; }
    }

    [JSProperty(Name = "id")]
    public string Id
    {
      get { return m_termStore.Id.ToString(); }
    }

    [JSProperty(Name = "isOnline")]
    public bool IsOnline
    {
      get { return m_termStore.IsOnline; }
    }

    [JSProperty(Name = "languages")]
    public ArrayInstance Languages
    {
      get
      {
        var result = this.Engine.Array.Construct();
        foreach (var lang in m_termStore.Languages)
        {
          ArrayInstance.Push(result, lang);
        }
        return result;
      }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_termStore.Name; }
    }

    [JSProperty(Name = "workingLanguage")]
    public int WorkingLanguage
    {
      get { return m_termStore.WorkingLanguage; }
    }
    #endregion

    [JSFunction(Name = "addLanguage")]
    public void AddLanguage(int lcid)
    {
      m_termStore.AddLanguage(lcid);
    }

    [JSFunction(Name = "addTermStoreAdministrator")]
    public void AddTermStoreAdministrator(string principalName)
    {
      m_termStore.AddTermStoreAdministrator(principalName);
    }

    [JSFunction(Name = "commitAll")]
    public void CommitAll()
    {
      m_termStore.CommitAll();
    }

    [JSFunction(Name = "createGroup")]
    public TermGroupInstance CreateGroup(string name)
    {
      var newGroup = m_termStore.CreateGroup(name);
      return new TermGroupInstance(this.Engine.Object.InstancePrototype, newGroup);
    }

    [JSFunction(Name = "deleteLanguage")]
    public void DeleteLanguage(int lcid)
    {
      m_termStore.DeleteLanguage(lcid);
    }

    [JSFunction(Name = "deleteTermStoreAdministrator")]
    public void DeleteTermStoreAdministrator(string principalName)
    {
      m_termStore.DeleteTermStoreAdministrator(principalName);
    }

    [JSFunction(Name = "doesUserHavePermissions")]
    public bool DoesUserHavePermissions(string rights)
    {
      var rightsEnum = (TaxonomyRights)Enum.Parse(typeof(TaxonomyRights), rights);

      return m_termStore.DoesUserHavePermissions(rightsEnum);
    }

    [JSFunction(Name = "getGroup")]
    public TermGroupInstance GetGroup(string id)
    {
      Guid guid = new Guid(id);
      var group = m_termStore.GetGroup(guid);
      return new TermGroupInstance(this.Engine.Object.InstancePrototype, group);
    }

    [JSFunction(Name = "getGroups")]
    public ArrayInstance GetGroups()
    {
      var result = this.Engine.Array.Construct();
      foreach (var group in m_termStore.Groups)
      {
        ArrayInstance.Push(result, new TermGroupInstance(this.Engine.Object.InstancePrototype, group));
      }
      return result;
    }

    [JSFunction(Name = "getSiteCollectionGroup")]
    public TermGroupInstance GetSiteCollectionGroup(SPSiteInstance site)
    {
      var group = m_termStore.GetSiteCollectionGroup(site.Site);
      return new TermGroupInstance(this.Engine.Object.InstancePrototype, group);
    }

    [JSFunction(Name = "getSystemGroup")]
    public TermGroupInstance GetSystemGroup(SPSiteInstance site)
    {
      var group = m_termStore.SystemGroup;
      return new TermGroupInstance(this.Engine.Object.InstancePrototype, group);
    }

    [JSFunction(Name = "resyncHiddenList")]
    public void ResyncHiddenList(SPSiteInstance site)
    {
      m_termStore.ResyncHiddenList(site.Site);
    }

    [JSFunction(Name = "updateUsedTermsOnSite")]
    public void UpdateUsedTermsOnSite(SPSiteInstance site)
    {
      m_termStore.UpdateUsedTermsOnSite(site.Site);
    }

  }
}
