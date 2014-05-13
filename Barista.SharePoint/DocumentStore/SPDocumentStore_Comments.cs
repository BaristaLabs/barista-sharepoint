namespace Barista.SharePoint.DocumentStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Barista.DocumentStore;
    using Microsoft.SharePoint;

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

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            SPFile defaultEntityPart;
            if (SPDocumentStoreHelper.TryGetDocumentStoreDefaultEntityPart(list, folder, entityId, out defaultEntityPart) == false)
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
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            SPFile defaultEntityPart;
            if (SPDocumentStoreHelper.TryGetDocumentStoreDefaultEntityPart(list, folder, entityId, out defaultEntityPart) == false)
                return null;

            List<Comment> result = new List<Comment>();
            Comment[] lastComment = {
                                        new Comment
                                                {
                                                CommentText = null
                                                }
                                    };

            foreach (var itemVersion in defaultEntityPart.Item.Versions.OfType<SPListItemVersion>()
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

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            SPFile entityPart;
            if (SPDocumentStoreHelper.TryGetDocumentStoreEntityPart(list, folder, entityId, partName, out entityPart) == false)
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
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            SPFile entityPart;
            if (SPDocumentStoreHelper.TryGetDocumentStoreEntityPart(list, folder, entityId, partName, out entityPart) == false)
                return null;

            List<Comment> result = new List<Comment>();
            Comment lastComment = new Comment
            {
                CommentText = null
            };

            foreach (var itemVersion in entityPart.Item.Versions.OfType<SPListItemVersion>().OrderBy(v => Double.Parse(v.VersionLabel)))
            {
                if (String.CompareOrdinal(itemVersion["Comments"] as string, lastComment.CommentText) != 0)
                {
                    lastComment = SPDocumentStoreHelper.MapCommentFromSPListItemVersion(itemVersion);
                    result.Add(lastComment);
                }
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

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            SPFile attachment;
            if (SPDocumentStoreHelper.TryGetDocumentStoreAttachment(list, folder, entityId, fileName, out attachment) == false)
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
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            SPFile attachment;
            if (SPDocumentStoreHelper.TryGetDocumentStoreAttachment(list, folder, entityId, fileName, out attachment) == false)
                return null;

            List<Comment> result = new List<Comment>();
            Comment lastComment = new Comment
            {
                CommentText = null
            };

            foreach (var itemVersion in attachment.Item.Versions.OfType<SPListItemVersion>().OrderBy(v => Double.Parse(v.VersionLabel)))
            {
                if (String.CompareOrdinal(itemVersion["Comments"] as string, lastComment.CommentText) != 0)
                {
                    lastComment = SPDocumentStoreHelper.MapCommentFromSPListItemVersion(itemVersion);
                    result.Add(lastComment);
                }
            }
            result.Reverse();
            return result;
        }
        #endregion
    }
}
