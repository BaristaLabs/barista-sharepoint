namespace Barista.SharePoint.DocumentStore
{
    using Barista.DocumentStore;
    using Microsoft.SharePoint;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class SPDocumentStore
    {
        #region Comments
        /// <summary>
        /// Adds a comment to the specified entity.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="comment">The comment.</param>
        /// <returns></returns>
        public virtual Comment AddEntityComment(string containerTitle, Guid entityId, string comment)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPFile defaultEntityPart;
            SPList list;
            SPFolder folder;
            if (
                SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) ==
                false)
                return null;

            if (
                SPDocumentStoreHelper.TryGetDocumentStoreDefaultEntityPart(list, folder, entityId,
                    out defaultEntityPart) == false)
                return null;

            web.AllowUnsafeUpdates = true;
            try
            {
                defaultEntityPart.Item[Constants.CommentFieldId] = comment;
                defaultEntityPart.Item.Update();

                return SPDocumentStoreHelper.MapCommentFromSPListItem(defaultEntityPart.Item);
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }
        }

        /// <summary>
        /// Lists the comments associated with the specified entity.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public virtual IList<Comment> ListEntityComments(string containerTitle, Guid entityId)
        {
            return ListEntityComments(containerTitle, entityId, String.Empty);
        }

        public virtual IList<Comment> ListEntityComments(string containerTitle, Guid entityId, string path)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPFile defaultEntityPart;
            if (!SPDocumentStoreHelper.TryGetDocumentStoreDefaultEntityPartDirect(web, containerTitle, path, entityId, out defaultEntityPart))
            {
                SPList list;
                SPFolder folder;
                if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, path) == false)
                    return null;

                if (SPDocumentStoreHelper.TryGetDocumentStoreDefaultEntityPart(list, folder, entityId,
                        out defaultEntityPart) == false)
                    return null;
            }

            var result = new List<Comment>();
            Comment[] lastComment =
                {
                    new Comment
                    {
                        CommentText = null
                    }
                };

            var versions = defaultEntityPart.Item.Versions;
            foreach (var itemVersion in versions.OfType<SPListItemVersion>()
                .OrderBy(v => Double.Parse(v.VersionLabel))
                .Where(itemVersion => String.CompareOrdinal(itemVersion["Comments"] as string, lastComment[0].CommentText) != 0))
            {
                lastComment[0] = SPDocumentStoreHelper.MapCommentFromSPListItemVersion(itemVersion);
                result.Add(lastComment[0]);
            }

            result.Reverse();
            return result;
        }

        /// <summary>
        /// Adds a comment to the specified entity part.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="partName">Name of the part.</param>
        /// <param name="comment">The comment.</param>
        /// <returns></returns>
        public virtual Comment AddEntityPartComment(string containerTitle, Guid entityId, string partName, string comment)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPFile entityPart;
            SPList list;
            SPFolder folder;
            if (
                SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) ==
                false)
                return null;

            if (
                SPDocumentStoreHelper.TryGetDocumentStoreEntityPart(list, folder, entityId, partName, out entityPart) ==
                false)
                return null;

            web.AllowUnsafeUpdates = true;
            try
            {
                entityPart.Item[Constants.CommentFieldId] = comment;
                entityPart.Item.Update();

                return SPDocumentStoreHelper.MapCommentFromSPListItem(entityPart.Item);
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }
        }

        /// <summary>
        /// Lists the comments associated with the specified entity part.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="partName">Name of the part.</param>
        /// <returns></returns>
        public virtual IList<Comment> ListEntityPartComments(string containerTitle, Guid entityId, string partName)
        {
            return ListEntityPartComments(containerTitle, entityId, partName, String.Empty);
        }

        public virtual IList<Comment> ListEntityPartComments(string containerTitle, Guid entityId, string partName,
            string path)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPFile entityPart;
            if (!SPDocumentStoreHelper.TryGetDocumentStoreEntityPartDirect(web, containerTitle, path, entityId, partName,
                    out entityPart))
            {
                SPList list;
                SPFolder folder;
                if (
                    SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, path) ==
                    false)
                    return null;

                if (
                    SPDocumentStoreHelper.TryGetDocumentStoreEntityPart(list, folder, entityId, partName, out entityPart) ==
                    false)
                    return null;
            }

            var result = new List<Comment>();
            Comment[] lastComment =
            {
                new Comment
                {
                    CommentText = null
                }
            };

            var versions = entityPart.Item.Versions;
            foreach (var itemVersion in versions.OfType<SPListItemVersion>()
                .OrderBy(v => Double.Parse(v.VersionLabel))
                .Where(itemVersion => String.CompareOrdinal(itemVersion["Comments"] as string, lastComment[0].CommentText) != 0))
            {
                lastComment[0] = SPDocumentStoreHelper.MapCommentFromSPListItemVersion(itemVersion);
                result.Add(lastComment[0]);
            }

            result.Reverse();
            return result;
        }

        /// <summary>
        /// Adds a comment to the specified attachment.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="comment">The comment.</param>
        /// <returns></returns>
        public virtual Comment AddAttachmentComment(string containerTitle, Guid entityId, string fileName, string comment)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPFile attachment;
            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) ==
                false)
                return null;

            if (SPDocumentStoreHelper.TryGetDocumentStoreAttachment(list, folder, entityId, fileName, out attachment) ==
                false)
                return null;

            web.AllowUnsafeUpdates = true;
            try
            {
                attachment.Item[Constants.CommentFieldId] = comment;
                attachment.Item.Update();

                return SPDocumentStoreHelper.MapCommentFromSPListItem(attachment.Item);
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }
        }

        /// <summary>
        /// Lists the comments associated with the specified attachment.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public virtual IList<Comment> ListAttachmentComments(string containerTitle, Guid entityId, string fileName)
        {
            return ListAttachmentComments(containerTitle, entityId, fileName, String.Empty);
        }

        public virtual IList<Comment> ListAttachmentComments(string containerTitle, Guid entityId, string fileName,
            string path)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPFile attachment;
            if (!SPDocumentStoreHelper.TryGetDocumentStoreAttachmentDirect(web, containerTitle, path, entityId, fileName, out attachment))
            {
                SPList list;
                SPFolder folder;
                if (
                    SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) ==
                    false)
                    return null;


                if (
                    SPDocumentStoreHelper.TryGetDocumentStoreAttachment(list, folder, entityId, fileName, out attachment) ==
                    false)
                    return null;
            }

            var result = new List<Comment>();
            Comment[] lastComment =
            {
                new Comment
                {
                    CommentText = null
                }
            };

            var versions = attachment.Item.Versions;
            foreach (var itemVersion in versions.OfType<SPListItemVersion>()
                .OrderBy(v => Double.Parse(v.VersionLabel))
                .Where(itemVersion => String.CompareOrdinal(itemVersion["Comments"] as string, lastComment[0].CommentText) != 0))
            {
                lastComment[0] = SPDocumentStoreHelper.MapCommentFromSPListItemVersion(itemVersion);
                result.Add(lastComment[0]);
            }
            result.Reverse();
            return result;
        }

        #endregion
    }
}
