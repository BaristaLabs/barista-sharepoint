namespace Barista.SharePoint.Library
{
    using System;
    using System.Collections;
    using Barista.Extensions;
    using Barista.Library;
    using Jurassic;
    using Jurassic.Library;
    using Microsoft.SharePoint;
    using Microsoft.Office.DocumentManagement.DocumentSets;

    [Serializable]
    public class SPDocumentSetConstructor : ClrFunction
    {
        public SPDocumentSetConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPDocumentSet", new SPDocumentSetInstance(engine.Object.InstancePrototype))
        {
            this.PopulateFunctions();
        }

        [JSConstructorFunction]
        public SPDocumentSetInstance Construct(object obj)
        {
            SPSite site = null;
            SPWeb web = null;
            DocumentSet result;

            if (obj is SPFolderInstance)
            {
                var folderInstance = obj as SPFolderInstance;
                result = DocumentSet.GetDocumentSet(folderInstance.Folder);
            }
            else if (obj is string)
            {
                var url = obj as string;
                SPFolder folder;
                if (SPHelper.TryGetSPFolder(url, out site, out web, out folder) == false)
                    throw new JavaScriptException(this.Engine, "Error", "No documentSet is available at the specified url.");
                result = DocumentSet.GetDocumentSet(folder);
            }
            else
                throw new JavaScriptException(this.Engine, "Error", "Cannot create a document set with the specified object: " + TypeConverter.ToString(obj));

            return new SPDocumentSetInstance(this.InstancePrototype, site, web, result);
        }

        [JSFunction(Name = "createDocumentSet")]
        [JSDoc("Creates a new document set in the specified folder with the specified name and content type id.")]
        public SPDocumentSetInstance CreateDocumentSet(
            [JSDoc("The folder to create the document set in.")]
            SPFolderInstance folder,
            [JSDoc("The name of the new document set.")]
            string name,
            [JSDoc("The content type id of the new documentset. Use new ContentTypeId('<CTID>')")]
            SPContentTypeIdInstance ctid,
            [JSDoc("Optional. Specifies a hashtable of fields that will be set on the document set where the key is the static field name.")]
            object properties,
            [JSDoc("Optional. Specifies a value that indicates if the default document set content will be provisioned. Default is true.")]
            object provisionDefaultContent,
            [JSDoc("Optional. Specifies the SPUser that created the document set.")]
            object user)
        {
            if (folder == null)
                throw new JavaScriptException(this.Engine, "Error", "An instance of an SPFolder must be specified as the first argument.");
            
            if (name.IsNullOrWhiteSpace())
                throw new JavaScriptException(this.Engine, "Error", "The name of the new document set must be specified.");

            if (ctid == null)
                throw new JavaScriptException(this.Engine, "Error", "The Content Type Id of the new document set must be specified.");

            var htProperties = new Hashtable();
            if (properties != null && properties != Null.Value && properties != Undefined.Value)
                htProperties = SPHelper.GetFieldValuesHashtableFromPropertyObject(properties);

            var bProvisionDefaultContent = true;
            if (provisionDefaultContent != null && provisionDefaultContent != Null.Value &&
                provisionDefaultContent != Undefined.Value)
                bProvisionDefaultContent = TypeConverter.ToBoolean(provisionDefaultContent);

            DocumentSet result;
            var spUser = user as SPUserInstance;

            if (user == null || user == Null.Value || user == Undefined.Value || spUser == null)
                result = DocumentSet.Create(folder.Folder, name, ctid.ContentTypeId, htProperties, bProvisionDefaultContent);
            else
            {
                result = DocumentSet.Create(folder.Folder, name, ctid.ContentTypeId, htProperties, bProvisionDefaultContent, spUser.User);
            }

            if (result == null)
                return null;

            return new SPDocumentSetInstance(this.Engine.Object.InstancePrototype, folder.Folder.ParentWeb.Site,
                folder.Folder.ParentWeb, result);
        }

        [JSFunction(Name = "getDocumentSet")]
        public SPDocumentSetInstance GetDocumentSet(SPFolderInstance folder)
        {
            if (folder == null)
                throw new JavaScriptException(this.Engine, "Error", "An instance of an SPFolder must be specified as the first argument.");

            var result = DocumentSet.GetDocumentSet(folder.Folder);
            
            if (result == null)
                return null;

            return new SPDocumentSetInstance(this.Engine.Object.InstancePrototype, folder.Folder.ParentWeb.Site,
                folder.Folder.ParentWeb, result);
        }

        [JSFunction(Name = "import")]
        public SPDocumentSetInstance Import(Base64EncodedByteArrayInstance bytes, string name, SPFolderInstance parentFolder, object ctid, object properties, object user)
        {
            if (bytes == null)
                throw new JavaScriptException(this.Engine, "Error", "A base64 encoded byte array must be supplied as the first argument.");

            if (name.IsNullOrWhiteSpace())
                throw new JavaScriptException(this.Engine, "Error", "The name of the new document set must be specified.");

            if (parentFolder == null)
                throw new JavaScriptException(this.Engine, "Error", "The parent folder must be specified.");

            if (ctid == Undefined.Value && properties == Undefined.Value && user == Undefined.Value)
            {
                var r = DocumentSet.Import(bytes.Data, name, parentFolder.Folder);
                if (r == null)
                    return null;

                return new SPDocumentSetInstance(this.Engine.Object.InstancePrototype, parentFolder.Folder.ParentWeb.Site,
                    parentFolder.Folder.ParentWeb, r);
            }



            var spCtId = ctid as SPContentTypeIdInstance;
            if (ctid == null || ctid == Null.Value || ctid == Undefined.Value || spCtId == null)
                throw new JavaScriptException(this.Engine, "Error", "The Content Type Id of the imported document set must be specified.");

            var spUser = user as SPUserInstance;
            if (user == null || user == Null.Value || user == Undefined.Value || spUser == null)
                throw new JavaScriptException(this.Engine, "Error", "The user of the imported document set must be specified.");

            var htProperties = new Hashtable();
            if (properties != null && properties != Null.Value && properties != Undefined.Value)
                htProperties = SPHelper.GetFieldValuesHashtableFromPropertyObject(properties);

            var result = DocumentSet.Import(bytes.Data, name, parentFolder.Folder, spCtId.ContentTypeId, htProperties, spUser.User);

            if (result == null)
                return null;

            return new SPDocumentSetInstance(this.Engine.Object.InstancePrototype, parentFolder.Folder.ParentWeb.Site,
                parentFolder.Folder.ParentWeb, result);
        }

        public SPDocumentSetInstance Construct(DocumentSet documentSet)
        {
            if (documentSet == null)
                throw new ArgumentNullException("documentSet");

            return new SPDocumentSetInstance(this.InstancePrototype, null, null, documentSet);
        }
    }

    [Serializable]
    public class SPDocumentSetInstance : ObjectInstance, IDisposable
    {
        private SPSite m_site;
        private SPWeb m_web;
        private readonly DocumentSet m_documentSet;

        public SPDocumentSetInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPDocumentSetInstance(ObjectInstance prototype, SPSite site, SPWeb web, DocumentSet documentSet)
            : this(prototype)
        {
            this.m_site = site;
            this.m_web = web;
            this.m_documentSet = documentSet;
        }

        #region Properties
        [JSProperty(Name = "item")]
        public SPListItemInstance Item
        {
            get { return new SPListItemInstance(this.Engine, m_documentSet.Item); }
        }

        [JSProperty(Name = "welcomePageUrl")]
        public string WelcomePageUrl
        {
            get { return m_documentSet.WelcomePageUrl; }
        }

        #endregion

        [JSFunction(Name = "export")]
        public Base64EncodedByteArrayInstance Export()
        {
            var result = m_documentSet.Export();
            return new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getContentType")]
        public SPContentTypeInstance GetContentType()
        {
            return new SPContentTypeInstance(this.Engine.Object.InstancePrototype, m_documentSet.ContentType);
        }

        //TODO: getContentTypeTemplate

        [JSFunction(Name = "getFolder")]
        public SPFolderInstance GetFolder()
        {
            return new SPFolderInstance(this.Engine.Object.InstancePrototype, null, null, m_documentSet.Folder);
        }

        [JSFunction(Name = "getParentFolder")]
        public SPFolderInstance GetParentFolder()
        {
            return new SPFolderInstance(this.Engine.Object.InstancePrototype, null, null, m_documentSet.ParentFolder);
        }

        [JSFunction(Name = "getParentList")]
        public SPListInstance GetParentList()
        {
            return new SPListInstance(this.Engine, null, null, m_documentSet.ParentList);
        }

        public void Dispose()
        {
            if (m_site != null)
            {
                m_site.Dispose();
                m_site = null;
            }

            if (m_web != null)
            {
                m_web.Dispose();
                m_web = null;
            }
        }
    }
}
