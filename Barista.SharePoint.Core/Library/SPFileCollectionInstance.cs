namespace Barista.SharePoint.Library
{
    using System.Collections;
    using System.IO;
    using Barista.Extensions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;
    using Microsoft.SharePoint;

    [Serializable]
    public class SPFileCollectionConstructor : ClrFunction
    {
        public SPFileCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPFileCollection", new SPFileCollectionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPFileCollectionInstance Construct()
        {
            return new SPFileCollectionInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPFileCollectionInstance : ObjectInstance
    {
        private readonly SPFileCollection m_fileCollection;

        public SPFileCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPFileCollectionInstance(ObjectInstance prototype, SPFileCollection fileCollection)
            : this(prototype)
        {
            if (fileCollection == null)
                throw new ArgumentNullException("fileCollection");

            m_fileCollection = fileCollection;
        }

        public SPFileCollection SPFileCollection
        {
            get { return m_fileCollection; }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return m_fileCollection.Count;
            }
        }

        [JSFunction(Name = "add")]
        public SPFileInstance Add(string urlOfFile,
            Base64EncodedByteArrayInstance file,
            object properties,
            object createdBy,
            object modifiedBy,
            object timeCreated,
            object timeLastModified,
            object checkInComment,
            object overwrite,
            object requireWebFilePermissions)
        {
            if (urlOfFile.IsNullOrWhiteSpace())
                throw new JavaScriptException(this.Engine, "Error", "File Url must be specified as the first parameter.");

            if (file == null)
                throw new JavaScriptException(this.Engine, "Error", "File Url must be specified as the first parameter.");

            Hashtable htProperties = null;
            if (properties is HashtableInstance)
                htProperties = (properties as HashtableInstance).Hashtable;
            else if (properties != null && properties != Null.Value && properties != Undefined.Value)
                    htProperties = SPHelper.GetFieldValuesHashtableFromPropertyObject(properties);

            SPUser uCreatedBy = null;
            if (createdBy is SPUserInstance)
                uCreatedBy = (createdBy as SPUserInstance).User;


            SPUser uModifiedBy = null;
            if (modifiedBy is SPUserInstance)
                uModifiedBy = (modifiedBy as SPUserInstance).User;

            var dtTimeCreated = default(DateTime);
            if (timeCreated is DateInstance)
                dtTimeCreated = (timeCreated as DateInstance).Value;
            else if (timeCreated != null && timeCreated != Null.Value && timeCreated != Undefined.Value)
                dtTimeCreated = new DateInstance(this.Engine.Object.InstancePrototype, DateInstance.Parse(TypeConverter.ToString(timeCreated))).Value;

            var dtTimeLastModified = default(DateTime);
            if (timeLastModified is DateInstance)
                dtTimeLastModified = (timeLastModified as DateInstance).Value;
            else if (timeLastModified != null && timeLastModified != Null.Value && timeLastModified != Undefined.Value)
                dtTimeLastModified = new DateInstance(this.Engine.Object.InstancePrototype, DateInstance.Parse(TypeConverter.ToString(timeLastModified))).Value;

            string strCheckInComment = null;
            if (checkInComment != null && checkInComment != Null.Value && checkInComment != Undefined.Value)
                strCheckInComment = TypeConverter.ToString(checkInComment);

            var bOverwrite = false;
            if (overwrite != null && overwrite != Null.Value && overwrite != Undefined.Value)
                bOverwrite = TypeConverter.ToBoolean(overwrite);

            var bRequireWebFilePermissions = false;
            if (requireWebFilePermissions != null && requireWebFilePermissions != Null.Value && requireWebFilePermissions != Undefined.Value)
                bRequireWebFilePermissions = TypeConverter.ToBoolean(requireWebFilePermissions);

            using (var ms = new MemoryStream(file.Data))
            {
                var result = m_fileCollection.Add(urlOfFile, ms, htProperties, uCreatedBy, uModifiedBy, dtTimeCreated, dtTimeLastModified, strCheckInComment, bOverwrite, bRequireWebFilePermissions);
                return result == null
                    ? null
                    : new SPFileInstance(this.Engine.Object.InstancePrototype, result);
            }
        }

        [JSFunction(Name = "delete")]
        public void Delete(string urlOfFile)
        {
            m_fileCollection.Delete(urlOfFile);
        }

        [JSFunction(Name = "getFileByIndex")]
        public SPFileInstance GetFileByIndex(int index)
        {
            var result = m_fileCollection[index];
            if (result == null)
                return null;

            return new SPFileInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getFolderByUrl")]
        public SPFileInstance GetFolderByUrl(string urlOfFile)
        {
            var result = m_fileCollection[urlOfFile];
            if (result == null)
                return null;

            return new SPFileInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getParenFlder")]
        public SPFolderInstance GetParentFolder()
        {
            var result = m_fileCollection.Folder;
            if (result == null)
                return null;

            return new SPFolderInstance(this.Engine.Object.InstancePrototype, result.ParentWeb.Site, result.ParentWeb,
                result);
        }

        [JSFunction(Name = "getWeb")]
        public SPWebInstance GetWeb()
        {
            var result = m_fileCollection.Web;
            return result == null
                ? null
                : new SPWebInstance(this.Engine, result);
        }
    }
}
