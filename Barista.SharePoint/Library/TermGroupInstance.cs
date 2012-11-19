namespace Barista.SharePoint.Library
{
  using System;
  using System.Linq;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Taxonomy;

  [Serializable]
  public class TermGroupInstance : ObjectInstance
  {
    private Group m_termGroup;

    public TermGroupInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public TermGroupInstance(ObjectInstance prototype, Group termGroup)
      : this(prototype)
    {
      this.m_termGroup = termGroup;
    }

    internal Group TermGroup
    {
      get { return m_termGroup; }
    }

    #region Properties
    [JSProperty(Name = "createdDate")]
    public DateInstance CreatedDate
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_termGroup.CreatedDate); }
    }

    [JSProperty(Name = "id")]
    public string Id
    {
      get { return m_termGroup.Id.ToString(); }
    }

    [JSProperty(Name = "isSiteCollectionGroup")]
    public bool IsSiteCollectionGroup
    {
      get { return m_termGroup.IsSiteCollectionGroup; }
    }

    [JSProperty(Name = "isSystemGroup")]
    public bool IsSystemGroup
    {
      get { return m_termGroup.IsSystemGroup; }
    }

    [JSProperty(Name = "lastModifiedDate")]
    public DateInstance LastModifiedDate
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_termGroup.LastModifiedDate); }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_termGroup.Name; }
      set { m_termGroup.Name = value; }
    }
    #endregion

    [JSFunction(Name = "addContributor")]
    public void AddContributor(string principalName)
    {
      m_termGroup.AddContributor(principalName);
    }

    [JSFunction(Name = "addGroupManager")]
    public void AddGroupManager(string principalName)
    {
      m_termGroup.AddGroupManager(principalName);
    }

    [JSFunction(Name = "addGroupManager")]
    public void AddSiteCollectionAccess(string siteCollectionId)
    {
      Guid id = new Guid(siteCollectionId);
      m_termGroup.AddSiteCollectionAccess(id);
    }

    [JSFunction(Name = "createTermSet")]
    public TermSetInstance CreateTermSet(string name)
    {
      var newTermSet = m_termGroup.CreateTermSet(name);
      return new TermSetInstance(this.Engine.Object.InstancePrototype, newTermSet);
    }

    public TermSetInstance CreateTermSet(string name, string newTermSetId)
    {
      Guid newId = new Guid(newTermSetId);
      var newTermSet = m_termGroup.CreateTermSet(name, newId);
      return new TermSetInstance(this.Engine.Object.InstancePrototype, newTermSet);
    }

    public TermSetInstance CreateTermSet(string name, int lcid)
    {
      var newTermSet = m_termGroup.CreateTermSet(name, lcid);
      return new TermSetInstance(this.Engine.Object.InstancePrototype, newTermSet);
    }

    public TermSetInstance CreateTermSet(string name, string newTermSetId, int lcid)
    {
      Guid newId = new Guid(newTermSetId);
      var newTermSet = m_termGroup.CreateTermSet(name, newId, lcid);
      return new TermSetInstance(this.Engine.Object.InstancePrototype, newTermSet);
    }

    [JSFunction(Name = "delete")]
    public void Delete()
    {
      m_termGroup.Delete();
    }

    [JSFunction(Name = "deleteContributor")]
    public void DeleteContributor(string principalName)
    {
      m_termGroup.DeleteContributor(principalName);
    }

    [JSFunction(Name = "deleteGroupManager")]
    public void DeleteGroupManager(string principalName)
    {
      m_termGroup.DeleteGroupManager(principalName);
    }

    [JSFunction(Name = "deleteSiteCollectionAccess")]
    public void DeleteSiteCollectionAccess(string siteCollectionId)
    {
      Guid id = new Guid(siteCollectionId);
      m_termGroup.DeleteSiteCollectionAccess(id);
    }

    [JSFunction(Name = "doesUserHavePermissions")]
    public bool DoesUserHavePermissions(string rights)
    {
      var rightsEnum = (TaxonomyRights)Enum.Parse(typeof(TaxonomyRights), rights);

      return m_termGroup.DoesUserHavePermissions(rightsEnum);
    }

    [JSFunction(Name = "export")]
    public string Export()
    {
      return m_termGroup.Export();
    }

    [JSFunction(Name = "getTermSets")]
    public ArrayInstance GetTermSets()
    {
      var result = this.Engine.Array.Construct();
      foreach (var termSet in m_termGroup.TermSets)
      {
        ArrayInstance.Push(result, new TermSetInstance(this.Engine.Object.InstancePrototype, termSet));
      }
      return result;
    }

    [JSFunction(Name = "getTermStore")]
    public TermStoreInstance GetTermStore()
    {
      return new TermStoreInstance(this.Engine.Object.InstancePrototype, m_termGroup.TermStore);
    }

    //TODO: Get/Set GroupManager rights, Contributor rights, site collection rights.
  }
}
