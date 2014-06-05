
namespace Barista.SharePoint.Bundles
{
    using Barista.SharePoint.Library;
    using Microsoft.SharePoint.Administration;
    using System;

    [Serializable]
    public class SharePointBundle : IBundle
    {
        public bool IsSystemBundle
        {
            get { return true; }
        }

        public string BundleName
        {
            get { return "SharePoint"; }
        }

        public string BundleDescription
        {
            get { return "SharePoint Bundle. Provides top-level objects to interact with SharePoint."; }
        }

        public object InstallBundle(Jurassic.ScriptEngine engine)
        {
            //Template Related
            engine.SetGlobalValue("SPWebTemplate", new SPWebTemplateConstructor(engine));
            engine.SetGlobalValue("SPDocTemplate", new SPDocTemplateConstructor(engine));
            engine.SetGlobalValue("SPFeatureDefinition", new SPFeatureDefinitionConstructor(engine));
            engine.SetGlobalValue("SPFeature", new SPFeatureConstructor(engine));

            //Data Related
            engine.SetGlobalValue("SPWebApplication", new SPWebApplicationConstructor(engine));
            engine.SetGlobalValue("SPSite", new SPSiteConstructor(engine));
            engine.SetGlobalValue("SPWeb", new SPWebConstructor(engine));
            engine.SetGlobalValue("SPFile", new SPFileConstructor(engine));
            engine.SetGlobalValue("SPDocumentSet", new SPDocumentSetConstructor(engine));
            engine.SetGlobalValue("SPFolder", new SPFolderConstructor(engine));
            engine.SetGlobalValue("SPList", new SPListConstructor(engine));
            engine.SetGlobalValue("SPDocumentLibrary", new SPDocumentLibraryConstructor(engine));
            engine.SetGlobalValue("SPView", new SPViewConstructor(engine));
            engine.SetGlobalValue("SPListItem", new SPListItemConstructor(engine));
            engine.SetGlobalValue("SPCamlQuery", new SPCamlQueryConstructor(engine));
            engine.SetGlobalValue("SPCamlQueryBuilder", new SPCamlQueryBuilderConstructor(engine));
            engine.SetGlobalValue("SPAuditQuery", new SPAuditQueryConstructor(engine));
            engine.SetGlobalValue("SPSiteDataQuery", new SPSiteDataQueryConstructor(engine));
            engine.SetGlobalValue("SPContentType", new SPContentTypeConstructor(engine));
            engine.SetGlobalValue("SPContentTypeId", new SPContentTypeIdConstructor(engine));
            engine.SetGlobalValue("SPField", new SPFieldConstructor(engine));
            engine.SetGlobalValue("SPFieldLink", new SPFieldLinkConstructor(engine));

            //Navigation Related
            engine.SetGlobalValue("SPNavigationNode", new SPNavigationNodeConstructor(engine));

            //Security Related
            engine.SetGlobalValue("SPRoleDefinition", new SPRoleDefinitionConstructor(engine));
            engine.SetGlobalValue("SPRoleAssignment", new SPRoleAssignmentConstructor(engine));
            engine.SetGlobalValue("SPUser", new SPUserConstructor(engine));
            engine.SetGlobalValue("SPGroup", new SPGroupConstructor(engine));
            engine.SetGlobalValue("SPUserToken", new SPUserTokenConstructor(engine));

            //Web parts.
            engine.SetGlobalValue("SPWebPartConnection", new SPWebPartConnectionConstructor(engine));

            return new SPInstance(engine, SPBaristaContext.Current, SPFarm.Local, SPServer.Local);
        }
    }
}
