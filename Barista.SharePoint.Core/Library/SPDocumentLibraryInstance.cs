namespace Barista.SharePoint.Library
{
    using System.Reflection;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint;

    [Serializable]
    public class SPDocumentLibraryConstructor : ClrFunction
    {
        public SPDocumentLibraryConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPDocumentLibrary", new SPDocumentLibraryInstance(engine, null, null, null))
        {
        }

        [JSConstructorFunction]
        public SPDocumentLibraryInstance Construct(string documentLibraryUrl)
        {
            SPSite site;
            SPWeb web;
            SPList list;
            if (!SPHelper.TryGetSPList(documentLibraryUrl, out site, out web, out list))
                throw new JavaScriptException(this.Engine, "Error", "A list at the specified url was not found.");

            var docLib = list as SPDocumentLibrary;

            if (docLib == null)
                throw new JavaScriptException(this.Engine, "Error", "A list at the specified url was found, but the list is not a document library.");

            var result = new SPDocumentLibraryInstance(this.Engine, site, web, docLib);
            return result;
        }
    }

    [Serializable]
    public class SPDocumentLibraryInstance : SPListInstance
    {
        private readonly SPDocumentLibrary m_documentLibrary;

        public SPDocumentLibraryInstance(ScriptEngine engine, SPSite site, SPWeb web, SPDocumentLibrary documentLibrary)
            : base(new SPListInstance(engine, site, web, documentLibrary), documentLibrary)
        {
            m_documentLibrary = documentLibrary;

            this.PopulateFunctions(this.GetType(), BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        }

        public SPDocumentLibrary DocumentLibrary
        {
            get { return m_documentLibrary; }
        }

        [JSProperty(Name = "documentTemplateUrl")]
        public string DocumentTemplateUrl
        {
            get
            {
                return m_documentLibrary.DocumentTemplateUrl;
            }
            set
            {
                m_documentLibrary.DocumentTemplateUrl = value;
            }
        }

        [JSProperty(Name = "isCatalog")]
        public bool IsCatalog
        {
            get
            {
                return m_documentLibrary.IsCatalog;
            }
        }

        [JSProperty(Name = "serverRelativeDocumentTemplateUrl")]
        public string ServerRelativeDocumentTemplateUrl
        {
            get
            {
                return m_documentLibrary.ServerRelativeDocumentTemplateUrl;
            }
        }

        [JSProperty(Name = "thumbnailsEnabled")]
        public bool ThumbnailsEnabled
        {
            get
            {
                return m_documentLibrary.ThumbnailsEnabled;
            }
            set
            {
                m_documentLibrary.ThumbnailsEnabled = value;
            }
        }

        [JSProperty(Name = "thumbnailSize")]
        public int ThumbnailSize
        {
            get
            {
                return m_documentLibrary.ThumbnailSize;
            }
            set
            {
                m_documentLibrary.ThumbnailSize = value;
            }
        }

        [JSProperty(Name = "webImageHeight")]
        public int WebImageHeight
        {
            get
            {
                return m_documentLibrary.WebImageHeight;
            }
            set
            {
                m_documentLibrary.WebImageHeight = value;
            }
        }

        [JSProperty(Name = "webImageWidth")]
        public int WebImageWidth
        {
            get
            {
                return m_documentLibrary.WebImageWidth;
            }
            set
            {
                m_documentLibrary.WebImageWidth = value;
            }
        }


        [JSFunction(Name = "getCheckedOutFiles")]
        public SPCheckedOutFilesListInstance GetCheckedOutFiles()
        {
            return m_documentLibrary.CheckedOutFiles == null
                ? null
                : new SPCheckedOutFilesListInstance(this.Engine, m_documentLibrary.CheckedOutFiles);
        }

        [JSFunction(Name = "getItemsInFolder")]
        public SPListItemCollectionInstance GetItemsInFolder(SPViewInstance view, SPFolderInstance folder)
        {
            if (view == null)
                throw new JavaScriptException(this.Engine, "Error", "A SPView must be specified as the first argument.");

            if (folder == null)
                throw new JavaScriptException(this.Engine, "Error", "A SPFolder must be specified as the second argument.");

            var items = m_documentLibrary.GetItemsInFolder(view.View, folder.Folder);

            return items == null
                ? null
                : new SPListItemCollectionInstance(this.Engine.Object.InstancePrototype, items);
        }
    }
}
