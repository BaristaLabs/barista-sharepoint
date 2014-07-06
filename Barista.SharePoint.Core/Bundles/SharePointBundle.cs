
namespace Barista.SharePoint.Bundles
{
    using System.Linq;
    using System.Reflection;
    using Barista.Library;
    using Barista.SharePoint.Library;
    using Microsoft.SharePoint;
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

            //Lookups
            var spBuiltInFieldId = engine.Object.Construct();
            var type = typeof(SPBuiltInFieldId);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var field in fields.OrderBy(f => f.Name))
                spBuiltInFieldId.SetPropertyValue(field.Name, new GuidInstance(engine.Object.InstancePrototype, (Guid)field.GetValue(null)), false);

            engine.SetGlobalValue("SPBuiltInFieldId", spBuiltInFieldId);

            var spBuiltInContentTypeId = engine.Object.Construct();
            type = typeof(SPBuiltInContentTypeId);
            fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var field in fields.OrderBy(f => f.Name))
                spBuiltInContentTypeId.SetPropertyValue(field.Name, new SPContentTypeIdInstance(engine.Object.InstancePrototype, (SPContentTypeId)field.GetValue(null)), false);

            engine.SetGlobalValue("SPBuiltInContentTypeId", spBuiltInContentTypeId);

            //Data Related
            engine.SetGlobalValue("SPResource", new SPResourceConstructor(engine));
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

            //Fields
            engine.SetGlobalValue("SPField", new SPFieldConstructor(engine));
            engine.SetGlobalValue("SPFieldLink", new SPFieldLinkConstructor(engine));

            //FieldValues
            engine.SetGlobalValue("SPFieldUrlValue", new SPFieldUrlValueConstructor(engine));
            engine.SetGlobalValue("SPFieldLookupValue", new SPFieldLookupValueConstructor(engine));
            engine.SetGlobalValue("SPFieldLookupValueCollection", new SPFieldLookupValueCollectionConstructor(engine));
            engine.SetGlobalValue("SPFieldUserValue", new SPFieldUserValueConstructor(engine));
            engine.SetGlobalValue("SPFieldUserValueCollection", new SPFieldUserValueCollectionConstructor(engine));
            engine.SetGlobalValue("SPFieldMultiChoiceValueCollection", new SPFieldMultiChoiceValueConstructor(engine));
            engine.SetGlobalValue("SPFieldMultiColumnValueCollection", new SPFieldMultiColumnValueConstructor(engine));

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
