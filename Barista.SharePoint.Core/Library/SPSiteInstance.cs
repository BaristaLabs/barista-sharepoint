namespace Barista.SharePoint.Library
{
    using System.Text;
    using Barista.Extensions;
    using Barista.Library;
    using Barista.SharePoint.Taxonomy.Library;
    using Barista.SharePoint.Workflow;
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
            PopulateFunctions();
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

        [JSFunction(Name = "getSiteFromGuid")]
        public SPSiteInstance CreateFromGuid(object id, object userToken)
        {
            var guidId = GuidInstance.ConvertFromJsObjectToGuid(id);

            SPUserToken spUserToken = null;
            if (userToken != Undefined.Value)
            {
                if (userToken is Base64EncodedByteArrayInstance)
                    spUserToken = new SPUserToken((userToken as Base64EncodedByteArrayInstance).Data);
                else if (userToken is SPUserTokenInstance)
                    spUserToken = (userToken as SPUserTokenInstance).SPUserToken;
                else
                    spUserToken = new SPUserToken(Encoding.UTF8.GetBytes(TypeConverter.ToString(userToken)));
            }

            var site = spUserToken == null
                ? new SPSite(guidId)
                : new SPSite(guidId, spUserToken);

            return new SPSiteInstance(this.Engine.Object.InstancePrototype, site);
        }

        [JSFunction(Name = "exists")]
        public bool Exists(object uri)
        {
            Uri localUri;
            if (uri is UriInstance)
                localUri = (uri as UriInstance).Uri;
            else
                localUri = new Uri(TypeConverter.ToString(uri));

            return SPSite.Exists(localUri);
        }

        [JSFunction(Name = "invalidateCacheEntry")]
        public bool InvalidateCacheEntry(object uri, object siteId)
        {
            Uri localUri;
            if (uri is UriInstance)
                localUri = (uri as UriInstance).Uri;
            else
                localUri = new Uri(TypeConverter.ToString(uri));

            var guidSiteId = GuidInstance.ConvertFromJsObjectToGuid(siteId);
            return SPSite.InvalidateCacheEntry(localUri, guidSiteId);
        }

        //LookupUriInRemoteFarm

        [JSFunction(Name = "validateDomainCompatibility")]
        public bool ValidateDomainCompatibility(string site1Url, string site2Url)
        {
            return SPSite.ValidateDomainCompatibility(site1Url, site2Url);
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

        //AdminSiteType

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

        [JSProperty(Name = "allWebs")]
        public SPWebCollectionInstance AllWebs
        {
            get
            {
                return m_site.AllWebs == null
                    ? null
                    : new SPWebCollectionInstance(this.Engine.Object.InstancePrototype, m_site.AllWebs);
            }
        }

        //ApplcaitonRightsMask

        [JSProperty(Name = "audit")]
        public SPAuditInstance Audit
        {
            get
            {
                return m_site.Audit == null
                    ? null
                    : new SPAuditInstance(this.Engine.Object.InstancePrototype, m_site.Audit);
            }
        }

        [JSProperty(Name = "auditLogTrimmingCallout")]
        public string AuditLogTrimmingCallout
        {
            get
            {
                return m_site.AuditLogTrimmingCallout;
            }
            set
            {
                m_site.AuditLogTrimmingCallout = value;
            }
        }

        [JSProperty(Name = "auditLogTrimmingRetention")]
        public int AuditLogTrimmingRetention
        {
            get
            {
                return m_site.AuditLogTrimmingRetention;
            }
            set
            {
                m_site.AuditLogTrimmingRetention = value;
            }
        }

        [JSProperty(Name = "averageResourceUsage")]
        public double AverageResourceUsage
        {
            get
            {
                return m_site.AverageResourceUsage;
            }
        }

        [JSProperty(Name = "browserDocumentsEnabled")]
        public bool BrowserDocumentsEnabled
        {
            get
            {
                return m_site.BrowserDocumentsEnabled;
            }
        }

        //Cache

        [JSProperty(Name = "catchAccessDeniedException")]
        public bool CatchAccessDeniedException
        {
            get
            {
                return m_site.CatchAccessDeniedException;
            }
            set
            {
                m_site.CatchAccessDeniedException = value;
            }
        }

        [JSProperty(Name = "certificationDate")]
        public DateInstance CertificationDate
        {
            get
            {
                return JurassicHelper.ToDateInstance(this.Engine, m_site.CertificationDate);
            }
        }

        [JSProperty(Name = "contentDatabase")]
        public SPContentDatabaseInstance ContentDatabase
        {
            get
            {
                return m_site.ContentDatabase == null
                    ? null
                    : new SPContentDatabaseInstance(this.Engine.Object.InstancePrototype, m_site.ContentDatabase);
            }
        }

        //currentchangetoken

        [JSProperty(Name = "currentResourceUsage")]
        public double CurrentResourceUsage
        {
            get
            {
                return m_site.CurrentResourceUsage;
            }
        }

        [JSProperty(Name = "deadWebNotificationCount")]
        public int DeadWebNotificationCount
        {
            get
            {
                return m_site.DeadWebNotificationCount;
            }
        }

        [JSProperty(Name = "eventReceivers")]
        public SPEventReceiverDefinitionCollectionInstance EventReceivers
        {
            get
            {
                return m_site.EventReceivers == null
                    ? null
                    : new SPEventReceiverDefinitionCollectionInstance(this.Engine.Object.InstancePrototype, m_site.EventReceivers);
            }
        }

        //externalbinaryids

        [JSProperty(Name = "featureDefinitions")]
        public SPFeatureDefinitionCollectionInstance FeatureDefinitions
        {
            get
            {
                return m_site.FeatureDefinitions == null
                    ? null
                    : new SPFeatureDefinitionCollectionInstance(this.Engine.Object.InstancePrototype, m_site.FeatureDefinitions);
            }
        }

        [JSProperty(Name = "features")]
        public SPFeatureCollectionInstance Features
        {
            get
            {
                return m_site.Features == null
                    ? null
                    : new SPFeatureCollectionInstance(this.Engine.Object.InstancePrototype, m_site.Features);
            }
        }

        //globalpermmask

        [JSProperty(Name = "hostHeaderIsSiteName")]
        public bool HostHeaderIsSiteName
        {
            get
            {
                return m_site.HostHeaderIsSiteName;
            }
        }

        [JSProperty(Name = "hostName")]
        public string HostName
        {
            get
            {
                return m_site.HostName;
            }
        }

        [JSProperty(Name = "id")]
        public GuidInstance Id
        {
            get { return new GuidInstance(this.Engine.Object.InstancePrototype, m_site.ID); }
        }

        [JSProperty(Name = "iisAllowsAnonymous")]
        public bool IisAllowsAnonymous
        {
            get
            {
                return m_site.IISAllowsAnonymous;
            }
        }

        [JSProperty(Name = "impersonating")]
        public bool Impersonating
        {
            get
            {
                return m_site.Impersonating;
            }
        }

        [JSProperty(Name = "lastContentModifiedDate")]
        public DateInstance LastContentModifiedDate
        {
            get
            {
                return JurassicHelper.ToDateInstance(this.Engine, m_site.LastContentModifiedDate);
            }
        }

        [JSProperty(Name = "lastSecurityModifiedDate")]
        public DateInstance LastSecurityModifiedDate
        {
            get
            {
                return JurassicHelper.ToDateInstance(this.Engine, m_site.LastSecurityModifiedDate);
            }
        }

        [JSProperty(Name = "lockIssue")]
        public string LockIssue
        {
            get
            {
                return m_site.LockIssue;
            }
            set
            {
                m_site.LockIssue = value;
            }
        }

        [JSProperty(Name = "owner")]
        public SPUserInstance Owner
        {
            get
            {
                return m_site.Owner == null
                    ? null
                    : new SPUserInstance(this.Engine.Object.InstancePrototype, m_site.Owner);
            }
            set
            {
                m_site.Owner = value == null ? null : value.User;
            }
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

        [JSProperty(Name = "quota")]
        public SPQuotaInstance Quota
        {
            get
            {
                return m_site.Quota == null
                    ? null
                    : new SPQuotaInstance(this.Engine.Object.InstancePrototype, m_site.Quota);
            }
            set
            {
                m_site.Quota = value == null
                    ? null
                    : value.SPQuota;
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

        [JSProperty(Name="recycleBin")]
        public SPRecycleBinItemCollectionInstance RecycleBin
        {
            get
            {
                return m_site.RecycleBin == null
                    ? null
                    : new SPRecycleBinItemCollectionInstance(this.Engine.Object.InstancePrototype, m_site.RecycleBin);
            }
        }

        [JSProperty(Name = "resourceQuotaExceeded")]
        public bool ResourceQuotaExceeded
        {
            get
            {
                return m_site.ResourceQuotaExceeded;
            }
        }

        [JSProperty(Name = "resourceQuotaExceededNotificationSent")]
        public bool ResourceQuotaExceededNotificationSent
        {
            get
            {
                return m_site.ResourceQuotaExceededNotificationSent;
            }
        }

        [JSProperty(Name = "resourceQuotaWarningNotificationSent")]
        public bool ResourceQuotaWarningNotificationSent
        {
            get
            {
                return m_site.ResourceQuotaWarningNotificationSent;
            }
        }

        [JSProperty(Name = "rootWeb")]
        public SPWebInstance RootWeb
        {
            get { return new SPWebInstance(this.Engine, m_site.RootWeb); }
        }

        [JSProperty(Name = "searchServiceInstance")]
        public SPServiceInstanceInstance SearchServiceInstance
        {
            get
            {
                return m_site.SearchServiceInstance == null
                    ? null
                    : new SPServiceInstanceInstance(this.Engine.Object.InstancePrototype, m_site.SearchServiceInstance);
            }
        }

        [JSProperty(Name = "secondaryContact")]
        public SPUserInstance SecondaryContact
        {
            get
            {
                return m_site.SecondaryContact == null
                    ? null
                    : new SPUserInstance(this.Engine.Object.InstancePrototype, m_site.SecondaryContact);
            }
            set
            {
                m_site.SecondaryContact = value == null ? null : value.User;
            }
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

        //SiteSubscription

        [JSProperty(Name = "solutions")]
        public SPUserSolutionCollectionInstance Solutions
        {
            get
            {
                return m_site.Solutions == null
                    ? null
                    : new SPUserSolutionCollectionInstance(this.Engine.Object.InstancePrototype, m_site.Solutions);
            }
        }

        [JSProperty(Name = "syndicationEnabled")]
        public bool SyndicationEnabled
        {
            get
            {
                return m_site.SyndicationEnabled;
            }
            set
            {
                m_site.SyndicationEnabled = value;
            }
        }

        [JSProperty(Name = "systemAccount")]
        public SPUserInstance SystemAccount
        {
            get
            {
                return m_site.SystemAccount == null
                    ? null
                    : new SPUserInstance(this.Engine.Object.InstancePrototype, m_site.SystemAccount);
            }
        }

        [JSProperty(Name = "trimAuditLog")]
        public bool TrimAuditLog
        {
            get
            {
                return m_site.TrimAuditLog;
            }
            set
            {
                m_site.TrimAuditLog = value;
            }
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

        [JSProperty(Name="usage")]
        public UsageInfoInstance UsageInfo
        {
            get
            {
                return new UsageInfoInstance(this.Engine.Object.InstancePrototype, m_site.Usage);
            }
        }

        [JSProperty(Name = "userCodeEnabled")]
        public bool UserCodeEnabled
        {
            get
            {
                return m_site.UserCodeEnabled;
            }
        }

        [JSProperty(Name = "userCustomActions")]
        public SPUserCustomActionCollectionInstance UserCustomActions
        {
            get
            {
                return m_site.UserCustomActions == null
                    ? null
                    : new SPUserCustomActionCollectionInstance(this.Engine.Object.InstancePrototype, m_site.UserCustomActions);
            }
        }

        [JSProperty(Name = "userDefinedWorkflowsEnabled")]
        public bool UserDefinedWorkflowsEnabled
        {
            get
            {
                return m_site.UserDefinedWorkflowsEnabled;
            }
            set
            {
                m_site.UserDefinedWorkflowsEnabled = value;
            }
        }

        [JSProperty(Name = "userToken")]
        public SPUserTokenInstance UserToken
        {
            get
            {
                return m_site.UserToken == null
                    ? null
                    : new SPUserTokenInstance(this.Engine.Object.InstancePrototype, m_site.UserToken);
            }
        }

        [JSProperty(Name = "warningNotificationSent")]
        public bool WarningNotificationSent
        {
            get
            {
                return m_site.WarningNotificationSent;
            }
        }

        [JSProperty(Name = "workflowManager")]
        public SPWorkflowManagerInstance WorkflowManager
        {
            get
            {
                return m_site.WorkflowManager == null
                ? null
                : new SPWorkflowManagerInstance(this.Engine.Object.InstancePrototype, m_site.WorkflowManager);
            }
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
        #endregion

        #region Functions

        //AddWorkItem

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

        //BypassUseRemoteApis
        //CheckForPermissions

        [JSFunction(Name = "confirmUsage")]
        public bool ConfirmUsage()
        {
            return m_site.ConfirmUsage();
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

        [JSFunction(Name = "delete")]
        public void Delete(object deleteADAccounts, object gradualDelete)
        {
            if (deleteADAccounts == Undefined.Value || gradualDelete == Undefined.Value)
                m_site.Delete();
            else if (deleteADAccounts != Undefined.Value && gradualDelete == Undefined.Value)
                m_site.Delete(TypeConverter.ToBoolean(deleteADAccounts));
            else if (deleteADAccounts != Undefined.Value && gradualDelete != Undefined.Value)
                m_site.Delete(TypeConverter.ToBoolean(deleteADAccounts), TypeConverter.ToBoolean(gradualDelete));
        }

        [JSFunction(Name = "dispose")]
        public void Dispose()
        {
            if (m_site != null)
                m_site.Dispose();
        }

        //DoesUserHavePermissions

        //GetAllReusableAcls

        [JSFunction(Name = "getCatalog")]
        public SPListInstance GetCatalog(string typeCatalog)
        {
            SPListTemplateType listTemplateType;
            if (!typeCatalog.TryParseEnum(true, out listTemplateType))
                throw new JavaScriptException(this.Engine, "Error", "Type Catalog must be specified.");

            var result = m_site.GetCatalog(listTemplateType);
            return result == null
                ? null
                : new SPListInstance(this.Engine, m_site, result.ParentWeb, result);
        }

        //GetChanges
        //GetCustomListTemplates
        //GetCustomWebTemplates
        //GetEffectiveRightsForAcl
        //GetRecycleBinItems
        //GetRecycleBinStatistics
        //GetReusableAclForScope
        //GetSelfServiceSiteCreationSettings
        //GetVisualReport
        //GetWebTemplates
        //GetWorkItems

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

        [JSFunction(Name = "makeFullUrl")]
        public string MakeFullUrl(string strUrl)
        {
            return m_site.MakeFullUrl(strUrl);
        }

        [JSFunction(Name = "openWeb")]
        public SPWebInstance OpenWeb(object url, object requireExactUrl)
        {
            SPWeb web = null;
            if (url == Undefined.Value && requireExactUrl == Undefined.Value)
                web = m_site.OpenWeb();
            else if (url != Undefined.Value && requireExactUrl == Undefined.Value)
                web = m_site.OpenWeb(TypeConverter.ToString(url));
            else if (url != Undefined.Value && requireExactUrl != Undefined.Value)
                web = m_site.OpenWeb(TypeConverter.ToString(url), TypeConverter.ToBoolean(requireExactUrl));

            return web == null
                ? null
                : new SPWebInstance(this.Engine, web);
        }

        [JSFunction(Name = "openWebById")]
        public SPWebInstance OpenWebById(object guid)
        {
            var webId = GuidInstance.ConvertFromJsObjectToGuid(guid);

            return new SPWebInstance(this.Engine, m_site.OpenWeb(webId));
        }

        //QueryFeatures

        [JSFunction(Name = "recalculateStorageUsed")]
        public void RecalculateStorageUsed()
        {
            m_site.RecalculateStorageUsed();
        }

        [JSFunction(Name = "refreshEmailEnabledObjects")]
        public void RefreshEmailEnabledObjects()
        {
            m_site.RefreshEmailEnabledObjects();
        }

        [JSFunction(Name = "rename")]
        public void Rename(object uri)
        {
            Uri localUri;
            if (uri is UriInstance)
                localUri = (uri as UriInstance).Uri;
            else
                localUri = new Uri(TypeConverter.ToString(uri));

            m_site.Rename(localUri);
        }

        //SelfServiceCreateSite
        //StorageManagementInformation

        [JSFunction(Name = "toString")]
        public override string ToString()
        {
            return m_site.ToString();
        }

        [JSFunction(Name = "updateValidationKey")]
        public void UpdateValidationKey()
        {
            m_site.UpdateValidationKey();
        }

        [JSFunction(Name = "visualUpgradeWebs")]
        public void VisualUpgradeWebs()
        {
            m_site.VisualUpgradeWebs();
        }

        #endregion
    }
}
