namespace Barista.SharePoint.Library
{
  using System;
  using System.Linq;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Taxonomy;

  public class TermInstance : ObjectInstance
  {
    private Term m_term;

    public TermInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public TermInstance(ObjectInstance prototype, Term term)
      : this(prototype)
    {
      this.m_term = term;
    }

    internal Term Term
    {
      get { return m_term; }
    }

    #region Properties
    [JSProperty(Name = "createdDate")]
    public DateInstance CreatedDate
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_term.CreatedDate); }
    }

    [JSProperty(Name = "customProperties")]
    public ObjectInstance CustomProperties
    {
      get
      {
        var result = this.Engine.Object.Construct();
        foreach (var property in m_term.CustomProperties)
        {
          result.SetPropertyValue(property.Key, property.Value, false);
        }
        return result;
      }
    }

    [JSProperty(Name = "customSortOrder")]
    public string CustomSortOrder
    {
      get { return m_term.CustomSortOrder; }
      set { m_term.CustomSortOrder = value; }
    }

    [JSProperty(Name = "id")]
    public string Id
    {
      get { return m_term.Id.ToString(); }
    }

    [JSProperty(Name = "isAvailableForTagging")]
    public bool IsAvailableForTagging
    {
      get { return m_term.IsAvailableForTagging;  }
      set { m_term.IsAvailableForTagging = value; }
    }

    [JSProperty(Name = "isDeprecated")]
    public bool IsDeprecated
    {
      get { return m_term.IsDeprecated; }
    }

    [JSProperty(Name = "isKeyword")]
    public bool IsKeyword
    {
      get { return m_term.IsKeyword; }
    }

    [JSProperty(Name = "isReused")]
    public bool IsReused
    {
      get { return m_term.IsReused; }
    }

    [JSProperty(Name = "isRoot")]
    public bool IsRoot
    {
      get { return m_term.IsRoot; }
    }

    [JSProperty(Name = "isSourceTerm")]
    public bool IsSourceTerm
    {
      get { return m_term.IsSourceTerm; }
    }

    [JSProperty(Name = "lastModifiedDate")]
    public DateInstance LastModifiedDate
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_term.LastModifiedDate); }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_term.Name; }
      set { m_term.Name = value; }
    }

    [JSProperty(Name = "owner")]
    public string Owner
    {
      get { return m_term.Owner; }
      set { m_term.Owner = value; }
    }

    [JSProperty(Name = "termsCount")]
    public int TermsCount
    {
      get { return m_term.TermsCount; }
    }
    #endregion

    [JSFunction(Name = "copy")]
    public TermInstance Copy([DefaultParameterValue(true)] bool doCopyChildren)
    {
      var copiedTerm = m_term.Copy(doCopyChildren);
      return new TermInstance(this.Engine.Object.InstancePrototype, copiedTerm);
    }

    [JSFunction(Name = "createLabel")]
    public TermLabelInstance CreateLabel(string labelName, int lcid, bool isDefault)
    {
      var newLabel = m_term.CreateLabel(labelName, lcid, isDefault);
      return new TermLabelInstance(this.Engine.Object.InstancePrototype, newLabel);
    }

    [JSFunction(Name = "createTerm")]
    public TermInstance CreateTerm(string name, int lcid)
    {
      var newTerm = m_term.CreateTerm(name, lcid);
      return new TermInstance(this.Engine.Object.InstancePrototype, newTerm);
    }

    [JSFunction(Name = "delete")]
    public void Delete()
    {
      m_term.Delete();
    }

    [JSFunction(Name = "deleteCustomProperty")]
    public void DeleteCustomProperty(string name)
    {
      m_term.DeleteCustomProperty(name);
    }

    [JSFunction(Name = "deprecate")]
    public void Deprecate(bool doDeprecate)
    {
      m_term.Deprecate(doDeprecate);
    }

    [JSFunction(Name = "DoesUserHavePermissions")]
    public bool DoesUserHavePermissions(string rights)
    {
      var rightsEnum = (TaxonomyRights)Enum.Parse(typeof(TaxonomyRights), rights);
      return m_term.DoesUserHavePermissions(rightsEnum);
    }

    [JSFunction(Name = "getAllLabels")]
    public ArrayInstance GetAllLabels(int lcid)
    {
      var result = this.Engine.Array.Construct();
      foreach (var label in m_term.GetAllLabels(lcid))
      {
        ArrayInstance.Push(result, new TermLabelInstance(this.Engine.Object.InstancePrototype, label));
      }
      return result;
    }

    [JSFunction(Name = "getDefaultLabel")]
    public string GetDefaultLabel(int lcid)
    {
      return m_term.GetDefaultLabel(lcid);
    }

    [JSFunction(Name = "getDescription")]
    public string GetDescription()
    {
      return m_term.GetDescription();
    }

    public string GetDescription([DefaultParameterValue(1033)] int lcid)
    {
      return m_term.GetDescription(lcid);
    }

    [JSFunction(Name = "getIsDescendantOf")]
    public bool GetIsDescendantOf(TermInstance term)
    {
      return m_term.GetIsDescendantOf(term.m_term);
    }

    [JSFunction(Name = "getLabels")]
    public ArrayInstance GetLabels(int lcid)
    {
      var result = this.Engine.Array.Construct();
      foreach (var label in m_term.Labels)
      {
        ArrayInstance.Push(result, new TermLabelInstance(this.Engine.Object.InstancePrototype, label));
      }
      return result;
    }

    [JSFunction(Name = "getPath")]
    public string GetPath()
    {
      return m_term.GetPath();
    }

    public string GetPath([DefaultParameterValue(1033)] int lcid)
    {
      return m_term.GetPath(lcid);
    }

    [JSFunction(Name = "getParent")]
    public TermInstance GetParent()
    {
      return new TermInstance(this.Engine.Object.InstancePrototype, m_term.Parent);
    }

    [JSFunction(Name = "getSourceTerm")]
    public TermInstance GetSourceTerm()
    {
      return new TermInstance(this.Engine.Object.InstancePrototype, m_term.SourceTerm);
    }

    [JSFunction(Name = "getTerms")]
    public ArrayInstance GetTerms([DefaultParameterValue(0)] int pagingLimit)
    {
      var result = this.Engine.Array.Construct();
      foreach (var term in m_term.GetTerms(pagingLimit))
      {
        ArrayInstance.Push(result, new TermInstance(this.Engine.Object.InstancePrototype, term));
      }
      return result;
    }

    [JSFunction(Name = "GetTermSet")]
    public TermSetInstance GetTermSet()
    {
      return new TermSetInstance(this.Engine.Object.InstancePrototype, m_term.TermSet);
    }

    [JSFunction(Name = "GetTermSets")]
    public ArrayInstance GetTermSets()
    {
      var result = this.Engine.Array.Construct();
      foreach (var termSet in m_term.TermSets)
      {
        ArrayInstance.Push(result, new TermSetInstance(this.Engine.Object.InstancePrototype, termSet));
      }
      return result;
    }

    [JSFunction(Name = "getTermStore")]
    public TermStoreInstance GetTermStore()
    {
      return new TermStoreInstance(this.Engine.Object.InstancePrototype, m_term.TermStore);
    }

    [JSFunction(Name = "merge")]
    public TermInstance Merge(TermInstance term)
    {
      var mergedTerm = m_term.Merge(term.m_term);
      return new TermInstance(this.Engine.Object.InstancePrototype, mergedTerm);
    }

    [JSFunction(Name = "move")]
    public void Move(TermInstance newParentTerm)
    {
      m_term.Move(newParentTerm.m_term);
    }

    public void Move(TermSetInstance parentTermSet)
    {
      m_term.Move(parentTermSet.TermSet);
    }

    [JSFunction(Name = "reassignSourceTerm")]
    public void ReassignSourceTerm(TermInstance reusedTerm)
    {
      m_term.ReassignSourceTerm(reusedTerm.m_term);
    }

    [JSFunction(Name = "reuseTerm")]
    public TermInstance ReuseTerm(TermInstance sourceTerm, bool reuseBranch)
    {
      var reusedTerm = m_term.ReuseTerm(sourceTerm.m_term, reuseBranch);
      return new TermInstance(this.Engine.Object.InstancePrototype, reusedTerm);
    }

    [JSFunction(Name = "setCustomProperty")]
    public void SetCustomProperty(string name, string value)
    {
      m_term.SetCustomProperty(name, value);
    }

    [JSFunction(Name = "setDescription")]
    public void SetDescription(string description, int lcid)
    {
      m_term.SetDescription(description, lcid);
    }
  }
}
