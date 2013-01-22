﻿namespace Barista.SharePoint.DocumentStore
{
  using Barista.DocumentStore;
  using Microsoft.Office.DocumentManagement.DocumentSets;
  using Microsoft.Office.Server.Utilities;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Utilities;
  using Newtonsoft.Json;
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using System.Web;

  /// <summary>
  /// Contains methods that assist with the retrieval of Document Store objects.
  /// </summary>
  public static class SPDocumentStoreHelper
  {
    #region Mapping

    /// <summary>
    /// Returns an Attachment which represents the specified SPFile.
    /// </summary>
    /// <param name="file">The file to map from.</param>
    /// <returns>Attachment.</returns>
    public static Attachment MapAttachmentFromSPFile(SPFile file)
    {
      var result = new Attachment
        {
          Category = file.Item["Category"] as string,
          Path = file.Item["Path"] as string,
          ETag = file.ETag,
          FileName = file.Name,
          MimeType = StringHelper.GetMimeTypeFromFileName(file.Name),
          Size = file.Length,
          Url = file.Web.Url + "/" + file.Url,
          Created = (DateTime) file.Item[SPBuiltInFieldId.Created],
          Modified = (DateTime) file.Item[SPBuiltInFieldId.Modified]
        };

      var createdByUser = file.Item[SPBuiltInFieldId.Created_x0020_By] as SPFieldUserValue;
      if (createdByUser != null)
      {
        result.CreatedBy = new User
          {
            Email = createdByUser.User.Email,
            LoginName = createdByUser.User.LoginName,
            Name = createdByUser.User.Name,
          };
      }

      var modifiedByUser = file.Item[SPBuiltInFieldId.Modified_x0020_By] as SPFieldUserValue;
      if (modifiedByUser != null)
      {
        result.ModifiedBy = new User
          {
            Email = modifiedByUser.User.Email,
            LoginName = modifiedByUser.User.LoginName,
            Name = modifiedByUser.User.Name,
          };
      }

      return result;
    }

    /// <summary>
    /// Returns a comment which represents the first comment defined on the specified list item.
    /// </summary>
    /// <param name="listItem">The list item.</param>
    /// <returns>Comment.</returns>
    public static Comment MapCommentFromSPListItem(SPListItem listItem)
    {
      return
        MapCommentFromSPListItemVersion(
          listItem.Versions.OfType<SPListItemVersion>().OrderByDescending(v => v.Created).First());
    }

    public static Comment MapCommentFromSPListItemVersion(SPListItemVersion listItemVersion)
    {
      var result = new Comment
        {
          Id = listItemVersion.VersionId,
          CommentText = listItemVersion["Comments"] as string,
          Created = listItemVersion.Created,
          CreatedBy = new User
            {
              Email = listItemVersion.CreatedBy.User.Email,
              LoginName = listItemVersion.CreatedBy.User.LoginName,
              Name = listItemVersion.CreatedBy.User.Name,
            },
        };

      return result;
    }

    /// <summary>
    /// Returns a container which represents the SPList.
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public static Container MapContainerFromSPList(SPList list)
    {
      if (list == null)
        throw new ArgumentNullException("list");

      var result = new Container
        {
          Id = list.ID,
          Title = list.Title,
          Description = list.Description,
          EntityCount = list.ItemCount,
          Url = list.RootFolder.Url.Substring(list.RootFolder.Url.IndexOf('/') + 1),
          Created = list.Created,
          CreatedBy = new User
            {
              Email = list.Author.Email,
              LoginName = list.Author.LoginName,
              Name = list.Author.Name,
            },
          Modified = (DateTime) list.RootFolder.Properties["vti_timelastmodified"]
        };

      //TODO: ModifiedBy using the last item added to the list.
      return result;
    }

    /// <summary>
    /// Returns an entity that represents the specified list item.
    /// </summary>
    /// <param name="listItem"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static Entity MapEntityFromSPListItem(SPListItem listItem, string data)
    {
      if (listItem == null)
        throw new ArgumentNullException("listItem",
                                        @"When creating an Entity, the SPListItem that represents the entity must not be null.");

      //FIXME: Hmmm.. why did we go through the work of re-retrieving the file again instead of using the property???

      SPFile dataFile = null;

      try
      {
        dataFile = listItem.Web.GetFile(listItem.Folder.Url + "/" + Constants.DocumentStoreDefaultEntityPartFileName);
      }
      catch (Exception)
      {
        /* Do Nothing... */
      }

      if (dataFile == null || dataFile.Exists == false) //The default entity part file doesn't exist, get outta dodge (something happened here...)
        throw new InvalidOperationException(
          "No corresponding entity file exists on the document set that represents the entity.");

      return MapEntityFromSPListItem(listItem, dataFile, data);
    }

    /// <summary>
    /// Returns an entity that represents the specified list item.
    /// </summary>
    /// <param name="listItem"></param>
    /// <param name="file"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static Entity MapEntityFromSPListItem(SPListItem listItem, SPFile file, string data)
    {
      if (listItem == null)
        throw new ArgumentNullException("listItem");

      if (file == null)
        throw new ArgumentNullException("file");

      var entity = new Entity();

      try
      {
        var id = listItem[Constants.DocumentEntityGuidFieldId] as string;

        if (id != null)
          entity.Id = new Guid(id);
      }
      catch
      {
        //Do Nothing...
      }

      entity.Namespace = listItem[Constants.NamespaceFieldId] as string;

      var docSet = DocumentSet.GetDocumentSet(file.ParentFolder);

      entity.ETag = file.ETag;
      entity.Title = docSet.Item.Title;
      entity.Description = docSet.Item["DocumentSetDescription"] as string;
      entity.Created = (DateTime)docSet.Item[SPBuiltInFieldId.Created];
      entity.Modified = (DateTime)docSet.Item[SPBuiltInFieldId.Modified];

      entity.ContentsETag = docSet.Item["DocumentEntityContentsHash"] as string;

      if (docSet.Item["DocumentEntityContentsLastModified"] != null)
      {
        entity.ContentsModified = (DateTime)docSet.Item["DocumentEntityContentsLastModified"];
      }

      entity.Path = docSet.ParentFolder.Url.Substring(listItem.ParentList.RootFolder.Url.Length);
      entity.Path = entity.Path.TrimStart('/');

      entity.Data = data ?? Encoding.UTF8.GetString(file.OpenBinary());

      var createdByUserValue = listItem[SPBuiltInFieldId.Author] as String;
      var createdByUser = new SPFieldUserValue(listItem.Web, createdByUserValue);

      entity.CreatedBy = new User
      {
        Email = createdByUser.User.Email,
        LoginName = createdByUser.User.LoginName,
        Name = createdByUser.User.Name,
      };

      var modifiedByUser = new SPFieldUserValue(listItem.Web, createdByUserValue);

      entity.ModifiedBy = new User
      {
        Email = modifiedByUser.User.Email,
        LoginName = modifiedByUser.User.LoginName,
        Name = modifiedByUser.User.Name,
      };

      return entity;
    }

    /// <summary>
    /// Maps the entity from SP list item version.
    /// </summary>
    /// <param name="version">The version.</param>
    /// <returns>Entity.</returns>
    public static Entity MapEntityFromSPListItemVersion(SPListItemVersion version)
    {
      var result = new Entity();
      try
      {
        string id = version[version.ListItem.Fields[Constants.DocumentEntityGuidFieldId].Title] as string;

        if (id != null)
          result.Id = new Guid(id);
      }
      catch
      {
        //Do Nothing...
      }

      result.Namespace = version[version.ListItem.Fields[Constants.NamespaceFieldId].Title] as string;

      result.Title = version[version.ListItem.Fields[SPBuiltInFieldId.Title].Title] as string;
      result.Description = version["DocumentSetDescription"] as string;
      result.Created = (DateTime) version[version.ListItem.Fields[SPBuiltInFieldId.Created].Title];
      result.Modified = (DateTime) version[version.ListItem.Fields[SPBuiltInFieldId.Modified].Title];

      result.Path = version.ListItem.File.ParentFolder.Url.Substring(version.ListItem.ParentList.RootFolder.Url.Length);
      result.Path = result.Path.TrimStart('/');

      var createdByUserValue = version[version.ListItem.Fields[SPBuiltInFieldId.Author].Title] as string;
      var createdByUser = new SPFieldUserValue(version.ListItem.Web, createdByUserValue);

      result.CreatedBy = new User
        {
          Email = createdByUser.User.Email,
          LoginName = createdByUser.User.LoginName,
          Name = createdByUser.User.Name,
        };

      var modifiedByUser = new SPFieldUserValue(version.ListItem.Web, createdByUserValue);

      result.ModifiedBy = new User
        {
          Email = modifiedByUser.User.Email,
          LoginName = modifiedByUser.User.LoginName,
          Name = modifiedByUser.User.Name,
        };

      if (version.IsCurrentVersion)
      {
        var dataFile = version.ListItem.File;
        result.ETag = dataFile.ETag;
        using (var reader = new StreamReader(dataFile.OpenBinaryStream()))
        {
          result.Data = reader.ReadToEnd();
        }
      }
      else
      {
        var dataFile = version.ListItem.File.Versions.GetVersionFromID(version.VersionId);
        //result.ETag = dataFile.Properties["ETag"];
        using (var reader = new StreamReader(dataFile.OpenBinaryStream()))
        {
          result.Data = reader.ReadToEnd();
        }
      }
      return result;
    }

    /// <summary>
    /// Maps an SPFile to an Entity Part. If the data parameter is null, the file is opened and the data retrieved.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static EntityPart MapEntityPartFromSPFile(SPFile file, string data)
    {
      if (file == null)
        throw new ArgumentNullException("file",
                                        @"When creating an EntityPart, the SPFile that represents the entity part must not be null.");

      var entityPart = new EntityPart();
      try
      {
        string id = file.Item[Constants.DocumentEntityGuidFieldId] as string;

        if (id != null)
          entityPart.EntityId = new Guid(id);
      }
      catch
      {
        //Do Nothing...
      }

      entityPart.Category = file.Item["Category"] as string;
      entityPart.ETag = file.ETag;
      entityPart.Name = file.Name.Substring(0, file.Name.Length - Constants.DocumentSetEntityPartExtension.Length);
      entityPart.Created = (DateTime) file.Item[SPBuiltInFieldId.Created];
      entityPart.Modified = (DateTime) file.Item[SPBuiltInFieldId.Modified];

      entityPart.Data = data ?? Encoding.UTF8.GetString(file.OpenBinary());

      var createdByUserValue = file.Item[SPBuiltInFieldId.Author] as string;
      var createdByUser = new SPFieldUserValue(file.Web, createdByUserValue);

      entityPart.CreatedBy = new User
        {
          Email = createdByUser.User.Email,
          LoginName = createdByUser.User.LoginName,
          Name = createdByUser.User.Name,
        };

      var modifiedByUser = new SPFieldUserValue(file.Web, createdByUserValue);

      entityPart.ModifiedBy = new User
        {
          Email = modifiedByUser.User.Email,
          LoginName = modifiedByUser.User.LoginName,
          Name = modifiedByUser.User.Name,
        };

      return entityPart;
    }

    /// <summary>
    /// Returns a folder which represents the specified SPFolder.
    /// </summary>
    /// <param name="folder"></param>
    /// <returns></returns>
    public static Folder MapFolderFromSPFolder(SPFolder folder)
    {
      if (folder == null)
        throw new ArgumentNullException("folder");

      var rootFolderUrl = folder.ParentWeb.Lists[folder.ParentListId].RootFolder.Url;
      var folderListItem = folder.Item ?? folder.ParentWeb.GetFolder(folder.Url).Item;

      var result = new Folder
        {
          Name = folder.Name,
          EntityCount = folder.ItemCount,
        };

      if (folder.Url == rootFolderUrl)
      {
        var list = folder.ParentWeb.Lists[folder.ParentListId];
        result.FullPath = "";
        result.Created = list.Created;
        result.Modified = list.LastItemModifiedDate;

        var createdByUser = list.Author;

        result.CreatedBy = new User
        {
          Email = createdByUser.Email,
          LoginName = createdByUser.LoginName,
          Name = createdByUser.Name,
        };

        var modifiedByUserValue = list.RootFolder.Properties["vti_modifiedby"] as string;

        if (String.Compare(modifiedByUserValue, "SHAREPOINT\\system", StringComparison.InvariantCultureIgnoreCase) != 0)
        {
          var modifiedByUser = new SPFieldUserValue(folder.ParentWeb, modifiedByUserValue);

          result.ModifiedBy = new User
            {
              Email = modifiedByUser.User.Email,
              LoginName = modifiedByUser.User.LoginName,
              Name = modifiedByUser.User.Name,
            };
        }
      }
      else
      {
        result.FullPath = folder.Url.Substring(rootFolderUrl.Length + 1);
        result.Created = (DateTime) folderListItem[SPBuiltInFieldId.Created];
        result.Modified = (DateTime) folderListItem[SPBuiltInFieldId.Modified];

        var createdByUserValue = folderListItem[SPBuiltInFieldId.Author] as String;
        var createdByUser = new SPFieldUserValue(folder.ParentWeb, createdByUserValue);

        result.CreatedBy = new User
        {
          Email = createdByUser.User.Email,
          LoginName = createdByUser.User.LoginName,
          Name = createdByUser.User.Name,
        };

        var modifiedByUserValue = folderListItem[SPBuiltInFieldId.Editor] as String;
        var modifiedByUser = new SPFieldUserValue(folder.ParentWeb, modifiedByUserValue);

        result.ModifiedBy = new User
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

      var result = new List<SPDocumentLibrary>();

      var listsIterator = new ContentIterator();
      listsIterator.ProcessLists(web.Lists, currentList =>
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
    public static bool TryGetFolderFromPath(SPWeb web, string containerTitle, out SPList list, out SPFolder folder,
                                            string path)
    {
      if (web == null)
        throw new ArgumentNullException("web");

      if (containerTitle == null)
        throw new ArgumentNullException("containerTitle");

      if (path == null)
        path = String.Empty;

      list = web.Lists.TryGetList(containerTitle);

      if ((list == null) || (list.TemplateFeatureId != Constants.DocumentContainerFeatureId))
      {
        list = null;
        folder = null;
        return false;
      }

      SPFolder currentFolder;
      try
      {
        currentFolder = web.GetFolder(SPUtility.ConcatUrls(list.RootFolder.Url, path));
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
      SPFolder folder;
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
    public static bool TryGetDocumentStoreDefaultEntityPart(SPList list, SPFolder folder, Guid id,
                                                            out SPFile defaultEntityPart)
    {
      defaultEntityPart = SPDocumentStoreHelper.GetDocumentStoreDefaultEntityPartForGuid(list, folder, id);
      return defaultEntityPart != null;
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
      //Attempt to prevent having to do a full SPQuery by attempting to retrieve the file from the sub folder. (Performance)
      var documentStoreEntityContentTypeId = new SPContentTypeId(Constants.DocumentStoreEntityContentTypeId);
      var documentStoreEntityPartContentTypeId = new SPContentTypeId(Constants.DocumentStoreEntityPartContentTypeId);
      SPFolder childFolder = null;
      try
      {
        childFolder = list.ParentWeb.GetFolder(folder.Url + "/" + id);
      }
      catch (ArgumentException)
      {
        /* Do Nothing */
      }

      if (childFolder != null && childFolder.Exists &&
          childFolder.Item.ContentTypeId.IsChildOf(documentStoreEntityContentTypeId))
      {
        SPFile childFile = null;
        try
        {
          childFile =
            list.ParentWeb.GetFile(folder.Url + "/" + id + "/" +
                                   Constants.DocumentStoreDefaultEntityPartFileName);
        }
        catch (ArgumentException)
        {
          /* Do Nothing */
        }

        if (childFile != null && childFile.Exists &&
            childFile.Item.ContentTypeId.IsChildOf(documentStoreEntityPartContentTypeId))
          return childFile;
      }

      //If we weren't able to retrieve the SPFile directly, use a SPQuery to recursively obtain the file (should be more efficient than a recursive function)
      SPQuery query = new SPQuery
        {
          Folder = folder,
          Query = @"<Where><And><And>
                        <BeginsWith><FieldRef Name=""ContentTypeId""/><Value Type=""Text"">" +
                  Constants.DocumentStoreEntityPartContentTypeId
                           .ToUpperInvariant() + @"</Value></BeginsWith>
                        <Eq><FieldRef Name=""DocumentEntityGuid"" /><Value Type=""Text"">" + id.ToString() +
                  @"</Value></Eq>
                      </And>
                        <Eq><FieldRef Name=""FileLeafRef""/><Value Type=""Text"">" +
                  Constants.DocumentStoreDefaultEntityPartFileName +
                  @"</Value></Eq>
                      </And></Where>",
          RowLimit = 1,
          QueryThrottleMode = SPQueryThrottleOption.Override,
          ViewAttributes = "Scope=\"Recursive\""
        };

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

      return item != null
        ? item.File
        : null;
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
    public static bool TryGetDocumentStoreEntityPart(SPList list, SPFolder folder, Guid id, string partName,
                                                     out SPFile entityPart)
    {
      entityPart = SPDocumentStoreHelper.GetDocumentStoreEntityPart(list, folder, id, partName);
      return entityPart != null;
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
      //Attempt to prevent having to do a full SPQuery by attempting to retrieve the file from the sub folder. (Performance)
      var documentStoreEntityContentTypeId = new SPContentTypeId(Constants.DocumentStoreEntityContentTypeId);
      var documentStoreEntityPartContentTypeId = new SPContentTypeId(Constants.DocumentStoreEntityPartContentTypeId);
      SPFolder childFolder = null;
      try
      {
        childFolder = list.ParentWeb.GetFolder(folder.Url + "/" + id.ToString());
      }
      catch (ArgumentException)
      {
        /* Do Nothing */
      }

      if (childFolder != null && childFolder.Exists &&
          childFolder.Item.ContentTypeId.IsChildOf(documentStoreEntityContentTypeId))
      {
        SPFile childFile = null;
        try
        {
          childFile =
            list.ParentWeb.GetFile(folder.Url + "/" + id.ToString() + "/" + partName +
                                   Constants.DocumentSetEntityPartExtension);
        }
        catch (ArgumentException)
        {
          /* Do Nothing */
        }

        if (childFile != null && childFile.Exists &&
            childFile.Item.ContentTypeId.IsChildOf(documentStoreEntityPartContentTypeId))
          return childFile;
      }

      SPQuery query = new SPQuery
        {
          Folder = folder,
          Query = @"<Where><And><And>
                        <BeginsWith><FieldRef Name=""ContentTypeId""/><Value Type=""Text"">" +
                  Constants.DocumentStoreEntityPartContentTypeId
                           .ToUpperInvariant() + @"</Value></BeginsWith>
                        <Eq><FieldRef Name=""DocumentEntityGuid"" /><Value Type=""Text"">" + id.ToString() +
                  @"</Value></Eq>
                      </And>
                        <Eq><FieldRef Name=""FileLeafRef""/><Value Type=""Text"">" + partName +
                  Constants.DocumentSetEntityPartExtension + @"</Value></Eq>
                      </And></Where>",
          RowLimit = 1,
          ViewAttributes = "Scope=\"Recursive\""
        };

      //SPFile dataFile = null;
      //ContentIterator itemsIterator = new ContentIterator();
      //itemsIterator.ProcessListItems(list, query, false, (spListItem) =>
      //{
      //  dataFile = spListItem.File;
      //  itemsIterator.Cancel = true;
      //}, null);

      //return dataFile;

      var item = list.GetItems(query).OfType<SPListItem>().FirstOrDefault();

      return item != null
        ? item.File
        : null;
    }

    /// <summary>
    /// Tries to the get document store attachment.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <param name="defaultEntityPart">The default entity part.</param>
    /// <param name="attachmentFileName">Name of the attachment file.</param>
    /// <param name="attachment">The attachment.</param>
    /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
    public static bool TryGetDocumentStoreAttachment(SPList list, SPFile defaultEntityPart, string attachmentFileName,
                                                     out SPFile attachment)
    {
      //TODO: Possibly SPQuerify this.
      var attachmentContentType =
        list.ContentTypes.OfType<SPContentType>()
            .FirstOrDefault(
              ct =>
              ct.Id.ToString()
                .ToLowerInvariant()
                .StartsWith(Constants.AttachmentDocumentContentTypeId.ToLowerInvariant()));

      attachment =
        defaultEntityPart.ParentFolder.Files.OfType<SPFile>()
                         .FirstOrDefault(
                           f =>
                           attachmentContentType != null &&
                           (f.Name == attachmentFileName && f.Item.ContentTypeId.IsChildOf(attachmentContentType.Id)));
      if (attachment == null)
        return false;

      return true;
    }

    /// <summary>
    /// Executes the specified action asynchronously, while preserving both the HttpContext and SPContext
    /// </summary>
    public static Task ExecuteAsync(Repository repository, HttpContext context, SPWeb webContext, Action action)
    {
      var siteId = webContext.Site.ID;
      var webId = webContext.ID;
      var task = new Task(() =>
        {
          SPUtility.ValidateFormDigest();
          try
          {
            if (HttpContext.Current == null || SPContext.Current == null)
            {
              SPSite site = new SPSite(siteId);
              SPWeb web = site.OpenWeb(webId);
              HttpContext.Current = new HttpContext(context.Request, context.Response) {User = context.User};
              HttpContext.Current.Items["HttpHandlerSPWeb"] = web;

              //Just bump the current context.
#pragma warning disable 168
              var threadLocalContext = SPContext.Current;
#pragma warning restore 168
            }
            action();
          }
          catch (Exception ex)
          {
            ApplicationLog.AddException(repository, ex);

            /* Do Nothing... */
          }
        });

      task.Start();
      return task;
    }


    /// <summary>
    /// Gets the hash of the specified Entities' contents.
    /// </summary>
    /// <param name="web"></param>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <returns></returns>
    public static string GetEntityContentsHash(SPWeb web, string containerTitle, Guid entityId)
    {
      SPList list;
      SPFolder folder;
      if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
        return null;

      SPFile defaultEntityPart;
      if (SPDocumentStoreHelper.TryGetDocumentStoreDefaultEntityPart(list, folder, entityId, out defaultEntityPart) ==
          false)
        return null;

      return defaultEntityPart.ParentFolder.Item["DocumentEntityContentsHash"] as string;
    }

    /// <summary>
    /// Gets the ETag of the specified entity part
    /// </summary>
    /// <param name="web"></param>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="partName"></param>
    /// <returns></returns>
    public static string GetEntityPartETag(SPWeb web, string containerTitle, Guid entityId, string partName)
    {
      SPList list;
      SPFolder folder;
      if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
        return null;

      SPFile entityPart;
      if (SPDocumentStoreHelper.TryGetDocumentStoreEntityPart(list, folder, entityId, partName, out entityPart) == false)
        return null;

      var result = entityPart.ETag;
      return result;
    }

    /// <summary>
    /// Removes the specified key from the content entity part for the specified document set folder.
    /// </summary>
    /// <param name="web"></param>
    /// <param name="list"></param>
    /// <param name="documentSetFolder"></param>
    /// <param name="key"></param>
    /// <param name="contentHash"></param>
    /// <returns></returns>
    public static string RemoveContentEntityPartKeyValue(SPWeb web, SPList list, SPFolder documentSetFolder, string key,
                                                         out string contentHash)
    {
      if (web == null)
        throw new ArgumentNullException("web",
                                        @"The web argument must be specified and contain the web that contains the document set folder.");

      if (list == null)
        throw new ArgumentNullException("list",
                                        @"The list argument must be specified and contain the list that contains the document set folder.");

      if (documentSetFolder == null)
        throw new ArgumentNullException("documentSetFolder",
                                        @"The document set folder argument must be specified and contains the folder object of the document set which represents a document store entity.");

      var entityPartContentType = list.ContentTypes.OfType<SPContentType>()
                                      .FirstOrDefault(
                                        ct =>
                                        ct.Id.ToString()
                                          .ToLowerInvariant()
                                          .StartsWith(
                                            Constants.DocumentStoreEntityPartContentTypeId.ToLowerInvariant()));

      if (entityPartContentType == null)
        throw new InvalidOperationException("Unable to locate the entity part content type");

      var contentFile =
        documentSetFolder.Files.OfType<SPFile>()
                         .FirstOrDefault(f => f.Name == Constants.DocumentStoreEntityContentsPartFileName);

      if (contentFile == null || contentFile.Exists == false)
      {
        contentHash = String.Empty;
        return String.Empty;
      }

      var contentData = contentFile.OpenBinary();
      var contentJson = System.Text.Encoding.UTF8.GetString(contentData);
      Dictionary<string, EntityPart> contentDictionary =
        JsonConvert.DeserializeObject<Dictionary<string, EntityPart>>(contentJson);

      if (contentDictionary.ContainsKey(key) == false)
      {
        var unmodifiedContent = JsonConvert.SerializeObject(contentDictionary);
        contentHash = StringHelper.CreateMD5Hash(unmodifiedContent);
        return unmodifiedContent;
      }

      contentDictionary.Remove(key);

      var content = JsonConvert.SerializeObject(contentDictionary);
      contentHash = StringHelper.CreateMD5Hash(content);

      web.AllowUnsafeUpdates = true;

      try
      {
        contentFile.SaveBinary(System.Text.Encoding.Default.GetBytes(content));
      }
      finally
      {
        web.AllowUnsafeUpdates = false;
      }

      return content;
    }

    /// <summary>
    /// Returns a value that indicates if the specified document set folder contains a content entity part.
    /// </summary>
    /// <param name="documentSetFolder">The document set folder.</param>
    /// <returns><c>true</c> if [has content entity part] [the specified document set folder]; otherwise, <c>false</c>.</returns>
    /// <exception cref="System.ArgumentNullException">documentSetFolder;@The document set folder argument must be specified and contains the folder object of the document set which represents a document store entity.</exception>
    public static bool HasContentEntityPart(SPFolder documentSetFolder)
    {
      if (documentSetFolder == null)
        throw new ArgumentNullException("documentSetFolder",
                                        @"The document set folder argument must be specified and contains the folder object of the document set which represents a document store entity.");

      var contentFile =
        documentSetFolder.Files.OfType<SPFile>()
                         .FirstOrDefault(f => f.Name == Constants.DocumentStoreEntityContentsPartFileName);

      return (contentFile != null);
    }

    /// <summary>
    /// Gets the entity contents entity part.
    /// </summary>
    /// <param name="web">The web.</param>
    /// <param name="list">The list.</param>
    /// <param name="documentSetFolder">The document set folder.</param>
    /// <returns>EntityContents.</returns>
    /// <exception cref="System.ArgumentNullException">web;@The web argument must be specified and contain the web that contains the document set folder.</exception>
    /// <exception cref="System.InvalidOperationException">Unable to locate the entity part content type</exception>
    public static EntityContents GetEntityContentsEntityPart(SPWeb web, SPList list, SPFolder documentSetFolder)
    {
      if (web == null)
        throw new ArgumentNullException("web",
                                        @"The web argument must be specified and contain the web that contains the document set folder.");

      if (list == null)
        throw new ArgumentNullException("list",
                                        @"The list argument must be specified and contain the list that contains the document set folder.");

      if (documentSetFolder == null)
        throw new ArgumentNullException("documentSetFolder",
                                        @"The document set folder argument must be specified and contains the folder object of the document set which represents a document store entity.");

      var entityPartContentType = list.ContentTypes.OfType<SPContentType>()
                                      .FirstOrDefault(
                                        ct =>
                                        ct.Id.ToString()
                                          .ToLowerInvariant()
                                          .StartsWith(
                                            Constants.DocumentStoreEntityPartContentTypeId.ToLowerInvariant()));

      if (entityPartContentType == null)
        throw new InvalidOperationException("Unable to locate the entity part content type");

      var contentFile =
        documentSetFolder.Files.OfType<SPFile>()
                         .FirstOrDefault(f => f.Name == Constants.DocumentStoreEntityContentsPartFileName);

      if (contentFile == null)
        return null;

      var contentData = contentFile.OpenBinary();
      var contentJson = System.Text.Encoding.UTF8.GetString(contentData);
      var entityContents = JsonConvert.DeserializeObject<EntityContents>(contentJson);

      return entityContents;
    }

    /// <summary>
    /// Creates or updates the content entity part for the specified document set folder, optionally using the entity part to update.
    /// </summary>
    /// <param name="web"></param>
    /// <param name="list"></param>
    /// <param name="documentSetFolder"></param>
    /// <param name="updatedEntity"></param>
    /// <param name="updatedEntityPart"></param>
    /// <param name="contentHash"></param>
    /// <param name="contentModified"></param>
    /// <returns></returns>
    public static string CreateOrUpdateContentEntityPart(SPWeb web, SPList list, SPFolder documentSetFolder,
                                                         Entity updatedEntity, EntityPart updatedEntityPart,
                                                         out string contentHash, out DateTime contentModified)
    {
      if (web == null)
        throw new ArgumentNullException("web",
                                        @"The web argument must be specified and contain the web that contains the document set folder.");

      if (list == null)
        throw new ArgumentNullException("list",
                                        @"The list argument must be specified and contain the list that contains the document set folder.");

      if (documentSetFolder == null)
        throw new ArgumentNullException("documentSetFolder",
                                        @"The document set folder argument must be specified and contains the folder object of the document set which represents a document store entity.");

      var entityPartContentType = list.ContentTypes.OfType<SPContentType>()
                                      .FirstOrDefault(
                                        ct =>
                                        ct.Id.ToString()
                                          .ToLowerInvariant()
                                          .StartsWith(
                                            Constants.DocumentStoreEntityPartContentTypeId.ToLowerInvariant()));

      if (entityPartContentType == null)
        throw new InvalidOperationException("Unable to locate the entity part content type");

      EntityContents entityContents;

      var contentFile =
        documentSetFolder.Files.OfType<SPFile>()
                         .FirstOrDefault(f => f.Name == Constants.DocumentStoreEntityContentsPartFileName);

      //If the content file does not exist, create a new content dictionary object
      //that contains all the entity parts contained in the document store entity
      //Except for the content part and, if specified, the updated entity part.
      //Otherwise, read the content file and update the entity part.
      if (contentFile == null)
      {
        var entityParts = documentSetFolder.Files.OfType<SPFile>()
                                           .Where(f => f.Item.ContentTypeId == entityPartContentType.Id &&
                                                       f.Name != Constants.DocumentStoreDefaultEntityPartFileName &&
                                                       f.Name != Constants.DocumentStoreEntityContentsPartFileName &&
                                                       (updatedEntityPart == null || f.Name != updatedEntityPart.Name + Constants.DocumentSetEntityPartExtension))
                                           .Select(f => SPDocumentStoreHelper.MapEntityPartFromSPFile(f, null))
                                           .ToList();

        entityContents = new EntityContents
          {
            Entity = SPDocumentStoreHelper.MapEntityFromSPListItem(documentSetFolder.Item, null),
            EntityParts = entityParts.ToDictionary(entityPart => entityPart.Name)
          };
      }
      else
      {
        var contentData = contentFile.OpenBinary();
        var contentJson = System.Text.Encoding.UTF8.GetString(contentData);
        entityContents = JsonConvert.DeserializeObject<EntityContents>(contentJson);
      }

      if (updatedEntity != null)
        entityContents.Entity = updatedEntity;

      if (updatedEntityPart != null)
      {
        if (entityContents.EntityParts.ContainsKey(updatedEntityPart.Name))
          entityContents.EntityParts[updatedEntityPart.Name] = updatedEntityPart;
        else
          entityContents.EntityParts.Add(updatedEntityPart.Name, updatedEntityPart);
      }

      var content = JsonConvert.SerializeObject(entityContents);
      contentHash = StringHelper.CreateMD5Hash(content);
      contentModified = DateTime.Now;

      //Update the hash on the entity to match what 'will' be the current.
      entityContents.Entity.ContentsETag = contentHash;
      entityContents.Entity.ContentsModified = contentModified;
      content = JsonConvert.SerializeObject(entityContents);

      //Perform the update.

      if (documentSetFolder.Item.DoesUserHavePermissions(SPBasePermissions.EditListItems) == false)
        throw new InvalidOperationException("Insufficient Permissions.");

      var originalAllowUnsafeUpdates = web.AllowUnsafeUpdates;

      web.AllowUnsafeUpdates = true;

      try
      {
        if (contentFile == null)
        {
          var properties = new Hashtable
            {
              {"ContentTypeId", entityPartContentType.Id.ToString()},
              {"Content Type", entityPartContentType.Name}
            };
          documentSetFolder.Files.Add(Constants.DocumentStoreEntityContentsPartFileName,
                                      System.Text.Encoding.Default.GetBytes(content), properties, true);
        }
        else
        {
          contentFile.SaveBinary(System.Text.Encoding.Default.GetBytes(content));
        }
      }
      finally
      {
        web.AllowUnsafeUpdates = originalAllowUnsafeUpdates;
      }

      return content;
    }
  }
}