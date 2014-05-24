namespace Barista.SharePoint.Library
{
    using System;
    using System.Linq;
    using Jurassic;
    using Jurassic.Library;
    using Microsoft.SharePoint;
    using Microsoft.Office.DocumentManagement.DocumentSets;
    using System.Text;
    using System.Collections.Generic;
    using Barista.Library;
    using Newtonsoft.Json;

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
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPFolderInstance(ObjectInstance prototype, SPSite site, SPWeb web, SPFolder folder)
            : this(prototype)
        {
            this.m_folder = folder;
            this.m_site = site;
            this.m_web = web;
            this.m_folder = folder;
        }

        #region Properties

        public SPFolder Folder
        {
            get { return m_folder; }
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

        [JSProperty(Name = "serverRelativeUrl")]
        public string ServerRelativeUrl
        {
            get
            {
                return m_folder.ServerRelativeUrl;
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

        [JSFunction(Name = "addSubFolder")]
        public SPFolderInstance AddSubFolder(string url)
        {
            var subFolder = m_folder.SubFolders.Add(url);
            return new SPFolderInstance(this.Engine.Object.InstancePrototype, null, null, subFolder);
        }

        [JSFunction(Name = "delete")]
        public void Delete()
        {
            m_folder.Delete();
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

        [JSFunction(Name = "getContentTypeOrder")]
        public ArrayInstance ContentTypeOrder()
        {
            if (m_folder.ParentListId == Guid.Empty)
                return null;

            if (m_folder.ContentTypeOrder == null)
                return null;

            var result = this.Engine.Array.Construct();
            foreach (var contentType in m_folder.ContentTypeOrder)
            {
                ArrayInstance.Push(result, new SPContentTypeInstance(this.Engine.Object.InstancePrototype, contentType));
            }

            return result;
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

        [JSFunction(Name = "getPropertyBagValue")]
        public string GetPropertyBagValue(string value)
        {
            return m_folder.GetProperty(value) as string;
        }

        [JSFunction(Name = "getFiles")]
        public ArrayInstance GetFiles([DefaultParameterValue(false)] bool recursive)
        {
            //ContentIterator iterator = new ContentIterator();
            //iterator.ProcessFilesInFolder(m_folder, recursive, (file) =>
            //  {
            //    files.Add(file);
            //  },
            //  (file, ex) =>
            //  {
            //    return false; // do not rethrow errors;
            //  });

            //var result = this.Engine.Array.Construct();

            //foreach (SPFile file in files)
            //{
            //  ArrayInstance.Push(result, new SPFileInstance(this.Engine.Object.InstancePrototype, file));
            //}
            //return result;

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

        [JSFunction(Name = "getUniqueContentTypeOrder")]
        public ArrayInstance GetUniqueContentTypeOrder()
        {
            if (m_folder.ParentListId == Guid.Empty)
                return null;

            if (m_folder.UniqueContentTypeOrder == null)
                return null;

            var result = this.Engine.Array.Construct();
            foreach (var contentType in m_folder.UniqueContentTypeOrder)
            {
                ArrayInstance.Push(result, contentType.Id.ToString());
            }

            return result;
        }

        [JSFunction(Name = "setPropertyBagValue")]
        public void SetPropertyBagValue(string key, string value)
        {
            m_folder.SetProperty(key, value);
        }

        [JSFunction(Name = "setUniqueContentTypeOrder")]
        public void SetUniqueContentTypeOrder(ArrayInstance value)
        {
            List<SPContentType> contentTypes = new List<SPContentType>();
            for (int i = 0; i < value.Length; i++)
            {
                SPContentType inOrderContentType = null;
                if (value[i] is SPContentTypeInstance)
                {
                    inOrderContentType = (value[i] as SPContentTypeInstance).ContentType;
                }

                if (inOrderContentType != null)
                {
                    if (m_folder.ContentTypeOrder.Any(ct => ct.Id == inOrderContentType.Id))
                        contentTypes.Add(inOrderContentType);
                }
            }

            if (m_folder.ContentTypeOrder.Count == contentTypes.Count)
                m_folder.UniqueContentTypeOrder = contentTypes;
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

            if (m_web != null)
            {
                m_web.Dispose();
                m_web = null;
            }
        }
    }
}
