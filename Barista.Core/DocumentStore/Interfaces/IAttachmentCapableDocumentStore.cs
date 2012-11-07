namespace Barista.DocumentStore
{
  using System;
  using System.Collections.Generic;
  using System.IO;

  public interface IAttachmentCapableDocumentStore
  {
    #region Attachments
    /// <summary>
    /// Deletes the attachment from the document store.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    bool DeleteAttachment(string containerTitle, Guid entityId, string fileName);

    /// <summary>
    /// Downloads the attachment as a stream.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    Stream DownloadAttachment(string containerTitle, Guid entityId, string fileName);

    /// <summary>
    /// Gets an attachment associated with the specified entity with the specified filename.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    Attachment GetAttachment(string containerTitle, Guid entityId, string fileName);

    /// <summary>
    /// Lists the attachments associated with the specified entity.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <returns></returns>
    IList<Attachment> ListAttachments(string containerTitle, Guid entityId);

    /// <summary>
    /// Renames the attachment with the specified filename to the new filename.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="newFileName">New name of the file.</param>
    /// <returns></returns>
    bool RenameAttachment(string containerTitle, Guid entityId, string fileName, string newFileName);

    /// <summary>
    /// Uploads the attachment and associates it with the specified entity with the specified filename.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="attachment">The attachment.</param>
    /// <returns></returns>
    Attachment UploadAttachment(string containerTitle, Guid entityId, string fileName, byte[] attachment);

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
    Attachment UploadAttachment(string containerTitle, Guid entityId, string fileName, byte[] attachment, string category, string path);

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
    Attachment UploadAttachmentFromSourceUrl(string containerTitle, Guid entityId, string fileName, string sourceUrl, string category, string path);
    #endregion
  }
}
