namespace Barista.SharePoint.Library
{
  using System;
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
    [JSProperty(Name = "author")]
    public SPUserInstance Author
    {
      get
      {
        if (m_file.Author == null)
          return null;

        return new SPUserInstance(this.Engine.Object.InstancePrototype, m_file.Author);
      }
    }

    [JSProperty(Name = "checkedOutByUser")]
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
    public bool Exists
    {
      get
      {
        return m_file.Exists;
      }
    }

    [JSProperty(Name = "length")]
    public double Length
    {
      get
      {
        return m_file.Length;
      }
    }

    [JSProperty(Name = "listRelativeUrl")]
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

    [JSProperty(Name = "serverRelativeUrl")]
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
    public string Url
    {
      get
      {
        return SPUtility.ConcatUrls(m_file.Web.Url, m_file.Url);
      }
    }
    #endregion

    [JSFunction(Name="checkIn")]
    public void CheckIn(string comment, string checkInType)
    {
      if (String.IsNullOrEmpty(checkInType))
      {
        m_file.CheckIn(comment);
      }
      else
      {
        var checkInTypeValue = (SPCheckinType)Enum.Parse(typeof(SPCheckinType), checkInType);
        m_file.CheckIn(comment, checkInTypeValue);
      }
    }

    [JSFunction(Name = "checkOut")]
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

    [JSFunction(Name = "getDocumentLibrary")]
    public SPListInstance GetDocumentLibrary()
    {
      if (m_file.InDocumentLibrary == false)
        return null;
      return new SPListInstance(this.Engine.Object.InstancePrototype, null, null, m_file.DocumentLibrary);
    }

    [JSFunction(Name = "getListItemAllFields")]
    public SPListItemInstance GetListItemAllFields()
    {
      return new SPListItemInstance(this.Engine.Object.InstancePrototype, m_file.ListItemAllFields);
    }

    [JSFunction(Name = "getParentFolder")]
    public SPFolderInstance GetParentFolder()
    {
      return new SPFolderInstance(this.Engine.Object.InstancePrototype, null, null, m_file.ParentFolder);
    }

    [JSFunction(Name = "getParentWeb")]
    public SPWebInstance GetParentWeb()
    {
      return new SPWebInstance(this.Engine.Object.InstancePrototype, m_file.Web);
    }

    [JSFunction(Name = "getPermissions")]
    public SPSecurableObjectInstance GetPermissions()
    {
      return new SPSecurableObjectInstance(this.Engine.Object.InstancePrototype, m_file.Item);
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
    public void Publish(string comment)
    {
      m_file.Publish(comment);
    }

    [JSFunction(Name = "recycle")]
    public string Recycle()
    {
      return m_file.Recycle().ToString();
    }

    [JSFunction(Name = "undoCheckOut")]
    public void UndoCheckOut()
    {
      m_file.UndoCheckOut();
    }

    [JSFunction(Name = "unPublish")]
    public void UnPublish(string comment)
    {
      m_file.UnPublish(comment);
    }
  }
}
