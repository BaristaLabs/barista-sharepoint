namespace Barista.SharePoint.Taxonomy.Library
{
    using System.Reflection;
    using Barista.Jurassic;
    using Barista.Library;
    using Barista.SharePoint.Library;
    using Jurassic.Library;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Taxonomy;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [Serializable]
    public class TaxonomySessionConstructor : ClrFunction
    {
        public TaxonomySessionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "TaxonomySession", new TermCollectionInstance(engine.Object.InstancePrototype))
        {
            this.PopulateFunctions();
        }

        [JSConstructorFunction]
        public TaxonomySessionInstance Construct(object site)
        {
            TaxonomySession session;
            if (site is SPSiteInstance)
                session = new TaxonomySession((site as SPSiteInstance).Site);
            else if (site is GuidInstance)
            {
                var id = (site as GuidInstance).Value;

                var spSite = new SPSite(id, SPBaristaContext.Current.Site.UserToken);
                session = new TaxonomySession(spSite);
            }
            else if (site is UriInstance)
            {
                var uri = (site as UriInstance).Uri;

                var spSite = new SPSite(uri.ToString(), SPBaristaContext.Current.Site.UserToken);
                session = new TaxonomySession(spSite);
            }
            else
            {
                var url = TypeConverter.ToString(site);

                var spSite = new SPSite(url, SPBaristaContext.Current.Site.UserToken);
                session = new TaxonomySession(spSite);
            }

            return new TaxonomySessionInstance(this.Engine.Object.InstancePrototype, session);
        }

        [JSFunction(Name = "syncHiddenList")]
        public void SyncHiddenList(object site)
        {
            if (site == null)
                throw new JavaScriptException(this.Engine, "Error", "A SPSite must be specified as the first argument.");

            SPSite spSite;
            if (site is SPSiteInstance)
                spSite = (site as SPSiteInstance).Site;
            else if (site is GuidInstance)
            {
                var id = (site as GuidInstance).Value;

                spSite = new SPSite(id, SPBaristaContext.Current.Site.UserToken);
            }
            else if (site is UriInstance)
            {
                var uri = (site as UriInstance).Uri;

                spSite = new SPSite(uri.ToString(), SPBaristaContext.Current.Site.UserToken);
            }
            else
            {
                var url = TypeConverter.ToString(site);

                spSite = new SPSite(url, SPBaristaContext.Current.Site.UserToken);
            }

            TaxonomySession.SyncHiddenList(spSite);
        }

        [JSFunction(Name = "addTaxonomyGuidToWss")]
        [JSDoc("Adds the specified term to the hidden taxonomy list on the specified list and returns the list id of the term.")]
        public int AddTaxonomyGuidToWss(object site, TermInstance term, bool isKeywordField)
        {
            if (site == null || site == Undefined.Value || site == Null.Value)
                throw new JavaScriptException(this.Engine, "Error", "A Site instance or url must be supplied as the first argument.");

            if (term == null)
                throw new JavaScriptException(this.Engine, "Error", "A term instance must be supplied as the second argument.");

            SPSite spSite;
            var dispose = false;

            if (site is SPSiteInstance)
            {
                spSite = (site as SPSiteInstance).Site;
            }
            else
            {
                spSite = new SPSite(TypeConverter.ToString(site), SPBaristaContext.Current.Site.UserToken);
                dispose = true;
            }

            try
            {
                var result = -1;
                var taxonomyFieldType = typeof(TaxonomyField);
                var miAddTaxonomyGuidToWss = taxonomyFieldType.GetMethod("AddTaxonomyGuidToWss",
                  BindingFlags.NonPublic | BindingFlags.Static, null,
                  new[] { typeof(SPSite), typeof(Term), typeof(bool) },
                  null
                  );

                if (miAddTaxonomyGuidToWss != null)
                    result = (int)miAddTaxonomyGuidToWss.Invoke(null, new object[] { spSite, term.Term, isKeywordField });

                return result;
            }
            finally
            {
                if (dispose)
                {
                    spSite.Dispose();
                }
            }
        }
    }

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

        [JSProperty(Name = "offlineTermStoreNames")]
        [JSDoc("ternPropertyType", "[string]")]
        public ArrayInstance OfflineTermStoreNames
        {
            get
            {
                // ReSharper disable CoVariantArrayConversion
                return this.Engine.Array.Construct(m_taxonomySession.OfflineTermStoreNames.OfType<string>().ToArray());
                // ReSharper restore CoVariantArrayConversion
            }
        }

        [JSProperty(Name = "termStores")]
        public TermStoreCollectionInstance TermStores
        {
            get
            {
                return m_taxonomySession.TermStores == null
                  ? null
                  : new TermStoreCollectionInstance(this.Engine.Object.InstancePrototype, m_taxonomySession.TermStores);
            }
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
        [JSDoc(
          "Gets a Term object that is based on Term IDs. If the current Term belongs to multiple TermSet objects, it will arbitrarily return the Term from one of the TermSet objects."
          )]
        public TermInstance GetTerm(object termId)
        {
            var termGuid = GuidInstance.ConvertFromJsObjectToGuid(termId);
            var term = m_taxonomySession.GetTerm(termGuid);
            return new TermInstance(this.Engine.Object.InstancePrototype, term);
        }

        [JSFunction(Name = "getTerms")]
        [JSDoc("Gets Term objects for the current TaxonomySession object.")]
        public TermCollectionInstance GetTerms(string termLabel, object arg2, object arg3)
        {
            TermCollection result;
            if (arg3 == Undefined.Value || arg3 == Null.Value || arg3 == null)
                result = m_taxonomySession.GetTerms(termLabel, TypeConverter.ToBoolean(arg2));
            else
                result = m_taxonomySession.GetTerms(termLabel, TypeConverter.ToInteger(arg2), TypeConverter.ToBoolean(arg3));

            return result == null
              ? null
              : new TermCollectionInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getTermsFromIds")]
        public TermCollectionInstance GetTermsFromIds(ArrayInstance ids)
        {
            var guids = ids.ElementValues.Select(GuidInstance.ConvertFromJsObjectToGuid).ToArray();

            var result = m_taxonomySession.GetTerms(guids);
            return result == null
              ? null
              : new TermCollectionInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getTermSetsFromLabels")]
        public TermSetCollectionInstance GetTermSets(ArrayInstance termLabels, object lcid)
        {
            var termLabelsList = new List<string>();
            for (var i = 0; i < termLabels.Length; i++)
            {
                var label = termLabels[i] as string;
                if (label != null)
                    termLabelsList.Add(label);
            }

            TermSetCollectionInstance result = null;

            if (lcid == null || lcid == Null.Value || lcid == Undefined.Value)
            {
                var termSets = m_taxonomySession.GetTermSets(termLabelsList.ToArray());
                if (termSets != null)
                    result = new TermSetCollectionInstance(this.Engine.Object.InstancePrototype, termSets);
            }
            else
            {
                var termSets = m_taxonomySession.GetTermSets(termLabelsList.ToArray(), TypeConverter.ToInteger(lcid));

                if (termSets != null)
                    result = new TermSetCollectionInstance(this.Engine.Object.InstancePrototype, termSets);
            }

            return result;
        }

        [JSFunction(Name = "getTermSets")]
        public TermSetCollectionInstance GetTermSets(string termSetName, int lcid)
        {
            var termSets = m_taxonomySession.GetTermSets(termSetName, lcid);

            return termSets == null
              ? null
              : new TermSetCollectionInstance(this.Engine.Object.InstancePrototype, termSets);
        }

        [JSFunction(Name = "getTermsEx")]
        public TermCollectionInstance GetTermsEx(string termLabel, bool defaultLabelOnly, string stringMatchOption,
          int resultCollectionSize, bool trimUnavailable)
        {
            var stringMatchOptionEnum = (StringMatchOption)Enum.Parse(typeof(StringMatchOption), stringMatchOption);

            var result = m_taxonomySession.GetTerms(termLabel, defaultLabelOnly, stringMatchOptionEnum,
              resultCollectionSize, trimUnavailable);
            return result == null
              ? null
              : new TermCollectionInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getTermsEx2")]
        public TermCollectionInstance GetTermsEx2(string termLabel, int lcid, bool defaultLabelOnly,
          string stringMatchOption, int resultCollectionSize, bool trimUnavailable, bool trimDeprecated)
        {
            var stringMatchOptionEnum = (StringMatchOption)Enum.Parse(typeof(StringMatchOption), stringMatchOption);

            var result = m_taxonomySession.GetTerms(termLabel, lcid, defaultLabelOnly, stringMatchOptionEnum,
              resultCollectionSize, trimUnavailable, trimDeprecated);
            return result == null
              ? null
              : new TermCollectionInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getTermsInDefaultLanguage")]
        public TermCollectionInstance GetTermsInDefaultLanguage(string termLabel, bool defaultLabelOnly,
          string stringMatchOption, int resultCollectionSize, bool trimUnavailable, bool trimDeprecated)
        {
            var stringMatchOptionEnum = (StringMatchOption)Enum.Parse(typeof(StringMatchOption), stringMatchOption);

            var result = m_taxonomySession.GetTermsInDefaultLanguage(termLabel, defaultLabelOnly, stringMatchOptionEnum,
              resultCollectionSize, trimUnavailable, trimDeprecated);
            return result == null
              ? null
              : new TermCollectionInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getTermsInWorkingLocale")]
        public TermCollectionInstance GetTermsInWorkingLocale(string termLabel, bool defaultLabelOnly,
          string stringMatchOption, int resultCollectionSize, bool trimUnavailable, bool trimDeprecated)
        {
            var stringMatchOptionEnum = (StringMatchOption)Enum.Parse(typeof(StringMatchOption), stringMatchOption);

            var result = m_taxonomySession.GetTermsInWorkingLocale(termLabel, defaultLabelOnly, stringMatchOptionEnum,
              resultCollectionSize, trimUnavailable, trimDeprecated);
            return result == null
              ? null
              : new TermCollectionInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getTermsWithCustomProperty")]
        public TermCollectionInstance GetTermsWithCustomProperty(string customPropertyName, string customPropertyValue,
          string stringMatchOption, int resultCollectionSize, bool trimUnavailable)
        {
            var stringMatchOptionEnum = (StringMatchOption)Enum.Parse(typeof(StringMatchOption), stringMatchOption);

            var result = m_taxonomySession.GetTermsWithCustomProperty(customPropertyName, customPropertyValue, stringMatchOptionEnum,
              resultCollectionSize, trimUnavailable);
            return result == null
              ? null
              : new TermCollectionInstance(this.Engine.Object.InstancePrototype, result);
        }
    }
}
