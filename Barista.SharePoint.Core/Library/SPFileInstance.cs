namespace Barista.SharePoint.Library
{
    using Barista.Extensions;
    using Barista.Library;
    using Barista.Newtonsoft.Json;
    using Jurassic;
    using Jurassic.Library;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Utilities;
    using System;
    using System.Linq;
    using System.Web.UI.WebControls.WebParts;

    [Serializable]
    public class SPFileConstructor : ClrFunction
    {
        public SPFileConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPFile", new SPFileInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPFileInstance Construct(string fileUrl)
        {
            SPFile file;

            if (SPHelper.TryGetSPFile(fileUrl, out file) == false)
                throw new JavaScriptException(this.Engine, "Error", "A file with the specified url does not exist.");

            return new SPFileInstance(this.InstancePrototype, file);
        }

        public SPFileInstance Construct(SPFile file)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            return new SPFileInstance(this.InstancePrototype, file);
        }
    }

    [Serializable]
    public class SPFileInstance : ObjectInstance
    {
        [NonSerialized]
        private readonly SPFile m_file;

        public SPFileInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPFileInstance(ObjectInstance prototype, SPFile file)
            : this(prototype)
        {
            this.m_file = file;
        }

        internal SPFile File
        {
            get { return m_file; }
        }

        #region Properties
        [JSProperty(Name = "allProperties")]
        public ObjectInstance AllProperties
        {
            get
            {
                var result = this.Engine.Object.Construct();

                foreach (var key in m_file.Properties.Keys)
                {
                    string serializedKey;
                    if (key is string)
                        serializedKey = key as string;
                    else
                        serializedKey = JsonConvert.SerializeObject(key);

                    var serializedValue = JsonConvert.SerializeObject(m_file.Properties[key]);

                    result.SetPropertyValue(serializedKey, JSONObject.Parse(this.Engine, serializedValue, null), false);
                }

                return result;
            }
        }

        [JSProperty(Name = "author")]
        [JSDoc("Gets the author (original creator) of the file.")]
        public SPUserInstance Author
        {
            get
            {
                return m_file.Author == null
                  ? null
                  : new SPUserInstance(this.Engine.Object.InstancePrototype, m_file.Author);
            }
        }

        //BackwardsLinks
        //CharSetName

        [JSProperty(Name = "checkedOutByUser")]
        [JSDoc("Gets the login name of the user who the file is checked out to.")]
        public SPUserInstance CheckedOutByUser
        {
            get
            {
                if (m_file.CheckedOutByUser == null)
                    return null;

                return new SPUserInstance(this.Engine.Object.InstancePrototype, m_file.CheckedOutByUser);
            }
        }

        [JSProperty(Name = "checkedOutDate")]
        public DateInstance CheckedOutDate
        {
            get
            {
                if (m_file.CheckOutType != SPFile.SPCheckOutType.None)
                    return JurassicHelper.ToDateInstance(this.Engine, m_file.CheckedOutDate);

                return null;
            }
        }

        [JSProperty(Name = "checkInComment")]
        public string CheckInComment
        {
            get
            {
                return m_file.CheckInComment;
            }
        }

        [JSProperty(Name = "checkOutType")]
        [JSDoc("Gets the current level of check out of the file.")]
        public string CheckOutType
        {
            get
            {
                return m_file.CheckOutType.ToString();
            }
        }

        [JSProperty(Name = "contentType")]
        public string ContentType
        {
            get
            {
                return StringHelper.GetMimeTypeFromFileName(m_file.Name);
            }
        }

        [JSProperty(Name = "customizedPageStatus")]
        public string CustomizedPageStatus
        {
            get
            {
                return m_file.CustomizedPageStatus.ToString();
            }
        }

        //effectiveauditmask
        //effectiverawpermissions

        [JSProperty(Name = "eTag")]
        public string ETag
        {
            get
            {
                return m_file.ETag;
            }
        }

        [JSProperty(Name = "eventReceivers")]
        public SPEventReceiverDefinitionCollectionInstance EventReceivers
        {
            get
            {
                return m_file.EventReceivers == null
                    ? null
                    : new SPEventReceiverDefinitionCollectionInstance(this.Engine.Object.InstancePrototype, m_file.EventReceivers);
            }
        }

        [JSProperty(Name = "exists")]
        [JSDoc("Returns a value that indicates if the file exists.")]
        public bool Exists
        {
            get
            {
                return m_file.Exists;
            }
        }

        //Forwardlinks
        //Generatingconverterid
        //Generator

        [JSProperty(Name = "iconUrl")]
        public string IconUrl
        {
            get
            {
                return m_file.IconUrl;
            }
        }

        [JSProperty(Name = "inDocumentLibrary")]
        public bool InDocumentLibrary
        {
            get
            {
                return m_file.InDocumentLibrary;
            }
        }

        [JSProperty(Name = "isConvertedFile")]
        public bool IsConvertedFile
        {
            get
            {
                return m_file.IsConvertedFile;
            }
        }

        [JSProperty(Name = "isSharedAccessRequested")]
        public bool IsSharedAccessRequested
        {
            get
            {
                return m_file.IsSharedAccessRequested;
            }
        }

        [JSProperty(Name = "length")]
        [JSDoc("Gets size of the file in bytes.")]
        public double Length
        {
            get
            {
                return m_file.Length;
            }
        }

        //lengthbyuser

        [JSProperty(Name = "listRelativeUrl")]
        [JSDoc("Gets the list relative url of the file")]
        public string ListRelativeUrl
        {
            get { return m_file.Url; }
        }

        [JSProperty(Name = "level")]
        public string Level
        {
            get
            {
                return m_file.Level.ToString();
            }
        }

        [JSProperty(Name = "lockedByUser")]
        public SPUserInstance LockedByUser
        {
            get
            {
                return m_file.LockedByUser == null
                    ? null
                    : new SPUserInstance(this.Engine.Object.InstancePrototype, m_file.LockedByUser);
            }
        }


        [JSProperty(Name = "lockedDate")]
        public DateInstance LockedDate
        {
            get
            {
                return m_file.LockType != SPFile.SPLockType.None
                    ? JurassicHelper.ToDateInstance(this.Engine, m_file.LockedDate)
                    : null;
            }
        }

        [JSProperty(Name = "lockExpires")]
        public DateInstance LockExpires
        {
            get
            {
                return m_file.LockType != SPFile.SPLockType.None
                    ? JurassicHelper.ToDateInstance(this.Engine, m_file.LockExpires)
                    : null;
            }
        }

        [JSProperty(Name = "lockId")]
        public string LockId
        {
            get
            {
                return m_file.LockId;
            }
        }

        [JSProperty(Name = "lockType")]
        public string LockType
        {
            get
            {
                return m_file.LockType.ToString();
            }
        }

        [JSProperty(Name = "majorVersion")]
        public int MajorVersion
        {
            get
            {
                return m_file.MajorVersion;
            }
        }

        [JSProperty(Name = "minorVersion")]
        public int MinorVersion
        {
            get
            {
                return m_file.MinorVersion;
            }
        }

        [JSProperty(Name = "modifiedBy")]
        public SPUserInstance ModifiedBy
        {
            get
            {
                if (m_file.ModifiedBy == null)
                    return null;

                return new SPUserInstance(this.Engine.Object.InstancePrototype, m_file.ModifiedBy);
            }
        }

        [JSProperty(Name = "name")]
        public string Name
        {
            get
            {
                return m_file.Name;
            }
        }

        [JSProperty(Name = "parentFolderName")]
        public string ParentFolderName
        {
            get
            {
                if (m_file.ParentFolder == null)
                    return String.Empty;

                return m_file.ParentFolder.Url;
            }
        }

        [JSProperty(Name = "progId")]
        public string ProgId
        {
            get { return m_file.ProgID; }
        }

        [JSProperty(Name = "propertyBag")]
        public HashtableInstance PropertyBag
        {
            get
            {
                return m_file.Properties == null
                    ? null
                    : new HashtableInstance(this.Engine.Object.InstancePrototype, m_file.Properties);
            }
        }

        [JSProperty(Name = "requiresCheckout")]
        public bool RequiresCheckout
        {
            get
            {
                return m_file.RequiresCheckout;
            }
        }

        [JSProperty(Name = "serverRedirected")]
        public bool ServerRedirected
        {
            get
            {
                return m_file.ServerRedirected;
            }
        }

        [JSProperty(Name = "serverRelativeUrl")]
        [JSDoc("Gets the server relative url of the file")]
        public string ServerRelativeUrl
        {
            get
            {
                return m_file.ServerRelativeUrl;
            }
        }

        //SourceFile

        [JSProperty(Name = "sourceLeafName")]
        public string SourceLeafName
        {
            get
            {
                return m_file.SourceLeafName;
            }
        }

        [JSProperty(Name = "sourceUIVersion")]
        public int SourceUIVersion
        {
            get
            {
                return m_file.SourceUIVersion;
            }
        }

        [JSProperty(Name = "timeCreated")]
        public DateInstance TimeCreated
        {
            get
            {
                return JurassicHelper.ToDateInstance(this.Engine, m_file.TimeCreated);
            }
        }

        [JSProperty(Name = "timeLastModified")]
        public DateInstance TimeLastModified
        {
            get
            {
                return JurassicHelper.ToDateInstance(this.Engine, m_file.TimeLastModified);
            }
        }

        [JSProperty(Name = "title")]
        public string Title
        {
            get
            {
                return m_file.Title;
            }
        }

        //TotalLength

        [JSProperty(Name = "uiVersion")]
        public int UIVersion
        {
            get
            {
                return m_file.UIVersion;
            }
        }

        [JSProperty(Name = "uiVersionLabel")]
        public string UIVersionLabel
        {
            get
            {
                return m_file.UIVersionLabel;
            }
        }

        [JSProperty(Name = "uniqueId")]
        public GuidInstance UniqueId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_file.UniqueId);
            }
        }

        [JSProperty(Name = "url")]
        [JSDoc("Gets the absolute url of the file")]
        public string Url
        {
            get
            {
                return SPUtility.ConcatUrls(m_file.Web.Url, m_file.Url);
            }
        }
        #endregion

        [JSFunction(Name = "addProperty")]
        public void AddProperty(object key, object value)
        {
            m_file.AddProperty(key, value);
        }

        [JSFunction(Name = "approve")]
        [JSDoc("Approves the file submitted for content approval with the specified comment.")]
        public void Approve(string comment)
        {
            m_file.Approve(comment);
        }

        [JSFunction(Name = "canOpenFile")]
        public string CanOpenFile(bool checkCanGetFileSource)
        {
            string result;
            if (m_file.CanOpenFile(checkCanGetFileSource, out result))
                return result;

            return null;
        }

        [JSFunction(Name = "checkIn")]
        [JSDoc("Checks the file in. The first argument is a (string) comment, the second is an optional (string) value of one of these values: MajorCheckIn, MinorCheckIn, OverwriteCheckIn")]
        public void CheckIn(string comment, object checkInType)
        {
            SPCheckinType checkInTypeValue;
            if (checkInType.TryParseEnum(true, SPCheckinType.MinorCheckIn, out checkInTypeValue))
            {
                m_file.CheckIn(comment, checkInTypeValue);
            }
            else
            {
                m_file.CheckIn(comment);
            }
        }

        [JSFunction(Name = "checkOut")]
        [JSDoc("Sets the checkout state of the file as checked out to the current user.")]
        public void CheckOut()
        {
            m_file.CheckOut();
        }

        [JSFunction(Name = "convert")]
        public ObjectInstance Convert(ObjectInstance options)
        {
            //Woah.
            var converterId = options.GetPropertyValue("converterId");
            var newFileName = TypeConverter.ToString(options.GetPropertyValue("newFileName"));
            var configInfo = TypeConverter.ToString(options.GetPropertyValue("configInfo"));
            var handlerAssembly = TypeConverter.ToString(options.GetPropertyValue("handlerAssembly"));
            var handlerClass = TypeConverter.ToString(options.GetPropertyValue("handlerClass"));
            var priority = TypeConverter.ToInteger(options.GetPropertyValue("priority"));
            var peopleToAlert = TypeConverter.ToString(options.GetPropertyValue("peopleToAlert"));
            var sendACopy = TypeConverter.ToBoolean(options.GetPropertyValue("sendACopy"));
            var synchronous = TypeConverter.ToBoolean(options.GetPropertyValue("synchronous"));

            var guidConverterId = GuidInstance.ConvertFromJsObjectToGuid(converterId);
            Guid guidWorkItemId;

            var conversionResult = m_file.Convert(guidConverterId, newFileName, configInfo, handlerAssembly, handlerClass,
                (byte) priority, peopleToAlert, sendACopy, synchronous, out guidWorkItemId);

            var result = this.Engine.Object.Construct();
            result.SetPropertyValue("workItemId", guidWorkItemId, false);
            result.SetPropertyValue("conversionResult", conversionResult.ToString(), false);
            return result;
        }

        [JSFunction(Name = "convertLock")]
        public void ConvertLock(string fromType, string toType, string fromLockId, string toLockId, string newTimeout)
        {
            SPFile.SPLockType ltFromType;
            SPFile.SPLockType ltToType;

            if (!fromType.TryParseEnum(true, out ltFromType))
                throw new JavaScriptException(this.Engine, "Error", "Cannot convert fromType");

            if (!toType.TryParseEnum(true, out ltToType))
                throw new JavaScriptException(this.Engine, "Error", "Cannot convert toType");

            m_file.ConvertLock(ltFromType, ltToType, fromLockId, toLockId, TimeSpan.Parse(newTimeout));
        }

        [JSFunction(Name = "copyTo")]
        public void CopyTo(string newUrl, bool overwrite)
        {
            m_file.CopyTo(newUrl, overwrite);
        }

        //CreateSharedAccessRequest

        [JSFunction(Name = "delete")]
        public void Delete()
        {
            m_file.Delete();
        }

        [JSFunction(Name = "deleteAllPersonalizations")]
        public void DeleteAllPersonalizations(int userId)
        {
            m_file.DeleteAllPersonalizations(userId);
        }

        [JSFunction(Name = "deleteAllPersonalizationsAllUsers")]
        public void DeleteAllPersonalizationsAllUsers()
        {
            m_file.DeleteAllPersonalizationsAllUsers();
        }

        [JSFunction(Name = "deleteProperty")]
        public void DeleteProperty(object key)
        {
            m_file.DeleteProperty(key);
        }

        [JSFunction(Name = "deny")]
        [JSDoc("Denies approval for a file that was submitted for content approval.")]
        public void Deny(string comment)
        {
            m_file.Deny(comment);
        }

        [JSFunction(Name = "getConversionState")]
        public string GetConversionState(object converterId, object workItemId)
        {
            var guidConverterId = GuidInstance.ConvertFromJsObjectToGuid(converterId);
            var guidWorkItemId = GuidInstance.ConvertFromJsObjectToGuid(workItemId);

            var result = m_file.GetConversionState(guidConverterId, guidWorkItemId);
            return result.ToString();
        }

        [JSFunction(Name = "getConvertedFile")]
        public SPFileInstance GetConvertedFile(object converterId)
        {
            var guidConverterId = GuidInstance.ConvertFromJsObjectToGuid(converterId);

            var result = m_file.GetConvertedFile(guidConverterId);
            if (result == null)
                return null;

            return new SPFileInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getDocumentLibrary")]
        public SPListInstance GetDocumentLibrary()
        {
            if (m_file.InDocumentLibrary == false)
                return null;
            return new SPListInstance(this.Engine, null, null, m_file.DocumentLibrary);
        }

        [JSFunction(Name = "getLimitedWebPartManager")]
        public SPLimitedWebPartManagerInstance GetLimitedWebPartManager(string personalizationScope)
        {
            PersonalizationScope scope;
            if (!personalizationScope.TryParseEnum(true, out scope))
                throw new JavaScriptException(this.Engine, "Error", "A valid personalization scope must be supplied.");

            var result = m_file.GetLimitedWebPartManager(scope);
            return result == null
                ? null
                : new SPLimitedWebPartManagerInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getListItem")]
        public SPListItemInstance GetListItem(object fields)
        {
            if (fields == null)
                throw new JavaScriptException(this.Engine, "Error", "Fields argument must be supplied.");

            var fieldsArrayInstance = fields as ArrayInstance;
            if (fields == Undefined.Value || fields == Null.Value || fieldsArrayInstance == null || fieldsArrayInstance.Length == 0)
                return new SPListItemInstance(this.Engine, m_file.GetListItem());

            return new SPListItemInstance(this.Engine, m_file.GetListItem(fieldsArrayInstance.ElementValues.Select(TypeConverter.ToString).ToArray()));
        }

        [JSFunction(Name = "getListItemAllFields")]
        public SPListItemInstance GetListItemAllFields()
        {
            return new SPListItemInstance(this.Engine, m_file.ListItemAllFields);
        }

        //GetHtmlTranslateCacheItem
        //GetHtmlTranslateCacheNameFromStsName

        [JSFunction(Name = "getParentFolder")]
        public SPFolderInstance GetParentFolder()
        {
            return new SPFolderInstance(this.Engine.Object.InstancePrototype, null, null, m_file.ParentFolder);
        }

        [JSFunction(Name = "getParentWeb")]
        public SPWebInstance GetParentWeb()
        {
            return new SPWebInstance(this.Engine, m_file.Web);
        }

        [JSFunction(Name = "getPermissions")]
        public SPSecurableObjectInstance GetPermissions()
        {
            return new SPSecurableObjectInstance(this.Engine)
            {
                SecurableObject = m_file.Item
            };
        }

        [JSFunction(Name = "getProperty")]
        public object GetProperty(object key)
        {
            return m_file.GetProperty(key);
        }

        [JSFunction(Name = "getVersionHistory")]
        public SPFileVersionCollectionInstance GetVersionHistory()
        {
            return new SPFileVersionCollectionInstance(this.Engine.Object.InstancePrototype, m_file.Versions);
        }

        [JSFunction(Name = "getWeb")]
        public SPWebInstance GetWeb()
        {
            return new SPWebInstance(this.Engine, m_file.Web);
        }

        [JSFunction(Name = "lock")]
        public void Lock(string lockType, string lockId, string timeout)
        {
            SPFile.SPLockType spLockType;
            lockType.TryParseEnum(true, SPFile.SPLockType.Exclusive, out spLockType);

            m_file.Lock(spLockType, lockId, TimeSpan.Parse(timeout));
        }

        [JSFunction(Name = "moveTo")]
        public void MoveTo(string newUrl, bool overwrite)
        {
            m_file.MoveTo(newUrl, overwrite);
        }

        [JSFunction(Name = "openBinary")]
        [JSDoc("Returns a Base-64 Encoded byte array of the contents of the file.")]
        public Base64EncodedByteArrayInstance OpenBinary(string openOptions)
        {
            Base64EncodedByteArrayInstance result;

            if (String.IsNullOrEmpty(openOptions) || openOptions == Undefined.Value.ToString())
            {
                result = new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, m_file.OpenBinary());
            }
            else
            {
                var openOptionsValue = (SPOpenBinaryOptions)Enum.Parse(typeof(SPOpenBinaryOptions), openOptions);
                result = new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, m_file.OpenBinary(openOptionsValue));
            }

            result.FileName = m_file.Name;
            result.MimeType = StringHelper.GetMimeTypeFromFileName(m_file.Name);
            return result;
        }

        [JSFunction(Name = "publish")]
        [JSDoc("Publishes the file.")]
        public void Publish(string comment)
        {
            m_file.Publish(comment);
        }

        [JSFunction(Name = "saveBinary")]
        [JSDoc("Updates the file with the contents of the specified argument.")]
        public void SaveBinary(Base64EncodedByteArrayInstance data)
        {
            m_file.SaveBinary(data.Data);
        }

        [JSFunction(Name = "recycle")]
        [JSDoc("Moves the file to the recycle bin.")]
        public GuidInstance Recycle()
        {
            return new GuidInstance(this.Engine.Object.InstancePrototype, m_file.Recycle());
        }

        [JSFunction(Name = "refreshLock")]
        public void RefreshLock(string lockId, string timeout)
        {
            m_file.RefreshLock(lockId, TimeSpan.Parse(timeout));
        }

        [JSFunction(Name = "releaseLock")]
        public void ReleaseLock(string lockId)
        {
            m_file.ReleaseLock(lockId);
        }

        [JSFunction(Name = "removeSharedAccessRequest")]
        public void RemoveSharedAccessRequest()
        {
            m_file.RemoveSharedAccessRequest();
        }

        [JSFunction(Name = "replaceLink")]
        public void ReplaceLink(string oldUrl, string newUrl)
        {
            m_file.ReplaceLink(oldUrl, newUrl);
        }

        [JSFunction(Name = "revertContentStream")]
        public void RevertContentStream()
        {
            m_file.RevertContentStream();
        }

        [JSFunction(Name = "revertToLastApprovedVersion")]
        [JSDoc("")]
        public string RevertToLastApprovedVersion()
        {
            throw new JavaScriptException(this.Engine, "Error", "Not Yet Implemented");

            //  var currentApprovedVersion = m_file.Item.Versions[0];

            //  var lastApprovedVersion = m_file.Versions
            //                                  .OfType<SPFileVersion>()
            //                                  .OrderByDescending(v => v.ID)
            //                                  .FirstOrDefault(
            //                                    v => v.Level == SPFileLevel.Published &&
            //                                         v.IsCurrentVersion == false);

            //  if (lastApprovedVersion == null)
            //    return "";

            //  m_file.Versions.RestoreByID(lastApprovedVersion.ID);
            //  m_file.Publish("Reverting to Last Approved Verison");
            //  m_file.Approve("Approving Last Approved Version.");

            //  m_file.Versions.RestoreByLabel(currentApprovedVersion.VersionLabel);
            //  return lastApprovedVersion.VersionLabel;
        }

        [JSFunction(Name = "scheduleEnd")]
        public void ScheduleEnd(DateInstance endDate)
        {
            if (endDate == null)
                throw new JavaScriptException(this.Engine, "Error", "endDate argument must be supplied.");
            m_file.ScheduleEnd(endDate.Value);
        }

        [JSFunction(Name = "scheduleStart")]
        public void ScheduleStart(DateInstance startDate, object setModerationStatus, object approvalComment)
        {
            if (startDate == null)
                throw new JavaScriptException(this.Engine, "Error", "startDate argument must be supplied.");

            if (setModerationStatus == Undefined.Value)
            {
                m_file.ScheduleStart(startDate.Value);
                return;
            }

            if (approvalComment == Undefined.Value)
            {
                m_file.ScheduleStart(startDate.Value, TypeConverter.ToBoolean(setModerationStatus));
                return;
            }

            m_file.ScheduleStart(startDate.Value, TypeConverter.ToBoolean(setModerationStatus), TypeConverter.ToString(approvalComment));
        }

        //SendToOfficialFile

        [JSFunction(Name = "setProperty")]
        public void SetProperty(object key, object value)
        {
            m_file.SetProperty(key, value);
        }

        [JSFunction(Name = "takeOffline")]
        public void TakeOffline()
        {
            m_file.TakeOffline();
        }

        [JSFunction(Name = "undoCheckOut")]
        [JSDoc("Un-checkouts the file.")]
        public void UndoCheckOut()
        {
            m_file.UndoCheckOut();
        }

        [JSFunction(Name = "unPublish")]
        [JSDoc("Unpublishes the file.")]
        public void UnPublish(string comment)
        {
            m_file.UnPublish(comment);
        }

        [JSFunction(Name = "update")]
        [JSDoc("Updates the file with any changes.")]
        public void Update()
        {
            m_file.Update();
        }
    }
}
