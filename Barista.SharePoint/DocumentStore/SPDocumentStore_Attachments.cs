namespace Barista.SharePoint.DocumentStore
{
    using Barista.DocumentStore;
    using Microsoft.Office.DocumentManagement.DocumentSets;
    using Microsoft.SharePoint;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;

    public partial class SPDocumentStore
    {
        #region Attachments

        /// <summary>
        /// Uploads the attachment and associates it with the specified entity with the specified filename.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="attachment">The attachment.</param>
        /// <returns></returns>
        public virtual Attachment UploadAttachment(string containerTitle, Guid entityId, string fileName, byte[] attachment)
        {
            return UploadAttachment(containerTitle, entityId, fileName, attachment, String.Empty, string.Empty);
        }

        /// <summary>
        /// Uploads the attachment and associates it with the specified entity with the specified filename.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="attachment">The attachment.</param>
        /// <param name="category">The category.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public virtual Attachment UploadAttachment(string containerTitle, Guid entityId, string fileName, byte[] attachment, string category, string path)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            DocumentSet documentSet;
            if (SPDocumentStoreHelper.TryGetDocumentStoreEntityDocumentSet(list, folder, entityId, out documentSet) == false)
                return null;

            var attachmentContentTypeId = new SPContentTypeId(Constants.AttachmentDocumentContentTypeId);
            var listAttachmentContentTypeId = list.ContentTypes.BestMatch(attachmentContentTypeId);
            var attachmentContentType = list.ContentTypes[listAttachmentContentTypeId];

            if (attachmentContentType != null)
            {
                var properties = new Hashtable
        {
        {"ContentTypeId", attachmentContentType.Id.ToString()},
        {"Content Type", attachmentContentType.Name}
        };

                if (String.IsNullOrEmpty(category) == false)
                    properties.Add("Category", category);

                if (String.IsNullOrEmpty(path) == false)
                {
                    Uri pathUri;
                    if (Uri.TryCreate(path, UriKind.Relative, out pathUri) == false)
                    {
                        throw new InvalidOperationException("The optional Path parameter is not in the format of a path.");
                    }

                    properties.Add("AttachmentPath", path);
                }

                web.AllowUnsafeUpdates = true;
                try
                {
                    var attachmentFile = documentSet.Folder.Files.Add(fileName, attachment, properties, true);
                    return SPDocumentStoreHelper.MapAttachmentFromSPFile(attachmentFile);
                }
                finally
                {
                    web.AllowUnsafeUpdates = false;
                }
            }
            return null;
        }

        /// <summary>
        /// Uploads the attachment from the specified source URL and associates it with the specified entity.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="sourceUrl">The source URL.</param>
        /// <param name="category">The category.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public virtual Attachment UploadAttachmentFromSourceUrl(string containerTitle, Guid entityId, string fileName, string sourceUrl, string category, string path)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            var sourceUri = new Uri(sourceUrl);

            byte[] fileContents;

            //Get the content via a httpwebrequest and copy it with the same filename.
            HttpWebResponse webResponse = null;

            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                var webRequest = (HttpWebRequest)WebRequest.Create(sourceUri);
                webRequest.Timeout = 10000;
                webRequest.AllowWriteStreamBuffering = false;
                webResponse = (HttpWebResponse)webRequest.GetResponse();

                using (var s = webResponse.GetResponseStream())
                {
                    byte[] buffer = new byte[32768];
                    using (var ms = new MemoryStream())
                    {
                        int read;
                        while (s != null && (read = s.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, read);
                        }
                        fileContents = ms.ToArray();
                    }
                }
                webResponse.Close();
            }
            catch (Exception)
            {
                if (webResponse != null)
                    webResponse.Close();

                throw;
            }

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            DocumentSet documentSet;
            if (SPDocumentStoreHelper.TryGetDocumentStoreEntityDocumentSet(list, folder, entityId, out documentSet) == false)
                return null;

            var attachmentContentTypeId = new SPContentTypeId(Constants.AttachmentDocumentContentTypeId);
            var listAttachmentContentTypeId = list.ContentTypes.BestMatch(attachmentContentTypeId);
            var attachmentContentType = list.ContentTypes[listAttachmentContentTypeId];

            if (attachmentContentType != null)
            {
                var properties = new Hashtable
        {
        {"ContentTypeId", attachmentContentType.Id.ToString()},
        {"Content Type", attachmentContentType.Name}
        };

                if (String.IsNullOrEmpty(category) == false)
                    properties.Add("Category", category);

                if (String.IsNullOrEmpty(path) == false)
                    properties.Add("Path", path);

                web.AllowUnsafeUpdates = true;
                try
                {
                    var attachmentFile = documentSet.Folder.Files.Add(fileName, fileContents, properties, true);
                    return SPDocumentStoreHelper.MapAttachmentFromSPFile(attachmentFile);
                }
                finally
                {
                    web.AllowUnsafeUpdates = false;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets an attachment associated with the specified entity with the specified filename.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public virtual Attachment GetAttachment(string containerTitle, Guid entityId, string fileName)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            SPFile attachment;
            return SPDocumentStoreHelper.TryGetDocumentStoreAttachment(list, folder, entityId, fileName, out attachment) == false
                ? null
                : SPDocumentStoreHelper.MapAttachmentFromSPFile(attachment);
        }

        /// <summary>
        /// Renames the attachment with the specified filename to the new filename.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="newFileName">New name of the file.</param>
        /// <returns></returns>
        public virtual bool RenameAttachment(string containerTitle, Guid entityId, string fileName, string newFileName)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return false;

            SPFile attachment;
            if (SPDocumentStoreHelper.TryGetDocumentStoreAttachment(list, folder, entityId, fileName, out attachment) == false)
                return false;

            web.AllowUnsafeUpdates = true;
            try
            {
                attachment.Item["Name"] = newFileName;
                attachment.Item.Update();
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }

            return true;
        }

        /// <summary>
        /// Deletes the attachment from the document store.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public virtual bool DeleteAttachment(string containerTitle, Guid entityId, string fileName)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return false;

            SPFile attachment;
            if (SPDocumentStoreHelper.TryGetDocumentStoreAttachment(list, folder, entityId, fileName, out attachment) == false)
                return false;

            web.AllowUnsafeUpdates = true;
            try
            {
                attachment.Recycle();
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }

            return true;
        }

        /// <summary>
        /// Lists the attachments associated with the specified entity.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public virtual IList<Attachment> ListAttachments(string containerTitle, Guid entityId)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            DocumentSet documentSet;
            if (SPDocumentStoreHelper.TryGetDocumentStoreEntityDocumentSet(list, folder, entityId, out documentSet) == false)
                return null;

            var attachmentContentTypeId = new SPContentTypeId(Constants.AttachmentDocumentContentTypeId);
            var listAttachmentContentTypeId = list.ContentTypes.BestMatch(attachmentContentTypeId);
            var attachmentContentType = list.ContentTypes[listAttachmentContentTypeId];

            return documentSet.Folder.Files.OfType<SPFile>()
                .Where(f => attachmentContentType != null && f.Item.ContentTypeId == attachmentContentType.Id)
                .Select(f => SPDocumentStoreHelper.MapAttachmentFromSPFile(f))
                .ToList();
        }

        /// <summary>
        /// Downloads the attachment as a stream.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public virtual Stream DownloadAttachment(string containerTitle, Guid entityId, string fileName)
        {
            var result = new MemoryStream();

            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            SPFile attachment;
            if (SPDocumentStoreHelper.TryGetDocumentStoreAttachment(list, folder, entityId, fileName, out attachment) == false)
                return null;

            var attachmentStream = attachment.OpenBinaryStream();
            DocumentStoreHelper.CopyStream(attachmentStream, result);

            result.Seek(0, SeekOrigin.Begin);
            return result;
        }
        #endregion
    }
}
