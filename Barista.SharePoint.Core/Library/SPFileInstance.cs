namespace Barista.SharePoint.Library
{
  using System;
  using Barista.Extensions;
  using Barista.Newtonsoft.Json;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using Barista.Library;
  using Microsoft.SharePoint.Utilities;

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

    [JSProperty(Name = "eTag")]
    public string ETag
    {
      get
      {
        return m_file.ETag;
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

    [JSProperty(Name = "length")]
    [JSDoc("Gets size of the file in bytes.")]
    public double Length
    {
      get
      {
        return m_file.Length;
      }
    }

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
        if (m_file.LockedByUser == null)
          return null;

        return new SPUserInstance(this.Engine.Object.InstancePrototype, m_file.LockedByUser);
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

    [JSProperty(Name = "serverRelativeUrl")]
    [JSDoc("Gets the server relative url of the file")]
    public string ServerRelativeUrl
    {
      get
      {
        return m_file.ServerRelativeUrl;
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

    [JSFunction(Name = "approve")]
    [JSDoc("Approves the file submitted for content approval with the specified comment.")]
    public void Approve(string comment)
    {
      m_file.Approve(comment);
    }

    [JSFunction(Name="checkIn")]
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

    [JSFunction(Name = "copyTo")]
    public void CopyTo(string newUrl, bool overwrite)
    {
      m_file.CopyTo(newUrl, overwrite);
    }

    [JSFunction(Name = "delete")]
    public void Delete()
    {
      m_file.Delete();
    }
    
    [JSFunction(Name = "deny")]
    [JSDoc("Denies approval for a file that was submitted for content approval.")]
    public void Deny(string comment)
    {
      m_file.Deny(comment);
    }

    [JSFunction(Name = "getDocumentLibrary")]
    public SPListInstance GetDocumentLibrary()
    {
      if (m_file.InDocumentLibrary == false)
        return null;
      return new SPListInstance(this.Engine, null, null, m_file.DocumentLibrary);
    }

    [JSFunction(Name = "getListItemAllFields")]
    public SPListItemInstance GetListItemAllFields()
    {
      return new SPListItemInstance(this.Engine, m_file.ListItemAllFields);
    }

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

    [JSFunction(Name = "getVersionHistory")]
    public SPFileVersionCollectionInstance GetVersionHistory()
    {
      return new SPFileVersionCollectionInstance(this.Engine.Object.InstancePrototype, m_file.Versions);
    }

    //TODO: getLimitedWebPartManager

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
    public string Recycle()
    {
      return m_file.Recycle().ToString();
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
