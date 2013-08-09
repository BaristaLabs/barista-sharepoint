namespace Barista.SharePoint.Taxonomy.Library
{
  using System;
  using Jurassic.Library;
  using Microsoft.SharePoint.Taxonomy;
  using System.Collections.Generic;

  [Serializable]
  public class TaxonomySessionInstance : ObjectInstance
  {
    private readonly TaxonomySession m_taxonomySession;

    public TaxonomySessionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public TaxonomySessionInstance(ObjectInstance prototype, TaxonomySession taxonomySession)
      : this(prototype)
    {
      this.m_taxonomySession = taxonomySession;
    }

    [JSFunction(Name = "getDefaultKeywordsTermStore")]
    public TermStoreInstance GetDefaultKeywordsTermStore()
    {
      var termStore = m_taxonomySession.DefaultKeywordsTermStore;
      return new TermStoreInstance(this.Engine.Object.InstancePrototype, termStore);
    }

    [JSFunction(Name = "getDefaultSiteCollectionTermStore")]
    public TermStoreInstance GetDefaultSiteCollectionTermStore()
    {
      var termStore = m_taxonomySession.DefaultSiteCollectionTermStore;
      return new TermStoreInstance(this.Engine.Object.InstancePrototype, termStore);
    }

    [JSFunction(Name = "getTerm")]
    public TermInstance GetTerm(string termId)
    {
      Guid termGuid = new Guid(termId);
      var term = m_taxonomySession.GetTerm(termGuid);
      return new TermInstance(this.Engine.Object.InstancePrototype, term);
    }

    [JSFunction(Name = "getTerms")]
    public ArrayInstance GetTerms(string termLabel, bool trimUnavailable)
    {
      var result = this.Engine.Array.Construct();
      foreach (var term in m_taxonomySession.GetTerms(termLabel, trimUnavailable))
      {
        ArrayInstance.Push(result, new TermInstance(this.Engine.Object.InstancePrototype, term));
      }
      return result;
    }

    //TODO: All the getterm overloads.

    [JSFunction(Name = "getTermSets")]
    public ArrayInstance GetTermSets(ArrayInstance termLabels)
    {
      var termLabelsList = new List<string>();
      for(int i = 0; i < termLabels.Length; i++)
      {
        var label = termLabels[i] as string;
        if (label != null)
          termLabelsList.Add(label);
      }

      var result = this.Engine.Array.Construct();

      foreach (var termSet in m_taxonomySession.GetTermSets(termLabelsList.ToArray()))
      {
        ArrayInstance.Push(result, new TermSetInstance(this.Engine.Object.InstancePrototype, termSet));
      }
      return result;
    }

    public ArrayInstance GetTermSets(string termSetName, int lcid)
    {
      var result = this.Engine.Array.Construct();

      foreach (var termSet in m_taxonomySession.GetTermSets(termSetName, lcid))
      {
        ArrayInstance.Push(result, new TermSetInstance(this.Engine.Object.InstancePrototype, termSet));
      }
      return result;
    }

    public ArrayInstance GetTermSets(ArrayInstance termLabels, int lcid)
    {
      List<String> termLabelsList = new List<string>();
      for (int i = 0; i < termLabels.Length; i++)
      {
        var label = termLabels[i] as string;
        if (label != null)
          termLabelsList.Add(label);
      }

      var result = this.Engine.Array.Construct();

      foreach (var termSet in m_taxonomySession.GetTermSets(termLabelsList.ToArray(), lcid))
      {
        ArrayInstance.Push(result, new TermSetInstance(this.Engine.Object.InstancePrototype, termSet));
      }
      return result;
    }

    [JSFunction(Name = "getTermsInDefaultLanguage")]
    public ArrayInstance GetTermsInDefaultLanguage(string termLabel, bool defaultLabelOnly, string stringMatchOption, int resultCollectionSize, bool trimUnavailable, bool trimDeprecated)
    {
      var stringMatchOptionEnum = (StringMatchOption)Enum.Parse(typeof(StringMatchOption), stringMatchOption);
      var result = this.Engine.Array.Construct();
      foreach (var term in m_taxonomySession.GetTermsInDefaultLanguage(termLabel, defaultLabelOnly, stringMatchOptionEnum, resultCollectionSize, trimUnavailable, trimDeprecated))
      {
        ArrayInstance.Push(result, new TermInstance(this.Engine.Object.InstancePrototype, term));
      }
      return result;
    }

    [JSFunction(Name = "getTermsInWorkingLocale")]
    public ArrayInstance GetTermsInWorkingLocale(string termLabel, bool defaultLabelOnly, string stringMatchOption, int resultCollectionSize, bool trimUnavailable, bool trimDeprecated)
    {
      var stringMatchOptionEnum = (StringMatchOption)Enum.Parse(typeof(StringMatchOption), stringMatchOption);
      var result = this.Engine.Array.Construct();
      foreach (var term in m_taxonomySession.GetTermsInWorkingLocale(termLabel, defaultLabelOnly, stringMatchOptionEnum, resultCollectionSize, trimUnavailable, trimDeprecated))
      {
        ArrayInstance.Push(result, new TermInstance(this.Engine.Object.InstancePrototype, term));
      }
      return result;
    }

    //TODO: a few more GetTerm overloads.
  }
}
