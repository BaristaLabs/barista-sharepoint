namespace Barista.SharePoint.Library
{
  using System;
  using System.Linq;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using Microsoft.Office.DocumentManagement.DocumentSets;
  using System.Collections;
  using System.Text;
  using System.Collections.Generic;
  using Microsoft.Office.Server.Utilities;
  using Barista.Library;

  public class SPFolderConstructor : ClrFunction
  {
    public SPFolderConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPFolder", new SPFolderInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPFolderInstance Construct(string folderUrl)
    {
      SPFolder folder;

      if (SPHelper.TryGetSPFolder(folderUrl, out folder) == false)
        throw new JavaScriptException(this.Engine, "Error", "No folder is available at the specified url.");

      return new SPFolderInstance(this.InstancePrototype, folder);
    }

    public SPFolderInstance Construct(SPFolder folder)
    {
      if (folder == null)
        throw new ArgumentNullException("folder");

      return new SPFolderInstance(this.InstancePrototype, folder);
    }
  }

  public class SPFolderInstance : ObjectInstance
  {
    private SPFolder m_folder;

    public SPFolderInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPFolderInstance(ObjectInstance prototype, SPFolder folder)
      : this(prototype)
    {
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
    public SPDocumentSetInstance addDocumentSet(string name, object contentType, [DefaultParameterValue(null)] object properties = null, [DefaultParameterValue(true)] bool provisionDefaultContent = true)
    {
      SPContentTypeId contentTypeId = SPContentTypeId.Empty;

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

      DocumentSet docSet = DocumentSet.Create(m_folder, name, contentTypeId, htProperties, provisionDefaultContent);
      return new SPDocumentSetInstance(this.Engine.Object.InstancePrototype, docSet);
    }

    [JSFunction(Name = "addFile")]
    public SPFileInstance AddFile(object file, [DefaultParameterValue(true)] bool overwrite = false)
    {
      SPFile result = null;
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
    public SPFileInstance AddFile(string url, object data, [DefaultParameterValue(true)] bool overwrite = true)
    {
      SPFile result = null;
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
    public SPFolderInstance addSubFolder(string url)
    {
      var subFolder = m_folder.SubFolders.Add(url);
      return new SPFolderInstance(this.Engine.Object.InstancePrototype, subFolder);
    }

    [JSFunction(Name = "delete")]
    public void Delete()
    {
      m_folder.Delete();
    }

    [JSFunction(Name = "ensureSubFolderExists")]
    public SPFolderInstance EnsureFolderExists(string folderName)
    {
      var subFolder = m_folder.SubFolders.OfType<SPFolder>()
                                         .Where(f => f.Name == folderName)
                                         .FirstOrDefault();
      if (subFolder == null)
      {
        m_folder.ParentWeb.AllowUnsafeUpdates = true;
        subFolder = m_folder.SubFolders.Add(m_folder.ServerRelativeUrl + "/" + folderName);

        m_folder.Update();
        m_folder.ParentWeb.AllowUnsafeUpdates = false;
      }

      return new SPFolderInstance(this.Engine.Object.InstancePrototype, subFolder);
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
      return new SPFolderInstance(this.Engine.Object.InstancePrototype, m_folder.ParentFolder);
    }

    [JSFunction(Name = "getPermissions")]
    public SPSecurableObjectInstance GetPermissions()
    {
      return new SPSecurableObjectInstance(this.Engine.Object.InstancePrototype, this.m_folder.Item);
    }

    [JSFunction(Name = "getFiles")]
    public ArrayInstance GetFiles([DefaultParameterValue(false)] bool recursive = false)
    {
      List<SPFile> files = new List<SPFile>();

      ContentIterator iterator = new ContentIterator();
      iterator.ProcessFilesInFolder(m_folder, recursive, (file) =>
        {
          files.Add(file);
        },
        (file, ex) =>
        {
          return false; // do not rethrow errors;
        });
      var result = this.Engine.Array.Construct();

      foreach (SPFile file in files)
      {
        ArrayInstance.Push(result, new SPFileInstance(this.Engine.Object.InstancePrototype, file));
      }
      return result;
    }

    [JSFunction(Name = "getSubFolders")]
    public ArrayInstance GetSubFolders()
    {
      var result = this.Engine.Array.Construct();
      foreach (var folder in m_folder.SubFolders.OfType<SPFolder>())
      {
        ArrayInstance.Push(result, new SPFolderInstance(this.Engine.Object.InstancePrototype, folder));
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

    [JSFunction(Name = "update")]
    public void Update()
    {
      m_folder.Update();
    }
  }
}
