namespace Barista.SharePoint.Library
{
    using System.Web.UI.WebControls.WebParts;
    using Barista.Extensions;
    using Barista.Library;
    using Barista.SharePoint.Workflow;
    using Jurassic;
    using Jurassic.Library;
    using Microsoft.Office.Server.Utilities;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Utilities;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    [Serializable]
    public class SPWebConstructor : ClrFunction
    {
        public SPWebConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPWeb", new SPWebInstance(engine, null))
        {
        }

        [JSConstructorFunction]
        public SPWebInstance Construct(string webUrl)
        {
            SPWeb web;

            if (SPHelper.TryGetSPWeb(webUrl, out web) == false)
                throw new JavaScriptException(this.Engine, "Error", "A web is not available at the specified url.");

            return new SPWebInstance(this.Engine, web);
        }

        public SPWebInstance Construct(SPWeb web)
        {
            if (web == null)
                throw new ArgumentNullException("web");

            return new SPWebInstance(this.Engine, web);
        }

        //OriginalBaseUrl
    }

    [JSDoc("Represents a SharePoint website.")]
    [Serializable]
    public class SPWebInstance : SPSecurableObjectInstance, IDisposable
    {
        private readonly SPWeb m_web;

        public SPWebInstance(ScriptEngine engine, SPWeb web)
            : base(new SPSecurableObjectInstance(engine))
        {
            this.m_web = web;
            SecurableObject = this.m_web;

            this.PopulateFunctions(this.GetType(), BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        }

        public SPWeb Web
        {
            get { return m_web; }
        }

        #region Properties

        [JSProperty(Name = "alerts")]
        public SPAlertCollectionInstance Alerts
        {
            get
            {
                return m_web.Alerts == null
                    ? null
                    : new SPAlertCollectionInstance(this.Engine.Object.InstancePrototype, m_web.Alerts);
            }
        }

        [JSProperty(Name = "allowAnonymousAccess")]
        public bool AllowAnonymousAccess
        {
            get { return m_web.AllowAnonymousAccess; }
        }

        [JSProperty(Name = "allowAutomaticAspxPageIndexing")]
        public bool AllowAutomaticAspxPageIndexing
        {
            get
            {
                return m_web.AllowAutomaticASPXPageIndexing;
            }
            set
            {
                m_web.AllowAutomaticASPXPageIndexing = value;
            }
        }

        [JSDoc("Gets a Boolean value that specifies whether the current user is allowed to use the designer for this website. The default value is false.")]
        [JSProperty(Name = "allowDesignerForCurrentUser")]
        public bool AllowDesignerForCurrentUser
        {
            get { return m_web.AllowDesignerForCurrentUser; }
        }

        [JSProperty(Name = "allowMasterPageEditingForCurrentUser")]
        public bool AllowMasterPageEditingForCurrentUser
        {
            get { return m_web.AllowMasterPageEditingForCurrentUser; }
        }

        [JSProperty(Name = "allowRevertFromTemplateForCurrentUser")]
        public bool AllowRevertFromTemplateForCurrentUser
        {
            get { return m_web.AllowRevertFromTemplateForCurrentUser; }
        }

        [JSProperty(Name = "allowRssFeeds")]
        public bool AllowRssFeeds
        {
            get { return m_web.AllowRssFeeds; }
        }

        [JSProperty(Name = "allowUnsafeUpdates")]
        public bool AllowUnsafeUpdates
        {
            get { return m_web.AllowUnsafeUpdates; }
            set { m_web.AllowUnsafeUpdates = value; }
        }

        [JSProperty(Name = "allProperties")]
        public HashtableInstance AllProperties
        {
            get
            {
                return m_web.AllProperties == null
                    ? null
                    : new HashtableInstance(this.Engine.Object.InstancePrototype, m_web.AllProperties);
            }
        }

        [JSProperty(Name = "allUsers")]
        public SPUserCollectionInstance AllUsers
        {
            get
            {
                return m_web.AllUsers == null
                  ? null
                  : new SPUserCollectionInstance(this.Engine.Object.InstancePrototype, m_web.AllUsers);
            }
        }

        [JSProperty(Name = "allWebTemplatesAllowed")]
        public bool AllWebTemplatesAllowed
        {
            get
            {
                return m_web.AllWebTemplatesAllowed;
            }
        }

        [JSProperty(Name = "alternateCssUrl")]
        public string AlternateCssUrl
        {
            get
            {
                return m_web.AlternateCssUrl;
            }
            set
            {
                m_web.AlternateCssUrl = value;
            }
        }

        [JSProperty(Name = "alternateHeader")]
        public string AlternateHeader
        {
            get
            {
                return m_web.AlternateHeader;
            }
            set
            {
                m_web.AlternateHeader = value;
            }
        }

        //AnonymousPermMask64
        //AnonymousState

        [JSProperty(Name = "aspxPageIndexed")]
        public bool AspxPageIndexed
        {
            get { return m_web.ASPXPageIndexed; }
        }

        [JSProperty(Name = "aspxPageIndexMode")]
        public string AspxPageIndexMode
        {
            get { return m_web.ASPXPageIndexMode.ToString(); }
            set
            {
                WebASPXPageIndexMode mode;
                if (!value.TryParseEnum(true, out mode))
                    throw new JavaScriptException(this.Engine, "Error", "Value must be a WebAspxPageIndexMode");
                
                m_web.ASPXPageIndexMode = mode;
            }
        }

        [JSProperty(Name = "associatedGroups")]
        public SPGroupListInstance AssociatedGroups
        {
            get
            {
                return m_web.AssociatedGroups == null
                    ? null
                    : new SPGroupListInstance(this.Engine, m_web.AssociatedGroups);
            }
        }

        [JSProperty(Name = "associatedMemberGroup")]
        public SPGroupInstance AssociatedMemberGroup
        {
            get
            {
                return m_web.AssociatedMemberGroup == null
                    ? null
                    : new SPGroupInstance(this.Engine.Object.InstancePrototype, m_web.AssociatedMemberGroup);
            }
        }

        [JSProperty(Name = "associatedOwnerGroup")]
        public SPGroupInstance AssociatedOwnerGroup
        {
            get
            {
                return m_web.AssociatedMemberGroup == null
                    ? null
                    : new SPGroupInstance(this.Engine.Object.InstancePrototype, m_web.AssociatedOwnerGroup);
            }
        }

        [JSProperty(Name = "associatedVisitorGroup")]
        public SPGroupInstance AssociatedVisitorGroup
        {
            get
            {
                return m_web.AssociatedMemberGroup == null
                    ? null
                    : new SPGroupInstance(this.Engine.Object.InstancePrototype, m_web.AssociatedVisitorGroup);
            }
        }

        [JSProperty(Name = "audit")]
        public SPAuditInstance Audit
        {
            get
            {
                return m_web.Audit == null
                    ? null
                    : new SPAuditInstance(this.Engine.Object.InstancePrototype, m_web.Audit);
            }
        }

        [JSProperty(Name = "author")]
        public SPUserInstance Author
        {
            get
            {
                return m_web.Author == null
                    ? null
                    : new SPUserInstance(this.Engine.Object.InstancePrototype, m_web.Author);
            }
        }

        [JSProperty(Name = "availableContentTypes")]
        public SPContentTypeCollectionInstance AvailableContentTypes
        {
            get
            {
                return m_web.AvailableContentTypes == null
                  ? null
                  : new SPContentTypeCollectionInstance(this.Engine.Object.InstancePrototype, m_web.AvailableContentTypes);
            }
        }

        [JSProperty(Name = "availableFields")]
        public SPFieldCollectionInstance AvailableFields
        {
            get
            {
                return m_web.AvailableFields == null
                  ? null
                  : new SPFieldCollectionInstance(this.Engine.Object.InstancePrototype, m_web.AvailableFields);
            }
        }

        [JSProperty(Name = "cacheAllSchema")]
        public bool CacheAllSchema
        {
            get
            {
                return m_web.CacheAllSchema;
            }
            set
            {
                m_web.CacheAllSchema = value;
            }
        }

        [JSProperty(Name = "clientTag")]
        public int ClientTag
        {
            get
            {
                return m_web.ClientTag;
            }
            set
            {
                m_web.ClientTag = (short)value;
            }
        }

        [JSProperty(Name = "configuration")]
        public int Configuration
        {
            get
            {
                return m_web.Configuration;
            }
        }


        [JSProperty(Name = "contentTypes")]
        public SPContentTypeCollectionInstance ContentTypes
        {
            get
            {
                return m_web.ContentTypes == null
                  ? null
                  : new SPContentTypeCollectionInstance(this.Engine.Object.InstancePrototype, m_web.ContentTypes);
            }
        }

        [JSProperty(Name = "created")]
        public DateInstance Created
        {
            get { return JurassicHelper.ToDateInstance(this.Engine, m_web.Created); }
        }

        [JSProperty(Name = "currencyLocaleId")]
        public int CurrencyLocaleId
        {
            get
            {
                return m_web.CurrencyLocaleID;
            }
            set
            {
                m_web.CurrencyLocaleID = value;
            }
        }

        //CurrentChangeToken

        [JSProperty(Name = "currentUser")]
        public SPUserInstance CurrentUser
        {
            get { return new SPUserInstance(this.Engine.Object.InstancePrototype, m_web.CurrentUser); }
        }

        [JSProperty(Name = "customJavaScriptFileUrl")]
        public string CustomJavaScriptFileUrl
        {
            get { return m_web.CustomJavaScriptFileUrl; }
            set { m_web.CustomJavaScriptFileUrl = value; }
        }

        [JSProperty(Name = "customMasterUrl")]
        public string CustomMasterUrl
        {
            get { return m_web.CustomMasterUrl; }
            set { m_web.CustomMasterUrl = value; }
        }

        [JSProperty(Name = "customUploadPage")]
        public string CustomUploadPage
        {
            get { return m_web.CustomUploadPage; }
            set { m_web.CustomUploadPage = value; }
        }

        //DataRetrievalServicesSettings

        [JSProperty(Name = "description")]
        public string Description
        {
            get { return m_web.Description; }
            set { m_web.Description = value; }
        }

        //DescriptionResource

        [JSProperty(Name = "docTemplates")]
        public SPDocTemplateCollectionInstance DocTemplates
        {
            get
            {
                return m_web.DocTemplates == null
                    ? null
                    : new SPDocTemplateCollectionInstance(this.Engine.Object.InstancePrototype, m_web.DocTemplates);
            }
        }

        //EffectiveBasePermissions

        [JSProperty(Name = "effectivePresenceEnabled")]
        public bool EffectivePresenceEnabled
        {
            get { return m_web.EffectivePresenceEnabled; }
        }

        [JSProperty(Name = "eventReceivers")]
        public SPEventReceiverDefinitionCollectionInstance EventReceivers
        {
            get
            {
                return m_web.EventReceivers == null
                    ? null
                    : new SPEventReceiverDefinitionCollectionInstance(this.Engine.Object.InstancePrototype, m_web.EventReceivers);
            }
        }

        [JSProperty(Name = "excludeFromOfflineClient")]
        public bool ExcludeFromOfflineClient
        {
            get
            {
                return m_web.ExcludeFromOfflineClient;
            }
            set
            {
                m_web.ExcludeFromOfflineClient = value;
            }
        }

        [JSProperty(Name = "executeUrl")]
        public string ExecuteUrl
        {
            get { return m_web.ExecuteUrl; }
        }

        [JSProperty(Name = "exists")]
        public bool Exists
        {
            get { return m_web.Exists; }
        }

        [JSProperty(Name = "features")]
        public SPFeatureCollectionInstance Features
        {
            get
            {

                return m_web.Features == null
                  ? null
                  : new SPFeatureCollectionInstance(this.Engine.Object.InstancePrototype, m_web.Features);
            }
        }

        [JSProperty(Name = "fields")]
        public SPFieldCollectionInstance Fields
        {
            get
            {
                return m_web.Fields == null
                  ? null
                  : new SPFieldCollectionInstance(this.Engine.Object.InstancePrototype, m_web.Fields);
            }
        }
        
        //FieldTypeDefinitionCollection

        [JSProperty(Name = "fileDialogPostProcessorId")]
        public GuidInstance FileDialogPostProcessorId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_web.FileDialogPostProcessorId);
            }
            set
            {
                m_web.FileDialogPostProcessorId = value == null
                    ? Guid.Empty
                    : value.Value;
            }
        }

        [JSProperty(Name = "files")]
        public SPFileCollectionInstance Files
        {
            get
            {
                return m_web.Files == null
                    ? null
                    : new SPFileCollectionInstance(this.Engine.Object.InstancePrototype, m_web.Files);
            }
        }

        //FirstUniqueAncestorSecurableObject
        //FirstUniqueAncestorWeb
        //FirstUniqueRoleDefinitionWeb

        [JSProperty(Name = "folders")]
        public SPFolderCollectionInstance Folders
        {
            get
            {
                return m_web.Folders == null
                    ? null
                    : new SPFolderCollectionInstance(this.Engine.Object.InstancePrototype, m_web.Folders);
            }
        }

        [JSProperty(Name = "groups")]
        public SPGroupCollectionInstance Groups
        {
            get
            {
                var result = m_web.Groups;
                return result == null
                  ? null
                  : new SPGroupCollectionInstance(this.Engine.Object.InstancePrototype, result);
            }
        }


        [JSProperty(Name = "hasUniqueRoleDefinitions")]
        public bool HasUniqueRoleDefinitions
        {
            get { return m_web.HasUniqueRoleDefinitions; }
        }

        //TODO: Effective base permissions

        [JSProperty(Name = "id")]
        public string Id
        {
            get { return m_web.ID.ToString(); }
        }

        [JSProperty(Name = "includeSupportingFolders")]
        public bool IncludeSupportingFolders
        {
            get
            {
                return m_web.IncludeSupportingFolders;
            }
            set
            {
                m_web.IncludeSupportingFolders = value;
            }
        }

        [JSProperty(Name = "isADAccountCreationMode")]
        public bool IsADAccountCreationMode
        {
            get
            {
                return m_web.IsADAccountCreationMode;
            }
        }

        [JSProperty(Name = "isADEmailEnabled")]
        public bool IsADEmailEnabled
        {
            get
            {
                return m_web.IsADEmailEnabled;
            }
        }

        [JSProperty(Name = "isMultilingual")]
        public bool IsMultilingual
        {
            get
            {
                return m_web.IsMultilingual;
            }
            set
            {
                m_web.IsMultilingual = value;
            }
        }

        [JSProperty(Name = "isRootWeb")]
        public bool IsRootWeb
        {
            get
            {
                return m_web.IsRootWeb;
            }
        }

        [JSProperty(Name = "language")]
        public string Language
        {
            get { return m_web.Language.ToString(CultureInfo.InvariantCulture); }
        }

        [JSProperty(Name = "lastItemModifiedDate")]
        public DateInstance LastItemModifiedDate
        {
            get { return JurassicHelper.ToDateInstance(this.Engine, m_web.LastItemModifiedDate); }
        }

        [JSProperty(Name = "lists")]
        public SPListCollectionInstance Lists
        {
            get
            {
                return m_web.Lists == null
                    ? null
                    : new SPListCollectionInstance(this.Engine.Object.InstancePrototype, m_web.Lists);
            }
        }

        [JSProperty(Name = "listTemplates")]
        public SPListTemplateCollectionInstance ListTemplates
        {
            get
            {
                return m_web.ListTemplates == null
                    ? null
                    : new SPListTemplateCollectionInstance(this.Engine.Object.InstancePrototype, m_web.ListTemplates);
            }
        }

        [JSProperty(Name = "masterPageReferenceEnabled")]
        public bool MasterPageReferenceEnabled
        {
            get
            {
                return m_web.MasterPageReferenceEnabled;
            }
        }

        [JSProperty(Name = "masterUrl")]
        public string MasterUrl
        {
            get
            {
                return m_web.MasterUrl;
            }
            set
            {
                m_web.MasterUrl = value;
            }
        }

        //Modules

        [JSProperty(Name = "name")]
        public string Name
        {
            get
            {
                return m_web.Name;
            }
            set
            {
                m_web.Name = value;
            }
        }

        [JSProperty(Name = "navigation")]
        public SPNavigationInstance Navigation
        {
            get
            {
                return m_web.Navigation == null
                    ? null
                    : new SPNavigationInstance(this.Engine.Object.InstancePrototype, m_web.Navigation);
            }
        }

        [JSProperty(Name = "noCrawl")]
        public bool NoCrawl
        {
            get
            {
                return m_web.NoCrawl;
            }
            set
            {
                m_web.NoCrawl = value;
            }
        }

        [JSProperty(Name = "overwriteTranslationsOnChange")]
        public bool OverwriteTranslationsOnChange
        {
            get
            {
                return m_web.OverwriteTranslationsOnChange;
            }
            set
            {
                m_web.OverwriteTranslationsOnChange = value;
            }
        }

        [JSProperty(Name = "parentWebId")]
        public GuidInstance ParentWebId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_web.ParentWebId);
            }
        }

        [JSProperty(Name = "parserEnabled")]
        public bool ParserEnabled
        {
            get
            {
                return m_web.ParserEnabled;
            }
            set
            {
                m_web.ParserEnabled = value;
            }
        }

        //Permissions

        [JSProperty(Name = "portalMember")]
        public bool PortalMember
        {
            get
            {
                return m_web.PortalMember;
            }
        }

        [JSProperty(Name = "portalName")]
        public string PortalName
        {
            get
            {
                return m_web.PortalName;
            }
        }

        [JSProperty(Name = "portalSubscriptionUrl")]
        public string PortalSubscriptionUrl
        {
            get
            {
                return m_web.PortalSubscriptionUrl;
            }
        }

        [JSProperty(Name = "portalUrl")]
        public string PortalUrl
        {
            get
            {
                return m_web.PortalUrl;
            }
        }

        [JSProperty(Name = "presenceEnabled")]
        public bool PresenceEnabled
        {
            get
            {
                return m_web.PresenceEnabled;
            }
            set
            {
                m_web.PresenceEnabled = value;
            }
        }

        [JSProperty(Name = "propertyBag")]
        public SPPropertyBagInstance PropertyBag
        {
            get
            {
                return m_web.Properties == null
                    ? null
                    : new SPPropertyBagInstance(this.Engine.Object.InstancePrototype, m_web.Properties);
            }
        }

        [JSProperty(Name = "provisioned")]
        public bool Provisioned
        {
            get
            {
                return m_web.Provisioned;
            }
        }

        [JSProperty(Name = "quickLaunchEnabled")]
        public bool QuickLaunchEnabled
        {
            get { return m_web.QuickLaunchEnabled; }
            set { m_web.QuickLaunchEnabled = value; }
        }

        [JSProperty(Name = "recycleBin")]
        public SPRecycleBinItemCollectionInstance RecycleBin
        {
            get
            {
                if (m_web.RecycleBinEnabled == false)
                    return null;

                return m_web.RecycleBin == null
                    ? null
                    : new SPRecycleBinItemCollectionInstance(this.Engine.Object.InstancePrototype, m_web.RecycleBin);
            }
        }

        [JSProperty(Name = "recycleBinEnabled")]
        public bool RecycleBinEnabled
        {
            get { return m_web.RecycleBinEnabled; }
        }

        //RegionalSettings

        [JSProperty(Name = "requestAccessEmail")]
        public object RequestAccessEmail
        {
            get
            {
                try
                {
                    return m_web.RequestAccessEmail;
                }
                catch (Exception)
                {
                    return Undefined.Value;
                }
            }
            set
            {
                m_web.RequestAccessEmail = TypeConverter.ToString(value);
            }
        }

        [JSProperty(Name = "requestAccessEnabled")]
        public bool RequestAccessEnabled
        {
            get { return m_web.RequestAccessEnabled; }
        }

        //ReusableAcl

        [JSProperty(Name = "roleDefinitions")]
        public SPRoleDefinitionCollectionInstance RoleDefinitions
        {
            get
            {
                return m_web.RoleDefinitions == null
                    ? null
                    : new SPRoleDefinitionCollectionInstance(this.Engine.Object.InstancePrototype, m_web.RoleDefinitions);
            }
        }

        //Roles property is Deprecated

        [JSProperty(Name = "rootFolder")]
        public SPFolderInstance RootFolder
        {
            get { return new SPFolderInstance(this.Engine.Object.InstancePrototype, null, null, m_web.RootFolder); }
        }

        [JSProperty(Name = "serverRelativeUrl")]
        public string ServerRelativeUrl
        {
            get { return m_web.ServerRelativeUrl; }
            set { m_web.ServerRelativeUrl = value; }
        }

        [JSProperty(Name = "showUrlStructureForCurrentUser")]
        public bool ShowUrlStructureForCurrentUser
        {
            get
            {
                return m_web.ShowUrlStructureForCurrentUser;
            }
        }

        [JSProperty(Name = "siteAdministrators")]
        public object SiteAdministrators
        {
            get
            {
                try
                {
                    return m_web.SiteAdministrators == null
                        ? null
                        : new SPUserCollectionInstance(this.Engine.Object.InstancePrototype, m_web.SiteAdministrators);
                }
                catch (Exception)
                {
                    return Undefined.Value;
                }
            }
        }

        [JSProperty(Name = "siteGroups")]
        public SPGroupCollectionInstance SiteGroups
        {
            get
            {
                var result = m_web.SiteGroups;
                return result == null
                  ? null
                  : new SPGroupCollectionInstance(this.Engine.Object.InstancePrototype, result);
            }
        }

        [JSProperty(Name = "siteLogoDescription")]
        public string SiteLogoDescription
        {
            get
            {
                return m_web.SiteLogoDescription;
            }
            set
            {
                m_web.SiteLogoDescription = value;
            }
        }

        [JSProperty(Name = "siteLogoUrl")]
        public string SiteLogoUrl
        {
            get
            {
                return m_web.SiteLogoUrl;
            }
            set
            {
                m_web.SiteLogoUrl = value;
            }
        }

        [JSProperty(Name = "siteUserInfoList")]
        public SPListInstance SiteUserInfoList
        {
            get
            {
                return m_web.SiteUserInfoList == null
                    ? null
                    : new SPListInstance(this.Engine, null, null, m_web.SiteUserInfoList);
            }
        }

        [JSProperty(Name = "siteUsers")]
        public SPUserCollectionInstance SiteUsers
        {
            get
            {
                return m_web.SiteUsers == null
                  ? null
                  : new SPUserCollectionInstance(this.Engine.Object.InstancePrototype, m_web.SiteUsers);
            }
        }

        //SupportedUICultures

        [JSProperty(Name = "syndicationEnabled")]
        public bool SyndicationEnabled
        {
            get { return m_web.SyndicationEnabled; }
            set { m_web.SyndicationEnabled = value; }
        }

        [JSProperty(Name = "theme")]
        public string Theme
        {
            get { return m_web.Theme; }
        }

        [JSProperty(Name = "themeCssUrl")]
        public string ThemeCssUrl
        {
            get { return m_web.ThemeCssUrl; }
        }

        [JSProperty(Name = "themeCssFolderUrl")]
        public string ThemeCssFolderUrl
        {
            get
            {
                return m_web.ThemedCssFolderUrl;
            }
            set
            {
                m_web.ThemedCssFolderUrl = value;
            }
        }

        [JSProperty(Name = "title")]
        public string Title
        {
            get { return m_web.Title; }
        }

        //TitleResource

        [JSProperty(Name = "treeViewEnabled")]
        public bool TreeViewEnabled
        {
            get { return m_web.TreeViewEnabled; }
            set { m_web.TreeViewEnabled = value; }
        }

        //UICulture

        [JSProperty(Name = "uiVersion")]
        public int UIVersion
        {
            get { return m_web.UIVersion; }
            set { m_web.UIVersion = value; }
        }

        [JSProperty(Name = "uiVersionConfigurationEnabled")]
        public bool UIVersionConfigurationEnabled
        {
            get { return m_web.UIVersionConfigurationEnabled; }
            set { m_web.UIVersionConfigurationEnabled = value; }
        }

        [JSProperty(Name = "url")]
        public string Url
        {
            get { return m_web.Url; }
        }

        [JSProperty(Name = "userCustomActions")]
        public SPUserCustomActionCollectionInstance UserCustomActions
        {
            get
            {
                return m_web.UserCustomActions == null
                    ? null
                    : new SPUserCustomActionCollectionInstance(this.Engine.Object.InstancePrototype, m_web.UserCustomActions);
            }
        }

        [JSProperty(Name = "userIsSiteAdmin")]
        public bool UserIsSiteAdmin
        {
            get { return m_web.UserIsSiteAdmin; }
        }

        [JSProperty(Name = "userIsWebAdmin")]
        public bool UserIsWebAdmin
        {
            get { return m_web.UserIsWebAdmin; }
        }

        //UserResources

        [JSProperty(Name = "users")]
        public SPUserCollectionInstance Users
        {
            get
            {
                return m_web.Users == null
                  ? null
                  : new SPUserCollectionInstance(this.Engine.Object.InstancePrototype, m_web.Users);
            }
        }

        //ViewStyles

        [JSProperty(Name = "webs")]
        public SPWebCollectionInstance Webs
        {
            get { return new SPWebCollectionInstance(this.Engine.Object.InstancePrototype, m_web.Webs); }
        }

        [JSProperty(Name = "webTemplate")]
        public string WebTemplate
        {
            get { return m_web.WebTemplate; }
        }

        [JSProperty(Name = "webTemplateId")]
        public int WebTemplateId
        {
            get { return m_web.WebTemplateId; }
        }

        [JSProperty(Name = "workflowAssociations")]
        public SPWorkflowAssociationCollectionInstance WorkflowAssociations
        {
            get
            {
                return m_web.WorkflowAssociations == null
                    ? null
                    : new SPWorkflowAssociationCollectionInstance(this.Engine.Object.InstancePrototype, m_web.WorkflowAssociations);
            }
        }

        [JSProperty(Name = "workflows")]
        public SPWorkflowCollectionInstance Workflows
        {
            get
            {
                return m_web.Workflows == null
                    ? null
                    : new SPWorkflowCollectionInstance(this.Engine.Object.InstancePrototype, m_web.Workflows);
            }
        }

        [JSProperty(Name = "workflowTemplates")]
        public SPWorkflowTemplateCollectionInstance WorkflowTemplates
        {
            get
            {
                return m_web.Workflows == null
                    ? null
                    : new SPWorkflowTemplateCollectionInstance(this.Engine.Object.InstancePrototype, m_web.WorkflowTemplates);
            }
        }
        #endregion

        #region Functions
        [JSFunction(Name = "addApplicationPrincipal")]
        public SPUserInstance AddApplicationPrincipal(string logonName, bool allowBrowseUerInfo, bool requireRequestToken)
        {
            var result = m_web.AddApplicationPrincipal(logonName, allowBrowseUerInfo, requireRequestToken);
            return result == null
                ? null
                : new SPUserInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "addFileByUrl")]
        public SPFileInstance AddFile(string url, object data, [DefaultParameterValue(true)] bool overwrite)
        {
            SPFile result;
            if (data is Base64EncodedByteArrayInstance)
            {
                var byteArrayInstance = data as Base64EncodedByteArrayInstance;
                result = m_web.Files.Add(url, byteArrayInstance.Data, overwrite);
            }
            else if (data is string)
            {
                result = m_web.Files.Add(url, Encoding.UTF8.GetBytes(data as string), overwrite);
            }
            else
                throw new JavaScriptException(this.Engine, "Error", "Unable to create SPFile: Unsupported data type: " + data.GetType());

            return new SPFileInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "addProperty")]
        public void AddProperty(object key, object value)
        {
            m_web.AddProperty(key, value);
        }

        //AddSupportedUICulture

        [JSFunction(Name = "allowAllWebTemplates")]
        public void AllowAllWebTemplates()
        {
            m_web.AllowAllWebTemplates();
        }

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

            var activatedFeature = m_web.Features.Add(featureId, forceValue);
            return new SPFeatureInstance(this.Engine.Object.InstancePrototype, activatedFeature);
        }

        [JSFunction(Name = "applyTheme")]
        public void ApplyTheme(string newTheme)
        {
            m_web.ApplyTheme(newTheme);
        }

        [JSFunction(Name = "applyWebTemplate")]
        public void ApplyWebTemplate(object webTemplate)
        {
            if (webTemplate is SPWebTemplateInstance)
                m_web.ApplyWebTemplate((webTemplate as SPWebTemplateInstance).WebTemplate);
            else
                m_web.ApplyWebTemplate(TypeConverter.ToString(webTemplate));
        }

        [JSFunction(Name = "close")]
        public void Close()
        {
            m_web.Close();
        }

        [JSFunction(Name = "createDefaultAssociatedGroups")]
        public void CreateDefaultAssociatedGroups(string userLogin, string userLogin2, string groupNameSeed)
        {
            m_web.CreateDefaultAssociatedGroups(userLogin, userLogin2, groupNameSeed);
        }

        [JSFunction(Name = "createList")]
        public SPListInstance CreateList(object listCreationInfo)
        {
            return SPListCollectionInstance.CreateList(this.Engine, m_web.Lists, m_web.ListTemplates, listCreationInfo);
        }

        [JSFunction(Name = "customizeCss")]
        public void CustomizeCss(string cssFile)
        {
            m_web.CustomizeCss(cssFile);
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

            m_web.Features.Remove(featureId);
        }

        [JSFunction(Name = "delete")]
        public void Delete()
        {
            m_web.Delete();
        }

        [JSFunction(Name = "deleteFileIfExists")]
        public bool DeleteFileIfExists(string serverRelativeUrl)
        {
            var file = m_web.GetFile(serverRelativeUrl);
            if (file != null && file.Exists)
            {
                m_web.AllowUnsafeUpdates = true;
                try
                {
                    file.Delete();
                }
                finally
                {
                    m_web.AllowUnsafeUpdates = false;
                }
            }
            return false;
        }

        [JSFunction(Name = "deleteFolderIfExists")]
        public bool DeleteFolderIfExists(string serverRelativeUrl)
        {
            var folder = m_web.GetFolder(serverRelativeUrl);
            if (folder == null || !folder.Exists)
                return false;

            m_web.AllowUnsafeUpdates = true;
            try
            {
                folder.Delete();
            }
            finally
            {
                m_web.AllowUnsafeUpdates = false;
            }
            return false;
        }

        [JSFunction(Name = "deleteProperty")]
        public void DeleteProperty(object key)
        {
            m_web.DeleteProperty(key);
        }

        [JSFunction(Name = "ensureUser")]
        public SPUserInstance EnsureUser(string logonName)
        {
            var user = m_web.EnsureUser(logonName);
            return new SPUserInstance(this.Engine.Object.InstancePrototype, user);
        }

        [JSDoc("Returns a value that indicates if a file exists at the specified url.")]
        [JSFunction(Name = "fileExists")]
        public bool FileExists(string fileUrl)
        {
            SPFile file;
            if (Uri.IsWellFormedUriString(fileUrl, UriKind.Relative))
                fileUrl = SPUtility.ConcatUrls(m_web.Url, fileUrl);
            return SPHelper.TryGetSPFile(fileUrl, out file);
        }

        //ExportUserResources

        [JSFunction(Name = "getAvailableContentTypes")]
        public ArrayInstance GetAvailableContentTypes()
        {
            var result = this.Engine.Array.Construct();
            foreach (var contentType in m_web.AvailableContentTypes.OfType<SPContentType>())
            {
                ArrayInstance.Push(result, new SPContentTypeInstance(this.Engine.Object.InstancePrototype, contentType));
            }
            return result;
        }

        //GetAvailableCrossLanguageWebTemplates
        //GetAvailableWebTemplates

        [JSFunction(Name = "getCatalog")]
        public SPListInstance GetCatalog(string typeCatalog)
        {
            SPListTemplateType type;
            if (!typeCatalog.TryParseEnum(true, out type))
                throw new JavaScriptException(this.Engine, "Error", "A valid SPListTemplateType must be specified.");

            var list = m_web.GetCatalog(type);
            return list == null
                ? null
                : new SPListInstance(this.Engine, null, null, list);
        }

        [JSFunction(Name = "getContentTypes")]
        public ArrayInstance GetContentTypes()
        {
            var result = this.Engine.Array.Construct();
            foreach (var contentType in m_web.ContentTypes.OfType<SPContentType>())
            {
                ArrayInstance.Push(result, new SPContentTypeInstance(this.Engine.Object.InstancePrototype, contentType));
            }
            return result;
        }


        [JSFunction(Name = "getDocTemplates")]
        public ArrayInstance GetDocTemplates()
        {
            var result = this.Engine.Array.Construct();
            foreach (var docTemplate in m_web.DocTemplates.OfType<SPDocTemplate>())
            {
                ArrayInstance.Push(result, new SPDocTemplateInstance(this.Engine.Object.InstancePrototype, docTemplate));
            }

            return result;
        }
        
        //GetChanges
        //GetDocDiscussions
        //GetFieldLocalizations

        [JSFunction(Name = "getFileById")]
        public SPFileInstance GetFileById(object id)
        {
            var guidId = GuidInstance.ConvertFromJsObjectToGuid(id);
            var file = m_web.GetFile(guidId);
            return file == null
              ? null
              : new SPFileInstance(this.Engine.Object.InstancePrototype, file);
        }

        [JSFunction(Name = "getFileByServerRelativeUrl")]
        public SPFileInstance GetFileByServerRelativeUrl(string serverRelativeUrl)
        {
            var file = m_web.GetFile(serverRelativeUrl);
            return file == null
              ? null
              : new SPFileInstance(this.Engine.Object.InstancePrototype, file);
        }

        [JSFunction(Name = "getFiles")]
        public ArrayInstance GetFiles()
        {
            var result = this.Engine.Array.Construct();

            var files = m_web.Files;
            foreach (var file in files.OfType<SPFile>())
            {
                ArrayInstance.Push(result, new SPFileInstance(this.Engine.Object.InstancePrototype, file));
            }

            return result;
        }

        [JSFunction(Name = "getFileAsString")]
        public string GetFileAsString(string url)
        {
            return m_web.GetFileAsString(url);
        }

        [JSFunction(Name = "getFileOrFolderObject")]
        public object GetFileOrFolderObject(string strUrl)
        {
            var obj = m_web.GetFileOrFolderObject(strUrl);
            if (obj == null)
                return null;

            if (obj is SPFile)
                return new SPFileInstance(this.Engine.Object.InstancePrototype, obj as SPFile);

            if (obj is SPFolder)
                return new SPFolderInstance(this.Engine.Object.InstancePrototype, null, null, obj as SPFolder);

            return null;
        }
        
        //GetFilePersonalizationInformation

        [JSFunction(Name = "getFolderByServerRelativeUrl")]
        public SPFolderInstance GetFolderFromServerRelativeUrl(string serverRelativeUrl)
        {
            var folder = m_web.GetFolder(serverRelativeUrl);

            if (folder == null || folder.Exists == false)
                return null;

            return new SPFolderInstance(this.Engine.Object.InstancePrototype, null, null, folder);
        }

        [JSFunction(Name = "getFolders")]
        public ArrayInstance GetFolders()
        {
            var result = this.Engine.Array.Construct();
            foreach (var folder in m_web.Folders.OfType<SPFolder>())
            {
                ArrayInstance.Push(result, new SPFolderInstance(this.Engine.Object.InstancePrototype, null, null, folder));
            }

            return result;
        }

        [JSFunction(Name = "getLimitedWebPartManager")]
        public SPLimitedWebPartManagerInstance GetLimitedWebPartManager(string fullOrRelativeUrl, string personalizationScope)
        {
            PersonalizationScope scope;
            if (!personalizationScope.TryParseEnum(true, out scope))
                throw new JavaScriptException(this.Engine, "Error", "A valid personalization scope must be supplied.");

            var result = m_web.GetLimitedWebPartManager(fullOrRelativeUrl, scope);
            return result == null
                ? null
                : new SPLimitedWebPartManagerInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getListByServerRelativeUrl")]
        public object GetListFromServerRelativeUrl(string serverRelativeUrl)
        {
            SPList list = null;
            try
            {
                if (string.IsNullOrEmpty(serverRelativeUrl) || SPUrlUtility.IsUrlRelative(serverRelativeUrl))
                {
                    serverRelativeUrl = SPUtility.GetFullUrl(SPBaristaContext.Current.Site, serverRelativeUrl);
                }

                list = m_web.GetList(serverRelativeUrl);
            }
            catch (FileNotFoundException)
            {
                /* Do Nothing... */
            }

            if (list == null)
                return Null.Value;

            return new SPListInstance(this.Engine, null, null, list);
        }

        [JSFunction(Name = "getLists")]
        public ArrayInstance GetLists()
        {
            var lists = new List<SPList>();
            var instance = this.Engine.Array.Construct();

            var listsIterator = new ContentIterator();
            listsIterator.ProcessLists(m_web.Lists, lists.Add, null);

            foreach (var list in lists)
            {
                ArrayInstance.Push(instance, new SPListInstance(this.Engine, null, null, list));
            }

            return instance;
        }

        [JSFunction(Name = "getListTemplates")]
        public ArrayInstance GetListTemplates()
        {
            var result = this.Engine.Array.Construct();
            foreach (var listTemplate in m_web.ListTemplates.OfType<SPListTemplate>())
            {
                ArrayInstance.Push(result, new SPListTemplateInstance(this.Engine.Object.InstancePrototype, listTemplate));
            }

            return result;
        }

        [JSFunction(Name = "getList")]
        public SPListInstance GetList(string strUrl)
        {
            var list = m_web.GetList(strUrl);
            return list == null
                ? null
                : new SPListInstance(this.Engine, null, null, list);
        }

        [JSFunction(Name = "getListByTitle")]
        public SPListInstance GetListByTitle(string listTitle)
        {
            var list = m_web.Lists.TryGetList(listTitle);
            return list == null
                ? null
                : new SPListInstance(this.Engine, null, null, list);
        }

        [JSFunction(Name = "getListFromUrl")]
        public SPListInstance GetListFromUrl(string strUrl)
        {
            var list = m_web.GetListFromUrl(strUrl);
            return list == null
                ? null
                : new SPListInstance(this.Engine, null, null, list);
        }

        [JSFunction(Name = "getListFromWebPartPageUrl")]
        public SPListInstance GetListFromWebPartPageUrl(string pageUrl)
        {
            var list = m_web.GetListFromWebPartPageUrl(pageUrl);
            return list == null
                ? null
                : new SPListInstance(this.Engine, null, null, list);
        }

        [JSFunction(Name = "getListItem")]
        public SPListItemInstance GetListItem(string strUrl)
        {
            var result = m_web.GetListItem(strUrl);
            return result == null
                ? null
                : new SPListItemInstance(this.Engine, result);
        }

        [JSFunction(Name = "getListItemFields")]
        public SPListItemInstance GetListItemFields(string strUrl, ArrayInstance fields)
        {
            var strFields = new string[0];
            if (fields != null)
                strFields = fields.ElementValues.Select(TypeConverter.ToString).ToArray();

            var result = m_web.GetListItemFields(strUrl, strFields);
            return result == null
                ? null
                : new SPListItemInstance(this.Engine, result);
        }

        [JSFunction(Name = "getListsOfType")]
        public SPListCollectionInstance GetListsOfType(string baseType)
        {
            SPBaseType type;
            if (!baseType.TryParseEnum(true, out type))
                throw new JavaScriptException(this.Engine, "Error", "A valid SPBaseType must be specified.");

            var result = m_web.GetListsOfType(type);
            return result == null
                ? null
                : new SPListCollectionInstance(this.Engine.Object.InstancePrototype, result);
        }

        //GetObject??

        [JSFunction(Name = "getProperty")]
        public object GetProperty(object key)
        {
            return m_web.GetProperty(key);
        }

        //GetRecycleBinItems

        [JSFunction(Name = "getSiteData")]
        public object GetSiteData(SPSiteDataQueryInstance query)
        {
            var result = m_web.GetSiteData(query.SiteDataQuery);
            var jsonResult = JsonConvert.SerializeObject(result);
            return JSONObject.Parse(this.Engine, jsonResult, null);
        }

        [JSFunction(Name = "getSiteUserInfoList")]
        public SPListInstance GetSiteUserInfoList()
        {
            return new SPListInstance(this.Engine, null, null, m_web.SiteUserInfoList);
        }

        [JSFunction(Name = "getSubwebsForCurrentUser")]
        public SPWebCollectionInstance GetSubwebsForCurrentUser(object webTemplateFilter, object configurationFilter)
        {
            SPWebCollection result = null;
            if (webTemplateFilter == Undefined.Value && configurationFilter == Undefined.Value)
                result = m_web.GetSubwebsForCurrentUser();
            else if (webTemplateFilter != Undefined.Value && configurationFilter == Undefined.Value)
                result = m_web.GetSubwebsForCurrentUser(TypeConverter.ToInteger(webTemplateFilter));
            else if (webTemplateFilter != Undefined.Value && configurationFilter != Undefined.Value)
                result = m_web.GetSubwebsForCurrentUser(TypeConverter.ToInteger(webTemplateFilter), (short)TypeConverter.ToInteger(configurationFilter));

            return result == null
                ? null
                : new SPWebCollectionInstance(this.Engine.Object.InstancePrototype, result);
        }

        //GetUsageData

        [JSFunction(Name = "getUser")]
        public object GetUser(string loginName)
        {
            var user = m_web.Users[loginName];
            if (user != null)
                return new SPUserInstance(this.Engine.Object.InstancePrototype, user);
            return Null.Value;
        }
        
        //GetUserEffectivePermissionsInfo

        [JSFunction(Name = "getUserEffectivePermissions")]
        public string GetUserEffectivePermissions(string userName)
        {
            return m_web.GetUserEffectivePermissions(userName).ToString();
        }

        [JSFunction(Name = "getUserToken")]
        public SPUserTokenInstance GetUserToken(string userName)
        {
            var result = m_web.GetUserToken(userName);
            return result == null
                    ? null
                    : new SPUserTokenInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getViewFromUrl")]
        public SPViewInstance GetViewFromUrl(string listUrl)
        {
            var result = m_web.GetViewFromUrl(listUrl);
            return result == null
                ? null
                : new SPViewInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getWebs")]
        public ArrayInstance GetWebs()
        {
            var result = this.Engine.Array.Construct();
            foreach (var web in m_web.Webs)
            {
                ArrayInstance.Push(result, new SPWebInstance(this.Engine, (SPWeb)web));
            }

            return result;
        }

        [JSFunction(Name = "mapToIcon")]
        public string MapToIcon(string fileName, string progId, string iconSize)
        {
            if (String.IsNullOrEmpty(iconSize))
                return SPUtility.MapToIcon(m_web, fileName, progId);

            return SPUtility.MapToIcon(m_web, fileName, progId, (IconSize)Enum.Parse(typeof(IconSize), iconSize));
        }

        [JSFunction(Name = "getWebsAndListsWithUniquePermissions")]
        public ArrayInstance GetWebsAndListsWithUniquePermissions()
        {
            var result = this.Engine.Array.Construct();
            foreach(var x in m_web.GetWebsAndListsWithUniquePermissions())
            {
                ArrayInstance.Push(result, new SPWebListInfoInstance(this.Engine.Object.InstancePrototype, x));
            }

            return result;
        }

        //ImportUserResources
        //InsertAlertEvent

        [JSFunction(Name = "isCurrentUserMemberOfGroup")]
        public bool IsCurrentUserMemberOfGroup(int groupId)
        {
            return m_web.IsCurrentUserMemberOfGroup(groupId);
        }

        [JSDoc("Loads the file at the specified url as a byte array.")]
        [JSFunction(Name = "loadFileAsByteArray")]
        public Base64EncodedByteArrayInstance LoadFileAsByteArray(string fileUrl)
        {
            SPFile file;
            if (Uri.IsWellFormedUriString(fileUrl, UriKind.Relative))
                fileUrl = SPUtility.ConcatUrls(m_web.Url, fileUrl);
            if (!SPHelper.TryGetSPFile(fileUrl, out file))
                throw new JavaScriptException(this.Engine, "Error", "Could not locate the specified file:  " + fileUrl);

            var data = file.OpenBinary(SPOpenBinaryOptions.None);
            var result = new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, data)
            {
                FileName = file.SourceLeafName.IsNullOrWhiteSpace() ? file.Name : file.SourceLeafName
            };
            return result;
        }

        [JSDoc("Loads the file at the specified url as a string.")]
        [JSFunction(Name = "loadFileAsString")]
        public string LoadFileAsString(string fileUrl)
        {
            string path;
            bool isHiveFile;
            string fileContents;
            if (Uri.IsWellFormedUriString(fileUrl, UriKind.Relative))
                fileUrl = SPUtility.ConcatUrls(m_web.Url, fileUrl);
            if (SPHelper.TryGetSPFileAsString(fileUrl, out path, out fileContents, out isHiveFile))
                return fileContents;

            throw new JavaScriptException(this.Engine, "Error", "Could not locate the specified file:  " + fileUrl);
        }

        [JSDoc("Loads the file at the specified url as a JSON Object.")]
        [JSFunction(Name = "loadFileAsJSON")]
        public object LoadFileAsJson(string fileUrl)
        {
            string path;
            bool isHiveFile;
            string fileContents;
            if (Uri.IsWellFormedUriString(fileUrl, UriKind.Relative))
                fileUrl = SPUtility.ConcatUrls(m_web.Url, fileUrl);
            if (SPHelper.TryGetSPFileAsString(fileUrl, out path, out fileContents, out isHiveFile))
            {
                return JSONObject.Parse(this.Engine, fileContents, null);
            }

            throw new JavaScriptException(this.Engine, "Error", "Could not locate the specified file:  " + fileUrl);
        }

        [JSFunction(Name = "processBatchData")]
        public string ProcessBatchData(string strBatchData)
        {
            return m_web.ProcessBatchData(strBatchData);
        }

        [JSFunction(Name = "recalculateWebFineGrainedPermissions")]
        public void RecalculateWebFineGrainedPermissions()
        {
            m_web.RecalculateWebFineGrainedPermissions();
        }

        [JSFunction(Name = "recycle")]
        public void Recycle()
        {
            m_web.Recycle();
        }
        
        //RemoveSupporedUICulture

        [JSFunction(Name = "revertAllDocumentContentStreams")]
        public void RevertAllDocumentContentStreams()
        {
            m_web.RevertAllDocumentContentStreams();
        }

        [JSFunction(Name = "revertCss")]
        public void RevertCss(string cssFile)
        {
            m_web.RevertCss(cssFile);
        }

        [JSFunction(Name = "saveAsTemplate")]
        public void SaveAsTemplate(string strTemplateName, string strTemplateTitle, string strTemplateDescription, bool bSaveData)
        {
            m_web.SaveAsTemplate(strTemplateName, strTemplateTitle, strTemplateDescription, bSaveData);
        }

        //SearchDocuments
        //SearchListItems
        //SetAvailableCrossLanguageWebTemplates
        //SetAvailableWebTemplates

        [JSFunction(Name = "setProperty")]
        public void SetProperty(object key, object value)
        {
            m_web.SetProperty(key, value);
        }

        [JSFunction(Name = "update")]
        public void Update()
        {
            m_web.Update();
        }

        [JSFunction(Name = "validateFormDigest")]
        public bool ValidateFormDigest()
        {
            return m_web.ValidateFormDigest();
        }

        [JSDoc("Writes the specified contents to the file located at the specified url")]
        [JSFunction(Name = "write")]
        public SPFileInstance Write(string fileUrl, object contents)
        {
            byte[] data;
            if (contents is Base64EncodedByteArrayInstance)
                data = (contents as Base64EncodedByteArrayInstance).Data;
            else if (contents is StringInstance || contents is string)
                data = Encoding.UTF8.GetBytes((string)contents);
            else if (contents is ObjectInstance)
                data = Encoding.UTF8.GetBytes(JSONObject.Stringify(this.Engine, contents, null, null));
            else
                data = Encoding.UTF8.GetBytes(contents.ToString());

            if (Uri.IsWellFormedUriString(fileUrl, UriKind.Relative))
                fileUrl = SPUtility.ConcatUrls(m_web.Url, fileUrl);

            SPFile result;
            if (SPHelper.TryGetSPFile(fileUrl, out result))
            {
                SPWeb web;
                if (SPHelper.TryGetSPWeb(fileUrl, out web))
                {
                    result = web.Files.Add(fileUrl, data);
                }
                else
                {
                    throw new JavaScriptException(this.Engine, "Error", "Could not locate the specified web:  " + fileUrl);
                }
            }
            else
            {
                result.SaveBinary(data);
            }

            return new SPFileInstance(this.Engine.Object.InstancePrototype, result);
        }
        #endregion

        [JSFunction(Name = "dispose")]
        public void Dispose()
        {
            if (m_web != null)
                m_web.Dispose();
        }
    }
}
