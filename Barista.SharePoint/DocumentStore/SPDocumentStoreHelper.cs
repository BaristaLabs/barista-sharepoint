namespace Barista.SharePoint.DocumentStore
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;
  using System.Web;
  using Microsoft.Office.Server.Utilities;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Utilities;
  using Barista.DocumentStore;
  using System.Text;
  using Microsoft.Office.DocumentManagement.DocumentSets;

  /// <summary>
  /// Contains methods that assist with the retrieval of Document Store objects.
  /// </summary>
  public static class SPDocumentStoreHelper
  {
    #region Mapping

    public static bool TryGetDocumentStoreAttachment(SPList list, SPFile defaultEntityPart, string attachmentFileName, out SPFile attachment)
    {
      //TODO: Possibly SPQuerify this.
      var attachmentContentType = list.ContentTypes.OfType<SPContentType>().Where(ct => ct.Id.ToString().ToLowerInvariant().StartsWith(Constants.AttachmentDocumentContentTypeId.ToLowerInvariant())).FirstOrDefault();

      attachment = defaultEntityPart.ParentFolder.Files.OfType<SPFile>().Where(f => f.Name == attachmentFileName && f.Item.ContentTypeId.IsChildOf(attachmentContentType.Id)).FirstOrDefault();
      if (attachment == null)
        return false;

      return true;
    }

    public static Attachment MapAttachmentFromSPFile(SPFile file)
    {
      Attachment result = new Attachment()
      {
        Category = file.Item["Category"] as string,
        Path = file.Item["Path"] as string,
        ETag = file.ETag,
        FileName = file.Name,
        MimeType = StringHelper.GetMimeTypeFromFileName(file.Name),
        Size = file.Length,
        Url = file.Web.Url + "/" + file.Url
      };

      result.Created = (DateTime)file.Item[SPBuiltInFieldId.Created];
      result.Modified = (DateTime)file.Item[SPBuiltInFieldId.Modified];

      var createdByUser = file.Item[SPBuiltInFieldId.Created_x0020_By] as SPFieldUserValue;
      if (createdByUser != null)
      {
        result.CreatedBy = new User()
        {
          Email = createdByUser.User.Email,
          LoginName = createdByUser.User.LoginName,
          Name = createdByUser.User.Name,
        };
      }

      var modifiedByUser = file.Item[SPBuiltInFieldId.Modified_x0020_By] as SPFieldUserValue;
      if (modifiedByUser != null)
      {
        result.ModifiedBy = new User()
        {
          Email = modifiedByUser.User.Email,
          LoginName = modifiedByUser.User.LoginName,
          Name = modifiedByUser.User.Name,
        };
      }

      return result;
    }

    public static Comment MapCommentFromSPListItem(SPListItem listItem)
    {
      return MapCommentFromSPListItemVersion(listItem.Versions.OfType<SPListItemVersion>().OrderByDescending(v => v.Created).First());
    }

    public static Comment MapCommentFromSPListItemVersion(SPListItemVersion listItemVersion)
    {
      var result = new Comment()
        {
          Id = listItemVersion.VersionId,
          CommentText = listItemVersion["Comments"] as string,
          Created = listItemVersion.Created,
        };

      result.CreatedBy = new User()
      {
        Email = listItemVersion.CreatedBy.User.Email,
        LoginName = listItemVersion.CreatedBy.User.LoginName,
        Name = listItemVersion.CreatedBy.User.Name,
      };
      
      return result;
    }

    public static Container MapContainerFromSPList(SPList list)
    {
      if (list == null)
        throw new ArgumentNullException("list");

      var result = new Container()
      {
        Id = list.ID,
        Title = list.Title,
        Description = list.Description,
        EntityCount = list.ItemCount,
        Url = list.RootFolder.Url.Substring(list.RootFolder.Url.IndexOf('/') + 1),
        Created = list.Created,
        CreatedBy = new User()
        {
          Email = list.Author.Email,
          LoginName = list.Author.LoginName,
          Name = list.Author.Name,
        }

      };

      result.Modified = (DateTime)list.RootFolder.Properties["vti_timelastmodified"];
      
      //TODO: ModifiedBy using the last item added to the list.
      return result;
    }

    public static Entity MapEntityFromSPListItem(SPListItem listItem)
    {
      if (listItem == null)
        throw new ArgumentNullException("listItem", "When creating an Entity, the SPListItem that represents the entity must not be null.");

      Entity entity = new Entity();

      try
      {
        string id = listItem[Constants.DocumentEntityGuidFieldId] as string;

        entity.Id = new Guid(id);
      }
      catch
      {
        //Do Nothing...
      }

      entity.Namespace = listItem[Constants.NamespaceFieldId] as string;

      var docSet = DocumentSet.GetDocumentSet(listItem.Folder);

      SPFile dataFile = null;

      try
      {
        dataFile = listItem.Web.GetFile(listItem.Folder.Url + "/" + Constants.DocumentStoreDefaultEntityPartFileName);
      }
      catch (Exception) { /* Do Nothing... */ };

      if (dataFile == null) //The default entity part file doesn't exist, get outta dodge (something happened here...)
        throw new InvalidOperationException("No correpsonding entity file exists on the SP Doc Set that represents the entity.");


      entity.ETag = dataFile.ETag;
      entity.Title = docSet.Item.Title;
      entity.Description = docSet.Item["DocumentSetDescription"] as string;
      entity.Created = (DateTime)docSet.Item[SPBuiltInFieldId.Created];
      entity.Modified = (DateTime)docSet.Item[SPBuiltInFieldId.Modified];

      var latestFile = listItem.Folder.Files.OfType<SPFile>().OrderByDescending(f => f.TimeLastModified).FirstOrDefault();
      var combinedETag = String.Join(", ", listItem.Folder.Files.OfType<SPFile>().Select(f => f.ETag).ToArray());
      entity.ContentsETag = StringHelper.CreateMD5Hash(combinedETag);
      entity.ContentsModified = latestFile.TimeLastModified;

      entity.Path = docSet.ParentFolder.Url.Substring(listItem.ParentList.RootFolder.Url.Length);
      entity.Path = entity.Path.TrimStart('/');

      entity.Data = Encoding.UTF8.GetString(dataFile.OpenBinary());

      var createdByUserValue = listItem[SPBuiltInFieldId.Author] as String;
      SPFieldUserValue createdByUser = new SPFieldUserValue(listItem.Web, createdByUserValue);

      if (createdByUser != null)
      {
        entity.CreatedBy = new User()
        {
          Email = createdByUser.User.Email,
          LoginName = createdByUser.User.LoginName,
          Name = createdByUser.User.Name,
        };
      }

      var modifiedByUserValue = listItem[SPBuiltInFieldId.Editor] as String;
      SPFieldUserValue modifiedByUser = new SPFieldUserValue(listItem.Web, createdByUserValue);

      if (modifiedByUser != null)
      {
        entity.ModifiedBy = new User()
        {
          Email = modifiedByUser.User.Email,
          LoginName = modifiedByUser.User.LoginName,
          Name = modifiedByUser.User.Name,
        };
      }

      return entity;
    }

    public static Entity MapEntityFromSPListItemVersion(SPListItemVersion version)
    {
      Entity result = new Entity();
      try
      {
        string id = version[version.ListItem.Fields[Constants.DocumentEntityGuidFieldId].Title] as string;

        result.Id = new Guid(id);
      }
      catch
      {
        //Do Nothing...
      }

      result.Namespace = version[version.ListItem.Fields[Constants.NamespaceFieldId].Title] as string;

      result.Title = version[version.ListItem.Fields[SPBuiltInFieldId.Title].Title] as string;
      result.Description = version["DocumentSetDescription"] as string;
      result.Created = (DateTime)version[version.ListItem.Fields[SPBuiltInFieldId.Created].Title];
      result.Modified = (DateTime)version[version.ListItem.Fields[SPBuiltInFieldId.Modified].Title];

      result.Path = version.ListItem.File.ParentFolder.Url.Substring(version.ListItem.ParentList.RootFolder.Url.Length);
      result.Path = result.Path.TrimStart('/');

      var createdByUserValue = version[version.ListItem.Fields[SPBuiltInFieldId.Author].Title] as string;
      SPFieldUserValue createdByUser = new SPFieldUserValue(version.ListItem.Web, createdByUserValue);

      if (createdByUser != null)
      {
        result.CreatedBy = new User()
        {
          Email = createdByUser.User.Email,
          LoginName = createdByUser.User.LoginName,
          Name = createdByUser.User.Name,
        };
      }

      var modifiedByUserValue = version[version.ListItem.Fields[SPBuiltInFieldId.Editor].Title] as string;
      SPFieldUserValue modifiedByUser = new SPFieldUserValue(version.ListItem.Web, createdByUserValue);

      if (modifiedByUser != null)
      {
        result.ModifiedBy = new User()
        {
          Email = modifiedByUser.User.Email,
          LoginName = modifiedByUser.User.LoginName,
          Name = modifiedByUser.User.Name,
        };
      }

      if (version.IsCurrentVersion)
      {
        var dataFile = version.ListItem.File;
        result.ETag = dataFile.ETag;
        using (StreamReader reader = new StreamReader(dataFile.OpenBinaryStream()))
        {
          result.Data = reader.ReadToEnd();
        };
      }
      else
      {
        var dataFile = version.ListItem.File.Versions.GetVersionFromID(version.VersionId);
        //result.ETag = dataFile.Properties["ETag"];
        using (StreamReader reader = new StreamReader(dataFile.OpenBinaryStream()))
        {
          result.Data = reader.ReadToEnd();
        };
      }
      return result;
    }

    public static EntityPart MapEntityPartFromSPFile(SPFile file)
    {
      if (file == null)
        throw new ArgumentNullException("file", "When creating an EntityPart, the SPFile that represents the entity part must not be null.");

      EntityPart entityPart = new EntityPart();
      try
      {
        string id = file.Item[Constants.DocumentEntityGuidFieldId] as string;

        entityPart.EntityId = new Guid(id);
      }
      catch
      {
        //Do Nothing...
      }

      entityPart.Category = file.Item["Category"] as string;
      entityPart.ETag = file.ETag;
      entityPart.Name = file.Name.Substring(0, file.Name.Length - Constants.DocumentSetEntityPartExtension.Length);
      entityPart.Created = (DateTime)file.Item[SPBuiltInFieldId.Created];
      entityPart.Modified = (DateTime)file.Item[SPBuiltInFieldId.Modified];

      entityPart.Data = Encoding.UTF8.GetString(file.OpenBinary());

      var createdByUserValue = file.Item[SPBuiltInFieldId.Author] as string;
      SPFieldUserValue createdByUser = new SPFieldUserValue(file.Web, createdByUserValue);

      if (createdByUser != null)
      {
        entityPart.CreatedBy = new User()
        {
          Email = createdByUser.User.Email,
          LoginName = createdByUser.User.LoginName,
          Name = createdByUser.User.Name,
        };
      }

      var modifiedByUserValue = file.Item[SPBuiltInFieldId.Editor] as string;
      SPFieldUserValue modifiedByUser = new SPFieldUserValue(file.Web, createdByUserValue);

      if (modifiedByUser != null)
      {
        entityPart.ModifiedBy = new User()
        {
          Email = modifiedByUser.User.Email,
          LoginName = modifiedByUser.User.LoginName,
          Name = modifiedByUser.User.Name,
        };
      }

      return entityPart;
    }

    public static Folder MapFolderFromSPFolder(SPFolder folder)
    {
      if (folder == null)
        throw new ArgumentNullException("folder");

      var result = new Folder()
      {
        Name = folder.Name,
        FullPath = folder.Url.Substring(folder.ParentWeb.Lists[folder.ParentListId].RootFolder.Url.Length + 1),
        EntityCount = folder.ItemCount,
      };

      result.Created = (DateTime)folder.Item[SPBuiltInFieldId.Created];
      result.Modified = (DateTime)folder.Item[SPBuiltInFieldId.Modified];

      var createdByUserValue = folder.Item[SPBuiltInFieldId.Author] as String;
      SPFieldUserValue createdByUser = new SPFieldUserValue(folder.ParentWeb, createdByUserValue);

      if (createdByUser != null)
      {
        result.CreatedBy = new User()
        {
          Email = createdByUser.User.Email,
          LoginName = createdByUser.User.LoginName,
          Name = createdByUser.User.Name,
        };
      }

      var modifiedByUserValue = folder.Item[SPBuiltInFieldId.Editor] as String;
      SPFieldUserValue modifiedByUser = new SPFieldUserValue(folder.ParentWeb, createdByUserValue);

      if (modifiedByUser != null)
      {
        result.ModifiedBy = new User()
        {
          Email = modifiedByUser.User.Email,
          LoginName = modifiedByUser.User.LoginName,
          Name = modifiedByUser.User.Name,
        };
      }

      return result;
    }

    #endregion

    /// <summary>
    /// Returns a collection of all containers contained in the specified web.
    /// </summary>
    /// <param name="web"></param>
    /// <returns></returns>
    public static IEnumerable<SPDocumentLibrary> GetContainers(SPWeb web)
    {
      if (web == null)
        throw new ArgumentNullException("web");

      List<SPDocumentLibrary> result = new List<SPDocumentLibrary>();

      ContentIterator listsIterator = new ContentIterator();
      listsIterator.ProcessLists(web.Lists, (currentList) =>
      {
        if (currentList is SPDocumentLibrary && currentList.TemplateFeatureId == Constants.DocumentContainerFeatureId)
          result.Add(currentList as SPDocumentLibrary);
      }, null);

      return result;
    }

    /// <summary>
    /// Returns a value that indicates of a SPFolder that corresponds to the specified path is able to be retrieved.
    /// </summary>
    /// <param name="web">The web.</param>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="list">The list.</param>
    /// <param name="folder">The folder.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public static bool TryGetFolderFromPath(SPWeb web, string containerTitle, out SPList list, out SPFolder folder, string path)
    {
      list = web.Lists.TryGetList(containerTitle);

      if ((list == null) || (list.TemplateFeatureId != Constants.DocumentContainerFeatureId))
      {
        list = null;
        folder = null;
        return false;
      }

      SPFolder currentFolder = list.RootFolder;
      try
      {
        foreach (string subFolder in DocumentStoreHelper.GetPathSegments(path))
        {
          currentFolder = web.GetFolder(list.RootFolder.Url + "/" + path);
        }
      }
      catch (Exception)
      {
        list = null;
        folder = null;
        return false;
      }

      if (currentFolder.Exists == false)
      {
        list = null;
        folder = null;
        return false;
      }

      folder = currentFolder;
      return true;

    }

    /// <summary>
    /// Returns the SPFolder that corresponds to the specified path.
    /// </summary>
    /// <param name="web"></param>
    /// <param name="containerTitle"></param>
    /// <param name="path"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    public static SPFolder GetFolderFromPath(SPWeb web, string containerTitle, string path, out SPList list)
    {
      SPList localList;
      SPFolder folder = null;
      if (TryGetFolderFromPath(web, containerTitle, out localList, out folder, path) == false)
        throw new InvalidOperationException("The specified folder does not exist.");

      list = localList;
      return folder;
    }

    /// <summary>
    /// Returns a value that indicates if the default entity part for the specified document store is able to be retrieved.
    /// </summary>
    /// <param name="list"></param>
    /// <param name="folder"></param>
    /// <param name="id"></param>
    /// <param name="defaultEntityPart"></param>
    /// <returns></returns>
    public static bool TryGetDocumentStoreDefaultEntityPart(SPList list, SPFolder folder, Guid id, out SPFile defaultEntityPart)
    {
      defaultEntityPart = SPDocumentStoreHelper.GetDocumentStoreDefaultEntityPartForGuid(list, folder, id);
      if (defaultEntityPart == null)
      {
        return false;
      }
      return true;
    }

    /// <summary>
    /// Gets the document store default entity part contained in the specified folder that corresponds to the specified guid.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <param name="folder">The folder.</param>
    /// <param name="id">The id.</param>
    /// <returns></returns>
    public static SPFile GetDocumentStoreDefaultEntityPartForGuid(SPList list, SPFolder folder, Guid id)
    {
      //Attempt to prevent having to do a full SPQuery by attempting to retrieve the file from the subfolder. (Performance)
      var documentStoreEntityContentTypeId = new SPContentTypeId(Constants.DocumentStoreEntityContentTypeId);
      var documentStoreEntityPartContentTypeId = new SPContentTypeId(Constants.DocumentStoreEntityPartContentTypeId);
      SPFolder childFolder = null;
      try
      {
        childFolder = list.ParentWeb.GetFolder(folder.Url + "/" + id.ToString());
      }
      catch (ArgumentException) { /* Do Nothing */ }

      if (childFolder != null && childFolder.Exists && childFolder.Item.ContentTypeId.IsChildOf(documentStoreEntityContentTypeId))
      {
        SPFile childFile = null;
        try
        {
          childFile = list.ParentWeb.GetFile(folder.Url + "/" + id.ToString() + "/" + Constants.DocumentStoreDefaultEntityPartFileName);
        }
        catch (ArgumentException) { /* Do Nothing */ }

        if (childFile != null && childFile.Exists && childFile.Item.ContentTypeId.IsChildOf(documentStoreEntityPartContentTypeId))
          return childFile;
      }

      //If we weren't able to retrieve the SPFile directly, use a SPQuery to recursively obtain the file (should be more efficient than a recursive function)
      SPQuery query = new SPQuery();
      query.Folder = folder;
      query.Query = @"<Where><And><And>
                        <BeginsWith><FieldRef Name=""ContentTypeId""/><Value Type=""Text"">" + Constants.DocumentStoreEntityPartContentTypeId.ToUpperInvariant() + @"</Value></BeginsWith>
                        <Eq><FieldRef Name=""DocumentEntityGuid"" /><Value Type=""Text"">" + id.ToString() + @"</Value></Eq>
                      </And>
                        <Eq><FieldRef Name=""FileLeafRef""/><Value Type=""Text"">" + Constants.DocumentStoreDefaultEntityPartFileName + @"</Value></Eq>
                      </And></Where>";
      query.RowLimit = 1;
      query.QueryThrottleMode = SPQueryThrottleOption.Override;
      query.ViewAttributes = "Scope=\"Recursive\"";

      //SPFile dataFile = null;
      //ContentIterator itemsIterator = new ContentIterator();
      //itemsIterator.ProcessListItems(list, query, false, (spListItem) =>
      //{
      //  dataFile = spListItem.File;
      //  itemsIterator.Cancel = true;
      //}, null);
      //
      //return dataFile;

      var item = list.GetItems(query).OfType<SPListItem>().FirstOrDefault();

      if (item != null)
        return item.File;

      return null;
    }

    /// <summary>
    /// Returns a value that indicates if a SPFile in the specified SPFolder is able to be retrieved.
    /// </summary>
    /// <param name="list"></param>
    /// <param name="folder"></param>
    /// <param name="id"></param>
    /// <param name="partName"></param>
    /// <param name="entityPart"></param>
    /// <returns></returns>
    public static bool TryGetDocumentStoreEntityPart(SPList list, SPFolder folder, Guid id, string partName, out SPFile entityPart)
    {
      entityPart = SPDocumentStoreHelper.GetDocumentStoreEntityPart(list, folder, id, partName);
      if (entityPart == null)
        return false;
      return true;
    }

    /// <summary>
    /// Returns an SPFile that corresponds to the specified Entity Guid and Part Name (without Extension)
    /// </summary>
    /// <param name="list"></param>
    /// <param name="folder"></param>
    /// <param name="id"></param>
    /// <param name="partName"></param>
    /// <returns></returns>
    public static SPFile GetDocumentStoreEntityPart(SPList list, SPFolder folder, Guid id, string partName)
    {
      //Attempt to prevent having to do a full SPQuery by attempting to retrieve the file from the subfolder. (Performance)
      var documentStoreEntityContentTypeId = new SPContentTypeId(Constants.DocumentStoreEntityContentTypeId);
      var documentStoreEntityPartContentTypeId = new SPContentTypeId(Constants.DocumentStoreEntityPartContentTypeId);
      SPFolder childFolder = null;
      try
      {
        childFolder = list.ParentWeb.GetFolder(folder.Url + "/" + id.ToString());
      }
      catch (ArgumentException) { /* Do Nothing */ }

      if (childFolder != null && childFolder.Exists && childFolder.Item.ContentTypeId.IsChildOf(documentStoreEntityContentTypeId))
      {
        SPFile childFile = null;
        try
        {
          childFile = list.ParentWeb.GetFile(folder.Url + "/" + id.ToString() + "/" + partName + Constants.DocumentSetEntityPartExtension);
        }
        catch (ArgumentException) { /* Do Nothing */ }

        if (childFile != null && childFile.Exists && childFile.Item.ContentTypeId.IsChildOf(documentStoreEntityPartContentTypeId))
          return childFile;
      }

      SPQuery query = new SPQuery();
      query.Folder = folder;
      query.Query = @"<Where><And><And>
                        <BeginsWith><FieldRef Name=""ContentTypeId""/><Value Type=""Text"">" + Constants.DocumentStoreEntityPartContentTypeId.ToUpperInvariant() + @"</Value></BeginsWith>
                        <Eq><FieldRef Name=""DocumentEntityGuid"" /><Value Type=""Text"">" + id.ToString() + @"</Value></Eq>
                      </And>
                        <Eq><FieldRef Name=""FileLeafRef""/><Value Type=""Text"">" + partName + Constants.DocumentSetEntityPartExtension + @"</Value></Eq>
                      </And></Where>";
      query.RowLimit = 1;
      query.ViewAttributes = "Scope=\"Recursive\"";

      //SPFile dataFile = null;
      //ContentIterator itemsIterator = new ContentIterator();
      //itemsIterator.ProcessListItems(list, query, false, (spListItem) =>
      //{
      //  dataFile = spListItem.File;
      //  itemsIterator.Cancel = true;
      //}, null);

      //return dataFile;

      var item = list.GetItems(query).OfType<SPListItem>().FirstOrDefault();

      if (item != null)
        return item.File;

      return null;
    }

    /// <summary>
    /// Executes the specified action asynchronously, while preserving both the HttpContext and SPContext
    /// </summary>
    /// <param name="contextAction"></param>
    public static Task ExecuteAsync(HttpContext context, SPWeb webContext, Action action)
    {
      var siteId = webContext.Site.ID;
      var webId = webContext.ID;
      Task task = new Task(new Action(() =>
      {
        SPUtility.ValidateFormDigest();
        try
        {

          if (HttpContext.Current == null || SPContext.Current == null)
          {
            SPSite site = new SPSite(siteId);
            SPWeb web = site.OpenWeb(webId);
            HttpContext.Current = new HttpContext(context.Request, context.Response);
            HttpContext.Current.User = context.User;
            HttpContext.Current.Items["HttpHandlerSPWeb"] = web;

            //Just bump the current context.
            var threadLocalContext = SPContext.Current;
          }
          action();
        }
        catch(Exception ex)
        {
          ApplicationLog.AddException(ex);

          /* Do Nothing... */
        }
      }));

      task.Start();
      return task;
    }
  }
}
