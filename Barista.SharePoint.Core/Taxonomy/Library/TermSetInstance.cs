﻿namespace Barista.SharePoint.Taxonomy.Library
{
  using System;
  using Barista.Library;
  using Jurassic.Library;
  using Microsoft.SharePoint.Taxonomy;

  [Serializable]
  public class TermSetInstance : ObjectInstance
  {
    private readonly TermSet m_termSet;

    public TermSetInstance(ObjectInstance prototype)
      : base(prototype)
    {
      PopulateFields();
      PopulateFunctions();
    }

    public TermSetInstance(ObjectInstance prototype, TermSet termSet)
      : this(prototype)
    {
      m_termSet = termSet;
    }

    internal TermSet TermSet
    {
      get { return m_termSet; }
    }

    #region Properties
    [JSProperty(Name = "contact")]
    public string Contact
    {
      get { return m_termSet.Contact; }
      set { m_termSet.Contact = value; }
    }

    [JSProperty(Name = "createdDate")]
    public DateInstance CreatedDate
    {
      get { return JurassicHelper.ToDateInstance(Engine, m_termSet.CreatedDate); }
    }

    [JSProperty(Name = "customSortOrder")]
    public string CustomSortOrder
    {
      get { return m_termSet.CustomSortOrder; }
      set { m_termSet.CustomSortOrder = value; }
    }

    [JSProperty(Name = "description")]
    public string Description
    {
      get { return m_termSet.Description; }
      set { m_termSet.Description = value; }
    }

    [JSProperty(Name = "id")]
    public string Id
    {
      get { return m_termSet.Id.ToString(); }
    }

    [JSProperty(Name = "isAvailableForTagging")]
    public bool IsAvailableForTagging
    {
      get { return m_termSet.IsAvailableForTagging; }
      set { m_termSet.IsAvailableForTagging = value; }
    }

    [JSProperty(Name = "isOpenForTermCreation")]
    public bool IsOpenForTermCreation
    {
      get { return m_termSet.IsOpenForTermCreation; }
      set { m_termSet.IsOpenForTermCreation = value; }
    }

    [JSProperty(Name = "lastModifiedDate")]
    public DateInstance LastModifiedDate
    {
      get { return JurassicHelper.ToDateInstance(Engine, m_termSet.LastModifiedDate); }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_termSet.Name; }
      set { m_termSet.Name = value; }
    }

    [JSProperty(Name = "owner")]
    public string Owner
    {
      get { return m_termSet.Owner; }
      set { m_termSet.Owner = value; }
    }

    [JSProperty(Name = "stakeholders")]
    [JSDoc("ternPropertyType", "[string]")]
    public ArrayInstance Stakeholders
    {
      get
      {
        var result = Engine.Array.Construct();
        foreach (var stakeHolder in m_termSet.Stakeholders)
        {
          ArrayInstance.Push(result, stakeHolder);
        }
        return result;
      }
    }
    #endregion

    [JSFunction(Name = "addStakeholder")]
    public void AddStakeholder(string stakeHolderName)
    {
      m_termSet.AddStakeholder(stakeHolderName);
    }

    [JSFunction(Name = "copy")]
    public TermSetInstance Copy()
    {
      return new TermSetInstance(Engine.Object.InstancePrototype, m_termSet.Copy());
    }

    [JSFunction(Name = "createTerm")]
    public TermInstance CreateTerm(string name, int lcid)
    {
      var newTerm = m_termSet.CreateTerm(name, lcid);
      return new TermInstance(Engine.Object.InstancePrototype, newTerm);
    }

    [JSFunction(Name = "createTermWithId")]
    public TermInstance CreateTerm(string name, int lcid, object newTermId)
    {
      var guid = GuidInstance.ConvertFromJsObjectToGuid(newTermId);
      var newTerm = m_termSet.CreateTerm(name, lcid, guid);
      return new TermInstance(Engine.Object.InstancePrototype, newTerm);
    }

    [JSFunction(Name = "delete")]
    public void Delete()
    {
      m_termSet.Delete();
    }

    [JSFunction(Name = "deleteStakeholder")]
    public void DeleteStakeholder(string stakeholderName)
    {
      m_termSet.DeleteStakeholder(stakeholderName);
    }

    [JSFunction(Name = "doesUserHavePermissions")]
    public bool DoesUserHavePermissions(string rights)
    {
      var rightsEnum = (TaxonomyRights)Enum.Parse(typeof(TaxonomyRights), rights);
      return m_termSet.DoesUserHavePermissions(rightsEnum);
    }

    [JSFunction(Name = "export")]
    public string Export()
    {
      return m_termSet.Export();
    }

    [JSFunction(Name = "getAllTerms")]
    [JSDoc("ternReturnType", "[+Term]")]
    public ArrayInstance GetAllTerms()
    {
      var result = Engine.Array.Construct();
      foreach (var term in m_termSet.GetAllTerms())
      {
        ArrayInstance.Push(result, new TermInstance(Engine.Object.InstancePrototype, term));
      }
      return result;
    }

    [JSFunction(Name = "getGroup")]
    public TermGroupInstance GetGroup()
    {
      return new TermGroupInstance(Engine.Object.InstancePrototype, m_termSet.Group);
    }

    [JSFunction(Name = "getTerm")]
    public TermInstance GetTerm(object termId)
    {
      var guid = GuidInstance.ConvertFromJsObjectToGuid(termId);
      var term = m_termSet.GetTerm(guid);
      return new TermInstance(Engine.Object.InstancePrototype, term);
    }

    [JSFunction(Name = "getTerms")]
    [JSDoc("ternReturnType", "[+Term]")]
    public ArrayInstance GetTerms()
    {
      var result = Engine.Array.Construct();
      foreach (var term in m_termSet.Terms)
      {
        ArrayInstance.Push(result, new TermInstance(Engine.Object.InstancePrototype, term));
      }
      return result;
    }
    
    //TODO: Bunches of GetTerm overloads.

    [JSFunction(Name = "getTermStore")]
    public TermStoreInstance GetTermStore()
    {
      return new TermStoreInstance(Engine.Object.InstancePrototype, m_termSet.TermStore);
    }

    [JSFunction(Name = "move")]
    public void Move(TermGroupInstance targetGroup)
    {
      m_termSet.Move(targetGroup.TermGroup);
    }

    [JSFunction(Name = "reuseTerm")]
    public TermInstance ReuseTerm(TermInstance term, bool reuseBranch)
    {
      var reusedTerm = m_termSet.ReuseTerm(term.Term, reuseBranch);
      return new TermInstance(Engine.Object.InstancePrototype, reusedTerm);
    }
  }
}
