namespace Barista.SharePoint.Library
{
    using Barista.Library;
    using Jurassic;
    using Jurassic.Library;
    using Microsoft.Office.DocumentManagement.DocumentSets;
    using Microsoft.SharePoint;
    using Newtonsoft.Json;
    using System;
    using System.Linq;
    using System.Text;

    [Serializable]
    public class SPFolderConstructor : ClrFunction
    {
        public SPFolderConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPFolder", new SPFolderInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPFolderInstance Construct(string folderUrl)
        {
            SPSite site;
            SPWeb web;
            SPFolder folder;

            if (SPHelper.TryGetSPFolder(folderUrl, out site, out web, out folder) == false)
                throw new JavaScriptException(this.Engine, "Error", "No folder is available at the specified url.");

            return new SPFolderInstance(this.InstancePrototype, site, web, folder);
        }

        public SPFolderInstance Construct(SPFolder folder)
        {
            if (folder == null)
                throw new ArgumentNullException("folder");

            return new SPFolderInstance(this.InstancePrototype, null, null, folder);
        }
    }

    [Serializable]
    public class SPFolderInstance : ObjectInstance, IDisposable
    {
        private SPSite m_site;
        private SPWeb m_web;
        private readonly SPFolder m_folder;

        public SPFolderInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFunctions();
        }

        public SPFolderInstance(ObjectInstance prototype, SPSite site, SPWeb web, SPFolder folder)
            : this(prototype)
        {
            this.m_folder = folder;
            this.m_site = site;
            this.m_web = web;
            this.m_folder = folder;

            m_files = new Lazy<SPFileCollectionInstance>(() => m_folder.Files == null
                ? null
                : new SPFileCollectionInstance(this.Engine.Object.InstancePrototype, m_folder.Files));

            m_subFolders = new Lazy<SPFolderCollectionInstance>(() => m_folder.SubFolders == null
                ? null
                : new SPFolderCollectionInstance(this.Engine.Object.InstancePrototype, m_folder.SubFolders));
        }

        #region Properties

        public SPFolder Folder
        {
            get { return m_folder; }
        }

        [JSProperty(Name = "audit")]
        public SPAuditInstance Audit
        {
            get
            {
                return m_folder.Audit == null
                    ? null
                    : new SPAuditInstance(this.Engine.Object.InstancePrototype, m_folder.Audit);
            }
        }

        [JSProperty(Name = "containingDocumentLibrary")]
        public GuidInstance ContainingDocumentLibrary
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_folder.ContainingDocumentLibrary);
            }
        }

        [JSProperty(Name = "contentTypeOrder")]
        public SPContentTypeListInstance ContentTypeOrder
        {
            get
            {
                if (m_folder.ParentListId == Guid.Empty)
                    return null;

                return m_folder.ContentTypeOrder == null
                    ? null
                    : new SPContentTypeListInstance(this.Engine, m_folder.ContentTypeOrder);
            }
        }

        //Effective Audit Mask
        //EffectiveRawPermissions


        [JSProperty(Name = "exists")]
        public bool Exists
        {
            get
            {
                return m_folder.Exists;
            }
        }

        private readonly Lazy<SPFileCollectionInstance> m_files;

        [JSProperty(Name = "files")]
        public SPFileCollectionInstance Files
        {
            get
            {
                return m_files.Value;
            }
        }

        [JSProperty(Name = "itemCount")]
        public int ItemCount
        {
            get
            {
                return m_folder.ItemCount;
            }
        }

        [JSProperty(Name = "name")]
        public string Name
        {
            get
            {
                return m_folder.Name;
            }
        }

        [JSProperty(Name = "parentListId")]
        public GuidInstance ParentListId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_folder.ParentListId);
            }
        }

        [JSProperty(Name = "progId")]
        public string ProgId
        {
            get
            {
                return m_folder.ProgID;
            }
        }

        [JSProperty(Name = "propertyBag")]
        public HashtableInstance PropertyBag
        {
            get
            {
                return m_folder.Properties == null
                    ? null
                    : new HashtableInstance(this.Engine.Object.InstancePrototype, m_folder.Properties);
            }
        }

        [JSProperty(Name = "allProperties")]
        public ObjectInstance AllProperties
        {
            get
            {
                var result = this.Engine.Object.Construct();

                foreach (var key in m_folder.Properties.Keys)
                {
                    string serializedKey;
                    if (key is string)
                        serializedKey = key as string;
                    else
                        serializedKey = JsonConvert.SerializeObject(key);

                    var serializedValue = JsonConvert.SerializeObject(m_folder.Properties[key]);

                    result.SetPropertyValue(serializedKey, JSONObject.Parse(this.Engine, serializedValue, null), false);
                }

                return result;
            }
        }

        [JSProperty(Name = "requiresCheckout")]
        public bool RequiresCheckout
        {
            get
            {
                return m_folder.RequiresCheckout;
            }
        }

        [JSProperty(Name = "serverRelativeUrl")]
        public string ServerRelativeUrl
        {
            get
            {
                return m_folder.ServerRelativeUrl;
            }
        }

        private readonly Lazy<SPFolderCollectionInstance> m_subFolders;

        [JSProperty(Name = "subFolders")]
        public SPFolderCollectionInstance SubFolders
        {
            get
            {
                return m_subFolders.Value;
            }
        }

        [JSProperty(Name = "uniqueContentTypeOrder")]
        public SPContentTypeListInstance UniqueContentTypeOrder
        {
            get
            {
                if (m_folder.ParentListId == Guid.Empty)
                    return null;

                return m_folder.UniqueContentTypeOrder == null
                    ? null
                    : new SPContentTypeListInstance(this.Engine, m_folder.UniqueContentTypeOrder);
            }
        }

        [JSProperty(Name = "uniqueId")]
        public string UniqueId
        {
            get
            {
                return m_folder.UniqueId.ToString();
            }
        }

        [JSProperty(Name = "url")]
        public string Url
        {
            get
            {
                return m_folder.Url;
            }
        }

        [JSProperty(Name = "welcomePage")]
        public string WelcomePage
        {
            get
            {
                return m_folder.WelcomePage;
            }
            set
            {
                m_folder.WelcomePage = value;
            }
        }
        #endregion

        [JSFunction(Name = "addDocumentSet")]
        public SPDocumentSetInstance AddDocumentSet(string name, object contentType, [DefaultParameterValue(null)] object properties, [DefaultParameterValue(true)] bool provisionDefaultContent)
        {
            var contentTypeId = SPContentTypeId.Empty;

            if (contentType is SPContentTypeIdInstance)
            {
                contentTypeId = (contentType as SPContentTypeIdInstance).ContentTypeId;
            }
            else if (contentType is SPContentTypeInstance)
            {
                contentTypeId = (contentType as SPContentTypeInstance).ContentType.Id;
            }
            else if (contentType is string)
            {
                contentTypeId = new SPContentTypeId(contentType as string);
            }

            if (contentTypeId == SPContentTypeId.Empty)
                return null;

            var htProperties = SPHelper.GetFieldValuesHashtableFromPropertyObject(properties);

            var docSet = DocumentSet.Create(m_folder, name, contentTypeId, htProperties, provisionDefaultContent);
            return new SPDocumentSetInstance(this.Engine.Object.InstancePrototype, null, null, docSet);
        }

        [JSFunction(Name = "addFile")]
        public SPFileInstance AddFile(object file, [DefaultParameterValue(true)] bool overwrite)
        {
            SPFile result;
            if (file is Base64EncodedByteArrayInstance)
            {
                var byteArray = file as Base64EncodedByteArrayInstance;
                if (String.IsNullOrEmpty(byteArray.FileName))
                    throw new JavaScriptException(this.Engine, "Error", "The specified Base64EncodedByteArray did not specify a filename.");

                m_folder.ParentWeb.AllowUnsafeUpdates = true;
                result = m_folder.Files.Add(m_folder.ServerRelativeUrl + "/" + byteArray.FileName, byteArray.Data, overwrite);
                m_folder.Update();
                m_folder.ParentWeb.AllowUnsafeUpdates = false;
            }
            else
                throw new JavaScriptException(this.Engine, "Error", "Unsupported type when adding a file: " + file.GetType());

            return new SPFileInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "addFileByUrl")]
        public SPFileInstance AddFile(string url, object data, [DefaultParameterValue(true)] bool overwrite)
        {
            SPFile result;
            if (data is Base64EncodedByteArrayInstance)
            {
                var byteArrayInstance = data as Base64EncodedByteArrayInstance;
                result = m_folder.Files.Add(url, byteArrayInstance.Data, overwrite);
            }
            else if (data is string)
            {
                result = m_folder.Files.Add(url, Encoding.UTF8.GetBytes(data as string), overwrite);
            }
            else
                throw new JavaScriptException(this.Engine, "Error", "Unable to create SPFile: Unsupported data type: " + data.GetType());

            return new SPFileInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "addProperty")]
        public void AddProperty(object key, object value)
        {
            m_folder.AddProperty(key, value);
        }

        [JSFunction(Name = "addSubFolder")]
        public SPFolderInstance AddSubFolder(string url)
        {
            var subFolder = m_folder.SubFolders.Add(url);
            return new SPFolderInstance(this.Engine.Object.InstancePrototype, null, null, subFolder);
        }

        [JSFunction(Name = "copyTo")]
        public void CopyTo(string strNewUrl)
        {
            m_folder.CopyTo(strNewUrl);
        }

        [JSFunction(Name = "delete")]
        public void Delete()
        {
            m_folder.Delete();
        }

        [JSFunction(Name = "deleteProperty")]
        public void DeleteProperty(object key)
        {
            m_folder.DeleteProperty(key);
        }

        [JSFunction(Name = "ensureSubFolderExists")]
        public SPFolderInstance EnsureFolderExists(string folderName)
        {
            var subFolder = m_folder.SubFolders
                                    .OfType<SPFolder>()
                                    .FirstOrDefault(f => f.Name == folderName);

            if (subFolder == null)
            {
                m_folder.ParentWeb.AllowUnsafeUpdates = true;
                subFolder = m_folder.SubFolders.Add(m_folder.ServerRelativeUrl + "/" + folderName);

                m_folder.Update();
                m_folder.ParentWeb.AllowUnsafeUpdates = false;
            }

            return new SPFolderInstance(this.Engine.Object.InstancePrototype, null, null, subFolder);
        }

        [JSFunction(Name = "getDocumentLibrary")]
        public SPDocumentLibraryInstance GetDocumentLibrary()
        {
            return m_folder.DocumentLibrary == null
                ? null
                : new SPDocumentLibraryInstance(this.Engine, m_site, m_web, m_folder.DocumentLibrary);
        }

        [JSFunction(Name = "getItem")]
        public SPListItemInstance GetItem()
        {
            return m_folder.Item == null
                ? null
                : new SPListItemInstance(this.Engine, m_folder.Item);
        }

        [JSFunction(Name = "getParentFolder")]
        public SPFolderInstance GetParentFolder()
        {
            return new SPFolderInstance(this.Engine.Object.InstancePrototype, null, null, m_folder.ParentFolder);
        }

        [JSFunction(Name = "getParentWeb")]
        public SPWebInstance GetParentWeb()
        {
            return new SPWebInstance(this.Engine, m_folder.ParentWeb);
        }


        [JSFunction(Name = "getPermissions")]
        public SPSecurableObjectInstance GetPermissions()
        {
            return new SPSecurableObjectInstance(this.Engine)
            {
                SecurableObject = this.m_folder.Item
            };
        }

        [JSFunction(Name = "getPropertyBagValueAsString")]
        public string GetPropertyBagValue(string key)
        {
            var value = m_folder.GetProperty(key);

            if (value is string)
                return value as string;

            return value.ToString();
        }

        [JSFunction(Name = "getPropertyBagValue")]
        public object GetProperty(object key)
        {
            return TypeConverter.ToObject(this.Engine, m_folder.GetProperty(key));
        }

        [JSFunction(Name = "getFiles")]
        public ArrayInstance GetFiles(bool recursive)
        {
            var listItemInstances = m_folder.Files
                                            .OfType<SPFile>()
                                            .Select(file => new SPFileInstance(this.Engine.Object.InstancePrototype, file));

            // ReSharper disable CoVariantArrayConversion
            return this.Engine.Array.Construct(listItemInstances.ToArray());
            // ReSharper restore CoVariantArrayConversion
        }

        [JSFunction(Name = "getSubFolders")]
        public ArrayInstance GetSubFolders()
        {
            var result = this.Engine.Array.Construct();
            foreach (var folder in m_folder.SubFolders.OfType<SPFolder>())
            {
                ArrayInstance.Push(result, new SPFolderInstance(this.Engine.Object.InstancePrototype, null, null, folder));
            }

            return result;
        }

        [JSFunction(Name = "moveTo")]
        public void MoveTo(string strNewUrl)
        {
            m_folder.MoveTo(strNewUrl);
        }

        [JSFunction(Name = "setPropertyBagValue")]
        public void SetPropertyBagValue(string key, string value)
        {
            m_folder.SetProperty(key, value);
        }

        [JSFunction(Name = "setProperty")]
        public void SetProperty(object key, object value)
        {
            m_folder.SetProperty(key, value);
        }

        [JSFunction(Name = "recycle")]
        public string Recycle()
        {
            return m_folder.Recycle().ToString();
        }

        [JSFunction(Name = "deletePropertyBagValue")]
        public void DeletePropertyBagValue(string key)
        {
            m_folder.DeleteProperty(key);
        }

        [JSFunction(Name = "update")]
        public void Update()
        {
            m_folder.Update();
        }

        public void Dispose()
        {
            if (m_site != null)
            {
                m_site.Dispose();
                m_site = null;
            }

            if (m_web == null)
                return;

            m_web.Dispose();
            m_web = null;
        }

        [JSFunction(Name = "toString")]
        public override string ToString()
        {
            return m_folder.ToString();
        }
    }
}
