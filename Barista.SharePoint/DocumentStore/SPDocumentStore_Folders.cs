namespace Barista.SharePoint.DocumentStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Barista.DocumentStore;
    using Microsoft.Office.Server.Utilities;
    using Microsoft.SharePoint;

    public partial class SPDocumentStore
    {
        #region Folders

        /// <summary>
        /// Creates the folder with the specified path in the container.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public virtual Folder CreateFolder(string containerTitle, string path)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            var currentFolder = CreateFolderInternal(web, containerTitle, path);

            return SPDocumentStoreHelper.MapFolderFromSPFolder(currentFolder);
        }

        internal SPFolder CreateFolderInternal(SPWeb web, string containerTitle, string path)
        {
            SPList list;
            if (SPDocumentStoreHelper.TryGetListForContainer(web, containerTitle, out list) == false)
                throw new InvalidOperationException("A container with the specified title does not exist: " + web.Url + " " + containerTitle);

            var currentFolder = list.RootFolder;
            web.AllowUnsafeUpdates = true;

            try
            {
                foreach (var subFolder in DocumentStoreHelper.GetPathSegments(path))
                {
                    if (currentFolder.SubFolders.OfType<SPFolder>().Any(sf => sf.Name == subFolder) == false)
                    {
                        var folder = currentFolder.SubFolders.Add(subFolder);

                        //Ensure that the content type is, well, a folder and not a doc set. :-0
                        var folderListContentType = list.ContentTypes.BestMatch(SPBuiltInContentTypeId.Folder);
                        if (folder.Item.ContentTypeId != folderListContentType)
                        {
                            folder.Item["ContentTypeId"] = folderListContentType;
                            folder.Item.Update();
                        }
                    }
                    currentFolder = currentFolder.SubFolders[subFolder];
                }
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }

            return currentFolder;
        }

        /// <summary>
        /// Gets the folder with the specified path that is contained in the container.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public virtual Folder GetFolder(string containerTitle, string path)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            return SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, path) == false
                ? null
                : SPDocumentStoreHelper.MapFolderFromSPFolder(folder);
        }

        /// <summary>
        /// Renames the specified folder to the new folder name.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="path">The path.</param>
        /// <param name="newFolderName">New name of the folder.</param>
        /// <returns></returns>
        public virtual Folder RenameFolder(string containerTitle, string path, string newFolderName)
        {
            //Get a new web in case we're executing in elevated permissions.
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, path) == false)
                return null;

            web.AllowUnsafeUpdates = true;
            try
            {
                folder.Item["Name"] = newFolderName;
                folder.Item.Update();

                folder = list.GetItemByUniqueId(folder.UniqueId).Folder;

                return SPDocumentStoreHelper.MapFolderFromSPFolder(folder);
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }
        }

        /// <summary>
        /// Deletes the folder with the specified path from the container.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="path">The path.</param>
        public virtual void DeleteFolder(string containerTitle, string path)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, path) == false)
                return;

            web.AllowUnsafeUpdates = true;
            try
            {
                folder.Recycle();
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }
        }

        /// <summary>
        /// Lists the folders at the specified level in the container.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public virtual IList<Folder> ListFolders(string containerTitle, string path)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, path) == false)
                return null;

            ContentIterator.EnsureContentTypeIndexed(list);

            var folderContentTypeId = list.ContentTypes.BestMatch(SPBuiltInContentTypeId.Folder);

            var camlQuery = @"<Where>
    <Eq>
      <FieldRef Name=""ContentTypeId"" />
      <Value Type=""ContentTypeId"">" + folderContentTypeId + @"</Value>
    </Eq>
</Where>";

            var result = new List<Folder>();
            var itemsIterator = new ContentIterator();
            itemsIterator.ProcessListItems(list, camlQuery + ContentIterator.ItemEnumerationOrderByPath, ContentIterator.MaxItemsPerQuery, false, folder,
                                            spListItems =>
                                            {
                                                // ReSharper disable ConvertToLambdaExpression
                                                result.AddRange(spListItems.OfType<SPListItem>()
                                                                            .Select(
                                                                            spListItem =>
                                                                            SPDocumentStoreHelper.MapFolderFromSPFolder(
                                                                                spListItem.Folder)));
                                                // ReSharper restore ConvertToLambdaExpression

                                            },
                                            null);

            return result;
        }

        /// <summary>
        /// Lists all folders contained in the specified container.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public virtual IList<Folder> ListAllFolders(string containerTitle, string path)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, path) == false)
                return null;

            ContentIterator.EnsureContentTypeIndexed(list);

            var folderContentTypeId = list.ContentTypes.BestMatch(SPBuiltInContentTypeId.Folder);

            var camlQuery = @"<Where>
    <Eq>
      <FieldRef Name=""ContentTypeId"" />
      <Value Type=""ContentTypeId"">" + folderContentTypeId + @"</Value>
    </Eq>
</Where>";

            var result = new List<Folder>();
            var itemsIterator = new ContentIterator();
            itemsIterator.ProcessListItems(list, camlQuery + ContentIterator.ItemEnumerationOrderByPath, ContentIterator.MaxItemsPerQuery, false, folder,
                                            spListItems =>
                                            {
                                                // ReSharper disable ConvertToLambdaExpression
                                                result.AddRange(spListItems.OfType<SPListItem>()
                                                                            .Select(
                                                                            spListItem =>
                                                                            SPDocumentStoreHelper.MapFolderFromSPFolder(
                                                                                spListItem.Folder)));
                                                // ReSharper restore ConvertToLambdaExpression

                                            },
                                            null);

            return result;
        }
        #endregion
    }
}
