namespace Barista.SharePoint.Library
{
    using Barista.Library;
    using Barista.SharePoint.Taxonomy.Library;
    using Jurassic;
    using Jurassic.Library;
    using Microsoft.Office.Server.Utilities;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.Taxonomy;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    [Serializable]
    public class SPSiteConstructor : ClrFunction
    {
        public SPSiteConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPSite", new SPSiteInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPSiteInstance Construct(string siteUrl)
        {
            SPSite site;

            if (SPHelper.TryGetSPSite(siteUrl, out site) == false)
                throw new JavaScriptException(this.Engine, "Error", "A site is not available at the specified url.");

            return new SPSiteInstance(this.InstancePrototype, site);
        }

        public SPSiteInstance Construct(SPSite site)
        {
            if (site == null)
                throw new ArgumentNullException("site");

            return new SPSiteInstance(this.InstancePrototype, site);
        }
    }

    [Serializable]
    public class SPSiteInstance : ObjectInstance, IDisposable
    {
        private readonly SPSite m_site;

        public SPSiteInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPSiteInstance(ObjectInstance prototype, SPSite site)
            : this(prototype)
        {
            this.m_site = site;
        }

        internal SPSite Site
        {
            get { return m_site; }
        }

        #region Properties

        [JSProperty(Name = "allWebs")]
        public SPWebCollectionInstance AllWebs
        {
            get { return new SPWebCollectionInstance(this.Engine.Object.InstancePrototype, m_site.AllWebs); }
        }

        [JSProperty(Name = "audit")]
        public SPAuditInstance Audit
        {
            get
            {
                return new SPAuditInstance(this.Engine.Object.InstancePrototype, m_site.Audit);
            }
        }

        [JSProperty(Name = "allowDesigner")]
        public bool AllowDesigner
        {
            get { return m_site.AllowDesigner; }
            set { m_site.AllowDesigner = value; }
        }

        [JSProperty(Name = "allowMasterPageEditing")]
        public bool AllowMasterPageEditing
        {
            get { return m_site.AllowMasterPageEditing; }
            set { m_site.AllowMasterPageEditing = value; }
        }

        [JSProperty(Name = "allowRevertFromTemplate")]
        public bool AllowRevertFromTemplate
        {
            get { return m_site.AllowRevertFromTemplate; }
            set { m_site.AllowRevertFromTemplate = value; }
        }

        [JSProperty(Name = "allowRssFeeds")]
        public bool AllowRssFeeds
        {
            get { return m_site.AllowRssFeeds; }
        }

        [JSProperty(Name = "allowUnsafeUpdates")]
        public bool AllowUnsafeUpdates
        {
            get { return m_site.AllowUnsafeUpdates; }
            set { m_site.AllowUnsafeUpdates = value; }
        }

        [JSProperty(Name = "features")]
        public SPFeatureCollectionInstance Features
        {
            get { return new SPFeatureCollectionInstance(this.Engine.Object.InstancePrototype, m_site.Features); }
        }

        [JSProperty(Name = "id")]
        public GuidInstance Id
        {
            get { return new GuidInstance(this.Engine.Object.InstancePrototype, m_site.ID); }
        }

        [JSProperty(Name = "maxItemsPerThrottledOperation")]
        public string MaxItemsPerThrottledOperation
        {
            get { return m_site.WebApplication.MaxItemsPerThrottledOperation.ToString(CultureInfo.InvariantCulture); }
        }

        [JSProperty(Name = "port")]
        public int Port
        {
            get
            {
                return m_site.Port;
            }
        }

        [JSProperty(Name = "portalName")]
        public string PortalName
        {
            get
            {
                return m_site.PortalName;
            }
            set
            {
                m_site.PortalName = value;
            }
        }

        [JSProperty(Name = "portalUrl")]
        public string PortalUrl
        {
            get
            {
                return m_site.PortalUrl;
            }
            set
            {
                m_site.PortalUrl = value;
            }
        }

        [JSProperty(Name = "protocol")]
        public string Protocol
        {
            get
            {
                return m_site.Protocol;
            }
        }

        [JSProperty(Name = "readLocked")]
        public bool ReadLocked
        {
            get
            {
                return m_site.ReadLocked;
            }
            set
            {
                m_site.ReadLocked = value;
            }
        }

        [JSProperty(Name = "readOnly")]
        public bool ReadOnly
        {
            get
            {
                return m_site.ReadOnly;
            }
            set
            {
                m_site.ReadOnly = value;
            }
        }

        [JSProperty(Name = "rootWeb")]
        public SPWebInstance RootWeb
        {
            get { return new SPWebInstance(this.Engine, m_site.RootWeb); }
        }

        [JSProperty(Name = "serverRelativeUrl")]
        public string ServerRelativeUrl
        {
            get { return m_site.ServerRelativeUrl; }
        }

        [JSProperty(Name = "showUrlStructure")]
        public bool ShowUrlStructure
        {
            get { return m_site.ShowURLStructure; }
            set { m_site.ShowURLStructure = value; }
        }

        [JSProperty(Name = "uiVersionConfigurationEnabled")]
        public bool UIVersionConfigurationEnabled
        {
            get { return m_site.UIVersionConfigurationEnabled; }
            set { m_site.UIVersionConfigurationEnabled = value; }
        }

        [JSProperty(Name = "url")]
        public string Url
        {
            get { return m_site.Url; }
        }

        [JSProperty(Name = "writeLocked")]
        public bool WriteLocked
        {
            get
            {
                return m_site.WriteLocked;
            }
            set
            {
                m_site.WriteLocked = value;
            }
        }

        [JSProperty(Name = "zone")]
        public object Zone
        {
            get
            {
                var result = this.Engine.Array.Construct();

                foreach (var zone in m_site.Zone.GetIndividualFlags())
                {
                    ArrayInstance.Push(result, zone.ToString());
                }

                return result.Length == 1 ? result.ElementValues.First() : result;
            }
        }

        //TODO: Usage, UserCustomActions
        #endregion

        #region Functions

        [JSFunction(Name = "activateFeature")]
        public SPFeatureInstance ActivateFeature(object feature, object force)
        {
            var featureId = Guid.Empty;
            if (feature is string)
            {
                featureId = new Guid(feature as string);
            }
            else if (feature is GuidInstance)
            {
                featureId = (feature as GuidInstance).Value;
            }
            else if (feature is SPFeatureInstance)
            {
                featureId = (feature as SPFeatureInstance).Feature.DefinitionId;
            }
            else if (feature is SPFeatureDefinitionInstance)
            {
                featureId = (feature as SPFeatureDefinitionInstance).FeatureDefinition.Id;
            }

            if (featureId == Guid.Empty)
                return null;

            var forceValue = JurassicHelper.GetTypedArgumentValue(this.Engine, force, false);

            var activatedFeature = m_site.Features.Add(featureId, forceValue);
            return new SPFeatureInstance(this.Engine.Object.InstancePrototype, activatedFeature);
        }

        //TODO: GetCatalog, GetChanges, GetCustomListTemplates
        [JSFunction(Name = "createWeb")]
        public SPWebInstance CreateWeb(object webCreationInfo)
        {
            SPWeb createdWeb;

            if (webCreationInfo is string)
                createdWeb = m_site.AllWebs.Add(webCreationInfo as string);
            else
            {
                var creationInfo = JurassicHelper.Coerce<SPWebCreationInformation>(this.Engine, webCreationInfo);

                if (creationInfo.WebTemplate is string)
                    createdWeb = m_site.AllWebs.Add(creationInfo.Url, creationInfo.Title, creationInfo.Description, (uint)creationInfo.Language, creationInfo.WebTemplate as string, !creationInfo.UseSamePermissionsAsParentSite, creationInfo.ConvertIfThere);
                else
                {
                    //attempt to get an instance of a web template from the original object.
                    var webCreationInstance = webCreationInfo as ObjectInstance;
                    if (webCreationInstance == null)
                        throw new JavaScriptException(this.Engine, "Error", "Unable to create a web from the specified template. Could not determine the value of the web template.");

                    if (webCreationInstance.HasProperty("webTemplate") == false)
                        throw new JavaScriptException(this.Engine, "Error", "Unable to create a web from the specified template. Web Template property was null.");

                    var webTemplate = JurassicHelper.Coerce<SPWebTemplateInstance>(this.Engine, webCreationInstance.GetPropertyValue("webTemplate"));
                    createdWeb = m_site.AllWebs.Add(creationInfo.Url, creationInfo.Title, creationInfo.Description, (uint)creationInfo.Language, webTemplate.WebTemplate, !creationInfo.UseSamePermissionsAsParentSite, creationInfo.ConvertIfThere);
                }
            }

            return new SPWebInstance(this.Engine, createdWeb);
        }

        [JSFunction(Name = "getWebApplication")]
        public SPWebApplicationInstance GetWebApplication()
        {
            return new SPWebApplicationInstance(this.Engine.Object.InstancePrototype, m_site.WebApplication);
        }

        [JSFunction(Name = "deactivateFeature")]
        public void DeactivateFeature(object feature)
        {
            var featureId = Guid.Empty;
            if (feature is string)
            {
                featureId = new Guid(feature as string);
            }
            else if (feature is GuidInstance)
            {
                featureId = (feature as GuidInstance).Value;
            }
            else if (feature is SPFeatureInstance)
            {
                featureId = (feature as SPFeatureInstance).Feature.DefinitionId;
            }
            else if (feature is SPFeatureDefinitionInstance)
            {
                featureId = (feature as SPFeatureDefinitionInstance).FeatureDefinition.Id;
            }

            if (featureId == Guid.Empty)
                return;

            m_site.Features.Remove(featureId);
        }

        [JSFunction(Name = "getAllWebs")]
        public ArrayInstance GetAllWebs()
        {
            var webs = new List<SPWeb>();

            ContentIterator ci = new ContentIterator();
            ci.ProcessSite(m_site, true, webs.Add,
              (web, ex) => false);

            var result = this.Engine.Array.Construct();
            foreach (var web in webs)
            {
                ArrayInstance.Push(result, new SPWebInstance(this.Engine, web));
            }
            return result;
        }

        [JSFunction(Name = "getContentDatabase")]
        public SPContentDatabaseInstance GetContentDatabase()
        {
            return new SPContentDatabaseInstance(this.Engine.Object.InstancePrototype, m_site.ContentDatabase);
        }

        [JSFunction(Name = "getFeatureDefinitions")]
        public ArrayInstance GetFeatureDefinitions()
        {
            //SPSite.FeatureDefinitions always returns null... nice, SharePoint, nice...

            var result = this.Engine.Array.Construct();
            foreach (var featureDefinition in SPFarm.Local.FeatureDefinitions)
            {
                if (featureDefinition.Scope == SPFeatureScope.Site)
                {
                    ArrayInstance.Push(result, new SPFeatureDefinitionInstance(this.Engine.Object.InstancePrototype, featureDefinition));
                }
            }
            return result;
        }

        [JSFunction(Name = "getPermissions")]
        public SPSecurableObjectInstance GetPermissions()
        {
            return new SPSecurableObjectInstance(this.Engine)
            {
                SecurableObject = m_site.RootWeb
            };
        }

        [JSFunction(Name = "getRecycleBin")]
        public SPRecycleBinItemCollectionInstance GetRecycleBin()
        {
            return new SPRecycleBinItemCollectionInstance(this.Engine.Object.InstancePrototype, m_site.RecycleBin);
        }

        [JSFunction(Name = "getTaxonomySession")]
        public TaxonomySessionInstance GetTaxonomySession()
        {
            var session = new TaxonomySession(m_site);
            return new TaxonomySessionInstance(this.Engine.Object.InstancePrototype, session);
        }

        [JSFunction(Name = "getWebTemplates")]
        public ArrayInstance GetWebTemplates(object language)
        {
            var lcid = (uint)System.Threading.Thread.CurrentThread.CurrentCulture.LCID;

            // ReSharper disable PossibleInvalidCastException
            if (language is int)
                lcid = (uint)language;
            // ReSharper restore PossibleInvalidCastException

            var result = this.Engine.Array.Construct();
            var webTemplates = m_site.GetWebTemplates(lcid).OfType<SPWebTemplate>();
            foreach (var webTemplate in webTemplates)
            {
                ArrayInstance.Push(result, new SPWebTemplateInstance(this.Engine.Object.InstancePrototype, webTemplate));
            }
            return result;
        }

        [JSFunction(Name = "getUsageInfo")]
        public UsageInfoInstance GetUsage()
        {
            return new UsageInfoInstance(this.Engine.Object.InstancePrototype, m_site.Usage);
        }

        [JSFunction(Name = "openWeb")]
        public SPWebInstance OpenWeb(string url)
        {
            return new SPWebInstance(this.Engine, m_site.OpenWeb(url, false));
        }

        [JSFunction(Name = "openWebById")]
        public SPWebInstance OpenWebById(object guid)
        {
            var webId = GuidInstance.ConvertFromJsObjectToGuid(guid);

            return new SPWebInstance(this.Engine, m_site.OpenWeb(webId));
        }

        [JSFunction(Name = "dispose")]
        public void Dispose()
        {
            if (m_site != null)
                m_site.Dispose();
        }
        #endregion
    }
}
